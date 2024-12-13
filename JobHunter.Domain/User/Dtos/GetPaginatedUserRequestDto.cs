using JobHunter.Domain.Job.Dto;

namespace JobHunter.Domain.User.Dtos;

public record GetPaginatedUserRequestDto(
    int CurrentPage,
    int PageSize,
    bool? IsEnabled,
    List<SortModel> SortProperties);