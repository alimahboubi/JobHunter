using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.User.Dtos;

namespace JobHunter.Domain.User.Services;

public interface IUserService
{
    Task<PaginationResultDto<GetPaginatedUserResponseDto>> GetUsersPagination(GetPaginatedUserRequestDto requestDto,
        CancellationToken ct);

    Task<bool> RemoveById(Guid id, CancellationToken ct);
    Task<GetUserResponseDto> GetUserById(Guid id, CancellationToken ct);
    Task<Guid> AddUser(CreateUserRequestDto addRequestDto, CancellationToken ct);
    Task<bool> UpdateUser(UpdateUserRequestDto updateRequestDto, CancellationToken ct);
}