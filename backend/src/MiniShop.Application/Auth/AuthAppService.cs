using Microsoft.AspNetCore.Identity;
using MiniShop.Application.Contracts;
using MiniShop.Domain;
using MiniShop.Domain.Shared;

namespace MiniShop.Application;

public sealed class AuthAppService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService) : IAuthAppService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(email) is not null)
            throw new ConflictException("An account with this email already exists.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new BusinessException(string.Join(" ", result.Errors.Select(error => error.Description)));

        await userManager.AddToRoleAsync(user, Roles.User);
        return await jwtTokenService.CreateAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException("Invalid email or password.");

        return await jwtTokenService.CreateAsync(user, cancellationToken);
    }

    public async Task<CurrentUserDto> GetCurrentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User was not found.");
        var roles = await userManager.GetRolesAsync(user);
        return new CurrentUserDto(user.Id, user.FullName, user.Email!, roles.ToArray());
    }
}
