using System;
using System.Collections.Generic;
using System.Linq;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class ObjectUtil {
    public static bool debugMode;

    private static readonly HashSet<string> anchorPods = [
        VanillaFlora.ANCHOR_POD_SMALL1.getPrefabID(),
        VanillaFlora.ANCHOR_POD_SMALL2.getPrefabID(),
        VanillaFlora.ANCHOR_POD_MED1.getPrefabID(),
        VanillaFlora.ANCHOR_POD_MED2.getPrefabID(),
        VanillaFlora.ANCHOR_POD_LARGE.getPrefabID(),
    ];

    private static readonly HashSet<string> containmentDragonRepellents = [
        "c5512e00-9959-4f57-98ae-9a9962976eaa",
        "542aaa41-26df-4dba-b2bc-3fa3aa84b777",
        "5bcaefae-2236-4082-9a44-716b0598d6ed",
        "20ad299d-ca52-48ef-ac29-c5ec5479e070",
        "430b36ae-94f3-4289-91ac-25475ad3bf74",
    ];

    private static readonly HashSet<string> coralTubes = [
        "06562999-e575-4b02-b880-71d37616b5b9",
        "691723cf-d5e9-482f-b5af-8491b2a318b1",
        "f0295655-8f4f-4b18-b67d-925982a472d7",
    ];

    private static readonly HashSet<string> fossilPrefabs = [
        "29bcd3d7-48bf-4fd7-955a-23a9523aec47", //reaper skull
        "50031120-ab7a-4f10-b497-3a97f63b4de1", //reaper skull, LR
        "18042762-9460-44ca-a2d7-c4932d7d8193", //rib
        "33c31a89-9d3b-4717-ad26-4cc8106a1f24", //rib
        "4404f4f2-3d65-4338-adb3-a1a2e1f8fac5", //rib
        "6e37459e-d880-4da8-8dad-0cc10ff07f00", //rib
        "ee1807bf-6744-4fee-a66f-c71edc9e7fb6", //rib

        "b250309e-5ad0-43ca-9297-f79e22915db6", //garg, both (main is just 3x scale)
        "0552b196-d09a-45dd-b064-878966476179", //sea dragon
        "0b6ea118-1c0b-4039-afdb-2d9b26401ad2", //generic 01 (ghost canyon, crossing)
        "e10ff9a1-5f1e-4c4d-bf5f-170dba9e321b", //generic 02 (ghost canyon, crossing)
        "358012ab-6be8-412d-85ee-263a733c88ba", //generic 03 (ghost canyon, crossing)
        "8fe779a5-e907-4e9e-b748-1eee25589b34", //reaper
        "bfe993b9-8d6d-441c-922e-7dc074d81d3f", //reaper 2
        "71bf71c2-ecfb-47c0-aafe-040030d5954f", //drf specimen
        "0010bf17-15be-4350-955b-b4ac023815f3", //armored skull (ghost canyon)
    ];

    public static readonly string PRISON_VENT = "ec6fa336-2f55-468e-9bfe-626e655e146d";

    private static readonly HashSet<string> ventPrefabs = [
        "06856e8b-f612-495d-bc91-e9f629c0f689", //underislands
        "15378df5-5fce-4346-8811-267dd13d54fc", //sparse reef
        "15f55c15-2111-4ea8-bae0-20532029fe79", //dunes
        "57d96ba6-729c-4a33-ba3b-777b3c322ee8", //mountains
        "649ff503-126f-47b6-a446-6ac14f3bb533", //mushroom
        "e4897657-74bb-43fe-9b24-78ba26132055", //grand reef
        "a96ebe2c-3520-4181-9799-8d98649c3bbe", //generic vent, which is what all actually use

        PRISON_VENT,
    ];

    public static bool isAnchorPod(this GameObject go) {
        return isObjectInSet(go, anchorPods);
    }

    public static bool isCoralTube(this GameObject go) {
        return isObjectInSet(go, coralTubes);
    }

    public static bool isDragonRepellent(this GameObject go) {
        return isObjectInSet(go, containmentDragonRepellents);
    }

    public static bool isFossilPrefab(this GameObject go) {
        return isObjectInSet(go, fossilPrefabs);
    }

    public static bool isVent(this GameObject go) {
        return isObjectInSet(go, ventPrefabs);
    }

    public static IEnumerable<string> getFossils() {
        return fossilPrefabs.ToList().AsReadOnly();
    }

    public static IEnumerable<string> getVents() {
        return ventPrefabs.ToList().AsReadOnly();
    }

    public static bool isObjectInSet(GameObject go, HashSet<string> prefabs) {
        if (!go)
            return false;
        var pi = go.FindAncestor<PrefabIdentifier>();
        return pi && prefabs.Contains(pi.ClassId);
    }

    public static bool isFarmedPlant(this GameObject go) {
        return go.FindAncestor<Planter>();
    }

    public static GameObject createSeamothSparkSphere(SeaMoth sm, bool active = true) {
        var def = sm.seamothElectricalDefensePrefab.GetComponent<ElectricalDefense>();
        var sphere = def.fxElecSpheres[0];
        var go = Utils.SpawnZeroedAt(sphere, sm.transform, false);
        if (active)
            go.SetActive(true);
        return go;
    }

    public static ResourceTracker makeMapRoomScannable(this GameObject go, TechType tt, bool moving = false) {
        var res = go.EnsureComponent<ResourceTracker>();
        res.prefabIdentifier = go.GetComponent<PrefabIdentifier>();
        res.techType = tt;
        res.overrideTechType = tt;
        res.pickupable = go.GetComponentInChildren<Pickupable>();
        res.rb = go.GetComponentInChildren<Rigidbody>();
        if (moving)
            res.gameObject.EnsureComponent<ResourceTrackerUpdater>().tracker = res;
        return res;
    }

    public static bool isPDA(this GameObject go) {
        if (!go.GetComponent<StoryHandTarget>())
            return false;
        var pp = go.GetComponent<PrefabPlaceholdersGroup>();
        return pp && pp.prefabPlaceholders != null && pp.prefabPlaceholders.Length > 0 && pp.prefabPlaceholders[0] &&
               pp.prefabPlaceholders[0].prefabClassId == "4e8d9640-dd23-46ca-99f2-6924fcf250a4";
    }

    public static bool isBaseModule(TechType tt, bool includeFoundations) {
        switch (tt) {
            case TechType.BaseRoom:
            case TechType.BaseCorridorGlass:
            case TechType.BaseCorridor:
            case TechType.BaseCorridorI:
            case TechType.BaseCorridorL:
            case TechType.BaseCorridorT:
            case TechType.BaseCorridorX:
            case TechType.BaseCorridorGlassI:
            case TechType.BaseCorridorGlassL:
            case TechType.BaseMoonpool:
            case TechType.BaseObservatory:
            case TechType.BaseMapRoom:
                return true;
            case TechType.BaseFoundation:
                return includeFoundations;
            default:
                return false;
        }
    }

    public static void stripAllExcept(this GameObject go, params Type[] except) {
        var li = except.ToSet();
        foreach (var c in go.GetComponentsInChildren<Component>()) {
            if (c is Transform || li.Contains(c.GetType()))
                continue;
            c.destroy();
        }
    }

    public static void removeComponent(this GameObject go, Type tt, bool immediate = true) {
        foreach (var c in go.GetComponentsInChildren(tt)) {
            if (c is MonoBehaviour m)
                m.enabled = false;
            if (immediate)
                c.destroy();
            else
                c.destroy(false);
        }
    }

    public static void removeComponent<C>(this GameObject go, bool immediate = true) where C : Component {
        applyToComponents<C>(go, immediate ? 2 : 1, true, true);
    }

    public static void setActive<C>(this GameObject go, bool active) where C : Component {
        applyToComponents<C>(go, 0, true, active);
    }

    private static void applyToComponents<C>(this GameObject go, int destroy, bool setA, bool setTo)
        where C : Component {
        foreach (Component c in go.GetComponentsInChildren<C>(true)) {
            if (debugMode)
                SNUtil.log(
                    "Affecting component " + c + " in " + go + " @ " + go.transform.position + ": D=" + destroy + "/" +
                    setTo + "(" + setA + ")",
                    SNUtil.diDLL
                );
            if (c is MonoBehaviour m && setA)
                m.enabled = setTo;
            if (destroy == 2)
                c.destroy();
            else if (destroy == 1)
                c.destroy(false);
        }
    }

    public static void dumpObjectData(this GameObject go, bool includeChildren = true) {
        dumpObjectData(go, 0, includeChildren);
    }

    private static void dumpObjectData(this GameObject go, int indent, bool includeChildren = true) {
        if (!go) {
            SNUtil.log("null object");
            return;
        }

        SNUtil.log("object " + go, SNUtil.diDLL, indent);
        SNUtil.log("chain " + go.GetFullHierarchyPath(), SNUtil.diDLL, indent);
        SNUtil.log("components: " + string.Join(", ", (object[])go.GetComponents<Component>()), SNUtil.diDLL, indent);
        var p = go.GetComponent<Pickupable>();
        if (p) {
            SNUtil.log("pickup: " + p.GetTechType() + " = " + p.isPickupable, SNUtil.diDLL, indent);
        }

        var tag = go.GetComponent<TechTag>();
        if (tag) {
            SNUtil.log("techtag: " + tag.type, SNUtil.diDLL, indent);
        }

        var res = go.GetComponent<ResourceTracker>();
        if (res) {
            SNUtil.log("resource: " + res.name + " = " + res.techType, SNUtil.diDLL, indent);
        }

        var e = go.GetComponent<EntityTag>();
        if (e) {
            SNUtil.log("entity: " + e.name + " = " + e.tag, SNUtil.diDLL, indent);
        }

        var pp = go.GetComponent<Plantable>();
        if (pp) {
            SNUtil.log("plantable: " + pp.name + " = " + pp.plantTechType, SNUtil.diDLL, indent);
            SNUtil.log("plant: ", SNUtil.diDLL, indent);
            dumpObjectData(pp.growingPlant, indent + 1);
        }

        var live = go.GetComponent<LiveMixin>();
        if (live) {
            SNUtil.log(
                "live: " + live.name + " = " + live.health + "/" + live.maxHealth + " = " + live.IsAlive(),
                SNUtil.diDLL,
                indent
            );
        }

        var infect = go.GetComponent<InfectedMixin>();
        if (infect) {
            SNUtil.log("infected: " + infect.name + " = " + infect.infectedAmount, SNUtil.diDLL, indent);
        }

        SNUtil.log("transform: " + go.transform, SNUtil.diDLL, indent);
        if (go.transform != null) {
            SNUtil.log("position: " + go.transform.position, SNUtil.diDLL, indent);
            SNUtil.log("transform object: " + go.transform.gameObject, SNUtil.diDLL, indent);
            for (var i = 0; i < go.transform.childCount; i++) {
                var ch = go.transform.GetChild(i).gameObject;
                SNUtil.log("child object #" + i + ": " + (includeChildren ? "" : ch.name), SNUtil.diDLL, indent);
                if (includeChildren)
                    dumpObjectData(ch, indent + 3);
            }
        }
    }

    public static void dumpObjectData(this Component go) {
        dumpObjectData(go, 0);
    }

    private static void dumpObjectData(this Component go, int indent) {
        if (!go) {
            SNUtil.log("null component");
            return;
        }

        SNUtil.log("component " + go, SNUtil.diDLL, indent);
        dumpObjectData(go.gameObject);
    }

    public static void dumpObjectData(Mesh m) {
        SNUtil.log("Mesh " + m + ":");
        if (m == null) {
            SNUtil.log("Mesh is null");
            return;
        }

        SNUtil.log("Mesh has " + m.subMeshCount + " submeshes");
        SNUtil.log("Mesh has " + m.vertexCount + " vertices:");
        if (m.isReadable) {
            var verts = m.vertices;
            for (var i = 0; i < verts.Length; i++) {
                SNUtil.log("Vertex " + i + ": " + verts[i].ToString("F5"));
            }
        } else {
            SNUtil.log("[Not readable]");
        }
    }

    public static GameObject getItemGO(CustomPrefab item, string template) {
        return getItemGO(item.Info.TechType, item.Info.ClassID, template);
    }

    public static GameObject getItemGO(TechType tech, string template) {
        return getItemGO(tech, Enum.GetName(tech.GetType(), tech), template);
    }

    public static GameObject getItemGO(TechType tech, string id, string template) {
        var prefab = Resources.Load<GameObject>(template);
        if (prefab == null)
            throw new Exception("Null prefab result during item '" + template + "' lookup");
        var gameObject = UnityEngine.Object.Instantiate(prefab);
        if (gameObject == null)
            throw new Exception("Null item result during item '" + template + "' build");
        var component = gameObject.EnsureComponent<TechTag>();
        UniqueIdentifier component2 = gameObject.EnsureComponent<PrefabIdentifier>();
        if (component == null)
            throw new Exception("Null techtag result during item '" + template + "' repopulate");
        if (component2 == null)
            throw new Exception("Null UID result during item '" + template + "' repopulate");
        component.type = tech;
        component2.ClassId = id;
        return gameObject;
    }

    public static int removeChildObject(this GameObject go, string name, bool immediate = true) {
        var find = getChildObject(go, name);
        HashSet<int> removed = [];
        while (go && find) {
            find.SetActive(false);
            removed.Add(find.GetInstanceID());
            if (immediate)
                find.destroy();
            else
                find.destroy(false);
            find = getChildObject(go, name);
            if (find && removed.Contains(find.GetInstanceID()))
                find = null;
            if (removed.Count > 500) {
                SNUtil.log("REMOVING CHILD OBJECT STUCK IN INFINITE LOOP INSIDE " + find.GetFullHierarchyPath() + "!");
                return removed.Count;
            }
        }

        return removed.Count;
    }

    public static List<GameObject> getChildObjects(this GameObject go) {
        List<GameObject> ret = [];
        foreach (Transform t in go.transform) {
            ret.Add(t.gameObject);
        }

        return ret;
    }

    public static List<GameObject> getChildObjects(this GameObject go, string name, bool recursive = false) {
        var startWild = name[0] == '*';
        var endWild = name[name.Length - 1] == '*';
        var seek = name;
        if (startWild)
            seek = seek.Substring(1);
        if (endWild)
            seek = seek.Substring(0, seek.Length - 1);
        //SNUtil.writeToChat(seek+" > "+startWild+"&"+endWild);
        List<GameObject> ret = [];
        foreach (Transform t in go.transform) {
            var n = t.gameObject.name;
            n = n.Replace("(Placeholder)", "");
            n = n.Replace("(Clone)", "");
            var match = startWild && endWild ? n.Contains(seek) :
                startWild ? n.EndsWith(seek, StringComparison.InvariantCulture) :
                endWild ? n.StartsWith(seek, StringComparison.InvariantCulture) : n == seek;
            //SNUtil.writeToChat(seek+"&&"+n+" > "+match);
            if (match) {
                ret.Add(t.gameObject);
            }

            if (recursive && (startWild || endWild)) {
                ret.AddRange(getChildObjects(t.gameObject, name, true));
            }
        }

        return ret;
    }

    public static GameObject getChildObject(this GameObject go, string name) {
        if (!go)
            return null;
        if (name == "*")
            return go.transform.childCount > 0 ? go.transform.GetChild(0).gameObject : null;
        var startWild = name[0] == '*';
        var endWild = name[name.Length - 1] == '*';
        if (startWild || endWild) {
            if (debugMode)
                SNUtil.log(
                    "Looking for child wildcard match " + name + " > " + startWild + ", " + endWild,
                    SNUtil.diDLL
                );
            return findFirstChildMatching(go, name, startWild, endWild);
        } else {
            var t = go.transform.Find(name);
            if (t != null)
                return t.gameObject;
            t = go.transform.Find(name + "(Clone)");
            if (t != null)
                return t.gameObject;
            t = go.transform.Find(name + "(Placeholder)");
            return t != null ? t.gameObject : null;
        }
    }

    public static GameObject findFirstChildMatching(this GameObject go, string s0, bool startWild, bool endWild) {
        var s = s0;
        if (startWild)
            s = s.Substring(1);
        if (endWild)
            s = s.Substring(0, s.Length - 1);
        foreach (Transform t in go.transform) {
            var name = t.gameObject.name;
            var match = false;
            if (startWild && endWild) {
                match = name.Contains(s);
            } else if (startWild) {
                match = name.EndsWith(s, StringComparison.InvariantCulture);
            } else if (endWild) {
                match = name.StartsWith(s, StringComparison.InvariantCulture);
            }

            if (match) {
                return t.gameObject;
            } else {
                if (debugMode)
                    SNUtil.log(
                        "Found no match for " + s0 + " against " + t.gameObject.GetFullHierarchyPath(),
                        SNUtil.diDLL
                    );
                var inner = findFirstChildMatching(t.gameObject, s0, startWild, endWild);
                if (inner)
                    return inner;
            }
        }

        return null;
    }

    public static bool objectCollidesPosition(this GameObject go, Vector3 pos) {
        if (go.transform != null) {
            var c = go.GetComponentInParent<Collider>();
            if (c != null && c.enabled) {
                return (c.ClosestPoint(pos) - pos).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
            }

            var r = go.GetComponentInChildren<Renderer>();
            if (r != null && r.enabled) {
                return r.bounds.Contains(pos);
            }
        }

        return false;
    }

    public static string getPrefabID(this GameObject go) {
        if (go == null)
            return null;
        var p = go.GetComponentInParent<PrefabIdentifier>();
        if (p != null)
            return p.classId;
        var type = CraftData.GetTechType(go);
        return CraftData.GetClassIdForTechType(type);
    }

    public static GameObject createWorldObject(TechType tt, bool clone = true, bool makeActive = true) {
        if (tt == TechType.None)
            throw new Exception("Cannot spawn prefab for TechType.None!");
        var prefab = lookupPrefab(tt).GetResult();
        if (!prefab) {
            SNUtil.writeToChat(
                "Prefab not found for TechType '" + tt + "' [" + CraftData.GetClassIdForTechType(tt) + "]."
            );
            return null;
        }

        return createWorldObject(prefab, clone, makeActive);
    }

    public static GameObject createWorldObject(string id, bool clone = true, bool makeActive = true) {
        if (string.IsNullOrEmpty(id))
            throw new Exception("Cannot spawn prefab from null/empty classID!");
        var prefab = lookupPrefab(id);
        if (!prefab) {
            SNUtil.writeToChat("Prefab not found for id '" + id + "' [" + PrefabData.getPrefab(id) + "].");
            return null;
        }

        return createWorldObject(prefab, clone, makeActive);
    }

    private static GameObject createWorldObject(GameObject prefab, bool clone, bool makeActive) {
        var go = clone ? prefab.clone() : prefab;
        if (go) {
            go.SetActive(makeActive);
            return go;
        } else {
            SNUtil.writeToChat("Prefab found and placed but resulted in null?!");
            return null;
        }
    }

    public static O clone<O>(this O go) where O : UnityEngine.Object {
        return UnityEngine.Object.Instantiate(go);
    }

    public static O clone<O>(this O go, Vector3 pos, Quaternion rot) where O : UnityEngine.Object {
        return UnityEngine.Object.Instantiate(go, pos, rot);
    }

    public static GameObject getItem(this TechType tt) {
        var seek = tt;
        var sg = tt.AsString(false);
        if (sg.EndsWith("EggUndiscovered", StringComparison.InvariantCultureIgnoreCase)) {
            seek = (TechType)Enum.Parse(typeof(TechType), sg.Replace("EggUndiscovered", "Egg"));
        }

        switch (tt) { //special handling if any
            default:
                break;
        }

        var pfb = lookupPrefab(seek).GetResult().clone();
        pfb.SetActive(false);
        if (seek != tt) {
            var pp = pfb.GetComponentInChildren<Pickupable>();
            if (pp)
                pp.SetTechTypeOverride(tt);
        }

        return pfb;
    }

    public static bool isPlantable(this TechType tt) {
        var go = lookupPrefab(tt).GetResult();
        return go && go.GetComponent<Plantable>();
    }

    public static bool isRawFish(this TechType tt) {
        return false; // TODO
        // return CraftData.cookedCreatureList.ContainsKey(tt);
    }

    public static FoodCategory getFoodCategory(this TechType tt) {
        if (tt.isRawFish())
            return FoodCategory.RAWMEAT;
        // TODO
        // else if (CraftData.cookedCreatureList.Values.Contains(tt) || tt.AsString().StartsWith("Cured"))
        //     return FoodCategory.EDIBLEMEAT;
        else if (tt.isPlantable())
            return FoodCategory.PLANT;
        else
            return FoodCategory.OTHER;
    }

    public static CoroutineTask<GameObject> lookupPrefab(TechType tt) {
        /*
        string id = CraftData.GetClassIdForTechType(tt);
        return string.IsNullOrEmpty(id) ? null : lookupPrefab(id);*/
        return CraftData.GetPrefabForTechTypeAsync(tt);
    }

    public static GameObject lookupPrefab(string id) {
        if (UWE.PrefabDatabase.GetPrefabAsync(id).TryGetPrefab(out var ret))
            return ret;
        if (EnumHandler.TryGetValue(id, out TechType key)) {
            ret = lookupPrefab(key).GetResult();
        }

        return ret;
    }

    public static GameObject replaceObject(GameObject obj, string pfb) {
        var repl = createWorldObject(pfb);
        repl.transform.position = obj.transform.position;
        repl.transform.rotation = obj.transform.rotation;
        repl.transform.localScale = obj.transform.localScale;
        return repl;
    }

    public static void offsetColliders(this GameObject go, Vector3 move) {
        foreach (var c in go.GetComponentsInChildren<Collider>()) {
            if (c is SphereCollider sc) {
                sc.center += move;
            } else if (c is BoxCollider bc) {
                bc.center += move;
            } else if (c is CapsuleCollider cc) {
                cc.center += move;
            } else if (c is MeshCollider mc) {
                //TODO move to subobject
            }
        }
    }

    public static void refillItem(this GameObject item, TechType batteryType = TechType.Battery) {
        var o = item.GetComponentInParent<Oxygen>();
        if (o != null) {
            o.oxygenAvailable = o.oxygenCapacity;
        }

        var b = item.GetComponentInParent<Battery>();
        if (b != null) {
            b.charge = b.capacity;
        }

        var e = item.GetComponentInParent<EnergyMixin>();
        if (e != null) {
            e.StartCoroutine(e.SetBatteryAsync(batteryType, 1, new TaskResult<InventoryItem>()));
        }
    }

    public static GameObject getBasePiece(string n, bool clone = true) {
        if (n.StartsWith("base_", StringComparison.InvariantCultureIgnoreCase))
            n = n.Substring(5);
        Base.PieceDef? piece = null;
        var res = -1;
        if (int.TryParse(n, out res)) {
            piece = Base.pieces[res];
        } else {
            res = (int)Enum.Parse(typeof(Base.Piece), n);
            piece = Base.pieces[res];
        }

        var ret = piece != null && piece.HasValue ? getBasePiece(piece.Value, clone) : null;
        if (ret && clone && res == (int)Base.Piece.RoomWaterParkHatch) {
            foreach (Transform t in ret.transform) {
                if (t && t.name == "BaseCorridorHatch(Clone)")
                    t.gameObject.destroy();
            }
        }

        return ret;
    }

    public static GameObject getBasePiece(Base.Piece piece, bool clone = true) {
        return getBasePiece(Base.pieces[(int)piece], clone);
    }

    public static GameObject getBasePiece(Base.PieceDef piece, bool clone = true) {
        var go = piece.prefab.gameObject;
        if (clone) {
            go = go.clone();
            go.SetActive(true);
        }

        return go;
    }

    public static void applyGravity(this GameObject go) {
        //if (go.GetComponentInChildren<Collider>() == null || go.GetComponentInChildren<Rigidbody>() == null)
        //	return;
        if (go.GetComponentInChildren<Collider>() == null) {
            var box = go.AddComponent<BoxCollider>();
            box.center = Vector3.zero;
            box.size = Vector3.one * 0.25F;
        }

        //WorldForcesManager.instance.AddWorldForces(go.EnsureComponent<WorldForces>());
        var wf = go.EnsureComponent<WorldForces>();
        wf.enabled = true;
        wf.handleDrag = true;
        wf.handleGravity = true;
        wf.aboveWaterGravity = 9.81F;
        wf.underwaterGravity = 2;
        wf.underwaterDrag = 0.5F;
        var rb = go.EnsureComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = false; //true;
        rb.detectCollisions = true;
        rb.isKinematic = false;
        rb.drag = 0.5F;
        rb.angularDrag = 0.05F; /*
        rb.centerOfMass = new Vector3(0, 0.5F, 0);
        rb.inertiaTensor = new Vector3(0.2F, 0, 0.2F);*/
        // wf.Awake();
        wf.OnEnable();
        rb.WakeUp();
    }

    public static string formatFileName(CustomPrefab pfb) {
        var n = pfb.Info.ClassID;
        var ret = new System.Text.StringBuilder();
        for (var i = 0; i < n.Length; i++) {
            var c = n[i];
            if (c == '_')
                continue;
            var caps = i == 0 || n[i - 1] == '_';
            c = caps ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c);
            ret.Append(c);
        }

        return ret.ToString();
    }

    public static GameObject getModPrefabBaseObject<T>(DIPrefab<T> pfb) where T : PrefabReference {
        GameObject world = null;
        if (pfb is CustomPrefab p && p.TryGetGadget<CraftingGadget>(out var c) && false) {
            world = getItemGO(p as CustomPrefab, pfb.baseTemplate.getPrefabID());
            world = world.clone();
        } else {
            world = createWorldObject(pfb.baseTemplate.getPrefabID(), true, false);
        }

        if (!world) {
            SNUtil.writeToChat("Could not fetch template GO for " + pfb);
            return null;
        }

        world.SetActive(false);
        convertTemplateObject(world, pfb);
        return world;
    }

    public static void convertTemplateObject<T>(GameObject go, DIPrefab<T> pfb, bool basicPropertiesOnly = false)
        where T : PrefabReference {
        var mod = (CustomPrefab)pfb;
        go.EnsureComponent<TechTag>().type = mod.Info.TechType;
        var pi = go.EnsureComponent<PrefabIdentifier>();
        pi.ClassId = mod.Info.ClassID;
        if (pfb.isResource()) {
            var res = go.EnsureComponent<ResourceTracker>();
            res.prefabIdentifier = pi;
            res.techType = mod.Info.TechType;
            res.overrideTechType = mod.Info.TechType;
        }

        if (basicPropertiesOnly)
            return;
        var r = go.GetComponentsInChildren<Renderer>(true);
        if (r != null && r.Length > 0 && pfb.getTextureFolder() != null)
            RenderUtil.swapToModdedTextures(r, pfb);
        pfb.prepareGameObject(go, r);
        //writeToChat("Applying custom texes to "+world+" @ "+world.transform.position);
        go.name = pfb.GetType() + " " + mod.Info.ClassID;
    }

    public static void convertResourceChunk(this GameObject go, TechType tech) {
        /*
        go.EnsureComponent<TechTag>().type = tech;
        go.EnsureComponent<Pickupable>().SetTechTypeOverride(tech);
        go.EnsureComponent<PrefabIdentifier>().ClassId = mod.ClassID;
        go.EnsureComponent<ResourceTracker>().techType = tech;
        go.EnsureComponent<ResourceTracker>().overrideTechType = tech;
        */
        var world = createWorldObject(tech, true, false);
        world.transform.position = go.transform.position;
        world.transform.rotation = go.transform.rotation;
        world.transform.localScale = go.transform.localScale;
        go.destroy(false);
    }

    public static Light addLight(this GameObject go) {
        var child = new GameObject {
            transform = {
                parent = go.transform,
                localPosition = Vector3.zero,
            },
        };
        return child.AddComponent<Light>().setName("Light Entity");
    }

    public static Light addLight(this GameObject go, float intensity, float radius) {
        var l = go.addLight();
        l.intensity = intensity;
        l.range = radius;
        return l;
    }

    public static Light addLight(this GameObject go, float intensity, float radius, Color c) {
        var l = go.addLight(intensity, radius);
        l.color = c;
        return l;
    }

    public static T copyComponent<T>(GameObject from, GameObject to) where T : Component {
        var tgt = to.EnsureComponent<T>();
        tgt.copyObject(from.GetComponent<T>());
        return tgt;
    }

    public static void ignoreCollisions(this GameObject from, params GameObject[] with) {
        foreach (var go in with) {
            foreach (var c in go.GetComponentsInChildren<Collider>(true)) {
                foreach (var c0 in from.GetComponentsInChildren<Collider>(true)) {
                    Physics.IgnoreCollision(c0, c);
                }
            }
        }
    }

    public static void setSky(this GameObject go, mset.Sky sky) {
        if (!go)
            return;
        go.EnsureComponent<SkyApplier>();
        foreach (var sk in go.GetComponentsInChildren<SkyApplier>(true)) {
            if (!sk)
                continue;
            if (sk.renderers == null)
                sk.renderers = go.GetComponentsInChildren<Renderer>();
            sk.environmentSky = sky;
            sk.applySky = sk.environmentSky;
            sk.enabled = true;
            sk.ApplySkybox();
            sk.RefreshDirtySky();
        }
    }

    public static void fullyEnable(this GameObject go) {
        go.SetActive(true);
        foreach (var mb in go.GetComponentsInChildren<Behaviour>(true)) {
            mb.enabled = true;
        }

        foreach (Transform t in go.transform) {
            if (t)
                fullyEnable(t.gameObject);
        }
    }

    public static void addCyclopsHologramWarning(Component sub, GameObject go, Sprite spr = null) {
        var hud = sub.GetComponentInChildren<CyclopsHolographicHUD>();
        if (hud) {
            hud.AttachedLavaLarva(go);
            if (spr != null) {
                foreach (var ping in hud.lavaLarvaIcons) {
                    if (ping.refGo.Equals(go)) {
                        ping.creatureIcon.GetComponentInChildren<UnityEngine.UI.Image>().sprite = spr;
                    }
                }
            }
        }
    }

    public static bool isOnScreen(this GameObject go, Camera c) {
        var planes = GeometryUtility.CalculateFrustumPlanes(c);
        return GeometryUtility.TestPlanesAABB(planes, new Bounds(go.transform.position, Vector3.one * 0.25F));
    }

    public static bool isVisible(this GameObject go) {
        return WorldUtil.lineOfSight(go, Player.main.gameObject, r => !r.collider.gameObject.FindAncestor<Vehicle>()) &&
               isOnScreen(go, Camera.main);
    }

    public static bool isLookingAt(Transform looker, Vector3 pos, float maxAng) {
        return Vector3.Angle(looker.forward, pos - looker.transform.position) <= maxAng;
    }

    public static bool isRoom(this GameObject go, bool allowTunnelConnections) {
        return isPieceType(go, allowTunnelConnections, "BaseRoom");
    }

    public static bool isMoonpool(this GameObject go, bool allowTunnelConnections, bool allowRoof) {
        if (!allowRoof) {
            var face = go.GetComponentInParent<BaseExplicitFace>();
            return face && face.gameObject.name.StartsWith(
                "BaseMoonpoolCoverSide",
                StringComparison.InvariantCultureIgnoreCase
            );
        }

        return isPieceType(go, allowTunnelConnections, "BaseMoonpool");
    }

    private static bool isPieceType(this GameObject go, bool allowTunnelConnections, string type) {
        if (!allowTunnelConnections) {
            var g2 = go;
            while (g2.transform.parent && !g2.name.StartsWith("Base", StringComparison.InvariantCultureIgnoreCase))
                g2 = g2.transform.parent.gameObject;
            if (g2.name.Contains("Corridor") || g2.name.Contains("Hatch"))
                return false;
        }

        var bc = go.FindAncestor<BaseCell>();
        return bc && getChildObject(bc.gameObject, type);
    }

    public static bool isInWater(this GameObject go) {
        return go.transform.position.y <= Ocean.GetOceanLevel() && isLoose(go) &&
               !WorldUtil.isPrecursorBiome(go.transform.position);
    }

    public static bool isLoose(this GameObject go) {
        var t = go.transform.parent;
        return !t || t.name == "SerializerEmptyGameObject" || t.name == "CellRoot(Clone)";
    }

    public static bool isLODRenderer(Renderer r) {
        return !r.name.Contains("LOD1") && !r.name.Contains("LOD2") && !r.name.Contains("LOD3");
    }

    public static Renderer[] getNonLODRenderers(GameObject go) {
        return go.GetComponentsInChildren<Renderer>().Where(r => !isLODRenderer(r)).ToArray();
    }

    public static bool isPlayer(this Component c, bool allowChildren = false) {
        return isPlayer(c.gameObject, allowChildren);
    }

    public static bool isPlayer(this GameObject c, bool allowChildren = false) {
        return Player.main && (allowChildren ? (bool)c.FindAncestor<Player>() : c == Player.main.gameObject);
    }

    public static bool isPlayerOrCreature(this Component c, bool allowChildren = false) {
        return isPlayer(c, allowChildren) || (allowChildren
            ? (bool)c.gameObject.FindAncestor<Creature>()
            : (bool)c.gameObject.GetComponent<Creature>());
    } /*

    public static GameObject getRootObjectFromCollider(Component c) {
        ColliderPrefabLink cp = c.GetComponent<ColliderPrefabLink>();
        if (cp)
            return cp.root;
        PrefabIdentifier pi = c.gameObject.FindAncestor<PrefabIdentifier>();
        return pi.gameObject;
    }

    public static C getRootComponentFromCollider<C>(Component c) where C : Component {
        ColliderPrefabLink cp = c.GetComponent<ColliderPrefabLink>();
        return cp && cp.root ? cp.root.GetComponent<C>() : c.gameObject.FindAncestor<C>();
    }*/

    public static BaseCell getBaseRoom(BaseRoot bb, GameObject go) {
        var par = go.transform.parent.GetComponent<BaseCell>();
        return par ? par : getBaseRoom(bb, go.transform.position);
    }

    public static BaseCell getBaseRoom(BaseRoot bb, Vector3 pos) {
        foreach (var bc in bb.GetComponentsInChildren<BaseCell>()) {
            var room = bc.gameObject.getChildObject("BaseRoom");
            if (!room)
                continue;
            //Bounds box = new Bounds(room.transform.position, new Vector3(4.5F, 1.5F, 4.5F));
            if (MathUtil.isPointInCylinder(room.transform.position, pos, 4.75, 1.75)) {
                //if (box.Contains()) {
                return bc;
            }
        }

        return null;
    }

    public static List<PrefabIdentifier> getBaseObjectsInRoom(BaseRoot bb, BaseCell room) {
        //automatically skips contents of inventories
        List<PrefabIdentifier> li = [];
        getBaseObjects(
            bb,
            pi => {
                if (getBaseRoom(bb, pi.gameObject) == room)
                    li.Add(pi);
            }
        );
        return li;
    }

    public static void getBaseObjects(BaseRoot bb, Action<PrefabIdentifier> acceptor) {
        //automatically skips contents of inventories
        iterateChildPrefabs(bb.transform, acceptor);
    }

    private static void iterateChildPrefabs(Transform from, Action<PrefabIdentifier> acceptor) {
        //do not recurse into PIs inside other PIs (eg invs)
        foreach (Transform t in from.transform) {
            var at = t.GetComponent<PrefabIdentifier>();
            if (at)
                acceptor(at);
            else
                iterateChildPrefabs(t, acceptor);
        }
    }

    public static List<PrefabIdentifier> getBaseObjects(BaseRoot bb) {
        List<PrefabIdentifier> li = [];
        getBaseObjects(bb, li.Add);
        return li;
    }

    public static bool isOnBase(BaseRoot bb, Component c) {
        var baseObj = bb.transform;
        var t = c.transform;
        while (t != null) {
            if (t == baseObj)
                return true;
            t = t.parent;
        }

        return false;
    }

    public static void reparentTo(GameObject go, GameObject child) {
        var pos = child.transform.position;
        var rot = child.transform.rotation;
        child.transform.SetParent(go.transform);
        child.transform.position = pos;
        child.transform.rotation = rot;
    }

    public static bool isPrecursor(this GameObject go) {
        //if (go.name.ToLowerInvariant().Contains("precursor"))
        //	return true;
        var pi = go.GetComponent<PrefabIdentifier>();
        if (pi == null)
            return false;
        var pfb = PrefabData.getPrefab(pi.ClassId);
        return pfb != null && pfb.Contains("/Precursor/");
    }

    public static bool isRootObject(this GameObject go) {
        return (bool)go.GetComponent<LargeWorldEntity>() &&
               !(go.transform.parent && go.transform.parent.gameObject.FindAncestor<LargeWorldEntity>());
    }

    public static void cleanUpOriginObjects(Component c) {
        if (c.transform.position.sqrMagnitude < 0.01)
            c.gameObject.destroy(false);
    }

    public static string tryGetObjectIdentifiers(this Component c, out PrefabIdentifier prefab, out TechType tech) {
        return tryGetObjectIdentifiers(c.gameObject, out prefab, out tech);
    }

    public static string tryGetObjectIdentifiers(this GameObject go, out PrefabIdentifier prefab, out TechType tech) {
        prefab = go.FindAncestor<PrefabIdentifier>();
        var classID = prefab ? prefab.ClassId : "<NO PREFAB>";
        tech = CraftData.GetTechType(go);
        var techString = tech == TechType.None ? "<NO TECH>" : tech.AsString();
        return "ClID=" + classID + ", TT=" + techString;
    }

    public static void destroy(this UnityEngine.Object go, bool immediate = true, float delay = 0) {
        if (debugMode)
            SNUtil.log(
                "Destroying " + go + " (" + (go is GameObject go2 ? go2.GetFullHierarchyPath() : go.GetType().Name) +
                ") from\n" + SNUtil.getStacktrace(),
                SNUtil.diDLL
            );
        if (immediate)
            UnityEngine.Object.DestroyImmediate(go);
        else
            UnityEngine.Object.Destroy(go, delay);
    }

    public static GameObject setName(this GameObject go, string name) {
        go.name = name;
        return go;
    }

    public static C setName<C>(this C go, string name) where C : Component {
        go.gameObject.name = name;
        return go;
    }

    public static bool isAncestorOf(this GameObject go, GameObject obj) {
        var t = obj.transform;
        while (t) {
            if (t.gameObject == go)
                return true;
            t = t.parent;
        }

        return false;
    }

    public static bool isAncestorOf<C>(this GameObject go, C obj) where C : Component {
        return go.isAncestorOf(obj.gameObject);
    }

    public static GameObject createAirBubble() {
        var coral = lookupPrefab(VanillaFlora.BRAIN_CORAL.getPrefabID());
        var ii = coral.GetComponent<IntermittentInstantiate>();
        return ii.prefab.clone();
    }
}

public enum FoodCategory {
    PLANT,
    RAWMEAT,
    EDIBLEMEAT,
    OTHER,
}