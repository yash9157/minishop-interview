using System.ComponentModel.DataAnnotations;
using Mini.Access.Management.Application.Contracts;

namespace Mini.Access.Management.Application.Tests.AccessManagement;

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
