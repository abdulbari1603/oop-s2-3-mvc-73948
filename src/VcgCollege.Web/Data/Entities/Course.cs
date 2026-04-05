using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class Course
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public int BranchId { get; set; }

    [ValidateNever]
    public Branch Branch { get; set; } = null!;

    [Column(TypeName = "TEXT")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "TEXT")]
    public DateTime EndDate { get; set; }

    [ValidateNever]
    public ICollection<Module> Modules { get; set; } = new List<Module>();

    [ValidateNever]
    public ICollection<CourseEnrolment> Enrolments { get; set; } = new List<CourseEnrolment>();

    [ValidateNever]
    public ICollection<FacultyCourseAssignment> FacultyAssignments { get; set; } = new List<FacultyCourseAssignment>();

    [ValidateNever]
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    [ValidateNever]
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
