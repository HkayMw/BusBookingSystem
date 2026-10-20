# Bus Booking System

A .NET 10 solution for booking bus seats and cargo space between depots in Malawi. The working part is a REST API built with ASP.NET Core, EF Core and SQL Server. Customers browse depots, search trips, register, log in with a JWT and book seats, cargo or both. Bookings run inside a database transaction with optimistic concurrency, so two customers cannot take the last seat. The solution also contains a Blazor web project for the planned customer front end; it is created from the template but not built yet.

## Status

| Part                                       | Status                                                                                     |
| ------------------------------------------ | ------------------------------------------------------------------------------------------ |
| REST API (`BusBookingSystem.API`)          | Working: depots, trip search and details, register and log in, create and look up bookings |
| Automated tests (`BusBookingSystem.Tests`) | Service, API and SQL Server suites                                                         |
| Blazor web app (`BusBookingSystem.Web`)    | Template only, not connected to the API yet                                                |

## Customer journey

```text
browse depots -> search trips -> view trip -> register -> log in -> book seats and/or cargo -> look up booking
```

The whole journey, including failure cases, is scripted in [`CustomerJourney.http`](BusBookingSystem.API/HttpRequests/CustomerJourney.http). It has been run end to end on a freshly seeded database using the [clean-database run-through](docs/docs/CustomerJourney.md).

## Quick start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server. SQL Server Express is fine; the default connection string expects `(local)\SQLEXPRESS`.
- The EF Core CLI: `dotnet tool install --global dotnet-ef`

### Run the API

```bash
git clone https://github.com/HkayMw/BusBookingSystem.git
cd BusBookingSystem

# 1. Set the JWT signing secret (at least 32 bytes). It is never stored in appsettings.json.
dotnet user-secrets set "JwtSettings:Secret" "<a random value of at least 32 characters>" --project BusBookingSystem.API

# 2. If you are not using (local)\SQLEXPRESS, change ConnectionStrings:DefaultConnection
#    in BusBookingSystem.API/appsettings.json.

# 3. Create the database
dotnet ef database update --project BusBookingSystem.Infrastructure --startup-project BusBookingSystem.API

# 4. Run the API (Development seeds demo data on first start)
dotnet run --project BusBookingSystem.API --launch-profile https
```

Then either:

- open **https://localhost:7278/scalar** for the interactive API reference, or
- open [`BusBookingSystem.API/HttpRequests/CustomerJourney.http`](BusBookingSystem.API/HttpRequests/CustomerJourney.http) in Visual Studio (or VS Code with the REST Client extension) and send the requests top to bottom.

Use the `https` profile. Over plain HTTP, the redirect to HTTPS drops the `Authorization` header and protected requests return 401.

The API refuses to start if `JwtSettings:Secret` is missing or shorter than 32 bytes. Outside Development, set it with the `JwtSettings__Secret` environment variable.

## Seed data

In Development, `DevelopmentDataSeeder` fills an empty database on startup. It does nothing if any depot already exists, so restarting never duplicates data. No users are seeded: customers register through the API.

Trips are created for **tomorrow** (Malawi time, UTC+2), relative to the day the database is seeded. After a day or two they are in the past. To reseed, drop and recreate the database:

```bash
dotnet ef database drop --force --project BusBookingSystem.Infrastructure --startup-project BusBookingSystem.API
dotnet ef database update      --project BusBookingSystem.Infrastructure --startup-project BusBookingSystem.API
```

Fares are in Malawi kwacha (MWK).

| Data   | What is seeded                                                                       | Rule it demonstrates                                |
| ------ | ------------------------------------------------------------------------------------ | --------------------------------------------------- |
| Depots | Lilongwe Central, Blantyre City, Mzuzu City, Dedza Depot, and Jenda Depot (inactive) | Inactive depots are hidden from `GET /api/depots`   |
| Buses  | Three coaches with cargo space, one 25-seat minibus without                          | Cargo is rejected on a bus with no cargo space      |
| Trip   | Lilongwe → Blantyre, 14:00, 56 seats, 1000 kg cargo                                  | Main happy path: seats and cargo                    |
| Trip   | Blantyre → Lilongwe, 07:00, seat fare 70,000.50                                      | Fractional fares are kept exactly (`decimal(18,2)`) |
| Trip   | Lilongwe → Mzuzu, 14:00                                                              | Search filters by route                             |
| Trip   | Mzuzu → Lilongwe, 07:00, no cargo fare                                               | Cargo is rejected on a trip without a cargo fare    |
| Trip   | Lilongwe → Dedza, 09:00, on the minibus                                              | Cargo is rejected on a bus with no cargo space      |

