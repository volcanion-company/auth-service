# Volcanion Auth API - Usage Examples

## Authentication Examples

### 1. Register a New User

```bash
curl -X POST http://localhost:8080/api/v1/authentication/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecureP@ss123",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

**Response:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john.doe@example.com",
  "fullName": "John Doe"
}
```

### 2. Login

```bash
curl -X POST http://localhost:8080/api/v1/authentication/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecureP@ss123"
  }'
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "expiresAt": "2024-01-15T12:30:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john.doe@example.com",
  "fullName": "John Doe"
}
```

## Authorization Examples

### 3. Create a Role

```bash
curl -X POST http://localhost:8080/api/v1/authorization/roles \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{
    "name": "Editor",
    "description": "Can edit content"
  }'
```

### 4. Assign Role to User

```bash
curl -X POST http://localhost:8080/api/v1/authorization/users/{userId}/roles/{roleId} \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### 5. Create a Policy (ABAC)

```bash
curl -X POST http://localhost:8080/api/v1/authorization/policies \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{
    "name": "DepartmentDocumentAccess",
    "resource": "documents",
    "action": "read",
    "effect": "Allow",
    "conditions": "{\"department\":\"IT\",\"clearanceLevel\":\"high\"}",
    "priority": 10,
    "description": "Allow IT department with high clearance to read documents"
  }'
```

### 6. Get User Permissions

```bash
curl -X GET http://localhost:8080/api/v1/authorization/users/{userId}/permissions \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

**Response:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "roles": ["role-id-1", "role-id-2"],
  "permissions": [
    {
      "id": "perm-id-1",
      "resource": "users",
      "action": "read",
      "permissionString": "users:read"
    },
    {
      "id": "perm-id-2",
      "resource": "documents",
      "action": "write",
      "permissionString": "documents:write"
    }
  ]
}
```

### 7. Evaluate Policy

```bash
curl -X POST http://localhost:8080/api/v1/authorization/evaluate \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "resource": "documents",
    "action": "read",
    "context": {
      "department": "IT",
      "clearanceLevel": "high",
      "ipAddress": "192.168.1.100",
      "timeOfDay": "09:00"
    }
  }'
```

**Response:**
```json
{
  "isAllowed": true,
  "reason": "Allowed by policy"
}
```

## Health Check Examples

```bash
# Overall health
curl http://localhost:8080/health

# Readiness probe
curl http://localhost:8080/health/ready

# Liveness probe
curl http://localhost:8080/health/live
```

## Metrics

```bash
# Prometheus metrics
curl http://localhost:8080/metrics
```

## PowerShell Examples

### Register User
```powershell
$body = @{
    email = "jane.smith@example.com"
    password = "SecureP@ss456"
    firstName = "Jane"
    lastName = "Smith"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8080/api/v1/authentication/register" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

### Login
```powershell
$loginBody = @{
    email = "jane.smith@example.com"
    password = "SecureP@ss456"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:8080/api/v1/authentication/login" `
    -Method Post `
    -ContentType "application/json" `
    -Body $loginBody

$token = $response.accessToken
```

### Get Permissions with Token
```powershell
$headers = @{
    Authorization = "Bearer $token"
}

Invoke-RestMethod -Uri "http://localhost:8080/api/v1/authorization/users/$($response.userId)/permissions" `
    -Method Get `
    -Headers $headers
```
