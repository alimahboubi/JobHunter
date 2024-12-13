using JobHunter.Domain.Job.Configurations;
using JobHunter.Domain.Job.Entities;
using JobHunter.Domain.Job.Exceptions;
using JobHunter.Domain.Job.Repositories;
using JobHunter.Domain.Job.Services;
using JobHunter.Domain.User.Entities;
using JobHunter.Domain.User.Repositories;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace JobHunter.Application.Services;

public class JobAnalysisBackgroundService(
    IJobRepository jobRepository,
    IProceedJobCheckpointRepository proceedJobCheckpointRepository,
    ILogger<JobAnalysisBackgroundService> logger,
    IJobAnalyzerService jobAnalyzerService,
    IUserRepository userRepository,
    BackgroundJobConfigurations backgroundJobConfigurations,
    Tracer tracer)
    : IJobAnalysisBackgroundService
{
    private const string ServiceName = "OpenAIAnalyzer";

    public async Task Execute(CancellationToken cancellationToken)
    {
        var currentSpan = Tracer.CurrentSpan;
        var latestCheckpoint = await GetLatestProceedJob(cancellationToken);
        currentSpan.SetAttribute("Checkpoint", latestCheckpoint);
        try
        {
            var jobs = await jobRepository.GetUnprocessedJobsAfterCheckpoint(
                backgroundJobConfigurations.AnalyzerCheckpointCount, latestCheckpoint,
                cancellationToken);
            var users = await userRepository.GetUsersAsync(cancellationToken);
            foreach (var job in jobs)
            {
                await ProcessJob(users,job, cancellationToken);
                latestCheckpoint++;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Execute failed");
            currentSpan.SetStatus(Status.Error);
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

    private async Task ProcessJob(List<User> users, Job job, CancellationToken cancellationToken)
    {
        using var span = tracer.StartActiveSpan("ProcessJob");
        try
        {
            var userConfig = users.FirstOrDefault(e => e.Id == job.UserId);
            if (userConfig is null) return;
            span.SetAttribute("Job Id", job.Id);
            span.SetAttribute("Job title", job.Title);
            var result = await jobAnalyzerService.AnalyzeJob(job.Title, job.JobDescription,
                userConfig.Resume, cancellationToken);
            await UpdateJobs(job.Id, result, cancellationToken);
        }
        catch (InvalidJobDescription ex)
        {
            span.SetStatus(Status.Error);
            logger.LogError(ex, "invalid job description for fon id : {id}", job.Id);
        }
        catch (Invalidresume ex)
        {
            span.SetStatus(Status.Error);
            logger.LogError(ex, "File path not found");
        }
        catch (CalculateMatchPercentageException ex)
        {
            span.SetStatus(Status.Error);
            logger.LogError(ex, "{id}", job.Id);
        }
    }

    private async Task UpdateJobs(int jobId, float matchAccuracy, CancellationToken ct)
    {
        await jobRepository.UpdateMatchAccuracy(id: jobId, accuracy: matchAccuracy, ct: ct);
    }
}