using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PixelPeek;

public class SlideshowWindow : Window
{
    private List<string> _files = [];
    
    private readonly Image _image = new();
    
    private int _index = -1;
    
    private readonly ProgramOptions _options = new();

    private bool _paused;
    
    private Task? _timerTask;
    
    public SlideshowWindow()
    {
        this.TryParseCmdArgs();
        this.GetFiles();
        this.SetupWindow();
        
        this.KeyDown += this.OnWindowKeyDown;
        this.Loaded += this.OnWindowLoaded;
        this.Resized += this.OnWindowResized;
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        try
        {
            _timerTask?.Dispose();
        }
        catch
        {
            // Do nothing.
        }
    }

    private async Task OnTimerTick(TimeSpan span)
    {
        await Task.Delay(span);

        try
        {
            while (true)
            {
                while (_paused)
                {
                    await Task.Delay(1);
                }
                
                await this.LoadNextImage();
                await Task.Delay(span);
            }
        }
        catch (TaskCanceledException)
        {
            // Do nothing.
        }
    }
    
    private void OnWindowKeyDown(object? _, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                this.Close();
                break;
            
            case Key.F:
                this.WindowState = this.WindowState is WindowState.FullScreen
                    ? WindowState.Normal
                    : WindowState.FullScreen;
                break;
            
            case Key.P or Key.Space:
                _paused = !_paused;
                break;
        }
    }
    
    private async void OnWindowLoaded(object? _, RoutedEventArgs e)
    {
        try
        {
            await this.StartTimer();
            await this.LoadNextImage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{ex.GetType().Name}] {ex.Message}");
        }
    }
    
    private void OnWindowResized(object? _, WindowResizedEventArgs e)
    {
        _image.Stretch = Stretch.Uniform;
        _image.Width = this.ClientSize.Width;
        _image.Height = this.ClientSize.Height;
    }
    
    private void GetFiles()
    {
        var patterns = _options.Pattern.Split(';');

        foreach (var path in _options.Paths)
        {
            foreach (var pattern in patterns)
            {
                var files = Directory.GetFiles(
                        path,
                        pattern,
                        _options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .OrderBy(n => n)
                    .ToArray();
                
                _files.AddRange(files);
            }
        }

        if (_options.Shuffle)
        {
            _files = [.. _files.Shuffle()];
        }
    }
    
    private async Task LoadNextImage()
    {
        var completed = false;
        double? step = null;

        if (_image.Source is not null &&
            _options.FadeLength > 0)
        {
            step = 1D / _options.FadeLength;

            for (var i = 0; i < _options.FadeLength; i++)
            {
                _image.Opacity -= step.Value;
                await Task.Delay(1);
            }
        }

        while (!completed)
        {
            _index++;

            if (_index >= _files.Count)
            {
                _index = 0;
            }

            var path = _files[_index];

            try
            {
                var info = new FileInfo(path);
                var bmp = new Bitmap(info.FullName);
                
                this.Title = $"{info.Name} - {bmp.Size.Width}x{bmp.Size.Height}";

                _image.Source = bmp;
                _image.Stretch = Stretch.Uniform;
                _image.Width = this.ClientSize.Width;
                _image.Height = this.ClientSize.Height;

                if (_options.FadeLength > 0)
                {
                    step ??= 1D / _options.FadeLength;
                    
                    for (var i = 0; i < _options.FadeLength; i++)
                    {
                        _image.Opacity += step.Value;
                        await Task.Delay(1);
                    }

                    _image.Opacity = 1;
                }

                completed = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ex.GetType().Name}] {ex.Message}");
                Console.WriteLine($"Path: {path}");
            }
        }
    }
    
    private void SetupWindow()
    {
        this.Background = new SolidColorBrush(Color.FromRgb(12, 16, 23));

        if (_options.Fullscreen)
        {   
            this.WindowState = WindowState.FullScreen;
        }

        var grid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        
        grid.Children.Add(_image);

        this.Content = grid;
    }
    
    private Task StartTimer()
    {
        _timerTask = this.OnTimerTick(TimeSpan.FromMilliseconds(_options.Interval));
        return Task.CompletedTask;
    }
    
    private void TryParseCmdArgs()
    {
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
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
                    
                    _options.FadeLength = fms;
                    skip = true;
                    break;
                
                case "-fs":
                case "--fullscreen":
                    _options.Fullscreen = true;
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
                    
                    _options.Interval = ims;
                    skip = true;
                    break;
                
                case "-s":
                case "--shuffle":
                    _options.Shuffle = true;
                    break;
                
                case "-r":
                case "--recursive":
                    _options.Recursive = true;
                    break;
                
                case "-p":
                case "--pattern":
                    if (i == args.Length - 1)
                    {
                        throw new Exception($"{argv} must be followed by a file pattern.");
                    }
                    
                    _options.Pattern = args[i + 1];
                    skip = true;
                    break;
                
                default:
                    argv = args[i];

                    if (!Directory.Exists(argv))
                    {
                        throw new Exception($"{argv} folder does not exist.");
                    }
                    
                    _options.Paths.Add(argv);
                    break;
            }
        }
    }
}