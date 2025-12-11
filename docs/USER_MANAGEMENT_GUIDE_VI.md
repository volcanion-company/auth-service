# Hướng Dẫn Quản Lý Người Dùng

## Tổng Quan

Tính năng Quản lý Người dùng cung cấp các thao tác CRUD hoàn chỉnh để quản lý người dùng trong Volcanion Auth Service. Tính năng này triển khai Kiểm soát Truy cập Dựa trên Vai trò (RBAC) để đảm bảo chỉ những người dùng được ủy quyền mới có thể thực hiện các thao tác quản lý người dùng.

## Quyền Yêu Cầu

Để sử dụng các endpoint quản lý người dùng, người dùng phải có các quyền sau:

- **users:read** - Xem thông tin người dùng (các endpoint GET)
- **users:write** - Tạo và cập nhật người dùng (các endpoint POST, PUT)
- **users:delete** - Xóa người dùng (endpoint DELETE)
- **users:manage** - Kích hoạt/vô hiệu hóa tài khoản người dùng (endpoint PATCH)

Các quyền này có thể được gán cho vai trò thông qua các endpoint quản lý phân quyền.

## Các API Endpoint

### 1. Lấy Danh Sách Tất Cả Người Dùng

**Endpoint:** `GET /api/v1/usermanagement`

**Quyền Yêu Cầu:** `users:read`

**Tham Số Query:**
- `page` (tùy chọn, mặc định: 1) - Số trang
- `pageSize` (tùy chọn, mặc định: 10, tối đa: 100) - Số mục trên mỗi trang
- `includeInactive` (tùy chọn, mặc định: false) - Bao gồm người dùng không hoạt động
- `searchTerm` (tùy chọn) - Tìm kiếm theo tên hoặc email

**Phản Hồi:**
```json
{
  "users": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "phoneNumber": null,
      "isActive": true,
      "createdAt": "2024-01-15T10:30:00Z",
      "lastLoginAt": "2024-01-20T14:25:00Z",
      "roles": [
        {
          "roleId": "role-id-here",
          "roleName": "User"
        }
      ]
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10
}
```

### 2. Lấy Thông Tin Người Dùng Theo ID

**Endpoint:** `GET /api/v1/usermanagement/{id}`

**Quyền Yêu Cầu:** `users:read`

**Phản Hồi:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": null,
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "lastLoginAt": "2024-01-20T14:25:00Z",
  "roles": [
    {
      "roleId": "role-id-here",
      "roleName": "User"
    }
  ]
}
```

### 3. Tạo Người Dùng Mới

**Endpoint:** `POST /api/v1/usermanagement`

**Quyền Yêu Cầu:** `users:write`

**Request Body:**
```json
{
  "email": "jane.smith@example.com",
  "password": "SecurePassword123!",
  "firstName": "Jane",
  "lastName": "Smith",
  "phoneNumber": "+1234567890",
  "roleIds": [
    "role-id-1",
    "role-id-2"
  ]
}
```

**Phản Hồi:**
```json
{
  "userId": "new-user-id-here",
  "email": "jane.smith@example.com",
  "firstName": "Jane",
  "lastName": "Smith"
}
```

**Lưu Ý:**
- Mật khẩu sẽ tự động được mã hóa bằng BCrypt
- `phoneNumber` và `roleIds` là tùy chọn
- Email phải là duy nhất trong hệ thống
- Trả về 409 Conflict nếu email đã tồn tại

### 4. Cập Nhật Người Dùng

**Endpoint:** `PUT /api/v1/usermanagement/{id}`

**Quyền Yêu Cầu:** `users:write`

**Request Body:**
```json
{
  "userId": "user-id-here",
  "firstName": "Jane",
  "lastName": "Doe-Smith",
  "phoneNumber": "+1987654321"
}
```

**Phản Hồi:**
```json
{
  "userId": "user-id-here",
  "email": "jane.smith@example.com",
  "firstName": "Jane",
  "lastName": "Doe-Smith"
}
```

**Lưu Ý:**
- Tất cả các trường đều tùy chọn; chỉ các trường được cung cấp sẽ được cập nhật
- `userId` trong body phải khớp với `id` trong URL
- Email không thể thay đổi thông qua endpoint này
- Trường PhoneNumber hiện chưa được hỗ trợ trong entity User

### 5. Xóa Người Dùng

**Endpoint:** `DELETE /api/v1/usermanagement/{id}`

**Quyền Yêu Cầu:** `users:delete`

**Phản Hồi:** `204 No Content`

**Lưu Ý:**
- Xóa vĩnh viễn người dùng khỏi hệ thống
- Thao tác này không thể hoàn tác
- Tất cả dữ liệu liên quan (vai trò, thuộc tính, v.v.) sẽ bị xóa theo

### 6. Kích Hoạt/Vô Hiệu Hóa Người Dùng

**Endpoint:** `PATCH /api/v1/usermanagement/{id}/status`

**Quyền Yêu Cầu:** `users:manage`

**Request Body:**
```json
{
  "isActive": false
}
```

**Phản Hồi:**
```json
{
  "userId": "user-id-here",
  "isActive": false
}
```

**Lưu Ý:**
- Đặt `isActive: true` để kích hoạt người dùng
- Đặt `isActive: false` để vô hiệu hóa người dùng
- Người dùng bị vô hiệu hóa không thể đăng nhập
- Nên ưu tiên vô hiệu hóa thay vì xóa để giữ lại lịch sử kiểm toán

## Kiến Trúc

### Mô Hình CQRS

Tính năng quản lý người dùng tuân theo mô hình CQRS (Command Query Responsibility Segregation):

**Queries** (Thao tác Đọc):
- `GetAllUsersQuery` - Liệt kê người dùng với phân trang và lọc
- `GetUserByIdQuery` - Lấy chi tiết người dùng đơn lẻ

**Commands** (Thao tác Ghi):
- `CreateUserCommand` - Tạo người dùng mới
- `UpdateUserCommand` - Cập nhật thông tin người dùng
- `DeleteUserCommand` - Xóa người dùng
- `ToggleUserStatusCommand` - Kích hoạt/vô hiệu hóa người dùng

### Cấu Trúc Dự Án

```
src/VolcanionAuth.Application/Features/UserManagement/
├── Common/
│   └── UserDto.cs                    # DTOs được chia sẻ
├── Queries/
│   ├── GetAllUsers/
│   │   ├── GetAllUsersQuery.cs
│   │   └── GetAllUsersQueryHandler.cs
│   └── GetUserById/
│       ├── GetUserByIdQuery.cs
│       └── GetUserByIdQueryHandler.cs
└── Commands/
    ├── CreateUser/
    │   ├── CreateUserCommand.cs
    │   └── CreateUserCommandHandler.cs
    ├── UpdateUser/
    │   ├── UpdateUserCommand.cs
    │   └── UpdateUserCommandHandler.cs
    ├── DeleteUser/
    │   ├── DeleteUserCommand.cs
    │   └── DeleteUserCommandHandler.cs
    └── ToggleUserStatus/
        ├── ToggleUserStatusCommand.cs
        └── ToggleUserStatusCommandHandler.cs

