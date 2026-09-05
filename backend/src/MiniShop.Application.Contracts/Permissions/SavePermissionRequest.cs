using System.ComponentModel.DataAnnotations;

namespace MiniShop.Application.Contracts;

public sealed class SavePermissionRequest
{
    [Required, StringLength(80)]
    public string Code { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string Name { get; init; } = string.Empty;
}
