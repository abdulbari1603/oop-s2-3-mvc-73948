using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VcgCollege.Web.Data.Entities;

namespace VcgCollege.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<FacultyProfile> FacultyProfiles => Set<FacultyProfile>();
    public DbSet<FacultyCourseAssignment> FacultyCourseAssignments => Set<FacultyCourseAssignment>();
    public DbSet<CourseEnrolment> CourseEnrolments => Set<CourseEnrolment>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentResult> AssignmentResults => Set<AssignmentResult>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<StudentProfile>()
            .HasIndex(s => s.IdentityUserId)
            .IsUnique();

        builder.Entity<FacultyProfile>()
            .HasIndex(f => f.IdentityUserId)
            .IsUnique();

        builder.Entity<FacultyCourseAssignment>()
            .HasIndex(x => new { x.FacultyProfileId, x.CourseId })
            .IsUnique();

        builder.Entity<CourseEnrolment>()
            .HasIndex(e => new { e.StudentProfileId, e.CourseId })
            .IsUnique();

        builder.Entity<AssignmentResult>()
            .HasIndex(r => new { r.AssignmentId, r.StudentProfileId })
            .IsUnique();

        builder.Entity<ExamResult>()
            .HasIndex(r => new { r.ExamId, r.StudentProfileId })
            .IsUnique();

        builder.Entity<AttendanceRecord>()
            .HasIndex(a => new { a.CourseEnrolmentId, a.WeekNumber })
            .IsUnique();
    }
}
