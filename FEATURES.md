# Aura Commerce Feature Catalog

This is the canonical catalog of product capabilities. Update it in the same commit whenever a feature is added, changed, disabled, or removed. `PROJECT_MEMORY.md` contains the corresponding engineering handoff and delivery rules.

## Status legend

- **Released**: present on `main`
- **Ready for review**: implemented and pushed on a feature branch, but not merged
- **Planned**: approved direction with implementation still pending
- **Policy/provider gate**: intentionally blocked on a business decision or external service

## Released foundation

| Area | Capabilities | Status |
| --- | --- | --- |
| Catalog | Server-side search, category filtering, pagination, product details, active-product filtering | Released |
| Accounts | Registration, login/logout, profile and password changes, hashed passwords, role-based sessions | Released |
| Cart | Customer-owned cart, add/update/remove items, stock validation, duplicate-row prevention | Released |
| Checkout | Server-authoritative prices, serializable transaction, stock reduction, cart cleanup | Released |
| Orders | Customer-scoped history/details and product/price snapshots | Released |
| Reviews | Ratings, bounded comments, one review per customer/product | Released |
| Administration | Dashboard, products, product archive/restore, orders, customers | Released |
| Experience | Responsive Bootstrap UI, English/Arabic layout, RTL, light/dark themes | Released |
| Engineering | EF migrations, SQL Server, xUnit/SQLite tests, GitHub Actions CI | Released |

## Store expansion

Current branch: `feature/google-apple-sign-in` (stacked on `feature/customer-support-profile-layout`)

| Area | Capabilities | Status |
| --- | --- | --- |
| Catalog discovery | Price, minimum-rating, in-stock, brand, and category filters; sorting; featured products; SKU and brand metadata | Ready for review |
| Product media | Primary image plus additional product image gallery | Ready for review |
| Wishlist | Customer wishlist page and product toggle with duplicate database protection | Ready for review |
| Account recovery | Purpose-isolated, time-limited password-reset tokens and email-confirmation tokens | Ready for review |
| Email boundary | Provider-ready `IStoreEmailSender` with safe unconfigured behavior | Ready for review |
| Checkout snapshots | Recipient, address, delivery method, subtotal, shipping, discount, tax, and total persistence | Ready for review |
| Order lifecycle | Status timeline, detailed customer/admin views, and customer notifications on real status changes | Ready for review |
| Categories | Admin edit/create and active-state management | Ready for review |
| Inventory | Searchable stock workspace, validated adjustments, and immutable adjustment history | Ready for review |
| Review moderation | Admin visibility toggle; hidden reviews excluded from catalog rating calculations | Ready for review |
| Support | Customer support requests, ticket history, admin status queue, FAQ, shipping, returns, terms, and about pages | Ready for review |
| Customer navigation | Labeled public/customer Support entry, late-expanding overflow-safe header, and a compact profile layout whose account summary stays fully visible through extra-large widths | Ready for review |
| Reports | Responsive date-filtered sales workspace with revenue KPIs, product/category breakdowns, and a styled Excel-only workbook export | Ready for review |
| Admin navigation | Consistent title-first admin headers with the shared navigation directly beneath the page context and responsive horizontal access | Ready for review |
| Project continuity | Durable technical memory and feature catalog updated with every feature | Ready for review |
| External sign-in | Provider-ready Google and Apple authentication, verified customer onboarding, secure password-confirmed linking, and polished available/unavailable login states | Ready for review |

## Policy/provider gates

| Capability | Gate |
| --- | --- |
| Live recovery and verification emails | Select a transactional email provider and configure deployment secrets |
| Live card/online payments | Select a provider and approve webhook, idempotency, failure, and reconciliation behavior |
| Coupons and promotions | Approve stacking, eligibility, expiry, and monetary calculation rules |
| Customer cancellation | Approve status eligibility, inventory restoration, and payment reversal rules |
| Returns and refunds | Approve return windows, item condition, shipping responsibility, and refund workflow |
| Live Google sign-in | Create OAuth credentials and register each deployment's `/signin-google` HTTPS callback |
| Live Apple sign-in | Configure an Apple Services ID, Developer team/key, `.p8` secret, verified domain, and `/signin-apple` HTTPS return URL |

## Adding the next feature

For every new feature, add or update a row with its status, record architectural decisions and verification in `PROJECT_MEMORY.md`, and deliver both documents with the feature's code and tests on its feature branch.
