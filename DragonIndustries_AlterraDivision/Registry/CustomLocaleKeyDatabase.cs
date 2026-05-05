using System.Collections.Generic;
using Nautilus.Handlers;

namespace ReikaKalseki.DIAlterra;

public static class CustomLocaleKeyDatabase {
    private static readonly Dictionary<string, string> localeKeys = new();

    public static void registerKeys(XMLLocale s) {
        foreach (var e in s.getEntries()) {
            registerKey(e);
        }
    }

    public static void registerKey(XMLLocale.LocaleEntry e) {
        registerKey(e.key, e.desc);
    }

    public static void registerKey(string key, string text) {
        SNUtil.log("Mapped locale key '" + key + "' to \"" + text + "\"", SNUtil.diDLL);
        if (DIHooks.HasWorldLoadStarted())
            LanguageHandler.SetLanguageLine(key, text);
        else
            localeKeys[key] = text;
    }

    public static void onLoad() {
        foreach (var kvp in localeKeys)
            LanguageHandler.SetLanguageLine(kvp.Key, kvp.Value);
    }
}