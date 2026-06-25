# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`hoatheomua-be` is the .NET 8 ASP.NET Core Web API backend for the "Hoa Theo Mùa" storefront. It is mostly an integration layer over three external systems:

- **Nhanh.vn** — POS / product catalog / orders (`NhanhVnOption`)
- **Ahamove** — delivery quoting & dispatch (`AhamoveOption`)
- **Sepay** — payment webhooks (`SepayController`)

**This service does not issue JWTs and has no User/auth table.** Tokens are issued by a separate service, `taskcoper-be`; this service only *validates* them. The validated token claims are the source of truth for identity (see `ICurrentUserService`). The `JwtOption` Issuer/Audience/AccessTokenKey here **must stay byte-identical to taskcoper-be's** or cross-service tokens won't validate. The `HoaTheoMua.Service/Auth/Service.cs` and `JwtService/` are vestigial (largely commented out) — do not re-enable token issuance without understanding this split.

## Common commands

```bash
# Build (also what the pre-commit hook runs)
dotnet build HoaTheoMua.sln -c Release

# Run the API locally → http://localhost:5066, Swagger UI at /swagger
dotnet run --project HoaTheoMua.API

# Restore
dotnet restore HoaTheoMua.sln

# EF Core migrations (run from repo root)
dotnet ef migrations add <Name> --project HoaTheoMua.Repository --startup-project HoaTheoMua.API
dotnet ef database update --project HoaTheoMua.Repository --startup-project HoaTheoMua.API
```

Migrations are **applied automatically at startup** (`Program.cs` calls `dbContext.Database.MigrateAsync()`), so a fresh DB self-bootstraps on first run — you rarely need `database update` manually.

There is **no test project** and no test runner configured. The `HoaTheoMua.Service/test/` folder contains only ad-hoc scripts, not unit tests.

## Architecture

Three-project layered solution, dependencies flow API → Service → Repository:

- **`HoaTheoMua.API`** — web host. Controllers, middleware, DI wiring (`Program.cs`), and `Extentions/` (note the spelling). Root namespace is `HoaTheoMua`, **not** `HoaTheoMua.API`.
- **`HoaTheoMua.Service`** — business logic, organized by feature folder.
- **`HoaTheoMua.Repository`** — EF Core: `AppDbContext`, `Entity/`, `Configurations/`, `Migrations/`, `Enum/`, `Abtraction/` (note the spelling).

### Feature-folder convention (Service layer)

Every feature is a folder under `HoaTheoMua.Service/<Feature>/` with a fixed shape and namespace `HoaTheoMua.Service.<Feature>`:

- `IService.cs` — interface named exactly `IService`
- `Service.cs` — implementation named exactly `Service`
- `Request.cs` / `Response.cs` — request/response DTOs as nested classes (e.g. `Request.CreateConfigRequest`)
- `Convert.cs` — optional mapping helpers

Because every feature reuses the names `IService`/`Service`, `Program.cs` aliases them on import and registers each one explicitly:

```csharp
using ConfigService = HoaTheoMua.Service.Config;
builder.Services.AddScoped<ConfigService.IService, ConfigService.Service>();
```

When adding a feature: create the folder following this shape, then add the alias + `AddScoped` line in `Program.cs`. External-API features (e.g. `Product`, `AhamoveService`) are registered with `AddHttpClient<...>` instead of `AddScoped`.

### Controllers

Thin — they delegate to the feature `IService` and wrap the result in the response envelope. Routes are explicit attributes (`[HttpPost("/api/v1/...")]`), there is no convention-based routing:

```csharp
var result = await _service.DoThing(request);
return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
```

### Response & error envelope

All responses use `ApiResponseFactory` (in `HoaTheoMua.Service/Models/ApiResponse.cs`): `Base(...)`, `BasePagination(...)`, `Error(...)`.

Errors are **thrown**, not returned. Throw an `AppException` subclass from the Service layer (`BadRequestException`, `NotFoundException`, `UnauthorizedException`, `ServerException`, `ExpiredAccessTokenException` — see `HoaTheoMua.Service/Exceptions/`). `GlobalExceptionHandlerMiddleware` catches them and renders the error envelope with the right status code. Any non-`AppException` becomes a generic 500 `ServerException` (full detail only leaks in Development).

