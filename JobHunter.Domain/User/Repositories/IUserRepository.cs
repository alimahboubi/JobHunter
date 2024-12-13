using JobHunter.Domain.User.Dtos;

namespace JobHunter.Domain.User.Repositories;

public interface IUserRepository
{
    Task AddAsync(Entities.User user, CancellationToken ct = default);

    Task<(List<Entities.User> users, int totalUsers)> GetUsersPaginationAsync(GetPaginatedUserRequestDto requestDto,
        CancellationToken ct);

    Task<List<Entities.User>> GetActiveUsersAsync(CancellationToken ct);
    Task<List<Entities.User>> GetUsersAsync(CancellationToken ct);
    Task<bool> RemoveByIdAsync(Guid id, CancellationToken ct = default);
    Task<Entities.User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    Task<bool> UpdateAsync(Entities.User user, CancellationToken ct = default);
}