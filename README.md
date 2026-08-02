# Clean Architecture with Vertical Slice Template

A modern .NET 10 REST API template implementing **Clean Architecture** with **Vertical Slice Architecture** pattern, minimal APIs, FluentValidation, and Entity Framework Core with PostgreSQL.

## Tech Stack

- **.NET 10** - Latest .NET runtime
- **ASP.NET Core Minimal APIs** - Lightweight HTTP endpoints
- **Entity Framework Core 10.0.3** - ORM for database access
- **PostgreSQL** - Database provider (Npgsql)
- **FluentValidation 12.1.1** - Request validation
- **CQRS Pattern** - Command Query Responsibility Segregation
- **Clean Architecture** - Domain-centric layered design
- **Vertical Slice Architecture** - Feature-driven organization within Application layer
- **Pipeline Decorators** - Logging and validation decorators
- **Audit Interceptor** - Automatic CreatedOn/UpdatedOn tracking
- **Scalar UI** - OpenAPI documentation viewer

## Solution Structure

```
EduFlow/
├── EduFlow.slnx
└── src/
    ├── EduFlow.Domain/           # Core domain (no dependencies)
    │   ├── Abstractions/
    │   │   ├── Result.cs                            # Success/Failure result wrapper
    │   │   └── Errors/
    │   │       ├── Error.cs                         # Error type definition
    │   │       └── ValidationError.cs               # Validation error collection
    │   └── Entities/
    │       ├── AuditableEntity.cs                   # Base class with CreatedOn/UpdatedOn
    │       └── Book.cs                              # Domain entity
    │
    ├── EduFlow.Application/      # Use cases & vertical slices
    │   ├── DependencyInjection.cs                   # Application layer DI registration
    │   ├── GlobalUsings.cs                          # ASP.NET Core global usings
    │   ├── Abstractions/
    │   │   ├── IApiEndpoint.cs                      # Endpoint contract
    │   │   ├── IHandler.cs                          # Handler contract
    │   │   └── Data/
    │   │       ├── IRepository.cs                   # Repository abstraction
    │   │       └── IUnitOfWork.cs                   # Unit of Work abstraction
    │   ├── Constants/
    │   │   └── ApiTags.cs                           # OpenAPI tags
    │   ├── Extensions/
    │   │   ├── MapEndpointExtensions.cs             # Endpoint registration
    │   │   └── ResultExtensions.cs                  # Result pattern matching
    │   ├── Features/                                # Vertical slices (features)
    │   │   └── BookFeature/
    │   │       ├── BookErrors.cs                    # Feature-specific errors
    │   │       ├── CreateBook/
    │   │       │   ├── CreateBookHandler.cs         # Handler + Request/Response records
    │   │       │   ├── CreateBookValidator.cs
    │   │       │   └── CreateBookEndpoint.cs
    │   │       ├── GetAllBooks/
    │   │       │   ├── GetAllBooksHandler.cs
    │   │       │   └── GetAllBooksEndpoint.cs
    │   │       ├── GetBookById/
    │   │       │   ├── GetBookByIdHandler.cs
    │   │       │   ├── GetBookByIdValidator.cs
    │   │       │   └── GetBookByIdEndpoint.cs
    │   │       ├── UpdateBook/
    │   │       │   ├── UpdateBookHandler.cs
    │   │       │   ├── UpdateBookValidator.cs
    │   │       │   └── UpdateBookEndpoint.cs
    │   │       └── DeleteBook/
    │   │           ├── DeleteBookHandler.cs
    │   │           ├── DeleteBookValidator.cs
    │   │           └── DeleteBookEndpoint.cs
    │   └── Pipelines/                               # Request processing decorators
    │       ├── ValidationDecorator.cs
    │       └── LoggingDecorator.cs
    │
    ├── EduFlow.Infrastructure/   # External concerns
    │   ├── DependencyInjection.cs                   # Infrastructure DI registration
    │   ├── Database/
    │   │   └── ApplicationDbContext.cs              # EF Core DbContext
    │   ├── Interceptors/
    │   │   └── AuditInterceptor.cs                  # Auto CreatedOn/UpdatedOn
    │   ├── Migrations/
    │   │   └── ...
    │   └── Repository/
    │       ├── Repository.cs                        # Generic repository implementation
    │       └── UnitOfWork.cs                        # Unit of Work implementation
    │
    └── EduFlow.WebApi/           # Thin host / entry point
        ├── Program.cs                               # App startup & DI composition
        ├── appsettings.json
        ├── appsettings.Development.json
        ├── Exceptions/
        │   └── CustomExceptionHandler.cs            # Global exception handler
        └── Extensions/
            └── HealthChecksExtensions.cs            # Health check configuration
```

