using System.ComponentModel.DataAnnotations;

namespace VcgCollege.Web.Models;

public class StudentCreateViewModel
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

    public string? Address { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(32)]
    public string? StudentNumber { get; set; }
}

public class StudentEditViewModel
{
    public int Id { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    public string? Address { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(32)]
    public string? StudentNumber { get; set; }

    public string IdentityUserId { get; set; } = string.Empty;
}
