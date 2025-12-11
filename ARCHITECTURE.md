# Architecture Documentation

## 📐 Overview

Volcanion Auth Service is built on **Clean Architecture** principles, ensuring separation of concerns, maintainability, and testability. The system implements **Domain-Driven Design (DDD)** patterns with **CQRS** for scalable and maintainable code.

## 🏗️ Architecture Layers

### 1. API Layer (`VolcanionAuth.API`)

**Responsibility**: HTTP request handling, routing, and response formatting

```
Controllers/
├── V1/
│   ├── AuthenticationController.cs      # Authentication endpoints
│   ├── UserManagementController.cs      # User CRUD operations
│   ├── RoleManagementController.cs      # Role management (RBAC)
│   ├── PermissionManagementController.cs # Permission management
│   ├── PolicyManagementController.cs    # Policy management (PBAC)
│   └── UserProfileController.cs         # User profile demo
Middleware/
├── JwtAuthenticationMiddleware.cs     # JWT token validation
├── RequestLoggingMiddleware.cs        # Request/response logging
└── UserContextMiddleware.cs           # User context population
Filters/
├── RequirePermissionAttribute.cs      # Permission-based authorization
└── RequirePolicyAttribute.cs          # Policy-based authorization
```

**Key Features**:
- RESTful API design with versioning (v1)
- JWT authentication via middleware
- Custom authorization filters for RBAC/PBAC
- Global exception handling
- Request/response logging
- Swagger/OpenAPI documentation

**Dependencies**: → Application Layer

---

### 2. Application Layer (`VolcanionAuth.Application`)

**Responsibility**: Business logic orchestration, use cases implementation

```
Features/
├── Authentication/
│   ├── Commands/
│   │   ├── RegisterUserCommand.cs
│   │   ├── LoginCommand.cs
│   │   ├── RefreshTokenCommand.cs
│   │   └── ResetPasswordCommand.cs
│   └── Queries/
│       └── ValidateTokenQuery.cs
├── Authorization/
│   ├── Commands/
│   │   ├── CreateRoleCommand.cs
│   │   ├── AssignRoleCommand.cs
│   │   ├── CreatePolicyCommand.cs
│   │   └── AssignPermissionCommand.cs
│   └── Queries/
│       ├── GetUserPermissionsQuery.cs
│       ├── GetRolesQuery.cs
│       └── EvaluatePolicyQuery.cs
└── UserProfile/
    ├── Commands/
    │   └── UpdateProfileCommand.cs
    └── Queries/
        └── GetUserProfileQuery.cs
Common/
├── Behaviors/
│   ├── ValidationBehavior.cs          # FluentValidation pipeline
│   ├── LoggingBehavior.cs             # Command/query logging
│   └── CachingBehavior.cs             # Response caching
├── Interfaces/
│   ├── IApplicationDbContext.cs       # Database abstraction
│   ├── ITokenService.cs               # JWT service interface
│   └── ICacheService.cs               # Cache service interface
└── Mappings/
    └── MappingProfile.cs              # AutoMapper profiles
```

**Key Patterns**:
- **CQRS**: Separate commands (write) from queries (read)
- **MediatR**: Pipeline for cross-cutting concerns
- **FluentValidation**: Request validation
- **AutoMapper**: DTO mapping

**Dependencies**: → Domain Layer, → Infrastructure Layer (interfaces only)

---

### 3. Domain Layer (`VolcanionAuth.Domain`)

**Responsibility**: Core business logic, entities, and domain rules

```
Entities/
├── User.cs                            # User aggregate root
├── Role.cs                            # Role entity
├── Permission.cs                      # Permission entity
├── Policy.cs                          # PBAC policy entity
├── UserRole.cs                        # User-Role relationship
├── RolePermission.cs                  # Role-Permission relationship
├── RefreshToken.cs                    # Token management
└── BaseEntity.cs                      # Base class with Id, timestamps

ValueObjects/
├── Email.cs                           # Email validation logic
├── HashedPassword.cs                  # Password hashing encapsulation
└── PolicyRule.cs                      # Policy rule evaluation

Events/
├── UserCreatedEvent.cs
├── RoleAssignedEvent.cs
├── PermissionGrantedEvent.cs
└── PolicyEvaluatedEvent.cs

Common/
├── Enums/
│   ├── PolicyEffect.cs               # Allow/Deny
│   └── TokenType.cs                  # Access/Refresh
└── Interfaces/
    └── IBaseEntity.cs
```

