# emp-project-management-service - Progress Tracker

## Status: Core Implementation Complete

Build: **PASSING** (0 errors, 0 warnings)
Test: Unit + Integration test scaffolding in place (requires .NET 8 runtime to execute)

---

## Completed Items

### 1. Project Structure Cleanup
- [x] Removed duplicate `ProjectManagement.*` empty directories (kept canonical `EnterpriseMediator.ProjectManagement.*`)
- [x] Rewrote `EnterpriseMediator.ProjectManagement.sln` with correct project references
- [x] Fixed `global.json` for SDK compatibility

### 2. Domain Layer (`EnterpriseMediator.ProjectManagement.Domain`)
- [x] `Project.cs` - Full aggregate root with complete lifecycle state machine
  - Create, UploadSow, AttachSowDetails, MarkSowFailed, UpdateBrief, ApproveBrief, DistributeBrief
  - AddProposal, AwardTo, ConfigureFinancials, AddMilestone, ApproveMilestone
  - Activate, Complete, PutOnHold, Resume, Cancel
  - Domain events raised on all state transitions
- [x] `Proposal.cs` - Entity with status lifecycle (Submitted -> InReview -> Shortlisted -> Accepted/Rejected/Withdrawn)
- [x] `Milestone.cs` - Entity with status lifecycle (Pending -> InProgress -> PendingApproval -> Approved/Rejected)
- [x] `SowDocument.cs` - Entity for SOW file metadata with processing status
- [x] `SowDetails.cs` - Value object for AI-extracted structured SOW data
- [x] `ProjectPayoutRule.cs` - Entity for payout rule configuration
- [x] `ProjectStatus.cs` - All enums: ProjectStatus, ProposalStatus, MilestoneStatus, SowDocumentStatus
- [x] `IDomainEvent` interface + 6 domain event records (implements MediatR INotification)
- [x] `IProjectRepository` - Full repository interface with UnitOfWork
- [x] `IVendorMatchingService` - Domain service interface for vector similarity matching
- [x] Added `MediatR.Contracts` package reference (minimal dependency for INotification)

### 3. Application Layer (`EnterpriseMediator.ProjectManagement.Application`)
- [x] `Result.cs` / `Result<T>` - Operation outcome monad
- [x] `ValidationBehavior.cs` - FluentValidation MediatR pipeline behavior
- [x] `IMessageBus` - Integration event publishing abstraction
- [x] `IFileStorageService` - S3 file storage abstraction
- [x] `AwsOptions`, `DatabaseOptions`, `RabbitMqOptions` - Typed configuration
- [x] `DependencyInjection.cs` - AddApplication() extension method

#### Commands (10 total)
- [x] `CreateProject` - Command, Handler, Validator (duplicate name check)
- [x] `UploadSow` - Command, Handler, Validator (10MB limit, .pdf/.docx/.doc whitelist)
- [x] `UpdateProjectBrief` - Command, Handler, Validator
- [x] `ApproveBrief` - Command, Handler
- [x] `DistributeBrief` - Command, Handler
- [x] `AwardProject` - Command, Handler, Validator (publishes ProjectAwardedIntegrationEvent via IMessageBus)
- [x] `ChangeProjectStatus` - Command, Handler, Validator (activate/complete/hold/resume/cancel)
- [x] `ConfigureFinancials` - Command, Handler
- [x] `SubmitProposal` - Command, Handler, Validator
- [x] `AddMilestone` - Command, Handler, Validator
- [x] `ApproveMilestone` - Command, Handler
- [x] `UpdateProposalAssessment` - Command, Handler

#### Queries (3 total)
- [x] `GetProjectById` - Query, Handler (with ProjectDetailDto, SowDetailsDto)
- [x] `GetMatchingVendors` - Query, Handler (delegates to IVendorMatchingService)
- [x] `GetProjectProposals` - Query, Handler (with ProposalDto)

### 4. Infrastructure Layer (`EnterpriseMediator.ProjectManagement.Infrastructure`)
- [x] `ProjectDbContext` - DbContext with IUnitOfWork, domain event dispatching, pgvector extension
- [x] EF Core Configurations: Project, Proposal, SowDocument, Milestone, ProjectPayoutRule
  - JSONB mapping for SowDetails (owned entity)
  - Row versioning on Project
  - Unique index on (ProjectId, VendorId) for proposals
- [x] `ProjectRepository` - Full implementation with eager loading variants
- [x] `VectorVendorMatchingService` - pgvector cosine similarity vendor matching via raw SQL
- [x] `S3FileStorageService` - AWS S3 file upload/download/delete
- [x] `MassTransitMessageBus` - MassTransit publish endpoint adapter
- [x] `DependencyInjection.cs` - AddInfrastructure() with EF Core, MassTransit, AWS S3, repository registration

