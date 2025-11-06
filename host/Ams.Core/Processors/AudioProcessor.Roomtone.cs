using System;
using System.Globalization;
using System.Text;
using Ams.Core.Artifacts;
using Ams.Core.Services.Integrations.FFmpeg;

namespace Ams.Core.Processors;

public static partial class AudioProcessor
{
    public static AudioBuffer OverlayRoomtone(
        AudioBuffer source,
        AudioBuffer tone,
        double gapStartSec,
        double gapEndSec,
        double gainLinear,
        double fadeMs)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (tone is null) throw new ArgumentNullException(nameof(tone));
        if (gapEndSec <= gapStartSec) return source;
        if (source.SampleRate != tone.SampleRate)
        {
            throw new InvalidOperationException("Source and tone must share the same sample rate.");
        }

        double totalDurationSec = source.Length / (double)source.SampleRate;
        gapStartSec = Math.Clamp(gapStartSec, 0.0, totalDurationSec);
        gapEndSec = Math.Clamp(gapEndSec, gapStartSec, totalDurationSec);
        double gapDuration = gapEndSec - gapStartSec;
        if (gapDuration <= 0)
        {
            return source;
        }

        double fadeSec = Math.Max(0.0, fadeMs * 0.001);
        double leftFade = Math.Min(fadeSec, gapStartSec);
        double rightFade = Math.Min(fadeSec, Math.Max(0.0, totalDurationSec - gapEndSec));

        double toneDuration = gapDuration + leftFade + rightFade;
        if (toneDuration <= 0)
        {
            return source;
        }

        var invariant = CultureInfo.InvariantCulture;
        var builder = new StringBuilder();

        builder.AppendLine(FormattableString.Invariant(
            $"[main]atrim=0:{gapStartSec.ToString("F6", invariant)},asetpts=PTS-STARTPTS[head];"));

        builder.AppendLine(FormattableString.Invariant(
            $"[main]atrim={gapEndSec.ToString("F6", invariant)},asetpts=PTS-STARTPTS[tail];"));

        builder.Append(FormattableString.Invariant(
            $"[tone]aloop=-1:size={tone.Length},asetpts=PTS-STARTPTS,atrim=0:{toneDuration.ToString("F6", invariant)}"));

        if (leftFade > 0)
        {
            builder.Append(FormattableString.Invariant($",afade=t=in:ss=0:d={leftFade.ToString("F6", invariant)}"));
        }

        if (rightFade > 0)
        {
            double start = toneDuration - rightFade;
            builder.Append(FormattableString.Invariant($",afade=t=out:st={start.ToString("F6", invariant)}:d={rightFade.ToString("F6", invariant)}"));
        }

        if (Math.Abs(gainLinear - 1.0) > 1e-6)
        {
            builder.Append(FormattableString.Invariant($",volume={gainLinear.ToString("F6", invariant)}"));
        }

        builder.AppendLine("[toneprep];");

        string current = "head";
        string toneLabel = "toneprep";
        string mixLabel = "mix1";

        if (leftFade > 0)
        {
            builder.AppendLine(FormattableString.Invariant(
                $"[{current}][{toneLabel}]acrossfade=d={leftFade.ToString("F6", invariant)}[{mixLabel}];"));
        }
        else
        {
            builder.AppendLine(FormattableString.Invariant(
                $"[{current}][{toneLabel}]concat=n=2:v=0:a=1[{mixLabel}];"));
        }

        current = mixLabel;
        mixLabel = "mix2";

        if (rightFade > 0)
        {
            builder.Append(FormattableString.Invariant(
                $"[{current}][tail]acrossfade=d={rightFade.ToString("F6", invariant)}[out]"));
        }
        else
        {
            builder.Append(FormattableString.Invariant(
                $"[{current}][tail]concat=n=2:v=0:a=1[out]"));
        }

        var filter = builder.ToString();

        var inputs = new[]
        {
            new FfFilterGraphRunner.GraphInput("main", source),
            new FfFilterGraphRunner.GraphInput("tone", tone),
        };

        return FfFilterGraphRunner.Apply(inputs, filter);
    }
}
