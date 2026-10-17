# 🛠️ رحلة إنشاء طلب الصيانة (Create Request Flow)

> وثيقة تعليمية تفصيلية تشرح دورة حياة إنشاء طلب صيانة جديد في النظام عبر نقطة النهاية `POST /api/maintenance-requests`، بدءاً من استلام الطلب من العميل المصادق عليه وحتى حفظه في قاعدة البيانات وإرجاع الاستجابة، وفق معمارية البصلة (Onion Architecture) ونمط الوسيط (CQRS / MediatR).

---

## 🌐 1. واجهة الطلب (HTTP Request Contract)

يتم إرسال طلب إنشاء طلب الصيانة عبر بروتوكول HTTP POST إلى نقطة نهاية محمية تتطلب توكن JWT لمستخدم مصادق عليه (Authenticated User).

### 📝 مواصفات الطلب (Endpoint Spec)
- **Method:** `POST`
- **Route:** `/api/maintenance-requests`
- **Headers:**
  - `Content-Type: application/json`
  - `Authorization: Bearer <JWT_TOKEN>`
- **Authentication:** يتطلب مستخدماً مسجل الدخول (`[Authorize]`).

### 📦 جسم الطلب (Request Payload)
```json
{
  "title": "عطل في مكيف الهواء بالدور الثاني",
  "description": "المكيف يسرب المياه ولا يبرد بالشكل المطلوب في المكتب 204.",
  "priority": 2,
  "maintenanceTypeId": "01925b42-d610-7389-9d7a-123456789abc"
}
```

> [!NOTE]
> **ترميز الأولويات (`MaintenancePriority`):**
> - `1`: Low (منخفضة)
> - `2`: Medium (متوسطة)
> - `3`: High (عالية)
> - `4`: Critical (حرجة)

### 🔨 استدعاء مباشر (cURL Example)
```bash
curl -X POST "http://localhost:5000/api/maintenance-requests" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
     -d '{
       "title": "عطل في مكيف الهواء بالدور الثاني",
       "description": "المكيف يسرب المياه ولا يبرد بالشكل المطلوب في المكتب 204.",
       "priority": 2,
       "maintenanceTypeId": "01925b42-d610-7389-9d7a-123456789abc"
     }'
```

### 📤 الاستجابة الناجحة المتوقعة (201 Created Response)
```json
{
  "id": "01925b50-1234-789a-bcde-fedcba987654",
  "title": "عطل في مكيف الهواء بالدور الثاني",
  "description": "المكيف يسرب المياه ولا يبرد بالشكل المطلوب في المكتب 204.",
  "priority": 2,
  "status": 1,
  "maintenanceTypeId": "01925b42-d610-7389-9d7a-123456789abc",
  "createdByUserId": "01925b42-d610-7389-9d7a-111122223333",
  "createdAtUtc": "2026-10-17T13:10:00Z"
}
```

> [!IMPORTANT]
> **لماذا لا يستقبل الـ Endpoint حقل `CreatedByUserId` أو `Status` من العميل؟**
> - وفق قاعدة الأمان الصارمة: **لا تثق في أي مدخلات من العميل تتعلق بالهوية أو الحالة**.
> - يتم استخراج `CreatedByUserId` من الـ Claims الموثوقة عبر خدمة [`ICurrentUserService`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Application/Abstractions/Security/ICurrentUserService.cs).
> - تُحدد الحالة الابتدائية `Status = MaintenanceRequestStatus.New` تلقائياً داخل منشئ كينونة الدومين [`MaintenanceRequest`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Domain/Entities/MaintenanceRequest.cs).

---

## 🔄 2. المخطط التتابعي الكامل (Sequence Diagram)

يوضح المخطط التالي تدفق الطلب عبر طبقات النظام المختلفة:

