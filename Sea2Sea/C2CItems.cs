//For data read/write methods

using System.Collections.Generic; //Working with Lists and Collections
//For data read/write methods
using System.Linq; //More advanced manipulation of lists/collections
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine; //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.SeaToSea;

public static class C2CItems {
    public static SeamothVoidStealthModule voidStealth;
    public static CyclopsHeatModule cyclopsHeat;
    public static CyclopsStorageModule cyclopsStorage;
    public static SeamothDepthModule depth1300;
    public static SeamothPowerSealModule powerSeal;
    public static SeamothHeatSinkModule heatSinkModule;
    public static SeamothSpeedModule speedModule;
    public static VehicleLightModule lightModule;
    public static SeamothTetherModule tetherModule;

    public static SealedSuit sealSuit;
    public static SealedGloves sealGloves;
    public static AzuriteBattery t2Battery;

    public static RebreatherV2 rebreatherV2;
    public static LiquidTank liquidTank;
    //public static OxygeniteTank oxygeniteTank;

    public static ChargeFinRelay chargeFinRelay;

    public static BreathingFluid breathingFluid;
    public static SeamothHeatSink heatSink;
    public static CurativeBandage bandage;
    public static KharaaTreatment treatment;
    public static OxygeniteCharge oxygeniteCharge;

    public static AlkaliPlant alkali;
    public static VentKelp kelp;
    public static HealingFlower healFlower;
    public static MountainGlow mountainGlow;
    public static SanctuaryPlant sanctuaryPlant;

    public static TechType emperorRootCommon;

    public static readonly Dictionary<string, EmperorRoot> emperorRoots = new();
    //public static TechType postCoveTreeCommon;
    //public static readonly Dictionary<DecoPlants, PostCoveTree> postCoveTrees = new Dictionary<DecoPlants, PostCoveTree>();

    public static BrokenTablet brokenRedTablet;
    public static BrokenTablet brokenWhiteTablet;
    public static BrokenTablet brokenOrangeTablet;
    public static BrokenTablet brokenBlueTablet;

    public static DeepStalker deepStalker;
    public static SanctuaryJellyray sanctuaryray;
    public static PurpleHolefish purpleHolefish;
    public static PurpleBoomerang purpleBoomerang;
    public static PurpleHoopfish purpleHoopfish;

    public static VoltaicBladderfish voltaicBladderfish;

    //public static GiantRockGrub giantRockGrub;
    public static BloodKelpBroodmother broodmother;
    public static VoidSpikeLeviathan voidSpikeLevi;

    public static LargeOxygenite largeOxygenite;

    public static TechType brineCoral;
    public static WorldCollectedItem brineCoralPiece;
    public static EmperorRootOil emperorRootOil;

    public static WorldCollectedItem bkelpBumpWormItem;
    //public static WorldCollectedItem brineSalt;   
    //public static WorldCollectedItem wateryGel;   

    public static Bioprocessor processor;
    public static RebreatherRecharger rebreatherCharger;
    public static GeyserFilter geyserFilter;
    //public static IncubatorInjector incubatorInjector;

    public static TechCategory chemistryCategory;
    public static TechCategory ingotCategory;
    public static TechCategory precursorCategory;

    public static CraftTree.Type hatchingEnzymes;

    private static readonly Dictionary<TechType, IngotDefinition> ingots = new();

    private static readonly Dictionary<TechType, IngotDefinition> ingotsByUnpack = new();

    private static readonly Dictionary<TechType, TechType> brokenTablets = new();

    internal static void registerTabletTechKey(BrokenTablet tb) {
        brokenTablets[tb.Info.TechType] = tb.tablet;
        brokenTablets[tb.tablet] = tb.tablet;
    }
    /*
     public static bool hasNoGasMask() {
         return Inventory.main.equipment.GetCount(TechType.Rebreather) == 0 && Inventory.main.equipment.GetCount(rebreatherV2.TechType) == 0;
     }*/

