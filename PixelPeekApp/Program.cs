using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;

namespace PixelPeek;

public static class Program
{
    public const string Version = "0.2-beta";
    
    public static readonly ProgramOptions Options = new();

    public static List<string> Files { get; private set; } = [];

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            TryParseCmdArgs(args, Options);

            if (Options.ShowHelp)
            {
                ShowProgramUsage();
                return;
            }

            Files = GetFiles(Options);
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
    
    internal static List<string> GetFiles(ProgramOptions options)
    {
        if (options.Patterns.Count is 0)
        {
            options.Patterns.Add("*");
        }

        var result = new List<string>();

        foreach (var path in options.Paths)
        {
            foreach (var pattern in options.Patterns)
            {
                var files = Directory.GetFiles(
                        path,
                        pattern,
                        options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                result.AddRange(files);
            }
        }

        if (result.Count is 0)
        {
            throw new Exception("No files found in the specified paths.");
        }

        if (options.Shuffle)
        {
            result = [.. result.Shuffle()];
        }

        return result;
    }

    private static void ShowProgramUsage()
        => Console.WriteLine(
            $"""
            PixelPeek v{Version}
            Simple image slideshow
            
            Usage:
              PixelPeek [path-to-images] [<options>]
              
            You can specify multiple paths.
            
            Options:
              -f, --fade  <ms>        Duration of fade in milliseconds. Will be doubled for fade-out and fade-in.
              -fs, --fullscreen       Run the slideshow in fullscreen.
              -i, --interval  <ms>    Milliseconds between each image change. Defaults to 5000.
              -s, --shuffle           Shuffle images.
              -r, --recursive         Load images recursively from given paths.
              -p, --pattern  <ptn>    Set pattern of files to look for. Can be repeated. Defaults to *
            
            Source and documentation available at https://github.com/nagilum/pixelpeek
            
            """);
    
    internal static void TryParseCmdArgs(string[] args, ProgramOptions options)
    {
        if (args.Length is 0 ||
            args.Any(n => n is "-h" or "--help"))
        {
            options.ShowHelp = true;
            return;
        }
        
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
                        throw new Exception($"{argv} cannot be parsed as a positive integer.");
                    }
                    
                    options.FadeLength = fms;
                    skip = true;
                    break;
                
                case "-fs":
                case "--fullscreen":
                    options.Fullscreen = true;
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
                        throw new Exception($"{argv} cannot be parsed as a positive integer.");
                    }
                    
                    options.Interval = ims;
                    skip = true;
                    break;
                
                case "-s":
                case "--shuffle":
                    options.Shuffle = true;
                    break;
                
                case "-r":
                case "--recursive":
                    options.Recursive = true;
                    break;
                
                case "-p":
                case "--pattern":
                    if (i == args.Length - 1)
                    {
                        throw new Exception($"{argv} must be followed by a file pattern.");
                    }
                    
                    argv = args[i + 1];
                    
                    if (!options.Patterns.Contains(argv))
                    {
                        options.Patterns.Add(argv);
                    }
                    
                    skip = true;
                    break;
                
                default:
                    argv = args[i];

                    if (!Directory.Exists(argv))
                    {
                        throw new Exception($"{argv} folder does not exist.");
                    }
                    
                    options.Paths.Add(argv);
                    break;
            }
        }
        
        if (options.Paths.Count is 0)
        {
            throw new Exception("You must specify at least one path.");
        }
    }
}