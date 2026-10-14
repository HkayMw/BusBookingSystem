# Progress Tracker

Last checked: 14 Oct 2026 (against `master` @ `c6a1759`, PR #1 merged).
Update this file at the end of every session. Detailed steps live in the `day N walk through.md` files.

## Where you are

**Days 1–8: done. Day 9: git cleanup done; seed data next.**

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
| 9 | Release fact-check | **In progress** (1 of 8) | See checklist below |

## Day 9 checklist (from `day 9 walk through.md`, in order)

- [ ] 1. `DevelopmentDataSeeder` in `Infrastructure/Data/` (no seeder exists yet — **this blocks everything else**)
- [ ] 2. Optional: `GET /api/booking/{bookingId}` with an ownership check (otherwise drop it from all claims)
- [ ] 3. Full journey in `BusBookingSystem.API.http` (currently only 2 requests)
- [ ] 4. Clean-database run-through (19-row table, section 9)
- [ ] 5. README (currently one line)
- [ ] 6. CV claims fact-check
- [x] 7. Git cleanup: squashed, pushed, merged as PR #1 (`c6a1759`)
- [ ] 8. Decide `/api/booking` vs `/api/bookings` and make the docs match

## Git state

- `master` = `origin/master` = `c6a1759` (PR #1 merged `knowIssuesFix`).
- Feature branches deleted. Start each Day 9 task on a new branch from `master`.
- Review every PR with `docs/PR_REVIEW_CHECKLIST.md`.

## Known doc drift (code wins)

- The handoff doc says "skip AutoMapper"; `AuthService` uses it for `User` mapping.
- The plan says Swagger + `ProblemDetails`; the code uses Scalar + the `ApiResponse<T>` envelope.
- The Day 5/6 docs say `/api/bookings`; the controller route is `/api/booking`.

## Next session starts here

Day 9, Task 1: write `DevelopmentDataSeeder` (section 6 of the Day 9 walkthrough).
