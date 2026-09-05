using MiniShop.Domain.Shared;

namespace MiniShop.Domain;

public sealed class Category : IMultiTenant
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = [];
}
