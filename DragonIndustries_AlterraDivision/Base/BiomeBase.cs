using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public abstract class BiomeBase : IComparable<BiomeBase> {
    private static readonly List<string> Variants = [
        "",
        "_cave",
        "_cave_dark",
        "_cave_light",
        "_cave_trans",
        "_CaveEntrance",
        "_Caves",
        "_Geyser",
        "_ThermalVent",
        "_Skeleton",
        "_Water",
    ];

    private static readonly Dictionary<string, BiomeBase> BiomeMap = new();
    private static readonly List<BiomeBase> BiomeList = [];
    private static readonly List<CustomBiome> CustomBiomes = [];

    public static readonly UnknownBiome Unrecognized = new();

    public readonly string DisplayName;
    public readonly string MainID;
    public readonly float SceneryValue;
    private readonly HashSet<string> _internalNames = [];

    internal static readonly Dictionary<Vector3, BiomeBase> BiomeHoles = new();

    internal static void InitializeBiomeHoles() {
        BiomeHoles[new Vector3(1042.7F, -500F, 919.11F)] = VanillaBiomes.Mountains;
    }

    public static IEnumerable<BiomeBase> GetAllBiomes() {
        return new ReadOnlyCollection<BiomeBase>(BiomeList);
    }

    public static IEnumerable<CustomBiome> GetCustomBiomes() {
        return new ReadOnlyCollection<CustomBiome>(CustomBiomes);
    }

    protected BiomeBase(string d, float deco, params string[] ids) {
        SceneryValue = deco;
        DisplayName = d;
        MainID = ids.Length == 0 ? null : ids[0];
        foreach (var id in ids)
            RegisterID(this, id);
        BiomeList.Add(this);
        if (this is CustomBiome)
            CustomBiomes.Add((CustomBiome)this);
        SNUtil.Log(
            "Registered biome " + DisplayName + " with ids " + string.Join(", ", ids),
            SNUtil.TryGetModDLL(true)
        );
    }

    public static bool IsUnrecognized(BiomeBase bb) {
        return bb == Unrecognized;
    }

    private static void RegisterID(BiomeBase b, string id) {
        foreach (var key in Variants.Select(s => (id + s).ToLowerInvariant())) {
            BiomeMap[key] = b;
            b._internalNames.Add(key);
        }
    }

    public bool ContainsID(string id) {
        return id == null ? this == VanillaBiomes.Void : _internalNames.Contains(id.ToLowerInvariant());
    }

    public IEnumerable<string> GetIDs() {
        return new ReadOnlyCollection<string>(_internalNames.ToList());
    }

    public override string ToString() {
        return GetType().Name + " " + DisplayName + ": [" + string.Join(", ", _internalNames) + "]";
    }

    public static BiomeBase GetBiome(Vector3 pos) {
        //if (logBiomeFetch)
        //	SNUtil.writeToChat("Getting biome at "+pos);
        var biome = DIHooks.GetBiomeAt(WaterBiomeManager.main.GetBiome(pos, false), pos); //will fire the event
        //if (logBiomeFetch)
        //	SNUtil.writeToChat("WBM found "+biome);
        if (string.IsNullOrEmpty(biome)) {
            foreach (var kvp in BiomeHoles.Where(kvp => Vector3.Distance(kvp.Key, pos) <= 125)) {
                //if (logBiomeFetch)
                //	SNUtil.writeToChat("Matched to hole "+kvp.Key+", "+kvp.Value);
                return kvp.Value;
            }
        }

        var ret = string.IsNullOrEmpty(biome) ? VanillaBiomes.Void : GetBiome(biome);
        //if (logBiomeFetch)
        //	SNUtil.writeToChat("Lookup to "+ret.displayName);
        return ret;
    }

    public static BiomeBase GetBiome(string id) {
        id = id.ToLowerInvariant();
        return BiomeMap.TryGetValue(id, out var value) ? value : Unrecognized;
    }

    public abstract bool IsCaveBiome();
    public abstract bool IsVoidBiome();
    public abstract bool ExistsInSeveralPlaces();

    public abstract bool IsInBiome(Vector3 pos);

    public int CompareTo(BiomeBase ro) {
        return this is VanillaBiomes && ro is VanillaBiomes biomes
            ? VanillaBiomes.Compare((VanillaBiomes)this, biomes)
            : this is VanillaBiomes
                ? -1
                : ro is VanillaBiomes
                    ? 1
                    : string.Compare(DisplayName, ro.DisplayName, StringComparison.InvariantCultureIgnoreCase);
    }
}

public class UnknownBiome : BiomeBase {
    internal UnknownBiome() : base("[UNRECOGNIZED BIOME]", 0) {
    }

    public override bool IsCaveBiome() {
        return false;
    }

    public override bool ExistsInSeveralPlaces() {
        return false;
    }

    public override bool IsVoidBiome() {
        return false;
    }

    public override bool IsInBiome(Vector3 pos) {
        return false;
    }
}