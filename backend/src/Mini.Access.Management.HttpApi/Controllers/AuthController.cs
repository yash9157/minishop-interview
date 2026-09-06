using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mini.Access.Management.Application.Contracts;

namespace Mini.Access.Management.HttpApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthAppService authAppService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public Task<AuthResponse> Login(LoginRequest request) =>
        authAppService.LoginAsync(request);

    [Authorize]
    [HttpGet("me")]
    public Task<CurrentUserDto> Me()
        => authAppService.GetCurrentAsync(User.GetUserId());
}
