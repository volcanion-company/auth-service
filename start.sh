#!/bin/bash

# Volcanion Auth Service - Quick Start Script

echo "🚀 Volcanion Auth Service - Quick Start"
echo "========================================"
echo ""

# Check prerequisites
echo "Checking prerequisites..."

# Check Docker
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker."
    exit 1
fi
echo "✅ Docker is installed"

# Check Docker Compose
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed."
    exit 1
fi
echo "✅ Docker Compose is installed"

# Check .NET SDK (optional)
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "✅ .NET SDK is installed (version $DOTNET_VERSION)"
else
    echo "⚠️  .NET SDK not found. Docker-only mode will be used."
fi

echo ""
echo "Starting services..."

# Start Docker Compose
docker-compose up -d

echo ""
echo "Waiting for services to be ready..."
sleep 10

# Check service health
MAX_ATTEMPTS=30
ATTEMPT=0
HEALTHY=false

while [ $ATTEMPT -lt $MAX_ATTEMPTS ] && [ "$HEALTHY" = false ]; do
    if curl -f -s http://localhost:8080/health > /dev/null 2>&1; then
        HEALTHY=true
    else
        ATTEMPT=$((ATTEMPT + 1))
        echo "⏳ Waiting for API to be ready... ($ATTEMPT/$MAX_ATTEMPTS)"
        sleep 2
    fi
done

if [ "$HEALTHY" = true ]; then
    echo ""
    echo "✅ All services are running!"
    echo ""
    echo "📡 Service URLs:"
    echo "   API:        http://localhost:8080"
    echo "   Swagger:    http://localhost:8080/swagger"
    echo "   Health:     http://localhost:8080/health"
    echo "   Metrics:    http://localhost:8080/metrics"
    echo "   Prometheus: http://localhost:9090"
    echo "   Grafana:    http://localhost:3000 (admin/admin)"
    echo ""
    echo "🗄️  Database Ports:"
    echo "   PostgreSQL (Write): localhost:5432"
    echo "   PostgreSQL (Read1): localhost:5433"
    echo "   PostgreSQL (Read2): localhost:5434"
    echo "   Redis:              localhost:6379"
    echo ""
    echo "📝 Quick Test Commands:"
    echo ""
    echo "1️⃣  Register a test user:"
    echo '   curl -X POST http://localhost:8080/api/v1/authentication/register \'
    echo '     -H "Content-Type: application/json" \'
    echo '     -d '"'"'{"email":"test@example.com","password":"Test@123456","firstName":"John","lastName":"Doe"}'"'"
    echo ""
    echo "2️⃣  Login:"
    echo '   curl -X POST http://localhost:8080/api/v1/authentication/login \'
    echo '     -H "Content-Type: application/json" \'
    echo '     -d '"'"'{"email":"test@example.com","password":"Test@123456"}'"'"
    echo ""
    echo "3️⃣  Check health:"
    echo '   curl http://localhost:8080/health'
    echo ""
    echo "📚 Documentation:"
    echo "   README.md              - Project overview"
    echo "   docs/ARCHITECTURE.md   - Architecture details"
    echo "   docs/API_EXAMPLES.md   - API usage examples"
    echo "   docs/MIGRATION_GUIDE.md - Database migrations"
    echo ""
    echo "🛑 To stop all services:"
    echo "   docker-compose down"
    echo ""
else
    echo ""
    echo "❌ Services failed to start within timeout period."
    echo "   Check logs with: docker-compose logs"
    echo ""
fi
