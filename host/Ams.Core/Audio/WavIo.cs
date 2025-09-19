using System.Text;
using Ams.Core.Artifacts;

namespace Ams.Core.Audio;

public static class WavIo
{
    /// <summary>
    /// Reads little-endian RIFF/WAVE with PCM (8/16/24/32) or IEEE float (32/64) into a planar AudioBuffer.
    /// </summary>
    public static AudioBuffer ReadPcmOrFloat(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: false);

        string riff = new string(br.ReadChars(4));
        if (riff != "RIFF") throw new InvalidDataException("Not a RIFF file");
        br.ReadUInt32(); // file size (unused)
        string wave = new string(br.ReadChars(4));
        if (wave != "WAVE") throw new InvalidDataException("Not a WAVE file");

        ushort audioFormat = 0; // 1 = PCM, 3 = IEEE float
        ushort numChannels = 0;
        uint sampleRate = 0;
        ushort bitsPerSample = 0;
        uint dataSize = 0;
        long dataPos = 0;

        // Scan chunks (fmt, data, others are skipped)
        while (br.BaseStream.Position + 8 <= br.BaseStream.Length)
        {
            string chunkId = new string(br.ReadChars(4));
            uint chunkSize = br.ReadUInt32();
            long next = br.BaseStream.Position + chunkSize;

            switch (chunkId)
            {
                case "fmt ":
                    audioFormat = br.ReadUInt16();
                    numChannels = br.ReadUInt16();
                    sampleRate = br.ReadUInt32();
                    br.ReadUInt32(); // byte rate
                    br.ReadUInt16(); // block align
                    bitsPerSample = br.ReadUInt16();
                    // Skip any remaining fmt extras
                    break;

                case "data":
                    dataSize = chunkSize;
                    dataPos = br.BaseStream.Position;
                    break;

                default:
                    // ignore unknown chunks (LIST, JUNK, bext, fact, iXML...)
                    break;
            }

            br.BaseStream.Position = next;
        }

        if (dataPos == 0) throw new InvalidDataException("No data chunk found");
        if (audioFormat != 1 && audioFormat != 3)
            throw new NotSupportedException($"Unsupported WAV format: {audioFormat}");
        if (numChannels < 1 || numChannels > 64)
            throw new NotSupportedException($"Unsupported channel count: {numChannels}");
        if (bitsPerSample == 0) throw new InvalidDataException("bitsPerSample missing/invalid");

        br.BaseStream.Position = dataPos;

        int bytesPerSample = bitsPerSample / 8;
        if (bitsPerSample % 8 != 0)
            throw new NotSupportedException($"Bits per sample must be byte aligned. Got {bitsPerSample}.");

        int frameSize = bytesPerSample * numChannels;
        if (frameSize == 0) throw new InvalidDataException("Invalid frame size");

        int frames = (int)(dataSize / (uint)frameSize);
        var buf = new AudioBuffer(numChannels, (int)sampleRate, frames);

        // De-interleave into planar
        switch (audioFormat)
        {
            case 1: // PCM
                switch (bitsPerSample)
                {
                    case 8:
                        for (int i = 0; i < frames; i++)
                        for (int ch = 0; ch < numChannels; ch++)
                            buf.Planar[ch][i] = (br.ReadByte() - 128) / 128f; // unsigned 8-bit
                        break;

                    case 16:
                        for (int i = 0; i < frames; i++)
                        for (int ch = 0; ch < numChannels; ch++)
                            buf.Planar[ch][i] = br.ReadInt16() / 32768f;
                        break;

                    case 24:
                        for (int i = 0; i < frames; i++)
                        {
                            for (int ch = 0; ch < numChannels; ch++)
                            {
                                int b0 = br.ReadByte();
                                int b1 = br.ReadByte();
                                int b2 = br.ReadByte();
                                int s = (b2 << 24) | (b1 << 16) | (b0 << 8);
                                s >>= 8; // sign extend 24 -> 32
                                buf.Planar[ch][i] = s / 8388608f; // 2^23
                            }
                        }

                        break;

                    case 32:
                        for (int i = 0; i < frames; i++)
                        for (int ch = 0; ch < numChannels; ch++)
                            buf.Planar[ch][i] = br.ReadInt32() / 2147483648f; // 2^31
                        break;

                    default:
                        throw new NotSupportedException($"Unsupported PCM bits per sample: {bitsPerSample}");
                }

                break;

            case 3: // IEEE float
                switch (bitsPerSample)
                {
                    case 32:
                        for (int i = 0; i < frames; i++)
                        for (int ch = 0; ch < numChannels; ch++)
                            buf.Planar[ch][i] = br.ReadSingle();
                        break;

                    case 64:
                        for (int i = 0; i < frames; i++)
                        for (int ch = 0; ch < numChannels; ch++)
                            buf.Planar[ch][i] = (float)br.ReadDouble();
                        break;

                    default:
                        throw new NotSupportedException($"Unsupported IEEE float bits per sample: {bitsPerSample}");
                }

                break;
        }

