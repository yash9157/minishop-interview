using Microsoft.Extensions.DependencyInjection;
namespace MiniShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddScoped<IJwtTokenService, JwtTokenService>()
        .AddScoped<IAuthAppService, AuthAppService>()
        .AddScoped<ICategoryAppService, CategoryAppService>()
        .AddScoped<IProductAppService, ProductAppService>()
        .AddScoped<IProductImageAppService, ProductImageAppService>()
        .AddScoped<ICustomerAppService, CustomerAppService>()
        .AddScoped<IOrderAppService, OrderAppService>();
}
