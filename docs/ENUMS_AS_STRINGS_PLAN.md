# Plan: Store and Return Enums as Strings

Status: planned, not started (19 Oct 2026). Branch suggestion: `EnumsAsStrings`.

## Goal

Every enum is stored in SQL Server as its name (`'Pending'`, `'Coach'`) and returned in JSON as its name (`"bookingStatus": "Pending"`), instead of a number.

## Why

- **Readable data.** `SELECT * FROM Bookings` shows `Pending`, not `0`. Anyone reading the database or the API understands it without the C# source.
- **Safe to reorder.** Enum members have no explicit numbers, so `Pending = 0` only because it is first. Inserting a member in the middle today would silently change the meaning of every stored row. Names do not shift.
- **A clearer contract.** Clients and the README see `"Pending"`, and Scalar can list the allowed values.

The trade-off: renaming an enum member now needs a data migration, because the old name is stored in the rows. Treat member names as part of the contract.

## Current state (checked against the code)

| Enum | Used by | Database today | JSON today |
|---|---|---|---|
| `BookingStatus` (Pending, Confirmed, Cancelled) | `Booking.BookingStatus`, `BookingResponseDto` | `int` | number |
| `BusType` (Coaster, Coach, Minibus, DoubleDecker) | `Bus.BusType` | `int` | already a string in `TripDto` (`.ToString()` in the projection) |
| `BusStatus` (Available, InUse, UnderMaintenance, OutOfService, Retired) | `Bus.BusStatus` | `int` | not returned |
| `UserType` (Admin, Customer, Driver, Operator) | `User.UserType`, `UserDto` | **already `nvarchar(50)`** via `HasConversion<string>()` | number in `UserDto` |

`UserConfiguration` already shows the pattern to follow. `ResultStatus` is internal to the Application layer and is never stored or serialized, so it is out of scope.

## Part 1: database

### Step 1. Configure the conversions

In each configuration, match what `UserConfiguration` does for `UserType`:

```csharp
// BookingConfiguration
booking.Property(b => b.BookingStatus)
    .HasConversion<string>()
    .HasMaxLength(50)
    .IsRequired();

// BusConfiguration: the same for BusType and BusStatus
```

Per-property configuration keeps each column's mapping next to the rest of that entity, the same as `UserType`. The alternative is one place in `AppDbContext.ConfigureConventions` (`configurationBuilder.Properties<BookingStatus>().HaveConversion<string>().HaveMaxLength(50);` per enum type). It is less repetitive, but the mapping is hidden from the entity configurations. Pick one style and use it for all four enums.

Docs: [Value conversions](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions), [Pre-convention model configuration](https://learn.microsoft.com/en-us/ef/core/modeling/bulk-configuration#pre-convention-configuration).

### Step 2. Add the migration

```bash
dotnet ef migrations add StoreEnumsAsStrings --project BusBookingSystem.Infrastructure --startup-project BusBookingSystem.API
```

EF generates three `AlterColumn` calls (`int` to `nvarchar(50)`) for `Bookings.BookingStatus`, `Buses.BusType` and `Buses.BusStatus`.

### Step 3. Convert the existing rows (the important part)

`ALTER COLUMN` from `int` to `nvarchar` turns `0` into the text `'0'`, not `'Pending'`. EF would then fail to read those rows. Add the conversion yourself in the **new** migration, before you apply it. Editing a migration that has not been applied yet is normal; the "never hand-edit migrations" rule is about ones already applied.

In `Up`, **after** the `AlterColumn` calls:

```csharp
migrationBuilder.Sql("""
    UPDATE Bookings SET BookingStatus = CASE BookingStatus
        WHEN '0' THEN 'Pending' WHEN '1' THEN 'Confirmed' WHEN '2' THEN 'Cancelled' END;

    UPDATE Buses SET BusType = CASE BusType
        WHEN '0' THEN 'Coaster' WHEN '1' THEN 'Coach' WHEN '2' THEN 'Minibus' WHEN '3' THEN 'DoubleDecker' END;

    UPDATE Buses SET BusStatus = CASE BusStatus
        WHEN '0' THEN 'Available' WHEN '1' THEN 'InUse' WHEN '2' THEN 'UnderMaintenance'
        WHEN '3' THEN 'OutOfService' WHEN '4' THEN 'Retired' END;
    """);
```

In `Down`, do the reverse **before** the `AlterColumn` calls that turn the columns back into `int` (names to `'0'`, `'1'` and so on), or the conversion back to `int` fails.

There is deliberately no `ELSE`. An unexpected value becomes `NULL`, the `NOT NULL` column rejects it, and the migration fails loudly instead of storing a wrong status.

Docs: [Customize migration code](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing#customize-migration-code).

### Optional: restrict the allowed values

A check constraint such as `[BookingStatus] IN ('Pending','Confirmed','Cancelled')` stops bad values written outside EF. It must be updated whenever the enum changes. Worth it for `BookingStatus`; optional for the bus columns.

## Part 2: JSON responses

### Step 4. Register `JsonStringEnumConverter`

ASP.NET Core has two sets of JSON options, and this API uses both:

- **Controllers** use the MVC options: `AddControllers().AddJsonOptions(...)`.
- **`WriteAsJsonAsync`** (used by `GlobalExceptionHandler`) and minimal-API style results use the HTTP options: `ConfigureHttpJsonOptions(...)`. OpenAPI generation also reads JSON settings, so check in Scalar that enums show as strings after the change.

Register the converter in both, in `Program.cs`:

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
```

By default the converter still accepts numbers in request bodies. No request DTO has an enum today, so it does not matter yet. Use `new JsonStringEnumConverter(allowIntegerValues: false)` if you want requests to send names only.

Docs: [Enums as strings](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/customize-properties#enums-as-strings), [Configure System.Text.Json for controllers](https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting#configure-systemtextjson-based-formatters), [Configure JSON options for HTTP results](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses#configure-json-serialization-options).

### Step 5 (optional). Tidy `TripDto.BusType`

`TripDto.BusType` is a `string` filled with `.ToString()`. With the converter in place it could become `BusType`, so the DTO says exactly which values are possible. Either is fine; changing it is a small consistency gain.

## Part 3: tests

- **Service tests** compare enum values (`Assert.Equal(BookingStatus.Pending, ...)`) and use InMemory, which ignores column types. They should pass unchanged.
- **SQL Server tests** run every migration on a fresh database, so they prove the new migration applies. Run them with SQL Server available.
- **Add one API test:** create or look up a booking through `WebApplicationFactory` and assert that the raw JSON contains `"bookingStatus":"Pending"`. That locks in the response format.
- **Add one SQL Server test (optional):** save a booking, read the column with raw SQL, and assert it equals `'Pending'`.

## Part 4: verify and update docs

1. Migrate an existing database (do not drop it first) and check the rows were converted: `SELECT BookingStatus FROM Bookings; SELECT BusType, BusStatus FROM Buses;`
2. Reset to a clean database and run the [clean-database run-through](docs/CLEAN_DATABASE_RUN_THROUGH.md). Rows 7 and 9 should now show `bookingStatus "Pending"`.
3. Update the README: change `"bookingStatus": 0` to `"Pending"` in the example and remove the sentence explaining the numbers.
4. Update `CustomerJourney.http` if any expectation mentions numbers.
5. Run `dotnet test BusBookingSystem.slnx`.

## Commits

One slice, one or two commits, for example:

```text
Store enums as strings in the database
Return enums as strings in API responses
```
