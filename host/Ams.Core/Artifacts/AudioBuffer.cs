namespace Ams.Core.Artifacts;
public sealed class AudioBuffer
{
    public int Channels { get; }
    public int SampleRate { get; }
    public int Length { get; }
    public float[][] Planar { get; }

    public AudioBuffer(int channels, int sampleRate, int length)
    {
        Channels = channels;
        SampleRate = sampleRate;
        Length = length;
        Planar = new float[channels][];
        for (int ch = 0; ch < channels; ch++) Planar[ch] = new float[length];
    }
}
