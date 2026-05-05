using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public sealed class ObjectSpawner : CustomPrefab {
    internal static readonly Dictionary<string, SpawnSet> spawnSets = new Dictionary<string, SpawnSet>();

    public readonly SpawnSet spawns;

    [SetsRequiredMembers]
    public ObjectSpawner(string id, SpawnSet spawns) : base(id, "", "") {
        this.spawns = spawns;
        AddOnRegister(() => { spawnSets[Info.ClassID] = spawns; });
    }

    public GameObject GetGameObject() {
        GameObject go = new GameObject();
        go.EnsureComponent<ObjectSpawnerTag>().spawns = spawns;
        go.EnsureComponent<TechTag>().type = Info.TechType;
        go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        return go;
    }

    class ObjectSpawnerTag : MonoBehaviour {
        internal SpawnSet spawns;

        void Update() {
            if (spawns == null) {
                string id = this.GetComponent<PrefabIdentifier>().classId;
                spawns = ObjectSpawner.spawnSets.ContainsKey(id) ? ObjectSpawner.spawnSets[id] : null;
                if (spawns == null)
                    SNUtil.log("No spawn IDs for prefab " + id + " @ " + transform.position);
                return;
            }

            GameObject go = ObjectUtil.createWorldObject(spawns.spawnIDs.getRandomEntry().getPrefabID());
            go.transform.rotation = transform.rotation;
            go.transform.Rotate(go.transform.up, UnityEngine.Random.Range(0F, 360F));
            go.transform.position = transform.position;
            if (spawns.onSpawn != null)
                spawns.onSpawn.Invoke(go, transform);
            gameObject.destroy(false);
        }
    }

    public class SpawnSet {
        public readonly WeightedRandom<PrefabReference> spawnIDs;
        public readonly Action<GameObject, Transform> onSpawn;

        public SpawnSet(WeightedRandom<PrefabReference> spawns, Action<GameObject, Transform> act = null) {
            spawnIDs = spawns;
            onSpawn = act;
        }
    }
}