namespace BaseballApi.Contracts;

public class PagedResult<T>
{
    public int TotalCount { get; set; }
    public required List<T> Results { get; set; }
}
