using System.Collections.Generic;

namespace PixelPeek;

public record ProgramOptions
{
    public double FadeLength { get; set; }
    
    public bool Fullscreen { get; set; }

    public int Interval { get; set; } = 5000;

    public List<string> Paths { get; } = [];

    public List<string> Patterns { get; } = [];
    
    public bool Recursive { get; set; }
    
    public bool ShowHelp { get; set; }
    
    public bool Shuffle { get; set; }
}