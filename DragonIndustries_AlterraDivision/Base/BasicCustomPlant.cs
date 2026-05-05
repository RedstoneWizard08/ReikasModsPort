using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class BasicCustomPlant : CustomPrefab, DIPrefab<FloraPrefabFetch>, Flora {
    public float glowIntensity { get; set; }
    public FloraPrefabFetch baseTemplate { get; set; }

    public readonly BasicCustomPlantSeed seed;

    public HarvestType collectionMethod = HarvestType.DamageAlive;
    public int finalCutBonus = 2;

    private readonly Assembly ownerMod;

    private readonly List<BiomeBase> nativeBiomesCave = [];
    private readonly List<BiomeBase> nativeBiomesSurface = [];

    private static readonly Dictionary<TechType, BasicCustomPlant>
        plants = new();

    private static readonly Dictionary<string, BasicCustomPlant> plantIDs = new();

    public PDAManager.PDAPage pdaPage { get; private set; }

    [SetsRequiredMembers]
    public BasicCustomPlant(
        XMLLocale.LocaleEntry e,
        FloraPrefabFetch template,
        string seedPfb,
        string seedName = "Seed"
    ) : this(e.key, e.name, e.desc, template, seedPfb, seedName) {
    }

    [SetsRequiredMembers]
    public BasicCustomPlant(
        string id,
        string name,
        string desc,
        FloraPrefabFetch template,
        string seedPfb,
        string seedName = "Seed"
    ) : base(id, name, desc) {
        baseTemplate = template;
        ownerMod = SNUtil.tryGetModDLL();
        // typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
        seed = seedPfb == null ? null : new BasicCustomPlantSeed(this, seedPfb, seedName);
        AddOnRegister(() => {
                plants[Info.TechType] = this;
                plantIDs[Info.ClassID] = this;
                if (collectionMethod != HarvestType.None || generateSeed()) {
                    ItemRegistry.instance.addItem(seed);
                    setPlantSeed(seed, this);
                    CraftDataHandler.SetHarvestType(Info.TechType, collectionMethod);
                    CraftDataHandler.SetHarvestOutput(Info.TechType, seed.Info.TechType);
                    CraftDataHandler.SetHarvestFinalCutBonus(Info.TechType, finalCutBonus);
                    SNUtil.log("Finished patching " + this + " > " + seed, ownerMod);
                }
            }
        );
        SetGameObject(GetGameObject);
        Info.WithIcon(GetItemSprite()).WithSizeInInventory(SizeInInventory);
    }

    public static void setPlantSeed(CustomPrefab seed, BasicCustomPlant plant) {
        plants[seed.Info.TechType] = plant;
        plantIDs[seed.Info.ClassID] = plant;
    }

    public BasicCustomPlant addNativeBiome(BiomeBase b, bool caveOnly = false) {
        nativeBiomesCave.Add(b);
        if (!caveOnly)
            nativeBiomesSurface.Add(b);
        return this;
    }

    public bool isNativeToBiome(Vector3 vec) {
        return isNativeToBiome(BiomeBase.GetBiome(vec), WorldUtil.isInCave(vec));
    }

    public bool isNativeToBiome(BiomeBase b, bool cave) {
        return (cave ? nativeBiomesCave : nativeBiomesSurface).Contains(b);
    }

    public string getPrefabID() {
        return Info.ClassID;
    }

    public void addPDAEntry(string text, float scanTime = 2, string header = null) {
        var e = new PDAScanner.EntryData {
            key = Info.TechType,
            scanTime = scanTime,
            locked = true,
        };
        pdaPage = PDAManager.createPage("ency_" + Info.ClassID, Info.PrefabFileName, text, "Lifeforms");
        pdaPage.addSubcategory("Flora").addSubcategory(isExploitable() ? "Exploitable" : "Sea");
        if (header != null)
            pdaPage.setHeaderImage(TextureManager.getTexture(ownerMod, "Textures/PDA/" + header));
        pdaPage.register();
        e.encyclopedia = pdaPage.id;
        PDAHandler.AddCustomScannerEntry(e);
    }

    public virtual Vector2int SizeInInventory => new(1, 1);

    protected virtual Sprite GetItemSprite() {
        return TextureManager.getSprite(ownerMod, "Textures/Items/" + ObjectUtil.formatFileName(this));
    }

    public Sprite getSprite() {
        return GetItemSprite();
    }

    public virtual void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public sealed override string ToString() {
        return base.ToString() + " [" + Info.TechType + "] / " + Info.ClassID + " / " + Info.PrefabFileName + " S=" +
               seed;
    }

    public GameObject GetGameObject() {
        var go = ObjectUtil.getModPrefabBaseObject(this);
        var p = go.EnsureComponent<Pickupable>();
        p.isPickupable = false;
        go.EnsureComponent<ImmuneToPropulsioncannon>();
        return go;
    }

    public Assembly getOwnerMod() {
        return ownerMod;
    }

    protected virtual bool isExploitable() {
        return collectionMethod != HarvestType.None || isResource();
    }

    protected virtual bool generateSeed() {
        return collectionMethod != HarvestType.None;
    }

    public string ClassID => Info.ClassID;

    public virtual bool isResource() {
        return true;
    }

    public virtual string getTextureFolder() {
        return "Plants";
    }

    public Sprite getIcon() {
        return GetItemSprite();
    }

    public virtual Plantable.PlantSize getSize() {
        return Plantable.PlantSize.Large;
    }

    public virtual float getScaleInGrowbed(bool indoors) {
        return 1;
    }

    public virtual bool canGrowAboveWater() {
        return false;
    }

    public virtual bool canGrowUnderWater() {
        return true;
    } /*

    public virtual float getGrowthTime() {
        return 1200;
    }

    public virtual void prepareGrowingPlant(GrowingPlant g) {

    }*/

    public virtual void modifySeed(GameObject go) {
    }

    public static BasicCustomPlant getPlant(TechType tt) {
        return plants.ContainsKey(tt) ? plants[tt] : null;
    }

    public static BasicCustomPlant getPlant(string id) {
        return plantIDs.ContainsKey(id) ? plantIDs[id] : null;
    }
}

