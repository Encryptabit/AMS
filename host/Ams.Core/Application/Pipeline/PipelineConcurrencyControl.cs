namespace Ams.Core.Application.Pipeline;

public sealed class PipelineConcurrencyControl : IDisposable
{
    private int _bookIndexForceClaimed;

    public SemaphoreSlim BookIndexSemaphore { get; }
    public SemaphoreSlim AsrSemaphore { get; }
    public SemaphoreSlim MfaSemaphore { get; }

    private PipelineConcurrencyControl(int bookIndexDegree, int asrDegree, int mfaDegree)
    {
        BookIndexSemaphore = new SemaphoreSlim(Math.Max(1, bookIndexDegree), Math.Max(1, bookIndexDegree));
        AsrSemaphore = new SemaphoreSlim(Math.Max(1, asrDegree), Math.Max(1, asrDegree));
        MfaSemaphore = new SemaphoreSlim(Math.Max(1, mfaDegree), Math.Max(1, mfaDegree));
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
        return new PipelineConcurrencyControl(bookIndexDegree: 1, asrDegree: 1, mfaDegree: Math.Max(1, maxMfaParallelism));
    }

    public bool TryClaimBookIndexForce()
    {
        return Interlocked.CompareExchange(ref _bookIndexForceClaimed, 1, 0) == 0;
    }

    public void Dispose()
    {
        BookIndexSemaphore.Dispose();
        AsrSemaphore.Dispose();
        MfaSemaphore.Dispose();
    }
}
