using JobHunter.Infrastructure.Linkedin.Configurations;
using JobHunter.Infrastructure.Linkedin.Exceptions;
using JobHunter.Infrastructure.Linkedin.Models;
using JobHunter.Application.Abstraction.Cache;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using OpenTelemetry.Trace;
using System.Text.RegularExpressions;

namespace JobHunter.Infrastructure.Linkedin;

public class JobSearchCrawler(
    LinkedinConfiguration linkedinConfiguration,
    ILogger<JobSearchCrawler> logger,
    ICacheService cacheService,
    Tracer tracer)
{
    private const int MaxRetries = 3;
    private const int DelayMilliseconds = 1000;

    public async Task<List<JobCardDto>> SearchJobResultsAsync(
        IPage page,
        string location,
        string positionName,
        List<string> searchKeywords,
        CancellationToken ct = default)
    {
        if (page == null) throw new ArgumentNullException(nameof(page));
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location cannot be null or empty.", nameof(location));
        if (string.IsNullOrWhiteSpace(positionName))
            throw new ArgumentException("Position name cannot be null or empty.", nameof(positionName));
        if (searchKeywords == null) throw new ArgumentNullException(nameof(searchKeywords));

        try
        {
            await FetchJobSearchPageContentAsync(page, location, positionName, searchKeywords, ct);
            return await ExtractSearchResultsAsync(page);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while searching for job results.");
            throw;
        }
    }

    private async Task FetchJobSearchPageContentAsync(
        IPage page,
        string location,
        string positionName,
        List<string> searchKeywords,
        CancellationToken ct)
    {
        using var span = tracer.StartActiveSpan(nameof(FetchJobSearchPageContentAsync));
        var searchQuery = CreateSearchQuery(location, positionName, searchKeywords);
        var url = $"{linkedinConfiguration.JobSearchBaseUrl}?{searchQuery}";

        int retries = 0;
        while (retries < MaxRetries)
        {
            try
            {
                await NavigateToPageAsync(page, url, ct);
                span.SetStatus(Status.Ok);
                return;
            }
            catch (Exception ex)
            {
                retries++;
                logger.LogWarning(ex, "Retry {RetryCount}/{MaxRetries} for fetching job search page content.",
                    retries, MaxRetries);
                span.SetAttribute("retries", retries);
                span.SetStatus(Status.Error);

                if (retries >= MaxRetries)
                {
                    throw new JobSearchCrawlerException(location);
                }

                await Task.Delay(DelayMilliseconds, ct);
            }
        }
    }

    private async Task NavigateToPageAsync(IPage page, string url, CancellationToken ct)
    {
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.Load });
        await page.WaitForSelectorAsync(PageNodes.JobListNode,
            new PageWaitForSelectorOptions { Timeout = DelayMilliseconds });
    }

    private async Task<List<JobCardDto>> ExtractSearchResultsAsync(IPage page)
    {
        using var span = tracer.StartActiveSpan(nameof(ExtractSearchResultsAsync));
        var jobElements = await page.QuerySelectorAllAsync(PageNodes.JobListItems);
        var searchResults = new List<JobCardDto>();

        foreach (var element in jobElements)
        {
            var url = await FindCardUrlAsync(element);
            var id = ExtractJobIdFromUrl(url);

            if (IsJobAlreadyCached(id))
            {
                logger.LogInformation("Job with ID {Id} is already cached.", id);
                continue;
            }

            try
            {
                var job = await ParseJobDetailAsync(page, element, id, url);
                searchResults.Add(job);
                cacheService.Set(id, job, TimeSpan.FromDays(1));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse job detail for ID {Id}.", id);
            }
        }

        return searchResults;
    }

    private async Task<JobCardDto> ParseJobDetailAsync(IPage page, IElementHandle element, string id, string url)
    {
        var description = await GetJobDescriptionAsync(element, page, id);

        var jobCardDto = new JobCardDto
        {
            Id = id,
            Title = await ParseCardItemAsync(element, PageNodes.JobTitleNode),
            Company = await ParseCardItemAsync(element, PageNodes.CompanyNode),
            Location = await ParseCardItemAsync(element, PageNodes.LocationNode),
            Url = url,
            PostedDate = await ParsePostedDateAsync(element),
            Description = await ExtractJobDescriptionAsync(page)
        };

        return jobCardDto;
    }


    private bool IsJobAlreadyCached(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        return cacheService.Get<JobCardDto>(id) != null;
    }

    private async Task<String> GetJobDescriptionAsync(IElementHandle element, IPage page, string id)
    {
        try
        {
            var clickableDescription = await element.QuerySelectorAsync(PageNodes.ClickableDescription);
            if (clickableDescription != null)
            {
                var responseTask = page.WaitForResponseAsync(response =>
                    Regex.IsMatch(response.Url, $".*{id}.*"));

                await clickableDescription.ClickAsync();
                await responseTask;
            }

            return await ExtractJobDescriptionAsync(page);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to load description");
            return "N/A";
        }
    }

    private async Task<string> ExtractJobDescriptionAsync(IPage page)
    {
        var selectedNode = await page.QuerySelectorAsync(PageNodes.Description);
        return selectedNode == null ? "N/A" : (await selectedNode.InnerTextAsync()).Trim();
    }

    private async Task<string> ParseCardItemAsync(IElementHandle node, string selector)
    {
        var selectedNode = await node.QuerySelectorAsync(selector);
        return selectedNode == null ? "N/A" : (await selectedNode.InnerTextAsync()).Trim();
    }


    private async Task<string> FindCardUrlAsync(IElementHandle element)
    {
        var node = await element.QuerySelectorAsync(PageNodes.JobUrlNode);
        if (node == null)
        {
            throw new Exception("Job URL node not found.");
        }

        var href = await node.GetAttributeAsync("href");
        if (string.IsNullOrWhiteSpace(href))
        {
            throw new Exception("Job URL not found.");
        }

        var url = href.Split('?')[0];
        return new Uri(new Uri("https://linkedin.com"), url).ToString();
    }

    private async Task<DateTime> ParsePostedDateAsync(IElementHandle element)
    {
        var selectedNode = await element.QuerySelectorAsync(PageNodes.PostedDateNode);
        if (selectedNode == null)
        {
            return DateTime.UtcNow;
        }

        var postedTimeText = (await selectedNode.InnerTextAsync()).Trim();
        var match = Regex.Match(postedTimeText, @"(\d+)\s+(second|minute|hour|day|week|month|year)s?",
            RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return DateTime.UtcNow;
        }

        int value = int.Parse(match.Groups[1].Value);
        string unit = match.Groups[2].Value.ToLowerInvariant();

        return unit switch
        {
            "second" => DateTime.UtcNow.AddSeconds(-value),
            "minute" => DateTime.UtcNow.AddMinutes(-value),
            "hour" => DateTime.UtcNow.AddHours(-value),
            "day" => DateTime.UtcNow.AddDays(-value),
            "week" => DateTime.UtcNow.AddDays(-7 * value),
            "month" => DateTime.UtcNow.AddMonths(-value),
            "year" => DateTime.UtcNow.AddYears(-value),
            _ => DateTime.UtcNow
        };
    }

    private string ExtractJobIdFromUrl(string url)
    {
        var jobUri = new Uri(url);
        var segments = jobUri.AbsolutePath.Trim('/').Split('/');
        return segments.LastOrDefault() ?? string.Empty;
    }

    private string CreateSearchQuery(string location, string positionName, List<string> searchKeywords)
    {
        var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString["f_TPR"] = $"r{linkedinConfiguration.TimeIntervalSeconds}";
        queryString["location"] = location;
        var keywordsString = $"&keywords={CreateKeywordQuery(positionName, searchKeywords)}";
        return queryString + keywordsString;
    }

    private string CreateKeywordQuery(string positionName, List<string> searchKeywords)
    {
        var keywords = new List<string> { positionName };
        keywords.AddRange(searchKeywords);

        var encodedKeywords = keywords.Select(k => System.Web.HttpUtility.UrlEncode(k));
        return string.Join(" OR ", encodedKeywords);
    }
}