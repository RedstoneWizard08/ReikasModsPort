using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class BaseCellEnviroHandler : MonoBehaviour {
    public Base seabase;
    public BaseCell cell;
    public int baseSize;

    private bool hasHatch;
    private bool hasMoonpool;
    private Bounds boundingBox;

    private readonly List<Bounds> boundingBoxPlanktonSpawns = [];

    private float timeSinceEnviro;
    private BiomeBase currentBiome;
    private float temperature;
    private float depth;
    private float planktonSpawnRate;

    private float timeSincePlankton;
    private GameObject plankton;

    private float age;

    private void Update() {
        if (!cell)
            cell = GetComponent<BaseCell>();
        if ((currentBiome == null || timeSinceEnviro > 120) && DIHooks.IsWorldLoaded() &&
            (transform.position - Player.main.transform.position).sqrMagnitude < 160000) {
            if (computeEnvironment())
                computeBaseCell();
        }

        var dT = Time.deltaTime;

        age += dT;
        timeSinceEnviro += dT;

        if (plankton) {
            timeSincePlankton = 0;
            if (planktonSpawnRate <= 0)
                plankton.destroy(false);
        } else if (Player.main && age > 15 && planktonSpawnRate > 0 && depth > 5 && (hasHatch || hasMoonpool) &&
                   baseSize > 2 && (Player.main.transform.position - transform.position).sqrMagnitude < 40000) {
            timeSincePlankton += dT;
            var iVal = (float)MathUtil.linterpolate(baseSize, 5, 30, 60, 10, true);
            if (timeSincePlankton >= iVal) {
                if (Random.Range(0F, 1F) <= planktonSpawnRate) {
                    var bb = boundingBoxPlanktonSpawns.GetRandom();
                    plankton = ObjectUtil.createWorldObject(EcoceanMod.plankton.Info.ClassID);
                    plankton.transform.position = MathUtil.getRandomVectorAround(
                        bb.RandomWithin(),
                        new Vector3(3, 0, 3)
                    );
                    plankton.transform.localScale = Vector3.one * 0.5F;
                    plankton.fullyEnable();
                    plankton.GetComponent<PlanktonCloudTag>().isBaseBound = this;
                    timeSincePlankton = 0;
                }
            }
        }
    }

    private void OnDisable() {
        OnDestroy();
    }

    private void OnDestroy() {
        if (plankton)
            plankton.destroy();
    }

    public void computeBaseCell() {
        var hatches = GetComponentsInChildren<UseableDiveHatch>();
        boundingBoxPlanktonSpawns.Clear();
        hasHatch = false;
        for (var i = 0; i < hatches.Length; i++) {
            var h = hatches[i];
            if (!h.isForEscapePod && !h.isForWaterPark) {
                hasHatch = true;
                var bb = new Bounds(cell.transform.position, Vector3.zero);
                foreach (var c in h.GetComponentsInChildren<Collider>()) {
                    bb.Encapsulate(c.bounds);
                }

                boundingBoxPlanktonSpawns.Add(bb);
            }
        }

        boundingBox = new Bounds(cell.transform.position, Vector3.zero);
        foreach (var c in cell.GetComponentsInChildren<Collider>()) {
            if (c.gameObject.GetFullHierarchyPath().Contains("/AdjustableSupport/"))
                continue;
            boundingBox.Encapsulate(c.bounds);
        }

        hasMoonpool = (bool)GetComponentInChildren<VehicleDockingBay>();
        if (hasMoonpool) {
            boundingBoxPlanktonSpawns.Clear();
            boundingBoxPlanktonSpawns.Add(boundingBox);
        }

        if (!seabase)
            seabase = gameObject.FindAncestor<Base>();
        baseSize = seabase.GetComponentsInChildren<BaseCell>().Length;
    }

    public bool computeEnvironment() {
        depth = -transform.position.y;
        currentBiome = BiomeBase.GetBiome(transform.position);
        if (currentBiome == VanillaBiomes.Void) { //void base very unlikely, probably loaded in with null
            currentBiome = null;
            return false;
        }

        temperature = WaterTemperatureSimulation.main.GetTemperature(transform.position);

        planktonSpawnRate =
            transform.position.y < -500
                ? 0
                : MushroomVaseStrand.getSpawnRate(currentBiome); //there are no plankton spawns below -500
        SNUtil.log(
            "Computed plankton spawn rate of " + planktonSpawnRate + " for base cell " + transform.position + " (" +
            currentBiome.DisplayName + ") @ " + DayNightCycle.main.timePassedAsFloat
        );

        timeSinceEnviro = 0;
        return true;
    }
}