**Key Principles**:
- **Aggregate Roots**: User is the main aggregate
- **Value Objects**: Immutable objects for domain concepts
- **Domain Events**: Publish domain-significant events
- **Encapsulation**: Business rules within entities

**Dependencies**: None (Pure domain logic)

---

### 4. Infrastructure Layer (`VolcanionAuth.Infrastructure`)

**Responsibility**: External concerns (database, caching, security)

```
Persistence/
├── ApplicationDbContext.cs           # EF Core DbContext
├── Configurations/
│   ├── UserConfiguration.cs          # Entity mapping
│   ├── RoleConfiguration.cs
│   └── PermissionConfiguration.cs
├── Repositories/
│   ├── GenericRepository.cs          # Base repository
│   ├── UserRepository.cs
│   ├── RoleRepository.cs
│   └── PermissionRepository.cs
└── Migrations/                       # EF Core migrations

Caching/
├── RedisCacheService.cs              # Redis implementation
└── CacheKeys.cs                      # Cache key constants

Security/
├── TokenService.cs                   # JWT generation/validation
├── PasswordHasher.cs                 # BCrypt hashing
└── PasswordValidator.cs              # Password complexity

Services/
├── EmailService.cs                   # Email sending
├── PolicyEvaluator.cs                # PBAC evaluation engine
└── PermissionChecker.cs              # RBAC permission check

Seeding/
└── DatabaseSeeder.cs                 # Test data generation
```

**Key Technologies**:
- **Entity Framework Core 9**: ORM with PostgreSQL
- **Redis**: Distributed caching
- **BCrypt**: Password hashing
- **MailKit**: Email sending

**Dependencies**: → Domain Layer, → Application Layer (interface implementation)

---

## 🔄 Request Flow

### Example: User Login

```
1. Client → HTTP POST /api/v1/auth/login
   ↓
2. AuthController.Login()
   ↓
3. MediatR.Send(LoginCommand)
   ↓
4. Pipeline Behaviors:
   - ValidationBehavior (FluentValidation)
   - LoggingBehavior
   ↓
5. LoginCommandHandler
   - Retrieve user from repository
   - Validate password (BCrypt)
   - Generate JWT token
   - Create refresh token
   - Cache token in Redis
   ↓
6. Return LoginResponse (DTO)
   ↓
7. Controller → HTTP 200 OK with JWT
```

---

## 🗄️ Database Design

### Entity Relationship Diagram

```
┌─────────────────┐
│      User       │
├─────────────────┤
│ Id (PK)         │
│ Email           │◄──┐
│ PasswordHash    │   │
│ FirstName       │   │
│ LastName        │   │
│ IsEmailVerified │   │
│ CreatedAt       │   │
└─────────────────┘   │
         │            │
         │ 1:N        │
         ▼            │
┌─────────────────┐   │
│    UserRole     │   │
├─────────────────┤   │
│ UserId (FK)     │───┘
│ RoleId (FK)     │───┐
└─────────────────┘   │
                      │ N:1
                      ▼
              ┌─────────────────┐
              │      Role       │
              ├─────────────────┤
              │ Id (PK)         │
              │ Name            │◄──┐
              │ Description     │   │
              │ IsSystemRole    │   │
              └─────────────────┘   │
                       │            │
                       │ 1:N        │
                       ▼            │
              ┌─────────────────┐   │
              │ RolePermission  │   │
              ├─────────────────┤   │
              │ RoleId (FK)     │───┘
              │ PermissionId    │───┐
              └─────────────────┘   │ N:1
                                    ▼
                            ┌─────────────────┐
                            │   Permission    │
                            ├─────────────────┤
                            │ Id (PK)         │
                            │ Name            │
                            │ Resource        │
                            │ Action          │
                            │ Description     │
                            └─────────────────┘

┌─────────────────┐
│     Policy      │
├─────────────────┤
│ Id (PK)         │
│ Name            │
│ Description     │
│ Effect          │ (Allow/Deny)
│ Conditions      │ (JSON)
│ Resources       │ (JSON array)
│ Actions         │ (JSON array)
└─────────────────┘

┌─────────────────┐
│  RefreshToken   │
├─────────────────┤
│ Id (PK)         │
│ UserId (FK)     │───► User
│ Token           │
│ ExpiresAt       │
│ CreatedAt       │
│ RevokedAt       │
└─────────────────┘
```

