# ============================
# 1️⃣ Base runtime image
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

# ============================
# 2️⃣ Build image (SDK)
# ============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Environment variables
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Copy csproj riêng biệt để tận dụng cache restore
COPY App/App.csproj App/
COPY App.BLL/App.BLL.csproj App.BLL/
COPY App.DAL/App.DAL.csproj App.DAL/
COPY App.UTIL/App.UTIL.csproj App.UTIL/

# Copy toàn bộ source code (obj folders should be excluded by .dockerignore)
COPY . .

# Clean any obj folders that might have been copied (they contain Windows paths from local build)
# This is critical - obj/project.assets.json contains Windows paths that break Linux builds
# Even though .dockerignore should exclude them, we clean them explicitly to be safe
RUN find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
RUN find . -type f -name "project.assets.json" -delete 2>/dev/null || true
RUN find . -type f -name "*.nuget.g.props" -delete 2>/dev/null || true
RUN find . -type f -name "*.nuget.g.targets" -delete 2>/dev/null || true

# Restore dependencies - this will generate clean assets.json in Linux container
RUN dotnet restore App/App.csproj

# Build project - use MSBuild property to disable fallback folders
WORKDIR /src
RUN dotnet build App/App.csproj -c Release -o /app/build --no-restore

# ============================
# 3️⃣ Publish image
# ============================
FROM build AS publish
WORKDIR /src
# Publish directly (will build if needed) - không dùng --no-build vì build output ở /app/build
# Publish sẽ tự động build lại nếu cần và copy tất cả dependencies
RUN dotnet publish App/App.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

# ============================
# 4️⃣ Final runtime image
# ============================
FROM base AS final
WORKDIR /app

# Install dependencies: curl (for healthcheck), openssl, ca-certificates
# Clean up apt cache để giảm image size
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
        openssl \
        ca-certificates \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get clean \
    && curl --version || exit 1

# Tạo non-root user để chạy app (security best practice)
RUN groupadd -r appuser && useradd -r -g appuser appuser \
    && chown -R appuser:appuser /app

# Copy publish output sang final image
COPY --from=publish /app/publish .

# Set ownership cho app files
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Labels cho metadata
LABEL maintainer="EcomWebsite Team"
LABEL description="EcomWebsite API - ASP.NET Core 9.0"
LABEL version="1.0"

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Healthcheck (sẽ được override bởi docker-compose nếu cần)
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "App.dll"]
