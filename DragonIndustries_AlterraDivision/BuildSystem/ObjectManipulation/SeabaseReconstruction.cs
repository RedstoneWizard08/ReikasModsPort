/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Nautilus.Assets;
using Nautilus.Utility;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class SeabaseReconstruction {
    private static readonly Dictionary<string, SeabasePrefab> dataCache = new();

    static SeabaseReconstruction() {
        new SeabaseDoorOpener();
    }

    internal static SeabasePrefab getOrCreatePrefab(XmlElement e) {
        var id = e.GetProperty("identifier");
        if (!dataCache.ContainsKey(id)) {
            dataCache[id] = new SeabasePrefab(id, e);
            SNUtil.Log("Created worldgen seabase " + id);
        }

        return dataCache[id];
    }

    private class SeabaseIDHolder : MonoBehaviour {
    }

    public class WorldgenSeabaseController : MonoBehaviour {
        private static readonly string GEN_MARKER = "GenMarker";

        internal XmlElement reconstructionData;
        internal string seabaseID;

        private float lastBuildTime = -1;
        private float lastSkyTime = -1;
        private float lastModifyTime = -1;

        private Vector3 baseCenter = Vector3.zero;
        private int pieceCount;

        //private Planter[] planters = null;
        private Animator[] animations;
        private StorageContainer[] storages;
        private Charger[] chargers;

        public static event Action<WorldgenSeabaseController> onWorldgenSeabaseLoad;

        private void Awake() {
            onWorldgenSeabaseLoad?.Invoke(this);
        }

        private void rebuild(float time) {
            SNUtil.Log("Seabase '" + seabaseID + "' undergoing reconstruction", SNUtil.DiDLL);
            if (reconstructionData == null) {
                SNUtil.WriteToChat("Cannot rebuild worldgen seabase @ " + baseCenter + " - no data");
                return;
            }

            var li = reconstructionData.GetDirectElementsByTagName("part");
            SNUtil.Log("Reconstructing base from " + li.Count + " parts", SNUtil.DiDLL);
            var idx = 0;
            foreach (var e2 in li) {
                //SNUtil.log("Reconstructing part #"+idx+" from "+e2.InnerXml, SNUtil.diDLL);
                idx++;
                try {
                    var pfb = new DICustomPrefab("9d3e9fa5-a5ac-496e-89f4-70e13c0bedd5"); //BaseCell
                    pfb.loadFromXML(e2);
                    if (baseHasPart(gameObject, pfb) &&
                        pfb.prefabName != "9d3e9fa5-a5ac-496e-89f4-70e13c0bedd5") { //ie is loose
                        SNUtil.Log("Skipped recreate of loose piece: " + pfb, SNUtil.DiDLL);
                        continue;
                    }

                    SNUtil.Log("Reconstructed BaseCell/loose piece: " + pfb, SNUtil.DiDLL);
                    var go2 = pfb.createWorldObject();
                    go2.transform.parent = gameObject.transform;
                    go2.EnsureComponent<WorldgenSeabasePart>();
                    go2.removeComponent<CustomMachineLogic>();
                    go2.EnsureComponent<PreventDeconstruction>();
                    baseCenter += go2.transform.position;
                    pieceCount++;
                    var li1 = e2.GetDirectElementsByTagName("cellData");
                    if (li1.Count == 1) {
                        foreach (var e3 in li1[0].GetDirectElementsByTagName("component")) {
                            var pfb2 = new DICustomPrefab("basePart");
                            //Base.Piece type = Enum.Parse(typeof(Base.Piece), e3.getProperty("piece"));
                            pfb2.loadFromXML(e3);
                            if (pfb2.prefabName == PlacedObject.BUBBLE_PREFAB)
                                continue;
                            SNUtil.Log("Reconstructed base component: " + pfb2, SNUtil.DiDLL);
                            var go3 = pfb2.createWorldObject();
                            if (pfb2.prefabName == "RoomWaterParkBottom")
                                go3.removeChildObject("BaseWaterParkFloorBottom/Bubbles");
                            else if (pfb2.prefabName == "RoomWaterParkHatch")
                                go3.removeChildObject("BaseCorridorHatch(Clone)");
                            else if (pfb2.prefabName == "CorridorBulkhead")
                                go3.EnsureComponent<WorldgenBulkhead>();
                            go3.transform.parent = go2.transform;
                            rebuildNestedObjects(go3, e3);
                            if (!reconstructionData.GetBoolean("allowDeconstruct")) {
                                go3.removeComponent<BaseDeconstructable>();
                                go3.removeComponent<Constructable>();
                                var pv = go3.EnsureComponent<PreventDeconstruction>();
                                pv.inBase = true;
                                pv.inCyclops = true;
                                pv.inEscapePod = true;
                            }

                            go3.removeComponent<Fabricator>();
                            go3.removeComponent<MedicalCabinet>();
                            var li0 = e3.GetDirectElementsByTagName("supportData");
                            if (li0.Count == 1)
                                new SeabaseLegLengthPreservation(li0[0]).applyToObject(go3);
                            else if (li0.Count == 0)
                                new SeabaseLegLengthPreservation(null).applyToObject(go3);
                            li0 = e3.GetDirectElementsByTagName("modify");
                            if (li0.Count == 1) {
                                List<ManipulationBase> li2 = [];
                                DICustomPrefab.loadManipulations(li0[0], li2);
                                foreach (var mb in li2) {
                                    mb.applyToObject(go3);
                                }
                            }
                        }
                    }

                    li1 = e2.GetDirectElementsByTagName("inventory");
                    if (li1.Count == 1) {
                        //SNUtil.log("Recreating inventory contents: "+li1[0].OuterXml, SNUtil.diDLL);
                        var sc = go2.GetComponent<StorageContainer>();
                        var cg = go2.GetComponent<Charger>();
                        var p = go2.GetComponent<Planter>();
                        if (sc == null && cg == null) {
                            SNUtil.Log("Tried to deserialize inventory to a null container in " + go2);
                            continue;
                        }

                        GrowbedPropifier pg = null;
                        if (p != null) {
                            pg = go2.EnsureComponent<GrowbedPropifier>();
                        }

                        foreach (var e3 in li1[0].GetDirectElementsByTagName("item")) {
                            var tt = SNUtil.GetTechType(e3.GetProperty("type"));
                            if (tt == TechType.None) {
                                SNUtil.Log("Could not deserialize item - null TechType: " + e3.OuterXml, SNUtil.DiDLL);
                            } else {
                                var lootCube = false;
                                var igo = tt.getItem();
                                if (igo == null) {
                                    SNUtil.Log(
                                        "Item did not have prefab, using loot cube: " + e3.OuterXml,
                                        SNUtil.DiDLL
                                    );
                                    igo = ObjectUtil.lookupPrefab("01de572d-5549-44c6-97cf-645b07d1c79d");
                                    lootCube = true;
                                }

                                var amt = e3.GetInt("amount", 1);
                                var slot = e3.GetProperty("slot", true);
                                for (var i = 0; i < amt; i++) {
                                    var igo2 = igo.clone();
                                    igo2.SetActive(false);
                                    var pp = igo2.GetComponent<Pickupable>();
                                    pp.SetTechTypeOverride(tt, true);
                                    InventoryItem item = null;
                                    if (pp == null) {
                                        SNUtil.Log(
                                            "Could not deserialize item - no pickupable: " + e3.OuterXml,
                                            SNUtil.DiDLL
                                        );
                                    }

                                    if (cg != null) {
                                        cg.equipment.AddItem(slot, new InventoryItem(pp), true);
                                    } else if (sc != null) {
                                        item = sc.container.AddItem(pp);
                                    }
                                }

                                SNUtil.Log("Added " + tt + " x" + amt, SNUtil.DiDLL);
                            }
                        } /*
                        if (sc != null)
                            SNUtil.log("Recreated inventory contents: "+sc.container._items.toDebugString(), SNUtil.diDLL);
                        if (cg != null)
                            SNUtil.log("Recreated charger contents: "+cg.equipment.equipment.toDebugString(), SNUtil.diDLL);
                            */
                    }

                    var mdl = go2.getChildObject("MachineModel");
                    if (mdl)
                        mdl.SetActive(true);
                } catch (Exception ex) {
                    SNUtil.Log("Threw exception reconstructing part: " + ex.ToString(), SNUtil.DiDLL);
                }
            }

            //ObjectUtil.debugMode = true;
            gameObject.removeChildObject("SubDamageSounds");
            gameObject.removeChildObject("PowerAttach");
            gameObject.removeChildObject("MapRoomFunctionality");
            gameObject.removeChildObject("*x_TechLight_Cone");
            gameObject.removeComponent<Light>();
            gameObject.removeComponent<BaseFloodSim>();
            gameObject.removeComponent<BaseHullStrength>();
            gameObject.removeComponent<BasePowerRelay>();
            gameObject.removeComponent<PowerFX>();
            gameObject.removeComponent<VoiceNotificationManager>();
            gameObject.removeComponent<VoiceNotification>();
            gameObject.removeComponent<BaseRoot>();
            gameObject.removeComponent<Base>();
            gameObject.removeComponent<WaterPark>();
            gameObject.removeComponent<CustomMachineLogic>();
            gameObject.removeComponent<Constructable>(); //TODO find a way to not need this so you *can* dismantle parts
            //ObjectUtil.debugMode = false;

            baseCenter /= pieceCount;

            //gameObject.removeComponent<SkyApplier>();				
            /*
            SkyApplier sk = gameObject.EnsureComponent<SkyApplier>();
            sk.renderers = gameObject.GetComponentsInChildren<Renderer>();
            sk.environmentSky = MarmoSkies.main.skyBaseInterior;
            sk.applySky = sk.environmentSky;
            sk.enabled = true;
            sk.ApplySkybox();
            sk.RefreshDirtySky();*/

            foreach (var c in gameObject.GetComponentsInChildren<UseableDiveHatch>(true)) {
                if (c.gameObject.name.Contains("WaterPark"))
                    c.gameObject.EnsureComponent<WorldgenBaseWaterparkHatch>();
                else
                    c.destroy(); //component not object
            }

            foreach (var c in gameObject.GetComponentsInChildren<MapRoomCamera>(true)) {
                c.gameObject.destroy();
            }

            SNUtil.Log(
                "Finished reconstructing seabase '" + seabaseID + "' @ " + baseCenter + " @ " + time,
                SNUtil.DiDLL
            );
            //ObjectUtil.dumpObjectData(gameObject);
        }

        private void Update() {
            var time = DayNightCycle.main.timePassedAsFloat;
            if (seabaseID == null)
                seabaseID = gameObject.GetComponentInChildren<SeabaseIDHolder>().name;
            if (seabaseID == null) {
                SNUtil.WriteToChat("Could not find seabase ID in " + this + " @ " + transform.position);
                return;
            }

            if (reconstructionData == null) {
                reconstructionData = dataCache[seabaseID].data;
            }

            if (time - lastBuildTime >= 1) {
                foreach (Transform t in transform) {
                    if (t.gameObject.name.Contains("BaseCell") && t.childCount == 0) {
                        t.gameObject.destroy(false);
                    }
                }

                if (!gameObject.getChildObject("BaseCell")) { /*
                    GameObject marker = gameObject.getChildObject(GEN_MARKER);
                    bool isNew = !marker;
                    if (!marker) {
                        marker = new GameObject(GEN_MARKER);
                        marker.transform.parent = transform;
                    }*/
                    rebuild(time);
                }

                lastBuildTime = time;
            }

            if (time - lastSkyTime >= 15) {
                var skies = gameObject.GetComponentsInChildren<SkyApplier>(true);
                var skyAt = WaterBiomeManager.main.GetBiomeEnvironment(baseCenter);
                foreach (var sk in skies) {
                    if (!sk)
                        continue;
                    if (sk.environmentSky != MarmoSkies.main.skyBaseGlass &&
                        sk.environmentSky != MarmoSkies.main.skyBaseInterior && sk.environmentSky != skyAt) {
                        sk.environmentSky = skyAt;
                    } /*
                    bool glass = true; looks bad
                    foreach (Renderer r in sk.renderers) {
                        if (r && !r.name.ToLowerInvariant().Contains("glass")) {
                            glass = false;
                            break;
                        }
                    }
                    if (glass) {
                        sk.environmentSky = MarmoSkies.main.skyBaseGlass;
                    }*/

                    sk.applySky = sk.environmentSky;
                    sk.enabled = true;
                    sk.ApplySkybox();
                    sk.RefreshDirtySky();
                }

                //SNUtil.writeToChat("Set skies: "+skyAt+" @ "+baseCenter);
                lastSkyTime = time;
                gameObject.setActive<Animator>(false);
                //GetComponent<LightingController>().state = LightingController.LightingState.Damaged;
            } /*
            if (planters == null) {
                planters = gameObject.GetComponentsInChildren<Planter>();
            }*/

            if (storages == null) {
                storages = gameObject.GetComponentsInChildren<StorageContainer>();
                SNUtil.Log(
                    "Worldgen Seabase " + seabaseID + " finding storages: " + storages.ToDebugString(),
                    SNUtil.DiDLL
                );
            }

            if (chargers == null) {
                chargers = gameObject.GetComponentsInChildren<Charger>();
                SNUtil.Log(
                    "Worldgen Seabase " + seabaseID + " finding chargers: " + chargers.ToDebugString(),
                    SNUtil.DiDLL
                );
            }

            if (animations == null) {
                animations = gameObject.GetComponentsInChildren<Animator>();
                SNUtil.Log(
                    "Worldgen Seabase " + seabaseID + " finding animations: " + animations.ToDebugString(),
                    SNUtil.DiDLL
                );
            }

            if (time - lastModifyTime >= 5) {
                lastModifyTime = time;
                foreach (var a in animations)
                    if (a)
                        a.enabled = keepAnimator(a);

                foreach (var p in storages) {
                    if (p.container.IsEmpty() && p.storageRoot.transform.childCount > 0) {
                        SNUtil.Log(
                            "Worldgen Seabase " + seabaseID + " rebuilding storage for " + p + ": " +
                            p.storageRoot.GetComponentsInChildren<Pickupable>().ToDebugString(),
                            SNUtil.DiDLL
                        );
                        try {
                            foreach (var pp in p.GetComponentsInChildren<Pickupable>(true)) {
                                p.container.AddItem(pp);
                            }
                        } catch (Exception e) {
                            SNUtil.Log(
                                "Exception initializing worldgen seabase inventory @ " + p.transform.position + ": " +
                                e,
                                SNUtil.DiDLL
                            );
                        }
                    }
                }

                /*
                foreach (Planter p in planters) {
                    if (p.grownPlantsRoot.childCount == 0 && p.storageContainer.storageRoot.transform.childCount > 0) {
                        try {
                            //p.InitPlantsDelayed();
                            foreach (Pickupable pp in p.storageContainer.GetComponentsInChildren<Pickupable>(true)) {
                                p.AddItem(new InventoryItem(pp));
                            }
                        }
                        catch (Exception e) {
                            SNUtil.log("Exception initializing worldgen seabase planter @ "+p.transform.position+": "+e, SNUtil.diDLL);
                        }
                    }
                }*/
                foreach (var p in chargers) {
                    //SNUtil.writeToChat(p+" @ "+p.transform.position+" : "+p.equipment.equippedCount.Count+" : "+p.equipmentRoot.transform.childCount);
                    if (p.equipment.equippedCount.Count == 0 && p.equipmentRoot.transform.childCount > 0) {
                        SNUtil.Log(
                            "Worldgen Seabase " + seabaseID + " rebuilding storage for " + p + ": " +
                            p.equipmentRoot.GetComponentsInChildren<Pickupable>().ToDebugString(),
                            SNUtil.DiDLL
                        );
                        try {
                            var i = 0;
                            var pc = p.equipmentRoot.GetComponentsInChildren<Pickupable>(true);
                            //SNUtil.writeToChat("PC"+pc.Length+" > "+string.Join(",", p.slots.Keys));
                            foreach (var key in p.slots.Keys) {
                                p.equipment.AddItem(key, new InventoryItem(pc[i]), true);
                                i++;
                                if (i >= pc.Length)
                                    break;
                            }

                            p.opened = true;
                            p.animator.SetBool(p.animParamOpen, true);
                            p.ToggleUI(true);
                        } catch (Exception e) {
                            SNUtil.Log(
                                "Exception initializing worldgen seabase charger @ " + p.transform.position + ": " + e,
                                SNUtil.DiDLL
                            );
                        }
                    }
                }
            }
        }

        private bool keepAnimator(Animator a) {
            return a.gameObject.FindAncestor<SpikePlant>() || a.gameObject.FindAncestor<Aquarium>();
        }
    }
    /*
    class BaseHider : MonoBehaviour {

        void Update() {
            bool active = gameObject.activeSelf;
            gameObject.SetActive(Vector3.Distance(Player.main.transform.position, transform.position) <= 100);
            if (active != gameObject.activeSelf) {
                SNUtil.writeToChat("Toggling seabase @ "+baseCenter+": "+active+" > "+gameObject.activeSelf);
            }
        }

    }*/

    private static bool baseHasPart(GameObject main, DICustomPrefab pfb) {
        foreach (Transform t in main.transform) {
            var pi = t.GetComponent<PrefabIdentifier>();
            if (!pi || pi.ClassId != pfb.prefabName)
                continue;
            if (Vector3.Distance(pfb.position, t.position) >= 0.1)
                continue;
            return true;
        }

        return false;
    }

    private static void rebuildNestedObjects(GameObject main, XmlElement e) {
        foreach (var e2 in e.GetDirectElementsByTagName("child")) {
            var pfb = new DICustomPrefab(e2.GetProperty("prefab"));
            pfb.loadFromXML(e2);
            var go = pfb.createWorldObject();
            if (go != null) {
                go.transform.parent = main.transform;
                rebuildNestedObjects(go, e2);
            }
        }
    }

    public class WorldgenBaseWaterparkHatch : MonoBehaviour {
        private UseableDiveHatch hatch;
        private bool cleanedModel;

        private void Update() {
            if (!hatch) {
                hatch = gameObject.GetComponent<UseableDiveHatch>();
                gameObject.setActive<Animator>(true);
            }

            if (!cleanedModel)
                cleanedModel = gameObject.removeChildObject("BaseCorridorHatch(Clone)") > 0;
        }

        public bool isPlayerInside() {
            var acuCenter = transform.position;
            var outside = transform.position + transform.forward * 4.5F;
            var pos = Player.main.transform.position;
            return Vector3.Distance(pos, acuCenter) > Vector3.Distance(pos, outside);
        }
    }

    private class GrowbedPropifier : MonoBehaviour {
        private void Update() {
            var p = gameObject.GetComponent<Planter>();
            if (p != null) {
                foreach (var t in p.slots) {
                    if (t != null) {
                        var g = t.gameObject.GetComponentInChildren<GrowingPlant>(true);
                        if (g != null)
                            g.SetProgress(1);
                    }
                }

                foreach (var t in p.bigSlots) {
                    if (t != null) {
                        var g = t.gameObject.GetComponentInChildren<GrowingPlant>(true);
                        if (g != null)
                            g.SetProgress(1);
                    }
                }

                gameObject.GetComponent<StorageContainer>().enabled = false;
            }
        }
    }

    public static List<mset.Sky> getBiomeSkies() {
        return WaterBiomeManager.main.biomeSkies;
    }

    public static int getBiomeIndex(string s) {
        return WaterBiomeManager.main.GetBiomeIndex(s);
    }

    internal class WorldgenSeabasePart : MonoBehaviour {
        private float lastTickTime;

        private void Update() {
            var time = DayNightCycle.main.timePassedAsFloat;
            if (time - lastTickTime >= 1) {
                lastTickTime = time;
                gameObject.removeComponent<CustomMachineLogic>();
            }
        }
    }

    internal class SeabasePrefab : CustomPrefab {
        internal readonly XmlElement data;
        internal readonly string id;

        [SetsRequiredMembers]
        internal SeabasePrefab(string id, XmlElement e) : base("seabase##C2C##" + id, "Seabase: " + id, "") {
            data = e;
            this.id = id;

            SetGameObject(GetGameObject);
        }

        public GameObject GetGameObject() {
            SNUtil.Log("Reconstructing seabase with " + data.ChildNodes.Count + " parts", SNUtil.DiDLL);
            var go = new GameObject(Info.ClassID);
            go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
            go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
            go.EnsureComponent<TechTag>().type = Info.TechType;

            var ws = go.EnsureComponent<WorldgenSeabaseController>();
            ws.reconstructionData = data;
            ws.seabaseID = id;
            var holder = new GameObject(id);
            holder.EnsureComponent<SeabaseIDHolder>();
            holder.transform.parent = go.transform;
            //go.GetComponent<LightingController>().state = LightingController.LightingState.Damaged;
            //go.EnsureComponent<BaseHider>();
            var pos = data.GetVector("position").Value;
            go.transform.position = pos;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            SNUtil.Log("Finished deserializing seabase @ " + pos, SNUtil.DiDLL);
            return go;
        }
    }

    internal class WorldgenBulkhead : MonoBehaviour {
        internal BulkheadDoor door;

        private float lastOpenerCheckTime = -1;

        //private float openTime;

        private void Update() {
            if (!door)
                door = GetComponentInChildren<BulkheadDoor>();

            if (door && door.opened && DayNightCycle.main.timePassedAsFloat - lastOpenerCheckTime >= 0.5F &&
                !hasOpener()) { /*
                    if (openTime > 4)
                        GenUtil.OpenWorldgenSeabaseDoor.lockOpen(door);
                    else
                        openTime += Time.deltaTime;*/
                lastOpenerCheckTime = DayNightCycle.main.timePassedAsFloat;
                var go = ObjectUtil.createWorldObject("SeabaseDoorOpener");
                go.transform.position = transform.position;
            }
        }

        private bool hasOpener() {
            return WorldUtil.areAnyObjectsNear(transform.position, 2, go => go.GetComponent<SeabaseDoorOpenerTag>());
        }
    }

    private class SeabaseDoorOpener : CustomPrefab {
        [SetsRequiredMembers]
        internal SeabaseDoorOpener() : base("SeabaseDoorOpener", "", "") {
            SetGameObject(GetGameObject);
        }

        public GameObject GetGameObject() {
            var go = new GameObject();
            go.EnsureComponent<PrefabIdentifier>().classId = Info.ClassID;
            go.EnsureComponent<TechTag>().type = Info.TechType;
            go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
            go.EnsureComponent<SeabaseDoorOpenerTag>();
            var s = go.EnsureComponent<SphereCollider>();
            s.radius = 2;
            s.center = Vector3.zero;
            s.isTrigger = true;
            return go;
        }
    }

    private class SeabaseDoorOpenerTag : MonoBehaviour {
        private bool hasRun;

        private void Update() {
            if (!hasRun) {
                var bk = WorldUtil.getClosest<WorldgenBulkhead>(transform.position);
                if (bk && Vector3.Distance(bk.transform.position, transform.position) < 2) {
                    bk.door.SetState(true);
                    bk.door.NotifyStateChange();
                    hasRun = true;
                    bk.gameObject.removeComponent<Sealed>();
                }
            }
        }
    }
}