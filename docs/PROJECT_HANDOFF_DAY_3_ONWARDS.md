# Bus Booking System - Handoff Context for Day 3 Onwards

## Purpose

This document gives the next development environment the context needed to continue the Bus Booking System MVP without repeating earlier architectural discussions.

The project is being built as a backend REST API MVP for the MRA application. The Blazor/Web project is out of scope for this 9-day sprint.

## Current working approach

Build the product by customer-visible vertical feature slices, not by completing one technical layer at a time.

For every feature, follow this order:

```text
User story
  -> acceptance criteria
  -> use case
  -> focused DTO/request contract
  -> service interface
  -> Core/Infrastructure/API implementation
  -> manual verification
  -> automated tests
```

The architecture is still layered:

```text
Core <- Application <- Infrastructure <- API
```

Dependency meaning:

- `Core` contains domain entities and enums. It should not depend on databases, EF Core, HTTP, or the API.
- `Application` contains DTOs and service interfaces. It should reference Core, but not Infrastructure or API.
- `Infrastructure` contains `AppDbContext`, EF Core configuration, database queries, and concrete service implementations.
- `API` contains controllers, dependency injection registration, HTTP behavior, Swagger, and configuration.

The vertical-slice approach means each feature can touch all required projects and finish with a working endpoint and feedback.

## Important architecture decisions

### Direct DbContext access

Do not add custom repository interfaces or repository classes for this MVP.

Infrastructure services should inject and use `AppDbContext` directly. A repository would add another wrapper around EF Core's existing data-access abstraction without enough benefit for this project.

The intended path is:

```text
Controller
  -> Application service interface
  -> Infrastructure service implementation
  -> AppDbContext
  -> DTO projection
  -> HTTP response
```

### DTO design

DTOs must be designed backward from a real user story or API use case. They should not mirror the entire entity.

For each property, ask:

1. Which current MVP screen or request uses it?
2. Is it displayed, submitted later, or needed to identify a record?
3. Can the use case work without it?
4. Is it sensitive, internal, audit-only, nested, or future-feature data?

Include IDs when the client needs to identify a record or submit it in a later request. Include display names when the client needs to show information without another request.

Exclude navigation objects, internal database details, audit timestamps, sensitive data, and fields belonging only to future features.

### Manual projection and mapping

Skip AutoMapper during the sprint. Use explicit LINQ projection or manual mapping so it is obvious which fields the API returns.

## Domain decisions

These decisions were made before the current handoff:

- Use `User` with `UserType.Customer` as the booking owner.
- Do not create a separate `Customer` entity for this MVP.
- Treat `Trip` as the combined schedule/trip concept.
- `Trip` contains the bus, route, times, seats, and fare needed by the booking flow.
- `BookingStatus` contains `Pending`, `Confirmed`, and `Cancelled`.
- Entity identifiers use `Guid`.

The current Core entities include:

- `Address`
- `User`
- `Depot`
- `Bus`
- `Route`
- `Trip`
- `Booking`

`UserType` is currently associated with `User`, and `BookingStatus` is currently associated with `Booking`. Their placement beside the entity is valid. They may later be moved to a separate `Core/Enums` folder for organization, but that is not required for functionality.

## Completed context: Day 1

Day 1 established or reviewed the Core domain model.

The important outcome was that Core contains the domain concepts needed for depot selection, trip search, trip details, and booking.

Before changing Core, inspect the actual current entity definitions because the repository may contain user edits made after this handoff was written.

## Completed context: Day 2 direction

Day 2 is the first vertical slice: depot choices.

User story:

> As a customer, I want to see active depots so that I can choose an origin and destination when searching for a trip.

Acceptance criteria:

- `GET /api/depots` exists.
- It returns active depots.
- It excludes inactive depots.
- Each item contains an ID and display values needed by a search form.
- It does not expose the full entity or navigation object graph.
- The controller calls a service instead of querying the database directly.
- Infrastructure accesses the database through `AppDbContext`.
- Swagger can exercise the endpoint.

### Day 2 naming decisions

The depot list DTO is named:

```text
DepotListDto
```

The service method is named:

```text
GetActiveDepotsAsync()
```

Do not rename these back to `DepotDto` or `GetActiveAsync` without a specific reason.

The intended contract is conceptually:

```csharp
Task<IReadOnlyList<DepotListDto>> GetActiveDepotsAsync();
```

The exact collection type may follow the existing project style, but the method should return focused DTOs, not `Depot` entities or EF Core types.

### DepotListDto purpose

`DepotListDto` is a list response used to populate origin and destination choices for trip search.

It should contain only the fields required by that use case, such as:

```text
Id
DepotCode
Name
DisplayLocation or City
```

Use the actual address property available in the current `Address` model. Do not expose the complete `Address` navigation object merely to avoid choosing one display field.

The ID is included because the customer sees the name but the later trip-search request must submit a stable depot identifier.

## Current project state to inspect first

Before continuing, inspect the actual workspace rather than assuming every Day 2 step has already been implemented. The docs describe the intended work, but the next environment must verify the code.

