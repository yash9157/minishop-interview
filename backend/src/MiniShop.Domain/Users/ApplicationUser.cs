using Microsoft.AspNetCore.Identity;

namespace MiniShop.Domain;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
}
