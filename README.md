# Maintenance Request Management System

نظام متكامل لإدارة ومتابعة طلبات الصيانة التشغيلية داخل المنشآت، يتيح تقديم الطلبات، إسنادها للفنيين المختصين، وتتبع دورة حياة الطلب خطوة بخطوة حتى الإغلاق وفق حوكمة وصلاحيات صارمة.

---

## 1. فكرة المشروع والأدوار

يعتمد النظام على نموذج صلاحيات مخصص يوزع المسؤوليات على ثلاثة أدوار رئيسية:

* **الموظف (Employee):** تقديم طلبات الصيانة وتتبع حالتها واستعراض طلباته الخاصة.
* **الفني (Technician):** استعراض الطلبات المسندة إليه، وبدء العمل عليها، وإشعار الإنجاز بحلها.
* **مسؤول الصيانة (MaintenanceAdmin):** إدارة أنواع الصيانة وتفعيلها/تعطيلها، إسناد الطلبات للفنيين، إدارة المستخدمين وحالاتهم، ورفض أو إغلاق الطلبات.

---

## 2. التقنيات المستخدمة

* **منصة التطوير:** C# / .NET 10 (C# 13)
* **واجهة برمجة التطبيقات:** ASP.NET Core Web API (Controllers)
* **نمط التصميم:** Onion Architecture مع CQRS عبر مكتبة MediatR
* **قواعد البيانات:** SQL Server (LocalDB) مع Entity Framework Core Code-First Migrations
* **التخزين المؤقت:** Redis (عبر Memurai على نظام Windows) مع StackExchange.Redis
* **المصادقة والتفويض:** نظام مصادقة مخصص بالكامل (Custom Auth)، تشفير BCrypt.Net-Next، وتوكنات JWT Bearer مع Guid v7
* **التحقق من البيانات:** FluentValidation مع MediatR Pipeline Behaviors
* **هيكلة الأخطاء:** معيار RFC 7807 المشتق عبر ProblemDetails

---

## 3. المعمارية البرمجية

يتبع المشروع معمارية البصلة (Onion Architecture) مقسماً إلى أربعة مشاريع مع اعتماديات تتجه من الخارج إلى الداخل:

* **MaintenanceSystem.Domain:** قلب النظام ويحتوي الكيانات (Rich Domain Model)، الدوال السلوكية، وقواعد العمل دون أي اعتماديات خارجية.
* **MaintenanceSystem.Application:** تنسيق حالات الاستخدام عبر Commands/Queries (Vertical Slice)، والواجهات المجردة (Abstractions).
* **MaintenanceSystem.Infrastructure:** التفاصيل التقنية وتنفيذ الواجهات (EF Core DbContext، التشفير بـ BCrypt، خدمة JWT، والتخزين المؤقت Redis).
* **MaintenanceSystem.API:** نقطة الدخول واستقبال طلبات HTTP، تحويل الـ Contracts إلى Commands/Queries، وتطبيق الـ Middlewares.

للمزيد من التفاصيل المعمارية، راجع: [الدليل المعماري للمشروع](docs/architecture-guide.md).

---

## 4. قرار نظام المصادقة المخصص (Custom Auth)

تم اتخاذ قرار معماري معتمد بعدم استخدام ASP.NET Core Identity أو جداول `AspNet*` مسبقة الصنع:
* **جداول مخصصة:** `Users` و `Roles` و `UserRoles` بمفاتيح تسلسلية متوافقة مع Guid v7.
* **تشفير كلمات المرور:** باستخدام خوارزمية BCrypt بمعامل تشغيل (Work Factor = 11).
* **سياسة القفل (Lockout Policy):** قفل الحساب تلقائياً لمدة 15 دقيقة بعد 5 محاولات دخول فاشلة متتالية.
* **إبطال التوكنات فورياً (SecurityStamp):** كل توكن يحمل ختم أمان، ويتم التحقق منه عند كل طلب؛ في حال تعديل الدور أو كلمة المرور أو تعطيل المستخدم يتغير الختم وتبطل التوكنات القديمة فوراً.

للاطلاع على القرار المعماري بالكامل، راجع: [وثيقة ADR-0001: Custom Authentication](docs/adr/0001-custom-auth.md).

---

## 5. متطلبات التشغيل

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* SQL Server LocalDB أو SQL Server Express
* Memurai Developer Edition (Redis لنظام Windows) مشغل على المنفذ الافتراضي `127.0.0.1:6379`
* موجه أوامر PowerShell أو Visual Studio 2026 / VS Code

---

## 6. خطوات التشغيل والإعداد

