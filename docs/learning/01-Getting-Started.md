# 🚀 دليل الإعداد والتشغيل المحلي (Getting Started Guide)

> دليل عملي خطوة بخطوة لتهيئة بيئة التطوير، ضبط أسرار المستخدم (User Secrets)، تطبيق هجرات قاعدة البيانات (EF Core Migrations)، وتشغيل واختبار نظام إدارة طلبات الصيانة محلياً على جهازك.

---

## 📋 1. المتطلبات المسبقة (Prerequisites Checklist)

تأكد من تثبيت الأدوات التالية على نظام التشغيل الخاص بك قبل البدء:

- [ ] **[.NET 10 SDK](https://dotnet.microsoft.com/):** تحقق عبر الأمر `dotnet --version` (يجب أن يبدأ بـ `10.x`).
- [ ] **[Git](https://git-scm.com/):** لإدارة المستودع والفروع.
- [ ] **SQL Server LocalDB:** مثبت تلقائياً مع Visual Studio، أو تثبيت SQL Server Express / Developer.
  - تأكد من تشغيل الخادم المحلي عبر PowerShell:
    ```powershell
    sqllocaldb start MSSQLLocalDB
    ```
- [ ] **Redis Server:** يعمل على المنفذ الافتراضي `127.0.0.1:6379`.
  - على نظام Windows، يُفضل استخدام **Memurai Developer Edition** أو تشغيل Redis عبر **WSL2** أو **Docker**:
    ```powershell
    docker run --name redis -p 6379:6379 -d redis:alpine
    ```
- [ ] **أداة `dotnet-ef` العامة:**
  ```powershell
  dotnet tool install --global dotnet-ef
  ```

---

## 🛠️ 2. خطوات الإعداد والتشغيل الست (The 6 Setup Steps)

### 1️⃣ الخطوة الأولى: استنساخ المستودع (Clone)

افتح الطرفية (PowerShell) وانتقل إلى مجلد المشاريع المفضل لديك:

```powershell
git clone https://github.com/your-org/MaintenanceSystem.git
cd MaintenanceSystem
```

---

### 2️⃣ الخطوة الثانية: استعادة الحزم البرمجية (Restore)

قم باستعادة حزم NuGet لكافة مشاريع الحل (`Solution`):

```powershell
dotnet restore
```

> [!NOTE]
> تحقق من عدم وجود أي تحذيرات أمنية بتشغيل:
> ```powershell
> dotnet list package --vulnerable
> ```

---

### 3️⃣ الخطوة الثالثة: تهيئة أسرار المستخدم (User Secrets)

وفق مبادئ الأمان الصارمة، **لا يحتوي كود المشروع على أي كلمات مرور أو سلاسل اتصال أو مفاتيح تشفير**. يتم حفظ كافة المتغيرات الحساسة في أداة `Secret Manager` الخاصة بـ .NET.

انتقل إلى مجلد مشروع الـ API وقم بتهيئة الأسرار:

```powershell
cd src/MaintenanceSystem.API
dotnet user-secrets init
```

قم بضبط **المفاتيح الستة الأساسية** عبر الأوامر التالية:

```powershell
# 1. سلسلة الاتصال بقاعدة بيانات SQL Server LocalDB
dotnet user-secrets set "ConnectionStrings:Default" "Server=(localdb)\mssqllocaldb;Database=MaintenanceSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"

# 2. سلسلة الاتصال بخادم Redis للكاش
dotnet user-secrets set "ConnectionStrings:Redis" "127.0.0.1:6379"

# 3. المفتاح السري لتوقيع توكنات JWT (يجب أن يكون قوياً وبطول 32 محرفاً على الأقل)
dotnet user-secrets set "Jwt:SigningKey" "SuperSecretKeyForMaintenanceSystemDevEnvironment2026!#"

# 4. كلمة مرور حساب مدير الصيانة الأولي (Admin)
dotnet user-secrets set "Seed:AdminPassword" "AdminSecurePassword123!"

# 5. كلمة مرور حساب الموظف الأولي (Employee)
dotnet user-secrets set "Seed:EmployeePassword" "EmployeeSecurePassword123!"

# 6. كلمة مرور حساب فني الصيانة الأولي (Technician)
dotnet user-secrets set "Seed:TechnicianPassword" "TechnicianSecurePassword123!"
```

> [!IMPORTANT]
> **قائمة المفاتيح المطلوبة إجبارياً:**
> 1. `ConnectionStrings:Default`
> 2. `ConnectionStrings:Redis`
> 3. `Jwt:SigningKey`
> 4. `Seed:AdminPassword`
> 5. `Seed:EmployeePassword`
> 6. `Seed:TechnicianPassword`
>
> نسيان أي مفتاح من هذه المفاتيح سيؤدي إلى فشل بدء تشغيل الخادم فورياً لمنع العمل بإعدادات غير آمنة.

---

### 4️⃣ الخطوة الرابعة: تطبيق هجرات قاعدة البيانات (Database Migrations)

عد إلى المجلد الرئيسي للمستودع، ثم طبق أحدث الميجريشنز لإنشاء الجداول في `MaintenanceSystemDb`:

```powershell
cd ../..
dotnet ef database update --project src/MaintenanceSystem.Infrastructure --startup-project src/MaintenanceSystem.API
```

> [!TIP]
> يقوم Entity Framework Core ببناء الجداول التالية:
> - `Users` و `Roles` و `UserRoles` (Custom Auth).
> - `MaintenanceTypes` (أنواع الصيانة).
> - `MaintenanceRequests` (طلبات الصيانة).

---

### 5️⃣ الخطوة الخامسة: تشغيل الخادم والتغذية التلقائية (Run & Seed)

شغّل خادم الـ API عبر الأمر:

```powershell
dotnet run --project src/MaintenanceSystem.API
```

**ماذا يحدث أثناء الإقلاع في بيئة التطوير (Development)؟**
يقوم كلاس [`DevelopmentDataSeeder.cs`](file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/src/MaintenanceSystem.Infrastructure/Persistence/Seed/DevelopmentDataSeeder.cs) تلقائياً بما يلي:
1. التأكد من وجود الأدوار الأساسية (`MaintenanceAdmin`, `Employee`, `Technician`).
2. إنشاء الحسابات الأولية الثلاثة وربطها بكلمات المرور المحددة في `User Secrets`:
   - `admin` ⬅️ دور `MaintenanceAdmin`
   - `employee` ⬅️ دور `Employee`
   - `technician` ⬅️ دور `Technician`
3. التغذية تتم بشكل تكراري آمن (`Idempotent`)؛ إذا كانت الحسابات موجودة مسبقاً يتم تخطيها دون تعديل.

---

### 6️⃣ الخطوة السادسة: الاختبار والتحقق (Test & Verification)

#### أ) تشغيل حزمة الاختبارات الآلية
افتح نافذة طرفية أخرى وتأكد من نجاح كافة اختبارات الوحدة والتكامل:
```powershell
dotnet test
```

#### ب) استعراض واجهة Swagger التفاعلية
افتح المتصفح على العنوان التالي:
- **Swagger UI:** `http://localhost:5000/swagger` أو `https://localhost:5001/swagger`

#### ج) تجربة تسجيل الدخول السريعة عبر PowerShell
```powershell
$body = @{
    userName = "admin"
    password = "AdminSecurePassword123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $body -ContentType "application/json"
Write-Host "تم تسجيل الدخول بنجاح! التوكن:" -ForegroundColor Green
$response.token
```

---

## 🔧 3. استكشاف الأخطاء وإصلاحها (Troubleshooting)

### ❌ المشكلة 1: `Connection string 'Default' not found` أو خطأ في LocalDB
- **السبب:** لم يتم إدخال `ConnectionStrings:Default` في User Secrets، أو خدمة LocalDB متوقفة.
- **الحل:**
  1. أعد تشغيل LocalDB: `sqllocaldb start MSSQLLocalDB`.
  2. تأكد من ضبط الأسرار داخل مجلد `src/MaintenanceSystem.API`.

### ❌ المشكلة 2: `Password secret 'Seed:AdminPassword' is not configured`
- **السبب:** محاولة تشغيل التطبيق دون ضبط كلمات مرور التغذية الأولية.
- **الحل:** راجع الخطوة الثالثة وأدخل قيم `Seed:AdminPassword`, `Seed:EmployeePassword`, `Seed:TechnicianPassword`.

### ❌ المشكلة 3: تحذيرات اتصال Redis في الـ Log
- **الأثر:** التطبيق لن يتوقف؛ سيعمل تلقائياً بنمط الارتداد لقاعدة البيانات (Fallback to SQL Server)، لكن استعلامات الكاش ستكون أبطأ.
- **الحل:** تأكد من تشغيل Memurai أو حاوية Docker الخاصة بـ Redis على المنفذ `6379`.

### ❌ المشكلة 4: خطأ `Jwt:SigningKey is not configured`
- **السبب:** مفتاح التوقيع مفقود أو فارغ.
- **الحل:** قم بضبط مفتاح نصي لا يقل طوله عن 256 بت (32 حرفاً) عبر:
  `dotnet user-secrets set "Jwt:SigningKey" "YourLongSecretKeyHere..."`.

---

## 💡 4. نقاط ذهبية للمطور (Golden Setup Rules)

> [!TIP]
> - **تذكر:** مجلد `User Secrets` يُخزن خارج مسار المستودع تماماً في مجلد المستخدم على نظام التشغيل، لذا لن يتم رفعه أبداً بالخطأ إلى GitHub.
> - **تذكر:** عند إضافة أي مطور جديد لفريق العمل، شارك معه قائمة المفاتيح المطلوبة دون كتابة قيمها في ملفات `appsettings.json`.
> - **تذكر:** في بيئات الإنتاج (Production)، تُستبدل هذه الأسرار بمتغيرات بيئة (Environment Variables) أو خدمات مثل **Azure Key Vault** أو **AWS Secrets Manager**.
