using System.ComponentModel.DataAnnotations;

namespace MiniShop.Application.Contracts;

public sealed class SaveRoleRequest
{
    [Required, StringLength(80)]
    public string Name { get; init; } = string.Empty;

    public bool IsRequestable { get; init; }
}
