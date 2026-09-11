# Day 2 Walkthrough — Application Layer: Read-Side Contracts

This is the day-2 implementation pass for the 9-day MVP plan.

Goal for today:
- Define the DTOs the API will return.
- Define the service interfaces the API will depend on.
- Keep the Application project focused on contracts, not database logic.
- Align with the simpler design: Infrastructure uses AppDbContext directly rather than repository classes.

Important rule for today:
- Do not implement the database or EF Core yet.
- Do not build endpoints yet.
- Do not add business logic like validation rules or booking logic.
- Keep this day limited to contracts and mapping boundaries.

---

## 1) Confirm the Day 1 foundation

Before writing any Application code, confirm the Core layer contains the domain objects you agreed on:

- Depot
- Bus
- Address
- Route
- Trip
- Booking
- User
- BookingStatus enum

Your Day 1 entities are the source of truth. The Application layer should only describe what data can be requested and what operations are available.

Check these assumptions before starting:
- A trip connects to a Bus and a Route.
- A route connects origin and destination depots.
- A booking is attached to a user and a trip.
- The Day 1 model is intentionally simple and does not include a full UI or EF setup.

If the Core model is still incomplete, fix that before moving forward. Do not start the Application layer on shaky domain objects.

---

## 2) Create the Application project structure

Inside the Application project, keep the organization clean:

- DTOs/
- Interfaces/
  - Services/

This keeps the project readable and consistent with the layered architecture:

Core -> Application -> Infrastructure -> API

The Application project should only reference Core. It should not reference Infrastructure or API.

If you do not yet have the folders, create them manually.

---

## 3) Add the DTOs

Create DTO models that represent the shape of the data returned from the system.

A DTO is not just a copy of the entity. It is a response contract for the API and the consumer. That is why it usually includes an Id: the caller needs a stable identifier to route to a specific item, link to a detail screen, or use in a later request.

The Id is not about persistence logic; it is about identity in the API contract.

### 3.1 DepotDto
Purpose: return the data needed to list or detail a depot.

Include fields such as:
- Id
- DepotCode
- Name
- Address
- PhoneNumber
- Latitude
- Longitude
- IsActive
- CreatedAt
- UpdatedAt

Why include the Id:
- the client needs to know which depot was selected,
- later operations may fetch the depot by Id,
- it matches the natural API pattern of object identifiers in responses.

### 3.2 BusDto
Purpose: return the bus fleet data used in listings and trip details.

Include fields such as:
- Id
- FleetNumber
- Model
- Capacity
- RegistrationNumber
- ManufacturerName
- ManufactureYear
- IsAvailable
- IsActive
- CreatedAt
- UpdatedAt

Why include the Id:
- the client may need to show or filter a specific bus,
- trip and schedule responses often identify the bus with its Id,
- it keeps the API consistent with other resource-style responses.

### 3.3 RouteDto
Purpose: return a route definition so the client can browse source-destination combinations.

Include fields such as:
- Id
- Name
- OriginDepotId
- DestinationDepotId
- OriginDepotName
- DestinationDepotName
- CreatedAt
- UpdatedAt

Why include the Id:
- route selection is usually based on a route record,
- the client may need to click into or reference a specific route,
- the Id makes a route usable as a first-class API resource.

### 3.4 TripDto
Purpose: return the most common object the client will show when browsing schedules and availability.

Include fields such as:
- Id
- BusId
- RouteId
- BusName or FleetNumber
- OriginDepotId
- DestinationDepotId
- DepartureTime
- ArrivalTime
- BaseFare
- AvailableSeats
- TotalSeats
- CreatedAt
- UpdatedAt

Why include the Id:
- the client must select a specific trip,
- booking requests will normally reference the trip by Id,
- trip search results are only useful if they can be identified as unique records.

### 3.5 TripSearchResultDto
Purpose: represent one result from a trip-search query.

Include fields such as:
- Id
- RouteId
- OriginDepotId
- DestinationDepotId
- OriginDepotName
- DestinationDepotName
- DepartureTime
- ArrivalTime
- BaseFare
- AvailableSeats
- BusModel
- BusCapacity

