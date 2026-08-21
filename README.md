# Aura Commerce

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=flat-square&logo=dotnet)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![Entity Framework Core](https://img.shields.io/badge/Entity%20Framework-Core-68217A?style=flat-square)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=flat-square&logo=microsoftsqlserver)

Aura Commerce is a production-minded ASP.NET Core MVC storefront backed by Entity Framework Core and SQL Server. It covers the complete customer journey—from product discovery to transaction-safe checkout and order history—alongside a role-protected administration workspace.

## Live demo

[http://auracomerce.runasp.net/](http://auracomerce.runasp.net/)

## Highlights

### Customer experience

- Paginated product catalog with server-side search and category filtering
- Product details, ratings, and one review per customer/product
- Stock-aware shopping cart with ownership protection
- Server-priced, serializable checkout with product snapshots in order history
- Customer-only order history and order details
- Registration, login, logout, and profile/password management

### Administration

- Role-protected dashboard with revenue, pending-order, and low-stock metrics
- Overposting-safe product creation and editing
- Product archiving/restoration without deleting historical order data
- Order status management using centralized valid statuses
- Customer directory and order overview

### User experience

- Responsive Bootstrap UI
- English/Arabic support and RTL layout
- Light and dark themes
- Accessible labels, semantic controls, and keyboard skip navigation

## Screenshots

Screenshots are intentionally omitted from this repository. Run the application locally or open the live demo to explore the interface.

## Tech stack

- ASP.NET Core MVC on .NET 9
- Entity Framework Core 9
- SQL Server / LocalDB
- Razor Views and Bootstrap
- xUnit with SQLite in-memory integration tests
- GitHub Actions CI

## Security

- Passwords are stored as ASP.NET Core `PasswordHasher<TUser>` hashes
- Admin/customer authorization uses persisted roles and centralized session keys
- Registration assigns the customer role server-side and uses a unique normalized-email index
- Login and registration have per-client rate limiting
- Unsafe MVC requests use anti-forgery validation
- Session cookies are HTTP-only, `SameSite=Lax`, essential, and project-specific
- Return URLs are accepted only when local, preventing open redirects
- Product, cart, review, profile, checkout, and order inputs use server-side validation
- Cart items and customer orders are always scoped to the signed-in customer
- Checkout uses database prices, serializable transactions, stock checks, and database constraints
- Products are archived instead of being deleted from historical orders

The application intentionally does not use ASP.NET Core Identity; it keeps its small custom session-based authentication design while using the framework password hasher and explicit role checks.

## Demo accounts

The development seed creates one administrator and sample customer accounts. Credentials are intentionally not documented in this repository; obtain the current demo login details from the project maintainer or the deployment owner.

Production deployments must replace seeded accounts, use unique strong passwords, and keep all secrets in environment variables or the host's secret store.

## Local setup

Prerequisites:

- .NET SDK 9.0 or later capable of targeting .NET 9
- SQL Server LocalDB, SQL Server Express, or another reachable SQL Server instance

```powershell
git clone https://github.com/Mohammed-Alqerem/AuraCommerce.git
cd AuraCommerce
dotnet tool restore
dotnet restore OnlineStore.slnx
dotnet ef database update --project OnlineStore/OnlineStore.csproj --startup-project OnlineStore/OnlineStore.csproj
dotnet run --project OnlineStore/OnlineStore.csproj
```

The default development connection uses LocalDB and contains no credentials. Override it without changing committed files when using another SQL Server:

```powershell
$env:ConnectionStrings__DefaultConnection = "your-development-connection-string"
dotnet run --project OnlineStore/OnlineStore.csproj
```

## Configuration and deployment

Important configuration keys:

| Key | Default | Purpose |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | LocalDB | SQL Server connection supplied through environment/deployment secrets in production |
| `Database:ApplyMigrationsOnStartup` | `false` | Set `true` only when the host explicitly supports startup migrations |
| `Security:RequireHttps` | `false` | Set `true` on an HTTPS deployment to require secure session cookies |

For runasp.net, configure the production connection as `ConnectionStrings__DefaultConnection`; never place its password in the repository. Apply migrations with the deployment process or enable `Database__ApplyMigrationsOnStartup=true` only when the host permits startup schema changes. Set `Security__RequireHttps=true` only when the public site is consistently available over HTTPS.

## Database migrations

The `HardenAuthenticationAndDataIntegrity` migration adds roles, normalized email, product activity, order-item product snapshots, unique indexes, check constraints, and restrictive historical foreign keys. Apply it with:

```powershell
dotnet tool restore
dotnet ef database update --project OnlineStore/OnlineStore.csproj --startup-project OnlineStore/OnlineStore.csproj
```

Before applying to a database with user-generated legacy data, take a backup and verify that it has no duplicate normalized emails, cart products, or user/product reviews; the new unique indexes intentionally reject those invalid states.

## Tests

```powershell
dotnet restore OnlineStore.slnx
dotnet build OnlineStore.slnx --no-restore
dotnet test OnlineStore.slnx --no-build --no-restore
```

The tests cover authentication hashing and role filters, cart stock/ownership, review validation/uniqueness, and checkout totals, stock, ownership, and cart cleanup.

## Project structure

```text
OnlineStore/                 ASP.NET Core MVC application
  Constants/                Roles, statuses, session keys, store thresholds
  Controllers/              HTTP input and response handling
  Data/                     DbContext, entity configurations, and seed data
  Filters/                  Session login and role authorization filters
  Models/                   EF entities and form/page view models
  Services/                 Transactional checkout workflow
  Views/                    Razor UI
  Migrations/               EF Core schema history
AuraCommerce.Tests/         xUnit and SQLite integration-style tests
.github/workflows/ci.yml    Restore, build, and test workflow
```

## License

This repository is currently presented for learning and portfolio use. No open-source license has been declared.
