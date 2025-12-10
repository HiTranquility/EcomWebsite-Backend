#!/bin/bash
# ============================================
# EC2 Initial Setup Script
# ============================================
# Chạy script này trên EC2 instance lần đầu
# Usage: bash EC2_SETUP_SCRIPT.sh

set -e

echo "🚀 Starting EC2 setup for EcomWebsite Backend..."

# ============================================
# 1. Update System
# ============================================
echo "📦 Updating system packages..."
sudo apt update && sudo apt upgrade -y

# ============================================
# 2. Install Docker
# ============================================
echo "🐳 Installing Docker..."
if ! command -v docker &> /dev/null; then
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    rm get-docker.sh
    echo "✅ Docker installed successfully"
else
    echo "✅ Docker already installed"
fi

# ============================================
# 3. Install Docker Compose
# ============================================
echo "🐳 Installing Docker Compose..."
if ! command -v docker-compose &> /dev/null; then
    DOCKER_COMPOSE_VERSION=$(curl -s https://api.github.com/repos/docker/compose/releases/latest | grep 'tag_name' | cut -d\" -f4)
    sudo curl -L "https://github.com/docker/compose/releases/download/${DOCKER_COMPOSE_VERSION}/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
    echo "✅ Docker Compose installed successfully"
else
    echo "✅ Docker Compose already installed"
fi

# ============================================
# 4. Install Git
# ============================================
echo "📥 Installing Git..."
if ! command -v git &> /dev/null; then
    sudo apt install git -y
    echo "✅ Git installed successfully"
else
    echo "✅ Git already installed"
fi

# ============================================
# 5. Install Required Tools
# ============================================
echo "🛠️ Installing additional tools..."
sudo apt install -y curl wget unzip jq

# ============================================
# 6. Setup SSH for GitLab Runner
# ============================================
echo "🔐 Setting up SSH..."
mkdir -p ~/.ssh
chmod 700 ~/.ssh

# Tạo authorized_keys nếu chưa có
if [ ! -f ~/.ssh/authorized_keys ]; then
    touch ~/.ssh/authorized_keys
    chmod 600 ~/.ssh/authorized_keys
fi

echo "📝 Please add GitLab Runner public key to ~/.ssh/authorized_keys"
echo "   You can do this by running:"
echo "   echo 'YOUR_PUBLIC_KEY' >> ~/.ssh/authorized_keys"

# ============================================
# 7. Create Project Directory
# ============================================
echo "📁 Creating project directory..."
PROJECT_DIR="$HOME/EcomWebsite-Backend"
mkdir -p "$PROJECT_DIR"
echo "✅ Project directory created: $PROJECT_DIR"

# ============================================
# 8. Setup Docker Log Rotation
# ============================================
echo "📋 Setting up Docker log rotation..."
sudo tee /etc/docker/daemon.json > /dev/null <<EOF
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  }
}
EOF

# ============================================
# 9. Display Information
# ============================================
echo ""
echo "============================================"
echo "✅ EC2 Setup Completed!"
echo "============================================"
echo ""
echo "📋 Next Steps:"
echo "1. Clone repository:"
echo "   cd $PROJECT_DIR"
echo "   git clone <your-repo-url> ."
echo ""
echo "2. Create .env.stag file:"
echo "   cd $PROJECT_DIR/App"
echo "   cp .env.stag.example .env.stag"
echo "   nano .env.stag"
echo ""
echo "3. Add GitLab Runner SSH public key:"
echo "   nano ~/.ssh/authorized_keys"
echo ""
echo "4. Logout and login again to apply Docker group:"
echo "   exit"
echo "   ssh ubuntu@\$(hostname)"
echo ""
echo "5. Test Docker:"
echo "   docker --version"
echo "   docker compose --version"
echo ""
echo "============================================"

