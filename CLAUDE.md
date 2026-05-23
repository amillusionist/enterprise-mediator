# CLAUDE.md — Enterprise Mediator Platform (EMP)

You are a senior software engineer with FAANG-level experience working on this codebase. Apply production-quality standards unconditionally. Every file you write or modify must be correct, complete, and maintainable. No stubs, no `throw new NotImplementedException()`, no `// TODO: implement`.

---

## Project Overview

**Enterprise Mediator Platform (EMP)** is a cloud-native, multi-tenant SaaS CRM and project management system for a software consulting intermediary. Its core capability is the full lifecycle from SOW upload, AI-powered vendor matchmaking, proposal management, to multi-stakeholder financial settlement.

This is a **Polyrepo** containing all 11 service repositories as subdirectories. All subdirectories are independent deployable units with their own solution files, Dockerfiles, and CI pipelines.

---

## Repository Map

```
emp-api-gateway/               # BFF API Gateway (ASP.NET Core 8)
emp-ai-processing-worker/      # Async SOW processing worker (.NET 8 Worker Service)
emp-core-shared-kernel/        # Shared library: Result<T>, behaviors, resiliency, logging
emp-domain-models/             # DDD domain entities (zero external deps)
emp-financial-service/         # Invoicing, Stripe payments, Wise payouts
emp-frontend-webapp/           # Next.js 14 SPA (App Router)
emp-platform-infrastructure/   # Terraform IaC for AWS (docs only, not yet implemented)
emp-project-management-service/# Project/SOW/Proposal lifecycle service
emp-shared-contracts/          # DTOs + event schemas (docs only, not yet implemented)
emp-ui-component-library/      # React component library (Radix UI + Tailwind + CVA)
emp-user-management-service/   # User/Client/Vendor profiles + RBAC
docs/                          # Architecture, DB schema, requirements, sequence diagrams, UI mockups
```

### Implementation Status
- **emp-api-gateway**: Scaffolded, partially implemented (Controllers, MediatR pipeline, MassTransit publisher, AWS Cognito auth)
- **emp-ai-processing-worker**: Scaffolded, partially implemented (SOW processing pipeline, adapters)
- **emp-core-shared-kernel**: Implemented (Result<T>, behaviors, Polly, Serilog)
- **emp-domain-models**: Implemented (all aggregates, value objects, domain events)
- **emp-financial-service**: Domain + Infrastructure layers implemented; Application + Web.API partially done; tests folder empty
- **emp-frontend-webapp**: Partially implemented (auth, SOW review, proposals, finance features)
- **emp-platform-infrastructure**: Terraform config files only, no `.tf` modules yet
- **emp-project-management-service**: Has duplicate folder structure issue (see Known Issues section); partially implemented
- **emp-shared-contracts**: Docs only, no source code yet
- **emp-ui-component-library**: Partially implemented (Atoms: Button, Badge, Input, Label; Molecules: Avatar, Dialog, Select, Toast)
- **emp-user-management-service**: Scaffolded and partially implemented

---

## Architecture

### Style: Modular Monolith with Event-Driven Async Processing

The backend uses **Clean Architecture** strictly:
- `Domain` layer — zero external dependencies; aggregates, value objects, domain events, repository interfaces
- `Application` layer — MediatR commands/queries, use-case orchestration, application interfaces
- `Infrastructure` layer — EF Core, external APIs (Stripe, Wise, AWS SES, Azure OpenAI), messaging
- `Web/API` layer — ASP.NET Core controllers, middleware, DI wiring

Dependency direction: Web → Application → Domain ← Infrastructure

### Key Patterns
- **DDD**: Aggregate roots with typed IDs (e.g., `ProjectId`, `VendorId`), value objects, domain events
- **CQRS via MediatR**: All writes are Commands, all reads are Queries; handlers in `Features/{Domain}/{Commands|Queries}/`
- **Repository Pattern**: Interface in Domain, EF Core implementation in Infrastructure; base `EfRepository<T>` in shared kernel
- **Event-Driven**: `SowUploadedEvent` published via MassTransit → consumed by AI worker; `ProjectAwardedEvent` → financial service
- **Result<T>**: Use `Result<T>` from shared kernel for operation outcomes — never throw for business logic failures
- **MediatR Pipeline Behaviors** (from shared kernel): `ValidationBehavior` → `LoggingBehavior` → `PerformanceBehavior`

