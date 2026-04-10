using System.Collections.ObjectModel;

namespace Ams.Workstation.Server.Components.Navigation;

public sealed record StageRouteDescriptor(
    string Id,
    string DisplayName,
    string EntryPath,
    IReadOnlyList<StageModuleRouteDescriptor> Modules);

public sealed record StageModuleRouteDescriptor(
    string Id,
    string DisplayName,
    string CanonicalPath,
    bool SupportsBatching,
    IReadOnlyList<string> CompatibilityPaths);

public sealed record StageRouteMatch(
    string RequestedPath,
    string NormalizedPath,
    StageRouteDescriptor Stage,
    StageModuleRouteDescriptor Module,
    string MatchedTemplate,
    bool IsCompatibilityAlias)
{
    public string DiagnosticContext
        => $"path='{RequestedPath}', normalized='{NormalizedPath}', stage='{Stage.Id}', module='{Module.Id}', template='{MatchedTemplate}', compatibility='{IsCompatibilityAlias}'";
}

public static class StageRouteCatalog
{
    public static class StageIds
    {
        public const string Prep = "prep";
        public const string Proof = "proof";
        public const string Polish = "polish";
    }

    public static class ModuleIds
    {
        public const string PrepPipeline = "prep-pipeline";
        public const string ProofEditing = "proof-editing";
        public const string ProofOverview = "proof-overview";
        public const string ProofPatterns = "proof-patterns";
        public const string PolishScaffold = "polish-scaffold";
    }

    public const string RootPath = "/";
    public const string ProofChapterCompatibilityTemplate = "/proof/{chapter}";
    public const string ProofChapterCanonicalTemplate = "/proof/editing/{chapter}";

    public static StageRouteDescriptor Prep { get; } = new(
        StageIds.Prep,
        "Prep",
        "/prep",
        [
            new StageModuleRouteDescriptor(
                ModuleIds.PrepPipeline,
                "Pipeline",
                "/prep/pipeline",
                SupportsBatching: true,
                CompatibilityPaths:
                [
                    "/prep"
                ])
        ]);

    public static StageRouteDescriptor Proof { get; } = new(
        StageIds.Proof,
        "Proof",
        "/proof",
        [
            new StageModuleRouteDescriptor(
                ModuleIds.ProofEditing,
                "Editing",
                "/proof/editing",
                SupportsBatching: false,
                CompatibilityPaths:
                [
                    "/proof",
                    ProofChapterCompatibilityTemplate,
                    ProofChapterCanonicalTemplate
                ]),
            new StageModuleRouteDescriptor(
                ModuleIds.ProofOverview,
                "Overview",
                "/proof/overview",
                SupportsBatching: false,
                CompatibilityPaths:
                [
                    "/proof/overview"
                ]),
            new StageModuleRouteDescriptor(
                ModuleIds.ProofPatterns,
                "Patterns",
                "/proof/patterns",
                SupportsBatching: false,
                CompatibilityPaths:
                [
                    "/proof/patterns"
                ])
        ]);

    public static StageRouteDescriptor Polish { get; } = new(
        StageIds.Polish,
        "Polish",
        "/polish",
        [
            new StageModuleRouteDescriptor(
                ModuleIds.PolishScaffold,
                "Scaffold",
                "/polish/scaffold",
                SupportsBatching: false,
                CompatibilityPaths:
                [
                    "/polish",
                    "/polish/pickups",
                    "/polish/batch"
                ])
        ]);

    public static IReadOnlyList<StageRouteDescriptor> Stages { get; } =
    [
        Prep,
        Proof,
        Polish
    ];

    private static readonly IReadOnlyDictionary<string, StageRouteDescriptor> StageById;
    private static readonly IReadOnlyDictionary<string, StageModuleRouteDescriptor> ModuleByStageAndId;
    private static readonly IReadOnlyDictionary<string, StageRouteBinding> ExactPathBindings;
    private static readonly IReadOnlyList<StageRouteBinding> ParameterizedBindings;

    static StageRouteCatalog()
    {
        var stageById = new Dictionary<string, StageRouteDescriptor>(StringComparer.OrdinalIgnoreCase);
        var moduleByStageAndId = new Dictionary<string, StageModuleRouteDescriptor>(StringComparer.OrdinalIgnoreCase);
        var exactPaths = new Dictionary<string, StageRouteBinding>(StringComparer.OrdinalIgnoreCase);
        var parameterized = new List<StageRouteBinding>();

        foreach (var stage in Stages)
        {
            stageById.Add(stage.Id, stage);

            foreach (var module in stage.Modules)
            {
                var moduleLookupKey = CreateModuleLookupKey(stage.Id, module.Id);
                if (!moduleByStageAndId.TryAdd(moduleLookupKey, module))
                {
                    throw new InvalidOperationException($"Duplicate module descriptor registration for stage='{stage.Id}', module='{module.Id}'.");
                }

                RegisterPath(exactPaths, parameterized, stage, module, module.CanonicalPath, isCompatibilityAlias: false);

                foreach (var compatibilityPath in module.CompatibilityPaths)
                {
                    RegisterPath(exactPaths, parameterized, stage, module, compatibilityPath, isCompatibilityAlias: true);
                }
            }
        }

        StageById = new ReadOnlyDictionary<string, StageRouteDescriptor>(stageById);
        ModuleByStageAndId = new ReadOnlyDictionary<string, StageModuleRouteDescriptor>(moduleByStageAndId);
        ExactPathBindings = new ReadOnlyDictionary<string, StageRouteBinding>(exactPaths);
        ParameterizedBindings = parameterized.AsReadOnly();
    }

