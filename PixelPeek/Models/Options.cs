﻿using PixelPeek.Models.Interfaces;

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
    /// <inheritdoc cref="IOptions.SortOrder"/>
    /// </summary>
    public FilesSortOrder SortOrder { get; set; } = FilesSortOrder.Alphabetical;
}