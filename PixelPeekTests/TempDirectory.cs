namespace PixelPeekTests;

/// <summary>
/// A unique, empty directory under the system temp folder that is deleted on dispose.
/// </summary>
public sealed class TempDirectory : IDisposable
{
    public string FullName { get; } = Directory.CreateTempSubdirectory("pixelpeek-tests-").FullName;

    /// <summary>
    /// Creates an empty file (and any missing parent folders) and returns its full path.
    /// </summary>
    public string CreateFile(string relativePath)
    {
        var fullPath = Path.Combine(this.FullName, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, []);

        return fullPath;
    }

    public string CreateSubdirectory(string name)
        => Directory.CreateDirectory(Path.Combine(this.FullName, name)).FullName;

    public void Dispose()
    {
        try
        {
            Directory.Delete(this.FullName, recursive: true);
        }
        catch (IOException)
        {
            // Leftovers in the temp folder are harmless.
        }
    }
}
