using Microsoft.AspNetCore.Identity;
using Mini.Access.Management.Application.Contracts;
using Mini.Access.Management.Domain;
using Mini.Access.Management.Domain.Shared;
using Mini.Access.Management.EntityFrameworkCore;

namespace Mini.Access.Management.Application;

public sealed class AuthAppService(
    UserManager<ApplicationUser> userManager,
    JwtTokenService jwtTokenService,
    AccessManagementDbContext dbContext) : IAuthAppService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        var passwordIsValid = user is not null &&
            await userManager.CheckPasswordAsync(user, request.Password);
        if (user is null || !user.IsActive || user.IsDeleted || !passwordIsValid)
        {
            var attemptedEmail = request.Email.Trim().ToLowerInvariant();
            dbContext.AuditLogs.Add(new AuditLog
            {
                UserId = user?.Id,
                Action = "LoginFailed",
                Entity = "User",
                EntityId = attemptedEmail[..Math.Min(attemptedEmail.Length, 80)]
            });
            await dbContext.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var response = await jwtTokenService.CreateAsync(user);
        dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "Login",
            Entity = "User",
            EntityId = user.Id.ToString()
        });
        await dbContext.SaveChangesAsync();
        return response;
    }

    public async Task<CurrentUserDto> GetCurrentAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User was not found.");
        var roles = await userManager.GetRolesAsync(user);
        return new CurrentUserDto(user.Id, user.FullName, user.Email!, roles.ToArray());
    }
}
