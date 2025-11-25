# Hướng Dẫn Seeding Database - Tiếng Việt

## Tổng Quan

Hệ thống seeding database cung cấp dữ liệu mẫu cho:
- **Roles (Vai trò)**: Admin, Manager, User, Guest, Developer, Support
- **Permissions (Quyền hạn)**: Quyền truy cập cho users, roles, policies, documents, orders, support, system, reports
- **Role-Permission Mappings**: Gán quyền cho từng vai trò
- **Users (Người dùng)**: 7 tài khoản mẫu với các vai trò khác nhau
- **User Attributes (Thuộc tính)**: Department, location, level cho ABAC
- **Policies (Chính sách)**: Các chính sách PBAC mẫu

## Cách Sử Dụng Nhanh

### Phương Pháp 1: Sử Dụng Script PowerShell (Khuyên Dùng)

```powershell
# Chạy đầy đủ (migration + seeding)
.\scripts\setup.ps1

# Xóa database cũ và tạo lại
.\scripts\setup.ps1 -DropDatabase

# Chỉ chạy migrations (bỏ qua seeding)
.\scripts\setup.ps1 -SkipSeeding

# Chỉ seeding dữ liệu (bỏ qua migrations)
.\scripts\setup.ps1 -SkipMigration
```

### Phương Pháp 2: Sử Dụng EF Core CLI

```powershell
# Di chuyển đến thư mục API project
cd src\VolcanionAuth.API

# Chạy migrations
dotnet ef database update

# Build và chạy với seeding
dotnet build
dotnet run --environment Development -- seed
```

## Dữ Liệu Mẫu

### Tài Khoản Người Dùng Mẫu

| Email | Mật khẩu | Vai trò | Phòng ban | Địa điểm | Cấp độ |
|-------|----------|---------|-----------|----------|--------|
| admin@volcanion.com | Admin@123456 | Admin | IT | HQ | Senior |
| manager@volcanion.com | Manager@123456 | Manager, User | Sales | HQ | Manager |
| user1@volcanion.com | User@123456 | User | Sales | Branch-A | Junior |
| user2@volcanion.com | User@123456 | User | Marketing | Branch-B | Mid |
| guest@volcanion.com | Guest@123456 | Guest | - | - | - |
| developer@volcanion.com | Dev@123456 | Developer | IT | Remote | Senior |
| support@volcanion.com | Support@123456 | Support | Support | HQ | Mid |

### Các Vai Trò

1. **Admin**: Toàn quyền quản trị hệ thống
2. **Manager**: Quyền quản lý nâng cao (trừ quản trị hệ thống)
3. **User**: Người dùng thông thường với quyền đọc/ghi cơ bản
4. **Guest**: Chỉ có quyền đọc
5. **Developer**: Quyền truy cập kỹ thuật
6. **Support**: Quyền truy cập hỗ trợ khách hàng

### Các Quyền Hạn

Quyền được tổ chức theo tài nguyên và hành động:

- **Users (Người dùng)**: read, write, delete, manage
- **Roles (Vai trò)**: read, write, delete, assign
- **Permissions (Quyền hạn)**: read, write, delete, assign
- **Policies (Chính sách)**: read, write, delete, evaluate
- **Documents (Tài liệu)**: read, write, delete, edit, share
- **Orders (Đơn hàng)**: read, write, delete, approve, cancel
- **Support (Hỗ trợ)**: access, read, write, resolve
- **System (Hệ thống)**: read, write, admin
- **Reports (Báo cáo)**: read, write, export

## Các Thành Phần Seeder

### Các Seeder Riêng Lẻ

1. **RoleSeeder**: Tạo các vai trò mặc định
2. **PermissionSeeder**: Tạo tập quyền hạn đầy đủ
3. **RolePermissionSeeder**: Gán quyền cho vai trò
4. **PolicySeeder**: Tạo các chính sách PBAC mẫu
5. **UserSeeder**: Tạo người dùng mẫu với mật khẩu đã hash
6. **UserRoleSeeder**: Gán vai trò cho người dùng
7. **UserAttributeSeeder**: Thêm thuộc tính ABAC cho người dùng

