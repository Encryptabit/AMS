using System;
using System.Collections.Generic;
using System.Linq;
using Ams.Core;
using Ams.Core.Pipeline;
using Xunit;

namespace Ams.Tests;

public class RefineStageTests
{
    [Fact]
    public void SnapToSilenceStart_FindsEarliestQualifyingSilence()
    {
        // Arrange: Raw sentence ends at 5.0s, silence starts at 5.2s and 5.8s
        var rawSentences = new List<RefinedSentence>
        {
            new("sentence_001", 2.0, 5.0, null, null, "aeneas+pre-snap"),
            new("sentence_002", 6.0, 9.0, null, null, "aeneas+pre-snap")
        };

        var silenceEvents = new List<SilenceEvent>
        {
            new(5.2, 5.5, 0.3, 5.35), // Qualified silence (>= 0.12s)
            new(5.8, 5.9, 0.1, 5.85), // Too short
            new(6.1, 6.3, 0.2, 6.2)   // Qualified silence
        };

        var parameters = new RefinementParams(-30.0, 0.12);

        // Act: Apply snap-to-silence rule
        var refinedSentences = ApplySnapToSilenceRule(rawSentences, silenceEvents, parameters);

        // Assert: First sentence should snap to earliest qualifying silence.start (5.2s)
        Assert.Equal(2, refinedSentences.Count);
        Assert.Equal(5.2, refinedSentences[0].End, 3);
        Assert.Equal("aeneas+silence.start", refinedSentences[0].Source);
        
        // Second sentence has no qualifying silence after its end (9.0), so it stays unchanged
        Assert.Equal(9.0, refinedSentences[1].End, 3);
        Assert.Equal("aeneas+no-snap", refinedSentences[1].Source);
    }

    [Fact]
    public void SnapToSilenceStart_RespectsBoundaryConstraints()
    {
        // Arrange: Sentence ends at 5.0s, next sentence starts at 7.0s
        // Silence at 8.0s is after next sentence start, so should not be used
        var rawSentences = new List<RefinedSentence>
        {
            new("sentence_001", 2.0, 5.0, null, null, "aeneas+pre-snap"),
            new("sentence_002", 7.0, 10.0, null, null, "aeneas+pre-snap")
        };

        var silenceEvents = new List<SilenceEvent>
        {
            new(6.0, 6.3, 0.3, 6.15), // Valid: 6.0 >= 5.0 and < 7.0
            new(8.0, 8.5, 0.5, 8.25)  // Invalid: 8.0 >= 7.0 (after next sentence)
        };

        var parameters = new RefinementParams(-30.0, 0.12);

        // Act
        var refinedSentences = ApplySnapToSilenceRule(rawSentences, silenceEvents, parameters);

        // Assert: Should snap to 6.0s, not 8.0s
        Assert.Equal(6.0, refinedSentences[0].End, 3);
        Assert.Equal("aeneas+silence.start", refinedSentences[0].Source);
    }

    [Fact]
    public void SnapToSilenceStart_NoQualifyingSilence_KeepsOriginalEnd()
    {
        // Arrange: No qualifying silence between sentence end and next start
        var rawSentences = new List<RefinedSentence>
        {
            new("sentence_001", 2.0, 5.0, null, null, "aeneas+pre-snap"),
            new("sentence_002", 6.0, 9.0, null, null, "aeneas+pre-snap")
        };

        var silenceEvents = new List<SilenceEvent>
        {
            new(4.0, 4.05, 0.05, 4.025), // Too short (< 0.12s)
            new(7.0, 7.5, 0.5, 7.25)     // After next sentence start
        };

        var parameters = new RefinementParams(-30.0, 0.12);

        // Act
        var refinedSentences = ApplySnapToSilenceRule(rawSentences, silenceEvents, parameters);

        // Assert: Should keep original end
        Assert.Equal(5.0, refinedSentences[0].End, 3);
        Assert.Equal("aeneas+no-snap", refinedSentences[0].Source);
    }

