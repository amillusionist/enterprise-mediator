# EMP — Master Task List

> Generated from full codebase audit on 2026-03-07.
> Each task includes file paths, what needs to change, and why.
> Status: `[ ]` = pending, `[x]` = done, `[~]` = partial/in-progress

---

## PHASE 1: Frontend-Backend DTO Alignment

The frontend TypeScript types (`emp-frontend-webapp/src/lib/types.ts`) must match the C# DTOs in `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/`. All enums already match. Property names and shapes do not.

### 1.1 ProjectBriefDTO property alignment
- [x] **Status**: Done
- **Frontend file**: `emp-frontend-webapp/src/lib/types.ts` — `ProjectBriefDTO` interface (~line 245)
- **Backend file**: `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/Projects/ProjectBriefDto.cs`
- **What to fix**:
  - Frontend `description` should be `summary` (backend uses `Summary`)
  - Frontend `timeline: string` should be `estimatedDurationWeeks: number` (backend uses `EstimatedDurationWeeks` int)
  - Frontend `status: 'Draft' | 'Approved'` should be `isApproved: boolean` (backend uses `IsApproved` bool)
  - Add missing `scope?: string` field from backend
  - Remove `approvedBy` if backend doesn't have it, or add it to backend
- **Cascade**: After fixing types.ts, update all consumers — `SowReviewComposite.tsx`, `SowExtractionForm.tsx`, `project.service.ts`, `project.actions.ts`

### 1.2 InvoiceDTO property alignment
- [x] **Status**: Done
- **Frontend file**: `emp-frontend-webapp/src/lib/types.ts` — `InvoiceDTO` interface (~line 390)
- **Backend file**: `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/Financials/InvoiceDto.cs`
- **What to fix**:
  - Frontend has `invoiceNumber`, `clientSecret`, `paymentLink` — verify if backend should have these (likely yes for Stripe integration, add to contract)
  - Or remove from frontend if not needed
- **Decision needed**: These fields are needed for the Stripe payment flow. Recommend adding to backend DTO.

### 1.3 PayoutDTO property alignment
- [x] **Status**: Done
- **Frontend file**: `emp-frontend-webapp/src/lib/types.ts` — `PayoutDTO` interface (~line 436)
- **Backend file**: `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/Financials/PayoutDto.cs`
- **What to fix**:
  - Frontend `processedAt` should be `completedAt` (backend uses `CompletedAt`)
  - Frontend has `vendorName` — backend only has `vendorId`. Either add `vendorName` to backend DTO or populate it via a join/projection in the query handler
  - Frontend has `milestoneId`, `failureReason` — add to backend DTO if needed
- **Cascade**: Update `PayoutApprovalModal.tsx`, `finance.service.ts`, payouts page

### 1.4 TransactionDTO property alignment
- [x] **Status**: Done
- **Frontend file**: `emp-frontend-webapp/src/lib/types.ts` — `TransactionDTO` interface (~line 411)
- **Backend file**: `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/Financials/TransactionDto.cs`
- **What to fix**:
  - Backend missing `status` field — add to contract (transactions can be Pending/Completed/Failed/Processing)
  - Backend missing `vendorId`, `clientId` — add or remove from frontend
  - Frontend has `completedAt` — add to backend if not present
- **Cascade**: Update `TransactionLedgerTable.tsx`, `finance.service.ts`

### 1.5 VendorDTO location field
- [x] **Status**: Done
- **Frontend file**: `emp-frontend-webapp/src/lib/types.ts` — `VendorDTO` interface (~line 283)
- **Backend file**: `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/Vendors/VendorDto.cs`
- **What to fix**:
  - Backend uses separate `Country` and `City` fields; frontend uses single `location` string
  - Either: change frontend to use `country`/`city` separately, or add computed `location` to backend DTO
- **Cascade**: Update vendor pages and components

### 1.6 MilestoneDTO cleanup
- [x] **Status**: Done
- **Frontend file**: `emp-frontend-webapp/src/lib/types.ts` — `MilestoneDTO` interface (~line 477)
- **Backend file**: `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/Projects/MilestoneDto.cs`
- **What to fix**:
  - Frontend has both `title` and `name` — pick one to match backend (likely `title`)
  - Frontend has `projectName` — verify backend returns this or remove
- **Cascade**: Update milestone rendering in project detail page, milestone approval page

