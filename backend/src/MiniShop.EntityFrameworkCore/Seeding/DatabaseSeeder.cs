using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniShop.Domain;
using MiniShop.Domain.Shared;

namespace MiniShop.EntityFrameworkCore;

public sealed class DatabaseSeeder(
    MiniShopDbContext dbContext,
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager)
{
    private const long MiniShopTenantId = 1;
    private const long NovaMartTenantId = 2;

    public async Task SeedAsync()
    {
        await dbContext.Database.MigrateAsync();
        await SeedTenantsAsync();
        await SeedRolesAsync();
        await SeedAdminAsync(MiniShopTenantId, "MiniShop Admin", "admin@minishop.local");
        await SeedAdminAsync(NovaMartTenantId, "NovaMart Admin", "admin@novamart.local");
        await SeedDemoDataAsync();
    }

    private async Task SeedTenantsAsync()
    {
        if (!await dbContext.Tenants.AnyAsync(tenant => tenant.Id == MiniShopTenantId))
            dbContext.Tenants.Add(new Tenant
            {
                Id = MiniShopTenantId,
                Code = "minishop",
                Name = "MiniShop"
            });

        if (!await dbContext.Tenants.AnyAsync(tenant => tenant.Id == NovaMartTenantId))
            dbContext.Tenants.Add(new Tenant
            {
                Id = NovaMartTenantId,
                Code = "novamart",
                Name = "NovaMart"
            });

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in new[] { Roles.Admin, Roles.User })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }

    private async Task SeedAdminAsync(long tenantId, string fullName, string email)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FullName = fullName,
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, "Admin@12345");
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));

        await userManager.AddToRoleAsync(admin, Roles.Admin);
    }

    private async Task SeedDemoDataAsync()
    {
        if (!await dbContext.Categories.IgnoreQueryFilters()
            .AnyAsync(category => category.TenantId == MiniShopTenantId))
        {
            var electronics = new Category
            {
                TenantId = MiniShopTenantId,
                Name = "Electronics",
                Description = "Everyday devices and accessories"
            };
            var books = new Category
            {
                TenantId = MiniShopTenantId,
                Name = "Books",
                Description = "Technology and business reading"
            };

            dbContext.Products.AddRange(
                new Product
                {
                    TenantId = MiniShopTenantId,
                    Category = electronics,
                    Sku = "ELEC-001",
                    Name = "Mechanical Keyboard",
                    Price = 79.99m,
                    StockQuantity = 25
                },
                new Product
                {
                    TenantId = MiniShopTenantId,
                    Category = electronics,
                    Sku = "ELEC-002",
                    Name = "Wireless Mouse",
                    Price = 39.50m,
                    StockQuantity = 40
                },
                new Product
                {
                    TenantId = MiniShopTenantId,
                    Category = books,
                    Sku = "BOOK-001",
                    Name = "Clean Architecture Notes",
                    Price = 29m,
                    StockQuantity = 15
                });
            dbContext.Customers.Add(new Customer
            {
                TenantId = MiniShopTenantId,
                Name = "Demo Customer",
                Email = "customer@example.com",
                Phone = "+91 9999999999"
            });
        }

        if (!await dbContext.Categories.IgnoreQueryFilters()
            .AnyAsync(category => category.TenantId == NovaMartTenantId))
        {
            var fashion = new Category
            {
                TenantId = NovaMartTenantId,
                Name = "Fashion",
                Description = "NovaMart clothing and accessories"
            };
            dbContext.Products.AddRange(
                new Product
                {
                    TenantId = NovaMartTenantId,
                    Category = fashion,
                    Sku = "NOVA-001",
                    Name = "Classic Backpack",
                    Price = 49.99m,
                    StockQuantity = 30
                },
                new Product
                {
                    TenantId = NovaMartTenantId,
                    Category = fashion,
                    Sku = "NOVA-002",
                    Name = "Travel Jacket",
                    Price = 89.50m,
                    StockQuantity = 20
                });
            dbContext.Customers.Add(new Customer
            {
                TenantId = NovaMartTenantId,
                Name = "Nova Customer",
                Email = "customer@novamart.local",
                Phone = "+91 8888888888"
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
