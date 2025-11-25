# Sample Credentials - Development Only

## ⚠️ Security Warning

**These credentials are for development and testing purposes only!**
- Never use these credentials in production
- Never commit real credentials to version control
- Change all default passwords before deploying to production

## Sample User Accounts

### Administrator
- **Email**: `admin@volcanion.com`
- **Password**: `Admin@123456`
- **Roles**: Admin
- **Permissions**: All permissions
- **Attributes**:
  - Department: IT
  - Location: HQ
  - Level: Senior

### Manager
- **Email**: `manager@volcanion.com`
- **Password**: `Manager@123456`
- **Roles**: Manager, User
- **Permissions**: All except system administration
- **Attributes**:
  - Department: Sales
  - Location: HQ
  - Level: Manager

### Regular User 1
- **Email**: `user1@volcanion.com`
- **Password**: `User@123456`
- **Roles**: User
- **Permissions**: Standard user permissions
- **Attributes**:
  - Department: Sales
  - Location: Branch-A
  - Level: Junior

### Regular User 2
- **Email**: `user2@volcanion.com`
- **Password**: `User@123456`
- **Roles**: User
- **Permissions**: Standard user permissions
- **Attributes**:
  - Department: Marketing
  - Location: Branch-B
  - Level: Mid

### Guest User
- **Email**: `guest@volcanion.com`
- **Password**: `Guest@123456`
- **Roles**: Guest
- **Permissions**: Read-only access
- **Attributes**: None
- **Note**: Email NOT verified

### Developer
- **Email**: `developer@volcanion.com`
- **Password**: `Dev@123456`
- **Roles**: Developer
- **Permissions**: Technical and system access
- **Attributes**:
  - Department: IT
  - Location: Remote
  - Level: Senior

### Support Staff
- **Email**: `support@volcanion.com`
- **Password**: `Support@123456`
- **Roles**: Support
- **Permissions**: Customer support access
- **Attributes**:
  - Department: Support
  - Location: HQ
  - Level: Mid

## Password Requirements

All passwords must meet these requirements:
- At least 8 characters long
- Contains at least one uppercase letter
- Contains at least one lowercase letter
- Contains at least one digit
- Contains at least one special character (!@#$%^&*()_+-=[]{}|;:,.<>?)

## Database Connection Strings

### Development (Local)

**Write Database** (PostgreSQL):
```
Host=localhost;Port=5432;Database=volcanion_auth;Username=postgres;Password=postgres;
```

**Read Database** (PostgreSQL):
```
Host=localhost;Port=5433;Database=volcanion_auth;Username=postgres;Password=postgres;
```

**Redis Cache**:
```
localhost:6379
```

## JWT Configuration

### Development Settings

```json
{
  "JWT": {
    "SecretKey": "YourVerySecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm",
    "Issuer": "VolcanionAuth",
    "Audience": "VolcanionAuthAPI",
    "AccessTokenExpirationMinutes": "30",
    "RefreshTokenExpirationDays": "7"
  }
}
```

**⚠️ Important**: Change the `SecretKey` before deploying to production!

## Quick Login Test

### Using cURL

```bash
# Login as Admin
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@volcanion.com",
    "password": "Admin@123456"
  }'

# Login as Manager
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "manager@volcanion.com",
    "password": "Manager@123456"
  }'
```

### Using PowerShell

```powershell
# Login as Admin
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auth/login" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"email":"admin@volcanion.com","password":"Admin@123456"}'

$response | ConvertTo-Json
```

### Using Postman

1. Import the collection: `postman/Volcanion-Auth-Complete.postman_collection.json`
2. Import the environment: `postman/Volcanion-Auth-Local.postman_environment.json`
3. Use the "Login" request with any of the sample emails above

## Role-Based Access Control (RBAC)

### Role Hierarchy

1. **Admin** (Highest)
   - All system permissions
   - User management
   - System configuration

2. **Manager**
   - Most permissions except system admin
   - Team management
   - Approval workflows

3. **Developer**
   - Technical system access
   - Reports and analytics
   - Read-only user data

4. **Support**
   - Customer support tools
   - Ticket management
   - Read-only user and order data

5. **User**
   - Standard application features
   - Document management
   - Order creation

6. **Guest** (Lowest)
   - Read-only access
   - Limited features

## Policy-Based Access Control (PBAC)

### Sample Policies

Test these policies with the sample users:

1. **CanEditOwnDocument**
   - Users can edit their own documents
   - Test: Create a document as user1, try to edit it

2. **CanApproveIfManager**
   - Managers can approve orders under $10,000
   - Test: Create an order as user1, approve as manager

3. **BusinessHoursAccess**
   - Support system accessible 9 AM - 5 PM
   - Test: Access support endpoints during/outside business hours

4. **CanViewDepartmentUsers**
   - Users can view others in their department
   - Test: user1 and manager (both Sales) can see each other

## User Management Permissions

To use the User Management feature (see docs/USER_MANAGEMENT_GUIDE.md), create and assign these permissions:

| Permission | Resource | Action | Required For |
|------------|----------|--------|--------------|
| users:read | users | read | View user information |
| users:write | users | write | Create and update users |
| users:delete | users | delete | Delete users |
| users:manage | users | manage | Activate/deactivate users |

**Create permissions via API:**
```bash
POST /api/v1/permission-management
{
  "resource": "users",
  "action": "read",
  "description": "View user information"
}
```

**Assign to Admin role:**
```bash
POST /api/v1/role-management/{admin-role-id}/permissions/{permission-id}
```

**Note**: You can add these permissions to the seeder for automatic setup during development.

## Troubleshooting

### Cannot Login

1. Verify database is seeded: Check users table
2. Check password hash: Should be bcrypt hash
3. Verify email is verified (except guest)
4. Check account is active and not locked

### Invalid Token

1. Check JWT secret key matches in configuration
2. Verify token hasn't expired
3. Ensure correct issuer and audience

### Permission Denied

1. Check user's assigned roles
2. Verify role has required permissions
3. Review policy conditions for PBAC

## Clean Up

To reset everything:

```powershell
# Drop and recreate database
.\scripts\setup.ps1 -DropDatabase
```

---

**Remember**: Always use secure, unique credentials in production environments!
