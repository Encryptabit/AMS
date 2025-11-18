using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Prosody;
using Ams.Core.Runtime.Workspace;
using Ams.Cli.Utilities;
using Spectre.Console;
using Spectre.Console.Rendering;
using SentenceTiming = Ams.Core.Artifacts.SentenceTiming;

namespace Ams.Cli.Commands;

internal sealed class ValidateTimingSession
{
    private const double StructuralEpsilon = 1e-6;

    private readonly IWorkspace _workspace;
    private readonly FileInfo _transcriptFile;
    private readonly FileInfo _bookIndexFile;
    private readonly FileInfo _hydrateFile;
    private readonly bool _runProsodyAnalysis;
    private readonly bool _includeAllIntraSentenceGaps;
    private readonly bool _interSentenceOnly;
    private readonly PausePolicy _policy;
    private readonly string? _policySourcePath;
    private readonly FileInfo _pauseAdjustmentsFile;
    private PauseAnalysisReport? _prosodyAnalysis;

    public ValidateTimingSession(
        IWorkspace workspace,
        FileInfo transcriptFile,
        FileInfo bookIndexFile,
        FileInfo hydrateFile,
        bool runProsodyAnalysis, bool includeAllIntraSentenceGaps = false, bool interSentenceOnly = true)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _transcriptFile = transcriptFile ?? throw new ArgumentNullException(nameof(transcriptFile));
        _bookIndexFile = bookIndexFile ?? throw new ArgumentNullException(nameof(bookIndexFile));
        _hydrateFile = hydrateFile ?? throw new ArgumentNullException(nameof(hydrateFile));
        _runProsodyAnalysis = runProsodyAnalysis;
        _includeAllIntraSentenceGaps = includeAllIntraSentenceGaps;
        _interSentenceOnly = interSentenceOnly;
        (_policy, _policySourcePath) = PausePolicyResolver.Resolve(_transcriptFile);
        if (!string.IsNullOrWhiteSpace(_policySourcePath))
        {
            Log.Debug("Loaded pause policy from {Path}", _policySourcePath);
        }
        else
        {
            Log.Debug("Using default pause policy preset (house).");
        }

        var baseName = Path.GetFileNameWithoutExtension(_transcriptFile.Name);
        if (!string.IsNullOrWhiteSpace(baseName))
        {
            baseName = Path.GetFileNameWithoutExtension(baseName);
        }

        if (!string.IsNullOrWhiteSpace(baseName))
        {
            baseName = baseName.Replace(".pause-adjusted", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace(".align", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "pause";
        }

        var directory = _transcriptFile.DirectoryName ?? Environment.CurrentDirectory;
        var outputName = baseName + ".pause-adjustments.json";
        _pauseAdjustmentsFile = new FileInfo(Path.Combine(directory, outputName));
    }


    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var context = await LoadSessionContextAsync(cancellationToken).ConfigureAwait(false);

        if (_runProsodyAnalysis)
        {
            _prosodyAnalysis = context.Analysis;
        }

        RenderIntro(context.Transcript, context.BookIndex, context.Analysis.Spans.Count);
        var sessionState = new InteractiveState(
            context.PauseMap,
            context.Analysis,
            _policy,
            context.SentenceLookup,
            context.Paragraphs,
            context.SentenceToParagraph,
            context.ParagraphSentences);
        var renderer = new TimingRenderer(sessionState, _prosodyAnalysis, _policy);
        var controller = new TimingController(sessionState, renderer, result => OnCommit(sessionState, result));

        controller.Run();
    }

    public async Task<HeadlessResult> RunHeadlessAsync(CancellationToken cancellationToken)
    {
        var context = await LoadSessionContextAsync(cancellationToken).ConfigureAwait(false);

        if (_runProsodyAnalysis)
        {
            _prosodyAnalysis = context.Analysis;
        }

        var state = new InteractiveState(
            context.PauseMap,
            context.Analysis,
            _policy,
            context.SentenceLookup,
            context.Paragraphs,
            context.SentenceToParagraph,
            context.ParagraphSentences);

        state.ToggleOptionsFocus();
        var compressionSummary = state.ApplyCompressionPreview();
        var commitResult = state.CommitScope(state.Current, compressionSummary.HasChanges ? compressionSummary : null);
        _ = commitResult; // commitResult updates internal state; summary captured for logging.

        var adjustments = PersistPauseAdjustments(state);
        bool fileExists = _pauseAdjustmentsFile.Exists;
        bool hasAdjustments = adjustments.Count > 0 && fileExists;

        if (hasAdjustments)
        {
            var relativePath = GetRelativePathSafe(_pauseAdjustmentsFile.FullName);
            Log.Debug(
                "validate timing headless saved {Count} adjustment(s) to {Path} (compression total={Total}, within={Within}, downstream={Downstream})",
                adjustments.Count,
                relativePath,
                compressionSummary.TotalCount,
                compressionSummary.WithinScopeCount,
                compressionSummary.DownstreamCount);
        }
        else
        {
            Log.Debug("validate timing headless produced no adjustments; skipping pause-adjustments file");
        }

        return new HeadlessResult(
            hasAdjustments,
            new FileInfo(_pauseAdjustmentsFile.FullName),
            adjustments.Count,
            compressionSummary.TotalCount,
            compressionSummary.WithinScopeCount,
            compressionSummary.DownstreamCount);
    }

    private Task<SessionContext> LoadSessionContextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var openOptions = new ChapterOpenOptions
        {
            BookIndexFile = _bookIndexFile,
            TranscriptFile = _transcriptFile,
            HydrateFile = _hydrateFile
        };

        using var handle = _workspace.OpenChapter(openOptions);

        var bookIndex = handle.Book.Documents.BookIndex
            ?? throw new InvalidOperationException("BookIndex is not available in the chapter context.");
        var transcript = handle.Chapter.Documents.Transcript
            ?? throw new InvalidOperationException("TranscriptIndex is not available in the chapter context.");
        var hydrated = handle.Chapter.Documents.HydratedTranscript
            ?? throw new InvalidOperationException("Hydrated transcript is not available in the chapter context.");
        var sentenceLookup = BuildSentenceLookup(bookIndex);
        var (paragraphs, sentenceToParagraph, paragraphSentences) = BuildParagraphData(bookIndex);

        var silenceSpans = TryLoadMfaSilences(transcript);
        var policy = _policy;

        var service = new PauseDynamicsService();
        var analysis = service.AnalyzeChapter(
            transcript,
            bookIndex,
            hydrated,
            policy,
            silenceSpans,
            _includeAllIntraSentenceGaps);
        var pauseMap = PauseMapBuilder.Build(
            transcript,
            bookIndex,
            hydrated,
            policy,
            silenceSpans,
            _includeAllIntraSentenceGaps);
        handle.Save();

        var session = new SessionContext(
            transcript,
            bookIndex,
            hydrated,
            sentenceLookup,
            paragraphs,
            sentenceToParagraph,
            paragraphSentences,
            analysis,
            pauseMap);

