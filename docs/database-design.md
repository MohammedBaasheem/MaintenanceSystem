
---

## 3. قاموس البيانات

### 3.1 جدول `Users`

| العمود | النوع | Null | Default | الوصف |
|--------|------|------|---------|-------|
| `Id` | `uniqueidentifier` | NO | `NEWSEQUENTIALID()` | PK، Guid v7 |
| `UserName` | `nvarchar(50)` | NO | — | فريد، غير فارغ |
| `Email` | `nvarchar(256)` | NO | — | فريد، بصيغة Email صحيحة |
| `PasswordHash` | `nvarchar(200)` | NO | — | BCrypt hash (يشمل Salt) |
| `SecurityStamp` | `uniqueidentifier` | NO | — | يُجدَّد عند تغيير كلمة المرور/الأدوار/التعطيل |
| `IsActive` | `bit` | NO | `1` | تعطيل بدل حذف |
| `FailedLoginCount` | `int` | NO | `0` | عدّاد المحاولات الفاشلة |
| `LockoutEndUtc` | `datetime2` | YES | `NULL` | نهاية القفل |
| `CreatedAtUtc` | `datetime2` | NO | — | وقت الإنشاء |
| `UpdatedAtUtc` | `datetime2` | YES | `NULL` | آخر تعديل |

**الفهارس:**
- `IX_Users_UserName` — Unique.
- `IX_Users_Email` — Unique.
- `IX_Users_LockoutEndUtc` — للاستعلامات الإدارية.

**القيود:**
- `CHECK (FailedLoginCount >= 0)`.

**الحذف:** ممنوع ماديًا. يُستخدم `IsActive = 0`.

---

### 3.2 جدول `Roles`

| العمود | النوع | Null | Default | الوصف |
|--------|------|------|---------|-------|
| `Id` | `uniqueidentifier` | NO | `NEWSEQUENTIALID()` | PK |
| `Name` | `nvarchar(50)` | NO | — | فريد: `Employee`, `Technician`, `MaintenanceAdmin` |
| `Description` | `nvarchar(200)` | YES | `NULL` | وصف اختياري |

**الفهارس:**
- `IX_Roles_Name` — Unique.

**البيانات الأولية (Seed):**
| Name | Description |
|------|-------------|
| `Employee` | موظف ينشئ طلبات الصيانة |
| `Technician` | فني يعالج الطلبات المسندة إليه |
| `MaintenanceAdmin` | مسؤول إدارة أنواع الصيانة والمستخدمين والإسناد |

**الحذف:** ممنوع ماديًا.

---

### 3.3 جدول `UserRoles`

| العمود | النوع | Null | الوصف |
|--------|------|------|-------|
| `UserId` | `uniqueidentifier` | NO | FK → `Users.Id` (Restrict) |
| `RoleId` | `uniqueidentifier` | NO | FK → `Roles.Id` (Restrict) |

**المفتاح الأساسي:** مركّب `(UserId, RoleId)`.

**الفهارس:**
- `IX_UserRoles_RoleId` — لتسريع البحث عن المستخدمين بدور معين.

**العلاقات:**
- `FK_UserRoles_Users_UserId` — `On Delete: Restrict`.
- `FK_UserRoles_Roles_RoleId` — `On Delete: Restrict`.

**ملاحظة:** المستخدم يمكن أن يكون له دور واحد فقط في الإصدار الأول (منطق التطبيق)، لكن الجدول يسمح بأكثر من دور (مرونة مستقبلية).

---

### 3.4 جدول `MaintenanceTypes`

| العمود | النوع | Null | Default | الوصف |
|--------|------|------|---------|-------|
| `Id` | `uniqueidentifier` | NO | `NEWSEQUENTIALID()` | PK |
| `Name` | `nvarchar(100)` | NO | — | فريد، غير فارغ |
| `Description` | `nvarchar(500)` | YES | `NULL` | وصف اختياري |
| `IsActive` | `bit` | NO | `1` | تعطيل بدل حذف |
| `CreatedAtUtc` | `datetime2` | NO | — | وقت الإنشاء |
| `UpdatedAtUtc` | `datetime2` | YES | `NULL` | آخر تعديل |

