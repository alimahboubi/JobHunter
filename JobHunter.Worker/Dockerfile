FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JobHunter.Worker/JobHunter.Worker.csproj", "JobHunter.Worker/"]
COPY ["JobHunter.Infrastructure.Persistent.Postgres/JobHunter.Infrastructure.Persistent.Postgres.csproj", "JobHunter.Infrastructure.Persistent.Postgres/"]
COPY ["JobHunter.Domain/JobHunter.Domain.csproj", "JobHunter.Domain/"]
COPY ["JobHunter.Infrastructure.Linkedin/JobHunter.Infrastructure.Linkedin.csproj", "JobHunter.Infrastructure.Linkedin/"]
COPY ["JobHunter.Framework.Observability/JobHunter.Framework.Observability.csproj", "JobHunter.Framework.Observability/"]
COPY ["JobHunter.Application.Abstraction/JobHunter.Application.Abstraction.csproj", "JobHunter.Application.Abstraction/"]
COPY ["JobHunter.Application/JobHunter.Application.csproj", "JobHunter.Application/"]
COPY ["JobHunter.Infrastructure.Cache.InMemory/JobHunter.Infrastructure.Cache.InMemory.csproj", "JobHunter.Infrastructure.Cache.InMemory/"]
RUN dotnet restore "JobHunter.Worker/JobHunter.Worker.csproj"
COPY . .
WORKDIR "/src/JobHunter.Worker"
RUN dotnet build "JobHunter.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "JobHunter.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobHunter.Worker.dll"]
