using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PixelPeek.Dialogs;
using PixelPeek.Forms;
using PixelPeek.Models;
using PixelPeek.Models.Interfaces;

namespace PixelPeek;

internal static class Program
{
    /// <summary>
    /// URL to GitHub repository.
    /// </summary>
    public const string GitHubRepo = "https://github.com/nagilum/pixelpeek";

    /// <summary>
    /// Program name.
    /// </summary>
    public const string Name = "PixelPeek";

    /// <summary>
    /// Program version.
    /// </summary>
    public const string Version = "0.1-alpha";
    
    /// <summary>
    /// Application icon.
    /// </summary>
    public static Icon? ApplicationIcon { get; } = GetApplicationIcon();
    
    /// <summary>
    /// Init all the things...
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length is 0)
        {
            Application.Run(new AboutDialog());
            return;
        }

        if (!TryParseCmdArgs(args, out var options))
        {
            return;
        }
        
        Application.Run(new ViewerForm(options));
    }
    
    /// <summary>
    /// Attempt to get application icon from the executable.
    /// </summary>
    /// <returns>Application icon.</returns>
    private static Icon? GetApplicationIcon()
    {
        try
        {
            var assembly = Assembly.GetEntryAssembly()
                           ?? throw new Exception("Unable to get entry assembly.");

            return Icon.ExtractAssociatedIcon(assembly.Location);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Attempt to parse command line arguments.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <param name="options">Parsed options.</param>
    /// <returns>Success.</returns>
    private static bool TryParseCmdArgs(string[] args, [NotNullWhen(returnValue: true)] out IOptions? options)
    {
        options = new Options();

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
                case "--fullscreen":
                    options.SetFullscreen = true;
                    break;
                
                case "-i":
                case "--interval":
                    if (i == args.Length - 1)
                    {
                        MessageBox.Show(
                            $"{argv} must be followed by a number of milliseconds.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return false;
                    }

                    if (!int.TryParse(args[i + 1], out var ms) ||
                        ms < 0)
                    {
                        MessageBox.Show(
                            $"{args[i + 1]} cannot be parsed to milliseconds.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return false;
                    }

                    options.SlideshowInterval = ms;
                    skip = true;
                    break;
                
                case "-r":
                case "--random":
                    options.SortOrder = FilesSortOrder.Random;
                    break;
                
                case "-s":
                case "--slideshow":
                    options.StartSlideshow = true;
                    break;
                
                default:
                    if (Directory.Exists(argv))
                    {
                        options.Path = argv;
                    }
                    else if (File.Exists(argv))
                    {
                        options.File = argv;
                        options.Path = Path.GetDirectoryName(argv);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Invalid path \"{argv}\"",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return false;
                    }

                    break;
            }
        }

        if (options.Path is not null)
        {
            return true;
        }

        MessageBox.Show(
            "You have to supply a path to either a file or a folder.",
            "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);

        return true;
    }
}