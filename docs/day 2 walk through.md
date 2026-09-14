# Day 2 Walkthrough - Depot Choices End to End

Day 2 is the first vertical feature slice.

The goal is to complete one small customer feature from idea to working API:

```text
User story
  -> acceptance criteria
  -> use case
  -> DTO and service interface
  -> database access
  -> API endpoint
  -> manual verification
```

You are not expected to know the next step from memory. Follow this document in order. After each major step, there is a checkpoint explaining what should now exist and what result should confirm that it is working.

## 1. Understand the feature before coding

### User story

> As a customer, I want to see active depots so that I can choose an origin and destination when searching for a trip.

This feature is deliberately narrow. A customer will eventually need depot choices for the trip-search form, but today we are only building the endpoint that supplies those choices.

Do not build these features today:

- trip search,
- bus listing,
- route management,
- booking,
- payments,
- authentication.

Those are separate vertical slices on later days.

### Acceptance criteria

The feature is complete when all of the following are true:

- `GET /api/depots` exists.
- It returns active depots.
- It does not return inactive depots.
- Each item contains an ID and the display values needed by a search form.
- It does not return the full `Depot` entity or its navigation objects.
- The controller calls a service rather than querying the database itself.
- The service uses `AppDbContext` directly in Infrastructure.
- The Infrastructure service accesses the database through `AppDbContext`.
- Swagger can call the endpoint and show the result.

## 2. Confirm the current project structure

Before adding anything, check that the solution contains these projects:

```text
BusBookingSystem.Core
BusBookingSystem.Application
BusBookingSystem.Infrastructure
BusBookingSystem.API
```

The dependency direction should be:

```text
Core <- Application <- Infrastructure <- API
```

For this feature, the responsibilities are:

- **Core:** contains the `Depot` entity.
- **Application:** contains `DepotListDto` and `IDepotService`.
- **Infrastructure:** contains `AppDbContext` and the concrete depot service.
- **API:** contains `DepotsController`.

If `AppDbContext` does not exist yet, this walkthrough creates the minimum one needed for depots. Do not try to finish the entire database model before getting this feature working.

### Checkpoint

You should be able to explain where each part will live before creating it. If you cannot, stop and use the responsibility list above as the guide.

## 3. Confirm the Depot entity fields

Open the existing Depot entity in:

```text
BusBookingSystem.Core/Entities/Depot.cs
```

Identify the fields that are useful to a customer choosing a depot. In the current model, these include values such as:

- `Id`
- `DepotCode`
- `Name`
- `Address`
- `IsActive`

The `Address` is a navigation/property object, so do not expose it directly from the DTO. Decide which simple display value should be returned from it, such as a city or formatted address, based on the fields that exist in the `Address` entity.

Do not copy every Depot property into the response. The customer does not need audit timestamps, coordinates, or phone details merely to choose an origin or destination.

### Checkpoint

Write down the response fields before coding the service:

```text
Id
DepotCode
Name
DisplayLocation
```

Use the actual property name you choose, such as `City` or `Address`, consistently in the DTO and projection. The important part is that it is a simple display value, not a nested entity.

## 4. Confirm the DTO purpose and shape

You renamed the DTO to `DepotListDto`. Keep that name. It communicates that this DTO is designed for a list response, not a full depot detail response.

Its purpose is:

> Return the minimum information needed to populate the origin and destination dropdowns in the future trip-search screen.

A suitable shape is:

```csharp
namespace BusBookingSystem.Application.DTOs
{
    public class DepotListDto
    {
        public Guid Id { get; set; }
        public string DepotCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string DisplayLocation { get; set; } = null!;
    }
}
```

Use your chosen display property name if your current DTO differs. Do not add fields just because they exist on `Depot`.

Why `Id` is included:

- the customer sees `Name` and the display location,
- the client submits `Id` later when searching for trips,
- names are display text and are not reliable identifiers.

Why the entity itself is not returned:

- returning entities couples the API to the database model,
- navigation properties can expose too much data,
- the list response should remain small and intentional.

### Action

Create or update the file:

```text
BusBookingSystem.Application/DTOs/DepotListDto.cs
```

Make sure its namespace matches the other DTOs in the Application project.

### Checkpoint

Build the Application project. The DTO should compile before you continue. If it does not, fix the namespace or project-reference problem now; later steps depend on this type.

## 5. Define the service contract

The API controller should not know how depots are stored. It should ask a service for the data it needs.

Create or update:

```text
BusBookingSystem.Application/Interfaces/Services/IDepotService.cs
```

The interface should represent the user story:

```csharp
using BusBookingSystem.Application.DTOs;

namespace BusBookingSystem.Application.Interfaces.Services
{
    public interface IDepotService
    {
        Task<IReadOnlyList<DepotListDto>> GetActiveDepotsAsync();
    }
}
```

Your existing project style may use a different collection type. The important parts are:

