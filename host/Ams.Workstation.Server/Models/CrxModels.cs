using System;
using System.Linq;

namespace Ams.Workstation.Server.Models;

public record CrxEntry(
    int ErrorNumber,
    string Chapter,
    string Timecode,          // HH:MM:SS format
    string ErrorType,         // MR, PRON, DIC, NZ, PL, DIST, MW, ML, TYPO, CHAR
    string Comments,
    int SentenceId,
    double StartTime,
    double EndTime,
    string AudioFile,
    DateTime CreatedAt
);

public record CrxSubmitRequest(
    double Start,
    double End,
    int SentenceId,
    string ErrorType,
    string Comments,
    string Excerpt,
    int PaddingMs = 50
);

public record CrxSubmitResult(
    bool Success,
    int ErrorNumber,
    string Timecode,
    string AudioFile,
    string? Error = null
);

public static class ErrorTypes
{
    public static readonly string[] All = new[]
    {
        "MR",    // Misread
        "PRON",  // Pronunciation
        "DIC",   // Diction
        "NZ",    // Noise
        "PL",    // Plosive
        "DIST",  // Distortion
        "MW",    // Missing Word
        "ML",    // Missing Line
        "TYPO",  // Typo in script
        "CHAR"   // Character voice
    };
}
