using MiniShop.Domain.Shared;

namespace MiniShop.Domain;

public sealed class OrderItem : IMultiTenant
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;

    public Tenant Tenant { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
