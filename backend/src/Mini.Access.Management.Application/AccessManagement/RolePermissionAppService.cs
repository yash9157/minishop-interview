using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mini.Access.Management.Application.Contracts;
using Mini.Access.Management.Domain;
using Mini.Access.Management.Domain.Shared;
using Mini.Access.Management.EntityFrameworkCore;

namespace Mini.Access.Management.Application;

public sealed class RolePermissionAppService(
    AccessManagementDbContext db,
    RoleManager<ApplicationRole> roles,
    AuditWriter audit) : IRolePermissionAppService
{
    public async Task<PagedResult<RoleDto>> GetRolesAsync(
        PagedRequest request)
    {
        var query = db.Roles.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Name!.Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new RoleDto(
                x.Id, x.Name!, x.IsRequestable, Roles.PortalRoles.Contains(x.Name!),
                db.RolePermissions.Where(rp => rp.RoleId == x.Id)
                    .Select(rp => rp.PermissionId).ToArray()))
            .ToArrayAsync();
        return new PagedResult<RoleDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<PagedResult<PermissionDto>> GetPermissionsAsync(
        PagedRequest request)
    {
        var query = db.Permissions.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.Code)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new PermissionDto(x.Id, x.Code, x.Name))
            .ToArrayAsync();
        return new PagedResult<PermissionDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<PermissionDto> CreatePermissionAsync(
        SavePermissionRequest request, Guid actorId)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.Permissions.AnyAsync(x => x.Code == code))
            throw new InvalidOperationException("Permission code already exists.");
        var entity = new Permission { Code = code, Name = request.Name.Trim() };
        db.Permissions.Add(entity);
        await db.SaveChangesAsync();
        audit.Add(actorId, "Create", "Permission", entity.Id, newValue: code);
        await db.SaveChangesAsync();
        return new PermissionDto(entity.Id, entity.Code, entity.Name);
    }

    public async Task<PermissionDto> UpdatePermissionAsync(
        long id, SavePermissionRequest request, Guid actorId)
    {
        var entity = await db.Permissions.FindAsync([id])
            ?? throw new KeyNotFoundException("Permission was not found.");
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.Permissions.AnyAsync(
                x => x.Id != id && x.Code == code))
            throw new InvalidOperationException("Permission code already exists.");
        var oldValue = new { entity.Code, entity.Name };
        entity.Code = code;
        entity.Name = request.Name.Trim();
        audit.Add(actorId, "Update", "Permission", id, oldValue,
            new { entity.Code, entity.Name });
        await db.SaveChangesAsync();
        return new PermissionDto(entity.Id, entity.Code, entity.Name);
    }

    public async Task DeletePermissionAsync(
        long id, Guid actorId)
    {
        var entity = await db.Permissions.FindAsync([id])
            ?? throw new KeyNotFoundException("Permission was not found.");
        db.Permissions.Remove(entity);
        audit.Add(actorId, "Delete", "Permission", id, entity.Code);
        await db.SaveChangesAsync();
    }

    public Task<TargetSystemDto[]> GetSystemsAsync() =>
        db.TargetSystems.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new TargetSystemDto(x.Id, x.Name)).ToArrayAsync();

    public async Task<RoleDto> CreateRoleAsync(
        SaveRoleRequest request, Guid actorId)
    {
        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            IsRequestable = request.IsRequestable
        };
        EnsureSucceeded(await roles.CreateAsync(role));
        audit.Add(actorId, "Create", "Role", role.Id, newValue: role.Name);
        await db.SaveChangesAsync();
        return new RoleDto(role.Id, role.Name!, role.IsRequestable, false, []);
    }

    public async Task<RoleDto> UpdateRoleAsync(
        Guid id, SaveRoleRequest request, Guid actorId)
    {
        var role = await roles.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException("Role was not found.");
        if (Roles.PortalRoles.Contains(role.Name))
            throw new ArgumentException("Built-in portal roles cannot be edited.");
        var oldValue = new { role.Name, role.IsRequestable };
        role.Name = request.Name.Trim();
        role.IsRequestable = request.IsRequestable;
        EnsureSucceeded(await roles.UpdateAsync(role));
        audit.Add(actorId, "Update", "Role", id, oldValue,
            new { role.Name, role.IsRequestable });
        await db.SaveChangesAsync();
        return new RoleDto(role.Id, role.Name!, role.IsRequestable, false,
            await db.RolePermissions.Where(x => x.RoleId == id)
                .Select(x => x.PermissionId).ToArrayAsync());
    }

    public async Task DeleteRoleAsync(
        Guid id, Guid actorId)
    {
        var role = await roles.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException("Role was not found.");
        if (Roles.PortalRoles.Contains(role.Name))
            throw new ArgumentException("Built-in portal roles cannot be deleted.");
        if (await db.UserRoles.AnyAsync(x => x.RoleId == id) ||
            await db.AccessRequests.AnyAsync(x => x.RequestedRoleId == id))
            throw new InvalidOperationException("Role is currently in use.");
        db.RolePermissions.RemoveRange(db.RolePermissions.Where(x => x.RoleId == id));
        EnsureSucceeded(await roles.DeleteAsync(role));
        audit.Add(actorId, "Delete", "Role", id, role.Name);
        await db.SaveChangesAsync();
    }

    public async Task SetPermissionsAsync(
        Guid roleId, long[] permissionIds, Guid actorId)
    {
        if (await roles.FindByIdAsync(roleId.ToString()) is null)
            throw new KeyNotFoundException("Role was not found.");
        var ids = permissionIds.Distinct().ToArray();
        if (await db.Permissions.CountAsync(x => ids.Contains(x.Id)) != ids.Length)
            throw new ArgumentException("One or more permissions do not exist.");
        db.RolePermissions.RemoveRange(db.RolePermissions.Where(x => x.RoleId == roleId));
        db.RolePermissions.AddRange(ids.Select(id =>
            new RolePermission { RoleId = roleId, PermissionId = id }));
        audit.Add(actorId, "SetPermissions", "Role", roleId, newValue: ids);
        await db.SaveChangesAsync();
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new ArgumentException(
                string.Join(" ", result.Errors.Select(x => x.Description)));
    }
}
