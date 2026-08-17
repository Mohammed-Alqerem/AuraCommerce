using Microsoft.EntityFrameworkCore;
using OnlineStore.Models;

namespace OnlineStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<Users> Users { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItems> CartItems { get; set; }
        public DbSet<Reviews> Reviews { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =========================
            // Categories
            // =========================

            modelBuilder.Entity<Categories>().HasData(

                new Categories
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Electronic devices and accessories"
                },

                new Categories
                {
                    Id = 2,
                    Name = "Clothing",
                    Description = "Men and women clothing"
                },

                new Categories
                {
                    Id = 3,
                    Name = "Shoes",
                    Description = "Sports and casual shoes"
                },

                new Categories
                {
                    Id = 4,
                    Name = "Accessories",
                    Description = "Useful accessories and gadgets"
                }
            );


            // =========================
            // Users
            // =========================

            modelBuilder.Entity<Users>().HasData(

                new Users
                {
                    Id = 1,
                    Name = "Mohammed Alqerem",
                    Email = "mohammed@gmail.com",
                    Password = "123456",
                    Phone = "0599000001",
                    Address = "Jenin",
                    CreatedAt = new DateTime(2026, 1, 10)
                },

                new Users
                {
                    Id = 2,
                    Name = "Ahmad Ali",
                    Email = "ahmad@gmail.com",
                    Password = "123456",
                    Phone = "0599000002",
                    Address = "Nablus",
                    CreatedAt = new DateTime(2026, 1, 15)
                },

                new Users
                {
                    Id = 3,
                    Name = "Sara Khaled",
                    Email = "sara@gmail.com",
                    Password = "123456",
                    Phone = "0599000003",
                    Address = "Ramallah",
                    CreatedAt = new DateTime(2026, 2, 5)
                },

                new Users
                {
                    Id = 4,
                    Name = "Omar Hassan",
                    Email = "omar@gmail.com",
                    Password = "123456",
                    Phone = "0599000004",
                    Address = "Hebron",
                    CreatedAt = new DateTime(2026, 2, 20)
                }
            );


            // =========================
            // Products
            // =========================

            modelBuilder.Entity<Products>().HasData(

        new Products
        {
            Id = 1,
            Name = "Wireless Mouse",
            Description = "Comfortable wireless mouse for everyday use",
            Price = 25.00m,
            Stock = 50,
            ImageUrl = "https://images.unsplash.com/photo-1527814050087-3793815479db",
            CategoryId = 1
        },

        new Products
        {
            Id = 2,
            Name = "Mechanical Keyboard",
            Description = "RGB mechanical keyboard for gaming and work",
            Price = 70.00m,
            Stock = 30,
            ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3",
            CategoryId = 1
        },

        new Products
        {
            Id = 3,
            Name = "USB-C Charger",
            Description = "Fast charging USB-C wall charger",
            Price = 35.00m,
            Stock = 40,
            ImageUrl = "https://images.unsplash.com/photo-1583863788434-e58a36330cf0",
            CategoryId = 1
        },

        new Products
        {
            Id = 4,
            Name = "Classic T-Shirt",
            Description = "Comfortable cotton T-Shirt",
            Price = 20.00m,
            Stock = 100,
            ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab",
            CategoryId = 2
        },

        new Products
        {
            Id = 5,
            Name = "Hoodie",
            Description = "Warm casual hoodie for everyday wear",
            Price = 45.00m,
            Stock = 60,
            ImageUrl = "https://images.unsplash.com/photo-1556821840-3a63f95609a7",
            CategoryId = 2
        },

        new Products
        {
            Id = 6,
            Name = "Running Shoes",
            Description = "Lightweight running shoes for sports",
            Price = 80.00m,
            Stock = 25,
            ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff",
            CategoryId = 3
        },

        new Products
        {
            Id = 7,
            Name = "Casual Sneakers",
            Description = "Modern casual sneakers",
            Price = 65.00m,
            Stock = 35,
            ImageUrl = "https://images.unsplash.com/photo-1549298916-b41d501d3772",
            CategoryId = 3
        },

        new Products
        {
            Id = 8,
            Name = "Smart Watch",
            Description = "Smart watch with fitness tracking",
            Price = 120.00m,
            Stock = 20,
            ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30",
            CategoryId = 4
        },

        new Products
        {
            Id = 9,
            Name = "Backpack",
            Description = "Water resistant backpack for daily use",
            Price = 40.00m,
            Stock = 45,
            ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62",
            CategoryId = 4
        },

        new Products
        {
            Id = 10,
            Name = "Phone Stand",
            Description = "Adjustable desk phone stand",
            Price = 15.00m,
            Stock = 80,
            ImageUrl = "https://images.unsplash.com/photo-1586953208448-b95a79798f07",
            CategoryId = 4
        }
    );


            // =========================
            // Orders
            // =========================

