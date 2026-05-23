FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV DOCKER_BUILD=true
WORKDIR /src

COPY ["Directory.Build.props", "./"]

COPY ["emp-shared-contracts/src/EnterpriseMediator.Contracts/EnterpriseMediator.Contracts.csproj", "emp-shared-contracts/src/EnterpriseMediator.Contracts/"]
COPY ["emp-ai-processing-worker/src/EnterpriseMediator.AiWorker/EnterpriseMediator.AiWorker.csproj", "emp-ai-processing-worker/src/EnterpriseMediator.AiWorker/"]

RUN dotnet restore "emp-ai-processing-worker/src/EnterpriseMediator.AiWorker/EnterpriseMediator.AiWorker.csproj" /p:TargetFrameworks=net8.0

COPY ["emp-shared-contracts/", "emp-shared-contracts/"]
COPY ["emp-ai-processing-worker/", "emp-ai-processing-worker/"]

WORKDIR "/src/emp-ai-processing-worker/src/EnterpriseMediator.AiWorker"
RUN dotnet build "EnterpriseMediator.AiWorker.csproj" -c $BUILD_CONFIGURATION /p:TargetFrameworks=net8.0 -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EnterpriseMediator.AiWorker.csproj" -c $BUILD_CONFIGURATION /p:TargetFrameworks=net8.0 -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnterpriseMediator.AiWorker.dll"]
