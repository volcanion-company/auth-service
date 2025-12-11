# RBAC Guide - Role-Based Access Control

> Hướng dẫn chi tiết về hệ thống phân quyền dựa trên Role

## 📖 Tổng quan

**RBAC (Role-Based Access Control)** là mô hình phân quyền dựa trên **vai trò (Role)**. Thay vì gán quyền trực tiếp cho từng user, ta gán permissions cho roles, sau đó gán roles cho users.

### Hierarchy (Cấu trúc)

```
User → UserRole → Role → RolePermission → Permission
```

**Example:**
```
User: john@example.com
  ↓ has role
Role: ContentEditor
  ↓ has permissions
Permissions:
  - documents:read
  - documents:write
  - documents:delete
```

---

## 🧩 Core Components

### 1. Permission (Quyền)

Permission là đơn vị nhỏ nhất trong hệ thống phân quyền.

**Structure:**
```json
{
  "id": "uuid",
  "resource": "documents",  // Tài nguyên (resource)
  "action": "read",         // Hành động (action)
  "fullPermission": "documents:read"  // Format: resource:action
}
```

**Common Permission Patterns:**

#### CRUD Operations
```
users:create
users:read
users:update
users:delete
```

#### Admin Operations
```
users:manage      // Full admin control
roles:manage
policies:manage
```

#### Fine-grained Control
```
documents:read
documents:write
documents:delete
documents:publish
documents:approve
```

### 2. Role (Vai trò)

Role là tập hợp các permissions.

**Structure:**
```json
{
  "id": "uuid",
  "name": "ContentEditor",
  "description": "Can create, edit and publish documents",
  "isActive": true,
  "permissions": [
    "documents:create",
    "documents:read",
    "documents:update",
    "documents:publish"
  ]
}
```

**Common Roles:**

```
Admin           → Full system access
User            → Basic read access
ContentEditor   → Create/edit content
Moderator       → Review/approve content
Guest           → Limited read-only access
```

### 3. User-Role Assignment

Gán role cho user.

**Structure:**
```json
{
  "userId": "uuid",
  "roleId": "uuid",
  "assignedAt": "2024-11-25T10:00:00Z"
}
```

---

## 🚀 Implementation Guide

### Step 1: Create Permissions

**API:** `POST /api/v1/authorization/permissions`

```bash
# Create document permissions
curl -X POST http://localhost:5000/api/v1/authorization/permissions \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"resource": "documents", "action": "create"}'

curl -X POST http://localhost:5000/api/v1/authorization/permissions \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"resource": "documents", "action": "read"}'

curl -X POST http://localhost:5000/api/v1/authorization/permissions \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"resource": "documents", "action": "update"}'

curl -X POST http://localhost:5000/api/v1/authorization/permissions \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"resource": "documents", "action": "delete"}'
```

**Response:**
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "resource": "documents",
  "action": "create",
  "fullPermission": "documents:create"
}
```

### Step 2: Create Roles

**API:** `POST /api/v1/authorization/roles`

```bash
# Create ContentEditor role
curl -X POST http://localhost:5000/api/v1/authorization/roles \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "ContentEditor",
    "description": "Can create, read and update documents"
  }'
```

**Response:**
```json
{
  "id": "9b8a7f65-4321-40de-855c-f17ed2g91bf8",
  "name": "ContentEditor",
  "description": "Can create, read and update documents",
  "isActive": true
}
```

### Step 3: Assign Permissions to Role

**API:** `POST /api/v1/authorization/roles/{roleId}/permissions/{permissionId}`

```bash
ROLE_ID="9b8a7f65-4321-40de-855c-f17ed2g91bf8"

# Assign documents:create
PERM_ID_CREATE="7c9e6679-7425-40de-944b-e07fc1f90ae7"
curl -X POST "http://localhost:5000/api/v1/authorization/roles/$ROLE_ID/permissions/$PERM_ID_CREATE" \
  -H "Authorization: Bearer $TOKEN"

