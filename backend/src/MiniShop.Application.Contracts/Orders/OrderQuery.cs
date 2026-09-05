using MiniShop.Domain.Shared;

namespace MiniShop.Application.Contracts;

public sealed class OrderQuery : PagedRequest
{
    public long? CustomerId { get; init; }
    public OrderStatus? Status { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public bool Descending { get; init; } = true;
}
