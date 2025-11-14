namespace Ams.Core.Application.Pipeline;

public enum PipelineStage
{
    Pending = 0,
    BookIndex = 1,
    Asr = 2,
    Anchors = 3,
    Transcript = 4,
    Hydrate = 5,
    Mfa = 6,
    Complete = 7
}
