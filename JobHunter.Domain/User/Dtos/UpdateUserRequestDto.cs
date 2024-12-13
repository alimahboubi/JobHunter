namespace JobHunter.Domain.User.Dtos;

public record UpdateUserRequestDto(
    Guid Id,
    string Name,
    bool IsEnabled,
    string Resume,
    string TargetJobTitle,
    string TargetJobCategory,
    List<string> TargetJobLocations,
    List<string> TargetWorkTypes,
    List<string> TargetJobKeywords,
    List<string> TargetJobEssentialKeywords);