---

## Technology Stack (Authoritative — Verified from Source)

### Backend
| Concern | Technology |
|---|---|
| Language | C# 12, .NET 8 |
| Web Framework | ASP.NET Core 8 |
| Mediator / CQRS | MediatR |
| Messaging | MassTransit (over RabbitMQ) |
| Validation | FluentValidation |
| ORM | Entity Framework Core 8 (Npgsql) |
| Logging | Serilog (structured JSON, `ILogger<T>`) |
| Resiliency | Polly (retry, circuit breaker via `PollyPolicyExtensions`) |
| Testing | xUnit, Moq, FluentAssertions |
| Auth | **AWS Cognito** (JwtBearer — `AddJwtBearer` validating Cognito issuer) |
| Email | AWS SES SDK v2 |
| AI / LLM | Azure OpenAI SDK (`Azure.AI.OpenAI`) — GPT-4 Turbo + embeddings |
| Payments | Stripe SDK |
| Payouts | Wise API |
| Document parsing | TikaOnDotNet |
| Vector search | pgvector 0.7.x + `pgvector.EntityFrameworkCore` |

### Frontend
| Concern | Technology |
|---|---|
| Framework | Next.js 14 (App Router, React Server Components) |
| Language | TypeScript 5.4 (strict mode) |
| Styling | Tailwind CSS 3 |
| Components | Radix UI + `cva` (class-variance-authority) — shadcn/ui pattern |
| State (global) | Zustand (`src/store/`) |
| Forms | React Hook Form + Zod |
| API calls | `ApiClient` static class (`src/services/api-client.ts`) — server-side only |
| Server Actions | `src/actions/*.actions.ts` |
| Services | `src/services/*.service.ts` for data-fetching logic |
| Testing | Jest, React Testing Library, Playwright |

### Infrastructure
| Concern | Technology |
|---|---|
| Database | PostgreSQL 16 (AWS RDS) |
| Cache | Redis 7.2.x (AWS ElastiCache) |
| Broker | RabbitMQ 3.13.x (via MassTransit) |
| Storage | AWS S3 (SOW documents) |
| Containers | Docker |
| Orchestration | Kubernetes 1.29.x (AWS EKS) |
| IaC | Terraform (not yet scaffolded) |
| CI/CD | GitHub Actions (per-repo) |
| Config secrets | AWS Secrets Manager |

---

## Naming Conventions (Verified from Source)

### Backend Assembly Names
Each service has its own naming prefix — be consistent per service:

| Repo | Namespace prefix |
|---|---|
| `emp-api-gateway` | `Emp.ApiGateway.*` |
| `emp-ai-processing-worker` | `EnterpriseMediator.AiWorker.*` |
| `emp-core-shared-kernel` | `EnterpriseMediator.Core.SharedKernel.*` |
| `emp-domain-models` | `EnterpriseMediator.Domain.*` |
| `emp-financial-service` | `EnterpriseMediator.Financial.*` |
| `emp-project-management-service` | `EnterpriseMediator.ProjectManagement.*` |
| `emp-user-management-service` | `EnterpriseMediator.UserManagement.*` |

