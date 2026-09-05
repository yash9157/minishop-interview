using System.ComponentModel.DataAnnotations;
using MiniShop.Domain.Shared;

namespace MiniShop.Application.Contracts;

public sealed class SaveOrderRequest
{
    [Range(typeof(long), "1", "9223372036854775807")]
    public long CustomerId { get; init; }

    public DateTime? OrderDateUtc { get; init; }

    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus Status { get; init; } = OrderStatus.Draft;

    [Required, MinLength(1)]
    public List<SaveOrderItemRequest> Items { get; init; } = [];
}
