

# ADR-0001: نظام مصادقة وتفويض مخصص

**الحالة:** ✅ معتمد
**التاريخ:** 2026-10-13
**المرجع:** `MaintenanceSystem_Development_Backlog_v4.md`، `docs/architecture-guide.md`

---

## 1. السياق

المشروع يحتاج نظام مصادقة وتفويض يدعم:
- ثلاثة أدوار: `Employee`, `Technician`, `MaintenanceAdmin`.
- JWT للوصول إلى API.
- تسجيل دخول بكلمة مرور.
- إدارة مستخدمين بواسطة Admin.
- حماية من هجمات Brute Force.

الخيار الشائع هو **ASP.NET Core Identity**، لكن صاحب المشروع يريد بناء نظام مخصص للتحكم الكامل في الجداول والمنطق.

---

## 2. القرار

**نبني نظام مصادقة وتفويض مخصصًا** يستخدم:

| المكوّن | الاختيار | السبب |
|---------|----------|-------|
| Hashing | **BCrypt.Net-Next** (Work Factor 11) | ناضج، بسيط، يُدمج Salt |
| Token | **JWT** (60 دقيقة) | قياسي، stateless |
| Signing | HMAC SHA-256 | كافٍ لمشروع واحد |
| الأدوار | جدول `Roles` مخصص | 3 أدوار فقط، لا حاجة لتعقيد |
| التفويض | Role-based Policies | الأدوار كافية |
| إبطال Token | SecurityStamp | أبسط طريقة |
| Lockout | 5 محاولات / 15 دقيقة | OWASP موصى به |
| Refresh Tokens | ❌ مؤجلة | خارج النطاق |
| التسجيل العام | ❌ ممنوع | Admin فقط |
| Seed | Admin + Employee + Technician | للتطوير فقط |

**لا نستخدم ASP.NET Core Identity بأي شكل.**

---

## 3. المبررات

### 3.1 لماذا لا Identity؟
- نريد جداول خاصة بنا (Users, Roles, UserRoles).
- لا نحتاج ميزات Identity المعقدة (Claims, Logins, Tokens).
- تحكم كامل في المنطق.
- تعلم عميق لنظام المصادقة.

### 3.2 لماذا BCrypt وليس PBKDF2 أو Argon2؟
- **BCrypt:** ناضج، بسيط، مكتبة واحدة.
- **PBKDF2:** مدمج في .NET لكن يحتاج إعدادًا يدويًا.
- **Argon2:** أقوى لكن أقل انتشارًا في .NET.

BCrypt كافٍ لهذا المشروع، ومستخدم على نطاق واسع.

### 3.3 لماذا SecurityStamp؟
- أبسط طريقة لإبطال التوكنات عند تغيير كلمة المرور/الأدوار/التعطيل.
- لا حاجة لجدول `RevokedTokens`.
- التكلفة: استعلام DB لكل طلب مصادق (مقبول).

### 3.4 لماذا لا Refresh Tokens الآن؟
- توسيع للنطاق.
- تحتاج جدولًا جديدًا ومنطق Rotation.
- 60 دقيقة كافية للتطوير.
- يمكن إضافتها لاحقًا.

### 3.5 لماذا لا Permissions دقيقة؟
- 3 أدوار تغطي كل حالات الاستخدام.
- Permissions تضيف جداول وتعقيدًا.
- يمكن إضافتها لاحقًا.

---

## 4. المخطط التفصيلي

### 4.1 جداول قاعدة البيانات

انظر `docs/database-design.md` القسم 3.1، 3.2، 3.3.

### 4.2 تدفق تسجيل الدخول
1. POST /api/auth/login { userName, password }
    
2. Controller → LoginCommand
    
3. ValidationBehavior → التحقق من المدخلات
    
4. Handler:  
    a. UserRepository.GetByUserNameAsync(userName)  
    b. إذا غير موجود → UnauthorizedException (رسالة عامة)  
    c. إذا !IsActive → UnauthorizedException (رسالة عامة)  
    d. إذا user.IsLockedOut() → 423 Locked  
    e. IPasswordHasher.Verify(password, user.PasswordHash)  
    f. إذا فشل → user.RegisterFailedAttempt() → SaveChanges → UnauthorizedException  
    g. إذا نجح → user.ResetLockout() → SaveChanges  
    h. ITokenService.GenerateToken(user) → JWT
    
5. Response: { accessToken, expiresAtUtc, user: { id, userName, role } }
    

text

### 4.3 محتوى JWT
**Claims:**
| Claim | القيمة |
|-------|--------|
| `sub` | User.Id |
| `nameid` | User.Id |
| `unique_name` | User.UserName |
| `email` | User.Email |
| `role` | Role.Name |
| `security_stamp` | User.SecurityStamp |
| `iat` | وقت الإصدار |
| `exp` | وقت الانتهاء (60 دقيقة) |
| `iss` | Issuer |
| `aud` | Audience |
### 4.4 التحقق من JWT في كل طلب

1. JwtBearerMiddleware يقرأ Authorization: Bearer <token>
    
2. يتحقق من التوقيع، Issuer، Audience، Lifetime
    
3. OnTokenValidated:  
    a. يقرأ security_stamp من Claims  
    b. UserRepository.GetByIdAsync(userId)  
    c. إذا User.SecurityStamp != claim → context.Fail()  
    d. إذا !User.IsActive → context.Fail()
    
4. إذا نجح → HttpContext.User مبني
    

text

### 4.5 Lockout
**السياسة:**
- `MaxFailedAttempts = 5`.
- `LockoutDuration = 15 minutes`.
**منطق:**

