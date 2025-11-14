namespace Ams.Core.Artifacts;

public sealed record AudioBufferMetadata
{
    public string? SourcePath { get; init; }
    public string? ContainerFormat { get; init; }
    public string? CodecName { get; init; }
    public double? SourceDurationSeconds { get; init; }
    public double? SourceStartSeconds { get; init; }
    public int SourceSampleRate { get; init; }
    public int CurrentSampleRate { get; init; }
    public int SourceChannels { get; init; }
    public int CurrentChannels { get; init; }
    public string? SourceSampleFormat { get; init; }
    public string? CurrentSampleFormat { get; init; }
    public string? SourceChannelLayout { get; init; }
    public string? CurrentChannelLayout { get; init; }
    public IReadOnlyDictionary<string, string>? Tags { get; init; }

    public static AudioBufferMetadata CreateDefault(int sampleRate, int channels)
    {
        var layout = DescribeDefaultLayout(channels);
        return new AudioBufferMetadata
        {
            SourceSampleRate = sampleRate,
            CurrentSampleRate = sampleRate,
            SourceChannels = channels,
            CurrentChannels = channels,
            SourceSampleFormat = "fltp",
            CurrentSampleFormat = "fltp",
            SourceChannelLayout = layout,
            CurrentChannelLayout = layout
        };
    }

    public AudioBufferMetadata WithCurrentStream(int sampleRate, int channels, string sampleFormat, string? channelLayout)
    {
        return this with
        {
            CurrentSampleRate = sampleRate,
            CurrentChannels = channels,
            CurrentSampleFormat = sampleFormat,
            CurrentChannelLayout = channelLayout ?? DescribeDefaultLayout(channels)
        };
    }

    public static string DescribeDefaultLayout(int channels) => channels switch
    {
        1 => "mono",
        2 => "stereo",
        3 => "2.1",
        4 => "quad",
        5 => "5.0",
        6 => "5.1",
        7 => "6.1",
        8 => "7.1",
        _ => $"{channels}c"
    };
}