```mermaid
sequenceDiagram
    autonumber
    actor Client as العميل (Client / Web App)
    participant AuthMW as Authentication Middleware
    participant Ctrl as MaintenanceRequestsController (API)
    participant Pipe as ValidationBehavior (Application)
    participant Hndl as CreateMaintenanceRequestCommandHandler (Application)
    participant UserSvc as CurrentUserService (Infrastructure)
    participant TypeRepo as MaintenanceTypeRepository (Infrastructure)
    participant Dom as MaintenanceRequest Entity (Domain)
    participant ReqRepo as MaintenanceRequestRepository (Infrastructure)
    participant DB as SQL Server (Database)
    participant MW as ExceptionHandlingMiddleware (API)

    Client->>AuthMW: POST /api/maintenance-requests (Bearer Token)
    alt التوكن غير موجود أو منتهي الصلاحية
        AuthMW-->>Client: 401 Unauthorized
    end
    AuthMW->>Ctrl: تمرير الطلب الموثق

    Ctrl->>Pipe: Send(CreateMaintenanceRequestCommand) عبر MediatR
    
    rect rgb(240, 245, 255)
        Note over Pipe: فحص المدخلات (FluentValidation)
        alt المدخلات غير صالحة (فارغة / تجاوزت الطول / معرف فارغ)
            Pipe-->>MW: رمي ValidationException
            MW-->>Client: 400 Bad Request (ProblemDetails)
        end
    end

    Pipe->>Hndl: Handle(CreateMaintenanceRequestCommand)

    rect rgb(255, 245, 240)
        Note over Hndl,UserSvc: التحقق من هوية المستخدم الحالي
        Hndl->>UserSvc: UserId
        alt المستخدم غير مصادق عليه (UserId is null)
            Hndl-->>MW: رمي UnauthorizedException
            MW-->>Client: 401 Unauthorized
        end
    end

    rect rgb(240, 255, 240)
        Note over Hndl,TypeRepo: التحقق من نوع الصيانة وحالته
        Hndl->>TypeRepo: GetByIdAsync(maintenanceTypeId)
        TypeRepo->>DB: استعلام جدول MaintenanceTypes
        DB-->>TypeRepo: سجل نوع الصيانة (أو null)
        TypeRepo-->>Hndl: كائن MaintenanceType
        alt نوع الصيانة غير موجود
            Hndl-->>MW: رمي NotFoundException
            MW-->>Client: 404 Not Found
        else نوع الصيانة غير نشط (IsActive == false)
            Hndl-->>MW: رمي ConflictException
            MW-->>Client: 409 Conflict
        end
    end

    rect rgb(245, 240, 255)
        Note over Hndl,Dom: بناء كينونة الدومين الغنية وحفظها
        Hndl->>Dom: new MaintenanceRequest(title, description, priority, typeId, userId)
        Dom->>Dom: التحقق الداخلي من القواعد وضبط الحالة New وتوليد UUIDv7
        Hndl->>ReqRepo: AddAsync(maintenanceRequest)
        ReqRepo->>DB: إدراج السجل في جدول MaintenanceRequests
        DB-->>ReqRepo: تم الحفظ
    end

    Hndl-->>Ctrl: CreateMaintenanceRequestResult
    Ctrl-->>Client: 201 Created (CreateMaintenanceRequestResponse)
```

---

## 🚉 3. المحطات الست في رحلة الريكوست (The 6 Stations)

