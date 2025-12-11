# API Reference

> Complete API documentation for all endpoints

**Base URL:** `http://localhost:5000/api/v1`

**API Version:** v1.0

---

## 📑 Table of Contents

1. [Authentication API](#authentication-api)
2. [Authorization API](#authorization-api)
3. [User Profile API](#user-profile-api)
4. [Common Response Codes](#common-response-codes)
5. [Error Handling](#error-handling)

---

## Authentication API

**Base Path:** `/api/v1/auth`

### Register User

Create a new user account.

**Endpoint:** `POST /api/v1/auth/register`

**Authentication:** Not required

**Request Body:**
```json
{
  "email": "string (required, email format, max 255 chars)",
  "password": "string (required, min 8 chars, must contain uppercase, lowercase, number, special char)",
  "firstName": "string (required, max 100 chars)",
  "lastName": "string (required, max 100 chars)"
}
```

**Success Response (201 Created):**
```json
{
  "userId": "uuid",
  "email": "string",
  "firstName": "string",
  "lastName": "string"
}
```

**Error Responses:**
- `400 Bad Request` - Validation failed (email already exists, weak password)
- `500 Internal Server Error` - Server error

---

### Login

Authenticate user and receive JWT tokens.

**Endpoint:** `POST /api/v1/auth/login`

**Authentication:** Not required

**Request Body:**
```json
{
  "email": "string (required)",
  "password": "string (required)"
}
```

**Success Response (200 OK):**
```json
{
  "accessToken": "string (JWT token)",
  "refreshToken": "string",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid email or password
- `403 Forbidden` - Account is inactive
- `500 Internal Server Error` - Server error

---

### Refresh Token

Get new access token using refresh token.

**Endpoint:** `POST /api/v1/auth/refresh`

**Authentication:** Not required

**Request Body:**
```json
{
  "refreshToken": "string (required)"
}
```

**Success Response (200 OK):**
```json
{
  "accessToken": "string (new JWT token)",
  "refreshToken": "string (new refresh token)",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid or expired refresh token
- `500 Internal Server Error` - Server error

---

### Logout

Revoke refresh token.

**Endpoint:** `POST /api/v1/auth/logout`

**Authentication:** Required (Bearer token)

**Request Body:**
```json
{
  "refreshToken": "string (required)"
}
```

**Success Response (204 No Content):**
```
(empty body)
```

**Error Responses:**
- `401 Unauthorized` - Invalid or missing access token
- `500 Internal Server Error` - Server error

---

## Authorization API

**Base Path:** `/api/v1/authorization`

### Permissions

#### Create Permission

**Endpoint:** `POST /api/v1/authorization/permissions`

**Authentication:** Required

**Request Body:**
```json
{
  "resource": "string (required, max 100 chars, e.g., 'documents')",
  "action": "string (required, max 100 chars, e.g., 'read')"
}
```

**Success Response (201 Created):**
```json
{
  "id": "uuid",
  "resource": "string",
  "action": "string",
  "fullPermission": "string (resource:action)"
}
```

---

#### Get All Permissions

**Endpoint:** `GET /api/v1/authorization/permissions`

**Authentication:** Required

**Success Response (200 OK):**
```json
{
  "permissions": [
    {
      "id": "uuid",
      "resource": "string",
      "action": "string",
      "fullPermission": "string"
    }
  ],
  "total": 0
}
```

---

#### Get Permission by ID

**Endpoint:** `GET /api/v1/authorization/permissions/{id}`

**Authentication:** Required

**Path Parameters:**
- `id` (uuid, required) - Permission ID

**Success Response (200 OK):**
```json
{
  "id": "uuid",
  "resource": "string",
  "action": "string",
  "fullPermission": "string",
  "createdAt": "datetime"
}
```

**Error Responses:**
- `404 Not Found` - Permission not found

---

#### Delete Permission

**Endpoint:** `DELETE /api/v1/authorization/permissions/{id}`

**Authentication:** Required

**Path Parameters:**
- `id` (uuid, required) - Permission ID

**Success Response (204 No Content):**
```
(empty body)
```

**Error Responses:**
- `404 Not Found` - Permission not found
- `409 Conflict` - Permission is in use

---

### Roles

#### Create Role

**Endpoint:** `POST /api/v1/authorization/roles`

**Authentication:** Required

**Request Body:**
```json
{
  "name": "string (required, max 100 chars, unique)",
  "description": "string (optional)"
}
```

**Success Response (201 Created):**
```json
{
  "id": "uuid",
  "name": "string",
  "description": "string",
  "isActive": true,
  "createdAt": "datetime"
}
```

**Error Responses:**
- `400 Bad Request` - Validation failed (name already exists)

---

#### Get All Roles

**Endpoint:** `GET /api/v1/authorization/roles`

**Authentication:** Required

**Query Parameters:**
- `includeInactive` (boolean, optional) - Include inactive roles (default: false)

**Success Response (200 OK):**
```json
{
  "roles": [
    {
      "id": "uuid",
      "name": "string",
      "description": "string",
      "isActive": true,
      "permissionCount": 0
    }
  ],
  "total": 0
}
```

---

#### Get Role by ID

**Endpoint:** `GET /api/v1/authorization/roles/{id}`

**Authentication:** Required

**Path Parameters:**
- `id` (uuid, required) - Role ID

**Success Response (200 OK):**
```json
{
  "id": "uuid",
  "name": "string",
  "description": "string",
  "isActive": true,
  "permissions": [
    {
      "id": "uuid",
      "resource": "string",
      "action": "string",
      "fullPermission": "string"
    }
  ],
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

---

#### Update Role

**Endpoint:** `PUT /api/v1/authorization/roles/{id}`

**Authentication:** Required

**Path Parameters:**
- `id` (uuid, required) - Role ID

**Request Body:**
```json
{
  "name": "string (optional, max 100 chars)",
  "description": "string (optional)"
}
```

**Success Response (200 OK):**
```json
{
  "id": "uuid",
  "name": "string",
  "description": "string",
  "isActive": true,
  "updatedAt": "datetime"
}
```

---

#### Delete Role

**Endpoint:** `DELETE /api/v1/authorization/roles/{id}`

**Authentication:** Required

**Path Parameters:**
- `id` (uuid, required) - Role ID

**Success Response (204 No Content):**
```
(empty body)
```

**Error Responses:**
- `404 Not Found` - Role not found
- `409 Conflict` - Role is assigned to users

---

#### Assign Permission to Role

**Endpoint:** `POST /api/v1/authorization/roles/{roleId}/permissions/{permissionId}`

**Authentication:** Required

**Path Parameters:**
- `roleId` (uuid, required) - Role ID
- `permissionId` (uuid, required) - Permission ID

**Success Response (204 No Content):**
```
(empty body)
```

**Error Responses:**
- `404 Not Found` - Role or Permission not found
- `409 Conflict` - Permission already assigned

---

#### Remove Permission from Role

**Endpoint:** `DELETE /api/v1/authorization/roles/{roleId}/permissions/{permissionId}`

**Authentication:** Required

**Path Parameters:**
- `roleId` (uuid, required) - Role ID
- `permissionId` (uuid, required) - Permission ID

**Success Response (204 No Content):**
```
(empty body)
```

---

### User-Role Assignment

#### Assign Role to User

**Endpoint:** `POST /api/v1/authorization/users/{userId}/roles/{roleId}`

**Authentication:** Required

**Path Parameters:**
- `userId` (uuid, required) - User ID
- `roleId` (uuid, required) - Role ID

**Success Response (204 No Content):**
```
(empty body)
```

**Error Responses:**
- `404 Not Found` - User or Role not found
- `409 Conflict` - Role already assigned

---

#### Remove Role from User

**Endpoint:** `DELETE /api/v1/authorization/users/{userId}/roles/{roleId}`

**Authentication:** Required

**Path Parameters:**
- `userId` (uuid, required) - User ID
- `roleId` (uuid, required) - Role ID

**Success Response (204 No Content):**
```
(empty body)
```

---

#### Get User Roles

**Endpoint:** `GET /api/v1/authorization/users/{userId}/roles`

**Authentication:** Required

**Path Parameters:**
- `userId` (uuid, required) - User ID

**Success Response (200 OK):**
```json
{
  "userId": "uuid",
  "roles": [
    {
      "id": "uuid",
      "name": "string",
      "description": "string",
      "assignedAt": "datetime"
    }
  ],
  "total": 0
}
```

---

#### Get User Permissions

**Endpoint:** `GET /api/v1/authorization/users/{userId}/permissions`

**Authentication:** Required

**Path Parameters:**
- `userId` (uuid, required) - User ID

**Success Response (200 OK):**
```json
{
  "userId": "uuid",
  "permissions": [
    {
      "id": "uuid",
      "resource": "string",
      "action": "string",
      "fullPermission": "string",
      "assignedVia": "string (e.g., 'Role: Admin')"
    }
  ],
  "total": 0
}
```

---

### Policies

#### Create Policy

**Endpoint:** `POST /api/v1/authorization/policies`

**Authentication:** Required

**Request Body:**
```json
{
  "name": "string (required, max 100 chars, unique)",
  "resource": "string (required, max 100 chars)",
  "action": "string (required, max 100 chars)",
  "effect": "string (required, enum: 'Allow' | 'Deny')",
  "priority": "integer (required, 0-1000, higher = evaluated first)",
  "conditions": {
    "key": "value",
    "key.operator": "value",
    "$and": [...],
    "$or": [...]
  }
}
```

**Example Request:**
```json
{
  "name": "CanEditOwnDocument",
  "resource": "documents",
  "action": "edit",
  "effect": "Allow",
  "priority": 100,
  "conditions": {
    "ownerId": "{userId}",
    "status.in": ["Draft", "InReview"]
  }
}
```

**Success Response (201 Created):**
```json
{
  "id": "uuid",
  "name": "string",
  "resource": "string",
  "action": "string",
  "effect": "string",
  "priority": 0,
  "conditions": {},
  "isActive": true,
  "createdAt": "datetime"
}
```

---

#### Get All Policies

**Endpoint:** `GET /api/v1/authorization/policies`

**Authentication:** Required

**Query Parameters:**
- `includeInactive` (boolean, optional) - Include inactive policies (default: false)
- `resource` (string, optional) - Filter by resource
- `action` (string, optional) - Filter by action

**Success Response (200 OK):**
```json
{
  "policies": [
    {
      "id": "uuid",
      "name": "string",
      "resource": "string",
      "action": "string",
      "effect": "string",
      "priority": 0,
      "isActive": true
    }
  ],
  "total": 0
}
```

---

#### Get Policy by ID

**Endpoint:** `GET /api/v1/authorization/policies/{id}`

**Authentication:** Required

**Path Parameters:**
- `id` (uuid, required) - Policy ID

**Success Response (200 OK):**
```json
{
  "id": "uuid",
  "name": "string",
  "resource": "string",
  "action": "string",
  "effect": "string",
  "priority": 0,
  "conditions": {},
  "isActive": true,
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

---

#### Update Policy

**Endpoint:** `PUT /api/v1/authorization/policies/{id}`

**Authentication:** Required

**Path Parameters:**
- `id` (uuid, required) - Policy ID

**Request Body:**
```json
{
  "name": "string (required)",
  "resource": "string (required)",
  "action": "string (required)",
  "effect": "string (required, enum: 'Allow' | 'Deny')",
  "priority": "integer (required)",
  "conditions": {}
}
```

**Success Response (200 OK):**
```json
{
  "id": "uuid",
  "name": "string",
  "resource": "string",
  "action": "string",
  "effect": "string",
  "priority": 0,
  "conditions": {},
  "isActive": true,
  "updatedAt": "datetime"
}
```

---

#### Delete Policy

**Endpoint:** `DELETE /api/v1/authorization/policies/{id}`

**Authentication:** Required

**Path Parameters:**
- `id` (uuid, required) - Policy ID

**Success Response (204 No Content):**
```
(empty body)
```

---

### Authorization Check

#### Check Authorization

Combined RBAC + PBAC authorization check.

**Endpoint:** `POST /api/v1/authorization/check`

**Authentication:** Required

**Request Body:**
```json
{
  "userId": "uuid (required)",
  "resource": "string (required)",
  "action": "string (required)",
  "context": {
    "key": "value",
    "..." : "..."
  }
}
```

**Example Request:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "resource": "documents",
  "action": "edit",
  "context": {
    "ownerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "classification": "Public",
    "status": "Draft",
    "department": "Engineering"
  }
}
```

**Success Response (200 OK):**
```json
{
  "isAllowed": true,
  "reason": "string (e.g., 'Allowed by policy: CanEditOwnDocument')",
  "authorizationType": "string (enum: 'None' | 'Permission' | 'Policy')"
}
```

**Authorization Flow:**
1. If `context` provided → Evaluate policies by priority
2. If policy matches with effect "Deny" → Return denied
3. If policy matches with effect "Allow" → Return allowed
4. If no policy matches → Check RBAC permissions
5. If has permission → Return allowed
6. Otherwise → Return denied

---

## User Profile API

**Base Path:** `/api/v1/userprofile`

### Get Current User (Extension Methods)

**Endpoint:** `GET /api/v1/userprofile/me`

**Authentication:** Required

**Success Response (200 OK):**
```json
{
  "userId": "uuid",
  "email": "string",
  "roles": ["string"],
  "source": "Extension Methods"
}
```

---

### Get User Info (HttpContext Extensions)

**Endpoint:** `GET /api/v1/userprofile/context`

**Authentication:** Required

**Success Response (200 OK):**
```json
{
  "userId": "uuid",
  "email": "string",
  "roles": ["string"],
  "requestId": "string",
  "source": "HttpContext Extensions"
}
```

---

### Get User Info (Attribute Injection)

**Endpoint:** `GET /api/v1/userprofile/info`

**Authentication:** Required

**Success Response (200 OK):**
```json
{
  "userId": "uuid",
  "email": "string",
  "roles": ["string"],
  "source": "CurrentUser Attribute"
}
```

---

### Get User Permissions (IUserContextService)

**Endpoint:** `GET /api/v1/userprofile/permissions`

**Authentication:** Required

**Success Response (200 OK):**
```json
{
  "userId": "uuid",
  "email": "string",
  "roles": ["string"],
  "permissions": ["string"],
  "source": "IUserContextService"
}
```

---

### Set Custom Context Data

**Endpoint:** `POST /api/v1/userprofile/context/custom`

**Authentication:** Required

**Request Body:**
```json
{
  "key": "string (required)",
  "value": "any (required)"
}
```

**Success Response (200 OK):**
```json
{
  "message": "Custom data stored",
  "key": "string",
  "value": "any"
}
```

---

### Get Custom Context Data

**Endpoint:** `GET /api/v1/userprofile/context/custom/{key}`

**Authentication:** Required

**Path Parameters:**
- `key` (string, required) - Custom data key

**Success Response (200 OK):**
```json
{
  "key": "string",
  "value": "any"
}
```

**Error Responses:**
- `404 Not Found` - Key not found in context

---

### Check Permission

**Endpoint:** `GET /api/v1/userprofile/check/permission/{permission}`

**Authentication:** Required

**Path Parameters:**
- `permission` (string, required) - Permission to check (format: resource:action)

**Success Response (200 OK):**
```json
{
  "permission": "string",
  "hasPermission": true
}
```

---

### Check Role

**Endpoint:** `GET /api/v1/userprofile/check/role/{role}`

**Authentication:** Required

**Path Parameters:**
- `role` (string, required) - Role name to check

**Success Response (200 OK):**
```json
{
  "role": "string",
  "hasRole": true
}
```

---

## Common Response Codes

| Code | Description | When it occurs |
|------|-------------|----------------|
| `200 OK` | Success | GET, PUT requests successful |
| `201 Created` | Resource created | POST requests successful |
| `204 No Content` | Success, no content | DELETE requests, assignment operations |
| `400 Bad Request` | Validation error | Invalid input, missing required fields |
| `401 Unauthorized` | Not authenticated | Missing or invalid JWT token |
| `403 Forbidden` | Not authorized | User doesn't have permission |
| `404 Not Found` | Resource not found | Requested resource doesn't exist |
| `409 Conflict` | Conflict | Duplicate resource, constraint violation |
| `500 Internal Server Error` | Server error | Unhandled exception |

---

## Error Handling

### Standard Error Response

All error responses follow this format:

```json
{
  "type": "string (error type URI)",
  "title": "string (human-readable title)",
  "status": 0,
  "detail": "string (detailed error message)",
  "instance": "string (request path)",
  "errors": {
    "fieldName": ["error message 1", "error message 2"]
  }
}
```

### Example Error Responses

#### 400 Bad Request (Validation Error)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is required.", "The Email field is not a valid e-mail address."],
    "Password": ["Password must be at least 8 characters long."]
  }
}
```

#### 401 Unauthorized

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid or expired token"
}
```

#### 403 Forbidden

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "User does not have permission: documents:delete"
}
```

#### 404 Not Found

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Role with ID '3fa85f64-5717-4562-b3fc-2c963f66afa6' not found"
}
```

#### 409 Conflict

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "A role with the name 'Admin' already exists"
}
```

#### 500 Internal Server Error

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "detail": "Contact support with request ID: 7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"
}
```

---

## Authentication Headers

All protected endpoints require the `Authorization` header:

```
Authorization: Bearer <access_token>
```

**Example:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJlbWFpbCI6ImFkbWluQGV4YW1wbGUuY29tIiwicm9sZSI6WyJBZG1pbiJdLCJpYXQiOjE3MDAwMDAwMDAsImV4cCI6MTcwMDAwMzYwMCwiaXNzIjoiVm9sY2FuaW9uQXV0aCIsImF1ZCI6IlZvbGNhbmlvbkF1dGhBUEkifQ.signature
```

---

## Rate Limiting

Currently not implemented. Future consideration:

- 100 requests per minute per IP
- 1000 requests per hour per user
- Response headers: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`

---

## Pagination

For list endpoints that support pagination (future):

**Query Parameters:**
- `page` (integer, default: 1) - Page number
- `pageSize` (integer, default: 20, max: 100) - Items per page
- `sortBy` (string) - Field to sort by
- `sortOrder` (string, enum: 'asc' | 'desc') - Sort direction

**Response Headers:**
- `X-Total-Count` - Total number of items
- `X-Page` - Current page
- `X-Page-Size` - Items per page
- `Link` - Links to first, prev, next, last pages

---

**API Version:** v1.0  
**Last Updated:** November 25, 2024
