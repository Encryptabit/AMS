using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts;
using Ams.Core.Book;
using Spectre.Console;

namespace Ams.Cli.Commands;

internal sealed class ValidateTimingSession
{
    private readonly FileInfo _transcriptFile;
    private readonly FileInfo _bookIndexFile;
    private readonly FileInfo? _hydrateFile;

    private IReadOnlyList<SentenceGap>? _gaps;
    private IReadOnlyDictionary<int, string>? _sentenceLookup;
    private IReadOnlyList<ParagraphInfo>? _paragraphs;
    private IReadOnlyDictionary<int, int>? _sentenceToParagraph;
    private IReadOnlyDictionary<int, IReadOnlyList<int>>? _paragraphSentences;

    public ValidateTimingSession(FileInfo transcriptFile, FileInfo bookIndexFile, FileInfo? hydrateFile)
    {
        _transcriptFile = transcriptFile ?? throw new ArgumentNullException(nameof(transcriptFile));
        _bookIndexFile = bookIndexFile ?? throw new ArgumentNullException(nameof(bookIndexFile));
        _hydrateFile = hydrateFile;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var transcript = await LoadTranscriptAsync(_transcriptFile, cancellationToken).ConfigureAwait(false);
        var bookIndex = await LoadBookIndexAsync(_bookIndexFile, cancellationToken).ConfigureAwait(false);
        _sentenceLookup = BuildSentenceLookup(bookIndex);
        (_paragraphs, _sentenceToParagraph, _paragraphSentences) = BuildParagraphData(bookIndex);
        _gaps = BuildGaps(transcript, bookIndex);

        RenderIntro(transcript, bookIndex);
        var sessionState = new InteractiveState(_gaps, _sentenceLookup, _paragraphs, _sentenceToParagraph, _paragraphSentences);
        var renderer = new TimingRenderer(sessionState);
        var controller = new TimingController(sessionState, renderer);

        controller.Run();
    }

