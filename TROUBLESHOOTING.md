# Troubleshooting Guide - Volcanion Auth

## ✅ Tình trạng Project

### Build Status
- ✅ **Build thành công 100%** - Không có lỗi, không có warnings
- ✅ Tất cả 7 projects compile thành công
- ✅ Dependencies đã được restore đầy đủ

### Đã Fix
1. ✅ Missing `using MediatR;` trong `DependencyInjection.cs`
2. ✅ Warning CS0114: Thêm `override` keyword cho `SaveChangesAsync`
3. ✅ Warning CS1998: Sửa async methods không có await operators

## 🐳 Yêu cầu để chạy Project

### Prerequisites
Project **BẮT BUỘC** cần Docker để chạy vì phụ thuộc vào:
1. **PostgreSQL cluster** (1 primary + 2 replicas)
2. **Redis** cache server

### Option 1: Chạy với Docker (Recommended) ⭐

#### Cài đặt Docker Desktop
1. Tải Docker Desktop cho Windows: https://www.docker.com/products/docker-desktop/
2. Cài đặt và khởi động Docker Desktop
3. Đảm bảo Docker daemon đang chạy

#### Khởi động Services
```powershell
# Từ thư mục root của project
cd E:\Github\volcanion-auth-hybrid

# Start tất cả services (PostgreSQL, Redis, API, Prometheus, Grafana)
docker compose up -d

# Hoặc chỉ start database và cache (không start API container)
docker compose up -d postgres-primary postgres-replica-1 postgres-replica-2 redis

# Kiểm tra services
docker compose ps

# Xem logs
docker compose logs -f
```

#### Chạy API từ Visual Studio hoặc CLI
```powershell
# Sau khi Docker services đã start
cd src\VolcanionAuth.API
dotnet run
```

### Option 2: Chạy manual PostgreSQL & Redis

#### PostgreSQL
```powershell
# Nếu bạn có PostgreSQL installed locally
# Update connection string trong appsettings.Development.json:
"WriteDatabase": "Host=localhost;Port=5432;Database=volcanion_auth;Username=postgres;Password=YourPassword;"
```

#### Redis
```powershell
# Nếu bạn có Redis installed locally hoặc dùng Redis on Windows
# Download: https://github.com/microsoftarchive/redis/releases
# Hoặc dùng WSL2 với Redis
```

### Option 3: Test build mà không run

```powershell
# Build toàn bộ solution
dotnet build

# Build một project cụ thể
dotnet build src/VolcanionAuth.API/VolcanionAuth.API.csproj

# Run tests (không cần database)
dotnet test tests/VolcanionAuth.Domain.Tests
dotnet test tests/VolcanionAuth.Application.Tests
```

## 🔧 Các lệnh hữu ích

### Build & Clean
```powershell
# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Build solution
dotnet build

# Build without restore
dotnet build --no-restore

# Rebuild
dotnet clean; dotnet restore; dotnet build
```

### Run Application
```powershell
# Run với Development environment
cd src\VolcanionAuth.API
dotnet run --environment Development

# Run với Production settings
dotnet run --environment Production

# Watch mode (auto-reload on changes)
dotnet watch run
```

### Database Migrations (cần PostgreSQL running)
```powershell
# Tạo migration
cd src\VolcanionAuth.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../VolcanionAuth.API

# Apply migrations
dotnet ef database update --startup-project ../VolcanionAuth.API

# Remove last migration
dotnet ef migrations remove --startup-project ../VolcanionAuth.API
```

### Docker Commands
```powershell
# Start all services
docker compose up -d

# Stop all services
docker compose down

# Stop and remove volumes
docker compose down -v

# View logs
docker compose logs -f api
docker compose logs -f postgres-primary
docker compose logs -f redis

# Restart specific service
docker compose restart api

# Rebuild and start
docker compose up -d --build
```

## 🐛 Common Issues

### 1. Docker not found
**Error**: `The term 'docker' is not recognized`

