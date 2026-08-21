You are working on my existing ASP.NET Core MVC project:

**Repository:** `Mohammed-Alqerem/AuraCommerce`

This is an online store built with:

- ASP.NET Core MVC
- .NET 9
- Entity Framework Core
- SQL Server
- Razor Views
- Bootstrap
- Session-based authentication
- Arabic/English support
- RTL support
- Light/Dark mode

The application already includes:

- Product catalog
- Search and category filtering
- Product details
- Reviews
- Shopping cart
- Checkout
- Orders
- User registration/login
- User profile
- Admin dashboard
- Product management
- Order management
- User management
- EF Core migrations
- Seed data

The project already works. **Do not rebuild it from scratch.**

Your job is to improve the existing architecture, security, maintainability, correctness, and production quality while preserving the current UI and existing working functionality.

---

# Main Goal

Upgrade AuraCommerce from a good student/junior ASP.NET Core MVC project into a cleaner and more professional portfolio-quality application.

Do not unnecessarily redesign the UI.

Do not remove existing features.

Do not change working behavior unless there is a good engineering/security reason.

Before editing anything, inspect the current repository structure and understand the relationships between:

- Controllers
- Models
- ViewModels
- Filters
- ApplicationDbContext
- Program.cs
- Razor views
- Sessions
- EF Core migrations

Then implement the improvements below carefully.

---

# 1. Replace `UserId == 1` Admin Authorization

This is the highest-priority issue.

Currently the application determines whether a user is an admin using logic similar to:

```csharp
existingUser.Id == 1
```

and:

```csharp
if (userId.Value != 1)
```

This is not a proper authorization design.

## Required fix

Add a proper role system to the existing `Users` entity.

Preferred simple design:

```csharp
public string Role { get; set; } = "Customer";
```

Supported roles:

```text
Admin
Customer
```

Do not overengineer this project by migrating the whole application to ASP.NET Core Identity unless doing so is truly necessary.

For this project, keeping the current custom authentication system is acceptable, but role authorization must no longer depend on the database user ID.

Update:

- `Users` model
- EF Core configuration
- migration
- seed data
- login logic
- session role
- admin filters
- customer filters
- any Razor view logic
- any controller logic

The admin demo user should have:

```text
Role = Admin
```

Normal users should have:

```text
Role = Customer
```

After login:

```csharp
HttpContext.Session.SetString("UserRole", existingUser.Role);
```

The admin filter should verify:

```text
UserRole == "Admin"
```

and not check for `UserId == 1`.

Likewise, customer authorization should use role information correctly.

Centralize role names if useful to avoid magic strings, for example:

```csharp
public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
}
```

Use whichever clean approach best matches the current project.

---

# 2. Remove Plain-Text Seed Passwords

The application currently contains seed users whose passwords are stored directly as:

```text
123456
```

This must not remain as plain text inside persisted database seed data.

Registration already uses `PasswordHasher<Users>`.

Login also supports hashing and currently has migration logic for legacy plain-text passwords.

Improve this.

## Required outcome

All users stored in the database should ultimately contain password hashes.

Choose a maintainable solution compatible with EF Core migrations.

Possible approaches include:

- generate deterministic/precomputed password hashes for demo accounts,
- seed demo users through a startup database initializer using `PasswordHasher`,
- create a dedicated `DbSeeder` or `DatabaseSeeder`,
- or another clean approach.

Avoid placing authentication/seeding business logic directly inside `OnModelCreating` if a better architecture is practical.

Demo users can continue using a known password such as:

```text
123456
```

for portfolio demonstration, but the database value must be hashed.

Also make sure the README clearly says that these are demo credentials only.

Do not expose real credentials or secrets.

---

# 3. Improve Authentication Code

Keep the application's existing session-based authentication, but clean it up.

Current session values include approximately:

```text
UserId
UserName
UserRole
```

Create a clean and consistent authentication/session approach.

