namespace JobHunter.Infrastructure.Linkedin.Models;

public class JobDescriptionResultDto
{
    public string SeniorityLevel { get; set; } = "N/A";
    public string Description { get; set; } = "N/A";
    public string LocationType { get; set; } = "N/A";
    public List<string> Keywords { get; set; }
}