using AntDesign;
using AntDesign.TableModels;
using JobHunter.Domain.User.Dtos;
using JobHunter.Domain.User.Exceptions;
using JobHunter.Domain.User.Services;
using JobHunter.WebApp.Components.Helpers;
using Microsoft.AspNetCore.Components;

namespace JobHunter.WebApp.Components.Pages;

partial class UserUpdate
{
    [Inject]
    public IServiceProvider ServiceProvider { get; set; }

    [Inject]
    public ModalService ModalService { get; set; }

    [Inject]
    public ConfirmService ComfirmService { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Parameter]
    public Guid Id { get; set; }


    private GetUserResponseDto _userModel { get; set; } = new()
    {
        TargetWorkTypes = new()
    };
    private bool isLoading;
    private DynamicTagCollection _locationTagCollection = new(new());
    private DynamicTagCollection _keywordTagCollection = new(new());
    private DynamicTagCollection _essentialKeywordTagCollection = new(new());

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        await FetchJobData(Id);
        isLoading = false;
    }

    private async Task FetchJobData(Guid id)
    {
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            _userModel = await userService.GetUserById(id, default);
            _locationTagCollection.SetItems(_userModel.TargetJobLocations);
            _keywordTagCollection.SetItems(_userModel.TargetJobKeywords);
            _essentialKeywordTagCollection.SetItems(_userModel.TargetJobEssentialKeywords);
        }
        catch (UserNotFoundException)
        {
            NavigationManager.NavigateTo("/NotFoundError");
        }
    }

    async Task Save()
    {
        using var scope = ServiceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var toBeUpdated = new UpdateUserRequestDto(
            Id,
            _userModel.Name,
            _userModel.IsEnabled,
            _userModel.Resume,
            _userModel.TargetJobTitle,
            _userModel.TargetJobCategory,
            _userModel.TargetJobLocations,
            _userModel.TargetWorkTypes,
            _userModel.TargetJobKeywords,
            _userModel.TargetJobEssentialKeywords);
        var result = await userService.UpdateUser(toBeUpdated, default);

        if (result)
        {
            NavigationManager.NavigateTo("/users");
        }
    }

    async Task Delete()
    {
        using var scope = ServiceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var result = await userService.RemoveById(Id, default);

        if (result)
        {
            NavigationManager.NavigateTo("/users");
        }
    }
    
    void OnChangeCheckbox(string[] checkedValues)
    {
        _userModel.TargetWorkTypes = checkedValues.ToList();
    }
}