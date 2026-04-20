using Ams.Core.Processors;
using Whisper.net;

namespace Ams.Tests.Services;

public sealed class AsrProcessorWhisperXTests
{
    [Fact]
    public void AggregateTokens_PrefersDtwTimestamp_WhenPresent()
    {
        var rawTokens = new[]
        {
            new WhisperToken { Text = " Hello", Start = 100, End = 180, DtwTimestamp = 90 },
            new WhisperToken { Text = " world", Start = 180, End = 260, DtwTimestamp = 150 },
            new WhisperToken { Text = ".", Start = 260, End = 300, DtwTimestamp = 220 }
        };

        var tokens = AsrProcessor.AggregateTokens(rawTokens);

        Assert.Equal(2, tokens.Count);
        Assert.Equal("Hello", tokens[0].Word);
        Assert.Equal(0.90, tokens[0].StartTime, 3);
        Assert.Equal(0.60, tokens[0].Duration, 3);
        Assert.Equal("world.", tokens[1].Word);
        Assert.Equal(1.50, tokens[1].StartTime, 3);
        Assert.Equal(1.50, tokens[1].Duration, 3);
    }

    [Fact]
    public void BuildSegment_PrefersAggregatedTokenSpan_WhenPresent()
    {
        var tokens = new[]
        {
            new AsrToken(1158.292, 0.96, "Aetherslash"),
            new AsrToken(1159.252, 0.12, "and"),
            new AsrToken(1172.792, 0.05, "targets.")
        };

        var segment = AsrProcessor.BuildSegment(
            rawStartSec: 1172.896,
            rawEndSec: 1172.896,
            text: "Aetherslash and ... targets.",
            tokens);

        Assert.Equal(1158.292, segment.StartSec, 3);
        Assert.Equal(1172.842, segment.EndSec, 3);
    }

    [Fact]
    public void ParseWhisperXJson_UsesAlignedWordTimings_WhenPresent()
    {
        const string json = """
            {
              "segments": [
                {
                  "start": 1.25,
                  "end": 2.50,
                  "text": " Hello world.",
                  "words": [
                    { "word": "Hello", "start": 1.25, "end": 1.80 },
                    { "word": "world.", "start": 1.90, "end": 2.50 }
                  ]
                },
                {
                  "start": 2.60,
                  "end": 3.10,
                  "text": " Again.",
                  "words": [
                    { "word": "Again.", "start": 2.60, "end": 3.10 }
                  ]
                }
              ]
            }
            """;

        var response = AsrProcessor.ParseWhisperXJson(json, "whisperx:test-model");

        Assert.Equal("whisperx:test-model", response.ModelVersion);
        Assert.Equal(2, response.Segments.Length);
        Assert.Equal(3, response.Tokens.Length);
        Assert.Equal("Hello", response.Tokens[0].Word);
        Assert.Equal(1.25, response.Tokens[0].StartTime, 3);
        Assert.Equal(0.55, response.Tokens[0].Duration, 3);
        Assert.Equal("world.", response.Tokens[1].Word);
        Assert.Equal("Again.", response.Tokens[2].Word);
        Assert.Equal("Hello world. Again.", string.Join(" ", response.Words));
    }
}