### File/Folder Structure per Service (Backend)
```
src/
  {Namespace}.Domain/
    Aggregates/
    Enums/
    Events/
    Interfaces/          # Repository + service interfaces
    ValueObjects/
    Services/            # Domain services
  {Namespace}.Application/
    Features/
      {Domain}/
        Commands/
          {CommandName}/
            {CommandName}Command.cs
            {CommandName}Handler.cs
            {CommandName}Validator.cs (FluentValidation)
        Queries/
          {QueryName}/
            {QueryName}Query.cs
            {QueryName}Handler.cs
    Interfaces/          # Application-level service interfaces
    DependencyInjection.cs
  {Namespace}.Infrastructure/
    Persistence/
      Configurations/    # EF Core IEntityTypeConfiguration<T> files
      Repositories/
    Gateways/            # External API clients (Stripe, Wise, AWS SES, etc.)
    Messaging/           # MassTransit consumers/publishers
    DependencyInjection.cs
  {Namespace}.Web.API/ (or .WebAPI)
    Controllers/
    Middleware/
    Program.cs
tests/
  {Namespace}.UnitTests/
  {Namespace}.IntegrationTests/
```

### Frontend File/Folder Structure
```
src/
  app/
    (auth)/              # Login, registration routes
    (dashboard)/         # Protected admin/user dashboard routes
    (public)/            # Unauthenticated public routes (vendor portal, invoice pay, milestone approve)
  actions/               # Next.js Server Actions: {domain}.actions.ts
  components/
    features/            # Feature-specific composite components
      sow/
      proposals/
      finance/
      notifications/
      public/
  hooks/                 # Custom React hooks: use-{name}.ts
  lib/
    types.ts             # All shared TypeScript types/interfaces
    schemas.ts           # Zod validation schemas
    constants.ts
    utils.ts
  services/              # Data-fetching services (server-side): {domain}.service.ts
  store/                 # Zustand stores
  config/
  middleware.ts          # Next.js auth middleware
```

---

## Domain Model Reference

### Aggregates (from `emp-domain-models`)
- `User` (UserManagement) — system users; roles: `SystemAdministrator`, `VendorContact`, `ClientContact`
- `Client` (ClientManagement) — client company profiles
- `Vendor` (VendorManagement) — vendor profiles with skill embeddings (`EmbeddingVector` value object)
- `Project` (ProjectManagement) — full lifecycle aggregate with SOW, brief, proposals, milestones
- `SowDocument` — SOW file metadata and processing status
- `ProjectBrief` — AI-extracted structured data from SOW
- `Proposal` — vendor proposal against a project brief
- `Milestone` — project milestone with approval workflow
- `Invoice` (Financials) — client invoice, tracked in immutable ledger
- `Payout` (Financials) — vendor payout via Wise
- `Transaction` (Financials) — immutable financial transaction record
- `AuditLog` — system audit trail

### Value Objects
`Money`, `Currency`, `Address`, `Email`, `PhoneNumber`, `EmbeddingVector` (1536-dim vector for pgvector)

### Typed IDs
All aggregate IDs use strongly-typed wrappers: `ProjectId`, `VendorId`, `ClientId`, `UserId`, `InvoiceId`, etc. Always use typed IDs — never raw `Guid` in domain logic.

### Domain Events
`SowUploadedDomainEvent`, `ProjectCreatedDomainEvent`, `ProjectStatusChangedDomainEvent`, `ProjectAwardedDomainEvent`, `MilestoneApprovedDomainEvent`, `InvoicePaidDomainEvent`

### Project Status Flow
`Pending` → `Proposed` (brief distributed) → `Awarded` → `Active` → `Completed`
Also: `Active` → `OnHold` → `Active`, any non-closed state → `Cancelled`

---

## Known Issues / Technical Debt

1. **`emp-project-management-service` duplicate folders**: Both `EnterpriseMediator.ProjectManagement.*` and `ProjectManagement.*` folder trees exist. The `ProjectManagement.*` folders contain empty projects (just `.csproj` files). These are likely scaffolding artifacts and should be cleaned up — use `EnterpriseMediator.ProjectManagement.*` as the canonical namespace.

2. **`emp-shared-contracts` not implemented**: This repo contains only docs. DTOs and event schemas are currently defined within each service. When implementing, create shared contract types here and reference them across services.

3. **`emp-platform-infrastructure` not implemented**: Terraform modules not written yet. Config files (`.checkov.yaml`, `.tflint.hcl`, `.pre-commit-config.yaml`) are in place.