- the method is asynchronous,
- the method name is `GetActiveDepotsAsync`,
- it returns `DepotListDto` values,
- it does not return `Depot` entities,
- it does not expose `AppDbContext` or EF Core types.

### Why this method exists

`GetActiveDepotsAsync` exists because the user story needs active depot choices. It is not a generic database method.

Do not add these methods yet:

- `GetAllAsync`, because the customer should not receive inactive choices,
- `GetByIdAsync`, because there is no depot-detail user story,
- `CreateAsync`, `UpdateAsync`, or `DeleteAsync`, because depot administration is outside this MVP slice.

### Checkpoint

Build the Application project again. At this point, Application should contain a DTO and a service contract, but no EF Core code.

## 6. Check whether Infrastructure already has AppDbContext

Look in:

```text
BusBookingSystem.Infrastructure
```

Search for a class named `AppDbContext` or a class inheriting from `DbContext`.

### If AppDbContext already exists

Open it and check whether it has a depot set similar to:

```csharp
public DbSet<Depot> Depots { get; set; }
```

If the set exists, do not create a second context. Use the existing one and continue to the service implementation.

### If AppDbContext does not exist

Create the minimum Infrastructure database setup needed for this slice:

1. Add the EF Core SQL Server package to Infrastructure.
2. Add the EF Core design/tools package where your solution expects it.
3. Create `AppDbContext` inheriting from `DbContext`.
4. Add a `DbSet<Depot>` property.
5. Add a constructor accepting `DbContextOptions<AppDbContext>`.
6. Add the depot key and required-field configuration if conventions are not enough.

The context belongs in Infrastructure because EF Core is an implementation detail. Application should not reference it.

### Checkpoint

Infrastructure should compile and `AppDbContext` should expose exactly one depot set.

## 7. Configure the database connection

Open the API configuration file:

```text
BusBookingSystem.API/appsettings.json
```

Add or confirm a development connection string for SQL Server. Use the connection-string name that your `Program.cs` will register, for example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-development-sql-server-connection-string"
  }
}
```

Do not commit real passwords or production secrets. For local development, use the SQL Server instance available on your machine.

Then register the context in the API startup code. The registration should conceptually do this:

```text
Read DefaultConnection
  -> configure SQL Server
  -> register AppDbContext with dependency injection
```

The exact registration belongs in the API because the API is the application entry point, while the context type remains in Infrastructure.

### Checkpoint

Start the API or run the application build. If the connection string name, project reference, or provider package is wrong, fix that before migrations.

## 8. Create the database schema and seed data

Once the context and connection are configured:

1. Create the initial migration for the current model.
2. Apply the migration to the development database.
3. Add a few active depots.
4. Add at least one inactive depot.
5. Confirm the database contains the expected rows.

The seed data should make the acceptance criteria testable. For example:

```text
DEPOT-001 - Active
DEPOT-002 - Active
DEPOT-003 - Inactive
```

The exact names do not matter. Having both active and inactive records does.

If you do not yet have a seed mechanism, insert development rows using the approach already used by the project. Keep this data clearly development-only.

### Checkpoint

You should be able to inspect the database and see active and inactive depot rows. If the database is empty, the endpoint cannot prove that its filtering works.

## 9. Implement the concrete depot service

Create the service implementation in Infrastructure, for example:

```text
BusBookingSystem.Infrastructure/Services/DepotService.cs
```

The class should:

1. Implement `IDepotService`.
2. Inject `AppDbContext` through its constructor.
3. Query `context.Depots`.
4. Filter to `IsActive` rows.
5. Project only the fields required by `DepotListDto`.
6. Execute the query asynchronously.
7. Return the list.

The data flow should look like this:

```text
AppDbContext.Depots
  -> Where(IsActive)
  -> Select(DepotListDto fields)
  -> ToListAsync()
  -> return to controller
```

Do not do these things:

- return `Depot` entities,
- return the `Address` navigation object,
- load every property and map everything automatically,
- put the query in the controller.

The mapping should be explicit so you can see exactly why every response field exists.

### Handling the address display value

Your current `Depot` model contains an `Address` property. The DTO needs a string display value. Choose the appropriate field from `Address` and project it into `DisplayLocation` or your chosen property.

If the address model is not ready yet, use the simplest display value available for this slice and record the limitation. Do not expand the DTO with the entire address object just to avoid deciding on one display field.

### Checkpoint

Build Infrastructure. The service should compile against the Application interface and Core entity, using `AppDbContext` for data access.

## 10. Register the service with dependency injection

Open the API startup file:

```text
BusBookingSystem.API/Program.cs
```

Register the interface and its Infrastructure implementation:

```text
IDepotService -> DepotService
```

Use the lifetime already chosen for your other application services. For a service that uses a scoped EF Core context, a scoped service is the normal choice.

The registration matters because the controller will request `IDepotService`, while dependency injection must know which concrete class to create.

### Checkpoint

Run the API. If startup fails with “unable to resolve service,” check:

- the Application project references,
- the Infrastructure project reference,
- the namespace in the registration,
- the service constructor dependencies,
- the `AppDbContext` registration.

Do not bypass the problem by injecting `AppDbContext` into the controller. The controller should remain dependent on the service contract.

## 11. Create the controller endpoint

Create:

```text
BusBookingSystem.API/Controllers/DepotsController.cs
```

The controller should:

1. Be an API controller.
2. Use the route `api/[controller]` or the project convention.
3. Inject `IDepotService`.
4. Add a `GET` action.
5. Call `GetActiveDepotsAsync()`.
6. Return the resulting list.

The request flow should now be:

```text
HTTP GET /api/depots
  -> DepotsController
  -> IDepotService.GetActiveDepotsAsync()
  -> DepotService
  -> AppDbContext
  -> DepotListDto list
  -> JSON response
