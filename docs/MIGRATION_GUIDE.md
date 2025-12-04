# Hướng dẫn Migration Database

## 📋 Tổng quan

Khi bạn đã scaffold database (database-first) và sau đó thêm model mới vào code (code-first), bạn cần tạo migration để sync database.

## 🔄 Hai cách quản lý schema

### 1. **EnsureCreated** (Development - Nhanh nhưng mất dữ liệu)
- Tự động tạo/xóa toàn bộ schema từ model
- **Mất dữ liệu** khi restart nếu có thay đổi
- Không có migration history
- Phù hợp: Development, testing

### 2. **Migrate** (Production - An toàn, giữ dữ liệu)
- Apply migrations từng bước
- **Giữ nguyên dữ liệu** khi update schema
- Có migration history trong bảng `__EFMigrationsHistory`
- Phù hợp: Production, Staging

---

## 🚀 Các bước Migration cho SocialAccount

### Bước 1: Tạo Migration (Đã làm xong ✅)

```bash
cd App.DAL
dotnet ef migrations add AddSocialAccountsTable --context EcomUsersContext --output-dir Migrations/UserMigrations
```

**Kết quả:**
- ✅ `20251126175854_AddSocialAccountsTable.cs` - Migration file
- ✅ `20251126175854_AddSocialAccountsTable.Designer.cs` - Metadata
- ✅ `EcomUsersContextModelSnapshot.cs` - Current model snapshot

### Bước 2: Review Migration File

Xem file migration để đảm bảo đúng:
```csharp
// File: App.DAL/Migrations/UserMigrations/20251126175854_AddSocialAccountsTable.cs

// Up() method - Tạo bảng
migrationBuilder.CreateTable(
    name: "social_accounts",
    columns: table => new
    {
        id = table.Column<int>(...),
        user_id = table.Column<int>(...),
        provider = table.Column<string>(maxLength: 50, ...),
        provider_user_id = table.Column<string>(maxLength: 255, ...),
        // ... các columns khác
    });

// Tạo unique index
migrationBuilder.CreateIndex(
    name: "idx_provider_user_id",
    table: "social_accounts",
    columns: new[] { "provider", "provider_user_id" },
    unique: true);

// Tạo foreign key
migrationBuilder.AddForeignKey(
    name: "fk_social_accounts_users",
    table: "social_accounts",
    column: "user_id",
    principalTable: "users",
    principalColumn: "id",
    onDelete: ReferentialAction.Cascade);
```

### Bước 3: Apply Migration

#### Option A: Tự động khi app start (Khuyến nghị)

**1. Đổi SchemaMode trong `appsettings.Development.json`:**
```json
{
  "Database": {
    "SchemaMode": "Migrate",  // ← Đổi từ "EnsureCreated" → "Migrate"
    "SeedEnabled": false,
    "RecreateOnStart": false
  }
}
```

**2. Chạy app:**
```bash
dotnet run --project App
```

App sẽ tự động apply migration khi start.

#### Option B: Manual apply bằng CLI

```bash
cd App.DAL
dotnet ef database update --context EcomUsersContext
```

**Hoặc apply cho tất cả contexts:**
```bash
# Users DB
dotnet ef database update --context EcomUsersContext --project App.DAL --startup-project App

# Blogs DB (nếu có migration)
dotnet ef database update --context EcomBlogsContext --project App.DAL --startup-project App

# Products DB (nếu có migration)
dotnet ef database update --context EcomProductsContext --project App.DAL --startup-project App
```

### Bước 4: Verify Migration

**Kiểm tra trong database:**
```sql
-- Xem migration history
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId DESC;

-- Xem bảng social_accounts đã được tạo
DESCRIBE social_accounts;

-- Xem indexes
SHOW INDEXES FROM social_accounts;
```

---

## ⚠️ Lưu ý quan trọng

### Khi nào dùng EnsureCreated vs Migrate?

| Scenario | Mode | Lý do |
|----------|------|-------|
| Development mới, chưa có data | `EnsureCreated` | Nhanh, tự động sync model |
| Development có data quan trọng | `Migrate` | Giữ data, apply từng bước |
| Staging/Production | `Migrate` | **BẮT BUỘC** - An toàn, có rollback |

### Nếu đã dùng EnsureCreated và muốn chuyển sang Migrate:

**⚠️ CẢNH BÁO:** Nếu database đã có data và bạn đổi sang `Migrate`:
1. EF sẽ tạo bảng `__EFMigrationsHistory` mới
2. Cần **mark migrations đã apply** để tránh conflict:

```bash
# Mark migration đã apply (không chạy SQL)
dotnet ef database update AddSocialAccountsTable --context EcomUsersContext --connection "your-connection-string" --no-build
```

**Hoặc manual insert vào `__EFMigrationsHistory`:**
```sql
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20251126175854_AddSocialAccountsTable', '9.0.9');
```

---

## 🔧 Các lệnh Migration hữu ích

### Xem danh sách migrations:
```bash
dotnet ef migrations list --context EcomUsersContext
```

### Xóa migration cuối (chưa apply):
```bash
dotnet ef migrations remove --context EcomUsersContext
```

### Tạo SQL script (không apply):
```bash
dotnet ef migrations script --context EcomUsersContext --output migration.sql
```

### Apply migration cụ thể:
```bash
dotnet ef database update AddSocialAccountsTable --context EcomUsersContext
```

### Rollback migration:
```bash
# Rollback về migration trước đó
dotnet ef database update PreviousMigrationName --context EcomUsersContext
```

---

## 📝 Checklist Migration

- [x] Tạo migration file
- [ ] Review migration code
- [ ] Backup database (nếu có data)
- [ ] Apply migration (EnsureCreated hoặc Migrate)
- [ ] Verify bảng đã được tạo
- [ ] Test code với model mới
- [ ] Commit migration files vào Git

---

## 🎯 Next Steps

Sau khi migration thành công:
1. ✅ Bảng `social_accounts` đã có trong database
2. ✅ Code có thể dùng `SocialAccountRepo`
3. ✅ Có thể test `GetOrCreateSocialUserAsync()`

**Lưu ý:** Nếu database đã có data và bạn đang dùng `EnsureCreated`, khi restart app với model mới, EF sẽ **xóa và tạo lại** toàn bộ schema → **MẤT DATA**!

→ **Khuyến nghị:** Chuyển sang `Migrate` mode ngay khi có data quan trọng.