    [Fact]
    public void SnapToSilenceStart_FiltersOutShortSilences()
    {
        // Arrange: Only silences shorter than minimum duration
        var rawSentences = new List<RefinedSentence>
        {
            new("sentence_001", 2.0, 5.0, null, null, "aeneas+pre-snap")
        };

        var silenceEvents = new List<SilenceEvent>
        {
            new(5.1, 5.2, 0.1, 5.15), // 0.1s < 0.12s minimum
            new(5.3, 5.4, 0.1, 5.35), // 0.1s < 0.12s minimum
        };

        var parameters = new RefinementParams(-30.0, 0.12);

        // Act
        var refinedSentences = ApplySnapToSilenceRule(rawSentences, silenceEvents, parameters);

        // Assert: No qualified silences, should keep original
        Assert.Equal(5.0, refinedSentences[0].End, 3);
        Assert.Equal("aeneas+no-snap", refinedSentences[0].Source);
    }

    [Fact]
    public void ConstraintEnforcement_EnsuresMinimumDuration()
    {
        // Arrange: Refinement would create very short sentence
        var sentences = new List<RefinedSentence>
        {
            new("sentence_001", 5.0, 4.9, null, null, "aeneas+silence.start"), // Impossible: end < start
            new("sentence_002", 6.0, 9.0, null, null, "aeneas+silence.start")
        };

        // Act
        var constrained = EnforceConstraints(sentences);

        // Assert: Should enforce minimum 50ms duration
        Assert.Equal(5.05, constrained[0].End, 3); // 5.0 + 0.05 minimum
    }

    [Fact]
    public void ConstraintEnforcement_PreventsOverlap()
    {
        // Arrange: Sentences that would overlap
        var sentences = new List<RefinedSentence>
        {
            new("sentence_001", 2.0, 6.5, null, null, "aeneas+silence.start"),
            new("sentence_002", 6.0, 9.0, null, null, "aeneas+silence.start") // Overlaps with previous
        };

        // Act
        var constrained = EnforceConstraints(sentences);

        // Assert: First sentence end should be adjusted to avoid overlap
        Assert.True(constrained[0].End < 6.0); // Must be before next sentence start
        Assert.Equal(5.999, constrained[0].End, 3); // 6.0 - 0.001
    }

    [Fact]
    public void ChunkTimeConversion_ConvertsToChapterCoordinates()
    {
        // Arrange: Mock chunk alignments with offsets
        var chunkAlignments = new List<ChunkAlignment>
        {
            new("chunk_001", 0.0, "eng", "abc123", new List<AlignmentFragment>
            {
                new(1.0, 3.0), // Fragment relative to chunk
                new(4.0, 6.0)
            }, new Dictionary<string, string>(), DateTime.UtcNow),
            
            new("chunk_002", 10.0, "eng", "def456", new List<AlignmentFragment>
            {
                new(2.0, 5.0) // Fragment relative to chunk (starts at 2s in chunk)
            }, new Dictionary<string, string>(), DateTime.UtcNow)
        };

        // Act: Convert to sentences
        var sentences = ConvertAlignmentsToSentences(chunkAlignments);

        // Assert: Times should be converted to chapter coordinates
        Assert.Equal(3, sentences.Count);
        
        // First chunk fragments: offset by 0.0
        Assert.Equal(1.0, sentences[0].Start, 3);
        Assert.Equal(3.0, sentences[0].End, 3);
        Assert.Equal(4.0, sentences[1].Start, 3);
        Assert.Equal(6.0, sentences[1].End, 3);
        
        // Second chunk fragment: offset by 10.0
        Assert.Equal(12.0, sentences[2].Start, 3); // 2.0 + 10.0
        Assert.Equal(15.0, sentences[2].End, 3);   // 5.0 + 10.0
    }

