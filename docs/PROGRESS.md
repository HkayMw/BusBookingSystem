# Progress Tracker

Last checked: 19 Oct 2026 (against `origin/master` @ `0b47f8e`, `HttpJourney` merged).
Update this file at the end of every session. Detailed steps live in the `day N walk through.md` files.

## Where you are

**Days 1–8: done. Day 9: seed data, booking lookup, `.http` journey, clean-database run-through and git cleanup done; the README is next.**

| Day | Slice | Status | Evidence |
|---|---|---|---|
| 1 | Domain model | Done | `Core/Entities`, `Core/Enums` |
| 2 | Depot list | Done | `GET /api/depots`, `DepotServiceTests` |
| 3 | Trip search | Done | `GET /api/trips/search`, `TripServiceTests` |
| 4 | Trip details | Done | `GET /api/trips/{tripId}` |
| 5 | Create booking | Done | `POST /api/booking`, transaction + `RowVersion` |
| 6 | Auth (JWT) | Done | `/api/auth/register`, `/api/auth/login`, `[Authorize]` on booking |
| 7 | Errors and polish | Done (no walkthrough file) | `ApiResponse<T>`, `GlobalExceptionHandler`, DataAnnotations, Scalar |
| 8 | Automated tests | Done | Service, `Api/` and `SqlServer/` suites (39 tests per Day 9 doc) |
| 9 | Release fact-check | **In progress** (5 of 8) | See checklist below |

## Day 9 checklist (from `day 9 walk through.md`, in order)

- [x] 1. `DevelopmentDataSeeder` — merged in PR #2 (`710a2a0`, `4c8f50a`). 5 depots (Jenda inactive), 4 buses (incl. a no-cargo minibus), 6 routes, 5 trips incl. a fractional fare and a no-cargo-fare trip.
- [x] 2. `GET /api/booking/{bookingId}` — merged in PR #3 (`988aca3`). Owner-only (404 otherwise); fares stored on `Booking` (migration `AddBookingFares`); new bookings start `Pending`; a missing user returns 401. Service tests + API tests (401, unknown booking 404, owner 200 / other customer 404).
- [x] 3. Full journey in `BusBookingSystem.API.http` (since moved to `BusBookingSystem.API/HttpRequests/CustomerJourney.http`) — merged from `HttpJourney` (`ee6cc16`, `4220afb`, merge `0b47f8e`). Part A happy path (A1–A14) and Part B edge/failure paths (B1–B15), each with its expected status and message. `GetActiveDepotsAsync` orders by name so depot indexes are stable; `GetAllTripsAsync` message is "All trips retrieved successfully.". Spotted for later: typos in API messages ("acount" in the register conflict, "successfull" in the login success).
- [x] 4. Clean-database run-through — all 31 rows passed on 19 Oct (A1–A14, B1–B15, restart, `dotnet test`). Table in [`docs/docs/CLEAN_DATABASE_RUN_THROUGH.md`](docs/CLEAN_DATABASE_RUN_THROUGH.md); its Actual column is left blank on purpose as a reusable template.
- [ ] 5. README — **drafted 19 Oct** on branch `Readme` (uncommitted): status, journey, quick start, seed data, endpoints, response envelope with examples, architecture (incl. the template-only Blazor project), design decisions, testing, limitations, links to `docs/docs/`. Left: read it through, run the quick-start commands once as written, commit, PR. Spotted: `bookingStatus` serializes as a number (`0` = Pending) because no `JsonStringEnumConverter` is registered; the README documents it as-is.
- [ ] 6. CV claims fact-check
- [x] 7. Git cleanup: squashed, pushed, merged as PR #1 (`c6a1759`)
- [ ] 8. Decide `/api/booking` vs `/api/bookings` and make the docs match

## Git state

- `origin/master` = `0b47f8e` (PR `HttpJourney` merged with a merge commit; PRs #2 `DevelopmentDataSeeder` and #3 `BookingLookup` before it). Local `master` still at `f76e7f9` until you pull.
- Feature branches deleted locally and on GitHub. Start each task on a new branch from `master`.
- Review every PR with `docs/PR_REVIEW_CHECKLIST.md`. For stacked PRs, merge the lower one first and use merge commits, not squash.
- Commit messages describe the code and behaviour change only. No day or task numbers: those belong to the learning plan in `docs/`, not to the project history.

## Future work (not Day 9)

- Move the "user still exists" check into `JwtBearerEvents.OnTokenValidated` so every `[Authorize]` endpoint gets it once, then drop the per-method checks.
- A confirmation step that moves bookings from `Pending` to `Confirmed`.
- Admin endpoints for depots, buses, routes and trips.
- Serialize enums as strings (`JsonStringEnumConverter`) so `bookingStatus` reads `"Pending"` instead of `0`; update the README example when done.

## Docs layout

- `docs/` — development reference: plans, day walkthroughs, PROGRESS, PR checklist.
- `docs/docs/` — project documentation published with the system and linked from the README. New user-facing docs go here by default.

## Known doc drift (code wins)

- The handoff doc says "skip AutoMapper"; `AuthService` uses it for `User` mapping.
- The plan says Swagger + `ProblemDetails`; the code uses Scalar + the `ApiResponse<T>` envelope.
- The Day 5/6 docs say `/api/bookings`; the controller route is `/api/booking`.

## Next session starts here

Day 9, Task 5: review the drafted `README.md` on branch `Readme`, run its quick-start commands as written, commit and open the PR. Then Task 6: the CV claims fact-check (section 11 of the Day 9 walkthrough), using the README as the evidence list.
