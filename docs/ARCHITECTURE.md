# Architecture Documentation

## 📐 System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Client Layer                            │
│  Web Apps, Mobile Apps, Third-party Services, Postman           │
└────────────────────────┬────────────────────────────────────────┘
                         │ HTTPS/REST API
┌────────────────────────▼────────────────────────────────────────┐
│                      API Gateway Layer                          │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Middleware Pipeline                                     │   │
│  │  1. Request Logging → 2. JWT Auth → 3. User Context      │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Controllers (v1)                                        │   │
│  │  • AuthenticationController  • UserManagementController  │   │
│  │  • RoleManagementController  • PermissionManagementCtrl  │   │
│  │  • PolicyManagementController • UserProfileController    │   │
│  └──────────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│                   Application Layer (CQRS)                      │
│  ┌─────────────────────────┐  ┌─────────────────────────────┐   │
│  │  Commands (Write)       │  │  Queries (Read)             │   │
│  │  • CreateUser           │  │  • GetUserById              │   │
│  │  • AssignRole           │  │  • GetUserPermissions       │   │
│  │  • CreatePolicy         │  │  • EvaluatePolicy           │   │
│  └───────────┬─────────────┘  └────────────┬────────────────┘   │
│              │                              │                   │
│  ┌───────────▼──────────────────────────────▼────────────────┐  │
│  │  MediatR Pipeline (Handlers, Validators, Behaviors)       │  │
│  └───────────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│                      Domain Layer (DDD)                         │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Aggregates & Entities                                    │   │
│  │ User, Role, Permission, Policy, UserRole, RolePermission │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Domain Events                                           │   │
│  │  UserCreated, RoleAssigned, PolicyEvaluated              │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Business Rules & Invariants                             │   │
│  └──────────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│                  Infrastructure Layer                           │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  Write DB       │  │  Read DB        │  │  Redis Cache    │  │
│  │  (PostgreSQL)   │  │  (PostgreSQL)   │  │                 │  │
│  │  • Users        │  │  • Read Models  │  │  • Permissions  │  │
│  │  • Roles        │  │  • Denormalized │  │  • User Context │  │
│  │  • Policies     │  │                 │  │  • Sessions     │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  External Services                                       │   │
│  │  • JWT Service  • Password Hasher  • Email Service       │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## 🏗️ Clean Architecture Layers

### 1. API Layer (Presentation)

**Responsibilities:**
- HTTP request/response handling
- Input validation (DataAnnotations)
- Authentication middleware
- Authorization filters
- Swagger documentation
- API versioning

**Key Components:**
```csharp
Controllers/
├── V1/
│   ├── AuthenticationController.cs      # Login, Register, Refresh
│   ├── UserManagementController.cs      # User CRUD operations
│   ├── RoleManagementController.cs      # Role management
│   ├── PermissionManagementController.cs # Permission management
│   ├── PolicyManagementController.cs    # Policy management (PBAC)
│   └── UserProfileController.cs         # User context demo

Middleware/
├── JwtAuthenticationMiddleware.cs       # JWT validation
├── UserContextMiddleware.cs             # Load permissions
└── RequestLoggingMiddleware.cs          # Request tracking

Filters/
├── RequirePermissionAttribute.cs        # RBAC filter
└── RequirePolicyAttribute.cs            # PBAC filter
```

**Dependencies:** Application Layer only (no Domain or Infrastructure)

### 2. Application Layer (Use Cases)

**Responsibilities:**
- Business logic orchestration
- CQRS implementation
- Input validation (FluentValidation)
- DTO mapping
- Cross-cutting concerns (logging, caching)

**Key Patterns:**
- **CQRS**: Commands (write) vs Queries (read)
- **MediatR**: Request/response pattern
- **Pipeline Behaviors**: Validation, logging, transaction

**Structure:**
```csharp
Features/
├── Authentication/
│   ├── Commands/
│   │   ├── Register/
│   │   │   ├── RegisterCommand.cs
│   │   │   ├── RegisterCommandHandler.cs
│   │   │   └── RegisterCommandValidator.cs
│   │   └── Login/
│   │       ├── LoginCommand.cs
│   │       └── LoginCommandHandler.cs
│   └── Queries/
│       └── GetUserProfile/
│
├── Authorization/
│   ├── Commands/
│   │   ├── CreateRole/
│   │   ├── AssignRole/
│   │   ├── CreatePolicy/
│   │   └── UpdatePolicy/
│   └── Queries/
│       ├── GetUserPermissions/
│       ├── EvaluatePolicy/
│       └── CheckAuthorization/

Common/
├── Interfaces/
│   ├── IAuthorizationService.cs
│   ├── IRepository.cs
│   └── IUnitOfWork.cs
└── Behaviors/
    ├── ValidationBehavior.cs
    └── LoggingBehavior.cs
```

