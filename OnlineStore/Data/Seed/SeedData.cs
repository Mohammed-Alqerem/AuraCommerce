using Microsoft.EntityFrameworkCore;
using OnlineStore.Constants;
using OnlineStore.Models;

namespace OnlineStore.Data.Seed;

internal static class SeedData
{
    // Hashes for the public demo-only password documented in README.md.
    private const string AdminPasswordHash = "AQAAAAIAAYagAAAAEJ2wyfnuNORBZAX4OEmJZkWIHaa1yGwVQ+NWaLrSxeM9AZhXq2N10Si1w5TV2sjqkA==";
    private const string AhmadPasswordHash = "AQAAAAIAAYagAAAAEKhKgPLuSh6I8xZe/x+WyHyXiphNVT2/1mppogy0X2lv510pcGtFpr22bGERXnCZvA==";
    private const string SaraPasswordHash = "AQAAAAIAAYagAAAAEI9pAV+19TzA1Blbt0DPTjNoPHjZ9Og8CSHeZqEX4iNYSyNu3Qsb/iNN63xmyhvaUw==";
    private const string OmarPasswordHash = "AQAAAAIAAYagAAAAENeCscWzBfbMGK0nvla29rD7/nc+az5LPNRZw9hEYdh38r5imVSV9A6Fjay4LIEFvw==";

    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categories>().HasData(
            new Categories { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories" },
            new Categories { Id = 2, Name = "Clothing", Description = "Men and women clothing" },
            new Categories { Id = 3, Name = "Shoes", Description = "Sports and casual shoes" },
            new Categories { Id = 4, Name = "Accessories", Description = "Useful accessories and gadgets" });

        modelBuilder.Entity<Users>().HasData(
            User(1, "Mohammed Alqerem", "mohammed@gmail.com", AdminPasswordHash, "0599000001", "Jenin", UserRoles.Admin, 2026, 1, 10),
            User(2, "Ahmad Ali", "ahmad@gmail.com", AhmadPasswordHash, "0599000002", "Nablus", UserRoles.Customer, 2026, 1, 15),
            User(3, "Sara Khaled", "sara@gmail.com", SaraPasswordHash, "0599000003", "Ramallah", UserRoles.Customer, 2026, 2, 5),
            User(4, "Omar Hassan", "omar@gmail.com", OmarPasswordHash, "0599000004", "Hebron", UserRoles.Customer, 2026, 2, 20));

        modelBuilder.Entity<Products>().HasData(
            Product(1, "Wireless Mouse", "Comfortable wireless mouse for everyday use", 25m, 50, "https://images.unsplash.com/photo-1527814050087-3793815479db", 1),
            Product(2, "Mechanical Keyboard", "RGB mechanical keyboard for gaming and work", 70m, 30, "https://images.unsplash.com/photo-1587829741301-dc798b83add3", 1),
            Product(3, "USB-C Charger", "Fast charging USB-C wall charger", 35m, 40, "https://images.unsplash.com/photo-1583863788434-e58a36330cf0", 1),
            Product(4, "Classic T-Shirt", "Comfortable cotton T-Shirt", 20m, 100, "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab", 2),
            Product(5, "Hoodie", "Warm casual hoodie for everyday wear", 45m, 60, "https://images.unsplash.com/photo-1556821840-3a63f95609a7", 2),
            Product(6, "Running Shoes", "Lightweight running shoes for sports", 80m, 25, "https://images.unsplash.com/photo-1542291026-7eec264c27ff", 3),
            Product(7, "Casual Sneakers", "Modern casual sneakers", 65m, 35, "https://images.unsplash.com/photo-1549298916-b41d501d3772", 3),
            Product(8, "Smart Watch", "Smart watch with fitness tracking", 120m, 20, "https://images.unsplash.com/photo-1523275335684-37898b6baf30", 4),
            Product(9, "Backpack", "Water resistant backpack for daily use", 40m, 45, "https://images.unsplash.com/photo-1553062407-98eeb64c6a62", 4),
            Product(10, "Phone Stand", "Adjustable desk phone stand", 15m, 80, "https://images.unsplash.com/photo-1586953208448-b95a79798f07", 4));

        modelBuilder.Entity<Orders>().HasData(
            Order(1, 1, 2026, 3, 1, 95m, OrderStatuses.Delivered),
            Order(2, 2, 2026, 3, 5, 120m, OrderStatuses.Shipped),
            Order(3, 3, 2026, 3, 10, 65m, OrderStatuses.Processing),
            Order(4, 4, 2026, 3, 15, 150m, OrderStatuses.Pending));

        modelBuilder.Entity<OrderItems>().HasData(
            OrderItem(1, 1, 1, "Wireless Mouse", 1, 25m),
            OrderItem(2, 1, 2, "Mechanical Keyboard", 1, 70m),
            OrderItem(3, 2, 8, "Smart Watch", 1, 120m),
            OrderItem(4, 3, 7, "Casual Sneakers", 1, 65m),
            OrderItem(5, 4, 6, "Running Shoes", 1, 80m),
            OrderItem(6, 4, 8, "Smart Watch", 1, 70m));

        modelBuilder.Entity<Cart>().HasData(
            Cart(1, 1, 2026, 3, 1), Cart(2, 2, 2026, 3, 5),
            Cart(3, 3, 2026, 3, 10), Cart(4, 4, 2026, 3, 15));

        modelBuilder.Entity<CartItems>().HasData(
            new CartItems { Id = 1, CartId = 1, ProductId = 4, Quantity = 2 },
            new CartItems { Id = 2, CartId = 1, ProductId = 10, Quantity = 1 },
            new CartItems { Id = 3, CartId = 2, ProductId = 3, Quantity = 1 },
            new CartItems { Id = 4, CartId = 3, ProductId = 5, Quantity = 1 },
            new CartItems { Id = 5, CartId = 4, ProductId = 9, Quantity = 1 });

        modelBuilder.Entity<Reviews>().HasData(
            Review(1, 1, 1, 5, "Very good mouse and comfortable to use", 2026, 3, 2),
            Review(2, 2, 8, 4, "Good smart watch with useful features", 2026, 3, 6),
            Review(3, 3, 7, 5, "Very comfortable shoes", 2026, 3, 11),
            Review(4, 4, 4, 4, "Good quality and comfortable", 2026, 3, 16),
            Review(5, 1, 2, 5, "Excellent keyboard for gaming", 2026, 3, 3));
    }

