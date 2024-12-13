namespace JobHunter.Domain.User.Dtos;

public record GetUserResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public string Resume { get; set; }
    public string TargetJobTitle { get; set; }
    public string TargetJobCategory { get; set; }
    public List<string> TargetJobLocations { get; set; }
    public List<string> TargetJobKeywords { get; set; }
    public List<string> TargetJobEssentialKeywords { get; set; }
    public DateTime LastModificationDateTime { get; set; }
}