**Dependencies:** Domain Layer only

### 3. Domain Layer (Core Business Logic)

**Responsibilities:**
- Domain entities & aggregates
- Business rules & invariants
- Domain events
- Value objects
- Domain exceptions

**Key Entities:**
```csharp
Entities/
├── User.cs              # Aggregate Root
│   └── Methods: Create(), UpdateProfile(), Deactivate()
├── Role.cs              # Aggregate Root
│   └── Methods: Create(), AddPermission(), RemovePermission()
├── Permission.cs        # Entity
│   └── Methods: Create()
├── Policy.cs            # Aggregate Root
│   └── Methods: Create(), Update(), Evaluate()
├── UserRole.cs          # Join Entity
└── RolePermission.cs    # Join Entity

Events/
├── UserCreatedEvent.cs
├── RoleAssignedEvent.cs
└── PolicyEvaluatedEvent.cs

Common/
├── Entity<T>.cs         # Base entity
├── AggregateRoot<T>.cs  # Base aggregate
├── Result.cs            # Result pattern
└── DomainException.cs
```

**No Dependencies** (Pure domain logic)

### 4. Infrastructure Layer (Technical Details)

**Responsibilities:**
- Database access (EF Core)
- Caching (Redis)
- External services
- Security implementations
- Configuration

**Key Components:**
```csharp
Persistence/
├── WriteDbContext.cs         # Write operations
├── ReadDbContext.cs          # Read operations (queries)
├── Configurations/
│   ├── UserConfiguration.cs
│   └── PolicyConfiguration.cs
└── Repositories/
    ├── Repository<T>.cs
    └── ReadRepository<T>.cs

Services/
├── AuthorizationService.cs   # RBAC + PBAC logic
├── PolicyEngineService.cs    # Policy evaluation
└── JwtTokenService.cs        # JWT generation

Caching/
└── RedisCacheService.cs

Security/
├── BcryptPasswordHasher.cs
└── JwtTokenService.cs
```

**Dependencies:** Application Layer, Domain Layer

## 🔄 CQRS Flow

### Command Flow (Write Operations)

```
HTTP POST /api/v1/authorization/roles
           ↓
Controller validates input
           ↓
Sends CreateRoleCommand to MediatR
           ↓
ValidationBehavior validates command
           ↓
CreateRoleCommandHandler executes
           ↓
Domain: Role.Create() + business rules
           ↓
Repository saves to WriteDb
           ↓
UnitOfWork commits transaction
           ↓
Domain events published
           ↓
Response returned to client
```

### Query Flow (Read Operations)

```
HTTP GET /api/v1/authorization/roles/{id}
           ↓
Controller sends GetRoleByIdQuery to MediatR
           ↓
GetRoleByIdQueryHandler executes
           ↓
ReadRepository queries ReadDb (optimized for reads)
           ↓
Check Redis cache first
           ↓
If cache miss → query database
           ↓
Map to DTO
           ↓
Cache result in Redis
           ↓
Response returned to client
```

## 🛡️ Authorization Architecture

### Hybrid RBAC + PBAC Flow

```
Authorization Request
        ↓
┌───────────────────┐
│ Check Policies    │ ← Context provided?
│ (PBAC - Priority) │   YES → Evaluate policies
└────────┬──────────┘
         │
         ├─→ Policy ALLOW → Grant Access ✅
         ├─→ Policy DENY  → Deny Access ❌
         └─→ No Match
                 ↓
┌────────────────────┐
│ Check Permissions  │ ← Fallback to RBAC
│ (RBAC - Cached)    │
└────────┬───────────┘
         │
         ├─→ Has Permission → Grant Access ✅
         └─→ No Permission  → Deny Access ❌
```

### Policy Evaluation Engine

```
Policy: {
  Resource: "documents",
  Action: "edit",
  Effect: "Allow",
  Priority: 100,
  Conditions: {
    "ownerId": "{userId}",
    "classification.in": ["Public", "Internal"]
  }
}

Context: {
  userId: "abc-123",
  ownerId: "abc-123",        ← Matches!
  classification: "Public"    ← Matches!
}

Result: ALLOW (by policy)
```