    public static bool TryGetStage(string? stageId, out StageRouteDescriptor stage)
    {
        stage = default!;

        if (string.IsNullOrWhiteSpace(stageId))
        {
            return false;
        }

        return StageById.TryGetValue(stageId, out stage!);
    }

    public static bool TryGetModule(string? stageId, string? moduleId, out StageModuleRouteDescriptor module)
    {
        module = default!;

        if (string.IsNullOrWhiteSpace(stageId) || string.IsNullOrWhiteSpace(moduleId))
        {
            return false;
        }

        return ModuleByStageAndId.TryGetValue(CreateModuleLookupKey(stageId, moduleId), out module!);
    }

    public static bool TryGetModuleCanonicalPath(string? stageId, string? moduleId, out string canonicalPath)
    {
        canonicalPath = RootPath;

        if (!TryGetModule(stageId, moduleId, out var module))
        {
            return false;
        }

        canonicalPath = module.CanonicalPath;
        return true;
    }

    public static string GetModuleCanonicalPath(string stageId, string moduleId)
    {
        return TryGetModuleCanonicalPath(stageId, moduleId, out var canonicalPath)
            ? canonicalPath
            : GetStageEntryPath(stageId);
    }

    public static string GetStageEntryPath(string stageId)
    {
        return TryGetStage(stageId, out var stage)
            ? stage.EntryPath
            : RootPath;
    }

