# Day 6 Walkthrough - Apply Authentication to the Booking Journey

Day 6 adds identity to the customer journey. The goal is not to build a full identity platform; it is to ensure that a customer can register, log in, receive a JWT, and create or view bookings as the authenticated user.

```text
register
  -> login
  -> receive JWT
  -> send Bearer token
  -> create booking as authenticated user
  -> access only permitted booking data
```

The project remains a lightweight layered API:

```text
Core <- Application <- Infrastructure <- API
```

Do not add a repository layer for authentication. Keep the existing service-first approach and use `AppDbContext` directly inside Infrastructure services.

## 1. Current project state

Before Day 6 implementation, the repository already has:

- a `User` entity,
- `UserType` and booking-related domain concepts,
- booking creation through `IBookingService`,
- a booking controller,
- public depot and trip endpoints.

The current repository does not yet contain:

- JWT authentication configuration,
- register/login DTOs,
- an authentication service,
- a token service,
- an authentication controller,
- `[Authorize]` on booking endpoints.

The current booking request also contains `UserId`. That is acceptable for the pre-authentication prototype, but it should not remain the source of truth after Day 6. A client must not be able to submit another customer's ID and create a booking for that person.

## 2. User story

> As a customer, I want to register and log in so that my booking belongs to me.

## 3. Scope for this day

Build only the authentication needed by the booking journey:

- register a customer,
- log in with credentials,
- issue a signed JWT,
- authenticate requests using the Bearer scheme,
- protect booking creation and booking lookup,
- keep depot and trip browsing public.

Do not build these features today:

- refresh tokens,
- password reset emails,
- social login,
- multi-factor authentication,
- admin permissions,
- payment authentication,
- a separate Customer entity.

Those can be added later without changing the basic flow.

## 4. Acceptance criteria

Day 6 is complete when:

- a valid customer can register,
- duplicate usernames or emails are rejected,
- passwords are stored as hashes, never plain text,
- a valid login returns a JWT,
- invalid credentials are rejected,
- an unauthenticated request cannot create a booking,
- an authenticated customer can create a booking,
- the booking owner comes from the token, not from a trusted request-body `UserId`,
- public users can still browse depots and trips,
- an authenticated booking lookup cannot expose another user's booking.

## 5. Define focused contracts first

### Register request

```csharp
public class RegisterRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

### Login request

```csharp
public class LoginRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

### Authentication response

```csharp
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}
```

Keep these DTOs separate from `User`. Never return the entity directly because it contains internal identity and account fields.

## 6. Decide how the user identity flows

The important change to the booking use case is the owner identity.

Before authentication:

```text
request.UserId -> BookingService -> Booking.UserId
```

After authentication:

```text
JWT subject/claim -> Controller -> BookingService -> Booking.UserId
```

The request should contain only booking choices such as:

```csharp
public class BookingRequestDto
{
    public Guid TripId { get; set; }
    public int NumberOfSeats { get; set; }
    public int CargoWeight { get; set; }
}
```

The controller reads the authenticated user's ID from `HttpContext.User` and passes it to the service, either as a separate argument or through a request context abstraction already used by the project.

Do not trust a body field for ownership once authentication exists.

## 7. Password handling

Use a password hasher rather than implementing hashing manually. For a lightweight MVP, ASP.NET Core's `PasswordHasher<TUser>` is sufficient.

The registration flow is:

```text
validate request
  -> check username/email uniqueness
  -> create User with UserType.Customer
  -> hash password
  -> save user
  -> return safe user/auth response
```

The `User` entity currently needs a persisted password hash for this approach. That is a domain/model decision that should be made before implementation. Do not store the submitted password in the database.

## 8. JWT issuing

The token should contain a stable user identifier, normally as the subject claim, plus useful non-sensitive claims such as username and user type.

Conceptually:

```text
user ID -> token subject
username -> name claim
user type -> role claim
```

The API validates the token using the configured:

- signing key,
- issuer,
- audience,
- expiration rules.

