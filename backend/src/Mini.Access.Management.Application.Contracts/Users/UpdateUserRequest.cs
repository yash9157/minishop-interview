using System.ComponentModel.DataAnnotations;

namespace Mini.Access.Management.Application.Contracts;

public sealed class UpdateUserRequest
{
    [Required, StringLength(120)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    public Guid? ManagerId { get; init; }
    public bool IsActive { get; init; } = true;
}
