namespace Carrinho;

public class Cart
{
    public Guid Id { get; set; }
    public int? UserId { get; set; }
    public string Status { get; set; }
    
    public List<CartItem>? Items { get; set; }
}

public class CartItem
{
    public int Quantity { get; set; }
    public Guid CartId { get; set; }
    public Guid? ProductId { get; set; }
    public int UnitPrice { get; set; }
}
