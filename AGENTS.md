# AGENTS.md — Maintenance Request Management System

## Project Identity

Backend API على **.NET 10** (Onion Architecture + CQRS + Rich Domain Model).
المشروع يتبع وثائق مرجعية في مجلد `docs/`.

## Critical Rules (No Exceptions)

- **لا تستخدم ASP.NET Core Identity بأي شكل.**
- **لا تُنشئ جداول `AspNet*`.**
- **لا تُنشئ جميع الجداول مسبقاً.** كل Migration تطور المخطط فقط.
- **لا تبدأ Redis قبل نجاح قراءة أنواع الصيانة من SQL Server.**
- **لا تنشئ `MaintenanceRequest` قبل MS-025.**
- **الدوال السلوكية في Domain، لا في Handler أو Controller.**
- **لا تثق في `UserId` أو `Role` أو `Status` من العميل.** استخدم `ICurrentUserService` و Claims.
- **لا تضف Generic Repository أو AutoMapper أو Unit of Work أو Result Pattern.**
- **لا تنتقل إلى المهمة التالية قبل تحقق معايير قبول المهمة الحالية.**

## Session Protocol (Follow Every Task)

عند طلب تنفيذ مهمة (مثل "نفذ MS-001")، اتبع هذا البروتوكول بدقة:

1. **اقرأ المهمة:** افتح `docs/development-plan.md`، وابحث عن رقم المهمة المطلوب.
2. **اقرأ الوثائق المرتبطة:** إذا أشارت المهمة إلى وثيقة (architecture-guide, database-design, adr/...)، اقرأ القسم المحدد فقط، لا الملف كاملاً.
3. **اعرض خطة التنفيذ:** قبل أي تعديل أو أمر، اعرض على المستخدم:
   - ملخصاً لما فهمته من المهمة.
   - الخطوات التي تنوي تنفيذها بالترتيب.
   - الملفات التي ستُنشئها أو تعدلها.
   - معايير القبول التي ستتحقق منها.
4. **انتظر الموافقة:** لا تنفذ أي شيء حتى يوافق المستخدم.
5. **نفّذ خطوة بخطوة:** بعد الموافقة، نفذ كل خطوة على حدة. قبل كل أمر طرفية أو تعديل ملف، اعرض ما ستفعله وانتظر الموافقة (وفق إعدادات Security Preset = Default).
6. **تحقق من معايير القبول:** بعد اكتمال التنفيذ، تحقق من كل معيار قبول في المهمة.
7. **أبلغ النتيجة:** اعرض ملخصاً موجزاً: ما تم تنفيذه، ما تحقق، ما لم يتحقق (إن وُجد)، وأي ملاحظات.
8. **توقف:** لا تنتقل إلى المهمة التالية تلقائياً. انتظر طلب المستخدم.

**قواعد صارمة أثناء الجلسة:**

- لا تقرأ ملفات لا علاقة لها بالمهمة الحالية.
- لا تعدّل ملفات خارج نطاق المهمة.
- لا تنشئ ملفات مساعدة أو توثيقية إضافية إلا إذا طلبت المهمة ذلك صراحة.
- لا تُعِد شرحاً طويلاً لما فهمته — أبقِ الخطط موجزة وقابلة للقراءة السريعة.
- إذا واجهت غموضاً، اسأل قبل التخمين.

## Intentional Deviations from Docs

- **.NET 10** بدلاً من .NET 8 (قرار مقصود، VS 2026).
- **`Guid.CreateVersion7()` المدمج** بدلاً من مكتبة خارجية.

## Git — Ask Before Every Command

الوكيل مسموح له بتنفيذ أوامر Git (init, add, commit, push, status, branch, checkout, merge, ...)،
**لكن يجب أن يطلب موافقة المستخدم قبل كل أمر دون استثناء.**
لا تنفذ أي أمر Git تلقائياً. اعرض الأمر وسببه، وانتظر الموافقة.

**قواعد الفروع:**

- كل مهمة (MS-XXX) تُنفَّذ في فرع خاص: `feature/MS-XXX-<short-description>`.
- مثال: `feature/MS-006-project-references`.
- لا تعمل على `main` مباشرة بعد الآن.
- بعد اكتمال المهمة والتحقق من معايير القبول، اسأل المستخدم: هل يريد الدمج في `main` أم فتح Pull Request؟
- لا تدمج تلقائياً. انتظر قرار المستخدم.
- احذف الفرع بعد الدمج (إذا وافق المستخدم).

**قواعد الـ commits:**