## Architecture Overview

### Clean Architecture Layers

```
┌─────────────────────────────────────────────┐
│                  WebApi                      │  ← Entry point, thin host
│         (Program.cs, Exception Handler)     │
├─────────────────────────────────────────────┤
│              Infrastructure                  │  ← EF Core, Repository, Interceptors
│      (DbContext, Repository, UnitOfWork)    │
├─────────────────────────────────────────────┤
│               Application                    │  ← Vertical slices live here
│  (Features, Handlers, Validators, Endpoints)│
├─────────────────────────────────────────────┤
│                  Domain                      │  ← Entities, Result, Errors
│      (Book, AuditableEntity, Error)         │
└─────────────────────────────────────────────┘
```

### Dependency Flow (inward only)

```
WebApi → Infrastructure → Application → Domain
```

- **Domain** has zero dependencies
- **Application** depends only on Domain
- **Infrastructure** depends on Domain + Application (implements abstractions)
- **WebApi** depends on Application + Infrastructure (composes everything)

## Key Concepts

### 1. Clean Architecture + Vertical Slices
This template combines **Clean Architecture** (layered dependency inversion) with **Vertical Slice Architecture** (feature-driven organization):
- **Clean Architecture** provides the 4-layer boundary: Domain → Application → Infrastructure → WebApi
- **Vertical Slices** organize each feature (Handler + Validator + Endpoint) as a self-contained unit inside the Application layer
- **Independent** - Changes to one feature don't affect others
- **Scalable** - Easy to add new features
- **Testable** - Each slice can be tested in isolation

### 2. Audit Interceptor
Automatic tracking of `CreatedOn` and `UpdatedOn` via EF Core `SaveChangesInterceptor`:
```csharp
public abstract class AuditableEntity
{
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}
```
Any entity extending `AuditableEntity` gets timestamps set automatically on insert/update.

### 2. CQRS Pattern
- **Commands** - Operations that modify state (Create, Update, Delete)
- **Queries** - Operations that read data (GetAll, GetById)
- **Handlers** - `IHandler<TRequest, TResponse>` processes each request

### 3. Request/Response as Records
All DTOs use C# records for immutability and cleaner syntax:
```csharp
public sealed record CreateBookRequest(string Title, string Author, string ISBN, decimal Price, int PublishedYear);
public sealed record CreateBookResponse(Guid Id, string Title, string Author, string ISBN, decimal Price, int PublishedYear);
```

### 4. Validation Pipeline
FluentValidation decorators automatically validate requests before handlers:
```csharp
RuleFor(c => c.Title)
    .NotEmpty().WithMessage("Title is required")
    .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
```

### 5. Pipeline Logging
Decorators log request/response and execution time:
```csharp
LoggingDecorator     → Logs request, response, duration
ValidationDecorator  → Validates request
Handler              → Processes actual business logic
```

### 6. Error Handling
Centralized error handling with `Result<T>` wrapper:
```csharp
// Success
return Result.Success(new CreateBookResponse(...));

// Failure
return Result.Failure<CreateBookResponse>(BookErrors.NotFound(id));
```

## NuGet Packages

### Domain
No external dependencies.

### Application
| Package | Version | Purpose |
|---------|---------|---------|
| FluentValidation | 12.1.1 | Request validation |
| FluentValidation.DependencyInjectionExtensions | 12.1.1 | DI integration |
| Scrutor | 7.0.0 | Assembly scanning for DI (decorator registration) |

### Infrastructure
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.EntityFrameworkCore | 10.0.3 | ORM & data access |
| Microsoft.EntityFrameworkCore.Design | 10.0.3 | EF Core tools support |
| Microsoft.EntityFrameworkCore.Tools | 10.0.3 | Migrations & scaffolding |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 | PostgreSQL provider |

### WebApi
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.OpenApi | 10.0.3 | OpenAPI spec generation |
| Scalar.AspNetCore | 2.12.40 | OpenAPI UI |
| AspNetCore.HealthChecks.UI.Client | 9.0.0 | Health checks UI |
| Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore | 10.0.3 | DB health checks |

## Database Setup

### Using PostgreSQL (Current)

**Connection String** (appsettings.json):
```json
{
  "ConnectionStrings": {
    "connection": "Host=localhost;Port=5432;Database=EduFlow;Username=postgres;Password=your_password"
  }
}
```

