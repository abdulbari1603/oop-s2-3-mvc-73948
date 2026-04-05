using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class AttendanceRecord
{
    public int Id { get; set; }

    public int CourseEnrolmentId { get; set; }

    [ValidateNever]
    public CourseEnrolment CourseEnrolment { get; set; } = null!;

    public int WeekNumber { get; set; }

    [Column(TypeName = "TEXT")]
    public DateTime SessionDate { get; set; }

    public bool Present { get; set; }
}
