using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core;
using Ams.Core.Align;
using Ams.Core.Models;
using Xunit;

namespace Ams.Tests;

public class WindowAlignTests
{
    [Fact]
    public async Task AlignAsync_RoundsFragmentTimesAndReturnsToolVersions()
    {
        using var temp = new TempDirectory();
        var audioDir = Path.Combine(temp.DirectoryPath, "windows", "audio");
        Directory.CreateDirectory(audioDir);

        var handler = new StubHttpMessageHandler();
        handler.SetResponse("/v1/align-chunk", request =>
        {
            var payload = JsonSerializer.Deserialize<JsonElement>(request.Content!.ReadAsStringAsync().Result);
            Assert.True(payload.TryGetProperty("lines", out var linesArray));
            Assert.True(linesArray.ValueKind is JsonValueKind.Array);
            Assert.True(payload.TryGetProperty("audio_path", out var audioPath));
            Assert.Contains("window-001.wav", audioPath.GetString(), StringComparison.OrdinalIgnoreCase);
            var response = new
            {
                fragments = new[] { new { begin = 0.0001234, end = 2.9999876 } },
                tool = new { python = "3.11.0", aeneas = "1.7.3" }
            };
            return JsonSerializer.Serialize(response);
        });
        handler.SetResponse("/v1/health", _ => JsonSerializer.Serialize(new
        {
            python_version = "3.11.0",
            aeneas_version = "1.7.3"
        }));

        using var client = new HttpClient(handler);
        var service = new WindowAlignService(client, new FakeProcessRunner());

        var windows = new WindowsArtifact(
            new List<AnchorWindow> { new("window-001", 0, 10, 0, 1, null, null) },
            new WindowsParams(0.5, 0.5),
            Coverage: 1.0,
            LargestGapSec: 0.0,
            ToolVersions: new Dictionary<string, string>()
        );

        var transcript = JsonDocument.Parse(@"{ ""Words"": [ { ""Start"": 0.0001234, ""End"": 2.9999876, ""Word"": ""hello"" } ] }").RootElement;

        var request = new WindowAlignRequest(
            Path.Combine(temp.DirectoryPath, "input.wav"),
            10.0,
            windows,
            transcript,
            new WindowAlignParams(ServiceUrl: "http://localhost"),
            audioDir
        );

        var result = await service.AlignAsync(request);

        Assert.Single(result.Alignments);
        var alignment = result.Alignments[0];
        Assert.Equal("window-001", alignment.WindowId);
        Assert.Equal(0.000123, alignment.Fragments[0].Begin);
        Assert.Equal(2.999988, alignment.Fragments[0].End);
        Assert.Equal("3.11.0", result.ServiceToolVersions["python"]);
        Assert.Equal("1.7.3", result.ServiceToolVersions["aeneas"]);
        Assert.NotEmpty(alignment.DigestInput);
        Assert.True(File.Exists(Path.Combine(audioDir, "window-001.wav")));
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        private static readonly Regex OutputRegex = new("\\\"(?<path>[^\\\"]+)\\\"$", RegexOptions.Compiled);

        public Task<ProcessResult> RunAsync(string fileName, string arguments, CancellationToken ct = default)
        {
            var match = OutputRegex.Match(arguments);
            if (match.Success)
            {
                var rawPath = match.Groups["path"].Value;
                var normalized = rawPath.Replace('/', System.IO.Path.DirectorySeparatorChar);
                Directory.CreateDirectory(Path.GetDirectoryName(normalized)!);
                File.WriteAllBytes(normalized, Array.Empty<byte>());
            }
            return Task.FromResult(new ProcessResult(0, string.Empty, string.Empty));
        }
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, Func<HttpRequestMessage, string>> _routes = new(StringComparer.OrdinalIgnoreCase);

        public void SetResponse(string path, Func<HttpRequestMessage, string> factory) => _routes[path] = factory;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_routes.TryGetValue(request.RequestUri!.AbsolutePath, out var factory))
            {
                var payload = factory(request);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload)
                };
                return Task.FromResult(response);
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }

    private sealed class TempDirectory : IDisposable
    {
        public string DirectoryPath { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ams-test-" + Guid.NewGuid().ToString("N"));

        public TempDirectory()
        {
            Directory.CreateDirectory(DirectoryPath);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(DirectoryPath)) Directory.Delete(DirectoryPath, recursive: true);
            }
            catch
            {
                // ignore
            }
        }
    }
}