Avoid repeatedly writing hardcoded session keys throughout the application.

Prefer constants such as:

```csharp
public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string UserName = "UserName";
    public const string UserRole = "UserRole";
}
```

If a small helper/service would make the code significantly cleaner, create one.

Do not introduce unnecessary complexity.

Ensure logout completely clears authentication-related session state.

---

# 4. Keep Secure Login Behavior

Preserve the following good behavior already in the application:

- password hashing
- CSRF protection
- local-only `returnUrl`
- redirect to requested page after login
- secure session cookies
- `HttpOnly`
- secure cookie policy
- session timeout

Ensure that open redirects cannot be introduced.

Continue using:

```csharp
Url.IsLocalUrl(returnUrl)
```

or an equivalent safe approach.

---

# 5. Fix Admin Product Overposting

The current admin product form appears to bind the database entity directly:

```csharp
public IActionResult ProductForm(Products product)
```

and may use:

```csharp
_context.Products.Update(product);
```

This makes the code more vulnerable to overposting and accidentally changing properties that should not be modified through the form.

## Required fix

Create a dedicated ViewModel, for example:

```csharp
ProductFormViewModel
```

Include only editable fields such as:

```text
Id
Name
Description
Price
Stock
ImageUrl
CategoryId
```

Add proper validation.

For create:

- construct a new `Products` entity from the ViewModel.

For edit:

- retrieve the existing entity from the database first,
- update only approved properties,
- call `SaveChanges()`.

Do not blindly call:

```csharp
_context.Products.Update(model)
```

on user-controlled input.

Keep the current admin UI working.

---

# 6. Strengthen Product Validation

Add or verify sensible server-side validation for products.

Examples:

```text
Name: required
Description: required or appropriately optional
Price: > 0
Stock: >= 0
CategoryId: valid existing category
ImageUrl: reasonable URL validation where practical
```

Do not rely only on browser validation.

If the submitted category does not exist, reject the request gracefully.

---

# 7. Improve Review Security and Validation

The current review action should be strengthened.

Requirements:

- product must exist,
- rating must be from `1` to `5`,
- logged-in customer only,
- comment should have a reasonable maximum length,
- prevent invalid or extremely large input.

Prefer allowing only one review per user per product.

Implement a database uniqueness constraint for:

```text
UserId + ProductId
```

If the user already reviewed a product, either:

- update their existing review,

or:

- reject the duplicate with a friendly message.

Choose the behavior that best matches the existing UX.

Optional improvement, if cleanly implemented:

Only allow customers who purchased the product to review it.

If this would create too much change to the current UX, do not force it.

---

# 8. Add Important Database Constraints

Inspect all entity relationships and improve database integrity.

At minimum consider:

## Users

Unique email index.

Email comparisons should be handled consistently.

Do not depend only on application-level:

```csharp
Any(...)
```

because concurrent requests could still insert duplicates.

Create a database unique index.

For example conceptually:

```csharp
HasIndex(u => u.Email)
    .IsUnique();
```

If normalized email handling is appropriate, implement it cleanly.

---

## Cart

There should normally be only one cart per user.

Add a unique constraint/index on:

```text
Cart.UserId
```

if compatible with the current model.

---

## CartItems

Prevent duplicate rows of the same product inside the same cart.

Unique constraint:

```text
CartId + ProductId
```

---

## Reviews

Unique constraint:

```text
UserId + ProductId
```

---

## Other constraints

Inspect:

- foreign key behavior
- required relationships
- decimal precision
- maximum string lengths
- delete behaviors

Use appropriate EF Core fluent configuration.

Do not introduce cascade deletes that could accidentally destroy historical orders.

---

# 9. Preserve Order History

Historical order information must remain reliable even if product information changes later.

The current `OrderItems` already stores:

```text
UnitPrice
```

which is good.

Check whether important historical information should also be preserved instead of always depending on the live `Product`.

For example, consider:

```text
ProductName
```

