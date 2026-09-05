using System.ComponentModel.DataAnnotations;
using MiniShop.Application.Contracts;

namespace MiniShop.Application.Tests.Products;

public sealed class SaveProductRequestTests
{
    [Fact]
    public void Validation_RejectsInvalidValues()
    {
        var request = new SaveProductRequest
        {
            CategoryId = 0,
            Sku = string.Empty,
            Name = string.Empty,
            Price = 0,
            StockQuantity = -1
        };
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            errors,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.True(errors.Count >= 4);
    }
}
