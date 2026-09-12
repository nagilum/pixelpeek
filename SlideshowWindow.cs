using System;
using System.IO;
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
    private readonly Image _image = new();
    
    private int _index = -1;

    private bool _paused;
    
    private Task? _timerTask;
    
    public SlideshowWindow()
    {
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
    
    private async Task LoadNextImage()
    {
        var completed = false;
        double? step = null;

        if (_image.Source is not null &&
            Program.Options.FadeLength > 0)
        {
            step = 1D / Program.Options.FadeLength;

            for (var i = 0; i < Program.Options.FadeLength; i++)
            {
                _image.Opacity -= step.Value;
                await Task.Delay(1);
            }
        }

        while (!completed)
        {
            _index++;

            if (_index >= Program.Files.Count)
            {
                _index = 0;
            }

            var path = Program.Files[_index];

            try
            {
                var info = new FileInfo(path);
                var bmp = new Bitmap(info.FullName);
                
                this.Title = $"{info.Name} - {bmp.Size.Width}x{bmp.Size.Height}";

                _image.Source = bmp;
                _image.Stretch = Stretch.Uniform;
                _image.Width = this.ClientSize.Width;
                _image.Height = this.ClientSize.Height;

                if (Program.Options.FadeLength > 0)
                {
                    step ??= 1D / Program.Options.FadeLength;
                    
                    for (var i = 0; i < Program.Options.FadeLength; i++)
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

        if (Program.Options.Fullscreen)
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
        _timerTask = this.OnTimerTick(TimeSpan.FromMilliseconds(Program.Options.Interval));
        return Task.CompletedTask;
    }
}