src/VolcanionAuth.API/Controllers/V1/
└── UserManagementController.cs        # API endpoints
```

### Các Phụ Thuộc

Tính năng quản lý người dùng phụ thuộc vào:
- **MediatR** - Điều phối Command/Query
- **IRepository<User>** - Thao tác ghi
- **IReadRepository<User>** - Thao tác đọc
- **IPasswordHasher** - Mã hóa mật khẩu (BCrypt)
- **IUnitOfWork** - Quản lý giao dịch

## Bảo Mật

### Xác Thực
Tất cả các endpoint yêu cầu JWT token hợp lệ trong header Authorization:
```
Authorization: Bearer <your-jwt-token>
```

### Phân Quyền
Các endpoint được bảo vệ bởi attribute `[RequirePermission]` kiểm tra:
1. Người dùng đã được xác thực
2. Người dùng có quyền yêu cầu
3. Tài khoản người dùng đang hoạt động

### Các Phản Hồi Lỗi

**401 Unauthorized** - Không có JWT token hợp lệ
```json
{
  "error": "Unauthorized"
}
```

**403 Forbidden** - Người dùng thiếu quyền yêu cầu
```json
{
  "error": "User does not have permission: users:read"
}
```

**404 Not Found** - Không tìm thấy người dùng
```json
{
  "error": "User with ID 'xxx' was not found"
}
```

**409 Conflict** - Email đã tồn tại
```json
{
  "error": "A user with this email already exists"
}
```

**400 Bad Request** - Lỗi validation
```json
{
  "error": "Page size must be between 1 and 100"
}
```

## Ví Dụ Sử Dụng

### Ví Dụ 1: Tạo Người Dùng Admin

```bash
# 1. Lấy JWT token (với tư cách superadmin)
curl -X POST http://localhost:5000/api/v1/authentication/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "superadmin@volcanion.com",
    "password": "SuperAdmin@2024"
  }'

# 2. Tạo người dùng admin mới
curl -X POST http://localhost:5000/api/v1/usermanagement \
  -H "Authorization: Bearer <your-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@company.com",
    "password": "AdminPass123!",
    "firstName": "System",
    "lastName": "Administrator",
    "roleIds": ["<admin-role-id>"]
  }'
```

### Ví Dụ 2: Liệt Kê Người Dùng Đang Hoạt Động

```bash
curl -X GET "http://localhost:5000/api/v1/usermanagement?page=1&pageSize=20&includeInactive=false" \
  -H "Authorization: Bearer <your-token>"