### 1. استنساخ المستودع واستعادة الحزم
```bash
git clone https://github.com/MohammedBaasheem/MaintenanceSystem.git
cd MaintenanceSystem
dotnet restore
```

### 2. تهيئة أسرار التطوير (User Secrets)
يتم تخزين مفتاح توقيع JWT وكلمات مرور الحسابات الأولية داخل User Secrets:
```bash
cd src/MaintenanceSystem.API
dotnet user-secrets set "Jwt:SigningKey" "YOUR_SUPER_SECRET_SIGNING_KEY_AT_LEAST_64_CHARS_LONG_HERE"
dotnet user-secrets set "Seed:AdminPassword" "YourAdminPasswordHere123!"
dotnet user-secrets set "Seed:EmployeePassword" "YourEmployeePasswordHere123!"
dotnet user-secrets set "Seed:TechnicianPassword" "YourTechnicianPasswordHere123!"
cd ../..
```

### 3. تطبيق تهجيرات قاعدة البيانات (Migrations)
```bash
dotnet ef database update --project src/MaintenanceSystem.Infrastructure --startup-project src/MaintenanceSystem.API
```

### 4. تشغيل خادم التطوير
```bash
dotnet run --project src/MaintenanceSystem.API
```
سيعمل الـ API افتراضياً على: `http://localhost:5188`.

---

## 7. حسابات بيئة التطوير

يقوم مكوّن `DevelopmentDataSeeder` تلقائياً بإنشاء الحسابات الأولية التالية عند أول تشغيل في بيئة التطوير (Development):

| اسم المستخدم (UserName) | البريد الإلكتروني | الدور الأساسي | مفتاح كلمة المرور في User Secrets |
|:-----------------------|:------------------|:-------------|:-----------------------------------|
| `admin` | `admin@system.local` | `MaintenanceAdmin` | `Seed:AdminPassword` |
| `employee` | `employee@system.local` | `Employee` | `Seed:EmployeePassword` |
| `technician` | `technician@system.local` | `Technician` | `Seed:TechnicianPassword` |

---

## 8. أمثلة الـ API

### تسجيل الدخول (POST /api/auth/login)
```http
POST /api/auth/login
Content-Type: application/json

{
  "userName": "admin",
  "password": "YourAdminPasswordHere123!"
}
```
**الاستجابة الناجحة (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-10-14T23:59:59Z"
}
```

### إنشاء نوع صيانة جديد (POST /api/maintenance-types)
```http
POST /api/maintenance-types
Authorization: Bearer <AdminToken>
Content-Type: application/json

{
  "name": "أنظمة التكييف والتبريد",
  "description": "صيانة وإصلاح وحدات التبريد والتكييف المركزي والسبليت"
}
```

### تقديم طلب صيانة (POST /api/maintenance-requests)
```http
POST /api/maintenance-requests
Authorization: Bearer <EmployeeToken>
Content-Type: application/json

{
  "title": "عطل في مكيف المكتب رقم 302",
  "description": "تسريب مياه مع انخفاض كفاءة التبريد",
  "priority": 2,
  "maintenanceTypeId": "019273c5-9271-7000-8000-000000000001"
}
```

---

## 9. اختبارات الـ HTTP

يتضمن المشروع ملف اختبارات تفاعلي شامل مخصص لملحق **VS Code REST Client** يغطي أكثر من 20 سيناريو:
* مسار الملف: [`requests/maintenance.http`](requests/maintenance.http)
* يغطي اختبارات تسجيل الدخول، إدارة المستخدمين، إدارة أنواع الصيانة، دورة حياة الطلبات كاملة، وحالات التحقق ورفض الصلاحيات (401، 403، 404، 409، 423).

---

## 10. الوثائق المرجعية

للاطلاع على التوثيق الفني الكامل والتفاصيل المعمارية:
1. [الدليل المعماري (Architecture Guide)](docs/architecture-guide.md): شرح شجرة المشاريع وقواعد الطبقات ومسار تدفق العمليات.
2. [تصميم قاعدة البيانات (Database Design)](docs/database-design.md): قاموس البيانات الكامل ومخطط الجداول والفهارس.
3. [القرار المعماري للمصادقة (ADR-0001: Custom Auth)](docs/adr/0001-custom-auth.md): حيثيات وتفاصيل تصميم نظام المصادقة المخصص.
4. [قائمة اختبارات القبول (Acceptance Checklist)](docs/acceptance-checklist.md): نتائج الاختبارات الحرجة وسيناريوهات الصمود وقفل الحسابات.
5. [خطة التطوير الشاملة (Development Plan)](docs/development-plan.md): الخطة المرجعية لجميع مراحل ومهام بناء النظام.

