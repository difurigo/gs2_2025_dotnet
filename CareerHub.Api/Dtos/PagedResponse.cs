namespace CareerHub.Api.Dtos;

public class PagedResponse<T>
{
    public required IEnumerable<T> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<LinkDto> Links { get; set; } = new();
}
