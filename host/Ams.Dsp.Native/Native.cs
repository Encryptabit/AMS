using System.Runtime.InteropServices;

namespace Ams.Dsp.Native;

internal static unsafe class Native
{
    private const string Dll = "ams_dsp"; // "ams_dsp.dll" on Windows, default loader resolves it

    [DllImport(Dll, EntryPoint = "ams_get_abi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ams_get_abi(out int major, out int minor);

    [DllImport(Dll, EntryPoint = "ams_init", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int ams_init(float sample_rate, uint max_block, uint channels);

    [DllImport(Dll, EntryPoint = "ams_reset", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ams_reset();

    [DllImport(Dll, EntryPoint = "ams_shutdown", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ams_shutdown();

    [DllImport(Dll, EntryPoint = "ams_set_parameter", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ams_set_parameter(uint id, float value01, uint sampleOffset);

    // const float* const* in_ptrs, float* const* out_ptrs, uint32_t nframes
    [DllImport(Dll, EntryPoint = "ams_process", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ams_process(float** in_ptrs, float** out_ptrs, uint nframes);

    [DllImport(Dll, EntryPoint = "ams_get_latency_samples", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern uint ams_get_latency_samples();

    // void ams_save_state(uint8_t* buf, size_t* inout_len)
    [DllImport(Dll, EntryPoint = "ams_save_state", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ams_save_state(byte* buf, ref UIntPtr inout_len);

    // void ams_load_state(const uint8_t* buf, size_t len)
    [DllImport(Dll, EntryPoint = "ams_load_state", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void ams_load_state(byte* buf, UIntPtr len);
}
