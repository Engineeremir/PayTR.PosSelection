using PayTR.PosSelection.Shared.Filters;

namespace PayTR.PosSelection.Shared.Models;

public static class PaginationHelper
{
    public static int DefaultPage => 1;
    public static int DefaultPageSize => 1;

    public static int CalculateTake(BaseFilter filter)
    {
        var pageSize = filter.PageSize is null or <= 0 ? DefaultPageSize : filter.PageSize;

        return (int)pageSize;
    }

    public static int CalculateSkip(int? pageSize, int? page)
    {
        page = page is null or <= 0 ? DefaultPage : page;
        pageSize = pageSize is null or <= 0 ? DefaultPageSize : pageSize;

        return (int)(pageSize * (page - 1));
    }

    public static int CalculateSkip(BaseFilter filter)
    {
        return CalculateSkip(filter.PageSize, filter.Page);
    }

    public static async Task<PagedResults<T>> CreatePagedResults<T>(List<T> results, int page, int pageSize, int totalNumberOfRecords)
    {
        var mod = totalNumberOfRecords % pageSize;
        var totalPageCount = totalNumberOfRecords / pageSize + (mod == 0 ? 0 : 1);

        return await Task.FromResult(new PagedResults<T>
        {
            Results = results,
            PageNumber = page,
            PageSize = pageSize,
            TotalNumberOfRecords = totalNumberOfRecords,
            TotalNumberOfPages = totalPageCount
        });
    }
}
