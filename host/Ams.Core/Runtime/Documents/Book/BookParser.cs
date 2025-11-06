using System.Text;
using System.Text.RegularExpressions;
using Xceed.Words.NET;

namespace Ams.Core.Runtime.Documents;

/// <summary>
/// Book parser implementation using Xceed Document .NET for DOCX files
/// with fallback to plain text parsing for TXT and other text-based formats.
/// Supports PDF parsing as a future enhancement (marked as TODO).
/// </summary>
public class BookParser : IBookParser
{
    private static readonly HashSet<string> _supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".docx", ".txt", ".md", ".rtf"
    };

    private static readonly Regex _paragraphBreakRegex = new(@"(\r?\n){2,}", RegexOptions.Compiled);

    public IReadOnlyCollection<string> SupportedExtensions => _supportedExtensions;

    public bool CanParse(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            var extension = Path.GetExtension(filePath);
            return !string.IsNullOrEmpty(extension) && _supportedExtensions.Contains(extension);
        }
        catch
        {
            return false;
        }
    }

    public async Task<BookParseResult> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        if (!CanParse(filePath))
            throw new InvalidOperationException($"Unsupported file format: {Path.GetExtension(filePath)}");

        try
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            return extension switch
            {
                ".docx" => await ParseDocxAsync(filePath, cancellationToken),
                ".txt" => await ParseTextAsync(filePath, cancellationToken),
                ".md" => await ParseMarkdownAsync(filePath, cancellationToken),
                ".rtf" => await ParseRtfAsync(filePath, cancellationToken),
                _ => throw new InvalidOperationException($"Unsupported file format: {extension}")
            };
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is FileNotFoundException || ex is InvalidOperationException))
        {
            throw new BookParseException($"Failed to parse file '{filePath}': {ex.Message}", ex);
        }
    }

    private async Task<BookParseResult> ParseDocxAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            // Use Task.Run for CPU-bound operation to avoid blocking the calling thread
            return await Task.Run(() =>
            {
                using var document = DocX.Load(filePath);

                string? title = null;
                string? author = null;
                var metadata = new Dictionary<string, object>();

                if (document.CoreProperties != null)
                {
                    var props = document.CoreProperties;
                    title = props.GetValueOrDefault("title");
                    author = props.GetValueOrDefault("creator");

                    foreach (var kvp in props.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value) && kvp.Key != "title" && kvp.Key != "creator"))
                    {
                        metadata[kvp.Key] = kvp.Value;
                    }
                }

                var parsedParagraphs = new List<ParsedParagraph>();
                var sb = new StringBuilder();
                var paragraphs = document.Paragraphs;

                for (int i = 0; i < paragraphs.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var p = paragraphs[i];
                    var text = p.Text ?? string.Empty; // Do not trim/normalize
                    var style = p.StyleId ?? "Unknown";
                    var kind = style.Contains("Heading", StringComparison.OrdinalIgnoreCase) ? "Heading" : "Body";

                    if (!string.IsNullOrEmpty(text))
                    {
                        parsedParagraphs.Add(new ParsedParagraph(text, style, kind));
                        sb.Append(text);
                        if (i < paragraphs.Count - 1)
                            sb.AppendLine().AppendLine();
                    }
                }

                return new BookParseResult(
                    Text: sb.ToString(),
                    Title: title,
                    Author: author,
                    Metadata: metadata.Count > 0 ? metadata : null,
                    Paragraphs: parsedParagraphs
                );
                
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BookParseException($"Failed to parse DOCX file '{filePath}': {ex.Message}", ex);
        }
    }

    private async Task<BookParseResult> ParseTextAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var text = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
            
            // Try to extract title from first line if it looks like a title
            string? title = null;
            var lines = text.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                var firstLine = lines[0].TrimEnd('\r');
                // Heuristic: if first line is short and doesn't end with sentence punctuation, treat as title
                if (firstLine.Length <= 100 && !firstLine.EndsWith('.') && !firstLine.EndsWith('!') && !firstLine.EndsWith('?'))
                {
                    title = firstLine;
                }
            }

            var metadata = new Dictionary<string, object>
            {
                ["fileSize"] = new FileInfo(filePath).Length,
                ["encoding"] = "UTF-8"
            };

            // Build simple paragraphs split on blank lines (preserve original paragraph text)
            var paragraphs = _paragraphBreakRegex.Split(text)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            var parsedParagraphs = new List<ParsedParagraph>();
            foreach (var para in paragraphs)
            {
                var trimmed = para.TrimEnd('\r', '\n');
                if (string.IsNullOrEmpty(trimmed)) continue;
                parsedParagraphs.Add(new ParsedParagraph(trimmed, null, "Body"));
            }

            return new BookParseResult(
                Text: text,
                Title: title,
                Author: null,
                Metadata: metadata,
                Paragraphs: parsedParagraphs
            );
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            throw new BookParseException($"Failed to parse text file '{filePath}': {ex.Message}", ex);
        }
    }

    private static async Task<BookParseResult> ParseMarkdownAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var text = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);

            // Extract title from markdown headers (best-effort, do not modify text)
            string? title = null;
            var lines = text.Split('\n');
            foreach (var line in lines.Take(10)) // Check first 10 lines for title
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("# "))
                {
                    title = trimmed[2..].Trim();
                    break;
                }
            }

            var metadata = new Dictionary<string, object>
            {
                ["fileSize"] = new FileInfo(filePath).Length,
                ["format"] = "Markdown"
            };

            // Paragraphs split on blank lines
            var paragraphs = _paragraphBreakRegex.Split(text)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            var parsedParagraphs = new List<ParsedParagraph>();
            foreach (var para in paragraphs)
            {
                var trimmed = para.TrimEnd('\r', '\n');
                if (string.IsNullOrEmpty(trimmed)) continue;
                parsedParagraphs.Add(new ParsedParagraph(trimmed, null, "Body"));
            }

            return new BookParseResult(
                Text: text,
                Title: title,
                Author: null,
                Metadata: metadata,
                Paragraphs: parsedParagraphs
            );
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            throw new BookParseException($"Failed to parse markdown file '{filePath}': {ex.Message}", ex);
        }
    }

    private async Task<BookParseResult> ParseRtfAsync(string filePath, CancellationToken cancellationToken)
    {
        // For RTF files, we'll use Xceed's RTF parsing capabilities
        // This is a simplified implementation - in production you might want more robust RTF parsing
        try
        {
            return await Task.Run(() =>
            {
                // RTF files can be loaded as DocX in some cases with Xceed
                // For a more robust solution, you might need a dedicated RTF parser
                try
                {
                    using var document = DocX.Load(filePath);
                    var parsedParagraphs = new List<ParsedParagraph>();
                    var sb = new StringBuilder();
                    var paras = document.Paragraphs;

                    for (int i = 0; i < paras.Count; i++)
                    {
                        var p = paras[i];
                        var text = p.Text ?? string.Empty;
                        var style = p.StyleId ?? "Unknown";
                        var kind = style.Contains("Heading", StringComparison.OrdinalIgnoreCase) ? "Heading" : "Body";
                        if (!string.IsNullOrEmpty(text))
                        {
                            parsedParagraphs.Add(new ParsedParagraph(text, style, kind));
                            sb.Append(text);
                            if (i < paras.Count - 1) sb.AppendLine().AppendLine();
                        }
                    }

                    var metadata = new Dictionary<string, object>
                    {
                        ["fileSize"] = new FileInfo(filePath).Length,
                        ["format"] = "RTF"
                    };

                    return new BookParseResult(
                        Text: sb.ToString(),
                        Title: null,
                        Author: null,
                        Metadata: metadata,
                        Paragraphs: parsedParagraphs
                    );
                }
                catch
                {
                    // Fallback to plain text parsing for RTF
                    var text = File.ReadAllText(filePath, Encoding.UTF8);
                    // Basic RTF control code removal (best-effort decoding only)
                    var cleanText = Regex.Replace(text, @"\\[a-z]+\d*\s?|\{|\}", "", RegexOptions.IgnoreCase);

                    var metadata = new Dictionary<string, object>
                    {
                        ["fileSize"] = new FileInfo(filePath).Length,
                        ["format"] = "RTF (fallback)"
                    };

                    // Paragraphs split on blank lines
                    var parts = _paragraphBreakRegex.Split(cleanText).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                    var parsedParagraphs = parts
                        .Select(p => new ParsedParagraph(p.TrimEnd('\r', '\n'), null, "Body"))
                        .ToList();

                    return new BookParseResult(
                        Text: cleanText,
                        Title: null,
                        Author: null,
                        Metadata: metadata,
                        Paragraphs: parsedParagraphs
                    );
                }
            }, cancellationToken);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            throw new BookParseException($"Failed to parse RTF file '{filePath}': {ex.Message}", ex);
        }
    }
}

