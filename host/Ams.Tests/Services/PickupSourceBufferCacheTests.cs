using Ams.Core.Artifacts;
using Ams.Workstation.Server.Models;
using Ams.Workstation.Server.Services.Pickups.Edl;

namespace Ams.Tests.Services;

public sealed class PickupSourceBufferCacheTests
{
    [Fact]
    public void GetSliceByTime_ReusesSingleDecodeAcrossMultipleSlices()
    {
        var root = CreateTempDirectory();
        try
        {
            var sourcePath = CreateTempSourceFile(root, "pickup.wav");
            var decodeInvocations = 0;
            var backing = CreateRampBuffer(sampleRate: 100, length: 1_000);
            var cache = new PickupSourceBufferCache(_ =>
            {
                decodeInvocations++;
                return backing;
            });

            var source = cache.DescribeSource(sourcePath);
            var sliceA = cache.GetSliceByTime(source, 0.10, 0.30, "chapter-01", "op-001");
            var sliceB = cache.GetSliceByTime(source, 0.35, 0.40, "chapter-01", "op-002");

            Assert.Equal(1, decodeInvocations);
            Assert.Equal(1, cache.DecodeCount);
            Assert.Equal(20, sliceA.Length);
            Assert.Equal(5, sliceB.Length);
            Assert.Equal(10f, sliceA[0, 0]);
            Assert.Equal(35f, sliceB[0, 0]);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void GetSliceBySamples_OutOfBounds_ThrowsDetailedDiagnostics()
    {
        var root = CreateTempDirectory();
        try
        {
            var sourcePath = CreateTempSourceFile(root, "pickup.wav");
            var cache = new PickupSourceBufferCache(_ => CreateRampBuffer(sampleRate: 16000, length: 100));
            var source = cache.DescribeSource(sourcePath);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                cache.GetSliceBySamples(source, startSample: 50, endSample: 500, "chapter-02", "op-out-of-range"));

            Assert.Contains("op-out-of-range", ex.Message, StringComparison.Ordinal);
            Assert.Contains("chapter-02", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(source.Fingerprint, ex.Message, StringComparison.Ordinal);
            Assert.Contains("out of bounds", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void GetSliceByTime_RejectsStaleFingerprintAfterSourceMutation()
    {
        var root = CreateTempDirectory();
        try
        {
            var sourcePath = CreateTempSourceFile(root, "pickup.wav");
            var decodeInvocations = 0;
            var cache = new PickupSourceBufferCache(_ =>
            {
                decodeInvocations++;
                return CreateRampBuffer(sampleRate: 100, length: 1_000);
            });

            var source = cache.DescribeSource(sourcePath);

            File.AppendAllText(sourcePath, "mutated");
            File.SetLastWriteTimeUtc(sourcePath, DateTime.UtcNow.AddSeconds(2));

            var ex = Assert.Throws<InvalidOperationException>(() =>
                cache.GetSliceByTime(source, 0.0, 0.1, "chapter-03", "op-stale"));

            Assert.Contains("stale source fingerprint", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("op-stale", ex.Message, StringComparison.Ordinal);
            Assert.Contains("chapter-03", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, decodeInvocations);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    [Fact]
    public void GetSliceByTime_MissingSource_ThrowsPathAndFingerprintDiagnostics()
    {
        var root = CreateTempDirectory();
        try
        {
            var missingPath = Path.Combine(root, "missing.wav");
            var source = new PickupEdlSourceReference(
                path: missingPath,
                fingerprint: "fp-missing-001",
                fileSizeBytes: 0,
                modifiedAtUtc: DateTime.UtcNow);
            var cache = new PickupSourceBufferCache(_ => CreateRampBuffer(sampleRate: 100, length: 100));

            var ex = Assert.Throws<FileNotFoundException>(() =>
                cache.GetSliceByTime(source, 0.0, 0.1, "chapter-04", "op-missing"));

            Assert.Contains("fp-missing-001", ex.Message, StringComparison.Ordinal);
            Assert.Contains("op-missing", ex.Message, StringComparison.Ordinal);
            Assert.Contains("chapter-04", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(missingPath, ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            SafeDeleteDirectory(root);
        }
    }

    private static AudioBuffer CreateRampBuffer(int sampleRate, int length)
    {
        var buffer = new AudioBuffer(channels: 1, sampleRate: sampleRate, length: length);
        for (var i = 0; i < length; i++)
        {
            buffer[0, i] = i;
        }

        return buffer;
    }

    private static string CreateTempSourceFile(string root, string fileName)
    {
        var path = Path.Combine(root, fileName);
        File.WriteAllBytes(path, new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00 });
        return path;
    }

    private static string CreateTempDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ams-pickup-cache-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private static void SafeDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // best effort cleanup only
        }
    }
}
