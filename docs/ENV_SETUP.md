# Environment Variables Setup Guide

## 📋 Tóm tắt

File `.env.stag.example` chứa template cho tất cả các biến môi trường cần thiết cho staging environment.

## 🚀 Cách sử dụng

### 1. Copy template file

```bash
# Trên EC2
cd ~/EcomWebsite-Backend/App
cp .env.stag.example .env.stag
```

### 2. Edit file với giá trị thực tế

```bash
nano .env.stag
```

### 3. Các giá trị cần thay đổi

#### 🔴 Bắt buộc phải thay:

- `MYSQL_ROOT_PASSWORD` - Mật khẩu root MySQL (tối thiểu 32 ký tự)
- `MYSQL_PASSWORD` - Mật khẩu user MySQL
- `JwtSettings__SecretKey` - Secret key cho JWT (tối thiểu 32 ký tự, dùng random string)
- `GITHUB_USERNAME` - GitHub username để pull images
- `GITHUB_TOKEN` - GitHub Personal Access Token
- `API_IMAGE_NAME` - Tên image trên GHCR (ví dụ: `ghcr.io/your-org/ecom-website-backend`)

#### 🟡 Nên thay đổi:

- `Cors__AllowedOrigins` - Domain staging của bạn
- `GoogleAuth__ClientId` và `GoogleAuth__ClientSecret` - Nếu dùng Google OAuth
- `EmailSettings__*` - Cấu hình email
- `StripeSettings__*` hoặc `PayPalSettings__*` - Nếu dùng payment gateway

#### 🟢 Có thể giữ nguyên (hoặc tùy chỉnh):

- Ports (5095, 5096, etc.)
- Database names
- Cache settings
- Logging settings

## ⚠️ Lưu ý quan trọng

1. **KHÔNG commit `.env.stag` vào Git!**
   - File này chứa secrets
   - Thêm vào `.gitignore`:
     ```
     .env.stag
     .env.*
     !.env.*.example
     ```

2. **Connection Strings:**
   - Dùng Docker service name: `Server=mysql` (không phải `localhost`)
   - Tương tự cho Redis: `redis:6379`
   - Tương tự cho RabbitMQ: `rabbitmq:5672`

3. **Passwords:**
   - Dùng password mạnh (tối thiểu 32 ký tự cho root)
   - Generate random strings:
     ```bash
     openssl rand -base64 32
     ```

4. **JWT Secret:**
   - Phải là random string mạnh
   - Không dùng giá trị mặc định
   - Generate:
     ```bash
     openssl rand -base64 64
     ```

## 📝 Checklist

- [ ] Copy `.env.stag.example` → `.env.stag`
- [ ] Thay đổi `MYSQL_ROOT_PASSWORD`
- [ ] Thay đổi `MYSQL_PASSWORD`
- [ ] Thay đổi `JwtSettings__SecretKey`
- [ ] Thay đổi `GITHUB_USERNAME` và `GITHUB_TOKEN`
- [ ] Thay đổi `API_IMAGE_NAME`
- [ ] Cập nhật `Cors__AllowedOrigins`
- [ ] Cập nhật email settings (nếu cần)
- [ ] Cập nhật payment gateway settings (nếu cần)
- [ ] Thêm `.env.stag` vào `.gitignore`
- [ ] Test deployment với file `.env.stag` mới

## 🔍 Verify

Sau khi tạo file, verify:

```bash
# Check file tồn tại
ls -la .env.stag

# Check không có giá trị mặc định nguy hiểm
grep -i "CHANGE_ME\|YOUR_\|localhost" .env.stag
# Nếu còn kết quả, cần thay đổi thêm
```

## 🆘 Troubleshooting

### Lỗi: "Cannot connect to MySQL"

- Check `ConnectionStrings__*` dùng `Server=mysql` (Docker service name)
- Check `MYSQL_ROOT_PASSWORD` đúng chưa
- Check MySQL container đã chạy: `docker compose ps`

### Lỗi: "Cannot pull image"

- Check `GITHUB_TOKEN` có quyền `read:packages`
- Check `API_IMAGE_NAME` đúng format: `ghcr.io/org/repo`
- Test login: `echo $GITHUB_TOKEN | docker login ghcr.io -u $GITHUB_USERNAME --password-stdin`

### Lỗi: "JWT validation failed"

- Check `JwtSettings__SecretKey` đã được thay đổi
- Check secret key đủ dài (tối thiểu 32 ký tự)

