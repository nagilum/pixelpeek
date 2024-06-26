using System.Diagnostics.CodeAnalysis;
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
    /// Attempt to parse command line arguments.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <param name="options">Parsed options.</param>
    /// <returns>Success.</returns>
    private static bool TryParseCmdArgs(string[] args, [NotNullWhen(returnValue: true)] out IOptions? options)
    {
        options = new Options();

        foreach (var argv in args)
        {
            switch (argv)
            {
                case "-r":
                case "--random":
                    options.SortOrder = FilesSortOrder.Random;
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