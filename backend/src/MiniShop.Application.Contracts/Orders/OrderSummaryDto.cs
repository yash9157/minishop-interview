using MiniShop.Domain.Shared;

namespace MiniShop.Application.Contracts;

public sealed record OrderSummaryDto(
    long Id,
    string OrderNumber,
    long CustomerId,
    string CustomerName,
    DateTime OrderDateUtc,
    OrderStatus Status,
    decimal TotalAmount,
    int ItemCount);
