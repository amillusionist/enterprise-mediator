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

COPY ["emp-financial-service/src/EnterpriseMediator.Financial.Web.API/EnterpriseMediator.Financial.Web.API.csproj", "emp-financial-service/src/EnterpriseMediator.Financial.Web.API/"]
COPY ["emp-financial-service/src/EnterpriseMediator.Financial.Infrastructure/EnterpriseMediator.Financial.Infrastructure.csproj", "emp-financial-service/src/EnterpriseMediator.Financial.Infrastructure/"]
COPY ["emp-financial-service/src/EnterpriseMediator.Financial.Application/EnterpriseMediator.Financial.Application.csproj", "emp-financial-service/src/EnterpriseMediator.Financial.Application/"]
COPY ["emp-financial-service/src/EnterpriseMediator.Financial.Domain/EnterpriseMediator.Financial.Domain.csproj", "emp-financial-service/src/EnterpriseMediator.Financial.Domain/"]

RUN dotnet restore "emp-financial-service/src/EnterpriseMediator.Financial.Web.API/EnterpriseMediator.Financial.Web.API.csproj" /p:TargetFrameworks=net8.0

COPY ["emp-shared-contracts/", "emp-shared-contracts/"]
COPY ["emp-financial-service/", "emp-financial-service/"]

WORKDIR "/src/emp-financial-service/src/EnterpriseMediator.Financial.Web.API"
RUN dotnet build "EnterpriseMediator.Financial.Web.API.csproj" -c $BUILD_CONFIGURATION /p:TargetFrameworks=net8.0 -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EnterpriseMediator.Financial.Web.API.csproj" -c $BUILD_CONFIGURATION /p:TargetFrameworks=net8.0 -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnterpriseMediator.Financial.Web.API.dll"]
