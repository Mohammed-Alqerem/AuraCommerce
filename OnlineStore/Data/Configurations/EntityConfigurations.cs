using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Constants;
using OnlineStore.Models;

namespace OnlineStore.Data.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder.Property(user => user.Email).HasMaxLength(256);
        builder.Property(user => user.NormalizedEmail).HasMaxLength(256);
        builder.Property(user => user.Password).HasMaxLength(256);
        builder.Property(user => user.Role).HasMaxLength(20).HasDefaultValue(UserRoles.Customer);
        builder.Property(user => user.Phone).HasMaxLength(30);
        builder.HasIndex(user => user.NormalizedEmail).IsUnique();
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Users_Role",
            $"[Role] IN ('{UserRoles.Admin}', '{UserRoles.Customer}')"));
    }
}

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Categories>
{
    public void Configure(EntityTypeBuilder<Categories> builder)
    {
        builder.HasMany(category => category.Products)
            .WithOne(product => product.Category)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Products>
{
    public void Configure(EntityTypeBuilder<Products> builder)
    {
        builder.Property(product => product.Price).HasPrecision(18, 2);
        builder.Property(product => product.ImageUrl).HasMaxLength(2048);
        builder.Property(product => product.IsActive).HasDefaultValue(true);
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Products_Price", "[Price] > 0");
            table.HasCheckConstraint("CK_Products_Stock", "[Stock] >= 0");
        });
    }
}

internal sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasIndex(cart => cart.UserId).IsUnique();
        builder.HasOne(cart => cart.User)
            .WithOne(user => user.Cart)
            .HasForeignKey<Cart>(cart => cart.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItems>
{
    public void Configure(EntityTypeBuilder<CartItems> builder)
    {
        builder.HasIndex(item => new { item.CartId, item.ProductId }).IsUnique();
        builder.HasOne(item => item.Cart)
            .WithMany(cart => cart.CartItems)
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Product)
            .WithMany(product => product.CartItems)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table => table.HasCheckConstraint("CK_CartItems_Quantity", "[Quantity] > 0"));
    }
}

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Orders>
{
    public void Configure(EntityTypeBuilder<Orders> builder)
    {
        builder.Property(order => order.TotalPrice).HasPrecision(18, 2);
        builder.Property(order => order.Status).HasMaxLength(20);
        builder.HasOne(order => order.User)
            .WithMany(user => user.Orders)
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Orders_TotalPrice", "[TotalPrice] > 0");
            table.HasCheckConstraint(
                "CK_Orders_Status",
                $"[Status] IN ('{OrderStatuses.Pending}', '{OrderStatuses.Processing}', '{OrderStatuses.Shipped}', '{OrderStatuses.Delivered}', '{OrderStatuses.Cancelled}')");
        });
    }
}

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItems>
{
    public void Configure(EntityTypeBuilder<OrderItems> builder)
    {
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.ProductName).HasMaxLength(100);
        builder.HasOne(item => item.Order)
            .WithMany(order => order.OrderItems)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Product)
            .WithMany(product => product.OrderItems)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0");
            table.HasCheckConstraint("CK_OrderItems_UnitPrice", "[UnitPrice] > 0");
        });
    }
}

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Reviews>
{
    public void Configure(EntityTypeBuilder<Reviews> builder)
    {
        builder.HasIndex(review => new { review.UserId, review.ProductId }).IsUnique();
        builder.HasOne(review => review.User)
            .WithMany(user => user.Reviews)
            .HasForeignKey(review => review.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(review => review.Product)
            .WithMany(product => product.Reviews)
            .HasForeignKey(review => review.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table => table.HasCheckConstraint("CK_Reviews_Rating", "[Rating] BETWEEN 1 AND 5"));
    }
}
