using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class Module
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    [ValidateNever]
    public Course Course { get; set; } = null!;

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(40)]
    public string? Code { get; set; }

    public int Credits { get; set; }
}
