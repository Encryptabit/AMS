using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Ams.Core;

namespace Ams.Core;

/// <summary>
/// Implementation of IBookIndexer that processes parsed book text into indexed structures
/// with word tokenization, sentence/paragraph segmentation, and timing estimation.
/// </summary>
public class BookIndexer : IBookIndexer
{
    // Regex patterns for text segmentation
    private static readonly Regex _sentenceEndRegex = new(
        @"[.!?]+(?=\s+[A-Z]|\s*$)", 
        RegexOptions.Compiled
    );
    
    private static readonly Regex _wordRegex = new(
        @"\b[\w']+\b", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );
    
    private static readonly Regex _paragraphSeparatorRegex = new(
        @"\n\s*\n", 
        RegexOptions.Compiled
    );
    
    private static readonly Regex _whitespaceRegex = new(
        @"\s+", 
        RegexOptions.Compiled
    );

    public async Task<BookIndex> CreateIndexAsync(
        BookParseResult parseResult, 
        string sourceFile, 
        BookIndexOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (parseResult == null)
            throw new ArgumentNullException(nameof(parseResult));
        
        if (string.IsNullOrWhiteSpace(sourceFile))
            throw new ArgumentException("Source file path cannot be null or empty.", nameof(sourceFile));
        
        if (string.IsNullOrWhiteSpace(parseResult.Text))
            throw new ArgumentException("Parse result text cannot be null or empty.", nameof(parseResult));

        options ??= new BookIndexOptions();

        try
        {
            return await Task.Run(() => ProcessBook(parseResult, sourceFile, options, cancellationToken), cancellationToken);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is ArgumentException))
        {
            throw new BookIndexException($"Failed to create book index for '{sourceFile}': {ex.Message}", ex);
        }
    }

    public async Task<BookIndex> UpdateTimingAsync(
        BookIndex bookIndex,
        AsrToken[] asrTokens,
        CancellationToken cancellationToken = default)
    {
        if (bookIndex == null)
            throw new ArgumentNullException(nameof(bookIndex));
        
        if (asrTokens == null)
            throw new ArgumentNullException(nameof(asrTokens));

        try
        {
            return await Task.Run(() => AlignTimingData(bookIndex, asrTokens, cancellationToken), cancellationToken);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is ArgumentException))
        {
            throw new BookIndexException($"Failed to update timing for book index: {ex.Message}", ex);
        }
    }

    private BookIndex ProcessBook(
        BookParseResult parseResult, 
        string sourceFile, 
        BookIndexOptions options,
        CancellationToken cancellationToken)
    {
        // Calculate file hash
        var sourceFileHash = ComputeFileHash(sourceFile);
        
        // Normalize text if requested
        var text = options.NormalizeText ? NormalizeText(parseResult.Text) : parseResult.Text;
        
        // Split into paragraphs
        var paragraphs = SplitIntoParagraphs(text, options);
        cancellationToken.ThrowIfCancellationRequested();
        
        // Process paragraphs into words and sentences
        var words = new List<BookWord>();
        var segments = new List<BookSegment>();
        
        int globalWordIndex = 0;
        int sentenceIndex = 0;
        
        for (int paragraphIndex = 0; paragraphIndex < paragraphs.Count; paragraphIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var paragraph = paragraphs[paragraphIndex];
            var sentences = SplitIntoSentences(paragraph, options);
            
            int paragraphWordStart = globalWordIndex;
            
            // Process sentences in this paragraph
            for (int sentenceInParagraph = 0; sentenceInParagraph < sentences.Count; sentenceInParagraph++)
            {
                var sentence = sentences[sentenceInParagraph];
                var sentenceWords = ExtractWords(sentence);
                
                if (sentenceWords.Count == 0) continue;
                
                int sentenceWordStart = globalWordIndex;
                
                // Add words for this sentence
                foreach (var wordText in sentenceWords)
                {
                    var word = new BookWord(
                        Text: wordText,
                        WordIndex: globalWordIndex,
                        SentenceIndex: sentenceIndex,
                        ParagraphIndex: paragraphIndex
                        // StartTime and EndTime will be null until ASR alignment
                    );
                    
                    words.Add(word);
                    globalWordIndex++;
                }
                
                int sentenceWordEnd = globalWordIndex - 1;
                
                // Add sentence segment
                var sentenceSegment = new BookSegment(
                    Text: sentence.Trim(),
                    Type: BookSegmentType.Sentence,
                    Index: sentenceIndex,
                    WordStartIndex: sentenceWordStart,
                    WordEndIndex: sentenceWordEnd
                    // StartTime and EndTime will be null until ASR alignment
                );
                
                segments.Add(sentenceSegment);
                sentenceIndex++;
            }
            
            // Add paragraph segment if requested and paragraph has enough words
            if (options.IncludeParagraphSegments)
            {
                int paragraphWordEnd = globalWordIndex - 1;
                int paragraphWordCount = paragraphWordEnd - paragraphWordStart + 1;
                
                if (paragraphWordCount >= options.MinimumParagraphWords)
                {
                    var paragraphSegment = new BookSegment(
                        Text: paragraph.Trim(),
                        Type: BookSegmentType.Paragraph,
                        Index: paragraphIndex,
                        WordStartIndex: paragraphWordStart,
                        WordEndIndex: paragraphWordEnd
                        // StartTime and EndTime will be null until ASR alignment
                    );
                    
                    segments.Add(paragraphSegment);
                }
            }
        }
        
        // Calculate statistics
        var totalWords = words.Count;
        var totalSentences = segments.Count(s => s.Type == BookSegmentType.Sentence);
        var totalParagraphs = paragraphs.Count;
        
        // Estimate duration based on average reading speed
        var estimatedDuration = totalWords / options.AverageWpm * 60.0; // Convert minutes to seconds
        
        return new BookIndex(
            SourceFile: sourceFile,
            SourceFileHash: sourceFileHash,
            IndexedAt: DateTime.UtcNow,
            Title: parseResult.Title,
            Author: parseResult.Author,
            TotalWords: totalWords,
            TotalSentences: totalSentences,
            TotalParagraphs: totalParagraphs,
            EstimatedDuration: estimatedDuration,
            Words: words.ToArray(),
            Segments: segments.ToArray()
        );
    }

    private BookIndex AlignTimingData(
        BookIndex bookIndex, 
        AsrToken[] asrTokens,
        CancellationToken cancellationToken)
    {
        if (asrTokens.Length == 0)
            return bookIndex; // No ASR data to align

        // Create a normalized mapping of ASR tokens to book words
        var alignment = AlignAsrToBookWords(bookIndex.Words, asrTokens, cancellationToken);
        
        // Update words with timing information
        var updatedWords = new List<BookWord>(bookIndex.Words.Length);
        
        foreach (var word in bookIndex.Words)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (alignment.TryGetValue(word.WordIndex, out var asrToken))
            {
                var updatedWord = word with
                {
                    StartTime = asrToken.StartTime,
                    EndTime = asrToken.StartTime + asrToken.Duration,
                    Confidence = 0.8 // Default confidence for ASR alignment
                };
                updatedWords.Add(updatedWord);
            }
            else
            {
                updatedWords.Add(word);
            }
        }
        
        // Update segments with timing information
        var updatedSegments = new List<BookSegment>(bookIndex.Segments.Length);
        
        foreach (var segment in bookIndex.Segments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var segmentWords = updatedWords
                .Where(w => w.WordIndex >= segment.WordStartIndex && w.WordIndex <= segment.WordEndIndex)
                .Where(w => w.StartTime.HasValue && w.EndTime.HasValue)
                .ToList();
            
            if (segmentWords.Count > 0)
            {
                var startTime = segmentWords.Min(w => w.StartTime!.Value);
                var endTime = segmentWords.Max(w => w.EndTime!.Value);
                
                // Calculate confidence as average of word confidences
                var confidenceValues = segmentWords.Where(w => w.Confidence.HasValue).ToList();
                var avgConfidence = confidenceValues.Count > 0 
                    ? confidenceValues.Average(w => w.Confidence!.Value)
                    : 0.0;
                
                var updatedSegment = segment with
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Confidence = avgConfidence > 0 ? avgConfidence : null
                };
                
                updatedSegments.Add(updatedSegment);
            }
            else
            {
                updatedSegments.Add(segment);
            }
        }
        
        return bookIndex with
        {
            Words = updatedWords.ToArray(),
            Segments = updatedSegments.ToArray()
        };
    }

    private Dictionary<int, AsrToken> AlignAsrToBookWords(
        BookWord[] bookWords,
        AsrToken[] asrTokens,
        CancellationToken cancellationToken)
    {
        var alignment = new Dictionary<int, AsrToken>();
        
        // Simple alignment algorithm: match normalized words
        // In production, you might want a more sophisticated alignment like DTW
        
        var asrIndex = 0;
        
        foreach (var bookWord in bookWords)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (asrIndex >= asrTokens.Length) break;
            
            var normalizedBookWord = NormalizeWordForAlignment(bookWord.Text);
            
            // Look for matching ASR token within a reasonable window
            for (int searchIndex = asrIndex; searchIndex < Math.Min(asrTokens.Length, asrIndex + 5); searchIndex++)
            {
                var normalizedAsrWord = NormalizeWordForAlignment(asrTokens[searchIndex].Word);
                
                if (string.Equals(normalizedBookWord, normalizedAsrWord, StringComparison.OrdinalIgnoreCase))
                {
                    alignment[bookWord.WordIndex] = asrTokens[searchIndex];
                    asrIndex = searchIndex + 1;
                    break;
                }
            }
        }
        
        return alignment;
    }

    private static List<string> SplitIntoParagraphs(string text, BookIndexOptions options)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();
        
        var paragraphs = _paragraphSeparatorRegex.Split(text)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();
        
        return paragraphs;
    }

    private static List<string> SplitIntoSentences(string paragraph, BookIndexOptions options)
    {
        if (string.IsNullOrWhiteSpace(paragraph))
            return new List<string>();
        
        var sentences = new List<string>();
        var matches = _sentenceEndRegex.Matches(paragraph);
        
        if (matches.Count == 0)
        {
            // No sentence breaks found, treat entire paragraph as one sentence
            if (paragraph.Length >= options.MinimumSentenceLength)
                sentences.Add(paragraph);
        }
        else
        {
            int lastEnd = 0;
            
            foreach (Match match in matches)
            {
                var sentenceEnd = match.Index + match.Length;
                var sentence = paragraph.Substring(lastEnd, sentenceEnd - lastEnd).Trim();
                
                if (sentence.Length >= options.MinimumSentenceLength)
                    sentences.Add(sentence);
                
                lastEnd = sentenceEnd;
            }
            
            // Add remaining text if any
            if (lastEnd < paragraph.Length)
            {
                var remainingSentence = paragraph.Substring(lastEnd).Trim();
                if (remainingSentence.Length >= options.MinimumSentenceLength)
                    sentences.Add(remainingSentence);
            }
        }
        
        return sentences;
    }

    private static List<string> ExtractWords(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return new List<string>();
        
        var matches = _wordRegex.Matches(sentence);
        return matches.Cast<Match>().Select(m => m.Value.ToLowerInvariant()).ToList();
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        
        // Normalize whitespace
        text = _whitespaceRegex.Replace(text, " ");
        
        // Trim and ensure proper paragraph separation
        var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p));
        
        return string.Join("\n\n", paragraphs);
    }

    private static string NormalizeWordForAlignment(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return string.Empty;
        
        // Remove punctuation and convert to lowercase for alignment
        return Regex.Replace(word.ToLowerInvariant(), @"[^\w]", "");
    }

    private static string ComputeFileHash(string filePath)
    {
        try
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = sha256.ComputeHash(stream);
            return Convert.ToHexString(hashBytes);
        }
        catch (Exception ex)
        {
            throw new BookIndexException($"Failed to compute hash for file '{filePath}': {ex.Message}", ex);
        }
    }
}