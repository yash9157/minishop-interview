namespace MiniShop.Domain;

public sealed class OrderItem
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
