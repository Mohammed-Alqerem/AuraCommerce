# 🛒 OnlineStore MVC Platform

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-68217A?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoftsqlserver)

OnlineStore is a modern ASP.NET Core MVC e-commerce platform built with Entity Framework Core and SQL Server. It provides a full shopping experience for customers and a dedicated admin portal for managing store operations.

The UI is based on the `stitch_modern_mvc_onlinestore_platform` design direction and includes responsive layouts, animation, theme switching, Arabic/English language support, and role-aware navigation.

---

## ✨ Key Features

### 🛍️ Customer Storefront

- Responsive home page with featured products and categories
- Product catalog with search and category filtering
- Product details page with ratings and reviews
- Shopping cart with quantity updates and item removal
- Checkout flow with order confirmation
- Customer order history and order details

### 👤 User Accounts

- Login and register pages
- Profile management
- Secure session-based login/logout
- Logout uses POST with anti-forgery protection
- Customer-only shopping flow

### 🛠️ Admin Portal

- Dedicated admin dashboard
- Product management
- Order management and status updates
- User list overview
- Admin account is separated from the customer buying flow
- Admin users cannot access cart, checkout, customer orders, or product review posting

### 🌍 Experience

- English and Arabic language toggle
- RTL layout support for Arabic
- Light and dark theme toggle
- Scroll progress indicator
- Reveal-on-scroll animations
- Product hover and tilt effects
- Animated dashboard counters
- Add-to-cart feedback animation

---

## 🧱 Tech Stack

| Layer | Technology |
| --- | --- |
| Backend | ASP.NET Core MVC |
| Runtime | .NET 9 |
| Database ORM | Entity Framework Core |
| Database | SQL Server / LocalDB |
| UI | Razor Views, Bootstrap |
| Styling | CSS custom properties, responsive layout |
| Interactivity | JavaScript |

---

## 🔐 Demo Accounts

Use these seeded accounts for quick access:

| Role | Email | Password | Access |
| --- | --- | --- | --- |
| Admin | `mohammed@gmail.com` | `123456` | Admin dashboard and store management |
| Customer | `ahmad@gmail.com` | `123456` | Shopping, cart, checkout, orders |
| Customer | `sara@gmail.com` | `123456` | Shopping, cart, checkout, orders |

The login page includes clickable demo account cards that fill the form automatically.

---

## 🚀 Getting Started

### 1. Clone the repository

```powershell
git clone https://github.com/Mohammed-Alqerem/OnlineStore.git
cd OnlineStore
```

### 2. Restore and build

```powershell
dotnet restore
dotnet build OnlineStore.slnx
```

### 3. Configure the database

The default connection string is located in:

```text
OnlineStore/appsettings.json
```

Default value:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=OnlineStoreDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

If LocalDB is not available, update the connection string to your SQL Server instance.

### 4. Run the app

```powershell
dotnet run --project OnlineStore
```

The app applies EF Core migrations automatically during startup.

---

## 📁 Project Structure

```text
OnlineStore/
├── Controllers/        MVC controllers for store, account, cart, checkout, orders, admin
├── Data/               Entity Framework DbContext and seeded data
├── Filters/            Login, customer, and admin access filters
├── Migrations/         EF Core database migrations
├── Models/             Database entities and view models
├── Views/              Razor views for customer and admin UI
├── wwwroot/            CSS, JavaScript, and static assets
└── appsettings.json    Database connection and app settings
```

---

## 🧭 Main Routes

| Area | Route |
| --- | --- |
| Home | `/` |
| Products | `/Products` |
| Product Details | `/Products/Details/{id}` |
| Login | `/Account/Login` |
| Register | `/Account/Register` |
| Profile | `/Account/Profile` |
| Cart | `/Cart` |
| Checkout | `/Checkout` |
| My Orders | `/Orders` |
| Admin Dashboard | `/Admin` |
| Admin Products | `/Admin/Products` |
| Admin Orders | `/Admin/Orders` |
| Admin Users | `/Admin/Users` |

---

## 📝 Notes

- Admin access is intentionally separated from the customer shopping flow.
- Customer-only routes are protected with session checks.
- The project currently uses seeded users and plain passwords for academic/demo purposes.
- For production, replace plain password storage with ASP.NET Core Identity or a secure password hashing flow.

