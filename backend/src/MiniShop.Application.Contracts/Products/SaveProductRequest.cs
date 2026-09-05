using System.ComponentModel.DataAnnotations;
using MiniShop.Domain.Shared;

namespace MiniShop.Application.Contracts;

public sealed class SaveProductRequest
{
    [Range(typeof(long), "1", "9223372036854775807")]
    public long CategoryId { get; init; }

    [Required, StringLength(ValidationConstants.SkuMaxLength)]
    public string Sku { get; init; } = string.Empty;

    [Required, StringLength(ValidationConstants.NameMaxLength)]
    public string Name { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999.99")]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    public bool IsActive { get; init; } = true;
}
