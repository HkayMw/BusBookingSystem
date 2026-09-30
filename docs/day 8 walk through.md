# Day 8 Walkthrough - Testing the Vertical Slices (Beginner Friendly)

This is the day where we stop just running the app and start proving that the important behavior still works when the code changes later.

If you are new to testing, think of it like this:

- the app is a machine,
- the user follows a journey like "search trip -> select trip -> book ticket",
- the test is a checklist that says, "when this input is given, the result must be this".

We are not trying to test every line of code. We are testing the important customer journeys and the business rules that matter most.

```text
real business behavior
  -> real service logic
  -> real EF Core queries
  -> clear assertions
  -> repeatable confidence
```

This project already follows a simple layered design:

```text
Core <- Application <- Infrastructure <- API
```

That means the tests should focus on the service behavior rather than mocked fake classes invented just for the tests.

---

## 1. What Day 8 is trying to protect

The important idea is: Day 8 protects the features you already built.

The test suite should check the user journeys that matter most:

- active depots are returned and inactive ones are excluded,
- trip search filters correctly by origin, destination, and date,
- missing trips return a not-found result,
- trip details return the selected trip,
- a valid booking reserves seats,
- overbooking is rejected,
- authentication still behaves correctly for login and registration.

This is not a test of every property or every method. It is a test of the behavior that users care about.

A good test asks a simple question:

> If I pass this data into this service, what should happen?

That is the mindset we use in this project.

---

## 2. The testing workflow we follow

For this project, the usual pattern is:

1. create the objects needed for the scenario,
2. put them in a fresh in-memory database,
3. call the real service method,
4. check the result,
5. repeat for different cases.

That pattern is usually written as:

```text
Arrange -> Act -> Assert
```

### Arrange

Create the real data that the service needs.

### Act

Call the real service method.

### Assert

Check the result is correct.

If you remember only one thing from testing, remember this: test real behavior, not fake behavior.

---

## 3. What this project is actually doing

This application is not using a repository pattern for the MVP. The service layer talks directly to `AppDbContext` in Infrastructure.

So our testing approach should match the real design:

- create a real `AppDbContext`,
- create a real service instance,
- give it real test data,
- call the real method,
- assert the returned `Result<T>` or DTO.

That is much better than writing tests that only assert a mock was called.

---

## 4. What kind of test project we need

For this project, the simplest beginner-friendly setup is:

- xUnit as the test framework,
- EF Core InMemory provider for service-level tests,
- a new project such as `BusBookingSystem.Tests`.

The test project should reference the project(s) it is testing, such as:

- `BusBookingSystem.Application`
- `BusBookingSystem.Infrastructure`
- `BusBookingSystem.Core`

The important rule is:

- production code does not reference the test project,
- the test project references the application code.

Then we can run:

```bash
dotnet test "BusBookingSystem.slnx" --nologo
```

That command runs the full test suite and reports failures clearly.

---

## 5. Why we use a fresh database for each test

This is a very important beginner concept.

If one test adds data to a shared database, another test may accidentally see that data and fail for the wrong reason.

The safe pattern is:

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;

await using var context = new AppDbContext(options);
```

The key idea is this:

- `Guid.NewGuid().ToString()` creates a unique database name per test.

So each test runs in isolation.

That keeps tests clean and predictable.

---

## 6. First test: active depot filtering

This is the best first test because it is small and easy to understand.

### Scenario

- create one active depot,
- create one inactive depot,
- call the depot service,
- assert that only the active one is returned.

### Why this matters

It proves the service is filtering correctly and that inactive records are not exposed to customers.

### Pattern

```text
Arrange:
  create active depot
  create inactive depot
  create AppDbContext with unique database name
  create DepotService

Act:
  call GetActiveDepotsAsync()

Assert:
  result is successful
  count is 1
  returned depot is the active one