## Endpoints

All responses use the envelope described in [Responses](#responses).

| Method | Route                                                                 | Auth   | Purpose                                     | Responses               |
| ------ | --------------------------------------------------------------------- | ------ | ------------------------------------------- | ----------------------- |
| `GET`  | `/api/depots`                                                         | –      | Active depots, ordered by name              | 200                     |
| `GET`  | `/api/trips`                                                          | –      | All trips                                   | 200                     |
| `GET`  | `/api/trips/search?originDepotId=&destinationDepotId=&departureDate=` | –      | Trips on a route for one date               | 200, 400                |
| `GET`  | `/api/trips/{tripId}`                                                 | –      | Trip details with available seats and cargo | 200, 404                |
| `POST` | `/api/auth/register`                                                  | –      | Register a customer                         | 201, 400, 409           |
| `POST` | `/api/auth/login`                                                     | –      | Log in and receive a JWT                    | 200, 400, 401           |
| `POST` | `/api/booking`                                                        | Bearer | Book seats, cargo or both                   | 201, 400, 401, 404, 409 |
| `GET`  | `/api/booking/{bookingId}`                                            | Bearer | Look up one of your own bookings            | 200, 400, 401, 404      |

Notes:

- Emails are case-insensitive for both registration and login.
- A failed login returns the same message for an unknown email and a wrong password.
- Looking up another customer's booking returns **404**, not 403, so a stranger cannot tell that the booking ID exists.
- The booking route is singular (`/api/booking`).
- Unexpected errors return a generic 500 without internal details.

## Responses

Every response, success or failure, has the same shape:

```json
{
  "success": true,
  "statusCode": 201,
  "message": "Booking created successfully.",
  "data": {},
  "errors": null,
  "timestamp": "2026-10-19T08:00:00Z"
}
```

### Example: create a booking

```http
POST /api/booking
Authorization: Bearer <token>
Content-Type: application/json

{ "tripId": "<trip id>", "numberOfSeats": 2, "cargoWeight": 10 }
```

`201 Created` (IDs, reference and timestamp vary):

```json
{
  "success": true,
  "statusCode": 201,
  "message": "Booking created successfully.",
  "data": {
    "bookingId": "8c1f0a52-0000-0000-0000-000000000000",
    "customerName": "Demo Customer",
    "bookingReference": "BK-20261019080000-3F9A1C",
    "bookingStatus": 0,
    "tripSummary": "Lilongwe Central → Blantyre City on Tue, 20 Oct 2026",
    "numberOfSeats": 2,
    "cargoWeight": 10,
    "seatsFare": 140000.0,
    "cargoFare": 8000.0,
    "totalFare": 148000.0
  },
  "errors": null,
  "timestamp": "2026-10-19T08:00:00Z"
}
```

`bookingStatus` is serialized as a number: `0` = Pending, `1` = Confirmed, `2` = Cancelled.

### Example: not enough seats

`409 Conflict`:

```json
{
  "success": false,
  "statusCode": 409,
  "message": "Not enough seats available. Requested: 57, Available: 54.",
  "data": null,
  "errors": null,
  "timestamp": "2026-10-19T08:00:00Z"
}
```

Validation failures return `400` with `"message": "The request is invalid."` and the field errors in `errors`.

## Architecture

```text
Core  <-  Application  <-  Infrastructure  <-  API
```

| Project                           | Role                                                                                            |
| --------------------------------- | ----------------------------------------------------------------------------------------------- |
| `BusBookingSystem.Core`           | Domain entities and enums. No dependencies.                                                     |
| `BusBookingSystem.Application`    | DTOs, service interfaces and `Result<T>`. References Core only.                                 |
| `BusBookingSystem.Infrastructure` | `AppDbContext`, EF Core configurations and migrations, service implementations, seed data.      |
| `BusBookingSystem.API`            | Controllers, JWT authentication, OpenAPI and Scalar, global exception handler.                  |
| `BusBookingSystem.Tests`          | xUnit service, API and SQL Server tests.                                                        |
| `BusBookingSystem.Web`            | Blazor Web App (interactive server rendering). Template pages only; planned customer front end. |

Request flow:

```text
Controller -> I*Service (Application) -> *Service (Infrastructure) -> AppDbContext
           <- ApiResponse<T>           <- Result<T>
```

## Design decisions

- **No repository layer.** Services use `AppDbContext` directly. EF Core already provides the unit of work and the query abstraction, and an extra layer would add code without adding behaviour.
- **`Result<T>` for expected failures.** Validation errors, missing records and capacity conflicts are returned as results, not thrown as exceptions. Controllers convert them to HTTP responses in one place.
- **One response envelope.** `ApiResponse<T>` is used for every success and failure, including model validation errors and unhandled exceptions, so a client handles a single shape. This replaces the `ProblemDetails` format that was first planned.
- **Transactional booking with optimistic concurrency.** Creating a booking checks capacity, updates the trip's booked counts and inserts the booking in one transaction. `Trip.RowVersion` makes a concurrent update fail with `409 Conflict` instead of overbooking.
- **Fares are stored on the booking.** Seat, cargo and total fares are saved when the booking is made, so later fare changes do not change what a customer paid. Money uses `decimal(18,2)`.
- **Cargo needs both capacity and a fare.** A bus without cargo space, or a trip without a cargo fare, rejects cargo bookings with a specific message.
- **Simple domain model.** `Trip` combines the schedule and the trip (bus, route, times, fares, booked counts). A `User` with the customer type owns bookings; there is no separate customer entity.
- **Secrets stay out of source control.** The JWT secret comes from user secrets in Development and from an environment variable elsewhere.
- **Read queries project straight to DTOs** with `Select` and `AsNoTracking`, so entities and password hashes never leave the service layer.
- **Scalar instead of Swagger UI** for the API reference, at `/scalar` in Development.

## Testing

```bash
dotnet test BusBookingSystem.slnx
```

| Suite            | Folder                              | What it covers                                                                                                                         |
| ---------------- | ----------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| Service tests    | `BusBookingSystem.Tests/`           | Business rules in each service, using the EF Core InMemory provider                                                                    |
| API tests        | `BusBookingSystem.Tests/Api/`       | Authentication and ownership through the real HTTP pipeline (`WebApplicationFactory`), with a test JWT secret and an InMemory database |
| SQL Server tests | `BusBookingSystem.Tests/SqlServer/` | The last-seat race and a database check constraint, against a throwaway database that is created, migrated and dropped                 |

The SQL Server tests use the server in the `BUSBOOKING_TEST_SQLSERVER` environment variable, or `(local)\SQLEXPRESS` by default, and are skipped when no server is reachable. To skip them explicitly:

```bash
dotnet test BusBookingSystem.slnx --filter "Category!=SqlServer"
```

For the manual end-to-end check, follow the [clean-database run-through](docs/docs/CustomerJourney.md).

## Limitations and future work

What the system does not do yet:

- **Blazor front end.** `BusBookingSystem.Web` is the unmodified template and does not call the API.
- **Booking lifecycle.** New bookings stay `Pending`. There is no confirmation, cancellation or refund.
- **Payments.** None.
- **Administration.** Depots, buses, routes and trips come only from the seed data; there are no admin endpoints.
- **Authorization rules.** The token carries a role claim, but no endpoint checks it. Tokens last seven days and there are no refresh tokens or revocation.
- **Deployment.** No Docker setup, CI pipeline or hosting.

Next steps, roughly in order: the Blazor customer front end, a confirmation step for bookings, admin endpoints for managing the network, then roles, refresh tokens and a CI pipeline.

## Documentation

- [`docs/docs/`](docs/docs/): project documentation, including the [clean-database run-through](docs/docs/CustomerJourney.md).
- [`docs/`](docs/): development notes, the build plan and progress tracking.
