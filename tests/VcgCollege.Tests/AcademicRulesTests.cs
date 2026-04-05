using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Services;

namespace VcgCollege.Tests;

public class AcademicRulesTests
{
    [Theory]
    [InlineData(10, 100, true)]
    [InlineData(0, 50, true)]
    [InlineData(100, 100, true)]
    [InlineData(-1, 100, false)]
    [InlineData(101, 100, false)]
    public void IsAssignmentScoreValid_matches_bounds(decimal score, decimal max, bool expected) =>
        Assert.Equal(expected, AcademicRules.IsAssignmentScoreValid(score, max));

    [Fact]
    public void ValidateAssignmentScore_returns_null_when_valid() =>
        Assert.Null(AcademicRules.ValidateAssignmentScore(40, 50));

    [Fact]
    public void ValidateAssignmentScore_returns_message_when_over_max() =>
        Assert.Contains("maximum", AcademicRules.ValidateAssignmentScore(60, 50)!, StringComparison.OrdinalIgnoreCase);

    [Fact]
    public void CanStudentViewExamResult_requires_release_flag()
    {
        var released = new Exam { ResultsReleased = true };
        var hidden = new Exam { ResultsReleased = false };
        Assert.True(AcademicRules.CanStudentViewExamResult(released));
        Assert.False(AcademicRules.CanStudentViewExamResult(hidden));
    }

    [Theory]
    [InlineData(90, 100, "A")]
    [InlineData(70, 100, "C")]
    [InlineData(55, 100, "F")]
    public void ComputeLetterGrade_maps_percentages(decimal score, decimal max, string expected) =>
        Assert.Equal(expected, AcademicRules.ComputeLetterGrade(score, max));
}