```

The controller should not:

- inject `AppDbContext`,
- contain `Where` or `Select` database queries,
- return `Depot` entities,
- decide which depots are active.

### Checkpoint

Build and run the API. Swagger should list `GET /api/depots`. If it does not appear, check that the controller has the correct attributes and is in the API project.

## 12. Test the feature manually in Swagger

Open the Swagger URL shown by the API when it starts.

Find `GET /api/depots` and execute it.

### Test 1: active records

Expected result:

- HTTP success response,
- active depots are present,
- each item has an ID, code, name, and display value.

### Test 2: inactive records

Expected result:

- the inactive seeded depot does not appear.

If it appears, the filtering is either missing or being applied to the wrong property.

### Test 3: no active records

Temporarily use a database state with no active depots, or add a test setup that creates that state.

Expected result:

```json
[]
```

An empty result is a valid search-form state. It should not be treated as a server failure.

### Test 4: response shape

Inspect the JSON response.

Confirm that it contains only the fields in `DepotListDto`. It should not contain:

- the full `Address` object,
- EF navigation properties,
- audit fields that were not selected,
- unrelated entity fields.

## 13. Add focused automated tests

If a test project already exists, add tests for this feature there.

If it does not exist yet, do not get stuck creating a complete testing architecture today. Record these cases and verify the feature manually through Swagger. The 9-day plan reserves Day 8 for creating and expanding the automated test suite.

When the test project is available, add tests for real behavior:

1. `GetActiveDepotsAsync` returns active depots.
2. `GetActiveDepotsAsync` excludes inactive depots.
3. No active depots returns an empty collection.
4. The projection maps the expected ID and display values.

Use the database-testing approach chosen for this solution. The test should exercise active filtering and DTO projection, rather than merely checking that an internal method was called.

### Checkpoint

At least the manual acceptance criteria must pass today. Automated tests can be added on Day 8 if the test project is not ready yet, but the cases must be written down so they are not forgotten.

## 14. Troubleshooting guide

### The service cannot be resolved

Check that:

- `IDepotService` is registered,
- `DepotService` implements the exact interface,
- the API references Infrastructure,
- the constructor dependencies are registered.

### The table does not exist

The migration was probably not created or applied. Check the migration command, connection string, and database name.

### The endpoint returns an empty list unexpectedly

Check:

- the connection string points to the database you seeded,
- seed data was actually inserted,
- `IsActive` is true for the expected rows,
- the query is using the correct context.

### The address projection fails

Check the actual `Address` entity property names. Use a simple scalar address field for this list response rather than returning the navigation object.

### Swagger does not show the endpoint

Check the controller namespace, controller name, API attributes, route, and whether the API project builds successfully.

## 15. Day 2 completion checkpoint

Day 2 is complete when you can demonstrate this exact path:

```text
Run API
  -> open Swagger
  -> execute GET /api/depots
  -> see active depot choices
  -> confirm inactive depots are hidden
  -> inspect the focused DepotListDto response
  -> confirm the service, not the controller, queried the data
  -> record the automated test cases for Day 8 if the test project is not ready
```

The completed slice should leave behind:

- `DepotListDto` in Application,
- `IDepotService.GetActiveDepotsAsync()` in Application,
- a concrete `DepotService` in Infrastructure,
- direct `AppDbContext` access in that service,
- `DepotsController` in API,
- development seed data,
- manual Swagger verification,
- focused test cases, automated now or scheduled for Day 8.

That is the definition of done for this day.

## 16. Handoff to Day 3

Day 3 uses the depot IDs returned by this endpoint.

The next user story is:

> As a customer, I want to search for trips between two depots on a date so that I can choose a journey.

Day 3 will repeat the same workflow:

```text
User story
  -> acceptance criteria
  -> use case
  -> TripSearchResultDto
  -> ITripService.SearchTripsAsync(...)
  -> AppDbContext query
  -> TripsController
  -> Swagger and automated tests
```

Do not begin Day 3 until you can call `GET /api/depots` and explain what each layer contributed to the working feature.
