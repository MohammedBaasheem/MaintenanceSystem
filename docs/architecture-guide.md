# صياغة `docs/architecture-guide.md`

ممتاز. سأصيغ لك الآن **الدليل المعماري الكامل** بصيغة جاهزة للحفظ في `docs/architecture-guide.md`.

هذا الملف **ليس خطة تنفيذ**، بل **مرجع معماري** يُقرأ قبل البدء ويُراجَع عند كل مهمة. الخطة (v4.0) تبقى كما هي، وسنضيف إشارات إليها لاحقًا في v4.1.

---

```markdown
# الدليل المعماري — Maintenance Request Management System

**الإصدار:** 1.0
**تاريخ الاعتماد:** 2026-10-13
**المرجع المرتبط:** `MaintenanceSystem_Development_Backlog_v4.md`
**الغرض:** توثيق القرارات المعمارية، تنظيم المشاريع، وقواعد الكتابة قبل التنفيذ.

---

## 1. نظرة عامة

المشروع مبني على **Onion Architecture** بأربعة مشاريع، مع **CQRS** عبر MediatR، و**Rich Domain Model**.

### قاعدة الاعتماديات الذهبية

> **الاعتماديات تتجه من الخارج إلى الداخل. الطبقات الداخلية لا تعرف شيئًا عن الخارجية.**


API  ──►  Infrastructure  ──►  Application  ──►  Domain
   ─────────────────────────────────────────────►
                  (كل شيء يعرف Domain)
```

### أدوار المشاريع في جملة واحدة

| المشروع | الدور في جملة واحدة |
|---------|---------------------|
| `Domain` | قواعد العمل والمفاهيم الأساسية. لا يعرف EF Core ولا ASP.NET Core. |
| `Application` | تنسيق حالات الاستخدام عبر Commands/Queries. يعرف Domain فقط. |
| `Infrastructure` | تنفيذ التفاصيل التقنية (EF Core, BCrypt, JWT, Redis). يعرف Application وDomain. |
| `API` | استقبال HTTP وتحويله إلى Commands/Queries. يعرف Application وInfrastructure. |

### السؤال الحاكم لكل ملف جديد

> **"إلى أي طبقة ينتمي هذا الملف؟"**
> - إذا كان قاعدة عمل → Domain.
> - إذا كان تنسيق حالة استخدام → Application.
> - إذا كان تفصيلًا تقنيًا → Infrastructure.
> - إذا كان متعلقًا بـ HTTP → API.

---

## 2. القرارات المعمارية المعتمدة

| #      | القرار                    | القيمة                              | المبرر                           |
| ------ | ------------------------- | ----------------------------------- | -------------------------------- |
| AR-001 | معمارية الطبقات           | Onion Architecture (4 مشاريع)       | فصل واضح، اختبار سهل             |
| AR-002 | Domain Model              | Rich Domain Model (دوال سلوكية)     | قواعد العمل في مكان واحد         |
| AR-003 | Repository                | مخصص لكل كيان (لا Generic)          | يعبّر عن حالات الاستخدام الفعلية |
| AR-004 | CQRS                      | Commands + Queries عبر MediatR      | فصل القراءة عن الكتابة           |
| AR-005 | Validation                | FluentValidation + MediatR Behavior | لا تكرار، تحقق مركزي             |
| AR-006 | الأخطاء                   | Exceptions مخصصة + Middleware       | بسيط، مفهوم، لا Result Pattern   |
| AR-007 | تنظيم Application         | Features رأسية (Vertical Slices)    | كل ميزة في مجلد واحد             |
| AR-008 | ValueObjects              | ❌ لا في الإصدار الأول               | لا حاجة حقيقية                   |
| AR-009 | AutoMapper                | ❌ لا                                | Manual Mapping أبسط              |
| AR-010 | Unit of Work              | ❌ لا                                | EF Core هو UoW أصلًا             |
| AR-011 | Generic Repository        | ❌ لا                                | لا يعبّر عن الاحتياج             |
| AR-012 | Specification Pattern     | ❌ لا                                | لا استعلامات معقدة متكررة        |
| AR-013 | Domain Events             | ❌ لا                                | لا حاجة الآن                     |
| AR-014 | Result Pattern            | ❌ لا                                | Exceptions أبسط                  |
| AR-015 | DependencyInjection.cs    | ✅ نعم في كل مشروع                   | ينظّم Program.cs                 |
| AR-016 | Contract DTOs vs Commands | منفصلة                              | HTTP منفصل عن Application        |

