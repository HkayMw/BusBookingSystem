# Day 9 Walkthrough - Buffer, Documentation, and Release Fact-Check

Day 9 turns a working codebase into something a reviewer can run, follow and believe. No new features are planned. The day is for closing the riskiest gap, proving the journey from a clean database, writing the README, and checking that every claim about the project is backed by working code.

```text
close the highest-risk gap
  -> run the full journey from a clean database
  -> write the README around that journey
  -> fact-check every claim against working behaviour
  -> clean commit history and merge
  -> submit
```

The architecture does not change today:

```text
Core <- Application <- Infrastructure <- API
```

## 1. User story

> As a reviewer, I want to run the project and understand the implemented journey so that the work is credible and checkable.

## 2. Current project state

Check this list against the repository before starting. It was accurate when this walkthrough was written.

### What exists and works

- **Public read journey:** `GET /api/depots`, `GET /api/trips`, `GET /api/trips/search`, `GET /api/trips/{tripId}`.
- **Authentication:** `POST /api/auth/register`, `POST /api/auth/login`. Passwords are hashed with `PasswordHasher<User>`, and logins return a JWT. A failed login gives the same message for an unknown email and a wrong password.
- **Booking:** `POST /api/booking` (protected by `[Authorize]`, returns `201 Created`). It covers seats, cargo, or both, inside a transaction. A `RowVersion` conflict returns `409`, not `500`.
- **Consistent responses:** every response, including errors, uses the `ApiResponse<T>` envelope. Unhandled exceptions return a generic `500` without internal details.
- **Secrets:** the JWT secret is not in `appsettings.json`. It comes from user secrets in development or the `JwtSettings__Secret` environment variable elsewhere.
- **Tests:** 39 passing.
  - Service tests (EF InMemory).
  - API tests through `WebApplicationFactory` (401 cases and a valid token).
  - SQL Server tests (last-seat race, check constraint) that skip when no server is reachable.

### Gaps that matter for Day 9

| Gap | Why it matters today |
|---|---|
| **No seed data** | A clean database has no depots, buses, routes or trips, and there is no endpoint to create them. The journey cannot be demonstrated from a fresh setup. **This is the highest-risk gap.** |
| No booking lookup endpoint | Day 5 and Day 6 mention `GET /api/bookings/{id}` with an ownership check. The booking response already returns the reference and fares, so the lookup is optional for this MVP. Decide today whether to build it or leave it out of every claim. |
| README is one line | A reviewer has nothing to follow. |
| `BusBookingSystem.API.http` only has two requests | The journey is not repeatable without clicking through Scalar. |
| Branch `knowIssuesFix` is not pushed or merged | `master` does not contain the security and correctness fixes. |
| Commit `21094c8` does not build on its own | It deletes the exception classes before `b811702` removes their uses. Squash it before pushing. |

### Deliberate differences from the original plan

Write these down in the README rather than hiding them:

- **Errors use the `ApiResponse<T>` envelope, not `ProblemDetails`** (Day 7 suggested `ProblemDetails`). It is one consistent shape for success and failure.
- **The booking route is `/api/booking` (singular)**, not `/api/bookings` as in the Day 5 and Day 6 docs. Either rename the controller to `BookingsController` today, or document the singular route. Do not leave the docs and code disagreeing.
- **Swagger UI was replaced by Scalar**, at `/scalar` in Development.

## 3. Scope for this day

Build or do:

- development seed data,
- optionally, booking lookup with an ownership check, only if seed data is finished early,
- a repeatable journey in the `.http` file,
- a clean-database run-through with recorded results,
- the README,
- a claims-to-evidence fact-check,
- a clean, pushed and merged commit history.

Do not build these today:

- admin endpoints for managing depots, buses, routes or trips,
- booking cancellation,
- refresh tokens or role-based authorization rules,
- payments,
- the Blazor UI,
- Docker or CI pipelines.

They are reasonable next steps, and the README can list them as future work. They are not Day 9.

## 4. Acceptance criteria

Day 9 is complete when:

