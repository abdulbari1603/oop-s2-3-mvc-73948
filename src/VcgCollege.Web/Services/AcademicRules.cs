using VcgCollege.Web.Data.Entities;

namespace VcgCollege.Web.Services;

/// <summary>

/// </summary>
public static class AcademicRules
{
    public static bool IsAssignmentScoreValid(decimal score, decimal maxScore) =>
        score >= 0 && score <= maxScore;

    public static bool CanStudentViewExamResult(Exam exam) => exam.ResultsReleased;

    public static string? ValidateAssignmentScore(decimal score, decimal maxScore)
    {
        if (score < 0) return "Score cannot be negative.";
        if (score > maxScore) return $"Score cannot exceed the maximum ({maxScore}).";
        return null;
    }

    public static string? ValidateExamScore(decimal score, decimal maxScore)
    {
        if (score < 0) return "Score cannot be negative.";
        if (score > maxScore) return $"Score cannot exceed the maximum ({maxScore}).";
        return null;
    }

    /// <summary>
  
    /// </summary>
    public static string ComputeLetterGrade(decimal score, decimal maxScore)
    {
        if (maxScore <= 0) return "N/A";
        var pct = (double)(score / maxScore * 100m);
        return pct switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F"
        };
    }
}
