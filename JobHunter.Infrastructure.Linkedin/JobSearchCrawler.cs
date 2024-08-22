using HtmlAgilityPack;
using JobHunter.Infrastructure.Linkedin.Configurations;
using JobHunter.Infrastructure.Linkedin.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using JobHunter.Infrastructure.Linkedin.Exceptions;
using OpenTelemetry.Trace;

namespace JobHunter.Infrastructure.Linkedin
{
    public class JobSearchCrawler(
        LinkedinConfiguration linkedinConfiguration,
        ILogger<JobSearchCrawler> logger,
        IHttpClientFactory httpClientFactory,
        Tracer tracer)
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient(nameof(LinkedinConfiguration));

        public async Task<List<JobCardDto>> SearchJobResultsAsync(string location, string positionName,
            List<string> searchKeywords, CancellationToken ct = default)
        {
            try
            {
                var jobPage = await FetchJobSearchPageContentAsync(location, positionName, searchKeywords, ct);
                if (string.IsNullOrEmpty(jobPage))
                    throw new Exception("Failed to fetch job page content.");

                var jobCards = ParseJobCards(jobPage, location);
                return jobCards.Select(CreateJobCardDto).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while searching for job results.");
                throw;
            }
        }

        private async Task<string> FetchJobSearchPageContentAsync(string location, string positionName,
            List<string> searchKeywords, CancellationToken ct = default)
        {
            const int maxRetries = 3;
            int retries = 0;
            var searchQuery = "?" + CreateSearchQuery(location, positionName, searchKeywords);

            while (retries < maxRetries)
            {
                try
                {
                    var response = await _httpClient.GetAsync(searchQuery, ct);

                    response.EnsureSuccessStatusCode(); // Throws if not 2xx

                    return await response.Content.ReadAsStringAsync(ct);
                }
                catch (HttpRequestException ex) when (retries < maxRetries)
                {
                    logger.LogWarning(ex, "Retry {RetryCount}/{MaxRetries} for fetching job search page content",
                        retries + 1, maxRetries);
                    retries++;
                    await Task.Delay(3000, ct);
                }
            }

            throw new JobSearchCrawlerException(location);
        }

        private HtmlNodeCollection ParseJobCards(string pageContents, string location)
        {
            using var parsJobSpan = tracer.StartActiveSpan("parsJobSpan");
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pageContents);

            var jobCards = htmlDocument.DocumentNode.SelectNodes(PageNodes.JobCards);
            if (jobCards == null)
                throw new JobSearchCrawlerException(location);

            return jobCards;
        }

        private JobCardDto CreateJobCardDto(HtmlNode htmlContent)
        {
            var url = FindCardUrl(htmlContent);
            return new JobCardDto
            {
                Id = FindJobIdFromUrl(url),
                Title = FindCardItem(htmlContent, PageNodes.JobTitleNode),
                Company = FindCardItem(htmlContent, PageNodes.CompanyNode),
                Location = FindCardItem(htmlContent, PageNodes.LocationNode),
                Url = url,
                PostedDate = FindCardDateTime(htmlContent),
            };
        }

        private string FindCardItem(HtmlNode node, string xpath)
        {
            var selectedNode = node.SelectSingleNode(xpath);
            return selectedNode?.InnerText.Trim() ?? "N/A";
        }

        private string FindCardUrl(HtmlNode htmlContent)
        {
            var node = htmlContent.SelectSingleNode(PageNodes.JobUrlNode);
            var url = node?.Attributes["href"]?.Value;
            int questionMarkIndex = url.IndexOf('?');

            if (questionMarkIndex >= 0)
            {
                url = url.Substring(0, questionMarkIndex);
            }

            return url;
        }

        private DateTime FindCardDateTime(HtmlNode htmlContent)
        {
            var time = DateTime.UtcNow;
            var selectedNode = htmlContent.SelectSingleNode(PageNodes.PostedDateNode);
            var postedTime = selectedNode?.InnerText.Trim() ?? "N/A";
            var numericString = Regex.Match(postedTime, @"\d+").Value;

            if (string.IsNullOrEmpty(numericString))
                return time;

            var decrementValue = int.Parse(numericString);

            if (postedTime.Contains("second"))
                time = time.AddSeconds(-decrementValue);
            else if (postedTime.Contains("minute"))
                time = time.AddMinutes(-decrementValue);
            else if (postedTime.Contains("hour"))
                time = time.AddHours(-decrementValue);
            else if (postedTime.Contains("day"))
                time = time.AddDays(-decrementValue);
            else if (postedTime.Contains("week"))
                time = time.AddDays(-7 * decrementValue);
            else if (postedTime.Contains("month"))
                time = time.AddMonths(-decrementValue);
            else if (postedTime.Contains("year"))
                time = time.AddYears(-decrementValue);

            return time;
        }

        private string FindJobIdFromUrl(string url)
        {
            var jobUri = new Uri(url);
            var path = jobUri.LocalPath;
            return path.Split("-").Last();
        }

        private string? CreateSearchQuery(string location, string positionName, List<string> searchKeywords)
        {
            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("f_TPR", $"r{linkedinConfiguration.TimeIntervalSeconds}");
            queryString.Add("location", location);

            var keywordsString = $"&keywords={CreateKeywordQuery(positionName, searchKeywords)}";
            return queryString + keywordsString;
        }

        private string CreateKeywordQuery(string positionName, List<string> searchKeywords)
        {
            var quaryableKeywords = new List<string>();
            quaryableKeywords.Add(positionName);
            foreach (var keyword in searchKeywords)
            {
                quaryableKeywords.Add(keyword.Replace("#", "%23"));
            }

            return string.Join(" OR ", quaryableKeywords);
        }
    }
}