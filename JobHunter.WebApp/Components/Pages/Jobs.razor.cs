using AntDesign;
using AntDesign.TableModels;
using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.Job.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace JobHunter.WebApp.Components.Pages;

partial class Jobs
{
    [Inject]
    public IServiceProvider ServiceProvider { get; set; }

    [Inject]
    public ModalService ModalService { get; set; }

    [Inject]
    public ConfirmService ComfirmService { get; set; }

    RadzenDataGrid<GetPaginatedJobResponseDto> grid;
    private int _pageSize = 50;
    bool isLoading;
    private int _pageIndex = 1;
    private bool _isAppliedJob = false;
    ITable _table;

    private PaginationResultDto<GetPaginatedJobResponseDto> ResponseDto { get; set; } =
        new()
        {
            Data = new(),
            TotalItems = 10
        };

    private async Task OnChange(QueryModel<GetPaginatedJobResponseDto> query)
    {
        isLoading = true;
        var isApplied = (bool?)query.FilterModel?.FirstOrDefault(e => e.FieldName == "IsApplied")?.Filters
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

    private async Task CheckedChanged(int rowId,bool isChecked)
    {
        using var scope = ServiceProvider.CreateScope();
        var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
        var result =await jobService.UpdateAppliedState(rowId, isChecked, default);
        if (result)
            ResponseDto.Data.First(e => e.Id == rowId).IsApplied = isChecked;
        
        //_table.ReloadData();
    }

    private async Task FetchJobData(int pageIndex, int pageSize, bool? isApplied, List<SortModel> sort)
    {
        using var scope = ServiceProvider.CreateScope();
        var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
        ResponseDto = await jobService.GetJob(
            new GetPaginatedJobRequestDto(CurrentPage: pageIndex, PageSize: pageSize, IsApplied: isApplied,
                sort),
            default);
    }
    
    async Task Delete(GetPaginatedJobResponseDto row)
    {
        using var scope = ServiceProvider.CreateScope();
        var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
        var result = await jobService.RemoveById(row.Id, default);

        if (result)
        {
            ResponseDto.Data.Remove(row);
            _table.ReloadData();
        }
    }
}