**الفهارس:**
- `IX_MaintenanceTypes_Name` — Unique.
- `IX_MaintenanceTypes_IsActive` — للاستعلامات على النشطة.

**الحذف:** ممنوع ماديًا عند وجود طلبات مرتبطة. يُستخدم `IsActive = 0`.

**القيود:**
- `CHECK (LEN(Name) > 0)`.

---

### 3.5 جدول `MaintenanceRequests`

| العمود | النوع | Null | Default | الوصف |
|--------|------|------|---------|-------|
| `Id` | `uniqueidentifier` | NO | `NEWSEQUENTIALID()` | PK |
| `Title` | `nvarchar(200)` | NO | — | غير فارغ |
| `Description` | `nvarchar(2000)` | NO | — | غير فارغ |
| `Priority` | `int` | NO | — | Enum: 0=Low, 1=Medium, 2=High, 3=Critical |
| `Status` | `int` | NO | `0` | Enum: 0=New, 1=Assigned, 2=InProgress, 3=Resolved, 4=Closed, 5=Rejected |
| `MaintenanceTypeId` | `uniqueidentifier` | NO | — | FK → `MaintenanceTypes.Id` (Restrict) |
| `CreatedByUserId` | `uniqueidentifier` | NO | — | FK → `Users.Id` (Restrict) |
| `AssignedTechnicianId` | `uniqueidentifier` | YES | `NULL` | FK → `Users.Id` (Set Null) |
| `RejectionReason` | `nvarchar(500)` | YES | `NULL` | مطلوب عند `Status = Rejected` |
| `CreatedAtUtc` | `datetime2` | NO | — | وقت الإنشاء |
| `AssignedAtUtc` | `datetime2` | YES | `NULL` | وقت الإسناد |
| `ResolvedAtUtc` | `datetime2` | YES | `NULL` | وقت الحل |
| `ClosedAtUtc` | `datetime2` | YES | `NULL` | وقت الإغلاق |
| `UpdatedAtUtc` | `datetime2` | YES | `NULL` | آخر تعديل |

**الفهارس:**
- `IX_MaintenanceRequests_CreatedByUserId` — للاستعلام "طلباتي".
- `IX_MaintenanceRequests_AssignedTechnicianId` — للاستعلام "الطلبات المسندة إليّ".
- `IX_MaintenanceRequests_Status` — للفلترة.
- `IX_MaintenanceRequests_MaintenanceTypeId` — للعلاقة.
- `IX_MaintenanceRequests_Status_AssignedTechnicianId` — مركّب لاستعلامات الفني.

**العلاقات:**
- `FK_MaintenanceRequests_MaintenanceTypes_MaintenanceTypeId` — `On Delete: Restrict`.
- `FK_MaintenanceRequests_Users_CreatedByUserId` — `On Delete: Restrict`.
- `FK_MaintenanceRequests_Users_AssignedTechnicianId` — `On Delete: Set Null`.

**القيود:**
- `CHECK (Priority BETWEEN 0 AND 3)`.
- `CHECK (Status BETWEEN 0 AND 5)`.
- `CHECK (LEN(Title) > 0)`.
- `CHECK (LEN(Description) > 0)`.

---

## 4. قواعد العمل المرتبطة بقاعدة البيانات

### 4.1 ما يُفرض في قاعدة البيانات
- الفهارس الفريدة (UserName, Email, Role.Name, MaintenanceType.Name).
- القيود (`CHECK`).
- العلاقات (`FK`).
- القيم الافتراضية (`Status = 0`, `IsActive = 1`).

### 4.2 ما يُفرض في Domain
- انتقالات الحالة (`New → Assigned → InProgress → Resolved → Closed`).
- `RejectionReason` مطلوب عند `Rejected`.
- `ResolvedAtUtc` يُضبط عند `Resolved`.
- `ClosedAtUtc` يُضبط عند `Closed`.
- لا يمكن إسناد طلب مغلق أو مرفوض.
- لا يمكن تعديل طلب بعد الإغلاق.

### 4.3 ما يُفرض في Application
- التحقق من وجود `MaintenanceType` وأنه نشط قبل الإنشاء.
- التحقق من أن المستخدم المستهدف فني عبر `UserRoles`.
- التحقق من ملكية الطلب (`CreatedByUserId == CurrentUserId`).
- التحقق من أن الفني هو المسند إليه قبل تغيير الحالة.

