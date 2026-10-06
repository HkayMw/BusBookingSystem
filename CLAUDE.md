# CLAUDE.md

Guidance for working in this repository.

## Project overview

Bus Booking System: a backend REST API MVP (ASP.NET Core, .NET 10, EF Core + SQL Server). Customers browse depots, search trips, view trip details, register/log in (JWT), and book seats and/or cargo space on a trip.

Sprint planning and design decisions live in [docs/](docs/). Read [docs/PROJECT_HANDOFF_DAY_3_ONWARDS.md](docs/PROJECT_HANDOFF_DAY_3_ONWARDS.md) before making architectural changes. The docs can be out of date, so check the code too.

## Solution layout

Solution file: `BusBookingSystem.slnx`

| Folder | Project | Role |
|---|---|---|
| `BusBookingSystem.Core/` | Core | Domain entities (`Entities/`) and enums (`Enums/`). Has no dependencies. |
| `BusBookingSystem.Application/` | Application | DTOs, service interfaces, `Result<T>`, and custom exceptions. References Core only. |
| `BusBookingSystem.Infrastructure/` | Infrastructure | `AppDbContext`, EF configurations, migrations, and concrete services. |
| `BusBookingSystem.API/` | API | Controllers, DI registration, JWT, OpenAPI/Scalar, exception handler. |
| `BusBookingSystem.Tests/` | Tests | xUnit service tests that use the EF Core InMemory provider. |
| `BusBookingSystem.Web/` | Blazor Web | Default template only. Out of scope for the MVP. |

`BusBookingSystem.zip` is an archive and is not source code.

## Commands

```bash
dotnet build BusBookingSystem.slnx
dotnet test BusBookingSystem.Tests
dotnet test BusBookingSystem.Tests --filter "FullyQualifiedName~TripServiceTests"   # single class
dotnet run --project BusBookingSystem.API/BusBookingSystem.API.csproj                 # Scalar UI at /scalar in Development

# EF Core migrations (Infrastructure holds migrations, API is the startup project)
dotnet ef migrations add <Name> --project BusBookingSystem.Infrastructure --startup-project BusBookingSystem.API
dotnet ef database update      --project BusBookingSystem.Infrastructure --startup-project BusBookingSystem.API
```

The connection string (`DefaultConnection`, local SQLEXPRESS) and `JwtSettings:Secret` are in [BusBookingSystem.API/appsettings.json](BusBookingSystem.API/appsettings.json). The API will not start if the JWT secret is missing.

## Architecture

### Layered (Clean-style) dependencies

```
Core  <-  Application  <-  Infrastructure  <-  API
```

- Core never references EF Core, ASP.NET, or the other projects.
- Application defines **contracts** (`Interfaces/Services/I*Service`) and DTOs. It must not reference Infrastructure or API.
- Infrastructure **implements** those interfaces.
- API depends on abstractions (`I*Service`) and registers the implementations in [Program.cs](BusBookingSystem.API/Program.cs) with `AddScoped`.

### Feature development: vertical slices

Features are built one complete customer-visible slice at a time, not one layer at a time: user story → acceptance criteria → DTO → service interface → service implementation → controller → manual check (Scalar / `.http` file) → tests. A slice usually touches every project.

### Request flow

```
Controller  ->  I*Service (Application)  ->  *Service (Infrastructure)  ->  AppDbContext
            <-  ApiResponse<T> via result.ToApiResponse()  <-  Result<T>
```

### Key decisions (follow these)

- **No repository pattern.** Services inject `AppDbContext` directly. Do not add repository interfaces or classes.
- **Services return `Result<T>`** ([Application/Common/Result.cs](BusBookingSystem.Application/Common/Result.cs)) and use the factory methods: `Ok`, `Created`, `NoContent`, `BadRequest`, `NotFound`, `Conflict`, `UnAuthorized`, `Forbidden`, `Unexpected`. Expected failures such as validation, not-found, and capacity conflicts are returned as `Result`, not thrown.
- **Controllers stay thin.** They call the service, convert the result with `result.ToApiResponse()` ([Extensions/ResultExtensions.cs](BusBookingSystem.API/Extensions/ResultExtensions.cs)), and return `StatusCode(response.StatusCode, response.Body)`. They contain no business logic and no DbContext access.
- **Uniform response envelope:** every response, including errors, is an `ApiResponse<T>` with `Success`, `StatusCode`, `Message`, `Data`, `Errors`, and `Timestamp`. Model-validation failures are wrapped by the `InvalidModelStateResponseFactory` in Program.cs. Unhandled exceptions go through [GlobalExceptionHandler.cs](BusBookingSystem.API/GlobalExceptionHandler.cs), which maps `ArgumentException` to 400, `ResourceNotFoundException` to 404, `BookingConflictException` to 409, and anything else to 500.
- **DTOs are designed from the use case, not copied from the entity.** Include only the fields the client needs, such as IDs needed in a later request and display names. Never expose entities, navigation graphs, audit fields, or password hashes.
- **Read queries project directly to DTOs** with LINQ `.Select(...)` and `.AsNoTracking()` (see [TripService.cs](BusBookingSystem.Infrastructure/Services/TripService.cs)). AutoMapper is used only in `AuthService` for `User` mapping. Those maps are registered inline in Program.cs; `MappingProfile` is not registered. Prefer explicit projection or manual mapping for new code.
- **Writes that check and change capacity run inside a transaction** (`BeginTransactionAsync`, then roll back on every early return or exception). See `BookingService.CreateBookingAsync`. `Trip` and `Booking` have `RowVersion` concurrency tokens.
- **Authentication:** JWT bearer with an HMAC symmetric key. Tokens carry the `NameIdentifier` (user Id), `Email`, `Name`, and `Role` (`UserType`) claims. Protected controllers use `[Authorize]` and read the user id with `User.FindFirstValue(ClaimTypes.NameIdentifier)`. Passwords are hashed with `PasswordHasher<User>`.
- **Domain:** `User` with `UserType.Customer` owns bookings, and there is no separate Customer entity. `Trip` combines schedule and trip (bus + route + times + fares + booked counts). Available seats and cargo are computed as `Bus capacity - Trip booked`. All ids are `Guid`.

