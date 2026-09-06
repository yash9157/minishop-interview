using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mini.Access.Management.Domain;
using Mini.Access.Management.Domain.Shared;

namespace Mini.Access.Management.EntityFrameworkCore;

public sealed class DatabaseSeeder(
    AccessManagementDbContext db,
    RoleManager<ApplicationRole> roles,
    UserManager<ApplicationUser> users,
    IConfiguration configuration)
{
    public async Task SeedAsync()
    {
        await db.Database.MigrateAsync();
        await SeedRolesAsync();
        await SeedUsersAsync();
        await SeedAccessDataAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var name in Roles.PortalRoles)
            await EnsureRoleAsync(name, false);
        await EnsureRoleAsync(Roles.Maker, true);
        await EnsureRoleAsync(Roles.Checker, true);
        await EnsureRoleAsync("Viewer", true);
    }

    private async Task SeedUsersAsync()
    {
        var password = configuration["DemoPassword"]
            ?? throw new InvalidOperationException("DemoPassword is required.");
        var manager = await EnsureUserAsync(
            "manager@access.local", "Demo Manager", password, Roles.Manager);
        await EnsureUserAsync(
            "admin@access.local", "Access Admin", password, Roles.Admin, Roles.Provisioner);
        await EnsureUserAsync(
            "security@access.local", "Security Approver", password, Roles.SecurityAdmin);
        var employee = await EnsureUserAsync(
            "employee@access.local", "Demo Employee", password, Roles.Employee);
        if (employee.ManagerId != manager.Id)
        {
            employee.ManagerId = manager.Id;
            await users.UpdateAsync(employee);
        }
    }

    private async Task SeedAccessDataAsync()
    {
        if (!await db.Permissions.AnyAsync())
        {
            db.Permissions.AddRange(
                new Permission { Code = "ACCESS.READ", Name = "Read access data" },
                new Permission { Code = "ACCESS.REQUEST", Name = "Request system access" },
                new Permission { Code = "ACCESS.APPROVE", Name = "Approve access requests" },
                new Permission { Code = "ACCESS.PROVISION", Name = "Provision approved access" });
        }
        if (!await db.TargetSystems.AnyAsync())
        {
            db.TargetSystems.AddRange(
                new TargetSystem { Name = "Core Banking" },
                new TargetSystem { Name = "CRM" },
                new TargetSystem { Name = "HR Portal" });
        }
        await db.SaveChangesAsync();

        if (!await db.RolePermissions.AnyAsync())
        {
            var permissions = await db.Permissions.ToDictionaryAsync(x => x.Code);
            var maker = await roles.FindByNameAsync(Roles.Maker);
            var checker = await roles.FindByNameAsync(Roles.Checker);
            var viewer = await roles.FindByNameAsync("Viewer");
            db.RolePermissions.AddRange(
                new RolePermission { RoleId = maker!.Id, PermissionId = permissions["ACCESS.READ"].Id },
                new RolePermission { RoleId = maker.Id, PermissionId = permissions["ACCESS.REQUEST"].Id },
                new RolePermission { RoleId = checker!.Id, PermissionId = permissions["ACCESS.READ"].Id },
                new RolePermission { RoleId = checker.Id, PermissionId = permissions["ACCESS.APPROVE"].Id },
                new RolePermission { RoleId = viewer!.Id, PermissionId = permissions["ACCESS.READ"].Id });
            await db.SaveChangesAsync();
        }
    }

    private async Task EnsureRoleAsync(string name, bool requestable)
    {
        if (await roles.FindByNameAsync(name) is not null)
            return;
        var result = await roles.CreateAsync(new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsRequestable = requestable
        });
        EnsureSucceeded(result);
    }

    private async Task<ApplicationUser> EnsureUserAsync(
        string email, string fullName, string password, params string[] roleNames)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                FullName = fullName,
                EmailConfirmed = true
            };
            EnsureSucceeded(await users.CreateAsync(user, password));
        }
        foreach (var role in roleNames)
            if (!await users.IsInRoleAsync(user, role))
                EnsureSucceeded(await users.AddToRoleAsync(user, role));
        return user;
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(" ", result.Errors.Select(x => x.Description)));
    }
}
