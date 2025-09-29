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

    public async Task<Cart> getCartIfExists(User user)
    {
        var cart = await _cartDb.Cart.Where(c => c.Status == "Active" && c.UserId == user.Id).FirstOrDefaultAsync();
        if (cart is null)
            return null;
        return cart;
    }
    
    public async Task<User> getUser(string email)
    {
        var user = await _userDb.Users.Where(u => u.Email == email).FirstAsync();
        if (user is null)
            throw new KeyNotFoundException(email);
        return user;
    }

    public async Task<Cart> createCartIfNotExist(Cart cart,  string email)
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

    public async Task<Cart> cancelCartIfExist(string email)
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

    public async Task<Cart> addItemsToCart(CartItem[] cartItems, string cartId, string email)
    {
        var user = getUser(email);
        var cart = await _cartDb.Cart.Include(c => c.Items).Where(c => c.UserId == user.Id && c.Status == "Active").FirstAsync();
        if (cart is null)
            throw new KeyNotFoundException(cartId);
        if (cart.Items is not null)
            cart.Items.AddRange(cartItems);
        else
            cart.Items = new List<CartItem>(cartItems);
        await _cartDb.SaveChangesAsync();
        return cart;
    }
}