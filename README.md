# EcomWebsite Backend

## Project Structure

```
EcomWebsite-Backend/
├── App/                    # Web API (Presentation Layer - Entry Point)
├── App.BLL/                # Business Logic Layer (Services, DTOs, Validators)
├── App.DAL/                # Data Access Layer (DbContexts, Repositories, Entities)
├── App.INFRA/              # Infrastructure Layer (Caching, Email, Messaging, Auth)
├── App.UTIL/               # Shared Utilities (Base classes, Extensions, Helpers)
└── App.TEST/               # Unit & Integration Tests
```

### Project References

```
App (Entry Point)
 ├── App.BLL
 ├── App.UTIL
 └── App.INFRA

App.BLL (Business Logic)
 ├── App.DAL
 ├── App.UTIL
 └── App.INFRA

App.DAL (Data Access)
 └── App.UTIL

App.INFRA (External Services)
 └── App.UTIL

App.UTIL (Base Layer)
 └── (no dependencies)
```

### Layer Responsibilities

| Layer | Purpose |
|-------|---------|
| **App** | Controllers, Middlewares, DI Configuration, Program.cs |
| **App.BLL** | Services (IProductSvc, IAuthSvc...), DTOs, AutoMapper Profiles, Validators |
| **App.DAL** | EF Core DbContexts, Repositories, Entity Models, Data Seeding |
| **App.INFRA** | Caching (Redis/Memory), JWT Token, Email (SMTP), Messaging (RabbitMQ/Kafka), OAuth (Google/Facebook) |
| **App.UTIL** | GenericRepo, GenericSvc, Extensions, Shared Configs |

---

## Docker Compose

> 📖 **For detailed documentation**, see [DOCKER_COMPOSE.md](./App/DOCKER_COMPOSE.md)

### Docker Compose Files

| File | Purpose |
|------|---------|
| `docker-compose.yml` | Base configuration (shared across all environments) |
| `docker-compose.build.yml` | Build configuration only (for building images) |
| `docker-compose.dev.yml` | Development environment overrides |
| `docker-compose.stag.yml` | Staging environment overrides |
| `docker-compose.prod.yml` | Production environment overrides |

### Prerequisites
- Docker Desktop (or Docker Engine)
- PowerShell 7+ on Windows, or Bash on WSL/macOS/Linux

---

## Quick Start

### 1. Setup Environment Files
```bash
cd App
cp env.sample .env
cp env.dev.sample .env.dev
cp env.sample .env.stag
cp env.sample .env.prod
```

Edit values per environment:
- Connection strings (`ConnectionStrings__*`)
- `JwtSettings__SecretKey`, `Cors__AllowedOrigins`
- HTTPS certificates (see Certificates section)

---

## Build & Push Image

### Build Image
```powershell
cd App

# Using docker-compose (recommended)
docker compose --profile Build -f docker-compose.yml -f docker-compose.build.yml build api

# Or using docker build directly (from EcomWebsite-Backend/ directory)
cd ..
docker build -t ecom-api:latest -f App/Dockerfile .
```

### Tag Image
```powershell
# For Docker Hub
docker tag ecom-api:latest hitranquility/ecom-api:v1.0.0
docker tag ecom-api:latest hitranquility/ecom-api:staging

# For GitHub Container Registry (GHCR)
docker tag ecom-api:latest ghcr.io/hitranquility/ecom-website-api:v1.0.0
docker tag ecom-api:latest ghcr.io/hitranquility/ecom-website-api:staging
```

### Login to Registry
```powershell
# Docker Hub
docker login -u hitranquility
# Enter password when prompted

# Or using token
$env:DOCKERHUB_TOKEN = "dckr_pat_xxxxxxxxxxxxx"
echo $env:DOCKERHUB_TOKEN | docker login -u hitranquility --password-stdin

# GitHub Container Registry
echo $env:GITHUB_TOKEN | docker login ghcr.io -u $env:GITHUB_USERNAME --password-stdin
```

### Push Image
```powershell
# Docker Hub
docker push hitranquility/ecom-api:v1.0.0
docker push hitranquility/ecom-api:staging

# GHCR
docker push ghcr.io/hitranquility/ecom-website-api:v1.0.0
docker push ghcr.io/hitranquility/ecom-website-api:staging
```