### Persistence

- One `IEntityTypeConfiguration<T>` per entity in `Infrastructure/Data/Configurations/`, picked up by `ApplyConfigurationsFromAssembly`. Do not use data annotations on Core entities for mapping.
- Configurations set the key, required fields and lengths, `IsRowVersion()`, `datetimeoffset` audit columns with a `SYSUTCDATETIME()` default, `DeleteBehavior.Restrict` relationships, check constraints, and indexes.
- Timestamps use `DateTimeOffset` (`CreatedAt`, nullable `UpdatedAt`).
- Add a migration for every model change. Do not hand-edit existing migrations.

## Coding style

Match the existing code:

- **Namespaces:** block-scoped `namespace X { ... }`. Tests use file-scoped namespaces. Namespaces follow the folders, **except DTOs: every DTO uses the flat namespace `BusBookingSystem.Application.DTOs`** regardless of subfolder (`DTOs/Trip/`, `DTOs/Auth/`, ...).
- **Nullable reference types and implicit usings are enabled** in every project. Use `required` for mandatory init properties, `= default!` for non-nullable navigations and strings set by EF, and `?` for optional values.
- **Naming:** PascalCase types and members, `_camelCase` private readonly fields, `I` prefix for interfaces, and an `Async` suffix on async methods (`GetActiveDepotsAsync`, `CreateBookingAsync`). DTOs end in `Dto` (`TripListResultDto`, `BookingRequestDto`, `UserRegisterRequestDto`). Controllers are plural resource names (`TripsController`, `DepotsController`) with `[Route("api/[controller]")]`.
- **Constructors:** newer controllers use primary constructors that assign to a `_field` (`public class TripsController(ITripService tripService)` with `private readonly ITripService _tripService = tripService;`). Services use classic constructor injection. Either is fine, but keep each file consistent.
- **Controller actions** are documented for OpenAPI with `[EndpointSummary("...")]` and a `[ProducesResponseType(typeof(ApiResponse<T>), StatusCodes.StatusXxx)]` for each possible status (use `ApiResponse<object>` for error shapes). Return `Task<ActionResult<ApiResponse<T>>>`.
- **Validation:** do input shape checks with DataAnnotations on request DTOs (`[Required]`, `[Range]`, `[StringLength]`, `[EmailAddress]`). Do business and guard checks at the top of the service method, returning `Result<T>.BadRequest(message, nameof(field))`.
- **Service messages** are short, human-readable sentences ending in a period, e.g. `"Trip was not found."` and `"Trips retrieved successfully."`.
- Collections returned from services are `IReadOnlyList<T>`.
- Use `is null` / `is not null` rather than `== null`.
- **Tests:** xUnit `[Fact]`, names like `MethodName_ExpectedBehavior` (`GetActiveDepotsAsync_OnlyReturnsActiveDepots`), explicit `// Arrange / // Act / // Assert` sections, and a fresh `UseInMemoryDatabase(Guid.NewGuid().ToString())` per test. Tests call Infrastructure services directly against `AppDbContext`. The InMemory provider does not support transactions or SQL check constraints, so behavior that depends on them needs a relational or SQLite setup. When testing services that open a transaction, add `.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))`. InMemory also does not generate `RowVersion`, so seeded entities set it by hand and service-created ones need a `SavingChanges` hook (see [BookingServiceTests.cs](BusBookingSystem.Tests/BookingServiceTests.cs)).

## Known issues / gotchas

- The check constraint `CK_Booking_NumberOfSeats_greaterThanZero` now checks `>= 0` (to allow cargo-only bookings), so its name no longer matches what it checks.
- `BookingRequestDto` has no namespace declaration (it is in the global namespace).
- `BookingController` returns 200 (`Result.Ok`) for a created booking, not 201.
- `Program.cs` calls `AddOpenApi()` twice and also registers `AddSwaggerGen()`. Scalar (`MapScalarApiReference`) is the UI actually mapped.
- `ResourceNotFoundException` and `BookingConflictException` exist, but current services return `Result` failures instead of throwing them.
