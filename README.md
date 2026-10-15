# Maintenance Request Management System

A robust and secure enterprise Backend API for managing and tracking facility maintenance requests. It enables employees to submit requests, maintenance administrators to manage types and assign technicians, and technicians to update request lifecycles through resolution and closure with strict domain validation and role-based access control.

---

## 1. Project Overview & Roles

The system uses a custom role-based permission model with three distinct operational roles:

* **Employee:** Submits maintenance requests, tracks their lifecycle, and views personal request history.
* **Technician:** Views assigned maintenance requests, initiates work, and marks requests as resolved.
* **MaintenanceAdmin:** Manages maintenance types (create, update, activate, deactivate), assigns requests to technicians, manages users and account status, and rejects or closes requests.

---

## 2. Tech Stack

* **Runtime & Framework:** C# / .NET 10 (C# 13)
* **API Style:** ASP.NET Core Web API (Controllers)
* **Architecture:** Onion Architecture with CQRS via MediatR
* **Persistence:** SQL Server (LocalDB) with Entity Framework Core Code-First Migrations
* **Caching:** Redis (Memurai on Windows) with StackExchange.Redis
* **Security & Auth:** Custom Authentication (No ASP.NET Core Identity), BCrypt.Net-Next hashing, JWT Bearer tokens with sequential Guid v7 primary keys
* **Validation:** FluentValidation with MediatR Pipeline ValidationBehavior
* **Error Handling:** RFC 7807 ProblemDetails standard error responses

---

## 3. Architecture

The solution strictly follows Onion Architecture, organized into four clean layers with dependencies pointing inward:

* **MaintenanceSystem.Domain:** Core enterprise domain containing entities (Rich Domain Model), behavior methods, business invariants, and domain exceptions with zero external dependencies.
* **MaintenanceSystem.Application:** Application use-case orchestration organized by vertical slices (Commands/Queries), MediatR handlers, and abstraction interfaces.
* **MaintenanceSystem.Infrastructure:** Technical implementation of interfaces (EF Core AppDbContext, BCrypt password hashing, JWT token service, and Redis caching).
* **MaintenanceSystem.API:** HTTP entry point, controller routing, request contract mapping, and middleware pipeline.

For detailed architectural guidelines and rules, refer to: [Architecture Guide](docs/architecture-guide.md).

---

## 4. Custom Authentication Decision

The project explicitly implements a custom authentication system rather than relying on ASP.NET Core Identity or pre-packaged `AspNet*` tables:
* **Custom Schema:** `Users`, `Roles`, and `UserRoles` tables with sequential Guid v7 primary keys.
* **Password Hashing:** BCrypt algorithm with a documented Work Factor of 11.
* **Lockout Policy:** Accounts are locked for 15 minutes after 5 consecutive failed login attempts.
* **Immediate Token Revocation (SecurityStamp):** Every token includes a security stamp claim validated on every request. Updating passwords, changing roles, or deactivating accounts immediately revokes existing tokens.

For the architectural decision rationale, refer to: [ADR-0001: Custom Authentication](docs/adr/0001-custom-auth.md).

---

## 5. Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* SQL Server LocalDB or SQL Server Express
* Memurai Developer Edition (Redis for Windows) running on default port `127.0.0.1:6379`
* PowerShell or Visual Studio 2026 / VS Code

---

## 6. Setup & Getting Started

### 1. Clone Repository & Restore Dependencies
```bash
git clone https://github.com/MohammedBaasheem/MaintenanceSystem.git
cd MaintenanceSystem
dotnet restore
```

### 2. Configure Development User Secrets
Configure the JWT signing key and initial seed passwords in User Secrets:
```bash
cd src/MaintenanceSystem.API
dotnet user-secrets set "Jwt:SigningKey" "YOUR_SUPER_SECRET_SIGNING_KEY_AT_LEAST_64_CHARS_LONG_HERE"
dotnet user-secrets set "Seed:AdminPassword" "YourAdminPasswordHere123!"
dotnet user-secrets set "Seed:EmployeePassword" "YourEmployeePasswordHere123!"
dotnet user-secrets set "Seed:TechnicianPassword" "YourTechnicianPasswordHere123!"
cd ../..
```

### 3. Apply Database Migrations
```bash
dotnet ef database update --project src/MaintenanceSystem.Infrastructure --startup-project src/MaintenanceSystem.API
```

### 4. Run the API Application
```bash
dotnet run --project src/MaintenanceSystem.API
```
The API listens by default on: `http://localhost:5188`.

### 5. Run Automated Tests
```bash
dotnet test
```
Executes 132 automated tests across Domain (68), Application (53), and API Integration (11) layers with isolated SQLite in-memory persistence and zero external service dependencies.

---

## 7. Development Accounts

The `DevelopmentDataSeeder` automatically creates the following initial accounts upon first run in the `Development` environment:

| Username | Email | Role | Password Secret Key |
|:---------|:------|:-----|:--------------------|
| `admin` | `admin@system.local` | `MaintenanceAdmin` | `Seed:AdminPassword` |
| `employee` | `employee@system.local` | `Employee` | `Seed:EmployeePassword` |
| `technician` | `technician@system.local` | `Technician` | `Seed:TechnicianPassword` |

---

## 8. API Examples

### Login (POST /api/auth/login)
```http
POST /api/auth/login
Content-Type: application/json

{
  "userName": "admin",
  "password": "YourAdminPasswordHere123!"
}
```
**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-10-14T23:59:59Z"
}
```

### Create Maintenance Type (POST /api/maintenance-types)
```http
POST /api/maintenance-types
Authorization: Bearer <AdminToken>
Content-Type: application/json

{
  "name": "HVAC Systems",
  "description": "Heating, ventilation, and air conditioning repair and servicing"
}
```

### Create Maintenance Request (POST /api/maintenance-requests)
```http
POST /api/maintenance-requests
Authorization: Bearer <EmployeeToken>
Content-Type: application/json

{
  "title": "AC unit leaking in Room 302",
  "description": "Water leakage observed and cooling performance is reduced",
  "priority": 2,
  "maintenanceTypeId": "019273c5-9271-7000-8000-000000000001"
}
```

---

## 9. HTTP Requests Collection

A comprehensive interactive HTTP test collection for the **VS Code REST Client** extension is provided:
* File path: [`requests/maintenance.http`](requests/maintenance.http)
* Covers over 20 end-to-end scenarios including authentication, user management, maintenance types, full request lifecycles, and error responses (401, 403, 404, 409, 423).

---

## 10. References & Documentation

For technical specifications and architectural documentation:
1. [Architecture Guide](docs/architecture-guide.md): Project tree structure, layering boundaries, and flow of execution.
2. [Database Design](docs/database-design.md): Complete data dictionary, schema diagrams, and indexing strategy.
3. [ADR-0001: Custom Auth](docs/adr/0001-custom-auth.md): Architectural decision record for the custom authentication system.
4. [Acceptance Checklist](docs/acceptance-checklist.md): Critical acceptance scenarios, lockout testing, and resilience results.
5. [Development Plan](docs/development-plan.md): Step-by-step master development plan for all project milestones.
6. [Final Review Report](docs/final-review.md): Repository review, security audit, and quality gate report.
