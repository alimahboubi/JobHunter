using System.Text.RegularExpressions;
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
    private IPage? _page;

    public async Task<List<JobResultDto>> GetJobPositions(TargetPositionDto targetPositionDto,
        CancellationToken ct = default)
    {
        await InitializePlaywright();
        await LoginAsync(targetPositionDto.Username, targetPositionDto.Password);

        var results = new List<JobResultDto>();
        foreach (var location in targetPositionDto.TargetLocations)
        {
            var fetchedJob = await GetJobsForLocationAsync(location, targetPositionDto.JobTitle,
                targetPositionDto.TargetKeywords, targetPositionDto.MustHaveKeywords, targetPositionDto.JobCategory,
                ct);
            results.AddRange(fetchedJob);
        }

        return results;
    }

    private async Task<List<JobResultDto>> GetJobsForLocationAsync(string location, string positionName,
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
            jobLocationSpan.SetAttribute("total job", jobs.Count);
            foreach (var jobCardDto in jobs)
            {
                try
                {
                    if (jobCardDto.Id is not null && cacheService.Get<JobCardDto>(jobCardDto.Id) is not null)
                    {
                        logger.LogInformation("Job with id {@id} has been fetchd previosly", jobCardDto.Id);
                        continue;
                    }

                    cacheService.Set(jobCardDto.Id, jobCardDto, new TimeSpan(1, 0, 0, 0));

                    var jobDescription =
                        await jobDescriptionCrawler.FetchDescriptionAsync(_page, jobCardDto.Url, jobCategory);
                    if (criticalKeywords.Any() && !CheckJob(jobDescription.Description, criticalKeywords))
                    {
                        logger.LogInformation("Job with id {@id} doesn't match with any keywords", jobCardDto.Id);
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

    private async Task InitializePlaywright()
    {
        if (_page is not null)
            return;
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.ConnectAsync(playwrightConfigurations.PlaywrightUrl);
        var iphone13 = playwright.Devices["iPhone 13"];
        var context = await browser.NewContextAsync(iphone13);
        if (!browser.IsConnected)
        {
            throw new Exception("Failed to Connect browser");
        }

        _page = await context.NewPageAsync();
    }

    private async Task LoginAsync(string username, string password)
    {
        logger.LogInformation("Start login");
        var responsePage = await _page.GotoAsync("https://www.linkedin.com/login");
        await _page.WaitForLoadStateAsync(LoadState.Load);
        if (responsePage.Url == "https://www.linkedin.com/feed/")
        {
            logger.LogInformation("Already logged in");
            return;
        }

        await _page.WaitForSelectorAsync(".login__form");

        // Fill out the login form
        await _page.FillAsync("input#username", username);
        await _page.FillAsync("input#password", password);

        // Click the login button
        await _page.ClickAsync("button[type='submit']");

        // Wait for the navigation to complete
        await _page.ScreenshotAsync(new()
        {
            Path = "login.jpg"
        });
        await _page.WaitForURLAsync("https://www.linkedin.com/feed/");
        logger.LogInformation("Successfully logged in");
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
        var domainRegex = new Regex(@"\b\w+\.net\b", RegexOptions.IgnoreCase);
    
        return criticalKeywords.Any(keyword =>
        {
            if (keyword.Equals(".net", StringComparison.OrdinalIgnoreCase))
            {
                // Check if ".net" is present but exclude if part of a domain
                return jobDescription.Contains(".net", StringComparison.OrdinalIgnoreCase) &&
                       !domainRegex.IsMatch(jobDescription);
            }
            else
            {
                // Standard contains check for other keywords
                return jobDescription.Contains(keyword, StringComparison.OrdinalIgnoreCase);
            }
        });
    }
}