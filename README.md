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

# ClinicMS — Clinic Management System (Milestone 1)

ClinicMS is a Clinic Management System built using **Clean Architecture** principles with ASP.NET Core, Entity Framework Core, SQL Server, and ASP.NET Core MVC.

This milestone establishes the project foundation and implements the first functional module with authentication, role-based authorization, and department management.

---

# Architecture

The solution follows Clean Architecture with clear separation of responsibilities.

```
ClinicMS
│
├── ClinicMS.Domain          → Entities, Enums
├── ClinicMS.Shared          → Common models, Result<T>, Response wrappers
├── ClinicMS.Application     → DTOs, Interfaces, Contracts
├── ClinicMS.Infrastructure  → EF Core, DbContext, Services
├── ClinicMS.Api             → REST APIs
└── ClinicMS.Web             → ASP.NET Core MVC UI
```

---

# Tech Stack

- .NET 8
- ASP.NET Core MVC
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Bootstrap
- Tailwind CSS
- JWT Authentication
- Clean Architecture

---

# Implemented Features (Milestone 1)

## Authentication

- User Login
- JWT Token generation
- Secure API authorization
- Password hashing
- Session-based authentication in MVC

---

## Department Management

Complete Department CRUD operations:

- Add Department
- Edit Department
- View Department List
- Search Departments
- Server-side Pagination
- Soft Delete / Activate
- Validation
- Duplicate department name validation

---

## API Features

- RESTful APIs
- Standard API Response wrapper
- Proper HTTP Status Codes
- Global Exception Handling
- Dependency Injection
- CORS Configuration

---

## Web Features

- Responsive MVC UI
- AJAX-based CRUD operations
- Toast notifications
- Client-side validation
- API integration using IHttpService

---

## Database

Entity Framework Core migrations included.

Creates:

- Departments
- Users
- Roles
- Required supporting tables

Default admin user is seeded automatically.

---

# Prerequisites

- Visual Studio 2022
- .NET 8 SDK
- SQL Server

---

# Setup

## 1. Clone Repository

```bash
git clone https://github.com/Farman-netspeq/ClinicMS.git

cd ClinicMS
```

---

## 2. Configure Connection String

Update:

```
ClinicMS.Api/appsettings.json
```

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=ClinicMSDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
}
```

---

## 3. Apply Database Migrations

Package Manager Console

**Default Project**

```
ClinicMS.Infrastructure
```

**Startup Project**

```
ClinicMS.Api
```

Run

```powershell
Update-Database
```

---

## 4. Default Admin Credentials

Email

```
admin@clinicms.com
```

Password

```
Admin@123
```

---

## 5. Run the Solution

Set Startup Projects:

- ClinicMS.Api
- ClinicMS.Web

Run the solution.

API

```
https://localhost:xxxx/swagger
```

Web

```
https://localhost:7145
```

---

# Project Structure

## Domain

Contains:

- Entities
- Enums

---

## Shared

Contains:

- Result<T>
- ApiResponseDto<T>
- PaginatedResult<T>

---

## Application

Contains:

- DTOs
- Interfaces
- Contracts

---

## Infrastructure

Contains:

- DbContext
- Entity Configurations
- Service Implementations
- EF Core Migrations

---

## API

Contains:

- Controllers
- Authentication
- Middleware
- Swagger

---

## Web

Contains:

- MVC Controllers
- Views
- AJAX Integration
- IHttpService

---

# Configuration

## JWT

JWT settings are available under:

```
Jwt
```

section in

```
appsettings.json
```

---

## CORS

Allowed origins can be configured under

```
CorsSettings:AllowedOrigins
```

---

# Milestone 1 Summary

✔ Clean Architecture implementation

✔ JWT Authentication

✔ Role-based Authorization

✔ Department CRUD

✔ Search & Pagination

✔ Soft Delete / Restore

✔ Validation

✔ API & MVC integration

✔ Entity Framework Core

✔ SQL Server

✔ Dependency Injection

✔ Swagger
