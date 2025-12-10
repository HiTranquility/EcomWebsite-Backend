#!/bin/bash
# ============================================
# EC2 HTTPS Setup Script
# ============================================
# Setup Nginx reverse proxy với Let's Encrypt SSL
# Usage: sudo bash EC2_HTTPS_SETUP.sh api.yourdomain.com

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check domain argument
if [ -z "$1" ]; then
    echo -e "${RED}❌ Error: Domain name required${NC}"
    echo "Usage: sudo bash EC2_HTTPS_SETUP.sh api.yourdomain.com"
    exit 1
fi

DOMAIN=$1
API_PORT=5095

echo -e "${GREEN}🚀 Starting HTTPS setup for ${DOMAIN}...${NC}"

# ============================================
# 1. Update System
# ============================================
echo -e "${YELLOW}📦 Updating system...${NC}"
sudo apt update && sudo apt upgrade -y

# ============================================
# 2. Install Nginx
# ============================================
echo -e "${YELLOW}📦 Installing Nginx...${NC}"
if ! command -v nginx &> /dev/null; then
    sudo apt install nginx -y
    echo -e "${GREEN}✅ Nginx installed${NC}"
else
    echo -e "${GREEN}✅ Nginx already installed${NC}"
fi

# ============================================
# 3. Install Certbot
# ============================================
echo -e "${YELLOW}📦 Installing Certbot...${NC}"
if ! command -v certbot &> /dev/null; then
    sudo apt install certbot python3-certbot-nginx -y
    echo -e "${GREEN}✅ Certbot installed${NC}"
else
    echo -e "${GREEN}✅ Certbot already installed${NC}"
fi

# ============================================
# 4. Create Nginx Config
# ============================================
echo -e "${YELLOW}📝 Creating Nginx configuration...${NC}"
sudo tee /etc/nginx/sites-available/api-staging > /dev/null <<EOF
server {
    listen 80;
    server_name ${DOMAIN};

    location / {
        proxy_pass http://localhost:${API_PORT};
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOF

# Enable site
if [ ! -L /etc/nginx/sites-enabled/api-staging ]; then
    sudo ln -s /etc/nginx/sites-available/api-staging /etc/nginx/sites-enabled/
    echo -e "${GREEN}✅ Nginx site enabled${NC}"
fi

# Test and reload Nginx
echo -e "${YELLOW}🔍 Testing Nginx configuration...${NC}"
sudo nginx -t

sudo systemctl restart nginx
sudo systemctl enable nginx
echo -e "${GREEN}✅ Nginx started and enabled${NC}"

# ============================================
# 5. Setup Firewall
# ============================================
echo -e "${YELLOW}🔥 Configuring firewall...${NC}"
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw reload
echo -e "${GREEN}✅ Firewall configured${NC}"

# ============================================
# 6. Get SSL Certificate
# ============================================
echo -e "${YELLOW}🔒 Getting SSL certificate from Let's Encrypt...${NC}"
echo -e "${YELLOW}⚠️  Make sure domain ${DOMAIN} points to this server's IP!${NC}"
read -p "Press Enter to continue..."

sudo certbot --nginx -d ${DOMAIN} --non-interactive --agree-tos --email admin@${DOMAIN} --redirect

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ SSL certificate obtained and configured${NC}"
else
    echo -e "${RED}❌ Failed to obtain SSL certificate${NC}"
    echo -e "${YELLOW}Check:${NC}"
    echo "  1. Domain ${DOMAIN} points to this server's IP"
    echo "  2. Port 80 is accessible from internet"
    echo "  3. No firewall blocking port 80"
    exit 1
fi

# ============================================
# 7. Test Auto-renewal
# ============================================
echo -e "${YELLOW}🔄 Testing certificate auto-renewal...${NC}"
sudo certbot renew --dry-run

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Auto-renewal configured successfully${NC}"
else
    echo -e "${YELLOW}⚠️  Auto-renewal test failed, but certificate is valid${NC}"
fi

# ============================================
# 8. Summary
# ============================================
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}✅ HTTPS Setup Completed!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}📋 Next Steps:${NC}"
echo "1. Update CORS in .env.stag:"
echo "   Cors__AllowedOrigins=https://ecom-website-frontend-dusky.vercel.app,https://${DOMAIN}"
echo ""
echo "2. Restart API:"
echo "   cd ~/EcomWebsite-Backend/App"
echo "   docker compose -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag restart api"
echo ""
echo "3. Update Frontend VITE_API_URL on Vercel:"
echo "   VITE_API_URL=https://${DOMAIN}"
echo ""
echo "4. Test API:"
echo "   curl https://${DOMAIN}/health"
echo ""
echo -e "${GREEN}🎉 Your API is now accessible via HTTPS!${NC}"

