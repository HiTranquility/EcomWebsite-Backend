# GitLab CI/CD Setup Guide

## 📋 Mục lục
1. [Setup GitLab CI/CD Variables](#1-setup-gitlab-cicd-variables)
2. [Setup EC2 Server](#2-setup-ec2-server)
3. [Tạo SSH Key cho GitLab Runner](#3-tạo-ssh-key-cho-gitlab-runner)
4. [Verify Setup](#4-verify-setup)

---

## 1️⃣ Setup GitLab CI/CD Variables

### Cách thêm biến môi trường vào GitLab:

1. Vào **GitLab Project** → **Settings** → **CI/CD** → **Variables** → **Expand**

2. Click **Add variable** và thêm từng biến sau:

#### 🔐 GitHub Container Registry

| Key | Value | Protected | Masked | Description |
|-----|-------|-----------|--------|-------------|
| `GITHUB_USERNAME` | `your-github-username` | ☐ | ☐ | GitHub username |
| `GITHUB_TOKEN` | `ghp_xxxxxxxxxxxxx` | ☑ | ☑ | GitHub Personal Access Token (PAT) |

**Cách tạo GitHub Token:**
- GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
- Generate new token → Chọn scopes: `write:packages`, `read:packages`
- Copy token và paste vào GitLab variable

#### 🖥️ EC2 Staging Server

| Key | Value | Protected | Masked | Description |
|-----|-------|-----------|--------|-------------|
| `EC2_STAGING_HOST` | `ec2-xx-xx-xx-xx.compute-1.amazonaws.com` | ☐ | ☐ | EC2 public IP hoặc hostname |
| `EC2_STAGING_USER` | `ubuntu` | ☐ | ☐ | SSH user (ubuntu hoặc ec2-user) |
| `EC2_STAGING_DEPLOY_PATH` | `/home/ubuntu/EcomWebsite-Backend/App` | ☐ | ☐ | Đường dẫn deploy trên EC2 |
| `EC2_SSH_PRIVATE_KEY` | `<paste SSH private key>` | ☑ | ☑ | SSH private key (xem phần 3) |

#### 🐳 Docker Image

| Key | Value | Protected | Masked | Description |
|-----|-------|-----------|--------|-------------|
| `IMAGE_NAME` | `ghcr.io/your-org/ecom-website-backend` | ☐ | ☐ | Docker image name trên GHCR |

**Lưu ý:** Thay `your-org` bằng GitHub organization/username của bạn.

#### 📢 Notification (Optional)

| Key | Value | Protected | Masked | Description |
|-----|-------|-----------|--------|-------------|
| `SLACK_WEBHOOK_URL` | `https://hooks.slack.com/...` | ☐ | ☑ | Slack webhook URL (nếu dùng) |

---

## 2️⃣ Setup EC2 Server

### Bước 1: Launch EC2 Instance

1. **AWS Console** → **EC2** → **Launch Instance**
2. **AMI:** Ubuntu 22.04 LTS (hoặc Amazon Linux 2023)
3. **Instance Type:** t3.medium (hoặc lớn hơn tùy nhu cầu)
4. **Key Pair:** Tạo hoặc chọn key pair mới
5. **Security Group:** Mở ports:
   - `22` (SSH)
   - `5095` (API HTTP - staging)
   - `5096` (API HTTPS - staging)
   - `3306` (MySQL - chỉ internal, không expose)
   - `6379` (Redis - chỉ internal)
   - `5672` (RabbitMQ - chỉ internal)
6. **Launch Instance**

### Bước 2: SSH vào EC2 và Setup

```bash
# SSH vào EC2 (từ máy local)
ssh -i your-key.pem ubuntu@${EC2_STAGING_HOST}

# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker ubuntu

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Install Git
sudo apt install git -y

# Install GitLab Runner (nếu muốn chạy runner trên EC2)
# Hoặc dùng GitLab.com shared runners

# Logout và login lại để apply docker group
exit
ssh -i your-key.pem ubuntu@${EC2_STAGING_HOST}
```

### Bước 3: Clone Repository

```bash
# Tạo thư mục
mkdir -p ~/EcomWebsite-Backend
cd ~/EcomWebsite-Backend

# Clone repo (dùng SSH hoặc HTTPS)
git clone git@gitlab.com:your-org/EcomWebsite-Backend.git .
# Hoặc
git clone https://gitlab.com/your-org/EcomWebsite-Backend.git .

cd App

# Tạo .env.stag file
cp .env.stag.example .env.stag
nano .env.stag  # Edit với các giá trị staging
```

### Bước 4: Tạo thư mục certs (nếu cần HTTPS)

```bash
mkdir -p certs/staging
# Copy staging certificates vào đây nếu có
```

---

## 3️⃣ Tạo SSH Key cho GitLab Runner

### Cách 1: Tạo SSH Key mới (Khuyến nghị)

**Trên máy local hoặc GitLab Runner:**

```bash
# Tạo SSH key pair
ssh-keygen -t ed25519 -C "gitlab-runner@staging" -f ~/.ssh/gitlab_runner_staging

# Xem public key
cat ~/.ssh/gitlab_runner_staging.pub

# Xem private key (copy toàn bộ, kể cả BEGIN/END lines)
cat ~/.ssh/gitlab_runner_staging
```

**Copy public key vào EC2:**

```bash
# Trên EC2
mkdir -p ~/.ssh
chmod 700 ~/.ssh

# Thêm public key vào authorized_keys
echo "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAI..." >> ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys
```

**Copy private key vào GitLab Variable:**
- Copy toàn bộ nội dung file `~/.ssh/gitlab_runner_staging` (kể cả `-----BEGIN OPENSSH PRIVATE KEY-----` và `-----END OPENSSH PRIVATE KEY-----`)
- Paste vào GitLab variable `EC2_SSH_PRIVATE_KEY`
- Check **Protected** và **Masked**

### Cách 2: Dùng EC2 Key Pair hiện có

Nếu bạn đã có EC2 key pair (`.pem` file):

```bash
# Convert PEM sang format phù hợp (nếu cần)
ssh-keygen -p -f your-key.pem

# Copy private key vào GitLab variable
cat your-key.pem
# Copy toàn bộ và paste vào GitLab variable EC2_SSH_PRIVATE_KEY
```

**Lưu ý:** Đảm bảo public key tương ứng đã có trong `~/.ssh/authorized_keys` trên EC2.

---

## 4️⃣ Verify Setup

### Test SSH Connection từ GitLab Runner

Tạo một test job trong GitLab CI:

```yaml
test_ssh_connection:
  stage: .pre
  image: alpine:latest
  before_script:
    - apk add --no-cache openssh-client
    - eval $(ssh-agent -s)
    - echo "$EC2_SSH_PRIVATE_KEY" | tr -d '\r' | ssh-add -
    - mkdir -p ~/.ssh && chmod 700 ~/.ssh
    - ssh-keyscan -H $EC2_STAGING_HOST >> ~/.ssh/known_hosts
  script:
    - ssh -o StrictHostKeyChecking=no ${EC2_STAGING_USER}@${EC2_STAGING_HOST} "echo 'SSH connection successful!'"
  rules:
    - if: $CI_COMMIT_BRANCH == "staging"
```

### Test Docker trên EC2

```bash
# SSH vào EC2
ssh ubuntu@${EC2_STAGING_HOST}

# Test Docker
docker --version
docker compose --version

# Test pull image từ GHCR
echo $GITHUB_TOKEN | docker login ghcr.io -u $GITHUB_USERNAME --password-stdin
docker pull ${IMAGE_NAME}:staging-latest
```

### Test GitLab CI Pipeline

1. Push code lên branch `staging`
2. Vào GitLab → **CI/CD** → **Pipelines**
3. Xem pipeline chạy và check logs
4. Nếu có lỗi, check:
   - Variables đã set đúng chưa
   - SSH key có đúng format không
   - EC2 có accessible không
   - Docker trên EC2 có chạy không

---

## 🔧 Troubleshooting

### Lỗi SSH Connection Failed

```bash
# Check SSH key format
# Private key phải có format:
# -----BEGIN OPENSSH PRIVATE KEY-----
# ...
# -----END OPENSSH PRIVATE KEY-----

# Test SSH từ local
ssh -i ~/.ssh/gitlab_runner_staging ubuntu@${EC2_STAGING_HOST}
```

### Lỗi Docker Login Failed

```bash
# Verify GitHub token có quyền write:packages
# Test login từ local
echo $GITHUB_TOKEN | docker login ghcr.io -u $GITHUB_USERNAME --password-stdin
```

### Lỗi Permission Denied trên EC2

```bash
# Check permissions
ls -la ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys
chmod 700 ~/.ssh
```

### Lỗi Docker Compose không chạy

```bash
# Check Docker Compose version
docker compose version

# Check file permissions
ls -la docker-compose*.yml
ls -la .env.stag
```

---

## 📝 Checklist Setup

- [ ] GitLab CI/CD Variables đã được thêm đầy đủ
- [ ] EC2 instance đã được launch và cấu hình
- [ ] Docker và Docker Compose đã được cài trên EC2
- [ ] Repository đã được clone trên EC2
- [ ] `.env.stag` file đã được tạo và cấu hình
- [ ] SSH key đã được tạo và copy vào GitLab variable
- [ ] Public key đã được thêm vào EC2 `authorized_keys`
- [ ] Test SSH connection thành công
- [ ] Test Docker login vào GHCR thành công
- [ ] Pipeline chạy thành công

---

## 🎯 Next Steps

Sau khi setup xong:

1. **Test pipeline:** Push code lên `develop` → xem pipeline chạy
2. **Merge to staging:** Merge `develop` → `staging` → xem deploy job
3. **Verify deployment:** Check services trên EC2 đã chạy chưa
4. **Monitor:** Xem logs và metrics trên staging environment

---

## 📚 References

- [GitLab CI/CD Variables](https://docs.gitlab.com/ee/ci/variables/)
- [GitHub Container Registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry)
- [Docker Compose Documentation](https://docs.docker.com/compose/)

