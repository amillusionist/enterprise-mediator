# emp-frontend-webapp Progress Tracker

## Completed

### Foundation Layer
- [x] `src/lib/types.ts` — Single source of truth for all TypeScript types, aligned with emp-shared-contracts enums
- [x] `src/lib/constants.ts` — AUTH_COOKIE_NAME, REFRESH_COOKIE_NAME, COOKIE_OPTIONS, ACCEPTED_FILE_TYPES (.pdf, .docx, .doc), MAX_FILE_SIZE
- [x] `src/lib/schemas.ts` — Complete Zod validation schemas (Login, Register, SOW, Invoice, Vendor, ProjectBrief, UserInvite, RetentionPolicy, PasswordReset)
- [x] `src/app/globals.css` — Tailwind CSS with shadcn/ui CSS variable theme

### API & Services Layer
- [x] `src/services/api-client.ts` — Core BFF connector with get, post, put, patch, delete, postForm, getBlob
- [x] `src/services/auth.service.ts` — Static class: login, logout, refreshToken, getCurrentUser, getAllUsers, inviteUser, register, verifyMfa
- [x] `src/services/project.service.ts` — Static class: full project lifecycle, SOW, milestones, vendor matching, award
- [x] `src/services/vendor.service.ts` — Static class: CRUD, status management
- [x] `src/services/client.service.ts` — Static class: CRUD
- [x] `src/services/finance.service.ts` — Static class: invoices, transactions, payouts, dashboard metrics, profitability, retention
- [x] `src/services/audit.service.ts` — Static class: audit logs, CSV export
- [x] `src/services/proposal.service.ts` — Static class: proposals, award, portal brief, submission

### Server Actions
- [x] `src/actions/auth.actions.ts` — login, logout, register, verifyMfa, requestPasswordReset, inviteUser
- [x] `src/actions/project.actions.ts` — createProject, uploadSow, updateStatus, approveBrief, saveSowData, distributeBrief, awardProject, approveMilestone, rejectMilestone
- [x] `src/actions/finance.actions.ts` — generateInvoice, processInvoicePayment, initiatePayout, approvePayout, rejectPayout, processRefund, updateRetentionPolicy
- [x] `src/actions/vendor.actions.ts` — createVendor, updateVendor, activateVendor, deactivateVendor, inviteVendorContact
- [x] `src/actions/client.actions.ts` — createClient
- [x] `src/actions/proposal.actions.ts` — submitProposalAction (public, no auth — vendor proposal submission via portal)

### Stores
- [x] `src/store/use-notification-store.ts` — Separated toast notifications (local UI) from server notifications; fetchNotifications, markAsRead, markAllAsRead, unreadCount, toasts, dismissToast
- [x] `src/store/use-ui-store.ts` — modal state, sidebar, showToast

### Hooks
- [x] `src/hooks/use-notification-polling.ts` — Real-time notification polling

### Middleware
- [x] `src/middleware.ts` — Edge middleware for route protection via HttpOnly `access_token` cookie; correctly routes public paths (`/pay/*`, `/approve/*`, `/portal/*`, `/login`, `/register`)

### Pages — Dashboard (Admin)
- [x] `src/app/(dashboard)/admin/layout.tsx` — Admin layout with nav, auth guard, NotificationCenter
- [x] `src/app/(dashboard)/admin/dashboard/page.tsx` — Main dashboard with stat cards
- [x] `src/app/(dashboard)/admin/projects/page.tsx` — Projects list with status badges
- [x] `src/app/(dashboard)/admin/projects/new/page.tsx` — Create project form
- [x] `src/app/(dashboard)/admin/projects/[projectId]/page.tsx` — Project detail with milestones
- [x] `src/app/(dashboard)/admin/clients/page.tsx` — Clients list
- [x] `src/app/(dashboard)/admin/clients/new/page.tsx` — Create client form
- [x] `src/app/(dashboard)/admin/vendors/page.tsx` — Vendors list with skills
- [x] `src/app/(dashboard)/admin/vendors/new/page.tsx` — Create vendor form
- [x] `src/app/(dashboard)/admin/proposals/[projectId]/page.tsx` — Proposal comparison matrix
- [x] `src/app/(dashboard)/admin/finance/transactions/page.tsx` — Transaction ledger with client-side CSV export
- [x] `src/app/(dashboard)/admin/finance/payouts/page.tsx` — Payouts management with PayoutApprovalModal integration
- [x] `src/app/(dashboard)/admin/audit-trail/page.tsx` — Audit trail with client-side CSV export button
- [x] `src/app/(dashboard)/admin/users/page.tsx` — User management
- [x] `src/app/(dashboard)/admin/users/invite/page.tsx` — User invite form
- [x] `src/app/(dashboard)/admin/settings/retention/page.tsx` — Retention policy settings
- [x] `src/app/(dashboard)/admin/notifications/page.tsx` — Full notifications page with filter (all/unread), mark read, mark all read

### Pages — Public (no auth required)
- [x] `src/app/(public)/approve/milestone/[token]/page.tsx` — Milestone approval/rejection
- [x] `src/app/(public)/pay/invoice/[token]/page.tsx` — Stripe invoice payment form
- [x] `src/app/(public)/pay/confirm/page.tsx` — Payment confirmation redirect handler (uses server action + Suspense)
- [x] `src/app/(public)/portal/brief/[token]/page.tsx` — Vendor portal brief view with inline proposal submission form and decline action

