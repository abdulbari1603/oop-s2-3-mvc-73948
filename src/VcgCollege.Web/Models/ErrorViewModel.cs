namespace VcgCollege.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public bool ShowTechnicalDetails { get; set; }

    public string? TechnicalSummary { get; set; }
}
