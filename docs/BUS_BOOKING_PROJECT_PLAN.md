# BUS BOOKING SYSTEM — C# Project Plan

## From Idea to Production (14 weeks)

**Your stack:** C# + ASP.NET Core 10 + SQL Server + Blazor  
**Your time:** 2.5 hrs/day (Mon-Fri) + 4 hrs Saturday (learning) = 15-17 hrs/week  
**Your goal:** Production-ready bus booking API + web UI by Week 14

---

## **PART 1: HOW TO THINK THROUGH A BUS BOOKING SYSTEM**

### **What is a Bus Booking System?**

Users should be able to:

1. **Browse trips** (routes, schedules, depots, and prices)
2. **Check availability** (segment-aware seats)
3. **Book seats** (select passengers and seats)
4. **Pay** (provider checkout and webhooks)
5. **Get confirmation** (email/SMS and booking reference)
6. **Manage bookings** (view, cancel, and reschedule)

Admins and operators manage buses, depots, routes, schedules, trips, bookings, payments, and users.

### **Roadmap at a Glance**

**Key principle:** Each session builds one small, testable piece. Friday is a catch-up buffer; Saturday is for learning, review, and practice.

<!-- Misplaced duplicate schedule; the authoritative schedule appears after Part 2. -->
<!--
### **WEEK 1: SOLUTION AND DOMAIN ORIENTATION**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Create the solution and five projects | Solution | `.sln`, Core, Application, Infrastructure, API, and Web |
| Tuesday | Add project references and build | Solution | Correct dependency direction |
| Wednesday | Learn classes, properties, `Guid`, nullable types, and enums | Core | Small practice classes compile |
| Thursday | Create `User`, `Customer`, and `Depot` | Core | Identity and location entities |
| Friday | Catch up, fix compile errors, and commit | Solution | Working foundation commit |
| Saturday | Learn LINQ filtering and projection | Core | Five simple list queries |

**Checkpoint:** You can explain each project and why Core has no web or database dependency.

### **WEEK 2: REMAINING DOMAIN ENTITIES**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Create `Bus` and `BusSeat` | Core | Fleet and seat inventory |
| Tuesday | Create `Route` and `RouteStop` | Core | Ordered route with at least two depots |
| Wednesday | Create `Schedule` and `Trip` | Core | Recurring service and dated departure |
| Thursday | Create `Passenger`, `Booking`, and `BookingItem` | Core | Passenger and segment booking relationships |
| Friday | Create `Payment`, `Notification`, `AuditLog`, and enums | Core | Complete domain model |
| Saturday | Review relationships and practice LINQ | Core | Queries from trips to depots |

**Checkpoint:** All entities compile with the agreed key, time, and money types.

### **WEEK 3: APPLICATION CONTRACTS AND DEPENDENCY INJECTION**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Learn interfaces and dependency inversion | Application | Interface versus implementation notes |
| Tuesday | Create search, trip, booking, and payment DTOs | Application | Request and response contracts |
| Wednesday | Create service interfaces | Application | Depot, route, trip, booking, payment, notification interfaces |
| Thursday | Create validators and result/error models | Application | Predictable validation failures |
| Friday | Register and call one sample service | API | First dependency injection example |
| Saturday | Review DI, DTOs, and async/await | Application | One tested practice use case |

**Checkpoint:** A controller depends on an interface, not a concrete service or database.

### **WEEK 4: EF CORE AND SQL SERVER BASICS**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Learn `DbContext`, `DbSet`, and EF Core tracking | Infrastructure | EF Core notes and example |
| Tuesday | Install EF Core SQL Server packages | Infrastructure | Packages and connection settings |
| Wednesday | Create `ApplicationDbContext` | Infrastructure | `DbSet` for each entity |
| Thursday | Configure `Depot` with Fluent API | Infrastructure | First entity configuration |
| Friday | Add configurations for User, Customer, Bus, and BusSeat | Infrastructure | Keys, lengths, and relationships |
| Saturday | Create and apply the first migration | Infrastructure | Database created from code |

**Checkpoint:** You can save and retrieve a depot from SQL Server.

### **WEEK 5: MIGRATIONS, CONSTRAINTS, AND SEED DATA**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Configure routes and ordered route stops | Infrastructure | Stop sequence and unique indexes |
| Tuesday | Configure schedules and trips | Infrastructure | Dates, statuses, and concurrency token |
| Wednesday | Configure bookings, passengers, and booking items | Infrastructure | Foreign keys and booking indexes |
| Thursday | Configure payments, notifications, and audit logs | Infrastructure | Money precision and audit table |
| Friday | Add development seed data | Infrastructure | Depots, buses, routes, and trips to test with |
| Saturday | Test invalid data and review generated SQL | Infrastructure | Constraint failures are understood |

**Checkpoint:** A fresh database can be created entirely from migrations.

### **WEEK 6: API READ FEATURES**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Create depot and bus read services | Infrastructure | Read use cases with EF Core |
| Tuesday | Create `DepotsController` and `BusesController` | API | `GET /api/depots` and `GET /api/buses` |
| Wednesday | Create route read service and controller | API/Infrastructure | Routes include ordered stops |
| Thursday | Create schedule and trip read services | API/Infrastructure | Searchable dated trips |
| Friday | Register services and test with Swagger | API | Dependencies resolve and endpoints respond |
| Saturday | Practice GET requests and response DTOs | API | Saved Swagger/Postman requests |

**Checkpoint:** The API reads real database data without controllers using `DbContext`.

### **WEEK 7: API BOOKING FEATURES**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Add trip search by boarding depot, destination depot, and date | API | Segment-aware search endpoint |
| Tuesday | Create booking request validation | Application/API | Invalid stop and passenger data rejected |
| Wednesday | Create booking and availability endpoints | API | Booking details and available seats |
| Thursday | Add pagination, filtering, sorting, and ProblemDetails | API | Consistent manageable responses |
| Friday | Catch up and write search/booking integration tests | API | Invalid route and valid booking scenarios |
| Saturday | Practice debugging requests in Swagger/Postman | API | Test collection with expected results |

**Checkpoint:** Swagger can search trips and send a valid booking request.

### **WEEK 8: SEAT INVENTORY AND CONCURRENCY**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Calculate fares between route stops | Application | Segment fare calculation |
| Tuesday | Query seats available across overlapping segments | Application | Correct inventory result |
| Wednesday | Add expiring seat holds | Application/Infrastructure | Hold expiry and release behavior |
| Thursday | Wrap booking creation in a database transaction | Infrastructure | Normal double-booking protection |
| Friday | Catch up and test cancellation/rescheduling inventory | API/Application | Seats release or re-check correctly |
| Saturday | Learn transactions, isolation, and race conditions | Infrastructure | Explain the race being prevented |

**Checkpoint:** Two overlapping bookings cannot reserve the same seat.

### **WEEK 9: PAYMENTS, WEBHOOKS, REFUNDS, AND AUDIT**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Create the payment-provider adapter interface and fake provider | Infrastructure | Testable provider boundary |
| Tuesday | Add payment initiation and provider transaction IDs | API/Infrastructure | No raw card data stored |
| Wednesday | Add webhook signature verification and idempotency | API/Infrastructure | Safe repeated callbacks |
| Thursday | Add cancellation rules, refunds, and payment states | Application/Infrastructure | Correct refund amount and status |
| Friday | Add audit events for success, failure, and denial | Infrastructure | Complete action history |
| Saturday | Test payment failures, retries, refunds, and audit records | API/Infrastructure | Automated retry scenarios |

**Checkpoint:** A repeated payment webhook creates neither a second payment nor a second confirmation.

### **WEEK 10: NOTIFICATIONS, TESTING, AND AUTHORIZATION FOUNDATIONS**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Add notification queue and retry status | Infrastructure | Confirmation and cancellation notifications |
| Tuesday | Create unit tests for route and booking rules | Solution/Core | Domain invariant tests |
| Wednesday | Create API integration-test setup | Solution/API | Test server and database strategy |
| Thursday | Define role and ownership access matrix | Application/API | Customer, admin, operator, and driver rules |
| Friday | Add early authentication/authorization stubs | API | Initial `[Authorize]` boundaries |
| Saturday | Review failures and use the buffer session | Solution | Weak tests repaired before UI work |

