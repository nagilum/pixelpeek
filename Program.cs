using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;

namespace PixelPeek;

public static class Program
{
    public static readonly ProgramOptions Options = new();
    
    public static List<string> Files = [];
    
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            TryParseCmdArgs(args);
            GetFiles();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{ex.GetType().Name}] {ex.Message}");
            return;
        }
        
        var app = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
        
        #if DEBUG
            app.WithDeveloperTools();
        #endif

        app.StartWithClassicDesktopLifetime([]);
    }
    
    private static void GetFiles()
    {
        var patterns = Options.Pattern.Split(';');

        foreach (var path in Options.Paths)
        {
            foreach (var pattern in patterns)
            {
                var files = Directory.GetFiles(
                        path,
                        pattern,
                        Options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .OrderBy(n => n)
                    .ToArray();
                
                Files.AddRange(files);
            }
        }

        if (Files.Count is 0)
        {
            throw new Exception("No files found in the specified paths.");
        }

        if (Options.Shuffle)
        {
            Files = [.. Files.Shuffle()];
        }
    }
    
    private static void TryParseCmdArgs(string[] args)
    {
        var skip = false;

        for (var i = 0; i < args.Length; i++)
        {
            if (skip)
            {
                skip = false;
                continue;
            }
            
            var argv = args[i];

            switch (argv)
            {
                case "-f":
                case "--fade":
                    if (i == args.Length - 1)
                    {
                        throw new Exception($"{argv} must be followed by a number of milliseconds.");
                    }
                    
                    argv = args[i + 1];

                    if (!int.TryParse(argv, out var fms) ||
                        fms < 0)
                    {
                        throw new Exception($"{argv} cannot be parsed as an integer.");
                    }
                    
                    Options.FadeLength = fms;
                    skip = true;
                    break;
                
                case "-fs":
                case "--fullscreen":
                    Options.Fullscreen = true;
                    break;
                
                case "-i":
                case "--interval":
                    if (i == args.Length - 1)
                    {
                        throw new Exception($"{argv} must be followed by a number of milliseconds.");
                    }

                    argv = args[i + 1];

                    if (!int.TryParse(argv, out var ims) ||
                        ims < 0)
                    {
                        throw new Exception($"{argv} cannot be parsed as an integer.");
                    }
                    
                    Options.Interval = ims;
                    skip = true;
                    break;
                
                case "-s":
                case "--shuffle":
                    Options.Shuffle = true;
                    break;
                
                case "-r":
                case "--recursive":
                    Options.Recursive = true;
                    break;
                
                case "-p":
                case "--pattern":
                    if (i == args.Length - 1)
                    {
                        throw new Exception($"{argv} must be followed by a file pattern.");
                    }
                    
                    Options.Pattern = args[i + 1];
                    skip = true;
                    break;
                
                default:
                    argv = args[i];

                    if (!Directory.Exists(argv))
                    {
                        throw new Exception($"{argv} folder does not exist.");
                    }
                    
                    Options.Paths.Add(argv);
                    break;
            }
        }
        
        if (Options.Paths.Count is 0)
        {
            throw new Exception("You must specify at least one path.");
        }
    }
}