namespace PixelPeek.Models;

public class Options : IOptions
{
    /// <summary>
    /// <inheritdoc cref="IOptions.File"/>
    /// </summary>
    public string? File { get; set; }
    
    /// <summary>
    /// <inheritdoc cref="IOptions.Path"/>
    /// </summary>
    public string? Path { get; set; }
    
    /// <summary>
    /// <inheritdoc cref="IOptions.Recursive"/>
    /// </summary>
    public bool Recursive { get; set; }

    /// <summary>
    /// <inheritdoc cref="IOptions.SetFullscreen"/>
    /// </summary>
    public bool SetFullscreen { get; set; }

    /// <summary>
    /// <inheritdoc cref="IOptions.SlideshowInterval"/>
    /// </summary>
    public int SlideshowInterval { get; set; } = 5000;

    /// <summary>
    /// <inheritdoc cref="IOptions.StartSlideshow"/>
    /// </summary>
    public bool StartSlideshow { get; set; }

    /// <summary>
    /// <inheritdoc cref="IOptions.SortOrder"/>
    /// </summary>
    public FilesSortOrder SortOrder { get; set; } = FilesSortOrder.Alphabetical;
}