```

This is an easy first lesson in real service testing.

---

## 7. Second test: trip search filtering

Trip search is a little more complex, but still very valuable.

### Scenario

Create:

- origin depot,
- destination depot,
- a route that matches,
- a trip that matches the date,
- one or two extra trips that should not match.

Then call the real trip search service.

### Assertions

- matching trip shows up,
- wrong origin does not show up,
- wrong destination does not show up,
- wrong date does not show up,
- returned data includes the expected trip information.

This protects the actual user journey and not just a random method call.

---

## 8. Trip detail test: selected trip

Once the list of trips works, we should also test the selected-trip detail flow.

### Scenario

- create a valid trip,
- call `GetByIdAsync` (or the equivalent service method),
- assert the returned DTO contains the right trip data.

### Negative case

Also add the not-found case:

- call the service with a `Guid` that does not exist,
- assert the result is unsuccessful,
- assert the status is not found.

This protects both the happy path and the edge case.

---

## 9. Booking success test: the highest-value test

This is the most important test in the project because booking is the main business action.

### Scenario

Create:

- a user,
- a trip,
- a bus with known seat capacity,
- a route,
- a valid booking request.

Then call the booking service.

### Assertions

- booking succeeds,
- a booking is created,
- seat count is reduced correctly,
- returned booking data matches the request,
- trip availability reflects the new reservation.

This proves the core rule:

> a valid reservation creates a booking and updates availability.

---

## 10. Overbooking test: reject invalid demand

This is the second most important booking test.

### Scenario

Set up a trip where the available seats are already low or exhausted.

Then try to book more seats than remain.

### Assertions

- the booking fails,
- the service returns a meaningful failure result,
- the trip remains unchanged,
- no invalid booking is persisted.

This protects the central business rule: seats cannot be sold beyond capacity.

---

## 11. What not to do in Day 8

This is where beginner mistakes usually happen.

### Do not mock the whole system

Avoid tests that only assert a mock `DbContext` or service method was called.

That proves the mock exists, not that the app works.

### Do not share database state across tests

Each test should create its own isolated data set.

### Do not test private methods

Test the public service behavior instead. Private methods are implementation details.

### Do not test every field in the project

Test the critical behavior only: booking logic, filtering, not found, invalid input.

---

## 12. Why this is enough for an MVP

This project is a learning and MVP codebase, not a huge enterprise system. The goal is not to build a massive test suite.

The goal is to catch the important regressions:

- search behaves correctly,
- details work,
- booking respects capacity,
- invalid or missing requests fail correctly.

That gives you confidence without over-engineering the testing setup.

---

## 13. How to think when writing a test

Use this pattern:

```text
Given a certain scenario,
When the service is called with this input,
Then the result should be this outcome.
```

Example:

```text
Given a trip with 5 available seats,
When a customer books 3 seats,
Then the booking succeeds and remaining seats become 2.
```

This is a very natural and practical way to write tests.

---

## 14. Authentication tests

Authentication-related tests are also useful, but keep them focused.

Good cases include:

- registration creates a user,
- duplicate email registration is rejected,
- valid credentials produce a token,
- unknown email returns unauthorized,
- wrong password returns unauthorized.

JWT middleware behavior is better tested at the API/integration level because it depends on ASP.NET Core authentication pipeline setup.

---

## 15. Naming tests clearly

Use names that describe business behavior instead of implementation details.

Examples:

```text
GetActiveDepotsAsync_ExcludesInactiveDepots
SearchTripAsync_ReturnsTripsMatchingRouteAndDate
GetTripByIdAsync_ReturnsNotFoundForUnknownTrip
CreateBookingAsync_RejectsOverbooking
CreateBookingAsync_UpdatesBookedSeats
LoginAsync_ReturnsUnauthorizedForUnknownEmail
```

This makes a failing test easy to understand.

---

## 16. What success looks like at the end of Day 8

By the end of Day 8, the project should have:

- a test project,
- real service-level tests,
- isolated test data for each scenario,
- coverage of the critical journeys,
- a passing suite with no accidental cross-test pollution.

This is the goal: not excessive tests, but confidence in the most important rules.

---

## 17. Simple beginner takeaway

If you are new to testing, do not think of it as "writing code for the sake of code".

Think of it as:

- writing a specification,
- turning that specification into a check,
- running that check automatically.

That is what Day 8 is doing for this Bus Booking project.

The long-term value is that when someone later changes the trip search or booking logic, the test suite tells us immediately if a real rule was broken.

That is why this day matters.

---

## 18. The final mental model

When you are unsure what to test, ask:

- what user journey matters most here?
- what business rule could quietly be broken later?
- what would I want to know immediately if this feature regressed?

For this project, the answers are mostly:

- search,
- detail lookup,
- booking,
- capacity protection.

That is the right Day 8 focus.

Run the focused test project while developing:

```bash
dotnet test "BusBookingSystem.Tests/BusBookingSystem.Tests.csproj" --nologo
```

Run the complete solution suite before calling Day 8 complete:

```bash
dotnet test "BusBookingSystem.slnx" --nologo
```

A passing build is not a passing test suite. Verify that the test runner discovers and executes the tests.

The useful result should show:

```text
Passed: N
Failed: 0
Skipped: 0
```

If no tests are discovered, fix test project configuration before adding more test code.

## 15. Day 8 completion checklist

- [ ] An xUnit test project exists.
- [ ] The test project targets the same .NET version as the application.
- [ ] The test project is included in the solution.
- [ ] EF Core test setup creates isolated database state per test.
- [ ] Active depot filtering is covered.
- [ ] Empty depot results are covered.
- [ ] Trip search route filtering is covered.
- [ ] Trip search date filtering is covered.
- [ ] Trip-not-found behavior is covered.
- [ ] Trip detail projection is covered.
- [ ] Booking success is covered.
- [ ] Booking persistence and seat updates are covered.
- [ ] Overbooking rejection is covered.
- [ ] Authentication service behavior is covered where practical.
- [ ] Tests use real service and database behavior.
- [ ] The focused test project passes.
- [ ] The complete solution test command passes.

## 16. Summary

Day 8 turns the customer journey into repeatable evidence:

```text
active depots
  -> matching trip search
  -> selected trip details
  -> successful booking
  -> protected inventory rules