Relevant project locations:

```text
BusBookingSystem.Core/
BusBookingSystem.Application/
BusBookingSystem.Infrastructure/
BusBookingSystem.API/
```

Likely files to inspect:

```text
BusBookingSystem.Core/Entities/Depot.cs
BusBookingSystem.Application/DTOs/DepotListDto.cs
BusBookingSystem.Application/Interfaces/Services/IDepotService.cs
BusBookingSystem.Infrastructure/
BusBookingSystem.API/Program.cs
BusBookingSystem.API/appsettings.json
```

Check whether these exist before creating duplicates:

- `AppDbContext`
- `DepotListDto`
- `IDepotService`
- concrete depot service
- `DepotsController`
- EF Core package references
- connection string
- migration files
- depot seed data

## Day 3 starting point

Day 3 is the next vertical slice: trip search.

User story:

> As a customer, I want to search for trips between two depots on a date so that I can choose a journey.

Acceptance criteria should include:

- origin depot is required,
- destination depot is required,
- travel date is required,
- origin and destination cannot be the same,
- only trips matching the selected route are returned,
- only trips matching the requested date are returned,
- results include departure and arrival times,
- results include fare and available seats,
- results include enough bus and depot display information,
- every result includes the `TripId` needed for selection and later booking,
- no entity graph or internal audit fields are exposed,
- an empty match returns an empty list rather than an error.

### Day 3 DTO decision

The primary search response should be a focused `TripSearchResultDto`, derived from the search screen's needs.

The current intended shape is conceptually:

```text
TripId
RouteId
BusId
BusName
OriginId
OriginName
DestinationId
DestinationName
DepartureTime
ArrivalTime
BaseFare
AvailableSeats
```

The exact property names should follow the current code and entity model. `Guid` should be used for IDs because the Core entities use `Guid` identifiers.

This DTO is not required to mirror `Trip`, `Bus`, `Route`, or `Depot`. It flattens the display values needed by the search result while retaining IDs needed for later requests.

### Day 3 service contract

The intended service operation is conceptually:

```text
ITripService.SearchTripsAsync(originDepotId, destinationDepotId, date)
```

Use the date type already selected by the project. If the entity uses `DateTimeOffset`, be careful to define what "matching the date" means and avoid accidental timezone bugs.

Day 3 should implement the complete path:

```text
Trip-search user story
  -> acceptance criteria
  -> TripSearchResultDto
  -> ITripService.SearchTripsAsync(...)
  -> Infrastructure service using AppDbContext
  -> TripsController search endpoint
  -> Swagger verification
  -> focused tests
```

Do not create `IBusService` or `IRouteService` solely because Bus and Route entities exist. Add standalone service contracts only when a real MVP use case needs a separate bus or route endpoint.

## Remaining vertical slices

### Day 4: selected-trip details

User story:

> As a customer, I want to view the trip I selected so that I can confirm it before booking.

Add `GetByIdAsync(Guid id)` to the trip service only if this detail use case needs it. Reuse the search DTO if the detail response does not need a genuinely different shape. Add a separate `TripDto` only when the response has a clear additional purpose.

### Day 5: create a booking

User story:

> As a customer, I want to book seats on a selected trip so that my seats are reserved.

Build this end to end:

- booking request DTO,
- booking response DTO,
- `IBookingService`,
- direct `AppDbContext` implementation,
- availability check,
- seat decrement,
- booking creation,
- booking reference generation,
- transaction,
- concurrency handling,
- controller,
- success and failure tests.

Do not add `IBookingRepository`.

This is the highest-value feature and should not be cut if time is tight.

### Day 6: authentication

User story:

> As a customer, I want to register and log in so that my booking belongs to me.

Implement registration, login, JWT creation, and authorization around booking endpoints. Keep trip browsing public.

### Day 7: errors and polish

Add request validation, centralized exception handling, consistent ProblemDetails responses, Swagger summaries, and cleanup of scaffold code.

### Day 8: automated tests

Cover real behavior:

- active depot filtering,
- trip search filtering and projection,
- trip-not-found,
- booking success,
- overbooking rejection,
- authentication boundaries.

Prefer service/database behavior tests over mock interaction tests.

### Day 9: buffer and release fact-check

Use the day to finish the highest-risk incomplete slice, write the README, run the full journey from a clean setup, verify Swagger, and remove unsupported CV claims.

## Working style for the next environment

1. Read the relevant existing files before editing.
2. State one local hypothesis about what controls the behavior.
3. Make the smallest focused edit.
4. Run the narrowest useful build or test immediately.
5. Continue only after the focused check passes.
6. Do not modify files outside the requested scope without explaining why.
7. Preserve user changes already present in the workspace.
8. Keep the user informed about what was discovered and what is being implemented.

## Source documentation

The detailed project plan is in:

```text
docs/BusBookingSystem_9Day_Build_Plan.md
```

The detailed Day 2 walkthrough is in:

```text
docs/day 2 walk through.md
```

These documents should remain aligned with the decisions in this handoff.
