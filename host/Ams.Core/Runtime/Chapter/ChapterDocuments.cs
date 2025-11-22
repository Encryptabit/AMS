using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Alignment.Mfa;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Prosody;
using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Common;

namespace Ams.Core.Runtime.Chapter;

public sealed class ChapterDocuments
{
    private readonly DocumentSlot<TranscriptIndex> _transcript;
    private readonly DocumentSlot<HydratedTranscript> _hydratedTranscript;
    private readonly DocumentSlot<AnchorDocument> _anchors;
    private readonly DocumentSlot<AsrResponse> _asr;
    private readonly DocumentSlot<string> _asrTranscriptText;
    private readonly DocumentSlot<PauseAdjustmentsDocument> _pauseAdjustments;
    private readonly DocumentSlot<PausePolicy> _pausePolicy;
    private readonly DocumentSlot<TextGridDocument> _textGrid;

    internal ChapterDocuments(ChapterContext context, IArtifactResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(resolver);

        DocumentSlotOptions<TDocument> CreateOptions<TDocument>(
            Func<FileInfo?> fileAccessor,
            bool writeThrough = false,
            Func<TDocument?, TDocument?>? postLoad = null)
            where TDocument : class
            => new()
            {
                BackingFileAccessor = fileAccessor,
                WriteThrough = writeThrough,
                PostLoadTransform = postLoad
            };

        _transcript = new DocumentSlot<TranscriptIndex>(
            () => resolver.LoadTranscript(context),
            value => resolver.SaveTranscript(context, value),
            CreateOptions<TranscriptIndex>(() => resolver.GetTranscriptFile(context)));

        _hydratedTranscript = new DocumentSlot<HydratedTranscript>(
            () => resolver.LoadHydratedTranscript(context),
            value => resolver.SaveHydratedTranscript(context, value),
            CreateOptions<HydratedTranscript>(() => resolver.GetHydratedTranscriptFile(context)));

        _anchors = new DocumentSlot<AnchorDocument>(
            () => resolver.LoadAnchors(context),
            value => resolver.SaveAnchors(context, value),
            CreateOptions<AnchorDocument>(() => resolver.GetAnchorsFile(context)));

        _asr = new DocumentSlot<AsrResponse>(
            () => resolver.LoadAsr(context),
            value => resolver.SaveAsr(context, value),
            CreateOptions<AsrResponse>(() => resolver.GetAsrFile(context)));

        _asrTranscriptText = new DocumentSlot<string>(
            () => resolver.LoadAsrTranscriptText(context),
            value => resolver.SaveAsrTranscriptText(context, value),
            CreateOptions<string>(() => resolver.GetAsrTranscriptTextFile(context), writeThrough: true));

        _pauseAdjustments = new DocumentSlot<PauseAdjustmentsDocument>(
            () => resolver.LoadPauseAdjustments(context),
            value => resolver.SavePauseAdjustments(context, value),
            CreateOptions<PauseAdjustmentsDocument>(() => resolver.GetPauseAdjustmentsFile(context)));

        _pausePolicy = new DocumentSlot<PausePolicy>(
            () => resolver.LoadPausePolicy(context),
            value => resolver.SavePausePolicy(context, value),
            CreateOptions<PausePolicy>(
                () => resolver.GetPausePolicyFile(context),
                postLoad: static policy => policy ?? PausePolicyPresets.House()));

        _textGrid = new DocumentSlot<TextGridDocument>(
            new DelegateDocumentSlotAdapter<TextGridDocument>(
                () => resolver.LoadTextGrid(context),
                document => resolver.SaveTextGrid(context, document),
                () => resolver.GetTextGridFile(context)),
            CreateOptions<TextGridDocument>(
                () => resolver.GetTextGridFile(context),
                postLoad: doc =>
                {
                    if (doc is null)
                    {
                        return null;
                    }

                    var backing = resolver.GetTextGridFile(context).FullName;
                    return doc.SourcePath == backing ? doc : doc with { SourcePath = backing };
                }));
    }

    public TranscriptIndex? Transcript
    {
        get => _transcript.GetValue();
        set => _transcript.SetValue(value);
    }

    public HydratedTranscript? HydratedTranscript
    {
        get => _hydratedTranscript.GetValue();
        set => _hydratedTranscript.SetValue(value);
    }

    public AnchorDocument? Anchors
    {
        get => _anchors.GetValue();
        set => _anchors.SetValue(value);
    }

    public AsrResponse? Asr
    {
        get => _asr.GetValue();
        set => _asr.SetValue(value);
    }

    public string? AsrTranscriptText
    {
        get => _asrTranscriptText.GetValue();
        set => _asrTranscriptText.SetValue(value);
    }

    public PauseAdjustmentsDocument? PauseAdjustments
    {
        get => _pauseAdjustments.GetValue();
        set => _pauseAdjustments.SetValue(value);
    }

    public PausePolicy PausePolicy
    {
        get => _pausePolicy.GetValue() ?? PausePolicyPresets.House();
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _pausePolicy.SetValue(value);
        }
    }

    public TextGridDocument? TextGrid
    {
        get => _textGrid.GetValue();
        set => _textGrid.SetValue(value);
    }

    internal bool IsDirty =>
        _transcript.IsDirty ||
        _hydratedTranscript.IsDirty ||
        _anchors.IsDirty ||
        _asr.IsDirty ||
        _asrTranscriptText.IsDirty ||
        _pauseAdjustments.IsDirty ||
        _pausePolicy.IsDirty ||
        _textGrid.IsDirty;

    internal void SaveChanges()
    {
        _transcript.Save();
        _hydratedTranscript.Save();
        _anchors.Save();
        _asr.Save();
        _asrTranscriptText.Save();
        _pauseAdjustments.Save();
        _pausePolicy.Save();
        _textGrid.Save();
    }

    internal void InvalidateTextGrid() => _textGrid.Invalidate();

    internal FileInfo? GetTranscriptFile() => _transcript.GetBackingFile();
    internal FileInfo? GetHydratedTranscriptFile() => _hydratedTranscript.GetBackingFile();
    internal FileInfo? GetAnchorsFile() => _anchors.GetBackingFile();
    internal FileInfo? GetAsrFile() => _asr.GetBackingFile();
    internal FileInfo? GetAsrTranscriptTextFile() => _asrTranscriptText.GetBackingFile();
    internal FileInfo? GetPauseAdjustmentsFile() => _pauseAdjustments.GetBackingFile();
    internal FileInfo? GetPausePolicyFile() => _pausePolicy.GetBackingFile();
    internal FileInfo? GetTextGridFile() => _textGrid.GetBackingFile();
}