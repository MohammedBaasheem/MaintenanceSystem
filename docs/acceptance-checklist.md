# Acceptance Checklist — MS-043

| # | السيناريو | المتوقع | الفعلي | النتيجة |
|---|-----------|---------|--------|---------|
| 1 | Login admin صحيح | 200 + Token | 200 + Token | Pass |
| 2 | Login admin خاطئ | 401 Unauthorized | 401 Unauthorized | Pass |
| 3 | 5 محاولات فاشلة → قفل 15 دقيقة | 423 Locked | 423 Locked | Pass |
| 4 | GET /api/maintenance-requests/my بدون Token | 401 Unauthorized | 401 Unauthorized | Pass |
| 5 | POST /api/users بـ Token Employee | 403 Forbidden | 403 Forbidden | Pass |
| 6 | Redis Fallback (إيقاف خدمة Memurai) | 200 OK من SQL | إيقاف الخدمة يحتاج صلاحيات مسؤول للنظام | Skip |

