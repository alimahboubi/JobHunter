using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.Job.Enums;
using JobHunter.Domain.Job.Services;
using JobHunter.Infrastructure.Linkedin.Exceptions;
using JobHunter.Infrastructure.Linkedin.Models;
using Microsoft.Extensions.Logging;

namespace JobHunter.Infrastructure.Linkedin;

public class JobCrawlerService(
    JobDescriptionCrawler jobDescriptionCrawler,
    JobSearchCrawler jobSearchCrawler,
    ILogger<JobCrawlerService> logger)
    : IJobCrawlerService
{
    public async Task<List<JobResultDto>> GetJobPositions(TargetPositionDto targetPositionDto,
        CancellationToken ct = default)
    {
        var tasks = targetPositionDto.TargetLocations.Select(location =>
                GetJobsForLocationAsync(location, targetPositionDto.JobTitle,
                    targetPositionDto.TargetKeywords, targetPositionDto.MustHaveKeywords, targetPositionDto.JobCategory,
                    ct))
            .ToList();

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(result => result).ToList();
    }

    private async Task<List<JobResultDto>> GetJobsForLocationAsync(string location, string positionName,
        List<string> keywords, List<string> criticalKeywords, JobCategory jobCategory, CancellationToken ct)
    {
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
                    var jobDescription =
                        await jobDescriptionCrawler.FetchDescriptionAsync(jobCardDto.Url, jobCategory);
                    if (criticalKeywords.Any() && !CheckJob(jobDescription.Description, criticalKeywords))
                    {
                        continue;
                    }

                    jobResults.Add(CreateJobResultDto(jobCardDto, jobDescription));
                }
                catch (JobDescriptionCrawlerException ex)
                {
                    logger.LogError(ex, "Job Description failed to handle");
                }

                await Task.Delay(1000, ct);
            }
        }
        catch (JobSearchCrawlerException ex)
        {
            logger.LogError(ex, "Job Search failed to handle");
        }
        catch (Exception ex)
        {
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