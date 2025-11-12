using Ams.Core.Artifacts.Alignment;
using Ams.Core.Asr;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Prosody;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Runtime.Artifacts;

public interface IArtifactResolver
{
    BookIndex? LoadBookIndex(BookContext context);
    void SaveBookIndex(BookContext context, BookIndex bookIndex);

    TranscriptIndex? LoadTranscript(ChapterContext context);
    void SaveTranscript(ChapterContext context, TranscriptIndex transcript);

    HydratedTranscript? LoadHydratedTranscript(ChapterContext context);
    void SaveHydratedTranscript(ChapterContext context, HydratedTranscript hydrated);

    AnchorDocument? LoadAnchors(ChapterContext context);
    void SaveAnchors(ChapterContext context, AnchorDocument document);

    AsrResponse? LoadAsr(ChapterContext context);
    void SaveAsr(ChapterContext context, AsrResponse asr);

    PausePolicy LoadPausePolicy(ChapterContext context);
    void SavePausePolicy(ChapterContext context, PausePolicy policy);

    PauseAdjustmentsDocument? LoadPauseAdjustments(ChapterContext context);
    void SavePauseAdjustments(ChapterContext context, PauseAdjustmentsDocument document);
}