**Checkpoint:** Tests and ownership rules exist before the UI is built.

### **WEEK 11: BLAZOR SEARCH AND BOOKING UI**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Configure `ApiClient` and API base URL | Web | Web app calls API over HTTP |
| Tuesday | Build trip search page | Web | Boarding depot, destination depot, and date inputs |
| Wednesday | Build trip cards and seat selector | Web | Stop times, fares, and availability |
| Thursday | Build passenger and booking form | Web | Passenger and accessibility details |
| Friday | Connect search, hold, and booking | Web | Selected segment reaches the API |
| Saturday | Review Blazor routing and component parameters | Web | Clean component boundaries |

**Checkpoint:** A user can search and place a seat hold in the browser.

### **WEEK 12: BLAZOR PAYMENT AND BOOKING MANAGEMENT UI**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Build payment form and payment state display | Web | Payment initiation and failure feedback |
| Tuesday | Build confirmation page | Web | Booking reference, passenger, seat, and fare |
| Wednesday | Build customer booking history | Web | Status and trip details |
| Thursday | Add cancellation and rescheduling UI | Web | Refund result and new trip selection |
| Friday | Add loading, validation, and API error states | Web | Clear feedback for every request state |
| Saturday | Test the complete browser workflow | Web | Search → hold → pay → confirm |

**Checkpoint:** A user can complete a booking using the real API from the browser.

### **WEEK 13: SECURITY, OPERATIONS, AND PRIVACY**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Configure ASP.NET Core Identity and login | API/Web | Secure user authentication |
| Tuesday | Complete role-based authorization | API/Web | Customer, admin, operator, and driver permissions |
| Wednesday | Add rate limiting and input validation | API | Abuse protection and safe requests |
| Thursday | Add health checks, structured logs, and metrics | API | Operational visibility |
| Friday | Add privacy, retention, and secret-management rules | Solution | No passwords, cards, or secrets in source control |
| Saturday | Test authorization and privacy boundaries | Solution | Users access only permitted data |

**Checkpoint:** Security is applied to existing endpoints before release preparation.

### **WEEK 14: DEPLOYMENT AND RELEASE BUFFER**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Configure development and production settings | API/Web | Environment-specific configuration |
| Tuesday | Add and test database backup and restore | Infrastructure | Recovery procedure |
| Wednesday | Configure CI and automated tests | Solution | Build and tests run on every push |
| Thursday | Publish the API and Blazor Web App | API/Web | Deployment packages |
| Friday | Run acceptance tests and fix remaining issues | Solution | Release checklist and go/no-go decision |
| Saturday | Documentation and final buffer session | Solution | README, architecture notes, and v1.0 preparation |

**By end of Week 14:** Production-ready release candidate + GitHub commit "Prepare v1.0 release".
-->

<!--
## **PART 3: MICRO-TASKS FOR YOUR 2.5-HOUR SESSIONS**

**Key principle:** Each session builds one small, testable piece. The plan keeps every production concept, but introduces it in smaller steps. Use Saturday for learning and review; use Friday as a catch-up buffer when a task takes longer.

### **WEEK 1: Solution and Domain Orientation**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Create the solution and five projects | Solution | `.sln`, Core, Application, Infrastructure, API, and Web |
| Tuesday | Add project references and build | Solution | Correct dependency direction and first successful build |
| Wednesday | Learn C# classes, properties, `Guid`, nullable types, and enums | Core | Small practice classes and enum examples |
| Thursday | Create `User`, `Customer`, and `Depot` | Core | Independent identity and location entities |
| Friday | Buffer, review, and commit | Solution | Fix compile errors; commit the working foundation |
| Saturday | Learn LINQ basics and practice with lists | Core | Five simple filtering and projection queries |

**Checkpoint:** You can explain what each project does and why Core does not reference the API or database.

### **WEEK 2: Remaining Domain Entities**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Create `Bus` and `BusSeat` | Core | Bus fleet and physical seat inventory |
| Tuesday | Create `Route` and `RouteStop` | Core | Ordered route with at least two depots |
| Wednesday | Create `Schedule` and `Trip` | Core | Recurring service and dated departure |
| Thursday | Create `Passenger`, `Booking`, and `BookingItem` | Core | Passenger and segment booking relationships |
| Friday | Create `Payment`, `Notification`, `AuditLog`, and enums | Core | Remaining entities and statuses |
| Saturday | Review entity relationships and practice LINQ | Core | Queries from trip to route stops and depots |

**Checkpoint:** All entities compile and use `Guid` keys, `DateTimeOffset` timestamps, `DateOnly`/`TimeOnly` dates and times, and `decimal` money values.

### **WEEK 3: Application Layer and Dependency Injection**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Learn interfaces and dependency inversion | Application | Understand interface versus implementation |
| Tuesday | Create request and response DTOs | Application | Search, booking, trip, and payment contracts |
| Wednesday | Create service interfaces | Application | Depot, route, trip, booking, payment, and notification interfaces |
| Thursday | Create validators and result/error models | Application | Validation rules and predictable failures |
| Friday | Register a simple service in API | API | First dependency injection example |
| Saturday | Review DI, DTOs, and async/await | Application | One small practice use case |

**Checkpoint:** You can explain why a controller depends on an interface instead of directly creating a service.

### **WEEK 4: EF CORE AND SQL SERVER**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Learn EF Core, `DbContext`, and `DbSet` | Infrastructure | Connect the concepts to the entity classes |
| Tuesday | Install EF Core SQL Server packages | Infrastructure | Packages and connection configuration |
| Wednesday | Create `ApplicationDbContext` | Infrastructure | `DbSet` properties for all entities |
| Thursday | Create one entity configuration | Infrastructure | Fluent API example for `Depot` |
| Friday | Add remaining configurations | Infrastructure | Keys, relationships, precision, indexes, and concurrency tokens |
| Saturday | Create and apply the first migration | Infrastructure | Database created from code |

**Checkpoint:** You can add a depot through EF Core and see it in SQL Server.

### **WEEK 5: Migrations, Constraints, and Seed Data**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Create migrations for users, depots, buses, and seats | Infrastructure | Reference tables |
| Tuesday | Create migrations for routes and route stops | Infrastructure | Ordered stop constraints and indexes |
| Wednesday | Create migrations for schedules and trips | Infrastructure | Dated service tables |
| Thursday | Create migrations for bookings, payments, and notifications | Infrastructure | Transactional tables and foreign keys |
| Friday | Add audit log and seed development data | Infrastructure | Safe test data and audit table |
| Saturday | Test invalid data and review SQL schema | Infrastructure | Confirm constraints reject bad routes and amounts |

**Checkpoint:** A fresh database can be created from migrations and contains realistic development data.

### **WEEKS 6-7: ASP.NET CORE WEB API**

| Week | Day | Task | Project | Output |
|---|---|---|---|---|
| Week 6 | Monday | Create depot and bus read endpoints | API | `GET /api/depots` and `GET /api/buses` |
| Week 6 | Tuesday | Create route endpoints with stop validation | API | Reject routes with fewer than two depots |
| Week 6 | Wednesday | Create schedule and trip endpoints | API | Searchable dated trips |
| Week 6 | Thursday | Implement read services with EF Core | Infrastructure | Controllers use interfaces, not `DbContext` |
| Week 6 | Friday | Configure DI, Swagger, and ProblemDetails | API | Consistent startup and error responses |
| Week 7 | Monday | Create booking endpoint with request validation | API | Valid booking request reaches the service |
| Week 7 | Tuesday | Add booking details and availability endpoints | API | Segment-aware availability |
| Week 7 | Wednesday | Add pagination, filtering, and sorting | API | Manageable trip search responses |
| Week 7 | Thursday | Add cancellation and rescheduling endpoints | API | Re-check inventory before changing a booking |
| Week 7 | Friday | Buffer and integration test review | API | Search, invalid route, and booking tests |
| Week 7 | Saturday | Practice testing endpoints with Swagger/Postman | API | Saved test requests and expected responses |