    internal static void preAdd() {
        var e = SeaToSeaMod.MiscLocale.getEntry("CraftingNodes");
        chemistryCategory = EnumHandler.AddEntry<TechCategory>("C2Chemistry").WithPdaInfo(e.getString("chemistry"))
            .RegisterToTechGroup(TechGroup.Resources);
        CraftTreeHandler.AddTabNode(
            CraftTree.Type.Fabricator,
            "C2Chemistry",
            e.getString("chemistry"),
            TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/CraftTab/chemistry"),
            "Resources"
        );

        ingotCategory = EnumHandler.AddEntry<TechCategory>("C2CIngots").WithPdaInfo(e.getString("ingots"))
            .RegisterToTechGroup(TechGroup.Resources);
        CraftTreeHandler.AddTabNode(
            CraftTree.Type.Fabricator,
            "C2CIngots",
            e.getString("ingots"),
            TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/CraftTab/ingotmaking"),
            "Resources"
        );
        CraftTreeHandler.AddTabNode(
            CraftTree.Type.Fabricator,
            "C2CIngots2",
            e.getString("ingotUnpack"),
            TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/CraftTab/ingotbreaking"),
            "Resources"
        );

        CraftTreeHandler.AddTabNode(
            CraftTree.Type.Workbench,
            "C2CMedical",
            e.getString("medical"),
            TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/CraftTab/medical")
        );
        CraftTreeHandler.AddTabNode(
            CraftTree.Type.Workbench,
            "C2CHelmet",
            e.getString("helmet"),
            SpriteManager.Get(TechType.Rebreather)
        );
        CraftTreeHandler.AddTabNode(
            CraftTree.Type.Workbench,
            "C2CModElectronics",
            e.getString("modelectric"),
            TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/CraftTab/modelectronic")
        );

        precursorCategory = EnumHandler.AddEntry<TechCategory>("C2CPrecursor").WithPdaInfo(e.getString("precursor"))
            .RegisterToTechGroup(TechGroup.Resources);

        brokenRedTablet = new BrokenTablet(TechType.PrecursorKey_Red);
        brokenWhiteTablet = new BrokenTablet(TechType.PrecursorKey_White);
        brokenOrangeTablet = new BrokenTablet(TechType.PrecursorKey_Orange);
        brokenBlueTablet = new BrokenTablet(TechType.PrecursorKey_Blue);

        voidStealth = new SeamothVoidStealthModule();
        depth1300 = new SeamothDepthModule(
            "SMDepth4",
            "Seamoth Depth Module MK4",
            "Increases crush depth to 1300m.",
            1300
        );
        powerSeal = new SeamothPowerSealModule();
        heatSinkModule = new SeamothHeatSinkModule();
        speedModule = new SeamothSpeedModule();
        lightModule = new VehicleLightModule();
        tetherModule = new SeamothTetherModule();
        cyclopsHeat = new CyclopsHeatModule();
        cyclopsStorage = new CyclopsStorageModule();
        sealSuit = new SealedSuit();
        sealGloves = new SealedGloves();
        t2Battery = new AzuriteBattery();
        rebreatherV2 = new RebreatherV2();
        liquidTank = new LiquidTank();
        //oxygeniteTank = new OxygeniteTank();
        chargeFinRelay = new ChargeFinRelay();
        breathingFluid = new BreathingFluid();
        heatSink = new SeamothHeatSink();
        bandage = new CurativeBandage();
        treatment = new KharaaTreatment();
        oxygeniteCharge = new OxygeniteCharge();
        bkelpBumpWormItem = new WorldCollectedItem(
            SeaToSeaMod.ItemLocale.getEntry("BKelpBumpWormItem"),
            "WorldEntities/Natural/StalkerTooth"
        ) {
            sprite = TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/Items/BumpWormItem"),
            inventorySize = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE)
                ? new Vector2int(3, 2)
                : new Vector2int(2, 1),
            renderModify = (r) => {
                r.transform.localScale = new Vector3(8, 8, 6);
                r.materials[0].SetFloat("_Shininess", 0);
                r.materials[0].SetFloat("_SpecInt", 0.2F);
                r.materials[0].SetFloat("_Fresnel", 0F);
                RenderUtil.setEmissivity(r, 0.75F);
            },
        };
        bkelpBumpWormItem.Register();

