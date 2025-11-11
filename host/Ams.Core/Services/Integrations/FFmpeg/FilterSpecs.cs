namespace Ams.Core.Services.Integrations.FFmpeg;

public sealed record HighPassFilterParams(double Frequency = 70, double Poles = 2);


public sealed record ResampleFilterParams(ulong SampleRate = 48000);



public sealed record LowPassFilterParams(double Frequency = 12000, double Poles = 2);

public sealed record DeEsserFilterParams(
    double NormalizedFrequency = 0.5,
    double Intensity = 0.0,
    double MaxReduction = 0.5,
    string OutputMode = "o");

public sealed record FftDenoiseFilterParams(double NoiseReductionDb = 12);

public sealed record NeuralDenoiseFilterParams(string Model = "rnnoise");

public sealed record ACompressorFilterParams(
    double ThresholdDb = -18,
    double Ratio = 2.0,
    double AttackMilliseconds = 10,
    double ReleaseMilliseconds = 100,
    double MakeupDb = 2.0);

public sealed record ALimiterFilterParams(
    double LimitDb = -3,
    double AttackMilliseconds = 5,
    double ReleaseMilliseconds = 50);

public sealed record LoudNormFilterParams(
    double TargetI = -18,
    double TargetLra = 7,
    double TargetTp = -2,
    bool DualMono = false);

public sealed record SilenceRemoveFilterParams(
    int StartPeriods = 0,
    string StartThreshold = "-50dB",
    int StopPeriods = 0,
    string StopThreshold = "-50dB");
public sealed record AStatsFilterParams(bool EmitMetadata = true, int ResetInterval = 1);

public sealed record EbuR128FilterParams(string FrameLog = "verbose");

public sealed record GainFilterParams(double Multiplier = 1.0);
