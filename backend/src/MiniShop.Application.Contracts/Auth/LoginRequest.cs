using System.ComponentModel.DataAnnotations;

namespace MiniShop.Application.Contracts;

public sealed class LoginRequest
{
    [Required]
    public string TenantCode { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
