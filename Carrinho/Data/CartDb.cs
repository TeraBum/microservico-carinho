using Microsoft.EntityFrameworkCore;

using Npgsql.EntityFrameworkCore;
namespace Carrinho;

public class CartDb : DbContext
{
    public CartDb(DbContextOptions<CartDb> options) : base(options) { }
    public DbSet<Cart> Cart { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CartItem>()
            .HasKey(ci => new { ci.CartId, ci.ProductId });
    }
}