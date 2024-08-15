using JobHunter.Domain.Job.Enums;

namespace JobHunter.Domain.Job.Dto;

public record TargetPositionDto
{
    public string JobTitle { get; set; }
    public JobCategory JobCategory { get; set; }
    public List<string> TargetLocations { get; set; }
    public List<string> TargetKeywords { get; set; }
    public List<string> MustHaveKeywords { get; set; }
}