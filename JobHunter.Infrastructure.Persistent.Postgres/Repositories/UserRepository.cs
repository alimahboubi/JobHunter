using JobHunter.Domain.User.Dtos;
using JobHunter.Domain.User.Entities;
using JobHunter.Domain.User.Repositories;
using Microsoft.EntityFrameworkCore;
using JobHunter.Infrastructure.Persistent.Postgres.Extensions;

namespace JobHunter.Infrastructure.Persistent.Postgres.Repositories;

public class UserRepository(JobHunterDbContext context) : IUserRepository
{
    /// <summary>
    /// Adds a new user to the database.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        var addedUser = await context.Users.AddAsync(user, ct);
        await context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Retrieves a paginated list of users based on the provided request DTO.
    /// </summary>
    /// <param name="requestDto">The request DTO containing pagination and filter information.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A tuple containing the list of users and the total user count.</returns>
    public async Task<(List<User> users, int totalUsers)> GetUsersPaginationAsync(GetPaginatedUserRequestDto requestDto,
        CancellationToken ct)
    {
        var skip = CalculatePaginationSkip(requestDto);
        var query = ApplyFilters(context.Users.AsQueryable(), requestDto);
        var totalUsers = await query.CountAsync(ct);
        var users = await query
            .AsNoTracking()
            .Skip(skip)
            .Take(requestDto.PageSize)
            .ToListAsync(ct);
        return (users, totalUsers);
    }

    private static int CalculatePaginationSkip(GetPaginatedUserRequestDto requestDto)
    {
        var currentPage = requestDto.CurrentPage < 1 ? 1 : requestDto.CurrentPage;
        var skip = (currentPage - 1) * requestDto.PageSize;
        return skip;
    }

    /// <summary>
    /// Retrieves a list of all users.
    /// </summary>
    public async Task<List<User>> GetActiveUsersAsync(CancellationToken ct)
    {
        return await context.Users
            .Where(e => e.IsEnabled && !e.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task<List<User>> GetUsersAsync(CancellationToken ct)
    {
        return await context.Users
            .ToListAsync(ct);
    }

    /// <summary>
    /// Soft delete a user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the user was removed, otherwise false.</returns>
    public async Task<bool> RemoveByIdAsync(Guid id, CancellationToken ct = default)
    {
        await context.Users
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(calls => calls.SetProperty(e => e.IsDeleted, true), ct);
        return true;
    }

    /// <summary>
    /// Retrieves a user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The user entity if found, otherwise null.</returns>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Users
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
    {
        context.Users.Update(user);
        return await context.SaveChangesAsync(ct) > 0;
    }

    private IQueryable<User> ApplyFilters(IQueryable<User> query, GetPaginatedUserRequestDto requestDto)
    {
        if (requestDto.IsEnabled is not null)
        {
            query = query.Where(e => e.IsEnabled == requestDto.IsEnabled);
        }

        query = requestDto.SortProperties.Any()
            ? query.OrderBy(requestDto.SortProperties)
            : query.OrderByDescending(e => e.Id);

        return query;
    }
}