namespace TeejoshSystem.WebUI.Infrastructure.State;

public sealed class CircuitStateStore
{
    public List<SaleCartItem> CartItems { get; } = new();

    public void AddOrIncrement(int productId, string name, decimal unitPrice, int stock)
    {
        var existing = CartItems.FirstOrDefault(i => i.ProductId == productId);
        if (existing is null)
            CartItems.Add(new SaleCartItem(productId, name, unitPrice, 1, stock));
        else if (existing.Quantity < existing.Stock)
            existing.Quantity++;
    }

    public void Remove(int productId) => CartItems.RemoveAll(i => i.ProductId == productId);

    public void Clear() => CartItems.Clear();
}

public sealed class SaleCartItem
{
    public SaleCartItem(int productId, string name, decimal unitPrice, int quantity, int stock)
    {
        ProductId = productId;
        Name = name;
        UnitPrice = unitPrice;
        Quantity = quantity;
        Stock = stock;
    }

    public int ProductId { get; }
    public string Name { get; }
    public decimal UnitPrice { get; }
    public int Quantity { get; set; }
    public int Stock { get; }
    public decimal Subtotal => UnitPrice * Quantity;
}
