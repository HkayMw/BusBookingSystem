# Day 4 Walkthrough - Selected Trip Details End to End

Day 4 is the next vertical feature slice after trip search.

The goal is to complete one focused user story from idea to working API:

```text
User story
  -> acceptance criteria
  -> DTO and service contract
  -> database access
  -> API endpoint
  -> manual verification
```

This project is still following the lightweight layered pattern already established in the repo:

```text
Core entity
  -> Application DTO + service interface
  -> Infrastructure AppDbContext + concrete service
  -> API controller
```

Do not move into a full repository layer or an enterprise-style pattern unless the project clearly needs it.

## 1. Understand the feature before coding

### User story

> As a customer, I want to view the trip I selected so that I can confirm it before booking.

This is a detail view of a trip that the user chose from the trip-search results.

### What this feature is not

It is not:

- booking creation,
- payment,
- auth,
- route management,
- bus management.

Those remain later vertical slices.

### Acceptance criteria

The feature is complete when all of the following are true:

- a trip detail endpoint exists for selected trip IDs,
- the request includes the selected trip id,
- the response includes the trip details needed before booking,
- it returns the correct bus, route, depot, date, and fare information,
- it does not expose the full entity graph,
- it returns a not-found result when the trip does not exist,
- the response is focused and intentional.

## 2. Confirm the current project pattern

Before coding, confirm the current repo pattern:

- `Core` contains `Trip`, `Bus`, `Route`, `Depot`
- `Application` contains DTOs and service interfaces
- `Infrastructure` contains `AppDbContext` and the concrete service implementation
- `API` contains the controller and DI wiring

This project intentionally keeps the service layer in front of `AppDbContext`, rather than adding a repository abstraction for the MVP.

## 3. Confirm the actual Trip model

Open the current trip entity and inspect the actual fields before writing the API contract.

The current repo already has the model in:

```text
BusBookingSystem.Core/Entities/Trip.cs
```

The current shape is conceptually:

```csharp
public class Trip
{
    public Guid Id { get; set; }
    public Guid BusId { get; set; }
    public Bus Bus { get; set; } = default!;
    public Guid RouteId { get; set; }
    public Route Route { get; set; } = default!;
    public required decimal SeatFare { get; set; }
    public decimal? CargoFare { get; set; }
    public int SeatsBooked { get; set; }
    public required DateTimeOffset DepartureTime { get; set; }
    public DateTimeOffset? ArrivalTime { get; set; }
}
```

That tells us the trip detail endpoint should likely include:

- `TripId`
- `BusName` or `BusModel`
- `RouteName`
- `OriginName`
- `DestinationName`
- `DepartureTime`
- `ArrivalTime`
- `SeatFare`
- `AvailableSeats`

## 4. Decide whether to reuse the search DTO or add a new one

The handoff says:

> Add `GetByIdAsync(Guid id)` to the trip service only if this detail use case needs it. Reuse the search DTO if the detail response does not need a genuinely different shape. Add a separate `TripDto` only when the response has a clear additional purpose.

This means the first design decision is:

- reuse `TripSearchResultDto` if it already contains the fields needed for the detail page,
- create a dedicated `TripDto` when the selected trip detail needs different or extra data.

### Good default for this project

If the selected-trip detail screen only needs the same summary data plus a few extra context values, reuse the search DTO.

If the detail view needs a richer display or more route and bus data, create a dedicated DTO.

### Rule of thumb

Do not create a new `TripDto` just because a route is involved. Create it only when the consumer truly needs a different response shape.

## 5. Define the service contract

The Day 4 service operation is conceptually:

```csharp
Task<TripSearchResultDto?> GetByIdAsync(Guid tripId);
```

Or, if the detail response is distinctly different:

```csharp
Task<TripDto?> GetByIdAsync(Guid tripId);
```

The key requirement is:

- exact purpose is “selected trip details”
- returns a focused DTO, not a full `Trip` entity
- works by a stable trip identifier
- returns `null` or a not-found result when the trip does not exist

## 6. Implement the Infrastructure query

The concrete service should query `AppDbContext.Trips` and include the needed relationships.

The query should likely do this:

```csharp
var trip = await _context.Trips
    .AsNoTracking()
    .Include(t => t.Bus)
    .Include(t => t.Route)
    .ThenInclude(r => r.Origin)
    .Include(t => t.Route)
    .ThenInclude(r => r.Destination)
    .FirstOrDefaultAsync(t => t.Id == tripId);
```

Then project to the DTO.

### Important scope rule

Do not load the whole entity graph just because it is convenient. Use only the fields required by the selected-trip details page.

### Minimum info for the detail page

At a minimum, the selected-trip detail likely needs:

- trip id
- bus name/model
- route name
- origin depot name
- destination depot name
- departure time
- arrival time
- fare
- available seats

## 7. Add the controller endpoint

Create a route like:

```text
GET /api/trips/{id}
```

The controller should:

1. accept the trip id in the route,
2. call the service,
3. return `Ok(dto)` when found,
4. return `NotFound()` when not found.

Conceptually:

```csharp
[HttpGet("{tripId:guid}")]
public async Task<IActionResult> GetTripById(Guid tripId)
{
    var trip = await _tripService.GetByIdAsync(tripId);

    if (trip is null)
        return NotFound();

    return Ok(trip);
}
```

### Controller rules

The controller should not:

- query the database directly,
- return `Trip` entities,
- decide the business rules behind selected-trip detail retrieval,
- contain EF projection code.

## 8. Validation and edge cases

Check these scenarios:

### Valid trip id

Expected result:

- HTTP 200
- DTO contains this trip’s details

### Missing trip id

Expected result:

- HTTP 404

### Incorrect route or missing related data

Expected result:

- the endpoint should fail gracefully,
- no entity graph leak,
- no unrelated audit or internal fields.

## 9. Manual verification in Swagger or Scalar

Run the app and use the generated docs.

### Test 1: valid detail request

Ask for a real trip id from seeded data.

Expected result:

- a valid single trip detail object
- expected route and depot information visible
- correct fare and travel times

### Test 2: invalid trip id

Use a non-existent GUID.

Expected result:

- `404 Not Found`

### Test 3: response shape

Confirm the response only contains the focused DTO data, not raw entity navigation objects.

## 10. Suggested test cases

If a test project exists, add tests for:

1. returns the selected trip data when found,
2. returns null or not found when the trip does not exist,
3. includes bus, route, and depot display information,
4. excludes raw entity graph data,
5. returns the expected fare and schedule values.

## 11. Keep the scope disciplined

Day 4 should not become:

- booking creation,
- ticket reservation,
- seat entitlement logic,
- payment integration,
- auth.

Those are future slices.

This day is only about showing the selected trip in a clean, customer-safe detail form.

## 12. Summary

Day 4 is the selected-trip detail slice:

- use trip id to fetch one trip
- project to a focused DTO
- include only the display and booking-prep data required by the screen
- return not found when absent
- keep the API controller thin

This matches the repo’s current architecture and keeps the project moving in the same service-first pattern already established by the earlier day slices.
