using System.ComponentModel.DataAnnotations;

namespace Ams.Web.Server.Api.Models.ValidationViewer;

public sealed class ValidationViewerOptions
{
    [Required]
    public string BookRoot { get; set; } = string.Empty;

    public string? CrxTemplatePath { get; set; }

    [Required]
    public string CrxDirectoryName { get; set; } = "CRX";

    [Required]
    public string DefaultErrorType { get; set; } = "MR";

    public string? ReviewedStatusPath { get; set; }
}

