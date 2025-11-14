using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Alignment.Mfa;
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

    string? LoadAsrTranscriptText(ChapterContext context);
    void SaveAsr(ChapterContext context, AsrResponse asr);

    void SaveAsrTranscriptText(ChapterContext context, string text);

    PausePolicy LoadPausePolicy(ChapterContext context);
    void SavePausePolicy(ChapterContext context, PausePolicy policy);

    PauseAdjustmentsDocument? LoadPauseAdjustments(ChapterContext context);
	    void SavePauseAdjustments(ChapterContext context, PauseAdjustmentsDocument document);
	
	    TextGridDocument? LoadTextGrid(ChapterContext context);
	    void SaveTextGrid(ChapterContext context, TextGridDocument document);

	    FileInfo GetBookIndexFile(BookContext context);

	    FileInfo GetTranscriptFile(ChapterContext context);
	    FileInfo GetHydratedTranscriptFile(ChapterContext context);
	    FileInfo GetAnchorsFile(ChapterContext context);
	    FileInfo GetAsrFile(ChapterContext context);
	    FileInfo GetAsrTranscriptTextFile(ChapterContext context);
	    FileInfo GetPausePolicyFile(ChapterContext context);
	    FileInfo GetPauseAdjustmentsFile(ChapterContext context);
	    FileInfo GetTextGridFile(ChapterContext context);

	    FileInfo GetChapterArtifactFile(ChapterContext context, string suffix);
	    FileInfo GetBookArtifactFile(BookContext context, string fileName);
	}
