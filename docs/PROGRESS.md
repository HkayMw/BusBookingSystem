# Progress Tracker

Last checked: 24 Oct 2026 (against `origin/master` @ `3038058`, enums-as-strings merged as PR #7).
Update this file at the end of every session. Detailed steps live in the `day N walk through.md` files.

## Where you are

**Days 1–8: done. Day 9: all 8 tasks done (README merged; booking routes renamed to `/api/bookings`, PR #6 merged). Enums stored and returned as strings (PR #7 merged). Claims fact-check done (24 Oct). Left: tick the completion checklist.**

| Day | Slice | Status | Evidence |
|---|---|---|---|
| 1 | Domain model | Done | `Core/Entities`, `Core/Enums` |
| 2 | Depot list | Done | `GET /api/depots`, `DepotServiceTests` |
| 3 | Trip search | Done | `GET /api/trips/search`, `TripServiceTests` |
| 4 | Trip details | Done | `GET /api/trips/{tripId}` |
| 5 | Create booking | Done | `POST /api/bookings`, transaction + `RowVersion` |
| 6 | Auth (JWT) | Done | `/api/auth/register`, `/api/auth/login`, `[Authorize]` on booking |
| 7 | Errors and polish | Done (no walkthrough file) | `ApiResponse<T>`, `GlobalExceptionHandler`, DataAnnotations, Scalar |
| 8 | Automated tests | Done | Service, `Api/` and `SqlServer/` suites (39 tests per Day 9 doc) |
| 9 | Release fact-check | **In progress** (8 of 8; checklist to tick) | See checklist below |

## Day 9 checklist (from `day 9 walk through.md`, in order)

- [x] 1. `DevelopmentDataSeeder` — merged in PR #2 (`710a2a0`, `4c8f50a`). 5 depots (Jenda inactive), 4 buses (incl. a no-cargo minibus), 6 routes, 5 trips incl. a fractional fare and a no-cargo-fare trip.
- [x] 2. `GET /api/bookings/{bookingId}` — merged in PR #3 (`988aca3`). Owner-only (404 otherwise); fares stored on `Booking` (migration `AddBookingFares`); new bookings start `Pending`; a missing user returns 401. Service tests + API tests (401, unknown booking 404, owner 200 / other customer 404).
- [x] 3. Full journey in `BusBookingSystem.API.http` (since moved to `BusBookingSystem.API/HttpRequests/CustomerJourney.http`) — merged from `HttpJourney` (`ee6cc16`, `4220afb`, merge `0b47f8e`). Part A happy path (A1–A14) and Part B edge/failure paths (B1–B15), each with its expected status and message. `GetActiveDepotsAsync` orders by name so depot indexes are stable; `GetAllTripsAsync` message is "All trips retrieved successfully.". Spotted for later: typos in API messages ("acount" in the register conflict, "successfull" in the login success).
- [x] 4. Clean-database run-through — all 31 rows passed on 19 Oct (A1–A14, B1–B15, restart, `dotnet test`), and again on 24 Oct after the `/api/bookings` rename and enums-as-strings (50 tests). Table in [`docs/docs/CustomerJourney.md`](docs/CustomerJourney.md); its Actual column is left blank on purpose as a reusable template.
- [x] 5. README — merged in PR #5 (`fe75db4`, merge `a7e7171`): status, journey, quick start, seed data, endpoints, response envelope with examples, architecture (incl. the template-only Blazor project), design decisions, testing, limitations, links to `docs/docs/`. `bookingStatus` used to serialize as a number; since the enums-as-strings change (20 Oct) the README example shows `"Pending"`.
- [x] 6. Claims fact-check — done 24 Oct. Every description of the project was checked against the code. Corrections: this project documents its API with OpenAPI + Scalar (no Swagger); it issues a role claim but enforces no roles, so role-based authorization is not claimed for it; tests are described as service, API and SQL Server suites (50 tests); the project link is included wherever it is described.
- [x] 7. Git cleanup: squashed, pushed, merged as PR #1 (`c6a1759`)
- [x] 8. Booking route: **plural, `/api/bookings`** (decided 20 Oct). `BookingController` renamed to `BookingsController`, so `[controller]` gives the plural like `DepotsController` and `TripsController`. Tests (one `BookingUrl` constant), `CustomerJourney.http`, README and `docs/docs/CustomerJourney.md` updated. The old singular route is removed, not aliased (no external clients yet). Merged in PR #6 (`39db60b`, merge `44b505c`). The Day 9 walkthrough still says singular on purpose: it records the state at the time.

## Git state

- `origin/master` = local `master` = `3038058` (PR #7 `EnumsAsStrings`; before it #6 `BookingsRoute`, #5 `Readme`, `HttpJourney`, #3 `BookingLookup`, #2 `DevelopmentDataSeeder`).
- `EnumsAsStrings` merged as PR #7 (`3038058`); `BookingsRoute` and `EnumsAsStrings` deleted locally and on GitHub. Start the next task on a new branch from `master`.
- Markdown tables are padded by the editor's formatter. Decision 20 Oct: keep that formatting.
- Feature branches deleted locally and on GitHub. Start each task on a new branch from `master`.
- Review every PR with `docs/PR_REVIEW_CHECKLIST.md`. For stacked PRs, merge the lower one first and use merge commits, not squash.
- Commit messages describe the code and behaviour change only. No day or task numbers: those belong to the learning plan in `docs/`, not to the project history.

## Future work (not Day 9)

- Move the "user still exists" check into `JwtBearerEvents.OnTokenValidated` so every `[Authorize]` endpoint gets it once, then drop the per-method checks.
- A confirmation step that moves bookings from `Pending` to `Confirmed`.
- Admin endpoints for depots, buses, routes and trips.

## Enums as strings (20 Oct, merged as PR #7)

Plan and outcome in [`ENUMS_AS_STRINGS_PLAN.md`](ENUMS_AS_STRINGS_PLAN.md). `BookingStatus`, `BusType` and `BusStatus` are `nvarchar(50)` holding the name (like `UserType`); migration `StoreEnumsAsStrings` converts existing rows; `CK_Booking_ValidBookingStatus` restricts booking status values (migration `StoreEnumsAsStringsFixExisting`, name kept). JSON returns names (`JsonStringEnumConverter` in both `ConfigureHttpJsonOptions` and `AddJsonOptions`). `TripDto.BusType` is now the enum. Lesson: string enums change the API contract; any client (tests, the future Blazor app) needs `JsonStringEnumConverter` to read them. 50 tests pass.

Rule from now on: **enum member names are part of the contract.** Renaming one needs a data migration (and an update to the check constraint for `BookingStatus`).

## Docs layout

- `docs/` — development reference: plans, day walkthroughs, PROGRESS, PR checklist.
- `docs/docs/` — project documentation published with the system and linked from the README. New user-facing docs go here by default.

## Known doc drift (code wins)

- The handoff doc says "skip AutoMapper"; `AuthService` uses it for `User` mapping.
- The plan says Swagger + `ProblemDetails`; the code uses Scalar + the `ApiResponse<T>` envelope.
- README links to `docs/docs/CLEAN_DATABASE_RUN_THROUGH.md` (lines 19, 224, 241), but the file is `docs/docs/CustomerJourney.md`. Fixed in `ad4250a` on `EnumsAsStrings`; still broken on `master` until that branch merges.
- The Day 9 walkthrough says `/api/booking` (singular) and `CLEAN_DATABASE_RUN_THROUGH.md`. Left as a record of that day; the code and README are current.

## Next session starts here

1. Tick the completion checklist (section 14 of the Day 9 walkthrough) and mark Day 9 done here.
2. Pick the first post-Day 9 slice: `OnTokenValidated` user check (recommended), booking confirmation, or admin endpoints.
