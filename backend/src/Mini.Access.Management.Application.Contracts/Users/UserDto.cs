namespace Mini.Access.Management.Application.Contracts;

public sealed record UserDto(
    Guid Id, string FullName, string Email, Guid? ManagerId, string? ManagerName,
    bool IsActive, string[] Roles);
