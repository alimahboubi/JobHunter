namespace JobHunter.Domain.User.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsEnabled { get; set; }
    public required string Resume { get; set; }
    public UserJobTarget JobTarget { get; set; }
    public DateTime CreationDateTime { get; set; }
    public DateTime LastModificationDateTime { get; set; }
    public bool IsDeleted { get; set; }
}