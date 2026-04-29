using Ams.Core.Asr;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services;

namespace Ams.Tests.Services;

public class PickupMatchingServiceTests
{
    [Fact]
    public void BuildDeterministicSegments_MergesSplitSegmentsBackToCrxTargets()
    {
        var segments = new[]
        {
            new AsrSegment(35.856, 43.586, "He quickly made his way towards the east gate after he was certain he'd lost"),
            new AsrSegment(43.596, 44.436, "the last of them."),
            new AsrSegment(46.784, 53.064, "At the proclamation, every nearby legionnaire stood at attention."),
            new AsrSegment(56.448, 64.808, "Without him even having to call out a command, half the soldiers grouped up while the other"),
            new AsrSegment(64.808, 66.208, "half watched their flanks."),
            new AsrSegment(70.144, 79.024, "A maneuver like this wouldn't have been possible with the larger shield wall, but a shield wall"),
            new AsrSegment(79.024, 85.114, "of five was much more maneuverable than people gave it credit for."),
            new AsrSegment(85.144, 96.794, "As long as they stayed on human-occupied areas and didn't push too far, they would slowly grow.")
        };

        var targets = new[]
        {
            MakeTarget(4, "He quickly made his way towards the east gate after he was certain he'd lost the last of them."),
            MakeTarget(5, "At the proclamation, every nearby legionnaire stood at attention."),
            MakeTarget(9, "Without him even having to call out a command, half the soldiers grouped up while the other half watched their flanks."),
            MakeTarget(10, "A maneuver like this wouldn't have been possible with the larger shield wall, but a shield wall of five was much more maneuverable than people gave it credit for."),
            MakeTarget(11, "As long as they stayed on human-occupied areas and didn't push too far, they would slowly grow.")
        };

        var merged = PickupMatchingService.BuildDeterministicSegments(segments, targets);

        Assert.Equal(targets.Length, merged.Count);
        Assert.Equal(35.856, merged[0].StartSec);
        Assert.Equal(44.436, merged[0].EndSec);
        Assert.Equal("He quickly made his way towards the east gate after he was certain he'd lost the last of them.", merged[0].TranscribedText);
        Assert.Equal("At the proclamation, every nearby legionnaire stood at attention.", merged[1].TranscribedText);
        Assert.Equal("Without him even having to call out a command, half the soldiers grouped up while the other half watched their flanks.", merged[2].TranscribedText);
        Assert.Equal("A maneuver like this wouldn't have been possible with the larger shield wall, but a shield wall of five was much more maneuverable than people gave it credit for.", merged[3].TranscribedText);
        Assert.Equal("As long as they stayed on human-occupied areas and didn't push too far, they would slowly grow.", merged[4].TranscribedText);
    }

    [Fact]
    public void BuildDeterministicSegments_SkipsSegmentsOutsideSelectedChapterTargets()
    {
        var segments = new[]
        {
            new AsrSegment(0.0, 0.8, "chapter one correction"),
            new AsrSegment(0.9, 1.6, "the old gate was barred shut"),
            new AsrSegment(1.7, 2.4, "they turned north toward camp"),
            new AsrSegment(2.5, 3.2, "epilogue correction")
        };

        var targets = new[]
        {
            MakeTarget(21, "the old gate was barred shut"),
            MakeTarget(22, "they turned north toward camp")
        };

        var merged = PickupMatchingService.BuildDeterministicSegments(segments, targets);

        Assert.Equal(2, merged.Count);
        Assert.Equal(0.9, merged[0].StartSec);
        Assert.Equal(1.6, merged[0].EndSec);
        Assert.Equal("the old gate was barred shut", merged[0].TranscribedText);
        Assert.Equal(1.7, merged[1].StartSec);
        Assert.Equal(2.4, merged[1].EndSec);
        Assert.Equal("they turned north toward camp", merged[1].TranscribedText);
    }

    [Fact]
    public void BuildDeterministicSegmentAssignments_PreservesSkippedSegmentsForPickImport()
    {
        var segments = new[]
        {
            new AsrSegment(0.0, 0.8, "chapter one correction"),
            new AsrSegment(0.9, 1.6, "the old gate was barred shut"),
            new AsrSegment(1.7, 2.4, "they turned north toward camp"),
            new AsrSegment(2.5, 3.2, "epilogue correction")
        };

        var targets = new[]
        {
            MakeTarget(21, "the old gate was barred shut"),
            MakeTarget(22, "they turned north toward camp")
        };

        var assignments = PickupMatchingService.BuildDeterministicSegmentAssignments(segments, targets);

        Assert.Equal(4, assignments.Count);
        Assert.Null(assignments[0].Target);
        Assert.Equal("chapter one correction", assignments[0].Segment.TranscribedText);
        Assert.Equal(21, assignments[1].Target?.ErrorNumber);
        Assert.Equal("the old gate was barred shut", assignments[1].Segment.TranscribedText);
        Assert.Equal(22, assignments[2].Target?.ErrorNumber);
        Assert.Equal("they turned north toward camp", assignments[2].Segment.TranscribedText);
        Assert.Null(assignments[3].Target);
        Assert.Equal("epilogue correction", assignments[3].Segment.TranscribedText);
    }

    [Fact]
    public void BuildDeterministicSegments_SingleTarget_SelectsBestRangeFromCombinedSessionFile()
    {
        var segments = new[]
        {
            new AsrSegment(0.0, 0.8, "chapter one correction"),
            new AsrSegment(1.0, 1.7, "the scout crossed the frozen"),
            new AsrSegment(1.7, 2.3, "river at dusk"),
            new AsrSegment(2.4, 3.1, "chapter five correction")
        };

        var targets = new[]
        {
            MakeTarget(31, "the scout crossed the frozen river at dusk")
        };

        var merged = PickupMatchingService.BuildDeterministicSegments(segments, targets);

        var selected = Assert.Single(merged);
        Assert.Equal(1.0, selected.StartSec);
        Assert.Equal(2.3, selected.EndSec);
        Assert.Equal("the scout crossed the frozen river at dusk", selected.TranscribedText);
    }

    private static CrxPickupTarget MakeTarget(int errorNumber, string shouldBeText) =>
        new(
            ErrorNumber: errorNumber,
            ChapterStem: $"chapter-{errorNumber}",
            ChapterName: $"Chapter {errorNumber}",
            SentenceId: errorNumber,
            ShouldBeText: shouldBeText,
            BookText: shouldBeText,
            OriginalStartSec: 0,
            OriginalEndSec: 1);
}
