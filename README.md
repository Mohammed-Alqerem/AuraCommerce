# Aura Commerce

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-68217A?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoftsqlserver)

Aura Commerce is a responsive ASP.NET Core MVC storefront built with Entity Framework Core and SQL Server. It provides a complete customer journey—from discovery through checkout and order tracking—alongside an operational workspace for catalog, fulfillment, and customer management.

The interface includes a production-style account workspace, responsive layouts, accessible interaction states, theme switching, Arabic/English language support, RTL layout, and role-aware navigation.

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

### Prerequisites

- .NET SDK 9.0
- SQL Server LocalDB, SQL Server Express, or a reachable SQL Server instance

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

The default development URL is `http://localhost:5206`; the HTTPS profile also exposes `https://localhost:7219`.

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

## 🔐 Security and operations

- Admin access is intentionally separated from the customer shopping flow, and customer-only routes use server-side session checks.
- Unsafe requests use anti-forgery tokens; cart ownership, stock availability, and order access are validated on the server.
- New and updated passwords use ASP.NET Core's `PasswordHasher<TUser>`. Legacy demo passwords are migrated after a successful sign-in.
- Session cookies are HTTP-only, secure, and use `SameSite=Lax`.
- Demo accounts and the LocalDB connection string are for development only. Use environment-specific configuration and remove demo credentials before deployment.

## ✅ Quality checks

```powershell
dotnet build OnlineStore.slnx --no-restore
```

Before opening a pull request, verify the affected flow at desktop and mobile widths, in light and dark themes, and in Arabic/RTL mode.

## License

This project is provided for learning and portfolio use. Add an explicit license before distributing it as an open-source product.
