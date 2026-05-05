using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class WorldUtil {
    private static readonly Int3 batchOffset = new(13, 19, 13);
    private static readonly Int3 batchOffsetM = new(32, 0, 32);
    private static readonly int batchSize = 160;

    public static readonly Vector3 DUNES_METEOR = new(-1125, -380, 1130);
    public static readonly Vector3 LAVA_DOME = new(-273, -1355, -152);
    public static readonly Vector3 COVE_TREE = new(-923, -920, 439);
    public static readonly Vector3 SUNBEAM_SITE = new(301, 15, 1086);
    public static readonly Vector3 DEGASI_FLOATING_BASE = new(-763, 20, -1104);
    public static readonly Vector3 DEGASI_JELLY_BASE = new(85, -260, -356);
    public static readonly Vector3 DEGASI_DGR_BASE = new(-643, -505, -944.5F);

    public static readonly Vector3 SPARSE_VENT = new(-721.78F, -310.05F, -721.86F);
    public static readonly Vector3 MOUNTAIN_VENT = new(750.04F, -289.33F, 1018.49F);
    public static readonly Vector3 DUNES_VENT = new(-1158.62F, -253.64F, -140.30F);
    public static readonly Vector3 MUSHROOM_VENT = new(651.31F, -176.35F, 478.29F);
    public static readonly Vector3 GRANDREEF_VENT = new(-943.27F, -343.35F, -1220.01F);
    public static readonly Vector3 UNDERISLAND_VENT = new(-465.46F, -80.58F, 873.23F);

    public readonly static Vector3 lavaCastleCenter = new(-49, -1242, 118);
    public readonly static float lavaCastleInnerRadius = 65; //75;

    public readonly static float lavaCastleRadius =
        Vector3.Distance(new Vector3(-116, -1194, 126), lavaCastleCenter) + 32;

    private static readonly Vector3 auroraPoint1 = new(746, 0, -362 - 50);
    private static readonly Vector3 auroraPoint2 = new(1295, 0, 110 - 50);
    private static readonly float auroraPointRadius = 275;

    private static readonly Bounds prisonAquariumExpanded;

    //public static HashSet<PositionedPrefab> registeredGeysers = new HashSet<PositionedPrefab>();

    private static readonly HashSet<Vector3> geysers = [];

    /*
    public static void dumpGeysers() {
        string file = BuildingHandler.instance.dumpPrefabs("geysers", registeredGeysers);
        SNUtil.writeToChat("Exported "+registeredGeysers.Count+" geysers to "+file);
    }
    */
    public enum CompassDirection {
        NORTH,
        EAST,
        SOUTH,
        WEST,
    }

    public static CompassDirection getOpposite(CompassDirection dir) {
        switch (dir) {
            case CompassDirection.NORTH:
                return CompassDirection.SOUTH;
            case CompassDirection.EAST:
                return CompassDirection.WEST;
            case CompassDirection.SOUTH:
                return CompassDirection.NORTH;
            case CompassDirection.WEST:
                return CompassDirection.EAST;
        }

        throw new Exception("Unrecognized direction");
    }

    public static readonly Dictionary<CompassDirection, Vector3> compassAxes =
        new() {
            { CompassDirection.NORTH, new Vector3(0, 0, 1) },
            { CompassDirection.EAST, new Vector3(1, 0, 0) },
            { CompassDirection.SOUTH, new Vector3(0, 0, -1) },
            { CompassDirection.WEST, new Vector3(-1, 0, 0) },
        };

    //private static readonly Dictionary<string, string> biomeNames = new Dictionary<string, string>();

    static WorldUtil() {
        var file = Path.Combine(Path.GetDirectoryName(SNUtil.diDLL.Location), "geysers.xml");
        if (File.Exists(file)) {
            var doc = new XmlDocument();
            doc.Load(file);
            foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
                try {
                    var pfb = (PositionedPrefab)ObjectTemplate.construct(e);
                    geysers.Add(pfb.position);
                } catch (Exception ex) {
                    SNUtil.log(ex.ToString());
                    SNUtil.writeToChat(
                        "Could not load XML block, threw exception: " + ex.ToString() + " from " + e.format()
                    );
                }
            }
        }

        prisonAquariumExpanded = new Bounds(
            Creature.prisonAquriumBounds.center,
            Creature.prisonAquriumBounds.extents * 2
        );
        prisonAquariumExpanded.Expand(new Vector3(2, 10, 2));
    }
    /*
    private static void mapBiomeName(string name, params string[] keys) {
        foreach (string s in keys) {
            biomeNames[s] = name;
        }
    }*/

    public static Int3 getBatch(Vector3 pos) {
        //"Therefore e.g. batch (12, 18, 12) covers the voxels from (-128, -160, -128) to (32, 0, 32)."
        var coord = pos.roundToInt3();
        coord -= batchOffsetM;
        coord.x = (int)Math.Floor(coord.x / (float)batchSize);
        coord.y = (int)Math.Floor(coord.y / (float)batchSize);
        coord.z = (int)Math.Floor(coord.z / (float)batchSize);
        return coord + batchOffset;
    }

    /// <returns>The min XYZ corner</returns>
    public static Int3 getWorldCoord(Int3 batch) { //TODO https://i.imgur.com/sbXjIpq.png
        batch -= batchOffset;
        return batch * batchSize + batchOffsetM;
    }

    /*
batch_id = ((1117, -268, 568) + (2048.0,3040.0,2048.0)) / 160
batch_id = (3165.0, 2772.0, 2616.0) / 160
batch_id = (19.78125, 17.325, 16.35)
batch_id = (19, 17, 16)
     */

    public static GameObject dropItem(Vector3 pos, TechType item) {
        var id = CraftData.GetClassIdForTechType(item);
        if (id != null) {
            var go = ObjectUtil.createWorldObject(id);
            if (go != null)
                go.transform.position = pos;
            return go;
        } else {
            SNUtil.log("NO SUCH ITEM TO DROP: " + item);
            return null;
        }
    }

    public static mset.Sky getSkybox(string biome, bool allowNotFoundError = true) {
        var bb = BiomeBase.GetBiome(biome);
        if (bb is CustomBiome customBiome)
            return customBiome.getSky();
        var idx = WaterBiomeManager.main.GetBiomeIndex(biome);
        if (idx < 0) {
            if (allowNotFoundError) {
                SNUtil.writeToChat("Biome '" + biome + "' had no sky lookup. See log for biome table.");
                SNUtil.log(WaterBiomeManager.main.biomeLookup.toDebugString());
            }

            return null;
        }

        return idx < WaterBiomeManager.main.biomeSkies.Count ? WaterBiomeManager.main.biomeSkies[idx] : null;
    }

    public static C getClosest<C>(GameObject go) where C : Component {
        return getClosest<C>(go.transform.position);
    }

    public static C getClosest<C>(Vector3 pos) where C : Component {
        double distsq = -1;
        C ret = null;
        foreach (var obj in UnityEngine.Object.FindObjectsOfType<C>()) {
            if (!obj)
                continue;
            double dd = (pos - obj.transform.position).sqrMagnitude;
            if (dd < distsq || ret == null) {
                ret = obj;
                distsq = dd;
            }
        }

        return ret;
    }

    /// <remarks>Will not find things without colliders!
    /// 
    /// Avoid using this with components that will result in many findings, as you then end up iterating a large list. Use the getter version instead.
    /// </remarks>
    public static HashSet<C> getObjectsNearWithComponent<C>(Vector3 pos, float r) where C : MonoBehaviour {
        return getObjectsNear(pos, r, go => UWE.Utils.GetComponentInHierarchy<C>(go));
    }

    /// <remarks>Will not find things without colliders!</remarks>
    public static HashSet<GameObject> getObjectsNearMatching(Vector3 pos, float r, Predicate<GameObject> check) {
        return getObjectsNear(pos, r, go => check(go) ? go : null);
    }

    /// <remarks>Will not find things without colliders!</remarks>
    public static HashSet<GameObject> getObjectsNear(Vector3 pos, float r) {
        return getObjectsNear<GameObject>(pos, r, null);
    }

    /// <remarks>Will not find things without colliders!</remarks>
    public static HashSet<R> getObjectsNear<R>(Vector3 pos, float r, Func<GameObject, R> converter = null)
        where R : UnityEngine.Object {
        HashSet<R> set = [];
        getObjectsNear(
            pos,
            r,
            go => {
                set.Add(go);
                return false;
            },
            converter
        );
        return set;
    }

    /// <remarks>Will not find things without colliders!</remarks>
    public static void getGameObjectsNear(Vector3 pos, float r, Action<GameObject> getter) {
        getObjectsNear(pos, r, getter, null);
    }

    /// <remarks>Will not find things without colliders!</remarks>
    public static void getObjectsNear<R>(Vector3 pos, float r, Action<R> getter, Func<GameObject, R> converter = null)
        where R : UnityEngine.Object {
        getObjectsNear(
            pos,
            r,
            obj => {
                getter(obj);
                return false;
            },
            converter
        );
    }

    /// <remarks>Will not find things without colliders!</remarks>
    public static void getObjectsNear<R>(
        Vector3 pos,
        float r,
        Func<R, bool> getter,
        Func<GameObject, R> converter = null
    ) where R : UnityEngine.Object {
        HashSet<GameObject> found = [];
        foreach (var hit in Physics.SphereCastAll(pos, r, Vector3.up, 0.1F)) {
            if (hit.transform) {
                var go = UWE.Utils.GetEntityRoot(hit.transform.gameObject);
                if (!go)
                    go = hit.transform.gameObject;
                if (!go)
                    continue;
                if (found.Contains(go)) //prevent duplicates
                    continue;
                found.Add(go);
                var obj = converter == null ? (UnityEngine.Object)go : converter(go);
                if (obj) {
                    if (getter((R)obj))
                        return;
                }
            }
        }
    }

    /// <remarks>Will not find things without colliders!</remarks>
    public static GameObject areAnyObjectsNear(Vector3 pos, float r, Predicate<GameObject> check) {
        GameObject ret = null;
        getObjectsNear(
            pos,
            r,
            go => {
                ret = go;
                return true;
            },
            go => check(go) ? go : null
        );
        return ret;
    }

    public static bool isPlantInNativeBiome(GameObject go) {
        if (!go)
            return false;
        var pi = go.FindAncestor<PrefabIdentifier>();
        var tt = CraftData.GetTechType(go);
        if (tt == TechType.None) {
            var p = go.FindAncestor<Plantable>();
            if (p)
                tt = p.plantTechType;
        }

        var vf = VanillaFlora.getFromID(pi ? pi.ClassId : CraftData.GetClassIdForTechType(tt));
        if (vf != null && vf.isNativeToBiome(go.transform.position))
            return true;
        var plant = BasicCustomPlant.getPlant(tt);
        return plant != null && plant.isNativeToBiome(go.transform.position);
    }

    public static bool isInCave(Vector3 pos) {
        if (BiomeBase.GetBiome(pos).IsCaveBiome())
            return true;
        var b = WaterBiomeManager.main.GetBiome(pos, false);
        return !string.IsNullOrEmpty(b) && b.ToLowerInvariant().Contains("_cave");
    }

    public static bool isInWreck(Vector3 pos) {
        var biome = WaterBiomeManager.main.GetBiome(pos, false);
        return !string.IsNullOrEmpty(biome) && biome.ToLowerInvariant().Contains("wreck");
    }

    public static bool lineOfSight(GameObject o1, GameObject o2, Predicate<RaycastHit> filter = null) {
        return lineOfSight(o1, o2, o1.transform.position, o2.transform.position, filter);
    }

    public static bool lineOfSight(
        GameObject o1,
        GameObject o2,
        Vector3 pos1,
        Vector3 pos2,
        Predicate<RaycastHit> filter = null
    ) { /*
        RaycastHit hit;
        Physics.Linecast(o1.transform.position, o2.transform.position, out hit);
        if (hit) {

        }*/
        var dd = pos2 - pos1;
        var hits = Physics.RaycastAll(pos1, dd.normalized, dd.magnitude);
        foreach (var hit in hits) {
            if (!hit.collider || hit.collider.isTrigger)
                continue;
            if (hit.transform == o1.transform || hit.transform == o2.transform)
                continue;
            if (filter != null && !filter.Invoke(hit))
                continue;
            if (Array.IndexOf(o1.GetComponentsInChildren<Collider>(), hit.collider) >= 0)
                continue;
            if (Array.IndexOf(o2.GetComponentsInChildren<Collider>(), hit.collider) >= 0)
                continue;
            //SNUtil.writeToChat("Raytrace from "+o1+" to "+o2+" hit "+hit.transform+" @ "+hit.point+" (D="+hit.distance+")");
            return false;
        }

        return true;
    }
    /*
    public static float getLightAtPosition(Vector3 pos, GameLightType types) {

    }*/

    public static List<RaycastHit> getTerrainMountedPositionsAround(Vector3 pos, float range, int num) {
        List<RaycastHit> ret = [];
        for (var i = 0; i < num; i++) {
            var diff =
                new Vector3(UnityEngine.Random.Range(-range, range), 0, UnityEngine.Random.Range(-range, range))
                    .setLength(UnityEngine.Random.Range(0.01F, range));
            var pos2 = (pos + diff).setY(pos.y + 15);
            var hit = getTerrainVectorAt(pos2, 25);
            if (hit.HasValue)
                ret.Add(hit.Value);
        }

        return ret;
    }

    public static RaycastHit? getTerrainVectorAt(Vector3 pos, float maxDown = 1, Vector3? axis = null) {
        var ray = new Ray(pos, axis.HasValue ? axis.Value : Vector3.down);
        return UWE.Utils.RaycastIntoSharedBuffer(ray, maxDown, Voxeland.GetTerrainLayerMask()) > 0
            ? UWE.Utils.sharedHitBuffer[0]
            : (RaycastHit?)null;
    }

    public static bool isPrecursorBiome(Vector3 pos) {
        var over = AtmosphereDirector.main.GetBiomeOverride();
        return !string.IsNullOrEmpty(over) && over.ToLowerInvariant().Contains("precursor");
    }

    public static bool isInDRF(Vector3 pos) {
        return VanillaBiomes.Lostriver.IsInBiome(pos) && isPrecursorBiome(pos);
    }

    public static bool isInLavaCastle(Player ep) {
        return ep.IsInsideWalkable() && ep.precursorOutOfWater && isInLavaCastle(ep.transform.position);
    }

    public static bool isInLavaCastle(Vector3 pos) {
        return VanillaBiomes.Ilz.IsInBiome(pos) && isPrecursorBiome(pos);
    }

    public static bool isInsideAurora2D(Vector3 pos, float extra = 0) {
        return MathUtil.getDistanceToLineSegment(pos, auroraPoint1, auroraPoint2) <= auroraPointRadius + extra;
    }

    public static bool isMountainIsland(Vector3 pos) {
        return pos.y > 1 && ((pos - SUNBEAM_SITE).sqrMagnitude <= 2500 || VanillaBiomes.Mountains.IsInBiome(pos));
    }

    public static string getRegionalDescription(Vector3 pos, bool includeDepth) {
        if ((pos - LAVA_DOME).sqrMagnitude <= 6400)
            return "Lava Dome";
        if ((pos - DUNES_METEOR).sqrMagnitude <= 14400)
            return "Meteor Crater";
        if (isMountainIsland(pos))
            return "Mountain Island";
        var dist = (pos - lavaCastleCenter).sqrMagnitude;
        if (dist <= lavaCastleInnerRadius * lavaCastleInnerRadius + 225)
            return "Lava Castle (Interior)";
        if (dist <= lavaCastleRadius * lavaCastleRadius + 900)
            return "Lava Castle";
        var bb = BiomeBase.GetBiome(pos);
        if (pos.y < -650 && !bb.IsCaveBiome()) {
            bb = pos.y < -1000
                ? pos.y < -1400 ? VanillaBiomes.Alz : VanillaBiomes.Ilz
                : pos.y < -870 && (pos - COVE_TREE).sqrMagnitude <= 40000
                    ? VanillaBiomes.Cove
                    : VanillaBiomes.Lostriver;
        }

        if (BiomeBase.IsUnrecognized(bb))
            return "Unknown Biome @ " + pos;
        if (bb == VanillaBiomes.Lostriver || bb == VanillaBiomes.Crash) {
            switch (DIHooks.GetBiomeAt(WaterBiomeManager.main.GetBiome(pos), pos)) {
                case "LostRiver_BonesField_Corridor":
                case "LostRiver_BonesField_Corridor_Stream":
                case "LostRiver_BonesField":
                case "LostRiver_BonesField_Lake":
                case "LostRiver_BonesField_LakePit":
                    return "Bones Field";
                case "LostRiver_GhostTree_Lower":
                case "LostRiver_GhostTree":
                    return "Ghost Forest";
                case "LostRiver_Junction":
                    return "Lost River Junction";
                case "LostRiver_Canyon":
                case "LostRiver_SkeletonCave":
                    return "Ghost Canyon";
                case "Precursor_LostRiverBase":
                    return "Disease Research Facility";
                case "LostRiver_Corridor":
                    return "Lost River Corridor";
                case "crashZone_Mesa":
                    return "Crash Zone Mesas";
            }
        }

        if (bb == VanillaBiomes.Crash) {
            if (isInsideAurora2D(pos, 100)) {
                var ret = "The Aurora";
                if (pos.y >= 5 && CrashedShipExploder.main.IsExploded()) {
                    ret += " (Inside)";
                } else {
                    //Vector3 ship = (auroraPoint1+auroraPoint2)*0.5F;//CrashedShipExploder.main.transform.position;
                    var point = MathUtil.getClosestPointToLineSegment(pos, auroraPoint1, auroraPoint2);
                    var angle = Vector3.SignedAngle(auroraPoint2 - auroraPoint1, pos - point, Vector3.up);
                    angle = (angle + 360) % 360;
                    if (Mathf.Abs(angle) <= 30)
                        ret += " (Front)";
                    if (Mathf.Abs(angle - 180) <= 30)
                        ret += " (Rear)";
                    if (Mathf.Abs(angle - 90) <= 45)
                        ret += " (Far Side)";
                    if (Mathf.Abs(angle - 270) <= 45)
                        ret += " (Near Side)";
                }

                return ret;
            }
        }

        var biome = bb.DisplayName;
        var depth = (int)-pos.y;
        var ew = pos.x < 0 ? "West" : "East";
        var ns = pos.z > 0 ? "North" : "South";
        if (!bb.ExistsInSeveralPlaces() || Math.Abs(pos.x) < 250 || Math.Abs(pos.x) < Math.Abs(pos.z) / 2.5F)
            ew = "";
        if (!bb.ExistsInSeveralPlaces() || Math.Abs(pos.z) < 250 || Math.Abs(pos.z) < Math.Abs(pos.x) / 2.5F)
            ns = "";
        if (Vector3.Distance(pos, getNearestGeyserPosition(pos)) <= 50) {
            ew = "";
            ns = "";
            biome += " Geyser";
        }

        var pre = !string.IsNullOrEmpty(ew) || !string.IsNullOrEmpty(ns);
        var loc = ns + ew + (pre ? " " : "") + biome;
        if (includeDepth)
            loc += " (" + depth + "m)";
        return loc;
    }

    public static Vector3 getNearestGeyserPosition(Vector3 pos) {
        var nearest = new Vector3(0, 8000, 0);
        foreach (var at in geysers) {
            if ((at - pos).sqrMagnitude < (nearest - pos).sqrMagnitude)
                nearest = at;
        }

        return nearest;
    }

    /*
    public static bool isScannerRoomInRange(Vector3 position, bool needFunctional = true, float maxRange = 500, TechType scanningFor = TechType.None) {
        foreach (MapRoomFunctionality room in getObjectsNearWithComponent<MapRoomFunctionality>(position, maxRange)) {
            bool working = !needFunctional || room.CheckIsPowered();
            bool finding = scanningFor == TechType.None || room.typeToScan == scanningFor;
            if (working && finding && Vector3.Distance(room.transform.position, position) <= room.GetScanRange())
                return true;
        }
        return false;
    }
    */
    public static void setParticlesTemporary(ParticleSystem p, float dur, float killOffset = 5) {
        p.Play(true);
        p.gameObject.EnsureComponent<TransientParticleTag>().Invoke(nameof(TransientParticleTag.stop), dur);
        p.gameObject.destroy(false, dur + killOffset);
        var pi = p.gameObject.FindAncestor<PrefabIdentifier>();
        if (pi)
            pi.destroy();
        var lw = p.gameObject.FindAncestor<LargeWorldEntity>();
        if (lw)
            lw.destroy();
    }

    public static ParticleSystem spawnParticlesAt(
        Vector3 pos,
        string pfb,
        float dur,
        bool forceSpawn = false,
        float killOffset = 5
    ) {
        if (!forceSpawn && Vector3.Distance(pos, Player.main.transform.position) >= 200)
            return null;
        var particle = ObjectUtil.createWorldObject(pfb);
        particle.SetActive(true);
        particle.transform.position = pos;
        var p = particle.GetComponent<ParticleSystem>();
        setParticlesTemporary(p, dur, killOffset);
        return p;
    }

    public static GameObject reparentAllNearTo(string name, Vector3 pos, float r, Predicate<GameObject> check) {
        var ctr = new GameObject(name) {
            transform = {
                position = pos,
            },
        };
        getObjectsNear(
            pos,
            r,
            go => ObjectUtil.reparentTo(ctr, go),
            go => go.isRootObject() && check.Invoke(go) ? go : null
        );
        return ctr;
    }

    public static void reparentAllNearTo(GameObject ctr, Vector3 pos, float r, Predicate<GameObject> check) {
        ctr.transform.position = pos;
        getObjectsNear(
            pos,
            r,
            go => ObjectUtil.reparentTo(ctr, go),
            go => go.isRootObject() && check.Invoke(go) ? go : null
        );
    }

    public static GameObject getBatch(int x, int y, int z) {
        var root = LargeWorld.main.transform.parent;
        return root.gameObject.getChildObject("Batches/Batch " + x + "," + y + "," + z + " objects");
    }

    public static StasisSphere createStasisSphere(Vector3 pos, float r, float pwr = 1) {
        var sph = ObjectUtil.lookupPrefab(TechType.StasisRifle).GetResult().GetComponent<StasisRifle>()
            .effectSpherePrefab;
        sph = sph.clone();
        sph.SetActive(true);
        sph.fullyEnable();
        sph.transform.position = pos;
        var ss = sph.GetComponent<StasisSphere>();
        ss.fieldEnergy = pwr;
        ss.time = Mathf.Lerp(ss.minTime, ss.maxTime, ss.fieldEnergy);
        ss.radius = r;
        ss.EnableField();
        ss.soundActivate.Stop(true);
        SNUtil.log(
            "Created stasis sphere of radius " + ss.radius + ", duration " + ss.time + ", energy " + ss.fieldEnergy
        );
        return ss;
    }

    public static bool isInPCFTank(GameObject go) {
        return prisonAquariumExpanded.Contains(go.transform.position);
    }

    public static bool isInRocket() {
        return Player.main.precursorOutOfWater && Player.main.transform.position.y > 30;
    }

    private class TransientParticleTag : MonoBehaviour {
        public void stop() {
            GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void OnDestroy() {
            if (gameObject)
                gameObject.destroy(false);
        }

        private void OnDisable() {
            if (gameObject)
                gameObject.destroy(false);
        }
    }
}
/*
enum GameLightType {
    FLASHLIGHT,
    SEAGLIDE,
    SEAMOTH,
    EXOSUIT,
    CYCLOPS,
    FLARE,
    SKY,
    OTHER,
}*/