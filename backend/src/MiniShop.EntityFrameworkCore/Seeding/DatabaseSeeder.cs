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
    public async Task SeedAsync()
    {
        await dbContext.Database.MigrateAsync();
        await SeedRolesAsync();
        await SeedAdminAsync();
        await SeedDemoDataAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in new[] { Roles.Admin, Roles.User })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }

    private async Task SeedAdminAsync()
    {
        const string email = "admin@minishop.local";
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = "MiniShop Admin",
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
        if (await dbContext.Categories.AnyAsync())
            return;

        var electronics = new Category
        {
            Name = "Electronics",
            Description = "Everyday devices and accessories"
        };
        var books = new Category
        {
            Name = "Books",
            Description = "Technology and business reading"
        };

        dbContext.Products.AddRange(
            new Product
            {
                Category = electronics,
                Sku = "ELEC-001",
                Name = "Mechanical Keyboard",
                Price = 79.99m,
                StockQuantity = 25
            },
            new Product
            {
                Category = electronics,
                Sku = "ELEC-002",
                Name = "Wireless Mouse",
                Price = 39.50m,
                StockQuantity = 40
            },
            new Product
            {
                Category = books,
                Sku = "BOOK-001",
                Name = "Clean Architecture Notes",
                Price = 29m,
                StockQuantity = 15
            });
        dbContext.Customers.Add(new Customer
        {
            Name = "Demo Customer",
            Email = "customer@example.com",
            Phone = "+91 9999999999"
        });

        await dbContext.SaveChangesAsync();
    }
}
