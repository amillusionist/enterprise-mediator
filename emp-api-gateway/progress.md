# emp-api-gateway — Progress Tracker

## Status: Phase 3 Complete — Full Feature Implementation

Last updated: 2026-03-06

---

## Build Status

- **Build:** 0 warnings, 0 errors (all projects)
- **Unit Tests:** 67 passing (net10.0)
- **Shared Contracts Tests:** 17 passing (net10.0)
- **Integration Tests:** 18 written (require .NET 8 runtime for WebApplicationFactory — see Known Issues)
- **SDK used:** .NET 10.0.103 (building .NET 8 source projects, .NET 10 test projects)

---

## Phase 1 Completed — Compilation Fixes

1. DI Method Name Mismatches — FIXED
2. Missing AwsCognitoSettings Class — CREATED
3. Missing IMessageBus Interface — CREATED
4. AutoMapper Mapping Profile — REWRITTEN
5. ProjectServiceClient — REWRITTEN
6. FinancialServiceClient — FIXED
7. Missing Command Handlers — CREATED
8. ProjectsController — REWRITTEN
9. GlobalExceptionHandler Registration — FIXED
10. FinancialsController — FIXED
11. GetProjectDashboardHandler — FIXED
12. Infrastructure csproj Packages — FIXED
13. Web.csproj Cleanup — DONE
14. Application csproj — FIXED
15. Program.cs Cleanup — DONE
16. Unnecessary using directives — FIXED

---

## Phase 2 Completed — Validation, Rate Limiting, Tests

### FluentValidation — WIRED
- `CreateProjectCommandValidator`, `ValidationBehavior<,>` pipeline behavior
- `GlobalExceptionHandler` catches `FluentValidation.ValidationException` → `ValidationProblemDetails` (400)

### Rate Limiting — WIRED
- Auth endpoints: 10 req/min; General: 30 req/s, 300 req/min

### AllowedOrigins CORS — CONFIGURED
- Dev: `http://localhost:3000`; Prod: configure via secrets/env

### MassTransit RabbitMQ — CONFIGURED FROM APPSETTINGS

---

## Phase 3 Completed — emp-shared-contracts + Full Gateway Feature Set

### emp-shared-contracts (implemented from scratch)
- Multi-target library: `net8.0;net10.0` for cross-framework compatibility
- **Enums** (8): ProjectStatus, InvoiceStatus, PayoutStatus, ProposalStatus, MilestoneStatus, TransactionType, VendorStatus, UserRole — all with `JsonStringEnumConverter`
- **Integration Events** (10): SowUploadedEvent, ProjectBriefApprovedEvent, ProjectStatusChangedEvent, ProjectAwardedEvent, MilestoneApprovedEvent, ProposalSubmittedEvent, PaymentReceivedEvent, PayoutProcessedEvent, UserRegisteredEvent, VendorProfileUpdatedEvent — all implement `IIntegrationEvent`
- **DTOs** (16): Projects (6), Financials (5), Vendors (2), Users (3)
- **Common** (3): IIntegrationEvent, StandardizedErrorDto, PagedResultDto<T>
- **Tests**: 17 passing (IntegrationEventTests, DtoTests)

### Infrastructure Clients — Extended
- `IProjectServiceClient` — 7 new methods: brief CRUD, vendor matching, milestone management
- `IFinancialServiceClient` — added GenerateInvoiceAsync; FinancialSummaryDto → FinancialSummaryResponse
- `IUserServiceClient` — NEW: full user service interface (profile, invite, validate, activate)
- All HTTP clients implemented with resilience handlers (retry + circuit breaker)

### MediatR Features — 20 New Files
- `Briefs/Queries`: GetProjectBriefQuery + Handler
- `Briefs/Commands`: ApproveProjectBriefCommand + Handler
- `Vendors/Queries`: GetMatchingVendorsQuery + Handler
- `Milestones/Queries`: GetProjectMilestonesQuery + Handler
- `Milestones/Commands`: ApproveMilestoneCommand + Handler, RejectMilestoneCommand + Handler
- `Financials/Commands`: GenerateInvoiceCommand + Handler + Validator
- `Users/Queries`: GetUserProfileQuery + Handler
- `Users/Commands`: InviteUserCommand + Handler + Validator

### Controllers — New + Rewritten
- `BriefsController` — NEW: GET brief, PUT approve, GET matching vendors
- `MilestonesController` — NEW: GET milestones (auth), PUT public approve/reject (AllowAnonymous)
- `FinancialsController` — replaced 501 stub with GenerateInvoice
- `UsersController` — rewritten: GetCurrentUser enriched via User Service, InviteUser, public ValidateInvitation/ActivateUser

### Tests — 67 Unit + 18 Integration
**New Unit Tests (41 tests):**
- GetProjectBriefHandlerTests (3), ApproveProjectBriefHandlerTests (3)
- GetMatchingVendorsHandlerTests (3), GetProjectMilestonesHandlerTests (2)
- ApproveMilestoneHandlerTests (2), RejectMilestoneHandlerTests (2)
- GenerateInvoiceHandlerTests (3), GenerateInvoiceValidatorTests (5)
- GetUserProfileHandlerTests (2), InviteUserHandlerTests (3), InviteUserValidatorTests (5)

**Pre-existing Unit Tests (26 tests):** All still passing

**New Integration Tests (12 tests):**
- BriefsControllerTests (3), MilestonesControllerTests (3)
- UsersControllerTests (4), FinancialsControllerTests (2)

**Pre-existing Integration Tests (6 tests):** ProjectsControllerTests

---

## API Surface Summary

