# Build from the repository root: docker build -f api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props ./
COPY api/src/HexCatalog.Domain/HexCatalog.Domain.csproj api/src/HexCatalog.Domain/
COPY api/src/HexCatalog.Application/HexCatalog.Application.csproj api/src/HexCatalog.Application/
COPY api/src/HexCatalog.Infrastructure/HexCatalog.Infrastructure.csproj api/src/HexCatalog.Infrastructure/
COPY api/src/HexCatalog.Api/HexCatalog.Api.csproj api/src/HexCatalog.Api/
RUN dotnet restore api/src/HexCatalog.Api/HexCatalog.Api.csproj
COPY api/src api/src
RUN dotnet publish api/src/HexCatalog.Api/HexCatalog.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=3s --start-period=30s --retries=10 \
  CMD ["dotnet", "HexCatalog.Api.dll", "--health-probe"]
ENTRYPOINT ["dotnet", "HexCatalog.Api.dll"]
