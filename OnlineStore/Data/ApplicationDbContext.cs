using Microsoft.EntityFrameworkCore;
using OnlineStore.Data.Seed;
using OnlineStore.Models;

namespace OnlineStore.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Users> Users => Set<Users>();
    public DbSet<Categories> Categories => Set<Categories>();
    public DbSet<Products> Products => Set<Products>();
    public DbSet<Orders> Orders => Set<Orders>();
    public DbSet<OrderItems> OrderItems => Set<OrderItems>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItems> CartItems => Set<CartItems>();
    public DbSet<Reviews> Reviews => Set<Reviews>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        SeedData.Configure(modelBuilder);
    }
}
