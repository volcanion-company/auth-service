# User Management Feature Guide

## Overview

The User Management feature provides comprehensive CRUD operations for managing users in the Volcanion Auth Service. This feature implements Role-Based Access Control (RBAC) to ensure only authorized users can perform user management operations.

## Required Permissions

To use the user management endpoints, users must have the following permissions:

- **users:read** - View user information (GET endpoints)
- **users:write** - Create and update users (POST, PUT endpoints)
- **users:delete** - Delete users (DELETE endpoint)
- **users:manage** - Activate/deactivate user accounts (PATCH endpoint)

These permissions can be assigned to roles through the Authorization management endpoints.

## API Endpoints

### 1. Get All Users

**Endpoint:** `GET /api/v1/usermanagement`

**Permission Required:** `users:read`

**Query Parameters:**
- `page` (optional, default: 1) - Page number
- `pageSize` (optional, default: 10, max: 100) - Items per page
- `includeInactive` (optional, default: false) - Include inactive users
- `searchTerm` (optional) - Search by name or email

**Response:**
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

### 2. Get User By ID

**Endpoint:** `GET /api/v1/usermanagement/{id}`

**Permission Required:** `users:read`

**Response:**
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

### 3. Create User

**Endpoint:** `POST /api/v1/usermanagement`

**Permission Required:** `users:write`

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

**Response:**
```json
{
  "userId": "new-user-id-here",
  "email": "jane.smith@example.com",
  "firstName": "Jane",
  "lastName": "Smith"
}
```

**Notes:**
- Password will be automatically hashed using BCrypt
- `phoneNumber` and `roleIds` are optional
- Email must be unique in the system
- Returns 409 Conflict if email already exists

### 4. Update User

**Endpoint:** `PUT /api/v1/usermanagement/{id}`

**Permission Required:** `users:write`

**Request Body:**
```json
{
  "userId": "user-id-here",
  "firstName": "Jane",
  "lastName": "Doe-Smith",
  "phoneNumber": "+1987654321"
}
```

**Response:**
```json
{
  "userId": "user-id-here",
  "email": "jane.smith@example.com",
  "firstName": "Jane",
  "lastName": "Doe-Smith"
}
```

**Notes:**
- All fields are optional; only provided fields will be updated
- The `userId` in the body must match the `id` in the URL
- Email cannot be changed through this endpoint
- PhoneNumber field is currently not supported in the User entity

### 5. Delete User

**Endpoint:** `DELETE /api/v1/usermanagement/{id}`

**Permission Required:** `users:delete`

**Response:** `204 No Content`

**Notes:**
- Permanently deletes the user from the system
- This operation cannot be undone
- All associated data (roles, attributes, etc.) will be cascaded

### 6. Toggle User Status

**Endpoint:** `PATCH /api/v1/usermanagement/{id}/status`

**Permission Required:** `users:manage`

**Request Body:**
```json
{
  "isActive": false
}
```

**Response:**
```json
{
  "userId": "user-id-here",
  "isActive": false
}
```

**Notes:**
- Set `isActive: true` to activate a user
- Set `isActive: false` to deactivate a user
- Deactivated users cannot log in
- Preferable to deletion for audit trail purposes

## Architecture

### CQRS Pattern

The user management feature follows the CQRS (Command Query Responsibility Segregation) pattern:

**Queries** (Read Operations):
- `GetAllUsersQuery` - List users with pagination and filtering
- `GetUserByIdQuery` - Get single user details

**Commands** (Write Operations):
- `CreateUserCommand` - Create new user
- `UpdateUserCommand` - Update user information
- `DeleteUserCommand` - Delete user
- `ToggleUserStatusCommand` - Activate/deactivate user

### Project Structure

