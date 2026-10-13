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

## Intentional Deviations from Docs
- **.NET 10** بدلاً من .NET 8 (قرار مقصود، VS 2026).
- **`Guid.CreateVersion7()` المدمج** بدلاً من مكتبة خارجية.

## Git — Ask Before Every Command
الوكيل مسموح له بتنفيذ أوامر Git (init, add, commit, push, status, ...)،
**لكن يجب أن يطلب موافقة المستخدم قبل كل أمر دون استثناء.**
لا تنفذ أي أمر Git تلقائياً. اعرض الأمر وسببه، وانتظر الموافقة.

**قواعد الـ commits:**
- استخدم Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`, `refactor:`, `test:`).
- Commit واحد لكل وحدة عمل منطقية، وليس لكل ملف.
- لا ترفع إلى GitHub إلا بعد تأكيد المستخدم.
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