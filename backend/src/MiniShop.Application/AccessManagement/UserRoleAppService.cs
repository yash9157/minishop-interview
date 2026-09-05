using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniShop.Application.Contracts;
using MiniShop.Domain;
using MiniShop.Domain.Shared;
using MiniShop.EntityFrameworkCore;

namespace MiniShop.Application;

public sealed class UserRoleAppService(
    MiniShopDbContext db,
    UserManager<ApplicationUser> users,
    RoleManager<ApplicationRole> roles,
    IAuditWriter audit) : IUserRoleAppService
{
    public async Task<PagedResult<UserDto>> GetUsersAsync(
        PagedRequest request, CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking().Include(x => x.Manager)
            .Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.FullName.Contains(search) || x.Email!.Contains(search));
        }

        var total = await query.CountAsync(cancellationToken);
        var page = await query.OrderBy(x => x.FullName)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var userIds = page.Select(x => x.Id).ToArray();
        var assignedRoles = await (
                from userRole in db.UserRoles
                join role in db.Roles on userRole.RoleId equals role.Id
                where userIds.Contains(userRole.UserId)
                select new { userRole.UserId, Role = role.Name! })
            .ToListAsync(cancellationToken);
        var rolesByUser = assignedRoles.ToLookup(x => x.UserId, x => x.Role);
        var items = page.Select(user => new UserDto(
            user.Id, user.FullName, user.Email!, user.ManagerId, user.Manager?.FullName,
            user.IsActive, rolesByUser[user.Id].OrderBy(x => x).ToArray())).ToArray();
        return new PagedResult<UserDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<UserDto> CreateUserAsync(
        CreateUserRequest request, string idempotencyKey, Guid actorId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
            throw new BusinessException("A valid Idempotency-Key header is required.");

        const string operation = "CreateUser";
        var previous = await db.IdempotencyRecords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Operation == operation && x.Key == idempotencyKey,
                cancellationToken);
        if (previous is not null && Guid.TryParse(previous.ResourceId, out var previousUserId))
            return await MapUserAsync(await FindUserAsync(previousUserId));

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.FindByEmailAsync(email) is not null)
            throw new ConflictException("Email already exists.");
        await ValidateManagerAsync(request.ManagerId, cancellationToken);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            ManagerId = request.ManagerId
        };
        EnsureSucceeded(await users.CreateAsync(user, request.Password));
        EnsureSucceeded(await users.AddToRoleAsync(user, Roles.Employee));
        db.IdempotencyRecords.Add(new IdempotencyRecord
        {
            Key = idempotencyKey,
            Operation = operation,
            ResourceId = user.Id.ToString()
        });
        audit.Add(actorId, "Create", "User", user.Id, newValue: new { user.FullName, user.Email });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await MapUserAsync(user);
    }

    public async Task<UserDto> UpdateUserAsync(
        Guid id, UpdateUserRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        var user = await FindUserAsync(id);
        await ValidateManagerAsync(request.ManagerId, cancellationToken);
        if (request.ManagerId == id)
            throw new BusinessException("A user cannot be their own manager.");
        var oldValue = new { user.FullName, user.ManagerId, user.IsActive };
        user.FullName = request.FullName.Trim();
        user.ManagerId = request.ManagerId;
        user.IsActive = request.IsActive;
        EnsureSucceeded(await users.UpdateAsync(user));
        audit.Add(actorId, "Update", "User", id, oldValue,
            new { user.FullName, user.ManagerId, user.IsActive });
        await db.SaveChangesAsync(cancellationToken);
        return await MapUserAsync(user);
    }

    public async Task DeleteUserAsync(Guid id, Guid actorId, CancellationToken cancellationToken)
    {
        var user = await FindUserAsync(id);
        user.IsDeleted = true;
        user.IsActive = false;
        user.DeletedAtUtc = DateTime.UtcNow;
        audit.Add(actorId, "SoftDelete", "User", id);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignRoleAsync(
        Guid userId, Guid roleId, Guid actorId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var user = await FindUserAsync(userId);
        var role = await roles.FindByIdAsync(roleId.ToString())
            ?? throw new NotFoundException("Role was not found.");
        if (await users.IsInRoleAsync(user, role.Name!))
            throw new ConflictException("The role is already assigned to this user.");
        await EnsureNoConflictAsync(user, role.Name!);
        EnsureSucceeded(await users.AddToRoleAsync(user, role.Name!));
        audit.Add(actorId, "AssignRole", "UserRole", userId, newValue: role.Name);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RemoveRoleAsync(
        Guid userId, Guid roleId, Guid actorId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var user = await FindUserAsync(userId);
        var role = await roles.FindByIdAsync(roleId.ToString())
            ?? throw new NotFoundException("Role was not found.");
        if (!await users.IsInRoleAsync(user, role.Name!))
            throw new NotFoundException("The role is not assigned to this user.");
        EnsureSucceeded(await users.RemoveFromRoleAsync(user, role.Name!));
        audit.Add(actorId, "RemoveRole", "UserRole", userId, role.Name);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<string[]> GetEffectivePermissionsAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        await FindUserAsync(userId);
        return await (
            from userRole in db.UserRoles
            join rolePermission in db.RolePermissions on userRole.RoleId equals rolePermission.RoleId
            join permission in db.Permissions on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == userId
            select permission.Code)
            .Distinct().OrderBy(x => x).ToArrayAsync(cancellationToken);
    }

    public async Task<RoleDto[]> GetRolesAsync(CancellationToken cancellationToken) =>
        await db.Roles.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new RoleDto(x.Id, x.Name!, x.IsRequestable,
                db.RolePermissions.Where(rp => rp.RoleId == x.Id)
                    .Select(rp => rp.PermissionId).ToArray()))
            .ToArrayAsync(cancellationToken);

    public Task<PermissionDto[]> GetPermissionsAsync(CancellationToken cancellationToken) =>
        db.Permissions.AsNoTracking().OrderBy(x => x.Code)
            .Select(x => new PermissionDto(x.Id, x.Code, x.Name))
            .ToArrayAsync(cancellationToken);

    public async Task<PermissionDto> CreatePermissionAsync(
        SavePermissionRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.Permissions.AnyAsync(x => x.Code == code, cancellationToken))
            throw new ConflictException("Permission code already exists.");
        var entity = new Permission { Code = code, Name = request.Name.Trim() };
        db.Permissions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        audit.Add(actorId, "Create", "Permission", entity.Id, newValue: code);
        await db.SaveChangesAsync(cancellationToken);
        return new PermissionDto(entity.Id, entity.Code, entity.Name);
    }

    public async Task<PermissionDto> UpdatePermissionAsync(
        long id, SavePermissionRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        var entity = await db.Permissions.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException("Permission was not found.");
        var code = request.Code.Trim().ToUpperInvariant();
        if (await db.Permissions.AnyAsync(
                x => x.Id != id && x.Code == code, cancellationToken))
            throw new ConflictException("Permission code already exists.");
        var oldValue = new { entity.Code, entity.Name };
        entity.Code = code;
        entity.Name = request.Name.Trim();
        audit.Add(actorId, "Update", "Permission", id, oldValue,
            new { entity.Code, entity.Name });
        await db.SaveChangesAsync(cancellationToken);
        return new PermissionDto(entity.Id, entity.Code, entity.Name);
    }

    public async Task DeletePermissionAsync(
        long id, Guid actorId, CancellationToken cancellationToken)
    {
        var entity = await db.Permissions.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException("Permission was not found.");
        db.Permissions.Remove(entity);
        audit.Add(actorId, "Delete", "Permission", id, entity.Code);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<TargetSystemDto[]> GetSystemsAsync(CancellationToken cancellationToken) =>
        db.TargetSystems.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new TargetSystemDto(x.Id, x.Name)).ToArrayAsync(cancellationToken);

    public async Task<RoleDto> CreateRoleAsync(
        SaveRoleRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            IsRequestable = request.IsRequestable
        };
        EnsureSucceeded(await roles.CreateAsync(role));
        audit.Add(actorId, "Create", "Role", role.Id, newValue: role.Name);
        await db.SaveChangesAsync(cancellationToken);
        return new RoleDto(role.Id, role.Name!, role.IsRequestable, []);
    }

    public async Task<RoleDto> UpdateRoleAsync(
        Guid id, SaveRoleRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        var role = await roles.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Role was not found.");
        if (Roles.PortalRoles.Contains(role.Name))
            throw new BusinessException("Built-in portal roles cannot be edited.");
        var oldName = role.Name;
        role.Name = request.Name.Trim();
        role.IsRequestable = request.IsRequestable;
        EnsureSucceeded(await roles.UpdateAsync(role));
        audit.Add(actorId, "Update", "Role", id, oldName, role.Name);
        await db.SaveChangesAsync(cancellationToken);
        return new RoleDto(role.Id, role.Name!, role.IsRequestable,
            await db.RolePermissions.Where(x => x.RoleId == id)
                .Select(x => x.PermissionId).ToArrayAsync(cancellationToken));
    }

    public async Task DeleteRoleAsync(Guid id, Guid actorId, CancellationToken cancellationToken)
    {
        var role = await roles.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Role was not found.");
        if (Roles.PortalRoles.Contains(role.Name))
            throw new BusinessException("Built-in portal roles cannot be deleted.");
        if (await db.UserRoles.AnyAsync(x => x.RoleId == id, cancellationToken) ||
            await db.AccessRequests.AnyAsync(x => x.RequestedRoleId == id, cancellationToken))
            throw new ConflictException("Role is currently in use.");
        db.RolePermissions.RemoveRange(db.RolePermissions.Where(x => x.RoleId == id));
        EnsureSucceeded(await roles.DeleteAsync(role));
        audit.Add(actorId, "Delete", "Role", id, role.Name);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetPermissionsAsync(
        Guid roleId, long[] permissionIds, Guid actorId, CancellationToken cancellationToken)
    {
        if (await roles.FindByIdAsync(roleId.ToString()) is null)
            throw new NotFoundException("Role was not found.");
        var ids = permissionIds.Distinct().ToArray();
        if (await db.Permissions.CountAsync(x => ids.Contains(x.Id), cancellationToken) != ids.Length)
            throw new BusinessException("One or more permissions do not exist.");
        db.RolePermissions.RemoveRange(db.RolePermissions.Where(x => x.RoleId == roleId));
        db.RolePermissions.AddRange(ids.Select(id =>
            new RolePermission { RoleId = roleId, PermissionId = id }));
        audit.Add(actorId, "SetPermissions", "Role", roleId, newValue: ids);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<ApplicationUser> FindUserAsync(Guid id)
    {
        var user = await users.FindByIdAsync(id.ToString());
        if (user is null || user.IsDeleted)
            throw new NotFoundException("User was not found.");
        return user;
    }

    private async Task ValidateManagerAsync(Guid? managerId, CancellationToken cancellationToken)
    {
        if (managerId.HasValue &&
            !await db.Users.AnyAsync(x => x.Id == managerId && x.IsActive, cancellationToken))
            throw new BusinessException("Manager does not exist or is inactive.");
    }

    private async Task EnsureNoConflictAsync(ApplicationUser user, string newRole)
    {
        if ((newRole == Roles.Maker && await users.IsInRoleAsync(user, Roles.Checker)) ||
            (newRole == Roles.Checker && await users.IsInRoleAsync(user, Roles.Maker)))
            throw new ConflictException("Maker and Checker roles cannot be assigned to the same user.");
    }

    private async Task<UserDto> MapUserAsync(ApplicationUser user)
    {
        var roleNames = await users.GetRolesAsync(user);
        var managerName = user.Manager?.FullName;
        if (managerName is null && user.ManagerId.HasValue)
            managerName = (await users.FindByIdAsync(user.ManagerId.Value.ToString()))?.FullName;
        return new UserDto(user.Id, user.FullName, user.Email!, user.ManagerId,
            managerName, user.IsActive, roleNames.ToArray());
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new BusinessException(string.Join(" ", result.Errors.Select(x => x.Description)));
    }
}
