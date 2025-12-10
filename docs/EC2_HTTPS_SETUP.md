# Setup HTTPS cho API trên EC2

## 📋 Tổng quan

Hướng dẫn setup HTTPS cho API server trên EC2 sử dụng:
- **Nginx** làm reverse proxy
- **Let's Encrypt** cho SSL certificate (free)
- **Certbot** để tự động renew certificates

---

## 🎯 Yêu cầu

1. Domain name trỏ về EC2 IP (ví dụ: `api.yourdomain.com` → `13.212.226.13`)
2. EC2 instance đang chạy
3. Port 80 và 443 đã mở trong Security Group

---

## 🚀 Bước 1: Cài đặt Nginx và Certbot

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Nginx
sudo apt install nginx -y

# Install Certbot
sudo apt install certbot python3-certbot-nginx -y

# Start và enable Nginx
sudo systemctl start nginx
sudo systemctl enable nginx

# Check status
sudo systemctl status nginx
```

---

## 🔧 Bước 2: Cấu hình Nginx

Tạo file config cho API:

```bash
sudo nano /etc/nginx/sites-available/api-staging
```

Nội dung (chưa có SSL):

```nginx
server {
    listen 80;
    server_name api.yourdomain.com;  # Thay bằng domain của bạn

    location / {
        proxy_pass http://localhost:5095;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

Enable site:

```bash
sudo ln -s /etc/nginx/sites-available/api-staging /etc/nginx/sites-enabled/
sudo nginx -t  # Test config
sudo systemctl reload nginx
```

---

## 🔒 Bước 3: Setup SSL với Let's Encrypt

```bash
# Chạy Certbot để lấy certificate
sudo certbot --nginx -d api.yourdomain.com

# Certbot sẽ:
# 1. Tự động cấu hình Nginx
# 2. Lấy SSL certificate từ Let's Encrypt
# 3. Setup auto-renewal
```

Sau khi chạy, Certbot sẽ tự động update file config với HTTPS.

---

## ✅ Bước 4: Verify

```bash
# Test HTTPS
curl https://api.yourdomain.com/health

# Check certificate
openssl s_client -connect api.yourdomain.com:443 -servername api.yourdomain.com
```

---

## 🔄 Bước 5: Auto-renewal (đã tự động setup)

Certbot tự động setup cron job để renew certificates:

```bash
# Check renewal
sudo certbot renew --dry-run

# Manual renewal (nếu cần)
sudo certbot renew
```

---

## 📝 Bước 6: Update CORS trên API

Update `.env.stag` trên EC2:

```bash
cd ~/EcomWebsite-Backend/App
nano .env.stag
```

Thêm domain frontend vào CORS:

```bash
Cors__AllowedOrigins=https://ecom-website-frontend-dusky.vercel.app,https://api.yourdomain.com
```

Restart API:

```bash
docker compose -f docker-compose.yml -f docker-compose.stag.yml --env-file .env.stag restart api
```

---

## 🌐 Bước 7: Update Frontend

Update `VITE_API_URL` trên Vercel:

```
VITE_API_URL=https://api.yourdomain.com
```

Redeploy frontend trên Vercel.

---

## 🔧 Troubleshooting

### Lỗi: "Domain not found"

- Check DNS: `dig api.yourdomain.com` hoặc `nslookup api.yourdomain.com`
- Đảm bảo domain trỏ về EC2 IP

### Lỗi: "Port 80/443 not accessible"

- Check Security Group: mở port 80 và 443
- Check firewall: `sudo ufw allow 80/tcp && sudo ufw allow 443/tcp`

### Lỗi: "Nginx not running"

```bash
sudo systemctl status nginx
sudo nginx -t  # Check config errors
sudo systemctl restart nginx
```

### Check logs

```bash
# Nginx logs
sudo tail -f /var/log/nginx/error.log
sudo tail -f /var/log/nginx/access.log

# Certbot logs
sudo tail -f /var/log/letsencrypt/letsencrypt.log
```

---

## 📋 Checklist

- [ ] Domain đã trỏ về EC2 IP
- [ ] Security Group đã mở port 80 và 443
- [ ] Nginx đã được cài đặt và chạy
- [ ] Certbot đã được cài đặt
- [ ] SSL certificate đã được cấp
- [ ] Nginx config đã được update với HTTPS
- [ ] API đang chạy trên port 5095
- [ ] CORS đã được update
- [ ] Frontend đã được update với HTTPS API URL
- [ ] Test HTTPS endpoint thành công

---

## 🎉 Kết quả

Sau khi setup xong:
- API sẽ accessible qua: `https://api.yourdomain.com`
- Frontend có thể gọi API qua HTTPS
- Không còn Mixed Content error
- SSL certificate tự động renew mỗi 90 ngày

