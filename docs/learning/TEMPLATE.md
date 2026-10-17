# Learning Docs Template

> قالب مرن. عدّله حسب المهمة — زد أو انقص.

---

## Structure (مرن)

### للملفات من نوع "Flow"

1. **العنوان** (Emoji + الاسم).
2. **الوصف المختصر** (1-2 سطر).
3. **HTTP Request Contract**.
4. **Sequence Diagram** (Mermaid).
5. **المحطات** (3-7).
6. **Decision Tree** (Mermaid).
7. **Exceptions Matrix**.
8. **Security Layers** (إن وُجد).
9. **Testing Guide**.
10. **نقاط ذهبية** (3-5).

### للملفات من نوع "Conceptual"

1. **العنوان**.
2. **الوصف**.
3. **المفاهيم** (Tables + Mermaid).
4. **أمثلة**.
5. **نقاط ذهبية**.

### للملفات من نوع "Setup Guide"

1. **العنوان**.
2. **الوصف**.
3. **Prerequisites** (Checklist).
4. **Steps** (numbered).
5. **Verification**.
6. **Troubleshooting**.

---

## Style Rules

### Language
- عربي + مصطلحات إنجليزية تقنية.

### Callouts
- [!NOTE], [!TIP], [!IMPORTANT], [!WARNING], [!DANGER].

### Emojis
- 1️⃣ 2️⃣ 3️⃣ للتسلسل.
- 🔨 📝 🔐 🛡️ 🌐 🔄 📤 للأقسام.

### Mermaid
- sequence للـ Flows.
- flowchart للـ Decision Trees.
- state للـ State Machines.

### File Links
- `file:///C:/Users/KC-PC/source/repos/MaintenanceSystem/...`

### Stations Rule
- محطة واحدة لكل طبقة.
- لا دمج.
- لا تقسيم.
- العدد يتبع التعقيد.

### Length
- Flow: 300-600 سطر.
- Conceptual: 200-400 سطر.
- Setup: 150-300 سطر.

---

## مثال مرجعي

انظر `flows/01-Login.md` كمرجع للـ Flows.

---

## Verification Checklist

قبل التسليم:

- [ ] العنوان + الوصف.
- [ ] المخططات (Mermaid).
- [ ] المحطات (حسب النوع).
- [ ] الاستثناءات.
- [ ] الاختبارات.
- [ ] نقاط ذهبية.
- [ ] Callouts + Emojis.
- [ ] روابط file://.
- [ ] الطول مناسب.

---

## Flexibility

**القالب مرن:**

- Flow معقد (3 sub-flows) → أقسام أكثر.
- Setup guide → لا Decision Tree.
- Conceptual → لا Exceptions Matrix.

**القاعدة:** احتفظ بالروح، عدّل حسب الحاجة.
