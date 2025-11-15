using System.Text.Json;

namespace Ams.Core.Validation;

public class ScriptValidator(ValidationOptions? options = null)
{
    private readonly ValidationOptions _options = options ?? new ValidationOptions();

    public async Task<ValidationReport> ValidateAsync(string audioPath, string scriptPath, string asrJsonPath)
    {
        if (!File.Exists(audioPath))
            throw new FileNotFoundException($"Audio file not found: {audioPath}");
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Script file not found: {scriptPath}");
        if (!File.Exists(asrJsonPath))
            throw new FileNotFoundException($"ASR JSON file not found: {asrJsonPath}");

        var scriptText = await File.ReadAllTextAsync(scriptPath);
        var asrJsonText = await File.ReadAllTextAsync(asrJsonPath);
        
        var asrResponse = JsonSerializer.Deserialize<AsrResponse>(asrJsonText);
        if (asrResponse == null)
            throw new InvalidOperationException("Failed to deserialize ASR response");

        return Validate(audioPath, scriptPath, asrJsonPath, scriptText, asrResponse);
    }

    public ValidationReport Validate(string audioPath, string scriptPath, string asrJsonPath, string scriptText, AsrResponse asrResponse)
    {
        var expectedWords = ExtractWordsFromScript(scriptText);
        var actualWords = ExtractWordsFromAsrResponse(asrResponse);
        
        var alignment = AlignWords(expectedWords, actualWords);
        var findings = GenerateFindings(alignment, asrResponse);
        var segmentStats = GenerateSegmentStats(asrResponse, scriptText);
        
        var (correctWords, substitutions, insertions, deletions) = CalculateWordErrorStats(alignment);
        var wordErrorRate = CalculateWordErrorRate(correctWords, substitutions, insertions, deletions, expectedWords.Count);
        var characterErrorRate = CalculateCharacterErrorRate(scriptText, GetTranscriptText(asrResponse));

        return new ValidationReport(
            AudioFile: audioPath,
            ScriptFile: scriptPath,
            AsrFile: asrJsonPath,
            Timestamp: DateTime.UtcNow,
            WordErrorRate: wordErrorRate,
            CharacterErrorRate: characterErrorRate,
            TotalWords: expectedWords.Count,
            CorrectWords: correctWords,
            Substitutions: substitutions,
            Insertions: insertions,
            Deletions: deletions,
            Findings: findings.ToArray(),
            SegmentStats: segmentStats.ToArray()
        );
    }

    private List<string> ExtractWordsFromScript(string scriptText)
    {
        var normalized = TextNormalizer.Normalize(scriptText, _options.ExpandContractions, _options.RemoveNumbers);
        var words = TextNormalizer.TokenizeWords(normalized);
        return words.ToList();
    }

    private List<WordAlignment> ExtractWordsFromAsrResponse(AsrResponse asrResponse)
    {
        var words = new List<WordAlignment>();
        
        for (int i = 0; i < asrResponse.WordCount; i++)
        {
            var word = asrResponse.GetWord(i) ?? string.Empty;
            var normalizedWord = TextNormalizer.Normalize(word, _options.ExpandContractions, _options.RemoveNumbers);
            if (!string.IsNullOrEmpty(normalizedWord))
            {
                double start = 0;
                double end = 0;
                if (asrResponse.HasWordTimings && i < asrResponse.Tokens.Length)
                {
                    var token = asrResponse.Tokens[i];
                    start = token.StartTime;
                    end = token.StartTime + token.Duration;
                }

                words.Add(new WordAlignment
                {
                    Word = normalizedWord,
                    StartTime = start,
                    EndTime = end,
                    OriginalWord = word
                });
            }
        }
        
        return words;
    }

