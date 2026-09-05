using Microsoft.Extensions.DependencyInjection;
namespace MiniShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddScoped<IJwtTokenService, JwtTokenService>()
        .AddScoped<IAuthAppService, AuthAppService>()
        .AddScoped<AuditWriter>()
        .AddScoped<UserRoleAppService>()
        .AddScoped<AccessRequestAppService>()
        .AddScoped<DashboardAppService>();
}
