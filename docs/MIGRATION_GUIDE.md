# Database Migration Guide

## Prerequisites
- .NET 9 SDK installed
- PostgreSQL 16+ running
- EF Core tools installed

## Install EF Core Tools

```bash
dotnet tool install --global dotnet-ef
# or update if already installed
dotnet tool update --global dotnet-ef
```

## Create Initial Migration

```bash
# Navigate to Infrastructure project directory
cd src/VolcanionAuth.Infrastructure

# Create migration
dotnet ef migrations add InitialCreate --startup-project ../VolcanionAuth.API --output-dir Persistence/Migrations

# Review the migration files created in Persistence/Migrations/
```

## Apply Migration to Database

### Development Environment

```bash
# Apply migrations to write database
dotnet ef database update --startup-project ../VolcanionAuth.API --connection "Host=localhost;Port=5432;Database=volcanion_auth;Username=postgres;Password=postgres;"

# For read replica, migrations will be applied automatically via replication
```

### Using Docker Compose

```bash
# Start all services (migrations will run automatically)
docker-compose up -d

# Check migration status
docker-compose logs volcanion-auth-api

# If needed, run migrations manually
docker-compose exec volcanion-auth-api dotnet ef database update
```

## Migration Commands Reference

### Create a new migration
```bash
dotnet ef migrations add <MigrationName> --startup-project ../VolcanionAuth.API
```

### Remove last migration (if not applied)
```bash
dotnet ef migrations remove --startup-project ../VolcanionAuth.API
```

### List all migrations
```bash
dotnet ef migrations list --startup-project ../VolcanionAuth.API
```

### Update database to specific migration
```bash
dotnet ef database update <MigrationName> --startup-project ../VolcanionAuth.API
```

### Generate SQL script for migration
```bash
dotnet ef migrations script --startup-project ../VolcanionAuth.API --output migration.sql
```

### Rollback to previous migration
```bash
dotnet ef database update <PreviousMigrationName> --startup-project ../VolcanionAuth.API
```

## Seed Data

After applying migrations, you can seed initial data:

```bash
# Using the init script
psql -h localhost -U postgres -d volcanion_auth -f scripts/init-db.sql

# Or using PowerShell
Get-Content scripts/init-db.sql | psql -h localhost -U postgres -d volcanion_auth
```

## Production Migration Strategy

### Blue-Green Deployment

1. **Prepare new version**
   ```bash
   # Generate migration script
   dotnet ef migrations script --idempotent --startup-project ../VolcanionAuth.API --output production-migration.sql
   ```

2. **Review SQL script**
   - Check for breaking changes
   - Verify indexes and constraints
   - Test on staging environment

3. **Apply during maintenance window**
   ```bash
   # Backup database first
   pg_dump -h production-host -U postgres volcanion_auth > backup.sql
   
   # Apply migration
   psql -h production-host -U postgres -d volcanion_auth -f production-migration.sql
   ```

### Zero-Downtime Migration

For schema changes that don't break the old version:

1. Deploy new schema (compatible with both versions)
2. Deploy new application version
3. Remove old application version
4. Clean up old schema if needed

## Troubleshooting

### Migration already applied
```bash
# Force remove migration from database
dotnet ef database update <PreviousMigrationName> --startup-project ../VolcanionAuth.API
dotnet ef migrations remove --startup-project ../VolcanionAuth.API
```

### Connection issues
- Check connection string in appsettings.json
- Verify PostgreSQL is running: `pg_isready -h localhost -p 5432`
- Check firewall settings
- Verify credentials

### Permission errors
```bash
# Grant necessary permissions
psql -h localhost -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE volcanion_auth TO postgres;"
```

## Manual Database Setup (Alternative)

If EF Core migrations fail, you can create the schema manually:

```sql
-- Create database
CREATE DATABASE volcanion_auth;

-- Connect to database
\c volcanion_auth

-- Run the schema creation script
\i scripts/create-schema.sql

-- Run seed data
\i scripts/init-db.sql
```

## Backup and Restore

### Backup
```bash
# Full backup
pg_dump -h localhost -U postgres volcanion_auth > backup_$(date +%Y%m%d).sql

# Schema only
pg_dump -h localhost -U postgres --schema-only volcanion_auth > schema_backup.sql

# Data only
pg_dump -h localhost -U postgres --data-only volcanion_auth > data_backup.sql
```

### Restore
```bash
# Drop and recreate database
dropdb -h localhost -U postgres volcanion_auth
createdb -h localhost -U postgres volcanion_auth

# Restore from backup
psql -h localhost -U postgres volcanion_auth < backup_20240115.sql
```

## Monitoring Migrations

### Check migration history
```sql
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
```

### Check current schema version
```bash
dotnet ef migrations list --startup-project ../VolcanionAuth.API
```

## Best Practices

1. **Always backup before migration**
2. **Test migrations on staging first**
3. **Use idempotent scripts for production**
4. **Version control all migrations**
5. **Document breaking changes**
6. **Keep migrations small and focused**
7. **Never modify applied migrations**
8. **Use transactions for data migrations**

## Common Migration Patterns

### Adding a new table
```bash
dotnet ef migrations add AddAuditLogTable --startup-project ../VolcanionAuth.API
```

### Adding a column
```bash
dotnet ef migrations add AddUserPhoneNumber --startup-project ../VolcanionAuth.API
```

### Creating an index
```bash
dotnet ef migrations add AddIndexOnUserEmail --startup-project ../VolcanionAuth.API
```

### Data migration
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
        UPDATE Users 
        SET IsActive = true 
        WHERE IsActive IS NULL;
    ");
}
```
