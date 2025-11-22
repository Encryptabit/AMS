using Ams.Core.Artifacts.Validation;
using Ams.Core.Processors.Validation;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Services;

public sealed class ValidationService
{
    public Task<ReportResult> BuildReportAsync(
        ChapterContext chapter,
        ValidationReportOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapter);
        ArgumentNullException.ThrowIfNull(options);

        cancellationToken.ThrowIfCancellationRequested();
        var transcript = chapter.Documents.Transcript;
        var hydrated = chapter.Documents.HydratedTranscript;
        var report = ValidationReportBuilder.Build(transcript, hydrated, options);
        return Task.FromResult(report);
    }
}