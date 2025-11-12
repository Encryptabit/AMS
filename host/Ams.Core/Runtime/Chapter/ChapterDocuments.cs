using System;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Asr;
using Ams.Core.Artifacts;
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
    private readonly DocumentSlot<PauseAdjustmentsDocument> _pauseAdjustments;
    private readonly DocumentSlot<PausePolicy> _pausePolicy;

    internal ChapterDocuments(ChapterContext context, IArtifactResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(resolver);

        _transcript = new DocumentSlot<TranscriptIndex>(
            () => resolver.LoadTranscript(context),
            value => resolver.SaveTranscript(context, value));

        _hydratedTranscript = new DocumentSlot<HydratedTranscript>(
            () => resolver.LoadHydratedTranscript(context),
            value => resolver.SaveHydratedTranscript(context, value));

        _anchors = new DocumentSlot<AnchorDocument>(
            () => resolver.LoadAnchors(context),
            value => resolver.SaveAnchors(context, value));

        _asr = new DocumentSlot<AsrResponse>(
            () => resolver.LoadAsr(context),
            value => resolver.SaveAsr(context, value));

        _pauseAdjustments = new DocumentSlot<PauseAdjustmentsDocument>(
            () => resolver.LoadPauseAdjustments(context),
            value => resolver.SavePauseAdjustments(context, value));

        _pausePolicy = new DocumentSlot<PausePolicy>(
            () => resolver.LoadPausePolicy(context),
            value => resolver.SavePausePolicy(context, value));
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

    internal bool IsDirty =>
        _transcript.IsDirty ||
        _hydratedTranscript.IsDirty ||
        _anchors.IsDirty ||
        _asr.IsDirty ||
        _pauseAdjustments.IsDirty ||
        _pausePolicy.IsDirty;

    internal void SaveChanges()
    {
        _transcript.Save();
        _hydratedTranscript.Save();
        _anchors.Save();
        _asr.Save();
        _pauseAdjustments.Save();
        _pausePolicy.Save();
    }
}
