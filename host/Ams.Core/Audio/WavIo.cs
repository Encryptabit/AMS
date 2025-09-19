using System;
using System.IO;
using System.Text;

namespace Ams.Core;

public static class WavIo
{
    public static AudioBuffer ReadPcmOrFloat(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: false);

        string riff = new string(br.ReadChars(4));
        if (riff != "RIFF") throw new InvalidDataException("Not a RIFF file");
        br.ReadUInt32(); // file size
        string wave = new string(br.ReadChars(4));
        if (wave != "WAVE") throw new InvalidDataException("Not a WAVE file");

        ushort audioFormat = 0; // 1 = PCM, 3 = IEEE float
        ushort numChannels = 0;
        uint sampleRate = 0;
        ushort bitsPerSample = 0;
        uint dataSize = 0;
        long dataPos = 0;

        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            string chunkId = new string(br.ReadChars(4));
            uint chunkSize = br.ReadUInt32();
            long next = br.BaseStream.Position + chunkSize;
            if (chunkId == "fmt ")
            {
                audioFormat = br.ReadUInt16();
                numChannels = br.ReadUInt16();
                sampleRate = br.ReadUInt32();
                br.ReadUInt32(); // byte rate
                br.ReadUInt16(); // block align
                bitsPerSample = br.ReadUInt16();
                // skip any extra fmt
            }
            else if (chunkId == "data")
            {
                dataSize = chunkSize;
                dataPos = br.BaseStream.Position;
            }
            br.BaseStream.Position = next;
        }

        if (dataPos == 0) throw new InvalidDataException("No data chunk found");
        if (audioFormat != 1 && audioFormat != 3) throw new NotSupportedException($"Unsupported WAV format: {audioFormat}");
        if (numChannels < 1 || numChannels > 8) throw new NotSupportedException($"Unsupported channel count: {numChannels}");

        br.BaseStream.Position = dataPos;
        if (bitsPerSample % 8 != 0) throw new NotSupportedException($"Bits per sample must be byte aligned. Got {bitsPerSample}.");

        int bytesPerSample = bitsPerSample / 8;
        int frameSize = bytesPerSample * numChannels;
        int frames = (int)(dataSize / (uint)frameSize);
        var buf = new AudioBuffer(numChannels, (int)sampleRate, frames);

        if (audioFormat == 1)
        {
            switch (bitsPerSample)
            {
                case 16:
                    for (int i = 0; i < frames; i++)
                    {
                        for (int ch = 0; ch < numChannels; ch++)
                        {
                            short sample = br.ReadInt16();
                            buf.Planar[ch][i] = sample / 32768f;
                        }
                    }
                    break;

                case 24:
                    for (int i = 0; i < frames; i++)
                    {
                        for (int ch = 0; ch < numChannels; ch++)
                        {
                            int b0 = br.ReadByte();
                            int b1 = br.ReadByte();
                            int b2 = br.ReadByte();
                            int sample = b0 | (b1 << 8) | (b2 << 16);
                            if ((sample & 0x800000) != 0)
                            {
                                sample |= unchecked((int)0xFF000000);
                            }
                            buf.Planar[ch][i] = sample / 8388608f;
                        }
                    }
                    break;

                case 32:
                    for (int i = 0; i < frames; i++)
                    {
                        for (int ch = 0; ch < numChannels; ch++)
                        {
                            int sample = br.ReadInt32();
                            buf.Planar[ch][i] = sample / 2147483648f;
                        }
                    }
                    break;

                default:
                    throw new NotSupportedException($"Unsupported PCM bit depth: {bitsPerSample}");
            }
        }
        else if (audioFormat == 3)
        {
            if (bitsPerSample != 32)
            {
                throw new NotSupportedException($"Unsupported IEEE float bit depth: {bitsPerSample}");
            }

            for (int i = 0; i < frames; i++)
            {
                for (int ch = 0; ch < numChannels; ch++)
                {
                    buf.Planar[ch][i] = br.ReadSingle();
                }
            }
        }
        else
        {
            throw new NotSupportedException($"Unsupported WAV format: {audioFormat}");
        }

        return buf;
    }

    public static void WriteFloat32(string path, AudioBuffer audio)
    {
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs, Encoding.ASCII, leaveOpen: false);

        int channels = audio.Channels;
        int sampleRate = audio.SampleRate;
        int frames = audio.Length;
        int bytesPerSample = 4;
        int blockAlign = channels * bytesPerSample;
        int byteRate = sampleRate * blockAlign;
        uint dataBytes = (uint)(frames * blockAlign);

        void WriteAscii(string s) => bw.Write(Encoding.ASCII.GetBytes(s));

        WriteAscii("RIFF");
        bw.Write(36u + dataBytes);
        WriteAscii("WAVE");

        WriteAscii("fmt ");
        bw.Write(16u);
        bw.Write((ushort)3); // IEEE float
        bw.Write((ushort)channels);
        bw.Write((uint)sampleRate);
        bw.Write((uint)byteRate);
        bw.Write((ushort)blockAlign);
        bw.Write((ushort)(bytesPerSample * 8));

        WriteAscii("data");
        bw.Write(dataBytes);

        for (int i = 0; i < frames; i++)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                float sample = audio.Planar[ch][i];
                if (float.IsNaN(sample) || float.IsInfinity(sample)) sample = 0f;
                sample = Math.Clamp(sample, -1.0f, 1.0f);
                bw.Write(sample);
            }
        }
    }

    public static void WriteInt16Pcm(string path, AudioBuffer audio)
    {
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs, Encoding.ASCII, leaveOpen: false);

        int channels = audio.Channels;
        int sampleRate = audio.SampleRate;
        int frames = audio.Length;
        int bytesPerSample = 2;
        int blockAlign = channels * bytesPerSample;
        int byteRate = sampleRate * blockAlign;
        uint dataBytes = (uint)(frames * blockAlign);

        void WriteAscii(string s) => bw.Write(Encoding.ASCII.GetBytes(s));

        WriteAscii("RIFF");
        bw.Write(36u + dataBytes);
        WriteAscii("WAVE");

        // fmt chunk (PCM 16)
        WriteAscii("fmt ");
        bw.Write(16u); // chunk size
        bw.Write((ushort)1); // PCM
        bw.Write((ushort)channels);
        bw.Write((uint)sampleRate);
        bw.Write((uint)byteRate);
        bw.Write((ushort)blockAlign);
        bw.Write((ushort)(bytesPerSample * 8));

        // data chunk
        WriteAscii("data");
        bw.Write(dataBytes);

        for (int i = 0; i < frames; i++)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                float x = audio.Planar[ch][i];
                int s = (int)Math.Round(Math.Clamp(x, -1.0f, 1.0f) * 32767.0f);
                bw.Write((short)s);
            }
        }
    }
}


