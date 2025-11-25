# Volcanion Auth - Architecture Documentation

## Table of Contents
1. [System Overview](#system-overview)
2. [Architecture Patterns](#architecture-patterns)
3. [Layer Descriptions](#layer-descriptions)
4. [Authorization Models](#authorization-models)
5. [Database Design](#database-design)
6. [Caching Strategy](#caching-strategy)
7. [Security Design](#security-design)
8. [Performance Optimization](#performance-optimization)
9. [Scalability](#scalability)
10. [Monitoring & Observability](#monitoring--observability)

---

## System Overview

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Load Balancer                         │
└────────────────────┬────────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
        ▼            ▼            ▼
   ┌────────┐  ┌────────┐  ┌────────┐
   │  API   │  │  API   │  │  API   │  (Stateless, Horizontally Scalable)
   │ Node 1 │  │ Node 2 │  │ Node 3 │
   └───┬────┘  └───┬────┘  └───┬────┘
       │           │           │
       └───────────┼───────────┘
                   │
       ┌───────────┼────────────┐
       │           │            │
       ▼           ▼            ▼
   ┌──────┐   ┌────────┐   ┌──────────┐
   │Redis │   │ Write  │   │   Read   │
   │Cache │   │ Master │   │ Replicas │
   └──────┘   │  PG    │   │  (2x)    │
              └────────┘   └──────────┘
```

### Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| API | .NET 9 Web API | RESTful endpoints |
| Application | MediatR | CQRS implementation |
| Domain | Pure .NET | Business logic |
| Infrastructure | EF Core 9 | Data access |
| Database (Write) | PostgreSQL 16 | Primary database |
| Database (Read) | PostgreSQL 16 | Read replicas (2x) |
| Cache | Redis 7 | Distributed caching |
| Monitoring | Prometheus | Metrics collection |
| Logging | Serilog | Structured logging |
| Container | Docker | Containerization |

---

## Architecture Patterns

### 1. Clean Architecture

```
┌─────────────────────────────────────────────────┐
│               Domain Layer (Core)                │
│  - Entities, Value Objects, Domain Events       │
│  - Business Rules, Aggregates                   │
│  - No external dependencies                     │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│            Application Layer                     │
│  - Use Cases (Commands/Queries)                 │
│  - Interfaces for external concerns             │
│  - Business orchestration logic                 │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│           Infrastructure Layer                   │
│  - Database implementations                      │
│  - External service integrations                │
│  - Cross-cutting concerns                       │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│              Presentation Layer                  │
│  - API Controllers                              │
│  - Request/Response DTOs                        │
│  - Authentication/Authorization                 │
└─────────────────────────────────────────────────┘
```

**Benefits:**
- Testability: Core logic independent of infrastructure
- Maintainability: Clear separation of concerns
- Flexibility: Easy to swap implementations
- Independence: Business rules isolated from frameworks

### 2. Domain-Driven Design (DDD)

**Aggregates:**
- `User` (Aggregate Root)
  - LoginHistory
  - RefreshToken
  - UserRole
  - UserAttribute
  - UserRelationship

- `Role` (Aggregate Root)
  - RolePermission

- `Permission` (Aggregate Root)

- `Policy` (Aggregate Root)

**Value Objects:**
- Email
- Password
- FullName

**Domain Events:**
- UserRegisteredEvent
- UserLoggedInEvent
- UserPasswordChangedEvent
- UserEmailVerifiedEvent
- UserLockedEvent
- RoleCreatedEvent
- PolicyCreatedEvent

### 3. CQRS (Command Query Responsibility Segregation)

```
┌──────────────┐          ┌──────────────┐
│   Commands   │          │   Queries    │
│  (Write)     │          │   (Read)     │
└──────┬───────┘          └──────┬───────┘
       │                         │
       ▼                         ▼
┌──────────────┐          ┌──────────────┐
│  Write DB    │──Sync──▶ │   Read DB    │
│  (Primary)   │          │  (Replicas)  │
└──────────────┘          └──────────────┘
```

**Command Examples:**
- RegisterUserCommand
- LoginUserCommand
- CreateRoleCommand
- AssignRoleCommand
- CreatePolicyCommand

**Query Examples:**
- GetUserQuery
- GetUserPermissionsQuery
- EvaluatePolicyQuery

---

## Layer Descriptions

### Domain Layer

**Responsibilities:**
- Define business entities and rules
- Implement value objects with validation
- Raise domain events
- Enforce invariants

**Key Components:**
- `Entity<TId>`: Base class for entities
- `AggregateRoot<TId>`: Base for aggregates
- `ValueObject`: Base for value objects
- `Result<T>`: Result pattern for error handling
- `IDomainEvent`: Domain event marker

### Application Layer

**Responsibilities:**
- Implement use cases
- Coordinate domain objects
- Define abstractions for infrastructure
- Handle cross-cutting concerns

**Key Components:**
- Command/Query Handlers (MediatR)
- FluentValidation validators
- AutoMapper profiles
- Pipeline behaviors (Validation, Logging)

### Infrastructure Layer

**Responsibilities:**
- Implement data persistence
- External service integrations
- Caching implementation
- Security services (JWT, password hashing)

**Key Components:**
- `WriteDbContext`: Primary database
- `ReadDbContext`: Read replicas
- `Repository<T>`: Generic repository
- `RedisCacheService`: Caching
- `JwtTokenService`: Token generation
- `HybridAuthorizationService`: Authorization logic

### API Layer

**Responsibilities:**
- HTTP endpoints
- Request/Response handling
- Authentication/Authorization middleware
- API versioning
- Health checks

**Key Components:**
- Controllers (Authentication, Authorization)
- Middleware pipeline
- Swagger/OpenAPI
- Health check endpoints
- Prometheus metrics endpoint

---

## Authorization Models

### 1. RBAC (Role-Based Access Control)

```
User ──▶ UserRole ──▶ Role ──▶ RolePermission ──▶ Permission
```

**Example:**
```
User: john@example.com
├─ Role: Editor
│  ├─ Permission: documents:read
│  ├─ Permission: documents:write
│  └─ Permission: documents:publish
└─ Role: Reviewer
   └─ Permission: documents:review
```

**Use Case:** Traditional role-based access where users inherit permissions from their assigned roles.

### 2. ABAC (Attribute-Based Access Control)

```
Policy Evaluation:
├─ User Attributes (department, clearance_level, location)
├─ Resource Attributes (classification, owner)
├─ Environment Attributes (time, ip_address)
└─ Action (read, write, delete)
```

**Example Policy:**
```json
{
  "name": "HighClearanceDocumentAccess",
  "resource": "documents",
  "action": "read",
  "effect": "Allow",
  "conditions": {
    "user.clearance_level": "high",
    "user.department": "IT",
    "resource.classification": "confidential",
    "environment.time": "09:00-17:00"
  }
}
```

**Use Case:** Fine-grained access control based on multiple attributes and contextual information.

### 3. ReBAC (Relationship-Based Access Control)

```
User A ─[manager]─▶ User B ─[owns]─▶ Document X
```

**Example:**
- User A is manager of User B
- User B owns Document X
- User A can access Document X because of manager relationship

**Use Case:** Access control based on graph relationships between entities.

### 4. Hybrid Model Decision Flow

```
Request → Extract User & Context
           │
           ├─▶ Check RBAC
           │   ├─ Has Permission? → ALLOW
           │   └─ No → Continue
           │
           ├─▶ Evaluate ABAC Policies
           │   ├─ Policy Matched?
           │   │   ├─ Effect: Allow → ALLOW
           │   │   └─ Effect: Deny → DENY
           │   └─ No Match → Continue
           │
           └─▶ Check ReBAC
               ├─ Has Relationship? → ALLOW
               └─ No → DENY
```

---

## Database Design

### Read/Write Separation

**Write Database (Primary):**
- Handles all INSERT, UPDATE, DELETE operations
- Synchronous replication to replicas
- Connection string: `ConnectionStrings:WriteDatabase`

**Read Databases (Replicas):**
- Handle all SELECT queries
- Load balanced across 2 replicas
- Eventually consistent
- Connection string: `ConnectionStrings:ReadDatabase`

### Entity Relationships

```sql
Users (1) ──── (N) UserRoles (N) ──── (1) Roles
  │                                      │
  │                                      │
(1)                                    (1)
  │                                      │
(N)                                    (N)
UserAttributes                    RolePermissions
                                        │
                                      (1)
                                        │
                                      (N)
                                  Permissions

Users (1) ──── (N) LoginHistories
Users (1) ──── (N) RefreshTokens
Users (1) ──── (N) UserRelationships (N) ──── (1) Users
```

### Indexing Strategy

```sql
-- High-frequency lookups
CREATE INDEX idx_users_email ON Users(Email);
CREATE INDEX idx_refresh_tokens_token ON RefreshTokens(Token);

-- Join optimization
CREATE INDEX idx_user_roles_user_id_role_id ON UserRoles(UserId, RoleId);
CREATE INDEX idx_role_permissions_role_id_permission_id ON RolePermissions(RoleId, PermissionId);

-- Query optimization
CREATE INDEX idx_policies_resource_action ON Policies(Resource, Action);
CREATE INDEX idx_login_histories_timestamp ON LoginHistories(Timestamp);

-- Composite indexes for common queries
CREATE INDEX idx_user_attributes_user_id_key ON UserAttributes(UserId, AttributeKey);
```

---

## Caching Strategy

### Cache Layers

1. **User Session Cache** (30 minutes)
   - Key: `user_session:{userId}`
   - Data: User info, roles, permissions
   - Invalidation: On logout, role change

2. **Permission Cache** (15 minutes)
   - Key: `user_permissions:{userId}`
   - Data: Aggregated user permissions
   - Invalidation: On role/permission change

3. **Policy Evaluation Cache** (10 minutes)
   - Key: `permission_check:{userId}:{resource}:{action}`
   - Data: Boolean result
   - Invalidation: On policy change

4. **Relationship Cache** (10 minutes)
   - Key: `relationship:{userId}:{targetUserId}:{type}`
   - Data: Boolean result
   - Invalidation: On relationship change

### Cache-Aside Pattern

```csharp
public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory)
{
    var cached = await _cache.GetAsync<T>(key);
    if (cached != null) return cached;
    
    var value = await factory();
    await _cache.SetAsync(key, value, expiration);
    return value;
}
```

---

## Security Design

### Password Security
- **Hashing Algorithm:** BCrypt with work factor 12
- **Validation:** Minimum 8 chars, uppercase, lowercase, digit, special char
- **Storage:** Hashed value only, never plaintext

### JWT Token Design

**Access Token (30 minutes):**
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "roles": ["Admin", "Editor"],
  "permissions": ["users:read", "users:write"],
  "exp": 1705324800
}
```

**Refresh Token:**
- Cryptographically secure random 64-byte string
- Stored in database with expiration
- Single-use with rotation
- Revocable

### Security Headers
```csharp
app.UseHsts();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
```

---

## Performance Optimization

### 1. Database Optimization
- Connection pooling
- Prepared statements (EF Core)
- Query splitting for complex joins
- Read replicas for query load distribution

### 2. Caching
- Redis for distributed caching
- Cache stampede prevention
- TTL-based expiration
- Cache warming for critical data

### 3. Query Optimization
```csharp
// Good: Projection to reduce data transfer
var users = await context.Users
    .Select(u => new { u.Id, u.Email })
    .ToListAsync();

// Good: AsNoTracking for read-only queries
var user = await context.Users
    .AsNoTracking()
    .FirstOrDefaultAsync(u => u.Id == userId);
```

### 4. Async/Await
All I/O operations use async/await pattern for better thread utilization.

---

## Scalability

### Horizontal Scaling
- Stateless API design
- No in-memory session state
- All state in Redis/PostgreSQL
- Load balancer ready

### Database Scaling
- Primary-Replica replication
- Read query load distribution
- Connection pooling
- Future: Sharding support for multi-tenancy

### Caching Scaling
- Redis cluster support
- Distributed cache
- Cache partitioning

---

## Monitoring & Observability

### Prometheus Metrics

```csharp
// HTTP metrics
http_request_duration_seconds
http_requests_total

// Database metrics
database_query_duration_seconds
database_connection_pool_size

// Cache metrics
cache_hits_total
cache_misses_total

// Business metrics
authentication_attempts_total
authentication_failures_total
authorization_decisions_total
```

### Structured Logging

```csharp
_logger.LogInformation(
    "User {UserId} logged in from {IpAddress}",
    userId,
    ipAddress);
```

### Health Checks

- **Liveness:** `/health/live` - Is the service running?
- **Readiness:** `/health/ready` - Is the service ready to accept traffic?
- **Overall:** `/health` - Detailed health status of all dependencies

### Tracing
- Correlation IDs for request tracking
- Distributed tracing ready
- Request/response logging

---

## Best Practices Implemented

1. ✅ **Clean Architecture** - Separation of concerns
2. ✅ **DDD** - Rich domain model
3. ✅ **CQRS** - Read/write separation
4. ✅ **Repository Pattern** - Data access abstraction
5. ✅ **Unit of Work** - Transaction management
6. ✅ **Result Pattern** - Explicit error handling
7. ✅ **Value Objects** - Immutable domain primitives
8. ✅ **Domain Events** - Decoupled communication
9. ✅ **Pipeline Behaviors** - Cross-cutting concerns
10. ✅ **Health Checks** - Operational monitoring

---

## Future Enhancements

- [ ] Multi-tenancy support
- [ ] OAuth2/OIDC integration
- [ ] Two-factor authentication (2FA)
- [ ] Rate limiting
- [ ] API gateway integration
- [ ] Event sourcing for audit trail
- [ ] GraphQL API
- [ ] Microservices decomposition
- [ ] Kubernetes deployment manifests
- [ ] Distributed tracing with OpenTelemetry
