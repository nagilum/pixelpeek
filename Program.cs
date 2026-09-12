using System;
using Avalonia;

namespace PixelPeek;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var app = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
        
        #if DEBUG
            app.WithDeveloperTools();
        #endif

        app.StartWithClassicDesktopLifetime([]);
    }
}