    private static Users User(int id, string name, string email, string password, string phone, string address, string role, int year, int month, int day) =>
        new() { Id = id, Name = name, Email = email, NormalizedEmail = email.ToUpperInvariant(), Password = password, Phone = phone, Address = address, Role = role, CreatedAt = UtcDate(year, month, day) };

    private static Products Product(int id, string name, string description, decimal price, int stock, string imageUrl, int categoryId) =>
        new() { Id = id, Name = name, Description = description, Price = price, Stock = stock, ImageUrl = imageUrl, CategoryId = categoryId, IsActive = true };

    private static Orders Order(int id, int userId, int year, int month, int day, decimal total, string status) =>
        new() { Id = id, UserId = userId, OrderDate = UtcDate(year, month, day), TotalPrice = total, Status = status };

    private static OrderItems OrderItem(int id, int orderId, int productId, string productName, int quantity, decimal price) =>
        new() { Id = id, OrderId = orderId, ProductId = productId, ProductName = productName, Quantity = quantity, UnitPrice = price };

    private static Cart Cart(int id, int userId, int year, int month, int day) =>
        new() { Id = id, UserId = userId, CreatedAt = UtcDate(year, month, day) };

    private static Reviews Review(int id, int userId, int productId, int rating, string comment, int year, int month, int day) =>
        new() { Id = id, UserId = userId, ProductId = productId, Rating = rating, Comment = comment, CreatedAt = UtcDate(year, month, day) };

    private static DateTime UtcDate(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);
}
