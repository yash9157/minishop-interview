namespace MiniShop.Application.Contracts;

public sealed class ProductQuery : PagedRequest
{
    public long? CategoryId { get; init; }
    public bool? IsActive { get; init; }
    public string SortBy { get; init; } = "name";
    public bool Descending { get; init; }
}
