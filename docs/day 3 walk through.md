# Day 3 Walkthrough - Trip Search End to End

Day 3 is the next vertical feature slice after depots.

The goal is to complete one focused user story from idea to working API:

```text
User story
  -> acceptance criteria
  -> request contract
  -> DTO + service contract
  -> database access
  -> API endpoint
  -> manual verification
```

This project is not using the full repository-heavy workflow from a generic Clean Architecture template. The repo is intentionally using a lighter pattern:

```text
Core entity
  -> Application DTO and service interface
  -> Infrastructure AppDbContext + concrete service
  -> API controller
```

This is the local workflow already reflected in the current codebase.

## 1. Understand the feature before coding

### User story

> As a customer, I want to search for trips between two depots on a date so that I can choose a journey.

Do not build these features today:

- booking creation
- payment handling
- authentication
- depot management
- route management beyond what the trip search needs

### Acceptance criteria

The feature is complete when all of the following are true:

- origin depot is required
- destination depot is required
- travel date is required
- origin and destination cannot be the same
- only trips matching the selected route are returned
- only trips matching the requested date are returned
- results include departure and arrival times
- results include fare and available seats
- results include enough bus and depot display information
- every result includes a `TripId` needed for later booking
- no entity graph or internal audit fields are exposed
- an empty match returns an empty list instead of an error

## 2. Confirm the current project pattern

Before creating anything new, confirm the pattern already used in this repo:

```text
Core: domain entities
Application: DTOs + service interfaces
Infrastructure: AppDbContext + concrete service implementations
API: controller + dependency injection
```

This project intentionally avoids a custom repository layer for the MVP. Infrastructure services should use `AppDbContext` directly.

### Important repo-specific rule

Do not add repository interfaces or repository classes unless the feature truly requires it. The current day-2 and day-3 work is built to the service-first model already present in the code.

## 3. Confirm the current domain model

The relevant entities already in the repo are:

- `Depot`
- `Bus`
- `Route`
- `Trip`
- `Address`

The `Trip` entity already models the trip concept in a practical way:

```csharp
public class Trip
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Bus Bus { get; set; }
    public required Route Route { get; set; }
    public required decimal BaseFare { get; set; }
    public int SeatsBooked { get; set; } = 0;
    public required DateTimeOffset DepartureTime { get; set; }
    public DateTimeOffset? ArrivalTime { get; set; }
}
```

This indicates the trip search should focus on:

- route selection based on depots
- date filtering using departure time
- seat availability calculation
- flattened display values for the client

### What to verify before coding

Check the actual current entity fields in:

```text
BusBookingSystem.Core/Entities/Trip.cs
BusBookingSystem.Core/Entities/Route.cs
BusBookingSystem.Core/Entities/Bus.cs
BusBookingSystem.Core/Entities/Depot.cs
```

If the repo has already been edited after the handoff, use the current model rather than assumptions.

## 4. Confirm the DTO shape

The day-3 DTO already exists in the repo as:

```text
BusBookingSystem.Application/DTOs/TripSearchResultDto.cs
```

The intended shape is conceptually:

```csharp
public class TripSearchResultDto
{
    public Guid TripId { get; set; }
    public Guid RouteId { get; set; }
    public string BusName { get; set; } = null!;
    public string OriginName { get; set; } = null!;
    public string DestinationName { get; set; } = null!;
    public DateTime DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public decimal BaseFare { get; set; }
    public int AvailableSeats { get; set; }
}
```

This DTO is intentionally flattened. It is not the full entity graph.

### Why this shape works

It includes:

- the trip identifier required for later booking
- route information for selection and context
- bus name needed by the search result UI
- origin and destination names for display
- departure and arrival times for the user
- fare and available seats for comparison

It intentionally omits:

- full `Address` objects
- navigation graphs
- EF Core hidden state
- audit fields not needed by the UI

## 5. Define the service contract

The intended service contract for Day 3 is conceptually:

```csharp
public interface ITripService
{
    Task<IReadOnlyList<TripSearchResultDto>> SearchTripsAsync(
        Guid originDepotId,
        Guid destinationDepotId,
        DateOnly travelDate);
}
```

Or a close equivalent using the date type already chosen in the project.

### Important rule

Use the date type the project is already using in the Core model and DTOs. Do not change the model just to make the search easier. If the system uses `DateTimeOffset` for departures, make the filtering logic explicit so it does not accidentally compare against the wrong timezone.

### Why this method exists

This method is not a generic trip query. It is purpose-built for the story:

> search trips between two depots on a given date.

It should not expose the entire `Trip` entity or the `Route`/`Bus` graph.

## 6. Implement the infrastructure query

The concrete service should live in:

```text
BusBookingSystem.Infrastructure/Services/TripService.cs
```

The service should:

1. inject `AppDbContext`
2. query `context.Trips`
3. filter by route/origin/destination
4. filter by travel date
5. filter out invalid combinations
6. project to `TripSearchResultDto`
7. execute asynchronously
8. return the list

### The query pattern should look like this conceptually

