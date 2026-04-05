using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Data;
using VcgCollege.Web.Data.Entities;

namespace VcgCollege.Web.Services;

public static class AccessQueries
{
    public static Task<bool> FacultyTeachesCourseAsync(
        ApplicationDbContext db, int facultyProfileId, int courseId, CancellationToken ct = default) =>
        db.FacultyCourseAssignments.AsNoTracking()
            .AnyAsync(f => f.FacultyProfileId == facultyProfileId && f.CourseId == courseId, ct);

    public static Task<bool> StudentEnrolledInCourseAsync(
        ApplicationDbContext db, int studentProfileId, int courseId, CancellationToken ct = default) =>
        db.CourseEnrolments.AsNoTracking()
            .AnyAsync(e => e.StudentProfileId == studentProfileId && e.CourseId == courseId
                && e.Status == EnrolmentStatuses.Active, ct);

    public static Task<bool> FacultyHasStudentAsync(
        ApplicationDbContext db, int facultyProfileId, int studentProfileId, CancellationToken ct = default) =>
        db.FacultyCourseAssignments.AsNoTracking()
            .Where(f => f.FacultyProfileId == facultyProfileId)
            .Join(db.CourseEnrolments.AsNoTracking(),
                f => f.CourseId,
                e => e.CourseId,
                (f, e) => e)
            .AnyAsync(e => e.StudentProfileId == studentProfileId && e.Status == EnrolmentStatuses.Active, ct);
}
