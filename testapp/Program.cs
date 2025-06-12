using LibAvifSharp;
using LibAvifSharp.NativeTypes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static void TestConcurrentSvtEncoding()
    {
        Console.WriteLine("Starting TestConcurrentSvtEncoding...");
        const int numTasks = 5;
        var tasks = new List<Task>();

        // Create a simple SKBitmap
        var width = 100;
        var height = 100;
        using var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Blue);
        }

        Console.WriteLine($"Created a {width}x{height} test bitmap.");

        for (int i = 0; i < numTasks; i++)
        {
            int taskId = i; // Capture loop variable for lambda
            var task = Task.Run(() =>
            {
                Console.WriteLine($"Task {taskId}: Starting SVT-AV1 encoding.");
                try
                {
                    var encodedImage = AvifEncoder.Encode(bitmap, settings =>
                    {
                        settings.CodecChoice = AvifCodecChoice.AVIF_CODEC_CHOICE_SVT;
                        settings.PixelFormat = AvifPixelFormat.AVIF_PIXEL_FORMAT_YUV420; // Example, could be others
                        settings.Quality = 50; // Set a quality
                    });
                    Console.WriteLine($"Task {taskId}: Encoding finished, encoded size: {encodedImage.MemorySpan.Length} bytes.");
                    // Optionally, save the file to inspect, but be careful with concurrent writes to the same file name
                    // File.WriteAllBytes($"test_concurrent_svt_{taskId}.avif", encodedImage.MemorySpan);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Task {taskId}: Exception during encoding: {ex.Message}");
                    // Rethrow or handle as appropriate for a test
                    throw;
                }
            });
            tasks.Add(task);
        }

        Console.WriteLine($"Waiting for {numTasks} encoding tasks to complete...");
        Task.WhenAll(tasks).Wait(); // Using .Wait() here for simplicity in a console app Main. Consider await in async Main.

        Console.WriteLine("TestConcurrentSvtEncoding completed successfully.");
    }

    static void Main(string[] args)
    {
        // Existing code from Program.cs
        var imagePath = "/home/spacy/Pictures/image (1).jpg";
        if (!File.Exists(imagePath))
        {
            Console.WriteLine($"Warning: Test image not found at {imagePath}. Creating a dummy bitmap for basic tests.");
            using var dummyBitmap = new SKBitmap(100, 100, SKColorType.Bgra8888, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(dummyBitmap))
            {
                canvas.Clear(SKColors.Green);
            }

            var ds = AvifEncoder.Encode(dummyBitmap, settings =>
            {
                settings.PixelFormat = AvifPixelFormat.AVIF_PIXEL_FORMAT_YUV420;
                settings.CodecChoice = AvifCodecChoice.AVIF_CODEC_CHOICE_SVT;
            });
            File.WriteAllBytes("test_dummy_svt.avif", ds.MemorySpan.ToArray());

            var ds2 = AvifEncoder.Encode(dummyBitmap);
            File.WriteAllBytes("test_dummy_default.avif", ds2.MemorySpan.ToArray());
        }
        else
        {
            using var bitmap = SKBitmap.Decode(imagePath);

            var ds = AvifEncoder.Encode(bitmap, settings =>
            {
                settings.PixelFormat = AvifPixelFormat.AVIF_PIXEL_FORMAT_YUV420;
                settings.CodecChoice = AvifCodecChoice.AVIF_CODEC_CHOICE_SVT;
            });
            File.WriteAllBytes("test.avif", ds.MemorySpan.ToArray());

            var ds2 = AvifEncoder.Encode(bitmap);
            File.WriteAllBytes("test2.avif", ds2.MemorySpan.ToArray());
        }
        Console.WriteLine("Original tests completed.");

        // Call the new concurrent test
        TestConcurrentSvtEncoding();

        Console.WriteLine("All tests done.");
    }
}