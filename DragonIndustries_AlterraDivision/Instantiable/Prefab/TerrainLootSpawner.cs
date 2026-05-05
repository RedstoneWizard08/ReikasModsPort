using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class TerrainLootSpawner : CustomPrefab {
    internal static readonly Dictionary<string, TerrainLootSpawn> spawnIDs = new();

    public readonly TerrainLootSpawn spawnID;

    //spawn with localScale vec of x=exclusion radius, y=target count, z=max range
    [SetsRequiredMembers]
    public TerrainLootSpawner(string id, string spawn) : this(id, new BasicTerrainLootSpawn(spawn)) {
    }

    [SetsRequiredMembers]
    public TerrainLootSpawner(string id, TerrainLootSpawn spawn) : base(id, "", "") {
        if (spawn == null)
            throw new Exception("Cannot register a loot spawner of null!");
        spawnID = spawn;
        AddOnRegister(() => { spawnIDs[Info.ClassID] = spawnID; });
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var go = new GameObject();
        go.EnsureComponent<TerrainLootSpawnerTag>().spawnID = spawnID;
        go.EnsureComponent<TechTag>().type = Info.TechType;
        go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        return go;
    }

    private class TerrainLootSpawnerTag : MonoBehaviour {
        internal TerrainLootSpawn spawnID;

        private int spawned;

        private void Update() {
            if (spawnID == null) {
                var id = GetComponent<PrefabIdentifier>().classId;
                spawnID = spawnIDs.ContainsKey(id) ? spawnIDs[id] : null;
                if (spawnID == null)
                    SNUtil.log("No spawn ID for prefab " + id + " @ " + transform.position);
                return;
            }

            var vec = UnityEngine.Random.insideUnitSphere.normalized;
            var ray = new Ray(transform.position, vec);
            if (UWE.Utils.RaycastIntoSharedBuffer(
                    ray,
                    transform.localScale.z,
                    Voxeland.GetTerrainLayerMask(),
                    QueryTriggerInteraction.Ignore
                ) > 0) {
                var hit = UWE.Utils.sharedHitBuffer[0];
                if (hit.transform != null) {
                    foreach (var pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(
                                 hit.point,
                                 transform.localScale.x
                             )) {
                        if (spawnID.includesPrefab(pi))
                            return;
                    }

                    var go = ObjectUtil.createWorldObject(spawnID.getRandomSpawnID());
                    go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
                    go.transform.position = hit.point;
                    spawned++;
                }
            }

            if (spawned >= transform.localScale.y)
                gameObject.destroy();
        }
    }

    public class BasicTerrainLootSpawn : TerrainLootSpawn {
        public readonly string spawnID;

        public BasicTerrainLootSpawn(string id) {
            spawnID = id;
        }

        public string getRandomSpawnID() {
            return spawnID;
        }

        public bool includesPrefab(PrefabIdentifier pi) {
            return pi && pi.ClassId == spawnID;
        }
    }

    public class WeightedTerrainLootSpawn : TerrainLootSpawn {
        private readonly WeightedRandom<string> random = new();

        public WeightedTerrainLootSpawn() {
        }

        public WeightedTerrainLootSpawn addEntry(string id, double wt) {
            random.addEntry(id, wt);
            return this;
        }

        public string getRandomSpawnID() {
            return random.getRandomEntry();
        }

        public bool includesPrefab(PrefabIdentifier pi) {
            return pi && random.hasEntry(pi.ClassId);
        }
    }

    public interface TerrainLootSpawn {
        string getRandomSpawnID();

        bool includesPrefab(PrefabIdentifier pi);
    }
}