On failed login:  
user.FailedLoginCount++  
if user.FailedLoginCount >= 5:  
user.LockoutEndUtc = UtcNow + 15min  
SaveChanges

On successful login:  
user.FailedLoginCount = 0  
user.LockoutEndUtc = null  
SaveChanges

IsLockedOut():  
return LockoutEndUtc.HasValue && LockoutEndUtc.Value > UtcNow

text

### 4.6 SecurityStamp
**يُجدَّد عند:**
- تغيير كلمة المرور (Admin reset أو self change).
- تغيير الأدوار.
- تعطيل الحساب.
**التأثير:**
- كل التوكنات القديمة تُرفض في الطلب التالي.
- لا حاجة لإبطال يدوي.
### 4.7 إدارة المستخدمين
**كل المسارات محمية بـ `MaintenanceAdmin`:**
| المسار | الطريقة | الوظيفة |
|--------|---------|---------|
| `POST /api/users` | POST | إنشاء مستخدم |
| `PATCH /api/users/{id}/status` | PATCH | تفعيل/تعطيل |
| `PATCH /api/users/{id}/reset-password` | PATCH | إعادة تعيين كلمة المرور |
| `PATCH /api/users/{id}/roles` | PATCH | تغيير الدور |
**قيود:**
- Admin لا يستطيع تعطيل نفسه.
- Admin لا يستطيع تغيير دوره.
- كلمة المرور المؤقتة تُعاد مرة واحدة.
- لا يوجد `POST /api/auth/register`.
### 4.8 Seeder
- يُشغَّل فقط في `Development`.
- يُنشئ:
  - Admin (بيانات من User Secrets).
  - Employee.
  - Technician.
- إذا وُجد Admin بالفعل، لا يُنشئ مرة أخرى.
- في `Production`، لا يُنشأ شيء.
---
## 5. الاعتماديات (NuGet)
| الحزمة | الغرض |
|--------|-------|
| `BCrypt.Net-Next` | Hashing |
| `System.IdentityModel.Tokens.Jwt` | إصدار JWT |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | التحقق من JWT |
| `Microsoft.EntityFrameworkCore.SqlServer` | EF Core |
| `Microsoft.EntityFrameworkCore.Design` | Migrations |
---
## 6. الاعتبارات الأمنية
### 6.1 مُعالجة
- ✅ BCrypt لـ Hashing (لا نص عادي).
- ✅ SecurityStamp لإبطال التوكنات.
- ✅ Lockout بعد 5 محاولات.
- ✅ رسائل خطأ عامة في Login (لا User Enumeration).
- ✅ SigningKey في User Secrets (ليس في Git).
- ✅ HTTPS إلزامي في الإنتاج.
- ✅ لا كشف تفاصيل داخلية في 500.
- ✅ Admin لا يعطّل نفسه.
### 6.2 خارج النطاق (مؤجل)
- ⏸️ Refresh Tokens.
- ⏸️ Email Confirmation.
- ⏸️ Password Reset عبر البريد.
- ⏸️ 2FA.
- ⏸️ External Login.
- ⏸️ Rate Limiting على Login.
- ⏸️ CAPTCHA.
---
## 7. الاعتبارات المعمارية
### 7.1 أين يوجد كل شيء؟
| المكوّن | الطبقة |
|---------|--------|
| `User`, `Role`, `UserRole` | Domain |
| دوال Lockout | Domain (في `User`) |
| `IPasswordHasher` | Application |
| `ITokenService` | Application |
| `ICurrentUserService` | Application |
| `LoginCommand/Handler` | Application |
| `CreateUserCommand/Handler` | Application |
| `BCryptPasswordHasher` | Infrastructure |
| `JwtTokenService` | Infrastructure |
| `CurrentUserService` | Infrastructure |
| `UserRepository` | Infrastructure |
| `AuthController` | API |
| `UsersController` | API |
| `JwtBearer` config | API (Program.cs) |
| `OnTokenValidated` | API |
### 7.2 الاعتماديات بين الطبقات

API → Application → Domain  
API → Infrastructure → Application → Domain

text

لا عكس.
---
## 8. النتائج المتوقعة
### 8.1 إيجابيات
- تحكم كامل في الجداول.
- لا اعتماد على Identity.
- فهم عميق للمصادقة.
- كود قابل للاختبار.
### 8.2 سلبيات
- مسؤولية أمنية أكبر.
- وقت تطوير أطول قليلًا.
- حاجة لاختبارات أمان شاملة.
### 8.3 مخاطر
- خطأ في BCrypt أو JWT → ثغرة.
- **التخفيف:** استخدام مكتبات مُختبرة، اختبارات أمان شاملة (MS-016a, MS-043).
---
## 9. القرارات البديلة المرفوضة
| البديل | السبب |
|--------|-------|
| ASP.NET Core Identity | لا يريد جداول AspNet* |
| JWT يدوي من الصفر | مخاطرة أمنية |
| Sessions بدل JWT | لا يناسب API |
| API Keys | لا يدعم الأدوار |
| OAuth2 فقط | لا يناسب السيناريو |
| Refresh Tokens الآن | توسيع النطاق |
| Permissions دقيقة الآن | الأدوار كافية |
---
## 10. المراجع
- `docs/architecture-guide.md` — القسم 3 (Domain)، القسم 4 (Application).
- `docs/database-design.md` — القسم 3.1، 3.2، 3.3.
- `docs/adr/0002-lockout-policy.md` — تفاصيل Lockout.
- MS-013 إلى MS-016a في الخطة v4.0.
- OWASP: Password Storage Cheat Sheet.
- OWASP: Authentication Cheat Sheet.
---
**انتهى المستند — الإصدار 1.0**