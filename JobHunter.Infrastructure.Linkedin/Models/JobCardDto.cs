namespace JobHunter.Infrastructure.Linkedin.Models;

public class JobCardDto
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Url { get; set; }
    public DateTime PostedDate { get; set; }
    public string? EmploymentType { get; set; }
    public string? Description { get; set; }
}