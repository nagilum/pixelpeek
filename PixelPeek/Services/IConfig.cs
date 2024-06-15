namespace PixelPeek.Services;

public interface IConfig
{
    /// <summary>
    /// Get value from config storage.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <typeparam name="T">Type.</typeparam>
    /// <returns>Value, if found.</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Save a value to config storage.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    void Set(string key, object value);
}