# Docker Compose Configuration Documentation

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [File Structure](#file-structure)
- [Environment Configuration](#environment-configuration)
- [Services](#services)
- [Usage](#usage)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

This project uses a **multi-file Docker Compose** architecture with a base configuration file and environment-specific override files. This approach follows the **DRY (Don't Repeat Yourself)** principle and makes it easy to maintain configurations across different environments.

### Key Benefits

- ✅ **Single Source of Truth**: Base configuration in `docker-compose.yml`
- ✅ **Environment-Specific Overrides**: Minimal, focused override files
- ✅ **Easy Maintenance**: Change base config once, applies everywhere
- ✅ **Clear Separation**: Easy to understand what's different per environment
- ✅ **Flexible**: Easy to add new environments

---

## Architecture

### Base + Override Pattern

```
docker-compose.yml          → Base configuration (shared)
├── docker-compose.dev.yml  → Development overrides
├── docker-compose.stag.yml → Staging overrides
└── docker-compose.prod.yml → Production overrides
```

### How It Works

1. **Base File** (`docker-compose.yml`):
   - Contains all shared configuration
   - Defines services, networks, volumes
   - Includes build configs, healthchecks, dependencies
   - **No environment-specific settings** (ports, env_file, volumes for certs)

2. **Override Files** (`.dev.yml`, `.stag.yml`, `.prod.yml`):
   - Only contain environment-specific overrides
   - Override: `env_file`, `ports`, `volumes`, `environment` variables
   - Can add/remove: security settings, resource limits, profiles

3. **Docker Compose Merging**:
   - Docker Compose automatically merges base + override files
   - Override files take precedence over base file
   - Arrays are merged (e.g., `ports`, `volumes`)

---

## File Structure

### `docker-compose.yml` (Base Configuration)

**Purpose**: Shared configuration for all environments

**Contains**:
- Service definitions (api, mysql, redis, rabbitmq, aspire-dashboard)
- Build configurations
- Health checks
- Dependencies (`depends_on`)
- Networks configuration
- Volumes definitions
- Profiles configuration

**Does NOT contain**:
- ❌ Environment-specific `env_file` paths
- ❌ Port mappings (varies by environment)
- ❌ Certificate volume mounts (varies by environment)
- ❌ Environment variables (varies by environment)

### `docker-compose.dev.yml` (Development Overrides)

**Purpose**: Development environment specific settings

**Overrides**:
- ✅ `env_file: .env.dev` for all services
- ✅ Exposes all ports for local development access
- ✅ Mounts development certificates (`./certs/dev`)
- ✅ Enables Aspire Dashboard
- ✅ Development-specific environment variables

**Ports Exposed**:
- API: `5095:8080` (HTTP), `5096:8443` (HTTPS)
- MySQL: `3308:3306`
- Redis: `6380:6379`
- RabbitMQ: `5673:5672` (AMQP), `15672:15672` (Management UI)
- Aspire Dashboard: `18888:18888`

### `docker-compose.stag.yml` (Staging Overrides)

**Purpose**: Staging environment specific settings

**Overrides**:
- ✅ `env_file: .env.stag` for all services
- ✅ Exposes API ports only (5095/5096)
- ✅ Mounts staging certificates (`./certs/staging`)
- ✅ Hides database/cache ports (security)
- ✅ Disables Aspire Dashboard

**Ports Exposed**:
- API: `5095:8080` (HTTP), `5096:8443` (HTTPS)
- MySQL, Redis, RabbitMQ: **Not exposed** (internal network only)

### `docker-compose.prod.yml` (Production Overrides)

**Purpose**: Production environment specific settings with security hardening

**Overrides**:
- ✅ `env_file: .env.prod` for all services
- ✅ Standard HTTP/HTTPS ports (80/443)
- ✅ Mounts production certificates (`./certs/prod`)
- ✅ Security hardening:
  - `read_only: true` (read-only filesystem)
  - `security_opt: no-new-privileges:true`
  - Resource limits (CPU: 1.0, Memory: 512M)
- ✅ Hides all database/cache ports
- ✅ Disables Aspire Dashboard

**Ports Exposed**:
- API: `80:8080` (HTTP), `443:8443` (HTTPS)
- MySQL, Redis, RabbitMQ: **Not exposed** (internal network only)

---

## Environment Configuration

### Environment Files

Each environment requires a corresponding `.env` file:

| Environment | Env File | Compose Override |
|-------------|----------|------------------|
| Development | `.env.dev` | `docker-compose.dev.yml` |
| Staging | `.env.stag` | `docker-compose.stag.yml` |
| Production | `.env.prod` | `docker-compose.prod.yml` |

### Creating Environment Files

1. **Copy sample file**:
   ```bash
   cd App
   cp env.dev.sample .env.dev
   ```

2. **Edit values**:
   - Connection strings (pointing to Docker service names: `mysql`, `redis`, `rabbitmq`)
   - JWT secret keys
   - CORS allowed origins
   - HTTPS certificate passwords
   - Redis connection strings (use service name: `redis:6379`)

### Key Environment Variables

```bash
# Docker Configuration
DOCKER_PROJECT_NAME=EcomWebsite
DOCKER_COMPOSE_NETWORK=EcomNetwork
DOCKER_COMPOSE_PROFILE=App

# API Configuration
API_IMAGE_NAME=ecom-website-api
API_IMAGE_TAG=dev
API_HTTP_PORT=5095
API_HTTPS_PORT=5096

# Database Connection (use service name in Docker)
ConnectionStrings__MyUserSqlConn=server=mysql;port=3306;database=ecom_users;user=root;password=${MYSQL_ROOT_PASSWORD}

# Redis Connection (use service name in Docker)
Redis__ConnectionString=redis:6379

# RabbitMQ Configuration (use service name in Docker)
RabbitMqSettings__HostName=rabbitmq
RabbitMqSettings__Port=5672
```

**Important**: In Docker, use **service names** (`mysql`, `redis`, `rabbitmq`) instead of `localhost` for connections.

---

## Services

### API Service

**Base Configuration** (`docker-compose.yml`):
- Build context: `..` (parent directory)
- Dockerfile: `App/Dockerfile`
- Depends on: mysql, redis, rabbitmq (with health checks)
- Health check: HTTP endpoint `/health`
- Network: Shared Docker network

**Environment-Specific Overrides**:

| Setting | Dev | Staging | Production |
|---------|-----|---------|------------|
| Ports | 5095/5096 | 5095/5096 | 80/443 |
| Certificates | `./certs/dev` | `./certs/staging` | `./certs/prod` |
| Security | Basic | Medium | Hardened |
| Read-only FS | No | No | Yes |
| Resource Limits | None | None | CPU/Memory |

### MySQL Service

**Base Configuration**:
- Image: `mysql:8.4`
- Volume: `mysql-data:/var/lib/mysql`
- Health check: `mysqladmin ping`
- Network: Shared Docker network

**Environment-Specific**:
- **Dev**: Port `3308:3306` exposed
- **Staging/Prod**: Ports hidden (internal network only)

### Redis Service

**Base Configuration**:
- Image: `redis:latest`
- Volume: `redis-data:/data`
- Health check: `redis-cli ping`
- Network: Shared Docker network

**Environment-Specific**:
- **Dev**: Port `6380:6379` exposed
- **Staging/Prod**: Ports hidden (internal network only)

### RabbitMQ Service

**Base Configuration**:
- Image: `rabbitmq:management`
- Volumes: `rabbitmq-data`, `rabbitmq-logs`
- Health check: `rabbitmq-diagnostics ping`
- Network: Shared Docker network

**Environment-Specific**:
- **Dev**: Ports `5673:5672` (AMQP) and `15672:15672` (Management UI) exposed
- **Staging/Prod**: Ports hidden (internal network only)

### Aspire Dashboard Service

**Base Configuration**:
- Image: `mcr.microsoft.com/dotnet/nightly/aspire-dashboard`
- Health check: HTTP endpoint
- Network: Shared Docker network

**Environment-Specific**:
- **Dev**: Enabled on port `18888`
- **Staging/Prod**: Disabled (`profiles: never`)

---

## Usage

### Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- PowerShell 7+ (Windows) or Bash (Linux/macOS/WSL)
- Environment files (`.env.dev`, `.env.stag`, `.env.prod`)

### Using Scripts (Recommended)

#### Windows (PowerShell)

```powershell
cd App

# Development
./scripts/compose-dev.ps1            # Start services (up -d)
./scripts/compose-dev.ps1 logs -f api # View logs
./scripts/compose-dev.ps1 down -v     # Stop and remove volumes

# Staging
./scripts/compose-stag.ps1            # Start services
./scripts/compose-stag.ps1 ps         # View status

# Production
./scripts/compose-prod.ps1            # Start services
./scripts/compose-prod.ps1 pull       # Pull latest images
```

#### Linux/macOS/WSL (Bash)

```bash
cd App
chmod +x ./scripts/compose-*.sh

# Development
./scripts/compose-dev.sh              # Start services
./scripts/compose-dev.sh logs -f api  # View logs
./scripts/compose-dev.sh down -v      # Stop and remove volumes

# Staging
./scripts/compose-stag.sh             # Start services
./scripts/compose-stag.sh ps          # View status

# Production
./scripts/compose-prod.sh             # Start services
./scripts/compose-prod.sh pull        # Pull latest images
```

### Manual Commands

#### Development

```bash
cd App

# Start services
docker compose \
  --env-file .env.dev \
  -f docker-compose.yml \
  -f docker-compose.dev.yml \
  --profile App --profile Api --profile Database --profile Cache --profile Message --profile Dashboard \
  up -d --build

# View logs
docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml logs -f api

# Stop services
docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml down

# Stop and remove volumes
docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml down -v
```

#### Staging

```bash
cd App

# Start services
docker compose \
  --env-file .env.stag \
  -f docker-compose.yml \
  -f docker-compose.stag.yml \
  --profile App --profile Api --profile Database --profile Cache --profile Message \
  up -d

# View status
docker compose --env-file .env.stag -f docker-compose.yml -f docker-compose.stag.yml ps
```

#### Production

```bash
cd App

# Start services
docker compose \
  --env-file .env.prod \
  -f docker-compose.yml \
  -f docker-compose.prod.yml \
  --profile App --profile Api --profile Database --profile Cache --profile Message \
  up -d

# Pull latest images
docker compose --env-file .env.prod -f docker-compose.yml -f docker-compose.prod.yml pull
```

### Common Commands

```bash
# View running services
docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml ps

# View logs
docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml logs -f

# Restart a service
docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml restart api

# Execute command in container
docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml exec api bash

# View resource usage
docker stats

# Clean up (remove stopped containers, unused networks, images)
docker system prune -a
```

---

## Best Practices

### 1. Environment Separation

- ✅ **Always use environment-specific files**: Never mix dev/staging/prod configs
- ✅ **Use service names in connections**: `mysql`, `redis`, `rabbitmq` (not `localhost`)
- ✅ **Separate certificates**: Different certs for each environment

### 2. Security

- ✅ **Hide database ports in staging/prod**: Use `ports: []` for MySQL, Redis, RabbitMQ
- ✅ **Use read-only filesystem in production**: `read_only: true`
- ✅ **Set resource limits in production**: Prevent resource exhaustion
- ✅ **Disable Aspire Dashboard in production**: Use external monitoring tools

### 3. Development

- ✅ **Expose all ports in dev**: Easy local development and debugging
- ✅ **Enable Aspire Dashboard in dev**: Useful for observability during development
- ✅ **Use development certificates**: Separate from production certs

### 4. Maintenance

- ✅ **Update base file for shared changes**: Changes apply to all environments
- ✅ **Update override files for environment-specific changes**: Minimal, focused changes
- ✅ **Document environment-specific requirements**: Clear comments in override files

### 5. Networking

- ✅ **Use Docker networks**: Services communicate via internal network
- ✅ **Service discovery**: Use service names for inter-service communication
- ✅ **Port mapping**: Only expose necessary ports to host

---

## Troubleshooting

### Common Issues

#### 1. Port Already in Use

**Error**: `Bind for 0.0.0.0:5095 failed: port is already allocated`

**Solution**:
```bash
# Find process using port
netstat -ano | findstr :5095  # Windows
lsof -i :5095                 # Linux/macOS

# Change port in .env.dev file
API_HTTP_PORT=5097
```

#### 2. Connection Refused to Database

**Error**: `Unable to connect to MySQL`

**Solution**:
- Check service name in connection string: Use `mysql` (not `localhost`)
- Verify service is running: `docker compose ps`
- Check health check: Wait for service to be healthy
- Verify network: Services must be on same Docker network

#### 3. Certificate Not Found

**Error**: `Certificate file not found: /https/devcert.pfx`

**Solution**:
```bash
# Ensure certificate directory exists
mkdir -p certs/dev

# Place certificate file
cp your-cert.pfx certs/dev/devcert.pfx

# Set password in .env.dev
ASPNETCORE_HTTPS_PASSWORD=your-password
```

#### 4. Environment Variables Not Loading

**Error**: Variables from `.env.dev` not being used

**Solution**:
- Verify `--env-file .env.dev` is specified
- Check `env_file` in override file matches
- Ensure variable format: `KEY=value` (no spaces around `=`)
- Use double underscore for nested config: `Redis__ConnectionString`

#### 5. Build Fails

**Error**: `ERROR: failed to solve: failed to compute cache key`

**Solution**:
```bash
# Clean build cache
docker builder prune -a

# Rebuild without cache
docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml build --no-cache
```

#### 6. Services Not Starting

**Error**: Services exit immediately

**Solution**:
```bash
# Check logs
docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml logs

# Check health status
docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml ps

# Verify dependencies
docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml config
```

### Debugging Tips

1. **Validate Compose Files**:
   ```bash
   docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml config
   ```

2. **Check Service Logs**:
   ```bash
   docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml logs -f api
   ```

3. **Inspect Container**:
   ```bash
   docker compose --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml exec api bash
   ```

4. **View Network**:
   ```bash
   docker network ls
   docker network inspect ecomnetwork
   ```

5. **Check Volumes**:
   ```bash
   docker volume ls
   docker volume inspect app_mysql-data
   ```

---

## Environment Comparison

| Feature | Development | Staging | Production |
|---------|-------------|---------|------------|
| **API Ports** | 5095/5096 | 5095/5096 | 80/443 |
| **DB/Cache Ports** | Exposed | Hidden | Hidden |
| **Certificates** | `./certs/dev` | `./certs/staging` | `./certs/prod` |
| **Security** | Basic | Medium | Hardened |
| **Resource Limits** | None | None | CPU/Memory |
| **Read-only FS** | No | No | Yes |
| **Aspire Dashboard** | Enabled | Disabled | Disabled |
| **Use Case** | Local development | Pre-production testing | Live production |

---

## Additional Resources

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Docker Networking](https://docs.docker.com/network/)
- [Docker Security Best Practices](https://docs.docker.com/engine/security/)
- [ASP.NET Core Docker Documentation](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)

---

## Support

For issues or questions:
1. Check this documentation
2. Review Docker Compose logs
3. Validate configuration files
4. Check environment variables

---

**Last Updated**: 2025-01-XX
**Maintained By**: EcomWebsite Team