---

## 🔐 Security Architecture

### Authentication Flow

```
1. User Login
   ↓
2. Validate credentials
   ↓
3. Generate JWT Access Token (60 min)
   ↓
4. Generate Refresh Token (7 days)
   ↓
5. Cache token metadata in Redis
   ↓
6. Return tokens to client
```

### Authorization Flow (RBAC)

```
1. Request with JWT
   ↓
2. JwtAuthenticationMiddleware validates token
   ↓
3. UserContextMiddleware loads user context
   ↓
4. RequirePermissionAttribute checks permission
   ↓
5. Query cached permissions (Redis)
   ↓
6. If not cached, load from DB and cache
   ↓
7. Allow/Deny access
```

### Authorization Flow (PBAC)

```
1. Request with JWT + Resource context
   ↓
2. Authentication middleware
   ↓
3. RequirePolicyAttribute evaluates policy
   ↓
4. Build evaluation context:
   - User attributes (role, department, etc.)
   - Resource attributes (owner, type, etc.)
   - Environment attributes (time, IP, etc.)
   ↓
5. PolicyEvaluator.Evaluate(context)
   ↓
6. Match policy conditions (JSON evaluation)
   ↓
7. Return Effect (Allow/Deny)
```

---

## 📊 Caching Strategy

### Cache Layers

1. **L1 Cache**: In-memory (ASP.NET Core MemoryCache)
   - Short-lived data (< 5 minutes)
   - User context during request

2. **L2 Cache**: Redis (Distributed)
   - JWT token metadata
   - User permissions
   - Role hierarchies
   - Policy definitions

### Cache Keys Pattern

```
VolcanionAuth:User:{userId}
VolcanionAuth:Permissions:{userId}
VolcanionAuth:Roles:{roleId}
VolcanionAuth:Token:{tokenId}
VolcanionAuth:Policy:{policyId}
```

### Cache Invalidation

- **Time-based**: TTL of 15-60 minutes
- **Event-based**: Invalidate on user/role updates
- **Manual**: Admin can flush specific caches

---

## 🔄 CQRS Implementation

### Command Pattern (Write Operations)

```csharp
public class RegisterUserCommand : IRequest<Result<UserDto>>
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class RegisterUserCommandHandler 
    : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(
        RegisterUserCommand request, 
        CancellationToken cancellationToken)
    {
        // 1. Validate business rules
        // 2. Create domain entity
        // 3. Save to database
        // 4. Publish domain event
        // 5. Return DTO
    }
}
```

### Query Pattern (Read Operations)

```csharp
public class GetUserPermissionsQuery : IRequest<Result<List<PermissionDto>>>
{
    public Guid UserId { get; set; }
}

public class GetUserPermissionsQueryHandler 
    : IRequestHandler<GetUserPermissionsQuery, Result<List<PermissionDto>>>
{
    public async Task<Result<List<PermissionDto>>> Handle(
        GetUserPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Check cache first
        // 2. If miss, query database
        // 3. Cache results
        // 4. Return DTOs
    }
}
```

---

## 🚀 Scalability Considerations

### Horizontal Scaling

