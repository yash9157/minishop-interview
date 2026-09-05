using MiniShop.Domain.Shared;

namespace MiniShop.Domain;

public sealed class Customer : IMultiTenant
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = [];
}
