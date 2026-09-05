using MiniShop.Domain.Shared;

namespace MiniShop.Domain;

public sealed class Order
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public decimal TotalAmount { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = [];

    public void RecalculateTotal() =>
        TotalAmount = Items.Sum(item => item.LineTotal);
}
