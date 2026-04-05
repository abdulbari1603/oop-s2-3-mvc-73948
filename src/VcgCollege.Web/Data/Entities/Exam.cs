using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class Exam
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    [ValidateNever]
    public Course Course { get; set; } = null!;

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Column(TypeName = "TEXT")]
    public DateTime Date { get; set; }

    public decimal MaxScore { get; set; }

    public bool ResultsReleased { get; set; }

    [ValidateNever]
    public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
}
