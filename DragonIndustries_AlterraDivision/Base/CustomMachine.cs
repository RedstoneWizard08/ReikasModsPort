using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public abstract class CustomMachine<M> : CustomPrefab, DIPrefab<CustomMachine<M>, StringPrefabContainer>
    where M : CustomMachineLogic {
    private static readonly MachineSaveHandler saveHandler = new();

    private readonly List<PlannedIngredient> recipe = [];

    public readonly string id;

    private readonly Assembly ownerMod;

    private PDAManager.PDAPage page;

    public float glowIntensity { get; set; }
    public StringPrefabContainer baseTemplate { get; set; }

    //public string storageLabel { get; private set; }

    [SetsRequiredMembers]
    protected CustomMachine(string id, string name, string desc, string template) : base(id, name, desc) {
        ownerMod = SNUtil.tryGetModDLL();
        // typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
        this.id = id;
        baseTemplate = new StringPrefabContainer(template);

        AddOnRegister(() => {
                DIMod.machineList[Info.TechType] = this;
                SaveSystem.addSaveHandler(Info.ClassID, saveHandler);
                if (page != null)
                    TechnologyUnlockSystem.instance.registerPage(Info.TechType, page);
            }
        );

        SetGameObject(GetGameObject);
        this.SetPdaGroupCategory(GroupForPDA, CategoryForPDA);
        this.SetRecipe(GetBlueprintRecipe());
    }

    public CustomMachine<M> addIngredient(ItemDef item, int amt) {
        return addIngredient(item.getTechType(), amt);
    }

    public CustomMachine<M> addIngredient(CustomPrefab item, int amt) {
        return addIngredient(new ModPrefabTechReference(item), amt);
    }

    public CustomMachine<M> addIngredient(TechType item, int amt) {
        return addIngredient(new TechTypeContainer(item), amt);
    }

    public CustomMachine<M> addIngredient(TechTypeReference item, int amt) {
        recipe.Add(new PlannedIngredient(item, amt));
        return this;
    }

    public virtual TechGroup GroupForPDA => isOutdoors() ? TechGroup.ExteriorModules : TechGroup.InteriorModules;

    public virtual TechCategory CategoryForPDA =>
        isOutdoors() ? TechCategory.ExteriorModule : TechCategory.InteriorModule;

    public virtual bool isOutdoors() {
        return false;
    }

    protected void initializeStorageContainer(StorageContainer con, int w, int h /*, string label = null*/) {
        con.storageRoot.ClassId = Info.ClassID.ToLowerInvariant() + "container";
        //if (string.IsNullOrEmpty(label))
        //	label = FriendlyName;
        //storageLabel = label;
        con.hoverText = "Use " + Info.PrefabFileName;
        con.storageLabel = Info.PrefabFileName.ToUpperInvariant();
        con.container.containerType = ItemsContainerType.Default;
        con.enabled = true;
        con.Resize(w, h);
    }

    public void addFragments(int needed, float scanTime = 5, params CustomPrefab[] fragments) {
        SNUtil.log(
            "Creating " + fragments.Length + " fragments for " + this + " from " + fragments.toDebugString(),
            ownerMod
        );
        foreach (var m in fragments) {
            var info = m.GetGadget<ScanningGadget>().ScannerEntryData;
            info.blueprint = Info.TechType;
            // m = GenUtil.getOrCreateFragment(this, info.template, m.objectModify);
            SNUtil.log("Registered fragment " + m.Info.ClassID, ownerMod);
        }

        SNUtil.addPDAEntry(
            fragments[0],
            scanTime,
            null,
            null,
            null,
            e => {
                e.blueprint = Info.TechType;
                e.destroyAfterScan = shouldDeleteFragments();
                e.isFragment = true;
                e.totalFragments = needed;
                e.key = GenUtil.getFragment(Info.TechType, 0).Info.TechType;
                if (page != null)
                    e.encyclopedia = page.id;
            }
        );
    }

    public void addPDAPage(string text, string pageHeader = null) {
        page = PDAManager.createPage(
            "ency_" + Info.ClassID,
            Info.PrefabFileName,
            text,
            isPowerGenerator() ? "Tech/Power" : "Tech/Habitats"
        );
        if (pageHeader != null)
            page.setHeaderImage(TextureManager.getTexture(SNUtil.tryGetModDLL(), "Textures/PDA/" + pageHeader));
        page.register();
        if (_registered)
            TechnologyUnlockSystem.instance.registerPage(Info.TechType, page);
    }

    public PDAManager.PDAPage getPDAPage() {
        return page;
    }

    protected virtual bool isPowerGenerator() {
        return false;
    }

    protected virtual bool shouldDeleteFragments() {
        return true;
    }

    public virtual bool UnlockedAtStart => true;

    //protected abstract OrientedBounds[] GetBounds { get; }

    public GameObject GetGameObject() {
        var world = ObjectUtil.getModPrefabBaseObject(this);
        var lgc = world.EnsureComponent<M>();
        lgc.prefab = this;
        var capacity = lgc.getBaseEnergyStorageCapacityBonus();
        if (capacity > 0) {
            var src = world.EnsureComponent<PowerSource>();
            src.power = 0;
            src.maxPower = capacity;
        }

        var ctr = world.EnsureComponent<Constructable>();
        ctr.techType = Info.TechType;
        ctr.allowedInBase = !isOutdoors();
        ctr.allowedInSub = !isOutdoors();
        ctr.allowedOnGround = true;
        ctr.allowedOutside = isOutdoors();
        ctr.allowedOnCeiling = false;
        ctr.allowedOnWall = false;
        ctr.rotationEnabled = true;
        ctr.surfaceType = VFXSurfaceTypes.metal;
        ctr.forceUpright = true;
        ctr.allowedOnConstructables = false;
        var lw = world.EnsureComponent<LargeWorldEntity>();
        lw.cellLevel = LargeWorldEntity.CellLevel.Medium;
        initializeMachine(world);
        world.SetActive(true);
        return world;
    }

    public string ClassID => Info.ClassID;

    public virtual bool isResource() {
        return false;
    }

    public virtual string getTextureFolder() {
        return "Machines";
    }

    public virtual Sprite getIcon() {
        return GetItemSprite();
    }

    public Assembly getOwnerMod() {
        return ownerMod;
    }

    public void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public virtual void initializeMachine(GameObject go) {
    }

    public sealed override string ToString() {
        return base.ToString() + " [" + Info.TechType + "] / " + Info.ClassID + " / " + Info.PrefabFileName;
    }

    protected virtual RecipeData GetBlueprintRecipe() {
        return new RecipeData {
            Ingredients = RecipeUtil.buildRecipeList(recipe),
            craftAmount = 1,
        };
    }

    protected virtual Sprite GetItemSprite() {
        return TextureManager.getSprite(ownerMod, "Textures/Items/" + ObjectUtil.formatFileName(this));
    }
}

