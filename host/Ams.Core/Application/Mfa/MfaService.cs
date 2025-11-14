using System.Text;
using Ams.Core.Application.Processes;
using Ams.Core.Artifacts.Alignment.Mfa;

namespace Ams.Core.Application.Mfa;

public interface IMfaService
{
    Task<MfaCommandResult> ValidateAsync(MfaChapterContext context, CancellationToken cancellationToken = default);

    Task<MfaCommandResult> GeneratePronunciationsAsync(MfaChapterContext context, CancellationToken cancellationToken = default);

    Task<MfaCommandResult> AddWordsAsync(MfaChapterContext context, CancellationToken cancellationToken = default);

    Task<MfaCommandResult> AlignAsync(MfaChapterContext context, CancellationToken cancellationToken = default);
}

public sealed class MfaService : IMfaService
{
    internal const string DefaultDictionaryModel = "english_mfa";
    internal const string DefaultAcousticModel = "english_mfa";
    internal const string DefaultG2pModel = "english_us_mfa";

    internal MfaService()
    {
    }

    private static readonly string ValidateCommand = "validate";
    private static readonly string G2pCommand = "g2p";
    private static readonly string AddWordsCommand = "model add_words";
    private static readonly string AlignCommandName = "align";

    public Task<MfaCommandResult> ValidateAsync(MfaChapterContext context, CancellationToken cancellationToken = default)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var dictionary = string.IsNullOrWhiteSpace(context.DictionaryModel)
            ? DefaultDictionaryModel
            : context.DictionaryModel!;

        var acoustic = string.IsNullOrWhiteSpace(context.AcousticModel)
            ? DefaultAcousticModel
            : context.AcousticModel!;

        var args = new StringBuilder();
        args.Append(QuoteRequired(context.CorpusDirectory));
        args.Append(' ').Append(Quote(dictionary));
        args.Append(' ').Append(Quote(acoustic));

        if (context.SingleSpeaker == true)
        {
            args.Append(" --single_speaker");
        }

        // Avoid monophone training during validation (can fail on very small corpora)
        args.Append(" --no_train");

        return MfaProcessSupervisor.RunAsync(
            ValidateCommand,
            args.ToString(),
            context.WorkingDirectory,
            cancellationToken);
    }

    public Task<MfaCommandResult> GeneratePronunciationsAsync(MfaChapterContext context, CancellationToken cancellationToken = default)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (string.IsNullOrWhiteSpace(context.OovListPath))
        {
            throw new ArgumentException("OOV list path must be provided for G2P generation", nameof(context));
        }

        if (string.IsNullOrWhiteSpace(context.G2pOutputPath))
        {
            throw new ArgumentException("Output path must be provided for G2P generation", nameof(context));
        }

        var g2pModel = string.IsNullOrWhiteSpace(context.G2pModel)
            ? DefaultG2pModel
            : context.G2pModel!;

        var args = new StringBuilder();
        args.Append(QuoteRequired(context.OovListPath));
        args.Append(' ').Append(Quote(g2pModel));
        args.Append(' ').Append(QuoteRequired(context.G2pOutputPath));

        return MfaProcessSupervisor.RunAsync(
            G2pCommand,
            args.ToString(),
            context.WorkingDirectory,
            cancellationToken);
    }

    public Task<MfaCommandResult> AddWordsAsync(MfaChapterContext context, CancellationToken cancellationToken = default)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (string.IsNullOrWhiteSpace(context.G2pOutputPath))
        {
            throw new ArgumentException("G2P output path must be provided for add_words", nameof(context));
        }

        var baseDictionary = string.IsNullOrWhiteSpace(context.DictionaryModel)
            ? DefaultDictionaryModel
            : context.DictionaryModel!;

        var args = new StringBuilder();
        args.Append(Quote(baseDictionary));
        args.Append(' ').Append(QuoteRequired(context.G2pOutputPath));

        return MfaProcessSupervisor.RunAsync(
            AddWordsCommand,
            args.ToString(),
            context.WorkingDirectory,
            cancellationToken);
    }

    public Task<MfaCommandResult> AlignAsync(MfaChapterContext context, CancellationToken cancellationToken = default)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var dictionary = string.IsNullOrWhiteSpace(context.CustomDictionaryPath)
            ? QuoteRequired(context.DictionaryModel ?? DefaultDictionaryModel)
            : QuoteRequired(context.CustomDictionaryPath);

        var acoustic = QuoteRequired(string.IsNullOrWhiteSpace(context.AcousticModel)
            ? DefaultAcousticModel
            : context.AcousticModel!);

        var args = new StringBuilder();
        args.Append(QuoteRequired(context.CorpusDirectory));
        args.Append(' ').Append(dictionary);
        args.Append(' ').Append(acoustic);
        args.Append(' ').Append(QuoteRequired(context.OutputDirectory));

        if (context.Beam.HasValue)
        {
            args.Append(' ').Append("--beam ").Append(context.Beam.Value);
        }

        if (context.RetryBeam.HasValue)
        {
            args.Append(' ').Append("--retry_beam ").Append(context.RetryBeam.Value);
        }

        if (context.SingleSpeaker == true)
        {
            args.Append(" --single_speaker");
        }

        if (context.CleanOutput == true)
        {
            args.Append(" --clean");
        }

        return MfaProcessSupervisor.RunAsync(
            AlignCommandName,
            args.ToString(),
            context.WorkingDirectory,
            cancellationToken);
    }

    private static string Quote(string value)
    {
        if (value.Contains('"'))
        {
            value = value.Replace("\"", "\\\"");
        }

        return $"\"{value}\"";
    }

    private static string QuoteRequired(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Required value was null or whitespace", nameof(value));
        }

        return Quote(value.Trim());
    }
}
