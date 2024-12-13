using AutoMapper;
using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.User.Dtos;
using JobHunter.Domain.User.Entities;

namespace JobHunter.Application.Mapping;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<CreateUserRequestDto, User>()
            .ForMember(e => e.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(e => e.CreationDateTime, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(e => e.LastModificationDateTime, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(e => e.JobTarget, opt => opt.MapFrom(o => new UserJobTarget
            {
                JobTitle = o.TargetJobTitle,
                JobCategory = o.TargetJobCategory,
                TargetLocations = o.TargetJobLocations,
                TargetKeywords = o.TargetJobKeywords,
                EssentialKeywords = o.TargetJobEssentialKeywords
            }));
        CreateMap<UpdateUserRequestDto, User>()
            .ForMember(e => e.CreationDateTime, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(e => e.LastModificationDateTime, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(e => e.JobTarget, opt => opt.MapFrom(o => new UserJobTarget
            {
                JobTitle = o.TargetJobTitle,
                JobCategory = o.TargetJobCategory,
                TargetLocations = o.TargetJobLocations,
                TargetKeywords = o.TargetJobKeywords,
                EssentialKeywords = o.TargetJobEssentialKeywords
            }));
        CreateMap<User, GetUserResponseDto>()
            .ForMember(e => e.TargetJobTitle, opt => opt.MapFrom(o => o.JobTarget.JobTitle))
            .ForMember(e => e.TargetJobCategory, opt => opt.MapFrom(o => o.JobTarget.JobCategory))
            .ForMember(e => e.TargetJobLocations, opt => opt.MapFrom(o => o.JobTarget.TargetLocations))
            .ForMember(e => e.TargetJobKeywords, opt => opt.MapFrom(o => o.JobTarget.TargetKeywords))
            .ForMember(e => e.TargetJobEssentialKeywords, opt => opt.MapFrom(o => o.JobTarget.EssentialKeywords));
        CreateMap<User, GetPaginatedUserResponseDto>();
    }
}