# Assign documents:read
PERM_ID_READ="8d1f7789-8536-51ef-a55d-g28fe2h02cg9"
curl -X POST "http://localhost:5000/api/v1/authorization/roles/$ROLE_ID/permissions/$PERM_ID_READ" \
  -H "Authorization: Bearer $TOKEN"

# Assign documents:update
PERM_ID_UPDATE="9e2g8890-9647-62fg-b66e-h39gf3i13dh0"
curl -X POST "http://localhost:5000/api/v1/authorization/roles/$ROLE_ID/permissions/$PERM_ID_UPDATE" \
  -H "Authorization: Bearer $TOKEN"
```

### Step 4: Assign Role to User

**API:** `POST /api/v1/authorization/users/{userId}/roles/{roleId}`

```bash
USER_ID="3fa85f64-5717-4562-b3fc-2c963f66afa6"
ROLE_ID="9b8a7f65-4321-40de-855c-f17ed2g91bf8"

curl -X POST "http://localhost:5000/api/v1/authorization/users/$USER_ID/roles/$ROLE_ID" \
  -H "Authorization: Bearer $TOKEN"
```

**Response:** `204 No Content` (Success)

### Step 5: Verify User Permissions

**API:** `GET /api/v1/authorization/users/{userId}/permissions`

```bash
curl -X GET "http://localhost:5000/api/v1/authorization/users/$USER_ID/permissions" \
  -H "Authorization: Bearer $TOKEN"
```

**Response:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "permissions": [
    {
      "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "resource": "documents",
      "action": "create",
      "fullPermission": "documents:create",
      "assignedVia": "Role: ContentEditor"
    },
    {
      "id": "8d1f7789-8536-51ef-a55d-g28fe2h02cg9",
      "resource": "documents",
      "action": "read",
      "fullPermission": "documents:read",
      "assignedVia": "Role: ContentEditor"
    },
    {
      "id": "9e2g8890-9647-62fg-b66e-h39gf3i13dh0",
      "resource": "documents",
      "action": "update",
      "fullPermission": "documents:update",
      "assignedVia": "Role: ContentEditor"
    }
  ],
  "total": 3
}
```

---

## 🎭 Common Role Templates

### 1. Admin Role (Full Access)

```json
{
  "name": "Admin",
  "description": "Full system administrator",
  "permissions": [
    "users:manage",
    "roles:manage",
    "permissions:manage",
    "policies:manage",
    "documents:manage"
  ]
}
```

### 2. Content Editor Role

```json
{
  "name": "ContentEditor",
  "description": "Create and edit content",
  "permissions": [
    "documents:create",
    "documents:read",
    "documents:update",
    "documents:publish"
  ]
}
```

### 3. Moderator Role

```json
{
  "name": "Moderator",
  "description": "Review and approve content",
  "permissions": [
    "documents:read",
    "documents:approve",
    "documents:reject",
    "comments:moderate"
  ]
}
```

### 4. Viewer Role (Read-Only)

```json
{
  "name": "Viewer",
  "description": "Read-only access",
  "permissions": [
    "documents:read",
    "users:read"
  ]
}
```

### 5. Guest Role (Limited)

```json
{
  "name": "Guest",
  "description": "Limited guest access",
  "permissions": [
    "public:read"
  ]
}
```

---

## 🔍 Permission Check Flow

### Authorization Flow

```
1. User requests resource/action
         ↓
2. Extract userId from JWT token
         ↓
3. Load user's roles (with caching - 15 min TTL)
         ↓
4. Load permissions from roles
         ↓
5. Check if permission exists: resource:action
         ↓
6. Return ALLOW or DENY
```

### Code Example (Attribute-based)

```csharp
[RequirePermission("documents:read")]
public IActionResult GetDocuments()
{
    // Only accessible if user has documents:read permission
    var documents = _documentService.GetAll();
    return Ok(documents);
}

[RequirePermission("documents:delete")]
public IActionResult DeleteDocument(Guid id)
{
    // Only accessible if user has documents:delete permission
    _documentService.Delete(id);
    return NoContent();
}
```

### Manual Permission Check

