using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.Auroresource;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.Exscansion;
using ReikaKalseki.Reefbalance;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

[BepInPlugin(ModKey, "SeaToSea", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
[BepInDependency(DIMod.MOD_KEY)]
[BepInDependency(AqueousEngineeringMod.MOD_KEY)]
[BepInDependency(AuroresourceMod.ModKey)]
[BepInDependency(EcoceanMod.MOD_KEY)]
[BepInDependency(ExscansionMod.MOD_KEY)]
[BepInDependency(ReefbalanceMod.ModKey)]
public class SeaToSeaMod : BaseUnityPlugin {
    public const string ModKey = "ReikaKalseki.SeaToSea";

    //public static readonly ModLogger logger = new ModLogger();
    public static readonly Assembly ModDLL = Assembly.GetExecutingAssembly();

    internal static readonly Config<C2CConfig.ConfigEntries> ModConfig = new(ModDLL);
    internal static readonly XMLLocale ItemLocale = new(ModDLL, "XML/items.xml");
    internal static readonly XMLLocale PdaLocale = new(ModDLL, "XML/pda.xml");
    internal static readonly XMLLocale SignalLocale = new(ModDLL, "XML/signals.xml");
    internal static readonly XMLLocale TrackerLocale = new(ModDLL, "XML/tracker.xml");
    internal static readonly XMLLocale MouseoverLocale = new(ModDLL, "XML/mouseover.xml");
    internal static readonly XMLLocale MiscLocale = new(ModDLL, "XML/misc.xml");

    public static readonly WorldgenDatabase WorldGen = new();

    private static readonly Dictionary<string, Dictionary<string, Texture2D>> DegasiBaseTextures = new();

    public static readonly CustomPrefab[] RebreatherChargerFragments = [
        CreateFragment("f350b8ae-9ee4-4349-a6de-d031b11c82b1", go => go.transform.localScale = new Vector3(1, 3, 1)),
        CreateFragment("f744e6d9-f719-4653-906b-34ed5dbdb230", go => go.transform.localScale = new Vector3(1, 2, 1)),
        //new TechnologyFragment("589bf5a6-6866-4828-90b2-7266661bb6ed"),
        CreateFragment("3c076458-505e-4683-90c1-34c1f7939a0f", go => go.transform.localScale = new Vector3(1, 1, 0.2F)),
    ];

    public static readonly CustomPrefab[] BioprocFragments = [
        CreateFragment("85259b00-2672-497e-bec9-b200a1ab012f"),
        //new TechnologyFragment("ba258aad-07e9-4c9b-b517-2ce7400db7b2"),
        //new TechnologyFragment("cf4ca320-bb13-45b6-b4c9-2a079023e787"),
        CreateFragment(
            "f4b3942e-02d8-4526-b384-677a2ad9ce58",
            go => go.transform.localScale = new Vector3(0.25F, 0.25F, 0.5F)
        ),
        CreateFragment("f744e6d9-f719-4653-906b-34ed5dbdb230"),
    ];

    private static int _fragmentId;

    private static CustomPrefab CreateFragment(string targetClass, [CanBeNull] Action<GameObject> modifier = null) {
        var prefab = new CustomPrefab($"s2c_fragment_{_fragmentId}", $"Fragment {_fragmentId}", "");

        prefab.CreateFragment(CraftData.entClassTechTable[targetClass], 1);

        prefab.SetGameObject(() => {
                var go = PrefabUtil.GetPrefab(targetClass);
                modifier?.Invoke(go);
                return go;
            }
        );

        _fragmentId++;

        return prefab;
    }

    public static readonly HashSet<string> LrCoralClusters = [
        "a711c0fa-f31e-4426-9164-a9a65557a9a2",
        //"e0e3036d-93fc-4554-8a58-4efed1efbbd7",  not found under brine
        "e1022037-0897-4a64-b460-cda2a309d2f1",
    ];

    public static CustomPrefab LathingDroneFragment;

    public static MushroomTreeBacterialColony MushroomBioFragment;
    public static GeyserCoral GeyserCoral;
    public static GelFountain GelFountain;
    public static GeoGel Geogel;
    public static GeoGel GeogelDrip;
    public static GeoGelFog GeogelFog;
    public static GeoGelFog GeogelFogDrip;
    public static PostCoveDome PostCoveDome;
    public static PCFSecurityNode SecurityNodeLive;
    public static PCFSecurityNode SecurityNodeBroken;
    public static PrecursorPipeFastTravelConsole PipeConsole;

    public static TechType LavaCastleSmoker;
    public static PDAManager.PDAPage LavaCastleSmokerPda;

    public static PowerSealModuleFragment PowersealModuleFragment;
    public static EjectedHeatSink EjectedHeatSink;

    public static UnmovingHeatBlade Thermoblade;

    public static MountainBaseCuredPeeper Peeper;

    //public static SeaTreaderTunnelLocker locker;
    public static SeaTreaderTunnelLight TunnelLight;
    public static FallingGlassForestWreck GfWreckProp;
    public static DeadMelon DeadMelon;
    public static Campfire Campfire;
    public static Mattress Mattress;
    public static MarshmallowCan MarshCan;
    public static GunPoolBarrier GunPoolBarrier;
    public static LockedPrecursorDoor StepCaveBarrier;
    public static PartialPurpleTablet PurpleTabletPartA;

    public static PartialPurpleTablet PurpleTabletPartB;

    //public static PartialPurpleTablet floatingIslandTablet;
    public static ExplodingGrabbable BrokenAuroraDepthModule;
    public static BKelpBumpWorm BkelpBumpWorm;
    public static AcidSpit AcidSpit;

    private static BloodKelpBaseNuclearReactorMelter _reactorMelter;
    private static TrailerBaseConverter _bioBreaker;
    private static TerrainLootSpawner _mercuryLootSpawner;
    private static TerrainLootSpawner _calciteLootSpawner;
    private static ObjectSpawner _stepCaveTunnelSpawner;
    private static ObjectSpawner _stepCaveTunnelSpawnerSmall;
    private static StepCaveTunnelAtmo _stepCaveTunnelAtmo;

    internal static CrashZoneSanctuarySpawner CrashSanctuarySpawner;
    internal static SanctuaryGrassSpawner SanctuaryGrassSpawner;

    internal static CrashZoneSanctuaryFern CrashSanctuaryFern;
    //internal static CrashZoneSanctuaryGrassBump sanctuaryGrassBump;
    //internal static CrashZoneSanctuaryCoralSheet sanctuaryCoral;

    internal static LRNestGrass LrNestGrass;

    public static DataChit LaserCutterBulkhead;
    public static DataChit BioProcessorBoost;
    public static DataChit SeamothDepthUnlockChit;
    public static TechType SeamothDepthUnlockTrackerTech;
    public static PDAScanner.EntryData SeamothDepthUnlockTracker;
    // public static DataChit vehicleSpeedBoost;

    public static PrecursorFabricatorConsole PrisonEnzymeConsole;

    //internal static VoidLeviElecSphere leviPulse;

    public static SignalManager.ModSignal TreaderSignal;

    public static SignalManager.ModSignal VoidSpikeDirectionHint;

    //public static SignalManager.ModSignal duneArchWreckSignal;
    public static SignalManager.ModSignal SanctuaryDirectionHint;

    internal static Story.StoryGoal CrashMesaRadio;
    //public static Story.StoryGoal duneArchRadio;
    //public static Story.StoryGoal mountainPodRadio;

    internal static Story.StoryGoal AuroraTerminal;
    //internal static Story.StoryGoal jellyPDATriggeredPDAPrompt;

    internal static Story.StoryGoal SunbeamCountdownTrigger;

    internal static Harmony Harmony;

    internal static C2CModOptions Keybinds;

    //Not in C2CProgression because of classloading timing
    internal const string AdvWiringGoal = "NorthCaveAdvWiring";
    internal const string ReinfDBGoal = "C2CVoidWreckReinfDB";

    /*
public static SoundManager.SoundData voidspikeLeviRoar;
public static SoundManager.SoundData voidspikeLeviBite;
public static SoundManager.SoundData voidspikeLeviFX;
public static SoundManager.SoundData voidspikeLeviAmbient;
*/

    internal static bool AnywhereSeamothModuleCheatActive;
    internal static bool TrackerShowAllCheatActive;
    internal static bool FastSeaglideCheatActive;

    public void Start() {
        ModConfig.load();

        C2CIntegration.injectConfigValues();
    }

    public void Awake() {
        var hs = new HarmonySystem(ModKey, ModDLL, typeof(C2CPatches));
        Harmony = hs.harmonyInstance;
        hs.apply();

        ModVersionCheck.getFromGitVsInstall("Sea To Sea", ModDLL, "SeaToSea").register();

        // CustomPrefab.addPrefabNamespace("ReikaKalseki.SeaToSea");

        C2CIntegration.injectLoad();

        ItemLocale.load();
        PdaLocale.load();
        SignalLocale.load();
        TrackerLocale.load();
        MouseoverLocale.load();
        MiscLocale.load();

        C2CItems.preAdd();

        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(VoidSpike).TypeHandle);

        // voidspikeLeviRoar = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_roar", "Sounds/voidlevi-roar.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
        //voidspikeLeviFX = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_fx", "Sounds/voidlevi-fx1.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
        // voidspikeLeviAmbient = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_amb", "Sounds/voidlevi-longamb2.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);
        //voidspikeLeviBite = SoundManager.registerSound(SeaToSeaMod.modDLL, "voidspikelevi_bite", "Sounds/voidlevi-bite.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 200);}, SoundSystem.masterBus);

        C2CItems.addCreatures();

        var e = MiscLocale.getEntry("bulkheadLaserCutterUpgrade");
        LaserCutterBulkhead = new DataChit(
            e.key,
            e.name,
            e.desc,
            d => {
                d.ControlText = e.pda;
                d.Graphic = () => SNUtil.GetTechPopupSprite(TechType.LaserCutter);
            }
        );
        //laserCutterBulkhead.showOnScannerRoom = false;
        LaserCutterBulkhead.Register();
        e = MiscLocale.getEntry("bioprocessorBoost");
        BioProcessorBoost = new DataChit(e.key, e.name, e.desc, d => { d.ControlText = e.pda; });
        //bioProcessorBoost.showOnScannerRoom = false;
        BioProcessorBoost.Register();
        e = MiscLocale.getEntry("jellyshroomSeamothDepth");
        SeamothDepthUnlockChit = new DataChit(
            e.key,
            e.name,
            e.desc,
            d => { d.OnUnlock = C2CProgression.OnSeamothDepthChit; }
        ) {
            showOnScannerRoom = true,
        };
        SeamothDepthUnlockChit.Register();

        SeamothDepthUnlockTrackerTech = EnumHandler.AddEntry<TechType>("SeamothDepthUnlockTracker").WithPdaInfo("", "");
        SeamothDepthUnlockTracker = new PDAScanner.EntryData {
            key = SeamothDepthUnlockTrackerTech,
            blueprint = TechType.VehicleHullModule1,
            destroyAfterScan = false,
            locked = true,
            totalFragments = 3,
            isFragment = true,
        };
        PDAHandler.AddCustomScannerEntry(SeamothDepthUnlockTracker);

        e = ItemLocale.getEntry("Geogel");
        Geogel = new GeoGel(e, false);
        Geogel.Register();
        GeogelDrip = new GeoGel(e, true);
        GeogelDrip.Register();

        C2CItems.addFlora();

        C2CRecipes.addItemsAndRecipes();
        C2CItems.addTablets();
        PowersealModuleFragment = new PowerSealModuleFragment();
        PowersealModuleFragment.register();
        EjectedHeatSink = new EjectedHeatSink();
        EjectedHeatSink.Register();
        Thermoblade = new UnmovingHeatBlade();
        Thermoblade.Register();
        Peeper = new MountainBaseCuredPeeper();
        Peeper.Register();
        //locker = new SeaTreaderTunnelLocker();
        //locker.Register();
        TunnelLight = new SeaTreaderTunnelLight();
        TunnelLight.Register();
        GfWreckProp = new FallingGlassForestWreck();
        GfWreckProp.Register();
        DeadMelon = new DeadMelon();
        DeadMelon.Register();
        Campfire = new Campfire();
        Campfire.Register();
        Mattress = new Mattress();
        Mattress.Register();
        MarshCan = new MarshmallowCan();
        MarshCan.Register();
        GunPoolBarrier = new GunPoolBarrier();
        GunPoolBarrier.Register();
        StepCaveBarrier = new LockedPrecursorDoor(
            "StepCaveDoor",
            PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Purple,
            new PositionedPrefab("", new Vector3(34.895F, -167F, -649.277F), Quaternion.Euler(0, 287.2F, 0)),
            new PositionedPrefab(
                "",
                new Vector3(48.267F, -169.978F, -659.676F),
                Quaternion.Euler(3.335F, 273.717F, 0.541F)
            )
        );
        StepCaveBarrier.Register();
        PurpleTabletPartA = new PartialPurpleTablet(true, false);
        PurpleTabletPartA.Register();
        PurpleTabletPartB = new PartialPurpleTablet(false, true);
        PurpleTabletPartB.Register();
        BrokenAuroraDepthModule = new ExplodingGrabbable(
            "ExplodingAuroraModule",
            C2CHooks.AuroraDepthModule.prefabName
        );
        BrokenAuroraDepthModule.Register();
        _reactorMelter = new BloodKelpBaseNuclearReactorMelter();
        _reactorMelter.Register();
        _bioBreaker = new TrailerBaseConverter();
        _bioBreaker.Register();

        BkelpBumpWorm = new BKelpBumpWorm(ItemLocale.getEntry("BKelpBumpWorm"));
        BkelpBumpWorm.Register();
        AcidSpit = new AcidSpit();
        AcidSpit.Register();

        MushroomBioFragment = new MushroomTreeBacterialColony(ItemLocale.getEntry("TREE_BACTERIA"));
        MushroomBioFragment.register();

        _mercuryLootSpawner = new TerrainLootSpawner("MercuryLootSpawner", VanillaResources.MERCURY.prefab);
        _mercuryLootSpawner.Register();
        _calciteLootSpawner = new TerrainLootSpawner(
            "CalciteLootSpawner",
            CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).ClassID
        );
        _calciteLootSpawner.Register();

        var stepCavePlants = new WeightedRandom<PrefabReference>();
        stepCavePlants.addEntry(VanillaFlora.VIOLET_BEAU, 40);
        stepCavePlants.addEntry(VanillaFlora.PAPYRUS, 30);
        stepCavePlants.addEntry(VanillaFlora.REDWORT, 30);
        stepCavePlants.addEntry(new ModPrefabContainer(C2CItems.healFlower), 20);
        _stepCaveTunnelSpawner = new ObjectSpawner(
            "StepCaveTunnelPlantSpawner",
            new ObjectSpawner.SpawnSet(stepCavePlants)
        );
        _stepCaveTunnelSpawner.Register();

        stepCavePlants = new WeightedRandom<PrefabReference>();
        stepCavePlants.addEntry(VanillaFlora.ACID_MUSHROOM, 40);
        //often placed wrong stepCavePlants.addEntry(VanillaFlora.WRITHING_WEED.getPrefabID(), 20);
        _stepCaveTunnelSpawnerSmall = new ObjectSpawner(
            "StepCaveTunnelPlantSpawnerSmall",
            new ObjectSpawner.SpawnSet(
                stepCavePlants,
                (go, tt) => {
                    go.transform.rotation = tt.rotation;
                    go.transform.Rotate(go.transform.right, -90);
                }
            )
        );
        _stepCaveTunnelSpawnerSmall.Register();

        _stepCaveTunnelAtmo = new StepCaveTunnelAtmo();
        _stepCaveTunnelAtmo.Register();

        CrashSanctuarySpawner = new CrashZoneSanctuarySpawner();
        CrashSanctuarySpawner.Register();
        SanctuaryGrassSpawner = new SanctuaryGrassSpawner();
        SanctuaryGrassSpawner.Register();
        CrashSanctuaryFern = new CrashZoneSanctuaryFern();
        CrashSanctuaryFern.Register();
        //sanctuaryGrassBump = new CrashZoneSanctuaryGrassBump();
        //sanctuaryGrassBump.Register();
        //sanctuaryCoral = new CrashZoneSanctuaryCoralSheet();
        //sanctuaryCoral.Register();

        LrNestGrass = new LRNestGrass();
        LrNestGrass.Register();

        //PrecursorFabricatorConsole.CraftingIdentifier ci = new PrecursorFabricatorConsole.RecipeID(TechType.HatchingEnzymes, C2CRecipes.getHatchingEnzymeRecipe(), "HatchEnzymes");
        PrisonEnzymeConsole =
            new PrecursorFabricatorConsole(
                C2CRecipes.getHatchingEnzymeFab(),
                "PrecursorEnzymes",
                new Color(0.8F, 0.8F, 0.8F)
            ).addStoryGate("PrecursorPrisonAquariumIncubatorActive", MouseoverLocale.getEntry("EnzymesNotKnown").desc);
        PrisonEnzymeConsole.Register();

        e = PdaLocale.getEntry("LavaCastleSmoke");
        LavaCastleSmoker = EnumHandler.AddEntry<TechType>(e.key).WithPdaInfo(e.name, e.desc);
        LavaCastleSmokerPda = SNUtil.AddPdaEntry(
            LavaCastleSmoker,
            e.key,
            e.name,
            2,
            e.getString("category"),
            e.pda,
            e.getString("header")
        );

        CustomLocaleKeyDatabase.registerKeys(MouseoverLocale);

        //leviPulse = new VoidLeviElecSphere();
        //leviPulse.Register();

        var drone = CraftingItems.getItem(CraftingItems.Items.LathingDrone);

        LathingDroneFragment = new CustomPrefab("6e0f4652-c439-4540-95be-e61384e27692", "", "");
        LathingDroneFragment.CreateFragment(drone.TechType, 3, 2);
        LathingDroneFragment.SetGameObject(() => {
                UWE.PrefabDatabase.GetPrefabAsync("6e0f4652-c439-4540-95be-e61384e27692").TryGetPrefab(out var go);

                go.removeComponent<Pickupable>();
                go.removeComponent<Rigidbody>();
                go.EnsureComponent<LathingDroneSparker>();

                return go;
            }
        ); //it has its own model

        C2CItems.addMachines();

        GeyserCoral = new GeyserCoral(ItemLocale.getEntry("GEYSER_CORAL"));
        GeyserCoral.register();

        GelFountain = new GelFountain(ItemLocale.getEntry("GEL_FOUNTAIN"));
        GelFountain.register();

        GeogelFog = new GeoGelFog(false);
        GeogelFog.Register();
        GeogelFogDrip = new GeoGelFog(true);
        GeogelFogDrip.Register();

        PostCoveDome = new PostCoveDome(ItemLocale.getEntry("POST_COVE_DOME"));
        PostCoveDome.Register();

        SecurityNodeLive = new PCFSecurityNode(ItemLocale.getEntry("PCF_SECURITY"), true);
        SecurityNodeLive.Register();
        SecurityNodeBroken = new PCFSecurityNode(ItemLocale.getEntry("PCF_SECURITY_BROKEN"), false);
        SecurityNodeBroken.Register();

        PipeConsole = new PrecursorPipeFastTravelConsole(ItemLocale.getEntry("PIPE_TRAVEL_CONSOLE"));
        PipeConsole.Register();

        AddPdaEntries();

        AddOreGen();

        GenUtil.registerWorldgen(
            new PositionedPrefab(
                CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).ClassID,
                Azurite.mountainBaseAzurite,
                Quaternion.Euler(0, 202.4F, 33.2F)
            )
        );
        //GenUtil.registerWorldgen(new PositionedPrefab("e85adb0d-665e-48f5-9fa2-2dd316776864", C2CHooks.bkelpBaseGeoCenter), go => go.transform.localScale = Vector3.one*60);

        AddSignalsAndRadio();

        PDAMessages.addAll();

        AuroraTerminal = new Story.StoryGoal("auroraringterminal_c2c", Story.GoalType.PDA, 0);
        e = MiscLocale.getEntry(AuroraTerminal.key);
        SNUtil.AddVoLine(AuroraTerminal, e.desc, SoundManager.registerPDASound(ModDLL, e.key, e.pda).asset);
        //StoryHandler.instance.addListener(s => {if (s == auroraTerminal.key) {}});

        SunbeamCountdownTrigger = new Story.StoryGoal("c2cTriggerSunbeamCountdown", Story.GoalType.Story, 0);

        //DamageSystem.acidImmune = DamageSystem.acidImmune.AddToArray<TechType>(TechType.Seamoth);

        C2CItems.postAdd();

        VoidSpikesBiome.instance.register();
        UnderwaterIslandsFloorBiome.instance.register();
        CrashZoneSanctuaryBiome.instance.register();
        VoidSpike.register();
        AvoliteSpawner.Instance.Register();
        BiomeDiscoverySystem.instance.register();
        LifeformScanningSystem.instance.register();

        C2CItems.alkali.addNativeBiome(VanillaBiomes.Mountains, true).addNativeBiome(VanillaBiomes.Treader, true)
            .addNativeBiome(VanillaBiomes.Koosh, true);
        C2CItems.kelp.addNativeBiome(UnderwaterIslandsFloorBiome.instance);
        C2CItems.healFlower.addNativeBiome(VanillaBiomes.Redgrass, true);
        C2CItems.sanctuaryPlant.addNativeBiome(CrashZoneSanctuaryBiome.instance);
        C2CItems.mountainGlow.addNativeBiome(VanillaBiomes.Mountains);
        //C2CItems.underislandsCavePlant.addNativeBiome(VanillaBiomes.UNDERISLANDS);

        C2CItems.deepStalker.AddNativeBiome(VanillaBiomes.Grandreef);
        C2CItems.purpleBoomerang.AddNativeBiome(UnderwaterIslandsFloorBiome.instance);
        C2CItems.purpleHoopfish.AddNativeBiome(UnderwaterIslandsFloorBiome.instance);
        C2CItems.purpleHolefish.AddNativeBiome(UnderwaterIslandsFloorBiome.instance);
        C2CItems.broodmother.AddNativeBiome(VanillaBiomes.Bloodkelpnorth);
        C2CItems.voltaicBladderfish.AddNativeBiome(VoidSpikesBiome.instance);
        VoidSpikes.addFish(C2CItems.voltaicBladderfish.ClassID, 200);

        InitHandlers();

        var ang = new Vector3(0, 317, 0);
        var pos1 = new Vector3(-1226, -350, -1258);
        var pos2 = new Vector3(-1327, -350, -1105);
        var tgt = pos2 + (pos2 - pos1).SetLength(40);
        for (var i = 0; i <= 4; i++) {
            var pos = Vector3.Lerp(pos1, pos2, i / 4F);
            GenUtil.registerWorldgen(
                new PositionedPrefab(VanillaCreatures.SEA_TREADER.prefab, pos, Quaternion.Euler(ang)),
                go => {
                    go.GetComponent<TreaderMoveOnSurface>().timeNextTarget = Time.time + 120;
                    go.GetComponent<SeaTreader>().MoveTo(tgt);
                }
            );
        }

        SNUtil.AddMultiScanUnlock(TechType.PowerTransmitter, 2, TechType.PowerTransmitter, 1, false);
        SNUtil.AddMultiScanUnlock(TechType.LEDLight, 2, TechType.LEDLight, 1, false);
        SNUtil.AddMultiScanUnlock(TechType.ThermalPlant, 4, TechType.ThermalPlant, 1, false);
        SNUtil.AddMultiScanUnlock(TechType.NuclearReactor, 7, TechType.NuclearReactor, 1, false);

        SpriteHandler.RegisterSprite(C2CItems.brineCoral, TextureManager.getSprite(ModDLL, "Textures/BrineCoralIcon"));

        C2CIntegration.prePostAdd();

        // TODO
        // var modsWithIssues = new Dictionary<string, bool>() {
        //     { "CyclopsNuclearUpgrades", true },
        //     { "CyclopsBioReactor", false },
        //     //{"AquariumBreeding", false},
        //     { "RedBaron", true },
        //     //{"SeamothArms", true},
        //     { "HabitatControlPanel", true },
        //     { "MoreSeamothDepth", true },
        //     { "CustomCraft2", true },
        //     //{"FCSAlterraHub", false},
        //     { "SlotExtender", false },
        //     { "WarpChip", false },
        //     //{"Socknautica", false},
        //     { "Socksfor1Monsters", false },
        //     { "DADTankSubPack", true },
        //     { "DWEquipmentBonanza", false },
        //     { "SeaVoyager", false },
        //     { "SubnauticaRandomiser", true },
        //     { "EquivalentExchange", true },
        //     { "Deathrun", false },
        //     { "DecorationsMod", true },
        //     { "AnthCreatures", true },
        //     { "SpyWatch", true },
        //     { "SeamothEnergyShield", true },
        //     { "SeamothThermal", false },
        //     { "ArmorSuit", false },
        //     { "ShieldSuit", false },
        //     { "TimeControlSuit", true },
        //     { "CameraDroneStasisUpgrade", true },
        //     //{"CameraDroneFlightUpgrade", false},
        //     { "CustomizeYourSpawns", true },
        //     { "StasisModule", true },
        //     { "StasisTorpedo", true },
        //     { "CyclopsLaserCannonModule", false },
        //     { "DebrisRecycling", true },
        //     { "AD3D_DeepEngineMod", false },
        //     { "DeepEngineMod", false },
        //     { "AD3D_TechFabricatorMod", false },
        //     { "PassiveReapers", true },
        //     { "PlasmaCannonArm", false }, //add scanner module?
        //     { "AcceleratedStart", true },
        //     { "CyclopsNuclearReactor", true },
        //     { "LaserCannon", true },
        //     { "PartsFromScanning", true },
        //     { "StealthModule", true },
        //     { "RPG_Framework", true },
        //     { "CustomBatteries", true },
        //     { "DropUpgradesOnDestroy", false },
        //     { "All_Items_1x1", false },
        //     { "Radiant Depths", true }, //TODO id might be wrong, also might be 2.0
        //     { "SubnauticaAutosave", true },
        //     { "SeaToSeaWorldGenFixer", true },
        //     { "FCSIntegrationRemover", true },
        //     { "UpgradedVehicles", true },
        //     { "aaaaaaaaaa", true },
        // };
        //
        // foreach (QModManager.API.IQMod mod in QModManager.API.QModServices.Main.GetAllMods()) {
        //     SNUtil.log("Checking compat with 'mod " + mod.Id + "' (\"" + mod.DisplayName + "\")");
        //
        //     if (!modsWithIssues.TryGetValue(mod.Id, out var issue)) continue;
        //
        //     if (issue) {
        //         var msg = "Mod '" + mod.DisplayName +
        //                   "' detected. This mod is not compatible with SeaToSea, and cannot be used alongside it.";
        //         SNUtil.createPopupWarning(msg, false /*, null, SNUtil.createPopupButton("OK")*/);
        //         throw new Exception(msg);
        //     } else {
        //         var msg = "SeaToSea: Mod '" + mod.DisplayName +
        //                   "' detected. This mod will significantly alter the balance of your pack and risks completely breaking C2C progression.";
        //         SNUtil.createPopupWarning(msg, false /*, null, SNUtil.createPopupButton("OK")*/);
        //         SNUtil.log(msg + " You should remove this mod if possible when using SeaToSea.");
        //     }
        // }

        if (!BepInExUtil.IsModLoaded(PluginIDs.TerrainPatcher)) {
            var msg = "TerrainPatcher is a required dependency for SeaToSea!";

            SNUtil.CreatePopupWarning(
                msg,
                false /*, SNUtil.createPopupButton("Download", () => {
                System.Diagnostics.Process.Start("https://github.com/Esper89/Subnautica-TerrainPatcher/releases/download/v0.4/TerrainPatcher-v0.4.zip");
                Application.Quit(64);
            }), SNUtil.createPopupButton("Ignore")*/
            );

            throw new Exception(msg);
        }

        if (!BepInExUtil.IsModLoaded(PluginIDs.RadialTabs)) {
            var msg =
                "RadialTabs is recommended when using SeaToSea to ensure that all crafting nodes in fabricator UIs remain onscreen.";
            SNUtil.CreatePopupWarning(
                msg,
                true /*, SNUtil.createPopupButton("Download", () => {
                System.Diagnostics.Process.Start("https://www.nexusmods.com/Core/Libs/Common/Widgets/ModRequirementsPopUp?id=2624&game_id=1155");
                Application.Quit(64);
            }), SNUtil.createPopupButton("Ignore")*/
            );
            SNUtil.Log(msg + " You should add this mod if at all possible.");
        }

        var fn = "generated.optoctreepatch";
        if (File.Exists(Path.Combine(Path.GetDirectoryName(ModDLL.Location), fn))) {
            var msg = "Delete " + fn +
                      " from your install directory. This is an old file from previous versions and will conflict with new terrain patches.";
            SNUtil.CreatePopupWarning(msg, false);
            throw new Exception(msg);
        }

        // PostLoad
        new LavaCastleVentCrystalPlacer().Register();
        // TODO: FCS Compat
        // WorldGen.load(s => s != "fcswreck" || FCSIntegrationSystem.instance.isLoaded()
        // ); //load in post because some cross-mod TTs may not exist yet
        MushroomBioFragment.postRegister();
        GeyserCoral.PostRegister();
        GelFountain.postRegister();
        PostCoveDome.postRegister();
        SecurityNodeLive.postRegister();
        C2CProgression.Instance.PcfSecurityNodes = WorldGen.getCount(SecurityNodeLive.Info.ClassID);
        PipeConsole.setGoal(C2CProgression.PipeTravelEnabled);

        var n = C2CHooks.PurpleTabletsToBreak.Count + 1; //+1 for the broken one in front of gun
        n += WorldGen.getCount("83b61f89-1456-4ff5-815a-ecdc9b6cc9e4");
        n += WorldGen.getCount("PartialPurpleTablet_A");
        n += WorldGen.getCount("PartialPurpleTablet_B");
        PDAHandler.EditFragmentsToScan(
            TechType.PrecursorKey_PurpleFragment,
            n
        ); //hard ? n : n-1; //allow missing one in not-hard

        foreach (var pos in C2CProgression.Instance.BkelpNestBumps) {
            GenUtil.registerWorldgen(new BKelpBumpWormSpawner(pos + Vector3.down * 3));
        }

        AvoliteSpawner.Instance.PostRegister();
        DataboxTypingMap.instance.load();
        DataboxTypingMap.instance.addValue(-789.81, -216.10, -711.02, C2CItems.bandage.Info.TechType);
        DataboxTypingMap.instance.addValue(-483.55, -504.69, 1326.64, C2CItems.tetherModule.Info.TechType);
        DataboxTypingMap.instance.addValue(-317.05, -438.69, -1742.80, TechType.BaseReinforcement);

        ESHooks.AddLeviathan(C2CItems.voidSpikeLevi.TechType);
        ESHooks.ScannabilityEvent += C2CHooks.IsItemMapRoomDetectable;

        foreach (BiomeType bb in Enum.GetValues(typeof(BiomeType))) {
            LootDistributionHandler.EditLootDistributionData(VanillaResources.SULFUR.prefab, bb, 0, 1);
            LootDistributionHandler.EditLootDistributionData(
                CustomEgg.GetEgg(TechType.SpineEel).Info.ClassID,
                bb,
                0,
                1
            );
            if (bb == BiomeType.BonesField_Lake_Floor || bb == BiomeType.BonesField_LakePit_Floor ||
                bb == BiomeType.BonesField_LakePit_Wall || bb == BiomeType.BonesField_Cave_Ground) continue;
            foreach (var s in LrCoralClusters)
                LootDistributionHandler.EditLootDistributionData(s, bb, 0, 1);
        }

        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ExplorationTrackerPages).TypeHandle);

        C2CIntegration.addPostCompat();

        DumpAbandonedBaseTextures();
    }

    internal static void InitHandlers() {
        var h = Harmony;
        SaveSystem.addPlayerSaveCallback(
            typeof(LiquidBreathingSystem),
            "kharaaTreatmentRemainingTime",
            () => LiquidBreathingSystem.instance
        );
        SaveSystem.addPlayerSaveCallback(
            typeof(EnvironmentalDamageSystem),
            "recoveryWarningEndTime",
            () => EnvironmentalDamageSystem.instance
        );

        POITeleportSystem.instance.populate();
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(C2CUnlocks).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(C2CProgression).TypeHandle);
        DataCollectionTracker.instance.register();
        MoraleSystem.instance.register();

        Keybinds = new C2CModOptions();
        OptionsPanelHandler.RegisterModOptions(Keybinds);
        AddCommands();
    }

    private static void DumpAbandonedBaseTextures() {
        string[] prefabs = [
            "026c39c1-d0cc-442c-aa42-e574c9c281b2",
            "0e394d55-da8c-4b3e-b038-979477ce77c1",
            "255ed3c3-1973-40c0-9917-d16dd9a7018d",
            "256a06d3-b861-487a-b8ac-050daa0d683d",
            "2921736c-c898-4213-9615-ea1a72e28178",
            "569f22e0-274d-49b0-ae5e-21ef0ce907ca",
            "99b164ac-dfb4-4a14-b305-8666fa227717",
            "c1139534-b3b9-4750-b60b-a77ca054b3dd",
            "dd923ae3-20f6-47e0-87c0-ae2bc386607a",
        ];
        HashSet<string> exported = [];
        foreach (var s in prefabs) {
            var go = ObjectUtil.lookupPrefab(s);
            if (!go) continue;
            var rr = go.GetComponentsInChildren<Renderer>(true);
            //SNUtil.log("Exporting degasi base textures from "+s+": "+rr.Length+":"+string.Join(", ", rr.Select(r2 => r2.name)), modDLL);
            foreach (var r in rr) {
                foreach (var m in r.materials) {
                    if (!m || m.mainTexture == null) continue;
                    var n = m.mainTexture.name.Replace(" (Instance)", "").ToLowerInvariant();
                    if (exported.Contains(n)) continue;
                    SNUtil.Log(
                        "Exporting degasi base textures from " + r.gameObject.GetFullHierarchyPath() +
                        ": " + n,
                        ModDLL
                    );
                    DegasiBaseTextures[n] = new Dictionary<string, Texture2D>();
                    foreach (var tex in m.GetTexturePropertyNames())
                        DegasiBaseTextures[n][tex] = (Texture2D)m.GetTexture(tex);
                    if (DegasiBaseTextures[n].Count > 0)
                        exported.Add(n);
                }
            }
        }
    }

    public static bool HasDegasiBaseTextures(string n) {
        return DegasiBaseTextures.ContainsKey(n);
    }

    public static Texture2D GetDegasiBaseTexture(string n, string type) {
        return DegasiBaseTextures[n].ContainsKey(type) ? DegasiBaseTextures[n][type] : null;
    }

    private static void AddSignalsAndRadio() {
        var e = SignalLocale.getEntry("treaderpod");
        TreaderSignal = SignalManager.createSignal(e);
        TreaderSignal.addRadioTrigger(e.getString("sound"));
        TreaderSignal.register("32e48451-8e81-428e-9011-baca82e9cd32", new Vector3(-1239, -360, -1193));
        TreaderSignal.addWorldgen();
        /*
    e = SeaToSeaMod.signalLocale.getEntry("dunearch");
    duneArchWreckSignal = SignalManager.createSignal(e);
    duneArchWreckSignal.addRadioTrigger(e.getString("sound"));
    duneArchWreckSignal.register("32e48451-8e81-428e-9011-baca82e9cd32", new Vector3(-1623, -355.6, -98.5));
    duneArchWreckSignal.addWorldgen();
    */
        e = SignalLocale.getEntry("voidspike");
        VoidSpikeDirectionHint = SignalManager.createSignal(e);
        VoidSpikeDirectionHint.setStoryGate(PDAManager.getPage("voidpod").id);
        VoidSpikeDirectionHint.register(
            "4c10bbd6-5100-4632-962e-69306b09222f",
            SpriteManager.Get(SpriteManager.Group.Pings, "Sunbeam"),
            VoidSpikesBiome.end500m
        );
        VoidSpikeDirectionHint.addWorldgen();

        e = SignalLocale.getEntry("sanctuary");
        SanctuaryDirectionHint = SignalManager.createSignal(e);
        SanctuaryDirectionHint.register(
            "4c10bbd6-5100-4632-962e-69306b09222f",
            SpriteManager.Get(SpriteManager.Group.Pings, "Sunbeam"),
            CrashZoneSanctuaryBiome.biomeCenter.SetY(-360)
        );
        SanctuaryDirectionHint.addWorldgen();

        e = PdaLocale.getEntry("crashmesahint");
        CrashMesaRadio = SNUtil.AddRadioMessage("crashmesaradio", e.getString("radio"), e.getString("radioSound"));
    }

    private static void AddOreGen() {
        var vent = CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL);
        vent.registerWorldgen(BiomeType.Dunes_ThermalVent, 1, 3F);
        vent.registerWorldgen(BiomeType.Mountains_ThermalVent, 1, 1.0F);

        // TODO: FCS Compat
        // if (FCSIntegrationSystem.instance.isLoaded()) {
        //     vent.registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 0.2F);
        //     vent.registerWorldgen(BiomeType.DeepGrandReef_ThermalVent, 1, 0.4F);
        // }

        var irid = CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM);
        irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Ceiling, 1, 1.2F);
        irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor, 1, 0.3F);
        irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor_Far, 1, 0.67F);
        irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Wall, 1, 0.6F);
        irid.registerWorldgen(BiomeType.InactiveLavaZone_Chamber_Ceiling, 1, 0.5F);

        LootDistributionHandler.EditLootDistributionData(
            PostCoveDomeGenerator.hotResourceDome.ClassID,
            BiomeType.InactiveLavaZone_Corridor_Ceiling,
            0.5F,
            1
        );

        var calcite = CustomMaterials.getItem(CustomMaterials.Materials.CALCITE);
        calcite.registerWorldgen(BiomeType.BonesField_Cave_Ceiling, 1, 1.2F);

        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MAGNETITE.prefab,
            BiomeType.UnderwaterIslands_Geyser,
            2.5F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LARGE_MAGNETITE.prefab,
            BiomeType.UnderwaterIslands_Geyser,
            0.4F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LARGE_DIAMOND.prefab,
            BiomeType.UnderwaterIslands_Geyser,
            0.25F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LITHIUM.prefab,
            BiomeType.UnderwaterIslands_Geyser,
            1.5F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.QUARTZ.prefab,
            BiomeType.UnderwaterIslands_Geyser,
            2.5F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.DIAMOND.prefab,
            BiomeType.UnderwaterIslands_Geyser,
            1.5F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.QUARTZ.prefab,
            BiomeType.UnderwaterIslands_ValleyFloor,
            2.5F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LITHIUM.prefab,
            BiomeType.UnderwaterIslands_ValleyFloor,
            1F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LARGE_QUARTZ.prefab,
            BiomeType.UnderwaterIslands_ValleyFloor,
            0.33F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LARGE_MAGNETITE.prefab,
            BiomeType.UnderwaterIslands_ValleyFloor,
            0.15F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LARGE_DIAMOND.prefab,
            BiomeType.UnderwaterIslands_ValleyFloor,
            0.2F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MERCURY.prefab,
            BiomeType.UnderwaterIslands_Geyser,
            0.5F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LARGE_MERCURY.prefab,
            BiomeType.UnderwaterIslands_Geyser,
            0.15F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MERCURY.prefab,
            BiomeType.UnderwaterIslands_ValleyFloor,
            0.25F,
            1
        );

        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.QUARTZ.prefab,
            BiomeType.UnderwaterIslands_IslandCaveFloor,
            0.5F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.QUARTZ.prefab,
            BiomeType.UnderwaterIslands_IslandCaveWall,
            0.3F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MAGNETITE.prefab,
            BiomeType.UnderwaterIslands_IslandCaveFloor,
            0.5F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MAGNETITE.prefab,
            BiomeType.UnderwaterIslands_IslandCaveWall,
            0.3F,
            1
        );

        LootDistributionHandler.EditLootDistributionData(
            C2CItems.purpleHolefish.ClassID,
            BiomeType.UnderwaterIslands_OpenDeep_CreatureOnly,
            2.75F,
            1
        );

        //LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.LostRiverCorridor_LakeFloor, 0.2F, 1);
        //LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.LostRiverJunction_LakeFloor, 0.2F, 1);
        //LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_Corridor_Stream, 0.2F, 1);
        //LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_Lake_Floor, 0.2F, 1);
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LARGE_SULFUR.prefab,
            BiomeType.BonesField_LakePit_Floor,
            0.4F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LARGE_SULFUR.prefab,
            BiomeType.BonesField_LakePit_Wall,
            0.2F,
            1
        );
        //LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.SkeletonCave_Lake_Floor, 0.2F, 1);
        //CustomMaterials.getItem(CustomMaterials.Materials.).registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 8F);
        /*
    LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Dunes_ThermalVent, 2F, 1);
    LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Mountains_ThermalVent, 2F, 1);
    LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.GrandReef_ThermalVent, 2F, 1);
    LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.DeepGrandReef_ThermalVent, 2F, 1);*/

        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LARGE_MERCURY.prefab,
            BiomeType.KooshZone_CaveSpecial,
            2F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MERCURY.prefab,
            BiomeType.KooshZone_CaveSpecial,
            4F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MERCURY.prefab,
            BiomeType.KooshZone_CaveFloor,
            0.75F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MERCURY.prefab,
            BiomeType.KooshZone_CaveWall,
            0.5F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MERCURY.prefab,
            BiomeType.KooshZone_Geyser,
            0.5F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LARGE_MERCURY.prefab,
            BiomeType.KooshZone_Geyser,
            0.125F,
            1
        );

        foreach (var kvp in C2CUtil.mercurySpawners) {
            var vals = kvp.Value; //exclusion radius, target count, max range
            var count = vals.Item2;
            if (ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
                count = Math.Max(1, count * 2 / 3);
            GenUtil.registerWorldgen(
                new PositionedPrefab(
                    _mercuryLootSpawner.ClassID,
                    kvp.Key,
                    Quaternion.identity,
                    new Vector3(vals.Item1, count, vals.Item3)
                )
            );
        }

        foreach (var kvp in C2CUtil.calciteSpawners) {
            GenUtil.registerWorldgen(
                new PositionedPrefab(
                    _calciteLootSpawner.ClassID,
                    kvp.Key,
                    Quaternion.identity,
                    new Vector3(kvp.Value.Item1, kvp.Value.Item2, kvp.Value.Item3)
                )
            );
        }

        //LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.Dunes_CaveFloor, 0.05F, 1);
        //LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.Mountains_CaveFloor, 0.05F, 1);
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MERCURY.prefab,
            BiomeType.ActiveLavaZone_Falls_Wall,
            0.25F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MERCURY.prefab,
            BiomeType.ActiveLavaZone_Falls_Floor,
            0.25F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.MERCURY.prefab,
            BiomeType.ActiveLavaZone_Falls_Floor_Far,
            0.4F,
            1
        );
        /*
    LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP1.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP2.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP3.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP4.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    */
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.SCRAP1.prefab,
            BiomeType.SeaTreaderPath_Path,
            0.33F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.SCRAP2.prefab,
            BiomeType.SeaTreaderPath_Path,
            0.33F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.SCRAP3.prefab,
            BiomeType.SeaTreaderPath_Path,
            0.33F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.SCRAP4.prefab,
            BiomeType.SeaTreaderPath_Path,
            0.33F,
            1
        );
        //LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP1.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);
        //LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP2.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);
        //LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP3.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);
        //LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP4.prefab, BiomeType.GrandReef_TreaderPath, 0.1F, 1);

        //LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_DIAMOND.prefab, BiomeType.Mountains_IslandCaveFloor, 0.33F, 1);
    }

    private static void AddCommands() {
        // ConsoleCommandsHandler.RegisterConsoleCommand<Action>("voidsig", VoidSpikesBiome.instance.activateSignal);

        //ConsoleCommandsHandler.RegisterConsoleCommand<Action<float>>("spawnVKelp", spawnVentKelp);
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<bool>>(
            "triggerVoidFX",
            f => VoidSpikeLeviathanSystem.instance.doDistantRoar(Player.main, true, f)
        );
        ConsoleCommandsHandler.RegisterConsoleCommand(
            "triggerVoidFlash",
            VoidSpikeLeviathanSystem.instance.doDebugFlash
        );
        ConsoleCommandsHandler.RegisterConsoleCommand(
            "voidLeviReefback",
            VoidSpikeLeviathan.MakeReefbackTest
        );
        ConsoleCommandsHandler.RegisterConsoleCommand(
            "dumpGameStats",
            () => GameStatistics.collect()
                .writeToFile(Path.Combine(Path.GetDirectoryName(ModDLL.Location)!, "statdump.xml"))
        );
        ConsoleCommandsHandler.RegisterConsoleCommand("cleanup", C2CUtil.cleanup);

        ConsoleCommandsHandler.RegisterConsoleCommand<Action<bool>>(
            "c2cSMMAnyW",
            b => AnywhereSeamothModuleCheatActive = b && SNUtil.CanUseDebug()
        );
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<bool>>(
            "c2cTrackerAll",
            b => TrackerShowAllCheatActive = b && SNUtil.CanUseDebug()
        );
        //ConsoleCommandsHandler.RegisterConsoleCommand<Action>("c2cTrackerSetAll", ExplorationTrackerPages.instance.markAllDiscovered);
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<bool>>(
            "c2cSGSA",
            b => FastSeaglideCheatActive = b && SNUtil.CanUseDebug()
        );
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<bool>>(
            "c2cFRHS",
            b => SeamothHeatSinkModule.FREE_CHEAT = b && SNUtil.CanUseDebug()
        );
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<float>>(
            "c2cENVHEAT",
            b => {
                if (SNUtil.CanUseDebug())
                    EnvironmentalDamageSystem.instance.TEMPERATURE_OVERRIDE = b;
            }
        );
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<bool>>(
            "c2cSMTempDebug",
            b => C2CMoth.temperatureDebugActive = b
        );
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<string>>(
            "c2cSignalUnlock",
            arg => {
                if (SNUtil.CanUseDebug())
                    UnlockSignal(arg);
            }
        );
        ConsoleCommandsHandler.RegisterConsoleCommand(
            "c2cpoi",
            POITeleportSystem.instance.jumpToPOI
        );
        ConsoleCommandsHandler.RegisterConsoleCommand(
            "c2cRFLdebug",
            () => SNUtil.WriteToChat(
                "Rocket launch error: " + FinalLaunchAdditionalRequirementSystem.instance.hasAllCargo() +
                "; Missing scan=" + LifeformScanningSystem.instance.hasScannedEverything()
            )
        );
        ConsoleCommandsHandler.RegisterConsoleCommand<Action>(
            "c2cRFLForce",
            FinalLaunchAdditionalRequirementSystem.instance.forceLaunch
        );
        ConsoleCommandsHandler.RegisterConsoleCommand<Action>("c2cRecover", () => RescueSystem.rescue());
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<bool>>(
            "debugMorale",
            arg => MoraleSystem.printMoraleForDebug =
                arg ? 0xffffffff ^ (uint)MoraleSystem.MoraleDebugFlags.STACKTRACE : 0
        );
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<int>>(
            "debugMoraleInt",
            arg => MoraleSystem.printMoraleForDebug = (uint)arg
        );
        ConsoleCommandsHandler.RegisterConsoleCommand(
            "debugMoraleDetail",
            MoraleSystem.setMoraleDebugFlags
        );
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<float>>(
            "c2cMORALEDELTA",
            arg => {
                if (SNUtil.CanUseDebug())
                    MoraleSystem.instance.shiftMorale(arg);
            }
        );
        //ConsoleCommandsHandler.RegisterConsoleCommand<Action>("oxygenite", () => Oxygenite.spawnAt(Player.main.transform.position));
    }
    /*
private static void spawnVentKelp(float dist) {
      GameObject obj = ObjectUtil.createWorldObject(C2CItems.kelp.ClassID, true, false);
      obj.SetActive(false);
      obj.transform.position = Player.main.transform.position+MainCamera.camera.transform.forward.normalized*dist;
      LargeWorld.main.streamer.cellManager.RegisterEntity(obj);
      obj.SetActive(true);
}*/

    private static void UnlockSignal(string name) {
        switch (name) {
            case "treaderpod":
                TreaderSignal.fireRadio();
                break;
            case "crashmesa":
                Story.StoryGoal.Execute(CrashMesaRadio.key, CrashMesaRadio.goalType);
                break;
            case "voidpod":
                VoidSpikesBiome.instance.fireRadio();
                break;
        }
    }

    private static void AddPdaEntries() {
        foreach (var e in PdaLocale.getEntries()) {
            var page = PDAManager.createPage(e);
            if (e.hasField("audio"))
                page.setVoiceover(e.getString("audio"));
            if (e.hasField("header"))
                page.setHeaderImage(
                    TextureManager.getTexture(ModDLL, "Textures/PDA/" + e.getString("header"))
                );
            page.register();
        }
    }
}