using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Auroresource;

public class GeyserMaterialSpawner : MonoBehaviour {
    private static readonly WeightedRandom<TechType> Drops = new WeightedRandom<TechType>();
    private static readonly Dictionary<BiomeBase, float> BiomeMultipliers = new();
    private static readonly Dictionary<BiomeBase, HashSet<TechType>> BiomeFilters = new();

    internal Geyser Geyser;

    private BiomeBase _cachedBiome;

    private float _nextBiomeCheckTime = -1;

    private float _nextMineralTime = -1;

    static GeyserMaterialSpawner() {
        Drops.addEntry(TechType.Gold, 30);
        Drops.addEntry(TechType.Copper, 20);
        Drops.addEntry(TechType.Silver, 10);
        Drops.addEntry(TechType.Lead, 30);
        Drops.addEntry(TechType.Lithium, 40);
        Drops.addEntry(TechType.Magnetite, 10);
        Drops.addEntry(TechType.UraniniteCrystal, 5);
        Drops.addEntry(TechType.Quartz, 20);

        BiomeMultipliers[VanillaBiomes.Shallows] = 0.8F;
        BiomeMultipliers[VanillaBiomes.Jellyshroom] = 0.5F;
        BiomeMultipliers[VanillaBiomes.Underislands] = 0.25F;
        BiomeMultipliers[VanillaBiomes.Koosh] = 3.0F; //because very few, comparable to bottom of underislands

        BiomeFilters[VanillaBiomes.Shallows] = [TechType.Magnetite, TechType.UraniniteCrystal, TechType.Lithium];
        BiomeFilters[VanillaBiomes.Jellyshroom] = [TechType.UraniniteCrystal];
    }

    public static void AddGeyserMineral(TechType tt, float weight, params BiomeBase[] exclusions) {
        Drops.addEntry(tt, weight);
        foreach (var bb in exclusions) {
            if (!BiomeFilters.ContainsKey(bb)) {
                BiomeFilters[bb] = [];
            }

            BiomeFilters[bb].Add(tt);
        }
    }

    public static TechType GetRandomMineral(BiomeBase bb) {
        var blocked = BiomeFilters.TryGetValue(bb, out var filter) ? filter : null;
        var tt = Drops.getRandomEntry();
        while (blocked != null && blocked.Contains(tt))
            tt = Drops.getRandomEntry();
        return tt;
    }

    public static void AddBiomeRateMultiplier(BiomeBase bb, float rate) {
        BiomeMultipliers[bb] = rate;
    }

    public static float GetBiomeRateMultiplier(BiomeBase bb) {
        return BiomeMultipliers.ContainsKey(bb) ? BiomeMultipliers[bb] : 1;
    }

    private void Update() {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time >= _nextBiomeCheckTime) {
            _cachedBiome = null;
            _nextBiomeCheckTime = time + 10;
        }

        if (_cachedBiome == null || _cachedBiome == VanillaBiomes.Void)
            _cachedBiome = BiomeBase.GetBiome(transform.position);
        if (_nextMineralTime < 0)
            _nextMineralTime = GetRandomNextTime(0);
        if (!Geyser.erupting || Drops.isEmpty()) return;
        if (!(time >= _nextMineralTime)) return;
        TrySpawnMineral();
        _nextMineralTime = GetRandomNextTime(time); //set time no matter what, do not "queue" spawn
    }

    private bool TrySpawnMineral() {
        if (WorldUtil.getObjectsNearMatching(transform.position, 25, IsEjectedMineral).Count > 6)
            return false;
        var go = ObjectUtil.lookupPrefab(GetRandomMineral(_cachedBiome));
        if (!go) return false;
        go = go.clone(transform.position + Vector3.up * 3.5F, Random.rotationUniform);
        var rb = go.GetComponent<Rigidbody>();
        if (!rb) return true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.velocity = MathUtil.getRandomVectorAround(Vector3.zero, 6).SetY(Random.Range(5F, 18F));
        rb.angularVelocity = MathUtil.getRandomVectorAround(Vector3.zero, 8);

        return true;
    }

    private bool IsEjectedMineral(GameObject go) {
        var pp = go.GetComponent<Pickupable>();
        if (!pp || !Drops.hasEntry(pp.GetTechType()))
            return false;
        var rb = go.GetComponent<Rigidbody>();
        return rb && !rb.isKinematic;
    }

    private float GetRandomNextTime(float time) {
        return time + Random.Range(90, 240) /
            (AuroresourceMod.ModConfig.getFloat(ARConfig.ConfigEntries.GEYSER_RESOURCE_RATE) *
             GetBiomeRateMultiplier(_cachedBiome));
    }
}