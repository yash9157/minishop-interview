using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mini.Access.Management.Application.Contracts;
using Mini.Access.Management.Domain;
using Mini.Access.Management.Domain.Shared;
using Mini.Access.Management.EntityFrameworkCore;

namespace Mini.Access.Management.Application;

public sealed class UserAppService(
    AccessManagementDbContext db,
    UserManager<ApplicationUser> users,
    RoleManager<ApplicationRole> roles,
    AuditWriter audit) : IUserAppService
{
    public async Task<PagedResult<UserDto>> GetUsersAsync(
        PagedRequest request)
    {
        var query = db.Users.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.FullName.Contains(search) || x.Email!.Contains(search));
        }

        var total = await query.CountAsync();
        var page = await query.Include(x => x.Manager)
            .OrderBy(x => x.FullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
        var pagedUserIds = query.OrderBy(x => x.FullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => x.Id);
        var assignedRoles = await (
                from userRole in db.UserRoles
                join pagedUserId in pagedUserIds on userRole.UserId equals pagedUserId
                join role in db.Roles on userRole.RoleId equals role.Id
                select new { userRole.UserId, Role = role.Name! })
            .ToListAsync();
        var rolesByUser = assignedRoles.ToLookup(x => x.UserId, x => x.Role);
        var items = page.Select(user => new UserDto(
            user.Id, user.FullName, user.Email!, user.ManagerId, user.Manager?.FullName,
            user.IsActive, rolesByUser[user.Id].OrderBy(x => x).ToArray())).ToArray();
        return new PagedResult<UserDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<UserDto> CreateUserAsync(
        CreateUserRequest request, string idempotencyKey, Guid actorId)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
            throw new ArgumentException("A valid Idempotency-Key header is required.");

        const string operation = "CreateUser";
        var requestHash = HashRequest(request);
        var previous = await db.IdempotencyRecords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Operation == operation && x.Key == idempotencyKey);
        if (previous is not null)
            return await ReadPreviousAsync(previous, requestHash);

        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var idempotency = new IdempotencyRecord
            {
                Key = idempotencyKey,
                Operation = operation,
                RequestHash = requestHash,
                ResponseJson = string.Empty,
                StatusCode = 201
            };
            db.IdempotencyRecords.Add(idempotency);

            // Reserving the unique operation/key pair first serializes concurrent retries.
            await db.SaveChangesAsync();

            var email = NormalizeEmail(request.Email);
            if (await users.FindByEmailAsync(email) is not null)
                throw new InvalidOperationException("Email already exists.");
            await ValidateManagerAsync(request.ManagerId);

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
            audit.Add(actorId, "Create", "User", user.Id,
                newValue: new { user.FullName, user.Email });

            var response = await MapUserAsync(user);
            idempotency.ResourceId = user.Id.ToString();
            idempotency.ResponseJson = JsonSerializer.Serialize(response);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return response;
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            var completed = await db.IdempotencyRecords.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Operation == operation && x.Key == idempotencyKey);
            if (completed is not null)
                return await ReadPreviousAsync(completed, requestHash);
            throw;
        }
    }

    public async Task<UserDto> UpdateUserAsync(
        Guid id, UpdateUserRequest request, Guid actorId)
    {
        var user = await FindUserAsync(id);
        await ValidateManagerAsync(request.ManagerId);
        if (request.ManagerId == id)
            throw new ArgumentException("A user cannot be their own manager.");

        var email = NormalizeEmail(request.Email);
        var emailOwner = await users.FindByEmailAsync(email);
        if (emailOwner is not null && emailOwner.Id != id)
            throw new InvalidOperationException("Email already exists.");

        var oldValue = new { user.FullName, user.Email, user.ManagerId, user.IsActive };
        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.UserName = email;
        user.ManagerId = request.ManagerId;
        user.IsActive = request.IsActive;
        EnsureSucceeded(await users.UpdateAsync(user));
        audit.Add(actorId, "Update", "User", id, oldValue,
            new { user.FullName, user.Email, user.ManagerId, user.IsActive });
        await db.SaveChangesAsync();
        return await MapUserAsync(user);
    }

    public async Task DeleteUserAsync(
        Guid id, Guid actorId)
    {
        var user = await FindUserAsync(id);
        user.IsDeleted = true;
        user.IsActive = false;
        user.DeletedAtUtc = DateTime.UtcNow;
        audit.Add(actorId, "SoftDelete", "User", id);
        await db.SaveChangesAsync();
    }

    public async Task AssignRoleAsync(
        Guid userId, Guid roleId, Guid actorId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await FindUserAsync(userId);
        var role = await roles.FindByIdAsync(roleId.ToString())
            ?? throw new KeyNotFoundException("Role was not found.");
        if (await users.IsInRoleAsync(user, role.Name!))
            throw new InvalidOperationException("The role is already assigned to this user.");
        await EnsureNoConflictAsync(user, role.Name!);
        EnsureSucceeded(await users.AddToRoleAsync(user, role.Name!));
        audit.Add(actorId, "AssignRole", "UserRole", userId, newValue: role.Name);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task RemoveRoleAsync(
        Guid userId, Guid roleId, Guid actorId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await FindUserAsync(userId);
        var role = await roles.FindByIdAsync(roleId.ToString())
            ?? throw new KeyNotFoundException("Role was not found.");
        if (!await users.IsInRoleAsync(user, role.Name!))
            throw new KeyNotFoundException("The role is not assigned to this user.");
        EnsureSucceeded(await users.RemoveFromRoleAsync(user, role.Name!));
        audit.Add(actorId, "RemoveRole", "UserRole", userId, role.Name);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<string[]> GetEffectivePermissionsAsync(
        Guid userId)
    {
        await FindUserAsync(userId);
        return await (
            from userRole in db.UserRoles
            join rolePermission in db.RolePermissions on userRole.RoleId equals rolePermission.RoleId
            join permission in db.Permissions on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == userId
            select permission.Code)
            .Distinct().OrderBy(x => x).ToArrayAsync();
    }

    private async Task<UserDto> ReadPreviousAsync(
        IdempotencyRecord record, string requestHash)
    {
        if (!string.IsNullOrEmpty(record.RequestHash) && record.RequestHash != requestHash)
            throw new InvalidOperationException(
                "This Idempotency-Key was already used with a different request.");

        if (!string.IsNullOrEmpty(record.ResponseJson))
            return JsonSerializer.Deserialize<UserDto>(record.ResponseJson)
                ?? throw new InvalidOperationException("The saved idempotent response is invalid.");

        if (Guid.TryParse(record.ResourceId, out var userId))
            return await MapUserAsync(await FindUserAsync(userId));

        throw new InvalidOperationException("The matching request is still being processed. Retry shortly.");
    }

    private static string HashRequest(CreateUserRequest request)
    {
        var value = string.Join('\n', request.FullName.Trim(), NormalizeEmail(request.Email),
            request.Password, request.ManagerId?.ToString() ?? string.Empty);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private async Task<ApplicationUser> FindUserAsync(Guid id)
    {
        var user = await users.FindByIdAsync(id.ToString());
        if (user is null || user.IsDeleted)
            throw new KeyNotFoundException("User was not found.");
        return user;
    }

    private async Task ValidateManagerAsync(Guid? managerId)
    {
        if (managerId.HasValue &&
            !await db.Users.AnyAsync(
                x => x.Id == managerId && x.IsActive && !x.IsDeleted))
            throw new ArgumentException("Manager does not exist or is inactive.");
    }

    private async Task EnsureNoConflictAsync(ApplicationUser user, string newRole)
    {
        if ((newRole == Roles.Maker && await users.IsInRoleAsync(user, Roles.Checker)) ||
            (newRole == Roles.Checker && await users.IsInRoleAsync(user, Roles.Maker)))
            throw new InvalidOperationException(
                "Maker and Checker roles cannot be assigned to the same user.");
    }

    private async Task<UserDto> MapUserAsync(ApplicationUser user)
    {
        var roleNames = await users.GetRolesAsync(user);
        var managerName = user.Manager?.FullName;
        if (managerName is null && user.ManagerId.HasValue)
            managerName = (await users.FindByIdAsync(user.ManagerId.Value.ToString()))?.FullName;
        return new UserDto(user.Id, user.FullName, user.Email!, user.ManagerId,
            managerName, user.IsActive, roleNames.OrderBy(x => x).ToArray());
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new ArgumentException(
                string.Join(" ", result.Errors.Select(x => x.Description)));
    }
}
