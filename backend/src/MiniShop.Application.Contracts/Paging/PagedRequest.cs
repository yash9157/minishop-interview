using System.ComponentModel.DataAnnotations;
using MiniShop.Domain.Shared;

namespace MiniShop.Application.Contracts;

public class PagedRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, ValidationConstants.MaxPageSize)]
    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }
}
