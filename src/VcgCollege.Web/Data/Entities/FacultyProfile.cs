using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class FacultyProfile
{
    public int Id { get; set; }

    [Required]
    public string IdentityUserId { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(40)]
    public string? Phone { get; set; }

    [ValidateNever]
    public ICollection<FacultyCourseAssignment> CourseAssignments { get; set; } = new List<FacultyCourseAssignment>();
}
