# Volcanion Auth Service

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-336791?logo=postgresql)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7+-DC382D?logo=redis)](https://redis.io/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A comprehensive authentication and authorization service built with .NET 9, featuring RBAC (Role-Based Access Control), PBAC (Policy-Based Access Control), and Clean Architecture principles.

## 🌟 Features

### Core Authentication
- ✅ **JWT-based Authentication** - Secure token-based authentication with refresh tokens
- ✅ **User Management** - Complete CRUD operations for user accounts
- ✅ **Password Security** - BCrypt hashing with salt for secure password storage
- ✅ **Email Verification** - Email-based account verification system
- ✅ **Password Reset** - Secure password recovery flow

### Advanced Authorization
- ✅ **RBAC (Role-Based Access Control)** - Hierarchical role and permission management
- ✅ **PBAC (Policy-Based Access Control)** - Flexible attribute-based policies with JSON evaluation
- ✅ **Dynamic Permission Checking** - Runtime permission validation
- ✅ **Resource-Level Authorization** - Fine-grained access control at resource level

### Architecture & Design
- ✅ **Clean Architecture** - Clear separation of concerns (Domain, Application, Infrastructure, API)
- ✅ **CQRS Pattern** - Command Query Responsibility Segregation with MediatR
- ✅ **DDD Principles** - Domain-Driven Design with aggregates and domain events
- ✅ **Repository Pattern** - Generic repository with specification pattern
- ✅ **Unit of Work** - Transaction management across repositories

### Performance & Scalability
- ✅ **Redis Caching** - High-performance caching layer for tokens and permissions
- ✅ **Database Replication** - PostgreSQL primary-replica setup for read scalability
- ✅ **Connection Pooling** - Optimized database connection management
- ✅ **Asynchronous Operations** - Non-blocking I/O for better throughput

### Observability
- ✅ **Structured Logging** - Serilog with file and console output
- ✅ **Health Checks** - Database, Redis, and application health monitoring
- ✅ **Metrics** - Prometheus metrics endpoint for monitoring
- ✅ **API Versioning** - Version management for backward compatibility

### Developer Experience
- ✅ **Swagger/OpenAPI** - Interactive API documentation
- ✅ **Postman Collection** - Ready-to-use API collection with environments
- ✅ **Docker Support** - Complete Docker Compose setup for local development
- ✅ **Database Seeding** - Automated test data generation
- ✅ **Comprehensive Tests** - Unit, integration, and domain tests

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear layer separation:

```
┌─────────────────────────────────────────────────────────────────┐
│                         API Layer                               │
│  Controllers, Middleware, Filters, Attributes                   │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│                   Application Layer                             │
│  CQRS (Commands/Queries), Handlers, Validators, DTOs            │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│                      Domain Layer                               │
│  Entities, Value Objects, Domain Events, Interfaces             │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│                  Infrastructure Layer                           │
│  Persistence, Caching, Security, External Services              │
└─────────────────────────────────────────────────────────────────┘
```

For detailed architecture documentation, see [ARCHITECTURE.md](docs/ARCHITECTURE.md).

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 17+](https://www.postgresql.org/download/)
- [Redis 7+](https://redis.io/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional)

### Using Docker (Recommended)

1. **Clone the repository**
```bash
git clone https://github.com/volcanion-company/auth-service.git
cd auth-service
```

2. **Start all services**
```bash
docker-compose up -d
```

3. **Access the application**
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- Health Check: http://localhost:5000/health
- Metrics: http://localhost:5000/metrics

### Manual Setup

1. **Clone and restore**
```bash
git clone https://github.com/volcanion-company/auth-service.git
cd auth-service
dotnet restore
```

2. **Configure database**

Update `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=volcanion_auth;Username=postgres;Password=yourpassword"
  }
}
```

3. **Run migrations**
```bash
cd src/VolcanionAuth.API
dotnet ef database update
```

4. **Start the application**
```bash
dotnet run
```

For detailed setup instructions, see [GETTING_STARTED.md](docs/GETTING_STARTED.md).

## 📚 Documentation

- **[Getting Started](docs/GETTING_STARTED.md)** - Step-by-step setup guide
- **[Architecture](docs/ARCHITECTURE.md)** - Detailed architecture documentation
- **[API Reference](docs/API_REFERENCE.md)** - Complete API endpoint documentation
- **[RBAC Guide](docs/RBAC_GUIDE.md)** - Role-Based Access Control guide
- **[PBAC Guide](docs/PBAC_GUIDE.md)** - Policy-Based Access Control guide
- **[Contributing](CONTRIBUTING.md)** - How to contribute to this project

## 🔧 Configuration

### JWT Settings
```json
{
  "JWT": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "VolcanionAuth",
    "Audience": "VolcanionAuthUsers",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Redis Settings
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "VolcanionAuth:"
  }
}
```

### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=volcanion_auth;Username=postgres;Password=postgres",
    "ReplicaConnection": "Host=localhost;Port=5433;Database=volcanion_auth;Username=postgres;Password=postgres"
  }
}
```

## 🧪 Testing

### Run all tests
```bash
dotnet test
```

### Run specific test project
```bash
dotnet test tests/VolcanionAuth.Application.Tests
dotnet test tests/VolcanionAuth.Domain.Tests
dotnet test tests/VolcanionAuth.Integration.Tests
```

### Test coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 📦 API Endpoints

### Authentication
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/refresh-token` - Refresh access token
- `POST /api/v1/auth/verify-email` - Verify email address
- `POST /api/v1/auth/forgot-password` - Request password reset
- `POST /api/v1/auth/reset-password` - Reset password

