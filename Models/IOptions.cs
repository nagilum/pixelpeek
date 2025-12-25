namespace PixelPeek.Models;

public interface IOptions
{
    /// <summary>
    /// File to view.
    /// </summary>
    string? File { get; set; }
    
    /// <summary>
    /// Directory path.
    /// </summary>
    string? Path { get; set; }
    
    /// <summary>
    /// Get files recursively.
    /// </summary>
    bool Recursive { get; set; }
    
    /// <summary>
    /// Whether to go into fullscreen when the app starts.
    /// </summary>
    bool SetFullscreen { get; set; }
    
    /// <summary>
    /// Interval between each slideshow image, in milliseconds.
    /// </summary>
    int SlideshowInterval { get; set; }
    
    /// <summary>
    /// Whether to start the slideshow when the app starts.
    /// </summary>
    bool StartSlideshow { get; set; }
    
    /// <summary>
    /// Files sort order.
    /// </summary>
    FilesSortOrder SortOrder { get; set; }
}