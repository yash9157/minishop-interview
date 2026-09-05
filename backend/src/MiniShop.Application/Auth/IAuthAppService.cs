using MiniShop.Application.Contracts;

namespace MiniShop.Application;

public interface IAuthAppService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<CurrentUserDto> GetCurrentAsync(Guid userId, CancellationToken cancellationToken);
}
