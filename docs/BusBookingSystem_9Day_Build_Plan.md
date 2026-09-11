# Bus Booking System — 9-Day MVP Build Plan
Target: backend REST API only (no Blazor UI), true to what the MRA CV claims.
Window: today → 19 Sept (submit MRA application 20 Sept). ~8hrs/day.

Architecture reminder (yours):
Core ← Application ← Infrastructure ← API   (Web/Blazor is out of scope for this sprint)

Flow per feature, each day: **Entity (Core) → DTO + Interface (Application) → Implementation (Infrastructure) → Controller (API) → manual/automated test**

---

## Decisions locked in before Day 1
- Use `User` (with `UserType.Customer`) as the booking owner. Shelve the separate `Customer` entity for now — avoids a redundant join/mapping layer.
- Collapse `Schedule` + `Trip` into a single `Trip` entity: `BusId`, `RouteId`, `DepartureTime`, `ArrivalTime`, `TotalSeats`, `AvailableSeats`, `BaseFare`. This is a deliberate scope simplification — name it if asked in interview.

---

## Day 1 — Finish the Core domain model
- Review/complete existing entities: `Depot` (Name, City, AddressId), `Bus` (PlateNumber, Model, Capacity), `Address`.
- Add new entities: `Route` (OriginDepotId, DestinationDepotId, DistanceKm), `Trip` (BusId, RouteId, DepartureTime, ArrivalTime, TotalSeats, AvailableSeats, BaseFare, RowVersion for concurrency), `Booking` (UserId, TripId, SeatCount, BookingReference, Status, CreatedAt).
- Add `BookingStatus` enum: Pending, Confirmed, Cancelled.
- Goal: `Core` project compiles cleanly with the full domain model. No DB yet.

## Day 2 — Application layer: read-side contracts
- DTOs: `DepotDto`, `BusDto`, `RouteDto`, `TripDto`, `TripSearchResultDto`.
- Repository interfaces: `IDepotRepository`, `IBusRepository`, `IRouteRepository`, `ITripRepository` (include `SearchTrips(originDepotId, destinationDepotId, date)`).
- Service interfaces + implementations: `IDepotService`, `IBusService`, `ITripService` — thin orchestration, manual DTO mapping (skip AutoMapper, one less thing to debug this week).
- Goal: `Application` compiles against `Core`, all read-side contracts defined.

## Day 3 — Infrastructure: EF Core + SQL Server
- Install `Microsoft.EntityFrameworkCore.SqlServer` + `.Tools`.
- `AppDbContext` with all `DbSet`s; Fluent API configs for keys, required fields, relationships, `RowVersion` on `Trip`.
- Implement repositories (`DepotRepository`, `BusRepository`, `RouteRepository`, `TripRepository`) against `AppDbContext`.
- `dotnet ef migrations add InitialCreate` → `dotnet ef database update`.
- Seed a handful of depots/buses/routes/trips (seed method or migration seed data).
- Goal: real SQL Server database exists and is queryable.

## Day 4 — API: DI wiring + read-only endpoints
- `Program.cs`: register `AppDbContext`, all repositories and services.
- Swagger/OpenAPI enabled (package already present).
- Controllers: `DepotsController`, `BusesController`, `RoutesController`, `TripsController` (list, get-by-id, `GET /api/trips/search?from=&to=&date=`).
- Manually test every endpoint via Swagger UI.
- Goal: full read-side API browsable and working.

## Day 5 — Booking creation (the core value feature)
- Application: `CreateBookingRequest`/`BookingDto`, `IBookingService.CreateBooking(...)`, `IBookingRepository`.
- Infrastructure: implement booking creation inside a DB transaction — check `AvailableSeats`, decrement, insert `Booking`, generate a `BookingReference`; rely on the `RowVersion` concurrency token on `Trip` to reject a racing double-booking rather than silently overselling.
- API: `BookingsController` — `POST /api/bookings`, `GET /api/bookings/{id}`.
- Manually verify: booking succeeds normally, and fails cleanly when requested seats exceed availability.
- Goal: booking flow works end-to-end with basic overbooking protection. **This is the highest-value day — protect it if the schedule slips.**

## Day 6 — JWT authentication
- Lightweight custom auth on `User` (password hash field + `AuthController` with `/register` and `/login` issuing JWT from a symmetric key in config) — skip full ASP.NET Identity to save setup time.
- Wire JWT bearer middleware in `Program.cs`.
- `[Authorize]` on booking endpoints; leave search/read endpoints public.
- Goal: register → login → call protected booking endpoint with a Bearer token.

## Day 7 — Error handling & polish
- Model validation via DataAnnotations (fast, no extra package).
- Centralized exception-handling middleware → consistent `ProblemDetails`-style error responses (matches the pattern you already used in Nyumba).
- Clean up: remove leftover `Class1.cs` placeholders, tidy naming, add Swagger summaries per endpoint.
- Goal: the API looks and behaves like a finished product, not a scaffold.

## Day 8 — Tests
- Add an xUnit test project referencing `Application` (+ EF Core InMemory for repo/service tests).
- Cover: trip search filtering, booking happy path, booking rejected when overbooked, trip-not-found case.
- Goal: real, if modest, automated coverage — enough to honestly say "service-level tests" on the CV.

## Day 9 — Buffer, README, final fact-check
- Reserved slack day — sprints like this always slip somewhere (Day 5 or 6 most likely).
- Write a README: architecture, how to run, endpoint list.
- Push all commits with a real history (not one giant commit) — this is what makes the CV checkable.
- **Walk through every CV bullet against the live Swagger UI and cut anything that isn't actually true yet.**
- Submit the MRA application.

---

## If time runs short, cut in this order (last resort first):
1. Test coverage depth (keep at least the booking happy-path + overbooking test)
2. Role richness in JWT (keep basic auth, drop admin/customer role split)
3. Error-handling polish
Never cut Day 5 (booking creation) — it's the feature the whole CV story depends on.
