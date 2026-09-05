using System.ComponentModel.DataAnnotations;
using MiniShop.Application.Contracts;

namespace MiniShop.Application.Tests.AccessManagement;

public sealed class AccessRequestValidationTests
{
    [Fact]
    public void JustificationMustBeAtLeastTenCharacters()
    {
        var request = new CreateAccessRequest
        {
            TargetSystemId = 1,
            RequestedRoleId = Guid.NewGuid(),
            BusinessJustification = "short"
        };
        var errors = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(
            request, new ValidationContext(request), errors, true));
    }
}