        return Task.FromResult(session);
    }

    private IReadOnlyList<(double Start, double End)>? TryLoadMfaSilences(TranscriptIndex transcript)
    {
        var candidates = BuildTextGridCandidates(transcript);
        foreach (var path in candidates)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            try
            {
                var wordIntervals = TextGridParser.ParseWordIntervals(path);
                var silences = wordIntervals
                    .Where(interval => IsSilenceLabel(interval.Text))
                    .Select(interval => (interval.Start, interval.End))
                    .Where(span => span.End - span.Start > 0.0)
                    .ToList();

                if (silences.Count > 0)
                {
                    return silences;
                }
            }
            catch
            {
                // ignored – try next candidate
            }
        }

        return null;
    }

    private IReadOnlyList<string> BuildTextGridCandidates(TranscriptIndex transcript)
    {
        var roots = new List<string>();

        void AddRootFrom(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return;
            }

            var alignment = Path.Combine(directory, "alignment", "mfa");
            roots.Add(alignment);
        }

        AddRootFrom(_hydrateFile.FullName);
        AddRootFrom(_transcriptFile.FullName);
        AddRootFrom(transcript.ScriptPath);

        var uniqueRoots = roots
            .Where(Directory.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var results = new List<string>();
        foreach (var root in uniqueRoots)
        {
            results.AddRange(Directory.EnumerateFiles(root, "*.TextGrid", SearchOption.AllDirectories));
        }

        return results;
    }

    private static bool IsSilenceLabel(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        text = text.Trim();
        return text.Length switch
        {
            0 => true,
            2 when text.Equals("sp", StringComparison.OrdinalIgnoreCase) => true,
            3 when text.Equals("sil", StringComparison.OrdinalIgnoreCase) => true,
            _ => text.Equals("<sil>", StringComparison.OrdinalIgnoreCase)
        };
    }


    private static string ExtractBookText(BookIndex book, int start, int end)
    {
        if (book.Words.Length == 0 || end < start)
        {
            return string.Empty;
        }

        int safeStart = Math.Clamp(start, 0, book.Words.Length - 1);
        int safeEnd = Math.Clamp(end, safeStart, book.Words.Length - 1);

        var words = new List<string>(safeEnd - safeStart + 1);
        for (int i = safeStart; i <= safeEnd; i++)
        {
            var text = book.Words[i].Text;
            if (!string.IsNullOrEmpty(text))
            {
                words.Add(text);
            }
        }

        return string.Join(' ', words);
    }

    private static IReadOnlyDictionary<int, string> BuildSentenceLookup(BookIndex book)
    {
        var map = new Dictionary<int, string>(book.Sentences.Length);

        foreach (var sentence in book.Sentences)
        {
            var text = ExtractBookText(book, sentence.Start, sentence.End);
            map[sentence.Index] = text;
        }

        return map;
    }

    private static (IReadOnlyList<ParagraphInfo> Paragraphs, IReadOnlyDictionary<int, int> SentenceToParagraph,
        IReadOnlyDictionary<int, IReadOnlyList<int>> ParagraphSentences) BuildParagraphData(BookIndex book)
    {
        var paragraphs = new List<ParagraphInfo>(book.Paragraphs.Length);
        foreach (var paragraph in book.Paragraphs)
        {
            var text = ExtractBookText(book, paragraph.Start, paragraph.End);
            paragraphs.Add(new ParagraphInfo(paragraph.Index, paragraph.Kind, paragraph.Style, text));
        }

        var sentenceToParagraph = new Dictionary<int, int>(book.Sentences.Length);
        var paragraphBuckets = new Dictionary<int, List<int>>(book.Paragraphs.Length);

        foreach (var sentence in book.Sentences)
        {
            int paragraphIndex = 0;
            if (sentence.Start >= 0 && sentence.Start < book.Words.Length)
            {
                paragraphIndex = book.Words[sentence.Start].ParagraphIndex;
            }

            sentenceToParagraph[sentence.Index] = paragraphIndex;

            if (!paragraphBuckets.TryGetValue(paragraphIndex, out var list))
            {
                list = new List<int>();
                paragraphBuckets[paragraphIndex] = list;
            }

            list.Add(sentence.Index);
        }

        var frozenBuckets = paragraphBuckets.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<int>)kvp.Value);

        return (paragraphs, sentenceToParagraph, frozenBuckets);
    }

    private void RenderIntro(TranscriptIndex transcript, BookIndex book, int gapCount)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold dodgerblue1]Timing session[/] 🕒");
        AnsiConsole.MarkupLineInterpolated($"[grey]Transcript :[/] {_transcriptFile.FullName}");
        AnsiConsole.MarkupLineInterpolated($"[grey]Book index:[/] {_bookIndexFile.FullName}");
        if (_hydrateFile is not null)
        {
            AnsiConsole.MarkupLineInterpolated($"[grey]Hydrate   :[/] {_hydrateFile.FullName}");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated(
            $"Loaded [green]{transcript.Sentences.Count}[/] sentences, [green]{gapCount}[/] gap spans.");
        AnsiConsole.MarkupLineInterpolated($"Source manuscript words: [green]{book.Words.Length}[/].");
        AnsiConsole.WriteLine();
    }

    private void OnCommit(InteractiveState state, CommitResult result)
    {
        if (!result.HasChanges)
        {
            return;
        }

        try
        {
            _ = PersistPauseAdjustments(state);
        }
        catch (Exception ex)
        {
            state.UpdateLastCommitMessage($"Commit succeeded, but saving pause adjustments failed: {ex.Message}");
        }
    }

    private IReadOnlyList<PauseAdjust> PersistPauseAdjustments(InteractiveState state)
    {
        var adjustments = BuildAdjustmentsIncludingStatic(state);

        if (adjustments.Count == 0)
        {
            if (_pauseAdjustmentsFile.Exists)
            {
                _pauseAdjustmentsFile.Delete();
            }

            var baseMessage = state.LastCommitMessage ?? string.Empty;
            state.UpdateLastCommitMessage($"{baseMessage}  No adjustments to save.");
            return adjustments;
        }

        var document = PauseAdjustmentsDocument.Create(
            _transcriptFile.FullName,
            DateTime.UtcNow,
            _policy,
            adjustments);

        document.Save(_pauseAdjustmentsFile.FullName);

        var relativePath = GetRelativePathSafe(_pauseAdjustmentsFile.FullName);
        var message = state.LastCommitMessage ?? string.Empty;
        state.UpdateLastCommitMessage($"{message}  Saved to {relativePath}");

        return adjustments;
    }

    private IReadOnlyList<PauseAdjust> BuildAdjustmentsIncludingStatic(InteractiveState state)
    {
        var dynamicAdjustments = state.GetCommittedAdjustments()
            .Where(adjust => !IsStructuralClass(adjust.Class))
            .Where(adjust => !_interSentenceOnly || adjust.LeftSentenceId != adjust.RightSentenceId)
            .ToList();

        var structural = BuildStaticBufferAdjustments(state, dynamicAdjustments);
        if (structural.Count > 0)
        {
            dynamicAdjustments.AddRange(structural);
        }

        state.FilterParagraphZeroAdjustments(dynamicAdjustments);

        return dynamicAdjustments;
    }

    private IReadOnlyList<PauseAdjust> BuildStaticBufferAdjustments(InteractiveState state,
        IReadOnlyList<PauseAdjust> dynamicAdjustments)
    {
        var baseline = state.GetBaselineTimeline();
        if (baseline.Count == 0)
        {
            return Array.Empty<PauseAdjust>();
        }

        var timelineResult = PauseTimelineApplier.Apply(baseline, dynamicAdjustments);
        var ordered = timelineResult.Timeline
            .OrderBy(kvp => kvp.Value.StartSec)
            .Select(kvp => (SentenceId: kvp.Key, Timing: kvp.Value))
            .ToList();

        if (ordered.Count == 0)
        {
            return Array.Empty<PauseAdjust>();
        }

        var structural = new List<PauseAdjust>();

        var first = ordered[0];
        double currentHead = Math.Max(0d, first.Timing.StartSec);
        double forcedHeadTarget = 0.75d;
        structural.Add(new PauseAdjust(
            first.SentenceId,
            first.SentenceId,
            PauseClass.ChapterHead,
            currentHead,
            forcedHeadTarget,
            StartSec: 0d,
            EndSec: currentHead,
            HasGapHint: false));

        if (ordered.Count >= 2)
        {
            var second = ordered[1];
            double currentGap = second.Timing.StartSec - first.Timing.EndSec;
            double forcedPostChapterTarget = 1.5d;
            structural.Add(new PauseAdjust(
                first.SentenceId,
                second.SentenceId,
                PauseClass.PostChapterRead,
                currentGap,
                forcedPostChapterTarget,
                StartSec: first.Timing.EndSec,
                EndSec: second.Timing.StartSec,
                HasGapHint: false));
        }

        var last = ordered[^1];
        double currentTail = 0d;
        double targetTail = Math.Max(0d, _policy.Tail);
        if (Math.Abs(currentTail - targetTail) > StructuralEpsilon)
        {
            structural.Add(new PauseAdjust(
                last.SentenceId,
                -1,
                PauseClass.Tail,
                currentTail,
                targetTail,
                StartSec: last.Timing.EndSec,
                EndSec: last.Timing.EndSec + currentTail,
                HasGapHint: false));
        }

        return structural;
    }

    internal sealed record HeadlessResult(
        bool HasAdjustments,
        FileInfo AdjustmentsFile,
        int AdjustmentCount,
        int CompressionTotal,
        int CompressionWithin,
        int CompressionDownstream);

    private sealed record SessionContext(
        TranscriptIndex Transcript,
        BookIndex BookIndex,
        HydratedTranscript Hydrated,
        IReadOnlyDictionary<int, string> SentenceLookup,
        IReadOnlyList<ParagraphInfo> Paragraphs,
        IReadOnlyDictionary<int, int> SentenceToParagraph,
        IReadOnlyDictionary<int, IReadOnlyList<int>> ParagraphSentences,
        PauseAnalysisReport Analysis,
        ChapterPauseMap PauseMap);

    private static bool IsStructuralClass(PauseClass pauseClass)
    {
        return pauseClass is PauseClass.ChapterHead or PauseClass.PostChapterRead or PauseClass.Tail;
    }

    private static string GetRelativePathSafe(string path)
    {
        try
        {
            return Path.GetRelativePath(Environment.CurrentDirectory, path);
        }
        catch
        {
            return path;
        }
    }

    private sealed class InteractiveState
    {
        private const double DurationEpsilon = 1e-6;

        private readonly ChapterPauseMap _chapter;
        private readonly PauseAnalysisReport _analysis;
        private readonly PausePolicy _basePolicy;
        private readonly IReadOnlyDictionary<int, string> _sentenceLookup;
        private readonly IReadOnlyDictionary<int, IReadOnlyList<int>> _paragraphSentences;
        private readonly IReadOnlyDictionary<int, int> _sentenceToParagraph;
        private readonly Dictionary<int, ParagraphInfo> _paragraphLookup;
        private readonly Dictionary<int, List<EditablePause>> _sentencePauses;
        private readonly List<EditablePause> _chapterPauses;
        private readonly List<ScopeEntry> _entries;
        private readonly List<int> _orderedParagraphIds;
        private readonly Dictionary<int, SentenceTiming> _baselineTimeline;
        private readonly List<PauseAdjust> _committedAdjustments = new();
        private CompressionState? _compression;

        private int _treeOffset;
        private int _treeViewportSize = 24;
        private bool _optionsFocused;
        private string? _lastCommitMessage;

        public InteractiveState(
            ChapterPauseMap chapter,
            PauseAnalysisReport analysis,
            PausePolicy basePolicy,
            IReadOnlyDictionary<int, string> sentenceLookup,
            IReadOnlyList<ParagraphInfo> paragraphs,
            IReadOnlyDictionary<int, int> sentenceToParagraph,
            IReadOnlyDictionary<int, IReadOnlyList<int>> paragraphSentences)
        {
            _chapter = chapter ?? throw new ArgumentNullException(nameof(chapter));
            _analysis = analysis ?? PauseAnalysisReport.Empty;
            _basePolicy = basePolicy ?? throw new ArgumentNullException(nameof(basePolicy));
            _sentenceLookup = sentenceLookup ?? new Dictionary<int, string>();
            _sentenceToParagraph = sentenceToParagraph ?? new Dictionary<int, int>();
            _paragraphSentences = paragraphSentences ?? new Dictionary<int, IReadOnlyList<int>>();
            _paragraphLookup = (paragraphs ?? Array.Empty<ParagraphInfo>()).ToDictionary(info => info.Index);
            _sentencePauses = new Dictionary<int, List<EditablePause>>();
            _chapterPauses = new List<EditablePause>();
            PopulatePauseLookups();
            TotalSentenceCount = _chapter.Paragraphs.Sum(paragraph => paragraph.Sentences.Count);
            _entries = BuildEntries();
            _orderedParagraphIds = _chapter.Paragraphs
                .OrderBy(paragraph => paragraph.OriginalStart)
                .Select(paragraph => paragraph.ParagraphId)
                .ToList();
            _baselineTimeline = BuildBaselineTimeline(_chapter);
            CursorIndex = _entries.Count > 0 ? 0 : -1;
            EnsureTreeVisibility();
        }

        public int CursorIndex { get; private set; }

        public IReadOnlyList<ScopeEntry> Entries => _entries;

        public ScopeEntry Current =>
            CursorIndex >= 0 && CursorIndex < _entries.Count ? _entries[CursorIndex] : ScopeEntry.Empty;

        public int ParagraphCount => _chapter.Paragraphs.Count;

        public int TotalSentenceCount { get; }

        public int TotalPauseCount => _analysis.Spans.Count;

        public bool OptionsFocused => _optionsFocused;

        public string? LastCommitMessage => _lastCommitMessage;

        public IReadOnlyList<PauseAdjust> GetCommittedAdjustments() => _committedAdjustments.ToList();

        public IReadOnlyDictionary<int, SentenceTiming> GetBaselineTimeline()
        {
            var clone = new Dictionary<int, SentenceTiming>(_baselineTimeline.Count);
            foreach (var kvp in _baselineTimeline)
            {
                var timing = kvp.Value;
                clone[kvp.Key] = new SentenceTiming(timing.StartSec, timing.EndSec, timing.FragmentBacked,
                    timing.Confidence);
            }

            return clone;
        }

        public bool MoveWithinTier(int delta)
        {
            if (_entries.Count == 0)
            {
                CursorIndex = -1;
                return false;
            }

            if (CursorIndex < 0)
            {
                CursorIndex = 0;
                EnsureTreeVisibility();
                RefreshCompressionStateIfNeeded(resetSelection: true);
                return true;
            }

            var current = Current;
            int depth = current.Depth;

            int index = CursorIndex + delta;
            while (index >= 0 && index < _entries.Count)
            {
                var candidate = _entries[index];
                if (candidate.Kind == current.Kind && candidate.Depth == depth)
                {
                    CursorIndex = index;
                    EnsureTreeVisibility();
                    RefreshCompressionStateIfNeeded(resetSelection: true);
                    return true;
                }

                index += delta;
            }

            return false;
        }

        public bool StepInto()
        {
            if (_entries.Count == 0 || CursorIndex < 0)
            {
                return false;
            }

            var current = Current;
            int depth = current.Depth;
            for (int i = CursorIndex + 1; i < _entries.Count; i++)
            {
                var candidate = _entries[i];
                if (candidate.Depth <= depth)
                {
                    break;
                }

                if (candidate.Depth == depth + 1)
                {
                    CursorIndex = i;
                    EnsureTreeVisibility();
                    RefreshCompressionStateIfNeeded(resetSelection: true);
                    return true;
                }
            }

            return false;
        }

        public bool StepOut()
        {
            if (_entries.Count == 0 || CursorIndex <= 0)
            {
                return false;
            }

            var current = Current;
            if (current.Depth == 0)
            {
                return false;
            }

            int targetDepth = current.Depth - 1;
            for (int i = CursorIndex - 1; i >= 0; i--)
            {
                var candidate = _entries[i];
                if (candidate.Depth == targetDepth)
                {
                    CursorIndex = i;
                    EnsureTreeVisibility();
                    RefreshCompressionStateIfNeeded(resetSelection: true);
                    return true;
                }
            }

            return false;
        }

        public bool AdjustCurrent(double deltaSeconds)
        {
            if (CursorIndex < 0 || CursorIndex >= _entries.Count)
            {
                return false;
            }

            var entry = _entries[CursorIndex];
            if (entry.Kind != ScopeEntryKind.Pause || entry.Pause is null)
            {
                return false;
            }

            entry.Pause.Adjust(deltaSeconds);
            NotifyCompressionPauseAdjusted(entry.Pause);
            return true;
        }

        public bool SetCurrent(double newDuration)
        {
            if (CursorIndex < 0 || CursorIndex >= _entries.Count)
            {
                return false;
            }

            var entry = _entries[CursorIndex];
            if (entry.Kind != ScopeEntryKind.Pause || entry.Pause is null)
            {
                return false;
            }

            entry.Pause.Set(newDuration);
            NotifyCompressionPauseAdjusted(entry.Pause);
            return true;
        }

        public void ToggleOptionsFocus()
        {
            _optionsFocused = !_optionsFocused;
            if (_optionsFocused)
            {
                EnsureCompressionStateForCurrentScope(resetSelection: true);
            }
        }

        public void UpdateLastCommitMessage(string message)
        {
            _lastCommitMessage = message;
        }

        public void SetTreeViewportSize(int size)
        {
            int clamped = Math.Max(5, size);
            if (clamped != _treeViewportSize)
            {
                _treeViewportSize = clamped;
                EnsureTreeVisibility();
                RefreshCompressionStateIfNeeded(resetSelection: false);
            }
        }

        public ParagraphInfo? GetParagraphInfo(int paragraphId)
        {
            return _paragraphLookup.TryGetValue(paragraphId, out var info) ? info : null;
        }

        public string GetSentenceText(int sentenceId)
        {
            return _sentenceLookup.TryGetValue(sentenceId, out var text) ? text : string.Empty;
        }

        public bool IsParagraphZero(int sentenceId)
        {
            return _sentenceToParagraph.TryGetValue(sentenceId, out var paragraphId) && paragraphId == 0;
        }

        public void FilterParagraphZeroAdjustments(List<PauseAdjust> adjustments)
        {
            if (adjustments is null || adjustments.Count == 0)
            {
                return;
            }

            adjustments.RemoveAll(adj =>
                adj.Class is not PauseClass.ChapterHead and not PauseClass.PostChapterRead
                && (IsParagraphZero(adj.LeftSentenceId) || IsParagraphZero(adj.RightSentenceId)));
        }

        public IReadOnlyList<int> GetParagraphSentenceIds(int paragraphId)
        {
            return _paragraphSentences.TryGetValue(paragraphId, out var list) ? list : Array.Empty<int>();
        }

        public int GetParagraphSentenceCount(int paragraphId) => GetParagraphSentenceIds(paragraphId).Count;

        public int CountSentencePauses(int sentenceId)
        {
            return _sentencePauses.TryGetValue(sentenceId, out var list) ? list.Count : 0;
        }

        public int CountParagraphPauses(int paragraphId)
        {
            int total = 0;
            foreach (var sentenceId in GetParagraphSentenceIds(paragraphId))
            {
                if (_sentencePauses.TryGetValue(sentenceId, out var list))
                {
                    total += list.Count;
                }
            }

            return total;
        }

        public bool MoveCompressionControlSelection(int delta)
        {
            if (_compression is null || delta == 0)
            {
                return false;
            }

            return _compression.MoveControlSelection(delta);
        }

        public bool AdjustCompressionControl(int direction, ConsoleModifiers modifiers)
        {
            if (_compression is null || direction == 0)
            {
                return false;
            }

            double multiplier = (modifiers & ConsoleModifiers.Shift) != 0 ? 5d : 1d;
            return _compression.AdjustSelectedControl(direction * multiplier, _basePolicy);
        }

        public bool ScrollCompressionPreview(int delta)
        {
            if (_compression is null || delta == 0)
            {
                return false;
            }

            return _compression.ScrollPreview(delta);
        }

        public CompressionControlsSnapshot GetCompressionControlsSnapshot()
        {
            if (_compression is null)
            {
                return CompressionControlsSnapshot.Empty;
            }

            return _compression.GetSnapshot();
        }

        public IReadOnlyList<CompressionPreviewItem> GetCompressionPreview(int maxRows, out bool hasPrevious,
            out bool hasNext)
        {
            if (_compression is null)
            {
                hasPrevious = false;
                hasNext = false;
                return Array.Empty<CompressionPreviewItem>();
            }

            return _compression.GetPreviewSlice(maxRows, out hasPrevious, out hasNext);
        }

        public bool HasCompressionPreview => _compression is not null && _compression.HasPreview;

        public CompressionApplySummary ApplyCompressionPreview()
        {
            if (_compression is null)
            {
                return CompressionApplySummary.Empty;
            }

            return _compression.ApplyPreview(DurationEpsilon, _basePolicy);
        }

        private void RefreshCompressionStateIfNeeded(bool resetSelection)
        {
            if (_optionsFocused)
            {
                EnsureCompressionStateForCurrentScope(resetSelection);
            }
        }

        private void EnsureCompressionStateForCurrentScope(bool resetSelection)
        {
            if (CursorIndex < 0 || CursorIndex >= _entries.Count)
            {
                _compression = null;
                return;
            }

            var scope = Current;
            var pauses = CollectCompressionPauses(scope);

            if (pauses.Count == 0)
            {
                _compression = null;
                return;
            }

            if (_compression is not null && _compression.MatchesScope(scope))
            {
                if (resetSelection)
                {
                    _compression.ResetSelection();
                }

                _compression.RebuildPreview(_basePolicy);
                return;
            }

            _compression =
                new CompressionState(scope, CompressionControls.FromPolicy(_basePolicy), pauses, _basePolicy);
        }

        private List<EditablePause> CollectCompressionPauses(ScopeEntry scope)
        {
            var set = new HashSet<EditablePause>();

            void AddPause(EditablePause pause)
            {
                if (pause is not null)
                {
                    set.Add(pause);
                }
            }

            if (scope.Kind == ScopeEntryKind.Pause && scope.Pause is not null)
            {
                AddPause(scope.Pause);
                return set.OrderBy(p => p.Span.StartSec).ToList();
            }

            IEnumerable<int> paragraphRange;

            switch (scope.Kind)
            {
                case ScopeEntryKind.Chapter:
                    foreach (var pause in _chapterPauses)
                    {
                        AddPause(pause);
                    }

                    foreach (var list in _sentencePauses.Values)
                    {
                        foreach (var pause in list)
                        {
                            AddPause(pause);
                        }
                    }

                    break;

                case ScopeEntryKind.Paragraph when scope.ParagraphId.HasValue:
                    paragraphRange = GetParagraphRange(scope.ParagraphId.Value);
                    foreach (var paragraphId in paragraphRange)
                    {
                        var sentenceIds = GetParagraphSentenceIds(paragraphId);
                        foreach (var sentenceId in sentenceIds)
                        {
                            if (_sentencePauses.TryGetValue(sentenceId, out var list))
                            {
                                foreach (var pause in list)
                                {
                                    AddPause(pause);
                                }
                            }
                        }

                        foreach (var pause in _chapterPauses.Where(p => p.LeftParagraphId == paragraphId))
                        {
                            AddPause(pause);
                        }
                    }

                    break;

                case ScopeEntryKind.Sentence when scope.ParagraphId.HasValue && scope.SentenceId.HasValue:
                    var sentenceIdsInParagraph = GetParagraphSentenceIds(scope.ParagraphId.Value).ToList();
                    int sentenceIndex = sentenceIdsInParagraph.IndexOf(scope.SentenceId.Value);
                    if (sentenceIndex >= 0)
                    {
                        for (int i = sentenceIndex; i < sentenceIdsInParagraph.Count; i++)
                        {
                            int sentenceId = sentenceIdsInParagraph[i];
                            if (_sentencePauses.TryGetValue(sentenceId, out var list))
                            {
                                foreach (var pause in list)
                                {
                                    AddPause(pause);
                                }
                            }
                        }
                    }

                    break;

                default:
                    return new List<EditablePause>();
            }

            return set
                .OrderBy(pause => pause.Span.StartSec)
                .ToList();
        }

        public IReadOnlyList<ScopeEntry> GetTreeViewportEntries(out bool hasPrevious, out bool hasNext)
        {
            hasPrevious = false;
            hasNext = false;

            if (_entries.Count == 0)
            {
                return Array.Empty<ScopeEntry>();
            }

            EnsureTreeVisibility();

            int end = Math.Min(_treeOffset + _treeViewportSize, _entries.Count);
            hasPrevious = _treeOffset > 0;
            hasNext = end < _entries.Count;

            return _entries.GetRange(_treeOffset, end - _treeOffset);
        }

        private IEnumerable<int> GetParagraphRange(int startParagraphId)
        {
            int startIndex = _orderedParagraphIds.IndexOf(startParagraphId);
            if (startIndex < 0)
            {
                yield break;
            }

            for (int i = startIndex; i < _orderedParagraphIds.Count; i++)
            {
                yield return _orderedParagraphIds[i];
            }
        }

        public int GetPendingPauseCount(ScopeEntry scope)
        {
            return CollectPauses(scope).Count(pause => pause.HasChanges);
        }

        public CommitResult CommitScope(ScopeEntry scope, CompressionApplySummary? summary = null)
        {
            IEnumerable<EditablePause> pauseSource;

            if (_compression is not null && _compression.MatchesScope(scope))
            {
                pauseSource = _compression.PausesForCommit;
            }
            else
            {
                pauseSource = CollectPauses(scope);
            }

            var pauses = pauseSource.Where(pause => pause.HasChanges).ToList();
            if (pauses.Count == 0)
            {
                _lastCommitMessage = "No pending adjustments to commit.";
                return CommitResult.Empty;
            }

            var adjustments = new List<PauseAdjust>(pauses.Count);
            foreach (var pause in pauses)
            {
                var adjust = new PauseAdjust(
                    pause.Span.LeftSentenceId,
                    pause.Span.RightSentenceId,
                    pause.Span.Class,
                    pause.BaselineDurationSec,
                    pause.AdjustedDurationSec,
                    pause.Span.StartSec,
                    pause.Span.EndSec,
                    pause.Span.HasGapHint);

                _committedAdjustments.RemoveAll(existing => MatchesCommittedPause(existing, pause.Span));
                adjustments.Add(adjust);
                _committedAdjustments.Add(adjust);
                pause.Commit();
            }

            string scopeLabel = scope.Kind switch
            {
                ScopeEntryKind.Chapter => "chapter",
                ScopeEntryKind.Paragraph => scope.ParagraphId.HasValue ? $"paragraph {scope.ParagraphId}" : "paragraph",
                ScopeEntryKind.Sentence => scope.SentenceId.HasValue ? $"sentence {scope.SentenceId}" : "sentence",
                ScopeEntryKind.Pause => "pause",
                _ => "scope"
            };

            int total = summary?.TotalCount ?? adjustments.Count;
            int within = summary?.WithinScopeCount ?? adjustments.Count;
            int downstream = summary?.DownstreamCount ?? 0;

            if (summary is null)
            {
                downstream = Math.Max(0, total - within);
            }

            _lastCommitMessage = downstream > 0
                ? $"Committed {total} adjustment(s) for {scopeLabel} (scope {within}, downstream {downstream})."
                : $"Committed {total} adjustment(s) for {scopeLabel}.";

            _compression?.HandleCommit(scope, _basePolicy);
            return new CommitResult(adjustments.Count, scopeLabel, adjustments);
        }

        public IReadOnlyList<EditablePause> CollectPauses(ScopeEntry scope)
        {
            var result = new List<EditablePause>();

            void Append(IEnumerable<EditablePause> pauses)
            {
                foreach (var pause in pauses)
                {
                    if (!result.Contains(pause))
                    {
                        result.Add(pause);
                    }
                }
            }

            switch (scope.Kind)
            {
                case ScopeEntryKind.Chapter:
                    Append(_chapterPauses);
                    foreach (var list in _sentencePauses.Values)
                    {
                        Append(list);
                    }

                    break;

                case ScopeEntryKind.Paragraph when scope.ParagraphId.HasValue:
                    int paragraphId = scope.ParagraphId.Value;
                    Append(_chapterPauses.Where(pause =>
                        pause.LeftParagraphId == paragraphId || pause.RightParagraphId == paragraphId));
                    foreach (var sentenceId in GetParagraphSentenceIds(paragraphId))
                    {
                        if (_sentencePauses.TryGetValue(sentenceId, out var list))
                        {
                            Append(list);
                        }
                    }

                    break;

                case ScopeEntryKind.Sentence when scope.SentenceId.HasValue:
                    if (_sentencePauses.TryGetValue(scope.SentenceId.Value, out var sentencePauses))
                    {
                        Append(sentencePauses);
                    }

                    break;

                case ScopeEntryKind.Pause when scope.Pause is not null:
                    Append(new[] { scope.Pause });
                    break;
            }

            return result;
        }

        public ScopeEntry? GetChapterEntry() => _entries.FirstOrDefault(entry => entry.Kind == ScopeEntryKind.Chapter);

        public IEnumerable<ScopeEntry> EnumerateParagraphEntries() =>
            _entries.Where(entry => entry.Kind == ScopeEntryKind.Paragraph);

        public IEnumerable<ScopeEntry> EnumerateSentenceEntries(int paragraphId) => _entries
            .Where(entry => entry.Kind == ScopeEntryKind.Sentence && entry.ParagraphId == paragraphId);

        public IEnumerable<ScopeEntry> EnumeratePauseEntriesForSentence(int sentenceId) => _entries
            .Where(entry => entry.Kind == ScopeEntryKind.Pause && entry.SentenceId == sentenceId);

        public IEnumerable<ScopeEntry> EnumerateChapterPauseEntries() => _entries
            .Where(entry => entry.Kind == ScopeEntryKind.Pause && entry.SentenceId is null);

        public IReadOnlyList<DiffRow> GetPendingAdjustments(ScopeEntry scope)
        {
            if (scope is null)
            {
                return Array.Empty<DiffRow>();
            }

            var diffs = new List<DiffRow>();
            foreach (var pause in CollectPauses(scope))
            {
                if (pause.HasChanges && TryCreateDiffRow(pause, out var diff))
                {
                    diffs.Add(diff);
                }
            }

            return diffs.Count > 0 ? diffs : Array.Empty<DiffRow>();
        }

        private void EnsureTreeVisibility()
        {
            if (CursorIndex < 0 || _entries.Count == 0)
            {
                _treeOffset = 0;
                return;
            }

            if (_treeOffset > CursorIndex)
            {
                _treeOffset = CursorIndex;
            }

            int maxOffset = Math.Max(0, CursorIndex - _treeViewportSize + 1);
            if (CursorIndex >= _treeOffset + _treeViewportSize)
            {
                _treeOffset = Math.Min(maxOffset, _entries.Count - _treeViewportSize);
                if (_treeOffset < 0) _treeOffset = 0;
            }
        }

        public string DescribePauseContext(EditablePause pause) => BuildDiffContext(pause);

        private bool TryCreateDiffRow(EditablePause pause, out DiffRow diff)
        {
            if (!pause.HasChanges)
            {
                diff = default!;
                return false;
            }

            diff = new DiffRow(
                pause.Span.Class,
                pause.BaselineDurationSec,
                pause.AdjustedDurationSec,
                pause.Delta,
                BuildDiffContext(pause));
            return true;
        }

        private string BuildDiffContext(EditablePause pause)
        {
            var left = TrimAndEscape(pause.LeftText, 24);
            var right = TrimAndEscape(pause.RightText, 24);

            if (string.IsNullOrEmpty(left) && string.IsNullOrEmpty(right))
            {
                if (pause.IsIntraSentence)
                {
                    return Markup.Escape($"Sentence {pause.Span.LeftSentenceId}");
                }

                return Markup.Escape($"{pause.Span.LeftSentenceId}->{pause.Span.RightSentenceId}");
            }

            if (string.IsNullOrEmpty(left))
            {
                left = "[grey]<start>[/]";
            }

            if (string.IsNullOrEmpty(right))
            {
                right = "[grey]<end>[/]";
            }

            return $"{left} [grey]->[/] {right}";
        }

        private void NotifyCompressionPauseAdjusted(EditablePause pause)
        {
            _compression?.NotifyPauseAdjusted(pause, _basePolicy);
        }

        private sealed class CompressionState
        {
            private readonly List<EditablePause> _pauses;
            private readonly HashSet<EditablePause> _pauseLookup;

            public CompressionState(
                ScopeEntry scope,
                CompressionControls controls,
                List<EditablePause> pauses,
                PausePolicy basePolicy)
            {
                Scope = scope;
                Controls = controls;
                _pauses = pauses ?? new List<EditablePause>();
                _pauseLookup = new HashSet<EditablePause>(_pauses);
                Preview = new List<CompressionPreviewItem>();
                SelectedControlIndex = 0;
                PreviewOffset = 0;
                RebuildPreview(basePolicy);
            }

            public ScopeEntry Scope { get; }
            public CompressionControls Controls { get; private set; }
            public List<CompressionPreviewItem> Preview { get; private set; }
            public int SelectedControlIndex { get; private set; }
            public int PreviewOffset { get; private set; }
            public bool HasPreview => Preview.Count > 0;
            public IReadOnlyList<EditablePause> PausesForCommit => _pauses;

            public bool MatchesScope(ScopeEntry scope)
            {
                if (scope.Kind != Scope.Kind)
                {
                    return false;
                }

                return scope.Kind switch
                {
                    ScopeEntryKind.Chapter => true,
                    ScopeEntryKind.Paragraph => scope.ParagraphId == Scope.ParagraphId,
                    ScopeEntryKind.Sentence => scope.ParagraphId == Scope.ParagraphId &&
                                               scope.SentenceId == Scope.SentenceId,
                    ScopeEntryKind.Pause => scope.Pause is not null && Scope.Pause is not null &&
                                            ReferenceEquals(scope.Pause, Scope.Pause),
                    _ => false
                };
            }

            public void ResetSelection()
            {
                SelectedControlIndex = 0;
                PreviewOffset = 0;
            }

            public bool MoveControlSelection(int delta)
            {
                if (Controls.Count == 0)
                {
                    return false;
                }

                int newIndex = Math.Clamp(SelectedControlIndex + delta, 0, Controls.Count - 1);
                if (newIndex == SelectedControlIndex)
                {
                    return false;
                }

                SelectedControlIndex = newIndex;
                return true;
            }

            public bool AdjustSelectedControl(double deltaMultiplier, PausePolicy basePolicy)
            {
                if (Controls.Count == 0)
                {
                    return false;
                }

                if (!Controls.Adjust(SelectedControlIndex, deltaMultiplier))
                {
                    return false;
                }

                RebuildPreview(basePolicy);
                return true;
            }

            public bool ScrollPreview(int delta)
            {
                if (Preview.Count == 0 || delta == 0)
                {
                    return false;
                }

                int maxOffset = Math.Max(0, Preview.Count - 1);
                int newOffset = Math.Clamp(PreviewOffset + delta, 0, maxOffset);
                if (newOffset == PreviewOffset)
                {
                    return false;
                }

                PreviewOffset = newOffset;
                return true;
            }

            public IReadOnlyList<CompressionPreviewItem> GetPreviewSlice(int maxRows, out bool hasPrevious,
                out bool hasNext)
            {
                hasPrevious = false;
                hasNext = false;

                if (Preview.Count == 0 || maxRows <= 0)
                {
                    return Array.Empty<CompressionPreviewItem>();
                }

                int start = Math.Clamp(PreviewOffset, 0, Math.Max(0, Preview.Count - 1));
                int count = Math.Min(maxRows, Preview.Count - start);
                hasPrevious = start > 0;
                hasNext = start + count < Preview.Count;

                return Preview.GetRange(start, count);
            }

            public CompressionControlsSnapshot GetSnapshot()
            {
                var controls = new List<CompressionControlDisplay>
                {
                    new("Ratio inside", $"{Controls.RatioInside:0.00}x"),
                    new("Ratio outside", $"{Controls.RatioOutside:0.00}x"),
                    new("Knee width", $"{Controls.KneeWidth:0.000}s"),
                    new("Preserve top quantile", $"{Controls.PreserveTopQuantile:0.00}")
                };

                return new CompressionControlsSnapshot(controls, SelectedControlIndex);
            }

            public void NotifyPauseAdjusted(EditablePause pause, PausePolicy basePolicy)
            {
                if (_pauseLookup.Contains(pause))
                {
                    RebuildPreview(basePolicy);
                }
            }

            public void HandleCommit(ScopeEntry scope, PausePolicy basePolicy)
            {
                if (MatchesScope(scope))
                {
                    RebuildPreview(basePolicy);
                }
            }

            public void RebuildPreview(PausePolicy basePolicy)
            {
                if (basePolicy is null)
                {
                    Preview = new List<CompressionPreviewItem>();
                    return;
                }

                var policy = Controls.ToPolicy(basePolicy);

                var durations = new Dictionary<PauseClass, List<double>>();
                foreach (var pause in _pauses)
                {
                    double duration = pause.BaselineDurationSec;
                    if (duration <= 0d || !double.IsFinite(duration))
                    {
                        continue;
                    }

                    if (!durations.TryGetValue(pause.Span.Class, out var list))
                    {
                        list = new List<double>();
                        durations[pause.Span.Class] = list;
                    }

                    list.Add(duration);
                }

                var profiles = PauseCompressionMath.BuildProfiles(durations, policy);

                var preview = _pauses
                    .Select(pause =>
                    {
                        double original = pause.BaselineDurationSec;
                        double target = PauseCompressionMath.ShouldPreserve(original, pause.Span.Class, profiles)
                            ? original
                            : PauseCompressionMath.ComputeTargetDuration(original, pause.Span.Class, policy, profiles);
                        return new CompressionPreviewItem(pause, target);
                    })
                    .OrderBy(item => item.Pause.Span.StartSec)
                    .ToList();

                Preview = preview;
                int maxOffset = Math.Max(0, Preview.Count - 1);
                PreviewOffset = Math.Clamp(PreviewOffset, 0, maxOffset);
            }

            public CompressionApplySummary ApplyPreview(double epsilon, PausePolicy basePolicy)
            {
                int total = 0;
                int within = 0;

                foreach (var item in Preview)
                {
                    double delta = item.TargetDuration - item.OriginalDuration;
                    if (Math.Abs(delta) <= epsilon)
                    {
                        continue;
                    }

                    item.Pause.Set(item.TargetDuration);
                    total++;

                    if (IsWithinScope(item.Pause))
                    {
                        within++;
                    }
                }

                if (total > 0)
                {
                    RebuildPreview(basePolicy);
                }

                int downstream = Math.Max(0, total - within);
                return new CompressionApplySummary(total, within, downstream);
            }

            private bool IsWithinScope(EditablePause pause)
            {
                return Scope.Kind switch
                {
                    ScopeEntryKind.Chapter => true,
                    ScopeEntryKind.Paragraph when Scope.ParagraphId.HasValue => GetPauseParagraphId(pause) ==
                        Scope.ParagraphId.Value,
                    ScopeEntryKind.Sentence when Scope.SentenceId.HasValue => pause.Span.LeftSentenceId ==
                                                                              Scope.SentenceId.Value,
                    ScopeEntryKind.Pause => Scope.Pause is not null && ReferenceEquals(pause, Scope.Pause),
                    _ => false
                };
            }

            private static int GetPauseParagraphId(EditablePause pause)
            {
                return pause.LeftParagraphId ?? pause.RightParagraphId ?? -1;
            }
        }

        private sealed class CompressionControls
        {
            private const double MinRatio = 1.0;
            private const double MaxRatioInside = 5.0;
            private const double MaxRatioOutside = 6.0;
            private const double MaxKnee = 0.50;
            private const double MinQuantile = 0.0;
            private const double MaxQuantile = 0.99;

            private CompressionControls(double ratioInside, double ratioOutside, double kneeWidth,
                double preserveTopQuantile)
            {
                RatioInside = ratioInside;
                RatioOutside = ratioOutside;
                KneeWidth = kneeWidth;
                PreserveTopQuantile = preserveTopQuantile;
            }

            public double RatioInside { get; private set; }
            public double RatioOutside { get; private set; }
            public double KneeWidth { get; private set; }
            public double PreserveTopQuantile { get; private set; }

            public int Count => 4;

            public static CompressionControls FromPolicy(PausePolicy policy)
            {
                return new CompressionControls(policy.RatioInside, policy.RatioOutside, policy.KneeWidth,
                    policy.PreserveTopQuantile);
            }

            public PausePolicy ToPolicy(PausePolicy basePolicy)
            {
                return new PausePolicy(
                    new PauseWindow(basePolicy.Comma.Min, basePolicy.Comma.Max),
                    new PauseWindow(basePolicy.Sentence.Min, basePolicy.Sentence.Max),
                    new PauseWindow(basePolicy.Paragraph.Min, basePolicy.Paragraph.Max),
                    basePolicy.HeadOfChapter,
                    basePolicy.PostChapterRead,
                    basePolicy.Tail,
                    KneeWidth,
                    RatioInside,
                    RatioOutside,
                    PreserveTopQuantile);
            }

            public bool Adjust(int index, double deltaMultiplier)
            {
                double step = index switch
                {
                    0 => 0.05,
                    1 => 0.10,
                    2 => 0.01,
                    3 => 0.01,
                    _ => 0.05
                };

                double adjustedDelta = step * Math.Sign(deltaMultiplier) * Math.Max(1d, Math.Abs(deltaMultiplier));

                return index switch
                {
                    0 => SetRatioInside(RatioInside + adjustedDelta),
                    1 => SetRatioOutside(RatioOutside + adjustedDelta),
                    2 => SetKneeWidth(KneeWidth + adjustedDelta),
                    3 => SetPreserveTopQuantile(PreserveTopQuantile + adjustedDelta),
                    _ => false
                };
            }

            private bool SetRatioInside(double value)
            {
                double clamped = Math.Clamp(value, MinRatio, MaxRatioInside);
                if (Math.Abs(clamped - RatioInside) < 1e-6)
                {
                    return false;
                }

                RatioInside = clamped;
                return true;
            }

            private bool SetRatioOutside(double value)
            {
                double clamped = Math.Clamp(value, MinRatio, MaxRatioOutside);
                if (Math.Abs(clamped - RatioOutside) < 1e-6)
                {
                    return false;
                }

                RatioOutside = clamped;
                return true;
            }

            private bool SetKneeWidth(double value)
            {
                double clamped = Math.Clamp(value, 0d, MaxKnee);
                if (Math.Abs(clamped - KneeWidth) < 1e-6)
                {
                    return false;
                }

                KneeWidth = clamped;
                return true;
            }

            private bool SetPreserveTopQuantile(double value)
            {
                double clamped = Math.Clamp(value, MinQuantile, MaxQuantile);
                if (Math.Abs(clamped - PreserveTopQuantile) < 1e-6)
                {
                    return false;
                }

                PreserveTopQuantile = clamped;
                return true;
            }
        }

        public sealed record CompressionPreviewItem(EditablePause Pause, double TargetDuration)
        {
            public double OriginalDuration => Pause.BaselineDurationSec;
            public double Delta => TargetDuration - OriginalDuration;

            public string Label
            {
                get
                {
                    if (Pause.IsIntraSentence)
                    {
                        return $"Sentence {Pause.Span.LeftSentenceId} [{Pause.Span.Class}]";
                    }

                    if (Pause.Span.LeftSentenceId >= 0 && Pause.Span.RightSentenceId >= 0 &&
                        Pause.Span.LeftSentenceId != Pause.Span.RightSentenceId)
                    {
                        return $"Sent {Pause.Span.LeftSentenceId}->{Pause.Span.RightSentenceId} [{Pause.Span.Class}]";
                    }

                    return Pause.Span.Class.ToString();
                }
            }
        }

        public sealed record CompressionControlsSnapshot(
            IReadOnlyList<CompressionControlDisplay> Controls,
            int SelectedIndex)
        {
            public static CompressionControlsSnapshot Empty { get; } =
                new CompressionControlsSnapshot(Array.Empty<CompressionControlDisplay>(), 0);
        }

        public sealed record CompressionControlDisplay(string Label, string Value);

        public string BuildManuscriptMarkup(ScopeEntry entry)
        {
            var sb = new StringBuilder();

            switch (entry.Kind)
            {
                case ScopeEntryKind.Chapter:
                    AppendChapterPreview(sb);
                    break;
                case ScopeEntryKind.Paragraph when entry.ParagraphId.HasValue:
                    AppendParagraphMarkup(sb, entry.ParagraphId.Value, null, null);
                    break;
                case ScopeEntryKind.Sentence when entry.ParagraphId.HasValue && entry.SentenceId.HasValue:
                    AppendParagraphMarkup(sb, entry.ParagraphId.Value, entry.SentenceId.Value, null);
                    break;
                case ScopeEntryKind.Pause when entry.Pause is not null:
                    var pause = entry.Pause;
                    int? leftParagraph = pause.LeftParagraphId;
                    int? rightParagraph = pause.RightParagraphId;
                    int? highlight = pause.Span.LeftSentenceId >= 0 ? pause.Span.LeftSentenceId : null;
                    int? partner = pause.Span.RightSentenceId >= 0 ? pause.Span.RightSentenceId : null;

                    if (leftParagraph.HasValue)
                    {
                        AppendParagraphMarkup(sb, leftParagraph.Value, highlight, partner);
                    }

                    if (rightParagraph.HasValue && rightParagraph != leftParagraph)
                    {
                        if (sb.Length > 0)
                        {
                            sb.AppendLine().AppendLine();
                        }

                        AppendParagraphMarkup(sb, rightParagraph.Value, partner, highlight);
                    }

                    if (!leftParagraph.HasValue && !rightParagraph.HasValue)
                    {
                        AppendPauseSentencesFallback(sb, pause);
                    }

                    break;
            }

            if (sb.Length == 0)
            {
                sb.Append("[grey]No manuscript text available for this scope.[/]");
            }

            return sb.ToString().TrimEnd();
        }

        private void PopulatePauseLookups()
        {
            foreach (var span in _analysis.Spans)
            {
                var pause = CreateEditablePause(span);

                if (pause.IsCrossParagraph)
                {
                    _chapterPauses.Add(pause);
                    continue;
                }

                int key = pause.Span.LeftSentenceId >= 0 ? pause.Span.LeftSentenceId : pause.Span.RightSentenceId;
                if (key < 0)
                {
                    _chapterPauses.Add(pause);
                    continue;
                }

                if (!_sentencePauses.TryGetValue(key, out var list))
                {
                    list = new List<EditablePause>();
                    _sentencePauses[key] = list;
                }

                list.Add(pause);
            }

            foreach (var list in _sentencePauses.Values)
            {
                list.Sort(static (a, b) => a.Span.StartSec.CompareTo(b.Span.StartSec));
            }

            _chapterPauses.Sort(static (a, b) => a.Span.StartSec.CompareTo(b.Span.StartSec));
        }

        private static Dictionary<int, SentenceTiming> BuildBaselineTimeline(ChapterPauseMap chapter)
        {
            var timeline = new Dictionary<int, SentenceTiming>();

            foreach (var paragraph in chapter.Paragraphs)
            {
                foreach (var sentence in paragraph.Sentences)
                {
                    var timing = sentence.OriginalTiming;
                    timeline[sentence.SentenceId] = new SentenceTiming(
                        timing.StartSec,
                        timing.EndSec,
                        timing.FragmentBacked,
                        timing.Confidence);
                }
            }

            return timeline;
        }

        private EditablePause CreateEditablePause(PauseSpan span)
        {
            string leftText = _sentenceLookup.TryGetValue(span.LeftSentenceId, out var left) ? left : string.Empty;
            string rightText =
                span.RightSentenceId >= 0 && _sentenceLookup.TryGetValue(span.RightSentenceId, out var right)
                    ? right
                    : string.Empty;
            int? leftParagraph = _sentenceToParagraph.TryGetValue(span.LeftSentenceId, out var lp) ? lp : null;
            int? rightParagraph = _sentenceToParagraph.TryGetValue(span.RightSentenceId, out var rp) ? rp : null;
            return new EditablePause(span, leftText, rightText, leftParagraph, rightParagraph);
        }

        private List<ScopeEntry> BuildEntries()
        {
            var entries = new List<ScopeEntry>();
            if (_chapter.Paragraphs.Count == 0 && _chapterPauses.Count == 0)
            {
                return entries;
            }

            entries.Add(new ScopeEntry(
                ScopeEntryKind.Chapter,
                Depth: 0,
                Label: Markup.Escape("Chapter"),
                Stats: _chapter.Stats,
                ParagraphId: null,
                SentenceId: null,
                Pause: null,
                Start: _chapter.OriginalStart,
                End: _chapter.OriginalEnd));

            var topLevel = new List<TopLevelItem>(_chapter.Paragraphs.Count + _chapterPauses.Count);
            foreach (var paragraph in _chapter.Paragraphs)
            {
                topLevel.Add(new TopLevelItem(paragraph.OriginalStart, paragraph, null));
            }

            foreach (var pause in _chapterPauses)
            {
                topLevel.Add(new TopLevelItem(pause.Span.StartSec, null, pause));
            }

            topLevel.Sort(static (a, b) => a.Start.CompareTo(b.Start));

            foreach (var item in topLevel)
            {
                if (item.Paragraph is not null)
                {
                    AppendParagraph(entries, item.Paragraph);
                }
                else if (item.Pause is not null)
                {
                    AppendChapterPause(entries, item.Pause);
                }
            }

            return entries;
        }

        private void AppendParagraph(List<ScopeEntry> entries, ParagraphPauseMap paragraph)
        {
            var info = GetParagraphInfo(paragraph.ParagraphId);
            var label = BuildParagraphLabel(paragraph, info);

            entries.Add(new ScopeEntry(
                ScopeEntryKind.Paragraph,
                Depth: 1,
                label,
                paragraph.Stats,
                paragraph.ParagraphId,
                SentenceId: null,
                Pause: null,
                Start: paragraph.OriginalStart,
                End: paragraph.OriginalEnd));

            var orderedSentences = paragraph.Sentences
                .OrderBy(sentence => sentence.OriginalTiming.StartSec)
                .ToList();

            foreach (var sentence in orderedSentences)
            {
                AppendSentence(entries, paragraph.ParagraphId, sentence);
            }
        }

        private void AppendSentence(List<ScopeEntry> entries, int paragraphId, SentencePauseMap sentence)
        {
            var label = BuildSentenceLabel(sentence);
            entries.Add(new ScopeEntry(
                ScopeEntryKind.Sentence,
                Depth: 2,
                label,
                sentence.Stats,
                paragraphId,
                sentence.SentenceId,
                Pause: null,
                Start: sentence.OriginalTiming.StartSec,
                End: sentence.OriginalTiming.EndSec));

            if (_sentencePauses.TryGetValue(sentence.SentenceId, out var pauses))
            {
                foreach (var pause in pauses)
                {
                    entries.Add(new ScopeEntry(
                        ScopeEntryKind.Pause,
                        Depth: 3,
                        BuildPauseLabel(pause),
                        Stats: null,
                        ParagraphId: paragraphId,
                        SentenceId: sentence.SentenceId,
                        Pause: pause,
                        Start: pause.Span.StartSec,
                        End: pause.Span.EndSec));
                }
            }
        }

        private void AppendChapterPause(List<ScopeEntry> entries, EditablePause pause)
        {
            entries.Add(new ScopeEntry(
                ScopeEntryKind.Pause,
                Depth: 1,
                BuildPauseLabel(pause),
                Stats: null,
                ParagraphId: pause.LeftParagraphId ?? pause.RightParagraphId,
                SentenceId: null,
                Pause: pause,
                Start: pause.Span.StartSec,
                End: pause.Span.EndSec));
        }

        private string BuildParagraphLabel(ParagraphPauseMap paragraph, ParagraphInfo? info)
        {
            var builder = new StringBuilder();
            builder.Append("Paragraph ").Append(paragraph.ParagraphId);
            if (info is not null && !string.IsNullOrWhiteSpace(info.Kind))
            {
                builder.Append(" [").Append(info.Kind).Append(']');
            }

            builder.Append(" (").Append(GetParagraphSentenceCount(paragraph.ParagraphId)).Append(" sentences)");
            return Markup.Escape(builder.ToString());
        }

        private string BuildSentenceLabel(SentencePauseMap sentence)
        {
            return Markup.Escape($"Sentence {sentence.SentenceId}");
        }

        private string BuildPauseLabel(EditablePause pause)
        {
            var builder = new StringBuilder();
            builder.Append("Pause [").Append(pause.Span.Class).Append('|').Append(pause.Span.Provenance).Append(']');
            builder.Append(' ').Append(pause.BaselineDurationSec.ToString("0.000")).Append('s');
            if (pause.HasChanges)
            {
                builder.Append(" → ").Append(pause.AdjustedDurationSec.ToString("0.000")).Append('s');
            }

            return Markup.Escape(builder.ToString());
        }

        private static string TrimAndEscape(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = text.ReplaceLineEndings(" ").Trim();
            if (normalized.Length > maxLength)
            {
                normalized = normalized[..Math.Max(0, maxLength - 1)].TrimEnd() + "…";
            }

            return Markup.Escape(normalized);
        }

        private void AppendChapterPreview(StringBuilder sb)
        {
            if (_chapter.Paragraphs.Count == 0)
            {
                sb.Append("[grey]Chapter contains no paragraphs.[/]");
                return;
            }

            int rendered = 0;
            foreach (var paragraph in _chapter.Paragraphs.OrderBy(p => p.OriginalStart))
            {
                if (rendered > 0)
                {
                    sb.AppendLine().AppendLine();
                }

                AppendParagraphMarkup(sb, paragraph.ParagraphId, null, null);
                rendered++;
                if (rendered >= 3)
                {
                    break;
                }
            }
        }

        private void AppendParagraphMarkup(StringBuilder sb, int paragraphId, int? highlightSentenceId,
            int? partnerSentenceId)
        {
            if (!_paragraphLookup.TryGetValue(paragraphId, out var info))
            {
                if (highlightSentenceId.HasValue)
                {
                    AppendSentenceFallback(sb, highlightSentenceId.Value, partnerSentenceId);
                }

            }
            else
            {
                var titleBuilder = new StringBuilder().Append("Paragraph ").Append(paragraphId);
                if (!string.IsNullOrWhiteSpace(info.Kind))
                {
                    titleBuilder.Append(" [").Append(info.Kind).Append(']');
                }

                sb.Append("[bold steelblue1]").Append(Markup.Escape(titleBuilder.ToString())).AppendLine("[/]");
                sb.AppendLine();

                var sentenceIds = GetParagraphSentenceIds(paragraphId);
                if (sentenceIds.Count == 0)
                {
                    if (!string.IsNullOrWhiteSpace(info.Text))
                    {
                        sb.Append(Markup.Escape(info.Text));
                    }
                }
                else
                {
                    foreach (var sentenceId in sentenceIds)
                    {
                        var text = GetSentenceText(sentenceId);
                        var markup = Markup.Escape(string.IsNullOrWhiteSpace(text) ? "<no text>" : text);

                        if (highlightSentenceId.HasValue && sentenceId == highlightSentenceId.Value)
                        {
                            markup = $"[bold yellow]{markup}[/]";
                        }
                        else if (partnerSentenceId.HasValue && sentenceId == partnerSentenceId.Value)
                        {
                            markup = $"[bold lightseagreen]{markup}[/]";
                        }

                        sb.Append("• ").Append(markup).AppendLine();
                        sb.AppendLine();
                    }
                }
            }
        }

        private void AppendSentenceFallback(StringBuilder sb, int sentenceId, int? neighborSentenceId)
        {
            var text = GetSentenceText(sentenceId);
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.Append("[bold steelblue1]Sentence[/]").AppendLine();
                sb.Append(Markup.Escape(text)).AppendLine().AppendLine();
            }

            if (neighborSentenceId.HasValue)
            {
                var neighbor = GetSentenceText(neighborSentenceId.Value);
                if (!string.IsNullOrWhiteSpace(neighbor))
                {
                    sb.Append("[bold steelblue1]Neighbor[/]").AppendLine();
                    sb.Append(Markup.Escape(neighbor)).AppendLine();
                }
            }
        }

        private void AppendPauseSentencesFallback(StringBuilder sb, EditablePause pause)
        {
            AppendSentenceFallback(sb, pause.Span.LeftSentenceId,
                pause.Span.RightSentenceId >= 0 ? pause.Span.RightSentenceId : null);
        }

        private sealed record TopLevelItem(double Start, ParagraphPauseMap? Paragraph, EditablePause? Pause);

        private static bool MatchesCommittedPause(PauseAdjust adjust, PauseSpan span)
        {
            return adjust.LeftSentenceId == span.LeftSentenceId
                   && adjust.RightSentenceId == span.RightSentenceId
                   && Math.Abs(adjust.StartSec - span.StartSec) <= DurationEpsilon
                   && Math.Abs(adjust.EndSec - span.EndSec) <= DurationEpsilon;
        }
    }

    private sealed class TimingRenderer : IDisposable
    {
        private readonly InteractiveState _state;
        private readonly PauseAnalysisReport? _analysisSummary;
        private readonly PausePolicy _policy;

        public TimingRenderer(InteractiveState state, PauseAnalysisReport? analysisSummary, PausePolicy policy)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _analysisSummary = analysisSummary;
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public void Render()
        {
            if (AnsiConsole.Profile.Capabilities.Ansi)
            {
                AnsiConsole.Cursor.SetPosition(0, 0);
            }

            var layout = BuildLayout();
            AnsiConsole.Write(layout);
        }

        private Layout BuildLayout()
        {
            var header = new Layout("header") { Size = 3 };
            header.Update(new Rows(new IRenderable[]
            {
                new Markup(
                    "[bold dodgerblue1]Timing session[/] 🕒  [grey]Esc=exit  ↑/↓=next peer  Ctrl+↑/↓=level  ←/→=±5ms (pause)  Shift+←/→=±10ms  Enter=set duration[/]"),
                new Markup(string.Empty)
            }));

            if (_state.Entries.Count == 0)
            {
                var rootEmpty = new Layout("root").SplitRows(header);
                header.Update(new Rows(new IRenderable[]
                {
                    new Markup(
                        "[bold dodgerblue1]Timing session[/] 🕒  [grey]Esc=exit  ↑/↓=next peer  Ctrl+↑/↓=level  ←/→=±5ms (pause)  Shift+←/→=±10ms  Enter=set duration[/]"),
                    new Markup("[yellow]No pause spans available for this chapter.[/]")
                }));
                return rootEmpty;
            }

            int totalWidth;
            try
            {
                totalWidth = Console.WindowWidth;
            }
            catch
            {
                totalWidth = 180;
            }

            if (totalWidth <= 0)
            {
                totalWidth = 180;
            }

            int columnWidth = Math.Max(12, totalWidth / 12);
            int treeWidth = columnWidth * 3;
            int detailWidth = columnWidth * 3;
            int manuscriptWidth = Math.Max(columnWidth * 6, totalWidth - treeWidth - detailWidth);

            int viewportHeight;
            try
            {
                viewportHeight = Math.Max(10, Console.WindowHeight - 6);
            }
            catch
            {
                viewportHeight = 32;
            }

            var body = new Layout("body")
                .SplitColumns(
                    new Layout("tree") { Size = treeWidth },
                    new Layout("detail") { Size = detailWidth },
                    new Layout("manuscript") { Size = manuscriptWidth });

            body["tree"].Update(BuildTree(viewportHeight));

            var detailRoot = new Layout("detail-root")
                .SplitRows(
                    new Layout("detail-analytics"),
                    new Layout("detail-options") { Size = Math.Max(6, viewportHeight / 3) });

            detailRoot["detail-analytics"].Update(BuildDetailAnalytics());
            detailRoot["detail-options"].Update(BuildOptionsPanel());

            body["detail"].Update(detailRoot);
            body["manuscript"].Update(BuildManuscript());

            var root = new Layout("root")
                .SplitRows(header, body);

            return root;
        }

        private static void SoftClearViewport()
        {
            if (!AnsiConsole.Profile.Capabilities.Ansi)
            {
                AnsiConsole.Clear();
                return;
            }

            int width;
            int height;
            try
            {
                width = Math.Max(0, Console.WindowWidth);
                height = Math.Max(0, Console.WindowHeight);
            }
            catch
            {
                width = 180;
                height = 60;
            }

            if (width == 0 || height == 0)
            {
                return;
            }

            string blank = new string(' ', width);
            for (int row = 0; row < height; row++)
            {
                AnsiConsole.Cursor.SetPosition(0, row);
                System.Console.Write(blank);
            }

            AnsiConsole.Cursor.SetPosition(0, 0);
        }

        private IRenderable BuildTree(int viewportHeight)
        {
            _state.SetTreeViewportSize(viewportHeight);

            var entries = _state.GetTreeViewportEntries(out bool hasPrevious, out bool hasNext);

            var table = new Table
            {
                Border = TableBorder.None,
                ShowHeaders = false,
                Expand = true
            };
            table.AddColumn(new TableColumn("Scope") { NoWrap = false });
            table.AddColumn(new TableColumn("Summary").RightAligned());

            if (hasPrevious)
            {
                table.AddRow("[grey]…[/]", string.Empty);
            }

            foreach (var entry in entries)
            {
                table.AddRow(FormatTreeLabel(entry), BuildTreeSummary(entry));
            }

            if (hasNext)
            {
                table.AddRow("[grey]…[/]", string.Empty);
            }

            return new Panel(table)
            {
                Header = new PanelHeader("Scopes", Justify.Left),
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 0, 1, 0),
                Expand = true
            };
        }

        private string FormatTreeLabel(ScopeEntry entry)
        {
            var indent = new string(' ', entry.Depth * 2);
            var label = indent + entry.Label;
            if (entry.Equals(_state.Current))
            {
                label = $"[bold dodgerblue1]{label}[/]";
            }

            return label;
        }

        private string BuildTreeSummary(ScopeEntry entry)
        {
            string summary = entry.Kind switch
            {
                ScopeEntryKind.Chapter =>
                    $"Par {_state.ParagraphCount}  Sent {_state.TotalSentenceCount}  Pauses {_state.TotalPauseCount}",
                ScopeEntryKind.Paragraph when entry.ParagraphId.HasValue =>
                    $"Sent {_state.GetParagraphSentenceCount(entry.ParagraphId.Value)}  Pauses {_state.CountParagraphPauses(entry.ParagraphId.Value)}",
                ScopeEntryKind.Sentence when entry.SentenceId.HasValue =>
                    $"Pauses {_state.CountSentencePauses(entry.SentenceId.Value)}",
                ScopeEntryKind.Pause when entry.Pause is not null => $"{entry.Pause.Span.DurationSec:0.000}s",
                _ => string.Empty
            };

            return Markup.Escape(summary);
        }

        private IRenderable BuildDetailAnalytics()
        {
            return _state.Current.Kind switch
            {
                ScopeEntryKind.Chapter => BuildChapterDetail(),
                ScopeEntryKind.Paragraph => BuildParagraphDetail(),
                ScopeEntryKind.Sentence => BuildSentenceDetail(),
                ScopeEntryKind.Pause => BuildPauseDetail(),
                _ => new Markup("[grey]No details available.[/]")
            };
        }

        private IRenderable BuildChapterDetail()
        {
            var items = new List<IRenderable>();

            if (_state.Current.Stats is not null)
            {
                items.Add(WrapInPanel(CreateStatsTable(_state.Current.Stats),
                    $"Chapter Pauses ({_state.TotalPauseCount})"));
            }

            if (_analysisSummary is not null && _analysisSummary.Spans.Count > 0)
            {
                items.Add(WrapInPanel(CreateClassTable(_analysisSummary), "Observed Durations"));
            }

            var diffs = _state.GetPendingAdjustments(_state.Current);
            if (diffs.Count > 0)
            {
                items.Add(WrapInPanel(BuildDiffTable(diffs), "Pending Adjustments"));
            }

            if (items.Count == 0)
            {
                return new Markup("[grey]No pause statistics for this chapter.[/]");
            }

            return items.Count == 1 ? items[0] : new Rows(items);
        }

        private IRenderable BuildParagraphDetail()
        {
            var current = _state.Current;
            if (!current.ParagraphId.HasValue)
            {
                return new Markup("[grey]Paragraph metadata unavailable.[/]");
            }

            var items = new List<IRenderable>();

            if (current.Stats is not null)
            {
                items.Add(WrapInPanel(CreateStatsTable(current.Stats), $"Paragraph {current.ParagraphId}"));
            }

            var info = _state.GetParagraphInfo(current.ParagraphId.Value);
            if (info is not null && (!string.IsNullOrWhiteSpace(info.Kind) || !string.IsNullOrWhiteSpace(info.Style)))
            {
                var table = new Table
                {
                    Border = TableBorder.None,
                    ShowHeaders = false,
                    Expand = true
                };
                table.AddColumn(new TableColumn("Key") { NoWrap = true });
                table.AddColumn(new TableColumn("Value"));

                if (!string.IsNullOrWhiteSpace(info.Kind))
                {
                    table.AddRow("Kind", Markup.Escape(info.Kind));
                }

                if (!string.IsNullOrWhiteSpace(info.Style))
                {
                    table.AddRow("Style", Markup.Escape(info.Style));
                }

                items.Add(WrapInPanel(table, "Metadata"));
            }

            var diffs = _state.GetPendingAdjustments(current);
            if (diffs.Count > 0)
            {
                items.Add(WrapInPanel(BuildDiffTable(diffs), "Pending Adjustments"));
            }

            if (items.Count == 0)
            {
                return new Markup("[grey]No statistics for this paragraph.[/]");
            }

            return items.Count == 1 ? items[0] : new Rows(items);
        }

        private IRenderable BuildSentenceDetail()
        {
            var current = _state.Current;
            if (current.Stats is null)
            {
                return new Markup("[grey]No statistics for this sentence.[/]");
            }

            var items = new List<IRenderable>
            {
                WrapInPanel(CreateStatsTable(current.Stats), $"Sentence {current.SentenceId}")
            };

            var diffs = _state.GetPendingAdjustments(current);
            if (diffs.Count > 0)
            {
                items.Add(WrapInPanel(BuildDiffTable(diffs), "Pending Adjustments"));
            }

            return items.Count == 1 ? items[0] : new Rows(items);
        }

        private IRenderable BuildOptionsPanel()
        {
            var current = _state.Current;
            int currentPending = _state.GetPendingPauseCount(current);
            var chapterEntry = _state.GetChapterEntry();
            int chapterPending = chapterEntry is not null ? _state.GetPendingPauseCount(chapterEntry) : currentPending;
            string focusStatus = _state.OptionsFocused ? "[bold green]Options[/]" : "[grey]Tree[/]";
            string commitStatus = _state.LastCommitMessage is not null
                ? Markup.Escape(_state.LastCommitMessage)
                : "[grey]No commits yet.[/]";

            var summaryTable = new Table
            {
                Border = TableBorder.None,
                ShowHeaders = false,
                Expand = true
            };
            summaryTable.AddColumn(new TableColumn("Key") { NoWrap = true });
            summaryTable.AddColumn(new TableColumn("Value"));

            summaryTable.AddRow("Focus", focusStatus);
            summaryTable.AddRow("Pending (scope)", currentPending.ToString());
            summaryTable.AddRow("Pending (chapter)", chapterPending.ToString());
            summaryTable.AddRow("Last commit", commitStatus);

            string keysHint = _state.OptionsFocused
                ? "Up/Down=select knob  ←/→=adjust  Ctrl+↑/↓=scroll preview  Enter=commit  Space=exit"
                : "Space=open options  ←/→=±5ms (pause)  Shift+←/→=±10ms";
            summaryTable.AddRow("Keys", keysHint);

            var content = new List<IRenderable> { summaryTable };

            if (_state.OptionsFocused)
            {
                var controlsSnapshot = _state.GetCompressionControlsSnapshot();
                if (controlsSnapshot.Controls.Count > 0)
                {
                    var controlsTable = new Table
                    {
                        Border = TableBorder.None,
                        ShowHeaders = false,
                        Expand = true
                    };
                    controlsTable.AddColumn(new TableColumn("Knob") { NoWrap = true });
                    controlsTable.AddColumn(new TableColumn("Value") { Alignment = Justify.Right });

                    for (int i = 0; i < controlsSnapshot.Controls.Count; i++)
                    {
                        var display = controlsSnapshot.Controls[i];
                        bool isSelected = i == controlsSnapshot.SelectedIndex;
                        string label = isSelected ? $"[bold dodgerblue1]> {display.Label}[/]" : $"  {display.Label}";
                        string value = isSelected ? $"[bold dodgerblue1]{display.Value}[/]" : display.Value;
                        controlsTable.AddRow(label, value);
                    }

                    content.Add(controlsTable);
                }

                const int PreviewRows = 8;
                var previewItems = _state.GetCompressionPreview(PreviewRows, out bool hasPrev, out bool hasNext);
                var previewTable = new Table
                {
                    Border = TableBorder.None,
                    ShowHeaders = true,
                    Expand = true
                };
                previewTable.AddColumn(new TableColumn("Pause"));
                previewTable.AddColumn(new TableColumn("Original (s)") { Alignment = Justify.Right });
                previewTable.AddColumn(new TableColumn("Target (s)") { Alignment = Justify.Right });
                previewTable.AddColumn(new TableColumn("Δ (s)") { Alignment = Justify.Right });

                if (previewItems.Count == 0)
                {
                    previewTable.AddRow("[grey]No pauses available for compression in this scope.[/]", "-", "-", "-");
                }
                else
                {
                    if (hasPrev)
                    {
                        previewTable.AddRow("[grey]…[/]", string.Empty, string.Empty, string.Empty);
                    }

                    foreach (var item in previewItems)
                    {
                        string deltaText = item.Delta >= 0
                            ? $"[green]+{item.Delta:0.000}[/]"
                            : $"[red]{item.Delta:0.000}[/]";

                        previewTable.AddRow(
                            Markup.Escape(item.Label),
                            item.OriginalDuration.ToString("0.000"),
                            item.TargetDuration.ToString("0.000"),
                            deltaText);
                    }

                    if (hasNext)
                    {
                        previewTable.AddRow("[grey]…[/]", string.Empty, string.Empty, string.Empty);
                    }
                }

                content.Add(previewTable);
            }
            else
            {
                content.Add(new Markup("[grey]Press Space to open timing options for the current scope.[/]"));
            }

            var panel = new Panel(new Rows(content))
            {
                Header = new PanelHeader("Timing Options", Justify.Left),
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 0, 1, 0),
                Expand = true
            };

            if (_state.OptionsFocused)
            {
                panel.BorderStyle = new Style(Color.DeepSkyBlue1, decoration: Decoration.Bold);
            }

            return panel;
        }

        private IRenderable BuildPauseDetail()
        {
            var pause = _state.Current.Pause!;
            var table = new Table
            {
                Border = TableBorder.None,
                ShowHeaders = false,
                Expand = true
            };
            table.AddColumn(new TableColumn("Key") { NoWrap = true });
            table.AddColumn(new TableColumn("Value"));

            table.AddRow("Class", pause.Span.Class.ToString());
            table.AddRow("Original", $"{pause.Span.DurationSec:0.000} s");
            table.AddRow("Baseline", $"{pause.BaselineDurationSec:0.000} s");
            table.AddRow("Adjusted", $"{pause.AdjustedDurationSec:0.000} s");
            table.AddRow("Δ", $"{pause.Delta:+0.000;-0.000;0.000} s");
            table.AddRow("Start", $"{pause.Span.StartSec:0.000} s");
            table.AddRow("End", $"{pause.Span.EndSec:0.000} s");
            table.AddRow("Gap hint", pause.Span.HasGapHint ? "yes" : "no");
            table.AddRow("Policy", DescribePolicyWindow(pause.Span.Class));
            table.AddRow("Context", _state.DescribePauseContext(pause));
            table.AddRow("Pending", pause.HasChanges ? "[yellow]yes[/]" : "[grey]no[/]");

            return WrapInPanel(table, "Pause Detail");
        }

        private IRenderable BuildManuscript()
        {
            var markup = new Markup(_state.BuildManuscriptMarkup(_state.Current));
            return new Panel(markup)
            {
                Header = new PanelHeader("Manuscript", Justify.Left),
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 0, 1, 0),
                Expand = true
            };
        }

        private static Panel WrapInPanel(IRenderable content, string title)
        {
            return new Panel(content)
            {
                Header = new PanelHeader(title, Justify.Left),
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 0, 1, 0),
                Expand = true
            };
        }

        private static Table CreateStatsTable(PauseStatsSet stats)
        {
            var table = new Table
            {
                Border = TableBorder.Rounded,
                Expand = true
            };
            table.AddColumn("Class");
            table.AddColumn("Count");
            table.AddColumn("Min (s)");
            table.AddColumn("Median (s)");
            table.AddColumn("Max (s)");
            table.AddColumn("Mean (s)");

            bool hasRows = false;
            foreach (var (pauseClass, pauseStats) in EnumerateStats(stats))
            {
                if (pauseStats.Count == 0)
                {
                    continue;
                }

                table.AddRow(
                    pauseClass.ToString(),
                    pauseStats.Count.ToString(),
                    pauseStats.Min.ToString("0.000"),
                    pauseStats.Median.ToString("0.000"),
                    pauseStats.Max.ToString("0.000"),
                    pauseStats.Mean.ToString("0.000"));
                hasRows = true;
            }

            if (!hasRows)
            {
                table.AddRow("[grey]–[/]", "0", "-", "-", "-", "-");
            }

            return table;
        }

        private static IEnumerable<(PauseClass Class, PauseStats Stats)> EnumerateStats(PauseStatsSet stats)
        {
            yield return (PauseClass.Comma, stats.Comma);
            yield return (PauseClass.Sentence, stats.Sentence);
            yield return (PauseClass.Paragraph, stats.Paragraph);
            yield return (PauseClass.ChapterHead, stats.ChapterHead);
            yield return (PauseClass.PostChapterRead, stats.PostChapterRead);
            yield return (PauseClass.Tail, stats.Tail);
            yield return (PauseClass.Other, stats.Other);
        }

        private static IRenderable CreateClassTable(PauseAnalysisReport report)
        {
            var table = new Table
            {
                Border = TableBorder.Rounded,
                Expand = true
            };
            table.AddColumn("Class");
            table.AddColumn("Count");
            table.AddColumn("Min (s)");
            table.AddColumn("Median (s)");
            table.AddColumn("Max (s)");
            table.AddColumn("Mean (s)");

            bool hasRows = false;
            foreach (var kvp in report.Classes.OrderBy(kvp => kvp.Key.ToString()))
            {
                var stats = kvp.Value;
                table.AddRow(
                    kvp.Key.ToString(),
                    stats.Count.ToString(),
                    stats.Minimum.ToString("0.000"),
                    stats.Median.ToString("0.000"),
                    stats.Maximum.ToString("0.000"),
                    stats.Mean.ToString("0.000"));
                hasRows = true;
            }

            if (!hasRows)
            {
                table.AddRow("[grey]–[/]", "0", "-", "-", "-", "-");
            }

            return table;
        }

        private string DescribePolicyWindow(PauseClass pauseClass)
        {
            return pauseClass switch
            {
                PauseClass.Comma => $"{_policy.Comma.Min:0.000}-{_policy.Comma.Max:0.000}s",
                PauseClass.Sentence => $"{_policy.Sentence.Min:0.000}-{_policy.Sentence.Max:0.000}s",
                PauseClass.Paragraph => $"{_policy.Paragraph.Min:0.000}-{_policy.Paragraph.Max:0.000}s",
                PauseClass.ChapterHead => $"{_policy.HeadOfChapter:0.000}s target",
                PauseClass.PostChapterRead => $"{_policy.PostChapterRead:0.000}s target",
                PauseClass.Tail => $"{_policy.Tail:0.000}s target",
                _ => "n/a"
            };
        }

        private static Table BuildDiffTable(IReadOnlyList<DiffRow> diffs)
        {
            var table = new Table
            {
                Border = TableBorder.Rounded,
                Expand = true
            };
            table.AddColumn("Pause");
            table.AddColumn("Original (s)");
            table.AddColumn("Adjusted (s)");
            table.AddColumn("Δ (s)");
            table.AddColumn("Context");

            if (diffs.Count == 0)
            {
                table.AddRow("[grey]–[/]", "-", "-", "-", "[grey]No pending adjustments[/]");
                return table;
            }

            foreach (var diff in diffs)
            {
                table.AddRow(
                    diff.Class.ToString(),
                    diff.Original.ToString("0.000"),
                    diff.Adjusted.ToString("0.000"),
                    diff.Delta.ToString("+0.000;-0.000;0.000"),
                    diff.Context);
            }

            return table;
        }

        public void Dispose()
        {
        }
    }

    private sealed class TimingController
    {
        private readonly InteractiveState _state;
        private readonly TimingRenderer _renderer;
        private readonly Action<CommitResult> _onCommit;

        public TimingController(InteractiveState state, TimingRenderer renderer, Action<CommitResult> onCommit)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _onCommit = onCommit ?? (_ => { });
        }

        public void Run()
        {
            try
            {
                _renderer.Render();

                while (true)
                {
                    var key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        break;
                    }

                    bool updated = key.Key switch
                    {
                        ConsoleKey.UpArrow or ConsoleKey.K when _state.OptionsFocused &&
                                                                (key.Modifiers & ConsoleModifiers.Control) != 0 =>
                            _state.ScrollCompressionPreview(-5),
                        ConsoleKey.DownArrow or ConsoleKey.J when _state.OptionsFocused &&
                                                                  (key.Modifiers & ConsoleModifiers.Control) != 0 =>
                            _state.ScrollCompressionPreview(+5),
                        ConsoleKey.UpArrow or ConsoleKey.K when _state.OptionsFocused => _state
                            .MoveCompressionControlSelection(-1),
                        ConsoleKey.DownArrow or ConsoleKey.J when _state.OptionsFocused => _state
                            .MoveCompressionControlSelection(+1),
                        ConsoleKey.LeftArrow or ConsoleKey.H when _state.OptionsFocused => _state
                            .AdjustCompressionControl(-1, key.Modifiers),
                        ConsoleKey.RightArrow or ConsoleKey.L when _state.OptionsFocused => _state
                            .AdjustCompressionControl(+1, key.Modifiers),
                        ConsoleKey.UpArrow or ConsoleKey.K when !_state.OptionsFocused &&
                                                                (key.Modifiers & ConsoleModifiers.Control) != 0 =>
                            _state.StepOut(),
                        ConsoleKey.DownArrow or ConsoleKey.J when !_state.OptionsFocused &&
                                                                  (key.Modifiers & ConsoleModifiers.Control) != 0 =>
                            _state.StepInto(),
                        ConsoleKey.UpArrow or ConsoleKey.K when !_state.OptionsFocused => _state.MoveWithinTier(-1),
                        ConsoleKey.DownArrow or ConsoleKey.J when !_state.OptionsFocused => _state.MoveWithinTier(+1),
                        ConsoleKey.LeftArrow or ConsoleKey.H when !_state.OptionsFocused => AdjustCurrent(-1,
                            key.Modifiers),
                        ConsoleKey.RightArrow or ConsoleKey.L when !_state.OptionsFocused => AdjustCurrent(+1,
                            key.Modifiers),
                        ConsoleKey.Spacebar => ToggleOptionsFocus(),
                        ConsoleKey.Enter => _state.OptionsFocused ? CommitCurrentScope() : PromptForValue(),
                        _ => false
                    };

                    if (updated)
                    {
                        _renderer.Render();
                    }
                }
            }
            finally
            {
                _renderer.Dispose();
            }
        }

        private bool AdjustCurrent(int direction, ConsoleModifiers modifiers)
        {
            double step = (modifiers & ConsoleModifiers.Shift) != 0 ? 0.010 : 0.005;
            return _state.AdjustCurrent(step * direction);
        }

        private bool ToggleOptionsFocus()
        {
            _state.ToggleOptionsFocus();
            return true;
        }

        private bool CommitCurrentScope()
        {
            CompressionApplySummary summary = CompressionApplySummary.Empty;
            if (_state.OptionsFocused)
            {
                summary = _state.ApplyCompressionPreview();
            }

            var result = _state.CommitScope(_state.Current, summary.HasChanges ? summary : null);
            if (!result.HasChanges)
            {
                return false;
            }

            _onCommit(result);
            return true;
        }

        private bool PromptForValue()
        {
            if (_state.Current.Kind != ScopeEntryKind.Pause || _state.Current.Pause is null)
            {
                return false;
            }

            var current = _state.Current.Pause.AdjustedDurationSec;
            var prompt = new TextPrompt<double>("Set pause duration (seconds)")
                .DefaultValue(current)
                .ValidationErrorMessage("Duration must be non-negative.")
                .Validate(value => value >= 0.0);

            double newValue = AnsiConsole.Prompt(prompt);
            return _state.SetCurrent(newValue);
        }
    }

    private sealed record ParagraphInfo(int Index, string Kind, string Style, string Text);

    private sealed record ScopeEntry(
        ScopeEntryKind Kind,
        int Depth,
        string Label,
        PauseStatsSet? Stats,
        int? ParagraphId,
        int? SentenceId,
        EditablePause? Pause,
        double? Start,
        double? End)
    {
        public static ScopeEntry Empty { get; } = new ScopeEntry(ScopeEntryKind.Chapter, 0, Markup.Escape("No scope"),
            null, null, null, null, null, null);
    }

    private enum ScopeEntryKind
    {
        Chapter,
        Paragraph,
        Sentence,
        Pause
    }

    private sealed record DiffRow(
        PauseClass Class,
        double Original,
        double Adjusted,
        double Delta,
        string Context);

    private sealed record CommitResult(int Count, string ScopeLabel, IReadOnlyList<PauseAdjust> Adjustments)
    {
        public static CommitResult Empty { get; } = new CommitResult(0, string.Empty, Array.Empty<PauseAdjust>());
        public bool HasChanges => Count > 0;
    }

    private sealed record CompressionApplySummary(int TotalCount, int WithinScopeCount, int DownstreamCount)
    {
        public static CompressionApplySummary Empty { get; } = new CompressionApplySummary(0, 0, 0);
        public bool HasChanges => TotalCount > 0;
    }

    private sealed class EditablePause
    {
        private const double DurationEpsilon = 1e-6;
        private double _baselineDurationSec;

        public EditablePause(PauseSpan span, string leftText, string rightText, int? leftParagraphId,
            int? rightParagraphId)
        {
            Span = span;
            LeftText = leftText;
            RightText = rightText;
            LeftParagraphId = leftParagraphId;
            RightParagraphId = rightParagraphId;
            _baselineDurationSec = span.DurationSec;
            AdjustedDurationSec = _baselineDurationSec;
        }

        public PauseSpan Span { get; }

        public string LeftText { get; }

        public string RightText { get; }

        public int? LeftParagraphId { get; }

        public int? RightParagraphId { get; }

        public double AdjustedDurationSec { get; private set; }

        public double BaselineDurationSec => _baselineDurationSec;

        public bool HasChanges => Math.Abs(AdjustedDurationSec - _baselineDurationSec) > DurationEpsilon;

        public bool IsIntraSentence => Span.LeftSentenceId >= 0 && Span.LeftSentenceId == Span.RightSentenceId;

        public bool IsCrossParagraph
        {
            get
            {
                if (Span.CrossesParagraph)
                {
                    return true;
                }

                if (LeftParagraphId.HasValue && RightParagraphId.HasValue &&
                    LeftParagraphId.Value != RightParagraphId.Value)
                {
                    return true;
                }

                return LeftParagraphId.HasValue ^ RightParagraphId.HasValue;
            }
        }

        public double Delta => AdjustedDurationSec - _baselineDurationSec;

        public void Adjust(double deltaSeconds)
        {
            AdjustedDurationSec = Math.Max(0d, AdjustedDurationSec + deltaSeconds);
        }

        public void Set(double newDuration)
        {
            AdjustedDurationSec = Math.Max(0d, newDuration);
        }

        public void Commit()
        {
            _baselineDurationSec = AdjustedDurationSec;
        }
    }
}
