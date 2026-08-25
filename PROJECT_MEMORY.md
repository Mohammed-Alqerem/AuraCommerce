# Aura Commerce Project Memory

This is the durable technical handoff for future work on Aura Commerce. Read it at the start of a development session and update it in the same commit as every completed feature. User-visible capabilities belong in `FEATURES.md`; this file records engineering context, invariants, decisions, verification, and the next safe step.

## Feature delivery workflow

1. Start from an up-to-date `main` and create a `feature/<short-name>` branch.
2. Implement one coherent feature and its tests without mixing unrelated cleanup.
3. Update this file and `FEATURES.md` in the same feature commit.
4. Run the repository's Release build and tests. Generate and review SQL when the EF model changes.
5. Commit and push the feature branch; never push feature work directly to `main`.
6. Confirm GitHub Actions for the exact pushed commit. Merge only after the developer reviews that revision and explicitly approves the merge.

## Current handoff

- Date: 2026-08-25
- Working branch: `feature/store-expansion-memory`
- Base: `main` at `93d70a8` (`CI/CD`)
- Current increment: store expansion plus durable project records
- State: implementation and local verification complete; check the feature branch's remote CI and developer-review status before merging
- Recovery note: the expansion files found uncommitted on 2026-08-25 were confirmed as intentional work and are preserved in this feature branch

## Architecture snapshot

- ASP.NET Core MVC and Razor Views targeting .NET 9
- Entity Framework Core 9 with SQL Server in the application and in-memory SQLite in tests
- Custom session authentication using centralized session keys and persisted `Admin`/`Customer` roles
- Controllers own HTTP concerns; `CheckoutService` owns transactional checkout orchestration
- EF configuration is centralized under `OnlineStore/Data/Configurations`; development seed data is under `OnlineStore/Data/Seed`
- GitHub Actions restores tools and packages, then performs a Release build and test run on pushes and pull requests

## Invariants to preserve

- Never determine authorization from a specific user ID.
- Never store a plaintext password, and never log, document, send, or render a plaintext password or stored password hash.
- Accept a return URL only when `Url.IsLocalUrl` confirms it is local.
- Scope carts, wishlists, notifications, tickets, and order queries to the signed-in customer.
- Calculate prices and totals from database products, not posted form values.
- Checkout must remain serializable, stock-aware, and atomic; stock cannot become negative.
- Preserve product name, unit price, shipping, delivery, and total snapshots for historical orders.
- Archive products and deactivate categories instead of destroying referenced history.
- A customer may have only one normalized email, one cart, one cart row per product, one review per product, and one wishlist row per product.
- New persisted timestamps use UTC.
- All unsafe MVC actions require anti-forgery validation.

## Database migration chain

Apply migrations in their committed order; do not rename a migration after it has been applied to a shared database.

1. `20260816101954_initialSetUpDataBase`
2. `20260817072658_UpdateProductImages`
3. `20260821162228_HardenAuthenticationAndDataIntegrity`
4. `20260824214838_StoreExpansion`
5. `20260824215133_AddStoreNotifications`
6. `20260824223654_BackfillLegacyOrderSnapshots`

Back up databases containing real data and inspect generated SQL before applying schema changes. Production startup migrations remain opt-in through `Database:ApplyMigrationsOnStartup`.

## External provider and policy gates

- `IStoreEmailSender` is intentionally safe and unconfigured. Select and configure a transactional email provider before enabling real password-reset or email-verification delivery.
- Payments remain the existing manual flow. Do not add live payments until a provider and webhook/idempotency behavior are approved.
- Coupons, automatic cancellation, returns, and refunds require owner-approved pricing and eligibility policies before implementation.
- Connection strings and provider credentials belong in environment variables or deployment secrets, never committed configuration.

## Verification record

- 2026-08-25: the CI-equivalent Release build completed with zero warnings and all 25 tests passed.
- 2026-08-25: the Debug build completed with zero warnings.
- 2026-08-25: EF reported no pending model changes relative to the model snapshot.
- 2026-08-25: an idempotent SQL script through `BackfillLegacyOrderSnapshots` generated successfully at `OnlineStore/obj/store-expansion.sql` (ignored build output); the removed empty `test` migration is absent.

## Next-session checklist

1. Read this file and `FEATURES.md`.
2. Inspect `git status --short --branch`, the latest commits, and remote CI before editing.
3. Finish any item explicitly marked in progress before starting another feature.
4. Add the next requested capability to `FEATURES.md` and record its technical decisions here.
