using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Ams.Core.Application.Pipeline;

public sealed class PipelineConcurrencyControl : IDisposable
{
    private int _bookIndexForceClaimed;
    private readonly ConcurrentQueue<string> _mfaWorkspaceQueue = new();
    private readonly List<string> _mfaWorkspaces;
    private readonly HashSet<string> _mfaWorkspaceSet;

    public SemaphoreSlim BookIndexSemaphore { get; }
    public SemaphoreSlim AsrSemaphore { get; }
    public SemaphoreSlim MfaSemaphore { get; }
    public int MfaDegree { get; }

    private PipelineConcurrencyControl(int bookIndexDegree, int asrDegree, int mfaDegree)
    {
        var bookIndexCapacity = Math.Max(1, bookIndexDegree);
        var asrCapacity = Math.Max(1, asrDegree);
        var mfaCapacity = Math.Max(1, mfaDegree);

        BookIndexSemaphore = new SemaphoreSlim(bookIndexCapacity, bookIndexCapacity);
        AsrSemaphore = new SemaphoreSlim(asrCapacity, asrCapacity);
        MfaSemaphore = new SemaphoreSlim(mfaCapacity, mfaCapacity);
        MfaDegree = mfaCapacity;

        _mfaWorkspaces = ResolveWorkspaceRoots(mfaCapacity).ToList();
        _mfaWorkspaceSet = new HashSet<string>(_mfaWorkspaces, StringComparer.OrdinalIgnoreCase);
        foreach (var workspace in _mfaWorkspaces)
        {
            _mfaWorkspaceQueue.Enqueue(workspace);
        }
    }

    public static PipelineConcurrencyControl CreateSingle()
    {
        return new PipelineConcurrencyControl(bookIndexDegree: 1, asrDegree: 1, mfaDegree: 1);
    }

    public static PipelineConcurrencyControl Create(int bookIndexDegree, int asrDegree, int mfaDegree)
    {
        return new PipelineConcurrencyControl(bookIndexDegree, asrDegree, mfaDegree);
    }

    public static PipelineConcurrencyControl CreateShared(int maxMfaParallelism)
    {
        return new PipelineConcurrencyControl(bookIndexDegree: maxMfaParallelism,
            asrDegree: Math.Max(1, maxMfaParallelism), mfaDegree: Math.Max(1, maxMfaParallelism));
    }

    public bool TryClaimBookIndexForce()
    {
        return Interlocked.CompareExchange(ref _bookIndexForceClaimed, 1, 0) == 0;
    }

    public string? RentMfaWorkspace()
    {
        if (_mfaWorkspaceQueue.TryDequeue(out var workspace))
        {
            return workspace;
        }

        return _mfaWorkspaces.Count > 0 ? _mfaWorkspaces[0] : null;
    }

    public void ReturnMfaWorkspace(string? workspace)
    {
        if (string.IsNullOrWhiteSpace(workspace))
        {
            return;
        }

        if (!_mfaWorkspaceSet.Contains(workspace))
        {
            return;
        }

        _mfaWorkspaceQueue.Enqueue(workspace);
    }

    public void Dispose()
    {
        BookIndexSemaphore.Dispose();
        AsrSemaphore.Dispose();
        MfaSemaphore.Dispose();
    }

    private static IEnumerable<string> ResolveWorkspaceRoots(int requestedCount)
    {
        var configured = Environment.GetEnvironmentVariable("AMS_MFA_WORKSPACES");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            foreach (var path in configured.Split(Path.PathSeparator,
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                yield return EnsureWorkspace(path);
            }

            yield break;
        }

        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrWhiteSpace(documents))
        {
            yield break;
        }

        var desired = requestedCount <= 1
            ? 1
            : Math.Max(8, requestedCount);
        for (var i = 1; i <= desired; i++)
        {
            var path = Path.Combine(documents, $"MFA_{i}");
            yield return EnsureWorkspace(path);
        }
    }

    private static string EnsureWorkspace(string path)
    {
        Directory.CreateDirectory(path);
        return Path.GetFullPath(path);
    }
}