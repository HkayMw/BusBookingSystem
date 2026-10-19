# Clean-Database Run-Through

This checklist proves the whole customer journey works from a freshly seeded database. Run it before a release, after changing the seed data, or after any change to booking, trip or auth behaviour.

The rows follow the order of [`BusBookingSystem.API.http`](../../BusBookingSystem.API/BusBookingSystem.API.http): Part A (happy path), then Part B (edge cases and failure paths), then two checks outside the file. Each row's ID matches the request heading in the `.http` file.

## Run record

| Field | Value |
|---|---|
| Date | |
| Commit | |
| Run by | |
| Result | |

## 1. Reset to a clean database

The seeder creates trips for **tomorrow**, relative to the day it runs, and only seeds an empty database. Reset before every run so the dates and the expected numbers line up.

```bash
dotnet ef database drop --force --project BusBookingSystem.Infrastructure --startup-project BusBookingSystem.API
dotnet ef database update      --project BusBookingSystem.Infrastructure --startup-project BusBookingSystem.API
dotnet user-secrets list --project BusBookingSystem.API     # JwtSettings:Secret must be set
dotnet run --project BusBookingSystem.API --launch-profile https
```

Use the `https` profile. Over plain HTTP, the redirect to HTTPS drops the `Authorization` header and the protected requests fail with 401.

Open `https://localhost:7278/scalar` to confirm the API is up.

## 2. Run the journey

Open `BusBookingSystem.API/BusBookingSystem.API.http` and send the requests top to bottom, once. Later requests reuse IDs and the token from earlier responses, and the numbers in Part B depend on the bookings made in Part A.

Fill in the **Actual** column. Every row must match before the run counts as passed.

Seeded fares are in Malawi kwacha (MWK). Depots are listed by name: `[0]` Blantyre City, `[1]` Dedza Depot, `[2]` Lilongwe Central, `[3]` Mzuzu City.

### Part A: happy path

| # | ID | Request | Expected | Actual |
|---|---|---|---|---|
| 1 | A1 | `GET /api/depots` | 200, "Active depots retrieved successfully.", four depots by name (Blantyre City, Dedza Depot, Lilongwe Central, Mzuzu City); Jenda Depot (inactive) not listed | |
| 2 | A2 | `POST /api/auth/register` `demo@example.com` | 201, "User registered successfully." | |
| 3 | A3 | `POST /api/auth/login` `demo@example.com` | 200, "User login successfull", `data.token` holds the JWT | |
| 4 | A4 | `GET /api/trips` | 200, "All trips retrieved successfully.", the five seeded trips | |
| 5 | A5 | `GET /api/trips/search` Lilongwe → Blantyre, tomorrow | 200, "Trips searched successfully.", one trip at 14:00; the Lilongwe → Mzuzu trip is not listed | |
| 6 | A6 | `GET /api/trips/{id}` Lilongwe → Blantyre, before booking | 200, "Trip retrieved successfully.", `availableSeats` 56, `availableCargoCapacity` 1000 | |
| 7 | A7 | `POST /api/booking` 2 seats + 10 kg cargo, Lilongwe → Blantyre | 201, "Booking created successfully.", `seatsFare` 140000.00, `cargoFare` 8000.00, `totalFare` 148000.00, `bookingStatus` Pending | |
| 8 | A8 | `GET /api/trips/{id}` Lilongwe → Blantyre, after booking | 200, "Trip retrieved successfully.", `availableSeats` 54, `availableCargoCapacity` 990 | |
| 9 | A9 | `GET /api/booking/{bookingId}` (owner) | 200, "Booking retrieved successfully.", same `bookingId`, `customerName` "Demo Customer", stored fares 140000.00 + 8000.00 = 148000.00 | |
| 10 | A10 | `GET /api/trips/search` Lilongwe → Dedza, tomorrow | 200, "Trips searched successfully.", one trip at 09:00 on the minibus (no cargo space) | |
| 11 | A11 | `POST /api/booking` 1 seat, Lilongwe → Dedza | 201, "Booking created successfully.", `totalFare` 30000.00 | |
| 12 | A12 | `GET /api/trips/search` Blantyre → Lilongwe, tomorrow | 200, "Trips searched successfully.", one trip at 07:00, `seatFare` 70000.50 | |
| 13 | A13 | `POST /api/booking` 1 seat, Blantyre → Lilongwe | 201, "Booking created successfully.", `totalFare` 70000.50 (not truncated to 70000) | |
| 14 | A14 | `GET /api/trips/search` Mzuzu → Lilongwe, tomorrow | 200, "Trips searched successfully.", one trip at 07:00, `cargoFare` null | |

### Part B: edge cases and failure paths

| # | ID | Request | Expected | Actual |
|---|---|---|---|---|
| 15 | B1 | `GET /api/trips/search` Lilongwe → Blantyre, 30 days ahead | 200, "Trips searched successfully.", empty list (not an error) | |
| 16 | B2 | `GET /api/trips/search` same origin and destination | 400, "Origin and destination depot IDs cannot be the same." | |
| 17 | B3 | `GET /api/trips/{random guid}` | 404, "Trip was not found." | |
| 18 | B4 | `POST /api/auth/register` `DEMO@Example.COM` | 409, "The Email 'DEMO@Example.COM' already belongs to an existing acount." (emails are case-insensitive) | |
| 19 | B5 | `POST /api/auth/login` `DEMO@Example.COM` | 200, "User login successfull", token in `data.token` | |
| 20 | B6 | `POST /api/auth/login` wrong password | 401, "Invalid email or password." | |
| 21 | B7 | `POST /api/booking` with no token | 401, empty body (rejected by `[Authorize]`) | |
| 22 | B8 | `POST /api/booking` 57 seats, Lilongwe → Blantyre | 409, "Not enough seats available. Requested: 57, Available: 54." | |
| 23 | B9 | `POST /api/booking` 1001 kg cargo, Lilongwe → Blantyre | 409, "Not enough cargo capacity available. Requested: 1001, Available: 990." | |
| 24 | B10 | `POST /api/booking` 5 kg cargo, Lilongwe → Dedza (minibus) | 409, "This bus does not support cargo bookings." | |
| 25 | B11 | `POST /api/booking` 5 kg cargo, Mzuzu → Lilongwe (no cargo fare) | 409, "Cargo bookings are not available for this trip." | |
| 26 | B12 | `POST /api/booking` 0 seats and 0 cargo | 400, "A booking must include at least one seat or cargo quantity." | |
| 27 | B13 | `POST /api/booking` unknown trip | 404, "Trip was not found." | |
| 28 | B14 | `GET /api/booking/{random guid}` | 404, "Booking not found." | |
| 29 | B15 | `GET /api/booking/{bookingId}` with no token | 401, empty body (rejected by `[Authorize]`) | |

### Outside the `.http` file

| # | ID | Check | Expected | Actual |
|---|---|---|---|---|
| 30 | R1 | Stop and restart the API, then send A1 and A4 again | Still four depots and five trips: the seeder does not duplicate data | |
| 31 | R2 | `dotnet test BusBookingSystem.slnx` | All tests pass (SQL Server tests skip when no server is reachable) | |

## Notes

- "User login successfull" and "existing acount" are typos in the current API messages. When they are fixed, update the expected text here and in the `.http` file.
- If a row fails, fix the cause before moving on. A failure here is exactly what a reviewer would hit.
