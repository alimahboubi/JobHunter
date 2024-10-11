using JobHunter.Domain.Job.Services;
using Quartz;

namespace JobHunter.Worker.Jobs;

[DisallowConcurrentExecution]
public class AnalyzerBackgroundJobWorker(IJobAnalysisBackgroundService backgroundService,ILogger<AnalyzerBackgroundJobWorker>logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await backgroundService.Execute(context.CancellationToken);
        }
        catch (Exception e)
        {
            logger.LogWarning(e,"JobFailed");
        }
    }
}