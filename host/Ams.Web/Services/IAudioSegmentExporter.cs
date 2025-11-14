namespace Ams.Web.Services;

public interface IAudioSegmentExporter
{
    Task<FileInfo> ExportAsync(
        string sourceAudioPath,
        string destinationDirectory,
        string fileName,
        double startSeconds,
        double endSeconds,
        CancellationToken cancellationToken);
}