        /*
        Color c = new Color(0.5F, 1.6F, 0.8F);
        brineSalt = new WorldCollectedItem(SeaToSeaMod.itemLocale.getEntry("BrineSalt"), "WorldEntities/Natural/salt");
        brineSalt.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/BrineSalt");
        brineSalt.renderModify = (r) => {
            r.materials[0].SetColor("_Color", c);
            r.materials[0].SetColor("_SpecColor", c);
        };
        brineSalt.Patch();

        c = new Color(0.5F, 0.8F, 1.6F);
        wateryGel = new WorldCollectedItem(SeaToSeaMod.itemLocale.getEntry("WateryGel"), "WorldEntities/Natural/salt");
        wateryGel.sprite = TextureManager.getSprite(SeaToSeaMod.modDLL, "Textures/Items/WateryGel");
        wateryGel.Patch();*/
    }

    internal static void addCraftingItems() {
        CraftingItems.addAll();

        largeOxygenite = new LargeOxygenite(SeaToSeaMod.ItemLocale.getEntry("OXYGENITE"));
        largeOxygenite.Register();
    }

    internal static void addCreatures() {
        deepStalker = new DeepStalker(SeaToSeaMod.ItemLocale.getEntry("DeepStalker"));
        deepStalker.Register();
        sanctuaryray = new SanctuaryJellyray(SeaToSeaMod.ItemLocale.getEntry("SanctuaryJellyray"));
        sanctuaryray.Register();
        purpleBoomerang = new PurpleBoomerang(SeaToSeaMod.ItemLocale.getEntry("PurpleBoomerang"))
            {
                CookableIntoBase = 1,
            };
        purpleBoomerang.Register();
        purpleHolefish = new PurpleHolefish(SeaToSeaMod.ItemLocale.getEntry("PurpleHolefish"));
        purpleHolefish.Register();
        purpleHoopfish = new PurpleHoopfish(SeaToSeaMod.ItemLocale.getEntry("PurpleHoopfish"))
            {
                CookableIntoBase = 1,
            };
        purpleHoopfish.Register();
        voltaicBladderfish = new VoltaicBladderfish(SeaToSeaMod.ItemLocale.getEntry("VoltaicBladderfish"));
        voltaicBladderfish.Register();
        //giantRockGrub = new GiantRockGrub(SeaToSeaMod.itemLocale.getEntry("GiantRockGrub"));
        //giantRockGrub.Patch();
        broodmother = new BloodKelpBroodmother(SeaToSeaMod.ItemLocale.getEntry("BloodKelpBroodmother"));
        broodmother.Register();

        deepStalker.Data =
            SNUtil.getModifiedACUParams(TechType.Stalker, 1, 1, 1, 1.5F);
        sanctuaryray.Data =
            SNUtil.getModifiedACUParams(TechType.Jellyray, 1, 1, 1, 1.25F);
        purpleBoomerang.Data =
            SNUtil.getModifiedACUParams(TechType.Boomerang, 1, 1, 1, 0.67F);
        purpleHoopfish.Data =
            SNUtil.getModifiedACUParams(TechType.Hoopfish, 1.2F, 1.2F, 1.2F, 1.25F);
        purpleHolefish.Data =
            SNUtil.getModifiedACUParams(TechType.HoleFish, 4F, 4F, 4F, 3.0F);
        voltaicBladderfish.Data =
            SNUtil.getModifiedACUParams(TechType.Bladderfish, 1, 1, 1, 1);

        voidSpikeLevi = new VoidSpikeLeviathan(SeaToSeaMod.ItemLocale.getEntry("VoidSpikeLevi"));
        voidSpikeLevi.register();
    }

    internal static void addMainItems() {
        breathingFluid.Register();
        heatSink.Register();

        depth1300.preventNaturalUnlock();
        depth1300.Register();

        powerSeal.preventNaturalUnlock();
        powerSeal.Register();

        voidStealth.preventNaturalUnlock();
        voidStealth.Register();

        heatSinkModule.preventNaturalUnlock();
        heatSinkModule.Register();

        speedModule.preventNaturalUnlock();
        speedModule.Register();

        lightModule.preventNaturalUnlock();
        lightModule.Register();

        tetherModule.preventNaturalUnlock();
        tetherModule.Register();

        cyclopsHeat.preventNaturalUnlock();
        cyclopsHeat.Register();

        cyclopsStorage.preventNaturalUnlock();
        cyclopsStorage.Register();

        sealGloves.Register(); //has to be before suit since suit references this in craft
        sealSuit.Register();

        t2Battery.Register();

        rebreatherV2.Register();

        liquidTank.Register();
        //oxygeniteTank.Patch();

        chargeFinRelay.Register();

        bandage.Register();
        CraftDataHandler.SetEatingSound(bandage.Info.TechType, TechData.GetSoundUse(TechType.FirstAidKit));
        treatment.Register();
        CraftDataHandler.SetEatingSound(treatment.Info.TechType, TechData.GetSoundUse(TechType.FirstAidKit));
        oxygeniteCharge.Register();
        CraftDataHandler.SetEatingSound(oxygeniteCharge.Info.TechType, TechData.GetSoundUse(TechType.HighCapacityTank));
    }

    internal static void addFlora() {
        alkali = new AlkaliPlant();
        alkali.Register();
        var e = SeaToSeaMod.ItemLocale.getEntry(alkali.ClassID);
        alkali.addPDAEntry(e.pda, 3, e.getString("header"));
        SNUtil.log(" > " + alkali);
        GenUtil.registerPlantWorldgen(alkali, BiomeType.Mountains_IslandCaveFloor, 1, 1F);
        GenUtil.registerPlantWorldgen(alkali, BiomeType.Mountains_CaveFloor, 1, 0.5F);
        GenUtil.registerPlantWorldgen(alkali, BiomeType.Dunes_CaveFloor, 1, 0.5F);
        GenUtil.registerPlantWorldgen(alkali, BiomeType.KooshZone_CaveFloor, 1, 2F);
        GenUtil.registerPlantWorldgen(alkali, BiomeType.SeaTreaderPath_CaveFloor, 1, 1F);
        //GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.UnderwaterIslands_ValleyFloor, 1, 0.5F);

        kelp = new VentKelp();
        kelp.Register();
        e = SeaToSeaMod.ItemLocale.getEntry(kelp.ClassID);
        kelp.addPDAEntry(e.pda, 3, e.getString("header"));
        SNUtil.log(" > " + kelp);

        healFlower = new HealingFlower();
        healFlower.Register();
        e = SeaToSeaMod.ItemLocale.getEntry(healFlower.ClassID);
        healFlower.addPDAEntry(e.pda, 5, e.getString("header"));
        SNUtil.log(" > " + healFlower);
        GenUtil.registerPlantWorldgen(healFlower, BiomeType.GrassyPlateaus_CaveFloor, 1, 2.5F);

        mountainGlow = new MountainGlow();
        mountainGlow.Register();
        e = SeaToSeaMod.ItemLocale.getEntry(mountainGlow.ClassID);
        mountainGlow.addPDAEntry(e.pda, 8, e.getString("header"));
        SNUtil.log(" > " + mountainGlow);
        GenUtil.registerPrefabWorldgen(
            mountainGlow,
            EntitySlot.Type.Small,
            LargeWorldEntity.CellLevel.Medium,
            BiomeType.Mountains_Grass,
            1,
            0.5F
        );
        GenUtil.registerPrefabWorldgen(
            mountainGlow,
            EntitySlot.Type.Small,
            LargeWorldEntity.CellLevel.Medium,
            BiomeType.Mountains_Rock,
            1,
            0.1F
        );
        GenUtil.registerPrefabWorldgen(
            mountainGlow,
            EntitySlot.Type.Small,
            LargeWorldEntity.CellLevel.Medium,
            BiomeType.Mountains_Sand,
            1,
            0.3F
        );

        sanctuaryPlant = new SanctuaryPlant();
        sanctuaryPlant.Register();
        e = SeaToSeaMod.ItemLocale.getEntry(sanctuaryPlant.ClassID);
        sanctuaryPlant.addPDAEntry(e.pda, 10, e.getString("header"));
        SNUtil.log(" > " + sanctuaryPlant);

        e = SeaToSeaMod.ItemLocale.getEntry("BRINE_CORAL");
        brineCoral = SNUtil.addTechTypeToVanillaPrefabs(e, SeaToSeaMod.LrCoralClusters.ToArray());
        SNUtil.addPDAEntry(brineCoral, e.key, e.name, 3, e.getString("category"), e.pda, e.getString("header"));

        brineCoralPiece = new WorldCollectedItem(
            SeaToSeaMod.ItemLocale.getEntry("BrineCoralPiece"),
            VanillaResources.TITANIUM.prefab
        ) {
            sprite = TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/Items/BrineCoralPiece"),
            renderModify = (r) => {
                var mdl = r.setModel(
                    ObjectUtil.lookupPrefab("908d3f0e-04b9-42b4-80c8-a70624eb5455")
                        .getChildObject("lost_river_skull_coral_01")
                );
                //r = mdl.GetComponentInChildren<Renderer>();
                //RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/BrineCoralPiece"); //no such texture
            },
        };
        brineCoralPiece.Register();

        e = SeaToSeaMod.ItemLocale.getEntry("EMPEROR_ROOT");
        foreach (var pfb in VanillaFlora.BLOOD_ROOT_FERTILE) {
            emperorRoots[pfb] = new EmperorRoot(e, pfb);
            emperorRoots[pfb].Register();
        }

        emperorRootCommon = EnumHandler.AddEntry<TechType>(e.key).WithPdaInfo(e.name, e.desc);
        SNUtil.addPDAEntry(emperorRootCommon, e.key, e.name, 5, e.getString("category"), e.pda, e.getString("header"));

        emperorRootOil = new EmperorRootOil(SeaToSeaMod.ItemLocale.getEntry("EmperorRootOil"));
        emperorRootOil.Register();

        /*
        e = SeaToSeaMod.itemLocale.getEntry("POST_COVE_TREE");
        foreach (DecoPlants pfb in PostCoveTree.templates.Keys) {
            postCoveTrees[pfb] = new PostCoveTree(e, pfb);
            postCoveTrees[pfb].Patch();
        }
        postCoveTreeCommon = TechTypeHandler.AddTechType(SeaToSeaMod.modDLL, e.key, e.name, e.desc);
        PostCoveTree.postRegister().encyclopedia = SNUtil.addPDAEntry(postCoveTreeCommon, e.key, e.name, 5, e.getString("category"), e.pda, e.getString("header")).id;
        */
        BioReactorHandler.SetBioReactorCharge(
            alkali.seed.Info.TechType,
            BaseBioReactor.GetCharge(TechType.RedBushSeed) * 1.5F
        );
        BioReactorHandler.SetBioReactorCharge(
            kelp.seed.Info.TechType,
            BaseBioReactor.GetCharge(TechType.BloodOil) * 0.8F
        );
        BioReactorHandler.SetBioReactorCharge(healFlower.seed.Info.TechType, BaseBioReactor.GetCharge(TechType.Peeper));
        BioReactorHandler.SetBioReactorCharge(
            mountainGlow.seed.Info.TechType,
            BaseBioReactor.GetCharge(TechType.Oculus) * 2F
        );
        BioReactorHandler.SetBioReactorCharge(
            sanctuaryPlant.seed.Info.TechType,
            BaseBioReactor.GetCharge(TechType.RedBasketPlantSeed) * 1.5F
        );
        BioReactorHandler.SetBioReactorCharge(
            CraftingItems.getItem(CraftingItems.Items.AmoeboidSample).Info.TechType,
            BaseBioReactor.GetCharge(TechType.CreepvinePiece)
        );
        BioReactorHandler.SetBioReactorCharge(
            CustomEgg.getEgg(deepStalker.Info.TechType).Info.TechType,
            BaseBioReactor.GetCharge(TechType.StalkerEgg) * 0.9F
        );
        BioReactorHandler.SetBioReactorCharge(
            CustomEgg.getEgg(purpleHolefish.Info.TechType).Info.TechType,
            BaseBioReactor.GetCharge(TechType.GasopodEgg) * 1.5F
        );
        BioReactorHandler.SetBioReactorCharge(
            emperorRootOil.Info.TechType,
            BaseBioReactor.GetCharge(TechType.BloodOil) * 0.5F
        );
    }

    internal static void addTablets() {
        brokenBlueTablet.register();
        brokenRedTablet.register();
        brokenWhiteTablet.register();
        brokenOrangeTablet.register();
    }

    internal static void addMachines() {
        var e = SeaToSeaMod.ItemLocale.getEntry("bioprocessor");
        processor = new Bioprocessor(e);
        processor.Register();
        SNUtil.log("Registered custom machine " + processor);
        processor.addPDAPage(e.pda, "Bioprocessor");
        processor.addFragments(4, 5, SeaToSeaMod.BioprocFragments);
        Bioprocessor.addRecipes();

        e = SeaToSeaMod.ItemLocale.getEntry("rebreathercharger");
        rebreatherCharger = new RebreatherRecharger(e);
        rebreatherCharger.Register();
        SNUtil.log("Registered custom machine " + rebreatherCharger);
        rebreatherCharger.addPDAPage(e.pda, "RebreatherCharger");
        rebreatherCharger.addFragments(4, 7.5F, SeaToSeaMod.RebreatherChargerFragments);

        e = SeaToSeaMod.ItemLocale.getEntry("geyserfilter");
        geyserFilter = new GeyserFilter(e);
        geyserFilter.Register();
        SNUtil.log("Registered custom machine " + geyserFilter);
        geyserFilter.addPDAPage(e.pda, "GeyserFilter");
        /*
        e = SeaToSeaMod.itemLocale.getEntry("incubatorinjector");
        incubatorInjector = new IncubatorInjector(e);
        incubatorInjector.Patch();
        SNUtil.log("Registered custom machine "+incubatorInjector);
        incubatorInjector.addPDAPage(e.pda, "??");*/
    }

    internal static void postAdd() {
        registerTabletTechKey(brokenBlueTablet);
        registerTabletTechKey(brokenOrangeTablet);
        registerTabletTechKey(brokenWhiteTablet);
        registerTabletTechKey(brokenRedTablet);

        BatteryCharger.compatibleTech.Add(t2Battery.Info.TechType);

        //override first aid kit
        UsableItemRegistry.instance.addUsableItem(
            TechType.FirstAidKit,
            (s, go) => {
                if (C2CUtil.playerCanHeal() && !Player.main.GetComponent<HealingOverTime>() &&
                    Player.main.GetComponent<LiveMixin>().AddHealth(0.1F) > 0.05) {
                    var ht = Player.main.gameObject.EnsureComponent<HealingOverTime>();
                    ht.setValues(20, 20);
                    ht.activate();
                    return true;
                }

                return false;
            }
        );
        UsableItemRegistry.instance.addUsableItem(
            bandage.Info.TechType,
            (s, go) => {
                if (C2CUtil.playerCanHeal() && !Player.main.GetComponent<HealingOverTime>() &&
                    Player.main.GetComponent<LiveMixin>().AddHealth(0.1F) > 0.05) {
                    var ht = Player.main.gameObject.EnsureComponent<HealingOverTime>();
                    ht.setValues(50, 5);
                    ht.activate();
                    foreach (var dt in Player.main.gameObject.GetComponentsInChildren<DamageOverTime>()) {
                        dt.damageRemaining = 0;
                        dt.CancelInvoke(nameof(DamageOverTime.DoDamage));
                        dt.destroy();
                    }

                    foreach (var ds in Player.main.gameObject
                                 .GetComponents<PlayerMovementSpeedModifier>()) {
                        if (ds.speedModifier < 1)
                            ds.destroy();
                    }

                    Ecocean.FoodEffectSystem.instance.clearNegativeEffects();
                    Player.main.gameObject.removeComponent<Drunk>();
                    return true;
                }

                return false;
            }
        );
        UsableItemRegistry.instance.addUsableItem(
            treatment.Info.TechType,
            (s, go) => {
                var time = DayNightCycle.main.timePassedAsFloat;
                return LiquidBreathingSystem.instance.useKharaaTreatment();
            }
        );
        UsableItemRegistry.instance.addUsableItem(
            CraftingItems.getItem(CraftingItems.Items.WeakEnzyme42).Info.TechType,
            (s, go) => {
                var time = DayNightCycle.main.timePassedAsFloat;
                return LiquidBreathingSystem.instance.applyTemporaryKharaaTreatment();
            }
        );
        UsableItemRegistry.instance.addUsableItem(
            oxygeniteCharge.Info.TechType,
            (s, go) => {
                if (LiquidBreathingSystem.instance.hasLiquidBreathing())
                    return false;
                var ii = Inventory.main.equipment.GetItemInSlot("Tank");
                if (ii == null || !ii.item)
                    return false;
                var ox = ii.item.GetComponent<Oxygen>();
                if (!ox)
                    return false;
                var max = ox.oxygenCapacity * 5;
                var has = ox.oxygenAvailable;
                if (has > max * 0.75F)
                    return false;
                var ob = ox.gameObject.EnsureComponent<OxygenBoost>();
                ob.limit = max;
                ob.original = ox.oxygenCapacity;
                ob.oxygen = ox;
                ox.oxygenCapacity = max;
                ox.oxygenAvailable = max;
                return true;
            }
        );

        IrreplaceableItemRegistry.instance.registerItem(CraftingItems.getItem(CraftingItems.Items.BrokenT2Battery));
        IrreplaceableItemRegistry.instance.registerItem(CraftingItems.getItem(CraftingItems.Items.DenseAzurite));
        IrreplaceableItemRegistry.instance.registerItem(
            CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL)
        );
        IrreplaceableItemRegistry.instance.registerItem(voidStealth, SeamothVoidStealthModule.lossData);
        IrreplaceableItemRegistry.instance.registerItem(TechType.PrecursorKey_Blue);
        IrreplaceableItemRegistry.instance.registerItem(TechType.PrecursorKey_Red);
    }

    internal static void onTechUnlocked(TechType tech) {
        if (DIHooks.GetWorldAge() < 0.25F)
            return;
        if (brokenTablets.ContainsKey(tech))
            SNUtil.triggerTechPopup(brokenTablets[tech]);
        else if (tech == CraftingItems.getItem(CraftingItems.Items.BacterialSample).Info.TechType ||
                 tech == CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType)
            SNUtil.triggerTechPopup(tech);
    }

    private class OxygenBoost : MonoBehaviour {
        internal float limit;
        internal float original;
        internal Oxygen oxygen;

        private void Update() {
            oxygen.oxygenCapacity = Mathf.Min(limit, oxygen.oxygenAvailable);
            if (oxygen.oxygenAvailable <= original) {
                oxygen.oxygenCapacity = original;
                this.destroy();
            }
        }
    }

    internal class IngotDefinition {
        internal readonly TechType material;
        internal readonly TechType ingot;
        internal readonly DuplicateRecipeDelegateWithRecipe unpackingRecipe;
        internal readonly int count;

        internal IngotDefinition(TechType mat, TechType ing, DuplicateRecipeDelegateWithRecipe unpack, int amt) {
            material = mat;
            ingot = ing;
            count = amt;
            unpackingRecipe = unpack;
        }

        public void pickupUnpacked() {
            for (var i = 0; i < count; i++) {
                InventoryUtil.addItem(material);
            }
        }
    }

    internal static void addIngot(
        TechType item,
        BasicCraftingItem ing,
        DuplicateRecipeDelegateWithRecipe unpack,
        int amt
    ) {
        addIngot(item, ing.Info.TechType, unpack, amt);
    }

    internal static void addIngot(TechType item, TechType ing, DuplicateRecipeDelegateWithRecipe unpack, int amt) {
        var id = new IngotDefinition(item, ing, unpack, amt);
        ingots[item] = id;
        ingotsByUnpack[unpack.Info.TechType] = id;
    }

    internal static IngotDefinition getIngot(TechType item) {
        return ingots[item];
    }

    internal static IngotDefinition getIngotByUnpack(TechType item) {
        return ingotsByUnpack.ContainsKey(item) ? ingotsByUnpack[item] : null;
    }

    internal static List<IngotDefinition> getIngots() {
        return [..ingots.Values];
    }

    internal static void setChemistry(TechType item) {
        RecipeUtil.changeRecipePath(item, "Resources", "C2Chemistry");
        RecipeUtil.setItemCategory(item, TechGroup.Resources, chemistryCategory);
    }

    internal static void setModElectronics(TechType item) {
        RecipeUtil.changeRecipePath(item, CraftTree.Type.Workbench, "C2CModElectronics");
        RecipeUtil.setItemCategory(item, TechGroup.Workbench, TechCategory.Workbench);
    }

    public static bool hasSealedOrReinforcedSuit(out bool isSealed, out bool isReinf) {
        var suit = Inventory.main.equipment.GetItemInSlot("Body");
        var glove = Inventory.main.equipment.GetItemInSlot("Gloves");
        var sealSuit = suit != null && suit.item.GetTechType() == C2CItems.sealSuit.Info.TechType;
        var reinfSuit = suit != null && suit.item.GetTechType() == TechType.ReinforcedDiveSuit;
        var sealGlove = glove != null && glove.item.GetTechType() == sealGloves.Info.TechType;
        var reinfGlove = glove != null && glove.item.GetTechType() == TechType.ReinforcedGloves;
        isSealed = sealSuit && sealGlove;
        isReinf = reinfSuit && reinfGlove;
        return (sealSuit || reinfSuit) && (sealGlove || reinfGlove);
    }
}