internal class MachineSaveHandler : SaveSystem.SaveHandler {
    public override void save(PrefabIdentifier pi) {
        var lgc = pi.GetComponentInChildren<CustomMachineLogic>();
        if (lgc)
            lgc.save(data);
    }

    public override void load(PrefabIdentifier pi) {
        var lgc = pi.GetComponentInChildren<CustomMachineLogic>();
        if (lgc)
            lgc.load(data);
    }
}

public abstract class DiscreteOperationalMachineLogic : CustomMachineLogic {
    public abstract bool isWorking();

    public abstract float getProgressScalar();

    public abstract string getErrorHover();
}

public abstract class CustomMachineLogic : MonoBehaviour {
    public static bool logPowerConsume = false;

    public static float powerCostFactor = 1;

    public static event Action<CustomMachinePowerCostFactorCheck> getMachinePowerCostFactorEvent;

    public CustomPrefab prefab { get; internal set; }
    public Constructable buildable { get; private set; }
    public SubRoot sub { get; private set; }
    public StorageContainer storage { get; private set; }

    public float lastTickDelta { get; private set; }

    private float lastUpdateTime = -1;
    private float lastDayTime = -1;

    protected float powerConsumedLastAttempt;

    internal Renderer[] mainRenderers { get; private set; }

    private float lastColorChange = -1;
    private float colorCooldown = -1;
    private Color emissiveColor;

