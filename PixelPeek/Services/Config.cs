using System.Text;
using System.Text.Json;

namespace PixelPeek.Services;

public class Config : IConfig
{
    #region Fields

    /// <summary>
    /// Config key/value storage.
    /// </summary>
    private Dictionary<string, object>? _storage;
    
    #endregion
    
    #region IConfig implementation
    
    /// <summary>
    /// <inheritdoc cref="IConfig.Get{T}"/>
    /// </summary>
    public T? Get<T>(string key)
    {
        if (_storage is null)
        {
            this.LoadConfig();
        }

        _storage ??= new();

        if (!_storage.TryGetValue(key, out var value))
        {
            return default;
        }

        try
        {
            return (T)value;
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// <inheritdoc cref="IConfig.Set"/>
    /// </summary>
    public void Set(string key, object value)
    {
        if (_storage is null)
        {
            this.LoadConfig();
        }

        _storage ??= new();
        _storage[key] = value;

        this.SaveConfig();
    }
    
    #endregion
    
    #region Helper functions

    /// <summary>
    /// Load config from disk.
    /// </summary>
    private void LoadConfig()
    {
        try
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "config.json");

            if (!File.Exists(path))
            {
                return;
            }
            
            var json = File.ReadAllText(path, Encoding.UTF8);
            var storage = JsonSerializer.Deserialize<Dictionary<string, object>>(json)
                          ?? throw new Exception("Unable to deserialize config JSON.");

            _storage = storage;
        }
        catch
        {
            // Do nothing.
        }
    }

    /// <summary>
    /// Save config to disk.
    /// </summary>
    private void SaveConfig()
    {
        try
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "config.json");

            var json = JsonSerializer.Serialize(_storage);
            
            File.WriteAllText(path, json, Encoding.UTF8);
        }
        catch
        {
            // Do nothing.
        }
    }
    
    #endregion
}