using JobHunter.Domain.Job.Dto;

namespace JobHunter.Domain.User.Dtos;

public record GetPaginatedUserResponseDto(
    Guid Id,
    string Name,
    DateTime LastModificationDateTime);