**Solution**: 
- Cài đặt Docker Desktop
- Restart PowerShell/Terminal sau khi cài
- Kiểm tra: `docker --version`

### 2. Port already in use
**Error**: `Bind for 0.0.0.0:5432 failed: port is already allocated`

**Solution**:
```powershell
# Tìm process đang dùng port
netstat -ano | findstr :5432

# Kill process
taskkill /PID <process_id> /F

# Hoặc đổi port trong docker-compose.yml
```

### 3. Database connection failed
**Error**: `Npgsql.NpgsqlException: Connection refused`

**Solution**:
- Đảm bảo PostgreSQL container đang chạy: `docker compose ps`
- Check logs: `docker compose logs postgres-primary`
- Test connection: `docker compose exec postgres-primary psql -U postgres`

### 4. Redis connection failed
**Error**: `StackExchange.Redis.RedisConnectionException`

**Solution**:
- Kiểm tra Redis container: `docker compose ps redis`
- Test connection: `docker compose exec redis redis-cli ping`

### 5. JWT Secret missing
**Error**: `ArgumentNullException: JWT:Secret`

**Solution**:
- Kiểm tra `appsettings.Development.json` có section `Jwt` với `SecretKey`
- Secret key phải ít nhất 32 characters

## 📊 Project Structure Validation

### Files Created (60+ files)
```
✅ VolcanionAuth.sln
✅ Directory.Build.props

src/
✅ VolcanionAuth.Domain/ (13 files)
✅ VolcanionAuth.Application/ (15 files)
✅ VolcanionAuth.Infrastructure/ (18 files)
✅ VolcanionAuth.API/ (8 files)

tests/
✅ VolcanionAuth.Domain.Tests/ (4 files)
✅ VolcanionAuth.Application.Tests/ (4 files)
✅ VolcanionAuth.Integration.Tests/ (3 files)

docs/
✅ ARCHITECTURE.md
✅ API_EXAMPLES.md
✅ MIGRATION_GUIDE.md

Docker/
✅ Dockerfile
✅ docker-compose.yml

CI/CD/
✅ .github/workflows/ci-cd.yml
```

### Verify Build
```powershell
# Tất cả projects phải build thành công
dotnet build

# Expected output:
# Build succeeded in X.Xs
# 0 Warning(s)
# 0 Error(s)
```

## 🎯 Current Status

### ✅ What's Working
- Project structure hoàn chỉnh với Clean Architecture
- All code compiles successfully (0 errors, 0 warnings)
- Domain layer với DDD patterns
- Application layer với CQRS
- Infrastructure layer với repositories
- API layer với controllers
- Unit tests có thể chạy
- Docker configuration sẵn sàng

### ⚠️ What Needs Docker
- Running the API application (cần PostgreSQL + Redis)
- Integration tests
- Health checks
- Full end-to-end testing

### 🚀 Quick Start (Recommended Path)

1. **Cài Docker Desktop**
   ```
   https://www.docker.com/products/docker-desktop/
   ```

2. **Start services**
   ```powershell
   docker compose up -d
   ```

3. **Wait for services to be ready** (30 seconds)

4. **Test API**
   ```powershell
   # Via Docker container
   curl http://localhost:8080/health
   
   # Or run locally
   cd src/VolcanionAuth.API
   dotnet run
   ```

5. **Access Swagger**
   ```
   http://localhost:8080/swagger
   ```

## 💡 Next Steps

1. ✅ Build successful - Project code là hoàn toàn OK!
2. 🐳 Cài Docker Desktop để run application
3. 🚀 Start services với `docker compose up -d`
4. 🧪 Test API endpoints qua Swagger UI
5. 📊 Monitor với Prometheus (http://localhost:9090)
6. 📈 Visualize với Grafana (http://localhost:3000)

## 📞 Support

Nếu gặp issue:
1. Check build: `dotnet build`
2. Check Docker: `docker compose ps`
3. Check logs: `docker compose logs`
4. Review this guide
5. Check documentation in `/docs`
