using System.ComponentModel;

namespace JobHunter.Domain.Job.Dto;

public record GetPaginatedJobRequestDto(
    int CurrentPage,
    int PageSize,
    bool? IsApplied,
    List<SortModel> SortProperties);