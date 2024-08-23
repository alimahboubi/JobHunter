using System.Diagnostics;
using JobHunter.Application.Abstraction.Cache;
using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.Job.Enums;
using JobHunter.Domain.Job.Services;
using JobHunter.Infrastructure.Linkedin.Configurations;
using JobHunter.Infrastructure.Linkedin.Exceptions;
using JobHunter.Infrastructure.Linkedin.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using OpenTelemetry.Trace;

namespace JobHunter.Infrastructure.Linkedin;

public class JobCrawlerService(
    JobDescriptionCrawler jobDescriptionCrawler,
    JobSearchCrawler jobSearchCrawler,
    ILogger<JobCrawlerService> logger,
    Tracer tracer,
    PlaywrightConfigurations playwrightConfigurations,
    ICacheService cacheService) : IJobCrawlerService
{
    public async Task<List<JobResultDto>> GetJobPositions(TargetPositionDto targetPositionDto,
        CancellationToken ct = default)
    {
        var page = await InitializePlaywright();
        await LoginAsync(page, targetPositionDto.Username, targetPositionDto.Password);

        var results = new List<JobResultDto>();
        foreach (var location in targetPositionDto.TargetLocations)
        {
            var fetchedJob = await GetJobsForLocationAsync(page, location, targetPositionDto.JobTitle,
                targetPositionDto.TargetKeywords, targetPositionDto.MustHaveKeywords, targetPositionDto.JobCategory,
                ct);
            results.AddRange(fetchedJob);
        }

        return results;
    }

    private async Task<List<JobResultDto>> GetJobsForLocationAsync(IPage page, string location, string positionName,
        List<string> keywords, List<string> criticalKeywords, JobCategory jobCategory, CancellationToken ct)
    {
        using var jobLocationSpan = tracer.StartActiveSpan("GetJobsForLocationAsync");
        jobLocationSpan.SetAttribute("Location", location);
        var jobResults = new List<JobResultDto>();
        try
        {
            var jobs = await jobSearchCrawler.SearchJobResultsAsync(location, positionName, keywords, ct);

            if (!jobs.Any())
                return jobResults;

            foreach (var jobCardDto in jobs)
            {
                try
                {
                    if (jobCardDto.Id is not null && cacheService.Get<JobCardDto>(jobCardDto.Id) is not null)
                    {
                        continue;
                    }

                    cacheService.Set(jobCardDto.Id, jobCardDto, new TimeSpan(1, 0, 0, 0));

                    var jobDescription =
                        await jobDescriptionCrawler.FetchDescriptionAsync(page, jobCardDto.Url, jobCategory);
                    if (criticalKeywords.Any() && !CheckJob(jobDescription.Description, criticalKeywords))
                    {
                        continue;
                    }

                    jobResults.Add(CreateJobResultDto(jobCardDto, jobDescription));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Job Description failed to handle");
                }

                await Task.Delay(1000, ct);
            }
        }
        catch (JobSearchCrawlerException ex)
        {
            jobLocationSpan.SetStatus(Status.Error);
            logger.LogError(ex, "Job Search failed to handle");
        }
        catch (Exception ex)
        {
            jobLocationSpan.SetStatus(Status.Error);
            logger.LogError(ex, "Get Jobs For Location failed");
        }

        return jobResults;
    }

    private async Task<IPage> InitializePlaywright()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.ConnectAsync(playwrightConfigurations.PlaywrightUrl);
        var iphone13 = playwright.Devices["iPhone 13"];
        var context = await browser.NewContextAsync(iphone13);
        if (!browser.IsConnected)
        {
            throw new Exception("Failed to Connect browser");
        }

        var page = await context.NewPageAsync();
        return page;
    }

    private async Task LoginAsync(IPage page, string username, string password)
    {
        await page.GotoAsync("https://www.linkedin.com/login");
        await page.WaitForSelectorAsync(".login__form");

        // Fill out the login form
        await page.FillAsync("input#username", username);
        await page.FillAsync("input#password", password);

        // Click the login button
        await page.ClickAsync("button[type='submit']");

        // Wait for the navigation to complete
        await page.WaitForNavigationAsync();
    }

    private static JobResultDto CreateJobResultDto(JobCardDto jobCardDto, JobDescriptionResultDto jobDescription)
    {
        return new JobResultDto
        {
            Id = jobCardDto.Id ?? Guid.NewGuid().ToString(),
            Title = jobCardDto.Title,
            Company = jobCardDto.Company,
            Location = jobCardDto.Location,
            Url = jobCardDto.Url,
            PostedDate = jobCardDto.PostedDate,
            EmploymentType = jobCardDto.EmploymentType,
            LocationType = jobDescription.LocationType,
            JobDescription = jobDescription.Description,
            SeniorityLevel = jobDescription.SeniorityLevel
        };
    }

    private static bool CheckJob(string jobDescription, List<string> criticalKeywords)
    {
        return criticalKeywords.Any(keyword => jobDescription.Contains(keyword.ToLower()));
    }
}