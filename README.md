# Job Application API — المشروع الكامل

نظام إدارة طلبات التوظيف مبني بـ ASP.NET Core (.NET 10) — من تطوير EraaSoft × iCareer .NET Back-End Bootcamp.

الـ Recruiter بينشر وظايف ويديرها، والـ Candidate بيتقدم على الوظايف، والـ recruiter بيدير حالة كل طلب عبر مسار انتقالات محدد (State Machine)، مع إشعارات خلفية تلقائية للـ recruiter باستخدام Hangfire.

## التقنيات المستخدمة

| التقنية | الاستخدام |
|---------|-----------|
| ASP.NET Core Web API (.NET 10) | بناء الـ REST API |
| Entity Framework Core 10 | الـ ORM وقاعدة البيانات SQL Server |
| MediatR 14.2 | تطبيق نمط CQRS (Commands / Queries / Handlers) |
| Hangfire 1.8.25 | الـ Background Jobs والإشعارات المؤجلة |
| JWT Bearer Authentication | المصادقة وتأمين الـ endpoints |
| Swashbuckle (Swagger) | توثيق الـ API مع XML Comments |
| BCrypt-style PBKDF2 | تشفير الـ passwords (PasswordHasher) |

## بنية المشروع (Clean Architecture)

```
JobApplication.API/
├── JobApplication.Domain/          # الـ Entities والـ Enums (الطبقة الأعمق — مفيهاش أي dependencies)
│   └── Entities: Job, Candidate, User, JobCandidateApplication (+ State Machine)
├── JobApplication.Application/     # الـ Business Logic
│   ├── Features/                   # CQRS: Commands + Queries + Handlers لكل Use Case
│   ├── Interfaces/                 # IRepository<T>, IBackgroundJobScheduler, INotificationService ...
│   ├── Services/                   # AuthService
│   └── DTOs/
├── JobApplication.Infrastructure/  # التفاصيل التقنية
│   ├── Persistence/                # ApplicationDbContext + Migrations
│   ├── Repositories/               # Generic Repository<T> + User/Candidate Repos
│   ├── Authentication/             # TokenService (JWT) + PasswordHasher
│   └── Services/                   # HangfireBackgroundJobScheduler + EmailNotificationService
└── JobApplication.API/             # طبقة الـ HTTP (Controllers + Program.cs)
```

**قاعدة الاعتماديات (DIP):** الـ API → Infrastructure → Application → Domain. الـ Application متعرفش على EF Core تفاصيل SQL ولا على Hangfire — بيستخدم Interfaces بس.

## خطوات التشغيل

1. افتح الـ Solution في Visual Studio 2022+ أو VS Code (`.slnx`).
2. عدّل الـ Connection Strings في `JobApplication.API/appsettings.json`:
   - `DefaultConnection` — قاعدة بيانات المشروع.
   - `HangfireConnection` — قاعدة بيانات منفصلة للـ Hangfire (بتتعمل تلقائياً أول تشغيل).
3. شغّل الـ Migrations:
   ```
   dotnet tool install --global dotnet-ef
   dotnet ef database update --project JobApplication.Infrastructure --startup-project JobApplication.API
   ```
4. شغّل المشروع:
   ```
   dotnet run --project JobApplication.API
   ```
5. افتح Swagger على: `https://localhost:7237/swagger` — والـ Hangfire Dashboard على: `https://localhost:7237/hangfire`

## الـ Endpoints

| Method | Route | الوصف | Auth |
|--------|-------|-------|------|
| POST | `/api/auth/register` | تسجيل recruiter أو candidate (مع إنشاء Candidate profile تلقائياً) | — |
| POST | `/api/auth/login` | تسجيل دخول و RETURN JWT Token | — |
| GET | `/api/jobs` | كل الوظايف | JWT |
| GET | `/api/jobs/{id}` | وظيفة واحدة بالـ id | JWT |
| POST | `/api/jobs` | إنشاء وظيفة جديدة (لصاحب الـ Token الحالي) | JWT |
| PUT | `/api/jobs/{id}/close` | إغلاق وظيفة (الـ owner فقط) | JWT |
| GET | `/api/JobCandidateApplications` | كل الطلبات | — |
| GET | `/api/JobCandidateApplications/{id}` | طلب واحد بالـ id | — |
| POST | `/api/JobCandidateApplications` | التقديم على وظيفة `{jobId, candidateId}` — بتشغّل إشعار فوري للـ recruiter (Hangfire Enqueue) | — |
| PATCH | `/api/JobCandidateApplications/{id}/{status}` | تغيير حالة الطلب (State Machine) — بتجدول إشعار بعد دقيقتين (Hangfire Schedule) | — |
| DELETE | `/api/JobCandidateApplications/{id}?requesterId=` | إلغاء الطلب (الـ owner فقط وحالات Applied / UnderReview) | — |

## الـ Business Rules

1. **State Machine:** مسار الحالة ثابت — `Applied → UnderReview → InterView → Accepted/Rejected` وأي نط للانتقالات دي بيرجع 400.
2. **Ownership:** الـ candidate بيقدر يلغي طلبه هو بس، والـ recruiter بيقدر يقفل وظيفته هو بس (403 لو حاول غير كده).
3. **Unique Application:** الـ candidate مبيقدرش يتقدم على نفس الوظيفة مرتين.
4. **Active Jobs:** التقديم مرفوض لو الوظيفة مقفولة.

## الـ Background Jobs (Hangfire)

- **Enqueue (فوري):** عند التقديم على وظيفة → إشعار للـ recruiter بالـ application الجديدة.
- **Schedule (مؤجل):** عند تغيير حالة الطلب → إشعار للـ recruiter بعد دقيقتين.
- الشغلانيات محفوظة في قاعدة بيانات SQL Server منفصلة — فلو الـ server وقع الشغلانيات مش بتضيع وبتتكمل لما يرجع.
- كل حاجة قابلة للمتابعة من الـ Dashboard على `/hangfire` (Succeeded / Failed / Retries).