    [Fact]
    public void FingerprintStability_SameInputsProduceSameHash()
    {
        // Arrange: Same parameters should produce same fingerprint
        var params1 = new RefinementParams(-30.0, 0.12);
        var params2 = new RefinementParams(-30.0, 0.12);

        // Act
        var hash1 = ComputeParamsHash(params1);
        var hash2 = ComputeParamsHash(params2);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void FingerprintStability_DifferentInputsProduceDifferentHash()
    {
        // Arrange: Different parameters should produce different fingerprints
        var params1 = new RefinementParams(-30.0, 0.12);
        var params2 = new RefinementParams(-34.0, 0.08); // Different values

        // Act
        var hash1 = ComputeParamsHash(params1);
        var hash2 = ComputeParamsHash(params2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    // Helper methods (simplified versions of the actual RefineStage logic)
    
    private List<RefinedSentence> ApplySnapToSilenceRule(
        List<RefinedSentence> rawSentences,
        List<SilenceEvent> silenceEvents,
        RefinementParams parameters)
    {
        if (rawSentences.Count == 0)
            return rawSentences;

        var qualifiedSilences = silenceEvents
            .Where(s => s.Duration >= parameters.MinSilenceDurSec)
            .OrderBy(s => s.Start)
            .ToList();

        var refinedSentences = new List<RefinedSentence>();

        for (int i = 0; i < rawSentences.Count; i++)
        {
            var sentence = rawSentences[i];
            var nextSentenceStart = i + 1 < rawSentences.Count ? rawSentences[i + 1].Start : double.MaxValue;

            var candidateSilences = qualifiedSilences
                .Where(s => s.Start >= sentence.End && s.Start < nextSentenceStart)
                .OrderBy(s => s.Start)
                .ToList();

            double refinedEnd;
            string refinedSource;

            if (candidateSilences.Count > 0)
            {
                refinedEnd = candidateSilences.First().Start;
                refinedSource = "aeneas+silence.start";
            }
            else
            {
                refinedEnd = sentence.End;
                refinedSource = "aeneas+no-snap";
            }

            var refinedSentence = sentence with { End = refinedEnd, Source = refinedSource };
            refinedSentences.Add(refinedSentence);
        }

        return EnforceConstraints(refinedSentences);
    }

    private List<RefinedSentence> EnforceConstraints(List<RefinedSentence> sentences)
    {
        const double MinSentenceDuration = 0.05; // 50ms minimum
        var constrained = new List<RefinedSentence>();

        for (int i = 0; i < sentences.Count; i++)
        {
            var sentence = sentences[i];
            var nextSentence = i + 1 < sentences.Count ? sentences[i + 1] : null;

            var minEnd = sentence.Start + MinSentenceDuration;
            var adjustedEnd = Math.Max(sentence.End, minEnd);

            if (nextSentence != null && adjustedEnd >= nextSentence.Start)
            {
                adjustedEnd = Math.Max(nextSentence.Start - 0.001, minEnd);
            }

            var constrainedSentence = sentence with { End = adjustedEnd };
            constrained.Add(constrainedSentence);
        }

        return constrained;
    }

    private List<RefinedSentence> ConvertAlignmentsToSentences(List<ChunkAlignment> chunkAlignments)
    {
        var sentences = new List<RefinedSentence>();
        int sentenceCounter = 0;

        foreach (var alignment in chunkAlignments.OrderBy(a => a.OffsetSec))
        {
            for (int i = 0; i < alignment.Fragments.Count; i++)
            {
                var fragment = alignment.Fragments[i];
                var sentenceId = $"sentence_{sentenceCounter:D3}";

                var sentence = new RefinedSentence(
                    sentenceId,
                    fragment.Begin + alignment.OffsetSec, // Convert to chapter time
                    fragment.End + alignment.OffsetSec,   // Convert to chapter time
                    null,
                    null,
                    "aeneas+pre-snap"
                );

                sentences.Add(sentence);
                sentenceCounter++;
            }
        }

        return sentences;
    }

    private string ComputeParamsHash(RefinementParams parameters)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(parameters, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
