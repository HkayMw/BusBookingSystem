# Day 5 Walkthrough - Create a Booking End to End

Day 5 is the booking feature slice.

The goal is to complete one customer-visible flow from idea to working API:

```text
User story
  -> acceptance criteria
  -> request contract
  -> booking DTOs
  -> service contract
  -> direct AppDbContext access
  -> API endpoint
  -> validation and transaction handling
  -> manual verification
```

This repo is still using the lightweight layered pattern already established by earlier slices:

```text
Core entity
  -> Application DTO + service interface
  -> Infrastructure AppDbContext + concrete service
  -> API controller
```

Do not add a repository layer here unless the project later proves it is worth the extra abstraction. The project handoff explicitly says not to add `IBookingRepository` for this MVP.

## 1. Understand the feature before coding

### User story

> As a customer, I want to book seats on a selected trip so that my seats are reserved.

This feature is more valuable and more business-critical than the earlier reading-only slices.

### Do not build these features today

- authentication
- payment processing
- route management
- bus management
- trip creation admin screens

Those belong to separate later slices.

### Acceptance criteria

The feature is complete when all of the following are true:

- a booking request exists for a selected trip and number of seats,
- the request is validated before writing to the database,
- the trip exists,
- the trip still has enough available seats,
- the booking is created with a unique booking reference,
- the trip’s booked seats are decremented or otherwise marked reserved,
- the booking is persisted in a transaction,
- overbooking is rejected,
- unsuccessful requests return a clear API failure response,
- a success response returns enough information for the customer to see the result.

## 2. Confirm the current project pattern

Before creating anything new, confirm the project still follows:

```text
Core: domain entities and enums
Application: DTOs + service interfaces
Infrastructure: AppDbContext + concrete service implementations
API: controller + dependency injection
```

The direct `AppDbContext` approach is the expected pattern for this project, and the handoff explicitly says not to add `IBookingRepository`.

## 3. Confirm the current domain model

Before writing booking logic, inspect the actual current entity definitions.

The relevant existing entities include:

- `Trip`
- `Booking`
- `User`
- `Bus`
- `Route`
- `Depot`

The current repo already has a booking entity in:

```text
BusBookingSystem.Core/Entities/Booking.cs
```

The current booking entity is conceptually:

```csharp
public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required User User { get; set; }
    public Guid TripId { get; set; }
    public required Trip Trip { get; set; }
    public required int NumberOfSeats { get; set; }
    public required string BookingReference { get; set; }
    public BookingStatus BookingStatus { get; set; }
}
```

This shows that a booking is centered on:

- a customer/user,
- a selected trip,
- a seat count,
- a booking reference,
- status.

## 4. Define the request and response contract

The first contract is the booking request DTO.

It should be a focused request for this flow, for example:

```csharp
public class CreateBookingRequestDto
{
    public Guid TripId { get; set; }
    public Guid UserId { get; set; }
    public int NumberOfSeats { get; set; }
}
```

The response DTO should return only what the customer or client needs, such as:

```csharp
public class BookingResponseDto
{
    public Guid BookingId { get; set; }
    public string BookingReference { get; set; } = default!;
    public string TripSummary { get; set; } = default!;
    public int NumberOfSeats { get; set; }
    public decimal TotalFare { get; set; }
    public string Status { get; set; } = default!;
}
```

### Important rule

Do not return the full `Booking` entity or all of its navigation objects. Keep the response focused on the customer-facing outcome.

## 5. Define the service contract

The Day 5 service contract is conceptually:

```csharp
public interface IBookingService
{
    Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto request);
}
```

If the implementation needs a richer result or a separate error contract, add it only when the feature requires it.

This is not yet the time for a complex result pattern unless the project already has one.

## 6. Implement the booking logic in Infrastructure

The concrete service should live in:

```text
BusBookingSystem.Infrastructure/Services/BookingService.cs
```

The service should:

1. accept `AppDbContext` in the constructor,
2. load the selected trip with its bus and route,
3. validate the request,
4. check trip availability,
5. reject overbooking,
6. generate a booking reference,
7. create the booking record,
8. update the trip seat counts,
9. save the changes within a transaction,
10. return the response DTO.

