using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core.Runtime.Documents;

public interface IPronunciationProvider
{
    Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(IEnumerable<string> words, CancellationToken cancellationToken);
}

public sealed class NullPronunciationProvider : IPronunciationProvider
{
    public static NullPronunciationProvider Instance { get; } = new();

    private NullPronunciationProvider()
    {
    }

    public Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(IEnumerable<string> words, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyDictionary<string, string[]>>(new Dictionary<string, string[]>());
}
