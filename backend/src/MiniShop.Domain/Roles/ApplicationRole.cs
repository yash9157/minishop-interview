using Microsoft.AspNetCore.Identity;

namespace MiniShop.Domain;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public bool IsRequestable { get; set; }
}
