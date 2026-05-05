using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class BKelpBumpWormSpawner : WorldGenerator {
    static BKelpBumpWormSpawner() {
    }

    public BKelpBumpWormSpawner(Vector3 pos) : base(pos) {
    }

    public override void saveToXML(XmlElement e) {
    }

    public override void loadFromXML(XmlElement e) {
    }

    public override bool generate(List<GameObject> li) {
        float r = 9;
        foreach (var pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(position, r)) {
            if (pi.ClassId == "26ce64dd-e703-470d-a0e4-acd43841bdd8" ||
                pi.ClassId == "53e89f85-44a6-4ccf-9790-efae4b5fcae9" ||
                pi.ClassId == "2dd42944-a73f-4443-ba90-bf45956e72f0" ||
                VanillaFlora.DEEP_MUSHROOM.includes(pi.ClassId)) {
                pi.gameObject.destroy(false);
            }
        }

        var placed = 0;
        for (var i = 0; i < 40; i++) {
            var pos = MathUtil.getRandomVectorAround(position, r);
            if (pos.y < position.y) {
                i--;
                continue;
            }

            var vec = position - pos;
            var ray = new Ray(pos, vec);
            if (UWE.Utils.RaycastIntoSharedBuffer(ray, vec.magnitude, Voxeland.GetTerrainLayerMask()) > 0) {
                var hit = UWE.Utils.sharedHitBuffer[0];
                if (hit.transform != null) {
                    var go = spawner(SeaToSeaMod.BkelpBumpWorm.Info.ClassID);
                    go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
                    go.transform.position = hit.point;
                    go.transform.RotateAroundLocal(go.transform.up, Random.Range(0F, 360F));
                    li.Add(go);
                    placed++;
                }
            }
        }

        if (placed < 3)
            return false;
        for (var i = 0; i < 1; i++) {
            var grub = spawner(C2CItems.broodmother.ClassID);
            grub.transform.rotation = Quaternion.identity;
            grub.transform.position = MathUtil.getRandomVectorAround(position + Vector3.up * 6, 3);
            li.Add(grub);
        }

        return true;
    }

    public override LargeWorldEntity.CellLevel getCellLevel() {
        return LargeWorldEntity.CellLevel.Far;
    }

    private static float bkelpCheckTimer;

    public static void tickSpawnValidation(Player ep) {
        var root = C2CProgression.Instance.BkelpNestBumps[0];
        if (ep && (ep.transform.position - root).sqrMagnitude <= 10000) {
            bkelpCheckTimer += Time.deltaTime;
            if (bkelpCheckTimer >= 30) {
                doSpawnCheck();
                bkelpCheckTimer = 0;
            }
        } else {
            bkelpCheckTimer = 0;
        }
    }

    private static void doSpawnCheck() {
        var empty = new HashSet<Vector3>(C2CProgression.Instance.BkelpNestBumps.ToList());
        foreach (var pos in C2CProgression.Instance.BkelpNestBumps) {
            foreach (var pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(pos, 25)) {
                if (pi.ClassId == SeaToSeaMod.BkelpBumpWorm.Info.ClassID) {
                    empty.Remove(pos);
                    break;
                }
            }
        }

        if (empty.Count > 0) {
            foreach (var pos in empty) {
                //SNUtil.writeToChat("Regenerating nest @ "+pos);
                GenUtil.fireGenerator(new BKelpBumpWormSpawner(pos + Vector3.down * 3), []);
            }
        }
    }
}