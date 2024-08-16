using JobHunter.Domain.Job.Services;
using Quartz;

namespace JobHunter.Worker.Jobs;

[DisallowConcurrentExecution]
public class BackgroundJobWorker(IBackgroundJobService backgroundJobService,ILogger<BackgroundJobWorker>logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await backgroundJobService.Execute(context.CancellationToken);
        }
        catch (Exception e)
        {
            logger.LogWarning(e,"JobFailed");
        }
    }
}