using JobHunter.Domain.Job.Configurations;
using JobHunter.Domain.Job.Entities;
using JobHunter.Domain.Job.Exceptions;
using JobHunter.Domain.Job.Repositories;
using JobHunter.Domain.Job.Services;
using Microsoft.Extensions.Logging;

namespace JobHunter.Application.Services;

public class JobAnalysisBackgroundService(
    IJobRepository jobRepository,
    IProceedJobCheckpointRepository proceedJobCheckpointRepository,
    ILogger<JobAnalysisBackgroundService> logger,
    IJobAnalyzerService jobAnalyzerService,
    UsersConfigurations userConfigurations)
    : IJobAnalysisBackgroundService
{
    private const string ServiceName = "OpenAIAnalyzer";
    private const int JobCount = 10;

    public async Task Execute(CancellationToken cancellationToken)
    {
        var latestCheckpoint = await GetLatestProceedJob(cancellationToken);
        try
        {
            var jobs = await jobRepository.GetUnprocessedJobsAfterCheckpoint(JobCount, latestCheckpoint,
                cancellationToken);
            foreach (var job in jobs)
            {
                await ProcessJob(latestCheckpoint, job, cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Execute failed");
        }
        finally
        {
            await proceedJobCheckpointRepository.AddOrUpdate(ServiceName, latestCheckpoint, cancellationToken);
        }
    }

    private async Task<int> GetLatestProceedJob(CancellationToken ct)
    {
        var checkpoint = await proceedJobCheckpointRepository.GetAsync(ServiceName, ct);
        return checkpoint?.Checkpoint ?? 0;
    }

    private async Task ProcessJob(int latestCheckpoint, Job job, CancellationToken cancellationToken)
    {
        try
        {
            var userConfig = userConfigurations.Profiles.FirstOrDefault(e => e.Id == job.UserId);
            if (userConfig is null) return;

            var result = await jobAnalyzerService.AnalyzeJob(job.Title, job.JobDescription,
                userConfig.TextResumePath, cancellationToken);
            await UpdateJobs(job.Id, result, cancellationToken);
        }
        catch (InvalidJobDescription)
        {
            latestCheckpoint++;
        }
        catch (InvalidFilePath)
        {
            latestCheckpoint++;
        }
        catch (CalculateMatchPercentageException)
        {
            latestCheckpoint++;
        }
    }

    private async Task UpdateJobs(int jobId, float matchAccuracy, CancellationToken ct)
    {
        await jobRepository.UpdateMatchAccuracy(id: jobId, accuracy: matchAccuracy, ct: ct);
    }
}