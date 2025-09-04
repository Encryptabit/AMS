const std = @import("std");

pub fn build(b: *std.Build) void {
    const target = b.standardTargetOptions(.{});
    const optimize = b.standardOptimizeOption(.{});

    // Build a static library for consumers (C/C++ or Zig via @cImport / @import)
    const lib = b.addLibrary(.{
        .name = "ams_dsp",
        .linkage = .dynamic,
        .version = .{ .major = 0 , .minor = 0, .patch = 0},
        .root_module = b.createModule(.{
            .root_source_file = b.path("src/main.zig"),
            .target = target,
            .optimize = optimize,
        }),
    });

    b.installArtifact(lib);

    // Optional zig test wiring:
    // const tests = b.addTest(.{
    //     .root_source_file = b.path("src/main.zig"),
    //     .target = target,
    //     .optimize = optimize,
    // });
    // const run_tests = b.addRunArtifact(tests);
    // const test_step = b.step("test", "Run tests");
    // test_step.dependOn(&run_tests.step);
}
