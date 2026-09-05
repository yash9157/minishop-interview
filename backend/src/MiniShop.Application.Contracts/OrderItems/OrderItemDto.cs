namespace MiniShop.Application.Contracts;

public sealed record OrderItemDto(
    long Id,
    long ProductId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
