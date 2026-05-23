FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV DOCKER_BUILD=true
WORKDIR /src

COPY ["Directory.Build.props", "./"]

COPY ["emp-shared-contracts/src/EnterpriseMediator.Contracts/EnterpriseMediator.Contracts.csproj", "emp-shared-contracts/src/EnterpriseMediator.Contracts/"]

COPY ["emp-project-management-service/src/EnterpriseMediator.ProjectManagement.WebAPI/EnterpriseMediator.ProjectManagement.WebAPI.csproj", "emp-project-management-service/src/EnterpriseMediator.ProjectManagement.WebAPI/"]
COPY ["emp-project-management-service/src/EnterpriseMediator.ProjectManagement.Application/EnterpriseMediator.ProjectManagement.Application.csproj", "emp-project-management-service/src/EnterpriseMediator.ProjectManagement.Application/"]
COPY ["emp-project-management-service/src/EnterpriseMediator.ProjectManagement.Domain/EnterpriseMediator.ProjectManagement.Domain.csproj", "emp-project-management-service/src/EnterpriseMediator.ProjectManagement.Domain/"]
COPY ["emp-project-management-service/src/EnterpriseMediator.ProjectManagement.Infrastructure/EnterpriseMediator.ProjectManagement.Infrastructure.csproj", "emp-project-management-service/src/EnterpriseMediator.ProjectManagement.Infrastructure/"]
COPY ["emp-project-management-service/Directory.Build.props", "emp-project-management-service/"]

RUN dotnet restore "emp-project-management-service/src/EnterpriseMediator.ProjectManagement.WebAPI/EnterpriseMediator.ProjectManagement.WebAPI.csproj" /p:TargetFrameworks=net8.0

COPY ["emp-shared-contracts/", "emp-shared-contracts/"]
COPY ["emp-project-management-service/", "emp-project-management-service/"]

WORKDIR "/src/emp-project-management-service/src/EnterpriseMediator.ProjectManagement.WebAPI"
RUN dotnet build "EnterpriseMediator.ProjectManagement.WebAPI.csproj" -c $BUILD_CONFIGURATION /p:TargetFrameworks=net8.0 -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EnterpriseMediator.ProjectManagement.WebAPI.csproj" -c $BUILD_CONFIGURATION /p:TargetFrameworks=net8.0 -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EnterpriseMediator.ProjectManagement.WebAPI.dll"]
