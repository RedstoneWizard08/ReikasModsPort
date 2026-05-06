using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class GenUtil {
    public static readonly Bounds allowableGenBounds = MathUtil.getBounds(-2299, -3100, -2299, 2299, 150, 2299);

    private static readonly HashSet<string> alreadyRegisteredGen = [];
    private static readonly Dictionary<TechType, Databox> databoxes = new();
    private static readonly Dictionary<TechType, Crate>[] crates = new Dictionary<TechType, Crate>[2];

    private static readonly Dictionary<TechType, FragmentGroup> fragments = new();

    private static readonly Dictionary<LargeWorldEntity.CellLevel, WorldGeneratorPrefab> worldGeneratorPrefabs = new();

    private static readonly Dictionary<string, WorldGenerator> generatorTable = new();

    static GenUtil() {
        crates[0] = new Dictionary<TechType, Crate>();
        crates[1] = new Dictionary<TechType, Crate>();
    }

    public static void registerOreWorldgen(BasicCustomOre ore, BiomeType biome, int amt, float chance) {
        registerPrefabWorldgen(ore, ore.isLargeResource, biome, amt, chance);
    }

    public static void registerPlantWorldgen(BasicCustomPlant ore, BiomeType biome, int amt, float chance) {
        registerPrefabWorldgen(ore, ore.getSize() == Plantable.PlantSize.Large, biome, amt, chance);
    }

    public static void registerPrefabWorldgen(CustomPrefab sp, bool large, BiomeType biome, int amt, float chance) {
        registerPrefabWorldgen(
            sp,
            large ? EntitySlot.Type.Medium : EntitySlot.Type.Small,
            large ? LargeWorldEntity.CellLevel.Medium : LargeWorldEntity.CellLevel.Near,
            biome,
            amt,
            chance
        );
    }

    public static void registerPrefabWorldgen(
        CustomPrefab sp,
        EntitySlot.Type type,
        LargeWorldEntity.CellLevel size,
        BiomeType biome,
        int amt,
        float chance
    ) {
        registerSlotWorldgen(sp.Info.ClassID, sp.Info.PrefabFileName, sp.Info.TechType, type, size, biome, amt, chance);
    }

    public static void registerSlotWorldgen(
        string id,
        string file,
        TechType tech,
        EntitySlot.Type type,
        LargeWorldEntity.CellLevel size,
        BiomeType biome,
        int amt,
        float chance
    ) {
        if (alreadyRegisteredGen.Contains(id)) {
            LootDistributionHandler.EditLootDistributionData(id, biome, chance, amt); //will add if not present
        } else {
            var b = new LootDistributionData.BiomeData
                { biome = biome, count = amt, probability = chance };
            List<LootDistributionData.BiomeData> li = [b];
            var info = new UWE.WorldEntityInfo {
                cellLevel = size,
                classId = id,
                localScale = Vector3.one,
                slotType = type,
                techType = tech,
            };
            WorldEntityDatabaseHandler.AddCustomInfo(id, info);
            LootDistributionHandler.AddLootDistributionData(id, file, li, info);

            alreadyRegisteredGen.Add(id);
        }
    }

    public static SpawnInfo registerWorldgen(PositionedPrefab pfb, Action<GameObject> call = null) {
        return registerWorldgen(
            pfb.prefabName,
            pfb.position,
            pfb.rotation,
            go => {
                if (!Mathf.Approximately(pfb.scale.x, 1) || !Mathf.Approximately(pfb.scale.y, 1) ||
                    !Mathf.Approximately(pfb.scale.z, 1))
                    go.transform.localScale = pfb.scale;
                call?.Invoke(go);
            }
        );
    }

    public static SpawnInfo registerWorldgen(
        string prefab,
        Vector3 pos,
        Vector3? rot = null,
        Action<GameObject> call = null
    ) {
        return registerWorldgen(prefab, pos, Quaternion.Euler(getOrZero(rot)), call);
    }

    public static SpawnInfo registerWorldgen(
        string prefab,
        Vector3 pos,
        Quaternion? rot = null,
        Action<GameObject> call = null
    ) {
        if (string.IsNullOrEmpty(prefab))
            throw new Exception("Tried to register worldgen of null!");
        validateCoords(pos);
        var info = new SpawnInfo(prefab, pos, getOrIdentity(rot), Vector3.one, call);
        CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(info);
        //SNUtil.log("Registering prefab "+prefab+" @ "+pos);
        return info;
    }

    public static SpawnInfo registerWorldgen(WorldGenerator gen) {
        if (gen == null)
            throw new Exception("You cannot register a null gen!");
        validateCoords(gen.position);
        Action<GameObject> call = go => {
            var id = gen.uniqueID;
            SNUtil.Log("Placing world generator " + gen + " [" + id + "]");
            generatorTable[id] = gen;
            go.EnsureComponent<WorldGeneratorHolder>().generatorID = id;
        };
        var info = new SpawnInfo(
            getOrCreateWorldgenHolder(gen).Info.ClassID,
            gen.position,
            Quaternion.identity,
            Vector3.one,
            call
        );
        CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(info);
        SNUtil.Log("Queuing world generator " + gen);
        return info;
    }

    private static WorldGeneratorPrefab getOrCreateWorldgenHolder(WorldGenerator gen) {
        var lvl = gen.getCellLevel();
        if (!worldGeneratorPrefabs.ContainsKey(lvl)) {
            worldGeneratorPrefabs[lvl] = new WorldGeneratorPrefab(lvl);
        }

        return worldGeneratorPrefabs[lvl];
    }

    public static SpawnInfo spawnDatabox(Vector3 pos, TechType tech, Vector3? rot = null) {
        return spawnDatabox(pos, tech, Quaternion.Euler(getOrZero(rot)));
    }

    public static SpawnInfo spawnDatabox(Vector3 pos, TechType tech, Quaternion? rot = null) {
        return registerWorldgen(getOrCreateDatabox(tech).ClassID, pos, rot);
    }

    public static SpawnInfo spawnPDA(Vector3 pos, PDAManager.PDAPage page, Vector3? rot = null) {
        return spawnPDA(pos, page, Quaternion.Euler(getOrZero(rot)));
    }

    public static SpawnInfo spawnPDA(Vector3 pos, PDAManager.PDAPage page, Quaternion? rot = null) {
        return registerWorldgen(page.getPDAClassID(), pos, rot);
    }

    public static SpawnInfo spawnFragment(Vector3 pos, Fragment mf, Quaternion? rot = null) {
        return registerWorldgen(mf.ClassID, pos, rot);
    }

    public static SpawnInfo spawnFragment(Vector3 pos, CustomPrefab item, string template, Quaternion? rot = null) {
        return registerWorldgen(getOrCreateFragment(item, template).ClassID, pos, rot);
    }

    public static SpawnInfo spawnResource(VanillaResources res, Vector3 pos, Vector3? rot = null) {
        return registerWorldgen(res.prefab, pos, rot);
    }

    public static SpawnInfo spawnTechType(TechType tech, Vector3 pos, Vector3? rot = null) {
        validateCoords(pos);
        var info = new SpawnInfo(tech, pos, getOrZero(rot));
        CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(info);
        return info;
    }

    public static Vector3 getOrZero(Vector3? init) {
        return init != null && init.HasValue ? init.Value : Vector3.zero;
    }

    public static Quaternion getOrIdentity(Quaternion? init) {
        return init != null && init.HasValue ? init.Value : Quaternion.identity;
    }

    private static void validateCoords(Vector3 pos) {
        if (!allowableGenBounds.Contains(pos))
            throw new Exception(
                "Registered worldgen is out of bounds @ " + pos + "; allowable range is " + allowableGenBounds.min +
                " > " + allowableGenBounds.max
            );
    }

    public static CustomPrefab getOrCreateCrate(TechType tech, bool needsCutter = false, string goal = null) {
        var idx = needsCutter ? 1 : 0;
        var box = crates[idx].ContainsKey(tech) ? crates[idx][tech] : null;
        if (box == null) {
            box = new Crate(tech, needsCutter, goal);
            crates[idx][tech] = box;
        }

        return box;
    }

    public static ContainerPrefab getOrCreateDatabox(TechType tech, Action<GameObject> modify = null) {
        var box = databoxes.ContainsKey(tech) ? databoxes[tech] : null;
        if (box == null) {
            box = new Databox(tech, "1b8e6f01-e5f0-4ab7-8ba9-b2b909ce68d6", modify); //compass databox
            databoxes[tech] = box;
        }

        return box;
    }

    public static ContainerPrefab getOrCreateFragment(
        CustomPrefab tech,
        string template,
        Action<GameObject> modify = null
    ) {
        return getOrCreateFragment(tech.Info.TechType, tech.Info.PrefabFileName, template, modify);
    }

    public static ContainerPrefab getOrCreateFragment(
        TechType tech,
        string name,
        string template,
        Action<GameObject> modify = null
    ) {
        var li = fragments.ContainsKey(tech) ? fragments[tech] : null;
        if (li == null) {
            li = new FragmentGroup();
            fragments[tech] = li;
        }

        var f = li.variants.ContainsKey(template) ? li.variants[template] : null;
        if (f == null) {
            f = li.addVariant(tech, name, template, modify);
        }

        return f;
    }

    public static ContainerPrefab getFragment(TechType tech, int idx) {
        var li = fragments.ContainsKey(tech) ? fragments[tech] : null;
        return li == null || li.variantList.Count == 0 ? null : (ContainerPrefab)li.variantList[idx];
    }

    public static bool fireGenerator(WorldGenerator gen, List<GameObject> generatedObjects) {
        WorldgenLog.log("Running world generator " + gen);
        if (gen.generate(generatedObjects)) {
            WorldgenLog.log("Generator " + gen + " complete. Generation list (" + generatedObjects.Count + "):");
            foreach (var go in generatedObjects)
                WorldgenLog.log(go);
            if (generatedObjects.Count == 0) {
                var msg = "Warning: Nothing generated!";
                WorldgenLog.log(msg);
            }

            return true;
        } else {
            SNUtil.Log("Generator " + gen + " failed, trying again in one second", SNUtil.DiDLL);
            return false;
        }
    }

    public abstract class CustomPrefabImpl : CustomPrefab, DIPrefab<StringPrefabContainer> {
        public float glowIntensity { get; set; }
        public StringPrefabContainer baseTemplate { get; set; }

        private readonly Assembly ownerMod;

        [SetsRequiredMembers]
        public CustomPrefabImpl(string name, string template, string display = "") : base(name, display, "") {
            baseTemplate = new StringPrefabContainer(template);

            ownerMod = SNUtil.TryGetModDLL();
            SetGameObject(GetGameObject);
        }

        public virtual GameObject GetGameObject() {
            return ObjectUtil.getModPrefabBaseObject(this);
        }

        public string ClassID => Info.ClassID;

        public virtual bool isResource() {
            return false;
        }

        public virtual string getTextureFolder() {
            return null;
        }

        public Sprite getIcon() {
            return null;
        }

        Sprite DIPrefab.getIcon() {
            return null;
        }

        public Assembly getOwnerMod() {
            return ownerMod;
        }

        public abstract void prepareGameObject(GameObject go, Renderer[] r);
    }

    public abstract class ContainerPrefab : CustomPrefabImpl {
        public readonly TechType containedTech;

        private readonly Action<GameObject> modify;

        [SetsRequiredMembers]
        internal ContainerPrefab(
            TechType tech,
            string template,
            Action<GameObject> m,
            string pre = "container",
            string suff = "",
            string disp = ""
        ) : base(pre + "_" + tech + suff, template, disp) {
            if (tech == TechType.None)
                throw new Exception("TechType for worldgen container " + GetType() + " was null!");
            containedTech = tech;
            modify = m;
        }

        internal void modifyObject(GameObject go) {
            modify?.Invoke(go);
        }
    }

    private class Databox : ContainerPrefab, Story.IStoryGoalListener {
        [SetsRequiredMembers]
        internal Databox(TechType tech, string template, Action<GameObject> modify) : base(tech, template, modify) {
        }

        public override void prepareGameObject(GameObject go, Renderer[] r) {
            Story.StoryGoalManager.main.AddListener(this);
            var bpt = go.EnsureComponent<BlueprintHandTarget>();
            bpt.unlockTechType = containedTech;
            bpt.primaryTooltip = containedTech.AsString();
            var arg = Language.main.Get(containedTech);
            var arg2 = Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(containedTech));
            bpt.secondaryTooltip = Language.main.GetFormat("DataboxToolipFormat", arg, arg2);
            bpt.alreadyUnlockedTooltip = Language.main.GetFormat(
                "DataboxAlreadyUnlockedToolipFormat",
                arg,
                arg2
            );
            //redundant with the goal//bpt.useSound = SNUtil.getSound("event:/tools/scanner/new_blueprint");
            bpt.onUseGoal = new Story.StoryGoal(bpt.primaryTooltip, Story.GoalType.Encyclopedia, 0);

            modifyObject(go);
        }

        public void NotifyGoalComplete(string key) {
            if (key == containedTech.AsString()) {
                SNUtil.TriggerTechPopup(containedTech);
                TechnologyUnlockSystem.instance.triggerDirectUnlock(containedTech, false);
            }
        }
    }

    public sealed class Fragment : ContainerPrefab {
        [SetsRequiredMembers]
        internal Fragment(TechType tech, string name, string template, Action<GameObject> modify, int index) : base(
            tech,
            template,
            modify,
            "fragment",
            "_" + index,
            name + " Fragment"
        ) {
            var frag = this.CreateFragment(tech, 1, index);
        }

        public override void prepareGameObject(GameObject go, Renderer[] r) { /*
            TechFragment bpt = go.EnsureComponent<TechFragment>();
            bpt.defaultTechType = containedTech;
            bpt.techList.Clear();
            bpt.techList.Add(new TechFragment.RandomTech{techType = containedTech, chance = 100});*/
            var tt = fragments[containedTech].sharedTechType; // NOT our techtype since needs to be shared!
            go.EnsureComponent<TechTag>().type = tt;
            var p = go.EnsureComponent<Pickupable>();
            p.overrideTechType = tt;
            var rt = go.EnsureComponent<ResourceTracker>();
            rt.techType = TechType.Fragment;
            rt.overrideTechType = TechType.Fragment;
            rt.prefabIdentifier = go.GetComponent<PrefabIdentifier>();
            rt.pickupable = p;
            p.isPickupable = false;
            modifyObject(go);
        }

        public override GameObject GetGameObject() {
            var go = base.GetGameObject();
            var tt = fragments[containedTech].sharedTechType; // NOT our techtype since needs to be shared!
            go.EnsureComponent<TechTag>().type = tt;
            return go;
        }
    }

    private class FragmentGroup {
        internal readonly Dictionary<string, Fragment> variants = new();
        internal readonly List<Fragment> variantList = [];

        internal TechType sharedTechType = TechType.None;

        public FragmentGroup() {
        }

        internal Fragment addVariant(TechType tech, string name, string template, Action<GameObject> modify) {
            var f = new Fragment(tech, name, template, modify, variantList.Count);
            variants[template] = f;
            variantList.Add(f);
            if (sharedTechType == TechType.None)
                sharedTechType = f.containedTech;
            return f;
        }
    }

    private class Crate : CustomPrefab {
        private readonly bool needsCutter;
        private readonly string storyGoal;
        private readonly TechType containedItem;

        [SetsRequiredMembers]
        internal Crate(TechType tech, bool c = false, string goal = null) : base(
            "Crate_" + tech.AsString() + "_" + c,
            "Supply Crate",
            ""
        ) {
            containedItem = tech;
            needsCutter = c;
            storyGoal = goal;

            //SNUtil.log("Creating Crate_"+tech.AsString()+"_"+c);
            //SNUtil.log(new System.Diagnostics.StackTrace().ToString());

            SetGameObject(GetGameObject);

            AddOnRegister(() => {
                    SaveSystem.addSaveHandler(
                        Info.ClassID,
                        new SaveSystem.ComponentFieldSaveHandler<CrateManagement>().addField("isOpened")
                            .addField("itemGrabbed")
                    );
                }
            );
        }

        public GameObject GetGameObject() {
            var pfb = ObjectUtil.lookupPrefab("580154dd-b2a3-4da1-be14-9a22e20385c8");
            var go = new GameObject(Info.ClassID + "(Clone)");
            go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
            go.EnsureComponent<TechTag>().type = Info.TechType;
            go.EnsureComponent<LargeWorldEntity>().cellLevel = pfb.GetComponent<LargeWorldEntity>().cellLevel;
            var a = pfb.GetComponentInChildren<Animation>();
            var mdl = a.gameObject.clone();
            mdl.transform.SetParent(go.transform);
            mdl.transform.localRotation = Quaternion.identity;
            mdl.transform.localPosition = new Vector3(0, 0.36F, 0.02F);
            mdl.transform.localScale = Vector3.one * 5.5F;
            var mgr = go.EnsureComponent<CrateManagement>();
            mgr.itemToSpawn = containedItem;
            mgr.collectionGoal = storyGoal;

            if (needsCutter) {
                go.EnsureComponent<Sealed>()._sealed = true;
            }

            return go;
        }
    }

    /*
    internal class EmptyCrate : Spawnable {

        internal EmptyCrate() : base("EmptyCrate", "Empty Crate", "") {

        }

        public override GameObject GetGameObject() {
            GameObject pfb = ObjectUtil.lookupPrefab("580154dd-b2a3-4da1-be14-9a22e20385c8");
            GameObject go = new GameObject("EmptyCrate(Clone)");
            go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
            go.EnsureComponent<TechTag>().type = TechType;
            go.EnsureComponent<LargeWorldEntity>().cellLevel = pfb.GetComponent<LargeWorldEntity>().cellLevel;
            Animation a = pfb.GetComponentInChildren<Animation>();
            GameObject mdl = a.gameObject.clone();
            mdl.transform.SetParent(go.transform);
            mdl.transform.localRotation = Quaternion.identity;
            mdl.transform.localPosition = new Vector3(0, 0.36F, 0.02F);
            mdl.transform.localScale = Vector3.one*5.5F;
            CrateManagement mgr = go.EnsureComponent<CrateManagement>();
            mgr.autoOpen = true;
            return go;
        }

    }
    */
    internal class CrateManagement : MonoBehaviour, IHandTarget {
        public bool isOpened;
        public bool itemGrabbed;
        public TechType itemToSpawn;

        private string openAnimation = "Open_SupplyCrate";
        private string openText = "Open_SupplyCrate";
        private string snapOpenAnimation;
        private FMODAsset openSound;

        public string collectionGoal;

        private Sealed laserSeal;

        private Pickupable itemInside;

        private void Start() {
            laserSeal = GetComponent<Sealed>();
            var sc = ObjectUtil.lookupPrefab("580154dd-b2a3-4da1-be14-9a22e20385c8")
                .GetComponent<SupplyCrate>();
            openText = sc.openText;
            openSound = sc.openSound;
            openAnimation = sc.openClipName;
            snapOpenAnimation = sc.snapOpenOnLoad;
            Invoke(nameof(delayedStart), 0.5F);
        }

        private void delayedStart() {
            if (isOpened && !GetComponentInChildren<Animation>().Play(snapOpenAnimation)) {
                Invoke(nameof(delayedStart), 0.5F);
                return;
            }

            if (itemToSpawn != TechType.None) {
                cacheItem();
                if (!itemInside && !itemGrabbed && (collectionGoal == null ||
                                                    !Story.StoryGoalManager.main.IsGoalComplete(collectionGoal))) {
                    itemInside = ObjectUtil.createWorldObject(itemToSpawn).GetComponent<Pickupable>();
                    SNUtil.Log("Filling crate @ " + transform.position + " with " + itemInside);
                    itemInside.transform.SetParent(transform);
                    itemInside.transform.localPosition = new Vector3(0, 0.33F, 0);
                    itemInside.transform.localRotation = Quaternion.identity;
                    itemInside.transform.localScale = Vector3.one;
                    itemInside.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        }

        private void Update() {
            if (itemInside)
                itemInside.isPickupable = isOpened;
            if (string.IsNullOrEmpty(collectionGoal))
                collectionGoal = "Crate_" + transform.position.ToString("0.0").Trim();
        }

        private void cacheItem() {
            itemInside = GetComponentInChildren<Pickupable>();
        }

        public void OnHandHover(GUIHand h) {
            cacheItem();
            var flag = false;
            if (!isOpened) {
                if (!laserSeal || !laserSeal.IsSealed()) {
                    HandReticle.main.SetText(HandReticle.TextType.Use, openText, true);
                } else {
                    HandReticle.main.SetText(HandReticle.TextType.Use, "Sealed_SupplyCrate", true);
                    HandReticle.main.SetText(HandReticle.TextType.UseSubscript, "SealedInstructions", true);
                }

                flag = true;
            } else if (itemInside) {
                HandReticle.main.SetText(HandReticle.TextType.Use, "TakeItem_SupplyCrate", true);
                flag = true;
            }

            if (flag) {
                HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            }
        }

        public void OnHandClick(GUIHand h) {
            cacheItem();
            if (!laserSeal || !laserSeal.IsSealed()) {
                if (!isOpened) {
                    isOpened = true;
                    Utils.PlayFMODAsset(openSound, transform, 20f);
                    var a = GetComponentInChildren<Animation>();
                    if (a) {
                        a.Play(openAnimation);
                    }

                    return;
                }

                if (itemInside) {
                    Inventory.main.Pickup(itemInside, false);
                    clear();
                }
            }
        }

        public void onPickup(Pickupable p) {
            if (p && p.GetTechType() == itemToSpawn) {
                clear();
            }
        }

        private void clear() {
            itemGrabbed = true;
            itemInside = null;
            if (collectionGoal != null)
                Story.StoryGoal.Execute(collectionGoal, Story.GoalType.Story);
        }
    }

    private class WorldGeneratorPrefab : CustomPrefab {
        public readonly LargeWorldEntity.CellLevel cellLevel;

        [SetsRequiredMembers]
        internal WorldGeneratorPrefab(LargeWorldEntity.CellLevel lvl) : base(
            "WorldGeneratorHolder_" + lvl.ToString(),
            "",
            ""
        ) {
            cellLevel = lvl;
            SetGameObject(GetGameObject);
        }

        public GameObject GetGameObject() {
            var go = new GameObject("WorldGeneratorHolder");
            go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
            go.EnsureComponent<TechTag>().type = Info.TechType;
            go.EnsureComponent<LargeWorldEntity>().cellLevel = cellLevel;
            go.EnsureComponent<WorldGeneratorHolder>();
            return go;
        }
    }

    private class WorldGeneratorHolder : MonoBehaviour {
        internal string generatorID;

        private WorldGenerator generatorInstance;

        private readonly List<GameObject> generatedObjects = [];

        internal bool generate() {
            generatorInstance = generatorTable.ContainsKey(generatorID) ? generatorTable[generatorID] : null;
            if (generatorInstance == null) {
                SNUtil.Log(
                    "WorldGen holder '" + generatorID + "' @ " + transform.position + " had no generator!",
                    SNUtil.DiDLL
                );
                return false;
            }

            var flag = fireGenerator(generatorInstance, generatedObjects);
            if (flag) {
                gameObject.destroy(false);
            }

            return flag;
        }

        private void Start() {
            Invoke(nameof(tryGenerate), 0);
        }

        private void tryGenerate() {
            if (!generate())
                Invoke(nameof(tryGenerate), 1F);
        }
    }
}