    private float spawnTime = -1;

    private void Start() {
        setupSky();
    }

    //For things which can be built away from a base and should not count as part of it, eg a remote power source. Do note this breaks base-related functions like "get other pieces" and "consume power"
    protected virtual bool needsAttachedBase() {
        return true;
    }

    protected virtual float getTickRate() {
        return 0;
    }

    public virtual float getBaseEnergyStorageCapacityBonus() {
        return 0;
    }

    protected internal virtual void load(XmlElement data) {
        //spawnTime = (float)data.getFloat("spawnTime", float.NaN);
    }

    protected internal virtual void save(XmlElement data) {
        //data.addProperty("spawnTime", spawnTime);
    }

    protected void setupSky() {
        if (prefab == null || !WaterBiomeManager.main || !MarmoSkies.main)
            return;
        var baseSky = isOutdoors()
            ? WaterBiomeManager.main.GetBiomeEnvironment(transform.position)
            : MarmoSkies.main.skyBaseInterior;
        if (!baseSky)
            return;
        var skies = gameObject.GetComponentsInChildren<SkyApplier>(true);
        foreach (var sk in skies) {
            if (!sk)
                continue;
            sk.renderers = gameObject.GetComponentsInChildren<Renderer>();
            gameObject.setSky(baseSky);
        }
    }

    protected virtual bool isOutdoors() {
        return prefab.GetGadget<ScanningGadget>().CategoryForPda == TechCategory.ExteriorModule;
    }

    protected virtual Renderer[] findRenderers() {
        return GetComponentsInChildren<Renderer>();
    }

    private void Update() {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (prefab == null)
            tryGetPrefab();
        if (!buildable)
            buildable = GetComponent<Constructable>();
        if (mainRenderers == null)
            mainRenderers = findRenderers();
        if (spawnTime <= 0)
            spawnTime = time;
        lastTickDelta = time - lastUpdateTime;
        if (lastTickDelta > 0 && lastTickDelta >= getTickRate()) {
            updateEntity(lastTickDelta);
            lastUpdateTime = time;
        }

        if (time - lastDayTime >= 5)
            setupSky();
        lastDayTime = time;
        if (!storage) {
            storage = gameObject.GetComponentInChildren<StorageContainer>();
            if (storage)
                initStorage(storage);
        }

        if (needsAttachedBase()) {
            var par = transform.parent;
            if (!par || !par.GetComponent<SubRoot>()) {
                findClosestSub();
            }

            if (!sub) {
                sub = gameObject.GetComponentInParent<SubRoot>();
                if (!sub) {
                    findClosestSub();
                }
            }
        } /*
        if (storage && this is DiscreteOperationalMachineLogic lgc) {
            string err = lgc.getErrorHover();
            //storage.hoverText = string.IsNullOrEmpty(err) ? "Use "+prefab.FriendlyName : err;
            //storage.enabled = string.IsNullOrEmpty(err);
        }*/
    }

    private void tryGetPrefab() {
        var tt = CraftData.GetTechType(gameObject);
        if (tt != TechType.None && DIMod.machineList.ContainsKey(tt)) {
            prefab = DIMod.machineList[tt];
        }
    }

    protected float getAge() {
        return DayNightCycle.main.timePassedAsFloat - spawnTime;
    }

    protected virtual void initStorage(StorageContainer sc) {
        sc.preventDeconstructionIfNotEmpty = true;
        sc.errorSound = null;
        sc.container.errorSound = null;
    }

    protected int addItemToInventory(TechType tt, int amt = 1) {
        if (!storage)
            return 0;
        var add = 0;
        for (var i = 0; i < amt; i++) {
            var item = ObjectUtil.createWorldObject(CraftData.GetClassIdForTechType(tt), true, false);
            SNUtil.log("Adding " + item + " to " + GetType().Name + " inventory");
            item.SetActive(false);
            if (storage.container.AddItem(item.GetComponent<Pickupable>()) != null)
                add++;
        }

        return add;
    }

