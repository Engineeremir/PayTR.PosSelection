# PayTR Pos Selection System

This repository contains the PayTR Pos Selection Project, built with modern technologies including Docker, HashiCorp Vault for secret management, and PostgreSQL database.

## Prerequisites

- Docker and Docker Compose installed on your system
- Git for cloning repositories
- Web browser to access the interfaces

## Installation Guide

Follow these steps to set up and run the PayTR Pos Selection System:

### Step 1: Clone the Repositories

First, clone both required repositories:

```bash
git clone https://github.com/Engineeremir/PayTR.PosSelection.System
git clone https://github.com/Engineeremir/PayTR.PosSelection
```

### Step 2: Start Core Services

Navigate to the `PayTR.PosSelection.System` directory and create the required network, then start the database and vault services:

```bash
cd PayTR.PosSelection.System
docker network create paytr-shared-network
docker-compose up -d paytr.shared.database paytr.shared.vault paytr.shared.redis
```

**Expected Output:**
```
✔ Container paytr.shared.vault     Started
✔ Container paytr.shared.database  Started
✔ Container paytr.shared.redis  Started
```

### Step 3: Configure HashiCorp Vault

1. **Access Vault UI**: Open your web browser and navigate to `http://localhost:8200/`

2. **Login**: 
   - Method: **Token**
   - Token: `myroottoken`

3. **Navigate to Secrets**:
   - Click on **Secrets Engine** in the left menu
   - Select **secret**

4. **Create Secret**:
   - Click the **Create secret** button on the right side
   - In the "Path for this secret" field, enter: `dev/posselection`

5. **Add Configuration**:
   - Click the **JSON toggle** button to switch to JSON mode
   - Enter the following JSON configuration:

   ```json
   {
      "DatabaseSettings": {
        "PosSelectionDatabase": "Host=paytr.shared.database;Port=5432;Database=posselection;Username=postgres;Password=postgres"
      },
      "HangfireSettings": {
        "GetPosRatiosJobCronFormat": "*/10 * * * *",
        "HangfireDbConnectionString": "Host=paytr.shared.database;Port=5432;Database=posselection;Username=postgres;Password=postgres"
      },
      "RedisSettings": {
        "AbsoluteExpiration": "mymaster",
        "ConnectionString": "paytr.shared.redis:6379",
        "MasterName": "mymaster",
        "SentinelEndPoints": [
          "localhost:6383",
          "localhost:6384",
          "localhost:6385"
        ],
        "SlidingExpiration": "mymaster",
        "UseSentinel": false
      }
    }
   ```

6. **Save**: Click the **Save** button to store the configuration

### Step 4: Start the API Service

Once Vault is configured, start the API service:

```bash
docker-compose up -d paytr.posselection.api
```

**Expected Output:**
```
✔ Container paytr.shared.vault     Healthy
✔ Container paytr.shared.database  Healthy
✔ Container paytr.shared.redis  Healthy
✔ Container paytr.posselection.api    Started
```

**Important Note:** The `paytr.posselection.api` service requires `paytr.shared.vault` `paytr.shared.redis` and `paytr.shared.database` to be running and healthy before it can start successfully.

### Step 5: Verify Installation

The application should now be running successfully on port 8080.

**Access Swagger Documentation:**
Navigate to `http://localhost:8080/swagger/index.html` to explore and test the API endpoints through the interactive Swagger interface.

## Project Architecture

This project follows **Clean Architecture** and **Domain-Driven Design (DDD)** principles with the following layer structure:

- **Domain**: Core business entities and domain logic
- **Application**: Application services,Cross-cutting concerns, CQRS handlers and business workflows
- **Infrastructure.EFCore**: Data access layer with Entity Framework Core
- **API**: RESTful API controllers and presentation layer
- **SharedLayer**: Common utilities

### Design Patterns Used

- **Repository Pattern**: Data access abstraction
- **Specification Pattern**: Using Ardalis.Specification package
- **Unit of Work Pattern**: Transaction management
- **CQRS (Command Query Responsibility Segregation)**: Using MediatR
- **Pipeline Behaviors**: For cross-cutting concerns (logging, validation, exception handling)

### Key Features

- **Database**: PostgreSQL with Entity Framework Core
- **Automatic Migrations**: Database migrations run automatically on application startup
- **Seed Data**: Categories and products are automatically seeded
- **Validation**: FluentValidation with custom pipeline behaviors
- **Error Handling**: Comprehensive exception handling with ProblemDetails-compliant responses
- **Logging**: Microsoft.Extensions.Logging with custom pipeline behaviors
- **Domain Events**: Automatic domain event dispatching after entity changes

## Available Endpoints

The API provides the following functionality:

### PosRatios
- **POST** `/api/posratios/select` - Gets best pos option

## Service Architecture

- **paytr.shared.database**: PostgreSQL database service (Port: 5432)
- **paytr.shared.vault**: HashiCorp Vault for secure configuration management (Port: 8200)
- **paytr.shared.redis**: Redis cache service (Port: 6379)
- **paytr.posselection.api**: Main API service for the pos selection system (Port: 8080)

## Technical Implementation

### Database Management
- **Automatic Migrations**: The application automatically applies pending migrations on startup
- **Connection Management**: Database connections are managed through HashiCorp Vault

### Error Handling
The application implements comprehensive error handling:
- **Custom Exceptions**: Domain-specific exceptions for business logic violations
- **Exception Pipeline Behavior**: Global exception handling in the MediatR pipeline
- **ProblemDetails Response**: Standardized error responses following RFC 7807
- **Development vs Production**: Detailed error information in development mode

### Validation
- **FluentValidation**: Robust input validation for all requests
- **Validation Pipeline Behavior**: Automatic validation before request processing
- **Structured Error Responses**: User-friendly validation error messages

### Logging & Monitoring
- **Request/Response Logging**: Complete HTTP request and response logging for debugging and monitoring
- **Pipeline Logging**: Custom logging behaviors in the MediatR pipeline
- **Structured Logging**: Consistent log formatting throughout the application
- **Performance Tracking**: Request processing time monitoring

### Domain Events
- **Event Dispatching**: Automatic publishing of domain events after database changes
- **MediatR Integration**: Seamless integration with the CQRS pattern
- **Clean Entity State**: Automatic cleanup of domain events after processing

## Environment Requirements

- **.NET 8.0** or later
- **Docker & Docker Compose**
- **PostgreSQL** (via Docker container)
- **Redis** (via Docker container)
- **HashiCorp Vault** (via Docker container)

## Troubleshooting

If you encounter issues:

1. **Container Health**: Ensure all containers are healthy using `docker-compose ps`
2. **Service Dependencies**: Verify that `paytr.shared.vault` and `paytr.shared.database` are running before starting the API
3. **Vault Configuration**: Check that the database connection secret is properly configured in Vault
4. **Database Connection**: Ensure PostgreSQL container is accessible on port 5432

## API Documentation

Once the system is running, you can access the complete API documentation and test endpoints at:
`http://localhost:8080/swagger/index.html`