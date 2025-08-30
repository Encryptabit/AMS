const std = @import("std");

// =- Global Settings set by ams_init --
var gSampleRate: f32 = 0;
var gMaxBlock: u32 = 0;
var gChannels: u32 = 0;

// simple param : target and current gain (for smoothing)
var gGainTarget: f32 = 1.0;
var gGainCurrent: f32 = 1.0;

// one-pole smoother: moves current towards target with each sample. Prevents clicks
inline fn smooth(cur: f32, tgt: f32) f32 {
    return cur + (tgt - cur) * 0.001;
}

export fn ams_get_abi(major: *c_int, minor: *c_int) callconv(.c) void {
    major.* = 1;
    minor.* = 0;
}

// Initialize DSP. Host calls this once before processing.
export fn ams_init(sample_rate: f32, max_block: u32, channels: u32) callconv(.c) c_int {
    // Validate inputs (Power of 10: check returns / Zig Zen: crash > silent bug)
    if (sample_rate <= 0 or max_block == 0 or channels == 0) return 1; // AMS_EINVAL

    // Stash immutable configuration we'll use on every block
    gSampleRate = sample_rate;
    gMaxBlock   = max_block;
    gChannels   = channels;

    // Reset parameters
    gGainTarget  = 1.0;
    gGainCurrent = 1.0;

    return 0; // AMS_OK
}

// Clear history to a defined state (e.g., when host calls "reset")
export fn ams_reset() callconv(.c) void {
    gGainCurrent = gGainTarget;
}

// Shutdown: free resources if you had any (we don't yet)
export fn ams_shutdown() callconv(.c) void {}

// Set a parameter. id=0 is "gain" in [0..1] mapped to [0..2x].
export fn ams_set_parameter(id: c_uint, value01: f32, sample_offset: c_uint) callconv(.c) void {
    _ = sample_offset; // (future: sample-accurate automation)
    if (id == 0) {
        // Clamp to [0..1] then map to 0..2x
        var v = value01;
        if (v < 0) v = 0; if (v > 1) v = 1;
        gGainTarget = v * 2.0;
    }
}

// Core processing. Called for each audio block.
// Buffers are planar float32: in_ptrs[c][i] = sample i of channel c.
export fn ams_process(
    in_ptrs:  [*]const [*]const f32, // pointer to per-channel read-only rows
    out_ptrs: [*]       [*]      f32, // pointer to per-channel writable rows
    nframes:  c_uint
) callconv(.c) void {
    // Guard: fixed bounds only, never overrun
    if (nframes == 0 or nframes > gMaxBlock) return;

    const frames: usize = @intCast(nframes);
    const chs:    usize = @intCast(gChannels);

    // Outer loop over samples (frame index), inner over channels
    var i: usize = 0;
    while (i < frames) : (i += 1) {
        // Smooth parameters per-sample to avoid clicks
        gGainCurrent = smooth(gGainCurrent, gGainTarget);

        var ch: usize = 0;
        while (ch < chs) : (ch += 1) {
            const in_row  = in_ptrs[ch];   // row for channel ch
            const out_row = out_ptrs[ch];  // row for channel ch

            // Read input sample, sanitize NaN/Inf (don’t propagate garbage)
            var s = in_row[i];
            if (!std.math.isFinite(s)) s = 0.0;

            // Apply processing (here: gain)
            out_row[i] = s * gGainCurrent;
        }
    }
}

// Report constant algorithmic latency (0 for pass-through)
export fn ams_get_latency_samples() callconv(.c) c_uint {
    return 0;
}

fn as4(ptr: [*]u8, off: usize) *[4]u8 {
    // Pointer arithmetic on many-item pointer, then cast to *[4]u8.
    // u8 has alignment 1, so no @alignCast needed here.
    const p: [*]u8 = ptr + off;
    return @ptrCast(p); // If your Zig wants 1-arg form, do: const q: *[4]u8 = @ptrCast(p); return q;
}

fn as4c(ptr: [*]const u8, off: usize) *const [4]u8 {
    const p: [*]const u8 = ptr + off;
    return @ptrCast(p);
}

export fn ams_save_state(buf: [*]u8, inout_len: *usize) callconv(.c) void {
    if (inout_len.* < 8) { inout_len.* = 8; return; }

    const t_bits: u32 = @bitCast(gGainTarget);
    const c_bits: u32 = @bitCast(gGainCurrent);

    std.mem.writeInt(u32, as4(buf, 0), t_bits, .little);
    std.mem.writeInt(u32, as4(buf, 4), c_bits, .little);

    inout_len.* = 8;
}

export fn ams_load_state(buf: [*]const u8, len: usize) callconv(.c) void {
    if (len < 8) return;

    const t_bits = std.mem.readInt(u32, as4c(buf, 0), .little);
    const c_bits = std.mem.readInt(u32, as4c(buf, 4), .little);

    gGainTarget  = @bitCast(t_bits);
    gGainCurrent = @bitCast(c_bits);
}
