using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ams.Dsp.Native;

/// <summary>
/// Safe, allocation-free (in the hot path) wrapper over the ams_dsp C ABI.
/// Power of Ten: no heap allocs in Process loop; short funcs; check return codes; validate bounds.
/// </summary>
public sealed unsafe class AmsDsp : IDisposable
{
    public const int ExpectedAbiMajor = 1;     // match your Zig side
    public const int ExpectedAbiMinor = 0;     // bump on breaking/adding changes

    private readonly object _sync = new();
    private bool _initialized;
    private readonly uint _channels;
    private readonly uint _maxBlock;
    private readonly float _sampleRate;

    /// <summary>Total channels configured for this instance.</summary>
    public int Channels => checked((int)_channels);
    public int MaxBlock => checked((int)_maxBlock);
    public float SampleRate => _sampleRate;

    public uint LatencySamples => Native.ams_get_latency_samples();

    private AmsDsp(float sampleRate, uint maxBlock, uint channels)
    {
        _sampleRate = sampleRate;
        _maxBlock = maxBlock;
        _channels = channels;
    }

    /// <summary>Factory that checks ABI and initializes native state once.</summary>
    public static AmsDsp Create(float sampleRate, uint maxBlock, uint channels)
    {
        if (sampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        if (maxBlock == 0) throw new ArgumentOutOfRangeException(nameof(maxBlock));
        if (channels == 0 || channels > 8) throw new ArgumentOutOfRangeException(nameof(channels), "1..8 supported in this wrapper");

        Native.ams_get_abi(out var major, out var minor);
        if (major != ExpectedAbiMajor)
            throw new NotSupportedException($"ams_dsp ABI mismatch. Host expects {ExpectedAbiMajor}.{ExpectedAbiMinor}, DLL reports {major}.{minor}.");

        var rc = Native.ams_init(sampleRate, maxBlock, channels);
        if (rc != 0) throw new InvalidOperationException($"ams_init failed rc={rc}");

        var dsp = new AmsDsp(sampleRate, maxBlock, channels) { _initialized = true };
        return dsp;
    }

    public void Reset()
    {
        EnsureInit();
        Native.ams_reset();
    }

    public void SetParameter(uint id, float value01, uint sampleOffset = 0)
    {
        EnsureInit();
        // We let native clamp if it wants, but we can assert bounds for caller discipline:
        if (!(value01 >= 0f && value01 <= 1f)) throw new ArgumentOutOfRangeException(nameof(value01), "expected [0,1]");
        Native.ams_set_parameter(id, value01, sampleOffset);
    }

    /// <summary>
    /// Process exactly <paramref name="nframes"/> frames in one call.
    /// <para>Buffer format: PLANAR float32. in[ch].Length and out[ch].Length MUST be >= nframes.</para>
    /// </summary>
    public void ProcessBlock(float[][] input, float[][] output, int nframes)
    {
        EnsureInit();
        ValidatePlanarBuffers(input, output, nframes);

        int chs = Channels;

        // Pin bases once outside the call; we derive per-block offsets by pointer arithmetic.
        var inPins = new GCHandle[chs];
        var outPins = new GCHandle[chs];
        try
        {
            for (int ch = 0; ch < chs; ch++)
            {
                inPins[ch] = GCHandle.Alloc(input[ch], GCHandleType.Pinned);
                outPins[ch] = GCHandle.Alloc(output[ch], GCHandleType.Pinned);
            }

            // stackalloc arrays of per-channel pointers (avoid heap allocs in hot path)
            IntPtr* inPtrs = stackalloc IntPtr[chs];
            IntPtr* outPtrs = stackalloc IntPtr[chs];

            // Fill pointers at current offset (0 for single-block)
            for (int ch = 0; ch < chs; ch++)
            {
                var inBase = (float*)inPins[ch].AddrOfPinnedObject();
                var outBase = (float*)outPins[ch].AddrOfPinnedObject();
                inPtrs[ch] = (IntPtr)inBase;
                outPtrs[ch] = (IntPtr)outBase;
            }

            Native.ams_process((float**)inPtrs, (float**)outPtrs, checked((uint)nframes));
        }
        finally
        {
            for (int ch = 0; ch < chs; ch++)
            {
                if (inPins[ch].IsAllocated) inPins[ch].Free();
                if (outPins[ch].IsAllocated) outPins[ch].Free();
            }
        }
    }

    /// <summary>
    /// Process a long buffer in fixed-size blocks (<= MaxBlock). No heap allocs inside the inner loop.
    /// </summary>
    public void ProcessLong(float[][] input, float[][] output, int totalFrames)
    {
        EnsureInit();
        ValidatePlanarBuffers(input, output, totalFrames);

        int chs = Channels;

        // Pin bases once
        var inPins = new GCHandle[chs];
        var outPins = new GCHandle[chs];
        try
        {
            for (int ch = 0; ch < chs; ch++)
            {
                inPins[ch] = GCHandle.Alloc(input[ch], GCHandleType.Pinned);
                outPins[ch] = GCHandle.Alloc(output[ch], GCHandleType.Pinned);
            }

            IntPtr* inPtrs = stackalloc IntPtr[chs];
            IntPtr* outPtrs = stackalloc IntPtr[chs];

            int done = 0;
            while (done < totalFrames)
            {
                int remaining = totalFrames - done;
                uint n = checked((uint)Math.Min(remaining, MaxBlock));

                // compute per-channel pointers at current offset
                for (int ch = 0; ch < chs; ch++)
                {
                    var inBase = (float*)inPins[ch].AddrOfPinnedObject();
                    var outBase = (float*)outPins[ch].AddrOfPinnedObject();
                    inPtrs[ch] = (IntPtr)(inBase + done);
                    outPtrs[ch] = (IntPtr)(outBase + done);
                }

                Native.ams_process((float**)inPtrs, (float**)outPtrs, n);
                done += checked((int)n);
            }
        }
        finally
        {
            for (int ch = 0; ch < chs; ch++)
            {
                if (inPins[ch].IsAllocated) inPins[ch].Free();
                if (outPins[ch].IsAllocated) outPins[ch].Free();
            }
        }
    }

    public byte[] SaveState()
    {
        EnsureInit();

        // Two-phase: first query size by passing null/0, then allocate and fetch.
        UIntPtr len = UIntPtr.Zero;
        Native.ams_save_state((byte*)0, ref len);
        int size = checked((int)len);

        var buf = new byte[size];
        fixed (byte* p = buf)
        {
            var l = (UIntPtr)size;
            Native.ams_save_state(p, ref l);
        }
        return buf;
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        EnsureInit();
        fixed (byte* p = state) Native.ams_load_state(p, (UIntPtr)state.Length);
    }

    private void EnsureInit()
    {
        if (!_initialized) throw new InvalidOperationException("AmsDsp not initialized. Use AmsDsp.Create(...)");
    }

    private void ValidatePlanarBuffers(float[][] input, float[][] output, int frames)
    {
        if (input is null || output is null) throw new ArgumentNullException("input/output");
        if (input.Length != Channels || output.Length != Channels) throw new ArgumentException("channel count mismatch");
        for (int ch = 0; ch < Channels; ch++)
        {
            if (input[ch] is null || output[ch] is null) throw new ArgumentNullException($"null plane ch {ch}");
            if (input[ch].Length < frames || output[ch].Length < frames)
                throw new ArgumentOutOfRangeException($"plane {ch} shorter than required frames");
        }
    }

    public void Dispose()
    {
        if (_initialized)
        {
            Native.ams_shutdown();
            _initialized = false;
        }
        GC.SuppressFinalize(this);
    }

    ~AmsDsp()
    {
        if (!_initialized) return;
        try { Native.ams_shutdown(); }
        catch { /* best effort during finalization */ }
    }
}
