# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/VolcanionAuth.API/VolcanionAuth.API.csproj", "src/VolcanionAuth.API/"]
COPY ["src/VolcanionAuth.Application/VolcanionAuth.Application.csproj", "src/VolcanionAuth.Application/"]
COPY ["src/VolcanionAuth.Domain/VolcanionAuth.Domain.csproj", "src/VolcanionAuth.Domain/"]
COPY ["src/VolcanionAuth.Infrastructure/VolcanionAuth.Infrastructure.csproj", "src/VolcanionAuth.Infrastructure/"]

RUN dotnet restore "src/VolcanionAuth.API/VolcanionAuth.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/VolcanionAuth.API"
RUN dotnet build "VolcanionAuth.API.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "VolcanionAuth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VolcanionAuth.API.dll"]
