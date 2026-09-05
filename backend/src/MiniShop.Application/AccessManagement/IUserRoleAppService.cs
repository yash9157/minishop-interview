using MiniShop.Application.Contracts;

namespace MiniShop.Application;

public interface IUserRoleAppService
{
    Task<PagedResult<UserDto>> GetUsersAsync(PagedRequest request, CancellationToken cancellationToken);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, string idempotencyKey,
        Guid actorId, CancellationToken cancellationToken);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, Guid actorId, CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid id, Guid actorId, CancellationToken cancellationToken);
    Task AssignRoleAsync(Guid userId, Guid roleId, Guid actorId, CancellationToken cancellationToken);
    Task RemoveRoleAsync(Guid userId, Guid roleId, Guid actorId, CancellationToken cancellationToken);
    Task<string[]> GetEffectivePermissionsAsync(Guid userId, CancellationToken cancellationToken);
    Task<RoleDto[]> GetRolesAsync(CancellationToken cancellationToken);
    Task<PermissionDto[]> GetPermissionsAsync(CancellationToken cancellationToken);
    Task<PermissionDto> CreatePermissionAsync(SavePermissionRequest request, Guid actorId, CancellationToken cancellationToken);
    Task<PermissionDto> UpdatePermissionAsync(long id, SavePermissionRequest request, Guid actorId, CancellationToken cancellationToken);
    Task DeletePermissionAsync(long id, Guid actorId, CancellationToken cancellationToken);
    Task<TargetSystemDto[]> GetSystemsAsync(CancellationToken cancellationToken);
    Task<RoleDto> CreateRoleAsync(SaveRoleRequest request, Guid actorId, CancellationToken cancellationToken);
    Task<RoleDto> UpdateRoleAsync(Guid id, SaveRoleRequest request, Guid actorId, CancellationToken cancellationToken);
    Task DeleteRoleAsync(Guid id, Guid actorId, CancellationToken cancellationToken);
    Task SetPermissionsAsync(Guid roleId, long[] permissionIds, Guid actorId, CancellationToken cancellationToken);
}
