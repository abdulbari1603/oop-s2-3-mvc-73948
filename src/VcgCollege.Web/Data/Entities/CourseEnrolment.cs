using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class CourseEnrolment
{
    public int Id { get; set; }

    public int StudentProfileId { get; set; }

    [ValidateNever]
    public StudentProfile Student { get; set; } = null!;

    public int CourseId { get; set; }

    [ValidateNever]
    public Course Course { get; set; } = null!;

    public DateTime EnrolDate { get; set; }

    [StringLength(40)]
    public string Status { get; set; } = EnrolmentStatuses.Active;

    [ValidateNever]
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}

public static class EnrolmentStatuses
{
    public const string Active = "Active";
    public const string Completed = "Completed";
    public const string Withdrawn = "Withdrawn";
}
