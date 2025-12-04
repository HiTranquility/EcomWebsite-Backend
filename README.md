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
    docker compose --profile App --env-file .env.dev -f docker-compose.yml -f docker-compose.dev.yml up --build -d