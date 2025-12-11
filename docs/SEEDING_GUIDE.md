# Database Seeding Guide

This guide explains how to set up and seed the Volcanion Auth Service database with sample data.

## Overview

The database seeding system provides sample data for:
- **Roles**: Admin, Manager, User, Guest, Developer, Support
- **Permissions**: Comprehensive permissions for users, roles, policies, documents, orders, support, system, and reports
- **Role-Permission Mappings**: Predefined permission assignments for each role
- **Users**: 7 sample users with different roles
- **User Attributes**: Department, location, and level attributes for ABAC
- **Policies**: Sample PBAC policies for various scenarios

## Quick Start

### Method 1: Using the Setup Script (Recommended)

Run the PowerShell setup script to handle both migrations and seeding:

```powershell
# Full setup (migrate + seed)
.\scripts\setup.ps1

# Drop existing database and recreate
.\scripts\setup.ps1 -DropDatabase

# Only run migrations (skip seeding)
.\scripts\setup.ps1 -SkipSeeding

# Only seed data (skip migrations)
.\scripts\setup.ps1 -SkipMigration

# Use production environment
.\scripts\setup.ps1 -Environment Production
```

### Method 2: Using EF Core CLI

```powershell
# Navigate to the API project
cd src\VolcanionAuth.API

# Apply migrations
dotnet ef database update

# Build and run with seeding
dotnet build
dotnet run --environment Development -- seed
```

### Method 3: Manual Seeding via Code

You can also seed the database programmatically by calling the seeder in your code:

```csharp
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<WriteDbContext>();
var logger = services.GetRequiredService<ILogger<Program>>();

await DatabaseSeeder.SeedAllAsync(context, services, logger);
```

## Sample Data

### Sample Users

All users have verified emails except the Guest user.

| Email | Password | Roles | Department | Location | Level |
|-------|----------|-------|------------|----------|-------|
| admin@volcanion.com | Admin@123456 | Admin | IT | HQ | Senior |
| manager@volcanion.com | Manager@123456 | Manager, User | Sales | HQ | Manager |
| user1@volcanion.com | User@123456 | User | Sales | Branch-A | Junior |
| user2@volcanion.com | User@123456 | User | Marketing | Branch-B | Mid |
| guest@volcanion.com | Guest@123456 | Guest | - | - | - |
| developer@volcanion.com | Dev@123456 | Developer | IT | Remote | Senior |
| support@volcanion.com | Support@123456 | Support | Support | HQ | Mid |

### Sample Roles

1. **Admin**: Full system access with all permissions
2. **Manager**: Elevated privileges except system administration
3. **User**: Standard user with read/write access to documents, orders, and reports
4. **Guest**: Read-only access to most resources
5. **Developer**: Technical access to system and reports
6. **Support**: Access to support system and customer data

### Sample Permissions

Permissions are organized by resource and action:

- **Users**: read, write, delete, manage
- **Roles**: read, write, delete, assign
- **Permissions**: read, write, delete, assign
- **Policies**: read, write, delete, evaluate
- **Documents**: read, write, delete, edit, share
- **Orders**: read, write, delete, approve, cancel
- **Support**: access, read, write, resolve
- **System**: read, write, admin
- **Reports**: read, write, export

### Sample Policies

The system includes several pre-configured policies demonstrating PBAC:

1. **CanEditOwnDocument**: Users can edit documents they own
2. **CanApproveIfManager**: Managers can approve orders under $10,000
3. **BusinessHoursAccess**: Support system accessible during business hours (9 AM - 5 PM)
4. **CanViewDepartmentUsers**: Users can view others in their department
5. **DenyAccessToSensitive**: Explicit deny for sensitive resources
6. **LocationBasedAccess**: Access restricted to specific locations
7. **ManagerApprovalRequired**: High-value orders require manager approval

## Seeder Components

### Individual Seeders

1. **RoleSeeder**: Creates default roles
2. **PermissionSeeder**: Creates comprehensive permission set
3. **RolePermissionSeeder**: Maps permissions to roles
4. **PolicySeeder**: Creates sample PBAC policies
5. **UserSeeder**: Creates sample users with hashed passwords
6. **UserRoleSeeder**: Assigns roles to users
7. **UserAttributeSeeder**: Adds ABAC attributes to users

### DatabaseSeeder

The `DatabaseSeeder` class orchestrates all seeders in the correct order:

```
1. Roles
2. Permissions
3. Role-Permission relationships
4. Policies
5. Users
6. User-Role relationships
7. User Attributes
```

## Seeding Strategy

The seeding system follows these principles:

- **Idempotent**: Seeders check for existing data before inserting
- **Ordered**: Dependencies are seeded before dependents
- **Safe**: Uses value objects and domain entities to ensure validity
- **Logged**: All operations are logged for troubleshooting
- **Transactional**: Uses the WriteDbContext for consistency

## Development Workflow

### Initial Setup

```powershell
# Clone the repository
git clone <repository-url>
cd auth-service

# Run setup script
.\scripts\setup.ps1
```

### Reset Database

```powershell
# Drop and recreate with fresh data
.\scripts\setup.ps1 -DropDatabase
```

### Add More Sample Data

To add custom sample data:

1. Create a new seeder class in `src\VolcanionAuth.Infrastructure\Seeding\`
2. Follow the naming pattern: `{Entity}Seeder.cs`
3. Add the seeder call to `DatabaseSeeder.SeedAllAsync()`
4. Run the setup script

Example:

```csharp
public static class CustomEntitySeeder
{
    public static async Task SeedCustomEntitiesAsync(WriteDbContext context)
    {
        if (await context.Set<CustomEntity>().AnyAsync())
        {
            return;
        }

        // Create sample data
        var entities = new List<CustomEntity>
        {
            // ... your sample data
        };

        await context.Set<CustomEntity>().AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }
}
```

## Troubleshooting

### Seeding Fails

1. Check database connectivity in `appsettings.Development.json`
2. Ensure PostgreSQL is running
3. Verify migrations are up to date: `dotnet ef migrations list`
4. Check logs for specific error messages

### Duplicate Data

Seeders are idempotent and won't create duplicates. To reset:

```powershell
.\scripts\setup.ps1 -DropDatabase
```

### Permission Denied

Ensure the PostgreSQL user has CREATE and INSERT permissions:

```sql
GRANT ALL PRIVILEGES ON DATABASE volcanion_auth TO postgres;
```

## Production Considerations

**⚠️ WARNING**: Sample data is for development only. Do not use in production!

For production:

1. Remove or disable seeding in `Program.cs`
2. Create production users through the API
3. Define production roles and permissions carefully
4. Use environment-specific configuration

## Testing the Seeded Data

After seeding, you can test with the sample users:

```powershell
# Using the Postman collection
# Import: postman/Volcanion-Auth-Complete.postman_collection.json

# Or using curl
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@volcanion.com",
    "password": "Admin@123456"
  }'
```

## Additional Resources

- [Getting Started Guide](../docs/GETTING_STARTED.md)
- [API Reference](../docs/API_REFERENCE.md)
- [RBAC Guide](../docs/RBAC_GUIDE.md)
- [PBAC Guide](../docs/PBAC_GUIDE.md)
