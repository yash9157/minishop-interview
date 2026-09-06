namespace Mini.Access.Management.Application.Contracts;

public interface IUserAppService
{
    Task<PagedResult<UserDto>> GetUsersAsync(
        PagedRequest request);
    Task<UserDto> CreateUserAsync(
        CreateUserRequest request, string idempotencyKey, Guid actorId);
    Task<UserDto> UpdateUserAsync(
        Guid id, UpdateUserRequest request, Guid actorId);
    Task DeleteUserAsync(
        Guid id, Guid actorId);
    Task AssignRoleAsync(
        Guid userId, Guid roleId, Guid actorId);
    Task RemoveRoleAsync(
        Guid userId, Guid roleId, Guid actorId);
    Task<string[]> GetEffectivePermissionsAsync(
        Guid userId);
}
