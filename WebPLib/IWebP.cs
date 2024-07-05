using System.Drawing;

namespace WebPLib;

public interface IWebP
{
    /// <summary>
    /// Read a WebP file.
    /// </summary>
    /// <param name="path">WebP file to load.</param>
    /// <returns>Bitmap of the WebP file.</returns>
    Bitmap Load(string path);
}