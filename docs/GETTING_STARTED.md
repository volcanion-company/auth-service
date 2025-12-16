# Getting Started Guide

> Hướng dẫn chi tiết từng bước để bắt đầu với Volcanion Auth

## 📑 Mục lục

1. [Cài đặt môi trường](#1-cài-đặt-môi-trường)
2. [Chạy ứng dụng lần đầu](#2-chạy-ứng-dụng-lần-đầu)
3. [Tạo tài khoản và đăng nhập](#3-tạo-tài-khoản-và-đăng-nhập)
4. [Quản lý Role và Permission](#4-quản-lý-role-và-permission)
5. [Quản lý Policy](#5-quản-lý-policy)
6. [Kiểm tra phân quyền](#6-kiểm-tra-phân-quyền)
7. [Best Practices](#7-best-practices)

---

## 1. Cài đặt môi trường

### 1.1. Yêu cầu hệ thống

- ✅ **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download)
- ✅ **PostgreSQL 17+** - [Download](https://www.postgresql.org/download/)
- ✅ **Redis 7+** - [Download](https://redis.io/download)
- ✅ **Git** - [Download](https://git-scm.com/)
- 📝 **Postman** (optional) - [Download](https://www.postman.com/downloads/)
- 🐳 **Docker Desktop** (optional) - [Download](https://www.docker.com/products/docker-desktop)

### 1.2. Clone Repository

```bash
git clone https://github.com/yourusername/volcanion-auth-hybrid.git
cd volcanion-auth-hybrid
```

### 1.3. Setup Database với Docker (Recommended)

#### PostgreSQL

```bash
docker run -d \
  --name volcanion-postgres \
  -e POSTGRES_DB=volcanion_auth \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=YourSecurePassword123! \
  -p 5432:5432 \
  -v volcanion-postgres-data:/var/lib/postgresql/data \
  postgres:14-alpine
```

Kiểm tra:
```bash
docker ps | grep volcanion-postgres
docker logs volcanion-postgres
```

#### Redis

```bash
docker run -d \
  --name volcanion-redis \
  -p 6379:6379 \
  -v volcanion-redis-data:/data \
  redis:7-alpine redis-server --appendonly yes
```

Kiểm tra:
```bash
docker exec -it volcanion-redis redis-cli ping
# Response: PONG
```

### 1.4. Cấu hình ứng dụng

Tạo file `src/VolcanionAuth.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "WriteDatabase": "Host=localhost;Port=5432;Database=volcanion_auth;Username=postgres;Password=YourSecurePassword123!",
    "ReadDatabase": "Host=localhost;Port=5432;Database=volcanion_auth;Username=postgres;Password=YourSecurePassword123!",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-must-be-at-least-32-characters-long-for-security",
    "Issuer": "VolcanionAuth",
    "Audience": "VolcanionAuthAPI",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

⚠️ **Security Warning**: 
- Không commit file này lên Git
- Sử dụng User Secrets hoặc Environment Variables trong production

---

## 2. Chạy ứng dụng lần đầu

### 2.1. Restore Dependencies

```bash
cd src/VolcanionAuth.API
dotnet restore
```

### 2.2. Chạy Migration (Tạo Database Schema)

```bash
# Cài đặt EF Core Tools (nếu chưa có)
dotnet tool install --global dotnet-ef

# Chạy migrations
dotnet ef database update
```

Bạn sẽ thấy output:
```
Build started...
Build succeeded.
Applying migration '20241125_InitialCreate'.
Applying migration '20241125_AddPolicies'.
Done.
```

### 2.3. Seed dữ liệu mẫu (Optional)

Dữ liệu mẫu được tự động seed khi chạy ứng dụng lần đầu:
- ✅ 3 Permissions: users:read, users:write, users:delete
- ✅ 2 Roles: Admin, User
- ✅ 8 Policies mẫu (ownership, time-based, conditional)

### 2.4. Chạy ứng dụng

```bash
dotnet run
```

Output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

### 2.5. Kiểm tra Swagger

Mở trình duyệt: http://localhost:5000/swagger

Bạn sẽ thấy API documentation với các endpoint groups:
- 🔐 **Authentication** - /api/v1/auth
- 🎭 **Authorization** - /api/v1/authorization
- 👤 **UserProfile** - /api/v1/userprofile

---

## 3. Tạo tài khoản và đăng nhập

### 3.1. Register User mới

**Endpoint:** `POST /api/v1/auth/register`

**Request:**
```json
{
  "email": "admin@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (201 Created):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "admin@example.com",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Curl Command:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

### 3.2. Login để lấy JWT Token

**Endpoint:** `POST /api/v1/auth/login`

**Request:**
```json
{
  "email": "admin@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "def50200a1b2c3d4...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Curl Command:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "SecurePassword123!"
  }'
```

### 3.3. Sử dụng JWT Token

Sau khi login, lưu `accessToken` và sử dụng trong header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Ví dụ với Curl:**
```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X GET http://localhost:5000/api/v1/userprofile/me \
  -H "Authorization: Bearer $TOKEN"
```

### 3.4. Refresh Token khi hết hạn

**Endpoint:** `POST /api/v1/auth/refresh`

**Request:**
```json
{
  "refreshToken": "def50200a1b2c3d4..."
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "def50200e5f6g7h8...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

---

## 4. Quản lý Role và Permission

### 4.1. Tạo Permission mới

**Endpoint:** `POST /api/v1/authorization/permissions`

**Request:**
```json
{
  "resource": "documents",
  "action": "read"
}
```

**Response (201 Created):**
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "resource": "documents",
  "action": "read",
  "fullPermission": "documents:read"
}
```

**Các Permission thông dụng:**
```json
// CRUD operations
{ "resource": "documents", "action": "create" }
{ "resource": "documents", "action": "read" }
{ "resource": "documents", "action": "update" }
{ "resource": "documents", "action": "delete" }

// Admin operations
{ "resource": "users", "action": "manage" }
{ "resource": "roles", "action": "manage" }
{ "resource": "policies", "action": "manage" }
```

### 4.2. Tạo Role mới

**Endpoint:** `POST /api/v1/authorization/roles`

**Request:**
```json
{
  "name": "ContentEditor",
  "description": "Can create and edit documents"
}
```

**Response (201 Created):**
```json
{
  "id": "9b8a7f65-4321-40de-855c-f17ed2g91bf8",
  "name": "ContentEditor",
  "description": "Can create and edit documents",
  "isActive": true
}
```

### 4.3. Gán Permission cho Role

**Endpoint:** `POST /api/v1/authorization/roles/{roleId}/permissions/{permissionId}`

**Example:**
```bash
# Lấy roleId từ bước 4.2
ROLE_ID="9b8a7f65-4321-40de-855c-f17ed2g91bf8"

# Lấy permissionId từ bước 4.1
PERMISSION_ID="7c9e6679-7425-40de-944b-e07fc1f90ae7"

curl -X POST "http://localhost:5000/api/v1/authorization/roles/$ROLE_ID/permissions/$PERMISSION_ID" \
  -H "Authorization: Bearer $TOKEN"
```

**Response (204 No Content):**
```
Success - Permission assigned to role
```

### 4.4. Gán Role cho User

**Endpoint:** `POST /api/v1/authorization/users/{userId}/roles/{roleId}`

**Example:**
```bash
USER_ID="3fa85f64-5717-4562-b3fc-2c963f66afa6"
ROLE_ID="9b8a7f65-4321-40de-855c-f17ed2g91bf8"

curl -X POST "http://localhost:5000/api/v1/authorization/users/$USER_ID/roles/$ROLE_ID" \
  -H "Authorization: Bearer $TOKEN"
```

**Response (204 No Content):**
```
Success - Role assigned to user
```

### 4.5. Xem Permission của User

**Endpoint:** `GET /api/v1/authorization/users/{userId}/permissions`

**Response:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "permissions": [
    {
      "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "resource": "documents",
      "action": "read",
      "fullPermission": "documents:read",
      "assignedVia": "Role: ContentEditor"
    },
    {
      "id": "8d1f7789-8536-51ef-a55d-g28fe2h02cg9",
      "resource": "documents",
      "action": "create",
      "fullPermission": "documents:create",
      "assignedVia": "Role: ContentEditor"
    }
  ]
}
```

### 4.6. Xóa Permission khỏi Role

**Endpoint:** `DELETE /api/v1/authorization/roles/{roleId}/permissions/{permissionId}`

**Example:**
```bash
curl -X DELETE "http://localhost:5000/api/v1/authorization/roles/$ROLE_ID/permissions/$PERMISSION_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### 4.7. Xóa Role khỏi User

**Endpoint:** `DELETE /api/v1/authorization/users/{userId}/roles/{roleId}`

**Example:**
```bash
curl -X DELETE "http://localhost:5000/api/v1/authorization/users/$USER_ID/roles/$ROLE_ID" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 5. Quản lý Policy

### 5.1. Tạo Policy mới

**Endpoint:** `POST /api/v1/authorization/policies`

#### Example 1: Ownership Policy (User chỉ edit document của mình)

**Request:**
```json
{
  "name": "CanEditOwnDocument",
  "resource": "documents",
  "action": "edit",
  "effect": "Allow",
  "priority": 100,
  "conditions": {
    "ownerId": "{userId}"
  }
}
```

#### Example 2: Time-based Policy (Chỉ cho phép trong giờ làm việc)

**Request:**
```json
{
  "name": "BusinessHoursAccess",
  "resource": "api",
  "action": "access",
  "effect": "Allow",
  "priority": 50,
  "conditions": {
    "$timeRange": {
      "start": "09:00",
      "end": "18:00",
      "timezone": "Asia/Ho_Chi_Minh"
    }
  }
}
```

#### Example 3: Conditional Policy (Manager mới được approve)

**Request:**
```json
{
  "name": "CanApproveIfManager",
  "resource": "documents",
  "action": "approve",
  "effect": "Allow",
  "priority": 150,
  "conditions": {
    "$and": [
      { "userRole.contains": "Manager" },
      { "documentStatus": "Pending" }
    ]
  }
}
```

#### Example 4: Deny Policy (Cấm contractor xem confidential)

**Request:**
```json
{
  "name": "DenyContractorConfidential",
  "resource": "documents",
  "action": "view",
  "effect": "Deny",
  "priority": 300,
  "conditions": {
    "$and": [
      { "userType": "Contractor" },
      { "classification": "Confidential" }
    ]
  }
}
```

**Response (201 Created):**
```json
{
  "id": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
  "name": "CanEditOwnDocument",
  "resource": "documents",
  "action": "edit",
  "effect": "Allow",
  "priority": 100,
  "conditions": {
    "ownerId": "{userId}"
  },
  "isActive": true
}
```

### 5.2. Xem tất cả Policies

**Endpoint:** `GET /api/v1/authorization/policies`

**Query Parameters:**
- `includeInactive=true` - Include inactive policies
- `resource=documents` - Filter by resource
- `action=edit` - Filter by action

**Response:**
```json
{
  "policies": [
    {
      "id": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
      "name": "CanEditOwnDocument",
      "resource": "documents",
      "action": "edit",
      "effect": "Allow",
      "priority": 100,
      "isActive": true
    },
    {
      "id": "2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
      "name": "DenyContractorConfidential",
      "resource": "documents",
      "action": "view",
      "effect": "Deny",
      "priority": 300,
      "isActive": true
    }
  ],
  "total": 2
}
```

### 5.3. Xem chi tiết Policy

**Endpoint:** `GET /api/v1/authorization/policies/{id}`

**Response:**
```json
{
  "id": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
  "name": "CanEditOwnDocument",
  "resource": "documents",
  "action": "edit",
  "effect": "Allow",
  "priority": 100,
  "conditions": {
    "ownerId": "{userId}",
    "status.in": ["Draft", "InReview"]
  },
  "isActive": true,
  "createdAt": "2024-11-25T10:00:00Z",
  "updatedAt": "2024-11-25T10:00:00Z"
}
```

### 5.4. Cập nhật Policy

**Endpoint:** `PUT /api/v1/authorization/policies/{id}`

**Request:**
```json
{
  "name": "CanEditOwnDocument",
  "resource": "documents",
  "action": "edit",
  "effect": "Allow",
  "priority": 150,
  "conditions": {
    "ownerId": "{userId}",
    "status.in": ["Draft", "InReview"],
    "department": "{userDepartment}"
  }
}
```

### 5.5. Xóa Policy (Soft Delete)

**Endpoint:** `DELETE /api/v1/authorization/policies/{id}`

**Response (204 No Content):**
```
Success - Policy deactivated
```

### 5.6. Policy Condition Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `eq` | Equal | `{ "status": "Active" }` |
| `ne` | Not equal | `{ "status.ne": "Deleted" }` |
| `gt` | Greater than | `{ "age.gt": 18 }` |
| `gte` | Greater than or equal | `{ "score.gte": 80 }` |
| `lt` | Less than | `{ "price.lt": 1000 }` |
| `lte` | Less than or equal | `{ "quantity.lte": 10 }` |
| `in` | In array | `{ "status.in": ["Active", "Pending"] }` |
| `contains` | Contains substring | `{ "description.contains": "urgent" }` |
| `$and` | Logical AND | `{ "$and": [{...}, {...}] }` |
| `$or` | Logical OR | `{ "$or": [{...}, {...}] }` |
| `$timeRange` | Time range | `{ "$timeRange": {"start": "09:00", "end": "18:00"} }` |

---

## 6. Kiểm tra phân quyền

### 6.1. Check Authorization (Combined RBAC + PBAC)

**Endpoint:** `POST /api/v1/authorization/check`

**Request:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "resource": "documents",
  "action": "edit",
  "context": {
    "ownerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "classification": "Public",
    "status": "Draft"
  }
}
```

**Response (200 OK):**
```json
{
  "isAllowed": true,
  "reason": "Allowed by policy: CanEditOwnDocument",
  "authorizationType": "Policy"
}
```

**Possible authorizationType values:**
- `"Policy"` - Granted by policy evaluation
- `"Permission"` - Granted by RBAC permission
- `"None"` - Access denied

### 6.2. Test với Attribute trong Controller

```csharp
[RequirePermission("documents:read")]
public IActionResult GetDocuments()
{
    // Only accessible if user has documents:read permission
}

[RequirePolicy("documents", "edit")]
public IActionResult EditDocument(Guid id, [FromBody] UpdateDocumentDto dto)
{
    // Policy evaluated with context from request
}
```

### 6.3. Test Context từ JWT Token

**Endpoint:** `GET /api/v1/userprofile/me`

**Headers:**
```
Authorization: Bearer YOUR_JWT_TOKEN
```

**Response:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "admin@example.com",
  "roles": ["Admin", "ContentEditor"],
  "source": "Extension Methods"
}
```

**Other test endpoints:**
- `GET /api/v1/userprofile/permissions` - List all permissions
- `GET /api/v1/userprofile/check/permission/{permission}` - Check specific permission
- `GET /api/v1/userprofile/check/role/{role}` - Check if has role

---

## 7. Best Practices

### 7.1. Security Best Practices

✅ **JWT Token Management:**
- Store tokens securely (HttpOnly cookies hoặc secure storage)
- Không lưu trong localStorage nếu có thể
- Implement token refresh before expiration
- Revoke tokens when user logs out

✅ **Password Requirements:**
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character

✅ **HTTPS Only:**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001"
      }
    }
  }
}
```

### 7.2. Performance Best Practices

✅ **Caching Strategy:**
- Permissions cached for 15 minutes
- Invalidate cache on permission changes
- Use Redis for distributed caching

✅ **Database Optimization:**
- Use CQRS for read-heavy operations
- Index frequently queried columns
- Use read replicas for queries

### 7.3. Authorization Best Practices

✅ **Principle of Least Privilege:**
- Chỉ gán minimum permissions cần thiết
- Sử dụng roles để quản lý permissions theo groups
- Review permissions định kỳ

✅ **Policy Priority:**
- DENY policies: priority 200-300 (highest)
- Specific policies: priority 100-199
- General policies: priority 0-99

✅ **Policy Design:**
- Keep conditions simple and readable
- Document policy purpose in description
- Test policies with various contexts
- Use explicit DENY for security-critical resources

### 7.4. Development Workflow

1. **Local Development:**
   ```bash
   # Run with hot reload
   dotnet watch run
   ```

2. **Testing:**
   ```bash
   # Run all tests
   dotnet test
   
   # Run with coverage
   dotnet test /p:CollectCoverage=true
   ```

3. **Database Migrations:**
   ```bash
   # Create new migration
   dotnet ef migrations add MigrationName --project src/VolcanionAuth.Infrastructure --startup-project src/VolcanionAuth.API --output-dir Persistence/Migrations --context WriteDbContext
   
   # Update database
   dotnet ef database update --project src/VolcanionAuth.Infrastructure --startup-project src/VolcanionAuth.API --context WriteDbContext
   
   # Rollback
   dotnet ef database update PreviousMigrationName
   ```
4. **Seeding:**
   ```bash
   # Seeding with skip migration
   .\setup.ps1 -SkipMigration

5. **Logging:**
   ```bash
   # View structured logs
   docker logs volcanion-api --follow
   ```

---

## 🎯 Next Steps

Sau khi hoàn thành hướng dẫn này, bạn có thể:

1. ✅ Import Postman Collection (xem file `postman/Volcanion-Auth.postman_collection.json`)
2. ✅ Đọc [RBAC Guide](RBAC_GUIDE.md) để hiểu sâu về Role-Based Access Control
3. ✅ Đọc [PBAC Guide](PBAC_GUIDE.md) để hiểu về Policy-Based Access Control
4. ✅ Đọc [Hybrid Authorization](HYBRID_AUTHORIZATION.md) để hiểu cách RBAC + PBAC kết hợp
5. ✅ Xem [API Reference](API_REFERENCE.md) để biết tất cả endpoints

---

## 🆘 Troubleshooting

### Problem: Database connection failed

**Solution:**
```bash
# Check PostgreSQL is running
docker ps | grep postgres

# Check logs
docker logs volcanion-postgres

# Test connection
psql -h localhost -U postgres -d volcanion_auth
```

### Problem: Redis connection failed

**Solution:**
```bash
# Check Redis is running
docker exec -it volcanion-redis redis-cli ping

# Check logs
docker logs volcanion-redis
```

### Problem: JWT token invalid

**Solution:**
- Check `JwtSettings:SecretKey` is at least 32 characters
- Verify token hasn't expired
- Ensure `Issuer` and `Audience` match configuration

### Problem: Permission denied

**Solution:**
- Verify user has correct role assigned
- Check role has required permission
- Review policy conditions match context
- Check cache (may need to wait 15 minutes or clear Redis)

---

**Happy Coding! 🚀**
