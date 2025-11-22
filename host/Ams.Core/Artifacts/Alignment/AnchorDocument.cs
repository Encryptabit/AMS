using System.Text.Json.Serialization;

namespace Ams.Core.Artifacts.Alignment;

public sealed record AnchorDocument(
    [property: JsonPropertyName("sectionDetected")]
    bool SectionDetected,
    [property: JsonPropertyName("section")]
    AnchorDocumentSection? Section,
    [property: JsonPropertyName("policy")] AnchorDocumentPolicy Policy,
    [property: JsonPropertyName("tokens")] AnchorDocumentTokenStats Tokens,
    [property: JsonPropertyName("window")] AnchorDocumentWindow Window,
    [property: JsonPropertyName("anchors")]
    IReadOnlyList<AnchorDocumentAnchor> Anchors,
    [property: JsonPropertyName("windows")]
    IReadOnlyList<AnchorDocumentWindowSegment>? Windows);

public sealed record AnchorDocumentSection(
    [property: JsonPropertyName("Id")] int Id,
    [property: JsonPropertyName("Title")] string Title,
    [property: JsonPropertyName("Level")] int Level,
    [property: JsonPropertyName("Kind")] string Kind,
    [property: JsonPropertyName("StartWord")]
    int StartWord,
    [property: JsonPropertyName("EndWord")]
    int EndWord);

public sealed record AnchorDocumentPolicy(
    [property: JsonPropertyName("ngram")] int NGram,
    [property: JsonPropertyName("targetPerTokens")]
    int TargetPerTokens,
    [property: JsonPropertyName("minSeparation")]
    int MinSeparation,
    [property: JsonPropertyName("disallowBoundaryCross")]
    bool DisallowBoundaryCross,
    [property: JsonPropertyName("stopwords")]
    string Stopwords);

public sealed record AnchorDocumentTokenStats(
    [property: JsonPropertyName("bookCount")]
    int BookCount,
    [property: JsonPropertyName("bookFilteredCount")]
    int BookFilteredCount,
    [property: JsonPropertyName("asrCount")]
    int AsrCount,
    [property: JsonPropertyName("asrFilteredCount")]
    int AsrFilteredCount);

public sealed record AnchorDocumentWindow(
    [property: JsonPropertyName("bookStart")]
    int BookStart,
    [property: JsonPropertyName("bookEnd")]
    int BookEnd);

public sealed record AnchorDocumentAnchor(
    [property: JsonPropertyName("bp")] int BookPosition,
    [property: JsonPropertyName("bpWordIndex")]
    int BookWordIndex,
    [property: JsonPropertyName("ap")] int AsrPosition);

public sealed record AnchorDocumentWindowSegment(
    [property: JsonPropertyName("bLo")] int BookLow,
    [property: JsonPropertyName("bHi")] int BookHigh,
    [property: JsonPropertyName("aLo")] int AsrLow,
    [property: JsonPropertyName("aHi")] int AsrHigh);