FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JobHunter.WebApp/JobHunter.WebApp.csproj", "JobHunter.WebApp/"]
COPY ["JobHunter.Domain/JobHunter.Domain.csproj", "JobHunter.Domain/"]
COPY ["JobHunter.Infrastructure.Persistent.Postgres/JobHunter.Infrastructure.Persistent.Postgres.csproj", "JobHunter.Infrastructure.Persistent.Postgres/"]
COPY ["JobHunter.Application/JobHunter.Application.csproj", "JobHunter.Application/"]
RUN dotnet restore "JobHunter.WebApp/JobHunter.WebApp.csproj"
COPY . .
WORKDIR "/src/JobHunter.WebApp"
RUN dotnet build "JobHunter.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "JobHunter.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobHunter.WebApp.dll"]
