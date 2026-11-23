using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace PixelPeek;

public class SlideshowWindow : Window
{   
    private readonly Image _image = new();
    
    private int _index = -1;

    private bool _paused;
    
    private Task? _timerTask;

    private static readonly Cursor HiddenCursor = new(StandardCursorType.None);

    private static readonly TimeSpan FadeFrameInterval = TimeSpan.FromMilliseconds(16);

    private readonly DispatcherTimer _cursorTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    private Point? _lastPointerPosition;
    
    public SlideshowWindow()
    {
        this.SetupWindow();
        
        this.KeyDown += this.OnWindowKeyDown;
        this.Loaded += this.OnWindowLoaded;
        this.Resized += this.OnWindowResized;
        this.PointerMoved += this.OnWindowPointerMoved;

        _cursorTimer.Tick += this.OnCursorTimerTick;
        _cursorTimer.Start();
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        _cursorTimer.Stop();

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
                
                if (!await this.LoadNextImage())
                {
                    return;
                }

                await Task.Delay(span);
            }
        }
        catch (TaskCanceledException)
        {
            // Do nothing.
        }
    }
    
    private void OnCursorTimerTick(object? _, EventArgs e)
    {
        _cursorTimer.Stop();
        this.Cursor = HiddenCursor;
    }

    private void OnWindowPointerMoved(object? _, PointerEventArgs e)
    {
        var position = e.GetPosition(this);

        // Some platforms raise PointerMoved without actual movement (e.g. when
        // the cursor changes), so only react to real position changes.
        if (position == _lastPointerPosition)
        {
            return;
        }

        _lastPointerPosition = position;
        this.Cursor = Cursor.Default;

        // Restart the inactivity countdown.
        _cursorTimer.Stop();
        _cursorTimer.Start();
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
            if (await this.LoadNextImage())
            {
                await this.StartTimer();
            }
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
    
    /// <summary>
    /// Fades out the current image and fades in the next file that can be loaded.
    /// </summary>
    /// <returns>False if none of the files could be loaded, in which case the window is closed.</returns>
    private async Task<bool> LoadNextImage()
    {
        if (_image.Source is not null)
        {
            await this.FadeTo(0);
        }

        while (Program.Files.Count > 0)
        {
            _index = (_index + 1) % Program.Files.Count;

            var path = Program.Files[_index];
            FileInfo info;
            Bitmap bmp;

            try
            {
                info = new FileInfo(path);
                bmp = new Bitmap(info.FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ex.GetType().Name}] {ex.Message}");
                Console.WriteLine($"Path: {path}");

                // Drop the file so it isn't retried on every pass, and step back so the file
                // that moves into its slot isn't skipped. If every file fails, the list ends up
                // empty and the loop exits instead of spinning forever.
                Program.Files.RemoveAt(_index);
                _index--;
                continue;
            }

            this.Title = $"{info.Name} - {bmp.Size.Width}x{bmp.Size.Height} - PixelPeek v{Program.Version}";

            _image.Source = bmp;
            _image.Stretch = Stretch.Uniform;
            _image.Width = this.ClientSize.Width;
            _image.Height = this.ClientSize.Height;

            await this.FadeTo(1);
            return true;
        }

        Console.WriteLine("None of the files could be loaded as images.");
        this.Close();
        return false;
    }

    /// <summary>
    /// Animates the image opacity to the given value over the configured fade length.
    /// </summary>
    private async Task FadeTo(double target)
    {
        var duration = TimeSpan.FromMilliseconds(Program.Options.FadeLength);
        var start = _image.Opacity;

        if (duration <= TimeSpan.Zero ||
            Math.Abs(start - target) < 0)
        {
            _image.Opacity = target;
            return;
        }

        // Base each step on the time actually elapsed instead of counting delays, since
        // Task.Delay can't wait less than the OS timer resolution (about 15 ms on Windows).
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < duration)
        {
            _image.Opacity = start + (target - start) * (stopwatch.Elapsed / duration);
            await Task.Delay(FadeFrameInterval);
        }

        _image.Opacity = target;
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