    protected bool consumePower(float amt) {
        if (logPowerConsume)
            SNUtil.log(
                this + " attempting to draw " + amt + " power (==" + (amt / lastTickDelta).ToString("0.00") +
                "/s) from " + sub
            );
        if (!buildable || !buildable.constructed)
            return false;
        if (!sub)
            return false;
        if (!GameModeUtils.RequiresPower())
            return true;
        if (amt > 0) {
            var f = getPowerCostFactor();
            amt *= f;
            sub.powerRelay.ConsumeEnergy(amt, out powerConsumedLastAttempt);
            if (logPowerConsume)
                SNUtil.log(
                    "Power cost multiplied by " + f + ", and was attempted; " + powerConsumedLastAttempt +
                    " was drained"
                );
            if (amt - powerConsumedLastAttempt > 0.001) {
                if (logPowerConsume)
                    SNUtil.log("Refunding " + powerConsumedLastAttempt + " power which was less than requested " + amt);
                sub.powerRelay.AddEnergy(powerConsumedLastAttempt, out var trash); //refund
            } else {
                if (logPowerConsume)
                    SNUtil.log("Power drain successful");
                return true;
            }
        }

        return false;
    }

    private float getPowerCostFactor() {
        var ch = new CustomMachinePowerCostFactorCheck(this);
        getMachinePowerCostFactorEvent?.Invoke(ch);
        return powerCostFactor * ch.value;
    }

    private void findClosestSub() {
        if (!needsAttachedBase())
            return;
        SNUtil.log(
            "Custom machine " + this + " @ " + transform.position +
            " did not have proper parent component hierarchy: " + transform.parent,
            SNUtil.diDLL
        );
        foreach (var s in FindObjectsOfType<SubRoot>()) {
            if (s.isCyclops || !s.isBase)
                continue;
            var dist = Vector3.Distance(s.transform.position, transform.position);
            if (dist > 350)
                continue;
            if (!sub || dist < Vector3.Distance(sub.transform.position, transform.position)) {
                sub = s;
            }
        }

        if (sub) {
            transform.parent = sub.transform;
            onAttachToBase();
            SNUtil.log(
                "Custom machine " + this + " @ " + transform.position + " parented to sub: " + sub,
                SNUtil.diDLL
            );
        }

        foreach (var sky in gameObject.GetComponents<SkyApplier>()) {
            sky.renderers = gameObject.GetComponentsInChildren<Renderer>();
            sky.enabled = true;
            sky.RefreshDirtySky();
            sky.ApplySkybox();
        }
    }

    protected void setEmissiveColor(Color c, int matIdx = 0, float cooldown = -1) {
        if (mainRenderers == null)
            return;
        foreach (var r in mainRenderers)
            setEmissiveColor(r, c, matIdx, cooldown);
    }

    protected void setEmissiveColor(Renderer r, Color c, int matIdx = 0, float cooldown = -1) {
        if (!r)
            return;
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time - lastColorChange < colorCooldown && cooldown < colorCooldown)
            return;
        emissiveColor = c;
        colorCooldown = cooldown;
        var m = r.materials[matIdx];

        m.EnableKeyword("MARMO_EMISSION");
        if (RenderUtil.getEmissivity(m) < 0.1F)
            RenderUtil.setEmissivity(m, 1);
        m.SetColor("_GlowColor", emissiveColor);
        lastColorChange = time;
    }

    protected void resetEmissiveCooldown() {
        colorCooldown = -1;
    }

    protected abstract void updateEntity(float seconds);

    protected virtual void onAttachToBase() {
    }

    public virtual void onConstructedChanged(bool finished) {
    }
}

public class CustomMachinePowerCostFactorCheck {
    public readonly CustomMachineLogic machine;
    public float value = 1;

    internal CustomMachinePowerCostFactorCheck(CustomMachineLogic lgc) {
        machine = lgc;
    }
}