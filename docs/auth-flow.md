# Authentication & Authorization Flow — Maintenance Request Management System

توثيق مسار المصادقة والتحقق من الـ Tokens وإدارة الـ SecurityStamp.

---

## 1. نظرة عامة

يعتمد النظام على نظام مصادقة مخصص (Custom JWT Authentication) مبني بالكامل دون استخدام ASP.NET Core Identity (وفقاً للقرار المعماري `ADR-0001`).

---

## 2. تدفق تسجيل الدخول (Login Flow)

```mermaid
sequenceDiagram
    autonumber
    actor Client as العميل (Client)
    participant API as AuthController
    participant MediatR as MediatR Pipeline
    participant Handler as LoginCommandHandler
    participant DB as SQL Server (LocalDB)
    participant JWT as JwtTokenService

    Client->>API: POST /api/auth/login { userName, password }
    API->>MediatR: Send(LoginCommand)
    Note over MediatR: FluentValidation (LoginCommandValidator)
    MediatR->>Handler: Handle(LoginCommand)
    Handler->>DB: IUserRepository.GetByUserNameAsync(userName)
    
    alt المستخدم غير موجود
        Handler-->>Client: 401 Unauthorized (رسالة خطأ عامة)
    else المستخدم مقفول (IsLockedOut)
        Handler-->>Client: 423 Locked
    else كلمة المرور غير صحيحة
        Note over Handler: user.RegisterFailedAttempt()
        Handler->>DB: SaveChangesAsync()
        Handler-->>Client: 401 Unauthorized
    else كلمة المرور صحيحة وحساب نشط
        Note over Handler: user.ResetFailedAttempts()
        Handler->>DB: SaveChangesAsync()
        Handler->>JWT: GenerateToken(user)
        JWT-->>Handler: JWT Token + Expiry
        Handler-->>API: LoginResult
        API-->>Client: 200 OK { token, expiresAtUtc }
    end
```

### Claims المضمنة في JWT Token:

| Claim | المصدر | الغرض |
|---|---|---|
| `sub` | `User.Id` | المعرف الفريد للمستخدم (Guid v7) |
| `nameid` | `User.Id` | التوافق مع Claims القياسية |
| `unique_name` | `User.UserName` | اسم المستخدم |
| `email` | `User.Email` | البريد الإلكتروني |
| `role` | `Role.Name` | الدور الممنوح (مثل `MaintenanceAdmin`, `Employee`) |
| `security_stamp` | `User.SecurityStamp` | ختم الأمان لإبطال التوكنات |
| `iat` | `DateTimeOffset.UtcNow` | وقت الإصدار |
| `exp` | `UtcNow + 60 minutes` | وقت انتهاء الصلاحية |
| `iss` | `JwtOptions.Issuer` | مصدر التوكن |
| `aud` | `JwtOptions.Audience` | الجمهور المستهدف |

---

## 3. تدفق التحقق في كل طلب (Request Validation Flow & OnTokenValidated)

عند إرسال أي طلب إلى مسار محمي بـ `[Authorize]`:

```mermaid
flowchart TD
    A["استلام الطلب مع ترويسة Authorization: Bearer <Token>"] --> B{"التحقق القياسي من التوكن (التوقيع، الصلاحية الزمنية، Issuer، Audience)"}
    B -- "فشل" --> C["401 Unauthorized"]
    B -- "نجاح" --> D["حدث OnTokenValidated"]
    
    D --> E{"هل sub claim موجود وصالح؟"}
    E -- "لا" --> F["context.Fail() -> 401 Unauthorized"]
    E -- "نعم" --> G["استعلام DB: UserRepository.GetByIdAsync(userId)"]
    
    G --> H{"هل المستخدم موجود في قاعدة البيانات؟"}
    H -- "لا" --> F
    H -- "نعم" --> I{"هل الحساب نشط؟ (user.IsActive == true)"}
    I -- "لا" --> F
    I -- "نعم" --> J{"هل ختم الأمان متطابق؟ (user.SecurityStamp == claim)"}
    J -- "لا" --> F
    J -- "نعم" --> K["نجاح التحقق وتمرير الطلب إلى Controller / Handler"]
```

### ترتيب التحقق الدقيق داخل `OnTokenValidated`:
1. **التحقق من `sub` claim:** التأكد من وجوده وصلاحيته كـ `Guid`.
2. **استعلام المستخدم:** جلب المستخدم من قاعدة البيانات بواسطة `IUserRepository.GetByIdAsync(userId)`.
3. **التحقق من نشاط الحساب (`IsActive`):** إذا كان `false`، يُرفض الطلب فوراً.
4. **التحقق من تطابق `SecurityStamp`:** مقارنة القيمة المخزنة في DB مع القيمة الموجودة داخل Claim التوكن. إذا اختلفت، يُستدعى `context.Fail()` ويُرفض الطلب بـ `401 Unauthorized`.

---

## 4. أحداث تجديد `SecurityStamp` وإبطال التوكنات (Token Invalidation)

يتم تجديد `SecurityStamp` عبر استدعاء `User.RegenerateSecurityStamp()` في الحالات التالية:
1. **تغيير كلمة المرور:** سواء تم التغيير ذاتياً أو إعادة تعيينها بواسطة المسؤول (`Admin Password Reset`).
2. **تعديل أدوار المستخدم:** عند إضافة أو حذف دور من أدوار المستخدم، لضمان عدم استمرار التوكن القديم بصلاحيات منتهية.
3. **تعطيل حساب المستخدم:** عند إلغاء تفعيل المستخدم (`user.Deactivate()`).

> [!NOTE]
> بمجرد تجديد `SecurityStamp` في قاعدة البيانات، تصبح جميع التوكنات الصادرة سابقاً لهذا المستخدم غير صالحة تلقائياً في الطلب التالي، دون الحاجة لانتظار انتهاء صلاحيتها الزمنية (60 دقيقة).

---

## 5. المفاضلة المعمارية وتأثير الأداء (Performance Trade-off)

> [!IMPORTANT]
> **ملاحظة أداء حرجة (Database Roundtrip on Each Request):**
> التحقق من `SecurityStamp` و `IsActive` داخل حدث `OnTokenValidated` يتطلب استعلام قاعدة بيانات إضافي (`SELECT FROM Users`) مع كل طلب وارد إلى الـ API المصادق عليه.
> 
> **المبرر:**
> - يوفر إبطالاً فورياً وآمناً للتوكنات (Instant Revocation) دون الحاجة لتعقيد إضافي مثل إدارة Blacklist مركزية في Redis أو Message Broker في هذه المرحلة الأولية من المشروع.
> - يضمن تطبيق تغيير الصلاحيات وإغلاق الحسابات فوراً دون ثغرات أمنية.
> - استعلام المستخدم يتم عبر المفتاح الأساسي المعنقد (`Id` بنوع Guid v7)، وهو استعلام سريع جداً ومفهرس بالكامل.

