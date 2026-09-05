using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Application;
using MiniShop.Application.Contracts;

namespace MiniShop.HttpApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthAppService authAppService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("tenants")]
    public Task<IReadOnlyList<TenantDto>> GetTenants(CancellationToken cancellationToken) =>
        authAppService.GetTenantsAsync(cancellationToken);

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken) =>
        StatusCode(
            StatusCodes.Status201Created,
            await authAppService.RegisterAsync(request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("login")]
    public Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken) =>
        authAppService.LoginAsync(request, cancellationToken);

    [Authorize]
    [HttpGet("me")]
    public Task<CurrentUserDto> Me(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException();

        return authAppService.GetCurrentAsync(userId, cancellationToken);
    }
}
