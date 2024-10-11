namespace JobHunter.Domain.Job.Entities;

public class Job
{
    public int Id { get; set; }
    public required Guid UserId { get; set; }
    public required string Source { get; set; }
    public required string SourceId { get; set; }
    public required string Title { get; set; }
    public required string Company { get; set; }
    public required string Location { get; set; }
    public required string Url { get; set; }
    public DateTime PostedDate { get; set; }
    public string? EmploymentType { get; set; }
    public string? LocationType { get; set; }
    public string? NumberOfEmployees { get; set; }
    public string? JobDescription { get; set; }
    public string Keywords { get; set; }
    public bool IsApplied { get; set; }
    public float? MatchAccuracy { get; set; }
}