- a fresh clone can follow the README to a running API without asking anyone anything,
- a clean database is seeded automatically in Development,
- the full customer journey runs from the `.http` file with the expected status codes,
- `dotnet test` passes,
- every claim in the CV and README points to code or a test that proves it,
- nothing is claimed that is not demonstrated,
- `master` contains all the work, with one meaningful commit per slice.

## 5. Order of work

Do the work in this order. Each step depends on the one before it.

```text
1. Seed data              (blocks everything else)
2. Booking lookup         (optional, time-boxed)
3. Journey in .http file
4. Clean-database run     (proves 1 and 3)
5. README                 (describes what 4 proved)
6. Claims fact-check      (compares the CV with 4 and 5)
7. Commits, push, merge
8. Submit
```

If time runs short, see section 13 for what to cut.

## 6. Task 1: development seed data

### Goal

A clean database becomes demo-ready the first time the API starts in Development.

### Approach: a runtime seeder, not `HasData`

EF Core's `HasData` bakes rows into migrations. It needs fixed IDs and constant values, so trip dates would be frozen in the past by the time a reviewer runs the project. A small runtime seeder can create trips relative to today.

Rules for the seeder:

- **Development only.** Never seed in other environments.
- **Idempotent.** If any depot exists, do nothing. Running the API twice must not duplicate data.
- **Uses the domain model.** Build normal entities and call `SaveChangesAsync`, so the configured check constraints and defaults apply.
- **No seeded users.** Customers register through the API as part of the journey. A seeded password would be a secret in source code.
- **No fake counters.** Leave `SeatsBooked` and `CargoCapacityBooked` at 0. A counter without matching bookings is inconsistent data. Demonstrate overbooking by requesting more seats than the bus has.

### Where it goes

```text
BusBookingSystem.Infrastructure/Data/DevelopmentDataSeeder.cs
```

It is called from `Program.cs` after `app` is built:

```csharp
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DevelopmentDataSeeder.SeedAsync(context);
}
```

### What to seed

Choose data that exercises every rule the journey needs to show:

| Data | Purpose |
|---|---|
| Lilongwe Central, Blantyre Central, Mzuzu Depot (active) | Origin and destination choices |
| Jenda Depot (**inactive**) | Proves `GET /api/depots` filters out inactive depots |
| `BUS-101` Coach, 40 seats, cargo space 200 | Seat and cargo bookings |
| `BUS-202` Minibus, 30 seats, **no** cargo space | "This bus does not support cargo bookings." |
| Routes: Lilongwe → Blantyre, Blantyre → Lilongwe, Lilongwe → Mzuzu | Search by route |
| Trip: Lilongwe → Blantyre, tomorrow 07:00, `BUS-101`, seat fare 3500.00, cargo fare 800.00 | Main happy path (seats + cargo) |
| Trip: Lilongwe → Blantyre, tomorrow 14:00, `BUS-202`, seat fare 3000.00, no cargo fare | Second result on the same search; cargo rejected |
| Trip: Blantyre → Lilongwe, the day after tomorrow, `BUS-101`, seat fare 3500.50, **no cargo fare** | Fractional fare; cargo rejected because the trip has no cargo fare |
| Trip: Lilongwe → Mzuzu, tomorrow 09:00, `BUS-101`, seat fare 4200.00, cargo fare 950.00 | Route filtering (must not appear in a Lilongwe → Blantyre search) |

### Dates and time zones

Search filters on `DepartureTime.Date`, and SQL Server takes that date in the offset the value was stored with. Seed departure times with Malawi's offset (UTC+2) so "tomorrow 07:00" is tomorrow for a Malawian customer:

```csharp
var malawi = TimeSpan.FromHours(2);
var tomorrow = DateTime.UtcNow.Date.AddDays(1);
var departure = new DateTimeOffset(tomorrow.AddHours(7), malawi);
```

Check this during the clean-database run (section 9). Searching for tomorrow's date must return both Lilongwe → Blantyre trips.

### Test it

Add one SQL Server test in `BusBookingSystem.Tests/SqlServer/`. It runs the seeder twice on the throwaway database and checks that the row counts are the same after the second run. That proves the seeder is idempotent and satisfies the real check constraints.

