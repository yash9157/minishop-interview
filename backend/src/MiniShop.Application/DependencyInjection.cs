using Microsoft.Extensions.DependencyInjection;
using MiniShop.Application.Contracts;

namespace MiniShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddScoped<JwtTokenService>()
        .AddScoped<IAuthAppService, AuthAppService>()
        .AddScoped<AuditWriter>()
        .AddScoped<IUserRoleAppService, UserRoleAppService>()
        .AddScoped<IAccessRequestAppService, AccessRequestAppService>()
        .AddScoped<IAuditLogAppService, AuditLogAppService>()
        .AddScoped<IDashboardAppService, DashboardAppService>();
}