        return buf;
    }

    /// <summary>
    /// Writes a 16-bit PCM interleaved WAV from a planar AudioBuffer (clipped to [-1, 1]).
    /// </summary>
    /// <summary>
    /// Writes an IEEE float 32-bit interleaved WAV from a planar AudioBuffer.
    /// </summary>
    public static void WriteFloat32(string path, AudioBuffer buffer)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".");

        ushort audioFormat = 3; // IEEE float
        ushort numChannels = (ushort)buffer.Channels;
        uint sampleRate = (uint)buffer.SampleRate;
        ushort bitsPerSample = 32;
        ushort blockAlign = (ushort)(numChannels * (bitsPerSample / 8));
        uint byteRate = sampleRate * blockAlign;
        uint dataSize = (uint)(buffer.Length * blockAlign);
        uint riffSize = 4 + (8 + 16) + (8 + dataSize);

        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs, Encoding.ASCII, leaveOpen: false);

        // RIFF header
        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(riffSize);
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));

        // fmt chunk
        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16u);
        bw.Write(audioFormat);
        bw.Write(numChannels);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write(blockAlign);
        bw.Write(bitsPerSample);

        // data chunk
        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(dataSize);

        // interleave and write floats
        for (int i = 0; i < buffer.Length; i++)
        {
            for (int ch = 0; ch < buffer.Channels; ch++)
            {
                float sample = Math.Clamp(buffer.Planar[ch][i], -1f, 1f);
                bw.Write(sample);
            }
        }
    }

    public static void WritePcm16(string path, AudioBuffer buffer)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".");

        ushort audioFormat = 1;
        ushort numChannels = (ushort)buffer.Channels;
        uint sampleRate = (uint)buffer.SampleRate;
        ushort bitsPerSample = 16;
        ushort blockAlign = (ushort)(numChannels * (bitsPerSample / 8));
        uint byteRate = sampleRate * blockAlign;
        uint dataSize = (uint)(buffer.Length * blockAlign);
        uint riffSize = 4 + (8 + 16) + (8 + dataSize);

        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs, Encoding.ASCII, leaveOpen: false);

        // RIFF header
        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(riffSize);
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));

        // fmt chunk
        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16u); // PCM fmt chunk size
        bw.Write(audioFormat); // wFormatTag
        bw.Write(numChannels); // nChannels
        bw.Write(sampleRate); // nSamplesPerSec
        bw.Write(byteRate); // nAvgBytesPerSec
        bw.Write(blockAlign); // nBlockAlign
        bw.Write(bitsPerSample); // wBitsPerSample

        // data chunk
        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(dataSize);

        // interleave and write
        for (int i = 0; i < buffer.Length; i++)
        {
            for (int ch = 0; ch < buffer.Channels; ch++)
            {
                float f = buffer.Planar[ch][i];
                // clip & scale
                f = Math.Clamp(f, -1f, 1f);
                int v = (int)Math.Round(f * 32767.0f);
                short s = (short)Math.Clamp(v, -32768, 32767);
                bw.Write(s);
            }
        }
    }
}