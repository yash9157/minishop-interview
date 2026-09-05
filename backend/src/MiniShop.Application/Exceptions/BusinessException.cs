namespace MiniShop.Application;

public sealed class BusinessException(string message) : Exception(message);
