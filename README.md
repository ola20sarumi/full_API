# Employees API (full_API)

[![CI](https://github.com/ola20sarumi/full_API/actions/workflows/ci.yml/badge.svg)](https://github.com/ola20sarumi/full_API/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)
![SQL Server](https://img.shields.io/badge/SQL%20Server-EF%20Core%2010-CC2927)

A REST API for managing employees, built with ASP.NET Core 10 and Entity Framework Core on SQL Server.
It is the backend for [Full_UI](https://github.com/ola20sarumi/Full_UI), an Angular front end.

## Features

- **CRUD endpoints** with correct HTTP semantics: `201 Created` + `Location`, `204 No Content`, `404`, `409 Conflict`
- **Validation** on every write, returned as RFC 9457 `ValidationProblemDetails` with per-field errors
- **Consistent error responses**: every error, including unhandled exceptions, is a `ProblemDetails` body; no stack traces leak
- **Search, filtering and pagination** on the list endpoint, with the total in an `X-Total-Count` header
- **Unique emails**, enforced by a database index and reported as a clear `409`
- **OpenAPI 3.1 + Swagger UI**, generated from the code and XML doc comments
- **Health check** at `/health`, which verifies database connectivity
- **Config-driven CORS** (only the front end's origin is allowed, not `*`)
- **Integration tests** that run the full HTTP pipeline against an in-memory database
- **CI** on GitHub Actions (build with warnings as errors, test, Docker image build)
- **Docker Compose** for a one-command local stack with SQL Server

## Architecture

```
full.API/
├── Controllers/      HTTP layer: routing, status codes, OpenAPI metadata
├── Contracts/        Request/response DTOs and validation rules (the public API shape)
├── Services/         Business logic (IEmployeeService), e.g. email normalization and uniqueness
├── Data/             EF Core DbContext and schema configuration
├── Models/           Persistence entities
├── Infrastructure/   Cross-cutting concerns: global exception handler, JSON converters
└── Migrations/       EF Core migrations
full.API.Tests/       xUnit integration tests (WebApplicationFactory + SQLite in-memory)
```

Controllers stay thin and depend on `IEmployeeService`. The database entity is never exposed directly: requests
and responses use dedicated DTOs, so the schema can change without breaking clients.

## API

| Method | Route | Success | Errors |
|---|---|---|---|
| `GET` | `/api/employees?search=&department=&page=1&pageSize=50` | `200` list, `X-Total-Count` header | `400` |
| `GET` | `/api/employees/{id}` | `200` | `404` |
| `POST` | `/api/employees` | `201` + `Location` | `400`, `409` |
| `PUT` | `/api/employees/{id}` | `200` | `400`, `404`, `409` |
| `DELETE` | `/api/employees/{id}` | `204` | `404` |
| `GET` | `/health` | `200 Healthy` | `503` |

Example request body:

```json
{
  "name": "Ada Lovelace",
  "email": "ada@example.com",
  "phone": "+44 7700 900123",
  "salary": 72000,
  "department": "Engineering"
}
```

Example validation error:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."]
  }
}
```

## Running locally

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) and SQL Server LocalDB, which comes with Visual Studio.

```bash
dotnet run --project full.API --launch-profile https
```

Swagger UI opens at <https://localhost:7045/swagger>. In Development the database is created and migrated
automatically on startup.

To use a different SQL Server, override the connection string without editing committed files:

```bash
dotnet user-secrets set "ConnectionStrings:FullConnectionString" "<your connection string>" --project full.API
```

`full.API/full.API.http` has ready-made requests for Visual Studio, Rider or VS Code's REST Client.

### With Docker

```bash
docker compose up --build
```

Then open <http://localhost:8080/swagger>. This starts SQL Server 2022 alongside the API.

## Tests

```bash
dotnet test
```

The tests host the real application with `WebApplicationFactory`, replacing SQL Server with in-memory SQLite.
They cover the happy paths, validation, duplicate detection, not-found cases, filtering, pagination and the health check.

## Configuration

| Key | Default | Purpose |
|---|---|---|
| `ConnectionStrings:FullConnectionString` | LocalDB `FullDb` | SQL Server connection |
| `Cors:AllowedOrigins` | `["http://localhost:4200"]` | Origins allowed to call the API |
| `Database:MigrateOnStartup` | `true` in Development, otherwise `false` | Apply EF migrations at startup |

## Database migrations

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project full.API
dotnet ef database update --project full.API
```