```csharp
var trips = await _context.Trips
    .AsNoTracking()
    .Where(t => t.Route.OriginId == originDepotId)
    .Where(t => t.Route.DestinationId == destinationDepotId)
    .Where(t => t.DepartureTime.Date == travelDate)
    .Select(t => new TripSearchResultDto
    {
        TripId = t.Id,
        RouteId = t.Route.Id,
        BusName = t.Bus.Name,
        OriginName = t.Route.Origin.Name,
        DestinationName = t.Route.Destination.Name,
        DepartureTime = t.DepartureTime,
        ArrivalTime = t.ArrivalTime,
        BaseFare = t.BaseFare,
        AvailableSeats = t.Bus.SeatCapacity - t.SeatsBooked
    })
    .ToListAsync();
```

### Important operational details

- Do the filtering in the database, not in memory after loading everything.
- Project directly to the DTO.
- Return an empty list if there are no matches.
- Avoid returning entities or navigation graphs.

### Availability calculation

The project currently uses `SeatsBooked` on `Trip`. That is the simplest MVP model. The available seats can be calculated as:

```text
AvailableSeats = TotalSeats - SeatsBooked
```

Use the actual `Bus` or `Trip` field names already present in the current model. Do not invent a different seat model unless the repo already shows a consistent pattern.

## 7. Add the controller endpoint

Create:

```text
BusBookingSystem/Controllers/TripsController.cs
```

The controller should:

1. be an API controller
2. inject `ITripService`
3. expose a GET action, such as `SearchTrips`
4. accept origin depot id, destination depot id, and date
5. call the service
6. return `Ok(trips)`

Conceptually:

```csharp
[HttpGet("search")]
public async Task<IActionResult> SearchTrips(
    [FromQuery] Guid originDepotId,
    [FromQuery] Guid destinationDepotId,
    [FromQuery] DateTime travelDate)
{
    var trips = await _tripService.SearchTripsAsync(originDepotId, destinationDepotId, DateOnly.FromDateTime(travelDate));
    return Ok(trips);
}
```

### Controller rules

The controller should not:

- inject `AppDbContext`
- include EF query logic
- return `Trip` entities
- decide which route combinations are valid

The controller is just the HTTP adapter.

## 8. Validate the route and date rules

Before shipping the endpoint, define the validation rules in the service layer or controller layer.

At minimum:

- origin depot is required
- destination depot is required
- travel date is required
- origin and destination cannot be the same

The core business logic should handle the invalid combination gracefully, for example by returning an empty list or a validation error depending on the project's current API conventions.

### Repo-specific note

This project is still in a thin MVP shape. Do not overengineer validation with a full FluentValidation pipeline unless the project already uses it. Keep it practical and explicit.

## 9. Wire DI and startup

Register the service in the API startup file, following the pattern already used in this repo:

```csharp
builder.Services.AddScoped<ITripService, TripService>();
```

Also ensure the API project has access to the Application interface and Infrastructure implementation.

### Checkpoint

If the app fails with “unable to resolve service,” check:

- the Application project reference is present
- the Infrastructure project reference is present
- the interface and implementation namespaces are correct
- the startup file is the active API project
- `AppDbContext` is successfully registered

## 10. Manual verification in Swagger or Scalar

Run the app and open the API docs.

### Test 1: valid trip search

Use two valid depots and a date with known trips.

Expected result:

- HTTP 200
- one or more trip rows
- each row includes the expected DTO values

### Test 2: origin and destination same

Expected result:

- invalid request rejected or empty list depending on the chosen MVP rule
- no database exception

### Test 3: no results

Use a date with no matching trips.

Expected result:

```json
[]
```

This is a valid empty result, not an error.

### Test 4: response shape

Inspect the JSON and confirm it contains only the selected fields, not:

- full `Trip` entity data
- nested `Route` objects
- nested `Bus` objects
- full `Depot` entity graphs
- EF tracking metadata

## 11. Focused automated tests

If a test project is available, add tests that exercise the real behavior instead of mock-only assertions.

Recommended cases:

1. returns matching trips for valid origin/destination/date
2. excludes trips from other routes
3. excludes trips for other dates
4. returns empty list when no trips match
5. rejects same-origin/same-destination request
6. maps the DTO values correctly

This is the testing target for Day 8 if the repo does not yet have a proper test project ready.

## 12. Troubleshooting guide

### The query returns zero rows unexpectedly

Check:

- route origin/destination IDs are correct
- date filtering uses the same date semantics as the stored departure time
- `DepartureTime` is not null
- the correct `DbSet` is being queried

### The DTO is missing fields

Check the actual `Trip`, `Bus`, and `Route` entity definitions before adding new properties. The query should project only what the UI needs.

### The controller route is not visible

Verify:

- the controller is in the active API project
- the controller has `[ApiController]`
- the route is correct
- the service is registered in DI

### The route contains wrong depot data

The filtering should be done by the actual relation between `Trip.Route` and the selected depots, not by a name string.

## 13. Recommended working style for this project

Use this in-order workflow for Day 3 and future slices:

1. Read the actual entity and DTO files before changing anything.
2. State one local hypothesis for the behavior being controlled.
3. Make the smallest change that matches the user story.
4. Build or run the closest validation immediately.
5. Keep the service layer responsible for business filtering.
6. Keep the controller responsible for HTTP translation only.
7. Preserve user edits in the current workspace.

## 14. Summary

The Day 3 slice is the trip search feature:

- model and filter by route + date
- project results to `TripSearchResultDto`
- use `AppDbContext` directly in the Infrastructure service
- keep the API controller thin
- verify the endpoint through Swagger/Scalar

This matches the repo’s current architecture and the project handoff, and it keeps the implementation aligned with the rest of the sprint.
