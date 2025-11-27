using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Xceed.Words.NET;
using PDFiumCore;

namespace Ams.Core.Runtime.Book;

/// <summary>
/// Book parser implementation using Xceed Document .NET for DOCX files
/// with fallback to plain text parsing for TXT and other text-based formats.
/// Supports PDF parsing as a future enhancement (marked as TODO).
/// </summary>
public class BookParser : IBookParser
{
    private static readonly HashSet<string> _supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".docx", ".txt", ".md", ".rtf", ".pdf"
    };

    private static readonly Regex _paragraphBreakRegex = new(@"(\r?\n){2,}", RegexOptions.Compiled);
    private static readonly Regex PdfSentenceSplitRegex = new(@"(?<=[\.\!\?…])\s+|\n\s*\n+", RegexOptions.Compiled);
    private static readonly Regex MetadataBreakRegex = new(@"(?<=\S)\n(?=[A-Z0-9#©])", RegexOptions.Compiled);

    private static readonly Regex LeadingPageMarkerRegex =
        new(@"^\s*(?:#?\d+\s*|[©]{0,1}\d{2,4}\s*)+", RegexOptions.Compiled);

    private static readonly object PdfInitLock = new();
    private static bool _pdfiumInitialized;

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
                ".pdf" => await ParsePdfAsync(filePath, cancellationToken),
                _ => throw new InvalidOperationException($"Unsupported file format: {extension}")
            };
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is FileNotFoundException ||
                                     ex is InvalidOperationException))
        {
            throw new BookParseException($"Failed to parse file '{filePath}': {ex.Message}", ex);
        }
    }

    private static void EnsurePdfiumInitialized()
    {
        if (_pdfiumInitialized)
        {
            return;
        }

        lock (PdfInitLock)
        {
            if (_pdfiumInitialized)
            {
                return;
            }

            fpdfview.FPDF_InitLibrary();
            _pdfiumInitialized = true;
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
                var bodyUri = document.PackagePart?.Uri;

                string? title = null;
                string? author = null;
                var metadata = new Dictionary<string, object>();
                var suppressList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                static void AddSuppress(HashSet<string> suppress, string? value)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return;
                    }

                    var trimmed = value.Trim();
                    if (ShouldIgnoreSuppressEntry(trimmed))
                    {
                        return;
                    }

                    suppress.Add(trimmed);
                }

                if (document.CoreProperties != null)
                {
                    var props = document.CoreProperties;
                    title = props.GetValueOrDefault("title");
                    author = props.GetValueOrDefault("creator");
                    AddSuppress(suppressList, title);
                    AddSuppress(suppressList, author);

                    foreach (var kvp in props.Where(kvp =>
                                 !string.IsNullOrWhiteSpace(kvp.Value) && kvp.Key != "title" && kvp.Key != "creator"))
                    {
                        var trimmed = kvp.Value!.Trim();
                        metadata[kvp.Key] = trimmed;
                        AddSuppress(suppressList, trimmed);
                    }
                }

                var parsedParagraphs = new List<ParsedParagraph>();
                var sb = new StringBuilder();
                var paragraphs = document.Paragraphs;

                for (int i = 0; i < paragraphs.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var p = paragraphs[i];

                    if (bodyUri != null && p.PackagePart?.Uri != null && p.PackagePart.Uri != bodyUri)
                    {
                        continue;
                    }

                    var text = p.Text ?? string.Empty; // Do not trim/normalize before filtering metadata
                    text = RemoveSuppressEdges(text, suppressList);
                    var style = p.StyleId ?? "Unknown";
                    var kind = style.Contains("Heading", StringComparison.OrdinalIgnoreCase) ? "Heading" : "Body";

                    if (!string.IsNullOrWhiteSpace(text))
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
            var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                var firstLine = lines[0].TrimEnd('\r');
                // Heuristic: if first line is short and doesn't end with sentence punctuation, treat as title
                if (firstLine.Length <= 100 && !firstLine.EndsWith('.') && !firstLine.EndsWith('!') &&
                    !firstLine.EndsWith('?'))
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

    private async Task<BookParseResult> ParsePdfAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                EnsurePdfiumInitialized();

                var document = fpdfview.FPDF_LoadDocument(filePath, null);
                if (document == null || document.__Instance == nint.Zero)
                {
                    var error = fpdfview.FPDF_GetLastError();
                    throw new BookParseException($"PDFium failed to load '{filePath}' (error {error}).");
                }

                try
                {
                    var metadata = new Dictionary<string, object>
                    {
                        ["pageCount"] = fpdfview.FPDF_GetPageCount(document)
                    };

                    var title = TryGetPdfMetaText(document, "Title");
                    var author = TryGetPdfMetaText(document, "Author");
                    var subject = TryGetPdfMetaText(document, "Subject");
                    var keywords = TryGetPdfMetaText(document, "Keywords");
                    var creator = TryGetPdfMetaText(document, "Creator");
                    var producer = TryGetPdfMetaText(document, "Producer");

                    var suppressList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    void AddMeta(string key, string? value)
                    {
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            return;
                        }

                        var trimmed = value.Trim();
                        metadata[key] = trimmed;
                        suppressList.Add(trimmed);
                    }

                    AddMeta("title", title);
                    AddMeta("author", author);
                    AddMeta("subject", subject);
                    AddMeta("keywords", keywords);
                    AddMeta("creator", creator);
                    AddMeta("producer", producer);

                    var parsedParagraphs = new List<ParsedParagraph>();
                    var sb = new StringBuilder();
                    var pageCount = (int)metadata["pageCount"];

                    for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var page = fpdfview.FPDF_LoadPage(document, pageIndex);
                        if (page == null || page.__Instance == nint.Zero)
                        {
                            continue;
                        }

                        try
                        {
                            var textPage = fpdf_text.FPDFTextLoadPage(page);
                            if (textPage == null || textPage.__Instance == nint.Zero)
                            {
                                continue;
                            }

                            try
                            {
                                var charCount = fpdf_text.FPDFTextCountChars(textPage);
                                if (charCount <= 0)
                                {
                                    continue;
                                }

                                var buffer = new ushort[charCount + 1];
                                var written = fpdf_text.FPDFTextGetText(textPage, 0, charCount, ref buffer[0]);
                                if (written <= 1)
                                {
                                    continue;
                                }

                                var actualCount = written - 1;
                                var chars = new char[actualCount];
                                for (int i = 0; i < actualCount; i++)
                                {
                                    chars[i] = (char)buffer[i];
                                }

                                var text = SanitizePdfText(new string(chars));
                                if (string.IsNullOrWhiteSpace(text))
                                {
                                    continue;
                                }

                                foreach (var sentence in SplitPdfSentences(text))
                                {
                                    if (string.IsNullOrWhiteSpace(sentence))
                                    {
                                        continue;
                                    }

                                    var working = sentence.Trim();
                                    if (string.IsNullOrEmpty(working))
                                    {
                                        continue;
                                    }

                                    working = StripLeadingPageMarkers(working);
                                    if (string.IsNullOrEmpty(working))
                                    {
                                        continue;
                                    }

                                    var cleanedSentence = RemoveSuppressEdges(working, suppressList);
                                    if (string.IsNullOrWhiteSpace(cleanedSentence))
                                    {
                                        continue;
                                    }

                                    var flattened = Regex.Replace(cleanedSentence.ReplaceLineEndings(" "), "\\s+", " ")
                                        .Trim();
                                    if (string.IsNullOrWhiteSpace(flattened))
                                    {
                                        continue;
                                    }

                                    parsedParagraphs.Add(new ParsedParagraph(flattened, null, "Body"));
                                    sb.AppendLine(flattened);
                                    sb.AppendLine();
                                }
                            }
                            finally
                            {
                                fpdf_text.FPDFTextClosePage(textPage);
                            }
                        }
                        finally
                        {
                            fpdfview.FPDF_ClosePage(page);
                        }
                    }

                    return new BookParseResult(
                        Text: sb.ToString(),
                        Title: metadata.TryGetValue("title", out var titleObj) ? titleObj?.ToString() : null,
                        Author: metadata.TryGetValue("author", out var authorObj) ? authorObj?.ToString() : null,
                        Metadata: metadata.Count > 0 ? metadata : null,
                        Paragraphs: parsedParagraphs
                    );
                }
                finally
                {
                    fpdfview.FPDF_CloseDocument(document);
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BookParseException($"Failed to parse PDF file '{filePath}': {ex.Message}", ex);
        }
    }

    private static string? TryGetPdfMetaText(FpdfDocumentT document, string tag)
    {
        try
        {
            var length = (int)fpdf_doc.FPDF_GetMetaText(document, tag, nint.Zero, 0);
            if (length <= 2)
            {
                return null;
            }

            var buffer = Marshal.AllocHGlobal(length);
            try
            {
                var written = (int)fpdf_doc.FPDF_GetMetaText(document, tag, buffer, (uint)length);
                if (written <= 2)
                {
                    return null;
                }

                var bytes = new byte[written];
                Marshal.Copy(buffer, bytes, 0, written);
                var value = Encoding.Unicode.GetString(bytes, 0, written - 2);
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        catch
        {
            return null;
        }
    }

    private static string SanitizePdfText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        Span<char> pair = stackalloc char[2];
        var builder = new StringBuilder(text.Length);

        for (int i = 0; i < text.Length;)
        {
            var c = text[i];

            if (c == '\uFFFE' || c == '\uFFFF')
            {
                i++;
                continue;
            }

            if (char.IsSurrogate(c))
            {
                if (i + 1 < text.Length && char.IsSurrogatePair(c, text[i + 1]))
                {
                    pair[0] = c;
                    pair[1] = text[i + 1];
                    builder.Append(pair);
                    i += 2;
                    continue;
                }

                i++;
                continue;
            }

            if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
            {
                i++;
                continue;
            }

            builder.Append(c);
            i++;
        }

        return builder.ToString();
    }

    private static string StripLeadingPageMarkers(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var stripped = LeadingPageMarkerRegex.Replace(text, string.Empty, 1);
        return stripped.TrimStart();
    }

    private static string RemoveSuppressEdges(string sentence, HashSet<string> suppressList)
    {
        if (string.IsNullOrWhiteSpace(sentence) || suppressList.Count == 0)
        {
            return sentence;
        }

        var working = sentence.Trim();

        bool changed;
        do
        {
            changed = false;

            foreach (var entry in suppressList)
            {
                if (string.IsNullOrWhiteSpace(entry))
                {
                    continue;
                }

                if (working.StartsWith(entry, StringComparison.OrdinalIgnoreCase))
                {
                    working = working.Substring(entry.Length).TrimStart();
                    changed = true;
                }

                if (working.Length == 0)
                {
                    break;
                }

                if (working.EndsWith(entry, StringComparison.OrdinalIgnoreCase))
                {
                    working = working.Substring(0, working.Length - entry.Length).TrimEnd();
                    changed = true;
                }

                if (working.Length == 0)
                {
                    break;
                }

                if (entry.Contains(working, StringComparison.OrdinalIgnoreCase))
                {
                    working = string.Empty;
                    changed = true;
                }
            }
        } while (changed && working.Length > 0);


        var normalized = working.Trim();
        if (normalized.Length > 0)
        {
            var lower = normalized.ToLowerInvariant();
            if (suppressList.Contains(lower))
            {
                return string.Empty;
            }
        }

        return working;
    }

    private static bool ShouldIgnoreSuppressEntry(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        // Skip purely numeric or date-like metadata (e.g., revision numbers)
        bool hasLetters = false;
        foreach (var ch in value)
        {
            if (char.IsLetter(ch))
            {
                hasLetters = true;
                break;
            }
        }

        if (hasLetters)
        {
            return false;
        }

        // Treat entries that are digits plus common separators as numeric noise
        foreach (var ch in value)
        {
            if (!(char.IsDigit(ch) || char.IsWhiteSpace(ch) || ch is ':' or '-' or '/' or '.'))
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<string> SplitPdfSentences(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        var normalized = TextNormalizer.NormalizeTypography(text.Replace("\r\n", "\n").Replace('\r', '\n').Trim());
        if (normalized.Length == 0)
        {
            yield break;
        }

        var segments = PdfSentenceSplitRegex.Split(normalized);
        if (segments.Length <= 1)
        {
            foreach (var fragment in SplitOnMetadataBreaks(normalized))
            {
                if (fragment.Length > 0)
                {
                    yield return fragment;
                }
            }

            yield break;
        }

        foreach (var segment in segments)
        {
            foreach (var fragment in SplitOnMetadataBreaks(segment))
            {
                if (fragment.Length > 0)
                {
                    yield return fragment;
                }
            }
        }
    }

    private static IEnumerable<string> SplitOnMetadataBreaks(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        var fragments = MetadataBreakRegex.Split(text);
        foreach (var fragment in fragments)
        {
            var candidate = fragment.Trim();
            if (candidate.Length > 0)
            {
                yield return candidate;
            }
        }
    }
}
