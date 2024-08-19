using HtmlAgilityPack;
using JobHunter.Infrastructure.Linkedin.Configurations;
using JobHunter.Infrastructure.Linkedin.Models;
using Microsoft.Extensions.Logging;
using JobHunter.Domain.Job.Constants;
using JobHunter.Domain.Job.Enums;
using JobHunter.Infrastructure.Linkedin.Exceptions;
using OpenTelemetry.Trace;

namespace JobHunter.Infrastructure.Linkedin
{
    public class JobDescriptionCrawler(
        ILogger<JobDescriptionCrawler> logger,
        IHttpClientFactory httpClientFactory,
        Tracer tracer)
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

        private const int MaxRetries = 5;
        private int _delay = 3000;

        public async Task<JobDescriptionResultDto> SearchJobResultsAsync(string url, JobCategory jobCategory,
            CancellationToken ct = default)
        {
            try
            {
                var jobPage = await FetchJobSearchPageContentAsync(url,ct);
                if (string.IsNullOrEmpty(jobPage))
                    throw new Exception("Failed to fetch job page content.");

                return TryParseDescription(url, jobPage, jobCategory);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while searching for job results.");
                throw;
            }
        }


        private async Task<string> FetchJobSearchPageContentAsync(string url, CancellationToken ct = default)
        {
            int retries = 0;
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36");

            while (retries < MaxRetries)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url, ct);
                    
                    response.EnsureSuccessStatusCode(); // Throws if not 2xx

                    return await response.Content.ReadAsStringAsync(ct);
                }
                catch (HttpRequestException ex)
                {
                    retries++;
                    AdjustDelay();
                    logger.LogWarning(ex, "Retry {RetryCount}/{MaxRetries} for fetching job search page content",
                        retries, MaxRetries);
                    await Task.Delay(_delay, ct);
                }
            }

            throw new JobDescriptionCrawlerException(url);
        }

        private JobDescriptionResultDto TryParseDescription(string url, string pageDataHtml,
            JobCategory jobCategory)
        {
            using var span = tracer.StartActiveSpan("TryParseDescription");
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pageDataHtml);


            var jobDescription = GetDescriptionAsync(htmlDocument);
            var locationType = GetLocationType(jobDescription);
            var seniorityLevel = GetSeniorityLevelAsync(htmlDocument);
            var keywords = GetKeywords(jobDescription, jobCategory);
            logger.LogInformation("Description of {Url} Successfully Fetched", url);
            return new JobDescriptionResultDto
            {
                SeniorityLevel = seniorityLevel,
                Description = jobDescription,
                LocationType = locationType,
                Keywords = keywords
            };
        }

        private static string GetSeniorityLevelAsync(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.SelectSingleNode(PageNodes.SeniorityLevelNode).InnerText.Trim() ?? "N/A";
        }

        private static string GetDescriptionAsync(HtmlDocument htmlDocument)
        {
            return htmlDocument.DocumentNode.SelectSingleNode(PageNodes.DescriptionNode).InnerText ?? "N/A";
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
            if (_delay < 6000)
            {
                _delay += 1000;
            }
        }
    }
}