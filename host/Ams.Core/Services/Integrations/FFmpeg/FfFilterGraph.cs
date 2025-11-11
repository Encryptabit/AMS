using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Processors;

namespace Ams.Core.Services.Integrations.FFmpeg;

/// <summary>
/// Fluent helper for composing libavfilter graphs (ffmpeg/ffplay) on top of <see cref="FfFilterGraphRunner"/>.
/// </summary>
public sealed class FfFilterGraph
{
    private readonly List<FfFilterGraphRunner.GraphInput> _inputs = new();
    private readonly List<string> _clauses = new();
    private string _inputLabel;
    private string _outputLabel = "out";
    private bool _customGraphOverride;
    private bool _formatPinned;

    private FfFilterGraph(AudioBuffer buffer, string label)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (string.IsNullOrWhiteSpace(label))
        {
            label = "main";
        }

        _inputLabel = label;
        AddInput(buffer, label);
    }

    /// <summary>
    /// Begin a fluent graph with a single <see cref="AudioBuffer"/>.
    /// </summary>
    public static FfFilterGraph FromBuffer(AudioBuffer buffer, string? label = null) =>
        new(buffer, label ?? "main");

    /// <summary>
    /// Register another labeled input buffer (useful for sidechains).
    /// </summary>
    public FfFilterGraph WithInput(AudioBuffer buffer, string label)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Label must be provided.", nameof(label));
        }

        if (_inputs.Any(i => string.Equals(i.Label, label, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Input '{label}' already registered.");
        }

        AddInput(buffer, label);
        return this;
    }

    /// <summary>
    /// Selects which labeled input feeds the next chain (defaults to "main").
    /// </summary>
    public FfFilterGraph UseInput(string label)
    {
        if (!_inputs.Any(i => string.Equals(i.Label, label, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Input '{label}' has not been registered.");
        }

        _inputLabel = label;
        return this;
    }

    /// <summary>
    /// Override the final output label (defaults to "out").
    /// </summary>
    public FfFilterGraph WithOutputLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Output label must be provided.", nameof(label));
        }

        _outputLabel = label;
        return this;
    }

    /// <summary>
    /// Ensure downstream filters see a consistent format/layout.
    /// Uses libavfilter's <c>aformat</c> (ffmpeg <c>-af aformat</c>).
    /// </summary>
    public FfFilterGraph AFormat(string? sampleFormats = "flt", string? channelLayouts = null, int? sampleRate = null)
    {
        var args = new List<(string Key, string? Value)>
        {
            ("sample_fmts", sampleFormats)
        };
        if (!string.IsNullOrWhiteSpace(channelLayouts))
        {
            args.Add(("channel_layouts", channelLayouts));
        }
        if (sampleRate.HasValue)
        {
            args.Add(("sample_rates", FormatDouble(sampleRate.Value)));
        }
        return AddFilter("aformat", args, markFormatPinned: true);
    }

    /// <summary>
    /// High-pass filter (libavfilter <c>highpass</c>, ffmpeg <c>-af highpass=f=...</c>).
    /// </summary>
    public FfFilterGraph HighPass(double frequencyHz = 70, double poles = 2) =>
        HighPass(new HighPassFilterParams(frequencyHz, poles));

    public FfFilterGraph HighPass(HighPassFilterParams? parameters)
    {
        var p = parameters ?? new HighPassFilterParams();
        return AddFilter("highpass",
            ("frequency", FormatDouble(p.Frequency)),
            ("poles", FormatDouble(Math.Clamp(p.Poles, 1, 2))));
    }

    /// <summary>
    /// Low-pass filter (libavfilter <c>lowpass</c>).
    /// </summary>
    public FfFilterGraph LowPass(double frequencyHz = 12000, double poles = 2) =>
        LowPass(new LowPassFilterParams(frequencyHz, poles));

    public FfFilterGraph LowPass(LowPassFilterParams? parameters)
    {
        var p = parameters ?? new LowPassFilterParams();
        return AddFilter("lowpass",
            ("frequency", FormatDouble(p.Frequency)),
            ("poles", FormatDouble(Math.Clamp(p.Poles, 1, 2))));
    }

    /// <summary>
    /// Simple de-esser (libavfilter <c>deesser</c>).
    /// </summary>
    public FfFilterGraph DeEsser(double normalizedFrequency = 0.5, double intensity = 0, double maxReduction = 0.5, string outputMode = "o") =>
        DeEsser(new DeEsserFilterParams(normalizedFrequency, intensity, maxReduction, outputMode));

    public FfFilterGraph DeEsser(DeEsserFilterParams? parameters)
    {
        var p = parameters ?? new DeEsserFilterParams();
        return AddFilter("deesser",
            ("f", FormatFraction(p.NormalizedFrequency)),
            ("i", FormatFraction(p.Intensity)),
            ("m", FormatFraction(p.MaxReduction)),
            ("s", p.OutputMode));
    }

    /// <summary>
    /// Frequency-domain denoise (libavfilter <c>afftdn</c>).
    /// </summary>
    public FfFilterGraph FftDenoise(double noiseReductionDb = 12) =>
        FftDenoise(new FftDenoiseFilterParams(noiseReductionDb));

    public FfFilterGraph FftDenoise(FftDenoiseFilterParams? parameters)
    {
        var p = parameters ?? new FftDenoiseFilterParams();
        return AddFilter("afftdn", ("nr", FormatDouble(p.NoiseReductionDb)));
    }

    /// <summary>
    /// Neural denoiser (libavfilter <c>arnndn</c>).
    /// </summary>
    public FfFilterGraph NeuralDenoise(string model = "rnnoise") =>
        NeuralDenoise(new NeuralDenoiseFilterParams(model));

    public FfFilterGraph NeuralDenoise(NeuralDenoiseFilterParams? parameters)
    {
        var p = parameters ?? new NeuralDenoiseFilterParams();
        return AddFilter("arnndn", ("model", p.Model));
    }

    /// <summary>
    /// Gentle compressor (libavfilter <c>acompressor</c>).
    /// </summary>
    public FfFilterGraph ACompressor(double thresholdDb = -18, double ratio = 2.0, double attackMs = 10, double releaseMs = 100, double makeupDb = 2.0) =>
        ACompressor(new ACompressorFilterParams(thresholdDb, ratio, attackMs, releaseMs, makeupDb));

    public FfFilterGraph ACompressor(ACompressorFilterParams? parameters)
    {
        var p = parameters ?? new ACompressorFilterParams();
        return AddFilter("acompressor",
            ("threshold", FormatDecibels(p.ThresholdDb)),
            ("ratio", FormatDouble(p.Ratio)),
            ("attack", FormatDouble(p.AttackMilliseconds)),
            ("release", FormatDouble(p.ReleaseMilliseconds)),
            ("makeup", FormatDecibels(p.MakeupDb)));
    }

    /// <summary>
    /// Safety limiter (libavfilter <c>alimiter</c>).
    /// </summary>
    public FfFilterGraph ALimiter(double limitDb = -3, double attack = 5, double release = 50) =>
        ALimiter(new ALimiterFilterParams(limitDb, attack, release));

    public FfFilterGraph ALimiter(ALimiterFilterParams? parameters)
    {
        var p = parameters ?? new ALimiterFilterParams();
        return AddFilter("alimiter",
            ("limit", FormatDecibels(p.LimitDb)),
            ("attack", FormatDouble(p.AttackMilliseconds)),
            ("release", FormatDouble(p.ReleaseMilliseconds)));
    }

    /// <summary>
    /// Loudness normalization (libavfilter <c>loudnorm</c>).
    /// </summary>
    public FfFilterGraph LoudNorm(double targetI = -18, double targetLra = 7, double targetTp = -2, bool dualMono = true) =>
        LoudNorm(new LoudNormFilterParams(targetI, targetLra, targetTp, dualMono));

    public FfFilterGraph LoudNorm(LoudNormFilterParams? parameters)
    {
        var p = parameters ?? new LoudNormFilterParams();
        return AddFilter("loudnorm",
            ("I", FormatDouble(p.TargetI)),
            ("LRA", FormatDouble(p.TargetLra)),
            ("TP", FormatDouble(p.TargetTp)),
            ("dual_mono", p.DualMono ? "1" : "0"));
    }

    public FfFilterGraph Resample(ResampleFilterParams? parameters) =>
        AddRawFilter("aresample", $"{parameters.SampleRate}");

    /// <summary>
    /// Silence trimming (libavfilter <c>silenceremove</c>).
    /// </summary>
    public FfFilterGraph SilenceRemove(string args = "start_periods=0:start_threshold=-50dB:stop_periods=0:stop_threshold=-50dB") =>
        AddRawFilter("silenceremove", args);

    public FfFilterGraph SilenceRemove(SilenceRemoveFilterParams parameters)
        => AddRawFilter("silenceremove",
            $"start_periods={parameters.StartPeriods}:start_threshold={parameters.StartThreshold}:stop_periods={parameters.StopPeriods}:stop_threshold={parameters.StopThreshold}");

    /// <summary>
    /// Measurement helper (libavfilter <c>astats</c>).
    /// </summary>
    public FfFilterGraph AStats(string args = "metadata=1:reset=1") =>
        AddRawFilter("astats", args);

    public FfFilterGraph AStats(AStatsFilterParams parameters)
        => AddRawFilter("astats", $"metadata={(parameters.EmitMetadata ? 1 : 0)}:reset={parameters.ResetInterval}");

    /// <summary>
    /// Measurement helper (libavfilter <c>ebur128</c>).
    /// </summary>
    public FfFilterGraph EbuR128(string args = "framelog=verbose") =>
        AddRawFilter("ebur128", args);

    public FfFilterGraph EbuR128(EbuR128FilterParams parameters)
        => AddRawFilter("ebur128", $"framelog={parameters.FrameLog}");

    /// <summary>
    /// Inject a raw filter clause when fluent helpers are insufficient.
    /// </summary>
    public FfFilterGraph Custom(string rawClause)
    {
        if (string.IsNullOrWhiteSpace(rawClause))
        {
            throw new ArgumentException("Filter clause must be provided.", nameof(rawClause));
        }

        _clauses.Add(rawClause);
        return this;
    }

    /// <summary>
    /// Provide the entire filtergraph manually (bypasses fluent clauses).
    /// </summary>
    public FfFilterGraph UseCustomGraph(string filterGraph)
    {
        if (string.IsNullOrWhiteSpace(filterGraph))
        {
            throw new ArgumentException("Filter graph must be provided.", nameof(filterGraph));
        }

        _customGraphOverride = true;
        _clauses.Clear();
        _clauses.Add(filterGraph);
        return this;
    }

    /// <summary>
    /// Build the filter spec string (labels + filter chain).
    /// </summary>
    public string BuildSpec()
    {
        if (_customGraphOverride)
        {
            return _clauses.Count == 0 ? string.Empty : _clauses[0];
        }

        EnsureDefaultFormatClause();
        var chain = _clauses.Count == 0 ? "anull" : string.Join(",", _clauses);
        return $"[{_inputLabel}]{chain}[{_outputLabel}]";
    }

    /// <summary>
    /// Execute the composed graph and return a new <see cref="AudioBuffer"/>.
    /// </summary>
    public AudioBuffer ToBuffer()
    {
        var spec = BuildSpec();
        return FfFilterGraphRunner.Apply(BuildInputs(), spec);
    }

    /// <summary>
    /// Execute the graph in discard-output mode (useful for measurement filters).
    /// </summary>
    public void RunDiscardingOutput()
    {
        var spec = BuildSpec();
        FfFilterGraphRunner.Execute(BuildInputs(), spec, FfFilterGraphRunner.FilterExecutionMode.DiscardOutput);
    }

    /// <summary>
    /// Run the graph while capturing FFmpeg log output (via <see cref="FfLogCapture"/>).
    /// </summary>
    public IReadOnlyList<string> CaptureLogs()
    {
        var spec = BuildSpec();
        return FfLogCapture.Capture(() =>
        {
            FfFilterGraphRunner.Execute(BuildInputs(), spec, FfFilterGraphRunner.FilterExecutionMode.DiscardOutput);
        });
    }

    /// <summary>
    /// Execute the graph in measurement mode and parse the collected logs.
    /// </summary>
    public T Measure<T>(Func<IReadOnlyList<string>, T> parser)
    {
        ArgumentNullException.ThrowIfNull(parser);
        var logs = CaptureLogs();
        return parser(logs);
    }

    public void StreamToWave(Stream output, AudioEncodeOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(output);
        var spec = BuildSpec();
        using var sink = FfEncoder.CreateStreamingSink(output, options ?? new AudioEncodeOptions());
        FfFilterGraphRunner.Stream(BuildInputs(), spec, sink);
    }

    public FfFilterGraph Gain(double multiplier = 1.0) =>
        Gain(new GainFilterParams(multiplier));

    public FfFilterGraph Gain(GainFilterParams? parameters)
    {
        var p = parameters ?? new GainFilterParams();
        return AddFilter("volume", ("volume", FormatDouble(p.Multiplier)));
    }

    private IReadOnlyList<FfFilterGraphRunner.GraphInput> BuildInputs()
    {
        if (_inputs.Count == 0)
        {
            throw new InvalidOperationException("At least one input buffer must be registered.");
        }

        return _inputs;
    }

    private FfFilterGraph AddFilter(string name, params (string Key, string? Value)[] kv) =>
        AddFilter(name, kv.AsEnumerable(),
            markFormatPinned: string.Equals(name, "aformat", StringComparison.OrdinalIgnoreCase));

    private FfFilterGraph AddFilter(string name, IEnumerable<(string Key, string? Value)> kvPairs, bool markFormatPinned = false)
    {
        if (_customGraphOverride)
        {
            throw new InvalidOperationException("Cannot add fluent filters after setting a custom graph.");
        }

        if (markFormatPinned)
        {
            _formatPinned = true;
        }

        var args = SerializeArguments(kvPairs);
        _clauses.Add(string.IsNullOrWhiteSpace(args) ? name : $"{name}={args}");
        return this;
    }

    private FfFilterGraph AddRawFilter(string name, string rawArgs)
    {
        if (_customGraphOverride)
        {
            throw new InvalidOperationException("Cannot add fluent filters after setting a custom graph.");
        }

        if (string.Equals(name, "aformat", StringComparison.OrdinalIgnoreCase))
        {
            _formatPinned = true;
        }

        _clauses.Add(string.IsNullOrWhiteSpace(rawArgs) ? name : $"{name}={rawArgs}");
        return this;
    }

    private void AddInput(AudioBuffer buffer, string label) =>
        _inputs.Add(new FfFilterGraphRunner.GraphInput(label, buffer));

    private static string SerializeArguments(IEnumerable<(string Key, string? Value)> kvPairs)
    {
        var parts = new List<string>();
        foreach (var (key, value) in kvPairs)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            parts.Add($"{key}={Escape(value!)}");
        }

        return string.Join(":", parts);
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value
            .Replace(@"\", @"\\")
            .Replace(":", @"\:")
            .Replace(",", @"\,")
            .Replace(";", @"\;")
            .Replace("[", @"\[")
            .Replace("]", @"\]")
            .Replace("'", @"\'");
    }

    private static string FormatDouble(double value) => FfUtils.FormatNumber(value);
    private static string FormatDecibels(double value) => FfUtils.FormatDecibels(value);
    private static string FormatFraction(double value) => FfUtils.FormatFraction(value);

    private void EnsureDefaultFormatClause()
    {
        if (_customGraphOverride || _formatPinned)
        {
            return;
        }

        _clauses.Insert(0, "aformat=sample_fmts=flt");
        _formatPinned = true;
    }
}

