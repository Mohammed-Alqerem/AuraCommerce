# OnlineStore MVC Platform

OnlineStore is an ASP.NET Core MVC e-commerce platform built with Entity Framework Core and SQL Server. The project uses a modern storefront design inspired by the `stitch_modern_mvc_onlinestore_platform` reference folder and connects the UI to the existing seeded database models.

## Features

- Modern responsive storefront with product cards, categories, search, product details, and reviews
- Cart, checkout, order success, order history, and order details flows
- User account pages for login, register, profile, and logout
- Admin dashboard with product management, order status management, and users list
- Light and dark theme toggle
- English and Arabic language toggle with RTL support
- Scroll animations, product hover effects, animated counters, and cart button feedback
- Automatic EF Core migration on startup using the configured `DefaultConnection`

## Tech Stack

- ASP.NET Core MVC
- .NET 9
- Entity Framework Core
- SQL Server / LocalDB
- Bootstrap
- Razor Views
- JavaScript and CSS for theme, language, and interaction behavior

## Demo Accounts

Use either of these seeded users to sign in:

| Email | Password |
| --- | --- |
| `mohammed@gmail.com` | `123456` |
| `ahmad@gmail.com` | `123456` |

## Run

```powershell
dotnet run --project OnlineStore
```

The app uses the connection string in `OnlineStore/appsettings.json`:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=OnlineStoreDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

If LocalDB is not available, update the connection string to a running SQL Server instance.