### Entity Framework migrations

**Package Manager Console (Visual Studio):**
```powershell
Add-Migration InitialCreate -Project EduFlow.Infrastructure -StartupProject EduFlow.WebApi
Update-Database -Project EduFlow.Infrastructure -StartupProject EduFlow.WebApi
```

**1. Create Initial Migration:**
```bash
dotnet ef migrations add InitialCreate -p EduFlow.Infrastructure -s EduFlow.WebApi
```

**2. Apply Migration to Database:**
```bash
dotnet ef database update -p EduFlow.Infrastructure -s EduFlow.WebApi
```

**3. Add New Migration (after model changes):**
```bash
dotnet ef migrations add MigrationName -p EduFlow.Infrastructure -s EduFlow.WebApi
dotnet ef database update -p EduFlow.Infrastructure -s EduFlow.WebApi
```

**4. Revert Last Migration:**
```bash
dotnet ef database update LastGoodMigration -p EduFlow.Infrastructure -s EduFlow.WebApi
dotnet ef migrations remove -p EduFlow.Infrastructure -s EduFlow.WebApi
```

### Switching Database Providers

**For SQL Server:**
```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("connection"));
});
```

**For SQLite (Development):**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("connection"));
});
```

## Running the Application

**1. Install Dependencies:**
```bash
dotnet restore
```

**2. Build Solution:**
```bash
dotnet build EduFlow.slnx
```

**3. Run Application:**
```bash
dotnet run --project EduFlow.WebApi
```

## Scalar API Testing UI

Scalar is a modern, beautiful alternative to Swagger UI for testing and exploring APIs.

### Accessing Scalar

- **URL**: `http://localhost:5135/scalar/v1`
- **OpenAPI Spec**: `http://localhost:5135/openapi/v1.json`

### Features

✨ **Scalar provides:**
- 🚀 Beautiful, modern UI for API documentation
- 📝 Interactive request/response testing
- 🔄 Request history
- 📦 Request/response examples with syntax highlighting
- 🏷️ Endpoint organization by tags (our `ApiTags.Books`)
- 📱 Mobile-responsive design
- 🔐 Support for authentication headers
- 💾 Request templates and presets

### Using Scalar to Test Book API

1. **Open Scalar**: Navigate to `http://localhost:5135/scalar/v1`
2. **Select Endpoint**: Click any book endpoint under the "books" tag
3. **Enter Request Data**: Fill in parameters and request body
4. **Execute Request**: Click "Send" button
5. **View Response**: See status code, headers, and response body

### Example: Testing Create Book Endpoint

**In Scalar UI:**
```
Endpoint: POST /books
Request Body:
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "9780132350884",
  "price": 39.99,
  "publishedYear": 2008
}

Response 200:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "9780132350884",
  "price": 39.99,
  "publishedYear": 2008
}
```

---

## Health Checks

Health checks monitor the application and dependent services (database, external APIs, etc.).

### Built-in Health Checks

```csharp
// Program.cs
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();
```

### Health Check Endpoint

**URL**: `http://localhost:5135/health`

**Response (Healthy):**
```http
GET /health

HTTP/1.1 200 OK
Content-Type: application/json

{
  "status": "Healthy",
  "timestamp": "2026-02-23T10:30:00Z",
  "checks": {
    "ApplicationDbContext": {
      "status": "Healthy",
      "description": "Database connection successful"
    }
  }
}
```

**Response (Unhealthy):**
```http
HTTP/1.1 503 Service Unavailable

{
  "status": "Unhealthy",
  "timestamp": "2026-02-23T10:31:00Z",
  "checks": {
    "ApplicationDbContext": {
      "status": "Unhealthy",
      "description": "Database connection failed"
    }
  }
}
```

### Health Check Configuration

**Map Health Check Endpoint:**

```csharp
// In Program.cs after building app
var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
});

// Custom response formatter
static async Task WriteHealthCheckResponse(
    HttpContext httpContext,
    HealthReport report)
{
    httpContext.Response.ContentType = "application/json";
    
    var response = new
    {
        status = report.Status.ToString(),
        timestamp = DateTime.UtcNow,
        checks = report.Entries.ToDictionary(
            e => e.Key,
            e => new
            {
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
    };

    await httpContext.Response.WriteAsJsonAsync(response);
}
```

### Health Check Status Codes

| Status | HTTP Code | Meaning |
|--------|-----------|---------|
| `Healthy` | 200 | All checks passed |
| `Degraded` | 200 | Some checks passed, others degraded |
| `Unhealthy` | 503 | One or more checks failed |