            modelBuilder.Entity<Orders>().HasData(

                new Orders
                {
                    Id = 1,
                    UserId = 1,
                    OrderDate = new DateTime(2026, 3, 1),
                    TotalPrice = 95.00m,
                    Status = "Delivered"
                },

                new Orders
                {
                    Id = 2,
                    UserId = 2,
                    OrderDate = new DateTime(2026, 3, 5),
                    TotalPrice = 120.00m,
                    Status = "Shipped"
                },

                new Orders
                {
                    Id = 3,
                    UserId = 3,
                    OrderDate = new DateTime(2026, 3, 10),
                    TotalPrice = 65.00m,
                    Status = "Processing"
                },

                new Orders
                {
                    Id = 4,
                    UserId = 4,
                    OrderDate = new DateTime(2026, 3, 15),
                    TotalPrice = 150.00m,
                    Status = "Pending"
                }
            );


            // =========================
            // Order Items
            // =========================

            modelBuilder.Entity<OrderItems>().HasData(

                new OrderItems
                {
                    Id = 1,
                    OrderId = 1,
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 25.00m
                },

                new OrderItems
                {
                    Id = 2,
                    OrderId = 1,
                    ProductId = 2,
                    Quantity = 1,
                    UnitPrice = 70.00m
                },

                new OrderItems
                {
                    Id = 3,
                    OrderId = 2,
                    ProductId = 8,
                    Quantity = 1,
                    UnitPrice = 120.00m
                },

                new OrderItems
                {
                    Id = 4,
                    OrderId = 3,
                    ProductId = 7,
                    Quantity = 1,
                    UnitPrice = 65.00m
                },

                new OrderItems
                {
                    Id = 5,
                    OrderId = 4,
                    ProductId = 6,
                    Quantity = 1,
                    UnitPrice = 80.00m
                },

                new OrderItems
                {
                    Id = 6,
                    OrderId = 4,
                    ProductId = 8,
                    Quantity = 1,
                    UnitPrice = 70.00m
                }
            );


            // =========================
            // Carts
            // =========================

            modelBuilder.Entity<Cart>().HasData(

                new Cart
                {
                    Id = 1,
                    UserId = 1,
                    CreatedAt = new DateTime(2026, 3, 1)
                },

                new Cart
                {
                    Id = 2,
                    UserId = 2,
                    CreatedAt = new DateTime(2026, 3, 5)
                },

                new Cart
                {
                    Id = 3,
                    UserId = 3,
                    CreatedAt = new DateTime(2026, 3, 10)
                },

                new Cart
                {
                    Id = 4,
                    UserId = 4,
                    CreatedAt = new DateTime(2026, 3, 15)
                }
            );


            // =========================
            // Cart Items
            // =========================

            modelBuilder.Entity<CartItems>().HasData(

                new CartItems
                {
                    Id = 1,
                    CartId = 1,
                    ProductId = 4,
                    Quantity = 2
                },

                new CartItems
                {
                    Id = 2,
                    CartId = 1,
                    ProductId = 10,
                    Quantity = 1
                },

                new CartItems
                {
                    Id = 3,
                    CartId = 2,
                    ProductId = 3,
                    Quantity = 1
                },

                new CartItems
                {
                    Id = 4,
                    CartId = 3,
                    ProductId = 5,
                    Quantity = 1
                },

                new CartItems
                {
                    Id = 5,
                    CartId = 4,
                    ProductId = 9,
                    Quantity = 1
                }
            );


            // =========================
            // Reviews
            // =========================

            modelBuilder.Entity<Reviews>().HasData(

                new Reviews
                {
                    Id = 1,
                    UserId = 1,
                    ProductId = 1,
                    Rating = 5,
                    Comment = "Very good mouse and comfortable to use",
                    CreatedAt = new DateTime(2026, 3, 2)
                },

                new Reviews
                {
                    Id = 2,
                    UserId = 2,
                    ProductId = 8,
                    Rating = 4,
                    Comment = "Good smart watch with useful features",
                    CreatedAt = new DateTime(2026, 3, 6)
                },

                new Reviews
                {
                    Id = 3,
                    UserId = 3,
                    ProductId = 7,
                    Rating = 5,
                    Comment = "Very comfortable shoes",
                    CreatedAt = new DateTime(2026, 3, 11)
                },

                new Reviews
                {
                    Id = 4,
                    UserId = 4,
                    ProductId = 4,
                    Rating = 4,
                    Comment = "Good quality and comfortable",
                    CreatedAt = new DateTime(2026, 3, 16)
                },

                new Reviews
                {
                    Id = 5,
                    UserId = 1,
                    ProductId = 2,
                    Rating = 5,
                    Comment = "Excellent keyboard for gaming",
                    CreatedAt = new DateTime(2026, 3, 3)
                }
            );
        }
    }
}
