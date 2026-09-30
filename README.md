# Conference Room Booking API

A small ASP.NET Core application for managing conference rooms,
searching for available rooms, and creating bookings with time-based
pricing and optional additional services.

## Features

The API supports the following use cases:

1.  **Create a conference room**

    -   Name
    -   Capacity
    -   Hourly rate
    -   Optional additional services and their prices

2.  **Update a conference room**

    -   Update room details
    -   Add, update, or remove available services

3.  **Delete a conference room**

4.  **Search for available conference rooms**

    -   Search by date/time interval
    -   Filter by minimum capacity
    -   Exclude rooms with overlapping confirmed bookings

5.  **Book a conference room**

    -   Select a room
    -   Select booking start/end time
    -   Select optional additional services
    -   Validate room availability
    -   Calculate the final booking price
    -   Persist the booking and service-price snapshots

## Technology Stack

-   .NET 10
-   ASP.NET Core Web API
-   OpenAPI / Swagger UI
-   Entity Framework Core
-   Microsoft SQL Server
-   NUnit
-   Moq
-   Testcontainers for .NET
-   Respawn
-   Reqnroll
-   Docker / Docker Compose
-   GitHub Actions

## Solution Structure

``` text
ConferenceRoomBooking
├── src
│   ├── ConferenceRoomBooking.Api
│   ├── ConferenceRoomBooking.Application
│   ├── ConferenceRoomBooking.Domain
│   └── ConferenceRoomBooking.Infrastructure
└── tests
    ├── ConferenceRoomBooking.UnitTests
    ├── ConferenceRoomBooking.IntegrationTests
    └── ConferenceRoomBooking.E2ETests
```

### Domain

Contains the core domain entities and enums, including:
The domain project does not depend on infrastructure or HTTP concerns.

### Application

Contains application/business logic such as:

-   room management
-   booking validation
-   booking creation
-   availability rules
-   pricing calculation
-   application interfaces

### Infrastructure

Contains persistence implementation using Entity Framework Core and SQL
Server, including repository implementations and entity configuration.

### API

Contains the HTTP layer:

-   controllers
-   request models
-   response models
-   dependency injection configuration

Controllers are intentionally thin and delegate business logic to the
application layer.

## Web API

The application exposes REST endpoints for room management, room
availability, and booking creation.

The complete API contract, request/response schemas, available
endpoints, and interactive request execution are available through
**Swagger UI** when the API is running:

``` text
/swagger
```

Use Swagger UI as the primary reference for exploring and manually
testing the Web API.

## Booking Rules

Bookings follow these rules:

-   start must be before end
-   bookings must not start in the past
-   start and end must be aligned to full hours
-   bookings must start and finish on the same day
-   bookings must be within `06:00–23:00`
-   the room must exist
-   selected additional services must belong to the selected room
-   a confirmed booking cannot overlap another confirmed booking for the
    same room
-   cancelled bookings do not block availability
-   adjacent bookings are allowed

For example, if an existing booking is `10:00–12:00`, another booking
may start at `12:00`.

The overlap rule is implemented using half-open intervals:

``` text
existing.Start < requested.End
&&
existing.End > requested.Start
```

## Pricing

Room pricing depends on the time of day:

  Time             Multiplier
  -------------- ------------
  06:00--09:00           0.90
  09:00--12:00           1.00
  12:00--14:00           1.15
  14:00--18:00           1.00
  18:00--23:00           0.80

A booking that crosses pricing boundaries is split into pricing segments
and each segment is calculated independently.

For example, for a room with an hourly rate of `1000`:

``` text
08:00–09:00 → 1000 × 0.90 = 900
09:00–10:00 → 1000 × 1.00 = 1000
Total                         = 1900
```

Additional services are then added to the room cost:

``` text
TotalCost = RoomCost + SelectedAdditionalServices
```

An additional service is charged once per booking rather than once per
hour.

## Price Snapshots

When a booking is created, the selected service price is copied into
`BookingAdditionalService`.

This intentionally creates a **price snapshot**.

For example, if a projector costs `50` when a booking is created and its
room configuration is later changed to `70`, the existing booking still
records the original price of `50`.

This prevents changes to current room configuration from altering
historical booking prices.

## Room Update Semantics

Updating a room uses replacement semantics for its available services:

-   a service with an existing ID is updated
-   a service without an ID is added
-   an existing service omitted from the update request is removed
-   a service ID that does not belong to the room is rejected

## Testing Strategy

The solution uses several testing levels rather than relying exclusively
on end-to-end tests.

### Unit Tests

Unit tests cover isolated business logic, including:

-   booking validation
-   pricing rules and pricing boundaries
-   booking creation
-   room existence handling
-   overlap handling
-   selected-service validation
-   booking total-cost calculation
-   duplicate service IDs

Dependencies such as repositories and pricing services are mocked where
appropriate using Moq.

### Repository Integration Tests

Repository tests run against a real SQL Server instance created with
Testcontainers.

They verify behavior such as:

