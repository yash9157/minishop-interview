using MiniShop.Application.Contracts;

namespace MiniShop.Application;

public interface IAuthAppService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<CurrentUserDto> GetCurrentAsync(Guid userId, CancellationToken cancellationToken);
}