Why include the Id:
- the caller will need to choose which trip to book,
- this DTO is the search result contract, not the full entity,
- the Id is the key value that makes the search result actionable.

### 3.6 DTO design guidance

Keep these points in mind:
- Use simple properties, not nested domain objects.
- Keep naming consistent: Id, Name, CreatedAt, etc.
- Prefer plain C# types that are easy to serialize to JSON.
- Do not include logic in the DTOs.
- Keep each DTO focused on one purpose: list, detail, or search result.

A DTO is a contract, not a business object.

---

## 4) Keep the data access boundary simple

For this project, we are not using repository classes. The simpler design is:
- Application layer defines service contracts.
- Infrastructure layer owns AppDbContext.
- The service implementation uses DbContext directly to query data.

This is the preferred approach unless there is a strong reason to introduce repository abstractions later.

The key principle is: do not add a custom wrapper around DbContext unless the project grows enough to justify it. A repository is usually just another abstraction layer on top of another abstraction layer.

In practice, that means:
- no repository interfaces
- no repository classes
- no repository folder in the Application project
- instead, the service implementation injects AppDbContext and calls DbSet queries directly

This keeps the codebase lighter and easier to move through in the 9-day build window.

The Application layer should not know about EF Core types or DbContext. It only knows the service interface contract.

---

## 5) Create service interfaces

The service interfaces live under:
- Interfaces/Services/

These are the contracts the API controllers will call later.

Each interface should reflect one clear responsibility. Do not make a service interface a dumping ground for every query and operation. The purpose should be obvious from the method names.

### 5.1 IDepotService
Purpose: expose depot read operations used by the API.

Define methods such as:
- GetAllAsync()
- GetByIdAsync(Guid id)

Why this interface exists:
- the controller should depend on a depot service contract,
- the implementation can query AppDbContext and return DTOs,
- it gives the API a stable, testable boundary.

### 5.2 IBusService
Purpose: expose bus read operations needed for fleet queries and trip display.

Define methods such as:
- GetAllAsync()
- GetByIdAsync(Guid id)

Why this interface exists:
- the API can ask for bus data without directly depending on EF Core,
- it separates the controller from data-access details,
- it keeps the business-facing contract easier to test and reason about.

### 5.3 ITripService
Purpose: expose trip discovery and trip detail operations.

Define methods like:
- GetAllAsync()
- GetByIdAsync(Guid id)
- SearchTripsAsync(Guid originDepotId, Guid destinationDepotId, DateTime date)

Why this interface exists:
- trip search is the key read feature of the booking flow,
- the API needs a simple contract for browsing trips by origin, destination, and date,
- the implementation can map the raw query results into TripDto or TripSearchResultDto.

### 5.4 Service responsibilities for Day 2

Keep service interfaces thin and readable.

They should orchestrate the data flow, for example:
- Service receives a request from the controller.
- Service queries AppDbContext in Infrastructure.
- Service maps the entity results to DTOs.
- Controller receives the DTOs.

This is the day to define the orchestration boundary without implementing the logic.

---

## 6) Decide whether to add service implementations yet

For Day 2, the plan says: "Service interfaces + implementations: IDepotService, IBusService, ITripService — thin orchestration, manual DTO mapping (skip AutoMapper, one less thing to debug this week)."

That means today you can do both:
- Create the interfaces in Application.
- Create the implementations in Infrastructure, using AppDbContext directly.

The safe and simple version is:
- Put service interfaces in Application.
- Put service implementations in Infrastructure.

This is clean because:
- the service interface describes the use case,
- the Infrastructure implementation contains the actual EF queries,
- the controller depends only on the interface and the DTO contract.

If you want to keep the build clean and low-risk, do this:
- Create interfaces only on Day 2.
- Stub out implementation classes later if needed.

The main requirement is the contracts exist clearly and are named correctly.

---

## 7) Avoid AutoMapper today

The plan says to skip AutoMapper for now.

This is intentional.

Reasons:
- One less dependency to configure.
- One less thing to debug.
- Keep mapping manual and explicit.
- Easier to understand in a short MVP sprint.