### Commit

```
Add development seed data for the demo journey
```

## 7. Task 2 (optional): booking lookup

Time-box this to two hours. If seed data overran, skip it and remove booking lookup from every claim.

### User story

> As a customer, I want to see my booking after creating it so that I can confirm what I paid for.

### Contract

```text
GET /api/booking/{bookingId}     [Authorize]
200 -> BookingResponseDto
404 -> booking does not exist OR belongs to another customer
401 -> no or invalid token
```

### The ownership rule

Filter by both values in one query:

```text
booking.Id == bookingId AND booking.UserId == authenticated user ID
```

Return `404`, not `403`, for another customer's booking. A `403` tells a stranger that the booking ID exists.

### Fares

`Booking` does not store fares. Recalculate them from the trip's current fares, or add `TotalFare` to `Booking` with a migration so the price is fixed at booking time. Storing the fare is the correct choice for a real system. If you recalculate instead, say so in the README.

### Tests

- Service: the owner gets the booking; another user gets `NotFound`; an unknown ID gets `NotFound`.
- API (`Api/`): no token returns `401`.

### Commit

```
Add booking lookup restricted to the booking owner
```

## 8. Task 3: make the journey repeatable

Replace the contents of `BusBookingSystem.API/BusBookingSystem.API.http` with the full journey, using variables for values captured from earlier responses. Visual Studio and the VS Code REST Client both support named requests:

```http
@host = http://localhost:5267

### 1. Depots
# @name depots
GET {{host}}/api/depots

### 2. Register
POST {{host}}/api/auth/register
Content-Type: application/json

{ "firstName": "Demo", "lastName": "Customer", "email": "demo@example.com", "password": "Passw0rd!" }

### 3. Login
# @name login
POST {{host}}/api/auth/login
Content-Type: application/json

{ "email": "demo@example.com", "password": "Passw0rd!" }

### 4. Book seats and cargo
POST {{host}}/api/booking
Authorization: Bearer {{login.response.body.$.data.token}}
Content-Type: application/json

{ "tripId": "<paste trip id from search>", "numberOfSeats": 2, "cargoWeight": 10 }
```

Continue the file with the failure cases from section 9. A reviewer should be able to run it top to bottom.

## 9. Task 4: clean-database run-through

This is the most important check of the day. It proves the README will work for someone else.

### Reset to a clean state

```bash
dotnet ef database drop --force --project BusBookingSystem.Infrastructure --startup-project BusBookingSystem.API
dotnet ef database update      --project BusBookingSystem.Infrastructure --startup-project BusBookingSystem.API
dotnet user-secrets list --project BusBookingSystem.API     # confirm JwtSettings:Secret is set
dotnet run --project BusBookingSystem.API --launch-profile https
```

Open `https://localhost:7278/scalar` and confirm the API documentation loads.

### Run the journey and record the results

Copy this table into your notes and fill in the "Actual" column. Every row must match before you write the README.

| # | Request | Expected |
|---|---|---|
| 1 | `GET /api/depots` | 200, three depots, no Jenda Depot |
| 2 | `GET /api/trips/search` Lilongwe → Blantyre, tomorrow | 200, two trips; the Mzuzu trip is absent |
| 3 | Same search, a date with no trips | 200, empty list |
| 4 | `GET /api/trips/{id}` for the 07:00 trip | 200, 40 seats available, cargo 200 |
| 5 | `GET /api/trips/{random guid}` | 404 |
| 6 | Register a customer | 201 |
| 7 | Register the same email in different capitals | 409 |
| 8 | Log in with the email in different capitals | 200, token returned |
| 9 | Log in with a wrong password | 401, "Invalid email or password." |
| 10 | Book with no token | 401 |
| 11 | Book 2 seats + 10 cargo on the 07:00 trip | 201, seats 7000.00, cargo 8000.00, total 15000.00 |
| 12 | `GET /api/trips/{id}` for the same trip | 38 seats available, cargo 190 |
| 13 | Book 41 seats on the 07:00 trip | 409, not enough seats |
| 14 | Book cargo on the 14:00 minibus trip | 409, bus does not support cargo |
| 15 | Book cargo on the Blantyre → Lilongwe trip | 409, cargo not available for this trip |
| 16 | Book 1 seat on the Blantyre → Lilongwe trip | 201, total 3500.50 |
| 17 | Book 0 seats and 0 cargo | 400 |
| 18 | Restart the API | No duplicate depots or trips |
| 19 | `dotnet test` | All tests pass |