- استخدم Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`, `refactor:`, `test:`).
- Commit واحد لكل وحدة عمل منطقية، وليس لكل ملف.
- لا ترفع إلى GitHub إلا بعد تأكيد المستخدم.
- عند Commit مهمة كاملة، اذكر رقمها في الرسالة: `feat: MS-008 — dependency injection setup`.

## Work Order

اتبع `docs/development-plan.md` من MS-001 إلى MS-050.
رقم المهمة الحالية = مصدر الحقيقة الوحيد لما يجب تنفيذه.

## References

- المعمارية: `docs/architecture-guide.md`
- قاعدة البيانات: `docs/database-design.md`
- المصادقة: `docs/adr/0001-custom-auth.md`
- الخطة: `docs/development-plan.md`

عند أي غموض، ارجع إلى هذه الوثائق قبل اتخاذ قرار.

## Definition of Done (each task)

- الكود في الطبقة الصحيحة.
- `dotnet build` ينجح.
- سيناريو نجاح + سيناريو فشل جُرِّبا.
- لا أسرار في الكود.
- تحديث التوثيق إذا تغير السلوك.

## Terminal Command Hygiene (Token Efficiency)

- عند كتابة أوامر PowerShell طويلة (Invoke-RestMethod، سيناريوهات اختبار)،
  اكتبها في ملف `.ps1` مؤقت ثم شغّله. لا تكتب الأوامر الطويلة مباشرة في المحادثة.
- لا تكرر نفس الأمر الاختباري مرتين. إذا فشل، أصلح السبب أولاً ثم أعد التنفيذ.
- عند الاختبار المتكرر (مثل 5 محاولات فاشلة)، استخدم for loop في سكريبت واحد.
- لا تقرأ ملفات الإنتاج أثناء الاختبار إلا إذا اكتشفت خطأً فعلياً.
- عند الانتهاء من الاختبار، احذف كل ملفات السكريبت المؤقتة ومجلد `scratch/`.
- لا تكرر polling للأوامر الطرفية. الأمر إما نجح أو فشل. تحقق مرة واحدة.

## Scratch Files Policy

- كل الملفات المؤقتة (scripts, hashes, test files) تُوضع في `C:\Temp\`.
- لا تُنشئ ملفات مؤقتة في جذر المستودع.
- احذف الملفات المؤقتة بعد كل استخدام.

## Never Read Session Logs

- لا تقرأ ملفات `.gemini/antigravity/brain/**/logs/transcript.jsonl`.
- لا تقرأ أي ملف من `.gemini/`.
- إذا احتجت قيمة سابقة، أعد توليدها أو اسأل المستخدم.

## Never Read Session Logs

- لا تقرأ ملفات .gemini/antigravity/brain/\*\*/logs/transcript.jsonl.
- لا تقرأ أي ملف من .gemini/antigravity/brain/\*_/tasks/_.log.
- لا تقرأ أي ملف من .gemini/ على الإطلاق.
- استخدم stdout من الأوامر مباشرة، أو اطبع النتائج في نهاية السكريبت.
- إذا احتجت قيمة سابقة، أعد توليدها أو اسأل المستخدم.

## Report Format (Token Efficiency)

بعد تنفيذ أي مهمة، اعرض تقريراً موجزاً فقط. لا تنسخ الكود الكامل.

**الصيغة المطلوبة:**

### Files Changed

- [path]: [1-2 سطر وصف]
- [path]: [1-2 سطر وصف]

### Key Code Snippets (only if non-obvious)

- [path]:[line]: [3-5 سطر توضيحي]

### Changes Summary

- [1-3 نقاط عن التغييرات الجوهرية]

### Build Status

- dotnet build: [نجح / فشل - N error]

### Deviations from Plan

- [إن وُجد]

**⛔ لا تنسخ:**

- الكود الكامل لأي ملف.
- الملفات التي لم تتغير.
- DTOs متشابهة (استخدم "مطابق لـ X مع الحقول: ...").
- Configurations كاملة (اكتب "X Configuration مضاف مع: [ملخص]").

## Report Format — No Code Dumps

عندما يطلب المستخدم ملفاً للمراجعة، لا تنسخ الكود.
اعرض فقط:
1. مسار الملف.
2. وصف في 1-2 سطر.
3. أرقام الأسطر المهمة (إن لزم).

⛔ لا تنسخ الكود الكامل.
⛔ لا تستخدم ```csharp.
⛔ لا تستخدم --- كفواصل.

مثال صحيح:
src/MaintenanceSystem.Infrastructure/Caching/RedisMaintenanceTypeCache.cs
- ينفذ IMaintenanceTypeCache باستخدام StackExchange.Redis.
- Cache Key: "maintenance-types:active:v1"، TTL: 10 دقائق.
- يستخدم System.Text.Json مع JsonSerializerDefaults.Web.
- 3 دوال: GetAsync, SetAsync, InvalidateAsync.

### البنية التحتية المحلية:

- LocalDB: `(localdb)\MSSQLLocalDB` → `MaintenanceSystemDb`.
- Redis: 127.0.0.1:6379 (Memurai Developer Edition).

## NuGet Security Audit

- بعد كل إضافة حزمة، شغّل `dotnet list package --vulnerable`.
- إذا وُجدت ثغرات، رقّي الحزمة فوراً.
- لا تقبل Warnings من نوع NU1902/NU1903.

## Learning Docs

- Tasks: docs/learning/TASKS.md
- Template: docs/learning/TEMPLATE.md
- Example: docs/learning/flows/01-Login.md

Workflow:
1. عند "نفذ Task #N":
2. اقرأ TASKS.md (للمهمة).
3. اقرأ TEMPLATE.md (للقالب).
4. ابحث في الأكواد.
5. اعرض خطة.
6. انتظر الموافقة.
7. نفّذ.
8. اعرض الملف فقط.

⛔ لا تنسخ كوداً كاملاً.
⛔ اعرض diff فقط.