```csharp
// Using IUserContextService
public class DocumentService
{
    private readonly IUserContextService _userContext;

    public async Task<Document> GetDocument(Guid id)
    {
        if (!_userContext.HasPermission("documents:read"))
        {
            throw new ForbiddenException("Missing permission: documents:read");
        }

        return await _repository.GetByIdAsync(id);
    }
}
```

---

## 🗄️ Database Schema

### Permissions Table

```sql
CREATE TABLE permissions (
    id UUID PRIMARY KEY,
    resource VARCHAR(100) NOT NULL,
    action VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    UNIQUE(resource, action)
);
```

### Roles Table

```sql
CREATE TABLE roles (
    id UUID PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP
);
```

### RolePermissions Junction Table

```sql
CREATE TABLE role_permissions (
    role_id UUID REFERENCES roles(id) ON DELETE CASCADE,
    permission_id UUID REFERENCES permissions(id) ON DELETE CASCADE,
    assigned_at TIMESTAMP NOT NULL,
    PRIMARY KEY (role_id, permission_id)
);
```

### UserRoles Junction Table

```sql
CREATE TABLE user_roles (
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    role_id UUID REFERENCES roles(id) ON DELETE CASCADE,
    assigned_at TIMESTAMP NOT NULL,
    PRIMARY KEY (user_id, role_id)
);
```

---

## ⚡ Caching Strategy

### Permission Cache

**Cache Key Pattern:**
```
permission:{userId}:{resource}:{action}
```

**Example:**
```
permission:3fa85f64-5717-4562-b3fc-2c963f66afa6:documents:read
```

**Cache Configuration:**
- **Storage:** Redis
- **TTL:** 15 minutes
- **Strategy:** Cache-aside (lazy loading)

**Cache Invalidation:**
- On role assignment/removal
- On permission add/remove from role
- On user role change
- On role deactivation

### Implementation

```csharp
public async Task<bool> HasPermissionAsync(Guid userId, string resource, string action)
{
    var cacheKey = $"permission:{userId}:{resource}:{action}";
    
    // Try get from cache
    var cached = await _cache.GetStringAsync(cacheKey);
    if (cached != null)
    {
        return bool.Parse(cached);
    }
    
    // Load from database
    var hasPermission = await LoadPermissionFromDb(userId, resource, action);
    
    // Cache result (15 minutes)
    await _cache.SetStringAsync(cacheKey, hasPermission.ToString(), 
        new DistributedCacheEntryOptions 
        { 
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) 
        });
    
    return hasPermission;
}
```

---

## 🎯 Best Practices

### 1. Principle of Least Privilege

✅ **DO:**
- Chỉ gán minimum permissions cần thiết
- Tạo specific roles cho từng use case
- Review permissions định kỳ

❌ **DON'T:**
- Gán Admin role cho mọi user
- Tạo overly permissive roles
- Bỏ qua permission checks

### 2. Role Design

✅ **DO:**
- Đặt tên role rõ ràng, mô tả (ContentEditor, Moderator)
- Group permissions theo business function
- Document purpose của mỗi role

❌ **DON'T:**
- Tạo too many granular roles (role explosion)
- Đặt tên generic (Role1, Role2)
- Mix unrelated permissions trong 1 role

### 3. Permission Naming

✅ **DO:**
- Sử dụng pattern `resource:action`
- Consistent naming (lowercase, kebab-case)
- Clear action names (read, write, delete, manage)

❌ **DON'T:**
- Random naming (docsRead, documents_write)
- Vague actions (do, execute, run)
- Mix naming conventions

**Good Examples:**
```
documents:read
documents:write
users:manage
reports:generate
```

**Bad Examples:**
```
docs_r
writeDocument
UserManage
DoStuff
```

### 4. Multi-Role Assignment

Users có thể có nhiều roles:

```
User: john@example.com
Roles:
  - ContentEditor (documents:create, documents:read, documents:update)
  - Moderator (documents:approve, comments:moderate)

Effective Permissions (union):
  - documents:create
  - documents:read
  - documents:update
  - documents:approve
  - comments:moderate
```

### 5. Role Hierarchy (Future Enhancement)

