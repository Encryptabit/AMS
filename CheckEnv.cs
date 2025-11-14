using System;

class Program
{
    static void Main()
    {
        Console.WriteLine($"FFMPEG_ROOT={Environment.GetEnvironmentVariable("FFMPEG_ROOT")}");
        Console.WriteLine($"FFMPEG_HOME={Environment.GetEnvironmentVariable("FFMPEG_HOME")}");
        Console.WriteLine($"Path={Environment.GetEnvironmentVariable("PATH")}");
    }
}
