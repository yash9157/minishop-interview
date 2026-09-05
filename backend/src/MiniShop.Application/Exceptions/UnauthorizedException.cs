namespace MiniShop.Application;

public sealed class UnauthorizedException(string message) : Exception(message);
