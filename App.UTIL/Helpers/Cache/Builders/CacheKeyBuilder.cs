using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace App.UTIL.Helpers.Cache.Builders;

public sealed class CacheKeyBuilder
{
    private const int DefaultHashLength = 16;

    private readonly string _prefix;
    private readonly List<string> _segments = new();

    private CacheKeyBuilder(string prefix)
    {
        _prefix = NormalizeRoot(prefix);
    }

    public static CacheKeyBuilder ForPrefix(string prefix)
        => new(prefix);

    public CacheKeyBuilder With(string name, object? value, bool skipIfNullOrEmpty = true)
    {
        var normalizedValue = NormalizeValue(value);
        if (skipIfNullOrEmpty && string.IsNullOrEmpty(normalizedValue))
        {
            return this;
        }

        var safeValue = Sanitize(normalizedValue ?? string.Empty);
        _segments.Add($"{NormalizeName(name)}={safeValue}");
        return this;
    }

    public CacheKeyBuilder WithRaw(string name, string? value, bool skipIfNullOrEmpty = true)
    {
        if (skipIfNullOrEmpty && string.IsNullOrWhiteSpace(value))
        {
            return this;
        }

        var safeValue = Sanitize(value?.Trim() ?? string.Empty);
        _segments.Add($"{NormalizeName(name)}={safeValue}");
        return this;
    }

    public CacheKeyBuilder WithHashed(string name, params (string Component, object? Value)[] payload)
    {
        if (payload == null || payload.Length == 0)
        {
            return this;
        }

        var canonical = BuildCanonicalPayload(payload);
        if (string.IsNullOrEmpty(canonical))
        {
            return this;
        }

        var hash = ComputeHash(canonical);
        _segments.Add($"{NormalizeName(name)}={hash}");
        return this;
    }

    public string BuildKey()
    {
        if (_segments.Count == 0)
        {
            throw new InvalidOperationException("Structured cache keys require at least one segment.");
        }

        return string.Join(':', _segments);
    }

    public string BuildFullKey()
        => $"{_prefix}:{BuildKey()}";

    public string Prefix => _prefix;

    public string PrefixWithDelimiter => $"{_prefix}:";

    private static string NormalizeRoot(string prefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        var token = prefix.Trim();
        while (token.EndsWith(":", StringComparison.Ordinal))
        {
            token = token[..^1];
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Prefix cannot be empty.", nameof(prefix));
        }

        return token.ToLowerInvariant();
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "segment";
        }

        var trimmed = name.Trim().ToLowerInvariant();
        return Sanitize(trimmed);
    }

    private static string? NormalizeValue(object? value)
    {
        switch (value)
        {
            case null:
                return null;
            case string str:
                {
                    var trimmed = str.Trim();
                    return trimmed.Length == 0 ? null : trimmed.ToLowerInvariant();
                }
            case bool boolean:
                return boolean ? "1" : "0";
            case IFormattable formattable:
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            case IEnumerable<string> stringEnumerable:
                {
                    var normalized = stringEnumerable
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim().ToLowerInvariant())
                        .OrderBy(s => s, StringComparer.Ordinal)
                        .ToList();

                    return normalized.Count == 0 ? null : string.Join(',', normalized);
                }
            case IEnumerable<object?> objectEnumerable:
                {
                    var normalized = objectEnumerable
                        .Select(NormalizeValue)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .OrderBy(s => s, StringComparer.Ordinal)
                        .ToList();

                    return normalized.Count == 0 ? null : string.Join(',', normalized);
                }
            default:
                {
                    var str = value.ToString();
                    return string.IsNullOrWhiteSpace(str) ? null : str.Trim();
                }
        }
    }

    private static string BuildCanonicalPayload((string Component, object? Value)[] payload)
    {
        var parts = new List<string>(payload.Length);

        foreach (var (component, rawValue) in payload)
        {
            var normalizedValue = NormalizeValue(rawValue);
            if (string.IsNullOrEmpty(normalizedValue))
            {
                continue;
            }

            var safeValue = Sanitize(normalizedValue);
            parts.Add($"{NormalizeName(component)}={safeValue}");
        }

        if (parts.Count == 0)
        {
            return string.Empty;
        }

        parts.Sort(StringComparer.Ordinal);
        return string.Join('|', parts);
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(ch switch
            {
                ':' or '=' or '|' => '_',
                ' ' => '-',
                '\t' or '\r' or '\n' => '-',
                _ when char.IsControl(ch) => '_',
                _ => ch
            });
        }

        return builder.ToString();
    }

    private static string ComputeHash(string canonical)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(canonical);
        var hashBytes = sha.ComputeHash(bytes);
        var hex = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return hex.Substring(0, DefaultHashLength);
    }
}