### 1️⃣ المحطة الأولى: البوابة واستلام الطلب (Client & Controller)
- **الملف الفعلي:** [`MaintenanceRequestsController.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.API/Controllers/MaintenanceRequestsController.cs)
- **الدالة:** `Create([FromBody] CreateMaintenanceRequestRequest request, CancellationToken cancellationToken)`
- **ماذا يحدث هنا؟**
  1. تقوم طبقة حماية ASP.NET Core بالتحقق من وجود وصلاحية التوكن عبر السمة `[Authorize]`.
  2. يتم تحويل محتوى الـ JSON إلى كائن [`CreateMaintenanceRequestRequest`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.API/Contracts/MaintenanceRequests/CreateMaintenanceRequestRequest.cs).
  3. يقوم الـ Controller بتجريد الطلب وتغليفه في أمر [`CreateMaintenanceRequestCommand`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Application/Features/MaintenanceRequests/CreateMaintenanceRequest/CreateMaintenanceRequestCommand.cs).
  4. إرسال الأمر مباشرة عبر وسيط MediatR `_sender.Send(command)`.

> [!TIP]
> **تصميم وحدات التحكم النحيفة (Thin Controller):**
> الـ Controller لا يحتوي على أي منطق تحقق تجاري؛ يعيد النتيجة بتغليفها كرمز `201 Created` مع كائن الاستجابة الخفيف `CreateMaintenanceRequestResponse`.

---

### 2️⃣ المحطة الثانية: جدار التحقق التلقائي (Pipeline Behavior & FluentValidation)
- **الملفات الفعلية:**
  - وسيط التحقق: [`ValidationBehavior.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Application/Common/Behaviors/ValidationBehavior.cs)
  - قواعد التحقق: [`CreateMaintenanceRequestCommandValidator.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Application/Features/MaintenanceRequests/CreateMaintenanceRequest/CreateMaintenanceRequestCommandValidator.cs)
- **ماذا يحدث هنا؟**
  - يعترض الـ Pipeline الريكوست قبل وصوله للمعالج (Handler) ويفحص الشروط التالية:
    1. `Title`: مطلوب، بحد أقصى 200 حرف.
    2. `Description`: مطلوب، بحد أقصى 2000 حرف.
    3. `Priority`: قيمة صحيحة ضمن التعداد `MaintenancePriority`.
    4. `MaintenanceTypeId`: مطلوب وغير فارغ (`Guid.Empty` مرفوض).
  - عند وجود أي خطأ، يتم رمي `ValidationException` فورياً دون استهلاك موارد قاعدة البيانات.

---

### 3️⃣ المحطة الثالثة: فحص وتأكيد هوية المنشئ (Auth Check & ICurrentUserService)
- **الملفات الفعلية:**
  - الواجهة: [`ICurrentUserService.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Application/Abstractions/Security/ICurrentUserService.cs)
  - المعالج: [`CreateMaintenanceRequestCommandHandler.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Application/Features/MaintenanceRequests/CreateMaintenanceRequest/CreateMaintenanceRequestCommandHandler.cs)
- **ماذا يحدث هنا؟**
  - يقوم المعالج بقراءة خاصية `_currentUserService.UserId`.
  - إذا كانت القيمة `null` (مثل حالة تزييف أو غياب الـ Claim)، يتم رمي `UnauthorizedException("المستخدم غير مصادق عليه.")`.
  - يُعتمد هذا المعرف الموثوق كـ `CreatedByUserId`، مما يمنع نهائياً تزييف هوية صاحب الطلب من جهة واجهة المستخدم.

---

### 4️⃣ المحطة الرابعة: التحقق من نوع الصيانة وحالته (Type Lookup & Repo)
- **الملفات الفعلية:**
  - المستودع: [`IMaintenanceTypeRepository.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Application/Abstractions/Persistence/IMaintenanceTypeRepository.cs)
  - التنفيذ: [`MaintenanceTypeRepository.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Infrastructure/Persistence/Repositories/MaintenanceTypeRepository.cs)
- **ماذا يحدث هنا؟**
  1. الاستعلام عن السجل عبر `_maintenanceTypeRepository.GetByIdAsync(request.MaintenanceTypeId, cancellationToken)`.
  2. إذا كان نوع الصيانة غير موجود، يتم رمي استثناء `NotFoundException`.
  3. إذا كان نوع الصيانة موجوداً ولكنه غير نشط (`!maintenanceType.IsActive`)، يتم رمي استثناء تعارض الأعمال `ConflictException`.

> [!IMPORTANT]
> **قاعدة أعمال صارمة:** لا يمكن إنشاء طلب صيانة لنوع معطل، حتى وإن كان المعرف موجوداً في قاعدة البيانات مسبقاً.

---

### 5️⃣ المحطة الخامسة: إنشاء الكينونة وحفظها (Rich Domain Model & Persistence)
- **الملفات الفعلية:**
  - كينونة الدومين: [`MaintenanceRequest.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Domain/Entities/MaintenanceRequest.cs)
  - مستودع الطلبات: [`MaintenanceRequestRepository.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Infrastructure/Persistence/Repositories/MaintenanceRequestRepository.cs)
- **ماذا يحدث هنا؟**
  1. استدعاء المنشئ المخصص لكينونة الدومين `new MaintenanceRequest(...)`.
  2. تقوم الكينونة بالتحقق الداخلي الذاتي:
     - إزالة الفراغات الزائدة عبر `Trim()`.
     - منع `Guid.Empty` لكل من `MaintenanceTypeId` و `CreatedByUserId` برمي `BusinessRuleViolationException`.
     - توليد المعرف الفريد عبر المعمارية المدمجة `Guid.CreateVersion7()`.
     - ضبط الحالة المبدئية دائماً على `MaintenanceRequestStatus.New`.
     - تصفير حقول المعالجة (`AssignedTechnicianId = null`, `RejectionReason = null`...).
  3. إضافة الكائن وحفظه في قاعدة البيانات عبر `_maintenanceRequestRepository.AddAsync(maintenanceRequest, cancellationToken)`.

---

### 6️⃣ المحطة السادسة: مخرج الاستجابة وتوليد النتيجة (Response & Result)
- **الملفات الفعلية:**
  - نتيجة التطبيق: [`CreateMaintenanceRequestResult.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Application/Features/MaintenanceRequests/CreateMaintenanceRequest/CreateMaintenanceRequestResult.cs)
  - وسيطة معالجة الأخطاء: [`ExceptionHandlingMiddleware.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.API/Middleware/ExceptionHandlingMiddleware.cs)
- **ماذا يحدث هنا؟**
  - يعيد المعالج كائن `CreateMaintenanceRequestResult` متضمناً المعرف الجديد، التواريخ، والحالة المبدئية.
  - ينشئ الـ Controller استجابة `201 Created` مع كائن `CreateMaintenanceRequestResponse`.
  - في حال حدوث أي خطأ في أي مرحلة سابقة، تلتقط وسيطة `ExceptionHandlingMiddleware` الاستثناء وتعيده بصيغة RFC 7807 (`ProblemDetails`).

---

## 🌳 4. شجرة القرار الكاملة (Decision Tree)

```mermaid
flowchart TD
    Start([بدء الطلب: POST /api/maintenance-requests]) --> AuthCheck{هل المستخدم مصادق عليه؟<br/>Bearer Token Valid}
    
    AuthCheck -- لا --> ErrAuth["رفض الطلب من Auth Middleware<br/><b>401 Unauthorized</b>"]
    AuthCheck -- نعم --> ValCheck{هل المدخلات تطابق قواعد التحقق؟<br/>FluentValidation}

    ValCheck -- لا --> ErrVal["رمي ValidationException<br/><b>400 Bad Request</b><br/><i>(العنوان/الوصف/الأولوية/النوع غير صالح)</i>"]
    ValCheck -- نعم --> UserCheck{هل معرف المستخدم متاح؟<br/>ICurrentUserService.UserId}

    UserCheck -- لا --> ErrUser["رمي UnauthorizedException<br/><b>401 Unauthorized</b>"]
    UserCheck -- نعم --> TypeCheck{هل نوع الصيانة موجود؟<br/>GetByIdAsync}

    TypeCheck -- لا --> ErrTypeNotFound["رمي NotFoundException<br/><b>404 Not Found</b><br/><i>(نوع الصيانة غير موجود)</i>"]
    TypeCheck -- نعم --> ActiveCheck{هل نوع الصيانة نشط؟<br/>IsActive == true}

    ActiveCheck -- لا --> ErrTypeInactive["رمي ConflictException<br/><b>409 Conflict</b><br/><i>(نوع الصيانة معطل)</i>"]
    ActiveCheck -- نعم --> DomBuild["إنشاء كينونة الدومين:<br/>Status = New<br/>Id = Guid.CreateVersion7()"]

    DomBuild --> SaveDB["حفظ الطلب في قاعدة البيانات:<br/>AddAsync + SaveChangesAsync"]
    SaveDB --> ReturnCreated["إرجاع بيانات الطلب الجديد<br/><b>201 Created</b>"]

    style ReturnCreated fill:#d4edda,stroke:#28a745,stroke-width:2px;
    style ErrVal fill:#f8d7da,stroke:#dc3545,stroke-width:1px;
    style ErrAuth fill:#f8d7da,stroke:#dc3545,stroke-width:1px;
    style ErrUser fill:#f8d7da,stroke:#dc3545,stroke-width:1px;
    style ErrTypeNotFound fill:#fff3cd,stroke:#ffc107,stroke-width:2px;
    style ErrTypeInactive fill:#f8d7da,stroke:#dc3545,stroke-width:1px;
```

---

## ⚠️ 5. مصفوفة الاستثناءات وحالات الخطأ (Exceptions Matrix)

تتكفل وسيطة [`ExceptionHandlingMiddleware.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.API/Middleware/ExceptionHandlingMiddleware.cs) بتحويل كافة الاستثناءات البرمجية إلى هيكل RFC 7807 المعياري:

