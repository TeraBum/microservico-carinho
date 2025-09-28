using Microsoft.EntityFrameworkCore;

namespace Carrinho;

public class UserDb: DbContext
{
    public UserDb(DbContextOptions<UserDb> options) : base(options) { }
    public DbSet<User> Users { get; set; }
}