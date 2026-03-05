namespace Ams.Core.Application.Mfa.Models;

/// <summary>
/// Predefined beam search profiles for MFA forced alignment.
/// Higher beam values improve alignment quality at the cost of throughput.
/// </summary>
public enum MfaBeamProfile
{
    /// <summary>Beam 20 / retry 80. Fastest throughput, suitable for clean recordings.</summary>
    Fast,

    /// <summary>Beam 40 / retry 120. Default tradeoff between speed and quality.</summary>
    Balanced,

    /// <summary>Beam 80 / retry 200. Maximum precision, used for difficult alignments and adaptive retry.</summary>
    Strict
}

/// <summary>
/// Resolved beam search parameters for an MFA alignment run.
/// </summary>
public sealed record MfaBeamSettings(int Beam, int RetryBeam)
{
    /// <summary>
    /// Resolves beam settings from an optional profile and optional explicit overrides.
    /// Explicit values always supersede profile defaults.
    /// </summary>
    public static MfaBeamSettings Resolve(
        MfaBeamProfile? profile = null,
        int? explicitBeam = null,
        int? explicitRetryBeam = null)
    {
        var (defaultBeam, defaultRetryBeam) = (profile ?? MfaBeamProfile.Balanced) switch
        {
            MfaBeamProfile.Fast => (20, 80),
            MfaBeamProfile.Strict => (80, 200),
            _ => (40, 120) // Balanced
        };

        return new MfaBeamSettings(
            explicitBeam ?? defaultBeam,
            explicitRetryBeam ?? defaultRetryBeam);
    }

    /// <summary>Settings for the strict retry profile, used by adaptive retry logic.</summary>
    public static MfaBeamSettings StrictRetry => Resolve(MfaBeamProfile.Strict);
}
