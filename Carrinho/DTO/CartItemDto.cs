namespace Carrinho.DTO;

public class CartItemDto
{
    public int Quantity { get; set; }
    public Guid? ProductId { get; set; }
    public int UnitPrice { get; set; }
}