**Checkpoint:** Swagger can search trips and create a booking without a controller directly querying the database.

### **WEEK 8: BOOKING RULES AND SEAT INVENTORY**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Calculate fares between route stops | Application | Segment-based fare calculation |
| Tuesday | Implement seat availability for overlapping segments | Application | Correct available-seat query |
| Wednesday | Add expiring seat holds | Application/Infrastructure | Hold expiry and release behavior |
| Thursday | Add transactional booking creation | Infrastructure | Prevent double booking during normal requests |
| Friday | Buffer and test cancellation/rescheduling rules | API/Application | Inventory is released or rechecked |
| Saturday | Learn database transactions and isolation | Infrastructure | Explain the race condition being prevented |

**Checkpoint:** Two bookings cannot reserve the same seat for overlapping parts of a trip.

### **WEEK 9: PAYMENTS, NOTIFICATIONS, AND AUDIT**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Create the payment-provider adapter | Infrastructure | Provider interface and test implementation |
| Tuesday | Add payment initiation and provider transaction IDs | API/Infrastructure | No raw card details stored |
| Wednesday | Add webhook signature verification and idempotency | API/Infrastructure | Safe webhook retries |
| Thursday | Add refunds and cancellation payment rules | Application/Infrastructure | Correct refund state and amount |
| Friday | Add notification queue, retries, and audit events | Infrastructure | Email/SMS status and action history |
| Saturday | Test payment failures, retries, refunds, and audit records | API/Infrastructure | Automated scenarios and manual review |

**Checkpoint:** A repeated payment webhook does not create a second payment or confirmation.

### **WEEK 10: TESTING AND AUTHORIZATION FOUNDATIONS**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Create unit-test project and test domain rules | Solution/Core | Route and booking invariant tests |
| Tuesday | Create API integration-test setup | Solution/API | Test server and database strategy |
| Wednesday | Add concurrency tests for seat booking | Solution/API | Competing requests are tested |
| Thursday | Define roles and ownership rules | Application/API | Customer, admin, operator, and driver access matrix |
| Friday | Add authentication/authorization stubs | API | Early `[Authorize]` boundaries |
| Saturday | Review test failures and add a buffer day | Solution | Repair weak tests before UI work |

**Checkpoint:** Tests and authorization assumptions exist before the UI depends on them.

### **WEEKS 11-12: BLAZOR WEB APP**

| Week | Day | Task | Project | Output |
|---|---|---|---|---|
| Week 11 | Monday | Configure `ApiClient` and API base URL | Web | Web app calls API over HTTP |
| Week 11 | Tuesday | Build trip search page | Web | Boarding depot, destination depot, and date inputs |
| Week 11 | Wednesday | Build trip cards and seat selector | Web | Stop times, fares, and availability |
| Week 11 | Thursday | Build passenger and booking form | Web | Passenger and accessibility details |
| Week 11 | Friday | Connect search, hold, and booking | Web | Selected segment reaches the API |
| Week 12 | Monday | Build payment and confirmation pages | Web | Pay and display booking reference |
| Week 12 | Tuesday | Build customer booking history | Web | View booking status and trip details |
| Week 12 | Wednesday | Add cancellation and rescheduling UI | Web | Show refund result and new trip |
| Week 12 | Thursday | Add loading, validation, and error states | Web | Clear feedback for every API state |
| Week 12 | Friday | Buffer and test the browser workflow | Web | Search → hold → pay → confirm |
| Week 12 | Saturday | Review Blazor components and routing | Web | Clean component boundaries |

**Checkpoint:** A user can complete the booking flow in the browser using the real API.

### **WEEK 13: SECURITY, OPERATIONS, AND PRIVACY**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Configure ASP.NET Core Identity and login | API/Web | Secure user authentication |
| Tuesday | Complete role-based authorization | API/Web | Customer, admin, operator, and driver permissions |
| Wednesday | Add rate limiting and input validation | API | Abuse protection and safe requests |
| Thursday | Add health checks, structured logs, and metrics | API | Operational visibility |
| Friday | Add privacy, retention, and secret-management rules | Solution | No passwords, cards, or secrets in source control |
| Saturday | Test authorization and privacy boundaries | Solution | Users access only permitted data |

**Checkpoint:** Security is applied to existing endpoints, not postponed until after release.

### **WEEK 14: DEPLOYMENT AND RELEASE BUFFER**

| Day | Task | Project | Output |
|---|---|---|---|
| Monday | Configure production settings and environments | API/Web | Development and production configuration |
| Tuesday | Add database backup and restore procedure | Infrastructure | Tested recovery process |
| Wednesday | Configure CI and automated tests | Solution | Build and tests run on every push |
| Thursday | Publish API and Blazor Web App | API/Web | Deployment packages and environment settings |
| Friday | Run final acceptance tests and fix remaining issues | Solution | Release checklist and go/no-go decision |
| Saturday | Full buffer and documentation session | Solution | README, architecture notes, and v1.0 preparation |

**By end of Week 14:** Production-ready release candidate + GitHub commit "Prepare v1.0 release".
-->

#### **1. User**

The authenticated identity for customers, admins, operators, and drivers. ASP.NET Core Identity manages passwords and authentication; this entity stores application-level identity and roles.

#### **2. Customer**

The customer profile linked to a `User`. It stores contact details and owns customer bookings.

#### **3. Depot**

```csharp
public class Depot
{
   public Guid Id { get; set; }
   public string Code { get; set; } = default!; // Unique, e.g. LIL-CENTRAL
   public string Name { get; set; } = default!;
   public string AddressLine1 { get; set; } = default!;
   public string? AddressLine2 { get; set; }
   public string City { get; set; } = default!;
   public string Region { get; set; } = default!;
   public string CountryCode { get; set; } = default!; // ISO 3166-1 alpha-2
   public string TimeZoneId { get; set; } = default!; // IANA ID
   public decimal? Latitude { get; set; }
   public decimal? Longitude { get; set; }
   public string? PhoneNumber { get; set; }
   public bool IsActive { get; set; } = true;
   public DateTimeOffset CreatedAt { get; set; }
   public DateTimeOffset? UpdatedAt { get; set; }
}
```

#### **4. Bus**

```csharp
public class Bus
{
   public Guid Id { get; set; }
   public string FleetNumber { get; set; } = default!; // Unique internal identifier
   public string RegistrationNumber { get; set; } = default!; // Unique legal identifier
   public string Manufacturer { get; set; } = default!;
   public string Model { get; set; } = default!;
   public int ManufactureYear { get; set; }
   public int Capacity { get; set; }
   public bool IsAccessible { get; set; }
   public bool IsActive { get; set; } = true;
   public DateTimeOffset CreatedAt { get; set; }
   public DateTimeOffset? UpdatedAt { get; set; }
}
```

#### **5. BusSeat**

```csharp
public class BusSeat
{
   public Guid Id { get; set; }
   public Guid BusId { get; set; }
   public string SeatNumber { get; set; } = default!;
   public int RowNumber { get; set; }
   public int ColumnNumber { get; set; }
   public SeatType Type { get; set; }
   public bool IsActive { get; set; } = true;
   public Bus Bus { get; set; } = default!;
}
```

#### **6. Route**

```csharp
public class Route
{
   public Guid Id { get; set; }
   public string Code { get; set; } = default!; // Unique, e.g. LIL-BLA-01
   <!-- The following block was an accidental duplicate insertion and is hidden. -->
   <!--
   ## **PART 3: MICRO-TASKS FOR YOUR 2.5-HOUR SESSIONS**
   public string? Description { get; set; }
   public int TotalDistanceKm { get; set; }
   public bool IsActive { get; set; } = true;
   public DateTimeOffset CreatedAt { get; set; }
   public DateTimeOffset? UpdatedAt { get; set; }
   public List<RouteStop> Stops { get; set; } = [];
}
```

