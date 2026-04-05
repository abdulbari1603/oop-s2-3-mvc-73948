using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class AssignmentResult
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }

    [ValidateNever]
    public Assignment Assignment { get; set; } = null!;

    public int StudentProfileId { get; set; }

    [ValidateNever]
    public StudentProfile Student { get; set; } = null!;

    public decimal Score { get; set; }

    [StringLength(2000)]
    public string? Feedback { get; set; }
}
