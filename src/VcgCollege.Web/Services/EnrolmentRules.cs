using VcgCollege.Web.Data.Entities;

namespace VcgCollege.Web.Services;

public static class EnrolmentRules
{
    /// <summary>
   
    /// </summary>
    public static bool CanCreateEnrolment(string? existingStatus)
    {
        if (string.IsNullOrEmpty(existingStatus)) return true;
        if (existingStatus == EnrolmentStatuses.Withdrawn) return true;
        return false;
    }

    public static bool IsActive(string status) => status == EnrolmentStatuses.Active;
}