---

## 5. استراتيجية المفاتيح (Guid v7)

### 5.1 لماذا Guid v7 (Sequential)؟
- **Guid عشوائي:** يسبب Fragmenting في الفهارس العنقودية.
- **Guid v7:** يعتمد على الوقت + عشوائية، فيُولّد قيمًا متسلسلة جزئيًا.
- **النتيجة:** فهارس أفضل، أداء إدراج أعلى.

### 5.2 كيف يُولَّد؟
- في .NET 9: `Guid.CreateVersion7()`.
- في .NET 8: استخدام مكتبة `SequentialGuid` أو توليد يدوي.
- في SQL Server: `NEWSEQUENTIALID()` كـ Default.

**القرار:** التوليد في .NET عبر `Entity` base class، مع `NEWSEQUENTIALID()` كـ Default احتياطي.

---

## 6. استراتيجية الحذف

| الجدول | الحذف المادي | البديل |
|--------|--------------|--------|
| `Users` | ❌ ممنوع | `IsActive = 0` |
| `Roles` | ❌ ممنوع | لا يُحذف |
| `UserRoles` | ✅ مسموح | عند تغيير دور المستخدم |
| `MaintenanceTypes` | ❌ ممنوع إذا له طلبات | `IsActive = 0` |
| `MaintenanceRequests` | ❌ ممنوع | لا يُحذف (سجل تاريخي) |

**لا `Cascade Delete`** في أي علاقة.

---

## 7. استراتيجية الفهارس

### 7.1 الفهارس الإلزامية
- كل PK (تلقائي).
- كل FK (تلقائي).
- كل Unique Constraint.

### 7.2 الفهارس المضافة يدويًا
| الفهرس | السبب |
|--------|-------|
| `IX_Users_LockoutEndUtc` | استعلامات "المستخدمون المقفولون" |
| `IX_UserRoles_RoleId` | البحث عن مستخدمين بدور معين |
| `IX_MaintenanceRequests_Status` | فلترة الطلبات بالحالة |
| `IX_MaintenanceRequests_Status_AssignedTechnicianId` | استعلامات الفني |

### 7.3 الفهارس المرفوضة
- فهرس على `Priority` وحده — تنوّع منخفض، لا فائدة.
- فهرس على `CreatedAtUtc` وحده — لا استعلام يعتمد عليه وحده.

---

## 8. خريطة Migrations

| # | Migration | الجداول المضافة | المرحلة | ملاحظات |
|---|-----------|-----------------|---------|---------|
| 1 | `InitialAuth` | `Users`, `Roles`, `UserRoles` | المرحلة 3 (MS-014) | لا جداول AspNet* |
| 2 | `AddMaintenanceTypes` | `MaintenanceTypes` | المرحلة 4 (MS-018) | لا تعد جداول Auth |
| 3 | `AddMaintenanceRequests` | `MaintenanceRequests` | المرحلة 5 (MS-025) | لا تعد الجداول السابقة |

**قاعدة:** كل Migration تطور المخطط، لا تعيد بناءه.

---

## 9. قرارات مؤجلة (Future Enhancements)

| القرار | السبب |
|--------|-------|
| جدول `RefreshTokens` | مؤجل مع Refresh Tokens |
| جدول `Permissions` / `RolePermissions` | الأدوار كافية حاليًا |
| جدول `MaintenanceRequestHistory` | تتبع انتقالات الحالة — مؤجل |
| `RowVersion` على `MaintenanceRequests` | التزامن — مؤجل |
| Soft Delete للمستخدمين | غير مطلوب |
| جدول `AuditLog` | غير مطلوب |

---

## 10. ما هو خارج نطاق هذا المستند

- تفاصيل كل Migration → في الخطة (MS-014, MS-018, MS-025).
- تفاصيل JWT وSecurityStamp → `docs/adr/0001-custom-auth.md`.
- تفاصيل Lockout → `docs/adr/0002-lockout-policy.md`.
- تفاصيل الطبقات → `docs/architecture-guide.md`.

---

**انتهى المستند — الإصدار 1.0**