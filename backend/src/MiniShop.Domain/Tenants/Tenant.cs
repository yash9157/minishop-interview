namespace MiniShop.Domain;

public sealed class Tenant
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<ApplicationUser> Users { get; set; } = [];
}
