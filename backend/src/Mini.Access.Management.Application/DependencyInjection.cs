using Microsoft.Extensions.DependencyInjection;
using Mini.Access.Management.Application.Contracts;

namespace Mini.Access.Management.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddScoped<JwtTokenService>()
        .AddScoped<IAuthAppService, AuthAppService>()
        .AddScoped<AuditWriter>()
        .AddScoped<IUserAppService, UserAppService>()
        .AddScoped<IRolePermissionAppService, RolePermissionAppService>()
        .AddScoped<IAccessRequestAppService, AccessRequestAppService>()
        .AddScoped<IAuditLogAppService, AuditLogAppService>()
        .AddScoped<IDashboardAppService, DashboardAppService>();
}