### Best Practices for Health Checks

✅ **Do:**
- Check critical dependencies (database, cache, external APIs)
- Set reasonable timeouts for checks
- Include descriptive status messages
- Log health check failures
- Use health checks in load balancers
- Implement liveness & readiness probes

❌ **Don't:**
- Make health checks too complex
- Hit external APIs on every health check
- Override built-in status codes
- Ignore degraded status
- Leave health checks unconfigured

---



### Endpoints

All book endpoints are tagged with `"books"` in OpenAPI documentation.

#### 1. Create Book
```http
POST /books
Content-Type: application/json

{
  "title": "The Art of Programming",
  "author": "John Doe",
  "isbn": "9781234567890",
  "price": 49.99,
  "publishedYear": 2023
}

Response 200:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "The Art of Programming",
  "author": "John Doe",
  "isbn": "9781234567890",
  "price": 49.99,
  "publishedYear": 2023
}
```

#### 2. Get All Books
```http
GET /books

Response 200:
{
  "books": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "title": "The Art of Programming",
      "author": "John Doe",
      "isbn": "9781234567890",
      "price": 49.99,
      "publishedYear": 2023
    }
  ]
}
```

#### 3. Get Book by ID
```http
GET /books/550e8400-e29b-41d4-a716-446655440000

Response 200:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "The Art of Programming",
  "author": "John Doe",
  "isbn": "9781234567890",
  "price": 49.99,
  "publishedYear": 2023
}

Response 404:
{
  "code": "Books.NotFound",
  "description": "The Book with Id '550e8400-e29b-41d4-a716-446655440000' was not found"
}
```

#### 4. Update Book (Partial Updates Supported)
```http
PUT /books/550e8400-e29b-41d4-a716-446655440000
Content-Type: application/json

{
  "title": "Advanced Programming",
  "author": null,
  "isbn": null,
  "price": 59.99,
  "publishedYear": null
}

Response 200:
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Advanced Programming",
  "author": "John Doe",
  "isbn": "9781234567890",
  "price": 59.99,
  "publishedYear": 2023
}
```

#### 5. Delete Book
```http
DELETE /books/550e8400-e29b-41d4-a716-446655440000

Response 200:
{
  "id": "550e8400-e29b-41d4-a716-446655440000"
}

Response 404:
{
  "code": "Books.NotFound",
  "description": "The Book with Id '550e8400-e29b-41d4-a716-446655440000' was not found"
}
```

### Validation Example

**Invalid Create Request:**
```http
POST /books
{
  "title": "",
  "author": "John Doe",
  "isbn": "invalid",
  "price": -10,
  "publishedYear": 2050
}

Response 400:
{
  "errors": [
    {
      "code": "NotEmptyValidator",
      "description": "Title is required",
      "type": 2
    },
    {
      "code": "NotEmptyValidator",
      "description": "Author is required",
      "type": 2
    },
    {
      "code": "NotEmptyValidator",
      "description": "ISBN is required",
      "type": 2
    },
    {
      "code": "GreaterThanValidator",
      "description": "Published year must be a valid year",
      "type": 2
    }
  ],
  "code": "Validation.General",
  "description": "One or more validation errors occurred",
  "type": 2
}
```

## Global Level Exception Handling

Global exception handling ensures all unhandled exceptions are caught and returned as consistent API responses.

### Built-in Exception Handler Middleware

ASP.NET Core 10 provides `IExceptionHandler` for global exception handling:

```csharp
// Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler();
```

### Custom Global Exception Handler

Global exception handling can be implemented using `IExceptionHandler` to catch and format all unhandled exceptions:

```csharp
// Register in Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var app = builder.Build();
app.UseExceptionHandler();
```

### Exception Types & Status Codes

| Exception Type | Status Code | Example |
|---|---|---|
| `ValidationException` | 400 Bad Request | FluentValidation errors |
| `KeyNotFoundException` | 404 Not Found | Resource not found |
| `ArgumentException` | 400 Bad Request | Invalid input argument |
| `UnauthorizedAccessException` | 401 Unauthorized | Authentication failed |
| `InvalidOperationException` | 409 Conflict | Operation cannot be performed |
| Default Exception | 500 Internal Server Error | Unhandled errors |

### Registering Global Exception Handler

Global exception handlers are configured in `Program.cs` using `AddExceptionHandler<>()` and `UseExceptionHandler()` middleware.

### Example: Uncaught Exception Response

