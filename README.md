# VcgCollege — Student & Course Management

VcgCollege is an ASP.NET Core 8 MVC web app for multi-branch student registration, attendance, assignments (gradebook), and exams with ASP.NET Core Identity role-based access (Administrator / Faculty / Student) and Entity Framework Core with SQLite.

## Prerequisites

- .NET 8 SDK

## Run locally

From the repository root:

```bash
dotnet restore VcgCollege.sln
dotnet tool install --global dotnet-ef   
dotnet ef database update --project src/VcgCollege.Web --startup-project src/VcgCollege.Web
dotnet run --project src/VcgCollege.Web
```

The VcgCollege app listens on the HTTPS URL shown in the console (e.g. `https://localhost:7xxx`).

On first run, the database is created (via migrations) and seed data is applied (branches, courses, faculty assignment, enrolments, attendance, assignments, exams, and demo users).



## Run tests

```bash
dotnet test VcgCollege.sln --configuration Release
```


## Seeded demo accounts (VcgCollege)

| Role          | Email                         | Password      |
|---------------|-------------------------------|---------------|
| Administrator | admin@vcgcollege.local        | Password123!  |
| Faculty       | faculty@vcgcollege.local      | Password123!  |
| Student       | student1@vcgcollege.local     | Password123!  |
| Student       | student2@vcgcollege.local     | Password123!  |



## Module brief alignment 

- Stack: ASP.NET Core 8 MVC, EF Core + SQLite, Identity with RBAC `Administrator`, `Faculty`, `Student`, xUnit tests, GitHub Actions CI on every push and pull request.
- Administrator: branches, courses, modules, student/faculty accounts and roles, enrolments, attendance, assignments & exams; exam results release via `ResultsReleased` / Toggle release 
- Faculty: only assigned courses; attendance, assignment gradebook, exam marks
- Student: profile; assignment grades as entered; exam marks hidden until admin releases 
- Server-side security
- Database: schema from EF Core migrations in `src/VcgCollege.Web/Data/Migrations/` 

## Design decisions and assumptions

- Exam visibility: students only see scores/grades 
- Faculty see contact details only for students on courses they are assigned.
- Modules are optional per course; attendance is simplified as weekly rows per enrolment.
- No public self-registration by default; admins create student/faculty accounts .

## Repository layout

Open `VcgCollege.sln` at the repo root (solution folders: src, tests).