For each service, map in a straightforward way:
- Domain entity -> DTO
- Query result -> response DTO

Do not create a configuration file or model mapping registry today. Just write the mapping as regular code in small service methods when the implementation is added.

---

## 8) Keep the architecture clean

At the end of Day 2, your project should be organized like this:

- BusBookingSystem.Core
  - Entities/
  - Enums/

- BusBookingSystem.Application
  - DTOs/
  - Interfaces/
    - Services/

- BusBookingSystem.Infrastructure
  - Data/
  - AppDbContext.cs
  - Services/

- BusBookingSystem.API
  - Controllers/
  - Program.cs

This separation matters because:
- Core contains business concepts.
- Application defines the use-case contracts and response DTOs.
- Infrastructure handles data access with AppDbContext.
- API handles HTTP endpoints.

Day 2 is where you lock in that boundary.

---

## 9) Compile check after the contract layer exists

Once the DTOs and interfaces are in place, do a build check.

The goal is not to have working endpoints yet. The goal is to verify that:
- the Application project compiles cleanly,
- it references Core without issue,
- there are no missing namespaces or broken references,
- the project is ready for Infrastructure implementation on Day 3.

If the build fails, fix the compile issues before moving on.

---

## 10) What “done” looks like for Day 2

By the end of the day, you should be able to confidently say:

- I defined the DTOs for depots, buses, routes, and trips.
- I explained each DTO’s purpose and why it includes an Id.
- I created the service interfaces for the read-side API contract.
- I removed the repository abstraction because DbContext is the simpler and sufficient data-access layer here.
- I clarified the Application layer boundary.
- I kept the design simple and free of database or API code.

This is the contract layer that Day 3 will build on.

---

## 11) Day 2 checklist

Use this as a quick completion checklist:

- [ ] Core entities reviewed and confirmed
- [ ] DTO classes created in Application/DTOs
- [ ] DepotDto purpose explained and created
- [ ] BusDto purpose explained and created
- [ ] RouteDto purpose explained and created
- [ ] TripDto purpose explained and created
- [ ] TripSearchResultDto purpose explained and created
- [ ] Service interfaces created
- [ ] IDepotService created
- [ ] IBusService created
- [ ] ITripService created
- [ ] AppDbContext-first design confirmed
- [ ] No repository abstraction added
- [ ] Build passes
- [ ] No Infrastructure or API code added yet

---

## 12) Tomorrow’s handoff to Day 3

On Day 3, you will move into Infrastructure and implement the actual data access layer:
- Add EF Core SQL Server packages
- Create AppDbContext
- Add DbSets for depots, buses, routes, trips
- Implement service classes using AppDbContext directly
- Connect the service interfaces to actual database queries

Day 2 is the contract foundation that allows Day 3 to be clean, predictable, and easy to test.

Do not rush into data access without this boundary in place.

---

## 4) Keep the data access boundary simple

For this project, we are not using repository classes. The simpler design is:
- Application layer defines service contracts.
- Infrastructure layer owns AppDbContext.
- The service implementation uses DbContext directly to query data.

This is the preferred approach unless there is a strong reason to introduce repository abstractions later.

In practice, that means:
- no IDepotRepository
- no IBusRepository
- no IRouteRepository
- no ITripRepository
- instead, the service implementation can inject AppDbContext and call DbSet queries directly

This keeps the codebase lighter and easier to move through in the 9-day build window.

The Application layer should not know about EF Core types or DbContext. It only knows the service interface contract.

---

## 5) Create service interfaces

The service interfaces live under:
- Interfaces/Services/

These are the contracts the API controllers will call later.

### 5.1 IDepotService

Define methods such as:
- GetAllAsync()
- GetByIdAsync(Guid id)

### 5.2 IBusService

Define methods such as:
- GetAllAsync()
- GetByIdAsync(Guid id)

### 5.3 ITripService

This is the key service interface for Day 2.

Define methods like:
- GetAllAsync()
- GetByIdAsync(Guid id)
- SearchTripsAsync(Guid originDepotId, Guid destinationDepotId, DateTime date)

