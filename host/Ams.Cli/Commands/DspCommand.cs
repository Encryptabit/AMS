using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ams.Cli.Utilities;

namespace Ams.Cli.Commands;

public static class DspCommand
{
    private const string DefaultChainFileName = "dsp.chain.json";
    private const int DefaultBitDepth = 32;

    public static Command Create()
    {
        var dsp = new Command("dsp", "Audio processing utilities powered by Plugalyzer");

        dsp.AddCommand(CreateRunCommand());
        dsp.AddCommand(CreateListParamsCommand());
        dsp.AddCommand(CreateListPluginsCommand());
        dsp.AddCommand(CreateOutputModeCommand());
        dsp.AddCommand(CreateOverwriteCommand());
        dsp.AddCommand(CreateSetDirCommand());
        dsp.AddCommand(CreateInitCommand());
        dsp.AddCommand(CreateChainCommand());

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

        var pluginOption = new Option<string?>("--plugin", "Quick single-node run with the specified plugin (path or friendly name)");
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

                var chainOptionValue = context.ParseResult.GetValueForOption(chainOption);
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
                var overwriteResult = context.ParseResult.FindResultFor(overwriteOption);
                var overwrite = overwriteResult is not null
                    ? context.ParseResult.GetValueForOption(overwriteOption)
                    : DspSessionState.OverwriteOutputs;

                FileInfo? chainFile = null;
                if (chainOptionValue is not null)
                {
                    chainFile = ResolveChainFile(chainOptionValue);
                }
                else
                {
                    var defaultChain = ResolveChainFile(null, inputFile.DirectoryName);
                    if (defaultChain.Exists)
                    {
                        chainFile = defaultChain;
                        Log.Debug("[dsp] Using default chain file {File}", chainFile.FullName);
                    }
                }

                if (chainFile is null && string.IsNullOrWhiteSpace(pluginPath))
                {
                    throw new InvalidOperationException("Specify either --chain or --plugin");
                }

                TreatmentChain chain;
                var chainBaseDirectory = chainFile?.DirectoryName
                    ?? inputFile.DirectoryName
                    ?? Directory.GetCurrentDirectory();

                if (chainFile is not null)
                {
                    chain = await LoadChainAsync(chainFile, cancellationToken).ConfigureAwait(false);
                    if (chain.Nodes.Count == 0)
                    {
                        throw new InvalidOperationException($"Chain file '{chainFile.FullName}' does not contain any nodes.");
                    }
                }
                else
                {
                    var config = await DspConfigService.LoadAsync(cancellationToken).ConfigureAwait(false);
                    chain = BuildSingleNodeChain(pluginPath!, paramValues, presetPath, paramFile, chainBaseDirectory, config);
                }

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

    private static Command CreateChainCommand()
    {
        var root = new Command("chain", "Interactively manage DSP chain files");

        root.AddCommand(CreateChainListCommand());
        root.AddCommand(CreateChainAddCommand());
        root.AddCommand(CreateChainPrependCommand());
        root.AddCommand(CreateChainInsertCommand());
        root.AddCommand(CreateChainRemoveCommand());

        return root;
    }

    private static Command CreateChainListCommand()
    {
        var cmd = new Command("list", "Show nodes in a chain");
        var chainOption = new Option<FileInfo?>("--chain", "Chain file path (defaults to dsp.chain.json)");
        cmd.AddOption(chainOption);

        cmd.SetHandler(async context =>
        {
            var token = context.GetCancellationToken();
            var chainFile = ResolveChainFile(context.ParseResult.GetValueForOption(chainOption));
            var chain = await LoadChainAsync(chainFile, token, createIfMissing: true).ConfigureAwait(false);
            var config = await DspConfigService.LoadAsync(token).ConfigureAwait(false);

            if (chain.Nodes.Count == 0)
            {
                Console.WriteLine("Chain is empty. Use 'dsp chain add --plugin <path>' to add nodes.");
                return;
            }

            Console.WriteLine($"Chain file: {chainFile.FullName}");
            var baseDirectory = chainFile.DirectoryName ?? Directory.GetCurrentDirectory();
            for (int i = 0; i < chain.Nodes.Count; i++)
            {
                var node = chain.Nodes[i];
                var pluginPath = ResolvePath(node.Plugin, baseDirectory);
                var friendly = TryGetFriendlyName(config, pluginPath);
                var name = !string.IsNullOrWhiteSpace(node.Name)
                    ? node.Name!
                    : friendly ?? Path.GetFileNameWithoutExtension(pluginPath);
                Console.WriteLine($"[{i,3}] {name}");
                Console.WriteLine($"      Plugin: {pluginPath}");
                if (!string.IsNullOrWhiteSpace(node.Description))
                {
                    Console.WriteLine($"      Notes : {node.Description}");
                }
                if (node.Parameters is { Count: > 0 })
                {
                    Console.WriteLine($"      Params: {string.Join(", ", node.Parameters)}");
                }
            }
        });

        return cmd;
    }

    private static Command CreateChainAddCommand()
    {
        var cmd = new Command("add", "Append a node to the chain");
        cmd.AddAlias("append");

        var options = new NodeOptionBundle(cmd);
        var chainOption = new Option<FileInfo?>("--chain", "Chain file path (defaults to dsp.chain.json)");
        cmd.AddOption(chainOption);

        cmd.SetHandler(async context =>
        {
            var token = context.GetCancellationToken();
            var chainFile = ResolveChainFile(context.ParseResult.GetValueForOption(chainOption));
            var chain = await LoadChainAsync(chainFile, token, createIfMissing: true).ConfigureAwait(false);
            var config = await DspConfigService.LoadAsync(token).ConfigureAwait(false);

            var baseDirectory = chainFile.DirectoryName ?? Directory.GetCurrentDirectory();
            var node = CreateNodeFromOptions(context, options, baseDirectory, config);

            var nodes = chain.Nodes.ToList();
            nodes.Add(node);
            var updated = chain with { Nodes = nodes };

            await SaveChainAsync(chainFile, updated, token).ConfigureAwait(false);
            Log.Debug("[dsp] Appended node '{Name}'", node.Name ?? Path.GetFileNameWithoutExtension(node.Plugin));
        });

        return cmd;
    }

    private static Command CreateChainPrependCommand()
    {
        var cmd = new Command("prepend", "Insert a node at the beginning of the chain");

        var options = new NodeOptionBundle(cmd);
        var chainOption = new Option<FileInfo?>("--chain", "Chain file path (defaults to dsp.chain.json)");
        cmd.AddOption(chainOption);

        cmd.SetHandler(async context =>
        {
            var token = context.GetCancellationToken();
            var chainFile = ResolveChainFile(context.ParseResult.GetValueForOption(chainOption));
            var chain = await LoadChainAsync(chainFile, token, createIfMissing: true).ConfigureAwait(false);
            var config = await DspConfigService.LoadAsync(token).ConfigureAwait(false);

            var baseDirectory = chainFile.DirectoryName ?? Directory.GetCurrentDirectory();
            var node = CreateNodeFromOptions(context, options, baseDirectory, config);

            var nodes = chain.Nodes.ToList();
            nodes.Insert(0, node);
            var updated = chain with { Nodes = nodes };

            await SaveChainAsync(chainFile, updated, token).ConfigureAwait(false);
            Log.Debug("[dsp] Prepended node '{Name}'", node.Name ?? Path.GetFileNameWithoutExtension(node.Plugin));
        });

        return cmd;
    }

    private static Command CreateChainInsertCommand()
    {
        var cmd = new Command("insert", "Insert a node at a specific index");

        var indexArgument = new Argument<int>("index", description: "Zero-based index to insert at");
        cmd.AddArgument(indexArgument);

        var options = new NodeOptionBundle(cmd);
        var chainOption = new Option<FileInfo?>("--chain", "Chain file path (defaults to dsp.chain.json)");
        cmd.AddOption(chainOption);

        cmd.SetHandler(async context =>
        {
            var token = context.GetCancellationToken();
            var index = context.ParseResult.GetValueForArgument(indexArgument);
            var chainFile = ResolveChainFile(context.ParseResult.GetValueForOption(chainOption));
            var chain = await LoadChainAsync(chainFile, token, createIfMissing: true).ConfigureAwait(false);
            var config = await DspConfigService.LoadAsync(token).ConfigureAwait(false);

            var nodes = chain.Nodes.ToList();
            if (index < 0 || index > nodes.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be between 0 and {nodes.Count}");
            }

            var baseDirectory = chainFile.DirectoryName ?? Directory.GetCurrentDirectory();
            var node = CreateNodeFromOptions(context, options, baseDirectory, config);

            nodes.Insert(index, node);
            var updated = chain with { Nodes = nodes };

            await SaveChainAsync(chainFile, updated, token).ConfigureAwait(false);
            Log.Debug("[dsp] Inserted node '{Name}' at index {Index}", node.Name ?? Path.GetFileNameWithoutExtension(node.Plugin), index);
        });

        return cmd;
    }

    private static Command CreateChainRemoveCommand()
    {
        var cmd = new Command("remove", "Remove a node by index or name");

        var chainOption = new Option<FileInfo?>("--chain", "Chain file path (defaults to dsp.chain.json)");
        var indexOption = new Option<int?>("--index", description: "Zero-based index to remove");
        var nameOption = new Option<string?>("--name", description: "Name of the node to remove");

        cmd.AddOption(chainOption);
        cmd.AddOption(indexOption);
        cmd.AddOption(nameOption);

        cmd.SetHandler(async context =>
        {
            var token = context.GetCancellationToken();
            var chainFile = ResolveChainFile(context.ParseResult.GetValueForOption(chainOption));
            var chain = await LoadChainAsync(chainFile, token, createIfMissing: true).ConfigureAwait(false);

            if (chain.Nodes.Count == 0)
            {
                Log.Debug("[dsp] Chain is already empty");
                return;
            }

            var index = context.ParseResult.GetValueForOption(indexOption);
            var name = context.ParseResult.GetValueForOption(nameOption);

            if (index is null && string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Specify either --index or --name to remove a node");
            }

            var nodes = chain.Nodes.ToList();
            TreatmentNode removedNode;

            if (index is not null)
            {
                if (index.Value < 0 || index.Value >= nodes.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index.Value, $"Index must be between 0 and {nodes.Count - 1}");
                }

                removedNode = nodes[index.Value];
                nodes.RemoveAt(index.Value);
            }
            else
            {
                var targetName = name!.Trim();
                var matchIndex = nodes.FindIndex(node =>
                    string.Equals(node.Name, targetName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Path.GetFileNameWithoutExtension(node.Plugin), targetName, StringComparison.OrdinalIgnoreCase));

                if (matchIndex < 0)
                {
                    throw new InvalidOperationException($"No node named '{targetName}' found");
                }

                removedNode = nodes[matchIndex];
                nodes.RemoveAt(matchIndex);
            }

            var updated = chain with { Nodes = nodes };
            await SaveChainAsync(chainFile, updated, token).ConfigureAwait(false);
            Log.Debug("[dsp] Removed node '{Name}'", removedNode.Name ?? Path.GetFileNameWithoutExtension(removedNode.Plugin));
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

    private static Command CreateListPluginsCommand()
    {
        var cmd = new Command("list", "List cached plugins available for chains");

        var filterOption = new Option<string?>("--filter", () => null, "Filter plugins by substring in name or path");
        var detailedOption = new Option<bool>("--detailed", () => false, "Show scan timestamps and parameter counts");

        cmd.AddOption(filterOption);
        cmd.AddOption(detailedOption);

        cmd.SetHandler(async context =>
        {
            var token = context.GetCancellationToken();
            var filter = context.ParseResult.GetValueForOption(filterOption);
            var detailed = context.ParseResult.GetValueForOption(detailedOption);

            var config = await DspConfigService.LoadAsync(token).ConfigureAwait(false);

            if (config.Plugins.Count == 0)
            {
                Console.WriteLine("No cached plugins. Use 'dsp set-dir add <path>' and 'dsp init' first.");
                return;
            }

            IEnumerable<KeyValuePair<string, DspPluginMetadata>> items = config.Plugins;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                items = items.Where(p =>
                    (!string.IsNullOrWhiteSpace(p.Value.PluginName) && p.Value.PluginName.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                    p.Key.Contains(filter, StringComparison.OrdinalIgnoreCase));
            }

            var ordered = items
                .OrderBy(p => p.Value.PluginName ?? Path.GetFileName(p.Key), StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ordered.Count == 0)
            {
                Console.WriteLine("No plugins matched the filter.");
                return;
            }

            Console.WriteLine("Cached plugins:");
            for (int i = 0; i < ordered.Count; i++)
            {
                var metadata = ordered[i].Value;
                var displayName = metadata.PluginName ?? Path.GetFileName(metadata.Path);
                Console.WriteLine($"[{i,3}] {displayName}");
                Console.WriteLine($"      Path     : {metadata.Path}");
                if (detailed)
                {
                    Console.WriteLine($"      Scanned   : {metadata.ScannedAtUtc:O}");
                    Console.WriteLine($"      Modified  : {metadata.PluginModifiedUtc:O}");
                    Console.WriteLine($"      Parameters: {metadata.Parameters?.Count ?? 0}");
                }
            }
        });

        return cmd;
    }

    private static Command CreateOutputModeCommand()
    {
        var cmd = new Command("output-mode", "Get or set the DSP output mode (source or post)");
        var modeArgument = new Argument<string?>("mode", () => null, "Specify 'source' or 'post' to change mode");
        cmd.AddArgument(modeArgument);

        cmd.SetHandler(context =>
        {
            var token = context.ParseResult.GetValueForArgument(modeArgument);
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine($"Output mode: {DspSessionState.OutputMode.ToString().ToLowerInvariant()}");
                return;
            }

            var normalized = token.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "source":
                    DspSessionState.OutputMode = DspOutputMode.Source;
                    Console.WriteLine("Output mode set to source (processed files written next to input source).");
                    break;
                case "post":
                    DspSessionState.OutputMode = DspOutputMode.Post;
                    Console.WriteLine("Output mode set to post (processed files written to chapter artefact directory).");
                    break;
                default:
                    throw new InvalidOperationException("Mode must be 'source' or 'post'.");
            }
        });

        return cmd;
    }

    private static Command CreateOverwriteCommand()
    {
        var cmd = new Command("overwrite", "Get or set the default overwrite behaviour");
        var stateArgument = new Argument<string?>("state", () => null, "Specify 'on' or 'off' to change overwrite behaviour");
        cmd.AddArgument(stateArgument);

        cmd.SetHandler(context =>
        {
            var value = context.ParseResult.GetValueForArgument(stateArgument);
            if (string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine($"Overwrite outputs: {(DspSessionState.OverwriteOutputs ? "on" : "off")}");
                return;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "on":
                case "true":
                case "yes":
                    DspSessionState.OverwriteOutputs = true;
                    Console.WriteLine("Overwrite outputs set to on.");
                    break;
                case "off":
                case "false":
                case "no":
                    DspSessionState.OverwriteOutputs = false;
                    Console.WriteLine("Overwrite outputs set to off.");
                    break;
                default:
                    throw new InvalidOperationException("State must be 'on' or 'off'.");
            }
        });

        return cmd;
    }

    private static Command CreateSetDirCommand()
    {
        var root = new Command("set-dir", "Manage directories containing plugins");

        root.AddCommand(CreateSetDirListCommand());
        root.AddCommand(CreateSetDirAddCommand());
        root.AddCommand(CreateSetDirRemoveCommand());
        root.AddCommand(CreateSetDirClearCommand());

        return root;
    }

    private static Command CreateSetDirListCommand()
    {
        var cmd = new Command("list", "Show configured plugin directories");

        cmd.SetHandler(async context =>
        {
            var token = context.GetCancellationToken();
            var config = await DspConfigService.LoadAsync(token).ConfigureAwait(false);

            if (config.PluginDirectories.Count == 0)
            {
                Console.WriteLine("No plugin directories configured. Use 'dsp set-dir add <path>' to add one.");
                return;
            }

            Console.WriteLine("Configured plugin directories:");
            foreach (var dir in config.PluginDirectories)
            {
                Console.WriteLine($" - {dir}");
            }
        });

        return cmd;
    }

    private static Command CreateSetDirAddCommand()
    {
        var cmd = new Command("add", "Add one or more plugin directories");
        var pathsArgument = new Argument<List<string>>("paths")
        {
            Arity = ArgumentArity.OneOrMore,
            Description = "Directory paths to add"
        };
        cmd.AddArgument(pathsArgument);

        cmd.SetHandler(async context =>
        {
            var token = context.GetCancellationToken();
            var paths = context.ParseResult.GetValueForArgument(pathsArgument);

            var config = await DspConfigService.LoadAsync(token).ConfigureAwait(false);
            var added = new List<string>();

            foreach (var path in paths)
            {
                var full = Path.GetFullPath(path);
                if (!Directory.Exists(full))
                {
                    Log.Error("Directory not found: {Directory}", full);
                    context.ExitCode = 1;
                    return;
                }

                if (!config.PluginDirectories.Contains(full, StringComparer.OrdinalIgnoreCase))
                {
                    config.PluginDirectories.Add(full);
                    added.Add(full);
                }
            }

            if (added.Count == 0)
            {
                Log.Debug("[dsp] No new directories added (duplicates ignored)");
                return;
            }

            await DspConfigService.SaveAsync(config, token).ConfigureAwait(false);
            foreach (var dir in added.OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
            {
                Log.Debug("[dsp] Added plugin directory {Directory}", dir);
            }
        });

        return cmd;
    }

    private static Command CreateSetDirRemoveCommand()
    {
        var cmd = new Command("remove", "Remove one or more plugin directories");
        var pathsArgument = new Argument<List<string>>("paths")
        {
            Arity = ArgumentArity.OneOrMore,
            Description = "Directory paths to remove"
        };
        cmd.AddArgument(pathsArgument);

        cmd.SetHandler(async context =>
        {
            var token = context.GetCancellationToken();
            var paths = context.ParseResult.GetValueForArgument(pathsArgument);

            var config = await DspConfigService.LoadAsync(token).ConfigureAwait(false);
            var removed = new List<string>();

            foreach (var path in paths)
            {
                var full = Path.GetFullPath(path);
                if (config.PluginDirectories.RemoveAll(d => string.Equals(d, full, StringComparison.OrdinalIgnoreCase)) > 0)
                {
                    removed.Add(full);
                }
            }

            if (removed.Count == 0)
            {
                Log.Debug("[dsp] No directories removed");
                return;
            }

            await DspConfigService.SaveAsync(config, token).ConfigureAwait(false);
            foreach (var dir in removed.OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
            {
                Log.Debug("[dsp] Removed plugin directory {Directory}", dir);
            }
        });

        return cmd;
    }

    private static Command CreateSetDirClearCommand()
    {
        var cmd = new Command("clear", "Remove all configured plugin directories");
        var confirmOption = new Option<bool>("--yes", "Confirm clearing directories") { Arity = ArgumentArity.ZeroOrOne };
        cmd.AddOption(confirmOption);

        cmd.SetHandler(async context =>
        {
            var token = context.GetCancellationToken();
            var confirm = context.ParseResult.GetValueForOption(confirmOption);

            if (!confirm)
            {
                Log.Debug("Use --yes to confirm clearing all directories");
                context.ExitCode = 1;
                return;
            }

            var config = await DspConfigService.LoadAsync(token).ConfigureAwait(false);
            if (config.PluginDirectories.Count == 0)
            {
                Log.Debug("[dsp] No directories to clear");
                return;
            }

            config.PluginDirectories.Clear();
            await DspConfigService.SaveAsync(config, token).ConfigureAwait(false);
            Log.Debug("[dsp] Cleared all plugin directories");
        });

        return cmd;
    }

    private static Command CreateInitCommand()
    {
        var cmd = new Command("init", "Scan plugin directories and cache parameter metadata");

        var pluginOption = new Option<FileInfo?>("--plugin", "Scan a single plugin file (overrides configured directories)");
        var forceOption = new Option<bool>("--force", () => false, "Re-scan plugins even if metadata is up to date");

        cmd.AddOption(pluginOption);
        cmd.AddOption(forceOption);

        cmd.SetHandler(async context =>
        {
            var token = context.GetCancellationToken();
            var pluginFile = context.ParseResult.GetValueForOption(pluginOption);
            var force = context.ParseResult.GetValueForOption(forceOption);

            try
            {
                var config = await DspConfigService.LoadAsync(token).ConfigureAwait(false);

                var targets = new List<string>();
                var discovered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (pluginFile is not null)
                {
                    if (!pluginFile.Exists)
                    {
                        throw new FileNotFoundException($"Plugin file not found: {pluginFile.FullName}");
                    }

                    targets.Add(Path.GetFullPath(pluginFile.FullName));
                }
                else
                {
                    if (config.PluginDirectories.Count == 0)
                    {
                        throw new InvalidOperationException("No plugin directories configured. Use 'dsp set-dir add <path>' first.");
                    }

                    foreach (var directory in config.PluginDirectories)
                    {
                        if (!Directory.Exists(directory))
                        {
                            Log.Debug("[dsp] Directory missing, skipping: {Directory}", directory);
                            continue;
                        }

                        foreach (var file in Directory.EnumerateFiles(directory, "*.vst3", SearchOption.AllDirectories))
                        {
                            var full = Path.GetFullPath(file);
                            discovered.Add(full);
                            if (!targets.Contains(full))
                            {
                                targets.Add(full);
                            }
                        }
                    }

                    // prune removed plugins
                    var missing = config.Plugins.Keys
                        .Where(path => !discovered.Contains(path))
                        .ToList();
                    if (missing.Count > 0)
                    {
                        foreach (var path in missing)
                        {
                            config.Plugins.Remove(path);
                        }
                        if (missing.Count > 0)
                        {
                            Log.Debug("[dsp] Removed {Count} cached plugin entries that no longer exist", missing.Count);
                        }
                    }
                }

                if (targets.Count == 0)
                {
                    Log.Debug("[dsp] No plugins found to scan");
                    await DspConfigService.SaveAsync(config, token).ConfigureAwait(false);
                    return;
                }

                var scanned = 0;
                var skipped = 0;
                var failed = 0;

                foreach (var pluginPath in targets.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
                {
                    token.ThrowIfCancellationRequested();

                    var lastWrite = File.GetLastWriteTimeUtc(pluginPath);
                    if (!force && config.Plugins.TryGetValue(pluginPath, out var existing) && existing.PluginModifiedUtc >= lastWrite)
                    {
                        skipped++;
                        continue;
                    }

                    var stdout = new List<string>();
                    var stderr = new List<string>();

                    var args = new List<string> { "listParameters", $"--plugin={pluginPath}" };
                    var exitCode = await PlugalyzerService.RunAsync(args, Path.GetDirectoryName(pluginPath), token,
                        line => stdout.Add(line),
                        line => stderr.Add(line)).ConfigureAwait(false);

                    if (exitCode != 0)
                    {
                        failed++;
                        Log.Error("[dsp] Plugalyzer exited with code {Code} for plugin {Plugin}", exitCode, pluginPath);
                        if (stderr.Count > 0)
                        {
                            foreach (var line in stderr)
                            {
                                Log.Error("[dsp] {Line}", line);
                            }
                        }
                        continue;
                    }

                    var raw = string.Join(Environment.NewLine, stdout);
                    var pluginName = ExtractPluginName(stdout);
                    var parameters = ParseParameterLines(stdout);
                    var metadata = new DspPluginMetadata(
                        Path: pluginPath,
                        PluginName: pluginName,
                        RawParameters: raw,
                        Parameters: parameters,
                        ScannedAtUtc: DateTimeOffset.UtcNow,
                        PluginModifiedUtc: lastWrite);

                    config.Plugins[pluginPath] = metadata;
                    scanned++;

                    Log.Debug("[dsp] Cached metadata for {Plugin}", pluginName ?? pluginPath);
                }

                await DspConfigService.SaveAsync(config, token).ConfigureAwait(false);
                Log.Debug("[dsp] Scan complete. Scanned {Scanned}, skipped {Skipped}, failed {Failed}", scanned, skipped, failed);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "dsp init failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    private static FileInfo ResolveOutputFile(FileInfo? provided, FileInfo inputFile)
    {
        if (provided is not null)
        {
            return provided;
        }

        if (DspSessionState.OutputMode == DspOutputMode.Post)
        {
            try
            {
                return CommandInputResolver.ResolveOutput(null, "dsp.wav");
            }
            catch (Exception ex)
            {
                Log.Debug("[dsp] Falling back to source directory for output resolution: {0}", ex.Message);
            }
        }

        var stem = Path.GetFileNameWithoutExtension(inputFile.Name);
        var directory = inputFile.DirectoryName ?? Directory.GetCurrentDirectory();
        var suffix = DspSessionState.OutputMode == DspOutputMode.Source ? "treated" : "dsp";
        return new FileInfo(Path.Combine(directory, $"{stem}.{suffix}.wav"));
    }

    private static FileInfo ResolveChainFile(FileInfo? provided, string? baseDirectory = null)
    {
        if (provided is not null)
        {
            return new FileInfo(Path.GetFullPath(provided.FullName));
        }

        var root = baseDirectory ?? Directory.GetCurrentDirectory();
        return new FileInfo(Path.Combine(root, DefaultChainFileName));
    }

    private static async Task<TreatmentChain> LoadChainAsync(FileInfo chainFile, CancellationToken cancellationToken, bool createIfMissing = false)
    {
        if (!chainFile.Exists)
        {
            if (!createIfMissing)
            {
                throw new FileNotFoundException($"Chain file not found: {chainFile.FullName}");
            }

            return new TreatmentChain();
        }

        await using var stream = chainFile.OpenRead();
        var chain = await JsonSerializer.DeserializeAsync<TreatmentChain>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        }, cancellationToken).ConfigureAwait(false);

        if (chain is null)
        {
            return new TreatmentChain();
        }

        return chain;
    }

    private static async Task SaveChainAsync(FileInfo chainFile, TreatmentChain chain, CancellationToken cancellationToken)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        Directory.CreateDirectory(chainFile.DirectoryName ?? Directory.GetCurrentDirectory());
        await using var stream = new FileStream(chainFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, chain, options, cancellationToken).ConfigureAwait(false);
    }

    private static TreatmentChain BuildSingleNodeChain(
        string plugin,
        IReadOnlyList<string> parameters,
        string? preset,
        FileInfo? paramFile,
        string baseDirectory,
        DspConfig? config = null)
    {
        var pluginPath = config is not null
            ? ResolvePluginPath(plugin, baseDirectory, config)
            : ResolvePath(plugin, baseDirectory);

        var friendlyName = config is not null ? TryGetFriendlyName(config, pluginPath) : null;

        var node = new TreatmentNode(
            name: friendlyName ?? Path.GetFileNameWithoutExtension(pluginPath),
            plugin: pluginPath,
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

                Log.Debug("[dsp] Node {Index}/{Total}: {Name}", index + 1, chain.Nodes.Count, nodeName);

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

            Log.Debug("[dsp] Chain complete. Output -> {Output}", outputPath);
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
                Log.Debug("[dsp] Intermediate files kept at {Path}", workRoot);
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

        var bitDepth = overrideBitDepth ?? node.BitDepth ?? chain.BitDepth ?? DefaultBitDepth;
        args.Add($"--bitDepth={bitDepth}");

        if (!string.IsNullOrWhiteSpace(node.ParameterFile))
        {
            var paramFilePath = ResolvePath(node.ParameterFile!, baseDirectory ?? Directory.GetCurrentDirectory());
            args.Add($"--paramFile={paramFilePath}");
        }

        if (!string.IsNullOrWhiteSpace(node.Preset))
        {
            var presetPath = ResolvePath(node.Preset!, baseDirectory ?? Directory.GetCurrentDirectory());
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

    private static string? ExtractPluginName(IReadOnlyList<string> lines)
    {
        foreach (var raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            if (raw.Contains("Loaded plugin", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(raw, "Loaded plugin\\s+\"(?<name>.+?)\"", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups["name"].Value.Trim();
                }

                var parts = raw.Split('"');
                if (parts.Length >= 2)
                {
                    return parts[1].Trim();
                }

                return raw.Trim();
            }
        }

        return null;
    }

    private static IReadOnlyList<DspPluginParameter>? ParseParameterLines(IReadOnlyList<string> lines)
    {
        var parameters = new List<DspPluginParameter>();

        for (int i = 0; i < lines.Count; i++)
        {
            var raw = lines[i];
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var line = raw.Trim();
            if (!char.IsDigit(line[0]))
            {
                continue;
            }

            var colonIndex = line.IndexOf(':');
            if (colonIndex <= 0)
            {
                continue;
            }

            if (!int.TryParse(line[..colonIndex], out var index))
            {
                continue;
            }

            var remainder = line[(colonIndex + 1)..].Trim();
            if (string.IsNullOrWhiteSpace(remainder))
            {
                continue;
            }

            var builder = new StringBuilder(remainder);

            // include indented continuation lines
            int j = i + 1;
            while (j < lines.Count)
            {
                var next = lines[j];
                if (string.IsNullOrWhiteSpace(next) || !char.IsWhiteSpace(next[0]))
                {
                    break;
                }

                builder.Append(' ');
                builder.Append(next.Trim());
                j++;
            }

            i = j - 1;

            var aggregate = builder.ToString();
            var metaMatches = Regex.Matches(
                aggregate,
                @"(?<label>Values|Default|Supports text values):\s*(?<value>.*?)(?=(?:\s+(?:Values|Default|Supports text values):)|$)",
                RegexOptions.IgnoreCase);

            string name;
            if (metaMatches.Count > 0)
            {
                name = aggregate[..metaMatches[0].Index].Trim();
            }
            else
            {
                name = aggregate.Trim();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            string? values = null;
            string? defaultValue = null;
            bool? supportsText = null;

            foreach (Match match in metaMatches)
            {
                var label = match.Groups["label"].Value.Trim().ToLowerInvariant();
                var value = match.Groups["value"].Value.Trim();
                if (value.Length == 0)
                {
                    continue;
                }

                switch (label)
                {
                    case "values":
                        values = value;
                        break;
                    case "default":
                        defaultValue = value;
                        break;
                    case "supports text values":
                        if (value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        {
                            supportsText = bool.Parse(value);
                        }
                        break;
                }
            }

            parameters.Add(new DspPluginParameter(index, name, values, defaultValue, supportsText));
        }

        return parameters.Count > 0 ? parameters : null;
    }

    private static string? TryGetFriendlyName(DspConfig config, string pluginPath)
    {
        pluginPath = Path.GetFullPath(pluginPath);
        if (config.Plugins.TryGetValue(pluginPath, out var metadata))
        {
            return metadata.PluginName;
        }

        return null;
    }

    private static string ResolvePluginPath(string token, string baseDirectory, DspConfig config)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Plugin token cannot be empty", nameof(token));
        }

        string candidate;
        try
        {
            candidate = ResolvePath(token, baseDirectory);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }
        catch
        {
            candidate = token;
        }

        // Attempt to match friendly name or file name from cache
        var matches = config.Plugins.Values
            .Where(meta => string.Equals(meta.PluginName, token, StringComparison.OrdinalIgnoreCase)
                           || string.Equals(Path.GetFileNameWithoutExtension(meta.Path), token, StringComparison.OrdinalIgnoreCase)
                           || string.Equals(meta.Path, token, StringComparison.OrdinalIgnoreCase))
            .Select(meta => meta.Path)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (matches.Count == 1)
        {
            return matches[0];
        }

        if (matches.Count > 1)
        {
            throw new InvalidOperationException($"Ambiguous plugin token '{token}'. Matches: {string.Join(", ", matches)}");
        }

        throw new FileNotFoundException($"Unable to resolve plugin '{token}'. Provide a full path or run 'dsp init' to cache it.");
    }

    private static TreatmentNode CreateNodeFromOptions(InvocationContext context, NodeOptionBundle options, string baseDirectory, DspConfig config)
    {
        var pluginToken = context.ParseResult.GetValueForOption(options.Plugin);
        if (string.IsNullOrWhiteSpace(pluginToken))
        {
            throw new InvalidOperationException("Plugin path or name is required.");
        }

        var pluginPath = ResolvePluginPath(pluginToken!, baseDirectory, config);
        if (!File.Exists(pluginPath))
        {
            throw new FileNotFoundException($"Plugin file not found: {pluginPath}");
        }

        var friendly = TryGetFriendlyName(config, pluginPath);

        var name = context.ParseResult.GetValueForOption(options.Name);
        var description = context.ParseResult.GetValueForOption(options.Description);
        var parameters = context.ParseResult.GetValueForOption(options.Parameters) ?? Array.Empty<string>();
        var paramFile = context.ParseResult.GetValueForOption(options.ParameterFile);
        var preset = context.ParseResult.GetValueForOption(options.Preset);
        var sampleRate = context.ParseResult.GetValueForOption(options.SampleRate);
        var blockSize = context.ParseResult.GetValueForOption(options.BlockSize);
        var outChannels = context.ParseResult.GetValueForOption(options.OutChannels);
        var bitDepth = context.ParseResult.GetValueForOption(options.BitDepth);
        var inputs = context.ParseResult.GetValueForOption(options.Inputs);
        var midiInput = context.ParseResult.GetValueForOption(options.MidiInput);
        var additional = context.ParseResult.GetValueForOption(options.AdditionalArguments);
        var outputFile = context.ParseResult.GetValueForOption(options.OutputFile);

        var resolvedName = !string.IsNullOrWhiteSpace(name)
            ? name
            : friendly ?? Path.GetFileNameWithoutExtension(pluginPath);

        return new TreatmentNode(
            name: resolvedName,
            plugin: pluginPath,
            description: string.IsNullOrWhiteSpace(description) ? null : description,
            parameters: parameters.Length > 0 ? parameters : null,
            parameterFile: paramFile?.FullName,
            preset: string.IsNullOrWhiteSpace(preset) ? null : ResolvePath(preset!, baseDirectory),
            sampleRate: sampleRate,
            blockSize: blockSize,
            outChannels: outChannels,
            bitDepth: bitDepth,
            inputs: inputs is { Length: > 0 } ? inputs : null,
            midiInput: string.IsNullOrWhiteSpace(midiInput) ? null : midiInput,
            additionalArguments: additional is { Length: > 0 } ? additional : null,
            outputFile: string.IsNullOrWhiteSpace(outputFile) ? null : outputFile);
    }

    private sealed class NodeOptionBundle
    {
        public Option<string> Plugin { get; }
        public Option<string?> Name { get; }
        public Option<string?> Description { get; }
        public Option<string[]> Parameters { get; }
        public Option<FileInfo?> ParameterFile { get; }
        public Option<string?> Preset { get; }
        public Option<int?> SampleRate { get; }
        public Option<int?> BlockSize { get; }
        public Option<int?> OutChannels { get; }
        public Option<int?> BitDepth { get; }
        public Option<string[]> Inputs { get; }
        public Option<string?> MidiInput { get; }
        public Option<string[]> AdditionalArguments { get; }
        public Option<string?> OutputFile { get; }

        public NodeOptionBundle(Command command)
        {
            Plugin = new Option<string>("--plugin", description: "Plugin path or friendly name")
            {
                IsRequired = true
            };
            Name = new Option<string?>("--name", "Optional logical name for the node");
            Description = new Option<string?>("--description", "Optional notes for the node");
            Parameters = new Option<string[]>("--param", () => Array.Empty<string>(), "Parameter override in Plugalyzer syntax (repeatable)");
            ParameterFile = new Option<FileInfo?>("--param-file", "JSON automation file for parameters");
            Preset = new Option<string?>("--preset", "Optional preset file relative to chain");
            SampleRate = new Option<int?>("--sample-rate", "Node sample rate override");
            BlockSize = new Option<int?>("--block-size", "Node block size override");
            OutChannels = new Option<int?>("--out-channels", "Node output channel override");
            BitDepth = new Option<int?>("--bit-depth", "Node output bit depth override");
            Inputs = new Option<string[]>("--input", () => Array.Empty<string>(), "Explicit inputs (default uses previous output)");
            MidiInput = new Option<string?>("--midi-input", "Optional MIDI file token or path");
            AdditionalArguments = new Option<string[]>("--arg", () => Array.Empty<string>(), "Additional Plugalyzer arguments (repeatable)");
            OutputFile = new Option<string?>("--output-file", "Explicit output file name inside working directory");

            command.AddOption(Plugin);
            command.AddOption(Name);
            command.AddOption(Description);
            command.AddOption(Parameters);
            command.AddOption(ParameterFile);
            command.AddOption(Preset);
            command.AddOption(SampleRate);
            command.AddOption(BlockSize);
            command.AddOption(OutChannels);
            command.AddOption(BitDepth);
            command.AddOption(Inputs);
            command.AddOption(MidiInput);
            command.AddOption(AdditionalArguments);
            command.AddOption(OutputFile);
        }
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
            Log.Debug("Failed to delete temporary directory {Path}", path);
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
            Log.Debug("Failed to delete temporary file {Path}", path);
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