```

### Ví Dụ 3: Vô Hiệu Hóa Người Dùng

```bash
curl -X PATCH http://localhost:5000/api/v1/usermanagement/<user-id>/status \
  -H "Authorization: Bearer <your-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "isActive": false
  }'
```

### Ví Dụ 4: Tìm Kiếm Người Dùng

```bash
curl -X GET "http://localhost:5000/api/v1/usermanagement?searchTerm=john" \
  -H "Authorization: Bearer <your-token>"
```

## Thiết Lập Quyền Mẫu

Để kích hoạt quản lý người dùng, tạo các quyền sau và gán chúng cho các vai trò phù hợp:

```bash
# Tạo quyền
POST /api/v1/authorization/permissions
{
  "resource": "users",
  "action": "read",
  "description": "Xem thông tin người dùng"
}

POST /api/v1/authorization/permissions
{
  "resource": "users",
  "action": "write",
  "description": "Tạo và cập nhật người dùng"
}

POST /api/v1/authorization/permissions
{
  "resource": "users",
  "action": "delete",
  "description": "Xóa người dùng"
}

POST /api/v1/authorization/permissions
{
  "resource": "users",
  "action": "manage",
  "description": "Kích hoạt/vô hiệu hóa người dùng"
}

# Gán cho vai trò Admin
POST /api/v1/authorization/roles/<admin-role-id>/permissions/<permission-id>
```

## Thực Hành Tốt Nhất

1. **Sử Dụng Gán Vai Trò**
   - Luôn gán vai trò khi tạo người dùng
   - Không tạo người dùng mà không có vai trò nào

2. **Ưu Tiên Vô Hiệu Hóa Thay Vì Xóa**
   - Sử dụng `PATCH /status` để vô hiệu hóa người dùng
   - Chỉ xóa người dùng khi thực sự cần thiết
   - Vô hiệu hóa giữ lại lịch sử kiểm toán

3. **Bảo Mật Mật Khẩu**
   - Thực thi chính sách mật khẩu mạnh
   - Mật khẩu tự động được mã hóa bằng BCrypt
   - Không bao giờ ghi log hoặc hiển thị mật khẩu

4. **Phân Trang**
   - Luôn sử dụng phân trang cho các endpoint liệt kê
   - Giữ kích thước trang hợp lý (≤ 100)
   - Sử dụng từ khóa tìm kiếm để lọc tập kết quả lớn

5. **Xử Lý Lỗi**
   - Kiểm tra 409 Conflict khi tạo người dùng
   - Xử lý 404 Not Found cho các thao tác người dùng
   - Xác thực quyền trước khi thử thực hiện thao tác

## Khắc Phục Sự Cố

### Vấn đề: "403 Forbidden" khi gọi endpoint
**Giải pháp:** Đảm bảo người dùng của bạn có quyền yêu cầu (users:read, users:write, v.v.)

### Vấn đề: "409 Conflict" khi tạo người dùng
**Giải pháp:** Email đã tồn tại trong hệ thống. Sử dụng địa chỉ email khác.

### Vấn đề: Không thể cập nhật số điện thoại
**Giải pháp:** PhoneNumber hiện chưa được triển khai trong entity User. Trường này được dành cho sử dụng trong tương lai.

### Vấn đề: Người dùng đã tạo không thể đăng nhập
**Giải pháp:** 
1. Kiểm tra xem người dùng có đang hoạt động không (`isActive: true`)
2. Đảm bảo email đã được xác minh nếu yêu cầu
3. Xác minh mật khẩu đã được đặt đúng trong quá trình tạo

## Cải Tiến Trong Tương Lai

- [ ] Thêm endpoint thay đổi mật khẩu cho quản trị viên
- [ ] Triển khai thao tác hàng loạt người dùng (tạo, cập nhật, vô hiệu hóa)
- [ ] Thêm chức năng nhập/xuất người dùng
- [ ] Hỗ trợ ảnh đại diện người dùng
- [ ] Quy trình xác minh email
- [ ] Đặt lại mật khẩu bởi quản trị viên
- [ ] Nhật ký kiểm toán hoạt động người dùng
- [ ] Tùy chọn tìm kiếm và lọc nâng cao
- [ ] Hỗ trợ trường PhoneNumber

## Tài Liệu Liên Quan

- [Hướng Dẫn RBAC](RBAC_GUIDE.md) - Kiểm soát Truy cập Dựa trên Vai trò
- [Hướng Dẫn PBAC](PBAC_GUIDE.md) - Kiểm soát Truy cập Dựa trên Chính sách
- [Tham Khảo API](API_REFERENCE.md) - Tài liệu API đầy đủ
- [Bắt Đầu](GETTING_STARTED.md) - Hướng dẫn thiết lập ban đầu
- [Hướng Dẫn Seeding](SEEDING_GUIDE_VI.md) - Seeding cơ sở dữ liệu cho phát triển