If a row fails, fix it before moving on. A failure here is exactly what a reviewer would hit.

## 10. Task 5: the README

Write the README for a reviewer who has never seen the project and has about ten minutes. Lead with what the project does and how to run it; put depth further down.

### Suggested outline

```text
# Bus Booking System

One paragraph: what it is (REST API MVP for booking bus seats and cargo
space between depots in Malawi) and what it demonstrates.

## Customer journey
  browse depots -> search trips -> view trip -> register -> log in -> book

## Quick start
  Prerequisites: .NET 10 SDK, SQL Server (Express is fine), dotnet-ef tool
  1. Clone
  2. dotnet user-secrets set "JwtSettings:Secret" "<at least 32 bytes>" --project BusBookingSystem.API
  3. Adjust DefaultConnection in appsettings.json if not using (local)\SQLEXPRESS
  4. dotnet ef database update --project BusBookingSystem.Infrastructure --startup-project BusBookingSystem.API
  5. dotnet run --project BusBookingSystem.API --launch-profile https
  6. Open https://localhost:7278/scalar, or run BusBookingSystem.API.http

## Seed data
  What is seeded and which rule each item demonstrates (the table from section 6)

## Endpoints
  Table: method, route, auth required, purpose, success and failure codes

## Example requests and responses
  One booking request and its 201 response, plus one 409 response,
  showing the ApiResponse envelope

## Architecture
  Core <- Application <- Infrastructure <- API, with one line per project
  Request flow: Controller -> I*Service -> *Service -> AppDbContext -> Result<T> -> ApiResponse<T>

## Design decisions
  - No repository layer: services use AppDbContext directly
  - Result<T> for expected failures; ApiResponse<T> envelope for every response
  - Trip combines schedule and trip; User with UserType.Customer owns bookings
  - Booking runs in a transaction; Trip.RowVersion gives optimistic concurrency (409 on conflict)
  - Fares are decimal(18,2); a trip without a cargo fare does not accept cargo
  - JWT secret from user secrets or environment, never committed

## Testing
  dotnet test, the three suites, and how to skip SQL Server tests

## Limitations and future work
  No admin endpoints, cancellation, payments, refresh tokens or role rules;
  Blazor UI not started; fares not stored on bookings (if lookup recalculates)
```

### Rules for the README

- Every command must be one you ran during section 9.
- Every endpoint listed must exist. Every endpoint that exists must be listed.
- State the limitations plainly. A reviewer trusts a project that says what it does not do.
- Do not paste the JWT secret, connection strings with passwords, or real email addresses.

### Commit

```
Write README for setup, journey, architecture and testing
```

## 11. Task 6: fact-check the claims

Put the CV next to the README and go through it line by line. For each claim, find the evidence. If you cannot point to code or a test, rewrite or remove the claim.

### Claims this project can support

| Claim | Evidence |
|---|---|
| Layered / clean architecture in ASP.NET Core (.NET 10) | Project references: Core ← Application ← Infrastructure ← API |
| RESTful API with consistent responses | Controllers + `ApiResponse<T>` + `ResultExtensions.ToApiResponse()` |
| EF Core with SQL Server, migrations and constraints | `Infrastructure/Migrations`, `Data/Configurations`, `Database_RejectsBookingWithNegativeCargoWeight` |
| JWT authentication with hashed passwords | `AuthService`, `Program.cs` JWT setup, `BookingAuthorizationApiTests` |
| Secrets kept out of source control | User secrets / `JwtSettings__Secret`, startup check in `Program.cs` |
| Transactional booking with optimistic concurrency | `BookingService` transaction, `Trip.RowVersion`, `CreateBookingAsync_ReturnsConflict_WhenAnotherBookingTakesTheLastSeatFirst` |
| Validation and centralized error handling | DataAnnotations on request DTOs, `InvalidModelStateResponseFactory`, `GlobalExceptionHandler` |
| Automated tests: unit, API integration and database integration | `BusBookingSystem.Tests`: service tests, `Api/`, `SqlServer/` (39 tests) |
| API documentation | OpenAPI + Scalar, `EndpointSummary` and `ProducesResponseType` on every action |

