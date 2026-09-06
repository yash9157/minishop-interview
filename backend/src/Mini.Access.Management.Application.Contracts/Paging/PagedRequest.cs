using System.ComponentModel.DataAnnotations;
using Mini.Access.Management.Domain.Shared;

namespace Mini.Access.Management.Application.Contracts;

public class PagedRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, ValidationConstants.MaxPageSize)]
    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }
}
