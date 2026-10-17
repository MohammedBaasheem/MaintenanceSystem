# Learning Docs Tasks

> قائمة المهام لإنشاء ملفات تعليمية تحت docs/learning/.

## Progress

| # | File | Status | Priority |
|---|------|--------|----------|
| 1 | flows/02-Create-Request.md | ✅ | High |
| 2 | flows/03-Lifecycle-Operations.md | ✅ | High |
| 3 | flows/04-Query-Operations.md | ✅ | High |
| 4 | 00-README.md | ✅ | Medium |
| 5 | 01-Getting-Started.md | ✅ | Medium |
| 6 | 02-Request-Journey.md | ✅ | Medium |
| 7 | 03-Architecture-Overview.md | ✅ | Medium |
| 8 | cross-cutting/01-Exception-Handling.md | ✅ | Low |
| 9 | cross-cutting/02-Security-Layers.md | ✅ | Low |
| 10 | cross-cutting/03-Testing-Strategy.md | ✅ | Low |

---

## Task #1: flows/02-Create-Request.md

**Endpoint:** `POST /api/maintenance-requests`

**Files to analyze:**
- MaintenanceRequestsController.cs
- CreateMaintenanceRequestCommand.cs
- CreateMaintenanceRequestCommandValidator.cs
- CreateMaintenanceRequestCommandHandler.cs
- CreateMaintenanceRequestResult.cs
- MaintenanceRequest.cs (Domain)
- MaintenanceRequestRepository.cs
- IMaintenanceTypeRepository.cs
- ICurrentUserService.cs

**Key Focus:**
- CreatedByUserId من ICurrentUserService (لا من Client).
- Status = New يُحدد في Domain.
- التحقق من MaintenanceType موجود ونشط.
- Guid.Empty rejection.
- Title ≤ 200, Description ≤ 2000.

**Stations (متوقعة 6):**
1. Client → Controller
2. Validation
3. Auth Check
4. Type Lookup (Repo)
5. Domain Creation + Save
6. Response

---

## Task #2: flows/03-Lifecycle-Operations.md

**Endpoints:**
- `PATCH /api/maintenance-requests/{id}/assign`
- `PATCH /api/maintenance-requests/{id}/status`
- `PATCH /api/maintenance-requests/{id}/reject`

**Structure:** 3 sub-flows.

**Files:**
- AssignTechnicianCommandHandler.cs
- UpdateMaintenanceRequestStatusCommandHandler.cs
- RejectMaintenanceRequestCommandHandler.cs
- MaintenanceRequest.cs (State Machine)
- IUserRepository.cs

**Key Focus:**
- State Machine Transitions.
- InvalidStateTransitionException.
- Resource Ownership.
- Admin-only operations.

**Stations per sub-flow:** 4-6.

---

## Task #3: flows/04-Query-Operations.md

**Endpoints:**
- `GET /api/maintenance-requests/my`
- `GET /api/maintenance-requests/{id}`
- `GET /api/maintenance-requests/assigned`
- `GET /api/maintenance-requests` (Admin)
- `GET /api/maintenance-types/active` (Redis)

**Structure:** 4 sub-flows.

**Files:**
- GetMyMaintenanceRequestsQueryHandler.cs
- GetMaintenanceRequestByIdQueryHandler.cs
- GetAssignedMaintenanceRequestsQueryHandler.cs
- GetAllMaintenanceRequestsQueryHandler.cs
- GetActiveMaintenanceTypesQueryHandler.cs
- RedisMaintenanceTypeCache.cs

**Key Focus:**
- Pagination + Filters.
- Resource Ownership.
- Redis Cache-Aside.
- AsNoTracking.

---

## Task #4: 00-README.md

**Type:** Index (ليس Flow).

**Contents:**
- Overview.
- List of all files (links).
- How to navigate.
- Prerequisites.

**Structure:**
1. Welcome.
2. Learning Path (recommended order).
3. File Index (table).
4. Related Docs (architecture, database).

---

## Task #5: 01-Getting-Started.md

**Type:** Setup Guide (ليس Flow).

**Contents:**
- Prerequisites.
- Clone.
- Restore.
- User Secrets.
- Migrations.
- Run.
- Test.

**Structure:** 6 setup steps + verification.

---

## Task #6: 02-Request-Journey.md

**Type:** Conceptual (ليس Flow محدد).

**Contents:**
- Application Startup (Program.cs).
- Middleware Pipeline.
- Routing.
- Endpoint Matching.
- Auth Flow.
- Model Binding.
- DI Resolution.

**Structure:** 6 Stations (like ASP.NET Core).

---

## Task #7: 03-Architecture-Overview.md

**Type:** Conceptual.

**Contents:**
- Onion Architecture (4 layers).
- CQRS via MediatR.
- Rich Domain Model.
- Vertical Slices.
- Database Schema.
- Migrations.

**Structure:**
1. Overview.
2-5. Each Layer.
6. CQRS.
7. Database.

---

## Task #8: cross-cutting/01-Exception-Handling.md

**Type:** Cross-cutting.

**Contents:**
- Exception Types.
- Mapping to HTTP Status.
- ProblemDetails (RFC 7807).
- Middleware.

**Structure:**
1. Exception Taxonomy.
2. HTTP Mapping Table.
3. Middleware Flow.
4. ProblemDetails Format.
5. Examples.

---

## Task #9: cross-cutting/02-Security-Layers.md

**Type:** Cross-cutting.

**Contents:**
- BCrypt.
- JWT.
- SecurityStamp.
- Lockout.
- Policies.
- Resource Ownership.

**Structure:** 6 Security Layers (each with details).

---

## Task #10: cross-cutting/03-Testing-Strategy.md

**Type:** Cross-cutting.

**Contents:**
- Test Pyramid.
- Domain Tests.
- Application Tests.
- Integration Tests.
- Tools.

**Structure:**
1. Test Pyramid.
2-4. Each Layer.
5. Tools.
6. Best Practices.

---

## Commands

**To execute a task:**

"نفذ Task #1 من TASKS.md."
"نفذ Task #2 من TASKS.md."
....

**بعد كل Task:**

- راجع الناتج.
- Commit.
- عُد إلى المحادثة.

⛔ لا تُنشئ ملفات أخرى.
