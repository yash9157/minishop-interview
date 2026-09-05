using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniShop.Application.Contracts;
using MiniShop.Domain;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class JwtTokenService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtOptions> options,
    MiniShopDbContext dbContext) : IJwtTokenService
{
    public async Task<AuthResponse> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var roles = await userManager.GetRolesAsync(user);
        var tenantName = await dbContext.Tenants
            .Where(tenant => tenant.Id == user.TenantId)
            .Select(tenant => tenant.Name)
            .SingleAsync(cancellationToken);
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(settings.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new("tenant_id", user.TenantId.ToString()),
            new("tenant_name", tenantName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(issuedAt).ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            issuedAt,
            expiresAt,
            new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            new CurrentUserDto(
                user.Id,
                user.FullName,
                user.Email!,
                user.TenantId,
                tenantName,
                roles.ToArray()));
    }
}
