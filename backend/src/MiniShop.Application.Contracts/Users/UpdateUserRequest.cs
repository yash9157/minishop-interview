using System.ComponentModel.DataAnnotations;

namespace MiniShop.Application.Contracts;

public sealed class UpdateUserRequest
{
    [Required, StringLength(120)]
    public string FullName { get; init; } = string.Empty;

    public Guid? ManagerId { get; init; }
    public bool IsActive { get; init; } = true;
}
