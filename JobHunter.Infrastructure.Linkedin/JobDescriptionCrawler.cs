using System.Text.RegularExpressions;
using HtmlAgilityPack;
using JobHunter.Infrastructure.Linkedin.Configurations;
using JobHunter.Infrastructure.Linkedin.Models;
using Microsoft.Extensions.Logging;
using JobHunter.Domain.Job.Constants;
using JobHunter.Domain.Job.Enums;
using JobHunter.Infrastructure.Linkedin.Exceptions;
using Microsoft.Playwright;
using OpenTelemetry.Trace;

namespace JobHunter.Infrastructure.Linkedin
{
    public class JobDescriptionCrawler(
        ILogger<JobDescriptionCrawler> logger,
        Tracer tracer)
    {
        private const int MaxAttempts = 5;
        private int _delay = 1000;

        public async Task<JobDescriptionResultDto> FetchDescriptionAsync(IPage page, string url,
            JobCategory jobCategory)
        {
            for (int attempts = 0; attempts < MaxAttempts; attempts++)
            {
                using var fetchDescriptionAsyncSpan = tracer.StartActiveSpan("FetchDescriptionAsync");
                fetchDescriptionAsyncSpan.SetAttribute("url", url);
                fetchDescriptionAsyncSpan.SetAttribute("attempts", attempts);
                try
                {
                    return await TryFetchDescriptionAsync(page, url, jobCategory);
                }
                catch (Exception ex)
                {
                    fetchDescriptionAsyncSpan.SetStatus(Status.Error);
                    logger.LogWarning(ex, "Attempt {Attempt}: Failed to fetch job description from URL: {Url}",
                        attempts + 1, url);
                    AdjustDelay();
                }
            }

            throw new JobDescriptionCrawlerException(url);
        }

        private async Task<JobDescriptionResultDto> TryFetchDescriptionAsync(IPage page, string url,
            JobCategory jobCategory)
        {
            await NavigateToPageAsync(page, url);

            var jobDescription = await GetDescriptionAsync(page);
            var locationType = GetLocationType(jobDescription);
            var seniorityLevel = await GetSeniorityLevelAsync(page);
            var keywords = GetKeywords(jobDescription, jobCategory);
            logger.LogInformation("Description of {url} Successfully Fetched", url);
            return new JobDescriptionResultDto
            {
                SeniorityLevel = seniorityLevel,
                Description = jobDescription,
                LocationType = locationType,
                Keywords = keywords
            };
        }

        private async Task NavigateToPageAsync(IPage page, string url)
        {
            await page.GotoAsync(url, new PageGotoOptions
            {
                Timeout = 5000,
                WaitUntil = WaitUntilState.NetworkIdle
            });
            await page.WaitForSelectorAsync(PageNodes.JobDescriptionNode, new PageWaitForSelectorOptions
            {
                Timeout = _delay
            });
        }

        private static async Task<string> GetSeniorityLevelAsync(IPage page)
        {
            /*var seniorityLevelElementHandle = await page.WaitForSelectorAsync(".meta-data");
            return await seniorityLevelElementHandle.InnerTextAsync();*/
            return "";
        }

        private async Task<string> GetDescriptionAsync(IPage page)
        {
            var jobDescriptionElementHandle = await page.WaitForSelectorAsync(PageNodes.JobDescriptionNode);
            return await jobDescriptionElementHandle.TextContentAsync();
        }
        private static string GetLocationType(string description)
        {
            if (description.Contains("remote")) return "Remote";
            if (description.Contains("hybrid")) return "Hybrid";
            return "On-Site";
        }

        private static List<string> GetKeywords(string description, JobCategory jobCategory)
        {
            var keywordInvetory = KeywordConstants.GetKeywords(jobCategory);
            return keywordInvetory?
                .Where(e =>
                    description.Contains(e.ToLower())
                ).ToList() ?? new();
        }

        private void AdjustDelay()
        {
            if (_delay < 5000)
            {
                _delay += 1000;
            }
        }
    }
}