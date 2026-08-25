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
        builder.Property(user => user.EmailConfirmed).HasDefaultValue(false);
        builder.HasIndex(user => user.NormalizedEmail).IsUnique();
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Users_Role",
            $"[Role] IN ('{UserRoles.Admin}', '{UserRoles.Customer}')"));
    }
}

internal sealed class UserExternalLoginConfiguration : IEntityTypeConfiguration<UserExternalLogin>
{
    public void Configure(EntityTypeBuilder<UserExternalLogin> builder)
    {
        builder.Property(login => login.Provider).HasMaxLength(32);
        builder.Property(login => login.ProviderKey).HasMaxLength(256);
        builder.HasIndex(login => new { login.Provider, login.ProviderKey }).IsUnique();
        builder.HasIndex(login => new { login.UserId, login.Provider }).IsUnique();
        builder.HasOne(login => login.User)
            .WithMany(user => user.ExternalLogins)
            .HasForeignKey(login => login.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Categories>
{
    public void Configure(EntityTypeBuilder<Categories> builder)
    {
        builder.Property(category => category.IsActive).HasDefaultValue(true);
        builder.HasIndex(category => category.Name).IsUnique();
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
        builder.Property(product => product.Sku).HasMaxLength(50);
        builder.Property(product => product.Brand).HasMaxLength(80);
        builder.Property(product => product.IsFeatured).HasDefaultValue(false);
        builder.HasIndex(product => product.Sku).IsUnique().HasFilter("[Sku] <> ''");
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Products_Price", "[Price] > 0");
            table.HasCheckConstraint("CK_Products_Stock", "[Stock] >= 0");
            table.HasCheckConstraint("CK_Products_LowStockThreshold", "[LowStockThreshold] >= 0");
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
        builder.Property(order => order.Subtotal).HasPrecision(18, 2);
        builder.Property(order => order.ShippingAmount).HasPrecision(18, 2);
        builder.Property(order => order.TaxAmount).HasPrecision(18, 2);
        builder.Property(order => order.Status).HasMaxLength(20);
        builder.Property(order => order.ShippingName).HasMaxLength(80);
        builder.Property(order => order.ShippingEmail).HasMaxLength(256);
        builder.Property(order => order.ShippingPhone).HasMaxLength(30);
        builder.Property(order => order.ShippingAddress).HasMaxLength(300);
        builder.Property(order => order.DeliveryMethod).HasMaxLength(40);
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
        builder.Property(review => review.IsVisible).HasDefaultValue(true);
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

internal sealed class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.HasIndex(item => new { item.UserId, item.ProductId }).IsUnique();
        builder.HasOne(item => item.User).WithMany(user => user.WishlistItems)
            .HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Product).WithMany(product => product.WishlistItems)
            .HasForeignKey(item => item.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasIndex(image => new { image.ProductId, image.SortOrder });
        builder.HasOne(image => image.Product).WithMany(product => product.Images)
            .HasForeignKey(image => image.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasIndex(history => new { history.OrderId, history.CreatedAt });
        builder.HasOne(history => history.Order).WithMany(order => order.StatusHistory)
            .HasForeignKey(history => history.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class InventoryAdjustmentConfiguration : IEntityTypeConfiguration<InventoryAdjustment>
{
    public void Configure(EntityTypeBuilder<InventoryAdjustment> builder)
    {
        builder.HasIndex(adjustment => new { adjustment.ProductId, adjustment.CreatedAt });
        builder.HasOne(adjustment => adjustment.Product).WithMany(product => product.InventoryAdjustments)
            .HasForeignKey(adjustment => adjustment.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.HasIndex(ticket => new { ticket.Status, ticket.CreatedAt });
        builder.HasOne(ticket => ticket.User).WithMany(user => user.SupportTickets)
            .HasForeignKey(ticket => ticket.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class StoreNotificationConfiguration : IEntityTypeConfiguration<StoreNotification>
{
    public void Configure(EntityTypeBuilder<StoreNotification> builder)
    {
        builder.HasIndex(notification => new { notification.UserId, notification.IsRead, notification.CreatedAt });
        builder.HasOne(notification => notification.User).WithMany(user => user.Notifications)
            .HasForeignKey(notification => notification.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
