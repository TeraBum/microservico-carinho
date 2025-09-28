namespace Carrinho.Services;
using Microsoft.EntityFrameworkCore;

public class CartService
{
    private readonly UserDb _userDb;
    private readonly CartDb _cartDb;
    CartService(UserDb userDb, CartDb cartDb)
    {
       _userDb = userDb;
       _cartDb = cartDb;
    }

    async Task<Cart> getCartIfExists(User user)
    {
        var cart = await _cartDb.Cart.Where(c => c.Status == "Active" && c.UserId == user.Id).FirstOrDefaultAsync();
        if (cart is null)
            return null;
        return cart;
    }
    
    async Task<User> getUser(string email)
    {
        var user = await _userDb.Users.Where(u => u.Email == email).FirstAsync();
        if (user is null)
            throw new KeyNotFoundException(email);
        return user;
    }

    async Task<Cart> createCartIfNotExist(Cart cart,  string email)
    {
        var user = await getUser(email);
        var oldCart = await this.getCartIfExists(user);
        if (oldCart is not null)
        {
            throw new InvalidOperationException("Cart already exists");
        }
        _cartDb.Cart.Add(cart);
        await _cartDb.SaveChangesAsync();
        return cart;
    }

    async Task<Cart> cancelCartIfExist(string email)
    {
        var user = await getUser(email);
        var oldCart = await this.getCartIfExists(user);
        if (oldCart is null)
        {
            throw new InvalidOperationException("Cart already exists");
        }
        oldCart.Status = "Cancelled";
        await _cartDb.SaveChangesAsync();
        return oldCart;
    }

    async Task<Cart> addItemsToCart(CartItem[] cartItems, string cartId)
    {
        var cart = await _cartDb.Cart.FindAsync(cartId);
        if (cart is null)
            throw new KeyNotFoundException(cartId);
        cart.Items.AddRange(cartItems);
        await _cartDb.SaveChangesAsync();
        return cart;
    }
}