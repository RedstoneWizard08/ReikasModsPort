using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class ItemDisplay : CustomMachine<ItemDisplayLogic> {
    internal static readonly Dictionary<TechType, ItemDisplayRenderBehavior> renderPaths = new();

    static ItemDisplay() {
        /*
        setRendererBehavior(TechType.Flashlight, ItemDisplayRenderBehavior.getDefaultButSpecificChild("flashlight"));
        setRendererBehavior(TechType.BoneShark, ItemDisplayRenderBehavior.getDefaultButSpecificChild("bone_shark_anim"));
        setRendererBehavior(TechType.Biter, ItemDisplayRenderBehavior.getDefaultButSpecificChild("Biter_fish_anim"));
        setRendererBehavior(TechType.Blighter, ItemDisplayRenderBehavior.getDefaultButSpecificChild("Biter_fish_anim"));
        */


        setRendererBehavior(TechType.Polyaniline, new ItemDisplayRenderBehavior() { sizeMultiplier = 1.5F });
        setRendererBehavior(TechType.HydrochloricAcid, new ItemDisplayRenderBehavior() { sizeMultiplier = 1.5F });
        setRendererBehavior(TechType.Benzene, new ItemDisplayRenderBehavior() { sizeMultiplier = 2F });
        setRendererBehavior(TechType.BloodOil, new ItemDisplayRenderBehavior() { sizeMultiplier = 0.25F });
        setRendererBehavior(TechType.ReactorRod, ItemDisplayRenderBehavior.getDefaultButSpecificChild("model"));
    }

    public static void setRendererBehavior(TechType tt, ItemDisplayRenderBehavior path) {
        renderPaths[tt] = path;
    }

    public static void setRendererBehavior(TechType tt, TechType copyOf) {
        if (renderPaths.ContainsKey(copyOf))
            renderPaths[tt] = renderPaths[copyOf];
    }

    internal ItemDisplayRenderBehavior getBehavior(TechType tt) {
        return renderPaths.ContainsKey(tt) ? renderPaths[tt] : null;
    }

    [SetsRequiredMembers]
    public ItemDisplay(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "f1cde32e-101a-4dd5-8084-8c950b9c2432") {
        addIngredient(TechType.Titanium, 1);
    }

    public override bool UnlockedAtStart => true;

    public override bool isOutdoors() {
        return false;
    }

    public override TechGroup GroupForPDA => TechGroup.Miscellaneous;

    public override TechCategory CategoryForPDA => TechCategory.Misc;

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeComponent<Trashcan>();

        var mdl = go.setModel(
            "model",
            ObjectUtil.lookupPrefab("0fbf203a-a940-4b6e-ac63-0fe2737d84c2")
                .getChildObject("model/Base_interior_Planter_Pot_03")
        );
        mdl.removeChildObject("pot_generic_plant_03");
        mdl.transform.localScale = new Vector3(0.75F, 0.75F, 1.5F);
        mdl.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        var container = mdl.getChildObject("Base_interior_Planter_Pot_03 1");
        var floor = mdl.getChildObject("Base_exterior_Planter_Tray_ground");
        floor.transform.localScale = new Vector3(0.02F, 0.02F, 0.005F);
        floor.transform.localPosition = new Vector3(0, 0, 0.3F);

        var lgc = go.GetComponent<ItemDisplayLogic>();

        var con = go.GetComponentInChildren<StorageContainer>();
        initializeStorageContainer(con, 3, 3);
        con.errorSound = null;
        lgc.initStorageExt(con);

        go.removeChildObject("descent_trashcan_01");

        var r = container.GetComponentInChildren<Renderer>();
        RenderUtil.swapToModdedTextures(r, this);
        r.materials[0].SetColor("_Color", Color.white);
        r.materials[0].SetFloat("_Fresnel", 0.6F);
        r.materials[0].SetFloat("_SpecInt", 8F);
        r.materials[0].SetFloat("_Shininess", 15F);
        /*
        //SNUtil.dumpTextures(r);
        r.materials[0].SetFloat("_Shininess", 7.5F);
        lgc.mainRenderer = r;*/

        var screen = floor.GetComponentInChildren<Renderer>();
        RenderUtil.swapTextures(AqueousEngineeringMod.modDLL, screen, "Textures/Machines/ItemDisplayScreen");
        RenderUtil.setEmissivity(screen, 1.5F);
        screen.materials[0].SetColor("_Color", Color.white);
        screen.materials[0].SetColor("_GlowColor", Color.white);
        screen.materials[0].SetFloat("_Fresnel", 0F);
        screen.materials[0].SetFloat("_SpecInt", 5F);
        screen.materials[0].SetFloat("_Shininess", 0F);

        go.GetComponent<Constructable>().model = mdl;
        //go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
        //go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
    }
}

