using MiniShop.Domain;

namespace MiniShop.Application.Tests.Orders;

public sealed class OrderTests
{
    [Fact]
    public void RecalculateTotal_UsesQuantityAndCapturedUnitPrice()
    {
        var order = new Order
        {
            Items =
            [
                new OrderItem { Quantity = 2, UnitPrice = 10.50m },
                new OrderItem { Quantity = 1, UnitPrice = 5m }
            ]
        };

        order.RecalculateTotal();

        Assert.Equal(26m, order.TotalAmount);
    }
}