| رمز الحالة (Status) | نوع الاستثناء (Exception) | سبب الحدوث (Trigger) | شكل استجابة ProblemDetails |
| :--- | :--- | :--- | :--- |
| **`400 Bad Request`** | `ValidationException` | - العنوان فارغ أو تجاوز 200 حرف.<br>- الوصف فارغ أو تجاوز 2000 حرف.<br>- معرّف نوع الصيانة فارغ (`Guid.Empty`).<br>- قيمة الأولوية غير موجودة ضمن الـ Enum. | `{"status": 400, "title": "Validation Error", "validationErrors": {"Title": ["العنوان مطلوب."]}}` |
| **`401 Unauthorized`** | `UnauthorizedException` | - لم يتم إرسال توكن صالح في ترويسة الطلب.<br>- أو لم يمكن استخراج `UserId` من الـ Claims. | `{"status": 401, "title": "Unauthorized", "detail": "المستخدم غير مصادق عليه."}` |
| **`404 Not Found`** | `NotFoundException` | نوع الصيانة الممرر في `maintenanceTypeId` غير مسجل بقاعدة البيانات. | `{"status": 404, "title": "Not Found", "detail": "نوع الصيانة بالمعرّف '...' غير موجود."}` |
| **`409 Conflict`** | `ConflictException` | نوع الصيانة موجود ولكنه غير نشط (`IsActive = false`). | `{"status": 409, "title": "Conflict", "detail": "نوع الصيانة 'الكهرباء' معطّل ولا يمكن استخدامه."}` |
| **`500 Internal Error`** | `Exception` | فشل في الاتصال بقاعدة البيانات أو خطأ غير متوقع. | `{"status": 500, "title": "Internal Server Error", "detail": "An unhandled error occurred."}` |