---

## 3. مشروع Domain

### 3.1 الدور

قلب المشروع. يحتوي:
- الكيانات (Entities).
- Enums.
- الدوال السلوكية.
- قواعد العمل.

لا يعتمد على أي مشروع آخر. لا NuGet ثقيل.

### 3.2 شجرة المجلدات

```
MaintenanceSystem.Domain/
│
├── Entities/
│   ├── User.cs
│   ├── Role.cs
│   ├── UserRole.cs
│   ├── MaintenanceType.cs
│   └── MaintenanceRequest.cs
│
├── Enums/
│   ├── MaintenanceRequestStatus.cs
│   ├── MaintenancePriority.cs
│   └── RoleNames.cs
│
├── Common/
│   └── Entity.cs                    ← فئة أساسية فيها Id (Guid v7) و CreatedAtUtc
│
└── Exceptions/
    ├── DomainException.cs           ← أساس لكل استثناءات المجال
    ├── InvalidStateTransitionException.cs
    └── BusinessRuleViolationException.cs
```

### 3.3 الدوال السلوكية المعتمدة لكل كيان

#### `User`
- `RegisterFailedAttempt()` — يزيد `FailedLoginCount`، ويضبط `LockoutEndUtc` عند بلوغ الحد.
- `IsLockedOut()` — يتحقق من `LockoutEndUtc > UtcNow`.
- `ResetLockout()` — يصفّر `FailedLoginCount` و`LockoutEndUtc` بعد نجاح الدخول.
- `RegenerateSecurityStamp()` — يولّد Guid v7 جديد.
- `Deactivate()` / `Activate()` — يغيّر `IsActive`.

#### `MaintenanceType`
- `Rename(string name)` — مع التحقق من عدم الفراغ.
- `Deactivate()` / `Activate()`.

#### `MaintenanceRequest`
- `AssignTechnician(Guid technicianId)` — يتحقق من الحالة، يضبط `AssignedTechnicianId` و`AssignedAtUtc`، يغيّر الحالة إلى `Assigned`.
- `Start()` — من `Assigned` إلى `InProgress`.
- `Resolve()` — من `InProgress` إلى `Resolved`، يضبط `ResolvedAtUtc`.
- `Close()` — من `Resolved` إلى `Closed`، يضبط `ClosedAtUtc`.
- `Reject(string reason)` — من `New` إلى `Rejected`، يضبط `RejectionReason`.

### 3.4 ما لا يجب أن يوجد في Domain

- ❌ `DbContext`, `DbSet`.
- ❌ `[Table]`, `[Column]` (EF Annotations).
- ❌ `[Authorize]`, `[ApiController]`.
- ❌ `BCrypt.HashPassword()`.
- ❌ `HttpClient`, `ILogger`.
- ❌ `Task<IActionResult>`.
- ❌ `Email`, `Money` كـ ValueObjects (مؤجل).
- ❌ DTOs أو Response Models.

### 3.5 قواعد الكتابة في Domain

- كل كيان له `private set` على خصائصه.
- التعديل عبر دوال سلوكية فقط.
- Constructor يحمي الثوابت (`Id`, `CreatedAtUtc`).
- إذا خالفت العملية قاعدة، ترمي `DomainException`.

---

## 4. مشروع Application

### 4.1 الدور

تنسيق حالات الاستخدام. يعرف Domain فقط. لا يعرف EF Core، لا Redis، لا BCrypt، لا HTTP.

### 4.2 شجرة المجلدات

