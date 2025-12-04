using Microsoft.Extensions.Configuration;

namespace App.UTIL.Constants.Cache;

public static class CacheConfig
{
    public static string AppPrefix { get; private set; } = "myapp";

    public static void Init(IConfiguration config)
    {
        AppPrefix = config["CacheSettings:AppPrefix"] ?? AppPrefix;
    }
}
