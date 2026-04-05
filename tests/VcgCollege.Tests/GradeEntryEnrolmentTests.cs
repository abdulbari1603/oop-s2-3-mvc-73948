using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Services;

namespace VcgCollege.Tests;

public class GradeEntryEnrolmentTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task StudentEnrolledInCourseAsync_false_when_student_not_on_course()
    {
        await using var db = CreateDb();
        db.Branches.Add(new Branch { Id = 1, Name = "B" });
        db.Courses.Add(new Course { Id = 10, Name = "C", BranchId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) });
        db.StudentProfiles.Add(new StudentProfile { Id = 3, IdentityUserId = "s1", Name = "S", Email = "s@test" });
        await db.SaveChangesAsync();

        Assert.False(await AccessQueries.StudentEnrolledInCourseAsync(db, 3, 10));
    }

    [Fact]
    public async Task StudentEnrolledInCourseAsync_false_when_enrolment_withdrawn()
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
            Status = EnrolmentStatuses.Withdrawn
        });
        await db.SaveChangesAsync();

        Assert.False(await AccessQueries.StudentEnrolledInCourseAsync(db, 3, 10));
    }
}
