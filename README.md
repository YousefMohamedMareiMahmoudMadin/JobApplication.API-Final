# Job Application API — Complete Project

A job application management system built with ASP.NET Core (.NET 10) — developed through the EraaSoft × iCareer .NET Back-End Bootcamp.

Recruiters publish and manage jobs, candidates apply for positions, and recruiters manage each application's status through a defined state transition path (State Machine), complete with automated background notifications for recruiters using Hangfire.

## Technologies Used

| Technology | Usage |
| --- | --- |
| ASP.NET Core Web API (.NET 10) | Building the REST API |
| Entity Framework Core 10 | ORM and SQL Server database |
| MediatR 14.2 | Implementing the CQRS pattern (Commands / Queries / Handlers) |
| Hangfire 1.8.25 | Background jobs and delayed notifications |
| JWT Bearer Authentication | Authentication and securing endpoints |
| Swashbuckle (Swagger) | API documentation with XML comments |
| BCrypt-style PBKDF2 | Password hashing (PasswordHasher) |

## Project Structure (Clean Architecture)

```
JobApplication.API/
├── JobApplication.Domain/          # Entities and Enums (Innermost layer — no external dependencies)
│   └── Entities: Job, Candidate, User, JobCandidateApplication (+ State Machine)
├── JobApplication.Application/     # Business Logic
│   ├── Features/                   # CQRS: Commands + Queries + Handlers for each Use Case
│   ├── Interfaces/                 # IRepository<T>, IBackgroundJobScheduler, INotificationService ...
│   ├── Services/                   # AuthService
│   └── DTOs/
├── JobApplication.Infrastructure/  # Technical details
│   ├── Persistence/                # ApplicationDbContext + Migrations
│   ├── Repositories/               # Generic Repository<T> + User/Candidate Repos
│   ├── Authentication/             # TokenService (JWT) + PasswordHasher
│   └── Services/                   # HangfireBackgroundJobScheduler + EmailNotificationService
└── JobApplication.API/             # HTTP layer (Controllers + Program.cs)

```

**Dependency Inversion Principle (DIP):** API → Infrastructure → Application → Domain. The Application layer has no knowledge of EF Core SQL details or Hangfire — it uses interfaces exclusively.

## Getting Started Steps

1. Open the solution in Visual Studio 2022+ or VS Code (`.slnx`).
2. Update the connection strings in `JobApplication.API/appsettings.json`:
* `DefaultConnection` — Project database.
* `HangfireConnection` — Separate database for Hangfire (created automatically on first run).


3. Run migrations:
```
dotnet tool install --global dotnet-ef
dotnet ef database update --project JobApplication.Infrastructure --startup-project JobApplication.API

```


4. Run the project:
```
dotnet run --project JobApplication.API

```


5. Open Swagger at: `https://localhost:7237/swagger` — and the Hangfire Dashboard at: `https://localhost:7237/hangfire`

## Endpoints

| Method | Route | Description | Auth |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | Register a recruiter or candidate (automatically creates a Candidate profile) | — |
| POST | `/api/auth/login` | Log in and return a JWT Token | — |
| GET | `/api/jobs` | Get all jobs | JWT |
| GET | `/api/jobs/{id}` | Get a single job by id | JWT |
| POST | `/api/jobs` | Create a new job (for the current token owner) | JWT |
| PUT | `/api/jobs/{id}/close` | Close a job (owner only) | JWT |
| GET | `/api/JobCandidateApplications` | Get all applications | — |
| GET | `/api/JobCandidateApplications/{id}` | Get a single application by id | — |
| POST | `/api/JobCandidateApplications` | Apply for a job `{jobId, candidateId}` — triggers an immediate notification for the recruiter (Hangfire Enqueue) | — |
| PATCH | `/api/JobCandidateApplications/{id}/{status}` | Change application status (State Machine) — schedules a notification after two minutes (Hangfire Schedule) | — |
| DELETE | `/api/JobCandidateApplications/{id}?requesterId=` | Cancel an application (owner only and status must be Applied / UnderReview) | — |

## Business Rules

1. **State Machine:** Fixed status transition flow — `Applied → UnderReview → InterView → Accepted/Rejected`. Any invalid transition returns a 400 status code.
2. **Ownership:** Candidates can only cancel their own applications, and recruiters can only close their own jobs (returns 403 otherwise).
3. **Unique Application:** A candidate cannot apply for the same job twice.
4. **Active Jobs:** Applications are rejected if the job is closed.

## Background Jobs (Hangfire)

* **Enqueue (Immediate):** Upon applying for a job → sends an immediate notification to the recruiter regarding the new application.
* **Schedule (Delayed):** Upon changing an application status → schedules a notification to the recruiter after two minutes.
* Jobs are persisted in a separate SQL Server database — ensuring that if the server crashes, jobs are not lost and will resume when it comes back online.
* All tasks can be tracked via the Dashboard at `/hangfire` (Succeeded / Failed / Retries).
