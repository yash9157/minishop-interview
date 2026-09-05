using MiniShop.Domain.Shared;

namespace MiniShop.Domain;

public sealed class Product : IMultiTenant
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public long CategoryId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