4. **Financial service tests empty**: `tests/EnterpriseMediator.Financial.IntegrationTests/` and `UnitTests/` directories exist but contain no test files.

5. **`Money` value object duplication**: `emp-domain-models` has `Money` as a `ValueObject` subclass; `emp-financial-service` has its own `Money` as a `record`. Consolidate to use the one from `emp-domain-models` long-term.

---

## Coding Standards

### C# / .NET

- **Clean Architecture is a hard constraint.** Domain layer has zero dependencies on Application, Infrastructure, or external libs.
- Use `Result<T>` from `EnterpriseMediator.Core.SharedKernel.Common` for operation outcomes. Never throw for expected business failures.
- `CancellationToken ct` parameter on **every** async method signature.
- Structured logging only: `_logger.LogInformation("Created project {ProjectId}", projectId)` — no string interpolation in log messages.
- `IOptions<T>` for all typed configuration sections.
- FluentValidation for all DTOs at the API boundary. Validators auto-register via `AddValidatorsFromAssembly`.
- MediatR handlers: one handler per file, one responsibility per handler.
- EF Core: all queries async; use `IUnitOfWork` (single `SaveChangesAsync()` per business operation); migrations must be additive.
- Controllers return `ActionResult<T>` with XML doc comments and `[ProducesResponseType]` attributes on every endpoint.
- Global exception handling via `GlobalExceptionHandler` middleware returning ProblemDetails (RFC 7807).
- All entity IDs are GUIDs, expressed as strongly-typed ID wrappers in Domain.
- Financial ledger entries (`Transaction`) are immutable — no UPDATE/DELETE in migrations or repositories.
- Secrets never in `appsettings.json` — use `from-secrets-manager` convention and AWS Secrets Manager in production.
- Follow the `Program.cs` pattern established in `emp-api-gateway`: bootstrap Serilog → add services → build → configure pipeline.
- MassTransit consumers live in `Infrastructure/Messaging/`; publishers use `IMassTransitPublisher` interface.

### TypeScript / React / Next.js

- `"strict": true` in `tsconfig.json`. No `any`. No type assertions without a comment explaining why.
- All page-level data fetching happens in **Server Components** using parallel `Promise.all()` for independent requests.
- Server Actions in `src/actions/` — these are the mutation entry points from the UI.
- `ApiClient` (static class in `src/services/api-client.ts`) is the **only** way to make API calls from the server side.
- UI components from `emp-ui-component-library` only — never import directly from Radix UI in `emp-frontend-webapp`.
- Radix UI + `cva` pattern: define variants with `cva()`, compose with `cn()` utility, expose via `forwardRef`.
- Zod schemas in `src/lib/schemas.ts` for all form validation and API response validation.
- Route groups in App Router: `(auth)` for public auth pages, `(dashboard)` for protected admin routes, `(public)` for unauthenticated externally-shared links (vendor portal, invoice payment, milestone approval).
- `next/image` for all images, never raw `<img>`.
- Tailwind classes only — no inline styles, no CSS modules.
- WCAG 2.1 AA: every interactive element must have accessible labels; rely on Radix UI's built-in accessibility where possible.
- Type definitions in `src/lib/types.ts` — single source of truth for all TS interfaces.

### Testing

- Backend unit tests: test Application layer (handlers) in isolation using Moq; use FluentAssertions for assertions.
- Backend integration tests: use Testcontainers.NET for real PostgreSQL; test repository implementations and EF Core queries.
- Every MassTransit consumer must have an integration test verifying the message contract.
- Frontend: Jest + React Testing Library for components; Playwright for critical user journeys (SOW upload → review → vendor match → proposal → award).
- Test file location: mirror the `src/` structure under `tests/`.

---

## Performance Targets

