# Progress Tracker

Last checked: 14 Oct 2026 (against branch `knowIssuesFix` @ `0197378`).
Update this file at the end of every session. Detailed steps live in the `day N walk through.md` files.

## Where you are

**Days 1–8: done. Day 9: walkthrough written, no tasks started.**

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
| 9 | Release fact-check | **Not started** | See checklist below |

## Day 9 checklist (from `day 9 walk through.md`, in order)

- [ ] 1. `DevelopmentDataSeeder` in `Infrastructure/Data/` (no seeder exists yet — **this blocks everything else**)
- [ ] 2. Optional: `GET /api/booking/{bookingId}` with an ownership check (otherwise drop it from all claims)
- [ ] 3. Full journey in `BusBookingSystem.API.http` (currently only 2 requests)
- [ ] 4. Clean-database run-through (19-row table, section 9)
- [ ] 5. README (currently one line)
- [ ] 6. CV claims fact-check
- [ ] 7. Git cleanup: squash `21094c8` into `b811702`, push `knowIssuesFix`, merge into `master`
- [ ] 8. Decide `/api/booking` vs `/api/bookings` and make the docs match

## Git state

- Current branch `knowIssuesFix`: 5 commits ahead of `master`, **not pushed**.
- `master` = `origin/master` = `a58e7c2`.
- `docs/day 9 walk through.md` is untracked (not committed yet).

## Known doc drift (code wins)

- The handoff doc says "skip AutoMapper"; `AuthService` uses it for `User` mapping.
- The plan says Swagger + `ProblemDetails`; the code uses Scalar + the `ApiResponse<T>` envelope.
- The Day 5/6 docs say `/api/bookings`; the controller route is `/api/booking`.

## Next session starts here

Day 9, Task 1: write `DevelopmentDataSeeder` (section 6 of the Day 9 walkthrough).
