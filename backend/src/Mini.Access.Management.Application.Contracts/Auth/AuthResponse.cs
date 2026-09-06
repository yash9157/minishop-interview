namespace Mini.Access.Management.Application.Contracts;

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, CurrentUserDto User);