```

The project does not need a large testing framework or exhaustive coverage. It needs a small set of trustworthy tests around the behavior that matters most.

The recommended implementation order is:

1. create the test project,
2. test active depot filtering,
3. test trip search,
4. test trip details,
5. test booking success,
6. test overbooking,
7. add authentication service tests,
8. run the complete solution test suite.

# Day 8 Walkthrough - Automated Tests for the Vertical Slices

Day 8 adds repeatable automated coverage around the customer journey built during the previous days.

The goal is not to test every line of code. The goal is to protect the important behavior so future changes do not silently break depot selection, trip search, trip details, booking, or authentication boundaries.

```text
real service behavior
  -> real EF Core query
  -> controlled test database
  -> assert business result
  -> repeat automatically
```

The project continues to use the same lightweight architecture:

```text
Core <- Application <- Infrastructure <- API
```

The testing strategy should respect that architecture instead of replacing real behavior with large collections of mocks.

## 1. User story

> As a developer, I want tests around the important user journeys so that changes do not silently break booking.

Day 8 is complete when the main customer journey has repeatable tests that can run from the solution without manually opening Swagger or Scalar.

## 2. Current project state

The current solution contains the application projects:

- `BusBookingSystem.Core`
- `BusBookingSystem.Application`
- `BusBookingSystem.Infrastructure`
- `BusBookingSystem` API project
- `BusBookingSystem.Web`

The important service slices already exist:

- `DepotService`
- `TripService`
- `BookingService`
- `AuthService`

There is currently no test project. Day 8 therefore begins by creating a dedicated xUnit test project and referencing only the projects required by the tests.

The first tests should target services rather than controllers because the service layer owns the database queries and business rules.

## 3. Scope for this day

Build focused tests for:

- active depot filtering,
- trip search filtering,
- trip detail lookup,
- missing trip behavior,
- successful booking,
- overbooking rejection,
- authentication behavior where practical.

Do not build these features today:

- a complete end-to-end production deployment test suite,
- browser tests for the Blazor project,
- payment tests,
- performance testing,
- concurrency load testing,
- a large mocking framework setup for every dependency.

Those belong to later work or require a real environment decision.

## 4. Acceptance criteria

Day 8 is complete when:

- a test project exists in the solution,
- the test project runs with `dotnet test`,
- tests use a controlled database provider,
- active depot filtering is covered,
- trip search returns only matching trips,
- missing trip details are handled correctly,
- a valid booking updates the expected availability state,
- overbooking is rejected,
- the test suite can run repeatedly without depending on leftover database rows,
- failures identify the behavior that broke.

## 5. Choose the test boundary

There are three useful testing levels for this project.

### Service tests

These call services directly:

```text
Test -> DepotService/TripService/BookingService -> AppDbContext -> test database
```

These are the best first tests because they exercise the actual LINQ queries and business rules without requiring the API server to run.

### API integration tests

These send HTTP requests through the ASP.NET Core application:

```text
Test -> HTTP endpoint -> authentication/middleware/controller/service -> test database
```

These are valuable for authentication and response status behavior, but they require more setup. Add them after the service tests are stable.

### Unit tests with mocks

These replace the database or services with mocks. They can be useful for small isolated decisions, but they should not be the main strategy for this project because the important behavior is in EF Core queries and availability updates.

The recommended order is:

```text
service behavior tests first
  -> API integration tests for authentication boundaries
  -> broader coverage only when a real risk justifies it
