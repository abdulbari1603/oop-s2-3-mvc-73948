using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class StudentProfile
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

    [StringLength(500)]
    public string? Address { get; set; }

    [Column(TypeName = "TEXT")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(32)]
    public string? StudentNumber { get; set; }

    [ValidateNever]
    public ICollection<CourseEnrolment> Enrolments { get; set; } = new List<CourseEnrolment>();
}
