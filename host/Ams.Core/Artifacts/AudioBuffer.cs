namespace Ams.Core.Artifacts;
public sealed class AudioBuffer
{
    public int Channels { get; }
    public int SampleRate { get; }
    public int Length { get; }
    public float[][] Planar { get; }
    public AudioBufferMetadata Metadata { get; private set; }

    public AudioBuffer(int channels, int sampleRate, int length, AudioBufferMetadata? metadata = null)
    {
        Channels = channels;
        SampleRate = sampleRate;
        Length = length;
        Metadata = metadata ?? AudioBufferMetadata.CreateDefault(sampleRate, channels);

        Planar = new float[channels][];
        for (int ch = 0; ch < channels; ch++)
        {
            Planar[ch] = new float[length];
        }
    }

    public void UpdateMetadata(AudioBufferMetadata metadata)
    {
        Metadata = metadata;
    }
}
