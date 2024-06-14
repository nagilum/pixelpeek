namespace PixelPeek.Models.Interfaces;

public interface IFileEntry
{
    /// <summary>
    /// Error when loading data or image.
    /// </summary>
    string? Error { get; set; }
    
    /// <summary>
    /// Filename.
    /// </summary>
    string Filename { get; }
    
    /// <summary>
    /// Full path to image.
    /// </summary>
    string FullPath { get; }
    
    /// <summary>
    /// Loaded bitmap.
    /// </summary>
    Bitmap? Bitmap { get; set; }
}