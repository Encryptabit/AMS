using System.Reflection;
using Xunit.Sdk;
using Ams.Workstation.Server.Components.Pages.Proof;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services;

namespace Ams.Tests.Workstation.Proof;

public sealed class ProofGestureSelectionContractTests
{
    [Fact]
    public async Task OnSentenceLongPress_EntersSelectionMode_AndSeedsPressedSentence()
    {
        var sut = new ChapterReview();
        SetPrivateField(sut, "_sentences", CreateSentences(1, 2));

        await InvokeComponentStateTransitionAsync(() => sut.OnSentenceLongPress(2));

        Assert.True(GetPrivateField<bool>(sut, "_isSelectionModeActive"));
        Assert.False(GetPrivateField<bool>(sut, "_resumePlaybackOnSelectionExit"));
        Assert.Equal("long-press-enter", GetPrivateField<string>(sut, "_lastSelectionGestureEvent"));

        var selected = GetPrivateField<HashSet<int>>(sut, "_selectedSentenceIds");
        Assert.Equal(new[] { 2 }, selected.OrderBy(id => id).ToArray());
    }

    [Fact]
    public async Task OnSentenceLongPress_WhenSelectionAlreadyActive_ClearsSelectionAndResetsResumeGuard()
    {
        var sut = new ChapterReview();
        SetPrivateField(sut, "_sentences", CreateSentences(3, 4));

        await InvokeComponentStateTransitionAsync(() => sut.OnSentenceLongPress(3));

        // Simulate a real second long-press (not same-touch duplicate dispatch).
        SetPrivateField(sut, "_lastLongPressDispatchAtUtc", DateTimeOffset.UtcNow - TimeSpan.FromSeconds(1));
        SetPrivateField(sut, "_resumePlaybackOnSelectionExit", true);

        await InvokeComponentStateTransitionAsync(() => sut.OnSentenceLongPress(3));

        Assert.False(GetPrivateField<bool>(sut, "_isSelectionModeActive"));
        Assert.False(GetPrivateField<bool>(sut, "_resumePlaybackOnSelectionExit"));
        Assert.Equal("long-press-exit", GetPrivateField<string>(sut, "_lastSelectionGestureEvent"));

        var selected = GetPrivateField<HashSet<int>>(sut, "_selectedSentenceIds");
        Assert.Empty(selected);
    }

    [Fact]
    public async Task SelectionSwipeHandlers_FromPlaybackSurface_UseExportAndIgnorePipelines()
    {
        var sut = new ChapterReview();
        SetPrivateField(sut, "_currentView", "playback");
        SetPrivateField(sut, "_sentences", CreateSentences(5));
        SetPrivateField(sut, "_isSelectionModeActive", true);
        SetPrivateField(sut, "_selectedSentenceIds", new HashSet<int> { 5 });
        SetPrivateField(sut, "_report", CreateChapterReport(new[]
        {
            CreateSentenceReport(id: 5, diff: null)
        }));

        await sut.OnSelectionSwipeRight(5, "playback");
        Assert.Equal(
            "swipe-right-export-failed-missing-chapter",
            GetPrivateField<string>(sut, "_lastSelectionGestureEvent"));

        await sut.OnSelectionSwipeLeft(5, "playback");
        Assert.Equal(
            "swipe-left-ignore-noop",
            GetPrivateField<string>(sut, "_lastSelectionGestureEvent"));
    }

    [Fact]
    public async Task SelectionSwipeLeft_RejectsUnselectedGestureOrigin()
    {
        var sut = new ChapterReview();
        SetPrivateField(sut, "_currentView", "playback");
        SetPrivateField(sut, "_isSelectionModeActive", true);
        SetPrivateField(sut, "_selectedSentenceIds", new HashSet<int> { 7 });
        SetPrivateField(sut, "_report", CreateChapterReport(new[]
        {
            CreateSentenceReport(id: 7, diff: null)
        }));

        await sut.OnSelectionSwipeLeft(9, "playback");

        Assert.Equal(
            "swipe-left-ignore-ignored-unselected-context",
            GetPrivateField<string>(sut, "_lastSelectionGestureEvent"));
    }

