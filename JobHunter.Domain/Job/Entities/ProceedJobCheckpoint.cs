namespace JobHunter.Domain.Job.Entities;

public class ProceedJobCheckpoint
{
    public int Id { get; set; }
    public string ServiceName { get; set; }
    public int Checkpoint { get; set; }
    public DateTime TimeStamp { get; set; }
}