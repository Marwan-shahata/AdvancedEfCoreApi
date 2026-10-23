# AdvancedEFCoreAPI

ASP.NET Core Web API على .NET 10 مع EF Core وSQL Server. المشروع ينفذ Pagination، Projection، Soft Delete، Change Tracking، وOptimistic Concurrency.

## تشغيل المشروع

من مجلد المشروع:

```powershell
dotnet restore --ignore-failed-sources
dotnet ef database update
dotnet run
```

افتح `http://localhost:<port>/swagger`؛ الـport يظهر في terminal أو `Properties/launchSettings.json`.

## Connection String بأمان

الإعداد الافتراضي LocalDB للتطوير. لا تضع password في `appsettings.json`. لقاعدة أخرى:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=SERVER_NAME;Database=AdvancedEFCoreApiDb;Trusted_Connection=True;TrustServerCertificate=True"
```

يمكن بدل ذلك استخدام environment variable باسم `ConnectionStrings__DefaultConnection`.

## هيكل المشروع

- `Models`: Entities (`Product`, `Category`) تمثل الجداول.
- `Data/ApplicationDbContext.cs`: DbContext وFluent API للعلاقة، Index، Soft Delete، وRowVersion.
- `Data/DbInitializer.cs`: Seed data عند أول تشغيل ناجح.
- `DTOs`: العقود التي تتبادلها الـAPI بدل إرجاع Entity مباشرة.
- `Controllers/ProductsController.cs`: endpoints.
- `Migrations`: schema history المولّد من EF Core.

**Model** يمثل سجلًا، و**DbSet** يمثل جدولًا، و**DbContext** هو session EF Core. `Product.Id` هو Primary Key، و`Product.CategoryId` هو Foreign Key. الـMigration هي تغيير versioned يُطبّق في قاعدة البيانات. Fluent API هو إعداد database داخل `OnModelCreating`.

## Endpoints واختبارها

### GET مع Pagination

`GET /api/Products?pageNumber=1&pageSize=2`

- Expected: `200 OK` مع `items` تحتوي `categoryName` و`rowVersion` بصيغة Base64.
- الصفحة الثانية: `pageNumber=2&pageSize=2`.
- `pageNumber=0` أو `pageSize=101`: `400 Bad Request`.

الـquery يستخدم `AsNoTracking()` للقراءة بدون tracking و`Select` لجلب الأعمدة اللازمة فقط. `CategoryName = p.Category.Name` يتحول إلى join داخل SQL، فلا توجد N+1 queries أو استعلام منفصل لكل Product.

### GET منتج واحد

`GET /api/Products/1` يعيد `200` أو `404`. انسخ `rowVersion` للتحديث.

### PUT: Change Tracking + Concurrency

`PUT /api/Products/1`

```json
{
  "name": "Laptop Pro",
  "price": 47000.00,
  "categoryId": 1,
  "rowVersion": "ضع-هنا-قيمة-rowVersion-من-GET"
}
```

- Expected: `200 OK` وRowVersion جديدة.
- Category غير موجودة أو Base64 غير صالح: `400`.
- Product غير موجود أو Soft Deleted: `404`.
- لاختبار `409`: اقرأ المنتج واحفظ RowVersion، نفّذ PUT ناجحًا بها، ثم أعد PUT بنفس القيمة القديمة. الطلب الثاني يعيد `409 Conflict` ولا يستبدل آخر تعديل.

EF يتتبع الـentity التي قرأها ويكتب الخصائص المسموح بها فقط؛ لا يستخدم `Update()` على object جديد. تعيين `OriginalValue` لـRowVersion يجعل SQL يتحقق من نسخة السجل قبل الكتابة، فيمنع Lost Updates.

### DELETE: Soft Delete

`DELETE /api/Products/5`

- Expected: `204 No Content`.
- بعدها GET العادي لا يرجع المنتج لأن Global Query Filter يضيف تلقائيًا `IsDeleted = false`.
- إداريًا فقط يمكن رؤية المحذوفين بـ`IgnoreQueryFilters()`، ويجب عدم تقديمه في endpoint عام بلا Authorization.

Soft Delete يعدل `IsDeleted` بدل حذف row؛ Hard Delete يحذف السجل نهائيًا. تأكد من SQL Server أن row ما زال موجودًا و`IsDeleted = 1`.

## فحص SQL Server

بعد `dotnet ef database update` تحقق من:

- جدولَي `Products` و`Categories` والـForeign Key.
- index `IX_Products_Name` للبحث بالاسم.
- عمود `RowVersion` من نوع `rowversion` وتولّده SQL Server تلقائيًا.
- جدول `__EFMigrationsHistory` لتأكيد تطبيق Migration.

الـPrimary Key يعرّف السجل بشكل فريد؛ الـIndex على `Name` هدفه أداء البحث. لا نعمل Index لكل الأعمدة لأنه يستهلك مساحة ويبطئ عمليات INSERT/UPDATE/DELETE.

## حالة التحقق في بيئة التسليم

- تم تنفيذ `dotnet restore` و`dotnet build` بنجاح (0 errors).
- تم توليد Migration `InitialCreate` بنجاح وتحتوي relationship و`IX_Products_Name` وrowversion وdefault لـ`IsDeleted`.
- حاولنا `dotnet ef database update` فعليًا، لكنه فشل لأن LocalDB/SQL Server غير متاح في بيئة التنفيذ. لذلك لم يتم الادعاء أن endpoints أو migration اختُبرت runtime هنا. بعد توفير SQL Server، نفذ أوامر التشغيل أعلاه واستخدم checklist المذكورة.