public class FloraPrefabFetch : PrefabReference {
    private string prefab;
    private VanillaFlora flora;

    public FloraPrefabFetch(string pfb) {
        prefab = pfb;
    }

    public FloraPrefabFetch(VanillaFlora f) {
        flora = f;
    }

    public string getPrefabID() {
        return flora == null ? prefab : flora.getRandomPrefab(false);
    }
}

public class BasicCustomPlantSeed : CustomPrefab, DIPrefab<StringPrefabContainer> {
    public float glowIntensity { get; set; }
    public StringPrefabContainer baseTemplate { get; set; }

    public readonly BasicCustomPlant plant;

    public Sprite sprite;

    [SetsRequiredMembers]
    public BasicCustomPlantSeed(BasicCustomPlant p, string pfb, string seedName = "Seed") : base(
        p.Info.ClassID + "_seed",
        seedName,
        ""
    ) {
        plant = p;
        // typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, p.getOwnerMod());
        sprite = plant.getSprite();
        baseTemplate = new StringPrefabContainer(pfb);
        SetGameObject(GetGameObject);
        Info.WithIcon(GetItemSprite());
    }

    protected Sprite GetItemSprite() {
        return sprite;
    }

    // public Vector2int SizeInInventory {
    //     get { return plant.SizeInInventory; }
    // }
    /*
    public GrowingPlant getPlant(GameObject go) {
        return go.GetComponent<Plantable>().model.GetComponent<GrowingPlant>();
    }*/

    public Sprite getIcon() {
        return plant.getIcon();
    }

    public Assembly getOwnerMod() {
        return plant.getOwnerMod();
    }

    public GameObject GetGameObject() {
        var go = ObjectUtil.getModPrefabBaseObject(this);
        var pp = go.EnsureComponent<Pickupable>();
        pp.isPickupable = true;

        var p = go.EnsureComponent<Plantable>();
        p.aboveWater = plant.canGrowAboveWater();
        p.underwater = plant.canGrowUnderWater();
        p.isSeedling = true;
        p.plantTechType = plant.Info.TechType;
        p.size = plant.getSize();
        p.pickupable = pp;

        p.modelScale = Vector3.one * plant.getScaleInGrowbed(false);
        p.modelIndoorScale = Vector3.one * plant.getScaleInGrowbed(true);

        //GrowingPlant g = getPlant(go);
        //g.growthDuration = plant.getGrowthTime();
        //plant.prepareGrowingPlant(g);

        //ObjectUtil.convertTemplateObject(p.model, plant); //this is the GROWING but not grown one
        /*
        GrowingPlant grow = p.model.EnsureComponent<GrowingPlant>();
        grow.seed = p;
        grow.enabled = true;

        bool active = grow.grownModelPrefab.active;
        grow.grownModelPrefab = grow.grownModelPrefab.clone();
        grow.grownModelPrefab.SetActive(active);
        ObjectUtil.convertTemplateObject(grow.grownModelPrefab, plant);
        grow.grownModelPrefab.SetActive(true); //FIXME does not work
        Renderer r = grow.grownModelPrefab.GetComponentInChildren<Renderer>();
        plant.prepareGameObject(grow.grownModelPrefab, r);
        grow.growingTransform = grow.grownModelPrefab.transform;
        grow.growingTransform.gameObject.SetActive(true);*/
        /*
        CapsuleCollider cu = plant.GetGameObject().GetComponentInChildren<CapsuleCollider>();
        if (cu != null) {
            CapsuleCollider cc = p.model.EnsureComponent<CapsuleCollider>();
            cc.radius = cu.radius*0.8F;
            cc.center = cu.center;
            cc.direction = cu.direction;
            cc.height = cu.height;
            cc.material = cu.material;
            cc.name = cu.name;
            cc.enabled = cu.enabled;
            cc.isTrigger = cu.isTrigger;
        }*/

        plant.modifySeed(go);

        return go;
    }

    public virtual void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public string ClassID => Info.ClassID;

    public bool isResource() {
        return true;
    }

    public string getTextureFolder() {
        return "Items";
    }

    public sealed override string ToString() {
        return base.ToString() + " [" + Info.TechType + "] / " + ClassID + " / " + Info.PrefabFileName;
    }
}