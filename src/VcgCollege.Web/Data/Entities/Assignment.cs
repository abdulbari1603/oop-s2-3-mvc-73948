using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VcgCollege.Web.Data.Entities;

public class Assignment
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    [ValidateNever]
    public Course Course { get; set; } = null!;

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public decimal MaxScore { get; set; }

    [Column(TypeName = "TEXT")]
    public DateTime DueDate { get; set; }

    [ValidateNever]
    public ICollection<AssignmentResult> Results { get; set; } = new List<AssignmentResult>();
}
