using VcgCollege.Web.Data.Entities;
using VcgCollege.Web.Services;

namespace VcgCollege.Tests;

public class EnrolmentRulesTests
{
    [Fact]
    public void CanCreateEnrolment_when_no_existing_row() =>
        Assert.True(EnrolmentRules.CanCreateEnrolment(null));

    [Fact]
    public void CanCreateEnrolment_false_when_active() =>
        Assert.False(EnrolmentRules.CanCreateEnrolment(EnrolmentStatuses.Active));

    [Fact]
    public void CanCreateEnrolment_true_when_withdrawn() =>
        Assert.True(EnrolmentRules.CanCreateEnrolment(EnrolmentStatuses.Withdrawn));

    [Fact]
    public void IsActive_identifies_active_status() =>
        Assert.True(EnrolmentRules.IsActive(EnrolmentStatuses.Active));
}
