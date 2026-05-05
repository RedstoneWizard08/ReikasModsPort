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
using System.IO;
using System.Linq;
using System.Xml;
using Nautilus.Handlers;
using Nautilus.Utility;
using TMPro;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class BuildingHandler {
    public static readonly BuildingHandler instance = new();

    public bool isEnabled { get; private set; }

    private PlacedObject lastPlaced;
    private Dictionary<int, PlacedObject> items = new();

    private readonly List<ManipulationBase> globalTransforms = [];

    private List<string> text = [];
    private BasicText controlHint = new(TextAlignmentOptions.Center);

    private static readonly List<PositionedPrefab> objectCache = [];

    private BuildingHandler() {
        addText("LMB to select, Lalt+RMB to place selected on ground at look, LCtrl+RMB to duplicate them there");
        addText("Lalt+Arrow keys to move L/R Fwd/Back; +/- for U/D; add Z to make relative to obj");
        addText("Lalt+QR to yaw; [] to pitch (Z); ,. to roll (X); add Z to make relative to obj");
        addText("Add C for fast, X for slow; DEL to delete all selected");
    }

    private void addText(string s) {
        text.Add(s);
        controlHint.SetLocation(0, 300);
        controlHint.SetSize(16);
    }

    public void addCommand(string key, Action call) {
        ConsoleCommandsHandler.RegisterConsoleCommand(key, call);
        addText("/" + key + ": " + call.Method.Name);
    }

    public void addCommand<T>(string key, Action<T> call) {
        ConsoleCommandsHandler.RegisterConsoleCommand(key, call);
        addText("/" + key + ": " + call.Method.Name);
    }

    public void addCommand<T, R>(string key, Func<T, R> call) {
        ConsoleCommandsHandler.RegisterConsoleCommand(key, call);
        addText("/" + key + ": " + call.Method.Name);
    }

    public void addCommand<T1, T2>(string key, Action<T1, T2> call) {
        ConsoleCommandsHandler.RegisterConsoleCommand(key, call);
        addText("/" + key + ": " + call.Method.Name);
    }

    public void addCommand<T1, T2, T3>(string key, Action<T1, T2, T3> call) {
        ConsoleCommandsHandler.RegisterConsoleCommand(key, call);
        addText("/" + key + ": " + call.Method.Name);
    }

    public void setEnabled(bool on) {
        isEnabled = on;
        foreach (var go in items.Values) {
            try {
                go.fx.SetActive(go.isSelected);
            } catch (Exception ex) {
                SNUtil.writeToChat("Could not set enabled state of " + go + " due to GO error: " + ex.ToString());
            }
        }

        if (on) {
            controlHint.ShowMessage(string.Join("\n", text.ToArray()));
        } else {
            controlHint.Hide();
        }
    }

    public void selectedInfo() {
        foreach (var go in items.Values) {
            if (go.isSelected) {
                SNUtil.writeToChat(go.ToString());
            }
        }
    }

    public void activateObject() {
        foreach (var go in items.Values) {
            if (go.isSelected) {
                go.obj.fullyEnable();
            }
        }
    }

    public void setScale(float sc) {
        var sc2 = Vector3.one * sc;
        foreach (var go in items.Values) {
            if (go.isSelected) {
                go.obj.transform.localScale = sc2;
                go.scale = sc2;
            }
        }
    }

    public void setScaleXYZ(float x, float y, float z) {
        var sc2 = new Vector3(x, y, z);
        foreach (var go in items.Values) {
            if (go.isSelected) {
                go.obj.transform.localScale = sc2;
                go.scale = sc2;
            }
        }
    }

    public void dumpTextures() {
        foreach (var go in items.Values) {
            if (go.isSelected) {
                foreach (var r in go.obj.GetComponentsInChildren<Renderer>())
                    RenderUtil.dumpTextures(SNUtil.diDLL, r);
            }
        }
    }

    internal static int genID(GameObject go) { /*
        if (go.transform != null && go.transform.gameObject != null)
            return go.transform.gameObject.GetInstanceID();
        else*/
        return go.GetInstanceID();
    }

    public void handleRClick(bool isCtrl = false) {
        var transform = MainCamera.camera.transform;
        var position = transform.position;
        var forward = transform.forward;
        var ray = new Ray(position, forward);
        if (UWE.Utils.RaycastIntoSharedBuffer(ray, 30, -5, QueryTriggerInteraction.Ignore) > 0) {
            var hit = UWE.Utils.sharedHitBuffer[0];
            if (hit.transform != null) {
                if (isCtrl) {
                    List<PlacedObject> added = [];
                    foreach (var p in new List<PlacedObject>(items.Values)) {
                        if (p.isSelected) {
                            var b = PlacedObject.createNewObject(p);
                            items[b.referenceID] = b;
                            b.setRotation(MathUtil.unitVecToRotation(hit.normal));
                            b.setPosition(hit.point);
                            lastPlaced = b;
                            added.Add(b);
                        }
                    }

                    clearSelection();
                    foreach (var b in added)
                        select(b);
                } else if (Input.GetKeyDown(KeyCode.LeftAlt)) {
                    foreach (var go in items.Values) {
                        if (go.isSelected) {
                            go.setRotation(MathUtil.unitVecToRotation(hit.normal));
                            go.setPosition(hit.point);
                        }
                    }
                }
            }
        }
    }

    public void handleClick(bool isCtrl = false) {
        Targeting.GetTarget(Player.main.gameObject, 40, out var found, out var dist);
        Targeting.Reset();
        if (found == null) {
            SNUtil.writeToChat("Raytrace found nothing.");
        }

        var has = getPlacement(found);
        //SNUtil.writeToChat("Has is "+has);
        if (has == null) {
            if (!isCtrl) {
                clearSelection();
            }
        } else if (isCtrl) {
            if (has.isSelected)
                deselect(has);
            else
                select(has);
        } else {
            clearSelection();
            select(has);
        }
    }

    public void selectAll() {
        foreach (var go in items.Values) {
            select(go);
        }
    }

    public void selectOfID(string id) {
        foreach (var go in items.Values) {
            if (go.prefabName == id)
                select(go);
        }
    }

    public void clearCache() {
        objectCache.Clear();
    }

    public void cache() {
        var n = 0;
        foreach (var obj in new List<PlacedObject>(items.Values)) {
            var pfb = new PositionedPrefab(obj.prefabName, obj.position, obj.rotation, obj.scale);
            if (objectCache.Contains(pfb))
                continue;
            objectCache.Add(pfb);
            delete(obj);
            n++;
        }

        SNUtil.writeToChat("Cached " + n + " objects");
    }

    public void loadCache() {
        foreach (var pfb in objectCache) {
            var b = PlacedObject.createNewObject(pfb.prefabName);
            if (b != null) {
                b.obj.transform.SetPositionAndRotation(pfb.position, pfb.rotation);
                registerObject(b);
            }
        }

        objectCache.Clear();
    }

    public void saveCache(string id) {
        var file = dumpPrefabs(id, objectCache);
        SNUtil.writeToChat("Exported " + objectCache.Count + " cached prefabs to " + file);
        //objectCache.Clear();
    }

    public void saveSelection(string file) {
        dumpSome(file, s => s.isSelected);
    }

    public void saveAll(string file) {
        dumpSome(file, s => true);
    }

    private string dumpSome(string file, Func<PlacedObject, bool> flag) {
        var path = getDumpFile(file);
        var doc = new XmlDocument();
        var rootnode = doc.CreateElement("Root");
        var added = 0;
        doc.AppendChild(rootnode);
        SNUtil.log("=================================");
        SNUtil.log("Building Handler has " + items.Count + " items: ");
        foreach (var go in items.Values) {
            try {
                var use = flag(go);
                SNUtil.log(go.ToString() + " dump = " + use);
                if (use) {
                    var e = doc.CreateElement(go.getTagName());
                    go.saveToXML(e);
                    doc.DocumentElement.AppendChild(e);
                    added++;
                }
            } catch (Exception e) {
                throw new Exception(go.ToString(), e);
            }
        }

        SNUtil.log("=================================");
        doc.Save(path);
        SNUtil.writeToChat("Saved " + added + " objects to " + path);
        return path;
    }

    public string dumpObjectChildren(string file, GameObject go) {
        return dumpObjects(file, ObjectUtil.getChildObjects(go));
    }

    public string dumpObjects(string file, IEnumerable<GameObject> li) {
        return dumpPIs(file, li.Select(go => go.GetComponent<PrefabIdentifier>()));
    }

    public string dumpNear(string file, float r) {
        return dumpPIs(
            file,
            WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(Player.main.transform.position, r)
        );
    }

    public string dumpPIs(string file, IEnumerable<PrefabIdentifier> li) {
        return dumpPrefabs(file, li.Where(pi => (bool)pi).Select(pi => new PositionedPrefab(pi)));
    }

    public string dumpPrefabs(string file, IEnumerable<PositionedPrefab> li) {
        var path = getDumpFile(file);
        var doc = new XmlDocument();
        var rootnode = doc.CreateElement("Root");
        doc.AppendChild(rootnode);
        SNUtil.log("=================================");
        SNUtil.log("Exporting " + li.Count() + " PositionedPrefabs: ");
        foreach (var pfb in li) {
            try {
                var e = doc.CreateElement(pfb.getTagName());
                pfb.saveToXML(e);
                doc.DocumentElement.AppendChild(e);
            } catch (Exception e) {
                throw new Exception(pfb.ToString(), e);
            }
        }

        SNUtil.log("=================================");
        doc.Save(path);
        return path;
    }

    public void loadFile(string file) {
        var doc = new XmlDocument();
        doc.Load(getDumpFile(file));
        var rootnode = doc.DocumentElement;
        globalTransforms.Clear();
        DICustomPrefab.loadManipulations(rootnode.getAllChildrenIn("transforms"), globalTransforms);
        foreach (XmlElement e in rootnode.ChildNodes) {
            if (e.Name == "transforms")
                continue;
            try {
                buildElement(e);
            } catch (Exception ex) {
                SNUtil.log(ex.ToString());
                SNUtil.writeToChat(
                    "Could not load XML block, threw exception: " + ex.ToString() + " from " + e.format()
                );
            }
        }
    }

    private void buildElement(XmlElement e) {
        var count = e.GetAttribute("count");
        var amt = string.IsNullOrEmpty(count) ? 1 : int.Parse(count);
        for (var i = 0; i < amt; i++) {
            var ot = ObjectTemplate.construct(e);
            if (ot == null) {
                throw new Exception("Could not load XML block, null result from '" + e.Name + "': " + e.format());
            }

            switch (e.Name) {
                case "object":
                    var b = (PlacedObject)ot;
                    addObject(b);
                    foreach (var mb in globalTransforms) {
                        SNUtil.log("Applying global " + mb + " to " + b);
                        mb.applyToObject(b);
                        SNUtil.log("Is now " + b.ToString());
                    }

                    break;
                case "customprefab":
                case "basicprefab":
                    var p = (PositionedPrefab)ot;
                    var go0 = p.createWorldObject();
                    var p2 = new PlacedObject(go0, go0.getPrefabID());
                    addObject(p2);
                    var sel0 = go0.AddComponent<BuilderPlaced>();
                    sel0.placement = p2;
                    foreach (var mb in globalTransforms) {
                        SNUtil.log("Applying global " + mb + " to " + p2);
                        mb.applyToObject(p2);
                        SNUtil.log("Is now " + p2.ToString());
                    }

                    break;
                case "generator":
                    var gen = (WorldGenerator)ot;
                    List<GameObject> li = [];
                    gen.generate(li);
                    SNUtil.writeToChat("Ran generator " + gen + " which produced " + li.Count + " objects");
                    foreach (var go in li) {
                        if (go == null) {
                            SNUtil.writeToChat("Generator " + gen + " produced a null object!");
                            continue;
                        }

                        var b2 = new PlacedObject(go, go.getPrefabID());
                        addObject(b2);
                        var sel = go.AddComponent<BuilderPlaced>();
                        sel.placement = b2;
                    }

                    break;
            }
        }
    }

    private void addObject(PlacedObject b) {
        SNUtil.log("Loaded a " + b);
        items[b.referenceID] = b;
        lastPlaced = b;
        selectLastPlaced();
    }

    public string getDumpFile(string name) {
        var folder = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "ObjectDump"
        );
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, name + ".xml");
    }

    public void deleteSelected() {
        var li = new List<PlacedObject>(items.Values);
        foreach (var go in li) {
            if (go.isSelected) {
                delete(go);
            }
        }
    }

    private void delete(PlacedObject go) {
        GameObject.Destroy(go.obj);
        GameObject.Destroy(go.fx);
        items.Remove(go.referenceID);
    }

    /*
    private PlacedObject getSelectionFor(GameObject go) {
        PlacedObject s = getPlacement(go);
        if (s != null && selected.TryGetValue(s.referenceID, out s))
            return s;
        else
            return null;
    }*/

    public bool isSelected(GameObject go) {
        var s = getPlacement(go);
        return s != null && s.isSelected;
    }

    public void selectLastPlaced() {
        if (lastPlaced != null) {
            select(lastPlaced);
        }
    }

    public void syncObjects() {
        foreach (var p in items.Values) {
            if (p.isSelected)
                p.sync();
        }
    }

    private PlacedObject getPlacement(GameObject go) {
        if (go == null)
            return null;
        var pre = go.GetComponentInParent<BuilderPlaced>();
        if (pre != null) {
            return pre.placement;
        } else {
            SNUtil.writeToChat(
                "Game object " + go + " (" + genID(go) + ") was not was not placed by the builder system."
            );
            return null;
        }
    }

    public void select(GameObject go) {
        var pre = getPlacement(go);
        if (go != null && pre == null)
            ObjectUtil.dumpObjectData(go);
        if (pre != null) {
            select(pre);
        }
    }

    private void select(PlacedObject s) {
        //SNUtil.writeToChat("Selected "+s);
        s.setSelected(true);
    }

    public void deselect(GameObject go) {
        var pre = getPlacement(go);
        if (pre != null) {
            deselect(pre);
        }
    }

    private void deselect(PlacedObject go) {
        //SNUtil.writeToChat("Deselected "+go);
        go.setSelected(false);
    }

    public void clearSelection() {
        foreach (var go in items.Values) {
            deselect(go);
        }
    }

    public void manipulateSelected() {
        var s = Input.GetKeyDown(KeyCode.C) ? 0.15F : Input.GetKeyDown(KeyCode.X) ? 0.02F : 0.05F;
        foreach (var go in items.Values) {
            if (!go.isSelected)
                continue;
            var t = MainCamera.camera.transform;
            var rel = Input.GetKeyDown(KeyCode.Z);
            if (rel) {
                t = go.obj.transform;
            }

            var vec = t.forward.normalized;
            var right = t.right.normalized;
            var up = t.up.normalized;
            if (Input.GetKeyDown(KeyCode.UpArrow))
                go.move(vec * s);
            if (Input.GetKeyDown(KeyCode.DownArrow))
                go.move(vec * -s);
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                go.move(right * -s);
            if (Input.GetKeyDown(KeyCode.RightArrow))
                go.move(right * s);
            if (Input.GetKeyDown(KeyCode.Equals)) //+
                go.move(up * s);
            if (Input.GetKeyDown(KeyCode.Minus))
                go.move(up * -s);
            if (Input.GetKeyDown(KeyCode.R))
                go.rotateYaw(s * 20, rel ? null : getCenter());
            if (Input.GetKeyDown(KeyCode.Q))
                go.rotateYaw(-s * 20, rel ? null : getCenter());
            if (Input.GetKeyDown(KeyCode.LeftBracket))
                go.rotate(0, 0, -s * 20, rel ? null : getCenter());
            if (Input.GetKeyDown(KeyCode.RightBracket))
                go.rotate(0, 0, s * 20, rel ? null : getCenter());
            if (Input.GetKeyDown(KeyCode.Comma))
                go.rotate(-s * 20, 0, 0, rel ? null : getCenter());
            if (Input.GetKeyDown(KeyCode.Period))
                go.rotate(s * 20, 0, 0, rel ? null : getCenter());
        }
    }

    private Vector3? getCenter() {
        if (items.Count == 0)
            return null;
        var vec = Vector3.zero;
        foreach (var obj in items.Values) {
            vec += obj.position;
        }

        vec /= items.Values.Count;
        return vec;
    }

    private int selectionCount() {
        var ret = 0;
        foreach (var p in items.Values) {
            if (p.isSelected)
                ret++;
        }

        return ret;
    }

    internal PlacedObject spawnPrefabAtLook(string arg) {
        if (!isEnabled)
            return null;
        var transform = MainCamera.camera.transform;
        var position = transform.position;
        var forward = transform.forward;
        var pos = position + forward.normalized * 7.5F;
        var id = getPrefabKeyFromID(arg);
        var b = PlacedObject.createNewObject(id);
        if (b != null) {
            b.obj.transform.SetPositionAndRotation(pos, Quaternion.identity);
            registerObject(b);
            SNUtil.writeToChat("Spawned a " + b);
            SNUtil.log("Spawned a " + b);
        }

        return b;
    }
    /*
        internal void createFromCachedGo(IEnumerable<GameObject> li) {
            if (!isEnabled)
                return;
            foreach (GameObject go in li) {
                Transform transform = MainCamera.camera.transform;
                Vector3 position = transform.position;
                Vector3 forward = transform.forward;
                Vector3 pos = position+(forward.normalized*7.5F);
                string id = getPrefabKeyFromID(arg);
                PlacedObject b = PlacedObject.createNewObject(id);
                if (b != null) {
                    b.obj.transform.SetPositionAndRotation(pos, Quaternion.identity);
                    registerObject(b);
                    SNUtil.writeToChat("Spawned a "+b);
                    SNUtil.log("Spawned a "+b);
                }
            }
        }*/

    public void addRealObjects(IEnumerable<GameObject> li, bool withChildren = false) {
        foreach (var go in li)
            addRealObject(go, withChildren);
    }

    public void addRealObject_External(GameObject go, bool withChildren = false) {
        addRealObject(go, withChildren);
    }

    internal PlacedObject addRealObject(GameObject go, bool withChildren = false) {
        if (go.GetComponent<Base>() != null) {
            var bo = PlacedObject.createNewObject(go);
            registerObject(bo);
            bo.setSeabase();
            return bo;
        }

        var b = PlacedObject.createNewObject(go);
        if (b != null) {
            registerObject(b);
            SNUtil.writeToChat("Registered a pre-existing " + b);
            SNUtil.log("Registered a pre-existing " + b, SNUtil.diDLL);
            if (withChildren) {
                foreach (Transform t in go.transform) {
                    if (t.gameObject != go && t.gameObject != null) {
                        var b2 = addRealObject(t.gameObject, true);
                        if (b2 != null)
                            b2.parent = b;
                    }
                }
            }
        }

        return b;
    }

    private void registerObject(PlacedObject b) {
        items[b.referenceID] = b;
        lastPlaced = b;
        selectLastPlaced();
    }
    /*
    public void spawnTechTypeAtLook(string tech) {
        spawnTechTypeAtLook(getTech(tech));
    }

    public void spawnTechTypeAtLook(TechType tech) {

    }

    private TechType getTech(string name) {

    }*/

    private string getPrefabKeyFromID(string id) {
        //if (id.Length >= 24 && id[8] == '-' && id[13] == '-' && id[18] == '-' && id[23] == '-')
        //    return id;
        if (id.StartsWith("res_", StringComparison.InvariantCultureIgnoreCase)) {
            try {
                return ((VanillaResources)typeof(VanillaResources).GetField(id.Substring(4).ToUpper()).GetValue(null))
                    .prefab;
            } catch (Exception ex) {
                SNUtil.log("Unable to fetch vanilla resource field '" + id + "': " + ex);
                return null;
            }
        }

        if (id.StartsWith("flora_", StringComparison.InvariantCultureIgnoreCase)) {
            try {
                return ((VanillaFlora)typeof(VanillaFlora).GetField(id.Substring(6).ToUpper()).GetValue(null))
                    .getRandomPrefab(false);
            } catch (Exception ex) {
                SNUtil.log("Unable to fetch vanilla flora field '" + id + "': " + ex);
                return null;
            }
        }

        if (id.StartsWith("deco_", StringComparison.InvariantCultureIgnoreCase)) {
            try {
                return ((DecoPlants)typeof(DecoPlants).GetField(id.Substring(5).ToUpper()).GetValue(null)).prefab;
            } catch (Exception ex) {
                SNUtil.log("Unable to fetch deco flora field '" + id + "': " + ex);
                return null;
            }
        }

        if (id.StartsWith("fauna_", StringComparison.InvariantCultureIgnoreCase)) {
            try {
                return ((VanillaCreatures)typeof(VanillaCreatures).GetField(id.Substring(6).ToUpper()).GetValue(null))
                    .prefab;
            } catch (Exception ex) {
                SNUtil.log("Unable to fetch vanilla creature field '" + id + "': " + ex);
                return null;
            }
        }

        if (id.StartsWith("fragment_", StringComparison.InvariantCultureIgnoreCase)) {
            try {
                var idx1 = id.IndexOf('_');
                var idx2 = id.IndexOf('_', idx1 + 1);
                var index = int.Parse(id.Substring(idx1 + 1, idx2 - idx1 - 1));
                var tt = SNUtil.getTechType(id.Substring(idx2 + 1));
                return tt == TechType.None
                    ? throw new Exception("No techtype found")
                    : GenUtil.getFragment(tt, index).ClassID;
            } catch (Exception ex) {
                SNUtil.log("Unable to fetch tech fragment '" + id + "': " + ex);
                return null;
            }
        }

        if (id.StartsWith("pda_", StringComparison.InvariantCultureIgnoreCase)) {
            try {
                var page = PDAManager.getPage(id.Substring(4));
                return page == null ? throw new Exception("No page found") : page.getPDAClassID();
            } catch (Exception ex) {
                SNUtil.log("Unable to fetch pda '" + id + "': " + ex);
                return null;
            }
        }

        //if (id.IndexOf('/') >= 0)
        //    return PrefabData.getPrefabID(id);
        //return PrefabData.getPrefabIDByShortName(id);
        return id;
    }
}