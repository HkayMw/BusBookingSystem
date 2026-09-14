# Bus Booking System — 9-Day MVP Build Plan
Target: backend REST API only (no Blazor UI), true to what the MRA CV claims.
Window: today → 19 Sept (submit MRA application 20 Sept). ~8hrs/day.

Architecture reminder (yours):
Core ← Application ← Infrastructure ← API   (Web/Blazor is out of scope for this sprint)

Working method: build one customer-visible feature at a time. For each slice, start with a **user story → acceptance criteria → use case → focused DTO/service contract → Core/Infrastructure/API implementation → manual and automated test**. The architecture remains layered, but the work is organized vertically by behavior rather than horizontally by project.

---

## Decisions locked in before Day 1
- Use `User` (with `UserType.Customer`) as the booking owner. Shelve the separate `Customer` entity for now — avoids a redundant join/mapping layer.
- Collapse `Schedule` + `Trip` into a single `Trip` entity: `BusId`, `RouteId`, `DepartureTime`, `ArrivalTime`, `TotalSeats`, `AvailableSeats`, `BaseFare`. This is a deliberate scope simplification — name it if asked in interview.

---

## Day 1 — Domain foundation and first vertical-slice setup
User story: As a developer, I need the core booking concepts so later features have a stable model.

- Review/complete `Depot`, `Bus`, and `Address`.
- Add `Route`, `Trip`, and `Booking` with the agreed MVP fields.
- Add `UserType` and `BookingStatus` enums.
- Confirm the dependency direction: `Core` has no database or API dependency.
- Write the first acceptance notes for depot browsing and trip search.

Done when: Core compiles and the first two user journeys are written down clearly. No database is required yet.

## Day 2 — Depot choices, end to end
User story: As a customer, I want to see available depots so that I can choose an origin and destination for a trip search.

- Define the acceptance criteria and use case before coding.
- Create only the focused `DepotListDto` needed by the search form: ID, code/name, and display location.
- Define `IDepotService.GetActiveDepotsAsync()` around the actual MVP operation.
- Add the minimum EF Core setup needed for this slice: `AppDbContext`, depot mapping, and development seed data.
- Implement the depot service with direct `AppDbContext` queries. Do not add repository wrappers.
- Add `DepotsController` and `GET /api/depots`.
- Manually test the endpoint through Swagger.
- Add a focused test for active depot filtering and the empty-result case.

Done when: a real request returns seeded depot choices that could populate the trip-search form.

## Day 3 — Search trips, end to end
User story: As a customer, I want to search by origin, destination, and date so that I can choose a trip.

- Define validation and acceptance criteria: required origin/destination/date, different depots, matching route, matching travel date, and empty results.
- Create `TripSearchResultDto` from the search screen backward. Include IDs needed for selection or later booking and names/times/fare/availability needed for display.
- Define `ITripService.SearchTripsAsync(...)`.
- Add the required bus, route, and trip mappings and seed data.
- Implement the search with direct `AppDbContext` querying and explicit projection into the focused DTO.
- Add `GET /api/trips/search?originDepotId=&destinationDepotId=&date=`.
- Manually test matching, invalid, and empty searches.
- Add focused tests for filtering and projection.

Done when: Swagger returns a selectable trip result with no entity graph or internal audit fields exposed.

## Day 4 — Selected-trip details and read-side completion
User story: As a customer, I want to view the trip I selected so that I can confirm it before booking.

- Decide whether `TripDto` adds information that the search result does not. If not, reuse one response shape rather than duplicating DTOs.
- Define `ITripService.GetByIdAsync(Guid id)`.
- Add `GET /api/trips/{id}` with a clear not-found response.
- Keep `BusDto` and a standalone route service out unless a real MVP screen needs them.
- Register the services and DbContext in DI.
- Test the complete read journey: depots → search → selected trip.

Done when: the entire public read-side journey works through Swagger and has focused tests.

## Day 5 — Create a booking, end to end
User story: As a customer, I want to book seats on a selected trip so that my seats are reserved.

- Define `CreateBookingRequest`, `BookingDto`, and acceptance criteria before implementation.
- Define `IBookingService.CreateBooking(...)` and any read method required for confirmation.
- Validate the trip ID, seat count, and user context.
- Use `AppDbContext` directly in the Infrastructure service; do not introduce `IBookingRepository`.
- Check availability, decrement seats, create the booking, generate a reference, and save inside a transaction.
- Use the `RowVersion` concurrency token to protect against racing updates.
- Add `POST /api/bookings` and the booking lookup endpoint if required by the confirmation flow.
- Test success, trip-not-found, invalid seat count, and overbooking.

Done when: a selected trip can produce a booking and availability changes correctly. **This is the highest-value day.**

## Day 6 — Authentication applied to the booking journey
User story: As a customer, I want to register and log in so that my booking belongs to me.

- Define register/login acceptance criteria and focused request/response DTOs.
- Add password hashing and JWT issuing with the lightweight approach already chosen.
- Add `/register` and `/login`.
- Protect booking creation and booking lookup with `[Authorize]`.
- Keep depot and trip browsing public.
- Test register → login → authenticated booking, plus rejected unauthenticated access.

Done when: the customer can complete the booking journey with a Bearer token.

## Day 7 — Failure behaviour and product polish
User story: As an API consumer, I want consistent validation and error responses so that failures are understandable.

- Add DataAnnotations to request DTOs where useful.
- Add centralized exception handling with consistent `ProblemDetails` responses.
- Verify not-found, validation, unauthorized, and overbooking responses across the existing slices.
- Add concise Swagger summaries and remove scaffold leftovers.
- Re-run the core user journeys after the changes.

Done when: expected failures are predictable and the API reads like a finished MVP rather than a scaffold.

## Day 8 — Automated tests for the vertical slices
User story: As a developer, I want tests around the important user journeys so that changes do not silently break booking.

- Add an xUnit test project and the chosen database-testing approach.
- Cover depot selection, trip search filtering, trip-not-found, trip detail, booking success, and overbooking rejection.
- Prefer tests that exercise real service behaviour and database queries over tests that only verify mocks were called.
- Run the complete test suite and fix only failures related to the MVP.

Done when: the important customer journey has repeatable automated coverage.

## Day 9 — Buffer, documentation, and release fact-check
User story: As a reviewer, I want to run the project and understand the implemented journey so that the work is credible and checkable.

- Use the day as buffer for the highest-risk incomplete slice, especially booking or authentication.
- Write the README around user journeys, architecture, setup, seed data, and endpoint examples.
- Walk through every journey in Swagger from a clean database.
- Push a real commit history with one meaningful commit per completed slice.
- Compare every CV claim with working behaviour and remove anything not demonstrated.
- Submit the MRA application.

Done when: a fresh setup can run the API, follow the documented customer journey, and verify the claims.

---

## If time runs short, cut in this order (last resort first):
1. Test coverage depth (keep at least the booking happy-path + overbooking test)
2. Role richness in JWT (keep basic auth, drop admin/customer role split)
3. Error-handling polish
Never cut Day 5 (booking creation) — it's the feature the whole CV story depends on.