#### **7. RouteStop**

```csharp
public class RouteStop
{
   public Guid Id { get; set; }
   public Guid RouteId { get; set; }
   public Guid DepotId { get; set; }
   public int StopSequence { get; set; } // Unique per route, starts at 1
   public int DistanceFromOriginKm { get; set; }
   public int ArrivalOffsetMinutes { get; set; }
   public int DepartureOffsetMinutes { get; set; }
   public Route Route { get; set; } = default!;
   public Depot Depot { get; set; } = default!;
}
```

#### **8. Schedule**

```csharp
public class Schedule
{
   public Guid Id { get; set; }
   public Guid RouteId { get; set; }
   public string Name { get; set; } = default!;
   public TimeOnly PlannedDepartureTime { get; set; }
   public int OperatingDaysMask { get; set; } // Bit mask: Monday = 1, Tuesday = 2, etc.
   public DateOnly EffectiveFrom { get; set; }
   public DateOnly? EffectiveTo { get; set; }
   public bool IsActive { get; set; } = true;
   public Route Route { get; set; } = default!;
}
```

#### **9. Trip**

```csharp
public class Trip
{
   public Guid Id { get; set; }
   public Guid ScheduleId { get; set; }
   public Guid BusId { get; set; }
   public DateOnly ServiceDate { get; set; }
   public DateTimeOffset PlannedDepartureAt { get; set; }
   public DateTimeOffset? ActualDepartureAt { get; set; }
   public DateTimeOffset? ActualArrivalAt { get; set; }
   public TripStatus Status { get; set; }
   public string? CancellationReason { get; set; }
   public byte[] RowVersion { get; set; } = [];
   public Schedule Schedule { get; set; } = default!;
   public Bus Bus { get; set; } = default!;
}
```

#### **10. Passenger**

```csharp
public class Passenger
{
   public Guid Id { get; set; }
   public Guid? CustomerId { get; set; }
   public string FirstName { get; set; } = default!;
   public string LastName { get; set; } = default!;
   public DateOnly? DateOfBirth { get; set; }
   public string? IdentificationType { get; set; }
   public string? IdentificationNumber { get; set; }
   public string? EmergencyContactName { get; set; }
   public string? EmergencyContactPhone { get; set; }
   public bool RequiresAccessibleBoarding { get; set; }
   public Customer? Customer { get; set; }
}
```

#### **11. Booking**

```csharp
public class Booking
{
   public Guid Id { get; set; }
   public string BookingReference { get; set; } = default!; // Public unique reference
   public Guid CustomerId { get; set; }
   public Guid TripId { get; set; }
   public Guid BoardingStopId { get; set; }
   public Guid DestinationStopId { get; set; }
   public BookingStatus Status { get; set; }
   public decimal TotalAmount { get; set; }
   public string CurrencyCode { get; set; } = default!;
   public DateTimeOffset? HoldExpiresAt { get; set; }
   public DateTimeOffset CreatedAt { get; set; }
   public DateTimeOffset? ConfirmedAt { get; set; }
   public DateTimeOffset? CancelledAt { get; set; }
   public string? CancellationReason { get; set; }
   public Customer Customer { get; set; } = default!;
   public Trip Trip { get; set; } = default!;
   public RouteStop BoardingStop { get; set; } = default!;
   public RouteStop DestinationStop { get; set; } = default!;
   public List<BookingItem> Items { get; set; } = [];
}
```

#### **12. BookingItem**

```csharp
public class BookingItem
{
   public Guid Id { get; set; }
   public Guid BookingId { get; set; }
   public Guid PassengerId { get; set; }
   public Guid BusSeatId { get; set; }
   public decimal Amount { get; set; }
   public string CurrencyCode { get; set; } = default!;
   public BookingItemStatus Status { get; set; }
   public Booking Booking { get; set; } = default!;
   public Passenger Passenger { get; set; } = default!;
   public BusSeat BusSeat { get; set; } = default!;
}
```

#### **13. Payment**

```csharp
public class Payment
{
   public Guid Id { get; set; }
   public Guid BookingId { get; set; }
   public string Provider { get; set; } = default!;
   public string IdempotencyKey { get; set; } = default!;
   public string? ProviderTransactionId { get; set; }
   public decimal Amount { get; set; }
   public string CurrencyCode { get; set; } = default!;
   public PaymentStatus Status { get; set; }
   public DateTimeOffset CreatedAt { get; set; }
   public DateTimeOffset? CompletedAt { get; set; }
   public DateTimeOffset? RefundedAt { get; set; }
   public Booking Booking { get; set; } = default!;
}
```

#### **14. Notification**

```csharp
public class Notification
{
   public Guid Id { get; set; }
   public Guid CustomerId { get; set; }
   public Guid? BookingId { get; set; }
   public NotificationType Type { get; set; }
   public NotificationChannel Channel { get; set; }
   public string Recipient { get; set; } = default!;
   public NotificationStatus Status { get; set; }
   public int AttemptCount { get; set; }
   public DateTimeOffset? SentAt { get; set; }
   public string? LastError { get; set; }
   public Customer Customer { get; set; } = default!;
   public Booking? Booking { get; set; }
}
```

#### **15. AuditLog**

-->

```csharp
public class AuditLog
{
   public Guid Id { get; set; }
   public Guid? ActorUserId { get; set; }
   public string EntityName { get; set; } = default!;
   public Guid EntityId { get; set; }
   public string Action { get; set; } = default!;
   public string? OldValuesJson { get; set; }
   public string? NewValuesJson { get; set; }
   public DateTimeOffset CreatedAt { get; set; }
   public string? IpAddress { get; set; }
   public string? UserAgent { get; set; }
   public string? CorrelationId { get; set; }
   public User? ActorUser { get; set; }
}
```

Define these enums explicitly and store them as strings or small integers through EF Core conversions: `UserType`, `SeatType`, `TripStatus`, `BookingStatus`, `BookingItemStatus`, `PaymentStatus`, `NotificationType`, `NotificationChannel`, and `NotificationStatus`.

### **Required Database Constraints**

- Unique: `User.IdentityUserId`, `User.UserName`, `Depot.Code`, `Bus.FleetNumber`, `Bus.RegistrationNumber`, `Route.Code`, `Booking.BookingReference`, and `Payment.IdempotencyKey`.
- One-to-one: `Customer.UserId` should be unique when each customer profile belongs to exactly one user account.
- Unique composite: `(RouteId, StopSequence)`, `(RouteId, DepotId)`, `(BusId, SeatNumber)`, and `(TripId, BookingItem.BusSeatId, BoardingStopId, DestinationStopId)` where supported by the chosen segment-inventory design.
- Check: route has at least two stops, stop sequences are positive, destination sequence is greater than boarding sequence, amounts are non-negative, and `EffectiveTo` is on or after `EffectiveFrom`.
- Index: trip search by `(ServiceDate, Status)`, route stops by `(RouteId, StopSequence)`, and bookings by `(CustomerId, CreatedAt)`.
- Add a concurrency token to trip and booking inventory updates. A unique booking-item constraint alone is insufficient for overlapping route segments; enforce the overlap rule in a transaction or introduce a dedicated `TripSeatSegment` inventory table.

### **Business Logic (Simple Version)**

```
When customer books:
1. Check if seat available
2. Create booking (status = pending)
3. Process payment
4. If payment succeeds → status = confirmed
5. Send confirmation email

When customer cancels:
1. Mark booking as cancelled
2. Refund payment
3. Seat becomes available again
```

---

## **PART 2: ARCHITECTURE DESIGN**

### **Application Architecture**