---

## Run Services

### Using Scripts (Recommended)

**Windows (PowerShell):**
```powershell
cd App

# Development
./scripts/compose-dev.ps1                # up -d
./scripts/compose-dev.ps1 logs -f api    # view logs
./scripts/compose-dev.ps1 down -v        # stop & remove

# Staging
./scripts/compose-stag.ps1               # up -d
./scripts/compose-stag.ps1 ps            # list containers

# Production
./scripts/compose-prod.ps1               # up -d
./scripts/compose-prod.ps1 pull          # pull latest images
```

> **Note:** If PowerShell blocks script execution:
> ```powershell
> Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
> ```

**Bash (WSL/macOS/Linux):**
```bash
cd App
chmod +x ./scripts/compose-*.sh

# Development
./scripts/compose-dev.sh                 # up -d
./scripts/compose-dev.sh logs -f api     # view logs
./scripts/compose-dev.sh down -v         # stop & remove

# Staging / Production
./scripts/compose-stag.sh
./scripts/compose-prod.sh
```

### Using Docker Compose Directly

```powershell
cd App

# Development
docker compose --profile App --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml up -d
docker compose --profile App --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml down -v

# Staging
docker compose --profile App --env-file .env.stag -f docker-compose.yml -f docker-compose.stag.yml up -d

# Production
docker compose --profile App --env-file .env.prod -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

## Deploy to EC2 (Staging)

```bash
cd ~/EcomWebsite-Backend/App

# Login to Docker Hub (required)
docker login -u hitranquility

# Pull latest images
docker compose --profile App -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag pull

# Stop old containers
docker compose --profile App -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag down

# Start new containers
# ⚠️ Make sure image name is hitranquility/ecom-api in .env.stag
docker compose --profile App -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag up -d

# Verify
docker compose --profile App -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag ps
```

---

## Common Operations

```powershell
cd App

# Rebuild only API (no dependencies)
./scripts/compose-dev.ps1 up -d --no-deps api

# View logs
./scripts/compose-dev.ps1 logs -f
./scripts/compose-stag.ps1 logs -f api

# Restart a service
./scripts/compose-prod.ps1 restart api

# Remove stack and volumes
./scripts/compose-dev.ps1 down -v

# Clean up Docker (remove unused containers, networks, images)
docker system prune -f
```

---

## Certificates

| Environment | Path | Password Env |
|-------------|------|--------------|
| Development | `App/certs/dev/devcert.pfx` | `ASPNETCORE_HTTPS_PASSWORD` |
| Staging | `App/certs/staging/stagingcert.pfx` | `ASPNETCORE_HTTPS_PASSWORD` |
| Production | `App/certs/prod/prodcert.pfx` | `ASPNETCORE_HTTPS_PASSWORD` |

---

## Local Development (Without Docker)

Connection strings for local MySQL (port 3306):
```json
{
  "MyUserSqlConn": "Server=localhost;Port=3306;Database=ecom_users;User=root;Password=CaVN2004",
  "MyBlogSqlConn": "Server=localhost;Port=3306;Database=ecom_blogs;User=root;Password=CaVN2004",
  "MyProductSqlConn": "Server=localhost;Port=3306;Database=ecom_products;User=root;Password=CaVN2004",
  "MyOrderSqlConn": "Server=localhost;Port=3306;Database=ecom_orders;User=root;Password=CaVN2004"
}
```

---

## Troubleshooting

- **Scripts not running:** Ensure you're in the `App/` directory
- **Missing env files:** Make sure `.env.dev` / `.env.stag` / `.env.prod` exist
- **Port conflicts:** Change host ports in env file (`API_HTTP_PORT`, `MYSQL_HTTP_PORT`, etc.)
- **Image not found on EC2:** Ensure you're logged into Docker Hub and image name is correct (`hitranquility/ecom-api`)
## Project Structure

```
EcomWebsite-Backend/
├── App/                    # Web API (Presentation Layer - Entry Point)
├── App.BLL/                # Business Logic Layer (Services, DTOs, Validators)
├── App.DAL/                # Data Access Layer (DbContexts, Repositories, Entities)
├── App.INFRA/              # Infrastructure Layer (Caching, Email, Messaging, Auth)
├── App.UTIL/               # Shared Utilities (Base classes, Extensions, Helpers)
└── App.TEST/               # Unit & Integration Tests
```

### Project References

```
App (Entry Point)
 ├── App.BLL
 ├── App.UTIL
 └── App.INFRA

