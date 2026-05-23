# Stage 1: Base Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
USER app

# Stage 2: Build Image (monorepo root context)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV DOCKER_BUILD=true
WORKDIR /src

# Monorepo-wide build props (NU1903 suppression) — must exist before restore
COPY ["Directory.Build.props", "./"]

# Shared contracts (required by Application layer project reference)
COPY ["emp-shared-contracts/src/EnterpriseMediator.Contracts/EnterpriseMediator.Contracts.csproj", "emp-shared-contracts/src/EnterpriseMediator.Contracts/"]

# Service project files for layer-cached restore
COPY ["emp-api-gateway/src/Emp.ApiGateway.Application/Emp.ApiGateway.Application.csproj", "emp-api-gateway/src/Emp.ApiGateway.Application/"]
COPY ["emp-api-gateway/src/Emp.ApiGateway.Infrastructure/Emp.ApiGateway.Infrastructure.csproj", "emp-api-gateway/src/Emp.ApiGateway.Infrastructure/"]
COPY ["emp-api-gateway/src/Emp.ApiGateway.Web/Emp.ApiGateway.Web.csproj", "emp-api-gateway/src/Emp.ApiGateway.Web/"]

RUN dotnet restore "emp-api-gateway/src/Emp.ApiGateway.Web/Emp.ApiGateway.Web.csproj" /p:TargetFrameworks=net8.0

COPY ["emp-shared-contracts/", "emp-shared-contracts/"]
COPY ["emp-api-gateway/", "emp-api-gateway/"]

WORKDIR "/src/emp-api-gateway/src/Emp.ApiGateway.Web"
RUN dotnet build "Emp.ApiGateway.Web.csproj" -c $BUILD_CONFIGURATION /p:TargetFrameworks=net8.0 -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Emp.ApiGateway.Web.csproj" -c $BUILD_CONFIGURATION /p:TargetFrameworks=net8.0 -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Emp.ApiGateway.Web.dll"]