snapshot in `OrderItems`.

Only add this if it improves the current design without causing unnecessary complexity.

Do NOT delete old order history when a product is removed.

If product deletion currently conflicts with existing order items, improve the behavior.

Preferred options:

- prevent deletion when referenced by historical orders,
- soft-delete products,
- or safely configure restrictive FK behavior.

Choose the simplest clean solution suitable for this project.

---

# 10. Improve Product Delete Behavior

The admin currently physically deletes products.

This can become problematic when products are referenced by:

- OrderItems
- CartItems
- Reviews

Inspect the current FK configuration.

Implement safe deletion behavior.

Preferred solution for an e-commerce application:

Add something like:

```csharp
public bool IsActive { get; set; } = true;
```

Instead of deleting products that have historical references, allow admin to deactivate/archive them.

Product catalog should show only active products.

Admin should still be able to see archived/deactivated products if practical.

If implementing soft delete would require excessive changes, at minimum prevent destructive deletion when the product is referenced and show a friendly admin message.

---

# 11. Checkout — Preserve and Improve Existing Good Logic

The current checkout logic already has important strengths:

- totals are calculated server-side,
- product prices come from database,
- stock is checked server-side,
- order and order items are created server-side,
- stock is reduced,
- cart items are removed,
- a serializable database transaction is used.

Preserve these behaviors.

Review the checkout code for race conditions and correctness.

Improve it if necessary.

Ensure:

```text
Stock can never become negative.
```

Do not trust prices or totals posted from HTML forms.

The authoritative total must always come from database products.

---

# 12. Checkout Transaction Handling

Improve transaction handling where necessary.

Use async EF Core APIs if converting the controller cleanly.

Ensure rollback happens automatically/safely when exceptions occur.

Prefer patterns such as:

```csharp
await using var transaction =
    await _context.Database.BeginTransactionAsync(...);
```

Then:

```csharp
await transaction.CommitAsync();
```

Do not swallow exceptions silently.

---

# 13. Use Async EF Core

Where practical, convert database operations from synchronous methods such as:

```csharp
ToList()
FirstOrDefault()
SaveChanges()
Count()
Any()
```

to async counterparts:

```csharp
ToListAsync()
FirstOrDefaultAsync()
SaveChangesAsync()
CountAsync()
AnyAsync()
```

Controller actions should use:

```csharp
async Task<IActionResult>
```

Do not convert code blindly.

Only use async where database/network I/O is involved.

---

# 14. Use `AsNoTracking()` for Read-Only Queries

For read-only pages such as:

- product catalog
- product details where no tracked update is needed
- admin lists
- orders list
- dashboard queries

use:

```csharp
AsNoTracking()
```

where appropriate.

Do not use it when entities need to be updated in the same context.

---

# 15. Improve Search Queries

The product catalog currently supports search.

Review search behavior.

Requirements:

- empty search should work,
- category filter should work,
- search should remain server-side,
- avoid loading all products before filtering.

If SQL Server collation already provides case-insensitive search, avoid unnecessary:

```csharp
ToLower()
```

on database columns.

Prefer efficient EF-translatable expressions.

Consider pagination if the catalog currently loads all products.

---

# 16. Add Pagination

If the product catalog currently loads all products at once, implement pagination.

Suggested:

```text
12 products per page
```

Keep:

- search term
- category
- current page

in query parameters.

Example:

```text
/Products?searchTerm=mouse&categoryId=1&page=2
```

Create a reusable pagination model if useful.

Keep the existing visual design consistent.

---

# 17. Improve Admin Dashboard Queries

The admin dashboard currently performs multiple count/sum/list queries.

Review the implementation.

Make sure:

- revenue calculation handles an empty database safely,
- cancelled orders are not incorrectly counted as revenue if that is current business intent,
- pending orders are calculated correctly,
- low-stock threshold is centralized instead of magic-number duplication.

Example:

```csharp
private const int LowStockThreshold = 10;
```