This is exactly the contract the API will implement later.

### 5.4 Service responsibilities for Day 2

Keep service interfaces thin and readable.

They should orchestrate the data flow, for example:
- Service receives a request from the controller.
- Service queries AppDbContext in Infrastructure.
- Service maps the entity results to DTOs.
- Controller receives the DTOs.

This is the day to define the orchestration boundary without implementing the logic.

---

## 6) Decide whether to add service implementations yet

For Day 2, the plan says: "Service interfaces + implementations: IDepotService, IBusService, ITripService — thin orchestration, manual DTO mapping (skip AutoMapper, one less thing to debug this week)."

That means today you can do both:
- Create the interfaces in Application.
- Create the implementations in Infrastructure or Application, depending on your project structure.

The safe and simple version is:
- Put service interfaces in Application.
- Put service implementations in Infrastructure or a dedicated Application implementation folder if you prefer.

Since the plan explicitly says "Application layer: read-side contracts" and the work is primarily interface definition, many people keep this day focused on contracts only and defer heavy implementation to Day 3 or Day 4.

If you want to keep the build clean and low-risk, do this:
- Create interfaces only on Day 2.
- Stub out implementation classes later if needed.

The main requirement is the contracts exist clearly and are named correctly.

---

## 7) Avoid AutoMapper today

The plan says to skip AutoMapper for now.

This is intentional.

Reasons:
- One less dependency to configure.
- One less thing to debug.
- Keep mapping manual and explicit.
- Easier to understand in a short MVP sprint.

For each service, map in a straightforward way:
- Domain entity -> DTO
- Repository result -> output DTO

Do not create a configuration file or model mapping registry today. Just write the mapping as regular code in small service methods when the implementation is added.

---

## 8) Keep the architecture clean

At the end of Day 2, your project should be organized like this:

- BusBookingSystem.Core
  - Entities/
  - Enums/

- BusBookingSystem.Application
  - DTOs/
  - Interfaces/
    - Services/

- BusBookingSystem.Infrastructure
  - Data/
  - AppDbContext.cs
  - Services/

- BusBookingSystem.API
  - Controllers/
  - Program.cs

This separation matters because:
- Core contains business concepts.
- Application defines use cases and contracts.
- Infrastructure does storage and implementation.
- API handles HTTP endpoints.

Day 2 is where you lock in that boundary.

---

## 9) Compile check after the contract layer exists

Once the DTOs and interfaces are in place, do a build check.

The goal is not to have working endpoints yet. The goal is to verify that:
- the Application project compiles cleanly,
- it references Core without issue,
- there are no missing namespaces or broken references,
- the project is ready for Infrastructure implementation on Day 3.

If the build fails, fix the compile issues before moving on.

---

## 10) What “done” looks like for Day 2

By the end of the day, you should be able to confidently say:

- I defined the DTOs for depots, buses, routes, and trips.
- I created the repository contracts for read-side access.
- I created the service contracts for trip search and retrieval.
- I clarified the Application layer boundary.
- I kept the design simple and free of database or API code.

This is the contract layer that Day 3 will build on.

---

## 11) Day 2 checklist

Use this as a quick completion checklist:

- [ ] Core entities reviewed and confirmed
- [ ] DTO classes created in Application/DTOs
- [ ] DepotDto created
- [ ] BusDto created
- [ ] RouteDto created
- [ ] TripDto created
- [ ] TripSearchResultDto created
- [ ] Service interfaces created
- [ ] IDepotService created
- [ ] IBusService created
- [ ] ITripService created
- [ ] AppDbContext approach confirmed instead of repository classes
- [ ] Build passes
- [ ] No Infrastructure or API code added yet

---

## 12) Tomorrow’s handoff to Day 3

On Day 3, you will move into Infrastructure and implement the actual data access layer:
- Add EF Core SQL Server packages
- Create AppDbContext
- Add DbSets for depots, buses, routes, trips
- Implement repository classes
- Connect the repository interfaces to actual database queries

Day 2 is the contract foundation that allows Day 3 to be clean, predictable, and easy to test.

Do not rush into data access without this boundary in place.
