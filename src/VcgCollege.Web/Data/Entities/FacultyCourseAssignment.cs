using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class FacultyCourseAssignment
{
    public int Id { get; set; }

    public int FacultyProfileId { get; set; }

    [ValidateNever]
    public FacultyProfile Faculty { get; set; } = null!;

    public int CourseId { get; set; }

    [ValidateNever]
    public Course Course { get; set; } = null!;
}
