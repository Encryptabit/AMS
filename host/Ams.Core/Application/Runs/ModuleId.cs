using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ams.Core.Application.Runs;

public sealed record ModuleId
{
    private static readonly Regex Pattern = new(
        "^[a-z0-9]+(?:[a-z0-9_]*)(?:\\.[a-z0-9]+(?:[a-z0-9_]*)?)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [JsonConstructor]
    public ModuleId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            throw new ArgumentException("Module id cannot include leading or trailing whitespace.", nameof(value));
        }

        if (!Pattern.IsMatch(value))
        {
            throw new ArgumentException(
                "Module id must use dotted lower-case segments containing only a-z, 0-9, and underscores.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;

    public static ModuleId Parse(string value) => new(value);

    public static bool TryCreate(string? value, out ModuleId? moduleId)
    {
        try
        {
            moduleId = value is null ? null : new ModuleId(value);
            return moduleId is not null;
        }
        catch (ArgumentException)
        {
            moduleId = null;
            return false;
        }
    }
}