```
MaintenanceSystem.Application/
│
├── Abstractions/
│   ├── Persistence/
│   │   ├── IUserRepository.cs
│   │   ├── IMaintenanceTypeRepository.cs
│   │   └── IMaintenanceRequestRepository.cs
│   ├── Security/
│   │   ├── IPasswordHasher.cs
│   │   ├── ITokenService.cs
│   │   └── ICurrentUserService.cs
│   └── Caching/
│       └── IMaintenanceTypeCache.cs
│
├── Features/
│   ├── Auth/
│   │   └── Login/
│   │       ├── LoginCommand.cs
│   │       ├── LoginCommandHandler.cs
│   │       ├── LoginCommandValidator.cs
│   │       └── LoginResponse.cs
│   ├── Users/
│   │   ├── CreateUser/
│   │   ├── ChangeUserRole/
│   │   ├── DeactivateUser/
│   │   └── ResetPassword/
│   ├── MaintenanceTypes/
│   │   ├── CreateMaintenanceType/
│   │   ├── UpdateMaintenanceType/
│   │   └── GetActiveMaintenanceTypes/
│   └── MaintenanceRequests/
│       ├── CreateMaintenanceRequest/
│       ├── GetMyMaintenanceRequests/
│       ├── GetMaintenanceRequestById/
│       ├── AssignTechnician/
│       ├── UpdateMaintenanceRequestStatus/
│       ├── RejectMaintenanceRequest/
│       └── CloseMaintenanceRequest/
│
├── Common/
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs
│   └── Exceptions/
│       ├── NotFoundException.cs
│       ├── ForbiddenException.cs
│       └── ConflictException.cs
│
└── DependencyInjection.cs
```

### 4.3 شكل مجلد الميزة الواحدة

لكل عملية (Command أو Query):

```
Features/MaintenanceRequests/CreateMaintenanceRequest/
├── CreateMaintenanceRequestCommand.cs       ← المدخلات
├── CreateMaintenanceRequestCommandHandler.cs ← التنفيذ
├── CreateMaintenanceRequestCommandValidator.cs ← FluentValidation
└── CreateMaintenanceRequestResponse.cs       ← المُعاد
```

**للـ Query:**
```
Features/MaintenanceTypes/GetActiveMaintenanceTypes/
├── GetActiveMaintenanceTypesQuery.cs
├── GetActiveMaintenanceTypesQueryHandler.cs
└── MaintenanceTypeDto.cs
```

### 4.4 الواجهات (Abstractions)

كل واجهة هنا تُنفَّذ في Infrastructure:

- **Persistence:** Repository مخصص لكل كيان. لا Generic.
- **Security:** `IPasswordHasher`, `ITokenService`, `ICurrentUserService`.
- **Caching:** `IMaintenanceTypeCache`.

### 4.5 Behaviors

`ValidationBehavior<TRequest, TResponse>` هو **الوحيد** في الإصدار الأول.
- يشغّل كل Validators المسجلة قبل Handler.
- يرمي `ValidationException` عند الفشل.

**لا Behaviors أخرى** (Logging, Transaction) في الإصدار الأول.

### 4.6 Exceptions

- `NotFoundException` → 404.
- `ForbiddenException` → 403.
- `ConflictException` → 409.
- `ValidationException` → 400 (من FluentValidation).

Middleware في API يحوّلها إلى `ProblemDetails`.

### 4.7 ما لا يجب أن يوجد في Application

- ❌ `DbContext`.
- ❌ `SqlConnection`.
- ❌ `BCrypt`.
- ❌ `Redis`.
- ❌ `Controller`, `IActionResult`.
- ❌ `Include()`, `ToListAsync()`.
- ❌ Contract DTOs الخاصة بـ HTTP.
- ❌ ValueObjects.
- ❌ Result Pattern.

### 4.8 قواعد الكتابة في Application

- كل Command/Query **فئة immutable** (`record`).
- كل Handler **فئة واحدة** بمسؤولية واحدة.
- كل Validator **منفصل** لكل Command يحتاج تحققًا.
- Handler لا يحوي منطق تحقق (Behavior يفعله).
- Handler لا يعرف HTTP (لا يرجع `IActionResult`).
- Response DTOs خاصة بـ Application، مختلفة عن Contract DTOs.

---

## 5. مشروع Infrastructure

### 5.1 الدور

تنفيذ الواجهات المعرّفة في Application. كل تفصيل تقني هنا.

### 5.2 شجرة المجلدات

