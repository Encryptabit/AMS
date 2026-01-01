using Ams.Core.Application.Validation.Models;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Application.Validation;

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