- **Stateless API**: JWT tokens enable stateless authentication
- **Redis Clustering**: Distributed cache for multi-instance deployment
- **Database Read Replicas**: Separate read/write databases
- **Load Balancing**: Multiple API instances behind load balancer

### Performance Optimizations

- **Async/Await**: All I/O operations are asynchronous
- **Connection Pooling**: EF Core connection pooling
- **Compiled Queries**: EF Core compiled query cache
- **Response Compression**: Gzip/Brotli compression
- **Pagination**: All list endpoints support pagination

### Monitoring & Observability

- **Structured Logging**: Serilog with correlation IDs
- **Health Checks**: Database, Redis, application health
- **Metrics**: Prometheus metrics endpoint
- **Distributed Tracing**: (Planned: OpenTelemetry)

---

## 🧪 Testing Strategy

### Test Pyramid

```
        ┌───────────────────┐
        │  E2E Tests (5%)   │  Integration with real services
        ├───────────────────┤
        │ Integration (20%) │  API + Database + Cache
        ├───────────────────┤
        │  Unit Tests (75%) │  Domain logic, handlers
        └───────────────────┘
```

### Test Projects

1. **VolcanionAuth.Domain.Tests**: Domain logic, value objects
2. **VolcanionAuth.Application.Tests**: Handlers, validators, mappers
3. **VolcanionAuth.Integration.Tests**: API endpoints, database

---

## 📦 Deployment Architecture

### Docker Compose (Development)

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   API :5000  │───►│  PostgreSQL  │    │  Redis :6379 │
│              │    │   Primary    │    │              │
│  Swagger     │    │   :5432      │    │  Cache       │
│  /swagger    │    └──────┬───────┘    └──────────────┘
│              │           │
│  Health      │           │ Replication
│  /health     │           ▼
│              │    ┌──────────────┐
│  Metrics     │    │  PostgreSQL  │
│  /metrics    │    │   Replica    │
└──────────────┘    │   :5433      │
                    └──────────────┘
```

### Production (Kubernetes - Planned)

```
┌─────────────────────────────────────────┐
│          Load Balancer (Ingress)        │
└───────────────┬─────────────────────────┘
                │
      ┌─────────▼─────────┐
      │   API Pods (3x)   │
      │  Auto-scaling     │
      └─────────┬─────────┘
                │
    ┌───────────┼───────────┐
    ▼           ▼           ▼
┌────────┐  ┌─────────┐  ┌────────┐
│ Redis  │  │ Postgres│  │ Secrets│
│ Cluster│  │ Cluster │  │ Vault  │
└────────┘  └─────────┘  └────────┘
```

---

## 🔄 Data Flow Patterns

### Read Path (Query)

```
Request → Controller → MediatR → QueryHandler
                                     ↓
                                Cache Check (Redis)
                                     ↓
                              If miss: Database
                                     ↓
                              Cache Result
                                     ↓
                                Map to DTO
                                     ↓
                                 Response
```

### Write Path (Command)

```
Request → Controller → MediatR → ValidationBehavior
                                     ↓
                              CommandHandler
                                     ↓
                              Domain Logic
                                     ↓
                              Repository.Save
                                     ↓
                              UnitOfWork.Commit
                                     ↓
                              Publish Domain Event
                                     ↓
                              Invalidate Cache
                                     ↓
                                 Response
```

---

## 🛠️ Design Patterns Used

1. **Repository Pattern**: Data access abstraction
2. **Unit of Work**: Transaction management
3. **CQRS**: Command-Query separation
4. **Mediator**: Decoupled request handling
5. **Specification**: Complex query encapsulation
6. **Strategy**: Policy evaluation algorithms
7. **Factory**: Entity creation
8. **Builder**: Complex object construction
9. **Decorator**: MediatR pipeline behaviors
10. **Observer**: Domain events

---

## 📚 References

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [CQRS - Greg Young](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf)
- [Microsoft .NET Architecture Guides](https://dotnet.microsoft.com/learn/aspnet/architecture)

---

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for architecture guidelines and contribution workflow.
