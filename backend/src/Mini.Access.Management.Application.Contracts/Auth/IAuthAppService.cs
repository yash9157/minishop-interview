namespace Mini.Access.Management.Application.Contracts;

public interface IAuthAppService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<CurrentUserDto> GetCurrentAsync(Guid userId);
}
