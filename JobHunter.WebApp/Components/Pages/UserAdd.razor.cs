using AntDesign;
using AntDesign.TableModels;
using JobHunter.Domain.User.Dtos;
using JobHunter.Domain.User.Exceptions;
using JobHunter.Domain.User.Services;
using JobHunter.WebApp.Components.Helpers;
using Microsoft.AspNetCore.Components;

namespace JobHunter.WebApp.Components.Pages;

partial class UserAdd
{
    [Inject]
    public IServiceProvider ServiceProvider { get; set; }

    [Inject]
    public ModalService ModalService { get; set; }

    [Inject]
    public ConfirmService ComfirmService { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    private GetUserResponseDto _userModel { get; set; } = new();
    private bool isLoading;
    private DynamicTagCollection _locationTagCollection = new(new());
    private DynamicTagCollection _keywordTagCollection = new(new());
    private DynamicTagCollection _essentialKeywordTagCollection = new(new());

    async Task Save()
    {
        using var scope = ServiceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var toBeCreatedUseer = new CreateUserRequestDto(
            _userModel.Name,
            _userModel.IsEnabled,
            _userModel.Resume,
            _userModel.TargetJobTitle,
            _userModel.TargetJobCategory,
            _locationTagCollection.lstTags,
            _userModel.TargetWorkTypes,
            _keywordTagCollection.lstTags,
            _essentialKeywordTagCollection.lstTags);
        var result = await userService.AddUser(toBeCreatedUseer, default);

        if (result != Guid.Empty)
        {
            NavigationManager.NavigateTo("/users");
        }
    }
    
    void OnChangeCheckbox(string[] checkedValues)
    {
        _userModel.TargetWorkTypes = checkedValues.ToList();
    }
}