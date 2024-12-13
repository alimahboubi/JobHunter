using AutoMapper;
using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.User.Dtos;
using JobHunter.Domain.User.Entities;
using JobHunter.Domain.User.Exceptions;
using JobHunter.Domain.User.Repositories;
using JobHunter.Domain.User.Services;

namespace JobHunter.Application.Services;

public class UserService(IUserRepository userRepository, IMapper mapper) : IUserService
{
    public async Task<PaginationResultDto<GetPaginatedUserResponseDto>> GetUsersPagination(
        GetPaginatedUserRequestDto requestDto, CancellationToken ct)
    {
        var (users, totalUsers) = await userRepository.GetUsersPaginationAsync(requestDto, ct);
        var userDtos = mapper.Map<List<GetPaginatedUserResponseDto>>(users);
        return new PaginationResultDto<GetPaginatedUserResponseDto>
        {
            Data = userDtos,
            TotalItems = totalUsers,
            CurrentPage = requestDto.CurrentPage
        };
    }

    public async Task<bool> RemoveById(Guid id, CancellationToken ct)
    {
        return await userRepository.RemoveByIdAsync(id, ct);
    }

    public async Task<GetUserResponseDto> GetUserById(Guid id, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(id, ct);
        if(user is null)
        {
            throw new UserNotFoundException();
        }
        return mapper.Map<GetUserResponseDto>(user);
    }

    public async Task<Guid> AddUser(CreateUserRequestDto addRequestDto, CancellationToken ct)
    {
        var user = mapper.Map<User>(addRequestDto);
        await userRepository.AddAsync(user, ct);
        return user.Id;
    }

    public async Task<bool> UpdateUser(UpdateUserRequestDto updateRequestDto, CancellationToken ct)
    {
        var userNewValue = mapper.Map<User>(updateRequestDto);

        var user = await userRepository.GetByIdAsync(updateRequestDto.Id, ct);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        user.JobTarget = userNewValue.JobTarget;
        user.Name= userNewValue.Name;
        user.Resume = userNewValue.Resume;
        user.IsEnabled = userNewValue.IsEnabled;
        user.LastModificationDateTime=DateTime.UtcNow;
        return await userRepository.UpdateAsync(user, ct);
    }
}