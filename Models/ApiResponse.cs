namespace Waracle.Api.Models;

//Would implement pagination 
public class ApiResponse<T>
{
    public required T Data { get; set; }
    public required MetaData Meta { get; set; }

    public static ApiResponse<T> Create(T? data, int total, int page = 1, int pageSize = 10)
    {
        return new ApiResponse<T>
        {
            Data = data ?? default!, 
            Meta = new MetaData
            {
                Total = total,
                Page = page,
                PageSize = pageSize
            }
        };
    }
}

public class MetaData
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
} 