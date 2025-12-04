using Slugify;

namespace App.UTIL.Extensions;

public static class SlugServiceExtensions
{
    private static SlugHelper _helper = new SlugHelper();

    public static string ToSlug(string text)
    {
        return _helper.GenerateSlug(text);
    }
}