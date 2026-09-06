namespace Mini.Access.Management.Application.Contracts;

public sealed record CurrentUserDto(Guid Id, string FullName, string Email, IReadOnlyList<string> Roles);