```
┌─────────────────────────────────┐
│   BusBookingSystem.Web          │  Blazor Web App
│   - Search trips                │  - Book and pay
│   - Select seats                │  - View bookings
└────────────┬────────────────────┘
             │ HTTP/JSON
┌────────────▼────────────────────┐
│   BusBookingSystem.API          │  ASP.NET Core Web API
│   - Controllers                 │  - Authentication
│   - Request/response DTOs       │  - ProblemDetails errors
└────────────┬────────────────────┘
             │ Application services
┌────────────▼────────────────────┐
│   BusBookingSystem.Application  │  Use cases and contracts
│   - Booking and trip workflows  │  - DTOs and validators
│   - Service interfaces          │
└────────────┬────────────────────┘
             │ Infrastructure interfaces
┌────────────▼────────────────────┐
│   BusBookingSystem.Infrastructure│  Technical implementations
│   - EF Core and SQL Server      │  - Payments and notifications
└────────────┬────────────────────┘
             │
┌────────────▼────────────────────┐
│   BusBookingSystem.Core          │  Domain entities and rules
│   - Bus, Depot, Route, Booking  │  - Enums and invariants
└─────────────────────────────────┘
             │ EF Core migrations
┌────────────▼────────────────────┐
│   SQL Server Database            │  Data persistence
│   - Normalized tables           │  - Constraints and indexes
│   - Buses table                 │  - Normalized schema
│   - Depots and RouteStops       │  - Indexes for performance
│   - Routes table                │  - Indexes for performance
│   - Schedules and Trips tables  │  - Constraints
│   - BusSeats table               │
│   - Bookings table              │
│   - Customers table             │
│   - Passengers and BookingItems │
│   - Payments table              │
│   - Notifications and AuditLog  │
└─────────────────────────────────┘
```

### **Project Structure**

```
BusBookingSystem/
├── BusBookingSystem.Core/         (Class Library: domain only)
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Bus.cs
│   │   ├── BusSeat.cs
│   │   ├── Depot.cs
│   │   ├── Route.cs
│   │   ├── RouteStop.cs
│   │   ├── Schedule.cs
│   │   ├── Trip.cs
│   │   ├── Booking.cs
│   │   ├── BookingItem.cs
│   │   ├── Customer.cs
│   │   ├── Passenger.cs
│   │   ├── Payment.cs
│   │   └── Notification.cs
│   │   ├── AuditLog.cs
│   └── Enums/
│       ├── UserType.cs
│       ├── SeatType.cs
│       ├── TripStatus.cs
│       ├── BookingStatus.cs
│       ├── PaymentStatus.cs
│       └── NotificationStatus.cs

├── BusBookingSystem.Application/ (Class Library: use cases/contracts)
│   ├── DTOs/
│   │   ├── SearchTripsRequest.cs
│   │   ├── CreateBookingRequest.cs
│   │   ├── BookingResponse.cs
│   │   └── TripResponse.cs
│   ├── Interfaces/
│       ├── IBookingService.cs
│       ├── IDepotService.cs
│       ├── IRouteService.cs
│       ├── ITripService.cs
│       ├── IPaymentService.cs
│       └── INotificationService.cs
│   └── Validators/
│       ├── CreateBookingValidator.cs
│       └── SearchTripsValidator.cs

├── BusBookingSystem.Infrastructure/ (Class Library: technical details)
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── Configurations/
│   │       ├── UserConfiguration.cs
│   │       ├── DepotConfiguration.cs
│   │       ├── RouteConfiguration.cs
│   │       ├── TripConfiguration.cs
│   │       ├── BookingConfiguration.cs
│   │       └── PaymentConfiguration.cs
│   ├── Services/
│   │   ├── BookingService.cs
│   │   ├── DepotService.cs
│   │   ├── RouteService.cs
│   │   ├── TripService.cs
│   │   ├── PaymentService.cs
│   │   └── NotificationService.cs
│   └── Migrations/
│
├── BusBookingSystem.API/          (ASP.NET Core Web API)
│   ├── Controllers/
│   │   ├── BusesController.cs
│   │   ├── DepotsController.cs
│   │   ├── RoutesController.cs
│   │   ├── TripsController.cs
│   │   ├── BookingsController.cs
│   │   └── PaymentsController.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── BusBookingSystem.Web/          (Blazor Web App)
│   ├── Components/
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor
│   │   │   └── NavMenu.razor
│   │   ├── Pages/
│   │   │   ├── Home.razor
│   │   │   ├── SearchTrips.razor
│   │   │   ├── BookingForm.razor
│   │   │   ├── Confirmation.razor
│   │   │   └── MyBookings.razor
│   │   └── Shared/
│   │       ├── TripCard.razor
│   │       ├── SeatSelector.razor
│   │       └── PaymentForm.razor
│   ├── Services/
│   │   └── ApiClient.cs
│   ├── Models/
│   │   └── ApiModels.cs
│   ├── wwwroot/
│   │   ├── css/
│   │   └── appsettings.json
│   ├── Program.cs
│   └── Routes.razor
```

### **Project References**

```text
BusBookingSystem.Core
   ↑
BusBookingSystem.Application
   ↑
BusBookingSystem.Infrastructure
   ↑
BusBookingSystem.API  ← HTTP/JSON →  BusBookingSystem.Web
```

- `Core` references no other project.
- `Application` references `Core`.
- `Infrastructure` references `Application` and `Core`.
- `API` references `Application` and `Infrastructure`.
- `Web` references neither database nor Infrastructure; it calls `API` through `ApiClient`.

---

## **PART 3: MICRO-TASKS FOR YOUR 2.5-HOUR SESSIONS**

**Key principle:** Each session builds one small, testable piece. Friday is a catch-up buffer; Saturday is for learning, review, and practice.

Each week below has a small goal, a daily task, a project location, and a visible output. Friday is deliberately a buffer; Saturday is for learning and review.

### **WEEK 1: SOLUTION AND DOMAIN ORIENTATION**

| Day       | Task                                                         | Project  | Output                                                  |
| --------- | ------------------------------------------------------------ | -------- | ------------------------------------------------------- |
| Monday    | Create the solution and five projects                        | Solution | `.sln`, Core, Application, Infrastructure, API, and Web |
| Tuesday   | Add project references and build                             | Solution | Correct dependency direction                            |
| Wednesday | Learn classes, properties, `Guid`, nullable types, and enums | Core     | Practice classes compile                                |
| Thursday  | Create `User`, `Customer`, and `Depot`                       | Core     | Identity and location entities                          |
| Friday    | Catch up, fix compile errors, and commit                     | Solution | Working foundation commit                               |
| Saturday  | Learn LINQ filtering and projection                          | Core     | Five simple list queries                                |

**Checkpoint:** You can explain each project and why Core has no web or database dependency.

### **WEEK 2: REMAINING DOMAIN ENTITIES**

| Day       | Task                                                    | Project | Output                                 |
| --------- | ------------------------------------------------------- | ------- | -------------------------------------- |
| Monday    | Create `Bus` and `BusSeat`                              | Core    | Fleet and seat inventory               |
| Tuesday   | Create `Route` and `RouteStop`                          | Core    | Ordered route with at least two depots |
| Wednesday | Create `Schedule` and `Trip`                            | Core    | Recurring service and dated departure  |
| Thursday  | Create `Passenger`, `Booking`, and `BookingItem`        | Core    | Passenger and booking relationships    |
| Friday    | Create `Payment`, `Notification`, `AuditLog`, and enums | Core    | Complete domain model                  |
| Saturday  | Review relationships and practice LINQ                  | Core    | Queries from trips to depots           |

**Checkpoint:** All entities compile with the agreed key, time, and money types.

### **WEEK 3: APPLICATION CONTRACTS AND DEPENDENCY INJECTION**

| Day       | Task                                           | Project     | Output                                                            |
| --------- | ---------------------------------------------- | ----------- | ----------------------------------------------------------------- |
| Monday    | Learn interfaces and dependency inversion      | Application | Interface versus implementation notes                             |
| Tuesday   | Create search, trip, booking, and payment DTOs | Application | Request and response contracts                                    |
| Wednesday | Create service interfaces                      | Application | Depot, route, trip, booking, payment, and notification interfaces |
| Thursday  | Create validators and result/error models      | Application | Predictable validation failures                                   |
| Friday    | Register and call one sample service           | API         | First dependency injection example                                |
| Saturday  | Review DI, DTOs, and async/await               | Application | One tested practice use case                                      |

