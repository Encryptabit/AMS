using Ams.Core.Artifacts;
using System.Text;

namespace Ams.Tests;

public class AudioBufferSliceTests
{
    [Fact]
    public void Slice_SharesBacking()
    {
        var buffer = new AudioBuffer(1, 16000, 100);
        var span = buffer.GetChannelSpan(0);
        for (int i = 0; i < 100; i++)
            span[i] = i * 0.01f;

        var slice = buffer.Slice(10, 20);

        // Slice reads same data as parent without copy
        Assert.Equal(20, slice.Length);
        var sliceSpan = slice.GetChannel(0).Span;
        for (int i = 0; i < 20; i++)
            Assert.Equal((10 + i) * 0.01f, sliceSpan[i], 6);
    }

    [Fact]
    public void Slice_DoesNotAffectParent()
    {
        var buffer = new AudioBuffer(1, 16000, 100);
        var _ = buffer.Slice(10, 20);

        // Parent length unchanged after slicing
        Assert.Equal(100, buffer.Length);
    }

    [Fact]
    public void Slice_OutOfBounds_Throws()
    {
        var buffer = new AudioBuffer(1, 16000, 50);

        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Slice(40, 20));
    }

    [Fact]
    public void Slice_TimeOverload()
    {
        const int sampleRate = 16000;
        var buffer = new AudioBuffer(1, sampleRate, sampleRate * 2); // 2 seconds

        var slice = buffer.Slice(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1.5));

        // 0.5s to 1.5s = 1.0s = 16000 samples
        Assert.Equal(sampleRate, slice.Length);
        Assert.Equal(sampleRate, slice.SampleRate);
    }

    [Fact]
    public void Slice_ToWavStream_ProducesValidWav()
    {
        if (!TryEnsureFfmpeg()) return;

        var buffer = new AudioBuffer(1, 16000, 16000); // 1 second
        var span = buffer.GetChannelSpan(0);
        for (int i = 0; i < 16000; i++)
            span[i] = (float)(0.5 * Math.Sin(2 * Math.PI * 440 * i / 16000.0));

        var slice = buffer.Slice(4000, 8000); // 0.5 seconds

        using var wavStream = slice.ToWavStream();
        Assert.NotNull(wavStream);
        Assert.True(wavStream.Length > 44, "WAV stream should have header + data");

        var bytes = wavStream.ToArray();
        var dataOffset = FindChunk(bytes, "data");
        Assert.True(dataOffset >= 0, "WAV stream should contain a data chunk.");
        int dataSize = BitConverter.ToInt32(bytes, dataOffset + 4);

        // 8000 samples * 2 bytes/sample (16-bit) = 16000 bytes
        Assert.Equal(8000 * 2, dataSize);
    }

    private static int FindChunk(byte[] wavBytes, string chunkId)
    {
        for (int offset = 12; offset + 8 <= wavBytes.Length;)
        {
            if (Encoding.ASCII.GetString(wavBytes, offset, 4) == chunkId)
            {
                return offset;
            }

            var chunkSize = BitConverter.ToInt32(wavBytes, offset + 4);
            if (chunkSize < 0)
            {
                break;
            }

            offset += 8 + chunkSize + (chunkSize & 1);
        }

        return -1;
    }

    [Fact]
    public void Slice_GetChannel_ReturnsCorrectRange()
    {
        var buffer = new AudioBuffer(1, 16000, 100);
        var span = buffer.GetChannelSpan(0);
        for (int i = 0; i < 100; i++)
            span[i] = i;

        var slice = buffer.Slice(25, 50);
        var channel = slice.GetChannel(0);

        Assert.Equal(50, channel.Length);
        Assert.Equal(25f, channel.Span[0]);
        Assert.Equal(74f, channel.Span[49]);
    }

    [Fact]
    public void GetChannelSpan_WriteThrough()
    {
        var buffer = new AudioBuffer(1, 16000, 100);
        var span = buffer.GetChannelSpan(0);
        for (int i = 0; i < 100; i++)
            span[i] = 0f;

        var slice = buffer.Slice(10, 20);

        // Write through the slice
        slice.GetChannelSpan(0)[5] = 42.0f;

        // Visible in parent at offset 10 + 5 = 15
        Assert.Equal(42.0f, buffer.GetChannel(0).Span[15]);
    }

    [Fact]
    public void Contiguous_MultiChannel()
    {
        var buffer = new AudioBuffer(2, 44100, 100);

        // Write distinct values to each channel
        var ch0 = buffer.GetChannelSpan(0);
        var ch1 = buffer.GetChannelSpan(1);
        for (int i = 0; i < 100; i++)
        {
            ch0[i] = i * 1.0f;
            ch1[i] = i * -1.0f;
        }

        // Verify channels are independent (correctly strided)
        var readCh0 = buffer.GetChannel(0).Span;
        var readCh1 = buffer.GetChannel(1).Span;
        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(i * 1.0f, readCh0[i]);
            Assert.Equal(i * -1.0f, readCh1[i]);
        }

        // Verify the indexer agrees
        Assert.Equal(50.0f, buffer[0, 50]);
        Assert.Equal(-50.0f, buffer[1, 50]);
    }

    private static bool TryEnsureFfmpeg()
    {
        try
        {
            Ams.Core.Services.Integrations.FFmpeg.FfSession.EnsureInitialized();
            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            Console.WriteLine($"Skipping WAV test: {ex.Message}");
            return false;
        }
    }
}
