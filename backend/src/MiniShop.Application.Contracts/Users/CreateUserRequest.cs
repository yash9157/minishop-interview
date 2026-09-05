using System.ComponentModel.DataAnnotations;

namespace MiniShop.Application.Contracts;

public sealed class CreateUserRequest
{
    [Required, StringLength(120)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;

    public Guid? ManagerId { get; init; }
}
