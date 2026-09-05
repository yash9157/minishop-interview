using MiniShop.Domain.Shared;

namespace MiniShop.Application.Contracts;

public sealed record OrderDetailsDto(
    long Id,
    string OrderNumber,
    long CustomerId,
    string CustomerName,
    DateTime OrderDateUtc,
    OrderStatus Status,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items);
