using System.Text.RegularExpressions;
using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.Job.Services;
using JobHunter.Infrastructure.Linkedin.Configurations;
using JobHunter.Infrastructure.Linkedin.Exceptions;
using JobHunter.Infrastructure.Linkedin.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using OpenTelemetry.Trace;

namespace JobHunter.Infrastructure.Linkedin;

public class JobCrawlerService(
    JobSearchCrawler jobSearchCrawler,
    ILogger<JobCrawlerService> logger,
    Tracer tracer,
    LinkedinConfiguration linkedinConfiguration,
    PlaywrightConfigurations playwrightConfigurations) : IJobCrawlerService
{
    private IPage? _page;

    public async Task<List<JobResultDto>> GetJobPositions(TargetPositionDto targetPositionDto,
        CancellationToken ct = default)
    {
        await InitializePlaywright();
        await LoginAsync();

        var results = new List<JobResultDto>();
        foreach (var location in targetPositionDto.TargetLocations)
        {
            var fetchedJob = await GetJobsForLocationAsync(location, targetPositionDto.JobTitle,
                targetPositionDto.TargetKeywords, targetPositionDto.MustHaveKeywords, ct);
            results.AddRange(fetchedJob);
        }

        return results;
    }

    private async Task<List<JobResultDto>> GetJobsForLocationAsync(string location, string positionName,
        List<string> keywords, List<string> criticalKeywords, CancellationToken ct)
    {
        using var jobLocationSpan = tracer.StartActiveSpan("GetJobsForLocationAsync");
        jobLocationSpan.SetAttribute("Location", location);
        var jobResults = new List<JobResultDto>();
        try
        {
            var jobs = await jobSearchCrawler.SearchJobResultsAsync(_page, location, positionName, keywords, ct);

            if (!jobs.Any())
                return jobResults;
            jobLocationSpan.SetAttribute("total job", jobs.Count);
            foreach (var jobCardDto in jobs)
            {
                if (criticalKeywords.Any() && !CheckJob(jobCardDto.Description, criticalKeywords))
                {
                    logger.LogInformation("Job with id {@id} doesn't match with any keywords", jobCardDto.Id);
                    continue;
                }

                jobResults.Add(CreateJobResultDto(jobCardDto));
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

        if (!browser.IsConnected)
        {
            throw new Exception("Failed to Connect browser");
        }

        _page = await browser.NewPageAsync();
    }

    private async Task LoginAsync()
    {
        logger.LogInformation("Start login");
        var responsePage = await _page.GotoAsync("https://www.linkedin.com/login");
        await _page.WaitForLoadStateAsync(LoadState.Load);
        if (responsePage.Url == "https://www.linkedin.com/feed/")
        {
            logger.LogInformation("Already logged in");
            return;
        }

        var cookie = await _page.QuerySelectorAsync("artdeco-global-alert__body");
        if (cookie is not null)
        {
            await _page.ClickAsync("button[action-type='ACCEPT']");
        }

        await _page.WaitForSelectorAsync(".login__form");

        // Fill out the login form
        await _page.FillAsync("input#username", linkedinConfiguration.Username);
        await _page.FillAsync("input#password", linkedinConfiguration.Password);

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

    private static JobResultDto CreateJobResultDto(JobCardDto jobCardDto)
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
            JobDescription = jobCardDto.Description,
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