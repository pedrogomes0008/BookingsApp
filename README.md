# BookingsApp

An appointment booking app for small service businesses such as hair salons and clinics: customers request a time slot, and the business approves, reschedules or cancels it.

> **Status:** under active development — see the [roadmap](#roadmap).

## About

I'm a developer specialised in Umbraco CMS, and I built this project from scratch to deepen my knowledge of .NET beyond the CMS: domain modelling, testing, persistence, APIs, authentication and deployment.

The business rules come from a real booking module I designed for Umbraco sites, rebuilt here as a standalone ASP.NET Core application with a layered architecture.

## Tech stack

- .NET 10 / C#
- ASP.NET Core and Blazor
- Entity Framework Core with SQL Server
- xUnit

## Project structure

| Project | Responsibility |
| --- | --- |
| `Bookings.Core` | Domain model (the `Booking` entity and its state transitions) and application services. No dependencies on infrastructure or web. |
| `Bookings.Infrastructure` | EF Core `DbContext`, entity configuration, migrations and the repository implementation. |
| `Bookings.Web` | ASP.NET Core / Blazor host: configuration, dependency injection and, later, the API and UI. |
| `Bookings.Core.Tests` | Unit tests for the domain and application services. |
| `Bookings.Infrastructure.Tests` | Integration tests for the repository against a real SQL Server database. |

Dependencies point inwards: `Web` → `Infrastructure` → `Core`. The domain defines the repository interface and the infrastructure implements it.

### Domain highlights

- A booking is a small state machine: `Pending` → `Confirmed` / `Rejected` / `Expired`, and `Confirmed` → `Cancelled` / `NoShow`, plus rescheduling. Every transition is a method on the entity that enforces its own rules.
- A pending request expires after an approval deadline: the shorter of a configurable maximum window (24 hours by default) and half of the time left until the appointment starts.
- Time is never read from the system clock inside the domain: it is passed in, and services use an injected `TimeProvider`, which keeps every time-based rule testable.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express or LocalDB is enough)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

### Database

The development connection string is in `Bookings.Web/appsettings.Development.json` (`ConnectionStrings:Bookings`) and points to a local SQL Server Express instance. Adjust it to your setup, then create the database:

```bash
dotnet ef database update --project Bookings.Infrastructure --startup-project Bookings.Web
```

### Tests

```bash
dotnet test
```

The integration tests create and reset their own database (`BookingsApp_Tests`) on the same SQL Server instance before each test.

## Roadmap

- [x] Phase 0 – Solution setup with the .NET CLI
- [x] Phase 1 – Domain model: booking state machine, approval deadline, TimeProvider, unit tests
- [x] Phase 2 – Persistence: repository pattern, async, EF Core, migrations, SQL Server, integration tests
- [ ] Phase 3 – API: application services, Minimal APIs, validation, ProblemDetails, OpenAPI, API tests
- [ ] Phase 4 – Authentication and authorization: ASP.NET Core Identity, roles and policies
- [ ] Phase 5 – Blazor UI: day calendar, pending requests, booking details
- [ ] Phase 6 – Production readiness: Options pattern, background jobs, logging, CI, Docker, deployment
