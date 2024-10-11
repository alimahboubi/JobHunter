namespace JobHunter.Domain.Job.Dto;

public class PaginationResultDto<T>
{
    public List<T> Data { get; set; }
    public int CurrentPage { get; set; }
    public int TotalItems { get; set; }
}