**Checkpoint:** A controller depends on an interface, not a database.

### **WEEK 4: EF CORE AND SQL SERVER BASICS**

| Day       | Task                                             | Project        | Output                           |
| --------- | ------------------------------------------------ | -------------- | -------------------------------- |
| Monday    | Learn `DbContext`, `DbSet`, and EF Core tracking | Infrastructure | EF Core notes and example        |
| Tuesday   | Install EF Core SQL Server packages              | Infrastructure | Packages and connection settings |
| Wednesday | Create `ApplicationDbContext`                    | Infrastructure | `DbSet` properties               |
| Thursday  | Configure `Depot` with Fluent API                | Infrastructure | First entity configuration       |
| Friday    | Configure User, Customer, Bus, and BusSeat       | Infrastructure | Keys, lengths, and relationships |
| Saturday  | Create and apply the first migration             | Infrastructure | Database created from code       |

**Checkpoint:** You can save and retrieve a depot from SQL Server.

### **WEEK 5: MIGRATIONS, CONSTRAINTS, AND SEED DATA**

| Day       | Task                                              | Project        | Output                                 |
| --------- | ------------------------------------------------- | -------------- | -------------------------------------- |
| Monday    | Configure routes and ordered route stops          | Infrastructure | Stop indexes and constraints           |
| Tuesday   | Configure schedules and trips                     | Infrastructure | Dates, statuses, and concurrency token |
| Wednesday | Configure bookings, passengers, and booking items | Infrastructure | Foreign keys and indexes               |
| Thursday  | Configure payments, notifications, and audit logs | Infrastructure | Money precision and audit table        |
| Friday    | Add development seed data                         | Infrastructure | Depots, buses, routes, and trips       |
| Saturday  | Test invalid data and review generated SQL        | Infrastructure | Constraint failures understood         |

**Checkpoint:** A fresh database can be created entirely from migrations.

### **WEEK 6: API READ FEATURES**

| Day       | Task                                            | Project            | Output                       |
| --------- | ----------------------------------------------- | ------------------ | ---------------------------- |
| Monday    | Create depot and bus read services              | Infrastructure     | EF Core read use cases       |
| Tuesday   | Create `DepotsController` and `BusesController` | API                | GET endpoints                |
| Wednesday | Create route read service and controller        | API/Infrastructure | Routes include ordered stops |
| Thursday  | Create schedule and trip read services          | API/Infrastructure | Searchable dated trips       |
| Friday    | Register services and test with Swagger         | API                | Working API responses        |
| Saturday  | Practice GET requests and response DTOs         | API                | Saved test requests          |

### **WEEK 7: API BOOKING FEATURES**

| Day       | Task                                                           | Project         | Output                       |
| --------- | -------------------------------------------------------------- | --------------- | ---------------------------- |
| Monday    | Add trip search by boarding depot, destination depot, and date | API             | Segment-aware search         |
| Tuesday   | Validate booking requests                                      | Application/API | Invalid data rejected        |
| Wednesday | Create booking and availability endpoints                      | API             | Booking and seat responses   |
| Thursday  | Add pagination, filtering, sorting, and ProblemDetails         | API             | Consistent API responses     |
| Friday    | Catch up and write integration tests                           | API             | Search and booking scenarios |
| Saturday  | Practice debugging in Swagger/Postman                          | API             | Test collection              |

### **WEEK 8: SEAT INVENTORY AND CONCURRENCY**

| Day       | Task                                                  | Project                    | Output                    |
| --------- | ----------------------------------------------------- | -------------------------- | ------------------------- |
| Monday    | Calculate fares between route stops                   | Application                | Segment fare calculation  |
| Tuesday   | Query seats across overlapping segments               | Application                | Correct availability      |
| Wednesday | Add expiring seat holds                               | Application/Infrastructure | Hold expiry and release   |
| Thursday  | Use a database transaction for booking creation       | Infrastructure             | Double-booking protection |
| Friday    | Catch up and test cancellation/rescheduling inventory | API/Application            | Seats release or re-check |
| Saturday  | Learn transactions, isolation, and race conditions    | Infrastructure             | Race condition explained  |

### **WEEK 9: PAYMENTS, WEBHOOKS, REFUNDS, AND AUDIT**

| Day       | Task                                               | Project                    | Output                     |
| --------- | -------------------------------------------------- | -------------------------- | -------------------------- |
| Monday    | Create payment adapter and fake provider           | Infrastructure             | Testable provider boundary |
| Tuesday   | Add payment initiation and provider IDs            | API/Infrastructure         | No raw card data           |
| Wednesday | Add webhook verification and idempotency           | API/Infrastructure         | Safe repeated callbacks    |
| Thursday  | Add refunds and cancellation payment states        | Application/Infrastructure | Correct refund status      |
| Friday    | Add audit events for success, failure, and denial  | Infrastructure             | Action history             |
| Saturday  | Test failures, retries, refunds, and audit records | API/Infrastructure         | Automated retry scenarios  |

### **WEEK 10: NOTIFICATIONS, TESTING, AND AUTHORIZATION FOUNDATIONS**

| Day       | Task                                         | Project         | Output                                  |
| --------- | -------------------------------------------- | --------------- | --------------------------------------- |
| Monday    | Add notification queue and retry status      | Infrastructure  | Confirmation/cancellation notifications |
| Tuesday   | Test route and booking domain rules          | Solution/Core   | Unit tests                              |
| Wednesday | Create API integration-test setup            | Solution/API    | Test server and database strategy       |
| Thursday  | Define role and ownership access matrix      | Application/API | Customer/admin/operator/driver rules    |
| Friday    | Add early authentication/authorization stubs | API             | Initial `[Authorize]` boundaries        |
| Saturday  | Review failures and repair weak tests        | Solution        | UI-ready test foundation                |

### **WEEK 11: BLAZOR SEARCH AND BOOKING UI**

| Day       | Task                                    | Project | Output                          |
| --------- | --------------------------------------- | ------- | ------------------------------- |
| Monday    | Configure `ApiClient` and API base URL  | Web     | Web calls API over HTTP         |
| Tuesday   | Build trip search page                  | Web     | Depot and date inputs           |
| Wednesday | Build trip cards and seat selector      | Web     | Times, fares, and availability  |
| Thursday  | Build passenger and booking form        | Web     | Passenger/accessibility details |
| Friday    | Connect search, hold, and booking       | Web     | Selected segment reaches API    |
| Saturday  | Review routing and component parameters | Web     | Clean components                |

### **WEEK 12: BLAZOR PAYMENT AND MANAGEMENT UI**

| Day       | Task                                          | Project | Output                           |
| --------- | --------------------------------------------- | ------- | -------------------------------- |
| Monday    | Build payment form and state display          | Web     | Payment success/failure feedback |
| Tuesday   | Build confirmation page                       | Web     | Booking reference and details    |
| Wednesday | Build customer booking history                | Web     | Status and trip details          |
| Thursday  | Add cancellation and rescheduling UI          | Web     | Refund result and new trip       |
| Friday    | Add loading, validation, and API error states | Web     | Clear request feedback           |
| Saturday  | Test the complete browser workflow            | Web     | Search → hold → pay → confirm    |

### **WEEK 13: SECURITY, OPERATIONS, AND PRIVACY**

| Day       | Task                                                | Project  | Output                       |
| --------- | --------------------------------------------------- | -------- | ---------------------------- |
| Monday    | Configure ASP.NET Core Identity and login           | API/Web  | Secure authentication        |
| Tuesday   | Complete role-based authorization                   | API/Web  | Role permissions             |
| Wednesday | Add rate limiting and input validation              | API      | Abuse protection             |
| Thursday  | Add health checks, logs, and metrics                | API      | Operational visibility       |
| Friday    | Add privacy, retention, and secret-management rules | Solution | No secrets in source control |
| Saturday  | Test authorization and privacy boundaries           | Solution | Access tests                 |

