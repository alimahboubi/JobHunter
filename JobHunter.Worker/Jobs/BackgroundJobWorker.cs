using JobHunter.Domain.Job.Services;
using Quartz;

namespace JobHunter.Worker.Jobs;

[DisallowConcurrentExecution]
public class BackgroundJobWorker(IBackgroundJobService backgroundJobService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await backgroundJobService.Execute(context.CancellationToken);
    }
}