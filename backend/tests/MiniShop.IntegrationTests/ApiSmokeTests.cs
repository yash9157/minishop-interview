using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniShop.Application.Contracts;
using MiniShop.EntityFrameworkCore;
using Testcontainers.MySql;

namespace MiniShop.IntegrationTests;

public sealed class ApiSmokeTests : IAsyncLifetime
{
    private readonly MySqlContainer _mysql = new MySqlBuilder("mysql:8.4")
        .WithDatabase("minishop_tests")
        .WithUsername("minishop_test")
        .WithPassword("Test_password_123!")
        .Build();

    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        await _mysql.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = _mysql.GetConnectionString()
                }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<MiniShopDbContext>>();
                services.AddDbContext<MiniShopDbContext>(options =>
                    options.UseMySQL(_mysql.GetConnectionString()));
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AdminCanLoginAndReadSeededProducts()
    {
        var client = _client ?? throw new InvalidOperationException("The test client was not initialized.");
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            TenantCode = "minishop",
            Email = "admin@minishop.local",
            Password = "Admin@12345"
        });

        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(login);
        var loginResult = login!;
        Assert.Contains("Admin", loginResult.User.Roles);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
        var productsResponse = await client.GetAsync("/api/products?page=1&pageSize=10");

        productsResponse.EnsureSuccessStatusCode();
        var products = await productsResponse.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        Assert.NotNull(products);
        Assert.Equal(3, products.TotalCount);
        Assert.Contains(products.Items, product => product.Sku == "BOOK-001");
    }

    [Fact]
    public async Task TenantFilterKeepsBrandCatalogsSeparate()
    {
        var client = _client ?? throw new InvalidOperationException("The test client was not initialized.");
        var tenants = await client.GetFromJsonAsync<IReadOnlyList<TenantDto>>("/api/auth/tenants");
        Assert.NotNull(tenants);
        Assert.Contains(tenants, tenant => tenant.Code == "minishop");
        Assert.Contains(tenants, tenant => tenant.Code == "novamart");

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            TenantCode = "novamart",
            Email = "admin@novamart.local",
            Password = "Admin@12345"
        });
        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(login);
        Assert.Equal("NovaMart", login.User.TenantName);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var products = await client.GetFromJsonAsync<PagedResult<ProductDto>>(
            "/api/products?page=1&pageSize=10");

        Assert.NotNull(products);
        Assert.Equal(2, products.TotalCount);
        Assert.All(products.Items, product => Assert.StartsWith("NOVA-", product.Sku));
        Assert.DoesNotContain(products.Items, product => product.Sku == "BOOK-001");
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory is not null)
            await _factory.DisposeAsync();
        await _mysql.DisposeAsync();
    }
}