    private static async Task<TranscriptIndex> LoadTranscriptAsync(FileInfo file, CancellationToken cancellationToken)
    {
        if (!file.Exists)
        {
            throw new FileNotFoundException("TranscriptIndex not found", file.FullName);
        }

        await using var stream = file.OpenRead();
        var payload = await JsonSerializer.DeserializeAsync<TranscriptIndex>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }, cancellationToken).ConfigureAwait(false);

        return payload ?? throw new InvalidOperationException("Failed to deserialize TranscriptIndex JSON");
    }

    private static async Task<BookIndex> LoadBookIndexAsync(FileInfo file, CancellationToken cancellationToken)
    {
        if (!file.Exists)
        {
            throw new FileNotFoundException("book-index.json not found", file.FullName);
        }

        await using var stream = file.OpenRead();
        var payload = await JsonSerializer.DeserializeAsync<BookIndex>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }, cancellationToken).ConfigureAwait(false);

        return payload ?? throw new InvalidOperationException("Failed to deserialize book-index JSON");
    }

    private static IReadOnlyList<SentenceGap> BuildGaps(TranscriptIndex transcript, BookIndex book)
    {
        var sentences = transcript.Sentences
            .OrderBy(s => s.BookRange.Start)
            .ToList();

        var result = new List<SentenceGap>(Math.Max(0, sentences.Count - 1));
        if (sentences.Count < 2)
        {
            return result;
        }

        for (int i = 0; i < sentences.Count - 1; i++)
        {
            var current = sentences[i];
            var next = sentences[i + 1];

            var currentTiming = current.Timing;
            var nextTiming = next.Timing;
            double gapSec = Math.Max(0.0, nextTiming.StartSec - currentTiming.EndSec);

            string currentText = ExtractBookText(book, current.BookRange.Start, current.BookRange.End);
            string nextText = ExtractBookText(book, next.BookRange.Start, next.BookRange.End);

            var gap = new SentenceGap(
                Index: i,
                LeftSentenceId: current.Id,
                RightSentenceId: next.Id,
                LeftText: currentText,
                RightText: nextText,
                InitialGapSec: gapSec);

            result.Add(gap);
        }

        return result;
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

    private static (IReadOnlyList<ParagraphInfo> Paragraphs, IReadOnlyDictionary<int, int> SentenceToParagraph, IReadOnlyDictionary<int, IReadOnlyList<int>> ParagraphSentences) BuildParagraphData(BookIndex book)
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

    private void RenderIntro(TranscriptIndex transcript, BookIndex book)
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
        AnsiConsole.MarkupLineInterpolated($"Loaded [green]{transcript.Sentences.Count}[/] sentences, [green]{_gaps?.Count ?? 0}[/] gaps.");
        AnsiConsole.MarkupLineInterpolated($"Source manuscript words: [green]{book.Words.Length}[/].");
        AnsiConsole.WriteLine();
    }

    private sealed class InteractiveState
    {
        private readonly List<EditableGap> _gaps;
        private readonly IReadOnlyDictionary<int, string> _sentenceLookup;
        private readonly IReadOnlyList<ParagraphInfo> _paragraphs;
        private readonly IReadOnlyDictionary<int, int> _sentenceToParagraph;
        private readonly IReadOnlyDictionary<int, IReadOnlyList<int>> _paragraphSentences;

        public IReadOnlyList<EditableGap> Gaps => _gaps;

        public int CursorIndex { get; private set; }

        public InteractiveState(IReadOnlyList<SentenceGap> source,
            IReadOnlyDictionary<int, string>? sentenceLookup,
            IReadOnlyList<ParagraphInfo>? paragraphs,
            IReadOnlyDictionary<int, int>? sentenceToParagraph,
            IReadOnlyDictionary<int, IReadOnlyList<int>>? paragraphSentences)
        {
            _gaps = source.Select(EditableGap.From).ToList();
            CursorIndex = 0;
            _sentenceLookup = sentenceLookup ?? new Dictionary<int, string>();
            _paragraphs = paragraphs ?? Array.Empty<ParagraphInfo>();
            _sentenceToParagraph = sentenceToParagraph ?? new Dictionary<int, int>();
            _paragraphSentences = paragraphSentences ?? new Dictionary<int, IReadOnlyList<int>>();
        }

        public EditableGap Current => _gaps[CursorIndex];

        public void MoveCursor(int delta)
        {
            if (_gaps.Count == 0)
            {
                CursorIndex = 0;
                return;
            }

            CursorIndex = Math.Clamp(CursorIndex + delta, 0, _gaps.Count - 1);
        }

        public void AdjustCurrent(double deltaSeconds)
        {
            if (_gaps.Count == 0)
            {
                return;
            }

            var updated = _gaps[CursorIndex] with
            {
                AdjustedGapSec = Math.Max(0.0, _gaps[CursorIndex].AdjustedGapSec + deltaSeconds)
            };

            updated = updated with
            {
                HasChanges = Math.Abs(updated.AdjustedGapSec - updated.InitialGapSec) > 1e-9
            };

            _gaps[CursorIndex] = updated;
        }

        public void SetCurrent(double seconds)
        {
            if (_gaps.Count == 0)
            {
                return;
            }

            var updated = _gaps[CursorIndex] with
            {
                AdjustedGapSec = Math.Max(0.0, seconds)
            };

            updated = updated with
            {
                HasChanges = Math.Abs(updated.AdjustedGapSec - updated.InitialGapSec) > 1e-9
            };

            _gaps[CursorIndex] = updated;
        }

        public IReadOnlyList<ParagraphExcerpt> GetCurrentParagraphs(int radius)
        {
            if (_gaps.Count == 0 || _paragraphs.Count == 0)
            {
                return Array.Empty<ParagraphExcerpt>();
            }

            var current = Current;
            if (!_sentenceToParagraph.TryGetValue(current.LeftSentenceId, out var leftParagraph))
            {
                leftParagraph = 0;
            }

            int rightParagraph = leftParagraph;
            _sentenceToParagraph.TryGetValue(current.RightSentenceId, out rightParagraph);

            int start = Math.Max(0, leftParagraph - radius);
            int end = Math.Min(_paragraphs.Count - 1, leftParagraph + radius);

            var excerpts = new List<ParagraphExcerpt>(Math.Max(1, end - start + 1));
            for (int idx = start; idx <= end; idx++)
            {
                var paragraph = _paragraphs[idx];
                bool isCurrent = idx == leftParagraph;
                bool isPartner = idx == rightParagraph && rightParagraph != leftParagraph;

                IReadOnlyList<int> sentenceIds = Array.Empty<int>();
                if (_paragraphSentences.TryGetValue(idx, out var ids))
                {
                    sentenceIds = ids;
                }

                var snippets = sentenceIds
                    .Select(id => new SentenceSnippet(
                        id,
                        _sentenceLookup.TryGetValue(id, out var text) ? text : string.Empty,
                        isCurrent && id == current.LeftSentenceId,
                        isPartner && id == current.RightSentenceId))
                    .ToList();

                excerpts.Add(new ParagraphExcerpt(paragraph.Index, paragraph.Text, isCurrent, isPartner, snippets));
            }

            return excerpts;
        }
    }

    private sealed class TimingRenderer
    {
        private readonly InteractiveState _state;

        public TimingRenderer(InteractiveState state)
        {
            _state = state;
        }

        public void Render()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold dodgerblue1]Timing session[/] 🕒  [grey]Esc=exit  ←/→=±5ms  Shift+←/→=±10ms  Enter=set exact[/]");
            AnsiConsole.WriteLine();

            if (_state.Gaps.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No gaps detected for interactive editing.[/]");
                return;
            }

            var slice = GetViewport();
            foreach (var gap in slice)
            {
                RenderGap(gap);
                AnsiConsole.WriteLine();
            }

            RenderBookPreview();
        }

        private IEnumerable<EditableGap> GetViewport()
        {
            const int viewportSize = 3;
            int total = _state.Gaps.Count;
            int cursor = _state.CursorIndex;

            int start;
            if (total <= viewportSize)
            {
                start = 0;
            }
            else
            {
                start = Math.Clamp(cursor - 1, 0, total - viewportSize);
            }

            int end = Math.Min(start + viewportSize, total);
            for (int i = start; i < end; i++)
            {
                yield return _state.Gaps[i];
            }
        }

        private void RenderGap(EditableGap gap)
        {
            var isActive = gap.Index == _state.Current.Index;
            var headerStyle = isActive
                ? new Style(Color.DeepSkyBlue1, decoration: Decoration.Bold)
                : new Style(Color.Grey);

            var delta = gap.AdjustedGapSec - gap.InitialGapSec;
            var deltaMarkup = delta >= 0
                ? $"[green]+{delta:0.000} s[/]"
                : $"[red]{delta:0.000} s[/]";

            var statusMarkup = gap.HasChanges ? "[yellow]*[/] " : string.Empty;

            var headerText = $"{statusMarkup}Gap {gap.Index}  {deltaMarkup}";
            var panel = new Panel(new Markup(
                $"[grey]Prev:[/] {Markup.Escape(gap.LeftText)}\n" +
                $"[grey]Gap :[/] {gap.InitialGapSec:0.000} s -> [bold]{gap.AdjustedGapSec:0.000} s[/]\n" +
                $"[grey]Next:[/] {Markup.Escape(gap.RightText)}"))
            {
                Border = isActive ? BoxBorder.Rounded : BoxBorder.Square,
                BorderStyle = headerStyle,
                Padding = new Padding(1, 0, 1, 0),
                Expand = true
            };

            panel.Header = new PanelHeader(headerText, Justify.Left);

            if (isActive)
            {
                panel = panel.HeavyBorder();
            }

            AnsiConsole.Write(panel);
        }

        private void RenderBookPreview()
        {
            const int manuscriptRadius = 5;
            var excerpts = _state.GetCurrentParagraphs(manuscriptRadius);
            if (excerpts.Count == 0)
            {
                return;
            }

            var content = new Markup(string.Join("\n\n", excerpts.Select(RenderParagraphBlock)));
            var panel = new Panel(content)
            {
                Header = new PanelHeader("Manuscript context", Justify.Left),
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Grey),
                Padding = new Padding(1, 0, 1, 0),
                Expand = true
            };

            AnsiConsole.Write(panel);
        }

        private static string RenderParagraphBlock(ParagraphExcerpt excerpt)
        {
            if (excerpt.Sentences.Count == 0)
            {
                var escaped = Markup.Escape(excerpt.Text);
                if (excerpt.IsCurrent)
                {
                    return $"[white on dodgerblue1]{escaped}[/]";
                }

                if (excerpt.IsPartner)
                {
                    return $"[aqua]{escaped}[/]";
                }

                return escaped;
            }

            var rendered = new List<string>(excerpt.Sentences.Count);
            foreach (var sentence in excerpt.Sentences)
            {
                var escaped = Markup.Escape(sentence.Text);
                if (sentence.IsCurrent)
                {
                    rendered.Add($"[dodgerblue1]{escaped}[/]");
                }
                else if (sentence.IsPartner)
                {
                    rendered.Add($"[aqua]{escaped}[/]");
                }
                else
                {
                    rendered.Add(escaped);
                }
            }

            return string.Join(' ', rendered);
        }
    }

    private sealed class TimingController
    {
        private readonly InteractiveState _state;
        private readonly TimingRenderer _renderer;

        public TimingController(InteractiveState state, TimingRenderer renderer)
        {
            _state = state;
            _renderer = renderer;
        }

        public void Run()
        {
            if (_state.Gaps.Count == 0)
            {
                _renderer.Render();
                return;
            }

            _renderer.Render();

            while (true)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.K:
                        _state.MoveCursor(-1);
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.J:
                        _state.MoveCursor(+1);
                        break;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.H:
                        _state.AdjustCurrent((key.Modifiers & ConsoleModifiers.Shift) != 0 ? -0.010 : -0.005);
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.L:
                        _state.AdjustCurrent((key.Modifiers & ConsoleModifiers.Shift) != 0 ? +0.010 : +0.005);
                        break;
                    case ConsoleKey.Enter:
                        PromptForValue();
                        break;
                    default:
                        continue;
                }

                _renderer.Render();
            }
        }

        private void PromptForValue()
        {
            var prompt = new TextPrompt<double>("Set gap seconds")
                .DefaultValue(_state.Current.AdjustedGapSec)
                .ValidationErrorMessage("Please enter a non-negative number.")
                .Validate(value => value >= 0.0);

            var newValue = AnsiConsole.Prompt(prompt);
            _state.SetCurrent(newValue);
        }
    }

    private sealed record SentenceGap(
        int Index,
        int LeftSentenceId,
        int RightSentenceId,
        string LeftText,
        string RightText,
        double InitialGapSec);

    private sealed record EditableGap(
        int Index,
        int LeftSentenceId,
        int RightSentenceId,
        string LeftText,
        string RightText,
        double InitialGapSec,
        double AdjustedGapSec,
        bool HasChanges)
    {
        public static EditableGap From(SentenceGap gap) => new(
            gap.Index,
            gap.LeftSentenceId,
            gap.RightSentenceId,
            gap.LeftText,
            gap.RightText,
            gap.InitialGapSec,
            gap.InitialGapSec,
            HasChanges: false);
    }

    private sealed record ParagraphInfo(int Index, string Kind, string Style, string Text);

    private sealed record ParagraphExcerpt(int ParagraphIndex, string Text, bool IsCurrent, bool IsPartner, IReadOnlyList<SentenceSnippet> Sentences);

    private sealed record SentenceSnippet(int SentenceId, string Text, bool IsCurrent, bool IsPartner);
}
