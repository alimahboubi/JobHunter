namespace JobHunter.Domain.User.Entities;

public class UserJobTarget
{
    public required string JobTitle { get; set; }
    public required string JobCategory { get; set; }
    public List<string> TargetLocations { get; set; } = new();
    public List<string> TargetKeywords { get; set; } = new();
    public List<string> EssentialKeywords { get; set; } = new();
}