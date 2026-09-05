using Microsoft.AspNetCore.Identity;

namespace MiniShop.Domain;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public long TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;

    public Tenant Tenant { get; set; } = null!;
}
