namespace MiniShop.Application;

public sealed class NotFoundException(string message) : Exception(message);
