---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs"
access_modifier: "internal"
base_class: ~
interfaces: []
member_count: 14
dependency_count: 0
pattern: ~
tags:
  - class
---

# PronunciationLexiconCache

> Class in `Ams.Core.Application.Mfa`

**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/PronunciationLexiconCache.cs`

## Properties
- `SchemaVersion`: int
- `CacheFileEnvVar`: string
- `CacheDirEnvVar`: string
- `Gate`: SemaphoreSlim
- `_g2pModel`: string
- `_cacheFilePath`: string
- `_jsonOptions`: JsonSerializerOptions

## Members
- [[PronunciationLexiconCache..ctor]]
- [[PronunciationLexiconCache.GetManyAsync]]
- [[PronunciationLexiconCache.MergeAsync]]
- [[PronunciationLexiconCache.ReadAsync]]
- [[PronunciationLexiconCache.ReadCoreAsync]]
- [[PronunciationLexiconCache.WriteCoreAsync]]
- [[PronunciationLexiconCache.EmptyDocument]]
- [[PronunciationLexiconCache.NormalizeEntries]]
- [[PronunciationLexiconCache.MergeVariants]]
- [[PronunciationLexiconCache.NormalizeVariants]]
- [[PronunciationLexiconCache.AppendVariants]]
- [[PronunciationLexiconCache.AreSame]]
- [[PronunciationLexiconCache.ResolveCacheFilePath]]
- [[PronunciationLexiconCache.SanitizeFileName]]

