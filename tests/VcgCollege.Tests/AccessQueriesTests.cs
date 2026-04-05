using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Services;

namespace VcgCollege.Tests;

public class AccessQueriesTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task FacultyTeachesCourseAsync_true_when_assigned()
    {
        await using var db = CreateDb();
        db.Branches.Add(new Branch { Id = 1, Name = "B" });
        db.Courses.Add(new Course { Id = 10, Name = "C", BranchId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) });
        db.FacultyProfiles.Add(new FacultyProfile { Id = 5, IdentityUserId = "f1", Name = "F", Email = "f@test" });
        db.FacultyCourseAssignments.Add(new FacultyCourseAssignment { FacultyProfileId = 5, CourseId = 10 });
        await db.SaveChangesAsync();

        Assert.True(await AccessQueries.FacultyTeachesCourseAsync(db, 5, 10));
        Assert.False(await AccessQueries.FacultyTeachesCourseAsync(db, 5, 99));
    }

    [Fact]
    public async Task StudentEnrolledInCourseAsync_true_when_active_enrolment()
    {
        await using var db = CreateDb();
        db.Branches.Add(new Branch { Id = 1, Name = "B" });
        db.Courses.Add(new Course { Id = 10, Name = "C", BranchId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) });
        db.StudentProfiles.Add(new StudentProfile { Id = 3, IdentityUserId = "s1", Name = "S", Email = "s@test" });
        db.CourseEnrolments.Add(new CourseEnrolment
        {
            StudentProfileId = 3,
            CourseId = 10,
            EnrolDate = DateTime.UtcNow.Date,
            Status = EnrolmentStatuses.Active
        });
        await db.SaveChangesAsync();

        Assert.True(await AccessQueries.StudentEnrolledInCourseAsync(db, 3, 10));
        Assert.False(await AccessQueries.StudentEnrolledInCourseAsync(db, 3, 99));
    }

    [Fact]
    public async Task FacultyHasStudentAsync_true_when_shared_course()
    {
        await using var db = CreateDb();
        db.Branches.Add(new Branch { Id = 1, Name = "B" });
        db.Courses.Add(new Course { Id = 10, Name = "C", BranchId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) });
        db.FacultyProfiles.Add(new FacultyProfile { Id = 5, IdentityUserId = "f1", Name = "F", Email = "f@test" });
        db.StudentProfiles.Add(new StudentProfile { Id = 3, IdentityUserId = "s1", Name = "S", Email = "s@test" });
        db.FacultyCourseAssignments.Add(new FacultyCourseAssignment { FacultyProfileId = 5, CourseId = 10 });
        db.CourseEnrolments.Add(new CourseEnrolment
        {
            StudentProfileId = 3,
            CourseId = 10,
            EnrolDate = DateTime.UtcNow.Date,
            Status = EnrolmentStatuses.Active
        });
        await db.SaveChangesAsync();

        Assert.True(await AccessQueries.FacultyHasStudentAsync(db, 5, 3));
        Assert.False(await AccessQueries.FacultyHasStudentAsync(db, 5, 99));
    }
}
