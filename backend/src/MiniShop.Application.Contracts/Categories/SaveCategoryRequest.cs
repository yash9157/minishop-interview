using System.ComponentModel.DataAnnotations;
using MiniShop.Domain.Shared;

namespace MiniShop.Application.Contracts;

public sealed class SaveCategoryRequest
{
    [Required, StringLength(ValidationConstants.NameMaxLength)]
    public string Name { get; init; } = string.Empty;

    [StringLength(ValidationConstants.DescriptionMaxLength)]
    public string? Description { get; init; }
}