| Metric | Target |
|---|---|
| API p95 response time | < 250ms |
| Frontend LCP | < 2.5s |
| Vector search (cosine similarity) | < 100ms (HNSW index on vendor embeddings) |
| Dashboard load | Pre-calculated `DashboardMetrics` table, refreshed every 15 min |
| Min vendor match similarity score | 0.75 |
| Max vendor match results | 25 |

---

## Security Rules (Non-Negotiable)

- All controllers have `[Authorize]` unless explicitly public. Never leave endpoints open without intentional decision.
- RBAC via role claims from AWS Cognito groups.
- SOW file uploads: whitelist `.pdf`, `.docx`, `.doc`; max 10MB per controller.
- PII must be sanitized (`IPiiSanitizationService` in AI worker) before sending SOW content to Azure OpenAI.
- `access_token` stored in HttpOnly cookies on the frontend — never in `localStorage`.
- Rate limiting on auth endpoints (see `RateLimitExtensions.cs` in the gateway).
- CORS: only `AllowedOrigins` from config, never wildcard `*` in production.

---

## Working with the Docs

All design artifacts live in `docs/`. Before implementing any feature, consult:

- `docs/sequence/` — 18 sequence diagrams defining exact interaction contracts between services
- `docs/architecture/` — Architecture Details, Components, Technology, Deployment, CI/CD, Scaling
- `docs/database/` — Full PostgreSQL schema scripts (transactional + observability databases)
- `docs/requirements/` — Full requirements (31 sections, detail plan CSV)
- `docs/ui-mockups/` — HTML mockups for all major screens

**Sequence diagrams are the contract.** If your implementation deviates from a sequence diagram, that is a bug unless the diagram is outdated — flag it explicitly.

---

## Implementation Priorities and Workflow

### Before implementing any feature
1. Read the relevant sequence diagram(s) from `docs/sequence/`.
2. Check the database schema in `docs/database/` for entity structure.
3. Check if the domain aggregate already exists in `emp-domain-models`.
4. Check if the Application layer feature exists in the target service.

### When adding a backend feature (outside-in)
1. Define/update the DTO — in the service's Application layer (or `emp-shared-contracts` once implemented).
2. Write the FluentValidation validator.
3. Write the MediatR command/query and handler.
4. Update the domain aggregate if needed (domain logic only — no infra concerns).
5. Implement/update the repository in Infrastructure.
6. Wire up the controller endpoint.
7. Write unit tests for the handler and integration tests for the repository.

### When adding a MassTransit event flow
1. Define the event message contract (currently in service; will move to `emp-shared-contracts`).
2. Implement the publisher in the triggering service's Infrastructure/Messaging.
3. Implement the consumer in the subscribing service's Infrastructure/Messaging.
4. Register both with MassTransit DI.
5. Write integration tests for publisher and consumer separately.

### When adding a frontend page
1. Create a Server Component in the appropriate App Router route group.
2. Fetch data using `ProjectService`/`FinanceService`/etc. with `Promise.all()` for parallel fetches.
3. Pass data to Client Components as props.
4. Use components from `emp-ui-component-library` — never raw HTML for interactive elements.
5. Define any new types in `src/lib/types.ts`, Zod schemas in `src/lib/schemas.ts`.
6. Server mutations go in `src/actions/` as Server Actions.

### Financial operations
- Every financial write must be inside a DB transaction.
- `Transaction` records are immutable once written — only INSERT, never UPDATE/DELETE.
- Always validate monetary amounts server-side; never trust client-supplied financial figures.
- Use `Money` value object for all monetary arithmetic; never operate on raw `decimal` for money.

---

## Environment Configuration

Local development uses Docker Compose (per-service `docker-compose.dev.yml`):
- PostgreSQL 16 with `pgvector` extension
- RabbitMQ 3.13 management UI
- Redis 7.2

Secrets for local dev come from `appsettings.Development.json` or `.env` files. Never commit real credentials.

All backend services read config via ASP.NET Core `IConfiguration` with AWS Secrets Manager as the production source.

Frontend reads `NEXT_PUBLIC_API_URL` (public) and `API_URL` (server-side internal Docker network URL) from environment variables.