### Feature Components
- [x] `src/components/features/sow/SowUploadZone.tsx` — Drag-and-drop SOW upload
- [x] `src/components/features/sow/SowExtractionForm.tsx` — Human-in-the-loop SOW editing
- [x] `src/components/features/sow/SowReviewComposite.tsx` — Full SOW review orchestration
- [x] `src/components/features/sow/SanitizedSowViewer.tsx` — PII-redacted document viewer
- [x] `src/components/features/sow/ComparisonSplitView.tsx` — Split-pane layout
- [x] `src/components/features/proposals/VendorComparisonTable.tsx` — Proposal comparison matrix with award action
- [x] `src/components/features/proposals/ProposalSubmissionForm.tsx` — Inline proposal submission form (public portal)
- [x] `src/components/features/finance/TransactionLedgerTable.tsx` — Client-side filterable/paginated ledger with blob CSV export
- [x] `src/components/features/finance/PayoutApprovalModal.tsx` — Payout approve/reject dialog
- [x] `src/components/features/public/InvoicePaymentForm.tsx` — Stripe Elements payment form
- [x] `src/components/features/notifications/NotificationCenter.tsx` — Header notification bell
- [x] `src/components/features/projects/CreateProjectForm.tsx` — New project form
- [x] `src/components/features/clients/CreateClientForm.tsx` — New client form
- [x] `src/components/features/vendors/CreateVendorForm.tsx` — New vendor form

### Environment
- [x] `.env.example` — Includes NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY, API_URL, auth cookie name

---

## Bugs Fixed (This Session)

1. **Middleware blocking public routes** — `pathname.startsWith('/public')` didn't match actual resolved paths (`/pay/*`, `/approve/*`, `/portal/*`). Fixed to match real URL paths.
2. **PayoutApprovalModal broken imports** — Referenced `approvePayout`/`rejectPayout` (non-existent); fixed to `approvePayoutAction`/`rejectPayoutAction`. Also fixed `.error` to `.message`.
3. **Notification store type mismatch** — `addNotification({ type: 'success' })` didn't match `ServerNotification.type` union. Separated toast notifications from server notifications.
4. **Missing .doc MIME type** — CLAUDE.md specifies `.pdf, .docx, .doc` but only PDF/DOCX were accepted. Added `application/msword`.
5. **Missing npm dependencies** — `@stripe/stripe-js`, `@stripe/react-stripe-js` (InvoicePaymentForm), `recharts` (ProfitabilityChart) were not in package.json.
6. **PaymentConfirmPage raw fetch** — Used relative `fetch('/api/v1/...')` that can't reach backend; missing Suspense for `useSearchParams()`. Rewrote to use server action with Suspense boundary.
7. **CSV export non-functional** — Server actions can't return Blobs. Both audit trail and transaction exports used broken server action patterns. Created client-side blob download components.
8. **Payouts page "Review" non-functional** — Was a plain `<span>`. Extracted to `PayoutsTable` client component with `PayoutApprovalModal` integration.
9. **Portal brief proposal submission broken** — "Submit Proposal" linked to non-existent `/portal/proposal/${token}`. Replaced with inline `ProposalSubmissionForm` + `BriefActions` client component with decline handling.
10. **Notifications page was placeholder** — Implemented full `NotificationsList` client component with all/unread filter, mark read, mark all read.
11. **Dead code removed** — Deleted unused `vendor-actions.tsx`, `src/types/audit.d.ts`, `src/types/project.d.ts` (re-export files imported nowhere).

---

## Remaining / Known Gaps

### Pages Not Yet Created
- [ ] `src/app/(dashboard)/admin/vendors/[vendorId]/page.tsx` — Vendor detail page (linked from vendors list "View")
- [ ] `src/app/(dashboard)/admin/users/[userId]/edit/page.tsx` — User edit page (linked from users list "Edit")
- [ ] `src/app/(public)/pay/success/page.tsx` — Payment success confirmation page
- [ ] `src/app/(auth)/login/page.tsx` — Login page (redirected to from admin layout)
- [ ] `src/app/(auth)/register/page.tsx` — Registration page

### Components Not Yet Created
- [ ] `src/components/ui/` directory — shadcn/ui component wrappers (Button, Dialog, Input, etc.) from emp-ui-component-library. Currently components use raw HTML elements. Should port from emp-ui-component-library for consistency.

### Hooks Not Yet Created
- [ ] `src/hooks/use-file-upload.ts` — Custom hook for upload with progress tracking

### Dependencies to Verify
- [ ] `tailwindcss-animate` — Referenced in tailwind.config.ts plugins but may not be installed

### Integration Items
- [ ] Verify all API endpoint paths match emp-api-gateway controller routes
- [ ] Connect emp-ui-component-library atoms/molecules to frontend via package reference or copy

### Testing
- [ ] Jest + React Testing Library unit tests for all feature components
- [ ] Playwright E2E tests for critical user journeys:
  - SOW upload -> review -> approve
  - Vendor matching -> proposal comparison -> award
  - Invoice payment flow
  - Milestone approval flow

### Cross-Module Integration (emp-shared-contracts)
- emp-shared-contracts IS fully implemented as a NuGet library (37 C# files)
- Currently only emp-api-gateway references it
- emp-financial-service has duplicate DTOs that should be consolidated to use shared-contracts
- Frontend TypeScript types in `src/lib/types.ts` are aligned with the C# DTOs in emp-shared-contracts
