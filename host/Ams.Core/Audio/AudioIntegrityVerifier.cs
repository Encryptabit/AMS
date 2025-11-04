using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ams.Core.Audio
{
    /// <summary>
    /// Options for the timing-aware integrity verifier.
    /// </summary>
    public sealed class AudioVerifierOptions
    {
        public int TargetSampleRateHz { get; init; } = 16_000;
        public double FrameMs { get; init; } = 25.0;
        public double HopMs { get; init; } = 10.0;
        public int MfccCount { get; init; } = 13;            // 13 static MFCCs (no Δ/ΔΔ to keep it lean)
        public int MelBands { get; init; } = 26;
        public double FMinHz { get; init; } = 50.0;
        public double FMaxHz { get; init; } = 8000.0;        // clamped to Nyquist internally
        public double SilenceDb { get; init; } = -45.0;      // trim low-energy fringe inside a sentence
        public double TrimGuardMs { get; init; } = 15.0;     // don’t over-trim into phones at edges
        public double DtwBandRatio { get; init; } = 0.15;    // Sakoe–Chiba ±15% band
        public double SentenceMinDurSec { get; init; } = 0.25;
        public double PassThreshold { get; init; } = 0.30;   // normalized DTW cost (0=identical)
        public double PassFraction { get; init; } = 0.95;    // % sentences that must pass
        public bool DumpWorstSentences { get; init; } = true;
        public int WorstCount { get; init; } = 10;
        public int MaxDegreeOfParallelism { get; init; } = Math.Max(1, Environment.ProcessorCount - 1);
    }

    /// <summary>
    /// High-level report for a chapter/file pair.
    /// </summary>
    public sealed class AudioIntegrityReport
    {
        public required string ReferencePath { get; init; }
        public required string VariantPath { get; init; }
        public required int MatchedSentences { get; init; }
        public required int TotalComparableSentences { get; init; }
        public required double PassThreshold { get; init; }
        public required double PassFractionRequired { get; init; }
        public required double PassFractionObserved { get; init; }
        public required bool Passed { get; init; }
        public required List<SentenceMatch> Sentences { get; init; }
        public List<SentenceMatch> WorstByCost { get; set; } = new();
        public string VariantLabel { get; set; } = string.Empty;
    }

    /// <summary>
    /// Per-sentence comparison.
    /// </summary>
    public sealed class SentenceMatch
    {
        public required int SentenceId { get; init; }
        public required (double start, double end) RefBounds { get; init; }
        public required (double start, double end) VarBounds { get; init; }
        public required double RefDuration { get; init; }
        public required double VarDuration { get; init; }
        public required double NormalizedCost { get; init; } // 0=identical (after warping), higher = worse
        public required bool Passed { get; init; }
        public string? Note { get; init; }
    }

    /// <summary>
    /// Entry point – verify two WAV files guided by hydrate JSONs containing sentence IDs and timings.
    /// </summary>
    public static class AudioIntegrityVerifier 
    {

public static AudioIntegrityReport Verify(
    string referenceWav,
    string variantWav,
    string referenceHydrateJson,
    string variantHydrateJson,
    AudioVerifierOptions? options = null)
{
    if (string.IsNullOrWhiteSpace(referenceWav)) throw new ArgumentNullException(nameof(referenceWav));
    if (string.IsNullOrWhiteSpace(variantWav)) throw new ArgumentNullException(nameof(variantWav));
    if (string.IsNullOrWhiteSpace(referenceHydrateJson)) throw new ArgumentNullException(nameof(referenceHydrateJson));
    if (string.IsNullOrWhiteSpace(variantHydrateJson)) throw new ArgumentNullException(nameof(variantHydrateJson));

    options ??= new AudioVerifierOptions();

    var refAudio = WavReader.ReadMono(referenceWav);
    var varAudio = WavReader.ReadMono(variantWav);

    var refPcm = AudioUtil.ResampleMono(refAudio.Pcm, refAudio.SampleRate, options.TargetSampleRateHz);
    var varPcm = AudioUtil.ResampleMono(varAudio.Pcm, varAudio.SampleRate, options.TargetSampleRateHz);

    var refHyd = HydrateSlim.Parse(File.ReadAllText(referenceHydrateJson));
    var varHyd = HydrateSlim.Parse(File.ReadAllText(variantHydrateJson));

    var byIdRef = refHyd.Sentences.ToDictionary(s => s.Id);
    var byIdVar = varHyd.Sentences.ToDictionary(s => s.Id);

    var comparableIds = byIdRef.Keys.Intersect(byIdVar.Keys).ToArray();
    Array.Sort(comparableIds);

    var results = new ConcurrentBag<SentenceMatch>();

    var parallelOptions = new ParallelOptions
    {
        MaxDegreeOfParallelism = Math.Max(1, options.MaxDegreeOfParallelism)
    };

    Parallel.ForEach(
        comparableIds,
        parallelOptions,
        () => new MfccExtractor(
            sampleRate: options.TargetSampleRateHz,
            frameMs: options.FrameMs,
            hopMs: options.HopMs,
            melBands: options.MelBands,
            mfccCount: options.MfccCount,
            fmin: options.FMinHz,
            fmax: options.FMaxHz),
        (sid, _, localFe) =>
        {
            var rs = byIdRef[sid];
            var vs = byIdVar[sid];

            if (double.IsNaN(rs.StartSec) || double.IsNaN(rs.EndSec) || double.IsNaN(vs.StartSec) || double.IsNaN(vs.EndSec))
            {
                results.Add(new SentenceMatch
                {
                    SentenceId = sid,
                    RefBounds = (rs.StartSec, rs.EndSec),
                    VarBounds = (vs.StartSec, vs.EndSec),
                    RefDuration = double.NaN,
                    VarDuration = double.NaN,
                    NormalizedCost = double.NaN,
                    Passed = true,
                    Note = "Skipped: missing timing"
                });
                return localFe;
            }

            double rdur = Math.Max(0, rs.EndSec - rs.StartSec);
            double vdur = Math.Max(0, vs.EndSec - vs.StartSec);
            if (rdur < options.SentenceMinDurSec || vdur < options.SentenceMinDurSec)
            {
                results.Add(new SentenceMatch
                {
                    SentenceId = sid,
                    RefBounds = (rs.StartSec, rs.EndSec),
                    VarBounds = (vs.StartSec, vs.EndSec),
                    RefDuration = rdur,
                    VarDuration = vdur,
                    NormalizedCost = double.NaN,
                    Passed = true,
                    Note = "Skipped: too short"
                });
                return localFe;
            }

            var rseg = AudioUtil.SliceSeconds(refPcm, options.TargetSampleRateHz, rs.StartSec, rs.EndSec);
            var vseg = AudioUtil.SliceSeconds(varPcm, options.TargetSampleRateHz, vs.StartSec, vs.EndSec);

            var rtrim = AudioUtil.TrimSilenceDb(rseg, options.TargetSampleRateHz, options.SilenceDb, options.TrimGuardMs);
            var vtrim = AudioUtil.TrimSilenceDb(vseg, options.TargetSampleRateHz, options.SilenceDb, options.TrimGuardMs);

            var rfeat = localFe.ExtractSequence(rtrim, cmvn: true);
            var vfeat = localFe.ExtractSequence(vtrim, cmvn: true);

            if (rfeat.Count == 0 || vfeat.Count == 0)
            {
                results.Add(new SentenceMatch
                {
                    SentenceId = sid,
                    RefBounds = (rs.StartSec, rs.EndSec),
                    VarBounds = (vs.StartSec, vs.EndSec),
                    RefDuration = rdur,
                    VarDuration = vdur,
                    NormalizedCost = double.NaN,
                    Passed = true,
                    Note = "Skipped: empty features after trimming"
                });
                return localFe;
            }

            int maxLen = Math.Max(rfeat.Count, vfeat.Count);
            int band = Math.Max(1, (int)Math.Round(options.DtwBandRatio * maxLen));
            double cost = Dtw.CosineBand(rfeat, vfeat, band);
            bool pass = cost <= options.PassThreshold;

            results.Add(new SentenceMatch
            {
                SentenceId = sid,
                RefBounds = (rs.StartSec, rs.EndSec),
                VarBounds = (vs.StartSec, vs.EndSec),
                RefDuration = rdur,
                VarDuration = vdur,
                NormalizedCost = cost,
                Passed = pass,
                Note = null
            });

            return localFe;
        },
        _ => { });

    var ordered = results
        .OrderBy(sm => sm.SentenceId)
        .ToList();

    int comparable = ordered.Count(sm => !double.IsNaN(sm.NormalizedCost));
    int passed = ordered.Count(sm => !double.IsNaN(sm.NormalizedCost) && sm.Passed);
    double frac = comparable == 0 ? 1.0 : (double)passed / comparable;
    bool overall = frac >= options.PassFraction;

    var report = new AudioIntegrityReport
    {
        ReferencePath = referenceWav,
        VariantPath = variantWav,
        MatchedSentences = passed,
        TotalComparableSentences = comparable,
        PassThreshold = options.PassThreshold,
        PassFractionRequired = options.PassFraction,
        PassFractionObserved = frac,
        Passed = overall,
        Sentences = ordered
    };

    if (options.DumpWorstSentences && comparable > 0)
    {
        report.WorstByCost = ordered
            .Where(r => !double.IsNaN(r.NormalizedCost))
            .OrderByDescending(r => r.NormalizedCost)
            .Take(options.WorstCount)
            .ToList();
    }

    return report;
}
        private sealed class HydrateSlim
        {
            public required List<HydSentence> Sentences { get; init; }

            public static HydrateSlim Parse(string json)
            {
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Find "sentences" array (case-insensitive)
                var sentencesProp = root.EnumerateObject()
                    .FirstOrDefault(p => string.Equals(p.Name, "sentences", StringComparison.OrdinalIgnoreCase));

                if (sentencesProp.Value.ValueKind != JsonValueKind.Array)
                    throw new InvalidOperationException("Hydrate JSON missing 'sentences' array.");

                var list = new List<HydSentence>();
                foreach (var el in sentencesProp.Value.EnumerateArray())
                {
                    int id = GetInt(el, "id");
                    var (start, end) = ReadTiming(el);
                    list.Add(new HydSentence { Id = id, StartSec = start, EndSec = end });
                }

                return new HydrateSlim { Sentences = list };
            }

            private static (double start, double end) ReadTiming(JsonElement sentence)
            {
                // Accept several spellings:
                // 1) sentence.Timing.StartSec / EndSec
                // 2) sentence.Timing.Start / End
                // 3) sentence.start / sentence.end
                // 4) sentence.startSec / sentence.endSec
                if (TryGetElement(sentence, "timing", out var timing) && timing.ValueKind == JsonValueKind.Object)
                {
                    if (TryGetDouble(timing, "StartSec", out var s1) && TryGetDouble(timing, "EndSec", out var e1))
                        return (s1, e1);
                    if (TryGetDouble(timing, "Start", out var s2) && TryGetDouble(timing, "End", out var e2))
                        return (s2, e2);
                }

                if (TryGetDouble(sentence, "startSec", out var s3) && TryGetDouble(sentence, "endSec", out var e3))
                    return (s3, e3);
                if (TryGetDouble(sentence, "start", out var s4) && TryGetDouble(sentence, "end", out var e4))
                    return (s4, e4);

                throw new InvalidOperationException("Hydrate sentence is missing timing.");
            }

            private static int GetInt(JsonElement el, string name)
            {
                if (TryGetElementCI(el, name, out var v) && v.TryGetInt32(out var x)) return x;
                throw new InvalidOperationException($"Missing int property '{name}'.");
            }
            private static bool TryGetDouble(JsonElement el, string name, out double value)
            {
                value = 0;
                if (TryGetElementCI(el, name, out var v))
                {
                    if (v.ValueKind == JsonValueKind.Number) { value = v.GetDouble(); return true; }
                    if (v.ValueKind == JsonValueKind.String && double.TryParse(v.GetString(), out var d)) { value = d; return true; }
                }
                return false;
            }
            private static bool TryGetElement(JsonElement el, string name, out JsonElement value)
            {
                foreach (var p in el.EnumerateObject()) if (p.Name == name) { value = p.Value; return true; }
                value = default; return false;
            }
            private static bool TryGetElementCI(JsonElement el, string name, out JsonElement value)
            {
                foreach (var p in el.EnumerateObject())
                    if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) { value = p.Value; return true; }
                value = default; return false;
            }
        }

        private sealed class HydSentence
        {
            public required int Id { get; init; }
            public required double StartSec { get; init; }
            public required double EndSec { get; init; }
        }

        // -------------------------------------- WAV I/O ---------------------------------------------------

        private sealed class WavData
        {
            public required int SampleRate { get; init; }
            public required float[] Pcm { get; init; } // mono [-1,1]
        }

        private static class WavReader
        {
            public static WavData ReadMono(string path)
            {
                using var br = new BinaryReader(File.OpenRead(path));
                // RIFF header
                var riff = new string(br.ReadChars(4));
                if (riff != "RIFF") throw new InvalidDataException("Not a RIFF file.");
                br.ReadInt32(); // file size
                var wave = new string(br.ReadChars(4));
                if (wave != "WAVE") throw new InvalidDataException("Not a WAVE file.");

                int channels = 0, sampleRate = 0, bits = 0;
                long dataPos = -1; int dataBytes = 0; short formatTag = 1;

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var id = new string(br.ReadChars(4));
                    int size = br.ReadInt32();
                    long next = br.BaseStream.Position + size;

                    if (id == "fmt ")
                    {
                        formatTag = br.ReadInt16(); // 1=PCM, 3=float
                        channels = br.ReadInt16();
                        sampleRate = br.ReadInt32();
                        br.ReadInt32(); // byte rate
                        br.ReadInt16(); // block align
                        bits = br.ReadInt16();
                        if (size > 16) br.BaseStream.Position = next; // skip extras
                    }
                    else if (id == "data")
                    {
                        dataPos = br.BaseStream.Position;
                        dataBytes = size;
                        br.BaseStream.Position = next;
                    }
                    else
                    {
                        br.BaseStream.Position = next;
                    }
                }

                if (dataPos < 0) throw new InvalidDataException("Missing data chunk.");
                if (channels <= 0 || sampleRate <= 0) throw new InvalidDataException("Invalid fmt chunk.");

                br.BaseStream.Position = dataPos;
                int totalSamples = dataBytes * 8 / (bits * channels);
                var interleaved = new float[totalSamples * channels];

                if (formatTag == 1 && bits == 16)
                {
                    for (int i = 0; i < interleaved.Length; i++)
                        interleaved[i] = br.ReadInt16() / 32768f;
                }
                else if (formatTag == 3 && bits == 32)
                {
                    for (int i = 0; i < interleaved.Length; i++)
                        interleaved[i] = br.ReadSingle();
                }
                else
                {
                    throw new NotSupportedException($"Unsupported WAV format (tag={formatTag}, bits={bits}).");
                }

                // to mono
                var mono = new float[totalSamples];
                if (channels == 1)
                {
                    Array.Copy(interleaved, mono, mono.Length);
                }
                else
                {
                    for (int n = 0, i = 0; n < totalSamples; n++)
                    {
                        double sum = 0;
                        for (int ch = 0; ch < channels; ch++) sum += interleaved[i++];
                        mono[n] = (float)(sum / channels);
                    }
                }

                return new WavData { SampleRate = sampleRate, Pcm = mono };
            }
        }

        // ------------------------------------ DSP helpers -------------------------------------------------

        private static class AudioUtil
        {
            public static float[] ResampleMono(float[] x, int srIn, int srOut)
            {
                if (srIn == srOut) return (float[])x.Clone();
                if (x.Length == 0) return Array.Empty<float>();

                double ratio = (double)srOut / srIn;
                int outLen = Math.Max(1, (int)Math.Round(x.Length * ratio));
                var y = new float[outLen];

                // Linear interpolation (sufficient for MFCC front-end)
                for (int i = 0; i < outLen; i++)
                {
                    double pos = i / ratio;
                    int i0 = (int)Math.Floor(pos);
                    int i1 = Math.Min(x.Length - 1, i0 + 1);
                    double t = pos - i0;
                    double v = (1 - t) * x[i0] + t * x[i1];
                    y[i] = (float)v;
                }
                return y;
            }

            public static float[] SliceSeconds(float[] x, int sr, double startSec, double endSec)
            {
                int a = Clamp((int)Math.Round(startSec * sr), 0, x.Length);
                int b = Clamp((int)Math.Round(endSec * sr), a, x.Length);
                int n = b - a;
                var y = new float[n];
                Array.Copy(x, a, y, 0, n);
                return y;
            }

            /// <summary>Trim low-energy edges inside a sentence region with guard.</summary>
            public static float[] TrimSilenceDb(float[] x, int sr, double threshDb, double guardMs)
            {
                if (x.Length == 0) return x;
                double thresh = Math.Pow(10.0, threshDb / 20.0); // linear
                int guard = (int)Math.Round(guardMs * 0.001 * sr);
                int a = 0, b = x.Length - 1;

                // simple RMS window ~= 20 ms
                int win = Math.Max(8, (int)Math.Round(0.020 * sr));
                double sum = 0;
                for (int i = 0; i < Math.Min(win, x.Length); i++) sum += x[i] * x[i];

                int start = 0;
                for (int i = win; i < x.Length; i++)
                {
                    double rms = Math.Sqrt(sum / win);
                    if (rms > thresh) { start = i - win; break; }
                    sum += x[i] * x[i] - x[i - win] * x[i - win];
                }

                int end = x.Length - 1;
                sum = 0;
                for (int i = x.Length - 1; i >= Math.Max(0, x.Length - win); i--) sum += x[i] * x[i];

                for (int i = x.Length - 1 - win; i >= 0; i--)
                {
                    double rms = Math.Sqrt(sum / win);
                    if (rms > thresh) { end = i + win; break; }
                    sum += x[i] * x[i] - x[i + win] * x[i + win];
                }

                a = Clamp(start - guard, 0, x.Length);
                b = Clamp(end + guard, a, x.Length);
                int n = b - a;
                var y = new float[n];
                Array.Copy(x, a, y, 0, n);
                return y;
            }

            private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
        }

        // -------------------------------- MFCC + DTW implementation --------------------------------------

        private sealed class MfccExtractor
        {
            private readonly int _sr;
            private readonly int _win;
            private readonly int _hop;
            private readonly int _nfft;
            private readonly double[] _hann;
            private readonly double[][] _mel; // mel filterbank [melBands][nfft/2+1]
            private readonly int _melBands;
            private readonly int _mfccCount;

            public MfccExtractor(int sampleRate, double frameMs, double hopMs, int melBands, int mfccCount, double fmin, double fmax)
            {
                _sr = sampleRate;
                _win = (int)Math.Round(frameMs * 0.001 * sampleRate);
                _hop = (int)Math.Round(hopMs * 0.001 * sampleRate);
                _nfft = 1;
                while (_nfft < _win) _nfft <<= 1;
                _hann = new double[_win];
                for (int n = 0; n < _win; n++) _hann[n] = 0.5 - 0.5 * Math.Cos(2 * Math.PI * n / Math.Max(1, _win - 1));

                _melBands = melBands;
                _mfccCount = mfccCount;

                double nyq = sampleRate * 0.5;
                double fhi = Math.Min(fmax, nyq);
                _mel = BuildMelBank(melBands, _nfft, sampleRate, fmin, fhi);
            }

            public List<double[]> ExtractSequence(float[] x, bool cmvn)
            {
                if (x.Length < _win) return new List<double[]>();

                // Pre-emphasis (light, improves HF balance)
                var pre = new float[x.Length];
                pre[0] = x[0];
                for (int i = 1; i < x.Length; i++) pre[i] = (float)(x[i] - 0.97 * x[i - 1]);

                int frames = 1 + (x.Length - _win) / _hop;
                var seq = new List<double[]>(frames);

                var re = new double[_nfft];
                var im = new double[_nfft];

                for (int f = 0; f < frames; f++)
                {
                    int off = f * _hop;
                    // window + zero pad
                    for (int n = 0; n < _win; n++) re[n] = pre[off + n] * _hann[n];
                    for (int n = _win; n < _nfft; n++) re[n] = 0.0;
                    Array.Clear(im, 0, _nfft);

                    FftRadix2(re, im);

                    int bins = _nfft / 2 + 1;
                    var power = new double[bins];
                    for (int k = 0; k < bins; k++)
                    {
                        double rr = re[k];
                        double ii = im[k];
                        power[k] = (rr * rr + ii * ii) / _nfft;
                    }

                    // Mel energies
                    var mels = new double[_melBands];
                    for (int m = 0; m < _melBands; m++)
                    {
                        double sum = 0;
                        var w = _mel[m];
                        for (int k = 0; k < w.Length; k++) sum += w[k] * power[k];
                        mels[m] = Math.Log(Math.Max(1e-12, sum));
                    }

                    // DCT-II to MFCCs
                    var cep = Dct2(mels, _mfccCount);
                    seq.Add(cep);
                }

                if (cmvn && seq.Count > 0)
                {
                    int D = _mfccCount;
                    var mean = new double[D];
                    var var = new double[D];

                    foreach (var v in seq) for (int d = 0; d < D; d++) mean[d] += v[d];
                    for (int d = 0; d < D; d++) mean[d] /= seq.Count;
                    foreach (var v in seq) for (int d = 0; d < D; d++) var[d] += (v[d] - mean[d]) * (v[d] - mean[d]);
                    for (int d = 0; d < D; d++) var[d] = Math.Sqrt(var[d] / Math.Max(1, seq.Count - 1)) + 1e-6;

                    foreach (var v in seq) for (int d = 0; d < D; d++) v[d] = (v[d] - mean[d]) / var[d];
                }

                return seq;
            }

            private static double[] Dct2(double[] x, int m)
            {
                int N = x.Length;
                var y = new double[m];
                double scale0 = Math.Sqrt(1.0 / N);
                double scale = Math.Sqrt(2.0 / N);
                for (int k = 0; k < m; k++)
                {
                    double s = 0;
                    for (int n = 0; n < N; n++)
                        s += x[n] * Math.Cos(Math.PI * (n + 0.5) * k / N);
                    y[k] = (k == 0 ? scale0 : scale) * s;
                }
                return y;
            }

            private static double[][] BuildMelBank(int melBands, int nfft, int sr, double fmin, double fmax)
            {
                int bins = nfft / 2 + 1;
                double hz2mel(double f) => 2595.0 * Math.Log10(1.0 + f / 700.0);
                double mel2hz(double m) => 700.0 * (Math.Pow(10.0, m / 2595.0) - 1.0);

                double mmin = hz2mel(fmin);
                double mmax = hz2mel(fmax);
                var centers = new double[melBands + 2];
                for (int i = 0; i < centers.Length; i++)
                {
                    double mel = mmin + (mmax - mmin) * i / (centers.Length - 1);
                    centers[i] = mel2hz(mel);
                }

                var fb = new double[melBands][];
                for (int m = 0; m < melBands; m++)
                {
                    fb[m] = new double[bins];
                    double f0 = centers[m];
                    double f1 = centers[m + 1];
                    double f2 = centers[m + 2];

                    for (int k = 0; k < bins; k++)
                    {
                        double freq = (double)k * sr / nfft;
                        double w = 0;
                        if (freq >= f0 && freq <= f1)
                            w = (freq - f0) / Math.Max(1e-9, (f1 - f0));
                        else if (freq > f1 && freq <= f2)
                            w = (f2 - freq) / Math.Max(1e-9, (f2 - f1));
                        fb[m][k] = Math.Max(0, w);
                    }
                }
                return fb;
            }

            private static void FftRadix2(double[] re, double[] im)
            {
                int n = re.Length;
                // bit-reverse
                int j = 0;
                for (int i = 0; i < n; i++)
                {
                    if (i < j)
                    {
                        (re[i], re[j]) = (re[j], re[i]);
                        (im[i], im[j]) = (im[j], im[i]);
                    }
                    int k = n >> 1;
                    while (k <= j) { j -= k; k >>= 1; }
                    j += k;
                }

                for (int len = 2; len <= n; len <<= 1)
                {
                    double ang = -2 * Math.PI / len;
                    double wlenRe = Math.Cos(ang);
                    double wlenIm = Math.Sin(ang);
                    for (int i = 0; i < n; i += len)
                    {
                        double wRe = 1.0, wIm = 0.0;
                        for (int k = 0; k < len / 2; k++)
                        {
                            int u = i + k;
                            int v = u + len / 2;
                            double tRe = wRe * re[v] - wIm * im[v];
                            double tIm = wRe * im[v] + wIm * re[v];
                            re[v] = re[u] - tRe; im[v] = im[u] - tIm;
                            re[u] += tRe;         im[u] += tIm;

                            double nwRe = wRe * wlenRe - wIm * wlenIm;
                            double nwIm = wRe * wlenIm + wIm * wlenRe;
                            wRe = nwRe; wIm = nwIm;
                        }
                    }
                }
            }
        }

        private static class Dtw
        {
            /// <summary>
            /// Cosine-distance DTW with Sakoe–Chiba band. Returns path cost normalized by path length.
            /// </summary>
            public static double CosineBand(List<double[]> A, List<double[]> B, int band)
            {
                int N = A.Count, M = B.Count;
                if (N == 0 || M == 0) return double.PositiveInfinity;

                // Pre-normalize frames to unit length
                var nA = NormalizeFrames(A);
                var nB = NormalizeFrames(B);

                double CosDist(int i, int j)
                {
                    var x = nA[i]; var y = nB[j];
                    double dot = 0;
                    for (int d = 0; d < x.Length; d++) dot += x[d] * y[d];
                    // dot is cosine similarity in [-1,1]; convert to distance in [0,2]
                    return 1.0 - dot;
                }

                var INF = 1e18;
                var D = new double[N, M];
                for (int i = 0; i < N; i++) for (int j = 0; j < M; j++) D[i, j] = INF;

                // Band limits
                for (int i = 0; i < N; i++)
                {
                    int jmin = Math.Max(0, i - band);
                    int jmax = Math.Min(M - 1, i + band);
                    for (int j = jmin; j <= jmax; j++)
                    {
                        double c = CosDist(i, j);
                        if (i == 0 && j == 0) D[i, j] = c;
                        else
                        {
                            double best = INF;
                            if (i > 0) best = Math.Min(best, D[i - 1, j]);         // vertical
                            if (j > 0) best = Math.Min(best, D[i, j - 1]);         // horizontal
                            if (i > 0 && j > 0) best = Math.Min(best, D[i - 1, j - 1]); // diagonal
                            if (best < INF) D[i, j] = c + best;
                        }
                    }
                }

                // Backtrack to count path steps for normalization
                int ii = N - 1, jj = M - 1;
                int steps = 1;
                while (ii > 0 || jj > 0)
                {
                    double d = D[ii, jj];
                    double up = ii > 0 ? D[ii - 1, jj] : INF;
                    double left = jj > 0 ? D[ii, jj - 1] : INF;
                    double diag = (ii > 0 && jj > 0) ? D[ii - 1, jj - 1] : INF;

                    if (diag <= up && diag <= left) { ii--; jj--; }
                    else if (up <= left) { ii--; }
                    else { jj--; }
                    steps++;
                }

                return D[N - 1, M - 1] / steps; // mean cost per step ∈ [0,2]
            }

            private static List<double[]> NormalizeFrames(List<double[]> seq)
            {
                var outSeq = new List<double[]>(seq.Count);
                foreach (var v in seq)
                {
                    double norm = 0;
                    for (int i = 0; i < v.Length; i++) norm += v[i] * v[i];
                    norm = Math.Sqrt(norm) + 1e-12;
                    var u = new double[v.Length];
                    for (int i = 0; i < v.Length; i++) u[i] = v[i] / norm;
                    outSeq.Add(u);
                }
                return outSeq;
            }
        }
    }
}
