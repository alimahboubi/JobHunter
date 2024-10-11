namespace JobHunter.Domain.Job.Configurations;

public class JobTargetSettings
{
    public string JobTitle { get; set; }
    public string JobCategory { get; set; }
    public List<string> TargetLocations { get; set; }
    public List<string> TargetKeywords { get; set; }
    public List<string> MustHaveKeywords { get; set; }
}