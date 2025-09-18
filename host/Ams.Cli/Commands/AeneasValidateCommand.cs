using System.CommandLine;
using System.Net.Http;
using System.Text.Json;

namespace Ams.Cli.Commands;

public static class AeneasValidateCommand
{
    public static Command Create()
    {
        var cmd = new Command("aeneas-validate", "Validate Aeneas service health and run smoke test");

        var serviceOption = new Option<string>("--service", () => "http://localhost:8082", "Aeneas service URL");
        var smokeTestOption = new Option<bool>("--smoke-test", "Run a small smoke test with synthetic data");

        cmd.AddOption(serviceOption);
        cmd.AddOption(smokeTestOption);

        cmd.SetHandler(async (service, smokeTest) =>
        {
            var httpClient = new HttpClient();

            Console.WriteLine($"Validating Aeneas service at {service}...");

            // Health check
            try
            {
                var healthResponse = await httpClient.PostAsync($"{service}/v1/health", null);
                if (healthResponse.IsSuccessStatusCode)
                {
                    var healthJson = await healthResponse.Content.ReadAsStringAsync();
                    var health = JsonSerializer.Deserialize<JsonElement>(healthJson);
                    
                    Console.WriteLine("✓ Health check passed");
                    Console.WriteLine($"  Python version: {health.GetProperty("python_version").GetString()}");
                    Console.WriteLine($"  Aeneas version: {health.GetProperty("aeneas_version").GetString()}");
                    Console.WriteLine($"  Service: {health.GetProperty("service").GetString()}");
                }
                else
                {
                    Console.WriteLine($"✗ Health check failed: {healthResponse.StatusCode}");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Health check failed: {ex.Message}");
                Environment.Exit(1);
            }

            // Smoke test if requested
            if (smokeTest)
            {
                Console.WriteLine("\nRunning smoke test...");
                
                // Create a tiny synthetic test
                var tempDir = Path.GetTempPath();
                var testAudioPath = Path.Combine(tempDir, "test_audio.wav");
                var testLines = new[] { "Hello world.", "This is a test." };
                
                try
                {
                    // Create a minimal WAV file (silence)
                    await CreateSilentWavAsync(testAudioPath, 3.0, 44100);
                    
                    var alignRequest = new
                    {
                        chunk_id = "smoke_test",
                        audio_path = testAudioPath,
                        lines = testLines,
                        language = "eng",
                        timeout_sec = 30
                    };

                    var requestJson = JsonSerializer.Serialize(alignRequest);
                    var content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

                    var alignResponse = await httpClient.PostAsync($"{service}/v1/align-chunk", content);
                    
                    if (alignResponse.IsSuccessStatusCode)
                    {
                        var alignJson = await alignResponse.Content.ReadAsStringAsync();
                        var alignment = JsonSerializer.Deserialize<JsonElement>(alignJson);
                        
                        var fragmentCount = alignment.GetProperty("fragments").GetArrayLength();
                        Console.WriteLine($"✓ Smoke test passed: {fragmentCount} fragments aligned");
                        Console.WriteLine($"  Tool versions: Python {alignment.GetProperty("tool").GetProperty("python").GetString()}, Aeneas {alignment.GetProperty("tool").GetProperty("aeneas").GetString()}");
                    }
                    else
                    {
                        var errorContent = await alignResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"✗ Smoke test failed: {alignResponse.StatusCode} - {errorContent}");
                        Environment.Exit(1);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Smoke test failed: {ex.Message}");
                    Environment.Exit(1);
                }
                finally
                {
                    // Clean up
                    if (File.Exists(testAudioPath))
                        File.Delete(testAudioPath);
                }
            }

            Console.WriteLine("\n✓ All validations passed");

        }, serviceOption, smokeTestOption);

        return cmd;
    }

    private static Task CreateSilentWavAsync(string filePath, double durationSec, int sampleRate)
    {
        var samples = (int)(durationSec * sampleRate);
        var data = new short[samples];
        // All zeros = silence

        using var fs = new FileStream(filePath, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        // WAV header
        writer.Write("RIFF".ToCharArray());
        writer.Write((uint)(36 + samples * 2)); // ChunkSize
        writer.Write("WAVE".ToCharArray());
        writer.Write("fmt ".ToCharArray());
        writer.Write((uint)16); // Subchunk1Size
        writer.Write((ushort)1); // AudioFormat (PCM)
        writer.Write((ushort)1); // NumChannels
        writer.Write((uint)sampleRate); // SampleRate
        writer.Write((uint)(sampleRate * 2)); // ByteRate
        writer.Write((ushort)2); // BlockAlign
        writer.Write((ushort)16); // BitsPerSample
        writer.Write("data".ToCharArray());
        writer.Write((uint)(samples * 2)); // Subchunk2Size

        // Write silent samples
        foreach (var sample in data)
        {
            writer.Write(sample);
        }

        return Task.CompletedTask;
    }
}