App.BLL (Business Logic)
 ├── App.DAL
 ├── App.UTIL
 └── App.INFRA

App.DAL (Data Access)
 └── App.UTIL

App.INFRA (External Services)
 └── App.UTIL

App.UTIL (Base Layer)
 └── (no dependencies)
```

### Layer Responsibilities

| Layer | Purpose |
|-------|---------|
| **App** | Controllers, Middlewares, DI Configuration, Program.cs |
| **App.BLL** | Services (IProductSvc, IAuthSvc...), DTOs, AutoMapper Profiles, Validators |
| **App.DAL** | EF Core DbContexts, Repositories, Entity Models, Data Seeding |
| **App.INFRA** | Caching (Redis/Memory), JWT Token, Email (SMTP), Messaging (RabbitMQ/Kafka), OAuth (Google/Facebook) |
| **App.UTIL** | GenericRepo, GenericSvc, Extensions, Shared Configs |

---

## Compose scripts usage (Dev / Staging / Prod)

> 📖 **For detailed documentation**, see [DOCKER_COMPOSE.md](./App/DOCKER_COMPOSE.md)

### Prerequisites
- Docker Desktop (or Docker Engine)
- PowerShell 7+ on Windows, or Bash on WSL/macOS/Linux

### Environment files
1) Copy the samples to each environment file under `App/`:
```bash
cd App
cp env.sample .env
cp env.dev.sample .env.dev
cp env.sample .env.stag
cp env.sample .env.prod
```
2) Edit values per environment:
- Connection strings (`ConnectionStrings__*`) pointing to DB hosts
- `JwtSettings__SecretKey`, `Cors__AllowedOrigins`
- HTTPS certificate & password:
  - Dev: place `App/certs/dev/devcert.pfx` (set `ASPNETCORE_HTTPS_PASSWORD` if the PFX is protected)
  - Staging: `App/certs/staging/stagingcert.pfx`
  - Prod: `App/certs/prod/prodcert.pfx`

### Windows (PowerShell)
Run from the `App/` directory:
```powershell
# Development
./scripts/compose-dev.ps1            # defaults to: up -d
./scripts/compose-dev.ps1 logs -f api
./scripts/compose-dev.ps1 down -v

# Staging
./scripts/compose-stag.ps1           # up -d
./scripts/compose-stag.ps1 ps

# Production
./scripts/compose-prod.ps1           # up -d
./scripts/compose-prod.ps1 pull
```
Note: If PowerShell blocks script execution, run in the current session:
```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
```

### Bash (WSL/macOS/Linux)
Run from the `App/` directory:
```bash
chmod +x ./scripts/compose-*.sh

# Development
./scripts/compose-dev.sh             # up -d
./scripts/compose-dev.sh logs -f api
./scripts/compose-dev.sh down -v

# Staging
./scripts/compose-stag.sh            # up -d
./scripts/compose-stag.sh ps

# Production
./scripts/compose-prod.sh            # up -d
./scripts/compose-prod.sh pull
```

### What the scripts do
- Load the right env file: `.env.dev` / `.env.stag` / `.env.prod`
- Use base + override compose files:
  - `docker-compose.yml` + `docker-compose.dev.yml`
  - `docker-compose.yml` + `docker-compose.stag.yml`
  - `docker-compose.yml` + `docker-compose.prod.yml`
- Enable relevant profiles; Dev also enables `Dashboard` (Aspire)
- Forward any extra arguments to `docker compose`

### Common operations (examples)
```powershell
# Recreate only API (no dependencies), rebuild image
./scripts/compose-dev.ps1 up -d --no-deps --build api

# Tail logs of all services
./scripts/compose-stag.ps1 logs -f

# Restart a service
./scripts/compose-prod.ps1 restart api

