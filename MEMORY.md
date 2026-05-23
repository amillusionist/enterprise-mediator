# EMP Project Memory

## Project
Enterprise Mediator Platform (EMP) — cloud-native SaaS CRM for software consulting intermediary.
Root: `d:\enterprise-mediator-platform-emp-main`
CLAUDE.md created at root with full project context.

## Architecture
- Polyrepo monorepo: 11 service subdirectories + docs
- Backend: Clean Architecture (Domain → Application → Infrastructure → Web/API)
- CQRS via MediatR, Repository pattern, DDD aggregates
- Event-driven: MassTransit over RabbitMQ for async SOW processing
- Auth: AWS Cognito JwtBearer (NOT generic JWT)

## Tech Stack (verified from source)
- Backend: C# 12, .NET 8, ASP.NET Core 8, EF Core 8, MediatR, MassTransit, FluentValidation, Serilog, Polly
- Database: PostgreSQL 16 + pgvector 0.7.x
- Frontend: Next.js 14 App Router, TypeScript 5.4 strict, Tailwind CSS 3, Radix UI + cva (shadcn pattern), Zustand, React Hook Form + Zod
- AI: Azure OpenAI SDK (GPT-4 Turbo + embeddings)
- Payments: Stripe (client invoices), Wise (vendor payouts)
- Infrastructure: Docker, Kubernetes/AWS EKS, Terraform (not yet scaffolded)

## Service Namespace Prefixes
- emp-api-gateway → Emp.ApiGateway.*
- emp-ai-processing-worker → EnterpriseMediator.AiWorker.*
- emp-core-shared-kernel → EnterpriseMediator.Core.SharedKernel.*
- emp-domain-models → EnterpriseMediator.Domain.*
- emp-financial-service → EnterpriseMediator.Financial.*
- emp-project-management-service → EnterpriseMediator.ProjectManagement.*
- emp-user-management-service → EnterpriseMediator.UserManagement.*

## Implementation Status
- IMPLEMENTED: emp-core-shared-kernel (Result<T>, behaviors), emp-domain-models (all aggregates/VOs/events)
- PARTIAL: emp-api-gateway, emp-ai-processing-worker, emp-financial-service, emp-frontend-webapp, emp-user-management-service, emp-ui-component-library, emp-project-management-service
- DOCS ONLY (no source code): emp-shared-contracts, emp-platform-infrastructure

## Known Issues
1. emp-project-management-service has duplicate folder structure: both `EnterpriseMediator.ProjectManagement.*` AND `ProjectManagement.*` — use `EnterpriseMediator.ProjectManagement.*` canonical, the others are empty scaffolding artifacts
2. emp-shared-contracts: docs only, no source code — DTOs currently defined per-service
3. emp-platform-infrastructure: Terraform config files only, no .tf modules
4. Financial service test folders exist but are empty
5. Money value object duplicated: domain-models uses ValueObject base class; financial-service uses record — consolidate eventually

## Frontend Structure
- Route groups: (auth), (dashboard), (public)
- API calls: ApiClient static class in src/services/api-client.ts (server-side)
- Server Actions: src/actions/*.actions.ts
- Services: src/services/*.service.ts
- Types: src/lib/types.ts (single source of truth)
- Zod schemas: src/lib/schemas.ts
- access_token stored in HttpOnly cookies

## Key Patterns to Always Follow
- Result<T> from SharedKernel — never throw for business logic failures
- CancellationToken on all async methods
- Structured logging — no string interpolation in log messages
- Financial Transaction records are immutable (INSERT only, never UPDATE/DELETE)
- PII sanitization (IPiiSanitizationService) before Azure OpenAI calls
- Typed ID wrappers for all aggregate IDs (ProjectId, VendorId, etc.)
- SOW file whitelist: .pdf, .docx, .doc; max 10MB

## Docs Reference
- docs/sequence/ — 18 sequence diagrams (implementation contracts)
- docs/architecture/ — components, tech stack, deployment, CI/CD
- docs/database/ — PostgreSQL schema scripts
- docs/requirements/ — 31-section requirements
- docs/ui-mockups/ — HTML mockups for all screens
