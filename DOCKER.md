# Docker — Enterprise Mediator Platform

This monorepo ships **one Dockerfile per deployable service**. There is no single root `Dockerfile`; use **Docker Compose** to run the full stack locally.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/macOS) or Docker Engine + Compose v2 (Linux)
- Start Docker Desktop before running any commands below

## Run the full stack

From the **repository root**:

```bash
docker compose up --build
```

| Service            | URL / port                          |
|--------------------|-------------------------------------|
| Frontend           | http://localhost:3000               |
| API Gateway        | http://localhost:5000               |
| Project service    | http://localhost:5001               |
| User service       | http://localhost:5003               |
| RabbitMQ UI        | http://localhost:15672 (guest/guest)|
| PostgreSQL         | localhost:5432                      |
| Redis              | localhost:6379                      |

### Optional services

Financial service and AI worker use the `optional` profile:

```bash
docker compose --profile optional up --build
```

## Per-service Dockerfiles

Build context must always be the **monorepo root** (`.`), not an individual service folder.

| Service              | Dockerfile |
|----------------------|------------|
| API Gateway          | `emp-api-gateway/Dockerfile` |
| Project management   | `emp-project-management-service/Dockerfile` |
| User management      | `emp-user-management-service/src/EnterpriseMediator.UserManagement.API/Dockerfile` |
| Financial            | `emp-financial-service/Dockerfile` |
| AI processing worker | `emp-ai-processing-worker/src/EnterpriseMediator.AiWorker/Dockerfile` |
| Frontend (Next.js)   | `emp-frontend-webapp/Dockerfile` (context: `emp-frontend-webapp/`) |

Example — build API Gateway only:

```bash
docker build -f emp-api-gateway/Dockerfile -t emp-api-gateway .
```

Example — build AI worker only:

```bash
docker build -f emp-ai-processing-worker/src/EnterpriseMediator.AiWorker/Dockerfile -t emp-ai-worker .
```

## Local dev credentials (API Gateway)

When `LocalDevAuth__Enabled` is `true` in `docker-compose.yml`:

- **admin@local.dev** / `Admin@123` (SystemAdministrator)
- **user@local.dev** / `User@123` (ClientContact)
