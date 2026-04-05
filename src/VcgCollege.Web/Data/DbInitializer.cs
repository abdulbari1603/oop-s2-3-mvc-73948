using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Data.Entities;

namespace VcgCollege.Web.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var db = provider.GetRequiredService<ApplicationDbContext>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        foreach (var role in new[] { RoleNames.Administrator, RoleNames.Faculty, RoleNames.Student })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        async Task<(ApplicationUser User, IdentityResult Result)> EnsureUser(
            string email, string password, string displayName, string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
                return (existing, IdentityResult.Success);

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName
            };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
            return (user, result);
        }

        var (admin, adminRes) = await EnsureUser(
            "admin@vcgcollege.local", "Password123!", "System Admin", RoleNames.Administrator);
        var (faculty, facRes) = await EnsureUser(
            "faculty@vcgcollege.local", "Password123!", "Dr. Jane Faculty", RoleNames.Faculty);
        var (stu1, s1Res) = await EnsureUser(
            "student1@vcgcollege.local", "Password123!", "Alex Student", RoleNames.Student);
        var (stu2, s2Res) = await EnsureUser(
            "student2@vcgcollege.local", "Password123!", "Sam Student", RoleNames.Student);

        if (!adminRes.Succeeded || !facRes.Succeeded || !s1Res.Succeeded || !s2Res.Succeeded)
            return;

        if (await db.Branches.AnyAsync())
            return;

        var dublin = new Branch { Name = "VcgCollege Dublin", Address = "1 College Road, Dublin" };
        var cork = new Branch { Name = "VcgCollege Cork", Address = "2 Marina Walk, Cork" };
        var galway = new Branch { Name = "VcgCollege Galway", Address = "3 Quay Street, Galway" };
        db.Branches.AddRange(dublin, cork, galway);
        await db.SaveChangesAsync();

        var cs = new Course
        {
            Name = "BSc Computing — Year 1",
            BranchId = dublin.Id,
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2026, 6, 30)
        };
        var bus = new Course
        {
            Name = "Business Diploma",
            BranchId = cork.Id,
            StartDate = new DateTime(2025, 9, 1),
            EndDate = new DateTime(2026, 5, 31)
        };
        var web = new Course
        {
            Name = "Web Development Bootcamp",
            BranchId = galway.Id,
            StartDate = new DateTime(2025, 10, 1),
            EndDate = new DateTime(2026, 3, 31)
        };
        db.Courses.AddRange(cs, bus, web);
        await db.SaveChangesAsync();

        db.Modules.AddRange(
            new Module { CourseId = cs.Id, Title = "Programming Fundamentals", Code = "CS101", Credits = 10 },
            new Module { CourseId = cs.Id, Title = "Databases", Code = "CS102", Credits = 10 },
            new Module { CourseId = bus.Id, Title = "Marketing Basics", Code = "BU201", Credits = 5 });

        var fp = new FacultyProfile
        {
            IdentityUserId = faculty.Id,
            Name = "Dr. Jane Faculty",
            Email = faculty.Email!,
            Phone = "+353-1-555-0100"
        };
        db.FacultyProfiles.Add(fp);
        await db.SaveChangesAsync();

        db.FacultyCourseAssignments.AddRange(
            new FacultyCourseAssignment { FacultyProfileId = fp.Id, CourseId = cs.Id },
            new FacultyCourseAssignment { FacultyProfileId = fp.Id, CourseId = web.Id });

        var sp1 = new StudentProfile
        {
            IdentityUserId = stu1.Id,
            Name = "Alex Student",
            Email = stu1.Email!,
            Phone = "+353-86-111-2222",
            Address = "10 Main St, Dublin",
            DateOfBirth = new DateTime(2003, 4, 15),
            StudentNumber = "S2025-001"
        };
        var sp2 = new StudentProfile
        {
            IdentityUserId = stu2.Id,
            Name = "Sam Student",
            Email = stu2.Email!,
            Phone = "+353-87-333-4444",
            Address = "22 River Rd, Cork",
            DateOfBirth = new DateTime(2002, 11, 2),
            StudentNumber = "S2025-002"
        };
        db.StudentProfiles.AddRange(sp1, sp2);
        await db.SaveChangesAsync();

        var e1 = new CourseEnrolment
        {
            StudentProfileId = sp1.Id,
            CourseId = cs.Id,
            EnrolDate = DateTime.UtcNow.Date.AddDays(-30),
            Status = EnrolmentStatuses.Active
        };
        var e2 = new CourseEnrolment
        {
            StudentProfileId = sp1.Id,
            CourseId = web.Id,
            EnrolDate = DateTime.UtcNow.Date.AddDays(-20),
            Status = EnrolmentStatuses.Active
        };
        var e3 = new CourseEnrolment
        {
            StudentProfileId = sp2.Id,
            CourseId = bus.Id,
            EnrolDate = DateTime.UtcNow.Date.AddDays(-25),
            Status = EnrolmentStatuses.Active
        };
        db.CourseEnrolments.AddRange(e1, e2, e3);
        await db.SaveChangesAsync();

        db.AttendanceRecords.AddRange(
            new AttendanceRecord { CourseEnrolmentId = e1.Id, WeekNumber = 1, SessionDate = DateTime.UtcNow.Date.AddDays(-14), Present = true },
            new AttendanceRecord { CourseEnrolmentId = e1.Id, WeekNumber = 2, SessionDate = DateTime.UtcNow.Date.AddDays(-7), Present = true },
            new AttendanceRecord { CourseEnrolmentId = e1.Id, WeekNumber = 3, SessionDate = DateTime.UtcNow.Date, Present = false },
            new AttendanceRecord { CourseEnrolmentId = e2.Id, WeekNumber = 1, SessionDate = DateTime.UtcNow.Date.AddDays(-10), Present = true });

        var a1 = new Assignment
        {
            CourseId = cs.Id,
            Title = "MVC Lab 1",
            MaxScore = 100,
            DueDate = DateTime.UtcNow.Date.AddDays(7)
        };
        var a2 = new Assignment
        {
            CourseId = cs.Id,
            Title = "SQL Exercise",
            MaxScore = 50,
            DueDate = DateTime.UtcNow.Date.AddDays(14)
        };
        db.Assignments.AddRange(a1, a2);
        await db.SaveChangesAsync();

        db.AssignmentResults.AddRange(
            new AssignmentResult { AssignmentId = a1.Id, StudentProfileId = sp1.Id, Score = 88, Feedback = "Great work." },
            new AssignmentResult { AssignmentId = a2.Id, StudentProfileId = sp1.Id, Score = 45, Feedback = "Good effort." });

        var ex1 = new Exam
        {
            CourseId = cs.Id,
            Title = "Midterm Exam",
            Date = DateTime.UtcNow.Date.AddDays(-5),
            MaxScore = 100,
            ResultsReleased = true
        };
        var ex2 = new Exam
        {
            CourseId = cs.Id,
            Title = "Final Exam",
            Date = DateTime.UtcNow.Date.AddDays(30),
            MaxScore = 100,
            ResultsReleased = false
        };
        db.Exams.AddRange(ex1, ex2);
        await db.SaveChangesAsync();

        db.ExamResults.AddRange(
            new ExamResult { ExamId = ex1.Id, StudentProfileId = sp1.Id, Score = 72, Grade = "C" },
            new ExamResult { ExamId = ex2.Id, StudentProfileId = sp1.Id, Score = 91, Grade = "A" });

        await db.SaveChangesAsync();
    }
}
