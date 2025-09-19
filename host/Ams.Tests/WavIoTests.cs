using System;
using System.IO;
using System.Text;
using Ams.Core;
using Xunit;

namespace Ams.Tests.Audio;

public class WavIoTests
{
    [Fact]
    public void ReadPcm24_ConvertsSamplesToFloat()
    {
        var samples = new[] { -8388608, 0, 8388607 };
        var data = new byte[samples.Length * 3];
        for (int i = 0; i < samples.Length; i++)
        {
            int s = samples[i];
            data[i * 3 + 0] = (byte)(s & 0xFF);
            data[i * 3 + 1] = (byte)((s >> 8) & 0xFF);
            data[i * 3 + 2] = (byte)((s >> 16) & 0xFF);
        }

        var path = WriteTempWav(1, 24, 44100, data);
        try
        {
            var buffer = WavIo.ReadPcmOrFloat(path);
            Assert.Equal(1, buffer.Channels);
            Assert.Equal(44100, buffer.SampleRate);
            Assert.Equal(samples.Length, buffer.Length);

            Assert.InRange(buffer.Planar[0][0], -1.0f, -0.9999f);
            Assert.Equal(0f, buffer.Planar[0][1], 6);
            Assert.InRange(buffer.Planar[0][2], 0.9999f, 1.0f);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReadPcm32_ConvertsSamplesToFloat()
    {
        var samples = new[] { int.MinValue, 0, int.MaxValue };
        var data = new byte[samples.Length * 4];
        Buffer.BlockCopy(samples, 0, data, 0, data.Length);

        var path = WriteTempWav(1, 32, 48000, data);
        try
        {
            var buffer = WavIo.ReadPcmOrFloat(path);
            Assert.Equal(1, buffer.Channels);
            Assert.Equal(48000, buffer.SampleRate);
            Assert.Equal(samples.Length, buffer.Length);

            Assert.InRange(buffer.Planar[0][0], -1.0f, -0.99999f);
            Assert.Equal(0f, buffer.Planar[0][1], 6);
            Assert.InRange(buffer.Planar[0][2], 0.99999f, 1.0f);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReadFloat32_PreservesValues()
    {
        var samples = new[] { -0.5f, 0f, 0.5f };
        var data = new byte[samples.Length * sizeof(float)];
        Buffer.BlockCopy(samples, 0, data, 0, data.Length);

        var path = WriteTempWav(audioFormat: 3, bitsPerSample: 32, sampleRate: 32000, data: data);
        try
        {
            var buffer = WavIo.ReadPcmOrFloat(path);
            Assert.Equal(1, buffer.Channels);
            Assert.Equal(32000, buffer.SampleRate);
            Assert.Equal(samples.Length, buffer.Length);
            Assert.Equal(samples[0], buffer.Planar[0][0], 6);
            Assert.Equal(samples[1], buffer.Planar[0][1], 6);
            Assert.Equal(samples[2], buffer.Planar[0][2], 6);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string WriteTempWav(ushort audioFormat, ushort bitsPerSample, int sampleRate, byte[] data)
    {
        const ushort channels = 1;
        int bytesPerSample = bitsPerSample / 8;
        int blockAlign = channels * bytesPerSample;
        int byteRate = sampleRate * blockAlign;

        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
        {
            void WriteAscii(string s) => bw.Write(Encoding.ASCII.GetBytes(s));

            WriteAscii("RIFF");
            bw.Write((uint)(36 + data.Length));
            WriteAscii("WAVE");

            WriteAscii("fmt ");
            bw.Write(16u);
            bw.Write(audioFormat);
            bw.Write(channels);
            bw.Write((uint)sampleRate);
            bw.Write((uint)byteRate);
            bw.Write((ushort)blockAlign);
            bw.Write(bitsPerSample);

            WriteAscii("data");
            bw.Write((uint)data.Length);
            bw.Write(data);
        }

        var path = Path.Combine(Path.GetTempPath(), $"wav-io-{Guid.NewGuid():N}.wav");
        File.WriteAllBytes(path, ms.ToArray());
        return path;
    }
}
