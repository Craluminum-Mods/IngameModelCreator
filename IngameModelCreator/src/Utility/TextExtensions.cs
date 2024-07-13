using Vintagestory.API.Config;

namespace IngameModelCreator.Utility;

public static class TextExtensions
{
    public static string Localize(this string input, params object[] args)
    {
        return Lang.Get(input, args);
    }

    public static string LocalizeM(this string input, params object[] args)
    {
        return Lang.GetMatching(input, args);
    }

    public static bool HasTranslation(this string key) => Lang.HasTranslation(key);
}