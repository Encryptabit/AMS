using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ams.Cli.Utilities;

namespace Ams.Cli.Commands;

public static class DspCommand
{
    private const string DefaultChainFileName = "dsp.chain.json";

    public static Command Create()
    {
        var dsp = new Command("dsp", "Audio processing utilities powered by Plugalyzer");

        dsp.AddCommand(CreateRunCommand());
        dsp.AddCommand(CreateListParamsCommand());

        return dsp;
    }

    private static Command CreateRunCommand()
    {
        var cmd = new Command("run", "Run a DSP chain against an audio file");

        var inputOption = new Option<FileInfo?>("--input", "Input audio file (defaults to active chapter)")
        {
            Arity = ArgumentArity.ZeroOrOne
        };
        inputOption.AddAlias("-i");

        var outputOption = new Option<FileInfo?>("--output", "Output audio file (defaults to <chapter>.treated.wav)")
        {
            Arity = ArgumentArity.ZeroOrOne
        };
        outputOption.AddAlias("-o");

        var chainOption = new Option<FileInfo?>("--chain", "JSON file describing the DSP chain")
        {
            Arity = ArgumentArity.ZeroOrOne
        };
        chainOption.AddAlias("-c");

        var pluginOption = new Option<string?>("--plugin", "Quick single-node run with the specified plugin");
        pluginOption.AddAlias("-p");

        var paramOption = new Option<string[]>("--param", () => Array.Empty<string>(), "Parameter override in Plugalyzer syntax (repeatable)");
        var presetOption = new Option<string?>("--preset", "Optional preset file (.vstpreset)");
        var paramFileOption = new Option<FileInfo?>("--param-file", "JSON automation file to pass to Plugalyzer");

        var sampleRateOption = new Option<int?>("--sample-rate", "Override sample rate for all nodes (Hz)");
        var blockSizeOption = new Option<int?>("--block-size", "Override block size for all nodes");
        var outChannelsOption = new Option<int?>("--out-channels", "Override output channel count for all nodes");
        var bitDepthOption = new Option<int?>("--bit-depth", "Override output bit depth for all nodes");

        var workDirOption = new Option<DirectoryInfo?>("--work", "Working directory for intermediates (defaults to temp)");
        var keepTempOption = new Option<bool>("--keep-temp", () => false, "Keep intermediate files after completion");
        var overwriteOption = new Option<bool>("--overwrite", () => false, "Overwrite final output if it exists");

        cmd.AddOption(inputOption);
        cmd.AddOption(outputOption);
        cmd.AddOption(chainOption);
        cmd.AddOption(pluginOption);
        cmd.AddOption(paramOption);
        cmd.AddOption(presetOption);
        cmd.AddOption(paramFileOption);
        cmd.AddOption(sampleRateOption);
        cmd.AddOption(blockSizeOption);
        cmd.AddOption(outChannelsOption);
        cmd.AddOption(bitDepthOption);
        cmd.AddOption(workDirOption);
        cmd.AddOption(keepTempOption);
        cmd.AddOption(overwriteOption);

        cmd.SetHandler(async context =>
        {
            var cancellationToken = context.GetCancellationToken();

            try
            {
                var inputFile = CommandInputResolver.RequireAudio(context.ParseResult.GetValueForOption(inputOption));
                var outputFile = ResolveOutputFile(context.ParseResult.GetValueForOption(outputOption), inputFile);

                var chainFile = context.ParseResult.GetValueForOption(chainOption);
                var pluginPath = context.ParseResult.GetValueForOption(pluginOption);
                var paramValues = context.ParseResult.GetValueForOption(paramOption);
                var presetPath = context.ParseResult.GetValueForOption(presetOption);
                var paramFile = context.ParseResult.GetValueForOption(paramFileOption);

                var sampleRate = context.ParseResult.GetValueForOption(sampleRateOption);
                var blockSize = context.ParseResult.GetValueForOption(blockSizeOption);
                var outChannels = context.ParseResult.GetValueForOption(outChannelsOption);
                var bitDepth = context.ParseResult.GetValueForOption(bitDepthOption);

                var workDir = context.ParseResult.GetValueForOption(workDirOption);
                var keepTemp = context.ParseResult.GetValueForOption(keepTempOption);
                var overwrite = context.ParseResult.GetValueForOption(overwriteOption);

                if (chainFile is null && string.IsNullOrWhiteSpace(pluginPath))
                {
                    var defaultChainPath = Path.Combine(inputFile.DirectoryName ?? Directory.GetCurrentDirectory(), DefaultChainFileName);
                    if (File.Exists(defaultChainPath))
                    {
                        chainFile = new FileInfo(defaultChainPath);
                        Log.Info("[dsp] Using default chain file {File}", chainFile.FullName);
                    }
                }

                if (chainFile is null && string.IsNullOrWhiteSpace(pluginPath))
                {
                    throw new InvalidOperationException("Specify either --chain or --plugin");
                }

                var chain = chainFile is not null
                    ? await LoadChainAsync(chainFile, cancellationToken)
                    : BuildSingleNodeChain(pluginPath!, paramValues, presetPath, paramFile);

                var chainBaseDirectory = chainFile?.DirectoryName ?? Directory.GetCurrentDirectory();

                await RunChainAsync(chain, inputFile.FullName, outputFile.FullName, chainBaseDirectory,
                    sampleRate, blockSize, outChannels, bitDepth, workDir, keepTemp, overwrite,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "dsp run failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    private static Command CreateListParamsCommand()
    {
        var cmd = new Command("list-params", "List available parameters for a plugin");

        var pluginOption = new Option<string>("--plugin", description: "Path to plugin (.vst3, .component, etc.)")
        {
            IsRequired = true
        };
        pluginOption.AddAlias("-p");

        var sampleRateOption = new Option<int?>("--sample-rate", "Sample rate for plugin initialization");
        var blockSizeOption = new Option<int?>("--block-size", "Block size for plugin initialization");

        cmd.AddOption(pluginOption);
        cmd.AddOption(sampleRateOption);
        cmd.AddOption(blockSizeOption);

        cmd.SetHandler(async context =>
        {
            var plugin = context.ParseResult.GetValueForOption(pluginOption);
            var sampleRate = context.ParseResult.GetValueForOption(sampleRateOption);
            var blockSize = context.ParseResult.GetValueForOption(blockSizeOption);
            var cancellationToken = context.GetCancellationToken();

            try
            {
                var baseDir = Directory.GetCurrentDirectory();
                var pluginPath = ResolvePath(plugin, baseDir);

                var args = new List<string> { "listParameters", $"--plugin={pluginPath}" };
                if (sampleRate.HasValue)
                {
                    args.Add($"--sampleRate={sampleRate.Value}");
                }
                if (blockSize.HasValue)
                {
                    args.Add($"--blockSize={blockSize.Value}");
                }

                await PlugalyzerService.RunAsync(args, baseDir, cancellationToken,
                    line => Console.WriteLine(line),
                    line => Console.Error.WriteLine(line)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "dsp list-params failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    private static FileInfo ResolveOutputFile(FileInfo? provided, FileInfo inputFile)
    {
        try
        {
            return CommandInputResolver.ResolveOutput(provided, "treated.wav");
        }
        catch
        {
            if (provided is not null)
            {
                return provided;
            }

            var stem = Path.GetFileNameWithoutExtension(inputFile.Name);
            var fallback = Path.Combine(inputFile.DirectoryName ?? Directory.GetCurrentDirectory(), $"{stem}.treated.wav");
            return new FileInfo(fallback);
        }
    }

    private static async Task<TreatmentChain> LoadChainAsync(FileInfo chainFile, CancellationToken cancellationToken)
    {
        if (!chainFile.Exists)
        {
            throw new FileNotFoundException($"Chain file not found: {chainFile.FullName}");
        }

        await using var stream = chainFile.OpenRead();
        var chain = await JsonSerializer.DeserializeAsync<TreatmentChain>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        }, cancellationToken).ConfigureAwait(false);

        if (chain is null || chain.Nodes.Count == 0)
        {
            throw new InvalidOperationException("Treatment chain must define at least one node");
        }

        return chain;
    }

    private static TreatmentChain BuildSingleNodeChain(string plugin, IReadOnlyList<string> parameters, string? preset, FileInfo? paramFile)
    {
        var node = new TreatmentNode(
            name: Path.GetFileNameWithoutExtension(plugin),
            plugin: plugin,
            description: null,
            parameters: parameters,
            parameterFile: paramFile?.FullName,
            preset: preset,
            sampleRate: null,
            blockSize: null,
            outChannels: null,
            bitDepth: null,
            inputs: null,
            midiInput: null,
            additionalArguments: null,
            outputFile: null);

        return new TreatmentChain(
            Name: null,
            Description: null,
            SampleRate: null,
            BlockSize: null,
            OutChannels: null,
            BitDepth: null,
            Nodes: new[] { node });
    }

    private static async Task RunChainAsync(
        TreatmentChain chain,
        string inputPath,
        string outputPath,
        string chainBaseDirectory,
        int? overrideSampleRate,
        int? overrideBlockSize,
        int? overrideOutChannels,
        int? overrideBitDepth,
        DirectoryInfo? workDirOption,
        bool keepTemp,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException($"Input audio file not found: {inputPath}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? Directory.GetCurrentDirectory());
        if (File.Exists(outputPath) && !overwrite)
        {
            throw new InvalidOperationException($"Output already exists: {outputPath}. Use --overwrite to replace");
        }

        var workRoot = workDirOption?.FullName ?? Path.Combine(Path.GetTempPath(), "ams", "dsp", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        Directory.CreateDirectory(workRoot);

        var tempFiles = new List<string>();
        var initialInput = Path.GetFullPath(inputPath);
        var previousOutput = initialInput;

        try
        {
            for (int index = 0; index < chain.Nodes.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var node = chain.Nodes[index];
                var nodeName = !string.IsNullOrWhiteSpace(node.Name) ? node.Name! : Path.GetFileNameWithoutExtension(node.Plugin);
                var nodeOutput = ResolveNodeOutput(node, workRoot, index);

                var (inputs, midiInput) = ResolveInputs(node, chainBaseDirectory, initialInput, previousOutput);

                var args = BuildProcessArguments(
                    node,
                    chain,
                    inputs,
                    midiInput,
                    nodeOutput,
                    chainBaseDirectory,
                    overrideSampleRate,
                    overrideBlockSize,
                    overrideOutChannels,
                    overrideBitDepth);

                Log.Info("[dsp] Node {Index}/{Total}: {Name}", index + 1, chain.Nodes.Count, nodeName);

                var exitCode = await PlugalyzerService.RunAsync(args, chainBaseDirectory, cancellationToken).ConfigureAwait(false);
                if (exitCode != 0)
                {
                    throw new InvalidOperationException($"Plugalyzer exited with code {exitCode} for node '{nodeName}'");
                }

                tempFiles.Add(nodeOutput);
                previousOutput = nodeOutput;
            }

            var finalSource = previousOutput;
            if (!Path.GetFullPath(finalSource).Equals(Path.GetFullPath(outputPath), StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                File.Copy(finalSource, outputPath, overwrite: false);
            }

            Log.Info("[dsp] Chain complete. Output -> {Output}", outputPath);
        }
        finally
        {
            if (!keepTemp)
            {
                if (workDirOption is null)
                {
                    TryDeleteDirectory(workRoot);
                }
                else
                {
                    foreach (var file in tempFiles)
                    {
                        TryDeleteFile(file);
                    }
                }
            }
            else
            {
                Log.Info("[dsp] Intermediate files kept at {Path}", workRoot);
            }
        }
    }

    private static (IReadOnlyList<string> Inputs, string? MidiInput) ResolveInputs(
        TreatmentNode node,
        string baseDirectory,
        string initialInput,
        string previousOutput)
    {
        var inputs = new List<string>();
        if (node.Inputs is null || node.Inputs.Count == 0)
        {
            inputs.Add(previousOutput);
        }
        else
        {
            foreach (var input in node.Inputs)
            {
                inputs.Add(ExpandInputToken(input, baseDirectory, initialInput, previousOutput));
            }
        }

        string? midi = null;
        if (!string.IsNullOrWhiteSpace(node.MidiInput))
        {
            midi = ExpandInputToken(node.MidiInput!, baseDirectory, initialInput, previousOutput);
        }

        return (inputs, midi);
    }

    private static string ExpandInputToken(string token, string baseDirectory, string initialInput, string previousOutput)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Input token cannot be empty.");
        }

        return token switch
        {
            "{input}" or "{source}" => initialInput,
            "{prev}" or "{previous}" => previousOutput,
            _ => ResolvePath(token, baseDirectory)
        };
    }

    private static IReadOnlyList<string> BuildProcessArguments(
        TreatmentNode node,
        TreatmentChain chain,
        IReadOnlyList<string> inputs,
        string? midiInput,
        string nodeOutput,
        string baseDirectory,
        int? overrideSampleRate,
        int? overrideBlockSize,
        int? overrideOutChannels,
        int? overrideBitDepth)
    {
        var args = new List<string> { "process" };

        var pluginPath = ResolvePath(node.Plugin, baseDirectory);
        args.Add($"--plugin={pluginPath}");

        foreach (var input in inputs)
        {
            args.Add($"--input={input}");
        }

        if (!string.IsNullOrWhiteSpace(midiInput))
        {
            args.Add($"--midiInput={midiInput}");
        }

        args.Add($"--output={nodeOutput}");
        args.Add("--overwrite");

        var sampleRate = overrideSampleRate ?? node.SampleRate ?? chain.SampleRate;
        if (sampleRate.HasValue)
        {
            args.Add($"--sampleRate={sampleRate.Value}");
        }

        var blockSize = overrideBlockSize ?? node.BlockSize ?? chain.BlockSize;
        if (blockSize.HasValue)
        {
            args.Add($"--blockSize={blockSize.Value}");
        }

        var outChannels = overrideOutChannels ?? node.OutChannels ?? chain.OutChannels;
        if (outChannels.HasValue)
        {
            args.Add($"--outChannels={outChannels.Value}");
        }

        var bitDepth = overrideBitDepth ?? node.BitDepth ?? chain.BitDepth;
        if (bitDepth.HasValue)
        {
            args.Add($"--bitDepth={bitDepth.Value}");
        }

        if (!string.IsNullOrWhiteSpace(node.ParameterFile))
        {
            var paramFilePath = ResolvePath(node.ParameterFile!, baseDirectory);
            args.Add($"--paramFile={paramFilePath}");
        }

        if (!string.IsNullOrWhiteSpace(node.Preset))
        {
            var presetPath = ResolvePath(node.Preset!, baseDirectory);
            args.Add($"--preset={presetPath}");
        }

        if (node.Parameters is not null)
        {
            foreach (var parameter in node.Parameters)
            {
                if (string.IsNullOrWhiteSpace(parameter))
                {
                    continue;
                }
                args.Add($"--param={parameter}");
            }
        }

        if (node.AdditionalArguments is not null)
        {
            foreach (var extra in node.AdditionalArguments)
            {
                if (!string.IsNullOrWhiteSpace(extra))
                {
                    args.Add(extra);
                }
            }
        }

        return args;
    }

    private static string ResolveNodeOutput(TreatmentNode node, string workRoot, int index)
    {
        if (!string.IsNullOrWhiteSpace(node.OutputFile))
        {
            var path = ResolvePath(node.OutputFile!, workRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            return path;
        }

        var stem = !string.IsNullOrWhiteSpace(node.Name)
            ? node.Name!
            : Path.GetFileNameWithoutExtension(node.Plugin);
        var safeStem = Sanitize(stem);
        var fileName = $"{index + 1:00}-{safeStem}.wav";
        return Path.Combine(workRoot, fileName);
    }

    private static string Sanitize(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(c, '_');
        }
        return value;
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to delete temporary directory {Path}", path);
        }
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to delete temporary file {Path}", path);
        }
    }

    private static string ResolvePath(string path, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty", nameof(path));
        }

        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        var baseDir = string.IsNullOrWhiteSpace(baseDirectory) ? Directory.GetCurrentDirectory() : baseDirectory;
        return Path.GetFullPath(Path.Combine(baseDir, path));
    }
}
