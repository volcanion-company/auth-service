# Volcanion Auth - Hybrid Authorization Service

## 🚀 Overview

Volcanion Auth is a production-ready authentication and authorization service built with .NET 9, implementing Clean Architecture and Domain-Driven Design (DDD). It features a hybrid authorization model combining RBAC, ABAC, and ReBAC.

## ✨ Key Features

### Architecture
- **Clean Architecture** with clear separation of concerns
- **Domain-Driven Design (DDD)** with aggregates, entities, and value objects
- **CQRS Pattern** with MediatR for command/query separation
- **Repository & Unit of Work** patterns for data access

### Authorization Models
- **RBAC** (Role-Based Access Control) - Traditional role-permission mapping
- **ABAC** (Attribute-Based Access Control) - Policy-based with dynamic attributes
- **ReBAC** (Relationship-Based Access Control) - Graph-based relationships
- **CBAC** (Context-Based Access Control) - Contextual authorization decisions

### Technology Stack
- **.NET 9** - Latest framework version
- **PostgreSQL Cluster** - 1 primary (write) + 2 replicas (read)
- **Redis** - Distributed caching and session management
- **JWT** - Access & Refresh token authentication
- **Serilog** - Structured logging
- **Prometheus** - Metrics and monitoring
- **Docker** - Containerization

## 📁 Project Structure

```
volcanion-auth-hybrid/
├── src/
│   ├── VolcanionAuth.Domain/          # Core business logic & entities
│   │   ├── Common/                     # Base classes, interfaces
│   │   ├── Entities/                   # Domain entities
│   │   ├── ValueObjects/               # Value objects
│   │   └── Events/                     # Domain events
│   ├── VolcanionAuth.Application/      # Application logic & CQRS
│   │   ├── Common/
│   │   │   ├── Interfaces/            # Abstractions
│   │   │   └── Behaviors/             # MediatR pipeline behaviors
│   │   └── Features/
│   │       ├── Authentication/        # Auth commands/queries
│   │       └── Authorization/         # Authz commands/queries
│   ├── VolcanionAuth.Infrastructure/   # External concerns
│   │   ├── Persistence/               # EF Core, DbContexts
│   │   ├── Caching/                   # Redis implementation
│   │   └── Security/                  # JWT, Password hashing
│   └── VolcanionAuth.API/              # Web API
│       └── Controllers/               # API endpoints
├── tests/
│   ├── VolcanionAuth.Domain.Tests/
│   ├── VolcanionAuth.Application.Tests/
│   └── VolcanionAuth.Integration.Tests/
├── docker-compose.yml                  # Multi-container setup
├── Dockerfile                          # API container
└── README.md
```

## 🏗️ Architecture Diagram

```
┌─────────────────────────────────────────────────┐
│                   API Layer                      │
│  Controllers → MediatR → Commands/Queries        │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│              Application Layer                   │
│  CQRS Handlers → Repositories → Unit of Work    │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│            Infrastructure Layer                  │
│  EF Core → PostgreSQL (Write + Read Replicas)   │
│  Redis Cache → JWT Service → Password Hasher    │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│               Domain Layer                       │
│  Entities → Value Objects → Domain Events       │
└─────────────────────────────────────────────────┘
```

## 🚦 Getting Started

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose
- PostgreSQL 16+
- Redis 7+

### Quick Start with Docker

```bash
# Clone the repository
git clone https://github.com/yourusername/volcanion-auth-hybrid.git
cd volcanion-auth-hybrid

# Start all services
docker-compose up -d

# Check service health
curl http://localhost:8080/health

# Access Swagger UI
# Open browser: http://localhost:8080/swagger

# Access Prometheus
# Open browser: http://localhost:9090

# Access Grafana
# Open browser: http://localhost:3000 (admin/admin)
```

### Local Development

```bash
# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update --project src/VolcanionAuth.Infrastructure --startup-project src/VolcanionAuth.API

# Run the API
dotnet run --project src/VolcanionAuth.API

# Run tests
dotnet test
```

