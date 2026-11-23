using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace PixelPeek;

public class App : Application
{
    public override void Initialize()
    {
        this.Styles.Add(new FluentTheme());
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new SlideshowWindow();
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}