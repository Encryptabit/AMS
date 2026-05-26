using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Common;
using Ams.Core.Processors;
using Ams.Core.Processors.Alignment.Anchors;
using Ams.Core.Runtime.Chapter;
using System.Text.RegularExpressions;

namespace Ams.Core.Audio;

/// <summary>
/// Service for treating chapter audio with roomtone padding.
/// </summary>
public sealed class AudioTreatmentService
{
    private const double MaxTitleBoundarySearchSeconds = 30.0;
    private static readonly Regex ChapterTitlePattern = new(
        @"^\s*(chapter\b.+?)\s*[:\-–—]\s*(.+?)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Result of audio treatment processing.
    /// </summary>
    public sealed record TreatmentResult(
        string OutputPath,
        double TitleStartSec,
        double TitleEndSec,
        double ContentStartSec,
        double ContentEndSec,
        double TotalDurationSec);

    /// <summary>
    /// Treats a chapter audio by assembling:
    /// [preroll roomtone] -> [title segment] -> [gap roomtone] -> [content segment] -> [postroll roomtone]
    /// Uses roomtone from the book's audio context.
    /// </summary>
    /// <param name="chapter">The chapter context containing the audio buffer.</param>
    /// <param name="outputPath">Path for the output treated.wav file.</param>
    /// <param name="options">Treatment options (timing durations, thresholds).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Treatment result with timing information.</returns>
    public async Task<TreatmentResult> TreatChapterAsync(
        ChapterContext chapter,
        string outputPath,
        TreatmentOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var roomtoneBuffer = chapter.Book.Audio.Roomtone
            ?? throw new InvalidOperationException(
                $"Roomtone not found at {chapter.Book.Audio.RoomtonePath}. " +
                "Create a roomtone.wav file in the book's safe/ directory.");

        return await TreatChapterCoreAsync(chapter, roomtoneBuffer, outputPath, options, cancellationToken);
    }

    /// <summary>
    /// Treats a chapter audio by assembling:
    /// [preroll roomtone] -> [title segment] -> [gap roomtone] -> [content segment] -> [postroll roomtone]
    /// Uses an explicit roomtone file path.
    /// </summary>
    /// <param name="chapter">The chapter context containing the audio buffer.</param>
    /// <param name="roomtonePath">Path to the roomtone.wav file.</param>
    /// <param name="outputPath">Path for the output treated.wav file.</param>
    /// <param name="options">Treatment options (timing durations, thresholds).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Treatment result with timing information.</returns>
    public async Task<TreatmentResult> TreatChapterAsync(
        ChapterContext chapter,
        string roomtonePath,
        string outputPath,
        TreatmentOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        ArgumentException.ThrowIfNullOrWhiteSpace(roomtonePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (!File.Exists(roomtonePath))
        {
            throw new FileNotFoundException($"Roomtone file not found: {roomtonePath}", roomtonePath);
        }

        var roomtoneBuffer = AudioProcessor.Decode(roomtonePath);
        return await TreatChapterCoreAsync(chapter, roomtoneBuffer, outputPath, options, cancellationToken);
    }

    private Task<TreatmentResult> TreatChapterCoreAsync(
        ChapterContext chapter,
        AudioBuffer roomtoneBuffer,
        string outputPath,
        TreatmentOptions? options,
        CancellationToken cancellationToken)
    {
        // CancellationToken check at entry point
        cancellationToken.ThrowIfCancellationRequested();

        var opts = options ?? new TreatmentOptions();

        // Get chapter audio from the AudioBufferManager
        var audioContext = chapter.Audio.Current;
        var chapterBuffer = audioContext.Buffer
            ?? throw new InvalidOperationException(
                $"Failed to load audio buffer for chapter '{chapter.Descriptor.ChapterId}'");

        // Detect silence intervals to find speech boundaries
        var silenceOpts = new SilenceDetectOptions
        {
            NoiseDb = opts.SilenceThresholdDb,
            MinimumDuration = TimeSpan.FromSeconds(opts.MinimumSilenceDuration)
        };
        var silenceIntervals = AudioProcessor.DetectSilence(chapterBuffer, silenceOpts);

        // Find title and content boundaries
        var sectionTitle = ResolveSectionTitle(chapter);
        var rawLayout = FindTreatmentLayout(
            chapterBuffer,
            silenceIntervals,
            opts.TitleContentGapThreshold,
            chapter.Documents.HydratedTranscript,
            sectionTitle,
            opts.ClickImmunityBurstSec);
        // Add crossfade duration on top of user padding so the splice blend lands in
        // silence and the user's --padding-ms value represents the actual safety margin
        // *after* the crossfade, preventing fricative tails from being faded out.
        var effectivePaddingSec = opts.BoundaryPaddingSeconds + opts.SpliceCrossfadeDurationSec;
        var (titleStart, titleEnd, contentStart, contentEnd, decoratorEnd, titleResumeStart) = ApplyLayoutPadding(
            rawLayout,
            chapterBuffer.Length / (double)chapterBuffer.SampleRate,
            effectivePaddingSec);

        Log.Debug(
            "Treatment layout: rawTitle={RawTitleStart:F3}s-{RawTitleEnd:F3}s, rawDecoratorEnd={RawDecoratorEnd}, rawTitleResume={RawTitleResume}, rawContent={RawContentStart:F3}s-{RawContentEnd:F3}s, title={TitleStart:F3}s-{TitleEnd:F3}s, decoratorEnd={DecoratorEnd}, titleResume={TitleResume}, content={ContentStart:F3}s-{ContentEnd:F3}s, padding={Padding:F3}s (effective={EffectivePadding:F3}s incl. {Crossfade:F3}s crossfade), sectionTitle={SectionTitle}",
            rawLayout.TitleStart,
            rawLayout.TitleEnd,
            rawLayout.DecoratorEnd?.ToString("F3") ?? "-",
            rawLayout.TitleResumeStart?.ToString("F3") ?? "-",
            rawLayout.ContentStart,
            rawLayout.ContentEnd,
            titleStart,
            titleEnd,
            decoratorEnd?.ToString("F3") ?? "-",
            titleResumeStart?.ToString("F3") ?? "-",
            contentStart,
            contentEnd,
            opts.BoundaryPaddingSeconds,
            effectivePaddingSec,
            opts.SpliceCrossfadeDurationSec,
            sectionTitle ?? "-");

        // Check if we have a separate title segment (titleStart >= 0 AND has positive duration)
        bool hasTitle = titleStart >= 0 && titleEnd > titleStart;

        // Build segments in-memory (no temp files)
        var segments = new List<AudioBuffer>();

        // Preroll (always present)
        var prerollBuffer = PrepareRoomtoneSegment(roomtoneBuffer, opts.PrerollSeconds);
        segments.Add(prerollBuffer);

        if (hasTitle)
        {
            if (decoratorEnd is double decoratorBoundary &&
                titleResumeStart is double titleBoundary &&
                decoratorBoundary > titleStart &&
                titleBoundary < titleEnd)
            {
                Log.Debug("Extracting chapter decorator segment: {Start:F3}s - {End:F3}s",
                    titleStart, decoratorBoundary);
                var decoratorBuffer = SliceChapterSegment(
                    chapterBuffer,
                    titleStart,
                    decoratorBoundary,
                    "chapter decorator");
                segments.Add(decoratorBuffer);

                var decoratorGapBuffer = PrepareRoomtoneSegment(roomtoneBuffer, opts.PrerollSeconds);
                segments.Add(decoratorGapBuffer);

                Log.Debug("Extracting chapter title segment: {Start:F3}s - {End:F3}s",
                    titleBoundary, titleEnd);
                var titleBuffer = SliceChapterSegment(
                    chapterBuffer,
                    titleBoundary,
                    titleEnd,
                    "chapter title");
                segments.Add(titleBuffer);
            }
            else
            {
                // Extract title segment
                Log.Debug("Extracting title segment: {Start:F3}s - {End:F3}s", titleStart, titleEnd);
                var titleBuffer = SliceChapterSegment(
                    chapterBuffer,
                    titleStart,
                    titleEnd,
                    "title");
                segments.Add(titleBuffer);
            }

            // Gap between title and content
            var gapBuffer = PrepareRoomtoneSegment(roomtoneBuffer, opts.ChapterToContentGapSeconds);
            segments.Add(gapBuffer);
        }

        // Validate content segment has positive duration
        if (contentEnd <= contentStart)
        {
            throw new InvalidOperationException(
                $"Content segment has zero or negative duration: {contentStart:F3}s - {contentEnd:F3}s");
        }

        // Extract content segment
        Log.Debug("Extracting content segment: {Start:F3}s - {End:F3}s", contentStart, contentEnd);
        var contentBuffer = SliceChapterSegment(
            chapterBuffer,
            contentStart,
            contentEnd,
            "content");
        segments.Add(contentBuffer);

        // Postroll (always present)
        var postrollBuffer = PrepareRoomtoneSegment(roomtoneBuffer, opts.PostrollSeconds);
        segments.Add(postrollBuffer);

        Log.Debug(
            "Assembling {SegmentCount} segment(s) via audio splice (crossfade={Crossfade:F3}s, curve={Curve})",
            segments.Count,
            opts.SpliceCrossfadeDurationSec,
            opts.SpliceCrossfadeCurve);

        // Assemble with FFmpeg-backed splicing so joins are robust and consistent.
        var treatedBuffer = AssembleSegmentsWithSplice(
            segments,
            opts.SpliceCrossfadeDurationSec,
            opts.SpliceCrossfadeCurve);

        // Single encode to disk
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        var encodeOptions = BuildEncodeOptions(chapterBuffer);
        Log.Debug(
            "Encoding treated output at {SampleRate}Hz, {BitDepth}-bit PCM",
            encodeOptions.TargetSampleRate ?? treatedBuffer.SampleRate,
            encodeOptions.TargetBitDepth ?? 16);
        AudioProcessor.EncodeWav(outputPath, treatedBuffer, encodeOptions);

        // Calculate total duration from the rendered buffer so logs stay accurate with crossfades.
        var totalDuration = treatedBuffer.Length / (double)treatedBuffer.SampleRate;

        return Task.FromResult(new TreatmentResult(
            outputPath,
            hasTitle ? titleStart : -1,
            hasTitle ? titleEnd : -1,
            contentStart,
            contentEnd,
            totalDuration));
    }

    private static AudioBuffer AssembleSegmentsWithSplice(
        IReadOnlyList<AudioBuffer> segments,
        double crossfadeDurationSec,
        string crossfadeCurve)
    {
        if (segments.Count == 0)
        {
            throw new InvalidOperationException("No audio segments were generated for treatment.");
        }

        if (crossfadeDurationSec < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(crossfadeDurationSec),
                crossfadeDurationSec,
                "Crossfade duration must be non-negative.");
        }

        var curve = string.IsNullOrWhiteSpace(crossfadeCurve) ? "tri" : crossfadeCurve;
        var assembled = segments[0];

        for (int i = 1; i < segments.Count; i++)
        {
            var appendAtSec = assembled.Length / (double)assembled.SampleRate;
            assembled = AudioSpliceService.InsertAtPoint(
                assembled,
                appendAtSec,
                segments[i],
                crossfadeDurationSec,
                curve);
        }

        return assembled;
    }

    private static AudioBuffer SliceChapterSegment(
        AudioBuffer chapterBuffer,
        double startSec,
        double endSec,
        string description)
    {
        if (!chapterBuffer.TrySliceClamped(
                TimeSpan.FromSeconds(startSec),
                TimeSpan.FromSeconds(endSec),
                out var segment))
        {
            throw new InvalidOperationException(
                $"Unable to extract {description} segment from chapter audio: {startSec:F3}s - {endSec:F3}s.");
        }

        return segment;
    }

    private static AudioEncodeOptions BuildEncodeOptions(AudioBuffer chapterBuffer)
    {
        ArgumentNullException.ThrowIfNull(chapterBuffer);

        return new AudioEncodeOptions(
            TargetSampleRate: chapterBuffer.SampleRate,
            TargetBitDepth: ResolvePreferredBitDepth(chapterBuffer));
    }

    internal static int ResolvePreferredBitDepth(AudioBuffer sourceBuffer)
    {
        ArgumentNullException.ThrowIfNull(sourceBuffer);

        var codecName = sourceBuffer.Metadata.CodecName;
        if (TryResolvePcmBitDepth(codecName, out var pcmBitDepth))
        {
            return pcmBitDepth;
        }

        return 16;
    }

    private static bool TryResolvePcmBitDepth(string? codecName, out int bitDepth)
    {
        bitDepth = default;
        if (string.IsNullOrWhiteSpace(codecName))
        {
            return false;
        }

        var normalized = codecName.Trim().ToLowerInvariant();
        if (normalized.Contains("pcm_s24", StringComparison.Ordinal))
        {
            bitDepth = 24;
            return true;
        }

        if (normalized.Contains("pcm_s16", StringComparison.Ordinal))
        {
            bitDepth = 16;
            return true;
        }

        if (normalized.Contains("pcm_f32", StringComparison.Ordinal) ||
            normalized.Contains("pcm_s32", StringComparison.Ordinal))
        {
            bitDepth = 32;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Finds speech boundaries: title start/end and content start/end.
    /// Title ends when there's a significant silence gap (>threshold).
    /// Content starts after that gap and ends at the final speech offset.
    /// </summary>
    internal static (double TitleStart, double TitleEnd, double ContentStart, double ContentEnd) FindSpeechBoundaries(
        AudioBuffer buffer,
        IReadOnlyList<SilenceInterval> silenceIntervals,
        double gapThreshold,
        IReadOnlyList<HydratedSentence>? hydratedSentences = null,
        string? sectionTitle = null,
        double boundaryToleranceSec = 0.1)
    {
        double audioDuration = buffer.Length / (double)buffer.SampleRate;
        double contentEnd = FindContentEnd(audioDuration, silenceIntervals, boundaryToleranceSec);
        double speechStart = FindSpeechStart(silenceIntervals, audioDuration, boundaryToleranceSec);

        if (TryFindBoundariesFromHydrate(
                hydratedSentences,
                sectionTitle,
                speechStart,
                contentEnd,
                audioDuration,
                silenceIntervals,
                out var hydrateBoundaries))
        {
            return hydrateBoundaries;
        }

        if (silenceIntervals.Count == 0)
        {
            // No silence detected - treat entire audio as content, no separate title
            // Use negative titleEnd to signal "no title segment"
            return (-1.0, -1.0, speechStart, contentEnd);
        }

        // Find the first significant gap after title starts (>gapThreshold seconds)
        double titleEnd = -1.0;
        double contentStart = speechStart;
        bool foundTitleContentGap = false;
        var searchWindowEnd = Math.Min(audioDuration, speechStart + MaxTitleBoundarySearchSeconds);

        foreach (var interval in silenceIntervals)
        {
            // Skip silences before title starts
            if (interval.End.TotalSeconds <= speechStart)
            {
                continue;
            }

            if (interval.Start.TotalSeconds > searchWindowEnd)
            {
                break;
            }

            // Look for significant gap indicating title/content boundary
            if (interval.Duration.TotalSeconds >= gapThreshold)
            {
                titleEnd = interval.Start.TotalSeconds;
                contentStart = interval.End.TotalSeconds;
                foundTitleContentGap = true;
                break;
            }
        }

        // If no significant gap found, assume first sentence is title
        // Use a heuristic: first ~10 seconds is title
        if (!foundTitleContentGap)
        {
            // Find first gap after at least some speech
            foreach (var interval in silenceIntervals)
            {
                if (interval.End.TotalSeconds <= speechStart + 1.0)
                {
                    continue;
                }

                if (interval.Start.TotalSeconds > searchWindowEnd)
                {
                    break;
                }

                if (interval.Start.TotalSeconds > speechStart + 1.0 &&
                    interval.Duration.TotalSeconds >= 0.3)
                {
                    titleEnd = interval.Start.TotalSeconds;
                    contentStart = interval.End.TotalSeconds;
                    foundTitleContentGap = true;
                    break;
                }
            }
        }

        if (!foundTitleContentGap)
        {
            return (-1.0, -1.0, speechStart, contentEnd);
        }

        // Ensure valid boundaries
        double titleStart = speechStart;
        titleEnd = Math.Max(titleStart, titleEnd);
        contentStart = Math.Max(titleEnd, contentStart);
        contentEnd = Math.Max(contentStart, contentEnd);

        if (titleEnd <= titleStart || contentEnd <= contentStart)
        {
            return (-1.0, -1.0, speechStart, contentEnd);
        }

        return (titleStart, titleEnd, contentStart, contentEnd);
    }

    internal static (
        double TitleStart,
        double TitleEnd,
        double ContentStart,
        double ContentEnd,
        double? DecoratorEnd,
        double? TitleResumeStart) FindTreatmentLayout(
        AudioBuffer buffer,
        IReadOnlyList<SilenceInterval> silenceIntervals,
        double gapThreshold,
        HydratedTranscript? hydratedTranscript = null,
        string? sectionTitle = null,
        double boundaryToleranceSec = 0.1)
    {
        double audioDuration = buffer.Length / (double)buffer.SampleRate;
        double contentEnd = FindContentEnd(audioDuration, silenceIntervals, boundaryToleranceSec);
        double speechStart = FindSpeechStart(silenceIntervals, audioDuration, boundaryToleranceSec);

        if (TryFindDecoratorTitleLayoutFromHydrate(
                hydratedTranscript,
                sectionTitle,
                speechStart,
                contentEnd,
                audioDuration,
                silenceIntervals,
                out var layout))
        {
            return layout;
        }

        var boundaries = FindSpeechBoundaries(
            buffer,
            silenceIntervals,
            gapThreshold,
            hydratedTranscript?.Sentences,
            sectionTitle,
            boundaryToleranceSec);

        return (boundaries.TitleStart, boundaries.TitleEnd, boundaries.ContentStart, boundaries.ContentEnd, null, null);
    }

    internal static (
        double TitleStart,
        double TitleEnd,
        double ContentStart,
        double ContentEnd,
        double? DecoratorEnd,
        double? TitleResumeStart) ApplyLayoutPadding(
        (double TitleStart, double TitleEnd, double ContentStart, double ContentEnd, double? DecoratorEnd, double? TitleResumeStart) layout,
        double audioDuration,
        double paddingSec)
    {
        var duration = Math.Max(0.0, audioDuration);
        var hasTitle = layout.TitleStart >= 0 && layout.TitleEnd > layout.TitleStart;
        var titleStart = hasTitle ? Math.Max(0.0, layout.TitleStart - Math.Max(0.0, paddingSec)) : layout.TitleStart;
        var titleEnd = layout.TitleEnd;
        var contentStart = layout.ContentStart;
        var contentEnd = Math.Min(duration, layout.ContentEnd + Math.Max(0.0, paddingSec));
        var decoratorEnd = layout.DecoratorEnd;
        var titleResumeStart = layout.TitleResumeStart;

        if (!hasTitle)
        {
            contentStart = Math.Max(0.0, layout.ContentStart - Math.Max(0.0, paddingSec));
            contentEnd = Math.Max(contentStart, contentEnd);
            return (-1.0, -1.0, contentStart, contentEnd, null, null);
        }

        if (decoratorEnd is double decoratorBoundary &&
            titleResumeStart is double titleBoundary &&
            titleBoundary >= decoratorBoundary)
        {
            (decoratorBoundary, titleBoundary) = ExpandBoundaryPair(decoratorBoundary, titleBoundary, paddingSec);
            decoratorEnd = Math.Clamp(decoratorBoundary, titleStart, layout.TitleEnd);
            titleResumeStart = Math.Clamp(titleBoundary, decoratorEnd.Value, layout.TitleEnd);
        }

        (titleEnd, contentStart) = ExpandBoundaryPair(layout.TitleEnd, layout.ContentStart, paddingSec);
        titleEnd = Math.Clamp(titleEnd, titleStart, contentEnd);
        contentStart = Math.Clamp(contentStart, titleEnd, contentEnd);
        contentEnd = Math.Max(contentStart, contentEnd);

        return (titleStart, titleEnd, contentStart, contentEnd, decoratorEnd, titleResumeStart);
    }

    private static bool TryFindBoundariesFromHydrate(
        IReadOnlyList<HydratedSentence>? hydratedSentences,
        string? sectionTitle,
        double speechStart,
        double contentEnd,
        double audioDuration,
        IReadOnlyList<SilenceInterval> silenceIntervals,
        out (double TitleStart, double TitleEnd, double ContentStart, double ContentEnd) boundaries)
    {
        boundaries = default;
        if (hydratedSentences is null || hydratedSentences.Count < 2)
        {
            return false;
        }

        var timedSentences = GetTimedSentences(hydratedSentences, audioDuration);
        if (timedSentences.Count < 2)
        {
            return false;
        }

        var titleSentence = timedSentences
            .Where(static s => s.ScriptStart is not null)
            .OrderBy(static s => s.ScriptStart)
            .ThenBy(static s => s.StartSec)
            .FirstOrDefault();

        if (titleSentence.Equals(default(TimedSentence)))
        {
            titleSentence = timedSentences[0];
        }

        if (!IsLikelyHeadingSentence(titleSentence.Sentence.BookText, sectionTitle))
        {
            return false;
        }

        TimedSentence? nextSentence = null;
        if (titleSentence.ScriptStart is int titleScriptStart)
        {
            nextSentence = timedSentences
                .Where(s => s.ScriptStart is int scriptStart &&
                            scriptStart > titleScriptStart &&
                            s.StartSec > titleSentence.EndSec)
                .OrderBy(static s => s.ScriptStart)
                .ThenBy(static s => s.StartSec)
                .Cast<TimedSentence?>()
                .FirstOrDefault();
        }

        nextSentence ??= timedSentences
            .Where(s => s.StartSec > titleSentence.EndSec)
            .OrderBy(static s => s.StartSec)
            .Cast<TimedSentence?>()
            .FirstOrDefault();

        if (nextSentence is null)
        {
            return false;
        }

        var titleStart = Math.Max(speechStart, titleSentence.StartSec);
        var titleEnd = Math.Max(titleStart, titleSentence.EndSec);
        var contentStart = Math.Max(titleEnd, nextSentence.Value.StartSec);

        // Defensive snap: any long silence within (titleStart, contentStart) is the structural
        // title-body gap. If hydrate stretched titleEnd past it, pull titleEnd back to silence.Start.
        // If contentStart sits before silence.End, push contentStart forward. See FindLongestSilenceInWindow.
        var structuralSilence = FindLongestSilenceInWindow(titleStart, contentStart, silenceIntervals);
        if (structuralSilence is not null)
        {
            titleEnd = Math.Min(titleEnd, structuralSilence.Start.TotalSeconds);
            contentStart = Math.Max(contentStart, structuralSilence.End.TotalSeconds);
        }
        titleEnd = Math.Max(titleEnd, titleStart);
        contentStart = Math.Max(contentStart, titleEnd);

        if (contentEnd <= contentStart)
        {
            return false;
        }

        boundaries = (titleStart, titleEnd, contentStart, contentEnd);
        return true;
    }

    private static bool TryFindDecoratorTitleLayoutFromHydrate(
        HydratedTranscript? hydratedTranscript,
        string? sectionTitle,
        double speechStart,
        double contentEnd,
        double audioDuration,
        IReadOnlyList<SilenceInterval> silenceIntervals,
        out (double TitleStart, double TitleEnd, double ContentStart, double ContentEnd, double? DecoratorEnd, double? TitleResumeStart) layout)
    {
        layout = default;
        if (hydratedTranscript is null ||
            !TryParseChapterDecoratorTitle(sectionTitle, out var decoratorText, out var headingTitleText))
        {
            return false;
        }

        var timedSentences = GetTimedSentences(hydratedTranscript.Sentences, audioDuration);
        if (timedSentences.Count < 2)
        {
            return false;
        }

        var headingSentence = timedSentences
            .Where(static s => s.ScriptStart is not null)
            .OrderBy(static s => s.ScriptStart)
            .ThenBy(static s => s.StartSec)
            .FirstOrDefault();

        if (headingSentence.Equals(default(TimedSentence)))
        {
            headingSentence = timedSentences[0];
        }

        var headingSentenceIndex = timedSentences.IndexOf(headingSentence);
        if (headingSentenceIndex < 0 || headingSentenceIndex + 1 >= timedSentences.Count)
        {
            return false;
        }

        var nextSentence = timedSentences[headingSentenceIndex + 1];
        var headingStart = Math.Max(speechStart, headingSentence.StartSec);
        var headingSentenceText = NormalizeHeadingText(headingSentence.Sentence.BookText);
        var decoratorTextNormalized = NormalizeHeadingText(decoratorText);
        var headingTitleTextNormalized = NormalizeHeadingText(headingTitleText);
        var fullTitleNormalized = NormalizeHeadingText(sectionTitle!);

        if (headingSentenceText == decoratorTextNormalized &&
            NormalizeHeadingText(nextSentence.Sentence.BookText) == headingTitleTextNormalized)
        {
            if (headingSentenceIndex + 2 >= timedSentences.Count)
            {
                return false;
            }

            var firstContentSentence = timedSentences[headingSentenceIndex + 2];
            var headingEnd = Math.Max(headingStart, nextSentence.EndSec);
            var contentStart = Math.Max(headingEnd, firstContentSentence.StartSec);

            // Snap stretched hydrate boundaries around any contained silence (see TryFindBoundariesFromHydrate).
            // Window starts at the title sentence's StartSec, not headingStart — otherwise the
            // decorator/title pause (between headingSentence and nextSentence) could be selected
            // as the structural title/body silence and collapse the split layout.
            var headingSilence = FindLongestSilenceInWindow(nextSentence.StartSec, contentStart, silenceIntervals);
            if (headingSilence is not null)
            {
                headingEnd = Math.Min(headingEnd, headingSilence.Start.TotalSeconds);
                // Re-derive contentStart from the body sentence: the original Math.Max above can
                // bump contentStart past the body when nextSentence.EndSec is stretched, and the
                // snap needs to be able to pull it back to the natural body start.
                contentStart = Math.Max(headingSilence.End.TotalSeconds, firstContentSentence.StartSec);
            }
            headingEnd = Math.Max(headingEnd, headingStart);
            contentStart = Math.Max(contentStart, headingEnd);

            var rawSplitDecoratorEnd = Math.Max(headingStart, headingSentence.EndSec);
            var rawSplitTitleResumeStart = Math.Max(headingSentence.EndSec, nextSentence.StartSec);
            var splitDecoratorEnd = rawSplitDecoratorEnd;
            var splitTitleResumeStart = rawSplitTitleResumeStart;
            var splitSilence = FindLongestSilenceInWindow(headingSentence.StartSec, nextSentence.EndSec, silenceIntervals);
            if (splitSilence is not null)
            {
                splitDecoratorEnd = Math.Min(splitDecoratorEnd, splitSilence.Start.TotalSeconds);
                splitTitleResumeStart = Math.Max(splitTitleResumeStart, splitSilence.End.TotalSeconds);
            }
            splitTitleResumeStart = Math.Max(splitTitleResumeStart, splitDecoratorEnd);

            if (contentEnd <= contentStart)
            {
                return false;
            }

            layout = (
                headingStart,
                headingEnd,
                contentStart,
                contentEnd,
                splitDecoratorEnd,
                splitTitleResumeStart);
            return true;
        }

        if (headingSentenceText != fullTitleNormalized)
        {
            return false;
        }

        if (!TryFindDecoratorWordBoundary(
                hydratedTranscript,
                headingSentence,
                CountDisplayWords(decoratorText),
                out var decoratorEnd,
                out var titleResumeStart))
        {
            return false;
        }

        var overallTitleEnd = Math.Max(headingStart, headingSentence.EndSec);
        var overallContentStart = Math.Max(overallTitleEnd, nextSentence.StartSec);

        // Snap stretched hydrate boundaries around any contained silence (see TryFindBoundariesFromHydrate).
        // Window starts at titleResumeStart (where the title words resume after the decorator),
        // not headingStart — otherwise the decorator/title intra-heading pause could be selected
        // as the structural title/body silence and clamp the title end into the decorator region.
        var overallSilence = FindLongestSilenceInWindow(titleResumeStart, overallContentStart, silenceIntervals);
        if (overallSilence is not null)
        {
            overallTitleEnd = Math.Min(overallTitleEnd, overallSilence.Start.TotalSeconds);
            // Re-derive overallContentStart from the body sentence (see split path above).
            overallContentStart = Math.Max(overallSilence.End.TotalSeconds, nextSentence.StartSec);
        }
        overallTitleEnd = Math.Max(overallTitleEnd, headingStart);
        overallContentStart = Math.Max(overallContentStart, overallTitleEnd);

        var snappedDecoratorEnd = decoratorEnd;
        var snappedTitleResumeStart = titleResumeStart;
        var decoratorSilence = FindLongestSilenceInWindow(headingStart, overallTitleEnd, silenceIntervals);
        if (decoratorSilence is not null)
        {
            snappedDecoratorEnd = Math.Min(snappedDecoratorEnd, decoratorSilence.Start.TotalSeconds);
            snappedTitleResumeStart = Math.Max(snappedTitleResumeStart, decoratorSilence.End.TotalSeconds);
        }
        snappedTitleResumeStart = Math.Max(snappedTitleResumeStart, snappedDecoratorEnd);

        if (contentEnd <= overallContentStart)
        {
            return false;
        }

        layout = (
            headingStart,
            overallTitleEnd,
            overallContentStart,
            contentEnd,
            snappedDecoratorEnd,
            snappedTitleResumeStart);
        return true;
    }

    private static List<TimedSentence> GetTimedSentences(
        IReadOnlyList<HydratedSentence> hydratedSentences,
        double audioDuration)
    {
        var timed = new List<TimedSentence>(hydratedSentences.Count);
        foreach (var sentence in hydratedSentences)
        {
            if (sentence.Timing is not { } timing || timing.Duration <= 0)
            {
                continue;
            }

            var start = Math.Clamp(timing.StartSec, 0.0, audioDuration);
            var end = Math.Clamp(timing.EndSec, 0.0, audioDuration);
            if (end <= start)
            {
                continue;
            }

            timed.Add(new TimedSentence(sentence, start, end, sentence.ScriptRange?.Start));
        }

        timed.Sort(static (left, right) =>
        {
            var startComparison = left.StartSec.CompareTo(right.StartSec);
            return startComparison != 0 ? startComparison : left.EndSec.CompareTo(right.EndSec);
        });

        return timed;
    }

    // Find the longest silence interval overlapping the open window (windowStart, windowEnd).
    // Used by the hydrate boundary snap to identify the structural pause that should separate
    // a title from its body. Silences abutting either window edge are excluded — they're not
    // the structural gap. Returns null if no silence overlaps the window.
    //
    // Defensive snap rationale: MFA's chunked aligner can stretch a sparse-text chunk's word
    // intervals so the title sentence's reported endSec lands at or past a natural silence
    // (e.g., titleEnd=4.05 with the actual silence at [1.13–3.94]). Treat's hydrate path would
    // then extract a title segment that swallows the silence and appends a roomtone gap on top
    // of it. Callers detect this by searching for a contained silence and clamping titleEnd to
    // its Start / contentStart to its End.
    private static SilenceInterval? FindLongestSilenceInWindow(
        double windowStart,
        double windowEnd,
        IReadOnlyList<SilenceInterval> silenceIntervals)
    {
        if (silenceIntervals.Count == 0 || windowEnd <= windowStart)
        {
            return null;
        }

        SilenceInterval? selected = null;
        var selectedDuration = 0.0;

        foreach (var interval in silenceIntervals)
        {
            var startSec = interval.Start.TotalSeconds;
            var endSec = interval.End.TotalSeconds;
            if (endSec <= windowStart || startSec >= windowEnd || endSec <= startSec)
            {
                continue;
            }

            var duration = endSec - startSec;
            if (duration > selectedDuration)
            {
                selectedDuration = duration;
                selected = interval;
            }
        }

        return selected;
    }

    // The head/tail finders apply click-immunity locally: starting from the file edge, they
    // absorb subsequent silences separated by short non-silent bursts (mic pops, breaths, page
    // rustles). Coalescing happens only here — the silence list passed to structural-gap
    // detection (FindSpeechBoundaries, decorator/title walks) stays raw, so brief mid-content
    // utterances cannot merge two paragraph pauses into a fake title-body gap.
    private static double FindSpeechStart(
        IReadOnlyList<SilenceInterval> silenceIntervals,
        double audioDuration,
        double boundaryToleranceSec = 0.1)
    {
        if (silenceIntervals.Count == 0)
        {
            return 0.0;
        }

        var leading = silenceIntervals[0];
        if (leading.Start.TotalSeconds >= boundaryToleranceSec)
        {
            return 0.0;
        }

        var speechStartSec = leading.End.TotalSeconds;
        for (int i = 1; i < silenceIntervals.Count; i++)
        {
            var next = silenceIntervals[i];
            var burstSec = next.Start.TotalSeconds - speechStartSec;
            if (burstSec > boundaryToleranceSec)
            {
                break;
            }

            speechStartSec = next.End.TotalSeconds;
        }

        return Math.Clamp(speechStartSec, 0.0, audioDuration);
    }

    private static double FindContentEnd(
        double audioDuration,
        IReadOnlyList<SilenceInterval> silenceIntervals,
        double boundaryToleranceSec = 0.1)
    {
        if (silenceIntervals.Count == 0)
        {
            return audioDuration;
        }

        var trailing = silenceIntervals[^1];
        if (Math.Abs(trailing.End.TotalSeconds - audioDuration) > boundaryToleranceSec)
        {
            return audioDuration;
        }

        var trimStartSec = trailing.Start.TotalSeconds;
        for (int i = silenceIntervals.Count - 2; i >= 0; i--)
        {
            var prev = silenceIntervals[i];
            var burstSec = trimStartSec - prev.End.TotalSeconds;
            if (burstSec > boundaryToleranceSec)
            {
                break;
            }

            trimStartSec = prev.Start.TotalSeconds;
        }

        return Math.Clamp(trimStartSec, 0.0, audioDuration);
    }

    private static string? ResolveSectionTitle(ChapterContext chapter)
    {
        ArgumentNullException.ThrowIfNull(chapter);

        var bookIndex = chapter.Book.Documents.BookIndex;
        if (bookIndex is not null)
        {
            IEnumerable<string> candidates = Enumerable.Empty<string>();
            if (!string.IsNullOrWhiteSpace(chapter.Descriptor.ChapterId))
            {
                candidates = candidates.Append(chapter.Descriptor.ChapterId);
            }

            candidates = candidates.Concat(chapter.Descriptor.Aliases ?? Array.Empty<string>());
            foreach (var candidate in candidates.Where(static value => !string.IsNullOrWhiteSpace(value))
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var section = SectionLocator.ResolveSectionByTitle(bookIndex, candidate);
                if (!string.IsNullOrWhiteSpace(section?.Title))
                {
                    return section!.Title;
                }
            }
        }

        return chapter.Descriptor.Aliases?
            .Where(static alias => !string.IsNullOrWhiteSpace(alias) &&
                                   alias.StartsWith("Chapter", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(static alias => alias.Length)
            .FirstOrDefault();
    }

    private static bool TryParseChapterDecoratorTitle(
        string? sectionTitle,
        out string decoratorText,
        out string headingTitleText)
    {
        decoratorText = string.Empty;
        headingTitleText = string.Empty;
        if (string.IsNullOrWhiteSpace(sectionTitle))
        {
            return false;
        }

        var match = ChapterTitlePattern.Match(sectionTitle);
        if (!match.Success)
        {
            return false;
        }

        decoratorText = match.Groups[1].Value.Trim();
        headingTitleText = match.Groups[2].Value.Trim();
        return decoratorText.Length > 0 && headingTitleText.Length > 0;
    }

    private static bool TryFindDecoratorWordBoundary(
        HydratedTranscript hydratedTranscript,
        TimedSentence headingSentence,
        int decoratorWordCount,
        out double decoratorEnd,
        out double titleResumeStart)
    {
        decoratorEnd = 0;
        titleResumeStart = 0;
        if (decoratorWordCount <= 0)
        {
            return false;
        }

        var timedWords = hydratedTranscript.Words
            .Where(w => w.BookIdx is int idx &&
                        idx >= headingSentence.Sentence.BookRange.Start &&
                        idx <= headingSentence.Sentence.BookRange.End &&
                        w.StartSec is not null &&
                        w.EndSec is not null)
            .GroupBy(w => w.BookIdx!.Value)
            .OrderBy(group => group.Key)
            .Select(group => group.First())
            .ToList();

        if (timedWords.Count <= decoratorWordCount)
        {
            return false;
        }

        decoratorEnd = timedWords[decoratorWordCount - 1].EndSec!.Value;
        titleResumeStart = timedWords[decoratorWordCount].StartSec!.Value;
        return titleResumeStart > decoratorEnd;
    }

    private static int CountDisplayWords(string text)
        => Regex.Matches(text, @"\S+").Count;

    private static (double LeftEnd, double RightStart) ExpandBoundaryPair(
        double leftEnd,
        double rightStart,
        double paddingSec)
    {
        if (paddingSec <= 0 || rightStart < leftEnd)
        {
            return (leftEnd, rightStart);
        }

        var expandedLeftEnd = leftEnd + paddingSec;
        var expandedRightStart = rightStart - paddingSec;
        if (expandedLeftEnd <= expandedRightStart)
        {
            return (expandedLeftEnd, expandedRightStart);
        }

        var midpoint = (leftEnd + rightStart) / 2.0;
        return (midpoint, midpoint);
    }

    private static string NormalizeHeadingText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = TextNormalizer.Normalize(text, expandContractions: true, removeNumbers: false);
        var tokens = new List<string>(8);
        TextNormalizer.TokenizeWords(normalized, tokens);
        return string.Join(' ', tokens);
    }

    private static bool IsLikelyHeadingSentence(string sentenceText, string? sectionTitle)
    {
        var normalizedSentence = NormalizeHeadingText(sentenceText);
        if (string.IsNullOrWhiteSpace(normalizedSentence))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(sectionTitle))
        {
            var normalizedSectionTitle = NormalizeHeadingText(sectionTitle);
            if (normalizedSentence == normalizedSectionTitle)
            {
                return true;
            }

            if (TryParseChapterDecoratorTitle(sectionTitle, out _, out var headingTitleText))
            {
                return normalizedSentence == NormalizeHeadingText(headingTitleText);
            }

            return false;
        }

        return normalizedSentence.StartsWith("chapter ", StringComparison.OrdinalIgnoreCase);
    }

    private readonly record struct TimedSentence(HydratedSentence Sentence, double StartSec, double EndSec, int? ScriptStart);

    /// <summary>
    /// Prepares a roomtone segment of the specified duration.
    /// If roomtone is shorter than needed, loops it.
    /// </summary>
    private static AudioBuffer PrepareRoomtoneSegment(AudioBuffer roomtone, double durationSeconds)
    {
        if (roomtone.Length == 0)
        {
            throw new InvalidOperationException("Roomtone buffer is empty (0 samples).");
        }

        if (durationSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationSeconds),
                $"Duration must be positive, got {durationSeconds}");
        }

        double roomtoneDuration = roomtone.Length / (double)roomtone.SampleRate;
        Log.Debug(
            "PrepareRoomtoneSegment: roomtone={RoomtoneDuration:F3}s ({Samples} samples), target={TargetDuration:F3}s",
            roomtoneDuration,
            roomtone.Length,
            durationSeconds);

        return AudioSpliceService.GenerateRoomtoneFill(roomtone, durationSeconds);
    }

}
