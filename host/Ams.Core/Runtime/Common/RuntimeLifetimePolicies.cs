namespace Ams.Core.Runtime.Common;

internal sealed record RuntimeCachePolicy(
    string Name,
    int MaxEntries,
    TimeSpan? TimeToLive,
    bool SaveOnUnload,
    bool ReleaseResourcesOnUnload)
{
    public const int RetainAllEntries = int.MaxValue;

    public bool RetainsAll => MaxEntries == RetainAllEntries && TimeToLive is null;

    public int EffectiveMaxEntries(int knownEntryCount)
    {
        if (MaxEntries == RetainAllEntries)
        {
            return knownEntryCount <= 0 ? RetainAllEntries : knownEntryCount;
        }

        var upperBound = knownEntryCount <= 0 ? MaxEntries : knownEntryCount;
        return Math.Max(1, Math.Min(MaxEntries, upperBound));
    }
}

internal sealed record DocumentSlotLifetimePolicy(
    string Name,
    bool KeepLoadedAfterRead,
    bool SaveDirtyOnOwnerSave);

internal static class RuntimeLifetimePolicies
{
    public static RuntimeCachePolicy BookContexts { get; } = new(
        "retain-all-book-contexts",
        RuntimeCachePolicy.RetainAllEntries,
        TimeToLive: null,
        SaveOnUnload: true,
        ReleaseResourcesOnUnload: true);

    public static RuntimeCachePolicy ChapterContexts { get; } = new(
        "retain-known-chapter-contexts",
        RuntimeCachePolicy.RetainAllEntries,
        TimeToLive: null,
        SaveOnUnload: true,
        ReleaseResourcesOnUnload: true);

    public static RuntimeCachePolicy AudioBuffers { get; } = new(
        "retain-all-audio-buffer-contexts",
        RuntimeCachePolicy.RetainAllEntries,
        TimeToLive: null,
        SaveOnUnload: false,
        ReleaseResourcesOnUnload: true);

    public static DocumentSlotLifetimePolicy DocumentSlots { get; } = new(
        "keep-file-backed-documents-loaded",
        KeepLoadedAfterRead: true,
        SaveDirtyOnOwnerSave: true);
}
