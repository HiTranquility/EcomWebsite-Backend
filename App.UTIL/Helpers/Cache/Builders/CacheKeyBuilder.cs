using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.UTIL.Helpers.Cache.Builders;

/// <summary>
/// Simple builder to compose structured cache keys in a consistent way.
/// </summary>
public sealed class CacheKeyBuilder
{
    private readonly List<string> _parts;

    private CacheKeyBuilder(string prefix)
    {
        _parts = new List<string> { prefix };
    }

    public static CacheKeyBuilder ForPrefix(string prefix) => new(prefix);

    public CacheKeyBuilder AddPart(string name, object? value)
    {
        if (value == null) return this;
        var str = value.ToString();
        if (string.IsNullOrWhiteSpace(str)) return this;
        _parts.Add($"{name}:{str}");
        return this;
    }

    public string BuildKey()
    {
        if (_parts.Count == 0) return string.Empty;
        // Join with ':' to keep keys readable and consistent
        return string.Join(":", _parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}

