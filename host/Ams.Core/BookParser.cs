using System.Text;
using System.Text.RegularExpressions;
using Xceed.Words.NET;

namespace Ams.Core;

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

    private static readonly Regex _whitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex _paragraphBreakRegex = new(@"\r?\n\s*\r?\n", RegexOptions.Compiled);

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
                
                // Extract metadata from CoreProperties dictionary
                string? title = null;
                string? author = null;
                var metadata = new Dictionary<string, object>();
                
                if (document.CoreProperties != null)
                {
                    var props = document.CoreProperties;
                    
                    // Extract title and author
                    title = props.GetValueOrDefault("title");
                    author = props.GetValueOrDefault("creator");
                    
                    // Add other metadata
                    foreach (var kvp in props.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value) && kvp.Key != "title" && kvp.Key != "creator"))
                    {
                        metadata[kvp.Key] = kvp.Value;
                    }
                }

                // Extract text content from paragraphs
                var textBuilder = new StringBuilder();
                var paragraphs = document.Paragraphs;
                
                for (int i = 0; i < paragraphs.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var paragraph = paragraphs[i];
                    var paragraphText = paragraph.Text?.Trim();
                    
                    if (!string.IsNullOrWhiteSpace(paragraphText))
                    {
                        textBuilder.AppendLine(paragraphText);
                        
                        // Add extra line break between paragraphs (except for the last one)
                        if (i < paragraphs.Count - 1)
                        {
                            textBuilder.AppendLine();
                        }
                    }
                }

                var text = NormalizeText(textBuilder.ToString());
                
                return new BookParseResult(
                    Text: text,
                    Title: title,
                    Author: author,
                    Metadata: metadata.Count > 0 ? metadata : null
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
            var normalizedText = NormalizeText(text);
            
            // Try to extract title from first line if it looks like a title
            string? title = null;
            var lines = normalizedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                var firstLine = lines[0].Trim();
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

            return new BookParseResult(
                Text: normalizedText,
                Title: title,
                Author: null,
                Metadata: metadata
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
            
            // Extract title from markdown headers
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

            // Remove markdown formatting for plain text extraction
            var cleanText = RemoveMarkdownFormatting(text);
            var normalizedText = NormalizeText(cleanText);

            var metadata = new Dictionary<string, object>
            {
                ["fileSize"] = new FileInfo(filePath).Length,
                ["format"] = "Markdown"
            };

            return new BookParseResult(
                Text: normalizedText,
                Title: title,
                Author: null,
                Metadata: metadata
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
                    var text = string.Join("\n\n", document.Paragraphs.Select(p => p.Text).Where(t => !string.IsNullOrWhiteSpace(t)));
                    var normalizedText = NormalizeText(text);

                    var metadata = new Dictionary<string, object>
                    {
                        ["fileSize"] = new FileInfo(filePath).Length,
                        ["format"] = "RTF"
                    };

                    return new BookParseResult(
                        Text: normalizedText,
                        Title: null,
                        Author: null,
                        Metadata: metadata
                    );
                }
                catch
                {
                    // Fallback to plain text parsing for RTF
                    var text = File.ReadAllText(filePath, Encoding.UTF8);
                    // Basic RTF cleanup - remove RTF control codes
                    var cleanText = Regex.Replace(text, @"\\[a-z]+\d*\s?|\{|\}", "", RegexOptions.IgnoreCase);
                    var normalizedText = NormalizeText(cleanText);

                    var metadata = new Dictionary<string, object>
                    {
                        ["fileSize"] = new FileInfo(filePath).Length,
                        ["format"] = "RTF (fallback)"
                    };

                    return new BookParseResult(
                        Text: normalizedText,
                        Title: null,
                        Author: null,
                        Metadata: metadata
                    );
                }
            }, cancellationToken);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            throw new BookParseException($"Failed to parse RTF file '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Normalizes text by cleaning up whitespace and formatting while preserving paragraph structure.
    /// </summary>
    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // First, normalize line endings
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // Split into paragraphs (preserving paragraph breaks)
        var paragraphs = _paragraphBreakRegex.Split(text)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => _whitespaceRegex.Replace(p, " ").Trim())
            .Where(p => !string.IsNullOrEmpty(p));

        // Rejoin paragraphs with double line breaks
        return string.Join("\n\n", paragraphs);
    }

    /// <summary>
    /// Removes basic markdown formatting to extract plain text.
    /// This is a simplified implementation - for production use, consider a dedicated markdown parser.
    /// </summary>
    private static string RemoveMarkdownFormatting(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var text = markdown;

        // Remove headers
        text = Regex.Replace(text, @"^#{1,6}\s+", "", RegexOptions.Multiline);

        // Remove emphasis and strong
        text = Regex.Replace(text, @"\*\*(.*?)\*\*", "$1");
        text = Regex.Replace(text, @"\*(.*?)\*", "$1");
        text = Regex.Replace(text, @"__(.*?)__", "$1");
        text = Regex.Replace(text, @"_(.*?)_", "$1");

        // Remove links
        text = Regex.Replace(text, @"\[([^\]]*)\]\([^)]*\)", "$1");

        // Remove code blocks and inline code
        text = Regex.Replace(text, @"```[\s\S]*?```", "");
        text = Regex.Replace(text, @"`([^`]*)`", "$1");

        // Remove horizontal rules
        text = Regex.Replace(text, @"^[-*_]{3,}\s*$", "", RegexOptions.Multiline);

        // Remove list markers
        text = Regex.Replace(text, @"^[\s]*[-*+]\s+", "", RegexOptions.Multiline);
        text = Regex.Replace(text, @"^[\s]*\d+\.\s+", "", RegexOptions.Multiline);

        return text;
    }
}