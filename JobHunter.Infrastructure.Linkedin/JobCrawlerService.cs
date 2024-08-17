using System.Diagnostics;
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
    PlaywrightConfigurations playwrightConfigurations) : IJobCrawlerService
{
    public async Task<List<JobResultDto>> GetJobPositions(TargetPositionDto targetPositionDto,
        CancellationToken ct = default)
    {
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
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Firefox.ConnectAsync(playwrightConfigurations.PlaywrightUrl);
            if (!browser.IsConnected)
            {
                logger.LogError("Failed to Connect browser");
                return jobResults;
            }

            foreach (var jobCardDto in jobs)
            {
                try
                {
                    var jobDescription =
                        await jobDescriptionCrawler.FetchDescriptionAsync(browser, jobCardDto.Url, jobCategory);
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