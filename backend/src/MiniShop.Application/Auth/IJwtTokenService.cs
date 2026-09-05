using MiniShop.Application.Contracts;
using MiniShop.Domain;

namespace MiniShop.Application;

public interface IJwtTokenService
{
    Task<AuthResponse> CreateAsync(ApplicationUser user, CancellationToken cancellationToken);
}