## 📡 API Endpoints

### Authentication
```
POST /api/v1/authentication/register  - Register new user
POST /api/v1/authentication/login     - User login
POST /api/v1/authentication/logout    - User logout
POST /api/v1/authentication/refresh   - Refresh access token
```

### Authorization
```
POST   /api/v1/authorization/roles                      - Create role
POST   /api/v1/authorization/users/{id}/roles/{roleId}  - Assign role
POST   /api/v1/authorization/policies                   - Create policy
GET    /api/v1/authorization/users/{id}/permissions     - Get permissions
POST   /api/v1/authorization/evaluate                   - Evaluate policy
```

### Health Checks
```
GET /health       - Overall health status
GET /health/ready - Readiness probe
GET /health/live  - Liveness probe
```

### Metrics
```
GET /metrics - Prometheus metrics
```

## 🔐 Authentication Flow

```
1. User Registration
   └─> Email, Password, Name → Hash Password → Save User

2. User Login
   └─> Email, Password → Verify → Generate JWT (Access + Refresh)

3. API Request
   └─> Bearer Token → Validate JWT → Extract Claims → Authorize

4. Token Refresh
   └─> Refresh Token → Validate → Generate New Access Token
```

## 🛡️ Authorization Flow

```
Request → Extract User Claims
         │
         ├─> RBAC Check (Role-Permission)
         │   ├─ User has required permission? → Allow
         │   └─ No → Continue
         │
         ├─> ABAC Check (Policy Evaluation)
         │   ├─ Match policy conditions? → Allow/Deny
         │   └─ No match → Continue
         │
         └─> ReBAC Check (Relationship)
             ├─ Has required relationship? → Allow
             └─ No → Deny
```

## 📊 Database Schema

### Core Tables
- **Users** - User accounts
- **Roles** - Authorization roles
- **Permissions** - Granular permissions
- **Policies** - ABAC policies
- **UserRoles** - User-Role mapping
- **RolePermissions** - Role-Permission mapping
- **UserAttributes** - Dynamic user attributes
- **UserRelationships** - User relationship graph
- **LoginHistories** - Audit trail
- **RefreshTokens** - Token management

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "WriteDatabase": "Host=localhost;Port=5432;Database=volcanion_auth;",
    "ReadDatabase": "Host=localhost;Port=5433;Database=volcanion_auth;",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "SecretKey": "your-secret-key",
    "Issuer": "VolcanionAuth",
    "Audience": "VolcanionAuthAPI",
    "AccessTokenExpirationMinutes": "30"
  }
}
```

## 📈 Monitoring

### Prometheus Metrics
- HTTP request duration
- Database query performance
- Cache hit ratio
- Authentication success/failure rate
- Authorization decision latency

### Health Checks
- PostgreSQL write database
- PostgreSQL read replicas
- Redis cache
- API availability

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/VolcanionAuth.Domain.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 🚀 Deployment

### Docker
```bash
# Build image
docker build -t volcanion-auth:latest .

# Run container
docker run -p 8080:8080 volcanion-auth:latest
```

### Kubernetes
```bash
# Apply manifests (create your k8s manifests)
kubectl apply -f k8s/
```

## 📝 Best Practices

1. **Security**
   - JWT tokens with short expiration
   - Password hashing with BCrypt
   - HTTPS enforcement
   - CORS configuration

2. **Performance**
   - Read replicas for queries
   - Redis caching for sessions
   - Connection pooling
   - Query optimization

3. **Scalability**
   - Stateless API design
   - Horizontal scaling ready
   - Load balancing support
   - Database sharding support

4. **Observability**
   - Structured logging
   - Distributed tracing ready
   - Metrics collection
   - Health checks

## 🤝 Contributing

Contributions are welcome! Please read our contributing guidelines.

## 📄 License

This project is licensed under the MIT License.

## 📞 Support

For issues and questions, please open a GitHub issue.

## 🙏 Acknowledgments

Built with ❤️ using Clean Architecture principles and modern .NET practices.
