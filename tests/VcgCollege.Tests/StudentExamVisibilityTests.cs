using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Services;

namespace VcgCollege.Tests;

public class StudentExamVisibilityTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Visible_map_excludes_unreleased_exam_even_when_result_exists_in_database()
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
        var exam = new Exam
        {
            CourseId = 10,
            Title = "Final",
            Date = DateTime.UtcNow.Date,
            MaxScore = 100,
            ResultsReleased = false
        };
        db.Exams.Add(exam);
        await db.SaveChangesAsync();

        db.ExamResults.Add(new ExamResult { ExamId = exam.Id, StudentProfileId = 3, Score = 99, Grade = "A" });
        await db.SaveChangesAsync();

        var map = await StudentExamViewQueries.GetVisibleExamResultsMapAsync(db, 3);
        Assert.Empty(map);
    }

    [Fact]
    public async Task Visible_map_only_contains_current_student_exam_rows()
    {
        await using var db = CreateDb();
        db.Branches.Add(new Branch { Id = 1, Name = "B" });
        db.Courses.Add(new Course { Id = 10, Name = "C", BranchId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) });
        db.StudentProfiles.AddRange(
            new StudentProfile { Id = 3, IdentityUserId = "s1", Name = "A", Email = "a@test" },
            new StudentProfile { Id = 4, IdentityUserId = "s2", Name = "B", Email = "b@test" });
        db.CourseEnrolments.AddRange(
            new CourseEnrolment { StudentProfileId = 3, CourseId = 10, EnrolDate = DateTime.UtcNow.Date, Status = EnrolmentStatuses.Active },
            new CourseEnrolment { StudentProfileId = 4, CourseId = 10, EnrolDate = DateTime.UtcNow.Date, Status = EnrolmentStatuses.Active });
        var exam = new Exam
        {
            CourseId = 10,
            Title = "Midterm",
            Date = DateTime.UtcNow.Date,
            MaxScore = 100,
            ResultsReleased = true
        };
        db.Exams.Add(exam);
        await db.SaveChangesAsync();

        db.ExamResults.AddRange(
            new ExamResult { ExamId = exam.Id, StudentProfileId = 3, Score = 80, Grade = "B" },
            new ExamResult { ExamId = exam.Id, StudentProfileId = 4, Score = 50, Grade = "F" });
        await db.SaveChangesAsync();

        var mapForStudent3 = await StudentExamViewQueries.GetVisibleExamResultsMapAsync(db, 3);
        Assert.Single(mapForStudent3);
        Assert.Equal(80, mapForStudent3[exam.Id].Score);

        var mapForStudent4 = await StudentExamViewQueries.GetVisibleExamResultsMapAsync(db, 4);
        Assert.Single(mapForStudent4);
        Assert.Equal(50, mapForStudent4[exam.Id].Score);
    }
}