    private List<AlignmentResult> AlignWords(List<string> expected, List<WordAlignment> actual)
    {
        // Dynamic programming alignment with custom costs
        var dp = new double[expected.Count + 1, actual.Count + 1];
        var operations = new AlignmentOperation[expected.Count + 1, actual.Count + 1];

        // Initialize base cases
        for (int i = 0; i <= expected.Count; i++)
        {
            dp[i, 0] = i * _options.DeletionCost;
            operations[i, 0] = AlignmentOperation.Delete;
        }
        
        for (int j = 0; j <= actual.Count; j++)
        {
            dp[0, j] = j * _options.InsertionCost;
            operations[0, j] = AlignmentOperation.Insert;
        }

        // Fill DP table
        for (int i = 1; i <= expected.Count; i++)
        {
            for (int j = 1; j <= actual.Count; j++)
            {
                var expectedWord = expected[i - 1];
                var actualWord = actual[j - 1].Word;
                
                var matchCost = CalculateMatchCost(expectedWord, actualWord);
                var substitutionCost = dp[i - 1, j - 1] + matchCost;
                var deletionCost = dp[i - 1, j] + _options.DeletionCost;
                var insertionCost = dp[i, j - 1] + _options.InsertionCost;

                if (substitutionCost <= deletionCost && substitutionCost <= insertionCost)
                {
                    dp[i, j] = substitutionCost;
                    operations[i, j] = matchCost == 0 ? AlignmentOperation.Match : AlignmentOperation.Substitute;
                }
                else if (deletionCost <= insertionCost)
                {
                    dp[i, j] = deletionCost;
                    operations[i, j] = AlignmentOperation.Delete;
                }
                else
                {
                    dp[i, j] = insertionCost;
                    operations[i, j] = AlignmentOperation.Insert;
                }
            }
        }

        // Backtrack to build alignment
        var alignment = new List<AlignmentResult>();
        int ei = expected.Count, ai = actual.Count;

        while (ei > 0 || ai > 0)
        {
            var op = operations[ei, ai];
            
            switch (op)
            {
                case AlignmentOperation.Match:
                case AlignmentOperation.Substitute:
                    alignment.Add(new AlignmentResult
                    {
                        Operation = op,
                        ExpectedWord = expected[ei - 1],
                        ActualWord = actual[ai - 1],
                        Cost = dp[ei, ai] - dp[ei - 1, ai - 1]
                    });
                    ei--; ai--;
                    break;
                    
                case AlignmentOperation.Delete:
                    alignment.Add(new AlignmentResult
                    {
                        Operation = op,
                        ExpectedWord = expected[ei - 1],
                        ActualWord = null,
                        Cost = _options.DeletionCost
                    });
                    ei--;
                    break;
                    
                case AlignmentOperation.Insert:
                    alignment.Add(new AlignmentResult
                    {
                        Operation = op,
                        ExpectedWord = null,
                        ActualWord = actual[ai - 1],
                        Cost = _options.InsertionCost
                    });
                    ai--;
                    break;
            }
        }

        alignment.Reverse();
        return alignment;
    }

    private double CalculateMatchCost(string expected, string actual)
    {
        if (expected == actual) return 0.0;
        
        var similarity = TextNormalizer.CalculateSimilarity(expected, actual);
        return _options.SubstitutionCost * (1.0 - similarity);
    }

    private List<ValidationFinding> GenerateFindings(List<AlignmentResult> alignment, AsrResponse asrResponse)
    {
        var findings = new List<ValidationFinding>();
        
        foreach (var result in alignment)
        {
            switch (result.Operation)
            {
                case AlignmentOperation.Delete:
                    findings.Add(new ValidationFinding(
                        FindingType.Missing,
                        ValidationLevel.Word,
                        Expected: result.ExpectedWord,
                        Cost: result.Cost
                    ));
                    break;
                    
                case AlignmentOperation.Insert:
                    findings.Add(new ValidationFinding(
                        FindingType.Extra,
                        ValidationLevel.Word,
                        Actual: result.ActualWord?.Word,
                        StartTime: result.ActualWord?.StartTime,
                        EndTime: result.ActualWord?.EndTime,
                        Cost: result.Cost
                    ));
                    break;
                    
                case AlignmentOperation.Substitute:
                    findings.Add(new ValidationFinding(
                        FindingType.Substitution,
                        ValidationLevel.Word,
                        Expected: result.ExpectedWord,
                        Actual: result.ActualWord?.Word,
                        StartTime: result.ActualWord?.StartTime,
                        EndTime: result.ActualWord?.EndTime,
                        Cost: result.Cost
                    ));
                    break;
                case AlignmentOperation.Match:
                    break;
                default:
                    var exception = new ArgumentOutOfRangeException();
                    exception.HelpLink = null;
                    exception.HResult = 0;
                    exception.Source = null;
                    throw exception;
            }
        }
        
        return findings;
    }

