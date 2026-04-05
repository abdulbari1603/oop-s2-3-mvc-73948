using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class ExamResult
{
    public int Id { get; set; }

    public int ExamId { get; set; }

    [ValidateNever]
    public Exam Exam { get; set; } = null!;

    public int StudentProfileId { get; set; }

    [ValidateNever]
    public StudentProfile Student { get; set; } = null!;

    public decimal Score { get; set; }

    [StringLength(16)]
    public string? Grade { get; set; }
}
