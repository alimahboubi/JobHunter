using System.Text.Json;
using JobHunter.Domain.Job.Configurations;
using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.Job.Entities;
using JobHunter.Domain.Job.Enums;
using JobHunter.Domain.Job.Repositories;
using JobHunter.Domain.Job.Services;
using Microsoft.Extensions.Logging;

namespace JobHunter.Application.Services;

public class BackgroundJobService(
    IJobRepository jobRepository,
    ILogger<BackgroundJobService> logger,
    IJobCrawlerService jobCrawlerService,
    UsersConfigurations userConfigurations)
    : IBackgroundJobService
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        foreach (var userConfiguration in userConfigurations.Profiles)
        {
            if (!userConfiguration.IsEnabled)
                continue;
            try
            {
                var isJobTypeParsed =
                    Enum.TryParse<JobCategory>(userConfiguration.JobTargetSettings.JobCategory, out var jobType);
                var targetPosition = new TargetPositionDto
                {
                    Username = userConfiguration.Username,
                    Password = userConfiguration.Password,
                    JobTitle = userConfiguration.JobTargetSettings.JobTitle,
                    TargetLocations = userConfiguration.JobTargetSettings.TargetLocations,
                    TargetKeywords = userConfiguration.JobTargetSettings.TargetKeywords,
                    MustHaveKeywords = userConfiguration.JobTargetSettings.MustHaveKeywords,
                    JobCategory = isJobTypeParsed ? jobType : JobCategory.None
                };
                var jobs = await jobCrawlerService.GetJobPositions(targetPosition, cancellationToken);
                await SaveJobsToDb(jobs, userConfiguration.Id, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    private async Task SaveJobsToDb(List<JobResultDto> jobs, Guid userId, CancellationToken ct)
    {
        foreach (var job in jobs)
        {
            var isDuplicatedJob = await jobRepository.IsJobExistAsync(job.Id, ct);
            if (isDuplicatedJob)
                continue;

            var keywords = JsonSerializer.Serialize(job.Keywords);
            var jobEntity = new Job
            {
                UserId = userId,
                Source = "Linkedin",
                SourceId = job.Id,
                Title = job.Title,
                Company = job.Company,
                Location = job.Location,
                Url = job.Url,
                PostedDate = job.PostedDate,
                EmploymentType = job.EmploymentType,
                LocationType = job.LocationType,
                NumberOfEmployees = job.NumberOfEmployees,
                JobDescription = job.JobDescription,
                Keywords = keywords
            };
            await jobRepository.AddAsync(jobEntity, ct);
        }
    }
}