# Aura Commerce UX Contract

This contract complements [DESIGN.md](DESIGN.md). Domain rules remain in models, constants, database configuration, and tests.

## Canonical UI map

| Capability | Canonical owner | Source of truth | Allowed variants | Verification |
| --- | --- | --- | --- | --- |
| Form | Razor tag helpers + validation partial | This contract | create / edit | MVC tests + browser |
| Select/Listbox | Native `select.form-select` | DESIGN.md + this contract | native | Keyboard + narrow viewport |
| Date | Native `input[type=date]` | This contract | native | Browser |
| Toast | `_Layout.cshtml` `.store-toast` | This contract | success / warning / info / error | Static audit + browser |
| Scrollbar | `wwwroot/css/site.css` | DESIGN.md | standard / stable-gutter exception | Computed style |
| CRUD | MVC controllers + owning list | Domain constraints + this contract | return to list / stay for inline edit | Integration tests |
| Tables | Native semantic table | This contract | paged / bounded | Controller tests |
| Admin navigation | `_AdminNavigation.cshtml` inside `.admin-page-header` | DESIGN.md + this contract | horizontally scrollable on narrow screens | Keyboard + narrow viewport |

## Workflow ledger

| Operation | Pending | Success | Failure/recovery |
| --- | --- | --- | --- |
| Create/edit admin record | Disable duplicate submit | Return to owning list; status message | Preserve values; inline errors |
| Archive/restore | Server-confirmed | Stay on list; named feedback | Explain blocked dependency |
| Wishlist toggle | Server-confirmed | Return to originating local URL | Keep current page usable |
| Checkout | Server-confirmed transaction | Order confirmation and owned details | Preserve delivery form and refreshed cart |
| Password reset | Generic response | Login with confirmation | Invalid/expired link offers retry |
| Export sales report | Preserve the selected query period | Download one styled `.xlsx` workbook | Keep the filtered report visible so export can be retried |

## Dataset state

Catalog and admin filters, sort, and paging belong in query parameters. Filtering resets paging; out-of-range pages clamp. Empty and no-results states offer a clear next action. Tables use native semantics and visible horizontal overflow on narrow screens.

Admin routes that expose the shared navigation place the page title and actions first, then the navigation directly beneath them in the same header. The current route remains marked with `aria-current="page"`; narrow screens preserve every destination through horizontal overflow.

## Feedback and safety

Destructive or financially consequential work is pessimistic and requires an app-owned confirmation before future activation. Reversible archive/hide operations expose Restore. The application never uses browser dialogs. Sensitive tokens are not logged or stored in URLs beyond the short-lived reset-link query required for account recovery.

## Locale and accessibility

Target WCAG 2.2 AA. English and Arabic/RTL share semantic labels, keyboard order, visible focus, error association, and status announcements. Theme and locale changes must not alter workflow meaning. Motion respects `prefers-reduced-motion`.
