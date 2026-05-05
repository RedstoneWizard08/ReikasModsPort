using BepInEx.Bootstrap;

namespace ReikaKalseki.DIAlterra;

public static class BepInExUtil {
    public static bool IsModLoaded(string guid) => Chainloader.PluginInfos.ContainsKey(guid);
}