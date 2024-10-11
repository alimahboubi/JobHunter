namespace JobHunter.Domain.Job.Dto;

public record GetPaginatedJobResponseDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Url { get; set; }
    public DateTime PostedDate { get; set; }
    public string? SeniorityLevel { get; set; }
    public bool IsApplied { get; set; }
    public float? MatchAccuracy { get; set; }
}