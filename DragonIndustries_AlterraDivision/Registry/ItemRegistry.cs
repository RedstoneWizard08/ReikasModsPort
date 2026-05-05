using System;
using System.Collections.Generic;
using Nautilus.Assets;

namespace ReikaKalseki.DIAlterra;

public class ItemRegistry {
    public static readonly ItemRegistry instance = new ItemRegistry();

    private readonly Dictionary<string, CustomPrefab> registry = new Dictionary<string, CustomPrefab>();
    private readonly Dictionary<TechType, CustomPrefab> registryTech = new Dictionary<TechType, CustomPrefab>();

    private readonly List<Action<CustomPrefab>> listeners = new List<Action<CustomPrefab>>();

    private ItemRegistry() {
    }

    public CustomPrefab getItem(string id) {
        if (registry.ContainsKey(id)) {
            SNUtil.log("Fetching item '" + id + "'", SNUtil.tryGetModDLL(true));
            return registry[id];
        } else {
            SNUtil.log("Could not find item '" + id + "'", SNUtil.tryGetModDLL(true));
            return null;
        }
    }

    public void addListener(Action<CustomPrefab> a) {
        listeners.Add(a);
    }

    public CustomPrefab getItem(TechType tt, bool doLog = true) {
        if (registryTech.ContainsKey(tt)) {
            if (doLog)
                SNUtil.log("Fetching item '" + tt + "'", SNUtil.tryGetModDLL(true));
            return registryTech[tt];
        } else {
            if (doLog)
                SNUtil.log("Could not find item '" + tt + "'", SNUtil.tryGetModDLL(true));
            return null;
        }
    }

    public void addItem(CustomPrefab di) {
        registry[di.Info.ClassID] = di;
        registryTech[di.Info.TechType] = di;
        SNUtil.log("Registering item '" + di + "'", SNUtil.tryGetModDLL(true));
        foreach (Action<CustomPrefab> a in listeners) {
            a(di);
        }
    }
}