public class ItemDisplayLogic : CustomMachineLogic {
    private static readonly string DISPLAY_OBJECT_NAME = "DisplayItem";

    private GameObject display;
    private Renderer displayRender;
    private SkyApplier displaySky;

    public TechType displayType { get; private set; }

    public Pickupable currentItem =>
        displayType == TechType.None ? null : storage.container.GetItems(displayType)[0].item;

    private float additionalRenderSpace = 0.075F;
    private float rotationSpeedScale = 1;
    private float renderSizeScale = 1;

    private bool needsUpdate = true;

    //private Vector3 rotationSpeed = Vector3.zero;
    //private Vector3 rotationSpeedTargets = Vector3.zero;

    private void Start() {
        SNUtil.Log("Reinitializing base item display");
        AqueousEngineeringMod.displayBlock.initializeMachine(gameObject);
    }

    internal void initStorageExt(StorageContainer sc) {
        initStorage(sc);
    }

    protected override void initStorage(StorageContainer sc) {
        base.initStorage(sc);
        sc.container.onAddItem += updateStoredItem;
        sc.container.onRemoveItem += ii => setDisplay(null);
        sc.container.isAllowedToAdd = new IsAllowedToAdd((pp, vb) => storage.isEmpty());
    }

    private void OnDestroy() {
        if (display)
            display.destroy();
    }

    private void OnDisable() {
        if (display)
            display.destroy();
    }

    protected override float getTickRate() {
        return 0;
    }

    protected override void updateEntity(float seconds) {
        if (needsUpdate && DIHooks.GetWorldAge() > 0.5F) {
            updateStoredItem();
            needsUpdate = false;
        }

        if (display) { /*
            updateRotationSpeedTarget(ref rotationSpeed.x, ref rotationSpeedTargets.x);
            updateRotationSpeedTarget(ref rotationSpeed.y, ref rotationSpeedTargets.y);
            updateRotationSpeedTarget(ref rotationSpeed.z, ref rotationSpeedTargets.z);

            //display.transform.Rotate(rotationSpeed, Space.Self);
            Vector3 ctr = displayRender.bounds.center*2-displayRender.transform.position;
            display.transform.RotateAround(ctr, new Vector3(1, 0, 0), rotationSpeed.x);
            display.transform.RotateAround(ctr, new Vector3(0, 1, 0), rotationSpeed.y);
            display.transform.RotateAround(ctr, new Vector3(0, 0, 1), rotationSpeed.z);
            */
            var itemOffset = 0.75F + 0.1F * Mathf.Pow(
                Mathf.Max(
                    0,
                    1 + Mathf.Sin(DayNightCycle.main.timePassedAsFloat * 0.781F + gameObject.GetInstanceID() * 0.147F)
                ),
                2
            );
            var itemRotation = rotationSpeedScale * 1.33F * Mathf.Max(0, 4F - itemOffset * 3F);
            //SNUtil.writeToChat(itemOffset+" > "+itemRotation);
            display.transform.position = transform.position + Vector3.up * (itemOffset + additionalRenderSpace);
            display.transform.Rotate(display.transform.up * itemRotation, Space.Self);
            //if (displaySky)
            //	displaySky.SetSky(gets);
        }
    }

    public void updateStoredItem() {
        if (storage) {
            var items = storage.container.GetItemTypes();
            if (items.Count > 0) {
                var li = storage.container.GetItems(items[0]);
                if (li.Count > 0) {
                    updateStoredItem(li[0]);
                    return;
                }
            }
        }

        updateStoredItem(null);
    }

    public void updateStoredItem(InventoryItem ii) {
        //SNUtil.writeToChat("Set display to "+(ii != null && ii.item ? ii.item+"" : "null"));
        /*
        setDisplay(null);
        StorageContainer sc = getStorage();
        if (sc) {
            List<TechType> items = sc.container.GetItemTypes();
            if (items.Count > 0) {
                IList<InventoryItem> li = sc.container.GetItems(items[0]);
                if (li.Count > 0) {
                    setDisplay(li[0].item.gameObject);
                }
            }
        }*/
        setDisplay(ii != null && ii.item ? ii.item : null);
    }

