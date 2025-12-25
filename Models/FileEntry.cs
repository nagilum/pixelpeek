namespace PixelPeek.Models;

public class FileEntry(string path) : IFileEntry
{
    /// <summary>
    /// <inheritdoc cref="IFileEntry.Error"/>
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// <inheritdoc cref="IFileEntry.Filename"/>
    /// </summary>
    public string Filename { get; } = Path.GetFileName(path);
    
    /// <summary>
    /// <inheritdoc cref="IFileEntry.FullPath"/>
    /// </summary>
    public string FullPath { get; } = path;

    /// <summary>
    /// <inheritdoc cref="IFileEntry.Bitmap"/>
    /// </summary>
    public Bitmap? Bitmap { get; set; }
}