### **WEEK 14: DEPLOYMENT AND RELEASE BUFFER**

| Day       | Task                                          | Project        | Output                      |
| --------- | --------------------------------------------- | -------------- | --------------------------- |
| Monday    | Configure development and production settings | API/Web        | Environment configuration   |
| Tuesday   | Add and test database backup and restore      | Infrastructure | Recovery procedure          |
| Wednesday | Configure CI and automated tests              | Solution       | Build on every push         |
| Thursday  | Publish API and Blazor Web App                | API/Web        | Deployment packages         |
| Friday    | Run acceptance tests and fix remaining issues | Solution       | Release checklist           |
| Saturday  | Document the system and use the final buffer  | Solution       | README and v1.0 preparation |

**By end of Week 14:** Production-ready release candidate + GitHub commit "Prepare v1.0 release".

<!--
## **PART 3: MICRO-TASKS FOR YOUR 2.5-HOUR SESSIONS**

**Key principle:** Each session builds one complete, testable piece.

### **WEEK 1: Foundation (LINQ + Lambda mastery)**

**Saturday 8am-12pm:** Learn LINQ + Lambda (from Phase 1 outline)
**Mon-Fri 3:30-6am:** Build domain models

| Day | Task | Time | Output |
|---|---|---|---|
| **Mon** | Create project + Bus and BusSeat models | 2.5 hrs | Bus.cs, BusSeat.cs |
| **Tue** | Create Depot and RouteStop models | 2.5 hrs | Depot.cs, RouteStop.cs |
| **Wed** | Create Route, Schedule, and Trip models | 2.5 hrs | Route.cs, Schedule.cs, Trip.cs |
| **Thu** | Create Booking, BookingItem, and Passenger models | 2.5 hrs | Booking.cs, BookingItem.cs, Passenger.cs |
| **Fri** | Create User, Customer, Payment, and Notification models | 2.5 hrs | User.cs, Customer.cs, Payment.cs, Notification.cs |
| **Sat morning** | Learn LINQ/Lambda from Microsoft Learn | 2 hrs | Understand filtering, searching |
| **Sat afternoon** | Practice LINQ on your models | 2 hrs | Write 5 LINQ queries |

**By end of Week 1:** Domain models and route invariants documented + GitHub commit "Add domain models"

| Day | Task | Time | Output |
|---|---|---|---|
| **Mon** | Create ApplicationDbContext (DbContext) | 2.5 hrs | DbContext.cs (50 lines) |
| **Tue** | Create migration for Buses, BusSeats, and Depots | 2.5 hrs | Tables, keys, and indexes |
| **Wed** | Create migration for Routes and RouteStops | 2.5 hrs | Ordered stops; no Route-to-Bus FK |
| **Thu** | Create migration for Schedules and Trips | 2.5 hrs | Dated trips and status constraints |
| **Fri** | Create migration for Bookings, BookingItems, Passengers, and Payments | 2.5 hrs | FKs, money precision, status constraints |
| **Sat** | Learn Async/Await from Phase 1 | 2 hrs | Understand Task<T> |
| **Sat** | Apply async patterns to DbContext | 2 hrs | Practice async queries |

**By end of Week 2:** Database schema complete + migrations + concurrency indexes + GitHub commit "Add database schema"

---

### **WEEK 3-4: REST API Foundation**

**Sat 8am-12pm:** Start REST API course (Bhrugen Patel)

| Week | Day | Task | Time | Output |
|---|---|---|---|---|
| **Week 3** | **Mon** | Create BusController (GET all buses) | 2.5 hrs | GET /api/buses endpoint |
| | **Tue** | Add GET bus by ID endpoint | 2.5 hrs | GET /api/buses/{id} |
| | **Wed** | Add POST create bus endpoint | 2.5 hrs | POST /api/buses |
| | **Thu** | Create DepotController and RouteController | 2.5 hrs | Manage depots and ordered route stops |
| | **Fri** | Add route validation and search | 2.5 hrs | Reject fewer than 2 depots; search by boarding and destination |
| **Week 4** | **Mon** | Create ScheduleController | 2.5 hrs | GET /api/schedules |
| | **Tue** | Create TripController and trip generation | 2.5 hrs | Bookable dated trips from schedules |
| | **Wed** | Test all endpoints in Postman | 2.5 hrs | Verify CRUD works |
| | **Thu** | Create BookingController (GET bookings) | 2.5 hrs | GET /api/bookings |
| | **Fri** | Add pagination, filtering, and ProblemDetails errors | 2.5 hrs | Consistent API responses |

**By end of Week 4:** Working REST API with 5+ endpoints + GitHub commit "Add REST API endpoints"

---

### **WEEK 5-6: Business Logic**

**Sat 8am-12pm:** Continue REST API course

| Week | Day | Task | Time | Output |
|---|---|---|---|---|
| **Week 5** | **Mon** | Create BookingService class | 2.5 hrs | Check availability for a trip segment |
| | **Tue** | Add seat hold and CreateBooking transaction | 2.5 hrs | Expiring hold; prevent double booking |
| | **Wed** | Add GetAvailableSeats method | 2.5 hrs | Segment-aware inventory query |
| | **Thu** | Create CancelBooking method | 2.5 hrs | Cancellation policy + refund request |
| | **Fri** | Create RouteService and TripService | 2.5 hrs | Depot ordering, stop times, trip assignment |
| **Week 6** | **Mon** | Add segment fare calculation | 2.5 hrs | Fare by boarding/destination depots |
| | **Tue** | Add trip search by depots and date | 2.5 hrs | Filter schedules by route segment and time |
| | **Wed** | Add GetCustomerBookings method | 2.5 hrs | Customer history query |
| | **Thu** | Create PaymentService with provider adapter | 2.5 hrs | Webhook, idempotency, and refund handling |
| | **Fri** | Integrate services into controllers | 2.5 hrs | Use services, not raw queries |

**By end of Week 6:** Business logic layer complete + GitHub commit "Add business logic services"

---

### **WEEK 7-8: Advanced Features**

**Sat 8am-12pm:** SQL/EF Core course (Dominic Tripodi)

| Week | Day | Task | Time | Output |
|---|---|---|---|---|
| **Week 7** | **Mon** | Add seat selection logic | 2.5 hrs | Reserve specific seat number |
| | **Tue** | Add passenger details and accessibility needs | 2.5 hrs | One or more passengers per booking |
| | **Wed** | Create booking status tracking | 2.5 hrs | Pending → Confirmed → Completed |
| | **Thu** | Add search/filter by price range | 2.5 hrs | GET /api/schedules?minPrice=100&maxPrice=500 |
| | **Fri** | Add sorting (by time, price, availability) | 2.5 hrs | GET /api/schedules?sortBy=price |
| **Week 8** | **Mon** | Create GetBookingDetails endpoint | 2.5 hrs | Full booking info |
| | **Tue** | Add RescheduleBooking method | 2.5 hrs | Change trip and re-check segment inventory |
| | **Wed** | Add refund calculation | 2.5 hrs | Partial refund based on cancellation time |
| | **Thu** | Add audit trail and notification delivery | 2.5 hrs | Audit events, email/SMS retry status |
| | **Fri** | Test booking concurrency and payment webhooks | 2.5 hrs | Automated integration scenarios |

**By end of Week 8:** Complete API with all business logic + GitHub commit "Add advanced features"

---

### **WEEK 9-10: Blazor UI**

**Sat 8am-12pm:** Blazor course (Frank Liu)