# Remove stack and volumes
./scripts/compose-dev.ps1 down -v
```

### Certificates (Staging/Prod)
- Place PFX certs in:
  - `App/certs/staging/stagingcert.pfx`
  - `App/certs/prod/prodcert.pfx`
- Set `ASPNETCORE_HTTPS_PASSWORD` in `.env.stag` / `.env.prod`

### Troubleshooting
- Ensure the correct working directory: run scripts from `App/`
- Make sure `.env.dev` / `.env.stag` / `.env.prod` exist
- If ports are busy, change the host ports in the corresponding env file (`API_HTTP_PORT`, `API_HTTPS_PORT`, etc.)

### Development Database .ENV (Not used for Docker)
    "MyUserSqlConn": "Server=localhost;Port=3306;Database=ecom_users;User=root;Password=CaVN2004",
    "MyBlogSqlConn": "Server=localhost;Port=3306;Database=ecom_blogs;User=root;Password=CaVN2004",
    "MyProductSqlConn": "Server=localhost;Port=3306;Database=ecom_products;User=root;Password=CaVN2004",
    "MyOrderSqlConn": "Server=localhost;Port=3306;Database=ecom_orders;User=root;Password=CaVN2004"

    //Down
    docker compose --profile App --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml down -v

    //Up
    docker compose --profile App --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml build --no-cache api

  # Từ PowerShell, ở thư mục EcomWebsite-Backend/
  cd C:\Users\Admin\Downloads\EcomWebsitev5\EcomWebsite-Backend

  # Build image
  docker build -t ghcr.io/hitranquility/ecom-website-api:v1 -f App/Dockerfile .

  # Hoặc nếu muốn tag staging
  docker build -t ghcr.io/hitranquility/ecom-website-api:staging -f App/Dockerfile .

  # Login vào GHCR
  echo $env:GITHUB_TOKEN | docker login ghcr.io -u $env:GITHUB_USERNAME --password-stdin

  # Push image
  docker push ghcr.io/hitranquility/ecom-website-api:v1
  docker push ghcr.io/hitranquility/ecom-website-api:staging

  # or docker push tag only

  # 1. Login vào Docker Hub
  docker login -u hitranquility
  # Nhập password khi được hỏi

  # Hoặc dùng token (nếu có)
  $env:DOCKERHUB_TOKEN = "dckr_pat_xxxxxxxxxxxxx"
  echo $env:DOCKERHUB_TOKEN | docker login -u hitranquility --password-stdin

  # 2. Tag lại image với tên Docker Hub (format: username/repo:tag)
  docker tag ecom-api:dev hitranquility/ecom-api:v1
  docker tag hitranquility/ecom-api:v1 hitranquility/ecom-api:staging
  docker tag hitranquility/ecom-api:v1 hitranquility/ecom-api:staging-latest

  # 3. Push lên Docker Hub
  docker push hitranquility/ecom-api:v1
  docker push hitranquility/ecom-api:staging
  docker push hitranquility/ecom-api:staging-latest

    docker compose --profile App --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml up --build -d
    or (nếu đã xây rồi)
    docker compose --profile App --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml up -d

    docker compose --profile App --env-file .env.stag -f docker-compose.yml -f docker-compose.stag.yml up --build -d


# Trên EC2
cd ~/EcomWebsite-Backend/App

# Pull images mới
docker compose --profile App -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag pull

# Stop containers cũ (nếu có)
docker compose --profile App -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag down

# Start containers mới
Cái này nhớ docker login và chỉnh tên iamge là hitranquility/ecom-api (ko nó lỗi liên tục)
docker compose --profile App -f  docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag up -d

# Verify
docker compose --profile App -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag ps

# 1. Stop và remove containers hoàn toàn
docker-compose -f docker-compose.yml -f docker-compose.stag.yml down -v

# 2. Xóa cache network/containers cũ (optional)
docker system prune -f



docker compose --profile Build -f docker-compose.yml -f docker-compose.build.yml build --no-cache api or
docker build -t ecom-api:image -f Dockerfile .. --no-cache

xong rồi thì
docker tag ecom-api:image hitranquility/ecom-api:stag

và push
docker push hitranquility/ecom-api:stag