using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;


public class JsonStringLocalizer : IStringLocalizer
{
    private readonly JsonSerializer _serializer = new();
    private readonly IDistributedCache _cache;

    public JsonStringLocalizer(IDistributedCache cache)
    {
        _cache = cache;
    }

    public LocalizedString this[string key]
    {
        get
        {
            key = key.ToLower();
            var value = GetString(key);
            return new LocalizedString(key, value);
        }
    }

    public LocalizedString this[string key, params object[] arguments]
    {
        get
        {
            key = key.ToLower();
            var actualValue = this[key];
            return actualValue.ResourceNotFound
                ? actualValue
                : new LocalizedString(key, string.Format(actualValue.Value, arguments));
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var filePath = $"Localization/Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";
        using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader streamReader = new(stream);
        using JsonTextReader reader = new(streamReader);
        while (reader.Read())
        {
            if (reader.TokenType != JsonToken.PropertyName)
                continue;

            var key = reader.Value as string;
            reader.Read();
            var value = _serializer.Deserialize<string>(reader);
            yield return new LocalizedString(key, value);
        }
    }

    private string GetString(string key)
    {
        var filePath = $"Localization/Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";
        var fullFilePath = Path.GetFullPath(filePath);

        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (File.Exists(fullFilePath))
        {
            var cacheKey = $"{Thread.CurrentThread.CurrentCulture.Name}_{key}";
            var cacheValue = _cache.GetString(cacheKey);
            
            if (!string.IsNullOrEmpty(cacheValue))
                return cacheValue;
            
            var res = GetValueFromJSON(key, fullFilePath);
            _cache.SetString(cacheKey, res);
            return res;
        }

        return string.Empty;
    }

    private string GetValueFromJSON(string propertyName, string filePath)
    {
        using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader streamReader = new(stream);
        using JsonTextReader reader = new(streamReader);

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName && reader.Value as string == propertyName)
            {
                reader.Read();
                return _serializer.Deserialize<string>(reader);
            }
        }
        return propertyName;
    }
}