```

## 6. Test project structure

A possible structure is:

```text
BusBookingSystem.Tests/
  BusBookingSystem.Tests.csproj
  Services/
    DepotServiceTests.cs
    TripServiceTests.cs
    BookingServiceTests.cs
    AuthServiceTests.cs
  TestData/
    TestDbContextFactory.cs
    TestDataFactory.cs
```

The exact folder names can follow the project style. Keep the test project separate from production projects.

The test project should reference:

- `BusBookingSystem.Application`
- `BusBookingSystem.Infrastructure`
- `BusBookingSystem.Core` indirectly or directly when test data needs entities

## 7. Choose the database approach

The tests need a controlled database that starts in a known state.

For the first service tests, an EF Core in-memory database is simple and fast. Each test should receive a unique database name so tests do not share tracked entities or rows accidentally.

Conceptually:

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;
```

Then:

```text
create context
  -> arrange entities
  -> save seed data
  -> create service
  -> execute method
  -> assert result
```

### Important provider limitation

The in-memory provider is useful for service behavior, but it is not SQL Server. It may not reproduce:

- SQL Server constraint behavior,
- relational transaction behavior,
- query translation differences,
- row-version concurrency behavior.

For tests involving migrations, SQL constraints, or concurrency, use a relational test database strategy later. Do not claim that an in-memory test proves SQL Server behavior.

## 8. Arrange, act, assert

Each test should have three readable parts:

```text
Arrange -> create the test state
Act     -> call one operation
Assert  -> verify the observable result
```

Example shape:

```csharp
[Fact]
public async Task GetActiveDepotsAsync_returns_only_active_depots()
{
    // Arrange
    // create active and inactive depots

    // Act
    // call the service

    // Assert
    // verify only the active depot is returned
}
```

Keep one primary behavior per test. A test can have several assertions about the same result, but it should not quietly test unrelated features.

## 9. Depot service tests

Start with the smallest read-side slice.

### Test: active depots are returned

Arrange:

- one active depot,
- one inactive depot,
- both saved to the test database.

Act:

```text
IDepotService.GetActiveDepotsAsync()
```

Assert:

- the result is successful,
- exactly one depot is returned,
- the active depot is present,
- the inactive depot is absent.

### Test: no active depots returns an empty list

Arrange:

- no active depots.

Act:

```text
GetActiveDepotsAsync()
```

Assert:

- the operation succeeds,
- the returned collection is empty,
- no exception is required for a normal empty result.

These tests protect the customer journey's first step: choosing an origin and destination.

## 10. Trip service tests

Trip tests should verify filtering and projection rather than only checking that a method returned something.

### Test: search filters by route and date

Arrange:

- an origin depot,
- a destination depot,
- a matching trip on the requested date,
- a trip on a different date,
- a trip using a different route.

Act:

```text
ITripService.SearchTripAsync(originId, destinationId, date)
```

Assert:

- only the matching trip is returned,
- the result contains the expected trip ID,
- departure and arrival values are projected,
- available seats are calculated correctly.

### Test: search with no match returns an empty list

Arrange:

- trips that do not match the requested route or date.

Assert:

- the operation returns success,
- the collection is empty.

### Test: invalid search input is rejected

Test cases should include:

- empty origin ID,
- empty destination ID,
- equal origin and destination,
- missing date.

Assert that the result is a bad-request outcome and contains a useful message.

### Test: missing trip details

Act:

```text
ITripService.GetTripByIdAsync(nonExistentTripId)
```

Assert the chosen contract clearly. The current project uses `Result<T>`, so the expected behavior should be a not-found result rather than an unhandled exception.

## 11. Booking service tests

Booking tests protect the highest-value business behavior.

### Test: booking succeeds when capacity exists

Arrange:

- a user,
- a bus with known seat capacity,
- a route,
- a trip with known fares and zero booked seats,
- a booking request within capacity.

Act:

```text
IBookingService.CreateBookingAsync(userId, request)
```

Assert:

- the result is successful,
- a booking ID exists,
- a booking reference exists,
- the requested number of seats is stored,
- the trip's `SeatsBooked` value increases,
- the booking row exists in the test database.

### Test: overbooking is rejected

Arrange:

- a bus with a small capacity,
- a trip whose booked seats leave fewer seats than requested.

Act:

```text
CreateBookingAsync(userId, request)
```