```
MaintenanceSystem.Infrastructure/
│
├── Persistence/
│   ├── AppDbContext.cs
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   ├── RoleConfiguration.cs
│   │   ├── UserRoleConfiguration.cs
│   │   ├── MaintenanceTypeConfiguration.cs
│   │   └── MaintenanceRequestConfiguration.cs
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   ├── MaintenanceTypeRepository.cs
│   │   └── MaintenanceRequestRepository.cs
│   └── Migrations/                    ← تُولَّد تلقائيًا
│
├── Security/
│   ├── BCryptPasswordHasher.cs
│   ├── JwtTokenService.cs
│   ├── JwtOptions.cs
│   └── CurrentUserService.cs
│
├── Caching/
│   └── RedisMaintenanceTypeCache.cs
│
├── Seed/
│   └── DevelopmentDataSeeder.cs
│
└── DependencyInjection.cs
```

### 5.3 قواعد الكتابة في Infrastructure

- كل تنفيذ **يُسجَّل في `DependencyInjection.cs`**.
- كل كيان له **Configuration منفصل** (`IEntityTypeConfiguration<T>`)، لا Annotations في Domain.
- Repositories **مخصصة** حسب احتياج حالات الاستخدام.
- `AppDbContext` يعرف فقط `DbSet`s، ولا يحوي منطق أعمال.
- JWT SigningKey من **User Secrets / Environment**.
- Redis ConnectionString من **appsettings**.

### 5.4 ما لا يجب أن يوجد في Infrastructure

- ❌ قواعد عمل.
- ❌ منطق حالة استخدام.
- ❌ Controllers.
- ❌ Contract DTOs.
- ❌ Response DTOs.

---

## 6. مشروع API

### 6.1 الدور

استقبال HTTP، ترجمة إلى Commands/Queries، إرسال عبر MediatR، تحويل النتيجة إلى HTTP.

### 6.2 شجرة المجلدات

```
MaintenanceSystem.API/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── MaintenanceTypesController.cs
│   └── MaintenanceRequestsController.cs
│
├── Contracts/
│   ├── Auth/
│   │   └── LoginRequest.cs
│   ├── Users/
│   │   ├── CreateUserRequest.cs
│   │   └── ChangeUserRoleRequest.cs
│   ├── MaintenanceTypes/
│   │   ├── CreateMaintenanceTypeRequest.cs
│   │   └── UpdateMaintenanceTypeRequest.cs
│   └── MaintenanceRequests/
│       ├── CreateMaintenanceRequestRequest.cs
│       ├── AssignTechnicianRequest.cs
│       └── UpdateStatusRequest.cs
│
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
│
├── Authorization/
│   ├── Policies.cs                    ← ثوابت أسماء السياسات
│   └── PolicyExtensions.cs
│
├── Extensions/
│   └── (إن احتجت لاحقًا)
│
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── appsettings.Production.json
```

### 6.3 قواعد الكتابة في API

- كل Controller Action **3-5 أسطر**:
  1. استقبال `Request`.
  2. تحويله إلى Command/Query.
  3. إرسال عبر `ISender`.
  4. تحويل النتيجة إلى `IActionResult`.
- **Contract DTOs** منفصلة عن Command/Response.
- التحقق (Validation) **لا يُكتب هنا** — في Validators.
- `[Authorize(Policy = Policies.X)]` على كل Action حساس.
- Middleware واحد لمعالجة الاستثناءات.

### 6.4 ما لا يجب أن يوجد في API

- ❌ منطق أعمال.
- ❌ `DbContext`.
- ❌ `BCrypt`.
- ❌ `Redis`.
- ❌ استعلامات LINQ.
- ❌ تحقق يدوي.

---

## 7. رحلة كاملة: `CreateMaintenanceRequest`

مثال يربط كل الطبقات.

### الخطوة 1: HTTP Request
`POST /api/maintenance-requests` مع body.

### الخطوة 2: API
- `MaintenanceRequestsController.Create(CreateMaintenanceRequestRequest request)`.
- يحوّل `request` إلى `CreateMaintenanceRequestCommand`.
- يرسل: `await _sender.Send(command)`.
- يعيد `201 Created`.

### الخطوة 3: Application — ValidationBehavior
- يشغّل `CreateMaintenanceRequestCommandValidator`.
- إذا فشل → `ValidationException`.
- إذا نجح → يكمل إلى Handler.

