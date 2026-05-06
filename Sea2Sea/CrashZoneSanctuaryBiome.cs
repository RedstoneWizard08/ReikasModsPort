using System;
using System.Collections.Generic;
using System.Xml;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class CrashZoneSanctuaryBiome : CustomBiome {
    public static readonly float biomeRadius = 120;
    private static readonly float radiusFuzz = 24;
    public static readonly Vector3 biomeCenter = new(1111.16F, -360.5F, -985F);

    private static readonly Simplex3DGenerator edgeFuzz =
        (Simplex3DGenerator)new Simplex3DGenerator(2376547).setFrequency(0.1);

    public static readonly CrashZoneSanctuaryBiome instance = new();

    private readonly Dictionary<string, int> creatureCounts = new();

    private CrashZoneSanctuaryBiome() : base("Glowing Sanctuary", 1F) {
        creatureCounts[VanillaCreatures.BLADDERFISH.prefab] = 36;
        creatureCounts[VanillaCreatures.BOOMERANG.prefab] = 48;
        creatureCounts[VanillaCreatures.CAVECRAWLER.prefab] = 27;
        creatureCounts[VanillaCreatures.GASOPOD.prefab] = 4;
        creatureCounts[VanillaCreatures.HOOPFISH.prefab] = 90;
        creatureCounts[VanillaCreatures.MESMER.prefab] = 6;

        creatureCounts[VanillaCreatures.SCHOOL_HOOPFISH.prefab] = 6;
        creatureCounts[VanillaCreatures.SCHOOL_HOLEFISH.prefab] = 3;
        creatureCounts[VanillaCreatures.SCHOOL_BOOMERANG.prefab] = 6;
        creatureCounts[VanillaCreatures.SCHOOL_BLADDERFISH.prefab] = 3;
    }

    public override void register() {
        UnityEngine.Random.InitState(873451871);
        /* prebaked
        GenUtil.registerWorldgen(new PositionedPrefab(SeaToSeaMod.crashSanctuarySpawner.ClassID, biomeCenter));

        for (int i = 0; i < 160; i++) {
            Vector3 pos = MathUtil.getRandomVectorAround(biomeCenter, new Vector3(biomeRadius, 0, biomeRadius)*0.8F).setY(-300);
            if (isInBiome(pos))
                GenUtil.registerWorldgen(new PositionedPrefab(SeaToSeaMod.sanctuaryGrassSpawner.ClassID, pos));
        }
        */

        createDiscoveryStoryGoal(5, SeaToSeaMod.MiscLocale.getEntry("sanctuaryenter"));

        creatureCounts[C2CItems.sanctuaryray.ClassID] = 18;

        foreach (var kvp in creatureCounts) {
            for (var i = 0; i < kvp.Value; i++) {
                var pos = MathUtil.getRandomVectorAround(
                    biomeCenter,
                    new Vector3(biomeRadius, 0, biomeRadius) * 0.67F
                ).SetY(-340);
                if (IsInBiome(pos))
                    GenUtil.registerWorldgen(new PositionedPrefab(kvp.Key, pos));
            }
        }
    }

    public override mset.Sky getSky() {
        return WorldUtil.getSkybox("sparseReef");
    }

    public override VanillaMusic[] getMusicOptions() {
        return [VanillaMusic.COVE];
    }

    public override bool IsCaveBiome() {
        return false;
    }

    public override bool ExistsInSeveralPlaces() {
        return false;
    }

    public override bool IsInBiome(Vector3 pos) {
        var dist = Vector3.Distance(pos, biomeCenter);
        return dist <= biomeRadius + radiusFuzz && dist <= biomeRadius + edgeFuzz.getValue(pos) * radiusFuzz;
    }

    public override double getDistanceToBiome(Vector3 vec) {
        return Math.Max(0, Vector3.Distance(vec, biomeCenter) - biomeRadius);
    }

    public override float getMurkiness(float orig) {
        return 0.99F;
    }

    public override float getScatteringFactor(float orig) {
        return orig;
    }

    public override Vector3 getColorFalloff(Vector3 orig) {
        return new Vector3(40, 3.12F, 2.75F) * 0.875F;
    }

    public override float getFogStart(float orig) {
        return 18;
    }

    public override float getScatterFactor(float orig) {
        return orig;
    }

    public override Color getWaterColor(Color orig) {
        return orig;
    }

    public override float getSunScale(float orig) {
        return 0.5F;
    }

    public override bool IsVoidBiome() {
        return false;
    }

    public static void cleanPlantOverlap() { //called manually to compute prebaked positions
        HashSet<Vector3> positions = [];
        foreach (var sp in UnityEngine.Object.FindObjectsOfType<SanctuaryPlantTag>()) {
            var pos = sp.transform.position;
            if (!instance.IsInBiome(pos))
                continue;
            positions.Add(pos);
            foreach (var pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(pos, 2.5F)) {
                //does not find the grass because no collider
                if (CrashZoneSanctuarySpawner.spawnsPlant(pi.ClassId))
                    pi.gameObject.destroy();
            }
        }

        HashSet<Vector3> satellitePositions = [];
        foreach (var pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
            if (pi && CrashZoneSanctuarySpawner.spawnsPlant(pi.ClassId))
                satellitePositions.Add(pi.transform.position);
        }

        HashSet<Vector3> fernPositions = [];
        foreach (var pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
            if (pi && pi.ClassId == SeaToSeaMod.CrashSanctuaryFern.Info.ClassID) {
                foreach (var pos in positions) {
                    if (Vector3.Distance(pos, pi.transform.position) <= 3.75F) {
                        pi.gameObject.destroy();
                        break;
                    }
                }

                if (!pi || !pi.transform)
                    continue;
                foreach (var pos in satellitePositions) {
                    if (Vector3.Distance(pos, pi.transform.position) <= 2F) {
                        pi.gameObject.destroy();
                        break;
                    }
                }

                if (!pi || !pi.transform)
                    continue;
                foreach (var pos in fernPositions) {
                    if (Vector3.Distance(pos, pi.transform.position) <= 0.3F) {
                        pi.gameObject.destroy();
                        break;
                    }
                }

                if (!pi || !pi.transform)
                    continue;
                fernPositions.Add(pi.transform.position);
            }
        }
    }

    public static void dumpPlantData() {
        var path = BuildingHandler.instance.getDumpFile("sanctuary_plants");
        var doc = new XmlDocument();
        var rootnode = doc.CreateElement("Root");
        doc.AppendChild(rootnode);

        foreach (var sp in UnityEngine.Object.FindObjectsOfType<SanctuaryPlantTag>()) {
            if (!instance.IsInBiome(sp.transform.position))
                continue;
            var pfb = new PositionedPrefab(sp.GetComponent<PrefabIdentifier>());
            var e = doc.CreateElement("flame");
            pfb.saveToXML(e);
            doc.DocumentElement.AppendChild(e);
        }

        foreach (var pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
            if (pi && CrashZoneSanctuarySpawner.spawnsPlant(pi.ClassId) && instance.IsInBiome(pi.transform.position)) {
                var pfb = new PositionedPrefab(pi);
                var e = doc.CreateElement("plant");
                pfb.saveToXML(e);
                doc.DocumentElement.AppendChild(e);
            }
        }

        foreach (var pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
            if (pi && pi.ClassId == SeaToSeaMod.CrashSanctuaryFern.Info.ClassID) {
                var pfb = new PositionedPrefab(pi);
                var e = doc.CreateElement("fern");
                pfb.saveToXML(e);
                doc.DocumentElement.AppendChild(e);
            }
        }

        doc.Save(path);
    }
}