namespace MiniShop.Application.Contracts;

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, CurrentUserDto User);
