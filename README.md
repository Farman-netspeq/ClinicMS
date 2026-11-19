# Clinic Management System (ClinicMS)

A multi-department outpatient clinic management system built on Clean Architecture (.NET 10).

## Tech Stack

- **.NET 10** (Domain / Shared / Application / Infrastructure / Api / Web)
- **SQL Server** (local instance, Windows Authentication) — database-first (hand-written SQL, no EF migrations/scaffolding)
- **Bootstrap 5**, **jQuery**, **CustomAjax.js** (shared AJAX helper — data-attribute driven, no hand-written `$.ajax` for standard CRUD)
- Visual Studio 2022, Git

## Prerequisites

- .NET 10 SDK
- SQL Server (local instance) with Windows Authentication enabled
- Visual Studio 2022 (or VS Code + C# Dev Kit)
- Git

## Database Setup

This project is **database-first**. There are no EF Core migrations and no scaffolding.

1. Create the database in SSMS.
2. Run the hand-written SQL scripts under `/Database/Scripts` in order (tables use the `utblCMS` prefix, stored procedures use `udspCMS`).
3. Run the seed script — it inserts the predefined roles (Admin, Receptionist, Doctor) and the default Admin and Receptionist users below. No `dotnet ef migrations add` / `database update` is used anywhere in this project.

## Connection String

Update `ClinicMS.Api/appsettings.json` (and `ClinicMS.Web/appsettings.json` if applicable) with your local SQL Server instance, Windows Authentication:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ClinicMSDB;Trusted_Connection=True;TrustServerCertificate=true;"
}
```

> Local dev values only. Never commit real secrets — use user-secrets or environment variables outside local dev.

## Default Logins

| Role | Email | Password |
|---|---|---|
| Admin | admin@clinicms.com | Admin@123 |
| Receptionist | receptionist@clinicms.com | Reception@123 |

## Running the Application

1. Open the solution in Visual Studio.
2. Set **both** `ClinicMS.Api` and `ClinicMS.Web` as startup projects (Solution → Properties → Multiple startup projects → Start for both).
3. Confirm the Web project's `ApiBaseUrl` setting matches the Api project's local URL/port.
4. Run (F5). Log in with one of the default accounts above.

## Project Structure

| Project | Responsibility |
|---|---|
| `ClinicMS.Domain` | Entities, enums — no framework dependencies |
| `ClinicMS.Shared` | `Result<T>`, `PaginatedResult<T>`, `ApiResponseDto<T>` |
| `ClinicMS.Application` | DTOs (Request/Response/ListItem/Filter) and service interfaces |
| `ClinicMS.Infrastructure` | `ApplicationDbContext`, EF entity configurations, service implementations, DI registration |
| `ClinicMS.Api` | REST controllers, JWT auth, `ApiResponseDto<T>` responses |
| `ClinicMS.Web` | MVC controllers (via `IHttpService`), Razor views, CustomAjax.js-driven screens, cookie auth |

## Architecture Rules

- Interface in Application → implementation in Infrastructure → registered in `Infrastructure/DependencyInjection.cs` → inject the interface. Exception: `IHttpService`/`HttpService` live in Web.
- No Unit of Work, no Repository, no MediatR, no AutoMapper, no ViewModels — DTOs only, manual mapping.
- Data access via `ApplicationDbContext` through stored procedures (`FromSqlRaw`/`SqlQueryRaw`/`ExecuteSqlRawAsync`) — no direct LINQ queries against the context.
- Every Web POST action carries `[ValidateAntiForgeryToken]`; corresponding views render `@Html.AntiForgeryToken()`.

## Known Limitations

- Auto-numbering (Patient/Appointment/Invoice) derives the next number from the current max and is **not concurrency-safe** under heavy simultaneous load. Acceptable for this assignment's scope; flagged for future work.
- Stretch goals (reschedule, doctor leave/blocked slots, patient self-service portal, PDF export, dashboard charts, email/SMS reminders) are not implemented in this milestone.

## Milestones Delivered

M0 (setup) → M1 (Departments/Doctors/Schedules) → M2 (Patients) → M3 (Booking) → M4 (Lifecycle + doctor dashboard) → M5 (Medical records/prescriptions) → M6 (Billing) → M7 (Dashboard/audit/polish) — all complete.