Or put it in an appropriate constants/settings class.

---

# 18. Use Enums or Constants for Order Status

The project currently uses order status strings such as:

```text
Pending
Processing
Shipped
Delivered
Cancelled
```

Avoid repeating raw strings across the application.

Use either:

- enum,
- centralized constants,
- or another simple safe approach.

Example:

```csharp
public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
}
```

Update:

- models
- admin
- checkout
- dashboard
- views

as appropriate.

Do not break existing database data.

---

# 19. Improve Date Handling

The project currently uses:

```csharp
DateTime.Now
```

in multiple places.

Prefer:

```csharp
DateTime.UtcNow
```

for persisted timestamps.

Only convert to local display time at the presentation layer if necessary.

If changing existing timestamps would create excessive migration complexity, at least make new writes consistent.

---

# 20. Improve Money / Decimal Configuration

Ensure all monetary properties use `decimal`.

Configure database precision explicitly.

Example:

```csharp
.HasPrecision(18, 2)
```

for fields such as:

```text
Products.Price
Orders.TotalPrice
OrderItems.UnitPrice
```

Do not use `float` or `double` for money.

---

# 21. Improve Entity Naming Where Reasonable

The project currently has entity names such as:

```text
Users
Products
Categories
Orders
Reviews
```

which are plural class names.

Standard .NET style would normally use:

```text
User
Product
Category
Order
Review
```

However, renaming everything could create a large unnecessary migration and view/controller refactor.

Do not rename everything solely for style unless it can be done safely without destabilizing the project.

Focus on functional and architectural improvements first.

---

# 22. Split Large `ApplicationDbContext` Seed Code

`ApplicationDbContext.OnModelCreating()` currently contains a large amount of seed data.

Clean this up.

Possible structure:

```text
Data/
    ApplicationDbContext.cs
    Seed/
        DatabaseSeeder.cs
        SeedData.cs
```

or a similar clean structure.

Keep `ApplicationDbContext` focused on:

- DbSets
- relationships
- constraints
- indexes
- model configuration

If using `HasData`, move configuration into separate entity configurations if practical.

---

# 23. Consider Entity Configuration Classes

For better maintainability, consider:

```text
Data/
    Configurations/
        UserConfiguration.cs
        ProductConfiguration.cs
        OrderConfiguration.cs
        CartConfiguration.cs
        ReviewConfiguration.cs
```

using:

```csharp
IEntityTypeConfiguration<T>
```