### Claims this project cannot support yet

Remove or reword these if they appear anywhere:

- role-based authorization (the token carries a role claim, but no endpoint checks it),
- administration of depots, buses, routes or trips,
- booking cancellation, refunds or payments,
- a front end or Blazor UI,
- deployment, Docker, CI/CD or cloud hosting,
- the repository pattern (deliberately not used),
- load or performance testing,
- booking lookup or history, unless Task 2 was completed.

### Wording matters

"Built a bus booking API" is supported. "Built a bus booking platform" suggests a front end, administration and payments. Use the narrower word.

## 12. Task 7: commit history, push, and merge

### Current state

```text
master            a58e7c2   (pushed)
knowIssuesFix     6 commits ahead of master, not pushed
```

### Squash the commit that does not build

`21094c8` deletes the two exception classes, and `b811702` removes the code that used them. Both have the same message, and `21094c8` does not compile on its own. Combine them before pushing. The branch is not pushed yet, so rewriting it is safe:

```bash
git rebase -i master     # mark b811702 as "fixup" onto 21094c8
```

Or, from a terminal without an interactive editor, rebuild the branch with cherry-picks:

```bash
git switch -c knowIssuesFix-clean 0aeccd5
git cherry-pick --no-commit 21094c8 b811702   # apply both changes as one
git commit -C b811702                          # reuse the original message
git cherry-pick 0197378                        # last commit unchanged
dotnet build BusBookingSystem.slnx && dotnet test BusBookingSystem.slnx
git branch -f knowIssuesFix knowIssuesFix-clean
git switch knowIssuesFix && git branch -D knowIssuesFix-clean
```

The goal is that every commit on `master` builds and passes its tests.

### Push and merge

```bash
git push -u origin knowIssuesFix
```

Open a pull request into `master`, read the diff once more, and merge it. Then branch from `master` for today's slices (seed data, lookup, README) with one commit each.

Do not force-push `master`. The old JWT secret in its history has been rotated, so it no longer grants access.

## 13. If time runs short

Cut in this order, last resort first:

1. **Booking lookup.** Remove it from the claims instead.
2. **The seeder's SQL Server test.** Keep the manual restart check (row 18).
3. **README depth.** Keep Quick start, Customer journey, Endpoints and Limitations; the other sections can be short.

Never cut:

- seed data (without it nothing can be demonstrated),
- the clean-database run-through,
- the claims fact-check.

## 14. Day 9 completion checklist

- [ ] Development seed data is added, idempotent, and runs in Development only.
- [ ] Booking lookup is built with an ownership check, **or** removed from every claim.
- [ ] The booking route in the code matches the docs and README (`/api/booking` or `/api/bookings`).
- [ ] `BusBookingSystem.API.http` runs the full journey, including failure cases.
- [ ] The clean-database run-through table is filled in, and every row matches.
- [ ] `dotnet test` passes.
- [ ] The README covers the quick start, journey, seed data, endpoints, examples, architecture, decisions, testing and limitations.
- [ ] Every CV claim has evidence; unsupported claims are removed.
- [ ] Every commit on `master` builds; one meaningful commit per slice.
- [ ] `knowIssuesFix` and today's work are pushed and merged.
- [ ] Application submitted.

## Summary

Day 9 does not add features. It makes the existing ones provable:

```text
seed data
  -> repeatable journey
  -> clean-database run-through
  -> README that describes what was proven
  -> claims that match the code
  -> clean history on master
```

The first practical step is the seeder. Until a clean database can show depots and trips, nothing else on this list can be verified.
