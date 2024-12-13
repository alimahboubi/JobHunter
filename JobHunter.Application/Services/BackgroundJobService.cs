using JobHunter.Domain.Job.Configurations;
using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.Job.Enums;
using JobHunter.Domain.Job.Services;
using JobHunter.Domain.User.Entities;
using JobHunter.Domain.User.Repositories;
using Microsoft.Extensions.Logging;

namespace JobHunter.Application.Services;

public class BackgroundJobService(
    IJobService jobService,
    IUserRepository userRepository,
    ILogger<BackgroundJobService> logger,
    IJobCrawlerService jobCrawlerService)
    : IBackgroundJobService
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        var users = await GetActiveUsers(cancellationToken);
        foreach (var user in users)
        {
            try
            {
                await ProcessUserConfiguration(user, cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while processing user name : {Name} Id : {Id}", user.Name, user.Id);
            }
        }
    }

    private async Task ProcessUserConfiguration(User user, CancellationToken cancellationToken)
    {
        var targetPosition = CreateTargetPosition(user);
        var jobs = await jobCrawlerService.GetJobPositions(targetPosition, cancellationToken);
        await SaveJobsToDb(jobs, user.Id, cancellationToken);
    }

    private TargetPositionDto CreateTargetPosition(User user)
    {
        var isJobTypeParsed = Enum.TryParse<JobCategory>(user.JobTarget.JobCategory, out var jobType);
        return new TargetPositionDto
        {
            JobTitle = user.JobTarget.JobTitle,
            TargetLocations = user.JobTarget.TargetLocations,
            TargetKeywords = user.JobTarget.TargetKeywords,
            MustHaveKeywords = user.JobTarget.EssentialKeywords,
            JobCategory = isJobTypeParsed ? jobType : JobCategory.None
        };
    }

    private async Task SaveJobsToDb(List<JobResultDto> jobs, Guid userId, CancellationToken ct)
    {
        await jobService.AddJobs(jobs, userId, ct);
    }

    private async Task<List<User>> GetActiveUsers(CancellationToken ct)
    {
        return await userRepository.GetActiveUsersAsync(ct);
    }
}