### User Management
- `GET /api/v1/user-management` - Get all users (paginated)
- `GET /api/v1/user-management/{id}` - Get user by ID
- `POST /api/v1/user-management` - Create user
- `PUT /api/v1/user-management/{id}` - Update user
- `DELETE /api/v1/user-management/{id}` - Delete user

### Role Management (RBAC)
- `GET /api/v1/role-management` - Get all roles (paginated)
- `GET /api/v1/role-management/{id}` - Get role by ID
- `POST /api/v1/role-management` - Create role
- `PUT /api/v1/role-management/{id}` - Update role
- `DELETE /api/v1/role-management/{id}` - Delete role
- `POST /api/v1/role-management/{roleId}/users/{userId}` - Assign role to user
- `DELETE /api/v1/role-management/{roleId}/users/{userId}` - Remove role from user
- `POST /api/v1/role-management/{roleId}/permissions/{permissionId}` - Assign permission to role
- `DELETE /api/v1/role-management/{roleId}/permissions/{permissionId}` - Remove permission from role

### Permission Management
- `GET /api/v1/permission-management` - Get all permissions (paginated)
- `GET /api/v1/permission-management/grouped-by-resource` - Get permissions grouped by resource
- `POST /api/v1/permission-management` - Create permission

### Policy Management (PBAC)
- `GET /api/v1/policy-management` - Get all policies (paginated)
- `GET /api/v1/policy-management/{id}` - Get policy by ID
- `POST /api/v1/policy-management` - Create policy
- `PUT /api/v1/policy-management/{id}` - Update policy
- `DELETE /api/v1/policy-management/{id}` - Delete policy
- `POST /api/v1/policy-management/evaluate` - Evaluate policy

### User Profile
- `GET /api/v1/user-profile` - Get current user profile
- `PUT /api/v1/user-profile` - Update user profile
- `PUT /api/v1/user-profile/change-password` - Change password

For complete API documentation, see [Swagger UI](http://localhost:5000/swagger) or [API_REFERENCE.md](docs/API_REFERENCE.md).

## 🛠️ Technology Stack

### Backend
- **.NET 9.0** - Modern, high-performance framework
- **C# 13** - Latest language features
- **ASP.NET Core** - Web API framework

### Database & Caching
- **PostgreSQL 17** - Primary database with replication
- **Redis 7** - Caching and session management
- **Entity Framework Core 9** - ORM

### Libraries & Frameworks
- **MediatR** - CQRS implementation
- **FluentValidation** - Request validation
- **AutoMapper** - Object mapping
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **xUnit** - Testing framework
- **Moq** - Mocking framework

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **Prometheus** - Metrics collection
- **GitHub Actions** - CI/CD (planned)

## 🏃 Performance

- **Response Time**: < 50ms for cached requests
- **Throughput**: 1000+ requests/second
- **Database**: Read replicas for horizontal scaling
- **Caching**: Redis for sub-millisecond data access
- **Connection Pooling**: Optimized for concurrent requests

## 🔒 Security

- **BCrypt Password Hashing** - Industry-standard password security
- **JWT Tokens** - Stateless authentication
- **Token Expiration** - Automatic token lifecycle management
- **Refresh Tokens** - Secure token renewal
- **HTTPS Enforcement** - Encrypted communications
- **SQL Injection Protection** - Parameterized queries via EF Core
- **XSS Protection** - Input validation and output encoding

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details on:
- Code of conduct
- Development workflow
- Coding standards
- Pull request process
- Testing requirements

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

**Volcanion Company**
- GitHub: [@volcanion-company](https://github.com/volcanion-company)

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/volcanion-company/auth-service/issues)
- **Documentation**: [docs/](docs/)
- **Email**: support@volcanion.company

## 🗺️ Roadmap

- [ ] Multi-factor Authentication (MFA)
- [ ] OAuth2/OpenID Connect support
- [ ] SAML integration
- [ ] Rate limiting per user/role
- [ ] Audit logging
- [ ] Admin dashboard
- [ ] Kubernetes deployment manifests
- [ ] GraphQL API support

## 🙏 Acknowledgments

- Clean Architecture by Robert C. Martin
- Domain-Driven Design by Eric Evans
- CQRS pattern by Greg Young
- The .NET community for excellent libraries and tools

---

⭐ **Star this repository if you find it helpful!**
