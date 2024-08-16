using JobHunter.Infrastructure.Linkedin.Configurations;
using JobHunter.Infrastructure.Linkedin.Models;
using Microsoft.Extensions.Logging;
using JobHunter.Domain.Job.Constants;
using JobHunter.Domain.Job.Enums;
using JobHunter.Infrastructure.Linkedin.Exceptions;
using Microsoft.Playwright;

namespace JobHunter.Infrastructure.Linkedin
{
    public class JobDescriptionCrawler(
        ILogger<JobDescriptionCrawler> logger,
        PlaywrightConfigurations playwrightConfigurations)
    {
        private const int MaxAttempts = 5;
        private int _delay = 1000;

        public async Task<JobDescriptionResultDto> FetchDescriptionAsync(string url, JobCategory jobCategory)
        {
            for (int attempts = 0; attempts < MaxAttempts; attempts++)
            {
                try
                {
                    return await TryFetchDescriptionAsync(url, jobCategory);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Attempt {Attempt}: Failed to fetch job description from URL: {Url}",
                        attempts + 1, url);
                    AdjustDelay();
                }
            }

            throw new JobDescriptionCrawlerException(url);
        }

        private async Task<JobDescriptionResultDto> TryFetchDescriptionAsync(string url,
            JobCategory jobCategory)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Firefox.ConnectAsync(playwrightConfigurations.PlaywrightUrl);
            var page = await browser.NewPageAsync();

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
            await page.WaitForSelectorAsync(PageNodes.JobDescriptionNode,new PageWaitForSelectorOptions
            {
                Timeout = _delay
            });
        }

        private static async Task<string> GetSeniorityLevelAsync(IPage page)
        {
            return await page.EvaluateAsync<string>(
                "document.querySelector('.description__job-criteria-text').innerText") ?? "N/A";
        }

        private static async Task<string> GetDescriptionAsync(IPage page)
        {
            return await page.EvaluateAsync<string>(
                "document.querySelector('.description__text--rich').innerText") ?? "N/A";
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