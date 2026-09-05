using System.ComponentModel.DataAnnotations;

namespace MiniShop.Application.Contracts;

public sealed class SaveOrderItemRequest
{
    [Range(typeof(long), "1", "9223372036854775807")]
    public long ProductId { get; init; }

    [Range(1, 10_000)]
    public int Quantity { get; init; }
}
