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
    [HttpPost("login")]
    public Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken) =>
        authAppService.LoginAsync(request, cancellationToken);

    [Authorize]
    [HttpGet("me")]
    public Task<CurrentUserDto> Me(CancellationToken cancellationToken)
        => authAppService.GetCurrentAsync(User.GetUserId(), cancellationToken);
}
