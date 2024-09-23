using System.ComponentModel;

namespace JobHunter.Domain.Job.Dto;

public record SortModel
{
    public string Sort { get; set; }

    public int Priority { get; set; }

    public string FieldName { get; set; }
}