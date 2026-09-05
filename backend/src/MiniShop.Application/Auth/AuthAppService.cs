using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniShop.Application.Contracts;
using MiniShop.Domain;
using MiniShop.Domain.Shared;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class AuthAppService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    MiniShopDbContext dbContext) : IAuthAppService
{
    public async Task<IReadOnlyList<TenantDto>> GetTenantsAsync(CancellationToken cancellationToken) =>
        await dbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.IsActive)
            .OrderBy(tenant => tenant.Name)
            .Select(tenant => new TenantDto(tenant.Id, tenant.Code, tenant.Name))
            .ToListAsync(cancellationToken);

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var tenant = await FindTenantAsync(request.TenantCode, cancellationToken);
        var email = request.Email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(email) is not null)
            throw new ConflictException("An account with this email already exists.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Tenant = tenant,
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
        var tenant = await FindTenantAsync(request.TenantCode, cancellationToken);
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || user.TenantId != tenant.Id ||
            !await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException("Invalid email or password.");

        return await jwtTokenService.CreateAsync(user, cancellationToken);
    }

    public async Task<CurrentUserDto> GetCurrentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User was not found.");
        var roles = await userManager.GetRolesAsync(user);
        var tenantName = await dbContext.Tenants
            .Where(tenant => tenant.Id == user.TenantId)
            .Select(tenant => tenant.Name)
            .SingleAsync(cancellationToken);
        return new CurrentUserDto(
            user.Id,
            user.FullName,
            user.Email!,
            user.TenantId,
            tenantName,
            roles.ToArray());
    }

    private async Task<Tenant> FindTenantAsync(string code, CancellationToken cancellationToken) =>
        await dbContext.Tenants.FirstOrDefaultAsync(
            tenant => tenant.Code == code.Trim().ToLower() && tenant.IsActive,
            cancellationToken)
        ?? throw new UnauthorizedException("Tenant was not found or is inactive.");
}
