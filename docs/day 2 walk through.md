# Day 2 Walkthrough - Application Contracts From MVP Use Cases

This is the day 2 implementation pass for the 9-day MVP plan.

The guiding rule for today is:

> Start with what the user must do, then define only the data and service methods needed to support it.

Do not copy every entity property into every DTO. Do not create contracts for features that are not part of this MVP.

## 1) Define the MVP read-side use cases

The read-side of this bus booking MVP needs to support this basic journey:

1. A customer chooses an origin depot, destination depot, and travel date.
2. The system returns matching trips.
3. The customer selects one trip.
4. The system returns enough detail to confirm the selected trip before booking.
5. Later, Day 5 uses the selected trip ID when creating a booking.

That journey determines what Day 2 needs to define.

For now, do not design for:

- admin dashboards,
- bus maintenance screens,
- editing depots,
- payments,
- cancellation workflows,
- passenger management,
- reporting,
- route planning beyond origin and destination.

Those may be valid future features, but they should not make today's contracts larger.

## 2) Confirm the domain foundation

Before creating Application contracts, confirm the Core layer contains the concepts needed by the journey:

- `Depot`
- `Bus`
- `Address`
- `Route`
- `Trip`
- `Booking`
- `User`
- `UserType`
- `BookingStatus`

The domain entities are the source of data, but they are not automatically the API response shape.

The Application layer will select the fields needed by each use case and map entity data into DTOs.

## 3) Create the Application structure

Use this structure:

```text
BusBookingSystem.Application/
  DTOs/
  Interfaces/
    Services/
```

The Application project should reference Core. It should not reference Infrastructure or the API project.

Do not add repository interfaces or repository classes. Infrastructure will use `AppDbContext` directly inside the service implementations. A custom repository would add another wrapper around the EF Core abstraction without helping this small MVP.

## 4) Work backward from the trip-search screen

Imagine the eventual request:

```text
GET /api/trips/search?originDepotId=...&destinationDepotId=...&date=...
```

The user needs to compare results and select one. Therefore each result must contain:

- the selected trip's `Id`,
- enough route information to know where it goes,
- departure and arrival times,
- the price,
- remaining availability,
- enough bus information to make the result useful.

It does not need database audit timestamps, the complete Bus entity, the complete Route entity, or nested entity objects.

### 4.1 TripSearchResultDto

Purpose: one row in the trip-search response.

Suggested properties:

```text
TripId
RouteId
OriginDepotId
OriginDepotName
DestinationDepotId
DestinationDepotName
DepartureTime
ArrivalTime
BaseFare
AvailableSeats
BusModel
```

Why the IDs are included:

- `TripId` is required when the customer selects a trip and later creates a booking.
- `RouteId` identifies the route if the API later needs to link to route details.
- Depot IDs identify the selected origin and destination without requiring the client to match names.

Why other entity fields are excluded:

- The search screen does not need every bus property.
- It does not need `CreatedAt` or `UpdatedAt`.
- It does not need nested `Bus`, `Route`, or `Depot` objects.
- It does not need internal persistence details.

For the MVP, this is the most important DTO. Build it from the search screen's needs first.

## 5) Work backward from the selected-trip detail

After a customer selects a search result, the API may return a trip detail response before the booking request is submitted.

Ask: what must the customer confirm?

- Which trip was selected?
- Which bus is operating it?
- Which route is being travelled?
- When does it leave and arrive?
- What does it cost?
- How many seats remain?

### 5.1 TripDto

Purpose: details for one selected trip.

Suggested properties:

```text
Id
RouteId
BusId
OriginDepotId
OriginDepotName
DestinationDepotId
DestinationDepotName
DepartureTime
ArrivalTime
BaseFare
AvailableSeats
TotalSeats
BusModel
```

Keep `TripDto` separate from `TripSearchResultDto` only if the detail response genuinely needs more fields. If both responses stay identical during the MVP, use one DTO rather than creating two names for the same shape.

The important decision is not the class name. It is avoiding fields that no current use case needs.

## 6) Decide whether DepotDto is actually needed

Ask what the MVP client needs from depot data.

If the search form displays a list of depots, it needs:

```text
Id
Name
City or Address
```

That is enough for a depot selection dropdown.

### 6.1 DepotDto

Purpose: populate the origin and destination choices used by trip search.

Suggested MVP properties:

```text
Id
DepotCode
Name
City or Address
IsActive
```

Include `Id` because the search request should submit the selected depot's ID, not its display name.

Exclude these unless a current screen needs them:

- `Latitude`
- `Longitude`
- `PhoneNumber`
- `CreatedAt`
- `UpdatedAt`

Those may matter to an operations or depot-details screen, but they are not necessary for a customer searching for a trip.

## 7) Decide whether BusDto is needed for the MVP

Ask whether the application has a separate bus-list screen.

For the 9-day MVP, the answer is probably no. The customer sees bus information as part of a trip result, so a standalone `BusDto` may not be required yet.

Do not create `BusDto` just because a `Bus` entity exists.

Use bus fields directly in `TripSearchResultDto` or `TripDto`:

```text
BusId
BusModel
Capacity or available-seat information, only if displayed
```