Keep the signing key in development configuration or environment secrets. Never commit a production signing key to source control.

## 9. API configuration

Configure authentication in the API project:

```text
AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  -> AddJwtBearer(...)

AddAuthorization()
```

Middleware order matters:

```text
UseHttpsRedirection()
UseAuthentication()
UseAuthorization()
MapControllers()
```

`UseAuthentication()` must run before `UseAuthorization()`. Without it, the API will not populate the current user from the Bearer token.

## 10. Endpoint boundaries

Authentication endpoints:

```text
POST /api/auth/register
POST /api/auth/login
```

Public endpoints:

```text
GET /api/depots
GET /api/trips
GET /api/trips/search
GET /api/trips/{id}
```

Protected endpoints:

```text
POST /api/bookings
GET /api/bookings/{id}
```

Apply `[Authorize]` to protected controllers or actions. Keep the read-only discovery journey public so a customer can search before logging in.

## 11. Booking ownership rule

The booking service must use the authenticated user ID when creating a booking.

The service should verify:

- the authenticated user exists,
- the trip exists,
- the requested seats are available,
- the booking is created for that user,
- the transaction includes the booking and trip availability update.

For a lookup, filter by both values:

```text
booking ID == requested ID
AND booking UserId == authenticated UserId
```

Do not load by booking ID alone and then assume the caller is allowed to see it.

## 12. Manual verification sequence

### Test 1: register

Call `POST /api/auth/register` with a valid customer payload.

Expected:

- success response,
- user created,
- no password returned,
- stored password is a hash.

### Test 2: duplicate registration

Register the same username or email again.

Expected:

- clear conflict or validation response,
- no duplicate user.

### Test 3: login

Call `POST /api/auth/login` with valid credentials.

Expected:

- success response,
- JWT returned,
- expiry returned,
- no password data returned.

### Test 4: invalid login

Use an incorrect password.

Expected:

- unauthorized response,
- no token.

### Test 5: anonymous booking

Call `POST /api/bookings` without a Bearer token.

Expected:

- HTTP 401,
- no booking row,
- no trip availability change.

### Test 6: authenticated booking

Log in, add the token as a Bearer token, and create a valid booking.

Expected:

- success response,
- booking belongs to the logged-in user,
- seat/cargo counters update within the existing transaction.

### Test 7: ownership boundary

Log in as a second user and request the first user's booking.

Expected:

- HTTP 404 or 403 according to the chosen policy,
- first user's booking details are not exposed.

## 13. Focused automated tests

The highest-value tests are:

1. registers a customer with a hashed password,
2. rejects duplicate registration,
3. returns a token for valid credentials,
4. rejects invalid credentials,
5. rejects anonymous booking creation,
6. creates a booking for the authenticated user,
7. prevents one user from reading another user's booking,
8. keeps depot and trip browsing public.

Prefer real service/database behavior tests over tests that only verify that a mock method was called.

## 14. Day 6 completion checklist

- [ ] Register and login DTOs exist.
- [ ] Authentication service contract exists in Application.
- [ ] Password hashing is implemented.
- [ ] JWT configuration exists in API settings.
- [ ] JWT bearer authentication is registered.
- [ ] Authentication middleware runs before authorization middleware.
- [ ] Auth controller exposes register and login.
- [ ] Booking creation requires `[Authorize]`.
- [ ] Booking ownership comes from the authenticated identity.
- [ ] Public depot and trip endpoints still work anonymously.
- [ ] Manual register -> login -> booking journey succeeds.
- [ ] Anonymous and cross-user access tests fail safely.

## Summary

Day 6 changes the meaning of a booking from:

```text
someone submits a user ID and a trip
```

to:

```text
an authenticated customer selects a trip
  -> the API identifies the customer from the token
  -> the service creates the booking for that customer
```

The core architectural pattern stays the same. Authentication is another vertical slice across Application, Infrastructure, and API, while trip browsing remains public and booking ownership becomes trustworthy.