```
Admin (inherits from Moderator)
  ↓
Moderator (inherits from ContentEditor)
  ↓
ContentEditor (inherits from Viewer)
  ↓
Viewer (base permissions)
```

Currently not implemented - requires additional schema changes.

---

## 🔧 Management Operations

### List All Roles

```bash
curl -X GET http://localhost:5000/api/v1/authorization/roles \
  -H "Authorization: Bearer $TOKEN"
```

### Get Role Details with Permissions

```bash
curl -X GET "http://localhost:5000/api/v1/authorization/roles/$ROLE_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Update Role

```bash
curl -X PUT "http://localhost:5000/api/v1/authorization/roles/$ROLE_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "ContentEditor",
    "description": "Updated: Can create, edit, publish and archive documents"
  }'
```

### Deactivate Role (Soft Delete)

```bash
curl -X DELETE "http://localhost:5000/api/v1/authorization/roles/$ROLE_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Get User's Roles

```bash
curl -X GET "http://localhost:5000/api/v1/authorization/users/$USER_ID/roles" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 📊 Reporting & Auditing

### Permission Report

Query to get permission usage:

```sql
SELECT 
    p.resource,
    p.action,
    COUNT(DISTINCT rp.role_id) as role_count,
    COUNT(DISTINCT ur.user_id) as user_count
FROM permissions p
LEFT JOIN role_permissions rp ON p.id = rp.permission_id
LEFT JOIN roles r ON rp.role_id = r.id AND r.is_active = true
LEFT JOIN user_roles ur ON r.id = ur.role_id
GROUP BY p.id, p.resource, p.action
ORDER BY user_count DESC;
```

### Role Assignment Report

```sql
SELECT 
    r.name as role_name,
    COUNT(ur.user_id) as user_count,
    COUNT(rp.permission_id) as permission_count
FROM roles r
LEFT JOIN user_roles ur ON r.id = ur.role_id
LEFT JOIN role_permissions rp ON r.id = rp.role_id
WHERE r.is_active = true
GROUP BY r.id, r.name
ORDER BY user_count DESC;
```

---

## 🆚 RBAC vs Direct Permission Assignment

### RBAC (Recommended)

✅ **Advantages:**
- Easier to manage (assign 1 role vs many permissions)
- Scalable (add new user → assign role)
- Consistent permissions across user groups
- Easier auditing

❌ **Disadvantages:**
- Less flexible for edge cases
- May need custom roles for special users

### Direct Permission Assignment

✅ **Advantages:**
- Maximum flexibility
- Fine-grained control per user

❌ **Disadvantages:**
- Hard to manage at scale
- Inconsistent permissions
- Difficult to audit
- Permission sprawl

**Recommendation:** Use RBAC as primary, combine with PBAC for exceptions.

---

## 🚨 Troubleshooting

### Issue: User has role but permission check fails

**Diagnosis:**
1. Verify role is active
2. Check role has required permission
3. Clear cache (Redis)
4. Verify user-role assignment

**Solution:**
```bash
# Check user roles
curl -X GET "http://localhost:5000/api/v1/authorization/users/$USER_ID/roles"

# Check role permissions
curl -X GET "http://localhost:5000/api/v1/authorization/roles/$ROLE_ID"

# Re-assign role
curl -X POST "http://localhost:5000/api/v1/authorization/users/$USER_ID/roles/$ROLE_ID"
```

### Issue: Permission cache not updating

**Cause:** Redis cache not invalidated

**Solution:**
```bash
# Manual cache clear (if Redis CLI available)
redis-cli FLUSHDB

# Or wait 15 minutes for TTL expiration
# Or restart API to clear in-memory cache
```

---

## 📚 Related Documentation

- [PBAC Guide](PBAC_GUIDE.md) - Policy-Based Access Control
- [Hybrid Authorization](HYBRID_AUTHORIZATION.md) - RBAC + PBAC combined
- [API Reference](API_REFERENCE.md) - Complete API documentation
- [Getting Started](GETTING_STARTED.md) - Setup guide

---

**RBAC provides the foundation for scalable, manageable authorization! 🎭**
