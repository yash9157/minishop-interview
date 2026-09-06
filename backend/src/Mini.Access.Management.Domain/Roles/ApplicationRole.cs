using Microsoft.AspNetCore.Identity;

namespace Mini.Access.Management.Domain;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public bool IsRequestable { get; set; }
}
