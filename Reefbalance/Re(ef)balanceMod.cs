using System; //For data read/write methods
using System.Collections.Generic; //Working with Lists and Collections
//For data read/write methods
//More advanced manipulation of lists/collections
using System.Reflection;
using BepInEx;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine; //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.Reefbalance;

[BepInPlugin(ModKey, "Reefbalance", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
[BepInDependency(DIMod.MOD_KEY)]
public class ReefbalanceMod : BaseUnityPlugin {
    public const string ModKey = "ReikaKalseki.Reefbalance";

    public static readonly Assembly ModDLL = Assembly.GetExecutingAssembly();

    public static readonly Config<RBConfig.ConfigEntries> ModConfig = new(ModDLL);

    private static readonly TechType[] Decoratives = [ //does not include aurora as that has no techtype
        TechType.ToyCar,
        TechType.LabEquipment1,
        TechType.LabEquipment2,
        TechType.LabEquipment3,
        TechType.LabContainer,
    ];

    private static readonly TechType[] SmallSeeds = [
        TechType.BluePalmSeed,
        TechType.EyesPlantSeed,
        TechType.KooshChunk,
        TechType.GabeSFeatherSeed,
        TechType.MembrainTreeSeed,
        TechType.PurpleStalkSeed,
        TechType.RedBushSeed,
        TechType.RedRollPlantSeed,
        TechType.RedConePlantSeed,
    ];

    //private static readonly HashSet<TechType> meatFoods = new HashSet<TechType>();
    //private static readonly HashSet<TechType> vegFoods = new HashSet<TechType>();
    private static readonly Dictionary<TechType, int> ScanCountOverrides = new();

    public static BasicCraftingItem BaseGlass;

    public static event Action<Dictionary<TechType, int>> ScanCountOverridesCalculation;

    public void Start() {
        ModConfig.load();
    }

    public void Awake() {
        var harmony = new HarmonySystem(ModKey, ModDLL, typeof(RBPatches));
        harmony.apply();

        ModVersionCheck.getFromGitVsInstall("Re(ef)Balance", ModDLL, "Reefbalance").register();

        DIHooks.OnFruitPlantTickEvent += fpt => {
            var fp = fpt.GetPlant();
            if (CraftData.GetTechType(fp.gameObject) == TechType.HangingFruitTree)
                fp.fruitSpawnInterval = fpt.GetBaseGrowthTime() / ModConfig.getFloat(RBConfig.ConfigEntries.LANTERN_SPEED);
        };

        DIHooks.OnSkyApplierSpawnEvent += (sk) => {
            if (sk.name.StartsWith("Seamoth", StringComparison.InvariantCultureIgnoreCase) && sk.name.EndsWith(
                    "Arm(Clone)",
                    StringComparison.InvariantCultureIgnoreCase
                ))
                return;
            if (sk.gameObject.isDragonRepellent()) {
                sk.gameObject.EnsureComponent<ContainmentFacilityDragonRepellent>();
                return;
            }

            if (ModConfig.getBoolean(RBConfig.ConfigEntries.LARGE_KYANITE_DROPS) && sk.gameObject) {
                var d = sk.gameObject.FindAncestor<Drillable>();
                if (d && d.resources != null && d.resources.Length == 1 &&
                    d.resources[0].techType == TechType.Kyanite) {
                    var drs = ObjectUtil.lookupPrefab(VanillaResources.LARGE_DIAMOND.prefab)
                        .GetComponent<Drillable>();
                    d.resources[0].chance = drs.resources[0].chance;
                    // d.kChanceToSpawnResources = drs.kChanceToSpawnResources;
                    d.minResourcesToSpawn = drs.minResourcesToSpawn;
                    d.maxResourcesToSpawn = drs.maxResourcesToSpawn;
                }
            }
        };

        DIHooks.KnifeHarvestEvent += h => {
            if (h.Drops.Count > 0 && ModConfig.getBoolean(RBConfig.ConfigEntries.DOUBLE_THERMAL_CORAL) &&
                Inventory.main.GetHeld().GetTechType() == TechType.HeatBlade) {
                if (h.ObjectType == TechType.BigCoralTubes)
                    h.Drops[h.DefaultDrop] *= 2;
                else if (h.ObjectType == TechType.TreeMushroom)
                    h.Drops[h.DefaultDrop] *= 2;
                else if (h.ObjectType == TechType.RedTipRockThings)
                    h.Drops[TechType.CoralChunk] *= 2; //Ecocean
            }
        };

        if (ModConfig.getBoolean(RBConfig.ConfigEntries.REINF_GLASS)) {
            BaseGlass = new BasicCraftingItem(
                "BaseGlass",
                "Reinforced Glass",
                "Laminated glass with titanium reinforcement, suitable for underwater pressure vessels.",
                "WorldEntities/Natural/Glass"
            ) {
                craftingSubCategory = TechCategory.BasicMaterials,
                craftingTime = 1.5F,
                numberCrafted = 2,
                unlockRequirement = TechType.Unobtanium,
            };
            BaseGlass.addIngredient(TechType.Glass, 1).addIngredient(TechType.Titanium, 1);
            BaseGlass.sprite = TextureManager.getSprite(ModDLL, "Textures/Items/baseglass");
            BaseGlass.Register();

            HashSet<TechType> set = [TechType.Spotlight, TechType.Techlight];
            for (var tt = TechType.BaseRoom; tt <= TechType.BaseNuclearReactor; tt++) {
                set.Add(tt);
            }

            foreach (var tt in set) {
                if (RecipeUtil.recipeExists(tt)) {
                    var i = RecipeUtil.removeIngredient(tt, TechType.Glass);
                    if (i != null) {
                        RecipeUtil.addIngredient(tt, BaseGlass.Info.TechType, i.amount);
                    }
                }
            }

            TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Glass, BaseGlass.Info.TechType);

            RecipeUtil.removeIngredient(TechType.EnameledGlass, TechType.Glass);
            RecipeUtil.addIngredient(TechType.EnameledGlass, BaseGlass.Info.TechType, 1);
        }

        if (ModConfig.getBoolean(RBConfig.ConfigEntries.CHEAP_SEABASE)) {
            RecipeUtil.modifyIngredients(
                TechType.BaseRoom,
                i => {
                    if (i.techType == TechType.Titanium) i._amount = 4;
                    return false;
                }
            );
            RecipeUtil.modifyIngredients(
                TechType.BaseBulkhead,
                i => {
                    if (i.techType == TechType.Titanium) i._amount = 2;
                    return false;
                }
            );
            RecipeUtil.modifyIngredients(
                TechType.PlanterBox,
                i => {
                    if (i.techType == TechType.Titanium) i._amount = 3;
                    return false;
                }
            );
            RecipeUtil.modifyIngredients(
                TechType.BaseWaterPark,
                i => {
                    if (i.techType == TechType.Titanium) i._amount = 1;
                    return false;
                }
            );
        }

        RecipeUtil.addIngredient(TechType.BasePlanter, TechType.CreepvinePiece, 1);

        AdjustItemSizes();
        if (ModConfig.getBoolean(RBConfig.ConfigEntries.CHEAP_GLASS)) {
            RecipeUtil.modifyIngredients(
                TechType.Glass,
                i => {
                    if (i.techType == TechType.Quartz) i._amount = 1;
                    return false;
                }
            );
        }

        if (ModConfig.getBoolean(RBConfig.ConfigEntries.CHEAP_HUDCHIP)) {
            RecipeUtil.modifyIngredients(TechType.MapRoomHUDChip, i => i.techType == TechType.Magnetite);
            RecipeUtil.addIngredient(TechType.MapRoomHUDChip, TechType.Diamond, 1);
        }
        //cacheFoodTypes();

        RecipeUtil.getRecipe(TechType.LEDLight).craftAmount = 3;

        // PostLoad
        TechTypeMappingConfig<int>.loadInline(
            "fragment_scan_requirements",
            TechTypeMappingConfig<int>.IntParser.instance,
            TechTypeMappingConfig<int>.dictionaryAssign(ScanCountOverrides)
        );

        ScanCountOverridesCalculation?.Invoke(ScanCountOverrides);

        foreach (var kvp in ScanCountOverrides) {
            PDAHandler.EditFragmentsToScan(kvp.Key, kvp.Value);
            SNUtil.Log("Setting fragment scan requirement: " + kvp.Key + " = " + kvp.Value);
        }

        var uran = ModConfig.getInt(RBConfig.ConfigEntries.URANPERROD);
        if (uran != (int)ModConfig.getEntry(RBConfig.ConfigEntries.URANPERROD).vanillaValue) {
            RecipeUtil.modifyIngredients(
                TechType.ReactorRod,
                i => {
                    if (i.techType == TechType.UraniniteCrystal) i._amount = uran;
                    return false;
                }
            );

            var refuel = SNUtil.GetTechType("ReplenishReactorRod");
            if (refuel != TechType.None) {
                RecipeUtil.modifyIngredients(
                    refuel,
                    i => {
                        if (i.techType == TechType.UraniniteCrystal) i._amount = uran;
                        return false;
                    }
                );
            }
        }
    }

    public static int GetScanCountOverride(TechType tt) {
        return ScanCountOverrides.ContainsKey(tt) ? ScanCountOverrides[tt] : -1;
    }
    /*
    private static void cacheFoodTypes() {
        meatFoods.Add(TechType.CookedReginald);
        meatFoods.Add(TechType.CookedBladderfish);
        meatFoods.Add(TechType.CookedEyeye);
        meatFoods.Add(TechType.CookedGarryFish);
        meatFoods.Add(TechType.CookedHoleFish);
        meatFoods.Add(TechType.CookedHoopfish);
        meatFoods.Add(TechType.CookedHoverfish);
        meatFoods.Add(TechType.CookedLavaBoomerang);
        meatFoods.Add(TechType.CookedLavaEyeye);
        meatFoods.Add(TechType.CookedOculus);
        meatFoods.Add(TechType.CookedPeeper);
        meatFoods.Add(TechType.CookedSpadefish);
        meatFoods.Add(TechType.CookedSpinefish);

        meatFoods.Add(TechType.CuredReginald);
        meatFoods.Add(TechType.CuredBladderfish);
        meatFoods.Add(TechType.CuredEyeye);
        meatFoods.Add(TechType.CuredGarryFish);
        meatFoods.Add(TechType.CuredHoleFish);
        meatFoods.Add(TechType.CuredHoopfish);
        meatFoods.Add(TechType.CuredHoverfish);
        meatFoods.Add(TechType.CuredLavaBoomerang);
        meatFoods.Add(TechType.CuredLavaEyeye);
        meatFoods.Add(TechType.CuredOculus);
        meatFoods.Add(TechType.CuredPeeper);
        meatFoods.Add(TechType.CuredSpadefish);
        meatFoods.Add(TechType.CuredSpinefish);

        meatFoods.Add(TechType.HoleFish);
        meatFoods.Add(TechType.Jumper);
        meatFoods.Add(TechType.Peeper);
        meatFoods.Add(TechType.Oculus);
        meatFoods.Add(TechType.GarryFish);
        //meatFoods.Add(TechType.Slime); //what is this?
        meatFoods.Add(TechType.Boomerang);
        meatFoods.Add(TechType.Eyeye);
        meatFoods.Add(TechType.Bladderfish);
        meatFoods.Add(TechType.Hoverfish);
        meatFoods.Add(TechType.Reginald);
        meatFoods.Add(TechType.Spadefish);
        //meatFoods.Add(TechType.Floater);

        vegFoods.Add(TechType.CreepvinePiece);
        vegFoods.Add(TechType.Melon);
        vegFoods.Add(TechType.MelonSeed);
        vegFoods.Add(TechType.SmallMelon);
        vegFoods.Add(TechType.BulboTreePiece);
        vegFoods.Add(TechType.HangingFruit);
        vegFoods.Add(TechType.PurpleVegetable);
        vegFoods.Add(TechType.KooshChunk);
    }*/

    private static void AdjustItemSizes() {
        if (ModConfig.getBoolean(RBConfig.ConfigEntries.COMPACT_KELP))
            CraftDataHandler.SetItemSize(TechType.CreepvinePiece, new Vector2int(1, 2)); //1 wide 2 high

        if (ModConfig.getBoolean(RBConfig.ConfigEntries.SMALL_TOOLS)) {
            CraftDataHandler.SetItemSize(TechType.PropulsionCannon, new Vector2int(2, 1)); //2 wide 1 high     
            CraftDataHandler.SetItemSize(TechType.RepulsionCannon, new Vector2int(2, 1));
            CraftDataHandler.SetItemSize(TechType.Seaglide, new Vector2int(2, 2));
        }

        if (ModConfig.getBoolean(RBConfig.ConfigEntries.COMPACT_DECO)) {
            foreach (var deco in Decoratives) {
                CraftDataHandler.SetItemSize(deco, new Vector2int(1, 1));
            }
        }

        if (ModConfig.getBoolean(RBConfig.ConfigEntries.COMPACT_SEEDS)) {
            for (var i = (int)TechType.TreeMushroomPiece; i < (int)(object)TechType.HangingFruit - 1; i++) {
                if (Enum.IsDefined(typeof(TechType), i)) {
                    var item = (TechType)i;
                    //if (item == TechType.MelonSeed || item == TechType.HangingFruit)
                    //	continue;
                    CraftDataHandler.SetItemSize(item, new Vector2int(1, 1));
                }
            }

            for (var i = (int)TechType.MelonSeed + 1; i < (int)(object)TechType.SnakeMushroomSpore; i++) {
                if (Enum.IsDefined(typeof(TechType), i))
                    CraftDataHandler.SetItemSize((TechType)i, new Vector2int(1, 1));
            }
            //CraftDataHandler.SetItemSize(TechType.JellyPlantSeed, new Vector2int(1, 1));
        }

        //CraftDataHandler.SetItemSize(TechType.Shocker, new Vector2int(1, 3));//2,4
    }

    public static float GetFoodValue(Eatable e, float baseVal) {
        var ret = baseVal;
        if (e.decomposes) {
            var ce = GetFoodType(e);
            var elapsed = Mathf.Max(
                0,
                DayNightCycle.main.timePassedAsFloat - e.timeDecayStart - 1200 * ModConfig.getFloat(ce)
            ); //1 day = 1200 float units
            if (elapsed > 0)
                ret = Mathf.Max(baseVal - elapsed * e.kDecayRate, -25f);
        }

        return ret;
    }

    private static RBConfig.ConfigEntries GetFoodType(Eatable e) {
        var id = CraftData.GetTechType(e.gameObject);
        var cat = id.getFoodCategory();
        switch (cat) {
            case FoodCategory.PLANT:
                return RBConfig.ConfigEntries.FOOD_DELAY_VEG;
            case FoodCategory.RAWMEAT:
            case FoodCategory.EDIBLEMEAT:
                return RBConfig.ConfigEntries.FOOD_DELAY_MEAT;
            default:
                return RBConfig.ConfigEntries.FOOD_DELAY;
        }
    }

    public static void InitializeSeamothStorage(SeamothStorageContainer sc) {
        sc.width = 6;
        sc.height = 5;
    }

    public static void CalculatePrawnStorage(Exosuit s) {
        var height = 4 + s.modules.GetCount(TechType.VehicleStorageModule);
        s.storageContainer.Resize(8, 2 * height);
    }

    public static float GetDrillingSpeed(Drillable dr, Exosuit s) {
        if (!s) //eg seamoth arm
            return 1;
        s.energyInterface.GetValues(out var charge, out var capacity);
        var f = Math.Sqrt(charge / capacity);
        var sp = (float)(5 * MathUtil.linterpolate(capacity, 400, 2000, 1, 3, true));
        //SNUtil.writeToChat(charge+"/"+capacity+" ("+f+") > "+sp);
        return sp;
    }

    public static bool CanBuildingDestroyObject(GameObject go) {
        return !ModConfig.getBoolean(RBConfig.ConfigEntries.NO_BUILDER_CLEAR) && Builder.CanDestroyObject(go);
    }

    public static bool DeleteDuplicateDatabox(TechType tt) {
        return !ModConfig.getBoolean(RBConfig.ConfigEntries.ALWAYS_SPAWN_DB) && KnownTech.Contains(tt);
    }

    private class ContainmentFacilityDragonRepellent : MonoBehaviour {
        private void Update() {
            float r = 120;
            if (Player.main.transform.position.y <= 1350 &&
                Vector3.Distance(transform.position, Player.main.transform.position) <= 100) {
                var hit = Physics.SphereCastAll(gameObject.transform.position, r, new Vector3(1, 1, 1), r);
                foreach (var rh in hit) {
                    if (rh.transform != null && rh.transform.gameObject) {
                        var c = rh.transform.gameObject.GetComponent<SeaDragon>();
                        if (c) {
                            var vec = transform.position +
                                      (c.transform.position - transform.position).normalized * 120;
                            c.GetComponent<SwimBehaviour>().SwimTo(vec, 20);
                        }
                    }
                }
            }
        }
    }
}