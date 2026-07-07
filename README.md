# ClinicMS — Clinic Management System

Clean Architecture solution: Domain, Shared, Application, Infrastructure, Api, Web.

## Tech Stack
.NET Core, ASP.NET Core MVC/Web API, Entity Framework Core, SQL Server, Bootstrap, Tailwind CSS, ReactJS.

## Prerequisites
- .NET 8 SDK (or version used)
- SQL Server (Express or full) + SQL Server Management Studio (optional)
- Visual Studio 2022

## Setup

### 1. Clone repo

git clone https://github.com/Farman-netspeq/ClinicMS.git

cd ClinicMS

### 2. Connection string
Update `ClinicMS.Api/appsettings.json` (and `ClinicMS.Web/appsettings.json` if applicable):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=ClinicMSDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
}
```
Replace `YOUR_SERVER`, `YOUR_USER`, `YOUR_PASSWORD` with your local SQL Server details.

### 3. Apply migrations
Open Package Manager Console in Visual Studio:
- Default project: `ClinicMS.Infrastructure`
- Startup project: `ClinicMS.Api`

Run:
Update-Database
This creates database `ClinicMSDB` with all tables + seeds default admin (see below).

### 4. Default admin login

Email: admin@clinicms.com
Password: Admin@123


### 5. Run API and Web together
Right-click Solution → **Set Startup Projects** → Multiple startup projects → set both `ClinicMS.Api` and `ClinicMS.Web` to **Start**.

Press F5. Both launch:
- API: `https://localhost:7xxx` (Swagger UI at `/swagger`)
- Web: `https://localhost:7145`

Login via Web app using default admin credentials above.

## Project Structure
- **Domain** — Entities, enums
- **Shared** — Result<T>, PaginatedResult<T>, ApiResponseDto<T>
- **Application** — DTOs, interfaces
- **Infrastructure** — DbContext, EF configs, service implementations
- **Api** — Web API controllers
- **Web** — MVC controllers, views (uses IHttpService to call Api)

## Notes
- CORS origins configured in `appsettings.json` under `CorsSettings:AllowedOrigins`
- JWT settings under `Jwt` section in `appsettings.json`