| Week | Day | Task | Time | Output |
|---|---|---|---|---|
| **Week 9** | **Mon** | Create SearchTrips.razor page | 2.5 hrs | Search form (boarding depot, destination depot, date) |
| | **Tue** | Add TripList component | 2.5 hrs | Display trips, stop times, and fares |
| | **Wed** | Add SeatSelector component | 2.5 hrs | Interactive seat map |
| | **Thu** | Create BookingForm.razor | 2.5 hrs | Form (name, email, phone) |
| | **Fri** | Add search results display | 2.5 hrs | Show matching trips and segment availability |
| **Week 10** | **Mon** | Create Confirmation.razor page | 2.5 hrs | Display booking confirmation |
| | **Tue** | Add price display/calculation | 2.5 hrs | Show total price |
| | **Wed** | Create MyBookings.razor (view bookings) | 2.5 hrs | List customer's bookings |
| | **Thu** | Add cancel booking button | 2.5 hrs | Cancel + refund confirmation |
| | **Fri** | Connect all pages (navigation) | 2.5 hrs | Working flow: search → hold → pay → confirm |

**By end of Week 10:** Complete Blazor UI + GitHub commit "Add Blazor web UI"

---

### **WEEK 10: SECURITY, OPERATIONS, AND RELEASE**

| Week | Day | Task | Project | Output |
|---|---|---|---|---|
| Week 10 | Monday | Configure Identity, roles, and authorization | API/Web | Secure customer, admin, operator, and driver access |
| Week 10 | Tuesday | Add validation, rate limiting, and error feedback | API/Web | Safe requests and useful user messages |
| Week 10 | Wednesday | Add health checks, logs, metrics, backups, and secrets | API/Infrastructure | Operable production configuration |
| Week 10 | Thursday | Configure CI and publish both applications | Solution/API/Web | Repeatable build and deployment packages |
| Week 10 | Friday | Run acceptance tests and complete release checklist | Solution | Production-readiness decision |


---

-->

## **PART 4: THINKING FRAMEWORK**

### **When Starting Each Task, Ask:**

**1. What is the smallest piece I can build?**

- Not: "Build booking system"
- But: "Create Bus model (30 lines)"

**2. Can I test it immediately?**

- Add println statements
- Use Postman for API endpoints
- Use browser for Blazor pages

**3. Can I commit it to GitHub?**

- Each 2.5-hour session = one commit
- Meaningful commit message
- Working code, no broken branches

**4. Does it solve a real problem?**

- Monday: Can I query buses? YES
- Tuesday: Can I query routes? YES
- Each task is complete, not half-done

### **Session Structure (2.5 hours)**

```
0:00–0:05   Understand the task (read requirements)
0:05–0:10   Plan the code (pseudocode/structure)
0:10–2:00   Write code (focus time)
2:00–2:20   Test (Postman/browser/console)
2:20–2:25   Commit to GitHub
2:25–2:30   Document what you did
```

---

## **PART 5: STARTING MONDAY MORNING**

### **Session 1: Create the solution in Visual Studio**

1. Open Visual Studio and select **Create a new project**.
2. Search for **Blank Solution**, select it, and name it `BusBookingSystem`.
3. Right-click the solution and select **Add > New Project**.
4. Add three **Class Library** projects named `BusBookingSystem.Core`, `BusBookingSystem.Application`, and `BusBookingSystem.Infrastructure`.
5. Add an **ASP.NET Core Web API** project named `BusBookingSystem.API`.
6. Add a **Blazor Web App** project named `BusBookingSystem.Web`.
7. Choose the same target framework for every project, preferably `.NET 10` if installed.

### **Session 2: Add project references**

In Solution Explorer, right-click **Dependencies** for each project, select **Add Project Reference**, and use this dependency direction:

| Project                           | References                                                        |
| --------------------------------- | ----------------------------------------------------------------- |
| `BusBookingSystem.Core`           | None                                                              |
| `BusBookingSystem.Application`    | `BusBookingSystem.Core`                                           |
| `BusBookingSystem.Infrastructure` | `BusBookingSystem.Core`, `BusBookingSystem.Application`           |
| `BusBookingSystem.API`            | `BusBookingSystem.Application`, `BusBookingSystem.Infrastructure` |
| `BusBookingSystem.Web`            | None; call the API with `HttpClient`                              |

Do not add references from Core to API, Infrastructure, or Web. The Web project must not access `ApplicationDbContext` directly.

### **Optional PowerShell setup**

The following commands create the same solution from the Developer PowerShell for Visual Studio. Run them from the folder where you want the solution to live.

```powershell
New-Item -ItemType Directory -Name BusBookingSystem
Set-Location BusBookingSystem

dotnet new sln -n BusBookingSystem
dotnet new classlib -n BusBookingSystem.Core
dotnet new classlib -n BusBookingSystem.Application
dotnet new classlib -n BusBookingSystem.Infrastructure
dotnet new webapi -n BusBookingSystem.API
dotnet new blazor -n BusBookingSystem.Web

dotnet sln add .\BusBookingSystem.Core\BusBookingSystem.Core.csproj
dotnet sln add .\BusBookingSystem.Application\BusBookingSystem.Application.csproj
dotnet sln add .\BusBookingSystem.Infrastructure\BusBookingSystem.Infrastructure.csproj
dotnet sln add .\BusBookingSystem.API\BusBookingSystem.API.csproj
dotnet sln add .\BusBookingSystem.Web\BusBookingSystem.Web.csproj

dotnet add .\BusBookingSystem.Application\ reference .\BusBookingSystem.Core
dotnet add .\BusBookingSystem.Infrastructure\ reference .\BusBookingSystem.Core .\BusBookingSystem.Application
dotnet add .\BusBookingSystem.API\ reference .\BusBookingSystem.Application .\BusBookingSystem.Infrastructure

dotnet build
```

### **Session 3: Create the first entity**

In `BusBookingSystem.Core`, create `Entities/Bus.cs`:

```csharp
namespace BusBookingSystem.Core.Entities;

public class Bus
{
   public Guid Id { get; set; }
   public string FleetNumber { get; set; } = default!;
   public string Model { get; set; } = default!;
   public int Capacity { get; set; }
   public string RegistrationNumber { get; set; } = default!;
   public int ManufactureYear { get; set; }
   public bool IsAccessible { get; set; }
   public bool IsActive { get; set; } = true;
}
```

Build the solution. Do not put a temporary `Console.WriteLine` in the API's `Program.cs`; the API is an HTTP application. Test the entity later through an API endpoint and Swagger.

### **Session 4: Run the two applications**

1. Right-click the solution and select **Set Startup Projects**.
2. Choose **Multiple startup projects**.
3. Set both `BusBookingSystem.API` and `BusBookingSystem.Web` to **Start**.
4. Start debugging with **F5**.
5. Use the API Swagger URL for API checks and the Blazor URL for browser checks.

During the first lessons, start only the API until its first endpoint works. Then start both applications together.

### **First commit**

```powershell
git init
git add .
git commit -m "Create bus booking solution structure"
```

### **Week 1 checklist**

- [ ] Solution and five projects created
- [ ] Project references follow the dependency direction
- [ ] Solution builds successfully
- [ ] `Bus.cs` created in `BusBookingSystem.Core/Entities`
- [ ] API starts and Swagger is reachable
- [ ] Blazor Web App starts
- [ ] First commit created

---

## **KEY PRINCIPLES**

**1. Build incrementally**

- Each day = one complete, testable feature
- Not 5 incomplete features spread across week

**2. Test constantly**

- After every 50 lines of code, test
- Console.WriteLine is your friend
- Postman for API testing

**3. Commit frequently**

- Monday: 1 commit
- Tuesday: 1 commit
- By Friday: 5 commits
- By Week 14: 70+ commits

**4. Learn by doing**

- Don't watch course → build
- Watch 1 lecture → code 1 feature → test it
- Repeat

**5. One task per session**

- Not "build API and database"
- But "create BusController.cs with GET endpoint"

---

**You now have:**

- ✅ Architecture design
- ✅ 14-week detailed task breakdown
- ✅ Session-by-session what to build
- ✅ How to think about each task

**Monday 3:30 AM: Create Bus.cs**

**Go.**
