using AntDesign;
using AntDesign.TableModels;
using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.Job.Services;
using JobHunter.Domain.User.Dtos;
using JobHunter.Domain.User.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace JobHunter.WebApp.Components.Pages;

partial class Users
{
    [Inject]
    public IServiceProvider ServiceProvider { get; set; }

    [Inject]
    public ModalService ModalService { get; set; }

    [Inject]
    public ConfirmService ComfirmService { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    RadzenDataGrid<GetPaginatedUserResponseDto> grid;
    private int _pageSize = 50;
    bool isLoading;
    private int _pageIndex = 1;
    private bool _isEnabledUser = false;
    ITable _table;

    private PaginationResultDto<GetPaginatedUserResponseDto> ResponseDto { get; set; } =
        new()
        {
            Data = new(),
            TotalItems = 10
        };

    private async Task OnChange(QueryModel<GetPaginatedUserResponseDto> query)
    {
        isLoading = true;
        var isApplied = (bool?)query.FilterModel?.FirstOrDefault(e => e.FieldName == "IsEnabled")?.Filters
            .FirstOrDefault()?.Value;
        var sort = query.SortModel
            .Where(e => !string.IsNullOrWhiteSpace(e.Sort))
            .Select(e => new SortModel()
            {
                FieldName = e.FieldName,
                Sort = e.Sort,
                Priority = e.Priority
            }).ToList();

        await FetchJobData(query.PageIndex, query.PageSize, isApplied, sort);
        isLoading = false;
    }


    private async Task FetchJobData(int pageIndex, int pageSize, bool? isApplied, List<SortModel> sort)
    {
        using var scope = ServiceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        ResponseDto = await userService.GetUsersPagination(
            new GetPaginatedUserRequestDto(CurrentPage: pageIndex, PageSize: pageSize, IsEnabled: isApplied,
                sort),
            default);
    }

    private void OnClickAddUser()
    {
        NavigationManager.NavigateTo("/users/add");
    }

    public void EditItem(Guid id)
    {
        NavigationManager.NavigateTo($"/users/update/{id}");
    }
}