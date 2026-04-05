using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class Branch
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    [ValidateNever]
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
