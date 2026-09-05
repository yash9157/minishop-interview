namespace MiniShop.Application.Contracts;

public sealed record CustomerDto(long Id, string Name, string Email, string? Phone);