---

## 🛡️ 6. طبقات الأمان المطبقة في هذا المسار (Security Layers)

1. **التحقق من الهوية عبر الـ Claims (Zero-Trust Identity):**
   عدم قبول معرف المستخدم كحقل في الـ Body؛ استخراج الهوية حصرياً عبر `ICurrentUserService` من التوكن الموقع رقمياً.
2. **منع التلاعب بالحالة الأولية (Encapsulated State Initialization):**
   حقل الحالة `Status` لا يمكن تعديله من الخارج ويُحدد داخلياً في الـ Domain على أنه `New`.
3. **التحقق من الصلاحية على مرحلتين (Two-Phase Validation):**
   - المرحلة الأولى: فحص سلامة الشكل والحدود عبر `FluentValidation` (بحد أقصى 200 للعنوان و 2000 للوصف).
   - المرحلة الثانية: فحص قواعد العمل داخل منشئ الدومين `MaintenanceRequest` ضد `Guid.Empty` والتأكد من صحة التعدادات.
4. **منع إنشاء طلبات للخدمات المعطلة (Business Integrity):**
   التحقق الصارم من حالة نوع الصيانة ورفض الإضافة إذا كان معطلاً برمز `409 Conflict`.

---

## 🧪 7. كيفية اختبار المسار (Testing Guide)

