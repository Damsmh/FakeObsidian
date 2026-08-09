# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FakeObsidian/FakeObsidian.Api.csproj", "FakeObsidian/"]
COPY ["FakeObsidian.Application/FakeObsidian.Application.csproj", "FakeObsidian.Application/"]
COPY ["FakeObsidian.Domain/FakeObsidian.Domain.csproj", "FakeObsidian.Domain/"]
COPY ["FakeObsidian.Infrastructure/FakeObsidian.Infrastructure.csproj", "FakeObsidian.Infrastructure/"]
RUN dotnet restore "./FakeObsidian/FakeObsidian.Api.csproj"
COPY . .
WORKDIR "/src/FakeObsidian"
RUN dotnet build "./FakeObsidian.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FakeObsidian.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FakeObsidian.Api.dll"]