### DatabaseSeeder

Class `DatabaseSeeder` điều phối tất cả seeders theo đúng thứ tự:

```
1. Roles (Vai trò)
2. Permissions (Quyền hạn)
3. Role-Permission relationships (Quan hệ vai trò-quyền)
4. Policies (Chính sách)
5. Users (Người dùng)
6. User-Role relationships (Quan hệ người dùng-vai trò)
7. User Attributes (Thuộc tính người dùng)
```

## Quy Trình Phát Triển

### Thiết Lập Ban Đầu

```powershell
# Clone repository
git clone <repository-url>
cd auth-service

# Chạy script setup
.\scripts\setup.ps1
```

### Reset Database

```powershell
# Xóa và tạo lại với dữ liệu mới
.\scripts\setup.ps1 -DropDatabase
```

### Thêm Dữ Liệu Mẫu Tùy Chỉnh

Để thêm dữ liệu mẫu tùy chỉnh:

1. Tạo class seeder mới trong `src\VolcanionAuth.Infrastructure\Seeding\`
2. Đặt tên theo pattern: `{Entity}Seeder.cs`
3. Thêm lời gọi seeder vào `DatabaseSeeder.SeedAllAsync()`
4. Chạy script setup

## Kiểm Tra Lỗi

### Seeding Thất Bại

1. Kiểm tra kết nối database trong `appsettings.Development.json`
2. Đảm bảo PostgreSQL đang chạy
3. Xác minh migrations đã cập nhật: `dotnet ef migrations list`
4. Kiểm tra logs để xem thông báo lỗi cụ thể

### Dữ Liệu Trùng Lặp

Seeders được thiết kế idempotent và không tạo bản sao. Để reset:

```powershell
.\scripts\setup.ps1 -DropDatabase
```

### Lỗi Quyền

Đảm bảo PostgreSQL user có quyền CREATE và INSERT:

```sql
GRANT ALL PRIVILEGES ON DATABASE volcanion_auth TO postgres;
```

## Lưu Ý Production

**⚠️ CẢNH BÁO**: Dữ liệu mẫu chỉ dành cho development. Không sử dụng trong production!

Đối với production:

1. Xóa hoặc vô hiệu hóa seeding trong `Program.cs`
2. Tạo users production thông qua API
3. Định nghĩa roles và permissions cẩn thận
4. Sử dụng cấu hình theo môi trường

## Test Dữ Liệu Đã Seed

Sau khi seeding, bạn có thể test với các user mẫu:

```powershell
# Sử dụng Postman collection
# Import: postman/Volcanion-Auth-Complete.postman_collection.json

# Hoặc dùng curl
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@volcanion.com",
    "password": "Admin@123456"
  }'
```

## Tài Nguyên Bổ Sung

- [Getting Started Guide](../docs/GETTING_STARTED.md)
- [API Reference](../docs/API_REFERENCE.md)
- [RBAC Guide](../docs/RBAC_GUIDE.md)
- [PBAC Guide](../docs/PBAC_GUIDE.md)
- [Seeding Guide (English)](../docs/SEEDING_GUIDE.md)
- [Sample Credentials](../SAMPLE_CREDENTIALS.md)

## Các Tham Số của Script Setup

```powershell
# Xóa database trước khi tạo lại
-DropDatabase

# Bỏ qua migration
-SkipMigration

# Bỏ qua seeding
-SkipSeeding

# Chỉ định môi trường
-Environment <Development|Production>
```

## Ví Dụ Sử Dụng

```powershell
# Setup đầy đủ cho development
.\scripts\setup.ps1

# Reset hoàn toàn database
.\scripts\setup.ps1 -DropDatabase

# Chỉ chạy migrations
.\scripts\setup.ps1 -SkipSeeding

# Chỉ seeding dữ liệu
.\scripts\setup.ps1 -SkipMigration

# Setup cho production (không khuyên dùng seeding)
.\scripts\setup.ps1 -Environment Production -SkipSeeding
```