### Authenticated Endpoints (require JWT)
| Method | Route | Controller |
|--------|-------|------------|
| GET | `/api/v1/users/me` | UsersController |
| GET | `/api/v1/users/permissions/check?permission={perm}` | UsersController |
| POST | `/api/v1/users/invite` | UsersController |
| POST | `/api/v1/projects` | ProjectsController |
| GET | `/api/v1/projects/{id}/dashboard` | ProjectsController |
| POST | `/api/v1/projects/{id}/sow` | ProjectsController |
| GET | `/api/v1/projects/{id}/briefs` | BriefsController |
| PUT | `/api/v1/projects/{id}/briefs/approve` | BriefsController |
| GET | `/api/v1/projects/{id}/briefs/matching-vendors` | BriefsController |
| GET | `/api/v1/projects/{id}/milestones` | MilestonesController |
| GET | `/api/v1/financials/projects/{id}` | FinancialsController |
| POST | `/api/v1/financials/projects/{id}/invoices/generate` | FinancialsController |

### Public Endpoints (AllowAnonymous)
| Method | Route | Controller |
|--------|-------|------------|
| PUT | `/api/v1/public/milestones/{id}/approve` | MilestonesController |
| PUT | `/api/v1/public/milestones/{id}/reject` | MilestonesController |
| GET | `/api/v1/public/invitations/{token}/validate` | UsersController |
| POST | `/api/v1/public/invitations/{token}/activate` | UsersController |
| GET | `/health` | HealthCheck (built-in) |

---

## Known Issues / Technical Debt

### 1. Integration tests require .NET 8 runtime
Web project targets `net8.0` but dev machine only has .NET 10. `WebApplicationFactory<Program>` fails when test/host TFMs mismatch. **Fix:** Install .NET 8 runtime alongside .NET 10, or multi-target the Web project.

### 2. NuGet vulnerability warnings
`Microsoft.IdentityModel.JsonWebTokens` 7.0.3 and `System.IdentityModel.Tokens.Jwt` 7.0.3 have known moderate vulnerabilities (GHSA-59j7-ghrg-fj52). Upgrade when fixed versions are available.

### 3. Downstream service API contracts not yet implemented
The gateway's HTTP clients expect these endpoints from downstream services:
- **emp-project-management-service**: `/api/v1/projects/{id}/brief`, `/approve`, `/matching-vendors`, `/milestones`, milestone approve/reject
- **emp-financial-service**: `/api/v1/invoices/generate`
- **emp-user-management-service**: `/api/v1/users/{id}`, `/by-email/{email}`, `/invitations/*`

### 4. Missing FluentValidation validators for new commands/queries
ApproveProjectBriefCommand, GetProjectBriefQuery, GetMatchingVendorsQuery, GetProjectMilestonesQuery, ApproveMilestoneCommand, RejectMilestoneCommand — all lack validators.

### 5. Rate limiting not applied to public endpoints
Public milestone approval and invitation endpoints should have rate limiting to prevent abuse.

### 6. Swagger XML docs not configured
`GenerateDocumentationFile` is true but Swagger doesn't include XML comments. Add `c.IncludeXmlComments()`.

### 7. Docker Compose for local dev missing
No `docker-compose.dev.yml` for local PostgreSQL, RabbitMQ, Redis.

---

## File Structure (Current)

```
src/
  Emp.ApiGateway.Application/
    Behaviors/ValidationBehavior.cs
    DependencyInjection.cs
    Features/
      Briefs/Commands/ (ApproveProjectBriefCommand.cs, ApproveProjectBriefHandler.cs)
      Briefs/Queries/ (GetProjectBriefQuery.cs, GetProjectBriefHandler.cs)
      Financials/Commands/ (GenerateInvoiceCommand.cs, GenerateInvoiceHandler.cs, GenerateInvoiceValidator.cs)
      Milestones/Commands/ (ApproveMilestoneCommand.cs, ApproveMilestoneHandler.cs, RejectMilestoneCommand.cs, RejectMilestoneHandler.cs)
      Milestones/Queries/ (GetProjectMilestonesQuery.cs, GetProjectMilestonesHandler.cs)
      Projects/Commands/ (CreateProjectCommand.cs, CreateProjectCommandValidator.cs, CreateProjectHandler.cs, UploadSowCommand.cs, UploadSowHandler.cs)
      Projects/Queries/ (GetProjectDashboardQuery.cs, GetProjectDashboardHandler.cs)
      Users/Commands/ (InviteUserCommand.cs, InviteUserHandler.cs, InviteUserValidator.cs)
      Users/Queries/ (GetUserProfileQuery.cs, GetUserProfileHandler.cs)
      Vendors/Queries/ (GetMatchingVendorsQuery.cs, GetMatchingVendorsHandler.cs)
    Interfaces/Infrastructure/ (IFinancialServiceClient.cs, IMessageBus.cs, IProjectServiceClient.cs, IUserServiceClient.cs)
    Mappings/PublicApiMappingProfile.cs
  Emp.ApiGateway.Infrastructure/
    Configuration/ (AwsCognitoSettings.cs, ServiceUrls.cs)
    Messaging/MassTransitPublisher.cs
    Services/ (FinancialServiceClient.cs, ProjectServiceClient.cs, UserServiceClient.cs)
    DependencyInjection.cs
  Emp.ApiGateway.Web/
    Controllers/ (BriefsController.cs, FinancialsController.cs, MilestonesController.cs, ProjectsController.cs, UsersController.cs)
    Extensions/RateLimitExtensions.cs
    Middleware/ (CorrelationIdMiddleware.cs, GlobalExceptionHandler.cs)
    Program.cs
tests/
  Emp.ApiGateway.UnitTests/ (67 tests)
  Emp.ApiGateway.IntegrationTests/ (18 tests)
```
