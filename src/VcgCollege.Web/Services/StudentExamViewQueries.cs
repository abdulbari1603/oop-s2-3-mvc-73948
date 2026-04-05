using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;

namespace VcgCollege.Web.Services;

public static class StudentExamViewQueries
{
    public static async Task<Dictionary<int, ExamResult>> GetVisibleExamResultsMapAsync(
        ApplicationDbContext db,
        int studentProfileId,
        CancellationToken cancellationToken = default)
    {
        var courseIds = await db.CourseEnrolments.AsNoTracking()
            .Where(e => e.StudentProfileId == studentProfileId && e.Status == EnrolmentStatuses.Active)
            .Select(e => e.CourseId)
            .ToListAsync(cancellationToken);

        var releasedExamIds = await db.Exams.AsNoTracking()
            .Where(e => courseIds.Contains(e.CourseId) && e.ResultsReleased)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        var results = await db.ExamResults.AsNoTracking()
            .Where(r => r.StudentProfileId == studentProfileId && releasedExamIds.Contains(r.ExamId))
            .ToListAsync(cancellationToken);

        return results.ToDictionary(r => r.ExamId);
    }
}
