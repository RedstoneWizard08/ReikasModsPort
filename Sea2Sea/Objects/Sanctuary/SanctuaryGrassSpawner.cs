using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class SanctuaryGrassSpawner : CustomPrefab {
    internal static readonly Simplex3DGenerator densityNoise =
        (Simplex3DGenerator)new Simplex3DGenerator(23764311).setFrequency(0.25);

    [SetsRequiredMembers]
    internal SanctuaryGrassSpawner() : base("SanctuaryGrassSpawner", "", "") {
        SetGameObject(GetGameObject());
    }

    public GameObject GetGameObject() {
        var go = new GameObject();
        go.EnsureComponent<CrashZoneSanctuaryGrassSpawnerTag>();
        go.EnsureComponent<TechTag>().type = Info.TechType;
        go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        return go;
    }

    private class CrashZoneSanctuaryGrassSpawnerTag : MonoBehaviour {
        private float age;

        private void Update() {
            if (Vector3.Distance(Player.main.transform.position, transform.position) < 100)
                age += Time.deltaTime;
            if (age < 2)
                return;
            var at = WorldUtil.getTerrainVectorAt(transform.position, 90);
            if (!at.HasValue) {
                //SNUtil.log("Grass spawner @ "+transform.position+" not finding ground");
                age = 0;
                return;
            }

            var li = WorldUtil.getTerrainMountedPositionsAround(at.Value.point, 24F, 240);
            if (li.Count < 30) {
                //SNUtil.log("Grass spawner @ "+at.Value.point+" found too few hits, only "+li.Count);
                age = 0;
                return;
            }

            foreach (var hit in li) {
                if (Vector3.Angle(hit.normal, Vector3.up) >= 30)
                    continue;
                if (densityNoise.getValue(hit.point) <= 0.25)
                    continue;
                var go = ObjectUtil.createWorldObject(SeaToSeaMod.CrashSanctuaryFern.Info.ClassID);
                go.transform.position = hit.point;
                go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
                go.transform.Rotate(new Vector3(0, Random.Range(0F, 360F), 0), Space.Self);
                go.transform.position += go.transform.up * -0.25F;
            }

            gameObject.destroy();
        }
    }
}