-   entity persistence
-   relationships
-   room/service persistence
-   booking/service persistence
-   availability queries
-   confirmed booking overlap
-   cancelled bookings
-   adjacent booking boundaries

This verifies the actual EF Core mappings and SQL queries rather than
using an in-memory database provider.

### API Integration Tests

API integration tests use `WebApplicationFactory` together with a
Testcontainers SQL Server instance.

They exercise the real HTTP pipeline, application services,
repositories, EF Core, and database.

Examples include:

-   creating and updating rooms
-   validation failures
-   searching available rooms
-   creating bookings
-   booking with additional services
-   persisted booking totals
-   rejecting nonexistent rooms/services
-   rejecting overlapping bookings
-   allowing adjacent bookings

Respawn resets the database between tests while preserving the EF Core
migration history.

### End-to-End Tests

Reqnroll scenarios exercise the application as a black-box HTTP service.

The E2E project intentionally defines its **own request/response
contract models** instead of referencing API DTO classes. This keeps the
tests independent of the API implementation and allows them to detect
contract changes.

The scenarios cover complete business workflows such as:

``` text
Create room
    ↓
Add available services
    ↓
Book room
    ↓
Verify calculated cost and status
```

and cross-feature behavior such as:

``` text
Create room
    ↓
Book room 10:00–12:00
    ↓
Search availability 11:00–13:00
    ↓
Room is no longer available
```

Overlapping bookings and booking-boundary behavior are also covered at
E2E level.

## Database Migrations

The application uses Entity Framework Core migrations. On application
startup, pending migrations are applied automatically.

The database contains the relationships between rooms, available
services, bookings, and booked-service snapshots.

## Running Locally

### Prerequisites

Install:

-   .NET 10 SDK
-   Docker
-   Git

Docker must be running because the integration and E2E tests use
containerized SQL Server instances.

### 1. Clone the repository

``` bash
git clone https://github.com/fastsa85/conference-room-booking.git
cd conference-room-booking
```

### 2. Run the application
There are two options for running the application locally.

### 2.1 Run in Docker
The repository contains docker-compose.e2e.yml, which starts both the API and a SQL Server 2022 instance.

``` bash
docker compose -f docker-compose.e2e.yml up --build
```

The API is available at: http://localhost:8080
EF Core migrations are applied automatically when the API starts.

### 2.2 NET CLI or IDE

The API can be run directly using .NET CLI, or Visual Studio / VS Code or other IDE.

Any local or cloud-hosted SQL Server instance can be used. Configure the DefaultConnection connection string in appsettings.json or using .NET User Secrets.

Then run:
``` bash
dotnet restore
dotnet build
dotnet run --project src/ConferenceRoomBooking.Api
```

Alternatively, open ConferenceRoomBooking.sln in Visual Studio and run the ConferenceRoomBooking.Api project (or use other IDE).

The API is available at: http://localhost:8080
EF Core migrations are applied automatically when the API starts.

### 3.Swagger UI

Once the API is running, the complete API documentation and interactive endpoints are available at /swagger
Open the application URL followed by `/swagger`, for example:

``` text
https://localhost:8080/swagger
```

## Running Tests

Run the complete test suite:

``` bash
dotnet test
```

Tests can also be filtered by category.

Unit tests:

``` bash
dotnet test --filter "Category!=SqlIntegration&Category!=ApiIntegration"
```

Repository integration tests:

``` bash
dotnet test --filter "Category=SqlIntegration"
```

API integration tests:

``` bash
dotnet test --filter "Category=ApiIntegration"
```

Integration tests require Docker because SQL Server is started
dynamically using Testcontainers.

## CI

GitHub Actions is used to validate the project automatically.

The CI workflow performs the main validation stages, including:

``` text
Restore
  ↓
Build
  ↓
Unit tests
  ↓
Integration tests
  ↓
Start E2E environment (Docker Compose)
  ↓
Wait for API readiness
  ↓
E2E tests
  ↓
Environment cleanup
```

## Design Decisions and Trade-offs

### Simple layered architecture

The solution intentionally uses a small layered architecture instead of
introducing CQRS, MediatR, generic repositories, domain events, or
additional services.

Those patterns can be useful in larger systems, but the current scope
does not justify the additional abstraction.

### Real database integration testing

SQL Server Testcontainers are used instead of EF Core's in-memory
provider so that integration tests exercise real relational behavior,
foreign keys, migrations, and SQL queries.

### Historical prices are immutable snapshots

Selected additional-service prices are persisted with the booking.
Historical bookings therefore do not depend on the current service price
configured for a room.

### Availability and booking use the same overlap semantics

Availability search and booking creation use the same logical definition
of an overlap. Adjacent bookings are therefore allowed consistently
across both workflows.

### Concurrency

Booking creation currently performs an application-level availability
check before inserting the booking.

### Scope

Functionality outside the stated requirements was intentionally not
added. For example, the solution does not introduce
authentication/authorization, a full booking-management API, or
additional distributed-system infrastructure.

The focus is on implementing the requested behavior with clear business
rules and meaningful automated coverage.
