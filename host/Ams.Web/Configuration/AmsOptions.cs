using System.ComponentModel.DataAnnotations;

namespace Ams.Web.Configuration;

public sealed class AmsOptions
{
    public const string SectionName = "Ams";

    [Required]
    public WorkspaceOptions Workspace { get; init; } = new();

    [Required]
    public CrxOptions Crx { get; init; } = new();

    public FfmpegOptions Ffmpeg { get; init; } = new();

    public ReviewedStatusOptions ReviewedStatus { get; init; } = new();
}

public sealed class WorkspaceOptions
{
    /// <summary>
    /// Initial workspace root path (book directory). Can be changed at runtime via the workspace API.
    /// </summary>
    public string? RootPath { get; init; }

    /// <summary>
    /// Optional initial BookIndex path to prime the workspace context.
    /// </summary>
    public string? BookIndexPath { get; init; }
}

public sealed class CrxOptions
{
    /// <summary>
    /// Initial CRX template path. Users can change this at runtime.
    /// </summary>
    public string? TemplatePath { get; init; }

    [Range(1, int.MaxValue)]
    public int FirstDataRow { get; init; } = 11;

    [Required]
    public string DefaultErrorType { get; init; } = "MR";
}

public sealed class FfmpegOptions
{
    /// <summary>
    /// Optional explicit ffmpeg executable path. When not set, the resolver searches ExtTools/ffmpeg/bin.
    /// </summary>
    public string? ExecutablePath { get; init; }
}

public sealed class ReviewedStatusOptions
{
    /// <summary>
    /// Optional custom file name under %APPDATA%/AMS. Defaults to reviewed-status.json.
    /// </summary>
    public string StoreFileName { get; init; } = "reviewed-status.json";
}
