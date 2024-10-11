namespace JobHunter.Domain.Job.Dto;

public class JobResultDto
{
    public string Id { get; set; }
    public string? Title { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Url { get; set; }
    public DateTime PostedDate { get; set; }
    public string? EmploymentType { get; set; }
    public string? LocationType { get; set; }
    public string? NumberOfEmployees { get; set; }
    public string? JobDescription { get; set; }
    public string? SeniorityLevel { get; set; }
    public List<string> Keywords { get; set; }
}