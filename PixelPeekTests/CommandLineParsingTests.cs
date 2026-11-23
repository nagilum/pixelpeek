namespace PixelPeekTests;

/// <summary>
/// Tests for <see cref="Program.TryParseCmdArgs"/>.
/// </summary>
public sealed class CommandLineParsingTests : IDisposable
{
    private readonly TempDirectory _dir = new();

    public void Dispose() => _dir.Dispose();

    private static ProgramOptions Parse(params string[] args)
    {
        var options = new ProgramOptions();
        Program.TryParseCmdArgs(args, options);
        return options;
    }

    private static Exception ParseFails(params string[] args)
        => Assert.ThrowsAny<Exception>(() => Parse(args));

    [Fact]
    public void NoArguments_ShowsHelp()
    {
        var options = Parse();

        Assert.True(options.ShowHelp);
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    public void HelpFlag_ShowsHelp_AndSkipsAllOtherValidation(string flag)
    {
        // A missing folder and a value flag with no value would normally throw.
        var options = Parse("/this/path/does/not/exist", flag, "--interval");

        Assert.True(options.ShowHelp);
        Assert.Empty(options.Paths);
    }

    [Fact]
    public void PathOnly_UsesDefaultsForEverythingElse()
    {
        var options = Parse(_dir.FullName);

        Assert.Equal([_dir.FullName], options.Paths);
        Assert.False(options.ShowHelp);
        Assert.Equal(0, options.FadeLength);
        Assert.False(options.Fullscreen);
        Assert.Equal(5000, options.Interval);
        Assert.Empty(options.Patterns);
        Assert.False(options.Recursive);
        Assert.False(options.Shuffle);
    }

    [Fact]
    public void MultiplePaths_AreKeptInTheGivenOrder()
    {
        var first = _dir.CreateSubdirectory("first");
        var second = _dir.CreateSubdirectory("second");

        var options = Parse(second, first);

        Assert.Equal([second, first], options.Paths);
    }

    [Fact]
    public void NonExistentPath_Throws()
    {
        var missing = Path.Combine(_dir.FullName, "missing");

        var ex = ParseFails(missing);

        Assert.Equal($"{missing} folder does not exist.", ex.Message);
    }

    [Fact]
    public void OptionsWithoutAnyPath_Throws()
    {
        var ex = ParseFails("--shuffle", "--recursive");

        Assert.Equal("You must specify at least one path.", ex.Message);
    }

    [Theory]
    [InlineData("-f", 250)]
    [InlineData("--fade", 1000)]
    [InlineData("-f", 0)]
    public void FadeFlag_SetsFadeLength(string flag, int milliseconds)
    {
        var options = Parse(_dir.FullName, flag, milliseconds.ToString());

        Assert.Equal(milliseconds, options.FadeLength);
    }

    [Theory]
    [InlineData("-i", 2500)]
    [InlineData("--interval", 10000)]
    [InlineData("-i", 0)]
    public void IntervalFlag_SetsInterval(string flag, int milliseconds)
    {
        var options = Parse(_dir.FullName, flag, milliseconds.ToString());

        Assert.Equal(milliseconds, options.Interval);
    }

    [Theory]
    [InlineData("-fs")]
    [InlineData("--fullscreen")]
    public void FullscreenFlag_EnablesFullscreen(string flag)
    {
        Assert.True(Parse(_dir.FullName, flag).Fullscreen);
    }

    [Theory]
    [InlineData("-s")]
    [InlineData("--shuffle")]
    public void ShuffleFlag_EnablesShuffle(string flag)
    {
        Assert.True(Parse(_dir.FullName, flag).Shuffle);
    }

    [Theory]
    [InlineData("-r")]
    [InlineData("--recursive")]
    public void RecursiveFlag_EnablesRecursive(string flag)
    {
        Assert.True(Parse(_dir.FullName, flag).Recursive);
    }

    [Fact]
    public void PatternFlag_CanBeRepeated()
    {
        var options = Parse(_dir.FullName, "-p", "*.jpg", "--pattern", "*.png");

        Assert.Equal(["*.jpg", "*.png"], options.Patterns);
    }

    [Fact]
    public void PatternFlag_IgnoresDuplicatePatterns()
    {
        var options = Parse(_dir.FullName, "-p", "*.jpg", "-p", "*.jpg");

        Assert.Equal(["*.jpg"], options.Patterns);
    }

    [Theory]
    [InlineData("-f")]
    [InlineData("--fade")]
    [InlineData("-i")]
    [InlineData("--interval")]
    public void NumericFlag_WithoutValue_Throws(string flag)
    {
        var ex = ParseFails(_dir.FullName, flag);

        Assert.Equal($"{flag} must be followed by a number of milliseconds.", ex.Message);
    }

    [Theory]
    [InlineData("-p")]
    [InlineData("--pattern")]
    public void PatternFlag_WithoutValue_Throws(string flag)
    {
        var ex = ParseFails(_dir.FullName, flag);

        Assert.Equal($"{flag} must be followed by a file pattern.", ex.Message);
    }

    [Theory]
    [InlineData("-f", "abc")]
    [InlineData("-f", "1.5")]
    [InlineData("-f", "-1")]
    [InlineData("--fade", "")]
    [InlineData("-i", "five")]
    [InlineData("-i", "-5000")]
    [InlineData("--interval", "99999999999")]
    public void NumericFlag_WithInvalidOrNegativeValue_Throws(string flag, string value)
    {
        var ex = ParseFails(_dir.FullName, flag, value);

        Assert.Equal($"{value} cannot be parsed as a positive integer.", ex.Message);
    }

    [Fact]
    public void AllOptions_CanBeCombinedInAnyOrder_AndFlagValuesAreNotTreatedAsPaths()
    {
        var options = Parse("-i", "1000", "-f", "300", _dir.FullName, "-p", "*.jpg", "-s", "-r", "-fs");

        Assert.Equal([_dir.FullName], options.Paths);
        Assert.Equal(1000, options.Interval);
        Assert.Equal(300, options.FadeLength);
        Assert.Equal(["*.jpg"], options.Patterns);
        Assert.True(options.Shuffle);
        Assert.True(options.Recursive);
        Assert.True(options.Fullscreen);
        Assert.False(options.ShowHelp);
    }
}
