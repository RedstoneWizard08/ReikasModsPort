using System;
using System.Collections.Generic;
using Nautilus.Assets;

namespace ReikaKalseki.DIAlterra;

public class ItemRegistry {
    public static readonly ItemRegistry instance = new();

    private readonly Dictionary<string, CustomPrefab> registry = new();
    private readonly Dictionary<TechType, CustomPrefab> registryTech = new();

    private readonly List<Action<CustomPrefab>> listeners = [];

    private ItemRegistry() {
    }

    public CustomPrefab getItem(string id) {
        if (registry.ContainsKey(id)) {
            SNUtil.Log("Fetching item '" + id + "'", SNUtil.TryGetModDLL(true));
            return registry[id];
        } else {
            SNUtil.Log("Could not find item '" + id + "'", SNUtil.TryGetModDLL(true));
            return null;
        }
    }

    public void addListener(Action<CustomPrefab> a) {
        listeners.Add(a);
    }

    public CustomPrefab getItem(TechType tt, bool doLog = true) {
        if (registryTech.ContainsKey(tt)) {
            if (doLog)
                SNUtil.Log("Fetching item '" + tt + "'", SNUtil.TryGetModDLL(true));
            return registryTech[tt];
        } else {
            if (doLog)
                SNUtil.Log("Could not find item '" + tt + "'", SNUtil.TryGetModDLL(true));
            return null;
        }
    }

    public void addItem(CustomPrefab di) {
        registry[di.Info.ClassID] = di;
        registryTech[di.Info.TechType] = di;
        SNUtil.Log("Registering item '" + di + "'", SNUtil.TryGetModDLL(true));
        foreach (var a in listeners) {
            a(di);
        }
    }
}