```
src/VolcanionAuth.Application/Features/UserManagement/
├── Common/
│   └── UserDto.cs                    # Shared DTOs
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

### Dependencies

The user management feature depends on:
- **MediatR** - Command/Query dispatching
- **IRepository<User>** - Write operations
- **IReadRepository<User>** - Read operations
- **IPasswordHasher** - Password hashing (BCrypt)
- **IUnitOfWork** - Transaction management

## Security

### Authentication
All endpoints require a valid JWT token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

### Authorization
Endpoints are protected by the `[RequirePermission]` attribute which checks:
1. User is authenticated
2. User has the required permission
3. User's account is active

### Error Responses

**401 Unauthorized** - No valid JWT token provided
```json
{
  "error": "Unauthorized"
}
```

**403 Forbidden** - User lacks required permission
```json
{
  "error": "User does not have permission: users:read"
}
```

**404 Not Found** - User not found
```json
{
  "error": "User with ID 'xxx' was not found"
}
```

**409 Conflict** - Email already exists
```json
{
  "error": "A user with this email already exists"
}
```

**400 Bad Request** - Validation error
```json
{
  "error": "Page size must be between 1 and 100"
}
```

## Usage Examples

### Example 1: Create Admin User

```bash
# 1. Get JWT token (as superadmin)
curl -X POST http://localhost:5000/api/v1/authentication/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "superadmin@volcanion.com",
    "password": "SuperAdmin@2024"
  }'

# 2. Create new admin user
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

### Example 2: List Active Users

```bash
curl -X GET "http://localhost:5000/api/v1/usermanagement?page=1&pageSize=20&includeInactive=false" \
  -H "Authorization: Bearer <your-token>"
```

### Example 3: Deactivate User

```bash
curl -X PATCH http://localhost:5000/api/v1/usermanagement/<user-id>/status \
  -H "Authorization: Bearer <your-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "isActive": false
  }'
```

### Example 4: Search Users

```bash
curl -X GET "http://localhost:5000/api/v1/usermanagement?searchTerm=john" \
  -H "Authorization: Bearer <your-token>"
```

## Sample Permissions Setup

To enable user management, create the following permissions and assign them to appropriate roles:

```bash
# Create permissions
POST /api/v1/authorization/permissions
{
  "resource": "users",
  "action": "read",
  "description": "View user information"
}

POST /api/v1/authorization/permissions
{
  "resource": "users",
  "action": "write",
  "description": "Create and update users"
}

POST /api/v1/authorization/permissions
{
  "resource": "users",
  "action": "delete",
  "description": "Delete users"
}

POST /api/v1/authorization/permissions
{
  "resource": "users",
  "action": "manage",
  "description": "Activate/deactivate users"
}

# Assign to Admin role
POST /api/v1/authorization/roles/<admin-role-id>/permissions/<permission-id>
```

## Best Practices

1. **Use Role-Based Assignment**
   - Always assign roles when creating users
   - Don't create users without any roles

2. **Prefer Deactivation Over Deletion**
   - Use `PATCH /status` to deactivate users
   - Only delete users when absolutely necessary
   - Deactivation maintains audit trails

3. **Password Security**
   - Enforce strong password policies
   - Passwords are automatically hashed with BCrypt
   - Never log or display passwords

4. **Pagination**
   - Always use pagination for list endpoints
   - Keep page size reasonable (≤ 100)
   - Use search term to filter large result sets

5. **Error Handling**
   - Check for 409 Conflict when creating users
   - Handle 404 Not Found for user operations
   - Validate permissions before attempting operations

## Troubleshooting

### Issue: "403 Forbidden" when calling endpoints
**Solution:** Ensure your user has the required permission (users:read, users:write, etc.)

### Issue: "409 Conflict" when creating user
**Solution:** Email already exists in the system. Use a different email address.

### Issue: Cannot update phone number
**Solution:** PhoneNumber is currently not implemented in the User entity. This field is reserved for future use.

### Issue: Created user cannot login
**Solution:** 
1. Check if user is active (`isActive: true`)
2. Ensure email is verified if required
3. Verify password was set correctly during creation

## Future Enhancements

- [ ] Add change password endpoint for administrators
- [ ] Implement bulk user operations (create, update, deactivate)
- [ ] Add user import/export functionality
- [ ] Support for user profile pictures
- [ ] Email verification workflow
- [ ] Password reset by administrator
- [ ] User activity audit logs
- [ ] Advanced search and filtering options
- [ ] PhoneNumber field support

## Related Documentation

- [RBAC Guide](RBAC_GUIDE.md) - Role-Based Access Control
- [PBAC Guide](PBAC_GUIDE.md) - Policy-Based Access Control
- [API Reference](API_REFERENCE.md) - Complete API documentation
- [Getting Started](GETTING_STARTED.md) - Initial setup guide
- [Seeding Guide](SEEDING_GUIDE.md) - Database seeding for development
