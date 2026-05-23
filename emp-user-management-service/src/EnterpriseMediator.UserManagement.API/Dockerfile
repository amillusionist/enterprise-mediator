FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV DOCKER_BUILD=true
WORKDIR /src

COPY ["Directory.Build.props", "./"]

COPY ["emp-shared-contracts/src/EnterpriseMediator.Contracts/EnterpriseMediator.Contracts.csproj", "emp-shared-contracts/src/EnterpriseMediator.Contracts/"]

COPY ["emp-user-management-service/src/EnterpriseMediator.UserManagement.API/EnterpriseMediator.UserManagement.API.csproj", "emp-user-management-service/src/EnterpriseMediator.UserManagement.API/"]
COPY ["emp-user-management-service/src/EnterpriseMediator.UserManagement.Application/EnterpriseMediator.UserManagement.Application.csproj", "emp-user-management-service/src/EnterpriseMediator.UserManagement.Application/"]
COPY ["emp-user-management-service/src/EnterpriseMediator.UserManagement.Domain/EnterpriseMediator.UserManagement.Domain.csproj", "emp-user-management-service/src/EnterpriseMediator.UserManagement.Domain/"]
COPY ["emp-user-management-service/src/EnterpriseMediator.UserManagement.Infrastructure/EnterpriseMediator.UserManagement.Infrastructure.csproj", "emp-user-management-service/src/EnterpriseMediator.UserManagement.Infrastructure/"]
COPY ["emp-user-management-service/Directory.Build.props", "emp-user-management-service/"]
COPY ["emp-user-management-service/Directory.Packages.props", "emp-user-management-service/"]

RUN dotnet restore "emp-user-management-service/src/EnterpriseMediator.UserManagement.API/EnterpriseMediator.UserManagement.API.csproj" /p:TargetFrameworks=net8.0

COPY ["emp-shared-contracts/", "emp-shared-contracts/"]
COPY ["emp-user-management-service/", "emp-user-management-service/"]

WORKDIR "/src/emp-user-management-service/src/EnterpriseMediator.UserManagement.API"
RUN dotnet build "EnterpriseMediator.UserManagement.API.csproj" -c $BUILD_CONFIGURATION /p:TargetFrameworks=net8.0 -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EnterpriseMediator.UserManagement.API.csproj" -c $BUILD_CONFIGURATION /p:TargetFrameworks=net8.0 -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnterpriseMediator.UserManagement.API.dll"]
