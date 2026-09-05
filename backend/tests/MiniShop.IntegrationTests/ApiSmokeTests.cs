using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniShop.Application.Contracts;
using MiniShop.EntityFrameworkCore;
using Testcontainers.MySql;

namespace MiniShop.IntegrationTests;

public sealed class ApiSmokeTests : IAsyncLifetime
{
    private readonly string password = $"T!{Guid.NewGuid():N}aA1";
    private readonly MySqlContainer mysql;
    private WebApplicationFactory<Program>? factory;
    private HttpClient? client;
    private HttpClient Client => client ??= factory!.CreateClient();

    public ApiSmokeTests()
    {
        mysql = new MySqlBuilder("mysql:8.4")
            .WithDatabase("access_tests").WithUsername("test")
            .WithPassword(password).Build();
    }

    [Fact]
    public async Task AccessRequestCompletesTwoApprovalsAndProvisioning()
    {
        await LoginAsync("employee@access.local", password);
        var systems = await Client.GetFromJsonAsync<TargetSystemDto[]>("/api/target-systems");
        var roles = await Client.GetFromJsonAsync<RoleDto[]>("/api/roles");
        var maker = roles!.Single(x => x.Name == "Maker");
        var created = await (await Client.PostAsJsonAsync("/api/access-requests",
            new CreateAccessRequest
            {
                TargetSystemId = systems![0].Id,
                RequestedRoleId = maker.Id,
                BusinessJustification = "Required for daily transaction work."
            })).Content.ReadFromJsonAsync<AccessRequestDto>();
        var submitted = await (await Client.PostAsync(
            $"/api/access-requests/{created!.Id}/submit", null))
            .Content.ReadFromJsonAsync<AccessRequestDto>();
        Assert.Equal(MiniShop.Domain.Shared.AccessRequestStatus.Pending, submitted!.Status);

        await LoginAsync("manager@access.local", password);
        await Client.PostAsJsonAsync($"/api/access-requests/{created.Id}/approve",
            new ApprovalActionRequest { Remarks = "Manager approved." });
        await LoginAsync("security@access.local", password);
        var approved = await (await Client.PostAsJsonAsync(
            $"/api/access-requests/{created.Id}/approve",
            new ApprovalActionRequest { Remarks = "Security approved." }))
            .Content.ReadFromJsonAsync<AccessRequestDto>();
        Assert.Equal(MiniShop.Domain.Shared.AccessRequestStatus.Approved, approved!.Status);

        await LoginAsync("admin@access.local", password);
        var provisioned = await (await Client.PostAsync(
            $"/api/access-requests/{created.Id}/provision", null))
            .Content.ReadFromJsonAsync<AccessRequestDto>();
        Assert.Equal(MiniShop.Domain.Shared.AccessRequestStatus.Provisioned, provisioned!.Status);
    }

    public async Task InitializeAsync()
    {
        await mysql.StartAsync();
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = mysql.GetConnectionString(),
                    ["DemoPassword"] = password,
                    ["Jwt:SigningKey"] = password
                }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<MiniShopDbContext>>();
                services.AddDbContext<MiniShopDbContext>(
                    options => options.UseMySQL(mysql.GetConnectionString()));
            });
        });
    }

    private async Task LoginAsync(string email, string password)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest { Email = email, Password = password });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.AccessToken);
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (factory is not null) await factory.DisposeAsync();
        await mysql.DisposeAsync();
    }
}
