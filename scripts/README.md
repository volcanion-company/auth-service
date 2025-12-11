# Scripts

This folder contains utility scripts for database setup and management.

## Files

### setup.ps1

PowerShell script for complete database setup including migrations and seeding.

**Usage:**

```powershell
# Full setup (migrate + seed)
.\scripts\setup.ps1

# Drop and recreate database
.\scripts\setup.ps1 -DropDatabase

# Only run migrations (skip seeding)
.\scripts\setup.ps1 -SkipSeeding

# Only seed data (skip migrations)
.\scripts\setup.ps1 -SkipMigration

# Use production environment
.\scripts\setup.ps1 -Environment Production
```

**Parameters:**

- `-DropDatabase`: Drops the existing database before creating a new one
- `-SkipMigration`: Skips database migrations
- `-SkipSeeding`: Skips data seeding
- `-Environment`: Specifies the environment (Default: Development)

**Prerequisites:**

- .NET SDK installed
- PostgreSQL running and accessible
- Correct connection strings in `appsettings.Development.json`

**What it does:**

1. Checks for .NET CLI and EF Core tools
2. Optionally drops the existing database
3. Runs EF Core migrations to create/update schema
4. Seeds the database with sample data
5. Displays created sample user credentials

### init-db.sql

SQL script for manual database initialization (alternative to EF migrations).

Contains:
- Index creation for performance optimization
- Basic role and permission seeding

**Note:** This is an alternative approach. The recommended way is to use EF Core migrations and the setup.ps1 script.

### setup-authorization-demo.ps1

Script for demonstrating authorization features (RBAC and PBAC).

### test-swagger.ps1

Script for testing Swagger/OpenAPI endpoints.

## Quick Start

For first-time setup:

```powershell
# From the root of the repository
.\scripts\setup.ps1
```

This will:
- Apply all database migrations
- Seed sample data (users, roles, permissions, policies)
- Display sample credentials for testing

## Sample Data

After running the setup script, you'll have:

- **7 sample users** with different roles
- **6 roles** (Admin, Manager, User, Guest, Developer, Support)
- **40+ permissions** covering all resources
- **Role-permission mappings** for RBAC
- **User attributes** for ABAC
- **Sample policies** for PBAC

See [SAMPLE_CREDENTIALS.md](../SAMPLE_CREDENTIALS.md) for login details.

## Troubleshooting

### EF Core tools not found

Install globally:
```powershell
dotnet tool install --global dotnet-ef
```

Update to latest:
```powershell
dotnet tool update --global dotnet-ef
```

### Connection errors

Check `appsettings.Development.json` and ensure:
- PostgreSQL is running
- Connection strings are correct
- Database user has required permissions

### Migration errors

List migrations:
```powershell
cd src\VolcanionAuth.API
dotnet ef migrations list
```

Remove last migration (if needed):
```powershell
dotnet ef migrations remove
```

## Additional Resources

- [Getting Started Guide](../docs/GETTING_STARTED.md)
- [Seeding Guide (English)](../docs/SEEDING_GUIDE.md)
- [Seeding Guide (Vietnamese)](../docs/SEEDING_GUIDE_VI.md)
- [Sample Credentials](../SAMPLE_CREDENTIALS.md)