Assert:

- the result is a conflict,
- no booking row is created,
- the trip's booked seat count does not increase.

### Test: invalid seat quantity is rejected

Test:

- negative seats,
- zero seats with zero cargo.

Assert:

- a bad-request result,
- no booking is inserted.

### Test: cargo availability is enforced

Arrange a bus with either:

- no cargo support, or
- insufficient remaining cargo capacity.

Assert:

- the booking is rejected,
- no booking row is created,
- the trip cargo counter remains unchanged.

### Transaction note

The booking service uses a transaction for the booking insert and trip counter update. An in-memory provider may not prove all relational transaction guarantees. The test should still verify the observable state after success and failure, while a later SQL Server integration test can validate provider-specific behavior.

## 12. Authentication tests

Authentication has two useful layers.

### Auth service tests

Test:

- registration creates a user,
- the stored password is not the submitted password,
- duplicate email registration is rejected,
- valid credentials return a token,
- unknown email returns the same unauthorized outcome as a wrong password.

Do not assert the exact JWT string. Assert that a non-empty token is returned and that the response contains the expected user identity.

### API authentication tests

Later, test the HTTP boundary:

- no token on booking endpoint → `401`,
- malformed token → `401`,
- valid token → request reaches authorization and service logic.

These tests require the API host and token configuration, so they can be added after the service tests if setup becomes too large for the first pass.

## 13. Test data helpers

Test data should be explicit and small.

A helper can create valid entities, but each test should make the important values visible:

```text
create test user
create test depots
create test bus
create test route
create test trip
```

Avoid one enormous global seed that every test depends on. Large shared fixtures make failures harder to understand and can cause tests to influence one another.

Use fixed values where they clarify the scenario. Use unique database names and IDs when isolation is required.

## 14. Test naming

Use names that describe behavior and expected outcome:

```text
GetActiveDepotsAsync_returns_only_active_depots
SearchTripAsync_returns_only_trips_matching_route_and_date
GetTripByIdAsync_returns_not_found_for_unknown_trip
CreateBookingAsync_updates_seats_when_capacity_exists
CreateBookingAsync_returns_conflict_when_requested_seats_exceed_capacity
LoginAsync_returns_unauthorized_for_unknown_email
```

A good test name should explain the failure without requiring the reader to open the implementation.

## 15. Running the tests

From the solution folder:

```bash
dotnet test
```

For only the test project:

```bash
dotnet test BusBookingSystem.Tests/BusBookingSystem.Tests.csproj
```

For a focused test during development:

```bash
dotnet test --filter FullyQualifiedName~DepotServiceTests
```

The useful loop is:

```text
write one test
  -> run that test
  -> understand the failure
  -> make the smallest production or test-data correction
  -> rerun the focused test
  -> continue
```

Do not wait until every test is written before running the suite.

## 16. What the tests should not hide

A test is not useful if it only proves that a mock was called:

```text
mock.Verify(service => service.Save(...))
```

That does not prove that:

- the query filters correctly,
- the projection has the right values,
- the booking row is persisted,
- availability changes correctly,
- invalid requests are rejected.

Prefer assertions against returned results and the database state that the user-visible behavior depends on.

## 17. Day 8 completion checklist

- [ ] A dedicated xUnit test project exists.
- [ ] The test project is included in the solution.
- [ ] A controlled database provider is configured.
- [ ] Each test has isolated database state.
- [ ] Active depot filtering is covered.
- [ ] Empty depot results are covered.
- [ ] Trip search route filtering is covered.
- [ ] Trip search date filtering is covered.
- [ ] Empty trip search results are covered.
- [ ] Missing trip details are covered.
- [ ] Successful booking is covered.
- [ ] Overbooking rejection is covered.
- [ ] Invalid booking input is covered.
- [ ] Authentication service behavior is covered where practical.
- [ ] Anonymous booking access is covered at the API boundary.
- [ ] `dotnet test` runs successfully.
- [ ] The test suite can run repeatedly from a clean state.

## Summary

Day 8 turns the previous manual journey into repeatable evidence:

```text
browse depots
  -> search trips
  -> view trip details
  -> authenticate
  -> create booking
  -> reject invalid and over-capacity requests
```

The testing approach follows the project's existing architecture. Services are tested with real EF Core queries against a controlled database, while API integration tests are added where middleware and authentication behavior need to be proven.

The first practical step is deliberately small: create the test project and prove the active-depot service behavior before expanding into trips, bookings, and authentication.
