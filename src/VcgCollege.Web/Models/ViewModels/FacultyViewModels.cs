using System.ComponentModel.DataAnnotations;

namespace VcgCollege.Web.Models;

public class FacultyCreateViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }
}

public class FacultyEditViewModel
{
    public int Id { get; set; }
    public string IdentityUserId { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }
}

public class FacultyAssignCourseViewModel
{
    public int FacultyProfileId { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public int CourseId { get; set; }
}
