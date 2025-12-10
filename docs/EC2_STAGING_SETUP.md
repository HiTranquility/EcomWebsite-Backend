# EC2 Staging Setup Guide

## 📋 Tóm tắt

EC2 chỉ cần có **3 files** để chạy:
- `docker-compose.yml` (base)
- `docker-compose.stag.yml` (staging overrides)
- `.env.stag` (environment variables)

**Không cần clone repo!**

---

## 🚀 Setup EC2 lần đầu

### Bước 1: SSH vào EC2

```bash
ssh -i your-key.pem ubuntu@${EC2_STAGING_HOST}
```

### Bước 2: Install Docker & Docker Compose

```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker ubuntu

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Logout và login lại
exit
ssh -i your-key.pem ubuntu@${EC2_STAGING_HOST}
```

### Bước 3: Tạo thư mục và copy files

```bash
# Tạo thư mục
mkdir -p ~/EcomWebsite-Backend/App
cd ~/EcomWebsite-Backend/App
```

**Từ máy local, copy files lên EC2:**

```bash
# Từ máy local
scp -i your-key.pem App/docker-compose.yml ubuntu@${EC2_STAGING_HOST}:~/EcomWebsite-Backend/App/
scp -i your-key.pem App/docker-compose.stag.yml ubuntu@${EC2_STAGING_HOST}:~/EcomWebsite-Backend/App/
```

### Bước 4: Tạo `.env.stag` file

```bash
# Trên EC2
cd ~/EcomWebsite-Backend/App
nano .env.stag
```

**Nội dung `.env.stag` (ví dụ):**

```bash
# ============================================
# Docker Configuration
# ============================================
DOCKER_PROJECT_NAME=EcomWebsite-Staging
DOCKER_COMPOSE_NETWORK=EcomNetwork-Staging

# ============================================
# API Configuration
# ============================================
API_IMAGE_NAME=ghcr.io/your-org/ecom-website-backend
API_IMAGE_TAG=staging-latest
API_HTTP_PORT=5095
API_HTTPS_PORT=5096
API_CONTAINER_HTTP_PORT=8080
API_CONTAINER_HTTPS_PORT=8443

# ============================================
# MySQL Configuration
# ============================================
MYSQL_ROOT_PASSWORD=your-secure-password
MYSQL_DATABASE=EcomWebsite
MYSQL_USER=app_user
MYSQL_PASSWORD=app-password
MYSQL_HTTP_PORT=3306
MYSQL_CONTAINER_HTTP_PORT=3306

# ============================================
# Redis Configuration
# ============================================
REDIS_HTTP_PORT=6379
REDIS_CONTAINER_HTTP_PORT=6379

# ============================================
# RabbitMQ Configuration
# ============================================
RABBITMQ_HTTP_PORT=5672
RABBITMQ_CONTAINER_HTTP_PORT=5672
RABBITMQ_MGMT_HTTP_PORT=15672
RABBITMQ_CONTAINER_MGMT_HTTP_PORT=15672

# ============================================
# Connection Strings (Docker Service Names)
# ============================================
ConnectionStrings__MyUserSqlConn=Server=mysql;Port=3306;Database=ecom_users;User=root;Password=${MYSQL_ROOT_PASSWORD}
ConnectionStrings__MyBlogSqlConn=Server=mysql;Port=3306;Database=ecom_blogs;User=root;Password=${MYSQL_ROOT_PASSWORD}
ConnectionStrings__MyProductSqlConn=Server=mysql;Port=3306;Database=ecom_products;User=root;Password=${MYSQL_ROOT_PASSWORD}
ConnectionStrings__MyOrderSqlConn=Server=mysql;Port=3306;Database=ecom_orders;User=root;Password=${MYSQL_ROOT_PASSWORD}

# ============================================
# Redis Connection
# ============================================
Redis__ConnectionString=redis:6379
Redis__InstanceName=EcomWebsite-Staging

# ============================================
# RabbitMQ Configuration
# ============================================
RabbitMqSettings__HostName=rabbitmq
RabbitMqSettings__Port=5672
RabbitMqSettings__UserName=guest
RabbitMqSettings__Password=guest

# ============================================
# GitHub Container Registry (for docker login)
# ============================================
GITHUB_USERNAME=your-github-username
GITHUB_TOKEN=ghp_xxxxxxxxxxxxx
```

**Lưu ý:** File `.env.stag` chứa secrets, **KHÔNG commit vào Git!**

### Bước 5: Tạo thư mục certs (nếu cần HTTPS)

```bash
mkdir -p ~/EcomWebsite-Backend/App/certs/staging
# Copy certificates vào đây nếu có
```

### Bước 6: Test manual (lần đầu)

```bash
# Login to GHCR
echo ${GITHUB_TOKEN} | docker login ghcr.io -u ${GITHUB_USERNAME} --password-stdin

# Pull image
docker pull ${IMAGE_NAME}:staging-latest

# Test run
docker compose -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag up -d

# Check status
docker compose -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag ps

# Check logs
docker compose -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag logs -f api
```

---

## 🔄 Workflow sau khi setup

### Mỗi lần deploy:

1. **CI/CD tự động:**
   - Build Docker image
   - Push lên GHCR với tag `staging-latest`
   - SSH vào EC2
   - Pull image mới
   - Chạy `docker compose up -d`

2. **Bạn không cần làm gì!** 🎉

### Nếu cần update docker-compose files:

```bash
# Từ máy local
scp -i your-key.pem App/docker-compose.yml ubuntu@${EC2_STAGING_HOST}:~/EcomWebsite-Backend/App/
scp -i your-key.pem App/docker-compose.stag.yml ubuntu@${EC2_STAGING_HOST}:~/EcomWebsite-Backend/App/

# Trên EC2, restart containers
cd ~/EcomWebsite-Backend/App
docker compose -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag restart
```

---

## ✅ Checklist

- [ ] Docker và Docker Compose đã được cài trên EC2
- [ ] Thư mục `~/EcomWebsite-Backend/App` đã được tạo
- [ ] Files `docker-compose.yml` và `docker-compose.stag.yml` đã được copy
- [ ] File `.env.stag` đã được tạo và cấu hình
- [ ] Thư mục `certs/staging` đã được tạo (nếu cần)
- [ ] Test manual deployment thành công
- [ ] GitLab CI/CD variables đã được setup
- [ ] SSH key đã được thêm vào GitLab variables

---

## 🔧 Troubleshooting

### Lỗi: "Required files not found"

```bash
# Check files có tồn tại không
ls -la ~/EcomWebsite-Backend/App/
# Phải có: docker-compose.yml, docker-compose.stag.yml, .env.stag
```

### Lỗi: "Cannot pull image"

```bash
# Test login
echo $GITHUB_TOKEN | docker login ghcr.io -u $GITHUB_USERNAME --password-stdin

# Check image name
echo $IMAGE_NAME
# Phải đúng format: ghcr.io/your-org/ecom-website-backend
```

### Lỗi: "Permission denied"

```bash
# Check Docker group
groups
# Phải có 'docker' trong groups

# Nếu không có, logout và login lại
exit
ssh -i your-key.pem ubuntu@${EC2_STAGING_HOST}
```

---

## 📝 Notes

- **`.env.stag`** không được commit vào Git (thêm vào `.gitignore`)
- Chỉ cần update docker-compose files khi có thay đổi cấu trúc services
- Image sẽ được tự động pull mới mỗi lần deploy
- Containers sẽ được restart với image mới

