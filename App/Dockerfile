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

# Copy csproj riêng biệt để tận dụng cache restore
COPY App/App.csproj App/
COPY App.BLL/App.BLL.csproj App.BLL/
COPY App.DAL/App.DAL.csproj App.DAL/
COPY App.UTIL/App.UTIL.csproj App.UTIL/

# Restore dependencies cho solution
RUN dotnet restore App/App.csproj

# Copy toàn bộ source code sau khi restore xong
COPY . .

# Build solution
WORKDIR /src/App
RUN dotnet build -c Release -o /app/build

# ============================
# 3️⃣ Publish image
# ============================
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ============================
# 4️⃣ Final runtime image
# ============================
FROM base AS final
WORKDIR /app

# Cài CA certs nếu dùng HTTPS / HttpClient
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl openssl ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copy publish output sang final image
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "App.dll"]
