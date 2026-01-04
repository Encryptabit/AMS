namespace Ams.Workstation.Server.Models;

/// <summary>
/// View model for displaying a sentence in the sentence list with timing and status information.
/// </summary>
public class SentenceViewModel
{
    /// <summary>
    /// Unique identifier for the sentence.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The text content of the sentence.
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// Start time of the sentence in seconds.
    /// </summary>
    public double StartTime { get; set; }

    /// <summary>
    /// End time of the sentence in seconds.
    /// </summary>
    public double EndTime { get; set; }

    /// <summary>
    /// Status of the sentence: "ok", "warning", or "error".
    /// </summary>
    public string Status { get; set; } = "ok";

    /// <summary>
    /// Word error rate for this sentence, if available.
    /// </summary>
    public double? Wer { get; set; }

    /// <summary>
    /// Whether this sentence has a diff from the expected text.
    /// </summary>
    public bool HasDiff { get; set; }

    /// <summary>
    /// HTML representation of the diff, if HasDiff is true.
    /// </summary>
    public string? DiffHtml { get; set; }

    /// <summary>
    /// Duration of the sentence in seconds.
    /// </summary>
    public double Duration => EndTime - StartTime;
}
