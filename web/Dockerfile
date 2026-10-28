# Build from the repository root: docker build -f web/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props ./
COPY web/src/HexCatalog.Admin.Domain/HexCatalog.Admin.Domain.csproj web/src/HexCatalog.Admin.Domain/
COPY web/src/HexCatalog.Admin.Application/HexCatalog.Admin.Application.csproj web/src/HexCatalog.Admin.Application/
COPY web/src/HexCatalog.Admin.Infrastructure/HexCatalog.Admin.Infrastructure.csproj web/src/HexCatalog.Admin.Infrastructure/
COPY web/src/HexCatalog.Admin/HexCatalog.Admin.csproj web/src/HexCatalog.Admin/
RUN dotnet restore web/src/HexCatalog.Admin/HexCatalog.Admin.csproj
COPY web/src web/src
# Restore again with the sources present: the Blazor framework scripts are only pulled in once .razor files exist.
RUN dotnet publish web/src/HexCatalog.Admin/HexCatalog.Admin.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
# Data protection keys live in a volume so sign-in cookies survive restarts.
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys && chown -R $APP_UID /home/app/.aspnet
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "HexCatalog.Admin.dll"]