    private List<SegmentStats> GenerateSegmentStats(AsrResponse asrResponse, string scriptText)
    {
        var stats = new List<SegmentStats>();
        
        if (!asrResponse.HasWords)
            return stats;
        
        double startTime = 0;
        double endTime = 0;
        if (asrResponse.HasWordTimings && asrResponse.Tokens.Length > 0)
        {
            var firstToken = asrResponse.Tokens.First();
            var lastToken = asrResponse.Tokens.Last();
            startTime = firstToken.StartTime;
            endTime = lastToken.StartTime + lastToken.Duration;
        }
        else if (asrResponse.Segments.Length > 0)
        {
            var firstSegment = asrResponse.Segments.First();
            var lastSegment = asrResponse.Segments.Last();
            startTime = firstSegment.StartSec;
            endTime = lastSegment.EndSec;
        }
        
        var expectedText = TextNormalizer.Normalize(scriptText, _options.ExpandContractions, _options.RemoveNumbers);
        var actualText = string.Join(" ", asrResponse.Words);
        var normalizedActualText = TextNormalizer.Normalize(actualText, _options.ExpandContractions, _options.RemoveNumbers);
        
        var segmentWer = CalculateSegmentWER(expectedText, normalizedActualText);
        
        stats.Add(new SegmentStats(
            Index: 0,
            StartTime: startTime,
            EndTime: endTime,
            ExpectedText: expectedText,
            ActualText: normalizedActualText,
            WordErrorRate: segmentWer,
            Confidence: 0.0 // No confidence available in word-level response
        ));
        
        return stats;
    }

    private double CalculateSegmentWER(string expected, string actual)
    {
        var expectedWords = TextNormalizer.TokenizeWords(expected);
        var actualWords = TextNormalizer.TokenizeWords(actual);
        
        if (expectedWords.Length == 0) return actualWords.Length > 0 ? 1.0 : 0.0;
        
        var distance = CalculateEditDistance(expectedWords, actualWords);
        return (double)distance / expectedWords.Length;
    }

    private int CalculateEditDistance(string[] s1, string[] s2)
    {
        var dp = new int[s1.Length + 1, s2.Length + 1];
        
        for (int i = 0; i <= s1.Length; i++) dp[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++) dp[0, j] = j;
        
        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
            }
        }
        
        return dp[s1.Length, s2.Length];
    }

    private (int correct, int substitutions, int insertions, int deletions) CalculateWordErrorStats(List<AlignmentResult> alignment)
    {
        int correct = 0, substitutions = 0, insertions = 0, deletions = 0;
        
        foreach (var result in alignment)
        {
            switch (result.Operation)
            {
                case AlignmentOperation.Match: correct++; break;
                case AlignmentOperation.Substitute: substitutions++; break;
                case AlignmentOperation.Insert: insertions++; break;
                case AlignmentOperation.Delete: deletions++; break;
            }
        }
        
        return (correct, substitutions, insertions, deletions);
    }

    private double CalculateWordErrorRate(int correct, int substitutions, int insertions, int deletions, int totalExpected)
    {
        if (totalExpected == 0) return insertions > 0 ? 1.0 : 0.0;
        return (double)(substitutions + insertions + deletions) / totalExpected;
    }

    private double CalculateCharacterErrorRate(string expected, string actual)
    {
        var normalizedExpected = TextNormalizer.Normalize(expected, _options.ExpandContractions, _options.RemoveNumbers);
        var normalizedActual = TextNormalizer.Normalize(actual, _options.ExpandContractions, _options.RemoveNumbers);
        
        if (string.IsNullOrEmpty(normalizedExpected))
            return string.IsNullOrEmpty(normalizedActual) ? 0.0 : 1.0;
        
        var distance = CalculateEditDistance(normalizedExpected.ToCharArray().Select(c => c.ToString()).ToArray(),
                                           normalizedActual.ToCharArray().Select(c => c.ToString()).ToArray());
        
        return (double)distance / normalizedExpected.Length;
    }

    private string GetTranscriptText(AsrResponse asrResponse)
    {
        return string.Join(" ", asrResponse.Words);
    }

    private record WordAlignment
    {
        public required string Word { get; init; }
        public double StartTime { get; init; }
        public double EndTime { get; init; }
        public required string OriginalWord { get; init; }
    }

    private record AlignmentResult
    {
        public required AlignmentOperation Operation { get; init; }
        public string? ExpectedWord { get; init; }
        public WordAlignment? ActualWord { get; init; }
        public double Cost { get; init; }
    }

    private enum AlignmentOperation
    {
        Match,
        Substitute,
        Insert,
        Delete
    }
}

public record ValidationOptions
{
    public double SubstitutionCost { get; init; } = 1.0;
    public double InsertionCost { get; init; } = 1.0;
    public double DeletionCost { get; init; } = 1.0;
    public bool ExpandContractions { get; init; } = false;
    public bool RemoveNumbers { get; init; } = false;
}