Then:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(...)
```

Do this only if it genuinely improves the codebase.

Do not overengineer a small project just to add layers.

---

# 24. Improve Error Handling

The application already uses:

```csharp
app.UseExceptionHandler("/Home/Error");
```

outside development.

Keep it.

Make sure users get sensible results for:

- invalid product IDs,
- invalid order IDs,
- invalid cart item IDs,
- unauthorized access,
- missing categories,
- duplicate email,
- invalid checkout state.

Use appropriately:

```text
NotFound()
BadRequest()
Redirect
ModelState
TempData
```

Do not expose stack traces in production.

---

# 25. Improve Security Headers

The app currently sets headers including:

```text
X-Content-Type-Options
X-Frame-Options
Referrer-Policy
```

Keep them.

Review whether a basic Content Security Policy can safely be added.

However, the project may use external resources such as:

- Google Fonts
- CDN scripts/styles
- external image sources

Do not add a CSP that breaks the application.

If adding CSP safely requires significant work, leave a clear comment or recommendation instead of breaking the frontend.

---

# 26. Cookie / Session Security

Preserve current secure settings such as:

```csharp
HttpOnly = true
SecurePolicy = Always
SameSite = Lax
```

Review whether:

```csharp
Cookie.Name
```

should be explicitly set to a project-specific name.

For example:

```text
.AuraCommerce.Session
```

Do not store sensitive information such as passwords inside session.

---

# 27. Add Rate-Limit Protection for Login

Use ASP.NET Core built-in rate limiting if practical.

Protect sensitive endpoints such as:

```text
POST /Account/Login
POST /Account/Register
```

against excessive requests.

Do not lock normal users out aggressively.

Use a reasonable simple rate-limit policy.

Avoid third-party libraries unless necessary.

---

# 28. Validate Profile Changes

When updating the user's profile:

- trim name/email,
- validate email,
- enforce unique email,
- validate phone length/format reasonably,
- validate address length,
- validate new password strength.

If changing password, consider requiring the current password.

For a portfolio project this is preferred if it can be implemented cleanly.

Never display or return the stored password hash.

---

# 29. Add Password Validation

Introduce reasonable password rules.

For example:

```text
minimum 8 characters
```

Potentially require a combination of character types if not excessive.

Keep demo account usability in mind.

If demo password remains `123456`, either update demo credentials to meet the new password policy or apply the policy only to newly created passwords after updating demo data.

Prefer changing demo password to something such as:

```text
Aura123!
```

if updating README at the same time.

---

# 30. Review Sensitive Information

Search the repository for:

- passwords
- API keys
- secrets
- connection strings with credentials
- tokens
- private keys

The local development connection string:

```text
(localdb)\MSSQLLocalDB
```

is acceptable because it contains no credentials.

Production connection strings must not be committed.

Use:

- environment variables,
- deployment secrets,
- User Secrets for development if necessary.

Do not commit production database passwords.

---

# 31. Improve `.gitignore`

The `.gitignore` already ignores:

```text
bin/
obj/
.vs/
*.user
```

but generated files have already been committed.

Do not delete source code.

Remove tracked generated files from Git history/index going forward:

```text
.vs/
OnlineStore/bin/
OnlineStore/obj/
OnlineStore/OnlineStore.csproj.user
```

Do not commit these again.

Preserve `.gitignore`.

---

# 32. Rename Solution / Project Where Safe

The GitHub repository is named:

```text
AuraCommerce
```

but the actual solution/project still contains names like:

```text
OnlineStore
OnlineStore.slnx
namespace OnlineStore
```

This creates inconsistent branding.

Evaluate whether a safe rename to:

```text
AuraCommerce
```

is practical.

Ideally:

```text
AuraCommerce.slnx
AuraCommerce.csproj
namespace AuraCommerce
```

However, this touches many files.

Only perform it if Codex can do so safely and verify the project still builds.

If there is a meaningful risk of breaking migrations/deployment, leave the internal namespace as-is and fix branding only where user-visible.

---

# 33. Add Services Only Where They Improve Separation

Currently controllers contain substantial EF Core business logic.

Do NOT convert the whole project to a complicated Repository + Unit of Work architecture.

EF Core already acts similarly to those abstractions.

However, extract real business logic where useful.

Possible services:

```text
CartService
CheckoutService
CurrentUserService
PasswordService
```

Only create services when they reduce duplication and make controllers easier to understand/test.

Avoid unnecessary architectural ceremony.

---

# 34. Improve Controller Size

Controllers should primarily handle:

```text
HTTP input
validation
authorization
calling application/business logic
selecting response/view
```

Move complicated repeated business logic out when helpful.

The checkout workflow is a good candidate for extraction if it improves testability.

Do not create dozens of tiny abstractions.

---

# 35. Add Logging

Use:

```csharp
ILogger<T>
```

for important events.

Examples:

```text
Failed login attempt
Successful checkout
Order creation failure
Admin product update
Unexpected database error
```

Never log:

- passwords
- password hashes
- sensitive connection strings
- payment information

Keep logs useful and minimal.

---

# 36. Add Tests

Create a test project, for example:

```text
AuraCommerce.Tests
```

or compatible with the current project naming.

Use:

```text
xUnit
```

Prefer unit tests plus selected integration-style tests.

At minimum test important business behavior.

## Authentication tests

- valid password succeeds
- invalid password fails
- normal user cannot access admin functionality
- admin can access admin functionality

## Cart tests

- add product
- quantity cannot exceed available stock
- users cannot update another user's cart item

## Review tests

- invalid rating rejected
- duplicate review prevented

## Checkout tests

- empty cart cannot checkout
- insufficient stock prevents checkout
- order total uses database price
- checkout reduces stock
- checkout clears cart
- order belongs to correct user
- concurrent stock handling remains safe

Focus on meaningful tests rather than chasing coverage percentage.

---

# 37. Keep IDOR Protection

The current cart code correctly restricts cart-item operations using both:

```text
CartItem.Id
AND
Cart.UserId == logged-in user
```

Preserve this.

Review all other endpoints for IDOR vulnerabilities.

Especially:

```text
Orders
Checkout success
Profile
Reviews
Admin operations
```

A normal user must never be able to view or modify another user's private data simply by changing an ID in the URL.

---

# 38. Review Orders Controller

Ensure:

```text
/Orders
/Orders/Details/{id}
```

only returns orders belonging to the current customer.

Admin order pages may access all orders.

Normal users must not access other users' order details.

Return:

```csharp
NotFound()
```

or appropriate authorization behavior for unauthorized order IDs.

---

# 39. Prevent Mass Assignment Everywhere

Inspect every POST action.

Look for binding directly to database entities.

Where input should be restricted, use ViewModels.

Important areas:

```text
Registration
Profile
Admin product edit
Checkout
Reviews
Order status
```

Do not allow a customer to submit sensitive fields such as:

```text
Id
Role
TotalPrice
UserId
OrderStatus
CreatedAt
```

unless those values are explicitly server-controlled.

---

# 40. ViewModels

Keep entity models separate from form/input models where useful.

Recommended ViewModels include:

```text
LoginViewModel
RegisterViewModel
ProfileViewModel
ProductFormViewModel
CheckoutViewModel
ProductCatalogViewModel
CartViewModel
AdminDashboardViewModel
```

Some already exist.

Reuse them rather than creating duplicates.

Consider creating `RegisterViewModel` instead of binding registration directly to `Users`.

It should expose only:

```text
Name
Email
Password
ConfirmPassword
Phone
Address
```

The server should assign:

```text
Role
CreatedAt
Id
```

---

# 41. Improve Registration

Registration should:

1. normalize and trim data,
2. validate form,
3. check email uniqueness,
4. hash password,
5. assign customer role server-side,
6. create user,
7. create cart,
8. save atomically.

Prefer using one transaction or one appropriate `SaveChanges()` flow where possible.

Do not let registration bind/set:

```text
Role = Admin
```

from request data.

---

# 42. Database Migration Safety

Create proper EF Core migrations for schema changes.

Do NOT:

- manually edit production database tables outside migrations,
- delete existing migrations unless absolutely necessary,
- recreate the database just to simplify development,
- destroy existing demo/order data unnecessarily.

Migration names should be descriptive, for example:

```text
AddUserRolesAndConstraints
AddProductActiveFlag
AddReviewUniqueIndex
```

---

# 43. Do Not Auto-Migrate Production Blindly

The application currently runs:

```csharp
db.Database.Migrate();
```

during startup.

For a student/demo hosted project this is convenient.

But for better production architecture, evaluate this behavior.

Preferred:

- keep automatic migrations only for development/demo if necessary,
- or make it controlled by configuration.

Do not break the existing deployment.

If runasp.net depends on startup migration, keep functionality but document the tradeoff.

---

# 44. Improve Configuration

Move configurable values out of magic numbers where sensible.

Examples:

```text
Session timeout
Low stock threshold
Products per page
Maximum review length
```

Use:

```text
appsettings.json
```

or constants/options if appropriate.

Do not put trivial constants into configuration unnecessarily.

---

# 45. Localization / Arabic / RTL

The project has Arabic/English and RTL support.

Preserve it.

Do not introduce English-only UI text into views without considering the existing localization pattern.

Inspect how localization currently works.

If strings are currently duplicated directly in views, improve only where safe.

Do not rewrite the whole localization system unless necessary.

Verify that any new validation/admin messages work acceptably with the current bilingual design.

---

# 46. Dark Mode / Frontend

Preserve:

- current Bootstrap layout
- responsive design
- dark mode
- light mode
- RTL
- navigation
- mobile layout

Do not replace Bootstrap with Tailwind or another framework.

Do not redesign working pages unless needed to support a functional change.

Keep the current Aura Commerce visual identity.

---

# 47. Improve Accessibility

Make small improvements where obvious:

- labels tied to inputs,
- buttons use correct button types,
- images contain meaningful `alt`,
- form validation accessible,
- icon-only buttons have labels,
- sufficient semantic headings,
- avoid clickable `<div>` elements where proper links/buttons should be used.

Do not perform an unnecessary visual redesign.

---

# 48. Improve README

Rewrite/update `README.md` professionally after code changes.

The README should include:

## Header

```text
Aura Commerce
```

Short professional description.

## Live Demo

Include:

```text
http://auracomerce.runasp.net/
```

or HTTPS if supported.

## Screenshots

Create placeholders/sections if screenshots already exist.

Do not invent screenshot URLs.

## Features

Customer:

- product catalog
- search/filter
- product details
- reviews
- cart
- checkout
- orders
- profile
- authentication

Admin:

- dashboard
- product management
- order management
- user management

Other:

- responsive design
- Arabic/English
- RTL
- light/dark themes

## Tech Stack

```text
ASP.NET Core MVC
.NET 9
Entity Framework Core
SQL Server
Bootstrap
Razor Views
```

## Security

Mention:

- hashed passwords
- session authorization
- role-based access
- anti-forgery protection
- secure cookies
- validation
- transaction-safe checkout

Do not exaggerate.

## Demo Accounts

If demo credentials are intentionally public, clearly label them:

```text
Demo credentials only.
Do not reuse these passwords anywhere else.
```

## Setup

Fix any clone command still pointing to the old `OnlineStore` repo.

Correct clone command:

```bash
git clone https://github.com/Mohammed-Alqerem/AuraCommerce.git
```

Include:

```bash
cd AuraCommerce
dotnet restore
dotnet ef database update
dotnet run
```

Adapt paths if the `.csproj` remains under a subfolder.

---

# 49. Add Useful Repository Files

Consider adding:

```text
LICENSE
CONTRIBUTING.md
```

Only if appropriate.

A simple MIT license is acceptable only if the repository owner intends the project to be open source.

Do not invent licensing intentions if not already clear.

A `.editorconfig` could also improve consistency.

---

# 50. Add CI

Create a simple GitHub Actions workflow if none exists.

Example responsibilities:

```text
checkout
setup .NET
restore
build
test
```

Run on:

```text
push
pull_request
```

Do not include deployment secrets.

Example target:

```text
ubuntu-latest
.NET 9
```

Verify the project can build on the selected runner.

If LocalDB dependencies make tests incompatible with Linux, adjust tests/database provider accordingly.

---

# 51. Keep Production Deployment Working

The application is already hosted.

Do not make changes that unnecessarily break hosting on:

```text
runasp.net
```

Pay special attention to:

- connection string loading
- HTTPS behavior
- EF Core migrations
- session cookies
- environment-specific settings
- SQL Server compatibility
- static assets

Production connection string should still be supplied by the hosting environment/configuration rather than hardcoded.

---

# 52. Security Review

After implementing the changes, explicitly inspect for:

```text
SQL injection
XSS
CSRF
IDOR
mass assignment
open redirect
plain-text passwords
hardcoded secrets
weak authorization
unsafe redirects
stock race conditions
negative quantities
tampered prices
duplicate email race conditions
duplicate cart items
duplicate reviews
```

ASP.NET Core/EF Core already parameterize normal LINQ queries, so do not add unnecessary manual SQL.

---

# 53. Code Quality

Clean up:

- unnecessary `using` statements,
- duplicate `[ValidateAntiForgeryToken]` if a global anti-forgery filter already guarantees it, but retaining explicit annotations is acceptable,
- repeated magic strings,
- duplicated session code,
- repeated queries,
- inconsistent naming,
- dead code,
- stale comments.

Do not aggressively refactor just for style.

Correctness and readability matter more.

---

# 54. Do Not Overengineer

Very important.

Do NOT convert this into:

- Clean Architecture with 7 projects,
- CQRS,
- MediatR,
- event sourcing,
- microservices,
- generic repository everywhere,
- unnecessary mapping layers,
- unnecessary dependency-heavy architecture.

This is a portfolio ASP.NET Core MVC e-commerce application.

Keep it:

```text
simple
secure
clean
understandable
maintainable
interview-friendly
```

A recruiter or junior developer should be able to understand the architecture easily.

---

# 55. Preserve Working Features

Do not remove or break:

```text
registration
login
logout
profile
products
search
category filtering
reviews
cart
checkout
orders
admin dashboard
admin product management
order status updates
Arabic
English
RTL
dark mode
light mode
responsive design
```

---

# 56. Verify Everything After Changes

Run:

```bash
dotnet restore
dotnet build
```

Run tests:

```bash
dotnet test
```

If migrations are added, verify them.

Test manually or logically:

```text
Register customer
Login customer
Logout
Login admin
Access admin dashboard
Customer blocked from admin
Browse products
Search products
Filter categories
Open product details
Add review
Add product to cart
Update quantity
Remove cart item
Checkout
Stock decreases
Cart clears
Order appears
Customer can see own order
Customer cannot access another user's order
Admin sees orders
Admin changes order status
Admin creates product
Admin edits product
Admin archives/deletes product safely
Profile update works
Password change works
Arabic/RTL still works
Dark/light mode still works
```

---

# 57. Final Output Required From Codex

After finishing, provide a concise engineering report.

Include:

## Changes Made

List major changes.

## Security Improvements

Explain what security issues were fixed.

## Database Changes

List migrations, indexes, constraints, and schema changes.

## New Files

List important new:

```text
ViewModels
Services
Constants
Configurations
Tests
CI files
```

## Files Modified

Mention the main modified files.

## Breaking Changes

Clearly state if any exist.

## Migration Instructions

Provide exact commands needed.

## Deployment Notes

Explain anything I need to update on runasp.net.

## Remaining Recommendations

Only include improvements that were intentionally left out.

---

# Priority Order

If the full task becomes too broad, prioritize work in this exact order:

1. Proper Admin/Customer roles
2. Remove persisted plain-text passwords
3. Fix authorization
4. Prevent overposting
5. Add database uniqueness/integrity constraints
6. Review IDOR vulnerabilities
7. Harden checkout and stock handling
8. Safe product deletion/archive
9. Async EF Core
10. Registration/profile security
11. Reviews
12. Tests
13. Repository cleanup
14. README
15. CI
16. Minor architecture/refactoring

---

# Important Working Rules

- Inspect the repository before editing.
- Work against the existing code, not an imagined architecture.
- Preserve current behavior when possible.
- Keep changes focused and production-minded.
- Do not leave placeholder code.
- Do not use fake implementations.
- Do not comment out broken functionality instead of fixing it.
- Do not silently delete data.
- Do not expose secrets.
- Do not change frontend framework.
- Do not rewrite the entire application.
- Prefer incremental, reviewable improvements.
- Make sure the project builds after the changes.
- When making migrations, ensure existing data can migrate safely.

The result should feel like the same **Aura Commerce** application, but with stronger ASP.NET Core engineering, safer authentication/authorization, cleaner EF Core usage, better database integrity, tests, and a more professional GitHub repository.