Add `BusDto` later if the API needs a real `GET /api/buses` use case for an operator or admin.

## 8) Decide whether RouteDto is needed for the MVP

You already created `RouteDto`. Keep it small and make its purpose explicit.

Purpose: return a route when the API needs to list or inspect routes separately from trips.

Suggested properties:

```text
Id
Name
OriginDepotId
OriginDepotName
DestinationDepotId
DestinationDepotName
```

Exclude `CreatedAt` and `UpdatedAt` unless a current consumer needs audit information.

The route IDs and depot IDs are useful because routes and depots are separate records. The display names are useful because the client should not need another request just to render the route label.

If the MVP never has a standalone route screen, `RouteDto` may not be needed at all. The route information can remain inside the trip DTO. Keep it only if the API has a route-list or route-detail use case.

## 9) Use the same backward process for every DTO

For each proposed property, ask:

1. Which MVP screen or request uses this value?
2. Is it displayed, submitted later, or needed to identify a record?
3. Can the client perform the use case without it?
4. Is it sensitive, internal, or only useful to the database?
5. Would including it couple the API to implementation details?

Use this decision table:

| Property type | MVP decision |
|---|---|
| Resource ID used by a later request | Include |
| Display name shown to the user | Include |
| Search/filter value submitted by the client | Include |
| Price, time, and availability needed to choose a trip | Include |
| Navigation property or nested entity | Exclude; flatten needed values |
| `CreatedAt`/`UpdatedAt` | Exclude unless displayed or audited |
| Internal flags | Exclude unless they affect the current use case |
| Passwords or authentication data | Never include |
| Future-feature data | Leave out for now |

This is how you prevent DTOs from becoming copies of entities.

## 10) Define service interfaces from the same use cases

The service interfaces represent actions the Application layer promises to provide. They should be derived from API use cases, not from database tables.

### 10.1 IDepotService

Purpose: provide the active depots needed to populate trip-search inputs.

MVP contract:

```text
GetActiveAsync()
```

Add `GetByIdAsync(Guid id)` only if the MVP has a depot-detail use case. Do not add it automatically.

### 10.2 ITripService

Purpose: support the customer journey from searching for a trip to inspecting the selected trip.

MVP contract:

```text
SearchTripsAsync(Guid originDepotId, Guid destinationDepotId, DateOnly date)
GetByIdAsync(Guid id)
```

Why these methods exist:

- `SearchTripsAsync` supports the primary customer workflow.
- `GetByIdAsync` supports confirmation of a selected trip and gives Day 5 a clear trip lookup boundary before booking.

Do not add update, delete, or create-trip methods yet. Those belong to an operator/admin workflow that is outside this MVP slice.

### 10.3 IBusService

Do not create this interface unless the MVP has a standalone bus use case.

If the only place bus data appears is inside trip search results, the trip service can project the few required bus fields directly. Add `IBusService` later when a real bus-list or bus-detail endpoint is needed.

### 10.4 Route service

Apply the same rule to routes. Do not create `IRouteService` solely because a `Route` entity exists.

Create it only if the MVP needs a standalone route operation, such as:

```text
GetRoutesAsync()
GetByIdAsync(Guid id)
```

If route data is only needed while searching trips, keep it inside the trip-search projection.

## 11) Keep EF Core behind the service contract

The Application project defines DTOs and service interfaces only.

The Infrastructure implementation may:

- inject `AppDbContext`,
- query `DbSet` properties,
- filter by origin, destination, and date,
- project directly into DTOs,
- return the service contract's result.

The API controller should depend on `ITripService` or `IDepotService`, not on `AppDbContext`.

This gives the MVP one useful boundary without adding a repository wrapper around EF Core.

## 12) Manual mapping and projection

Skip AutoMapper for this sprint. Use explicit projection so it is obvious which fields are returned.

Conceptually, a trip search query should select only the fields in `TripSearchResultDto`, rather than loading complete entity graphs and exposing them.

This keeps responses smaller and makes the DTO decision visible in the code.

## 13) Day 2 implementation checklist

- [ ] Write down the MVP customer trip-search journey
- [ ] Identify the fields required to choose a trip
- [ ] Keep `RouteDto` limited to its actual route use case
- [ ] Create `DepotDto` only for depot selection data
- [ ] Create `TripSearchResultDto` for the primary search response
- [ ] Create `TripDto` only if selected-trip detail needs a different shape
- [ ] Skip `BusDto` unless a standalone bus use case exists
- [ ] Skip route or bus service interfaces unless their own use cases exist
- [ ] Create `IDepotService` around active-depot lookup
- [ ] Create `ITripService` around search and selected-trip lookup
- [ ] Keep IDs required for later requests and selection
- [ ] Exclude audit, internal, nested, and future-feature fields
- [ ] Use no repository interfaces or repository classes
- [ ] Confirm Application references Core only
- [ ] Build the Application project

## 14) Day 3 handoff

Day 3 will implement the contracts that survived this use-case review:

- add EF Core SQL Server packages,
- create `AppDbContext`,
- add the required `DbSet` properties,
- implement the depot and trip services with direct DbContext queries,
- project query results into the focused DTOs.

If a DTO or interface cannot be connected to a current MVP use case, leave it out until a real requirement appears.
