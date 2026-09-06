namespace Mini.Access.Management.Application.Contracts;

public interface IRolePermissionAppService
{
    Task<PagedResult<RoleDto>> GetRolesAsync(
        PagedRequest request);
    Task<PagedResult<PermissionDto>> GetPermissionsAsync(
        PagedRequest request);
    Task<PermissionDto> CreatePermissionAsync(
        SavePermissionRequest request, Guid actorId);
    Task<PermissionDto> UpdatePermissionAsync(
        long id, SavePermissionRequest request, Guid actorId);
    Task DeletePermissionAsync(
        long id, Guid actorId);
    Task<TargetSystemDto[]> GetSystemsAsync();
    Task<RoleDto> CreateRoleAsync(
        SaveRoleRequest request, Guid actorId);
    Task<RoleDto> UpdateRoleAsync(
        Guid id, SaveRoleRequest request, Guid actorId);
    Task DeleteRoleAsync(
        Guid id, Guid actorId);
    Task SetPermissionsAsync(
        Guid roleId, long[] permissionIds, Guid actorId);
}
