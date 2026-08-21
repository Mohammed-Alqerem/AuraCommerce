# 🛍️ Aura Commerce

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=flat-square&logo=dotnet)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![Entity Framework Core](https://img.shields.io/badge/Entity%20Framework-Core-68217A?style=flat-square)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=flat-square&logo=microsoftsqlserver)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?style=flat-square&logo=bootstrap)
![Tests](https://img.shields.io/badge/Tests-xUnit-25A162?style=flat-square)
![CI](https://img.shields.io/badge/CI-GitHub%20Actions-2088FF?style=flat-square&logo=githubactions)

**Aura Commerce** is a modern full-stack e-commerce application built with **ASP.NET Core MVC, Entity Framework Core, and SQL Server**.

It provides a complete shopping experience—from product discovery and reviews to a stock-safe transactional checkout and order history—along with a protected administration dashboard for managing products, customers, inventory, and orders.

🌐 **Live Demo:**  
[http://auracomerce.runasp.net/](http://auracomerce.runasp.net/)

---

## ✨ Features

### 🛒 Customer Experience

- Browse a paginated product catalog
- Search products by keyword
- Filter products by category
- View detailed product information
- View product ratings and customer reviews
- Submit one review per customer/product
- Add, update, and remove cart items
- Prevent purchases beyond available stock
- Complete a transaction-safe checkout
- View personal order history and order details
- Register, log in, and log out
- Manage profile information and password

### 🛠️ Administration

- Role-protected administration dashboard
- Revenue and order statistics
- Pending-order monitoring
- Low-stock product tracking
- Create and edit products safely
- Archive and restore products
- Preserve historical order information
- Manage customer orders and statuses
- View registered customers
- View store-wide order information

### 🎨 User Experience

- Responsive Bootstrap 5 interface
- Arabic and English language support
- RTL layout support
- Light and dark themes
- Mobile-friendly navigation
- Accessible form labels and controls
- Keyboard skip navigation
- Consistent validation and feedback messages

---

## 🔐 Security & Data Integrity

Aura Commerce includes several server-side protections instead of relying only on client-side validation.

- Password hashing with ASP.NET Core `PasswordHasher<TUser>`
- Role-based authorization for customers and administrators
- Centralized session keys and authorization filters
- Server-side customer role assignment during registration
- Normalized email addresses with a unique database index
- Per-client rate limiting for login and registration
- Anti-forgery validation for unsafe MVC requests
- HTTP-only session cookies
- `SameSite=Lax` cookie policy
- Local-only return URL validation to prevent open redirects
- Server-side validation for product, cart, review, profile, checkout, and order operations
- Customer-scoped carts and orders
- Database-controlled product pricing during checkout
- Serializable checkout transactions
- Stock validation during checkout
- Database uniqueness and check constraints
- Product archiving instead of destructive deletion

> Aura Commerce intentionally uses a lightweight custom session-based authentication system rather than ASP.NET Core Identity while still relying on the framework password hasher and explicit role authorization.

---

## 💳 Transaction-Safe Checkout

Checkout is designed to protect pricing, stock, and order consistency.

When an order is placed, the server:

1. Loads the authenticated customer's cart
2. Retrieves current product information from the database
3. Validates product availability and stock
4. Uses database prices instead of browser-submitted prices
5. Calculates the total on the server
6. Creates product snapshots for order history
7. Updates inventory
8. Creates the order and order items
9. Clears the customer's cart
10. Commits everything inside a serializable database transaction

If any part fails, the transaction is rolled back.

---

## 🧱 Tech Stack

| Layer | Technology |
| --- | --- |
| Backend | ASP.NET Core MVC |
| Framework | .NET 9 |
| ORM | Entity Framework Core 9 |
| Database | SQL Server / LocalDB |
| Frontend | Razor Views |
| UI | Bootstrap 5 |
| Authentication | Custom session-based authentication |
| Password Security | ASP.NET Core `PasswordHasher<TUser>` |
| Testing | xUnit + SQLite In-Memory |
| CI | GitHub Actions |
| Version Control | Git / GitHub |

---

## 🧪 Automated Testing

The project includes integration-style tests using **xUnit** and **SQLite in-memory databases**.

Current coverage includes:

- Password hashing and authentication behavior
- Role authorization filters
- Cart ownership protection
- Cart stock validation
- Review validation
- Review uniqueness
- Checkout totals
- Checkout stock validation
- Checkout ownership rules
- Cart cleanup after successful checkout

Run the test suite with:

```powershell
dotnet restore OnlineStore.slnx
dotnet build OnlineStore.slnx --no-restore
dotnet test OnlineStore.slnx --no-build --no-restore
```

---

## 🚀 Getting Started

### Prerequisites

Make sure you have:

- .NET SDK 9.0 or later capable of targeting .NET 9
- SQL Server LocalDB, SQL Server Express, or another reachable SQL Server instance
- Git

### 1. Clone the repository

```powershell
git clone https://github.com/Mohammed-Alqerem/AuraCommerce.git
cd AuraCommerce
```

### 2. Restore tools and dependencies

```powershell
dotnet tool restore
dotnet restore OnlineStore.slnx
```

### 3. Apply database migrations

```powershell
dotnet ef database update `
  --project OnlineStore/OnlineStore.csproj `
  --startup-project OnlineStore/OnlineStore.csproj
```

### 4. Run the application

```powershell
dotnet run --project OnlineStore/OnlineStore.csproj
```

---

## ⚙️ Database Configuration

The default development configuration uses SQL Server LocalDB and does not contain database credentials.

For another SQL Server instance, override the connection string using an environment variable instead of modifying committed configuration files:

```powershell
$env:ConnectionStrings__DefaultConnection = "your-development-connection-string"

dotnet run --project OnlineStore/OnlineStore.csproj
```

---

## 🌐 Deployment Configuration

Important configuration values:

| Configuration Key | Default | Description |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | LocalDB | SQL Server connection string |
| `Database:ApplyMigrationsOnStartup` | `false` | Automatically apply migrations when supported by the host |
| `Security:RequireHttps` | `false` | Require secure session cookies on HTTPS deployments |

Environment variable equivalents:

```text
ConnectionStrings__DefaultConnection
Database__ApplyMigrationsOnStartup
Security__RequireHttps
```

### runasp.net

For deployment on **runasp.net**:

- Store the production database connection in `ConnectionStrings__DefaultConnection`
- Never commit production credentials to GitHub
- Apply migrations through the deployment process when possible
- Enable `Database__ApplyMigrationsOnStartup=true` only when the host supports startup schema changes
- Enable `Security__RequireHttps=true` only when the application is consistently served over HTTPS

---

## 🗄️ Database Migrations

The `HardenAuthenticationAndDataIntegrity` migration strengthens authentication and database consistency.

It introduces:

- User roles
- Normalized email addresses
- Product activity/archive state
- Order-item product snapshots
- Unique indexes
- Check constraints
- Restrictive historical foreign keys

Apply migrations with:

```powershell
dotnet tool restore

dotnet ef database update `
  --project OnlineStore/OnlineStore.csproj `
  --startup-project OnlineStore/OnlineStore.csproj
```

> [!IMPORTANT]
> Before applying this migration to a database containing existing user-generated data, create a backup and verify there are no duplicate normalized emails, cart products, or user/product reviews. The new unique indexes intentionally reject these invalid states.

---

## 👤 Demo Accounts

Development seeding creates an administrator account and sample customer accounts.

Credentials are intentionally not stored in this repository.

For demo credentials, contact the project maintainer or deployment owner.

> [!WARNING]
> Production deployments should replace all seeded accounts, use unique strong passwords, and store secrets in environment variables or the hosting provider's secret manager.

---

## 📁 Project Structure

```text
AuraCommerce/
│
├── OnlineStore/
│   ├── Constants/
│   │   └── Roles, statuses, session keys, and store thresholds
│   │
│   ├── Controllers/
│   │   └── MVC request and response handling
│   │
│   ├── Data/
│   │   └── DbContext, EF Core configuration, and seed data
│   │
│   ├── Filters/
│   │   └── Authentication and role authorization filters
│   │
│   ├── Models/
│   │   └── Entities, form models, and page view models
│   │
│   ├── Services/
│   │   └── Checkout and business workflows
│   │
│   ├── Views/
│   │   └── Razor MVC user interface
│   │
│   └── Migrations/
│       └── Entity Framework Core migrations
│
├── AuraCommerce.Tests/
│   └── xUnit integration-style tests
│
├── .github/
│   └── workflows/
│       └── ci.yml
│
└── OnlineStore.slnx
```

---

## 🔄 Continuous Integration

GitHub Actions automatically validates the project by running:

```text
Restore
   ↓
Build
   ↓
Test
```

This helps ensure new changes do not break the application before they are merged or deployed.

---

## 📸 Screenshots

Screenshots are currently omitted from the repository.

You can explore the interface through the live deployment:

👉 [Aura Commerce Live Demo](http://auracomerce.runasp.net/)

---

## 🎯 Project Goals

Aura Commerce was built to demonstrate practical ASP.NET Core development concepts, including:

- MVC architecture
- Entity Framework Core
- Relational database design
- Transaction management
- Server-side validation
- Authentication and authorization
- Secure password storage
- Database integrity constraints
- Responsive UI development
- Automated testing
- CI workflows
- Deployment configuration

---

## 🗺️ Future Improvements

Potential improvements include:

- ASP.NET Core Identity integration
- Email verification
- Password reset workflow
- OAuth authentication
- Payment gateway integration
- Wishlist functionality
- Product image management
- Advanced product sorting
- Inventory notifications
- Admin analytics and charts
- Structured application logging
- Expanded automated test coverage
- Containerized deployment

---

## 📄 License

This project is currently provided for **learning, demonstration, and portfolio purposes**.

No open-source license has been declared.

---

## 👨‍💻 Author

**Mohammed Alqerem**

Computer Science Student & Software Developer

[GitHub](https://github.com/Mohammed-Alqerem) • [LinkedIn](https://www.linkedin.com/in/mohammed-alqerem-26b069303)

---

<p align="center">
  Built with ASP.NET Core MVC, Entity Framework Core, SQL Server & Bootstrap.
</p>
