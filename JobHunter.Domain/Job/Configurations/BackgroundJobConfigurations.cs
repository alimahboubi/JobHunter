namespace JobHunter.Domain.Job.Configurations;

public class BackgroundJobConfigurations
{
    public int CrawlerRepeatInterval { get; set; }
    public int AnalyzerRepeatInterval { get; set; }
    public int AnalyzerCheckpointCount { get; set; }
}