### Repository conventions

- Entities derive from `BaseEntity<TKey>` (`Id` + `IsDeleted`). Many also implement `IAuditableEntity` (`CreatedAt`/`UpdatedAt`).
- **Soft delete**: set `IsDeleted = true` and always filter reads with `.Where(x => !x.IsDeleted)`. There is no global query filter — the predicate is manual in each query.
- Entity mappings are `IEntityTypeConfiguration<T>` classes in `Configurations/`, auto-discovered via `ApplyConfigurationsFromAssembly`. Add a config class rather than fluent code in `OnModelCreating`.
- The Npgsql data source is built with `EnableDynamicJson()` so JSON columns work. `Config.Value` is stored as a raw JSON string and surfaced as a `System.Text.Json.JsonElement`.

### Auth & authorization

JWT Bearer configured in `Extentions/JwtExtensions.cs`. The token is read from the `Authorization` header, falling back to an access-token cookie (`CookieExtensions.AccessTokenCookieName`). `ICurrentUserService` (`HoaTheoMua.Service/Common/`) reads claims (`UserId`, `Email`, `FullName`, `Avatar`, `IsVerified`) straight off the validated token — no DB lookup. Use `_currentUser.GetRequiredUserId()` to require an authenticated caller.

Named policies are constants on `JwtExtensions`: `AdminPolicy`, `StaffPolicy`, `UserPolicy`, `StaffOrAdminPolicy`, `UserVerifiedPolicy`, `CustomerPolicy`. Role name constants live in `HoaTheoMua.API/Constants/RoleNames.cs`. The **default** authorization policy requires an authenticated user **and** an `IsVerified=true` claim.

### Configuration (options pattern)

Config sections bind to option classes via `configuration.GetSection(nameof(XOption)).Bind(...)`: `JwtOption`, `AhamoveOption`, `NhanhVnOption`, `PostgresOption`. The Postgres connection string is `ConnectionStrings:PostgresSql`.

In Docker, every setting is supplied as an env var using the `__` (double-underscore) section separator, e.g. `JwtOption__AccessTokenKey`, `NhanhVnOption__Token`, `ConnectionStrings__PostgresSql`. `appsettings.json` holds working dev defaults (including live-looking tokens) that are overridden by these env vars in deployed environments.

## Infrastructure & workflow

- **Git hooks (Husky.Net, installed via `dotnet tool`):**
  - `pre-commit` → `dotnet build -c Release` must pass.
  - `commit-msg` → Conventional Commits enforced (`feat|fix|docs|style|refactor|perf|test|chore|build|ci`, optional `(scope)`).
  - `pre-push` → branch name must match `feature/`, `bugfix/`, `hotfix/`, `release/`, or be `develop`/`main`/`master`; then restore + build.
- **Docker:** images build from `HoaTheoMua.API/Dockerfile` (multi-stage, exposes 8080, `ASPNETCORE_URLS=http://+:8080`). Compose lives in `.docker/` and is profile-driven (`dev`, `prod`); `.docker/sync-env-secrets.sh` pushes a `.docker/.env.<env>` file into GitHub environment secrets.
- **CI/CD (`.github/workflows/`):** push to `develop` deploys the `dev` profile, push to `main` deploys `prod`. Build → deploy (self-hosted runner behind a Cloudflare tunnel) → Discord notification.

## Conventions to respect

- The folder names `Extentions` and `Abtraction` are misspelled consistently throughout the codebase — match them, don't "fix" them, or namespaces/usings break.
- Match the `IService`/`Service` feature-folder shape for any new business capability; don't invent per-feature interface names.
- The API root namespace is `HoaTheoMua`; extension helpers live under both `HoaTheoMua.Extension` and `HoaTheoMua.Extentions` (both exist — check which a given helper uses).
- `Quartz` packages are referenced but no jobs are currently scheduled; there is no background-job infrastructure wired up despite the dependency.
