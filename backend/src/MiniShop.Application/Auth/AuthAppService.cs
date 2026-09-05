using Microsoft.AspNetCore.Identity;
using MiniShop.Application.Contracts;
using MiniShop.Domain;
using MiniShop.Domain.Shared;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class AuthAppService(
    UserManager<ApplicationUser> userManager,
    JwtTokenService jwtTokenService,
    MiniShopDbContext dbContext) : IAuthAppService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive || user.IsDeleted ||
            !await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException("Invalid email or password.");

        var response = await jwtTokenService.CreateAsync(user, cancellationToken);
        dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "Login",
            Entity = "User",
            EntityId = user.Id.ToString()
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task<CurrentUserDto> GetCurrentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User was not found.");
        var roles = await userManager.GetRolesAsync(user);
        return new CurrentUserDto(user.Id, user.FullName, user.Email!, roles.ToArray());
    }
}
