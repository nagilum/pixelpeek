namespace PixelPeekTests;

/// <summary>
/// Tests for <see cref="Program.GetFiles"/>.
/// </summary>
public sealed class FileDiscoveryTests : IDisposable
{
    private readonly TempDirectory _dir = new();

    public void Dispose() => _dir.Dispose();

    private static ProgramOptions OptionsFor(params string[] paths)
    {
        var options = new ProgramOptions();
        options.Paths.AddRange(paths);
        return options;
    }

    /// <summary>
    /// Creates image000.jpg, image001.jpg, ... and returns their paths in sorted order.
    /// </summary>
    private List<string> CreateNumberedFiles(int count)
        => [.. Enumerable.Range(0, count).Select(i => _dir.CreateFile($"image{i:D3}.jpg"))];

    [Fact]
    public void NoPatterns_DefaultsToWildcard_AndReturnsAllFiles()
    {
        var jpg = _dir.CreateFile("a.jpg");
        var png = _dir.CreateFile("b.png");
        var txt = _dir.CreateFile("c.txt");
        var options = OptionsFor(_dir.FullName);

        var files = Program.GetFiles(options);

        Assert.Equal([jpg, png, txt], files);
        Assert.Equal(["*"], options.Patterns);
    }

    [Fact]
    public void Pattern_OnlyReturnsMatchingFiles()
    {
        var jpg = _dir.CreateFile("photo.jpg");
        _dir.CreateFile("photo.png");
        _dir.CreateFile("notes.txt");
        var options = OptionsFor(_dir.FullName);
        options.Patterns.Add("*.jpg");

        Assert.Equal([jpg], Program.GetFiles(options));
    }

    [Fact]
    public void MultiplePatterns_ReturnFilesMatchingAnyOfThem()
    {
        var jpg = _dir.CreateFile("a.jpg");
        var png = _dir.CreateFile("b.png");
        _dir.CreateFile("c.txt");
        var options = OptionsFor(_dir.FullName);
        options.Patterns.AddRange(["*.jpg", "*.png"]);

        var files = Program.GetFiles(options);

        Assert.Equal(2, files.Count);
        Assert.Contains(jpg, files);
        Assert.Contains(png, files);
    }

    [Fact]
    public void Files_AreSortedByName_IgnoringCase()
    {
        // Plain ordinal sorting would put upper case first: B, C, a.
        var upperC = _dir.CreateFile("C.png");
        var lowerA = _dir.CreateFile("a.png");
        var upperB = _dir.CreateFile("B.png");

        var files = Program.GetFiles(OptionsFor(_dir.FullName));

        Assert.Equal([lowerA, upperB, upperC], files);
    }

    [Fact]
    public void WithoutRecursive_SubfoldersAreIgnored()
    {
        var top = _dir.CreateFile("top.jpg");
        _dir.CreateFile(Path.Combine("sub", "nested.jpg"));

        Assert.Equal([top], Program.GetFiles(OptionsFor(_dir.FullName)));
    }

    [Fact]
    public void WithRecursive_SubfoldersAreIncluded_SortedByFullPath()
    {
        var top = _dir.CreateFile("top.jpg");
        var nested = _dir.CreateFile(Path.Combine("sub", "nested.jpg"));
        var deep = _dir.CreateFile(Path.Combine("sub", "deeper", "deep.jpg"));
        var options = OptionsFor(_dir.FullName);
        options.Recursive = true;

        var files = Program.GetFiles(options);

        Assert.Equal([deep, nested, top], files);
    }

    [Fact]
    public void MultiplePaths_KeepTheGivenPathOrder()
    {
        // Sorting is done per path, so the first path's files come first
        // even when they would sort after the second path's files.
        var z = _dir.CreateFile(Path.Combine("zzz", "z.jpg"));
        var a = _dir.CreateFile(Path.Combine("aaa", "a.jpg"));
        var options = OptionsFor(Path.Combine(_dir.FullName, "zzz"), Path.Combine(_dir.FullName, "aaa"));

        Assert.Equal([z, a], Program.GetFiles(options));
    }

    [Fact]
    public void EmptyFolder_Throws()
    {
        var ex = Assert.ThrowsAny<Exception>(() => Program.GetFiles(OptionsFor(_dir.FullName)));

        Assert.Equal("No files found in the specified paths.", ex.Message);
    }

    [Fact]
    public void NoFilesMatchingPattern_Throws()
    {
        _dir.CreateFile("notes.txt");
        var options = OptionsFor(_dir.FullName);
        options.Patterns.Add("*.jpg");

        var ex = Assert.ThrowsAny<Exception>(() => Program.GetFiles(options));

        Assert.Equal("No files found in the specified paths.", ex.Message);
    }

    [Fact]
    public void Shuffle_ReturnsTheSameFiles()
    {
        var created = this.CreateNumberedFiles(100);
        var options = OptionsFor(_dir.FullName);
        options.Shuffle = true;

        var files = Program.GetFiles(options);

        Assert.Equal(created, files.Order(StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void Shuffle_ChangesTheOrder()
    {
        // The chance of 100 shuffled files coming back in sorted order is 1 in 100!, so this won't flake.
        var sorted = this.CreateNumberedFiles(100);
        var options = OptionsFor(_dir.FullName);
        options.Shuffle = true;

        Assert.NotEqual(sorted, Program.GetFiles(options));
    }
}