### 5. WebAPI Layer (`EnterpriseMediator.ProjectManagement.WebAPI`)
- [x] `ProjectsController.cs` - REST API with 15 endpoints, [Authorize], ProducesResponseType attributes
  - POST /api/projects (CreateProject)
  - GET /api/projects/{id} (GetProjectById)
  - POST /api/projects/{id}/sow (UploadSow, 10MB limit)
  - PUT /api/projects/{id}/brief (UpdateProjectBrief)
  - POST /api/projects/{id}/brief/approve (ApproveBrief)
  - POST /api/projects/{id}/brief/distribute (DistributeBrief)
  - POST /api/projects/{id}/award (AwardProject)
  - POST /api/projects/{id}/status (ChangeProjectStatus)
  - PUT /api/projects/{id}/financials (ConfigureFinancials)
  - GET /api/projects/{id}/matching-vendors (GetMatchingVendors)
  - POST /api/projects/{id}/proposals (SubmitProposal)
  - GET /api/projects/{id}/proposals (GetProjectProposals)
  - PUT /api/projects/{id}/proposals/{proposalId}/assessment (UpdateProposalAssessment)
  - POST /api/projects/{id}/milestones (AddMilestone)
  - POST /api/projects/{id}/milestones/{milestoneId}/approve (ApproveMilestone)
- [x] `Program.cs` - Serilog bootstrap, JWT (AWS Cognito), CORS, Swagger, health checks
- [x] `GlobalExceptionHandler.cs` - IExceptionHandler with ProblemDetails (RFC 7807)
- [x] `appsettings.json` / `appsettings.Development.json`
- [x] `Dockerfile` - Multi-stage build for Linux container

### 6. Unit Tests (`EnterpriseMediator.ProjectManagement.UnitTests`)
- [x] `ProjectAggregateTests.cs` - Comprehensive domain aggregate tests (all state transitions, domain events, edge cases)
- [x] `CreateProjectCommandHandlerTests.cs` - Handler tests with mocked repository
- [x] `UploadSowCommandHandlerTests.cs` - Handler tests with mocked repository + file storage

### 7. Integration Tests (`EnterpriseMediator.ProjectManagement.IntegrationTests`)
- [x] `PostgresFixture.cs` - Testcontainers fixture for PostgreSQL with pgvector
- [x] `ProjectRepositoryTests.cs` - Repository round-trip, client query, name exists, proposal include, status update

---

## Remaining / Future Items

### High Priority
- [ ] Add EF Core migration (initial create) for production schema management
- [ ] MassTransit consumer for `SowProcessedEvent` from AI worker (AttachSowDetails flow)
- [ ] MassTransit consumer for `SowProcessingFailedEvent` from AI worker (MarkSowFailed flow)
- [ ] Add `ListProjects` query with pagination (GET /api/projects?clientId=&status=&page=&pageSize=)

### Medium Priority
- [ ] Add `docker-compose.dev.yml` for local development (PostgreSQL, RabbitMQ, Redis)
- [ ] Add GitHub Actions CI pipeline (build, test, Docker push)
- [ ] Add Serilog enrichment with correlation IDs from request headers
- [ ] Add rate limiting middleware on SOW upload endpoint
- [ ] Add RBAC policy-based authorization (Admin vs Client vs Vendor roles)
- [ ] Implement `GetProjectsByStatus` and `GetProjectsByClient` query endpoints

### Low Priority / Enhancements
- [ ] Move `ProjectAwardedIntegrationEvent` to `emp-shared-contracts` when that repo is implemented
- [ ] Add Redis caching for frequently-queried project details
- [ ] Add OpenTelemetry tracing for cross-service observability
- [ ] Consider switching from raw SQL in VectorVendorMatchingService to a dedicated search endpoint
- [ ] Add Polly retry policies around S3 and RabbitMQ operations
- [ ] Add health check for PostgreSQL and RabbitMQ connectivity

### Dependencies on Other Services
- **emp-ai-processing-worker**: Consumes `SowUploadedDomainEvent`, produces `SowProcessedEvent`/`SowProcessingFailedEvent`
- **emp-financial-service**: Consumes `ProjectAwardedIntegrationEvent` and `MilestoneApprovedDomainEvent`
- **emp-user-management-service**: Vendor/Client profile data for proposal validation
- **emp-api-gateway**: Routes external requests, handles auth token forwarding

---

## Architecture Notes

### Project Status State Machine
```
Pending -> Processing -> Processed -> BriefApproved -> Proposed -> Awarded -> Active -> Completed
                 |                                                              |
                 v                                                              v
              Failed                                                         OnHold -> Active

Any non-terminal state -> Cancelled
```

### Key Design Decisions
1. **Domain events implement MediatR.INotification** via `MediatR.Contracts` package (minimal dep for DDD purity trade-off)
2. **SowDetails mapped as JSONB** via EF Core owned entity ToJson() for flexible schema
3. **Row versioning** on Project aggregate for optimistic concurrency
4. **Split queries** on full project load to avoid Cartesian explosion
5. **Integration events** published via `IMessageBus` abstraction (MassTransit) after UnitOfWork save
6. **Vendor matching** uses pgvector cosine similarity with HNSW index support