    private void updateRotationSpeedTarget(ref float speed, ref float target) {
        if (Mathf.Approximately(speed, target)) {
            target = UnityEngine.Random.Range(-4F, 4F);
        } else if (speed > target) {
            speed = Mathf.Max(target, speed - 0.15F);
        } else if (speed < target) {
            speed = Mathf.Min(target, speed + 0.15F);
        }
    }

    private void setDisplay(Pickupable pp) {
        displayRender = null;
        displaySky = null;
        if (display)
            display.destroy();
        var old = gameObject.getChildObject(DISPLAY_OBJECT_NAME);
        if (old)
            display.destroy();
        if (!pp)
            return;

        var tt = pp.GetTechType();
        var go = findRenderer(pp, tt, out var r);
        if (!go)
            return;

        // TODO
        // if (WaterParkCreature.waterParkCreatureParameters.ContainsKey(tt)) {
        //     renderSizeScale *= WaterParkCreature.waterParkCreatureParameters[tt].maxSize /
        //         WaterParkCreature.waterParkCreatureParameters[tt].outsideSize * 0.33F;
        // }

        var renderObj = go.clone();
        renderObj.convertToModel();
        display = new GameObject(DISPLAY_OBJECT_NAME);
        renderObj.transform.SetParent(display.transform);
        display.SetActive(true);
        renderObj.SetActive(true);
        display.transform.SetParent(transform);
        display.transform.position = transform.position + Vector3.up;
        renderObj.transform.localPosition = Vector3.zero;
        renderObj.transform.localRotation = r.transform.localRotation;
        renderObj.transform.localScale *= renderSizeScale;
        displayRender = r;
        displaySky = renderObj.GetComponentInChildren<SkyApplier>();
        displayType = pp.GetTechType();
    }

    private GameObject findRenderer(Pickupable pp, TechType tt, out Renderer ret) {
        var props = ItemDisplay.renderPaths.ContainsKey(tt)
            ? ItemDisplay.renderPaths[tt]
            : new ItemDisplayRenderBehavior();
        additionalRenderSpace = props.verticalOffset;
        rotationSpeedScale = props.rotationSpeedMultiplier;
        renderSizeScale = props.sizeMultiplier;
        if (props.getRenderObj != null) {
            var obj = props.getRenderObj(pp.gameObject);
            ret = obj ? obj.GetComponentInChildren<Renderer>() : null;
            return obj;
        }

        var a = pp.GetComponentInChildren<Animator>();
        if (a) {
            ret = a.GetComponentInChildren<Renderer>();
            return a.gameObject;
        }

        var rr = pp.GetComponentsInChildren<Renderer>();
        foreach (var r in rr) {
            if (r.GetComponent<Light>())
                continue;
            var cloneFrom = r.gameObject;
            if (cloneFrom.GetFullHierarchyPath().Contains("$DisplayRoot")) {
                while (cloneFrom.transform.parent && !cloneFrom.name.Contains("$DisplayRoot"))
                    cloneFrom = cloneFrom.transform.parent.gameObject;
                var seek = "offset=";
                var idx = cloneFrom.name.IndexOf(seek, StringComparison.InvariantCultureIgnoreCase);
                if (idx >= 0) {
                    additionalRenderSpace = float.Parse(
                        cloneFrom.name.Substring(idx + seek.Length),
                        System.Globalization.CultureInfo.InvariantCulture
                    );
                }
            }

            ret = r;
            return cloneFrom;
        }

        ret = null;
        return null;
    }
}

public class ItemDisplayRenderBehavior {
    public float verticalOffset = 0.075F;
    public float rotationSpeedMultiplier = 1;
    public float sizeMultiplier = 1;
    public Func<GameObject, GameObject> getRenderObj;

    public static ItemDisplayRenderBehavior getDefaultButSpecificChild(string path) {
        return new ItemDisplayRenderBehavior() { getRenderObj = getChildNamed(path) };
    }

    public static ItemDisplayRenderBehavior getDefaultButSpecialRenderObj(Func<GameObject, GameObject> f) {
        return new ItemDisplayRenderBehavior() { getRenderObj = f };
    }

    public static Func<GameObject, GameObject> getChildNamed(string path) {
        return go => go.getChildObject(path);
    }
}