### الخطوة 4: Application — Handler
- يقرأ `CurrentUserId` من `ICurrentUserService`.
- يستدعي `IMaintenanceTypeRepository.GetByIdAsync(typeId)`.
- إذا النوع غير موجود أو معطّل → `NotFoundException` أو `ConflictException`.
- ينشئ `MaintenanceRequest` (Domain) → يفرض القواعد.
- يستدعي `IMaintenanceRequestRepository.AddAsync(request)`.
- يرجع `CreateMaintenanceRequestResponse`.

### الخطوة 5: Domain
- `MaintenanceRequest` constructor:
  - يتحقق من العنوان.
  - يعيّن `Status = New`.
  - يعيّن `CreatedAtUtc`.
  - يولّد `Id` (Guid v7).

### الخطوة 6: Infrastructure
- `MaintenanceRequestRepository.AddAsync`:
  - يضيف إلى `DbContext`.
  - `SaveChangesAsync`.

### الخطوة 7: API — Response
- Handler يرجع `Response`.
- Controller يترجم إلى `201 Created` مع `Location`.

**ملاحظة:** لا Controller يعرف SQL، لا Domain يعرف HTTP، لا Application يعرف EF Core.

---

## 8. قواعد الكتابة العامة (Coding Conventions)

### التسمية
- الكيانات: `User`, `MaintenanceRequest` (مفرد، PascalCase).
- الـ Commands: `CreateMaintenanceRequestCommand`.
- الـ Queries: `GetActiveMaintenanceTypesQuery`.
- الـ Handlers: `CreateMaintenanceRequestCommandHandler`.
- الـ Validators: `CreateMaintenanceRequestCommandValidator`.
- الـ Responses: `CreateMaintenanceRequestResponse`.
- الـ Repositories: `IMaintenanceRequestRepository` / `MaintenanceRequestRepository`.

### التنظيم
- كل Command/Query في **ملف منفصل**.
- كل Handler في **ملف منفصل**.
- كل Validator في **ملف منفصل**.
- كل Response في **ملف منفصل**.
- كل مجلد ميزة يحتوي على 4 ملفات فقط (Command, Handler, Validator, Response).

### Async
- كل عمليات I/O بـ `async/await`.
- `CancellationToken` يمر عبر Controller → MediatR → Handler → Repository.
- لا `.Result` أو `.Wait()`.

### الأمان
- لا `UserId` من العميل — من `ICurrentUserService`.
- لا `Role` من العميل — من Claims.
- لا `Status` أولي من العميل — من Domain.
- لا `SecurityStamp` من العميل — من DB.

---

## 9. Anti-Patterns — ما لا نفعله

| النمط | السبب |
|-------|-------|
| Generic Repository | لا يعبّر عن حالات الاستخدام، يكسر الفصل |
| Unit of Work فوق EF Core | EF Core هو UoW أصلًا |
| AutoMapper | Manual Mapping أبسط وأسرع |
| ValueObjects | لا حاجة حقيقية في الإصدار الأول |
| Result Pattern | Exceptions أبسط |
| Specification Pattern | لا استعلامات معقدة متكررة |
| Domain Events | لا حاجة الآن |
| Anemic Domain Model | المنطق في الكيانات لا Services |
| [Table] في Domain | استخدم IEntityTypeConfiguration |
| DbContext في Application | استخدم Repository Interface |
| Business Logic في Controller | في Domain أو Handler |
| Validation يدوي في Controller | في Validators |
| الاستعلامات في Repository ثم الفلترة في الذاكرة | الفلترة في SQL |

---

## 10. كيف تستخدم هذا الدليل

1. **قبل البدء:** اقرأه كاملًا.
2. **قبل إنشاء ملف جديد:** اسأل "إلى أي طبقة ينتمي؟".
3. **قبل إضافة نمط:** اسأل "هل هو في قائمة Anti-Patterns؟".
4. **عند كل مهمة:** راجع القسم المتعلق بها.
5. **عند إعادة هيكلة:** ارجع إلى هذا الدليل لتأكيد الاتجاه.

---

## 11. ما هو خارج نطاق هذا الدليل

- تفاصيل تنفيذ كل مهمة → في الخطة (v4.0).
- تفاصيل قاعدة البيانات → في `docs/database-design.md`.
- تفاصيل JWT → في `docs/adr/0001-custom-auth.md`.
- تفاصيل Lockout → في `docs/adr/0002-lockout-policy.md`.

---

**انتهى الدليل — الإصدار 1.0**