**Database Connection Error:**
```http
POST /books
{
  "title": "Test",
  "author": "Author",
  "isbn": "1234567890",
  "price": 19.99,
  "publishedYear": 2023
}

Response 500:
{
  "type": "InvalidOperationException",
  "title": "An error occurred",
  "status": 500,
  "detail": "An exception has been raised that is likely due to a transient failure.",
  "instance": "/books"
}
```

**Validation Error (caught by FluentValidation):**
```http
Response 400:
{
  "errors": [
    {
      "code": "NotEmptyValidator",
      "description": "Title is required",
      "type": 2
    },
    {
      "code": "GreaterThanValidator",
      "description": "Published year must be a valid year",
      "type": 2
    }
  ],
  "code": "Validation.General",
  "description": "One or more validation errors occurred",
  "type": 2
}
```

### Logging Exceptions

Exceptions should be logged with context (path, timestamp, exception type) for debugging and monitoring purposes.

### Environment-Specific Error Details

Stack traces and detailed error information should only be exposed in development environments to prevent information leakage in production.

### Best Practices for Global Exception Handling

✅ **Do:**
- Log all exceptions with context (path, user, timestamp)
- Return consistent error response format
- Include error codes for client handling
- Hide sensitive information in production
- Handle specific exceptions first, generic last
- Use proper HTTP status codes
- Return failed operations as JSON

❌ **Don't:**
- Expose stack traces to clients (except dev)
- Log sensitive data (passwords, tokens)
- Return 500 for business logic failures
- Suppress logging of exceptions
- Ignore validation errors
- Return HTML error pages from APIs

## Architecture Flow

### Request Processing Pipeline

```
HTTP Request
    ↓
MinimalAPI Endpoint (Application layer - route binding)
    ↓
LoggingDecorator (Log request start)
    ↓
ValidationDecorator (FluentValidation)
    ↓
Handler (Business logic)
    ├─ IRepository (abstraction in Application, impl in Infrastructure)
    ├─ IUnitOfWork (abstraction in Application, impl in Infrastructure)
    └─ Returns Result<T>
    ↓
AuditInterceptor (auto-sets CreatedOn/UpdatedOn)
    ↓
LoggingDecorator (Log response, duration)
    ↓
Endpoint Result Mapping (Map to HTTP response)
    ↓
[Exception Caught?]
    ├─ Yes → CustomExceptionHandler (WebApi)
    │        └─ Format & Log Error
    │           └─ Return Error Response
    └─ No → HTTP Response (200/400/404)
```

## Adding a New Feature

1. **Create Feature Folder** under `Application/Features/`:
   ```
   Application/Features/BookFeature/NewFeature/
   ├── NewFeatureHandler.cs        # Handler + Request/Response records
   ├── NewFeatureValidator.cs      # FluentValidation rules
   └── NewFeatureEndpoint.cs       # Minimal API endpoint
   ```

2. **Implement Handler** (inherit `IHandler<TRequest, TResponse>`):
   ```csharp
   public sealed class NewFeatureHandler(
       IRepository<Book> _repo) : IHandler<NewFeatureRequest, Result<NewFeatureResponse>>
   {
       public async Task<Result<NewFeatureResponse>> HandleAsync(...) { }
   }
   ```

3. **Register Validator** (auto-discovered from assembly)

4. **Create Endpoint** (inherit `IApiEndpoint`, auto-discovered):
   ```csharp
   public void MapEndpoint(WebApplication app)
   {
       app.MapPost("path", async (...) => { ... })
           .WithTags(ApiTags.Books)
           .Produces<NewFeatureResponse>(StatusCodes.Status200OK);
   }
   ```

5. **Add Domain Entity** (if needed) in `Domain/Entities/` extending `AuditableEntity`

6. **No manual registration needed** — handlers, validators, and endpoints are all auto-discovered from the Application assembly.

## Best Practices

✅ **Do:**
- Keep each feature independent
- Use records for immutable DTOs
- Validate at the boundary (validators)
- Use dependency injection for all services
- Log important operations
- Handle errors with Result<T>
- Write handlers that do one thing well

❌ **Don't:**
- Put business logic in endpoints
- Modify requests/responses mid-pipeline
- Mix validators (create separate per feature)
- Expose entities in API responses
- Skip validation
- Ignore error codes from handlers

## Debugging

**Enable Debug Logging:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

**View Database Queries:**
```csharp
// In Program.cs
options.LogTo(Console.WriteLine, LogLevel.Information);
```

## License

MIT License - Use freely and commercially.

---

**Built with .NET 10 | Vertical Slice Architecture | CQRS Pattern**
