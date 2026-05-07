using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using Nautilus.Assets;
using Nautilus.Utility;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.Exscansion;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class AvoliteSpawner {
    public static readonly AvoliteSpawner Instance = new();

    public readonly int AvoliteCount = /* FCSIntegrationSystem.instance.isLoaded() ? 13 : */ 9; //6;
    private readonly int _scrapCount = 45; //60;//UnityEngine.Random.Range(45, 71); //45-70

    private readonly string _oldSaveDir = Path.Combine(
        Path.GetDirectoryName(SeaToSeaMod.ModDLL.Location),
        "avolite_spawns"
    );

    private static readonly string SaveFileName = "AvoSpawns.dat";

    private string _avo;

    private readonly Vector3 _eventCenter = new(215, 425.6F, 2623.6F);
    private readonly Vector3 _eventUITargetLocation = new(297.2F, 3.5F, 1101);
    private readonly Vector3 _biomeCenter = new(800, 0, 1300); //new Vector3(966, 0, 1336);

    private readonly Dictionary<string, int> _itemChoices = new();
    private readonly CustomPrefab _spawnerObject;

    private readonly Dictionary<Vector3, PositionedPrefab> _objects = new();
    private readonly Dictionary<string, int> _objectCounts = new();
    private readonly Dictionary<string, int> _objectCountsToGo = new();

    private AvoliteSpawner() {
        _spawnerObject = new SunbeamDebrisObject();
    }

    private void AddItem(TechType item, int amt) {
        var id = CraftData.GetClassIdForTechType(item);
        if (string.IsNullOrEmpty(id))
            throw new Exception("Could not find spawnable item for techtype " + item);
        AddItem(id, amt);
    }

    private void AddItem(string id, int amt) {
        _itemChoices[id] = amt;
    }

    public void Register() {
        //addItem(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, AVOLITE_COUNT);

        AddItem(TechType.NutrientBlock, 10);
        AddItem(TechType.DisinfectedWater, 8);
        AddItem(TechType.Battery, 4);
        //addItem(TechType.Beacon, 3);

        AddItem(TechType.PowerCell, 1);
        AddItem(TechType.FireExtinguisher, 2);

        AddItem(TechType.EnameledGlass, 5);
        AddItem(TechType.Titanium, 25);
        AddItem(TechType.ComputerChip, 3);
        AddItem(TechType.WiringKit, 6);
        AddItem(TechType.CopperWire, 20);

        AddItem(TechType.Cap1, 2);
        AddItem(TechType.Cap2, 3);
        AddItem("dfabc84e-c4c5-45d9-8b01-ca0eaeeb8e65", 3);
        AddItem(TechType.ArcadeGorgetoy, 2);
        AddItem(TechType.PurpleVegetable, 3);

        AddItem(CraftingItems.getItem(CraftingItems.Items.LathingDrone).ClassID, 1);
        AddItem(CraftingItems.getItem(CraftingItems.Items.Motor).ClassID, 2);

        //addItem(TechType.ScrapMetal, SCRAP_COUNT);

        _spawnerObject.Register();

        GenUtil.registerPrefabWorldgen(_spawnerObject, false, BiomeType.Mountains_Grass, 1, 0.5F);
        GenUtil.registerPrefabWorldgen(_spawnerObject, false, BiomeType.Mountains_Sand, 1, 0.3F);

        SaveUtils.RegisterOnSaveEvent(Save);
        SaveUtils.RegisterOnStartLoadingEvent(LoadSave);
        SNUtil.MigrateSaveDataFolder(_oldSaveDir, ".xml", SaveFileName);

        _avo = CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).ClassID;
    }

    public void PostRegister() {
        ESHooks.ScannabilityEvent += IsItemMapRoomDetectable;
    }

    private void LoadSave() {
        var path = Path.Combine(SNUtil.GetCurrentSaveDir(), SaveFileName);
        if (File.Exists(path)) {
            var doc = new XmlDocument();
            doc.Load(path);
            _objects.Clear();
            _objectCounts.Clear();
            _objectCountsToGo.Clear();
            foreach (var kvp in _itemChoices)
                _objectCountsToGo[kvp.Key] = kvp.Value;
            foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
                var pfb = new PositionedPrefab("");
                pfb.loadFromXML(e);
                AddObject(pfb);
            }
        }

        SNUtil.Log("Loaded sunbeam debris cache: ");
        SNUtil.Log(_objects.ToDebugString());
        SNUtil.Log("Remaining:");
        SNUtil.Log(_objectCountsToGo.ToDebugString());
    }

    private void Save() {
        var path = Path.Combine(SNUtil.GetCurrentSaveDir(), SaveFileName);
        var doc = new XmlDocument();
        var rootnode = doc.CreateElement("Root");
        doc.AppendChild(rootnode);
        foreach (var go in _objects.Values) {
            var e = doc.CreateElement(go.getTagName());
            go.saveToXML(e);
            doc.DocumentElement.AppendChild(e);
        }

        doc.Save(path);
    }

    private void AddObject(PositionedPrefab pfb) {
        _objects[pfb.position] = pfb;
        _objectCounts[pfb.prefabName] = GetCount(pfb.prefabName) + 1;
        var max = pfb.prefabName == _avo ? AvoliteCount :
            _itemChoices.ContainsKey(pfb.prefabName) ? _itemChoices[pfb.prefabName] : _scrapCount;
        var rem = max - GetCount(pfb.prefabName);
        if (rem > 0)
            _objectCountsToGo[pfb.prefabName] = rem;
        else if (_objectCountsToGo.ContainsKey(pfb.prefabName))
            _objectCountsToGo.Remove(pfb.prefabName);
        //SNUtil.log(objectCountsToGo[pfb.prefabName]+" remaining of "+pfb.prefabName+" from "+max);
    }

    private GameObject AllocateItem(Transform t) {
        var pfb = TryFindItem(t.position);
        if (pfb == null)
            return null;
        var ret = new PositionedPrefab(pfb, t.position, t.rotation) {
            scale = t.localScale,
        };
        var go = ret.createWorldObject();
        if (pfb != "471852d4-03b6-4c47-9d4e-2ae893d63ff7" &&
            pfb != "86589e2f-bd06-447f-b23a-1f35e6368010") //wiring kit, glass
            go.transform.rotation = UnityEngine.Random.rotationUniform;
        var rb = go.GetComponentInChildren<Rigidbody>();
        if (rb)
            rb.isKinematic = true;
        go.transform.position += Vector3.up * 0.05F;
        AddObject(ret);
        return go;
    }

    private int GetCount(string pfb) {
        return _objectCounts.ContainsKey(pfb) ? _objectCounts[pfb] : 0;
    }

    private string TryFindItem(Vector3 pos) {
        //SNUtil.log("Avo count = "+getCount(avo));
        if (UnityEngine.Random.Range(0, 2) == 0 && GetCount(_avo) < AvoliteCount && GetClosest(_avo, pos) >= 120 &&
            WorldUtil.getObjectsNearWithComponent<AvoliteTag>(pos, 120).Count == 0) {
            return _avo;
        }

        if (_objectCountsToGo.Count == 0 || UnityEngine.Random.Range(0, 5) == 0) {
            return GetCount("947f2823-c42a-45ef-94e4-52a9f1d3459c") < _scrapCount ? GetRandomScrap().prefab : null;
        }

        var pfb = _objectCountsToGo.Keys.ToList()[UnityEngine.Random.Range(0, _objectCountsToGo.Count)];
        var amt = _objectCountsToGo[pfb];
        //SNUtil.log("Tried "+pfb+" > "+getCount(pfb)+"/"+objectCountsToGo[pfb]);
        if (amt > 1) {
            _objectCountsToGo[pfb] = amt - 1;
        } else {
            _objectCountsToGo.Remove(pfb);
            //SNUtil.log("Removing "+pfb+" from dict: "+objectCountsToGo.toDebugString<string, int>());
        }

        return pfb;
    }

    private double GetClosest(string pfb, Vector3 pos) {
        double dist = 999999;
        foreach (var pp in _objects.Values) {
            if (pp.prefabName == pfb) {
                double d = Vector3.Distance(pp.position, pos);
                if (d < dist)
                    dist = d;
            }
        }

        //SNUtil.log("Closest avo to "+pos+" was "+dist);
        return dist;
    }

    private VanillaResources GetRandomScrap() {
        switch (UnityEngine.Random.Range(0, 4)) {
            case 0:
            default:
                return VanillaResources.SCRAP1;
            case 1:
                return VanillaResources.SCRAP2;
            case 2:
                return VanillaResources.SCRAP3;
            case 3:
                return VanillaResources.SCRAP4;
        }
    }

    private bool IsValidPosition(Vector3 pos) {
        if (pos.y is >= -100 or <= -400)
            return false;
        var biome = WaterBiomeManager.main.GetBiome(pos, false);
        return VanillaBiomes.Mountains.IsInBiome(pos) && pos.x >= C2CHooks.GunCenter.x &&
               Vector3.Distance(pos.SetY(0), _biomeCenter.SetY(0)) <= 600 &&
               Vector3.Distance(pos.SetY(0), C2CHooks.GunCenter.SetY(0)) >= 200 &&
               Vector3.Distance(pos.SetY(0), C2CHooks.MountainCenter.SetY(0)) >= 360;
    }

    private void IsItemMapRoomDetectable(ESHooks.ResourceScanCheck rt) {
        if (rt.Resource.techType == _spawnerObject.TechType || rt.Resource.overrideTechType == _spawnerObject.TechType)
            rt.IsDetectable = PDAManager.getPage("sunbeamdebrishint").isUnlocked();
    }

    internal void CleanPickedUp(Pickupable pp) {
        var s = pp.GetComponentInChildren<SunbeamDebris>();
        if (s) {
            Story.StoryGoal.Execute("SunbeamDebris", Story.GoalType.Story);
            s.Destroy();
        }
    }

    internal void TickMapRoom(MapRoomFunctionality map) {
        if (C2CHooks.SkipScannerTick)
            return;
        if (VanillaBiomes.Mountains.IsInBiome(map.transform.position)) { /*
            float r = map.GetScanRange();
            //HashSet<SunbeamDebris> arr = WorldUtil.getObjectsNearWithComponent<SunbeamDebris>(map.transform.position, r); cannot use because no collider
            IEnumerable<SunbeamDebris> arr = UnityEngine.Object.FindObjectsOfType<SunbeamDebris>();
            //SNUtil.writeToChat("Scanner room @ "+map.transform.position+" found "+arr.Count()+" debris in range "+r);
            foreach (SunbeamDebris s in arr) {
                //SNUtil.log("Trying to convert sunbeam debris at "+s.transform.position);
                s.tryConvert();
            }*/

            if (map.scanActive && ResourceTrackerDatabase.resources.ContainsKey(_spawnerObject.TechType) &&
                map.typeToScan == _spawnerObject.TechType && map.resourceNodes.Count > 0) {
                //SNUtil.writeToChat("Scanner room is scanning and has "+map.resourceNodes.Count+" hits");
                //Dictionary<string, ResourceTracker.ResourceInfo> info = ResourceTracker.resources[spawnerObject.TechType];
                /*
                HashSet<SunbeamDebris> set = WorldUtil.getObjectsNearWithComponent<SunbeamDebris>(map.resourceNodes[UnityEngine.Random.Range(0, map.resourceNodes.Count)].position, 4);
                if (set.Count > 0)
                    set.First().tryConvert();*/
                WorldUtil.getGameObjectsNear(
                    map.transform.position,
                    map.GetScanRange(),
                    go => {
                        var s = go.GetComponent<SunbeamDebris>();
                        if (s) {
                            s.TryConvert();
                        }
                    }
                );
            }
        }
        //SNUtil.writeToChat("Scanner room @ "+map.transform.position+" is not in mountains, is in "+BiomeBase.getBiome(map.transform.position));
    }

    private class SunbeamDebrisObject : CustomPrefab {
        [SetsRequiredMembers]
        public SunbeamDebrisObject() : base(
            "SunbeamDebris",
            "Sunbeam Debris",
            "Dropped salvageable material from the Sunbeam."
        ) {
            SetGameObject(GetGameObject);
        }

        private GameObject GetGameObject() {
            var go = ObjectUtil.createWorldObject(VanillaResources.KYANITE.prefab, true, false);
            go.SetActive(false);
            go.removeComponent<Pickupable>();
            go.removeComponent<Collider>();
            go.removeChildObject("collider");
            go.removeChildObject("kyanite_small_03");
            //go.removeComponent<ResourceTrackerUpdater>();
            go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
            var trigger = go.EnsureComponent<SphereCollider>(); //this is so can be found with a SphereCast
            trigger.isTrigger = true;
            trigger.radius = 0.1F;
            trigger.center = Vector3.zero;
            go.EnsureComponent<SunbeamDebris>(); /*
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
                if (r) {
                    r.enabled = false;
                    r.gameObject.destroy(false);
                }
            }*/
            var rt = go.EnsureComponent<ResourceTracker>();
            rt.techType = Info.TechType;
            rt.overrideTechType = Info.TechType;
            return go;
        }

        protected Sprite GetItemSprite() {
            return TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/SunbeamDebrisIcon");
        }
    }

    private class SunbeamDebris : MonoBehaviour {
        //private float lastConversionCheck = -1;

        private void Update() {
            //SNUtil.writeToChat("SunbeamCheckPlayerRange > "+Story.StoryGoalManager.main.IsGoalComplete("SunbeamCheckPlayerRange"));
            //SNUtil.writeToChat("sunbeamdebrishint > "+Story.StoryGoalManager.main.IsGoalComplete("sunbeamdebrishint"));
            if (Instance.IsValidPosition(transform.position)) return;
            SNUtil.Log("Invalid sunbeam debris location, deleting @ " + transform.position);
            Destroy();
            /*
            else if (DayNightCycle.main.timePassedAsFloat-lastConversionCheck >= 1) {
                lastConversionCheck = DayNightCycle.main.timePassedAsFloat;
                if (PDAManager.getPage("sunbeamdebrishint").isUnlocked() && !transform.parent.GetComponent<Pickupable>()) {
                    GameObject pfb = instance.allocateItem(gameObject.transform);
                    if (pfb != null) {
                        SNUtil.log("Converted sunbeam debris placeholder @ "+transform.position+" to "+pfb);
                        transform.parent = pfb.transform;
                    }
                    else {
                        SNUtil.log("Item set exhausted, deleting @ "+transform.position);
                    }
                    enabled = false;
                    //gameObject.destroy(); do not destroy immediately, do that when the bound item is collected/destroyed
                }
            }*/
        }

        internal void TryConvert() {
            if (PDAManager.getPage("sunbeamdebrishint").isUnlocked() && !transform.parent.GetComponent<Pickupable>()) {
                var pfb = Instance.AllocateItem(gameObject.transform);
                if (pfb) {
                    SNUtil.Log("Converted sunbeam debris placeholder @ " + transform.position + " to " + pfb);
                    transform.parent = pfb.transform;
                } else {
                    SNUtil.Log("Item set exhausted, deleting @ " + transform.position);
                    Destroy();
                }

                enabled = false;
                //gameObject.destroy(); do not destroy immediately, do that when the bound item is collected/destroyed
            }
        }

        internal void Destroy() {
            GetComponent<ResourceTracker>().Unregister();
            gameObject.destroy();
        }
    }

    public class TriggerCallback : MonoBehaviour {
        private void Trigger() {
            PDAManager.getPage("sunbeamdebrishint").unlock();
        }
    }
}