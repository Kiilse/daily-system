# Gateway.Bff image (ADR-004: built once, deployed to every environment). Build context: src/backend.
# Both base images are pinned by digest; Dependabot proposes the updates.

FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:e70cdb7f80b0348f5cb85f19a8f670fca061f033d57eed12fa003d58b0e06317 AS build
WORKDIR /src

# Restore layer: only the files that decide the dependencies. Docker reuses this layer from its
# cache until one of them changes, so a code-only change skips the restore.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY BuildingBlocks/SharedKernel/*.csproj BuildingBlocks/SharedKernel/packages.lock.json BuildingBlocks/SharedKernel/
COPY BuildingBlocks/Web/*.csproj BuildingBlocks/Web/packages.lock.json BuildingBlocks/Web/
COPY Gateway.Bff/*.csproj Gateway.Bff/packages.lock.json Gateway.Bff/
RUN dotnet restore Gateway.Bff/Gateway.Bff.csproj --locked-mode

COPY . .
RUN dotnet publish Gateway.Bff/Gateway.Bff.csproj --configuration Release --no-restore --output /app -p:UseAppHost=false

# Runtime: chiseled Ubuntu, no shell, no package manager, runs as the non-root "app" user.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled@sha256:48e51f2f6798897be7ac4e775c049ed8fe60d3190f637e1f9c9dc7513efa659c
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Gateway.Bff.dll"]