### أ) اختبارات الوحدة (Unit Tests)
- [`CreateMaintenanceRequestCommandHandlerTests.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/tests/MaintenanceSystem.Tests/Application/Handlers/CreateMaintenanceRequestCommandHandlerTests.cs):
  - تجربة نجاح إنشاء الطلب عند استيفاء كامل الشروط والتحقق من حفظ الكائن وإرجاع النتيجة.
  - تجربة رمي `UnauthorizedException` عند غياب هوية المستخدم.
  - تجربة رمي `NotFoundException` عند تمرير نوع صيانة غير مسجل.
  - تجربة رمي `ConflictException` عند تمرير نوع صيانة غير نشط.
- [`CreateMaintenanceRequestCommandValidatorTests.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/tests/MaintenanceSystem.Tests/Application/Validators/CreateMaintenanceRequestCommandValidatorTests.cs):
  - التحقق من رفض العناوين الفارغة أو التي تتجاوز 200 حرف.
  - التحقق من رفض الأوصاف التي تتجاوز 2000 حرف.
  - التحقق من رفض معرّف نوع الصيانة الفارغ (`Guid.Empty`).
- [`MaintenanceRequestTests.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/tests/MaintenanceSystem.Tests/Domain/MaintenanceRequestTests.cs):
  - التحقق من توليد معرف صالح من الإصدار السابع (UUIDv7).
  - التحقق من ضبط الحالة الابتدائية على `MaintenanceRequestStatus.New`.

### ب) اختبارات التكامل (Integration Tests)
- [`MaintenanceRequestEndpointsTests.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/tests/MaintenanceSystem.Tests/Integration/MaintenanceRequestEndpointsTests.cs):
  - إرسال طلب حقيقي إلى `POST /api/maintenance-requests` بتوكن مستخدم صالح والتأكد من إرجاع رمز `201 Created`.
  - اختبار الرفض بـ `401 Unauthorized` بدون توكن.
  - اختبار الرفض بـ `400 Bad Request` عند إرسال مدخلات غير صحيحة.

### ج) الفحص اليدوي السريع عبر PowerShell
```powershell
# 1. تسجيل الدخول للحصول على التوكن
$loginBody = @{ userName = "employee1"; password = "Password123!" } | ConvertTo-Json
$auth = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"

# 2. إنشاء طلب صيانة
$createBody = @{
    title = "تسريب مياه في الطابق الأول"
    description = "تسريب مياه مستمر تحت حوض الغسيل بدورة مياه الموظفين"
    priority = 2
    maintenanceTypeId = "01925b42-d610-7389-9d7a-123456789abc"
} | ConvertTo-Json

$headers = @{ Authorization = "Bearer $($auth.token)" }
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/maintenance-requests" -Method Post -Body $createBody -Headers $headers -ContentType "application/json"
$response | Format-List
```

---

## 💡 8. نقاط ذهبية للتذكر (Golden Architecture Takeaways)

> [!TIP]
> - **تذكر:** في الدومين الغني (Rich Domain)، كينونة `MaintenanceRequest` هي المسؤولة عن حماية حالتها الذاتية؛ لا تسمح للكود الخارجي بتعيين `Status` مباشرة (الخاصية مغلقة بـ `private set`).
> - **تذكر:** لا تثق مطلقاً في الواجهة الأمامية عند تعيين هوية المستخدم المنشئ، استخرجها دائماً من الـ Claims المشفرة بالتوكن عبر `ICurrentUserService`.
> - **تذكر:** الـ Pipeline Behavior في MediatR يعفيك من كتابة شروط التحقق التكرارية داخل الـ Handlers؛ أي خطأ شكلي في البيانات يتم إيقافه فورياً قبل أن يستهلك أي استعلام لقاعدة البيانات.
> - **مشكلة حقيقية شائعة:** السماح بإنشاء طلبات مرتبطة بخدمات تم تعطيلها مؤقتاً في النظام؛ التحقق من `maintenanceType.IsActive` وإرجاع `409 Conflict` يمنع دخول سجلات تالفة أو غير قابلة للتنفيذ في دورة العمل.
