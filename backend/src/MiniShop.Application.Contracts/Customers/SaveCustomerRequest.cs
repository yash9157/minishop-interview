using System.ComponentModel.DataAnnotations;
using MiniShop.Domain.Shared;

namespace MiniShop.Application.Contracts;

public sealed class SaveCustomerRequest
{
    [Required, StringLength(ValidationConstants.NameMaxLength)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(ValidationConstants.EmailMaxLength)]
    public string Email { get; init; } = string.Empty;

    [Phone, StringLength(ValidationConstants.PhoneMaxLength)]
    public string? Phone { get; init; }
}
