using System;
using System.Collections.Generic;
using Ams.Core.Artifacts.Hydrate;

namespace Ams.Core.Artifacts.Validation;

public sealed record SourceInfo(string AudioPath, string ScriptPath, string BookIndexPath, DateTime CreatedAtUtc);

public sealed record SentenceView(
    int Id,
    (int Start, int End) BookRange,
    (int? Start, int? End)? ScriptRange,
    SentenceMetrics Metrics,
    string Status,
    string? BookText,
    string? ScriptText,
    TimingRange? Timing,
    HydratedDiff? Diff);

public sealed record ParagraphView(
    int Id,
    (int Start, int End) BookRange,
    ParagraphMetrics Metrics,
    string Status,
    string? BookText,
    HydratedDiff? Diff);

public sealed record WordTallies(int Match, int Substitution, int Insertion, int Deletion, int Total);

public sealed record ReportResult(
    string Report,
    IReadOnlyList<SentenceView> Sentences,
    IReadOnlyList<ParagraphView> Paragraphs,
    WordTallies? WordTallies);

public sealed record ValidationReportOptions(
    bool AllErrors,
    int TopSentences,
    int TopParagraphs,
    bool IncludeWordTallies,
    bool IncludeAllFlagged);