### Minimal business rules

At least these rules should exist:

- trip id is required,
- user id is required,
- seat count is required and greater than zero,
- trip must exist,
- trip must still have enough seats,
- booking creation must be atomic.

### Availability calculation

Use the actual `Trip` and `Bus` fields already in the repo.

The current model uses:

- `Trip.SeatsBooked`
- `Bus.NumberOfSeats`

So the available seat calculation is conceptually:

```csharp
var availableSeats = trip.Bus.NumberOfSeats - trip.SeatsBooked;
```

If the request needs more than one seat, ensure:

```csharp
requestedSeats <= availableSeats
```

## 7. Use a transaction

The booking flow is a classic write transaction.

Wrap the mutation in a transaction so that both steps happen together:

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
```

Then:

- create booking row,
- update trip seat count,
- save changes,
- commit transaction.

If the seat check fails, do not write anything.

## 8. Create a booking reference

The model includes:

```csharp
public required string BookingReference { get; set; }
```

Generate a unique code using a simple, deterministic pattern such as:

```text
BK-XXXXXX
```

The exact pattern can follow project convention, but it must be unique and stable enough for a booking record.

### Good implementation rule

Use a generated reference and store it on the booking record. Do not leave it blank.

## 9. Controller endpoint

Create:

```text
BusBookingSystem/Controllers/BookingsController.cs
```

The controller should:

- inject `IBookingService`,
- expose a POST endpoint,
- accept a booking request DTO,
- return `Ok(response)` on success,
- return a validation or failure response on bad requests.

Conceptually:

```csharp
[HttpPost]
public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto request)
{
    var booking = await _bookingService.CreateBookingAsync(request);
    return Ok(booking);
}
```

### Controller rules

The controller should not:

- query the database directly,
- decide whether a trip is valid,
- manipulate trip seat counts itself,
- return raw `Booking` entities.

## 10. Validation and failure behavior

This is a key Day 5 requirement.

Before writing, validate:

- trip id not empty,
- user id not empty,
- number of seats > 0,
- trip exists,
- seats available.

On failure, return a clear API error or validation response. Keep it simple, but do not silently pretend the booking succeeded.

### Example failure cases

- trip not found
- not enough seats
- zero or negative seat count
- invalid user id

## 11. Manual verification in Swagger or Scalar

Run the app and exercise the booking endpoint.

### Test 1: valid booking

Use a valid trip and seat count.

Expected result:

- HTTP 200 or 201
- booking created
- booking reference present
- response contains summary data

### Test 2: overbooking

Request more seats than are available.

Expected result:

- clear failure response
- no booking created
- no seat decrement

### Test 3: invalid seat count

Use `0` or a negative number.

Expected result:

- validation error
- no booking created

### Test 4: invalid trip id

Use a non-existent trip id.

Expected result:

- clear not-found or validation error
- empty booking creation

## 12. Focused automated tests

If a test project is available, these are the highest-value tests:

1. creates booking successfully when seats are available,
2. rejects booking when requested seats exceed availability,
3. creates and stores a booking reference,
4. decrements trip booked seats as part of the same transaction,
5. rejects invalid seat quantities,
6. returns a clear failure when the trip is missing.

Use real DB or EF in-memory behavior tests instead of mock-only assertions.

## 13. Keep the scope disciplined

Day 5 should not drift into:

- auth flows,
- payment integration,
- email confirmation,
- advanced seat locking,
- generic booking management UI.

This day is specifically about creating a booking record and protecting availability.

## 14. Summary

Day 5 is the booking slice:

- request DTO and response DTO,
- `IBookingService` contract,
- direct `AppDbContext` implementation,
- validation and availability check,
- atomic booking creation,
- unique booking reference,
- controller endpoint,
- Swagger verification,
- focused tests.

This is exactly the kind of service-first, direct-DbContext workflow this repo is designed for while staying inside the MVP boundary.