    [Fact]
    public async Task GestureFlows_KeepPersistenceBoundaries_ServiceOwnedAndArtifactFree()
    {
        var root = CreateTempDirectory();
        var statePath = Path.Combine(root, ".workstation-state.json");

        try
        {
            using var workspace = new BlazorWorkspace(statePath, loadPersistedState: false);
            Assert.True(workspace.SetWorkingDirectory(root));

            var crxService = new CrxService(workspace, new AudioExportService(workspace));
            var reviewedStatusService = new ReviewedStatusService(workspace);

            var crxJsonPath = InvokeNonPublicMethod<string>(crxService, "GetCrxJsonPath", false);
            var reviewedStatusPath = InvokeNonPublicMethod<string>(reviewedStatusService, "GetFilePath");

            Assert.StartsWith(
                Path.Combine(root, "CRX"),
                crxJsonPath,
                StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith("_CRX.json", crxJsonPath, StringComparison.OrdinalIgnoreCase);

            Assert.Equal("reviewed-status.json", Path.GetFileName(reviewedStatusPath));
            Assert.DoesNotContain(root, reviewedStatusPath, StringComparison.OrdinalIgnoreCase);

            var sut = new ChapterReview();
            SetPrivateField(sut, "_sentences", CreateSentences(11));
            SetPrivateField(sut, "_isSelectionModeActive", true);
            SetPrivateField(sut, "_selectedSentenceIds", new HashSet<int> { 11 });
            SetPrivateField(sut, "_report", CreateChapterReport(new[]
            {
                CreateSentenceReport(id: 11, diff: null)
            }));

            await InvokeComponentStateTransitionAsync(() => sut.OnSentenceLongPress(11));
            await sut.OnSelectionSwipeRight(11, "playback");
            await sut.OnSelectionSwipeLeft(11, "playback");

            Assert.False(File.Exists(crxJsonPath));
            Assert.False(Directory.Exists(Path.Combine(root, "CRX")));
            Assert.Empty(crxService.GetEntries());
            Assert.Empty(reviewedStatusService.GetAll());
        }
        finally
        {
            TryDeleteDirectory(root);
        }
    }

    private static async Task InvokeComponentStateTransitionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("render handle is not yet assigned", StringComparison.OrdinalIgnoreCase))
        {
            // These tests intentionally exercise state-machine behavior without bootstrapping a full Blazor renderer.
            // State mutations happen before StateHasChanged() throws this framework-level lifecycle exception.
        }
    }

    private static List<SentenceViewModel> CreateSentences(params int[] sentenceIds)
    {
        return sentenceIds
            .Select((id, index) => new SentenceViewModel
            {
                Id = id,
                Text = $"Sentence {id}",
                StartTime = index,
                EndTime = index + 0.8,
                Status = "error"
            })
            .ToList();
    }

    private static ChapterReport CreateChapterReport(IEnumerable<SentenceReport> sentences)
    {
        var list = sentences.ToList();

        return new ChapterReport(
            ChapterName: "Chapter 01",
            AudioPath: "chapter.wav",
            ScriptPath: "chapter.md",
            Created: DateTime.UtcNow,
            Stats: new ChapterStats(
                SentenceCount: list.Count,
                FlaggedCount: list.Count,
                AvgWer: "0%",
                MaxWer: "0%",
                ParagraphCount: 0,
                ParagraphAvgWer: "0%",
                AvgCoverage: "0%"),
            Sentences: list,
            Paragraphs: Array.Empty<ParagraphReport>());
    }

    private static SentenceReport CreateSentenceReport(int id, DiffReport? diff)
    {
        return new SentenceReport(
            Id: id,
            Wer: "0%",
            Cer: "0%",
            Status: "error",
            BookRange: string.Empty,
            ScriptRange: string.Empty,
            Timing: string.Empty,
            BookText: $"Book sentence {id}",
            ScriptText: $"Script sentence {id}",
            Excerpt: $"Excerpt {id}",
            Diff: diff,
            StartTime: id,
            EndTime: id + 0.8,
            ParagraphId: null);
    }

    private static void SetPrivateField(object target, string fieldName, object? value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new XunitException($"Could not find private field '{fieldName}' on type '{target.GetType().FullName}'.");

        field.SetValue(target, value);
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new XunitException($"Could not find private field '{fieldName}' on type '{target.GetType().FullName}'.");

        var value = field.GetValue(target);
        if (value is T typed)
        {
            return typed;
        }

        throw new XunitException(
            $"Private field '{fieldName}' on type '{target.GetType().FullName}' was not of expected type '{typeof(T).FullName}'. Actual type: '{value?.GetType().FullName ?? "null"}'.");
    }

    private static T InvokeNonPublicMethod<T>(object target, string methodName, params object?[] args)
    {
        var method = target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Name, methodName, StringComparison.Ordinal)
                && candidate.GetParameters().Length == args.Length)
            ?? throw new XunitException($"Could not find non-public method '{methodName}' on type '{target.GetType().FullName}'.");

        var result = method.Invoke(target, args);
        if (result is T typed)
        {
            return typed;
        }

        throw new XunitException(
            $"Non-public method '{methodName}' on type '{target.GetType().FullName}' did not return expected type '{typeof(T).FullName}'. Actual type: '{result?.GetType().FullName ?? "null"}'.");
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ams-proof-gesture-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