    public static IReadOnlyList<string> GetCompatibilityPaths(string stageId)
    {
        if (!TryGetStage(stageId, out var stage))
        {
            return Array.Empty<string>();
        }

        return stage.Modules
            .SelectMany(module => module.CompatibilityPaths)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static StageRouteMatch? Resolve(string? path)
    {
        if (!TryNormalizePath(path, out var normalizedPath))
        {
            return null;
        }

        if (ExactPathBindings.TryGetValue(normalizedPath, out var exactMatch))
        {
            return exactMatch.ToMatch(path!, normalizedPath);
        }

        foreach (var parameterizedBinding in ParameterizedBindings)
        {
            if (TemplateMatches(normalizedPath, parameterizedBinding.NormalizedTemplate))
            {
                return parameterizedBinding.ToMatch(path!, normalizedPath);
            }
        }

        return null;
    }

    public static bool IsPathInStage(string? path, string stageId, out StageRouteMatch? match)
    {
        match = Resolve(path);
        return match is not null && string.Equals(match.Stage.Id, stageId, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsPathInStage(string? path, string stageId)
        => IsPathInStage(path, stageId, out _);

    public static string BuildProofChapterCompatibilityPath(string chapterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterName);
        return $"/proof/{Uri.EscapeDataString(chapterName)}";
    }

    public static string BuildProofChapterCanonicalPath(string chapterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapterName);
        return $"/proof/editing/{Uri.EscapeDataString(chapterName)}";
    }

    public static bool IsValidTemplate(string template, out string reason)
    {
        reason = string.Empty;

        if (string.IsNullOrWhiteSpace(template))
        {
            reason = "template is empty";
            return false;
        }

        if (!template.StartsWith('/'))
        {
            reason = "template must start with '/'";
            return false;
        }

        if (!TryNormalizePath(template, out var normalizedTemplate))
        {
            reason = "template could not be normalized";
            return false;
        }

        foreach (var segment in SplitSegments(normalizedTemplate))
        {
            var startsToken = segment.StartsWith('{');
            var endsToken = segment.EndsWith('}');

            if (startsToken != endsToken)
            {
                reason = $"segment '{segment}' has unbalanced token braces";
                return false;
            }

            if (startsToken && segment.Length <= 2)
            {
                reason = "token segment cannot be empty";
                return false;
            }
        }

        return true;
    }

    private static string CreateModuleLookupKey(string stageId, string moduleId)
        => $"{stageId.Trim()}::{moduleId.Trim()}".ToLowerInvariant();

    private static void RegisterPath(
        IDictionary<string, StageRouteBinding> exactPaths,
        ICollection<StageRouteBinding> parameterizedBindings,
        StageRouteDescriptor stage,
        StageModuleRouteDescriptor module,
        string template,
        bool isCompatibilityAlias)
    {
        if (!IsValidTemplate(template, out var reason))
        {
            throw new InvalidOperationException(
                $"Invalid route template '{template}' for stage '{stage.Id}', module '{module.Id}': {reason}.");
        }

        TryNormalizePath(template, out var normalizedTemplate);

        var binding = new StageRouteBinding(
            Stage: stage,
            Module: module,
            OriginalTemplate: template,
            NormalizedTemplate: normalizedTemplate,
            IsCompatibilityAlias: isCompatibilityAlias);

        if (binding.IsParameterizedTemplate)
        {
            var existing = parameterizedBindings
                .OfType<StageRouteBinding>()
                .FirstOrDefault(candidate => string.Equals(candidate.NormalizedTemplate, normalizedTemplate, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                if (!existing.Matches(stage, module))
                {
                    throw new InvalidOperationException(
                        $"Parameterized route template conflict for '{template}': existing stage='{existing.Stage.Id}', module='{existing.Module.Id}', new stage='{stage.Id}', module='{module.Id}'.");
                }

                parameterizedBindings.Remove(existing);
                parameterizedBindings.Add(existing with { IsCompatibilityAlias = existing.IsCompatibilityAlias || isCompatibilityAlias });
                return;
            }

            parameterizedBindings.Add(binding);
            return;
        }

        if (exactPaths.TryGetValue(normalizedTemplate, out var existingExact))
        {
            if (!existingExact.Matches(stage, module))
            {
                throw new InvalidOperationException(
                    $"Exact route template conflict for '{template}': existing stage='{existingExact.Stage.Id}', module='{existingExact.Module.Id}', new stage='{stage.Id}', module='{module.Id}'.");
            }

            exactPaths[normalizedTemplate] = existingExact with
            {
                IsCompatibilityAlias = existingExact.IsCompatibilityAlias || isCompatibilityAlias
            };
            return;
        }

        exactPaths.Add(normalizedTemplate, binding);
    }

    private static bool TryNormalizePath(string? path, out string normalizedPath)
    {
        normalizedPath = string.Empty;

        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var trimmed = path.Trim();
        if (!trimmed.StartsWith('/'))
        {
            return false;
        }

        var fragmentIndex = trimmed.IndexOf('#');
        if (fragmentIndex >= 0)
        {
            trimmed = trimmed[..fragmentIndex];
        }

        var queryIndex = trimmed.IndexOf('?');
        if (queryIndex >= 0)
        {
            trimmed = trimmed[..queryIndex];
        }

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return false;
        }

        if (trimmed.Length > 1)
        {
            trimmed = trimmed.TrimEnd('/');
        }

        if (trimmed.Length == 0)
        {
            normalizedPath = RootPath;
            return true;
        }

        normalizedPath = trimmed.ToLowerInvariant();
        return true;
    }

    private static bool TemplateMatches(string normalizedPath, string normalizedTemplate)
    {
        var pathSegments = SplitSegments(normalizedPath);
        var templateSegments = SplitSegments(normalizedTemplate);

        if (pathSegments.Count != templateSegments.Count)
        {
            return false;
        }

        for (var i = 0; i < templateSegments.Count; i++)
        {
            var templateSegment = templateSegments[i];
            var pathSegment = pathSegments[i];

            if (IsTemplateToken(templateSegment))
            {
                if (string.IsNullOrWhiteSpace(pathSegment))
                {
                    return false;
                }

                continue;
            }

            if (!string.Equals(pathSegment, templateSegment, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static IReadOnlyList<string> SplitSegments(string path)
    {
        if (string.Equals(path, RootPath, StringComparison.Ordinal))
        {
            return Array.Empty<string>();
        }

        return path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool IsTemplateToken(string segment)
        => segment.Length >= 3 && segment[0] == '{' && segment[^1] == '}';

    private sealed record StageRouteBinding(
        StageRouteDescriptor Stage,
        StageModuleRouteDescriptor Module,
        string OriginalTemplate,
        string NormalizedTemplate,
        bool IsCompatibilityAlias)
    {
        public bool IsParameterizedTemplate
            => NormalizedTemplate.Contains('{', StringComparison.Ordinal)
               && NormalizedTemplate.Contains('}', StringComparison.Ordinal);

        public bool Matches(StageRouteDescriptor stage, StageModuleRouteDescriptor module)
            => string.Equals(Stage.Id, stage.Id, StringComparison.OrdinalIgnoreCase)
               && string.Equals(Module.Id, module.Id, StringComparison.OrdinalIgnoreCase);

        public StageRouteMatch ToMatch(string requestedPath, string normalizedPath)
            => new(
                RequestedPath: requestedPath,
                NormalizedPath: normalizedPath,
                Stage: Stage,
                Module: Module,
                MatchedTemplate: OriginalTemplate,
                IsCompatibilityAlias: IsCompatibilityAlias);
    }
}
