using Ams.Web.Server.Api.Models.ValidationViewer;

namespace Ams.Web.Server.Api.Services;

/// <summary>
/// Mutable runtime state for the validation viewer workspace. Defaults are loaded from options,
/// but can be overridden at runtime via the Workspace endpoint.
/// </summary>
public sealed class ValidationViewerWorkspaceState
{
    private readonly object _sync = new();

    public ValidationViewerWorkspaceState(ValidationViewerOptions defaults)
    {
        BookRoot = defaults.BookRoot;
        BookIndexPath = string.IsNullOrWhiteSpace(defaults.BookRoot)
            ? null
            : Path.Combine(defaults.BookRoot, "book-index.json");
        CrxTemplatePath = defaults.CrxTemplatePath;
        CrxDirectoryName = defaults.CrxDirectoryName;
        DefaultErrorType = defaults.DefaultErrorType;
        ReviewedStatusPath = defaults.ReviewedStatusPath;
    }

    public string? BookRoot { get; private set; }
    public string? BookIndexPath { get; private set; }
    public string? CrxTemplatePath { get; private set; }
    public string CrxDirectoryName { get; private set; }
    public string DefaultErrorType { get; private set; }
    public string? ReviewedStatusPath { get; private set; }

    public void Update(string? bookRoot = null, string? bookIndexPath = null, string? crxTemplatePath = null,
        string? crxDirectoryName = null, string? defaultErrorType = null)
    {
        lock (_sync)
        {
            if (!string.IsNullOrWhiteSpace(bookRoot))
            {
                BookRoot = bookRoot;
                if (string.IsNullOrWhiteSpace(bookIndexPath))
                {
                    BookIndexPath = Path.Combine(bookRoot, "book-index.json");
                }
            }

            if (!string.IsNullOrWhiteSpace(bookIndexPath))
            {
                BookIndexPath = bookIndexPath;
            }

            if (crxTemplatePath is not null)
            {
                CrxTemplatePath = crxTemplatePath;
            }

            if (!string.IsNullOrWhiteSpace(crxDirectoryName))
            {
                CrxDirectoryName = crxDirectoryName;
            }

            if (!string.IsNullOrWhiteSpace(defaultErrorType))
            {
                DefaultErrorType = defaultErrorType;
            }
        }
    }
}

