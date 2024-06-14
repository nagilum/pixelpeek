namespace PixelPeek.Models.Interfaces;

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
    /// Files sort order.
    /// </summary>
    FilesSortOrder SortOrder { get; set; }
}