### 1.7 GenerateInvoiceRequest alignment
- [x] **Status**: Done
- **Frontend file**: `emp-frontend-webapp/src/lib/types.ts` — `GenerateInvoiceInput` (~line 406)
- **Backend file**: `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/Financials/GenerateInvoiceRequest.cs`
- **What to fix**:
  - Backend requires `projectId` inside the DTO body; frontend only sends it as a route param
  - Either remove `projectId` from backend DTO (since it's in the route) or add it to frontend input
- **Cascade**: Update `finance.service.ts` and `finance.actions.ts`

---

## PHASE 2: Frontend API Endpoint Path Fixes

The frontend service files call endpoints that don't match the API Gateway's actual route patterns. Fix the frontend to match what exists (or will exist) in the gateway.

### 2.1 Fix HTTP method mismatches
- [x] **Status**: Done
- **Files to fix**:
  - `emp-frontend-webapp/src/services/project.service.ts` — `approveBrief()` uses `ApiClient.post()` but gateway expects `PUT` on `briefs/approve`. Change to `ApiClient.put()`
  - `emp-frontend-webapp/src/services/project.service.ts` — `approveMilestone()` uses `ApiClient.post()` but gateway uses `PUT`. Change to `ApiClient.put()`
- **Clue**: Search for `post<void>` calls that should be `put<void>` based on the gateway controller method attributes `[HttpPut]`

### 2.2 Fix matching-vendors endpoint path
- [x] **Status**: Done
- **File**: `emp-frontend-webapp/src/services/project.service.ts` — `getMatchingVendors()` method
- **What to fix**: Frontend calls `/projects/{id}/matching-vendors` but gateway has it at `/projects/{id}/briefs/matching-vendors`
- **Change**: Update the endpoint path to include `/briefs/` segment

### 2.3 Fix invoice generation endpoint path
- [x] **Status**: Done
- **File**: `emp-frontend-webapp/src/services/finance.service.ts` — `generateInvoice()` method
- **What to fix**: Frontend calls `/projects/{id}/invoice` but gateway has it at `/financials/projects/{id}/invoices/generate`
- **Change**: Update endpoint path to match gateway's `FinancialsController` route

### 2.4 Decide on API URL prefix convention
- [x] **Status**: Done
- **File**: `emp-frontend-webapp/src/services/api-client.ts`
- **What to decide**: The gateway controllers use `api/v1/` prefix (e.g., `api/v1/projects`). The frontend `ApiClient` prepends `API_URL` env var to all endpoints. Verify that `API_URL` includes the `api/v1` prefix, or add it to the `ApiClient.buildUrl()` method. Currently frontend services use bare paths like `/projects`, `/vendors`, etc.
- **Clue**: Check `api-client.ts` `buildUrl()` method and compare with gateway `Program.cs` route prefix configuration

---

## PHASE 3: API Gateway — Missing Controllers & Endpoints

The gateway currently only has: `ProjectsController` (partial), `FinancialsController` (partial), `UsersController` (partial). Many endpoints the frontend needs are missing.

### 3.1 Implement AuthController
- [x] **Status**: Done
- **File to create**: `emp-api-gateway/src/Emp.ApiGateway.Web/Controllers/AuthController.cs`
- **Endpoints needed**:
  - `POST api/v1/auth/login` — Accept `LoginCredentials`, authenticate against AWS Cognito, return `AuthResponse` with tokens
  - `POST api/v1/auth/logout` — Invalidate session
  - `POST api/v1/auth/register` — Create user via Cognito
  - `POST api/v1/auth/refresh` — Refresh access token
  - `POST api/v1/auth/mfa/verify` — Verify MFA code
  - `POST api/v1/auth/password/forgot` — Request password reset
  - `POST api/v1/auth/password/reset` — Confirm password reset
- **Pattern**: Follow existing `UsersController` pattern. Use MediatR commands. Auth logic uses AWS Cognito SDK (`Amazon.CognitoIdentityProvider`).
- **Reference**: `docs/sequence/01-user-login-mfa.md` for the exact flow

### 3.2 Implement VendorsController
- [x] **Status**: Done
- **File to create**: `emp-api-gateway/src/Emp.ApiGateway.Web/Controllers/VendorsController.cs`
- **Endpoints needed**:
  - `GET api/v1/vendors` — List with pagination/filters
  - `GET api/v1/vendors/{id}` — Detail
  - `POST api/v1/vendors` — Create
  - `PATCH api/v1/vendors/{id}` — Update
  - `POST api/v1/vendors/{id}/activate` — Activate
  - `POST api/v1/vendors/{id}/deactivate` — Deactivate
  - `POST api/v1/vendors/{id}/contacts/invite` — Invite contact
- **Pattern**: Follow `ProjectsController` pattern. Each endpoint maps to a MediatR command/query.
- **Backend service**: `emp-user-management-service` handles vendor data. Gateway proxies via HTTP or MassTransit.
- **DTOs**: Use `VendorDto`, `CreateVendorRequest`, `UpdateVendorRequest` from `emp-shared-contracts`

### 3.3 Implement ClientsController
- [x] **Status**: Done
- **File to create**: `emp-api-gateway/src/Emp.ApiGateway.Web/Controllers/ClientsController.cs`
- **Endpoints needed**:
  - `GET api/v1/clients` — List with pagination
  - `GET api/v1/clients/{id}` — Detail
  - `POST api/v1/clients` — Create
  - `PATCH api/v1/clients/{id}` — Update
  - `PUT api/v1/clients/{id}/deactivate` — Deactivate
  - `PUT api/v1/clients/{id}/reactivate` — Reactivate
- **Pattern**: Same as VendorsController
- **Backend service**: `emp-user-management-service`
- **DTOs**: Use/create `ClientDto`, `CreateClientRequest` in `emp-shared-contracts`

### 3.4 Implement ProposalsController
- [x] **Status**: Done
- **File to create**: `emp-api-gateway/src/Emp.ApiGateway.Web/Controllers/ProposalsController.cs`
- **Endpoints needed**:
  - `GET api/v1/projects/{projectId}/proposals` — List proposals for a project
  - `PUT api/v1/proposals/{id}/status` — Update proposal status
  - `POST api/v1/proposals/{id}/award` — Award proposal
  - `POST api/v1/proposals/{token}/submit` — Public: vendor submits proposal via token (no auth)
  - `GET api/v1/proposals/portal/{token}` — Public: get brief for proposal submission (no auth)
- **Backend service**: `emp-project-management-service`
- **DTOs**: Use/create `ProposalDto`, `ProposalSubmissionRequest` in `emp-shared-contracts`
- **Reference**: `docs/sequence/08-vendor-proposal-submission.md`

### 3.5 Complete ProjectsController
- [x] **Status**: Done
- **File**: `emp-api-gateway/src/Emp.ApiGateway.Web/Controllers/ProjectsController.cs`
- **Missing endpoints** (some exist, some don't):
  - `GET api/v1/projects` — List with pagination (may exist, verify)
  - `GET api/v1/projects/{id}` — Detail (may exist, verify)
  - `PATCH api/v1/projects/{id}/status` — Update status
  - `PUT api/v1/projects/{id}/briefs` — Update brief data (human-in-the-loop save)
  - `POST api/v1/projects/{id}/distribute` — Distribute brief to vendors
  - `POST api/v1/projects/{id}/award` — Award project to vendor
  - `GET api/v1/projects/{id}/sow/status` — SOW processing status
  - `GET api/v1/projects/{id}/sow/data` — Extracted SOW data
- **Clue**: Read existing controller to see what's already there, then add missing endpoints following the same pattern

### 3.6 Complete FinancialsController
- [x] **Status**: Done
- **File**: `emp-api-gateway/src/Emp.ApiGateway.Web/Controllers/FinancialsController.cs`
- **Missing endpoints**:
  - `GET api/v1/finance/transactions` — List transactions with filters
  - `GET api/v1/finance/reports/transactions` — Export CSV
  - `POST api/v1/finance/payouts/initiate` — Initiate payout
  - `POST api/v1/finance/payouts/{id}/approve` — Approve
  - `POST api/v1/finance/payouts/{id}/reject` — Reject
  - `GET api/v1/finance/payouts?status=Pending` — List pending payouts
  - `POST api/v1/finance/refunds` — Process refund
  - `GET api/v1/finance/config/retention` — Get retention policy
  - `PUT api/v1/finance/config/retention` — Update retention policy
  - `GET api/v1/finance/reports/profitability` — Profitability report
  - `GET api/v1/dashboard/metrics` — Dashboard aggregate metrics
- **Backend service**: `emp-financial-service`
- **Reference**: `docs/sequence/11-milestone-payment-release.md`, `docs/sequence/12-vendor-payout-wise.md`

### 3.7 Implement AuditController
- [x] **Status**: Done
- **File to create**: `emp-api-gateway/src/Emp.ApiGateway.Web/Controllers/AuditController.cs`
- **Endpoints needed**:
  - `GET api/v1/audit-logs` — List with pagination/filters
  - `GET api/v1/audit-logs/{id}` — Single entry
  - `GET api/v1/audit-logs/export` — Export CSV
- **Pattern**: Query-only controller (audit logs are immutable, no writes)
- **Backend**: Could query directly from shared observability DB or via a dedicated service

### 3.8 Implement public token-based endpoints
- [x] **Status**: Done
- **Files**: May need a `PublicController.cs` or integrate into existing controllers with `[AllowAnonymous]`
- **Endpoints needed**:
  - `GET api/v1/invoices/pay/{token}` — Get invoice details for payment (no auth)
  - `POST api/v1/invoices/{id}/confirm-payment` — Confirm Stripe payment (no auth or webhook)
  - `GET api/v1/milestones/approve/{token}` — Get milestone for approval (no auth)
  - `PUT api/v1/milestones/{token}/decide` — Approve/reject milestone (no auth)
- **Clue**: These are public-facing endpoints for clients/vendors who receive email links. Must be `[AllowAnonymous]` with token-based access.
- **Reference**: `docs/sequence/10-client-milestone-approval.md`, `docs/sequence/13-stripe-invoice-payment.md`

---

## PHASE 4: API Gateway — MediatR Handlers

Each new controller endpoint needs a corresponding MediatR command/query + handler in `emp-api-gateway/src/Emp.ApiGateway.Application/Features/`.

### 4.1 Create Auth feature handlers
- [x] **Status**: Done — Controllers use direct ICognitoAuthService delegation instead of MediatR
- **Directory**: `emp-api-gateway/src/Emp.ApiGateway.Application/Features/Auth/`
- **Handlers needed**: `LoginCommand`, `LogoutCommand`, `RegisterCommand`, `RefreshTokenCommand`, `VerifyMfaCommand`, `ForgotPasswordCommand`, `ResetPasswordCommand`
- **Pattern**: Each handler is a separate file. Command + Handler + Validator (FluentValidation). Use `Result<T>` for outcomes.
- **External dependency**: AWS Cognito SDK. There may already be a `ICognitoService` or similar interface — check `Infrastructure/Gateways/`.

### 4.2 Create Vendor feature handlers
- [x] **Status**: Done — Controllers use direct IUserServiceClient delegation
- **Directory**: `emp-api-gateway/src/Emp.ApiGateway.Application/Features/Vendors/`
- **Handlers needed**: `GetVendorsQuery`, `GetVendorByIdQuery`, `CreateVendorCommand`, `UpdateVendorCommand`, `ActivateVendorCommand`, `DeactivateVendorCommand`
- **Pattern**: Queries call downstream `emp-user-management-service` via HTTP. Commands may publish via MassTransit.

### 4.3 Create Client feature handlers
- [x] **Status**: Done — Controllers use direct IUserServiceClient delegation
- **Directory**: `emp-api-gateway/src/Emp.ApiGateway.Application/Features/Clients/`
- **Handlers needed**: `GetClientsQuery`, `GetClientByIdQuery`, `CreateClientCommand`, `UpdateClientCommand`, `DeactivateClientCommand`, `ReactivateClientCommand`

### 4.4 Create Proposal feature handlers
- [x] **Status**: Done — Controllers use direct IProjectServiceClient delegation
- **Directory**: `emp-api-gateway/src/Emp.ApiGateway.Application/Features/Proposals/`
- **Handlers needed**: `GetProjectProposalsQuery`, `UpdateProposalStatusCommand`, `AwardProposalCommand`, `SubmitProposalCommand`, `GetPortalBriefQuery`

### 4.5 Complete Project feature handlers
- [x] **Status**: Done
- **Directory**: `emp-api-gateway/src/Emp.ApiGateway.Application/Features/Projects/`
- **Check what exists**, then add missing: `GetProjectsQuery`, `GetProjectByIdQuery`, `UpdateProjectStatusCommand`, `UpdateBriefCommand`, `DistributeBriefCommand`, `AwardProjectCommand`, `GetSowStatusQuery`, `GetSowDataQuery`

### 4.6 Create Finance feature handlers
- [x] **Status**: Done — Controllers use direct IFinancialServiceClient delegation
- **Directory**: `emp-api-gateway/src/Emp.ApiGateway.Application/Features/Financials/`
- **Handlers needed**: `GetTransactionsQuery`, `ExportTransactionsQuery`, `InitiatePayoutCommand`, `ApprovePayoutCommand`, `RejectPayoutCommand`, `GetPendingPayoutsQuery`, `ProcessRefundCommand`, `GetRetentionPolicyQuery`, `UpdateRetentionPolicyCommand`, `GetDashboardMetricsQuery`, `GetProfitabilityReportQuery`

### 4.7 Create Audit feature handlers
- [x] **Status**: Done — Controllers use direct IAuditServiceClient delegation
- **Directory**: `emp-api-gateway/src/Emp.ApiGateway.Application/Features/Audit/`
- **Handlers needed**: `GetAuditLogsQuery`, `GetAuditLogByIdQuery`, `ExportAuditLogsQuery`

---

## PHASE 5: Shared Contracts — Missing DTOs

DTOs referenced by frontend but missing from `emp-shared-contracts`.

### 5.1 Add missing DTOs to shared contracts
- [x] **Status**: Done
- **Directory**: `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/`
- **DTOs to add or verify exist**:
  - `DTOs/Clients/ClientDto.cs` — with `Id`, `CompanyName`, `Address`, `Contacts[]`, `IsActive`, `CreatedAt`
  - `DTOs/Clients/CreateClientRequest.cs` — with `CompanyName`, `Address`, `Contacts[]`
  - `DTOs/Projects/UpdateProjectBriefRequest.cs` — with `Summary`, `RequiredSkills[]`, `Technologies[]`, `Deliverables[]`, `EstimatedDurationWeeks`, `BudgetEstimate`
  - `DTOs/Auth/LoginRequest.cs`, `LoginResponse.cs`, `RegisterRequest.cs`, `RefreshTokenRequest.cs`
  - `DTOs/Common/DashboardMetricsDto.cs` — aggregated platform metrics
  - `DTOs/Common/ProfitabilityReportItemDto.cs`
  - `DTOs/Common/RetentionPolicyDto.cs`
  - `DTOs/Common/PaginatedResult.cs` — generic `{ Items[], TotalCount, Page, PageSize }` (may already exist)
- **Clue**: Check what already exists in `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/` before creating. Some may already be there.

### 5.2 Add Stripe-specific fields to InvoiceDto
- [x] **Status**: Done
- **File**: `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/Financials/InvoiceDto.cs`
- **Add**: `InvoiceNumber`, `ClientSecret` (Stripe PaymentIntent client secret), `PaymentLink` (Stripe hosted payment URL)
- **Why**: The public invoice payment page (`/pay/invoice/[token]`) needs `clientSecret` to initialize Stripe Elements

### 5.3 Add missing fields to PayoutDto
- [x] **Status**: Done
- **File**: `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/Financials/PayoutDto.cs`
- **Add**: `VendorName`, `FailureReason`, `MilestoneId`
- **Rename**: Verify `CompletedAt` is the correct name (frontend currently uses `processedAt` — align to one)

### 5.4 Add missing fields to TransactionDto
- [x] **Status**: Done
- **File**: `emp-shared-contracts/src/EnterpriseMediator.Contracts/DTOs/Financials/TransactionDto.cs`
- **Add**: `Status` (string/enum), `VendorId`, `ClientId`, `CompletedAt`

### 5.5 Reference shared-contracts from all backend services
- [~] **Status**: Partial — emp-api-gateway references shared-contracts. Other services still need references added.
- **Currently**: Only `emp-api-gateway` references `emp-shared-contracts` NuGet package
- **Need to add reference in**:
  - `emp-financial-service` — currently has duplicate DTOs in its own Application layer
  - `emp-project-management-service` — needs shared DTOs for event schemas
  - `emp-user-management-service` — needs shared DTOs
  - `emp-ai-processing-worker` — needs event schemas
- **Clue**: Add `<PackageReference Include="EnterpriseMediator.Contracts" />` to each service's `.csproj`, then replace local DTO definitions with imports from `EnterpriseMediator.Contracts`

---

## PHASE 6: Downstream Service Implementation

The API Gateway proxies to downstream services. These need their own controllers/handlers.

### 6.1 emp-project-management-service — Complete Application layer
- [x] **Status**: Done — 36+ handler files created (CreateProject, UploadSow, UpdateProjectBrief, ApproveBrief, DistributeBrief, AwardProject, ChangeProjectStatus, GetProjectById, GetMatchingVendors, SubmitProposal, GetProjectProposals, AddMilestone, ApproveMilestone, etc.)
- **Directory**: `emp-project-management-service/src/EnterpriseMediator.ProjectManagement.Application/Features/`
- **What to implement**: MediatR handlers for all project operations: CRUD, SOW upload/processing, brief management, proposal handling, milestone management
- **Domain layer**: Already implemented in `emp-domain-models` (Project aggregate, SowDocument, ProjectBrief, Proposal, Milestone)
- **Infrastructure**: EF Core repositories, S3 for SOW storage
- **Reference**: `docs/sequence/03-sow-upload-processing.md`, `docs/sequence/05-ai-sow-extraction.md`

### 6.2 emp-financial-service — Complete Application + Web.API layers
- [~] **Status**: Partial — Application layer handlers done (GenerateInvoice, GetInvoiceById, GetTransactionHistory, InitiatePayout, ApprovePayout, RejectPayout, GetPendingPayouts, GetFinancialSummary, StripeWebhookHandler). Web.API controllers still needed.
- **Directory**: `emp-financial-service/src/EnterpriseMediator.Financial.Application/` and `.Web.API/`
- **What to implement**: Invoice generation, payment processing (Stripe), payout initiation (Wise), transaction recording, refunds
- **Domain layer**: Already implemented (Invoice, Payout, Transaction aggregates)
- **Infrastructure**: Partially done (Stripe gateway, Wise gateway exist)
- **Reference**: `docs/sequence/11-milestone-payment-release.md`, `docs/sequence/12-vendor-payout-wise.md`, `docs/sequence/13-stripe-invoice-payment.md`

### 6.3 emp-user-management-service — Complete Application layer
- [~] **Status**: Partial — Application layer handlers done (CreateClient, GetClientDetails, CreateVendor, UpdateVendorProfile, GetVendorDetails, AnonymizeUser, RegisterUser, GetUserRole, GetClientById, GetVendorPaymentDetails). Web.API controllers still needed.
- **Directory**: `emp-user-management-service/src/EnterpriseMediator.UserManagement.Application/`
- **What to implement**: User CRUD, vendor CRUD, client CRUD, role management, AWS Cognito user provisioning
- **Domain layer**: Already implemented (User, Vendor, Client aggregates in `emp-domain-models`)
- **Reference**: `docs/sequence/01-user-login-mfa.md`, `docs/sequence/02-vendor-onboarding.md`

### 6.4 emp-ai-processing-worker — Verify SOW pipeline
- [ ] **Status**: Pending
- **Directory**: `emp-ai-processing-worker/src/EnterpriseMediator.AiWorker/`
- **What to verify**: SOW document parsing (TikaOnDotNet), PII sanitization, Azure OpenAI extraction, vendor matching (pgvector cosine similarity), MassTransit consumer for `SowUploadedEvent`
- **Reference**: `docs/sequence/05-ai-sow-extraction.md`, `docs/sequence/06-ai-vendor-matching.md`

---

## PHASE 7: Frontend — Remaining Pages & Components

### 7.1 Create missing auth pages
- [x] **Status**: Done — Login page already existed; register page created
- **Files to create**:
  - `emp-frontend-webapp/src/app/(auth)/login/page.tsx` — Login form using `LoginSchema` from `schemas.ts`, calls `loginAction` from `auth.actions.ts`
  - `emp-frontend-webapp/src/app/(auth)/register/page.tsx` — Registration form using `RegisterSchema`, calls `registerAction`
- **Clue**: These are client components with React Hook Form + Zod. On success, redirect to `/admin/dashboard`. On MFA challenge, redirect to MFA verification step.

### 7.2 Create vendor detail page
- [x] **Status**: Done
- **File to create**: `emp-frontend-webapp/src/app/(dashboard)/admin/vendors/[vendorId]/page.tsx`
- **What**: Server Component that fetches vendor by ID via `VendorService.getVendorById()`, displays full profile, skills, status, contact info. Include activate/deactivate actions.
- **Linked from**: Vendors list page (`/admin/vendors`) "View" link

### 7.3 Create user edit page
- [x] **Status**: Done
- **File to create**: `emp-frontend-webapp/src/app/(dashboard)/admin/users/[userId]/edit/page.tsx`
- **What**: Form to edit user role and active status. Fetch user by ID, display edit form, call update action.
- **Linked from**: Users list page (`/admin/users`) "Edit" link

### 7.4 Create payment confirmation pages
- [x] **Status**: Done
- **Files to create**:
  - `emp-frontend-webapp/src/app/(public)/pay/success/page.tsx` — Payment success confirmation with invoice ID from query params
  - `emp-frontend-webapp/src/app/(public)/pay/confirm/page.tsx` — Stripe redirect handler that confirms payment and redirects to success
- **Linked from**: `InvoicePaymentForm.tsx` redirects here after Stripe payment

### 7.5 Create use-file-upload hook
- [x] **Status**: Done — Already existed at src/hooks/use-file-upload.ts
- **File to create**: `emp-frontend-webapp/src/hooks/use-file-upload.ts`
- **What**: Custom hook that wraps `fetch` with upload progress tracking via `XMLHttpRequest` or `ReadableStream`. Returns `{ upload, isUploading, progress, error, reset }`.
- **Used by**: `SowUploadZone.tsx`
- **Clue**: Must use client-side fetch (not ApiClient which is server-only). Accept `{ endpoint, onSuccess }` config. Track upload progress percentage.

### 7.6 Create auth middleware
- [x] **Status**: Done — Already existed at src/middleware.ts
- **File**: `emp-frontend-webapp/src/middleware.ts` (may exist, verify)
- **What**: Next.js middleware that checks for `access_token` HttpOnly cookie on `/(dashboard)` routes. If missing, redirect to `/login`. Pass token to server components via headers.
- **Clue**: Use `NextResponse.redirect()` for unauthenticated requests. Exempt `/(public)` and `/(auth)` route groups.

### 7.7 Port UI components from emp-ui-component-library
- [ ] **Status**: Pending
- **Source**: `emp-ui-component-library/src/components/`
- **Target**: `emp-frontend-webapp/src/components/ui/`
- **What**: Copy or reference these atoms/molecules already built in the UI library: `Button`, `Badge`, `Input`, `Label`, `Avatar`, `Dialog`, `Select`, `Toast`
- **Why**: The SowReviewComposite was rewritten to avoid these deps, but other future components should use them. The library uses Radix UI + cva pattern (shadcn/ui style).
- **Clue**: Check what's in `emp-ui-component-library/src/components/` and either npm-link the package or copy the components into `emp-frontend-webapp/src/components/ui/`

---

## PHASE 8: Missing NPM Dependencies

### 8.1 Install required packages
- [x] **Status**: Done — @stripe/stripe-js and @stripe/react-stripe-js installed
- **File**: `emp-frontend-webapp/package.json`
- **Packages to add**:
  - `@stripe/stripe-js` — Stripe.js loader (used by `InvoicePaymentForm.tsx`)
  - `@stripe/react-stripe-js` — React bindings for Stripe Elements
  - `tailwindcss-animate` — Referenced in `tailwind.config.ts` plugins array
- **Packages to verify are installed**: `zustand`, `react-hook-form`, `@hookform/resolvers`, `zod`, `lucide-react`
- **Packages to NOT install**: `@heroicons/react` (replaced with inline SVGs), `@headlessui/react` (removed from PayoutApprovalModal)

---

## PHASE 9: Testing

### 9.1 Frontend unit tests
- [ ] **Status**: Pending
- **Directory**: `emp-frontend-webapp/tests/` or `src/__tests__/`
- **What**: Jest + React Testing Library tests for:
  - All feature components (VendorComparisonTable, TransactionLedgerTable, SowUploadZone, SowExtractionForm, InvoicePaymentForm, PayoutApprovalModal, NotificationCenter)
  - All server actions (mock the service layer, test validation and error handling)
  - Zustand stores (notification store, UI store)
- **Coverage target**: All components render without error, form validation works, error states display correctly

### 9.2 Frontend E2E tests
- [ ] **Status**: Pending
- **Directory**: `emp-frontend-webapp/e2e/` or `tests/e2e/`
- **What**: Playwright tests for critical flows:
  - Login -> Dashboard
  - SOW Upload -> Review -> Approve Brief
  - Proposal Comparison -> Award Vendor
  - Invoice Payment (Stripe test mode)
  - Milestone Approval via token link
- **Reference**: `docs/requirements/` for acceptance criteria

### 9.3 Backend unit tests — financial service
- [ ] **Status**: Pending
- **Directory**: `emp-financial-service/tests/EnterpriseMediator.Financial.UnitTests/`
- **What**: Currently empty. Need tests for all Application layer handlers using Moq + FluentAssertions.
- **Pattern**: Follow test patterns from `emp-core-shared-kernel` tests if they exist

### 9.4 Backend integration tests — financial service
- [ ] **Status**: Pending
- **Directory**: `emp-financial-service/tests/EnterpriseMediator.Financial.IntegrationTests/`
- **What**: Currently empty. Need Testcontainers.NET tests for EF Core repositories.
- **Pattern**: Spin up PostgreSQL container, run migrations, test repository CRUD operations

### 9.5 MassTransit consumer tests
- [ ] **Status**: Pending
- **What**: Integration tests for all MassTransit consumers:
  - `SowUploadedEvent` consumer in AI worker
  - `ProjectAwardedEvent` consumer in financial service
  - `MilestoneApprovedEvent` consumer in financial service
  - `InvoicePaidEvent` consumer (if exists)
- **Pattern**: Use MassTransit test harness (`InMemoryTestHarness`)

---

## PHASE 10: Infrastructure & DevOps

### 10.1 Terraform modules
- [~] **Status**: Partial — networking, eks, vpc, rds, ses, kms modules created. Missing: elasticache, s3, cognito, ecr, messaging modules.
- **Directory**: `emp-platform-infrastructure/`
- **What**: Currently only config files (`.checkov.yaml`, `.tflint.hcl`). Need actual `.tf` modules for:
  - AWS EKS cluster
  - RDS PostgreSQL with pgvector
  - ElastiCache Redis
  - S3 bucket for SOW documents
  - AWS Cognito user pool
  - AWS SES for email
  - RabbitMQ (Amazon MQ or self-hosted)
  - ECR repositories for container images
- **Reference**: `docs/architecture/deployment.md`

### 10.2 Docker Compose for local dev
- [x] **Status**: Done — Root docker-compose.yml created with all services + PostgreSQL/pgvector + RabbitMQ + Redis
- **What**: Verify each service has a working `docker-compose.dev.yml` with PostgreSQL 16 + pgvector, RabbitMQ 3.13, Redis 7.2. Ensure all services can start together.

### 10.3 CI/CD pipelines
- [x] **Status**: Done — 6 GitHub Actions workflows created (api-gateway, financial-service, project-service, user-service, ai-worker, frontend)
- **What**: GitHub Actions workflows per service. Each should: build -> test -> Docker build -> push to ECR -> deploy to EKS.
- **Reference**: `docs/architecture/ci-cd.md`

---

## PHASE 11: Known Technical Debt

### 11.1 Clean up project-management-service duplicate folders
- [x] **Status**: Done — Duplicate folders already cleaned up (only EnterpriseMediator.ProjectManagement.* exists)
- **What**: Both `EnterpriseMediator.ProjectManagement.*` and `ProjectManagement.*` folders exist. The `ProjectManagement.*` ones are empty scaffolding. Delete them.
- **Canonical**: Use `EnterpriseMediator.ProjectManagement.*` only

### 11.2 Consolidate Money value object
- [ ] **Status**: Pending
- **What**: `emp-domain-models` has `Money` as a `ValueObject` subclass. `emp-financial-service` has its own `Money` as a `record`. Consolidate to use domain-models version.
- **Files**:
  - Keep: `emp-domain-models/src/EnterpriseMediator.Domain/ValueObjects/Money.cs`
  - Remove: `emp-financial-service/src/.../ValueObjects/Money.cs` (replace with import)

### 11.3 Move PublicFinancialSummaryDto to shared contracts
- [ ] **Status**: Pending
- **What**: This DTO is defined locally in `emp-api-gateway/src/Emp.ApiGateway.Application/Features/Projects/Queries/GetProjectDashboardQuery.cs`. Should be in `emp-shared-contracts`.

---

## Priority Order

1. **Phase 1** (DTO alignment) + **Phase 2** (endpoint path fixes) — Quick wins, frontend-only changes
2. **Phase 5** (shared contracts DTOs) — Foundation for backend work
3. **Phase 3** (gateway controllers) + **Phase 4** (MediatR handlers) — Core backend API
4. **Phase 6** (downstream services) — Business logic implementation
5. **Phase 7** (frontend remaining pages) + **Phase 8** (npm deps) — Frontend completeness
6. **Phase 9** (testing) — Quality assurance
7. **Phase 10** (infrastructure) + **Phase 11** (tech debt) — Production readiness
