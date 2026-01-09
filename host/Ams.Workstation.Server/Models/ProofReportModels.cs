using System;
using System.Collections.Generic;

namespace Ams.Workstation.Server.Models;

/// <summary>
/// Complete report for a single chapter with all sentences and paragraphs.
/// </summary>
public record ChapterReport(
    string ChapterName,
    string AudioPath,
    string ScriptPath,
    DateTime Created,
    ChapterStats Stats,
    IReadOnlyList<SentenceReport> Sentences,
    IReadOnlyList<ParagraphReport> Paragraphs);

/// <summary>
/// Summary statistics for a chapter.
/// </summary>
public record ChapterStats(
    int SentenceCount,
    int FlaggedCount,
    string AvgWer,
    string MaxWer,
    int ParagraphCount,
    string ParagraphAvgWer,
    string AvgCoverage);

/// <summary>
/// Detailed report for a single sentence.
/// </summary>
public record SentenceReport(
    int Id,
    string Wer,
    string Cer,
    string Status,
    string BookRange,
    string ScriptRange,
    string Timing,
    string BookText,
    string ScriptText,
    string Excerpt,
    DiffReport? Diff,
    double StartTime,
    double EndTime,
    int? ParagraphId);

/// <summary>
/// Diff information showing text differences.
/// </summary>
public record DiffReport(IReadOnlyList<DiffOpReport> Ops);

/// <summary>
/// Single diff operation (equal, insert, delete).
/// </summary>
public record DiffOpReport(string Op, IReadOnlyList<string> Tokens);

/// <summary>
/// Detailed report for a single paragraph.
/// </summary>
public record ParagraphReport(
    int Id,
    string Wer,
    string Coverage,
    string Status,
    string BookRange,
    string Timing,
    string BookText,
    double StartTime,
    double EndTime,
    IReadOnlyList<int> SentenceIds,
    IReadOnlyList<int> FlaggedSentenceIds);