## 🗄️ Database Schema

### Core Tables

```sql
-- Users (Aggregate Root)
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP
);

-- Roles (Aggregate Root)
CREATE TABLE roles (
    id UUID PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP
);

-- Permissions (Entity)
CREATE TABLE permissions (
    id UUID PRIMARY KEY,
    resource VARCHAR(100) NOT NULL,
    action VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    UNIQUE(resource, action)
);

-- Policies (Aggregate Root)
CREATE TABLE policies (
    id UUID PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    resource VARCHAR(100) NOT NULL,
    action VARCHAR(100) NOT NULL,
    effect VARCHAR(10) NOT NULL, -- Allow/Deny
    conditions JSONB NOT NULL,
    priority INTEGER NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP
);

-- Many-to-Many Relationships
CREATE TABLE user_roles (
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    role_id UUID REFERENCES roles(id) ON DELETE CASCADE,
    assigned_at TIMESTAMP NOT NULL,
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE role_permissions (
    role_id UUID REFERENCES roles(id) ON DELETE CASCADE,
    permission_id UUID REFERENCES permissions(id) ON DELETE CASCADE,
    assigned_at TIMESTAMP NOT NULL,
    PRIMARY KEY (role_id, permission_id)
);

-- Indexes for performance
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_policies_resource_action ON policies(resource, action);
CREATE INDEX idx_policies_priority ON policies(priority DESC);
```

## 🔌 Integration Points

### External Dependencies

| Service | Purpose | Configuration |
|---------|---------|---------------|
| PostgreSQL | Primary database | ConnectionStrings:WriteDatabase |
| Redis | Caching & sessions | ConnectionStrings:Redis |
| SMTP (Future) | Email notifications | EmailSettings:* |
| Prometheus | Metrics collection | Built-in middleware |
| Serilog | Structured logging | Logging:* |

### API Versioning

```
/api/v1/auth/*              → Version 1.0
/api/v2/auth/*              → Version 2.0 (future)
```

## 🔐 Security Architecture

### Authentication Flow

```
1. User Login → Email + Password
         ↓
2. Validate credentials (BCrypt)
         ↓
3. Generate JWT token
   • Claims: UserId, Email, Roles
   • Expiration: 60 minutes
   • Signature: HS256
         ↓
4. Generate Refresh Token
   • Expiration: 7 days
   • Stored in Redis
         ↓
5. Return tokens to client
```

### JWT Token Structure

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-id-guid",
    "email": "user@example.com",
    "role": ["Admin", "User"],
    "iat": 1700000000,
    "exp": 1700003600,
    "iss": "VolcanionAuth",
    "aud": "VolcanionAuthAPI"
  },
  "signature": "..."
}
```

## 📊 Performance Considerations

### Caching Strategy

| Data Type | Cache Duration | Strategy |
|-----------|----------------|----------|
| User Permissions | 15 minutes | Cache-aside |
| Role Details | 30 minutes | Cache-aside |
| Policy List | No cache | Always fresh |
| User Session | Token lifetime | Write-through |

### Database Optimization

- **Write DB**: Master PostgreSQL for all write operations
- **Read DB**: Replica PostgreSQL for read-heavy queries
- **Indexes**: On frequently queried columns (email, resource, action)
- **Query Splitting**: EF Core split queries for complex joins

### CQRS Benefits

- **Scalability**: Scale reads and writes independently
- **Performance**: Optimized read models
- **Flexibility**: Different data models for reads vs writes

## 🧪 Testing Strategy

### Test Pyramid

```
         ╱╲
        ╱  ╲      E2E Tests (5%)
       ╱────╲     Integration Tests (15%)
      ╱──────╲    Unit Tests (80%)
     ╱────────╲
```

### Test Coverage

- **Domain Layer**: 100% coverage (business rules critical)
- **Application Layer**: 90% coverage (handlers, validators)
- **Infrastructure Layer**: 70% coverage (integration tests)
- **API Layer**: 60% coverage (E2E tests)

---

**Architecture follows:**
- ✅ SOLID Principles
- ✅ Clean Architecture
- ✅ Domain-Driven Design
- ✅ CQRS Pattern
- ✅ Repository Pattern
- ✅ Unit of Work Pattern
