namespace MiniShop.Application.Contracts;

public sealed record ProductDto(
    long Id,
    long CategoryId,
    string CategoryName,
    string Sku,
    string Name,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    bool HasImage);
