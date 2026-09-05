namespace MiniShop.Application.Contracts;

public interface IAuthAppService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<CurrentUserDto> GetCurrentAsync(Guid userId, CancellationToken cancellationToken);
}
