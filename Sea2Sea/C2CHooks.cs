using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Nautilus.Crafting;
using Nautilus.Handlers;
using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.Auroresource;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using ReikaKalseki.Exscansion;
using Story;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public static class C2CHooks {
    internal static readonly Vector3 DeepDegasiTablet = new(-638.9F, -506.0F, -941.3F);

    internal static readonly List<Vector3> PurpleTabletsToBreak = [
        new Vector3(291.19F, 30.94F, 848.86F), //south island shelf
        new Vector3(363.22F, 54.11F, 1015.80F), //internal island bridge
        new Vector3(383.18F, 18.21F, 1086.94F), //above gun entrance
        new Vector3(389.05F, -120.38F, 1150.36F), //this one is in that separate small cave loop
        new Vector3(320.14F, -91.85F, 1023.56F), //underwater near survivor cache
        new Vector3(-753.54F, 13.72F, -1107.50F), //floating island
    ];

    //internal static readonly Dictionary<Vector3, bool[]> purpleTabletsToRemoveParts = new Dictionary<Vector3, bool[]>();

    internal static readonly Vector3 CrashMesa = new(623.8F, -250.0F, -1105.2F);
    internal static readonly Vector3 MountainBaseGeoCenter = new(953, -344, 1453);
    internal static readonly Vector3 BkelpBaseGeoCenter = new(-1311.6F, -670.6F, -412.7F);
    internal static readonly Vector3 BkelpBaseNuclearReactor = new(-1325.67F, -660.60F, -392.70F);
    internal static readonly Vector3 TrailerBaseBioreactor = new(1314.94F, -80.2F, -412.97F);
    internal static readonly Vector3 LrpowerSealSetpieceCenter = new(-713.45F, -766.37F, -262.74F);
    internal static readonly Vector3 AuroraFront = new(1202.43F, -40.16F, 151.54F);
    internal static readonly Vector3 AuroraRepulsionGunTerminal = new(1029.51F, -8.7F, 35.87F);
    internal static readonly Vector3 LostRiverCachePanel = new(-1119.5F, -684.4F, -709.7F);

    internal static readonly Vector3 VoidWreckVoidPatch = new(-293.58F, -422.65F, -1753.40F);

    //internal static readonly Vector3 gunPoolBarrier = new Vector3(481.81F, -125.03F, 1257.85F);
    //internal static readonly Vector3 gunPoolBarrierTerminal = new Vector3();
    internal static readonly Vector3 GunCenter = new(460.6F, -99, 1208.4F);
    internal static readonly Vector3 MountainCenter = new(359.9F, 29F, 985.9F);

    internal static readonly Vector3 Lrnest = new(-809, -751, -321);
    internal static readonly Vector3 LrNestDefender = new(-754, -716, -279); //new Vector3(-761, -753, -285);

    internal static readonly Vector3 FcsWreckOpenableDoor = new(88.87F, -420.75F, 1449.10F);
    internal static readonly Vector3 FcsWreckBlockedDoor = new(93.01F, -421.27F, 1444.71F);

    internal static readonly PositionedPrefab AuroraStorageModule = new(
        "d290b5da-7370-4fb8-81bc-656c6bde78f8",
        new Vector3(991.5F, 3.21F, -30.99F),
        Quaternion.Euler(14.44F, 353.7F, 341.6F)
    );

    internal static readonly PositionedPrefab AuroraCyclopsModule = new(
        "049d2afa-ae76-4eef-855d-3466828654c4",
        new Vector3(872.5F, 2.69F, -0.66F),
        Quaternion.Euler(357.4F, 224.9F, 21.38F)
    );

    internal static readonly PositionedPrefab AuroraDepthModule = new(
        "74ec328c-e627-40ad-b373-97e384ec0385",
        new Vector3(903.52F, -0.16F, 16.06F),
        Quaternion.Euler(10.34F, 341.24F, 331.96F)
    );

    private static readonly HashSet<TechType> ScanToScannerRoom = [];

    private static readonly HashSet<string> FloaterRocks = [
        "44396d05-0910-4b4d-a046-119fab3512a5",
        "7637d968-4878-46a5-adf5-aa9e21fe3ddc",
        "9a9cdb4e-f110-412d-b16b-b9ace904b569",
        "a7b35deb-1ac7-4fb8-8393-c0252cbf6d23",
        "d4ad48a9-67fa-4b34-8447-5cd6a69d1270",
        "e3d778b5-a81e-4b64-8dd6-910fb22772db",
        "f895696c-cdc6-4427-a87f-2b62666ea0cb",
    ];

    private static readonly HashSet<string> AuroraFires = [
        "14bbf7f0-4276-48bf-868b-317b366edd16",
        "3877d31d-37a5-4c94-8eef-881a500c58bc",
        "afe53ea1-d2a8-4f76-8ffb-d41ff6046b52",
    ];

    private static readonly Dictionary<string, Color> AuroraPrawnFireColors = new() {
        { "xFireFlame", new Color(0, 0.67F, 2) },
        { "xFireCurl", new Color(1, 1, 1) },
        { "xAmbiant_Sparks", new Color(0, 1, 1) },
        { "xAmbiant_Ashes", new Color(0.1F, 0.1F, 1) },
        { "x_Fire_CrossPlanes", new Color(0.67F, 0.43F, 1) },
        { "x_Fire_GroundPlane", new Color(0.24F, 0.57F, 1) },
        { "x_SmokeLight_Cylindrical", new Color(0.67F, 0.72F, 0.97F) },
        { "x_Fire_Cylindrical", new Color(0.24F, 0.51F, 1) },
    };

    private static Oxygen _playerBaseO2;

    private static float _nextSanctuaryPromptCheckTime = -1;
    private static float _nextBkelpBaseAmbCheckTime = -1;

    public static bool InBKelpBase { get; private set; }

    private static float _nextBkelpBaseAmbTime = -1;
    private static float _nextCameraEmpTime = -1;

    private static float _foodToRestore;
    private static float _waterToRestore;

    public static bool SkipPlayerTick = false;
    public static bool SkipBiomeCheck = false;
    public static bool SkipTemperatureCheck = false;
    public static bool SkipSkyApplierSpawn = false;
    public static bool SkipRadiationLevel = false;
    public static bool SkipFruitPlantTick = false;
    public static bool SkipScannerTick = false;
    public static bool SkipCompassCalc = false;
    public static bool SkipPodTick = false;
    public static bool SkipSeamothTick = false;
    public static bool SkipCrawlerTick = false;
    public static bool SkipTreaderTick = false;
    public static bool SkipVoidLeviTick = false;
    public static bool SkipMagnetic = false;
    public static bool SkipWaveBob = false;
    public static bool SkipRaytrace = false;
    public static bool SkipReach = false;
    public static bool SkipResourceSpawn = false;
    public static bool SkipEnviroDamage = false;
    public static bool SkipO2 = false;
    public static bool SkipStalkerShiny = false;
    public static bool SkipRocketTick = false;

    private static TechType _techPistol = TechType.None;
    private static bool _searchedTechPistol;

    private static float _lastO2PipeTime = -1;

    private static bool _playerDied;

    private static float _lastSaveAlertTime = -1;

    private static float _lastCuddlefishPlay = -1;

    private static TechType LoadTechPistol() {
        if (_techPistol == TechType.None && !_searchedTechPistol) {
            _techPistol = SNUtil.GetTechType("TechPistol");
            if (DIHooks.IsWorldLoaded())
                _searchedTechPistol = true;
        }

        return _techPistol;
    }

    static C2CHooks() {
        SNUtil.Log("Initializing C2CHooks");
        DIHooks.OnWorldLoadedEvent += OnWorldLoaded;
        DIHooks.OnDamageEvent += RecalculateDamage;
        DIHooks.OnItemPickedUpEvent += OnItemPickedUp;
        DIHooks.OnSkyApplierSpawnEvent += OnSkyApplierSpawn;

        DIHooks.GetBiomeEvent += GetBiomeAt;
        DIHooks.GetTemperatureEvent += GetWaterTemperature;

        DIHooks.OnBaseTickEvent += TickBase;
        DIHooks.OnPlayerTickEvent += TickPlayer;
        DIHooks.GetPlayerInputEvent += ControlPlayerInput;

        DIHooks.OnSeamothModulesChangedEvent += UpdateSeamothModules;
        DIHooks.OnCyclopsModulesChangedEvent += UpdateCyclopsModules;
        DIHooks.OnPrawnModulesChangedEvent += UpdatePrawnModules;
        DIHooks.OnSeamothModuleUsedEvent += UseSeamothModule;

        DIHooks.SeamothDischargeEvent += PulseSeamothDefence;
        DIHooks.OnSeamothSonarUsedEvent += PingSeamothSonar;
        DIHooks.OnTorpedoFireEvent += OnTorpedoFired;
        DIHooks.OnTorpedoExplodeEvent += OnTorpedoExploded;

        DIHooks.OnSonarUsedEvent += PingAnySonar;

        DIHooks.OnEmpHitEvent += OnEmpHit;

        DIHooks.ConstructabilityEvent += ApplyGeyserFilterBuildability;
        DIHooks.BreathabilityEvent += CanPlayerBreathe;

        DIHooks.GetSwimSpeedEvent += GetSwimSpeed;

        DIHooks.SpawnTreaderChunk += OnTreaderChunkSpawn;

        DIHooks.CrashfishExplodeEvent += OnCrashfishExplode;

        //DIHooks.fogCalculateEvent += interceptChosenFog;

        DIHooks.RadiationCheckEvent += (ch) => {
            if (!SkipRadiationLevel)
                ch.Value = GetRadiationLevel(ch);
        };

        DIHooks.ItemTooltipEvent += GenerateItemTooltips;
        DIHooks.BulkheadLaserHoverEvent += InterceptBulkheadLaserCutter;

        DIHooks.KnifeAttemptEvent += TryKnife;
        DIHooks.OnKnifedEvent += OnKnifed;
        DIHooks.KnifeHarvestEvent += InterceptItemHarvest;

        DIHooks.OnFruitPlantTickEvent += TickFruitPlant;

        DIHooks.ReaperGrabVehicleEvent += OnReaperGrab;
        DIHooks.CyclopsDamageEvent += OnCyclopsDamage;

        DIHooks.VehicleEnterEvent += OnVehicleEnter;

        DIHooks.ScannerRoomTickEvent += AvoliteSpawner.Instance.TickMapRoom;

        DIHooks.SolarEfficiencyEvent += (ch) => ch.Value = GetSolarEfficiencyLevel(ch);
        DIHooks.DepthCompassEvent += GetCompassDepthLevel;
        DIHooks.PropulsibilityEvent += ModifyPropulsibility;
        //DIHooks.droppabilityEvent += modifyDroppability;	    	
        DIHooks.ModuleFireCostEvent += (ch) => ch.Value = GetModuleFireCost(ch);

        DIHooks.EquipmentTypeCheckEvent += ChangeEquipmentCompatibility;

        DIHooks.OnStasisRifleFreezeEvent += (ch) => ch.ApplyKinematicChange = !OnStasisFreeze(ch.Sphere, ch.Body);
        DIHooks.OnStasisRifleUnfreezeEvent += (ch) => ch.ApplyKinematicChange = !OnStasisUnFreeze(ch.Sphere, ch.Body);

        DIHooks.RespawnEvent += OnPlayerRespawned;
        DIHooks.ItemsLostEvent += OnItemsLost;
        DIHooks.SelfScanEvent += OnSelfScan;
        DIHooks.ScanCompleteEvent += OnScanComplete;

        DIHooks.TryEatEvent += TryEat;

        DIHooks.WaterFilterSpawnEvent += OnWaterFilterSpawn;

        DIHooks.OnPlayWithCuddlefish += OnCuddlefishPlay;
        DIHooks.OnRocketStageCompletedEvent += OnRocketStageComplete;
        DIHooks.OnSleepEvent += OnSleep;
        DIHooks.OnEatEvent += OnEat;
        DIHooks.GetFoodRateEvent += AffectFoodRate;

        DIHooks.TargetabilityEvent += CheckTargetingSkip;

        SNUtil.Log("Finished registering main DI event callbacks");

        KnownTech.onAdd += OnTechUnlocked;

        BaseSonarPinger.onBaseSonarPingedEvent += OnBaseSonarPinged;
        BaseDrillableGrinder.onDrillableGrindEvent += GetGrinderDrillableDrop;

        LavaBombTag.onLavaBombImpactEvent += OnLavaBombHit;
        ExplodingAnchorPod.onExplodingAnchorPodDamageEvent += OnAnchorPodExplode;
        PredatoryBloodvine.onBloodKelpGrabEvent += OnBloodKelpGrab;
        VoidTongueTag.onVoidTongueGrabEvent += OnVoidTongueGrab;
        VoidTongueTag.onVoidTongueReleaseEvent += OnVoidTongueRelease;
        PlanktonCloudTag.onPlanktonActivationEvent += OnPlanktonActivated;
        VoidBubble.voidBubbleSpawnerTickEvent += TickVoidBubbles;
        VoidBubble.voidBubbleTickEvent += TickVoidBubble;
        MushroomVaseStrand.vaseStrandFilterCollectEvent += OnCollectFromVaseStrand;

        FallingMaterialSystem.ImpactEvent += OnMeteorImpact;

        SNUtil.Log("Finished registering event callbacks");

        ScanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType);
        ScanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType);
        ScanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType);
        ScanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.OBSIDIAN).TechType);
        ScanToScannerRoom.Add(CustomMaterials.getItem(CustomMaterials.Materials.OXYGENITE).TechType);
        ScanToScannerRoom.Add(C2CItems.voidSpikeLevi.TechType);
        ScanToScannerRoom.Add(C2CItems.alkali.TechType);
        ScanToScannerRoom.Add(C2CItems.healFlower.TechType);
        ScanToScannerRoom.Add(C2CItems.kelp.TechType);
        ScanToScannerRoom.Add(C2CItems.broodmother.TechType);
    }

    //[System.Runtime.InteropServices.DllImport("WorldgenCheck.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
    //public static extern bool handleWorldgenIntegrity(string s);

    public static void OnWorldLoaded() {
        if (WorldgenIntegrityChecks.checkWorldgenIntegrity(false))
            WorldgenIntegrityChecks.throwError();

        MoraleSystem.instance.reset();

        Inventory.main.equipment.onEquip += OnEquipmentAdded;
        Inventory.main.equipment.onUnequip += OnEquipmentRemoved;

        //remove all since field does not serialize
        Inventory.main.container.forEachOfType(
            C2CItems.emperorRootOil.TechType,
            ii => Inventory.main.container.forceRemoveItem(ii)
        );

        BrokenTablet.updateLocale();

        PDAScanner.mapping[TechType.SeaEmperorJuvenile] =
            PDAScanner.mapping[TechType.SeaEmperorBaby]; //make juveniles scannable into baby page, so not missable

        PDAManager.getPage("rescuewarp").unlock(false);

        Player.main.playerRespawnEvent.AddHandler(
            Player.main,
            new UWE.Event<Player>.HandleFunction(ep => {
                    if (!ep.lastValidSub && !ep.lastEscapePod && !EscapePod.main) {
                        ep.SetPosition(new Vector3(0, -5, 0));
                        ep.SetMotorMode(Player.MotorMode.Dive);
                    }
                }
            )
        );

        // TODO: FCS Compat
        // if (FCSIntegrationSystem.instance.isLoaded()) {
        //     FCSIntegrationSystem.instance.initializeTechUnlocks();
        //     BaseBioReactor.charge[FCSIntegrationSystem.instance.getBiofuel()] = 18000;
        // }

        VoidSpikesBiome.instance.onWorldStart();
        UnderwaterIslandsFloorBiome.instance.onWorldStart();

        C2CProgression.Instance.OnWorldLoaded();

        MoveToExploitable("SeaCrown");
        MoveToExploitable("SpottedLeavesPlant");
        MoveToExploitable("OrangeMushroom");
        MoveToExploitable("SnakeMushroom");
        MoveToExploitable("PurpleVasePlant");

        foreach (var k in new List<string>(Language.main.strings.Keys)) {
            //SNUtil.log(k+" :>");
            //SNUtil.log(Language.main.Get(k));
            var k2 = k.ToLowerInvariant();
            if (k2.Contains("tooltip") || k2.Contains("desc") || k2.Contains("ency"))
                continue;
            var s = Language.main.Get(k);
            if (s.ToLowerInvariant().Contains("creepvine"))
                continue;
            var s0 = s;
            s = s.Replace(" seed", " Sample");
            //s = s.Replace(" spore", " Sample");
            s = s.Replace(" Seed", " Sample");
            //s = s.Replace(" Spore", " Sample");
            if (s != s0)
                CustomLocaleKeyDatabase.registerKey(k, s);
        }

        CustomLocaleKeyDatabase.registerKey(
            "EncyDesc_Aurora_DriveRoom_Terminal1",
            Language.main.Get("EncyDesc_Aurora_DriveRoom_Terminal1").Replace("from 8 lifepods", "from 14 lifepods")
                .Replace("T+8hrs: 1", "T+8hrs: 7")
        );
        CustomLocaleKeyDatabase.registerKey(
            "EncyDesc_WaterFilter",
            Language.main.Get("EncyDesc_WaterFilter") +
            "\n\nNote: In highly mineralized regions, salt collection is both accelerated and may yield additional byproducts."
        );
        var ghostLeviNote =
            " in most cases, but may hold the key to protecting oneself from certain aspects of the ambient environment.";
        CustomLocaleKeyDatabase.registerKey(
            "EncyDesc_" + TechType.GhostLeviathan.AsString(),
            Language.main.Get("EncyDesc_" + TechType.GhostLeviathan.AsString()) + ghostLeviNote
        );
        CustomLocaleKeyDatabase.registerKey(
            "EncyDesc_" + TechType.GhostLeviathanJuvenile.AsString(),
            Language.main.Get("EncyDesc_" + TechType.GhostLeviathanJuvenile.AsString()) + ghostLeviNote
        );

        var key = "EncyDesc_" + TechType.SpottedLeavesPlant.AsString();
        CustomLocaleKeyDatabase.registerKey(
            key,
            Language.main.Get(key) + "\n\nThese compounds appear to be compatible with human digestion."
        );

        CustomLocaleKeyDatabase.registerKey(
            "Need_laserCutterBulkhead_Chit",
            SeaToSeaMod.MiscLocale.getEntry("bulkheadLaserCutterUpgrade").getString("error")
        );
        CustomLocaleKeyDatabase.registerKey(
            "Tooltip_" + TechType.MercuryOre.AsString(),
            SeaToSeaMod.MiscLocale.getEntry("MercuryDesc").desc
        );
        CustomLocaleKeyDatabase.registerKey("EncyDesc_Mercury", SeaToSeaMod.MiscLocale.getEntry("MercuryDesc").pda);
        CustomLocaleKeyDatabase.registerKey(
            "Tooltip_" + TechType.PrecursorKey_Red.AsString(),
            SeaToSeaMod.ItemLocale.getEntry("redkey").desc
        );
        CustomLocaleKeyDatabase.registerKey(
            "Tooltip_" + TechType.PrecursorKey_White.AsString(),
            SeaToSeaMod.ItemLocale.getEntry("whitekey").desc
        );

        Campfire.updateLocale(); //call after the above locale init
        KeypadCodeSwappingSystem.instance.patchEncyPages();

        CustomLocaleKeyDatabase.registerKey(
            SeaToSeaMod.TunnelLight.TechType.AsString(),
            Language.main.Get(TechType.LEDLight)
        );
        CustomLocaleKeyDatabase.registerKey(
            "Tooltip_" + SeaToSeaMod.TunnelLight.TechType.AsString(),
            Language.main.Get("Tooltip_" + TechType.LEDLight.AsString())
        );

        CustomLocaleKeyDatabase.registerKey(
            SeaToSeaMod.DeadMelon.TechType.AsString(),
            Language.main.Get(TechType.MelonPlant)
        );

        EcoceanMod.lavaShroom.pdaPage.append(
            "\n\n" + SeaToSeaMod.MiscLocale.getEntry("Appends").getString("lavashroom")
        );

        CustomLocaleKeyDatabase.registerKey(
            "Tooltip_" + TechType.VehicleHullModule3.AsString(),
            Language.main.Get("Tooltip_" + TechType.VehicleHullModule3.AsString()).Replace("maximum", "900m")
        );

        StoryGoalCustomEventHandler.main.sunbeamGoals[StoryGoalCustomEventHandler.main.sunbeamGoals.Length - 2]
            .trigger = SeaToSeaMod.SunbeamCountdownTrigger.key;
    }

    private static void MoveToExploitable(string key) {
        var data = PDAEncyclopedia.mapping[key]; /*
        TreeNode root = PDAEncyclopedia.tree;
        TreeNode node = root;
        foreach (string s in data.path.Split('/')) {
            node = node[s];
        }
        if (node == null) {
            SNUtil.log("Found no ency node for "+key+" in "+data.path);
            return;
        }*/
        //node.parent.RemoveNode(node);
        //root[3][1][0].AddNode(node);
        data.path = data.path.Replace("Sea", "Exploitable").Replace("Land", "Exploitable");
        data.nodes = PDAEncyclopedia.ParsePath(data.path);
    }

    public static void TickBase(BaseRoot sub) {
        if (sub.IsLeaking()) {
            var leak = EnvironmentalDamageSystem.Instance.GetLrPowerLeakage(sub.gameObject); //ranges from 1 to 1.75
            var f = 1 + leak * 4; //1/s base, LR 5-8/s
            var arr = sub.flood.cellWaterLevel;
            float sum = 0;
            for (var i = 0; i < arr.Length; i++) {
                sum += arr[i];
            }

            f *= sum / arr.Length;
            f *= Mathf.Sqrt(arr.Length);
            sub.powerRelay.ConsumeEnergy(Time.deltaTime * f, out var trash);
        }
    }

    public static void TickPlayer(Player ep) {
        if (_playerDied) {
            C2CUtil.setupDeathScreen();
            return;
        }

        if (SkipPlayerTick || !ep || !DIHooks.IsWorldLoaded())
            return;

        //SNUtil.writeToChat(WorldUtil.getRegionalDescription(ep.transform.position));

        if (_playerBaseO2 == null) {
            foreach (Oxygen o in Player.main.oxygenMgr.sources) {
                if (o.isPlayer) {
                    _playerBaseO2 = o;
                    break;
                }
            }
        }

        var time = DayNightCycle.main.timePassedAsFloat;

        if (ep.GetBiomeString() == "observatory") {
            MoraleSystem.instance.shiftMorale(MoraleSystem.OBSERVATORY_CONSTANT_BONUS * Time.deltaTime);
            ObservatoryDiscoverySystem.instance.tick(ep);
        }

        MoraleSystem.instance.tick(ep);

        if (Input.GetKeyDown(SeaToSeaMod.Keybinds.GetBinding(C2CModOptions.PropGunSwap))) {
            C2CUtil.swapRepulsionCannons();
        }

        if (IngameMenu.main &&
            Time.timeSinceLevelLoad - IngameMenu.main.lastSavedStateTime >=
            SeaToSeaMod.ModConfig.getFloat(C2CConfig.ConfigEntries.SAVETHRESH) * 60F && time - _lastSaveAlertTime >=
            SeaToSeaMod.ModConfig.getFloat(C2CConfig.ConfigEntries.SAVECOOL) * 60F && AllowSaving(true)) {
            SNUtil.WriteToChat(
                "It has been " +
                Utils.PrettifyTime((int)(Time.timeSinceLevelLoad - IngameMenu.main.lastSavedStateTime)) +
                " since you last saved; you should do so again soon."
            );
            _lastSaveAlertTime = time;
        }

        // TODO: FCS Compat
        // if (FCSIntegrationSystem.instance.isLoaded())
        //     FCSIntegrationSystem.instance.tickNotifications(time);

        if (DeIntegrationSystem.Instance.IsLoaded())
            DeIntegrationSystem.Instance.TickVoidThalassaceanSpawner(ep);

        LifeformScanningSystem.instance.tick(time);
        DataCollectionTracker.instance.tick(time);
        EmperorRootOil.tickInventory(ep, time);

        if (Camera.main && Vector3.Distance(ep.transform.position, Camera.main.transform.position) > 5) {
            if (VoidSpikesBiome.instance.getDistanceToBiome(Camera.main.transform.position, true) < 200)
                WaterBiomeManager.main.GetComponent<WaterscapeVolume>().fogEnabled = true;
        }

        ItemUnlockLegitimacySystem.instance.tick(ep);

        if (Time.deltaTime > 0)
            BKelpBumpWormSpawner.tickSpawnValidation(ep);

        if (LiquidBreathingSystem.Instance.HasTankButNoMask()) {
            var ox = Inventory.main.equipment.GetItemInSlot("Tank").item.gameObject.GetComponent<Oxygen>();
            ep.oxygenMgr.UnregisterSource(ox);
            ep.oxygenMgr.UnregisterSource(_playerBaseO2);
        } else if (LiquidBreathingSystem.Instance.HasLiquidBreathing()) {
            //SNUtil.writeToChat("Tick liquid breathing: "+LiquidBreathingSystem.instance.isLiquidBreathingActive(ep));
            var ox = Inventory.main.equipment.GetItemInSlot("Tank").item.gameObject.GetComponent<Oxygen>();
            if (LiquidBreathingSystem.Instance.IsLiquidBreathingActive(ep)) {
                LiquidBreathingSystem.Instance.TickLiquidBreathing(true, true);
                ep.oxygenMgr.UnregisterSource(_playerBaseO2);
                ep.oxygenMgr.RegisterSource(ox);
            } else {
                LiquidBreathingSystem.Instance.TickLiquidBreathing(true, false);
                ep.oxygenMgr.UnregisterSource(ox);
                ep.oxygenMgr.RegisterSource(_playerBaseO2);
                var add = Mathf.Min(ep.oxygenMgr.oxygenUnitsPerSecondSurface, ox.oxygenCapacity - ox.oxygenAvailable) *
                          Time.deltaTime;
                if (add > 0.01) {
                    if (LiquidBreathingSystem.Instance.TryFillPlayerO2Bar(ep, ref add)) {
                        ox.AddOxygen(add);
                        //LiquidBreathingSystem.instance.onAddO2ToBar(add);
                    }
                }
            }
        } else {
            LiquidBreathingSystem.Instance.TickLiquidBreathing(false, false);
            ep.oxygenMgr.RegisterSource(_playerBaseO2);
            if (time - LiquidBreathingSystem.Instance.GetLastUnequippedTime() < 0.5)
                ep.oxygenMgr.RemoveOxygen(ep.oxygenMgr.GetOxygenAvailable());
        }

        if (!PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.SanctuaryPrompt).key))
            SeaToSeaMod.SanctuaryDirectionHint.deactivate();
        if (!VoidSpikesBiome.instance.isRadioFired())
            SeaToSeaMod.VoidSpikeDirectionHint.deactivate();
        if (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KelpCavePrompt).key) ||
            PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KelpCavePromptLate).key))
            StoryGoal.Execute("KelpCaveHint", Story.GoalType.Story);
        //	C2CProgression.spawnPOIMarker("kelpCavePOI", C2CProgression.instance.dronePDACaveEntrance.setY(-5));
        //if (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.DuneArchPrompt).key))
        //C2CProgression.spawnPOIMarker("duneArch", POITeleportSystem.getPOI("dunearch"));

        var distsq = (ep.transform.position - CrashMesa).sqrMagnitude - 400;
        if (time >= _nextSanctuaryPromptCheckTime &&
            !PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.SanctuaryPrompt).key)) {
            _nextSanctuaryPromptCheckTime = time + 1;
            if (distsq < 2500 || Vector3.Distance(ep.transform.position, AuroraFront) < 144 ||
                Vector3.Distance(ep.transform.position, TrailerBaseBioreactor) < 200 || Vector3.Distance(
                    ep.transform.position,
                    CrashZoneSanctuaryBiome.biomeCenter
                ) < 200) {
                Player.main.gameObject.EnsureComponent<DelayedPromptsCallback>().Invoke("TriggerSanctuary", 20);
            }
        }

        if (distsq < 25 * 25 || (distsq <= 250 * 250 &&
                                 UnityEngine.Random.Range(0F, 1F) <=
                                 0.075F * Time.timeScale * (distsq <= 10000 ? 2.5F : 1))) {
            var tgt = EcoRegionManager.main.FindNearestTarget(
                EcoTargetType.Leviathan,
                CrashMesa,
                eco => eco.GetGameObject().GetComponent<ReaperLeviathan>(),
                6
            );
            if (tgt != null && (tgt.GetPosition() - CrashMesa).sqrMagnitude >= Mathf.Max(distsq, 225)) {
                var go = tgt.GetGameObject();
                var v = ep.GetVehicle();
                var hit = v ? v.gameObject : ep.gameObject;
                var pos = distsq <= 2500
                    ? hit.transform.position
                    : MathUtil.getRandomVectorAround(CrashMesa, 40).SetY(CrashMesa.y);
                go.EnsureComponent<C2CReaper>().forceAggression(hit, pos);
            }
        }

        VoidSpikeLeviathanSystem.instance.tick(ep);

        ExplorationTrackerPages.instance.tick();

        if (ep.currentSub == null && UnityEngine.Random.Range(0, (int)(10 / Time.timeScale)) == 0) {
            if (ep.GetVehicle() == null) {
                float ventDist = -1;
                var tgt = EcoRegionManager.main.FindNearestTarget(
                    EcoTargetType.HeatArea,
                    ep.transform.position,
                    null,
                    3
                );
                if (tgt != null)
                    ventDist = Vector3.Distance(tgt.GetPosition(), ep.transform.position);
                if (ventDist >= 0 && ventDist <= 25) {
                    var f = Math.Min(1, (40 - ventDist) / 32F);
                    foreach (var item in Inventory.main.container) {
                        if (item != null && item.item) {
                            var b = item.item.gameObject.GetComponentInChildren<Battery>();
                            if (b != null && Mathf.Approximately(b.capacity, C2CItems.t2Battery.capacity)) {
                                b.charge = Math.Min(b.charge + 0.5F * f, b.capacity);
                                continue;
                            }

                            var e = item.item.gameObject.GetComponentInChildren<EnergyMixin>();
                            if (e != null && e.battery != null && Mathf.Approximately(
                                    e.battery.capacity,
                                    C2CItems.t2Battery.capacity
                                )) {
                                //SNUtil.writeToChat("Charging "+item.item+" by factor "+f+", d="+ventDist);
                                e.AddEnergy(0.5F * f);
                            }
                        }
                    }
                }
            }
        }

        if (time >= _nextBkelpBaseAmbCheckTime) {
            _nextBkelpBaseAmbCheckTime = time + UnityEngine.Random.Range(0.5F, 2.5F);
            InBKelpBase = Vector3.Distance(ep.transform.position, BkelpBaseGeoCenter) <= 60;
            if (InBKelpBase) {
                StoryGoal.Execute("SeeBkelpBase", Story.GoalType.Story);
                if (time >= _nextBkelpBaseAmbTime) {
                    SNUtil.Log("Queuing bkelp base ambience @ " + ep.transform.position);
                    VanillaMusic.WRECK.play();
                    _nextBkelpBaseAmbTime = DayNightCycle.main.timePassedAsFloat + UnityEngine.Random.Range(60F, 90F);
                }
            } else {
                VanillaMusic.WRECK.disable();
            }
        }
    }

    public static void OnEquipmentAdded(string slot, InventoryItem item) {
        if (item.item.GetTechType() == C2CItems.liquidTank.TechType)
            LiquidBreathingSystem.Instance.OnEquip();
    }

    public static void OnEquipmentRemoved(string slot, InventoryItem item) {
        if (item.item.GetTechType() == C2CItems.liquidTank.TechType)
            LiquidBreathingSystem.Instance.OnUnequip();
    }

    public static void TickO2Bar(uGUI_OxygenBar gui) {
        if (SkipO2)
            return;
        LiquidBreathingSystem.Instance.UpdateOxygenGUI(gui);
    }

    public static float GetO2RedPulseTime(float orig) {
        return SkipO2 ? orig : LiquidBreathingSystem.Instance.IsO2BarFlashingRed() ? 6 : orig;
    }

    public static void CanPlayerBreathe(DIHooks.BreathabilityCheck ch) {
        if (SkipO2)
            return;
        //SNUtil.writeToChat(orig+": "+p.IsUnderwater()+" > "+Inventory.main.equipment.GetCount(SeaToSeaMod.rebreatherV2.TechType));
        if (!LiquidBreathingSystem.Instance.IsO2BarAbleToFill(ch.Player))
            ch.Breathable = false;
    }

    public static float AddO2ToPlayer(OxygenManager mgr, float f) {
        if (SkipO2)
            return f;
        if (!LiquidBreathingSystem.Instance.IsO2BarAbleToFill(Player.main))
            f = 0;
        return f;
    }

    public static void AddOxygenAtSurfaceMaybe(OxygenManager mgr, float time) {
        if (SkipO2)
            return;
        if (LiquidBreathingSystem.Instance.IsO2BarAbleToFill(Player.main)) {
            //SNUtil.writeToChat("Add surface O2");
            mgr.AddOxygenAtSurface(time);
        }
    }

    public static void GetBiomeAt(DIHooks.BiomeCheck b) {
        if (SkipBiomeCheck)
            return;
        if (VoidSpikesBiome.instance.IsInBiome(b.Position)) {
            b.SetValue(VoidSpikesBiome.instance.biomeName);
            b.LockValue();
            //if (BiomeBase.logBiomeFetch)
            //	SNUtil.writeToChat("Biome WBM fetch overridden to "+VoidSpikesBiome.biomeName);
        } else if (UnderwaterIslandsFloorBiome.instance.isInBiome(b.OriginalValue, b.Position)) {
            b.SetValue(UnderwaterIslandsFloorBiome.instance.biomeName);
            b.LockValue();
            //if (BiomeBase.logBiomeFetch)
            //	SNUtil.writeToChat("Biome WBM fetch overridden to "+UnderwaterIslandsFloorBiome.biomeName);
        } /*
        if (Vector3.Distance(dmg.target.transform.position, bkelpBaseGeoCenter) <= 60 && !dmg.target.FindAncestor<Vehicle>()) {
            b.setValue(BKelpBaseBiome.biomeName);
            b.lockValue();
        }*/ else if (CrashZoneSanctuaryBiome.instance.IsInBiome(b.Position)) {
            b.SetValue(CrashZoneSanctuaryBiome.instance.biomeName);
            b.LockValue();
            //if (BiomeBase.logBiomeFetch)
            //	SNUtil.writeToChat("Biome WBM fetch overridden to "+UnderwaterIslandsFloorBiome.biomeName);
        } else if (Vector3.Distance(b.Position, VoidWreckVoidPatch) <= 40) {
            b.SetValue(VanillaBiomes.Void.MainID);
            b.LockValue();
        }
    }

    public static void GetSwimSpeed(DIHooks.SwimSpeedCalculation ch) {
        if (GameModeUtils.currentGameMode != GameModeOption.Survival)
            return;
        var morale = MoraleSystem.instance.moralePercentage;
        if (morale < 25) {
            ch.SetValue(ch.GetValue() * Mathf.Lerp(0.5F, 1F, morale / 25F));
        }

        if (Player.main.motorMode != Player.MotorMode.Dive)
            return;
        //SNUtil.writeToChat("Get swim speed, was "+f+", has="+LiquidBreathingSystem.instance.hasLiquidBreathing());
        if (LiquidBreathingSystem.Instance.HasLiquidBreathing())
            ch.SetValue(ch.GetValue() - 0.1F); //was 0.25
        if (WorldUtil.isInDRF(Player.main.transform.position))
            ch.SetValue(ch.GetValue() * 0.5F);
        if ((Player.main.transform.position - CrashMesa).sqrMagnitude <= 2500) {
            ch.SetValue(ch.GetValue() * 0.4F);
        }
    }

    public static float GetSeaglideSpeed(float f) { //1.45 by default
        if (SeaToSeaMod.FastSeaglideCheatActive)
            return 40;
        //SNUtil.writeToChat("Get SG speed, was "+f+", has="+Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity));
        if (IsHeldToolAzuritePowered()) {
            var bonus = 0.75F; //was 0.55 then 0.95
            var depth = Player.main.GetDepth();
            var depthFactor = depth <= 50 ? 1 : 1 - (depth - 50) / 350F;
            if (depthFactor > 0) {
                f += bonus * depthFactor;
            }
        }

        if (WorldUtil.isInDRF(Player.main.transform.position))
            f *= 0.5F;
        return f;
    }

    public static float GetScannerSpeed(float f) { //f is a divisor, scanTime
        if (IsHeldToolAzuritePowered()) {
            f *= 0.5F; //double speed
        }

        return f;
    }
    /* DO NOT USE - RISKS VOIDING
    public static float getBuilderSpeed(float f) { //f is a divisor, item count
        if (isHeldToolAzuritePowered()) {
            f *= 0.667F; //1.5x speed
        }
        return f;
    }*/

    public static float GetLaserCutterSpeed(LaserCutter lc) { //25 by default
        var amt = lc.healthPerWeld;
        if (IsHeldToolAzuritePowered())
            amt *= 1.5F;
        return amt;
    }

    public static float GetRepairSpeed(Welder lc) { //10 by default
        var amt = lc.healthPerWeld;
        if (IsHeldToolAzuritePowered())
            amt *= 2F;
        return amt;
    }

    public static float GetConstructableSpeed() {
        return NoCostConsoleCommand.main.fastBuildCheat ? 0.01F :
            !GameModeUtils.RequiresIngredients() ? 0.2F :
            StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.AuroraTerminal.key) ? 0.67F : 1F;
    }

    public static float GetVehicleConstructionSpeed(ConstructorInput inp, TechType made, float time) {
        if (StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.AuroraTerminal.key))
            time *= made == TechType.RocketBase ? 0.8F : 0.5F;
        else
            time *= made == TechType.Seamoth ? 2F : 1.5F;
        return time;
    }

    public static float GetRocketConstructionSpeed(float time) {
        time *= StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.AuroraTerminal.key) ? 0.8F : 1.6F;
        return time;
    }

    public static bool GetFabricatorTime(DIHooks.CraftTimeCalculation calc) {
        if (StoryGoalManager.main.IsGoalComplete(SeaToSeaMod.AuroraTerminal.key)) {
            calc.CraftingDuration *= (float)MathUtil.linterpolate(calc.CraftingDuration, 1, 2, 1, 0.5, true);
            calc.CraftingDuration = Mathf.Min(calc.CraftingDuration, 10);
        } else {
            calc.CraftingDuration *= 1.5F;
        }

        if (GameModeUtils.currentGameMode == GameModeOption.Survival &&
            !BepInExUtil.IsModLoaded(PluginIDs.RadialTabs)) {
            var morale = MoraleSystem.instance.moralePercentage;
            float f = 1;
            if (morale < 10) {
                f = Mathf.Lerp(6F, 3F, morale / 10F);
            } else if (morale < 25) {
                f = (float)MathUtil.linterpolate(morale, 10, 25, 3, 1.5, true);
            } else if (morale < 50) {
                f = (float)MathUtil.linterpolate(morale, 25, 50, 1.5, 1, true);
            } else if (morale >= 90) {
                f = (float)MathUtil.linterpolate(morale, 90, 100, 1, 0.5F, true);
            }

            calc.CraftingDuration *= f;
            //SNUtil.writeToChat("Morale is " + morale.ToString("0.0") + " -> "+f.ToString("0.00")+"x duration");
        }

        return true;
    }

    public static float GetRadialTabAnimSpeed(float orig) {
        if (GameModeUtils.currentGameMode != GameModeOption.Survival)
            return orig;
        var morale = MoraleSystem.instance.moralePercentage;
        float f = 1;
        if (morale < 10) {
            f = Mathf.Lerp(0.125F, 0.33F, morale / 10F);
        } else if (morale < 25) {
            f = (float)MathUtil.linterpolate(morale, 10, 25, 0.33, 0.67, true);
        } else if (morale < 50) {
            f = (float)MathUtil.linterpolate(morale, 25, 50, 0.67, 1, true);
        }

        return f * orig;
    }

    public static float GetPropulsionCannonForce(PropulsionCannon prop) {
        var ret = prop.attractionForce;
        if (IsHeldToolAzuritePowered())
            ret *= 3;
        var temp = WaterTemperatureSimulation.main.GetTemperature(Player.main.transform.position);
        if (temp >= 100)
            ret *= Mathf.Max(0.04F, 1F / ((temp - 99) / 50F));
        return ret;
    }

    public static float GetPropulsionCannonThrowForce(PropulsionCannon prop) {
        var ret = prop.shootForce;
        if (IsHeldToolAzuritePowered())
            ret *= 1.5F;
        return ret;
    }

    public static float GetRepulsionCannonThrowForce(RepulsionCannon prop) {
        var ret = RepulsionCannon.shootForce;
        if (IsHeldToolAzuritePowered())
            ret *= 4;
        return ret;
    }

    public static void OnRepulsionCannonTryHit(RepulsionCannon prop, Rigidbody rb) {
        if (IsHeldToolAzuritePowered() && rb.gameObject.GetFullHierarchyPath().Contains("CaptainsQuarters_Keypad")) {
            var s = rb.GetComponent<StarshipDoor>();
            //SNUtil.writeToChat("S: "+s);
            if (s) {
                var go = s.gameObject;
                var door = rb.gameObject.getChildObject("Starship_doors_manual_01/Starship_doors_automatic");
                var rb2 = door.EnsureComponent<Rigidbody>();
                rb2.CopyObject(rb);
                s.GetComponent<StarshipDoorLocked>()
                    .destroy(
                        false
                    ); //need to do directly since removecomponent calls destroyImmediate, and this is an anim call
                s.destroy(false);
                //SNUtil.writeToChat("C: "+string.Join(", ", go.GetComponents<MonoBehaviour>().Select<Component, string>(c => c.GetType().Name).ToArray()));
                rb2.isKinematic = false;
                rb2.mass = 500;
                rb2.ResetCenterOfMass();
                rb2.ResetInertiaTensor();
                rb2.detectCollisions = true;
                rb2.transform.SetParent(null);
                foreach (var c in rb2.GetComponentsInChildren<Collider>()) {
                    c.enabled = false;
                }

                rb2.velocity = MainCamera.camera.transform.forward * 30F + Vector3.up * 7.5F;
                rb2.angularVelocity = MathUtil.getRandomVectorAround(Vector3.zero, 2.5F);
                var fd = door.EnsureComponent<FlyingDoor>();
                fd.Invoke("Solidify", 0.05F);
                fd.Invoke("Thump", 0.15F);
            }
        }
    }

    private class FlyingDoor : MonoBehaviour {
        private static readonly SoundManager.SoundData ImpactSound = SoundManager.registerSound(
            SeaToSeaMod.ModDLL,
            "doorhit",
            "Sounds/doorhit.ogg",
            SoundManager.soundMode3D,
            s => { SoundManager.setup3D(s, 200); }
        );

        private void Solidify() {
            foreach (var c in GetComponentsInChildren<Collider>()) {
                c.enabled = true;
            }
        }

        private void Thump() {
            SoundManager.playSoundAt(ImpactSound, transform.position, false, 40, 2);
            SoundManager.playSoundAt(ImpactSound, transform.position, false, 40, 2);
        }
    }

    public static void ModifyPropulsibility(DIHooks.PropulsibilityCheck ch) {
        if (ch.Obj.FindAncestor<Rigidbody>().name.StartsWith(
                "ExplorableWreck",
                StringComparison.InvariantCultureIgnoreCase
            )) {
            ch.Value = 1;
            return;
        }

        var d = ch.Obj.FindAncestor<Drillable>();
        if (d) {
            var s = ch.Obj.FindAncestor<SpecialDrillable>();
            if (!s || s.canBeMoved())
                ch.Value = 99999999;
        }

        if (IsHeldToolAzuritePowered())
            ch.Value *= ch.IsMass ? 6 : 4;
    }

    public static bool IsHeldToolAzuritePowered() {
        if (Inventory.main == null)
            return false;
        var held = Inventory.main.GetHeld();
        if (!held || !held.gameObject)
            return false;
        var e = held.gameObject.GetComponent<EnergyMixin>();
        return e && e.battery != null && Mathf.Approximately(e.battery.capacity, C2CItems.t2Battery.capacity);
    }

    public static void OnThingInO2Area(OxygenArea a, Collider obj) {
        if (obj.isPlayer()) {
            _lastO2PipeTime = DayNightCycle.main.timePassedAsFloat;
            var o2ToAdd = Math.Min(
                a.oxygenPerSecond * Time.deltaTime,
                Player.main.GetOxygenCapacity() - Player.main.GetOxygenAvailable()
            );
            if (o2ToAdd > 0)
                LiquidBreathingSystem.Instance.TryFillPlayerO2Bar(Player.main, ref o2ToAdd, true);
            if (LiquidBreathingSystem.Instance.HasLiquidBreathing()) {
                LiquidBreathingSystem.Instance.CheckLiquidBreathingSupport(a);
            }
        }
    }

    public static void UpdateToolDefaultBattery(EnergyMixin mix) {
        var p = mix.gameObject.GetComponent<Pickupable>();
        //SNUtil.writeToChat("update tool default battery: "+p+" > "+(p == null ? "" : ""+p.GetTechType()));
        if (p == null)
            return;
        AddT2BatteryAllowance(mix);
        if (p.GetTechType() == LoadTechPistol()) {
            mix.defaultBattery = C2CItems.t2Battery.TechType;
            return;
        }

        switch (p.GetTechType()) {
            case TechType.StasisRifle:
            case TechType.LaserCutter:
                mix.defaultBattery = C2CItems.t2Battery.TechType;
                break;
        }
    }

    public static void AddT2BatteryAllowance(EnergyMixin mix) {
        if (mix.compatibleBatteries.Contains(TechType.Battery) &&
            !mix.compatibleBatteries.Contains(C2CItems.t2Battery.TechType)) {
            mix.compatibleBatteries.Add(C2CItems.t2Battery.TechType); /*
            List<EnergyMixin.BatteryModels> arr = mix.batteryModels.ToList();
            GameObject go = C2CItems.t2Battery.GetGameObject();
            go.SetActive(false);
            arr.Add(new EnergyMixin.BatteryModels{model = go, techType = C2CItems.t2Battery.TechType});
            mix.batteryModels = arr.ToArray();*/
        }
    }

    public static GameObject OnSpawnBatteryForEnergyMixin(GameObject go) {
        //SNUtil.writeToChat("Spawned a "+go);
        go.SetActive(false);
        return go;
    }

    public static IEnumerator CollectTimeCapsule(TimeCapsule tc) {
        var someBlocked = false;
        try {
            PDAEncyclopedia.AddTimeCapsule(tc.id, true);
            PlayerTimeCapsule.main.RegisterOpen(tc.instanceId);
            var items = TimeCapsuleContentProvider.GetItems(tc.id);
            if (items != null) {
                foreach (var tci in items) {
                    if (C2CProgression.Instance.IsTechGated(tci.techType) ||
                        C2CProgression.Instance.IsTechGated(tci.batteryType)) {
                        someBlocked = true;
                        continue;
                    }

                    var result = new TaskResult<Pickupable>();
                    yield return tci.SpawnAsync(result);
                    var pickupable = result.Get();
                    if (pickupable != null) {
                        Inventory.main.ForcePickup(pickupable);
                    }
                }
            }
        } finally {
            tc.gameObject.destroy(false);
        }

        if (someBlocked) {
        }
    }

    public static void SetPingAlpha(uGUI_Ping ico, float orig, PingInstance inst, bool text) {
        /*
        if (Player.main != null && VoidSpikesBiome.instance.isInBiome(Player.main.transform.position)) {
            return inst.pingType == PingType.Seamoth;
        }*/
        var a = Mathf.Min(VoidSpikeLeviathanSystem.instance.getNetScreenVisibilityAfterFlash(), orig);
        if (text)
            ico.SetTextAlpha(a);
        else
            ico.SetIconAlpha(a);
    }

    public static Vector3 GetApparentPingPosition(PingInstance inst) {
        if (!inst || !inst.origin)
            return Vector3.zero;
        var pos = inst.origin.position;
        if (inst.pingType == SeaToSeaMod.VoidSpikeDirectionHint.signalType) {
            pos = VoidSpikesBiome.instance.getPDALocation() + VoidSpikesBiome.voidEndpoint500m -
                  VoidSpikesBiome.end500m; //VoidSpikesBiome.voidEndpoint500m;
        }

        if (Player.main != null && VoidSpikesBiome.instance.IsInBiome(Player.main.transform.position) &&
            !VoidSpikesBiome.instance.IsInBiome(pos) && Vector3.Distance(Player.main.transform.position, pos) > 2) {
            pos += VoidSpikesBiome.end500m - VoidSpikesBiome.voidEndpoint500m;
        }

        return pos;
    }

    public static void RecalculateDamage(DIHooks.DamageToDeal dmg) {
        //if (type == DamageType.Acid && dealer == null && target.GetComponentInParent<SeaMoth>() != null)
        //	return 0;
        //SNUtil.writeToChat(dmg.target.name);
        var p = dmg.Target.FindAncestor<Player>();
        if (p != null) {
            if (dmg.Type == DamageType.Heat && Vector3.Distance(p.transform.position, MountainBaseGeoCenter) <= 27) {
                dmg.SetValue(0);
            } else {
                var flag = C2CItems.hasSealedOrReinforcedSuit(out var seal, out var reinf);
                if (!reinf && dmg.Type == DamageType.Heat &&
                    WaterTemperatureSimulation.main.GetTemperature(p.transform.position) > 270) {
                    dmg.SetValue(dmg.GetAmount() * 1.25F);
                } else if (flag) {
                    if (dmg.Type is DamageType.Poison or DamageType.Acid or DamageType.Electrical && dmg.Dealer != Player.main.gameObject) {
                        //this means something has to deal at least 50 damage to do anything with seal suit, and 20 with reinf (yet most poison is DoT and so does less per)
                        //and lots of other damage is *= Time.deltaTime too, so is tiny per
                        //even LR brine damage is 10 in 1s increments, though is caught by the upper case instead
                        if (dmg.Type == DamageType.Acid && dmg.Target.GetComponent<AcidicBrineDamage>()) {
                            dmg.SetValue(dmg.GetAmount() * (seal ? 0.4F : 0.8F)); //from 10 to 4 or 8
                        } else {
                            dmg.SetValue(dmg.GetAmount() * (seal ? 0.2F : 0.5F));
                            var skipFlat = false;
                            foreach (var dot in dmg.Target.GetComponents<DamageOverTime>()) {
                                //assume is DoT, do not do the flat reduction, just a -80% or -50%
                                if (dot.damageType == dmg.Type) {
                                    skipFlat = true;
                                    break;
                                }
                            }

                            if (!skipFlat) //only do flat reduction on singular hits, which does include Update *= dT, making you immune to gradual health loss from things 
                                dmg.SetValue(dmg.GetAmount() - 10);
                        }
                    }
                }
            }

            var amt = dmg.GetAmount();
            if (amt > 0.01 && !uGUI.isIntro) { //the panel to the face actually DOES DAMAGE...
                float hit = 0;
                if (amt <= 10) {
                    hit = Mathf.Lerp(2, 5, amt / 10F);
                } else {
                    var dmgRef = Mathf.Clamp(amt, 10, 50) - 10; //0-40
                    hit = Mathf.Lerp(5, 75, dmgRef / 40F);
                }

                MoraleSystem.instance.shiftMorale(-hit * MoraleSystem.MORALE_DAMAGE_COEFFICIENT);
            }
        } else {
            //SubRoot sub = dmg.target.FindAncestor<SubRoot>();
            //if (sub && sub.isCyclops)
            //	SNUtil.writeToChat("Cyclops ["+dmg.target.GetFullHierarchyPath()+"] took "+dmg.amount+" of "+dmg.type+" from '"+dmg.dealer+"'");
            if (dmg.Type is DamageType.Normal or DamageType.Drill or DamageType.Puncture or DamageType.Electrical) {
                var s = dmg.Target.FindAncestor<DeepStalkerTag>();
                if (s) {
                    if (dmg.Type == DamageType.Electrical)
                        s.onHitWithElectricDefense();
                    dmg.SetValue(
                        dmg.GetAmount() * 0.5F
                    ); //50% resistance to "factorio physical" damage, plus electric to avoid PD killing them
                }
            }

            if (dmg.Type == DamageType.Electrical) {
                var s = dmg.Target.FindAncestor<VoidSpikeLeviathan.VoidSpikeLeviathanAI>();
                if (s) {
                    dmg.SetValue(0);
                    dmg.LockValue();
                }

                if (Vector3.Distance(dmg.Target.transform.position, BkelpBaseGeoCenter) <= 60 &&
                    !dmg.Target.FindAncestor<Vehicle>()) {
                    dmg.SetValue(0);
                }
            }

            if (dmg.Type == DamageType.Heat && DeIntegrationSystem.Instance.IsLoaded() &&
                CraftData.GetTechType(dmg.Target) == DeIntegrationSystem.Instance.GetRubyPincher()) {
                dmg.SetValue(dmg.GetAmount() * 0.5F);
            }

            if (dmg.Type == DamageType.Normal && VanillaBiomes.Void.IsInBiome(dmg.Target.transform.position)) {
                var sm = dmg.Target.FindAncestor<SeaMoth>();
                if (sm && !sm.vehicleHasUpgrade(C2CItems.voidStealth.TechType))
                    dmg.SetValue(dmg.GetAmount() * 1.5F);
            }

            if (dmg.Type == DamageType.Poison || !dmg.Target.isFarmedPlant()) {
                if (dmg.Target.FindAncestor<GlowKelpTag>()) {
                    dmg.SetValue(0);
                }
            }
        }
    }

    public static float GetVehicleRechargeAmount(Vehicle v) {
        var baseline = 0.0025F;
        var b = v.GetComponentInParent<SubRoot>();
        if (b && b.isBase && b.currPowerRating > 0) {
            baseline *= 4;
        }

        return baseline;
    }

    public static float GetPlayerO2Rate(Player ep) {
        return EnvironmentalDamageSystem.Instance.GetPlayerO2Rate(ep);
    }

    public static float GetPlayerO2Use(Player ep, float breathingInterval, int depthClass) {
        return EnvironmentalDamageSystem.Instance.GetPlayerO2Use(ep, breathingInterval, depthClass);
    }

    public static void TickPlayerEnviroAlerts(RebreatherDepthWarnings warn) {
        EnvironmentalDamageSystem.Instance.TickPlayerEnviroAlerts(warn);
    }

    public static void DoEnvironmentalDamage(TemperatureDamage dmg) {
        EnvironmentalDamageSystem.Instance.TickTemperatureDamages(dmg);
    }

    public static void OnSetPlayerACU(Player ep, WaterPark w) {
        if (w) {
            foreach (var wp in w.items) {
                LifeformScanningSystem.instance.onObjectSeen(wp.gameObject, true, true);
            }
        }
    }

    public static void OnCrashfishExplode(Crash c) {
        LifeformScanningSystem.instance.onObjectSeen(c.gameObject, false);
    }

    public static void OnItemPickedUp(DIHooks.ItemPickup ip) {
        var p = ip.Item;
        AvoliteSpawner.Instance.CleanPickedUp(p);
        p.gameObject.removeComponent<SinkingGroundChunk>();
        // TODO: FCS Compat
        // FCSIntegrationSystem.instance.modifyPeeperFood(p);
        LifeformScanningSystem.instance.onObjectSeen(p.gameObject, true);
        var tt = p.GetTechType();
        var ingot = C2CItems.getIngotByUnpack(tt);
        if (ingot != null) {
            ingot.pickupUnpacked();
            ip.Destroy = true;
        } else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType) {
            StoryGoal.Execute("Azurite", Story.GoalType.Story);
            if (VanillaBiomes.Ilz.IsInBiome(p.transform.position))
                StoryGoal.Execute("ILZAzurite", Story.GoalType.Story);
            if (ip.Prawn || !C2CItems.hasSealedOrReinforcedSuit(out var seal, out var reinf)) {
                var lv = ip.Prawn ? ip.Prawn.liveMixin : Player.main.gameObject.GetComponentInParent<LiveMixin>();
                var dmg = lv.maxHealth *
                          (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 0.3F : 0.2F);
                if (Vector3.Distance(p.transform.position, Azurite.mountainBaseAzurite) <= 8)
                    dmg *= 0.75F;
                if (ip.Prawn)
                    dmg *= 0.04F; //do about 2% damage
                lv.TakeDamage(dmg, Player.main.gameObject.transform.position, DamageType.Electrical, p.gameObject);
            }
        } else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType) {
            StoryGoal.Execute("Platinum", Story.GoalType.Story);
            var set = WorldUtil.getObjectsNearWithComponent<DeepStalkerTag>(p.transform.position, 60);
            foreach (var c in set) {
                if (!c.currentlyHasPlatinum() && !c.GetComponent<WaterParkCreature>()) {
                    var chance = Mathf.Clamp01(1F - Vector3.Distance(c.transform.position, p.transform.position) / 90F);
                    if (UnityEngine.Random.Range(0F, 1F) <= chance)
                        c.triggerPtAggro(ip.Prawn ? ip.Prawn.gameObject : Player.main.gameObject);
                }
            }
        } else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType) {
            StoryGoal.Execute("PressureCrystals", Story.GoalType.Story);
        } else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType) {
            StoryGoal.Execute("Avolite", Story.GoalType.Story);
        } else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).TechType) {
            StoryGoal.Execute("Calcite", Story.GoalType.Story);
        } else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.OBSIDIAN).TechType) {
            StoryGoal.Execute("Obsidian", Story.GoalType.Story);
        } else if (tt == C2CItems.alkali.seed.TechType) {
            StoryGoal.Execute("AlkaliVine", Story.GoalType.Story);
        } else if (tt == C2CItems.kelp.seed.TechType) {
            StoryGoal.Execute("DeepvineSamples", Story.GoalType.Story);
        } else if (tt == C2CItems.mountainGlow.seed.TechType) {
            StoryGoal.Execute("Pyropod", Story.GoalType.Story);
        } else if (tt == C2CItems.voltaicBladderfish.TechType) {
            if (ip.Prawn || !C2CItems.hasSealedOrReinforcedSuit(out var seal, out var reinf)) {
                var lv = ip.Prawn ? ip.Prawn.liveMixin : Player.main.gameObject.GetComponentInParent<LiveMixin>();
                var dmg = lv.maxHealth *
                          (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 0.15F : 0.1F);
                if (ip.Prawn)
                    dmg *= 0.08F; //do about 2% damage
                lv.TakeDamage(dmg, Player.main.gameObject.transform.position, DamageType.Electrical, p.gameObject);
            }
        } else if (CustomEgg.GetEgg(TechType.SpineEel).Includes(tt)) {
            //SNUtil.writeToChat((Player.main.transform.position - lrnest).magnitude.ToString());
            if ((Player.main.transform.position - Lrnest).magnitude <= 50) {
                StoryGoal.Execute("LRNest", Story.GoalType.Story);
                var set = WorldUtil.getObjectsNearWithComponent<SpineEel>(Lrnest, 120);
                //SNUtil.writeToChat(set.Count.ToString());
                for (var i = set.Count; i < 6; i++) {
                    var go = ObjectUtil.createWorldObject(VanillaCreatures.RIVERPROWLER.prefab);
                    go.transform.position = MathUtil.getRandomVectorAround(LrNestDefender, 100);
                    set.Add(go.GetComponent<SpineEel>());
                }

                foreach (var e in set) {
                    AttractToTarget.attractCreatureToTarget(e, Player.main, false, 99999).deleteOnAttack = true;
                    //SNUtil.writeToChat(e.transform.position.ToString());
                }
            }
        } else if (tt == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType &&
                   VanillaBiomes.Ilz.IsInBiome(Player.main.transform.position)) {
            StoryGoal.Execute("Iridium", Story.GoalType.Story);
            C2CItems.hasSealedOrReinforcedSuit(out var seal, out var reinf);
            if (!ip.Prawn && !reinf) {
                var lv = Player.main.gameObject.GetComponentInParent<LiveMixin>();
                var dmg = 40 + (WaterTemperatureSimulation.main.GetTemperature(Player.main.transform.position) - 90) /
                    3;
                lv.TakeDamage(dmg, Player.main.gameObject.transform.position, DamageType.Heat, Player.main.gameObject);
            }
        } else if (tt == TechType.Kyanite) {
            StoryGoal.Execute("Kyanite", Story.GoalType.Story);
        } else if (tt == TechType.Sulphur) {
            StoryGoal.Execute("Sulfur", Story.GoalType.Story);
        } else if (tt == TechType.UraniniteCrystal) {
            StoryGoal.Execute("Uranium", Story.GoalType.Story);
        } else if (tt == TechType.Nickel) {
            StoryGoal.Execute("Nickel", Story.GoalType.Story);
        } else if (tt == TechType.MercuryOre) {
            StoryGoal.Execute("Mercury", Story.GoalType.Story);
        } else if
            (tt == TechType
                 .AdvancedWiringKit /* && Vector3.Distance(Player.main.transform.position, SeaToSeaMod.ADV_WIRING_POS) <= 10*/
            ) {
            StoryGoal.Execute(SeaToSeaMod.AdvWiringGoal, Story.GoalType.Story);
        } else if (tt == CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType) {
            p.GetComponent<NanocarbonTag>().reset();
            StoryGoal.Execute("Nanocarbon", Story.GoalType.Story);
        } else if (tt == CraftingItems.getItem(CraftingItems.Items.LavaPlankton).TechType) {
            StoryGoal.Execute("LavaPlankton", Story.GoalType.Story);
        } else if (tt == C2CItems.emperorRootOil.TechType) {
            var tag = p.gameObject.EnsureComponent<EmperorRootOil.EmperorRootOilTag>();
            if (tag.pickupTime < 0)
                tag.pickupTime = DayNightCycle.main.timePassedAsFloat;
        } else if (DeIntegrationSystem.Instance.IsLoaded() &&
                   tt == DeIntegrationSystem.Instance.ThalassaceanCud.TechType) {
            var thala = p.gameObject.FindAncestor<DeIntegrationSystem.C2CThalassacean>();
            if (!thala)
                thala = WorldUtil.getClosest<DeIntegrationSystem.C2CThalassacean>(p.transform.position);
            if (thala && Vector3.Distance(thala.transform.position, p.transform.position) < 30)
                thala.LastCollect = DayNightCycle.main.timePassedAsFloat;
            if (!thala)
                ip.Destroy = true; //destroy collected from 000
        }
    }

    public static float GetReachDistance() {
        return SkipRaytrace || Player.main.GetVehicle() ? 2 :
            (Player.main.transform.position - LostRiverCachePanel).sqrMagnitude <= 100 ? 4F :
            VoidSpikesBiome.instance.IsInBiome(Player.main.transform.position) ? 3.5F : 2;
    }

    public static void CheckTargetingSkip(DIHooks.TargetabilityCheck ch) {
        if (SkipRaytrace)
            return;
        //SNUtil.log("Checking targeting skip of "+id+" > "+id.ClassId);
        if (ch.Prefab.ClassId == "b250309e-5ad0-43ca-9297-f79e22915db6" && Vector3.Distance(
                Player.main.transform.position,
                LrpowerSealSetpieceCenter
            ) <= 8) { //to allow to hit the things inside the mouth
            ch.AllowTargeting = false;
        } else if (VoidSpike.isSpike(ch.Prefab.ClassId) && VoidSpikesBiome.instance.IsInBiome(ch.Transform.position)) {
            ch.AllowTargeting = false;
        }
    }

    public static EntityCell GetEntityCellForInt3(Array3<EntityCell> data, Int3 raw, BatchCells batch) {
        var n = data.GetLength(0) / 2;
        var real = raw + new Int3(n, n, n);
        return data.Get(real);
    }

    public static void SetEntityCellForInt3(Array3<EntityCell> data, Int3 raw, EntityCell put, BatchCells batch) {
        var n = data.GetLength(0) / 2;
        var real = raw + new Int3(n, n, n);
        data.Set(real, put);
    }

    public static void InitBatchCells(BatchCells b) { //default 10 5 5 5
        b.cellsTier0 = new Array3<EntityCell>(20);
        b.cellsTier1 = new Array3<EntityCell>(10);
        b.cellsTier2 = new Array3<EntityCell>(10);
        b.cellsTier3 = new Array3<EntityCell>(10);
    }

    public static void OnDataboxActivate(BlueprintHandTarget c) {
        var over = DataboxTypingMap.instance.getOverride(c);
        if (over == TechType.None && c.unlockTechType == TechType.RepulsionCannon)
            over = AqueousEngineeringMod.wirelessChargerBlock.TechType;
        if (over != TechType.None && over != c.unlockTechType) {
            SNUtil.Log(
                "Blueprint @ " + c.gameObject.transform.position + ", previously " + c.unlockTechType +
                ", found an override to " + over
            );
            var go = ObjectUtil.createWorldObject(GenUtil.getOrCreateDatabox(over).TechType);
            if (!go) {
                SNUtil.Log("Could not find prefab for databox for " + over + "!");
                return;
            }

            go.transform.SetParent(c.transform.parent);
            go.transform.position = c.transform.position;
            go.transform.rotation = c.transform.rotation;
            go.transform.localScale = c.transform.localScale;
            c.gameObject.destroy(false);
        } else if (c.unlockTechType == C2CItems.breathingFluid.TechType && c.transform.position.y < -500) {
            //clear old databox placement
            c.gameObject.destroy(false);
        } else if (c.unlockTechType == C2CItems.liquidTank.TechType && c.transform.position.y > -500) {
            c.gameObject.destroy(false);
        } else if (c.gameObject.name == "FCSDataBox(Clone)") { //lock to C2C way
            c.gameObject.destroy(false);
        }
    }

    public static void OnTreaderChunkSpawn(SinkingGroundChunk chunk) {
        if (UnityEngine.Random.Range(0F, 1F) <
            (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 0.92 : 0.88))
            return;
        var near = 0;
        foreach (var c in Physics.OverlapSphere(chunk.gameObject.transform.position, 40F)) {
            if (!c || !c.gameObject) {
                continue;
            }

            var p = c.gameObject.GetComponentInParent<TechTag>();
            if (p != null && p.type == CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType)
                near++;
        }

        if (near > 2)
            return;
        var owner = chunk.gameObject;
        var placed = ObjectUtil.createWorldObject(
            CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType.ToString()
        );
        placed.transform.position = owner.transform.position + Vector3.up * 0.08F;
        placed.transform.rotation = owner.transform.rotation;
        owner.destroy(false);
    }

    public static void OnResourceSpawn(ResourceTracker p) {
        if (SkipResourceSpawn)
            return;
        var pi = p.GetComponent<PrefabIdentifier>();
        if (pi && pi.ClassId == VanillaResources.LARGE_SULFUR.prefab) {
            p.overrideTechType = TechType.Sulphur;
            p.techType = TechType.Sulphur;
        }
    }

    public static void DoEnviroVehicleDamage(CrushDamage dmg) {
        EnvironmentalDamageSystem.Instance.TickCyclopsDamage(dmg);
    }

    public static void GetWaterTemperature(DIHooks.WaterTemperatureCalculation calc) {
        if (SkipTemperatureCheck)
            return;
        if (EnvironmentalDamageSystem.Instance.TemperatureOverride >= 0) {
            calc.SetValue(EnvironmentalDamageSystem.Instance.TemperatureOverride);
            calc.LockValue();
            return;
        }

        //SNUtil.writeToChat("C2C: Checking water temp @ "+calc.position+" def="+calc.originalValue);
        if (Vector3.Distance(calc.Position, MountainBaseGeoCenter) <= 20) {
            calc.SetValue(Mathf.Min(calc.GetTemperature(), 45));
        } else {
            var bdist = Vector3.Distance(calc.Position, BkelpBaseNuclearReactor);
            if (bdist <= 12)
                calc.SetValue(Mathf.Max(calc.GetTemperature(), 90 - bdist * 6F));
        }

        var biome = EnvironmentalDamageSystem.Instance.GetBiome(calc.Position);
        var poison = EnvironmentalDamageSystem.Instance.GetLrPoison(biome);
        if (poison > 0) {
            //make LR cold, down to -10C (4C is max water density point, but not for saltwater), except around vents
            var temp = calc.GetTemperature();
            var cooling = poison * Mathf.Max(0, 3F - Mathf.Max(0, temp - 30) / 10F);
            calc.SetValue(Mathf.Max(-10, temp - cooling));
        } else if (VanillaBiomes.Cove.IsInBiome(calc.Position))
            calc.SetValue(calc.GetTemperature() - 10);

        if (biome == null || (biome.ToLowerInvariant().Contains("void") && calc.Position.y <= -50))
            calc.SetValue(
                Mathf.Max(4, calc.GetTemperature() + (calc.Position.y + 50) / 20F)
            ); //drop 1C per 20m below 50m, down to 4C around 550m
        var dist = VoidSpikesBiome.instance.getDistanceToBiome(calc.Position, true);
        if (dist <= 500)
            calc.SetValue(
                (float)MathUtil.linterpolate(
                    dist,
                    200,
                    500,
                    VoidSpikesBiome.waterTemperature,
                    calc.GetTemperature(),
                    true
                )
            );
        if (VoidSpikesBiome.instance.IsInBiome(calc.Position)) {
            calc.SetValue(VoidSpikesBiome.waterTemperature);
        }

        dist = UnderwaterIslandsFloorBiome.instance.getDistanceToBiome(calc.Position);
        if (dist <= 150)
            calc.SetValue(
                (float)MathUtil.linterpolate(
                    dist,
                    0,
                    150,
                    UnderwaterIslandsFloorBiome.waterTemperature,
                    calc.GetTemperature(),
                    true
                )
            );
        if (UnderwaterIslandsFloorBiome.instance.IsInBiome(calc.Position))
            calc.SetValue(
                calc.GetTemperature() +
                UnderwaterIslandsFloorBiome.instance.getTemperatureBoost(calc.GetTemperature(), calc.Position)
            );
        calc.SetValue(
            Mathf.Max(calc.GetTemperature(), EnvironmentalDamageSystem.Instance.GetWaterTemperature(calc.Position))
        );
        EjectedHeatSink.iterateHeatSinks(h => {
                    if (h) {
                        dist = Vector3.Distance(h.transform.position, calc.Position);
                        if (dist <= EjectedHeatSink.HEAT_RADIUS) {
                            var f = 1F - (float)(dist / EjectedHeatSink.HEAT_RADIUS);
                            //SNUtil.writeToChat("Found heat sink "+lb.transform.position+" at dist "+dist+" > "+f+" > "+(f*lb.getTemperature()));
                            calc.SetValue(Mathf.Max(calc.GetTemperature(), f * h.getTemperature()));
                        }
                    }
                }
            ); /* Too expensive
            Geyser g = WorldUtil.getClosest<Geyser>(calc.position);
            if (g && g.erupting && calc.position.y > g.transform.position.y) {
                calc.setValue(Mathf.Max(calc.getTemperature(), 800-10*Vector3.Distance(g.transform.position, calc.position)));
            }
            calc.setValue(C2CMoth.getOverrideTemperature(calc.getTemperature()));*/
    }

    public static void OnPrecursorDoorSpawn(PrecursorKeyTerminal pk) {
        try {
            var parent = pk.transform.parent;
            var pi = parent == null ? null : parent.GetComponent<PrefabIdentifier>();
            if (!pi) { /*
                if (Vector3.Distance(pi.transform.position, gunPoolBarrierTerminal) < 0.5F) {
                    pk.acceptKeyType = PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Orange;
                    HashSet<GameObject> barrier = WorldUtil.getObjectsNearMatching(pk.transform.position, 90, isGunBarrier);
                    if (barrier.Count == 0) {
                    pk.transform.parent = barrier.First().transform;
                }*/
                return;
            }

            switch (pi.classId) {
                case "0524596f-7f14-4bc2-a784-621fdb23971f":
                case "47027cf0-dca8-4040-94bd-7e20ae1ca086":
                    new ChangePrecursorDoor(PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_White).applyToObject(pk);
                    break;
                case "fdb2bcbb-288a-40b6-bd7a-5585445eb43f":
                    if (parent.position.y > -100) {
                        new ChangePrecursorDoor(PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Purple)
                            .applyToObject(pk);
                        //does not exist parent.GetComponent<PrecursorGlobalKeyActivator>().doorActivationKey = "GunGateDoor";
                    } else if (Math.Abs(parent.position.y + 803.8) < 0.25) {
                        new ChangePrecursorDoor(PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Red).applyToObject(
                            pk
                        );
                        //parent.GetComponent<PrecursorGlobalKeyActivator>().doorActivationKey = "DRFGateDoor";
                    } else if (Math.Abs(parent.position.y + 803.8) < 15) { //the original
                        new ChangePrecursorDoor(PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Orange)
                            .applyToObject(pk);
                    }

                    break; /*
                case "d26276ab-0c29-4642-bcb8-1a5f8ee42cb2":
                    break;*/
            }
        } catch (Exception e) {
            SNUtil.Log(
                "Caught exception processing precursor door " + pk.gameObject.GetFullHierarchyPath() + " @ " +
                pk.transform.position + ": " + e.ToString()
            );
        }
    }

    /*
    private static bool isGunBarrier(GameObject go) {
        PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
        return pi && pi.ClassId == "d26276ab-0c29-4642-bcb8-1a5f8ee42cb2" && Vector3.Distance(pi.transform.position, gunPoolBarrier) < 0.5F;
    }
    */
    public static void OnInspectableSpawn(InspectOnFirstPickup pk) { /*
        PrefabIdentifier pi = pk.gameObject.GetComponentInParent<PrefabIdentifier>();
        if (pi != null && (pi.ClassId == "7d19f47b-6ec6-4a25-9b28-b3fd7f5661b7" || pi.ClassId == "066e533d-f854-435d-82c6-b28ba59858e0")) {
            VFXFabricating fab = pi.gameObject.transform.Find("Model").gameObject.EnsureComponent<VFXFabricating>();
            fab.localMaxY = 0.1F;
            fab.localMinY = -0.1F;
        }*/
    }

    public static GameObject GetCrafterGhostModel(GameObject ret, TechType tech) {
        SNUtil.Log("Crafterghost for " + tech + ": " + ret);
        if (tech is TechType.PrecursorKey_Red or TechType.PrecursorKey_White) {
            ret = ObjectUtil.lookupPrefab(CraftData.GetClassIdForTechType(tech));
            ret = ret.clone();
            ret = ret.getChildObject("Model");
            var fab = ret.EnsureComponent<VFXFabricating>();
            fab.localMaxY = 0.1F;
            fab.localMinY = -0.1F;
            fab.enabled = true;
            fab.gameObject.SetActive(true);
        }

        return ret;
    }

    public static void OnSpawnLifepod(EscapePod pod) {
        pod.gameObject.EnsureComponent<C2CLifepod>();
        pod.gameObject.EnsureComponent<Magnetic>();
    }

    public static void OnSkyApplierSpawn(SkyApplier pk) {
        if (SkipSkyApplierSpawn)
            return;
        var go = pk.gameObject;
        if (go.name.StartsWith("Seamoth", StringComparison.InvariantCultureIgnoreCase) && go.name.EndsWith(
                "Arm(Clone)",
                StringComparison.InvariantCultureIgnoreCase
            ))
            return;
        //if (DIHooks.isWorldLoaded())
        //	LifeformScanningSystem.instance.onObjectCreated(go);
        if (go.name.StartsWith("ExplorableWreck", StringComparison.InvariantCultureIgnoreCase)) {
            go.EnsureComponent<ImmuneToPropulsioncannon>(); //also implements IObstacle to prevent building
        }

        var pi = go.FindAncestor<PrefabIdentifier>();
        if (SNUtil.Match(pi, "d79ab37f-23b6-42b9-958c-9a1f4fc64cfd") &&
            Vector3.Distance(FcsWreckOpenableDoor, go.transform.position) <= 0.5) {
            new WreckDoorSwaps.DoorSwap(go.transform.position, "Handle").applyTo(go);
        } else if (SNUtil.Match(pi, "055b3160-f57b-46ba-80f5-b708d0c8180e") &&
                   Vector3.Distance(FcsWreckBlockedDoor, go.transform.position) <= 0.5) {
            new WreckDoorSwaps.DoorSwap(go.transform.position, "Blocked").applyTo(go);
        } else if (SNUtil.Match(pi, VanillaCreatures.SEA_TREADER.prefab)) {
            //go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
            go.EnsureComponent<C2CTreader>();
        } else if (SNUtil.Match(pi, VanillaCreatures.CAVECRAWLER.prefab)) {
            go.EnsureComponent<C2Crawler>();
        } else if (SNUtil.Match(pi, VanillaCreatures.REAPER.prefab)) {
            go.EnsureComponent<C2CReaper>();
        } else if (SNUtil.Match(pi, VanillaCreatures.GHOST_LEVIATHAN.prefab) ||
                   SNUtil.Match(pi, VanillaCreatures.GHOST_LEVIATHAN_BABY.prefab)) {
            go.EnsureComponent<GhostGelTracker>().setup();
        } else if (DeIntegrationSystem.Instance.IsLoaded() && !go.GetComponent<WaterParkCreature>() && SNUtil.Match(
                       go,
                       DeIntegrationSystem.Instance.GetThalassacean(),
                       DeIntegrationSystem.Instance.GetLrThalassacean()
                   )) {
            go.EnsureComponent<DeIntegrationSystem.C2CThalassacean>();
        } else if (DeIntegrationSystem.Instance.IsLoaded() && !go.GetComponent<WaterParkCreature>() &&
                   SNUtil.Match(go, DeIntegrationSystem.Instance.GetGulper())) {
            go.EnsureComponent<DeIntegrationSystem.C2CGulper>();
        } else if (SNUtil.Match(pi, "61ac1241-e990-4646-a618-bddb6960325b")) {
            if (Vector3.Distance(go.transform.position, Player.main.transform.position) <= 80 &&
                go.transform.position.y < -200) {
                PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.TreaderPooPrompt).key);
            }
        } /*
        else if (SNUtil.match(pi, "e1d8b721-0edb-466e-93d3-074dc90d57f2")) {
            if (go.transform.position.y < -1200) {
                go.EnsureComponent<TechTag>().type = SeaToSeaMod.prisonPipeRoomTank;
            }
        }*/ else if (go.isVent()) {
            go.EnsureComponent<C2CVentInteraction>();
        } else if (SNUtil.Match(pi, "407e40cf-69f2-4412-8ab6-45faac5c4ea2")) {
            //SNUtil.log("Initialized lava castle smoke");
            var root = go.getChildObject("CollisionHolder");
            if (!root) {
                root = new GameObject("CollisionHolder");
                root.transform.SetParent(go.transform);
            }

            root.EnsureComponent<TechTag>().type = SeaToSeaMod.LavaCastleSmoker;
            root.layer = LayerID.Useable;
            Utils.ZeroTransform(root.transform);
            var bounds = go.GetComponent<Renderer>().bounds;
            var lim = bounds.max;
            var dxz = (lim - go.transform.position).SetY(0).Rotated(0, -45, 0, go.transform.position);
            var dy = lim.y - go.transform.position.y;
            var n = 32;
            for (var i = 0; i < n; i++) {
                var nm = "smokeColumnVolumePoint#" + i;
                var sph = root.getChildObject(nm);
                if (!sph) {
                    sph = GameObject.CreatePrimitive(PrimitiveType.Sphere).setName(nm);
                    sph.transform.SetParent(root.transform);
                }

                var f = i / (float)n;
                Utils.ZeroTransform(sph.transform);
                //sph.transform.position = Vector3.Lerp(gg.transform.position, lim, f);
                var offset = i == 0 ? Vector3.one * 12 : Vector3.zero;
                sph.transform.position =
                    go.transform.position + offset + dxz * f + Vector3.up * dy * Mathf.Pow(f, 0.475F);
                sph.transform.localScale = Vector3.one * ((1 + f) * 12 + 2) * 2;
                sph.layer = LayerID.Useable;
                sph.GetComponent<Collider>().isTrigger = true;
                sph.GetComponent<Renderer>().enabled = false;
                sph.EnsureComponent<LavaCastleSmokeVolumeTrigger>();

                var sparkle = sph.getChildObject("Sparkle");
                if (!sparkle) {
                    sparkle = ObjectUtil.lookupPrefab("505e7eff-46b3-4ad2-84e1-0fadb7be306c")
                        .getChildObject("Leviathan_enzymeBall_anim/root/xBubbles").clone().setName("Sparkle");
                    sparkle.transform.SetParent(sph.transform);
                }

                Utils.ZeroTransform(sparkle.transform);
                sparkle.transform.localScale = Vector3.one * 8;
                sparkle.transform.localPosition = MathUtil.getRandomVectorAround(Vector3.zero, 0.9F);
                foreach (var r in sparkle.GetComponentsInChildren<Renderer>()) {
                    foreach (var m in r.materials)
                        m.SetColor("_Color", new Color(1.5F, 0.9F, 0.6F, 1F));
                }

                sparkle.GetComponent<ParticleSystem>().destroy(); //remove bubbles
                sparkle.GetComponent<ParticleSystemRenderer>().destroy();
                foreach (var p in sparkle.GetComponentsInChildren<ParticleSystem>()) {
                    var main = p.main;
                    main.simulationSpeed = UnityEngine.Random.Range(0.2F, 0.3F) * 1.25F;
                    main.startSize = 8 * 1.5F;
                    main.startLifetime = 1.2F; //from 0.75
                    p.emissionRate = UnityEngine.Random.Range(0.15F, 0.25F) * 0.25F;
                    p.Play();
                }
            }
        } /*
        else if (SNUtil.match(pi, "SeaVoyager")) {
            go.EnsureComponent<C2CVoyager>();
        }*/ else if (SNUtil.Match(pi, "172d9440-2670-45a3-93c7-104fee6da6bc")) {
            if (Vector3.Distance(go.transform.position, LostRiverCachePanel) < 2) {
                var r = go.getChildObject("Precursor_Lab_infoframe/Precursor_Lab_infoframe_glass")
                    .GetComponent<Renderer>();
                r.materials[0].SetColor("_Color", new Color(1, 1, 1, /*0.43F*/0.24F));
                r.materials[0].SetColor("_SpecColor", new Color(0.38F, 1, 0.52F, 1));
                RenderUtil.setGlossiness(r.materials[0], 50, 0, 0);
                var copy = r.gameObject.clone();
                copy.transform.SetParent(r.transform.parent);
                copy.transform.position = r.transform.position;
                copy.transform.rotation = r.transform.rotation;
                copy.transform.localScale = r.transform.localScale;
                var r2 = copy.GetComponent<Renderer>();
                r2.materials[0].shader = Shader.Find("UWE/Marmoset/IonCrystal");
                r2.materials[0].SetInt("_ZWrite", 1);
                r2.materials[0].SetColor("_DetailsColor", Color.white);
                r2.materials[0].SetColor("_SquaresColor", new Color(1, 4, 1.5F, 2));
                r2.materials[0].SetFloat("_SquaresTile", 200F);
                r2.materials[0].SetFloat("_SquaresSpeed", 12F);
                r2.materials[0].SetFloat("_SquaresIntensityPow", 20F);
                r2.materials[0].SetVector("_NoiseSpeed", new Vector4(1, 1, 1, 1));
                r2.materials[0].SetVector("_FakeSSSparams", new Vector4(1, 15, 1, 1));
                r2.materials[0].SetVector("_FakeSSSSpeed", new Vector4(1, 1, 1, 1));
                RenderUtil.setGlossiness(r2.materials[0], 0, 0, 0);
                r.transform.position = new Vector3(r.transform.position.x, r.transform.position.y, -709.79F);
                r2.transform.position = new Vector3(r.transform.position.x, r.transform.position.y, -709.80F);
                var ht = go.EnsureComponent<GenericHandTarget>();
                ht.onHandHover = new HandTargetEvent();
                ht.onHandClick = new HandTargetEvent();
                ht.onHandHover.AddListener(hte => {
                        if (!KnownTech.knownTech.Contains(C2CItems.treatment.TechType)) {
                            HandReticle.main.targetDistance = 15;
                            HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
                            HandReticle.main.SetInteractText("LostRiverCachePanel");
                        }
                    }
                );
                ht.onHandClick.AddListener(hte => {
                        if (!KnownTech.knownTech.Contains(C2CItems.treatment.TechType)) {
                            KnownTech.Add(C2CItems.treatment.TechType);
                            SNUtil.TriggerTechPopup(C2CItems.treatment.TechType);
                        }
                    }
                );
            }
        } /*
        else if (SNUtil.match(pi, VanillaCreatures.GHOST_LEVIATHAN && pi.GetComponentInChildren<GhostLeviatanVoid>()) {
            ***
        }*/ else if (pi && AuroraFires.Contains(pi.ClassId) &&
                     EnvironmentalDamageSystem.Instance.IsPositionInAuroraPrawnBay(pi.transform.position)) {
            BlueAuroraPrawnFire(go);
        } else if (SNUtil.Match(pi, "b86d345e-0517-4f6e-bea4-2c5b40f623b4") && pi.transform.parent &&
                   pi.transform.parent.name.Contains("ExoRoom_Weldable")) {
            var inner = go.getChildObject("Starship_doors_manual_01/Starship_doors_automatic");
            var d = go.transform.parent.GetComponentInChildren<StarshipDoorLocked>();
            var r = inner.GetComponentInChildren<Renderer>();
            RenderUtil.swapTextures(
                SeaToSeaMod.ModDLL,
                r,
                "Textures/",
                new Dictionary<int, string>() {
                    {
                        0,
                        "FireDoor"
                    }, {
                        1,
                        "FireDoor"
                    },
                }
            );
            d.lockedTexture =
                (Texture2D)r.materials[0]
                    .GetTexture(Shader.PropertyToID("_Illum")); //replace all since replaced the base texture too
            d.unlockedTexture = TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/FireDoor2_Illum");
            //WeldableWallPanelGeneric panel = go.transform.parent.GetComponentInChildren<WeldableWallPanelGeneric>();
            var pt = /*panel.sendMessageFrom*/go.transform.parent.gameObject.EnsureComponent<PrawnBayDoorTriggers>();
            pt.Door = d.GetComponent<StarshipDoor>();
            var ht = inner.EnsureComponent<GenericHandTarget>();
            pt.HoverHint = ht;
            ht.onHandHover = new HandTargetEvent();
            ht.onHandHover.AddListener(hte => {
                    HandReticle.main.SetIcon(HandReticle.IconType.Info, 1f);
                    HandReticle.main.SetInteractText("PrawnBayDoorHeatWarn");
                    HandReticle.main.SetTargetDistance(8);
                }
            );
            var p1 = new Vector3(991.1F, 1F, -3.2F);
            var p2 = new Vector3(991.7F, 1F, -2.8F); /*
            GameObject rippleHolder = new GameObject("ripples");
            rippleHolder.transform.parent = go.transform.parent;
            rippleHolder.transform.localPosition = Vector3.zero;
            GameObject vent = ObjectUtil.lookupPrefab("5bbd405c-ca10-4da8-832b-87558c42f4dc");
            GameObject bubble = vent.getChildObject("xThermalVent_Dark_Big/xBubbles");
            int n = 5;
            for (int i = 0; i <= n; i++) {
                GameObject p = bubble.clone();
                p.transform.parent = rippleHolder.transform;
                p.transform.position = Vector3.Lerp(p1, p2, i/(float)n);
                p.GetComponentInChildren<Renderer>().materials[0].color = new Color(-8, -8, -8, 0.3F);
            }*/
            var fire = ObjectUtil.createWorldObject("3877d31d-37a5-4c94-8eef-881a500c58bc");
            fire.transform.parent = go.transform;
            fire.transform.position = Vector3.Lerp(p1, p2, 0.5F) + new Vector3(1.3F, -0.05F, -1.7F);
            fire.transform.localScale = new Vector3(1.8F, 1, 1.8F);
            BlueAuroraPrawnFire(fire);
            //fire.removeComponent<VFXExtinguishableFire>();
            var lv = fire.GetComponent<LiveMixin>();
            lv.invincible = true;
            lv.data.maxHealth = 40000;
            lv.health = lv.data.maxHealth;
        } else if (pi && SeaToSeaMod.LrCoralClusters.Contains(pi.ClassId)) {
            var name = go.name;
            go.EnsureComponent<TechTag>().type = C2CItems.brineCoral;
            //if (!hard) require azurite battery if not set from 1600 to 750
            //	go.EnsureComponent<Rigidbody>().mass = 750;
            var pfb = ObjectUtil.lookupPrefab(VanillaResources.LARGE_QUARTZ.prefab);
            go.makeMapRoomScannable(C2CItems.brineCoral);
            var d = go.EnsureComponent<Drillable>();
            d.CopyObject(pfb.GetComponent<Drillable>());
            go.name = name;
            /*
            d.deleteWhenDrilled = true;
            d.kChanceToSpawnResources = 1;
            d.lootPinataOnSpawn = true;
            d.minResourcesToSpawn = 1;
            d.maxResourcesToSpawn = 2;*/
            d.resources = [
                new() {
                    techType = C2CItems.brineCoralPiece.TechType,
                    chance = 1,
                },
            ];
            var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
            // d.kChanceToSpawnResources = 1;
            d.minResourcesToSpawn = hard ? 1 : 2;
            d.maxResourcesToSpawn = hard ? 3 : 4;
            go.EnsureComponent<BrineCoralTag>();
            d.onDrilled += (dr) => { dr.GetComponent<BrineCoralTag>().onDrilled(); };
        } else if (SNUtil.Match(pi, "58247109-68b9-411f-b90f-63461df9753a") &&
                   Vector3.Distance(DeepDegasiTablet, go.transform.position) <= 0.2) {
            var go2 = ObjectUtil.createWorldObject(C2CItems.brokenOrangeTablet.ClassID);
            go2.transform.position = go.transform.position;
            go2.transform.rotation = go.transform.rotation;
            go.destroy(false);
        } else if (pi && (pi.ClassId == "92fb421e-a3f6-4b0b-8542-fd4faee4202a" ||
                          pi.classId == "53ffa3e8-f2f7-43b8-a5c7-946e766aff64")) {
            for (var i = 0; i < PurpleTabletsToBreak.Count; i++) {
                var pos = PurpleTabletsToBreak[i];
                if (Vector3.Distance(pos, go.transform.position) <= 0.2) {
                    var go2 = ObjectUtil.createWorldObject(
                        i == 2 ? SeaToSeaMod.PurpleTabletPartA.ClassID : "83b61f89-1456-4ff5-815a-ecdc9b6cc9e4"
                    );
                    go2.transform.position = go.transform.position;
                    go2.transform.rotation = go.transform.rotation;
                    go.destroy(false);
                }
            }
        } else if (SNUtil.Match(pi, "83b61f89-1456-4ff5-815a-ecdc9b6cc9e4")) { //broken purple tablet
            var light = ObjectUtil.lookupPrefab("53ffa3e8-f2f7-43b8-a5c7-946e766aff64").GetComponentInChildren<Light>()
                .gameObject;
            var rel = light.transform.localPosition;
            light = light.clone();
            light.transform.parent = pi.transform;
            light.transform.localPosition = rel;
            var l = light.GetComponent<Light>();
            l.intensity *= 0.67F;
            //l.intensity = 0.4F;
            //l.range = 18F;
            //l.color = new Color(0.573, 0.404, 1.000, 1.000);
            l.shadows = LightShadows.Soft;
            var f = light.EnsureComponent<FlickeringLight>();
            f.dutyCycle = 0.5F;
            f.updateRate = 0.3F;
            f.fadeRate = 5F;
            go.EnsureComponent<TabletFragmentTag>();
            /*
            GameObject models = pi.gameObject.getChildObject("precursor_key_cracked_01");
            MeshRenderer[] parts = models.GetComponentsInChildren<MeshRenderer>();
            SNUtil.log("Checking fragmentation of purple tablet @ "+go.transform.position+": "+parts.Length+" in map "+purpleTabletsToRemoveParts.toDebugString());
            if (parts.Length == 2) {
                foreach (KeyValuePair<Vector3, bool[]> kvp in purpleTabletsToRemoveParts) {
                    if (Vector3.Distance(kvp.Key, go.transform.position) <= 0.2) {
                        SNUtil.log("Found match "+kvp.Value.toDebugString());
                        for (int i = 0; i < parts.Length; i++) {
                            parts[i].gameObject.SetActive(i >= kvp.Value.Length || kvp.Value[i]);
                        }
                        break;
                    }
                }
            }*/
        } else if (SNUtil.Match(pi, "1c34945a-656d-4f70-bf86-8bc101a27eee")) {
            go.EnsureComponent<C2CMoth>();
            go.EnsureComponent<BrightLightController>().setLightValues(120, 1.75F, 135, 180, 2.5F)
                .setPowerValues(0.15F, 0.5F);
            go.EnsureComponent<SeamothTetherController>();
            //go.EnsureComponent<VoidSpikeLeviathanSystem.SeamothStealthManager>();
        } else if (SNUtil.Match(pi, "ba3fb98d-e408-47eb-aa6c-12e14516446b")) { //prawn
            var td = go.EnsureComponent<TemperatureDamage>();
            td.minDamageTemperature = 350;
            td.baseDamagePerSecond = Mathf.Max(10, td.baseDamagePerSecond) * 0.33F;
            td.onlyLavaDamage = false;
            td.InvokeRepeating(nameof(TemperatureDamage.UpdateDamage), 1f, 1f);
            //go.removeComponent<ImmuneToPropulsioncannon>();
            go.EnsureComponent<BrightLightController>().setLightValues(120, 1.6F, 120, 150, 2.25F)
                .setPowerValues(0.25F, 0.67F);
        } else if (SNUtil.Match(pi, "8b113c46-c273-4112-b7ef-65c50d2591ed")) { //rocket
            go.EnsureComponent<C2CRocket>();
        } else if (SNUtil.Match(pi, "d4be3a5d-67c3-4345-af25-7663da2d2898")) { //cuddlefish
            var p = go.EnsureComponent<Pickupable>();
            p.isPickupable = true;
            p.overrideTechType = TechType.Cutefish;
        }
        /*
        else if (SNUtil.match(pi, auroraStorageModule.prefabName && Vector3.Distance(auroraStorageModule.position, go.transform.position) <= 0.2) {
            go.transform.position = auroraCyclopsModule.position;
            go.transform.rotation = auroraCyclopsModule.rotation;
        }
        else if (SNUtil.match(pi, auroraCyclopsModule.prefabName && Vector3.Distance(auroraCyclopsModule.position, go.transform.position) <= 0.2) {
            go.transform.position = auroraStorageModule.position;
            go.transform.rotation = auroraStorageModule.rotation;
        }*/
        else if (SNUtil.Match(pi, AuroraDepthModule.prefabName) &&
                 Vector3.Distance(AuroraDepthModule.position, go.transform.position) <= 0.2) {
            var go2 = ObjectUtil.createWorldObject(SeaToSeaMod.BrokenAuroraDepthModule.ClassID);
            go2.transform.position = go.transform.position;
            go2.transform.rotation = go.transform.rotation;
            go.destroy(false);
        } else if (SNUtil.Match(pi, "bc9354f8-2377-411b-be1f-01ea1914ec49") &&
                   Vector3.Distance(AuroraRepulsionGunTerminal, go.transform.position) <= 0.2) {
            pi.GetComponent<StoryHandTarget>().goal = SeaToSeaMod.AuroraTerminal;
        } else if (pi && pi.GetComponent<BlueprintHandTarget>()) {
            DamagedDataboxSystem.instance.onDataboxSpawn(go);
            go.EnsureComponent<ImmuneToPropulsioncannon>();
        } else if (pi && (pi.ClassId == VanillaResources.MAGNETITE.prefab ||
                          pi.ClassId == VanillaResources.LARGE_MAGNETITE.prefab)) {
            go.EnsureComponent<Magnetic>();
        } else if (SNUtil.Match(pi, "160e99a7-cb46-409d-98e2-360a76ff92da")) {
            go.EnsureComponent<C2CStasisRifle>();
        }

        var sub = (bool)pi ? pi.GetComponent<SubRoot>() : go.GetComponent<SubRoot>();
        if (sub) {
            go.EnsureComponent<Magnetic>();
            if (sub.isCyclops)
                go.EnsureComponent<BrightLightController>().setLightValues(0, 0, 135, 200, 2.0F)
                    .setPowerValues(0, /*0.4F1.6F*/0.6F);
        }

        if (go.GetComponent<BaseCell>() || go.GetComponent<Constructable>() || go.FindAncestor<Vehicle>()) {
            go.EnsureComponent<Magnetic>();
        }

        if (go.GetComponent<MeleeAttack>()) {
            go.EnsureComponent<AttackRelay>();
        }

        if (pi && !FloaterRocks.Contains(pi.ClassId) && CraftData.GetTechType(go) != TechType.FloatingStone &&
            go.GetComponent<Drillable>()) {
            var rb = go.FindAncestor<Rigidbody>();
            if (rb)
                rb.mass = Mathf.Max(2400, rb.mass);
        }

        if (pi)
            KeypadCodeSwappingSystem.instance.handleDoor(pi);

        var panel = go.GetComponent<WeldableWallPanelGeneric>();
        if (panel && panel.liveMixin)
            panel.liveMixin.data.canResurrect = true;
    }

    public static void OnFireSpawn(VFXExtinguishableFire fire) { /*
        SNUtil.log("Spawned fire "+fire+" @ "+fire.transform.position);
        PrefabIdentifier pi = fire.gameObject.FindAncestor<PrefabIdentifier>();
        SNUtil.log("pi: "+(pi ? pi.classId : "null"));
        if (pi && auroraFires.Contains(pi.ClassId)) {
            blueAuroraPrawnFire(pi.gameObject);
        }*/
        fire.gameObject.EnsureComponent<AuroraFireChecker>();
    }

    private static void BlueAuroraPrawnFire(GameObject fire) {
        fire.EnsureComponent<AuroraFireBluer>();
    }

    private class AuroraFireChecker : MonoBehaviour {
        private void Update() {
            var pi = gameObject.FindAncestor<PrefabIdentifier>();
            if (pi) {
                if (AuroraFires.Contains(pi.ClassId) &&
                    EnvironmentalDamageSystem.Instance.IsPositionInAuroraPrawnBay(pi.transform.position)) {
                    pi.gameObject.EnsureComponent<AuroraFireBluer>();
                }

                this.destroy(false);
            }
        }
    }

    private class AuroraFireBluer : MonoBehaviour {
        private float _age;

        private void Update() {
            _age += Time.deltaTime;
            var flag = false;
            //SNUtil.log("Trying to blue prawn bay fire "+gameObject.name+" @ "+transform.position);
            foreach (var r in GetComponentsInChildren<Renderer>(true)) {
                if (!r || r.name == null || r.materials == null || r.materials.Length == 0)
                    continue;
                //SNUtil.log("Checking renderer "+r.name+" in "+r.gameObject.GetFullHierarchyPath());
                if (AuroraPrawnFireColors.ContainsKey(r.name)) {
                    foreach (var m in r.materials) {
                        //SNUtil.log("Setting color to "+auroraPrawnFireColors[r.name]);
                        if (!m)
                            continue;
                        m.color = AuroraPrawnFireColors[r.name];
                        flag = true;
                    }
                }
            }

            var l = GetComponentInChildren<Light>();
            if (l)
                l.color = new Color(0.55F, 0.67F, 1F);
            if (flag && _age >= 0.5F) {
                var l2 = gameObject.addLight(0.4F, 32F, l.color).setName("BlueFireLight");
                //SNUtil.log("Bluing complete. Destroying component.");
                this.destroy(false);
            }
        }
    }

    public static void OnStartWaterFilter(FiltrationMachine fm) {
        fm.storageContainer.Resize(2, 3); //add another row for byproducts
        fm.gameObject.EnsureComponent<C2CWaterFilter>().Machine = fm;
    }

    private class C2CWaterFilter : MonoBehaviour {
        internal FiltrationMachine Machine;

        private float _lastBiomeCheck;

        private BiomeBase _biome;

        private void Update() {
            if (!Machine)
                Machine = GetComponent<FiltrationMachine>();
            var time = DayNightCycle.main.timePassedAsFloat;
            if (time - _lastBiomeCheck >= 30) {
                _biome = BiomeBase.GetBiome(Machine.transform.position);
                _lastBiomeCheck = time;
            }

            if (_biome == VanillaBiomes.Lostriver || _biome == VanillaBiomes.Cove) {
                Machine.timeRemainingSalt -= Time.deltaTime * 1.5F; //2.5x salt rate in lost river
                if (Machine.timeRemainingSalt <= 0 &&
                    Machine.storageContainer.container.GetCount(TechType.Salt) < Machine.maxSalt) { //need to recheck
                    Machine.timeRemainingSalt = -1f;
                    Machine.Spawn(Machine.saltPrefab);
                    Machine.TryFilterSalt();
                }
            }
        }
    }

    /*

    public static void onPingAdd(uGUI_PingEntry e, PingType type, string name, string text) {
        SNUtil.log("Ping ID type "+type+" = "+name+"|"+text+" > "+e.label.text);
    }*/

    private class AttackRelay : MonoBehaviour {
        private void OnMeleeAttack(GameObject target) {
            if (target == Player.main.gameObject)
                LifeformScanningSystem.instance.onObjectSeen(gameObject, false);
        }
    }

    public static void TickFruitPlant(DIHooks.FruitPlantTag fpt) {
        if (SkipFruitPlantTick)
            return;
        var fp = fpt.GetPlant();
        if (fp && fp.gameObject.isFarmedPlant() && WorldUtil.isPlantInNativeBiome(fp.gameObject)) {
            fp.fruitSpawnInterval = fpt.GetBaseGrowthTime() / 1.5F;
        }
    }

    private class PrawnBayDoorTriggers : MonoBehaviour {
        internal GenericHandTarget HoverHint;

        internal StarshipDoor Door;

        private bool _wasOpen;

        public void UnlockDoor() {
            if (HoverHint)
                HoverHint.destroy();
        }

        private void Update() {
            if (Door && Door.doorOpen && !_wasOpen) {
                _wasOpen = true;
                EnvironmentalDamageSystem.Instance.TriggerAuroraPrawnBayWarning();
                Player.main.liveMixin.TakeDamage(5, Player.main.transform.position, DamageType.Heat, gameObject);
            }
        }
    }

    public static void UpdateSeamothModules(SeaMoth sm, int slotID, TechType tt, bool added) {
        sm.gameObject.EnsureComponent<C2CMoth>().recalculateModules();
        sm.gameObject.EnsureComponent<BrightLightController>().recalculateModule();
        sm.gameObject.EnsureComponent<SeamothTetherController>().recalculateModule();
        if (added && GameModeUtils.currentEffectiveMode != GameModeOption.Creative && !SNUtil.CanUseDebug())
            ItemUnlockLegitimacySystem.instance.validateModule(sm, slotID, tt);
    }

    public static void UpdateCyclopsModules(SubRoot sm) {
        if (C2CIntegration.seaVoyager != TechType.None && sm.GetType() == C2CIntegration.seaVoyagerComponent) {
            //this is the load hook as it has no SkyAppliers
            sm.gameObject.EnsureComponent<C2CVoyager>();
            return;
        }

        sm.gameObject.EnsureComponent<BrightLightController>().recalculateModule();
        C2CUtil.resizeCyclopsStorage(sm);
        if (GameModeUtils.currentEffectiveMode != GameModeOption.Creative && !SNUtil.CanUseDebug())
            ItemUnlockLegitimacySystem.instance.validateModules(sm);
    }

    public static void UpdatePrawnModules(Exosuit sm, int slotID, TechType tt, bool added) {
        sm.gameObject.EnsureComponent<BrightLightController>().recalculateModule();
        if (added && GameModeUtils.currentEffectiveMode != GameModeOption.Creative && !SNUtil.CanUseDebug())
            ItemUnlockLegitimacySystem.instance.validateModule(sm, slotID, tt);
    }

    public static void UseSeamothModule(SeaMoth sm, TechType tt, int slotID) {
    }

    public static float GetVehicleTemperature(Vehicle v) {
        return C2CMoth.getOverrideTemperature(v, WaterTemperatureSimulation.main.GetTemperature(v.transform.position));
    }

    public static bool IsSpawnableVoid(string biome) {
        var ret = VoidSpikeLeviathanSystem.instance.isSpawnableVoid(biome);
        if (ret && Player.main.IsSwimming() && !Player.main.GetVehicle() &&
            VoidGhostLeviathansSpawner.main.spawnedCreatures.Count < 3 &&
            !VoidSpikesBiome.instance.IsInBiome(Player.main.transform.position)) {
            VoidGhostLeviathansSpawner.main.timeNextSpawn = Time.time - 1;
        }

        return ret;
    }

    public static GameObject GetVoidLeviathan(VoidGhostLeviathansSpawner spawner, Vector3 pos) {
        return VoidSpikeLeviathanSystem.instance.getVoidLeviathan(spawner, pos);
    }

    public static void TickVoidLeviathan(GhostLeviatanVoid gv) {
        if (SkipVoidLeviTick)
            return;
        VoidSpikeLeviathanSystem.instance.tickVoidLeviathan(gv);
    }

    public static void PingSeamothSonar(SeaMoth sm) {
        var vv = VanillaBiomes.Void.IsInBiome(sm.transform.position);
        VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(sm, vv ? 30 : 10);
        if (vv) {
            for (var i = VoidGhostLeviathansSpawner.main.spawnedCreatures.Count;
                 i < VoidGhostLeviathansSpawner.main.maxSpawns;
                 i++) {
                VoidGhostLeviathansSpawner.main.timeNextSpawn = 0.1F;
                VoidGhostLeviathansSpawner.main.UpdateSpawn(); //trigger spawn and time recalc
            }
        }
    }

    public static void OnTorpedoFired(Bullet b, Vehicle v) {
        if (v is SeaMoth moth)
            VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(
                moth,
                SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 30 : 15
            );
    }

    public static void OnTorpedoExploded(SeamothTorpedo p, Transform result) {
        var v = Player.main.GetVehicle();
        if (v is SeaMoth moth)
            VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(
                moth,
                SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 60 : 30
            );
    }

    public static void PingAnySonar(SNCameraRoot cam) {
        if (VoidSpikesBiome.instance.IsInBiome(cam.transform.position)) {
            VoidSpikeLeviathanSystem.instance.triggerEMInterference();
        }
    }

    public static void PulseSeamothDefence(SeaMoth sm) {
        VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(sm, 12);
    }

    public static void OnBaseSonarPinged(GameObject go) {
        if (VoidSpikesBiome.instance.IsInBiome(go.transform.position)) {
            var ep = Player.main;
            var v = ep.GetVehicle();
            if (v && v is SeaMoth sm && VoidSpikesBiome.instance.IsInBiome(ep.transform.position))
                VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(sm, 40);
        }
    }

    public static void GetGrinderDrillableDrop(DrillableGrindingResult res) {
        if (res.materialTech == TechType.Sulphur) {
            //SNUtil.writeToChat("Intercepting grinding sulfur");
            StoryGoal.Execute("GrabSulfur", Story.GoalType.Story);
            res.drop = ObjectUtil.lookupPrefab(CraftingItems.getItem(CraftingItems.Items.SulfurAcid).ClassID);
            res.dropCount = UnityEngine.Random.Range(0F, 1F) < 0.33F ? 2 : 1;
        }
    }

    public static void OnLavaBombHit(LavaBombTag bomb, GameObject hit) {
        if (hit) {
            var cm = hit.FindAncestor<C2CMoth>();
            if (cm)
                cm.onHitByLavaBomb(bomb);
            if (hit.layer == Voxeland.GetTerrainLayerMask() || hit.layer == 30) {
                //for some reason this sometimes causes multiple (1-3!) to drop but that is actually a good thing
                var bs = ObjectUtil.createWorldObject(
                    CustomMaterials.getItem(CustomMaterials.Materials.OBSIDIAN).ClassID
                );
                bs.transform.position = bomb.transform.position;
                /*
                SinkingGroundChunk s = bs.EnsureComponent<SinkingGroundChunk>();
                s.modelTransform = bs.GetComponentInChildren<Renderer>().transform;
                s.sinkHeight = 1;
                s.sinkTime = 10;
                */
                bs.applyGravity();
            }
        }
    }

    public static void OnAnchorPodExplode(ExplodingAnchorPodDamage dmg) {
        if (VoidSpikesBiome.instance.IsInBiome(dmg.toDamage.transform.position) &&
            dmg.toDamage.gameObject.FindAncestor<Player>()) {
            dmg.damageAmount *= 0.67F;
        }
    }

    public static void OnBloodKelpGrab(PredatoryBloodvine kelp, GameObject tgt) {
        MoraleSystem.instance.shiftMorale(tgt.isPlayer() ? -40 : -10);
    }

    public static void OnVoidTongueGrab(VoidTongueTag tag, Rigidbody rb) {
        if (rb.isPlayer() || rb.GetComponent<Vehicle>() || rb.GetComponent<SubRoot>())
            MoraleSystem.instance.shiftMorale(-200);
        else if (rb.GetComponent<GhostLeviatanVoid>())
            MoraleSystem.instance.shiftMorale(-10);
    }

    public static void OnVoidTongueRelease(VoidTongueTag tag, Rigidbody rb) {
        if (rb.isPlayer() || rb.GetComponent<Vehicle>() || rb.GetComponent<SubRoot>())
            MoraleSystem.instance.shiftMorale(50);
    }

    public static void OnPlanktonActivated(PlanktonCloudTag cloud, GameObject hit) {
        var sm = hit.GetComponent<SeaMoth>();
        if (sm) {
            var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
            float amt = UnityEngine.Random.Range(hard ? 15 : 8, hard ? 25 : 15);
            if (VanillaBiomes.Void.IsInBiome(sm.transform.position))
                VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(sm, amt);
        }
    }

    public static void TickVoidBubbles(VoidBubbleSpawnerTick t) {
        var dist = VoidSpikesBiome.instance.getDistanceToBiome(t.player.transform.position, true) -
                   VoidSpikesBiome.biomeVolumeRadius;
        var f = (float)MathUtil.linterpolate(dist, 50, 300, 0, 1, true);
        //SNUtil.writeToChat(dist.ToString("0.0")+" > "+f.ToString("0.0000"));
        t.spawnChance *= f;
    }

    public static void TickVoidBubble(VoidBubbleTag t) {
        var dist = VoidSpikesBiome.instance.getDistanceToBiome(Player.main.transform.position, true) -
                   VoidSpikesBiome.biomeVolumeRadius;
        if (dist <= 120) {
            t.fade(dist <= 80 ? 2 : dist <= 80 ? 5 : 10);
        }
    }

    public static ClipMapManager.Settings ModifyWorldMeshSettings(ClipMapManager.Settings values) {
        var baseline = values.levels[0];

        for (var i = 1; i < values.levels.Length - 2; i++) {
            var lvl = values.levels[i];

            if (lvl.entities) {
                //lvl.downsamples = baseline.downsamples;
                lvl.colliders = true;
                //lvl.grass = true;
                //lvl.grassSettings = baseline.grassSettings;
            }
        }

        return values;
    }

    public static string GetO2Tooltip(Oxygen ox) {
        return ox.GetComponent<Pickupable>().GetTechType() == C2CItems.liquidTank.TechType
            ? ox.GetSecondsLeft() + "s fluid stored in supply tank"
            : LanguageCache.GetOxygenText(ox.GetSecondsLeft());
    }

    public static string GetBatteryTooltip(Battery ox) {
        return ox.GetComponent<Pickupable>().GetTechType() == C2CItems.liquidTank.TechType
            ? Mathf.RoundToInt(ox.charge) + "s fluid stored in primary tank"
            : Language.main.GetFormat(
                "BatteryCharge",
                ox.charge / ox.capacity,
                Mathf.RoundToInt(ox.charge),
                ox.capacity
            );
    }

    public static void OnClickedVehicleUpgrades(VehicleUpgradeConsoleInput v) {
        if (v.docked || SeaToSeaMod.AnywhereSeamothModuleCheatActive ||
            GameModeUtils.currentEffectiveMode == GameModeOption.Creative)
            v.OpenPDA();
    }

    public static void OnHoverVehicleUpgrades(VehicleUpgradeConsoleInput v) {
        var main = HandReticle.main;
        if (!v.docked && !SeaToSeaMod.AnywhereSeamothModuleCheatActive &&
            GameModeUtils.currentEffectiveMode != GameModeOption.Creative) {
            main.SetInteractText("DockToChangeVehicleUpgrades"); //locale key
            main.SetIcon(HandReticle.IconType.HandDeny, 1f);
        } else if (v.equipment != null) {
            main.SetInteractText(v.interactText);
            main.SetIcon(HandReticle.IconType.Hand, 1f);
        }
    }

    public static void TryKnife(DIHooks.KnifeAttempt k) {
        LifeformScanningSystem.instance.onObjectSeen(k.Target.gameObject, false);
        var tt = CraftData.GetTechType(k.Target.gameObject);
        if (tt == TechType.BlueAmoeba || tt == SeaToSeaMod.GelFountain.TechType) {
            k.AllowKnife = true;
            return;
        }

        var a = k.Target.GetComponent<AlkaliPlantTag>();
        if (a) {
            k.AllowKnife = a.isHarvestable();
            return;
        }
    }

    public static GameObject GetStalkerShinyTarget(GameObject def, CollectShiny cc) {
        if (SkipStalkerShiny)
            return def;
        if (cc.shinyTarget && cc.GetComponent<DeepStalkerTag>()) {
            bool hasPlat = cc.shinyTarget.GetComponent<PlatinumTag>();
            bool lookingAtPlat = def.GetComponent<PlatinumTag>();
            return hasPlat == lookingAtPlat ? def : hasPlat ? cc.shinyTarget : def;
        }

        return def;
    }

    public static void OnShinyTargetIsCurrentlyHeldByStalker(CollectShiny cc) {
        if (SkipStalkerShiny)
            return;
        if (cc.shinyTarget && cc.shinyTarget.GetComponent<PlatinumTag>()) {
            var ds = cc.GetComponent<DeepStalkerTag>();
            ds.tryStealFrom(cc.shinyTarget.GetComponentInParent<Stalker>());
        } else {
            cc.targetPickedUp = false;
            cc.shinyTarget = null;
        }
    }

    public static bool StalkerTryDropTooth(Stalker s) {
        return (!s.GetComponent<DeepStalkerTag>() || UnityEngine.Random.Range(0F, 1F) > 0.8) &&
               (!s.GetComponent<WaterParkCreature>() || PDAScanner.complete.Contains(TechType.StalkerTooth)) &&
               s.LoseTooth();
    }

    public static void TryEat(DIHooks.EatAttempt ea) {
        if (LiquidBreathingSystem.Instance.HasLiquidBreathing())
            ea.AllowEat = false;
    }

    public static void TryLaunchRocket(LaunchRocket r) {
        if (!r.IsRocketReady())
            return;
        if (LaunchRocket.launchStarted)
            return;
        if (!StoryGoalCustomEventHandler.main.gunDisabled && !r.forcedRocketReady) {
            r.gunNotDisabled.Play();
            return;
        }

        if (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
            if (!C2CUtil.checkConditionAndShowPDAAndVoicelogIfNot(
                    ExplorationTrackerPages.instance.isFullyComplete(false),
                    ExplorationTrackerPages.INCOMPLETE_PDA,
                    PDAMessages.Messages.NeedFinishExploreTrackerMessage
                )) {
                ExplorationTrackerPages.instance.showAllPages();
                return;
            }
        }

        if (!FinalLaunchAdditionalRequirementSystem.instance.checkIfScannedAllLifeforms()) {
            return;
        }

        if (!FinalLaunchAdditionalRequirementSystem.instance.checkIfCollectedAllEncyData()) {
            return;
        }

        if (!FinalLaunchAdditionalRequirementSystem.instance.checkIfFullyLoaded()) {
            return;
        }

        //if (!FinalLaunchAdditionalRequirementSystem.instance.checkIfVisitedAllBiomes()) {
        //	return;
        //}
        if (!C2CProgression.Instance.IsRequiredProgressionComplete()) {
            SNUtil.WriteToChat("Missing progression, cannot launch");
            return;
        }

        FinalLaunchAdditionalRequirementSystem.instance.forceLaunch(r);
    }

    public static void OnEmpHit(EMPBlast e, GameObject go) {
        VoidSpikeLeviathanSystem.instance.onObjectEMPHit(e, go);
    }
    /*
    public static void interceptChosenFog(DIHooks.WaterFogValues fog) {
        double d = VoidSpikesBiome.instance.getDistanceToBiome(Camera.main.transform.position, true)-VoidSpikesBiome.biomeVolumeRadius;
        if (d <= 50 && d > 0) {
            float f = (float)(1-d/50F);
            fog.density = (float)MathUtil.linterpolate(f, 0, 1, fog.originalDensity, VoidSpikesBiome.fogDensity, true);
            fog.color = Color.Lerp(fog.originalColor, VoidSpikesBiome.waterColor, f);
            return;
        }
        d = UnderwaterIslandsFloorBiome.instance.getDistanceToBiome(Camera.main.transform.position);
        //SNUtil.writeToChat(d.ToString("0.000"));
        if (d <= 100 && d > 0) {
            float f = (float)(1-d/100F);
            fog.density = (float)MathUtil.linterpolate(f, 0, 1, fog.originalDensity, UnderwaterIslandsFloorBiome.fogDensity, true);
            fog.sunValue = (float)MathUtil.linterpolate(f, 0, 1, fog.originalSunValue, UnderwaterIslandsFloorBiome.sunIntensity, true);
            fog.color = Color.Lerp(fog.originalColor, UnderwaterIslandsFloorBiome.waterColor, f);
            return;
        }
    }*/

    public static float GetRadiationLevel(DIHooks.RadiationCheck ch) {
        //SNUtil.writeToChat(ch.originalValue+" @ "+VoidSpikesBiome.instance.getDistanceToBiome(ch.position));
        if (VoidSpikesBiome.instance.getDistanceToBiome(ch.Position) <= VoidSpikesBiome.biomeVolumeRadius + 75)
            return 0;
        var dd = Vector3.Distance(ch.Position, BkelpBaseGeoCenter);
        if (dd <= 80) {
            var ret = (float)MathUtil.linterpolate(dd, 60, 80, 0.25F, 0, true);
            if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
                ret -= 0.17F;
            //do not require, as need rebreather v2 if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
            //	ret -= 0.12F;
            if (Inventory.main.equipment.GetCount(TechType.RadiationGloves) > 0)
                ret -= 0.08F;
            if (ret > 0)
                return ret;
        }

        return ch.Value;
    }

    public static float GetSolarEfficiencyLevel(DIHooks.SolarEfficiencyCheck ch) {
        if (!SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
            return ch.Value;
        var depth = Mathf.Max(0F, Ocean.GetDepthOf(ch.Panel.gameObject));
        var effectiveDepth = depth;
        if (depth > 150)
            effectiveDepth = Mathf.Max(depth, 250);
        else if (depth > 100)
            effectiveDepth = (float)MathUtil.linterpolate(depth, 100, 150, 125, 250, true);
        else if (depth > 50)
            effectiveDepth = (float)MathUtil.linterpolate(depth, 50, 100, 50, 125, true);
        var f = Mathf.Clamp01((ch.Panel.maxDepth - effectiveDepth) / ch.Panel.maxDepth);
        //SNUtil.writeToChat(depth+" > "+effectiveDepth+" > "+f+" > "+ch.panel.depthCurve.Evaluate(f));
        return ch.Panel.depthCurve.Evaluate(f) * ch.Panel.GetSunScalar();
    }

    public static float GetModuleFireCost(DIHooks.ModuleFireCostCheck ch) {
        var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
        if (hard)
            ch.Value *= 1.5F;
        if (ch.Module == TechType.SeamothSonarModule)
            ch.Value *= hard ? 8 / 3F : 4 / 3F;
        return ch.Value;
    }

    public static void FireSeamothDefence(SeaMoth sm) {
        VoidSpikeLeviathanSystem.instance.temporarilyDisableSeamothStealth(sm, 10); //x1.5 on hard already
        sm.energyInterface.ConsumeEnergy(SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 5 : 3);
    }

    public static void GenerateItemTooltips(StringBuilder sb, TechType tt, GameObject go) {
        if (tt == TechType.LaserCutter && HasLaserCutterUpgrade()) {
            TooltipFactory.WriteDescription(
                sb,
                "\nCutting Temperature upgraded to allow cutting selected seabase structural elements"
            );
        } else if (tt == C2CItems.emperorRootOil.TechType) {
            var tag = go.GetComponent<EmperorRootOil.EmperorRootOilTag>();
            if (tag && tag.pickupTime >= 0) {
                var age = DayNightCycle.main.timePassedAsFloat - tag.pickupTime;
                var pct = (float)MathUtil.linterpolate(age, 0, EmperorRootOil.LIFESPAN, 100, 0, true);
                TooltipFactory.WriteDescription(sb, "\n" + pct.ToString("0.0") + "% freshness remaining");
            }
        }
    }

    public static void InterceptBulkheadLaserCutter(DIHooks.BulkheadLaserCutterHoverCheck ch) {
        if (!HasLaserCutterUpgrade())
            ch.RefusalLocaleKey = "Need_laserCutterBulkhead_Chit";
    }

    public static bool HasLaserCutterUpgrade() {
        return StoryGoalManager.main.completedGoals.Contains(SeaToSeaMod.LaserCutterBulkhead.goal.key);
    }

    public static void OnKnifed(GameObject go) {
        var tt = CraftData.GetTechType(go);
        if (tt == TechType.BlueAmoeba)
            DIHooks.FireKnifeHarvest(
                go,
                new Dictionary<TechType, int> {
                    {
                        CraftingItems.getItem(CraftingItems.Items.AmoeboidSample).TechType,
                        1
                    },
                }
            );
        else if (tt == SeaToSeaMod.GelFountain.TechType)
            go.GetComponent<GelFountainTag>().onKnifed();
    }

    public static void InterceptItemHarvest(DIHooks.KnifeHarvest h) {
        if (h.Drops.Count > 0) {
            if (h.ObjectType == C2CItems.kelp.TechType) {
                var tag = h.Hit.FindAncestor<GlowKelpTag>();
                h.Drops[h.DefaultDrop] = 2;
                var f = tag.isFarmed() ? 0 : 0.25F;
                var egg = CustomEgg.GetEgg(C2CItems.purpleHolefish.TechType).TechType;
                f -= Inventory.main.GetPickupCount(egg) * 0.2F; //100% chance if 0, 80% chance if 1, down to 0% at >= 5
                /*
                WaterPark wp = tag.getACU();
                if (wp && wp.GetComponentInChildren<PurpleHolefishTag>())
                    f = 0.06F;
                    */
                if (f > 0 && UnityEngine.Random.Range(0F, 1F) <= f)
                    h.Drops[egg] = 1;
            }

            if (h.Hit.isFarmedPlant() && WorldUtil.isPlantInNativeBiome(h.Hit)) {
                h.Drops[h.DefaultDrop] *= 2;
            }
        }
    }

    public static void OnReaperGrab(ReaperLeviathan r, Vehicle v) {
        MoraleSystem.instance.shiftMorale(v == Player.main.GetVehicle() ? -40 : -20);
        if (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) &&
            KnownTech.Contains(TechType.BaseUpgradeConsole) && !KnownTech.Contains(TechType.SeamothElectricalDefense)) {
            KnownTech.Add(TechType.SeamothElectricalDefense);
            SNUtil.TriggerTechPopup(TechType.SeamothElectricalDefense);
        }
    }

    public static void OnCyclopsDamage(SubRoot r, DamageInfo di) { /*
        if (SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) && !KnownTech.Contains(TechType.CyclopsShieldModule)) {
            float healthFraction = r.live.GetHealthFraction();
            float num = (100f - r.damageManager.overshieldPercentage) / 100f;
            if (healthFraction < num) { //health below auto regen level
                KnownTech.Add(TechType.CyclopsShieldModule);
                SNUtil.triggerTechPopup(TechType.CyclopsShieldModule);
            }
        }*/
    }

    public static bool ChargerConsumeEnergy(IPowerInterface pi, float amt, out float consumed, Charger c) {
        if (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) &&
            (c is PowerCellCharger || c.GetType().Name.Contains("FCS")))
            amt *= 1.5F;
        return pi.ConsumeEnergy(amt, out consumed);
    }

    public static void TickScannerCamera(MapRoomCamera cam) {
        cam.gameObject.EnsureComponent<CameraLeviathanAttractor>();
        var campos = cam.transform.position;
        if (VoidSpikesBiome.instance.getDistanceToBiome(campos, true) < 200) {
            var time = DayNightCycle.main.timePassedAsFloat;
            if (time > _nextCameraEmpTime) {
                var d = UnityEngine.Random.Range(96F, 150F);
                var pos = campos + cam.transform.forward * d;
                pos = MathUtil.getRandomVectorAround(pos, 45);
                pos = campos + (pos - campos).SetLength(d);
                VoidSpikeLeviathanSystem.instance.spawnEMPBlast(pos);
                _nextCameraEmpTime = time + UnityEngine.Random.Range(1.2F, 2.5F);
            }
        }

        var temp = EnvironmentalDamageSystem.Instance.GetWaterTemperature(campos);
        if (temp >= 100) {
            var amt = 5 * (1 + (temp - 100) / 100F);
            cam.liveMixin.TakeDamage(amt * Time.deltaTime, campos, DamageType.Heat);
        }

        if (!cam.dockingPoint) {
            var leak = EnvironmentalDamageSystem.Instance.GetLrPowerLeakage(cam.gameObject);
            if (leak >= 0) {
                cam.energyMixin.ConsumeEnergy(leak * Time.deltaTime * 0.5F);
            }
        }
    }

    public static float GetCrushDamage(CrushDamage dmg) {
        float f = 1;
        if (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
            var ratio = dmg.GetDepth() / dmg.crushDepth;
            if (ratio > 1) {
                f += Mathf.Pow(
                         ratio,
                         4
                     ) -
                     1; //so at 1700 with a limit of 1300 it is ~3x as much damage; at 1200 with a 900 limit it is 3.2x, at 900 with 500 it is 10.5x
                ratio = (dmg.GetDepth() - 900) / 300F; //add another +33% per 100m over 900m
                if (ratio > 0)
                    f += ratio;
            } //net result: 1700 @ 1300 = 5.6x, 1200 @ 900 = 2.8x, 900 @ 500 = 7x, 300 @ 200 = 3.3x
        }

        return dmg.damagePerCrush * f;
    }

    internal static void IsItemMapRoomDetectable(ESHooks.ResourceScanCheck rt) {
        if (rt.Resource.techType == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType) {
            rt.IsDetectable = PDAScanner.complete.Contains(rt.Resource.techType) ||
                              StoryGoalManager.main.completedGoals.Contains(
                                  "Precursor_LavaCastle_Log2"
                              ); //mentions lava castle
        } else if (rt.Resource.techType == CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType) {
            rt.IsDetectable = PDAScanner.complete.Contains(rt.Resource.techType) ||
                              PDAManager.getPage("sunbeamdebrishint").isUnlocked();
        } else if (ScanToScannerRoom.Contains(rt.Resource.techType)) {
            rt.IsDetectable = PDAScanner.complete.Contains(rt.Resource.techType);
        } else if (rt.Resource.techType == SeaToSeaMod.MushroomBioFragment.TechType) {
            rt.IsDetectable = SNUtil.GetFragmentScanCount(rt.Resource.techType) >
                              SeaToSeaMod.MushroomBioFragment.fragmentCount - 2;
        } else if (rt.Resource.techType == SeaToSeaMod.GeyserCoral.TechType) {
            rt.IsDetectable = SNUtil.GetFragmentScanCount(rt.Resource.techType) >
                              SeaToSeaMod.GeyserCoral.fragmentCount - 4;
        }

        if (rt.Resource.GetComponent<Drillable>()) {
            rt.IsDetectable = StoryGoalManager.main.completedGoals.Contains("OnConstructExosuit") ||
                              KnownTech.knownTech.Contains(AqueousEngineeringMod.grinderBlock.TechType);
        }
    }

    private static void OnVehicleEnter(Vehicle v, Player ep) { /*
        if (v is SeaMoth) {
            VoidSpikesBiome.instance.onSeamothEntered((SeaMoth)v, ep);
        }*/
    }

    public static void GetCompassDepthLevel(DIHooks.DepthCompassCheck ch) {
        if (SkipCompassCalc)
            return;
        if (VoidSpikeLeviathanSystem.instance.isVoidFlashActive(true)) {
            ch.Value = VoidSpikeLeviathanSystem.instance.getRandomDepthForDisplay();
            ch.CrushValue = 1000 - ch.Value;
        }
    }

    public static bool OnStasisFreeze(StasisSphere s, Rigidbody c) {
        var pi = c.GetComponent<PrefabIdentifier>();
        //SNUtil.writeToChat("Froze "+pi??pi.ClassId);
        if (pi && pi.ClassId == C2CItems.alkali.ClassID) {
            pi.GetComponentInChildren<AlkaliPlantTag>().OnFreeze( /*s.time*/);
            return true;
        }

        return false;
    }

    public static bool OnStasisUnFreeze(StasisSphere s, Rigidbody c) {
        var pi = c.GetComponent<PrefabIdentifier>();
        //SNUtil.writeToChat("Unfroze "+pi??pi.ClassId);
        if (pi && pi.ClassId == C2CItems.alkali.ClassID) {
            pi.GetComponentInChildren<AlkaliPlantTag>().OnUnfreeze();
            return true;
        }

        return false;
    }

    public static float Get3AxisSpeed(float orig, Vehicle v, Vector3 input) {
        if (orig <= 0 || input.magnitude < 0.01F)
            return orig;
        //vanilla is float d = Mathf.Abs(vector.x) * this.sidewardForce + Mathf.Max(0f, vector.z) * this.forwardForce + Mathf.Max(0f, -vector.z) * this.backwardForce + Mathf.Abs(vector.y * this.verticalForce);
        var netForward = Mathf.Max(0, input.z) * v.forwardForce + Mathf.Max(0, -input.z) * v.backwardForce;
        var inputFracX = Mathf.Pow(Mathf.Abs(input.x / input.magnitude), 0.75F);
        var inputFracY = Mathf.Pow(Mathf.Abs(input.y / input.magnitude), 0.75F);
        var inputFracZ = Mathf.Pow(Mathf.Abs(input.z / input.magnitude), 0.75F);
        var origX = Mathf.Abs(input.x) * v.sidewardForce;
        var origY = Mathf.Abs(input.y * v.verticalForce);
        var ret = netForward * inputFracZ + origX * inputFracX +
                  origY * inputFracY; //multiply each component by its component of the input vector rather than a blind sum
        //SNUtil.writeToChat("Input vector "+input+" > speeds "+orig.ToString("00.0000")+" & "+ret.ToString("00.0000"));
        return ret;
    }

    //Not called anymore, because kick to main menu when die now
    public static void OnPlayerRespawned(Survival s, Player ep, bool post) {
        if (post) {
            var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
            s.water = Mathf.Max(hard ? 5 : 15, _waterToRestore);
            s.food = Mathf.Max(hard ? 5 : 15, _foodToRestore);
            MoraleSystem.instance.reset();
        } else {
            _waterToRestore = s.water;
            _foodToRestore = s.food;
            EnvironmentalDamageSystem.Instance.ResetCooldowns();
        }
    }

    public static void OnItemsLost() {
        /* no longer necessary because kick to main menu instead
        foreach (InventoryItem ii in ((IEnumerable<InventoryItem>)Inventory.main.container)) {
            if (ii != null && ii.item && ii.item.GetTechType() == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType) {
                ii.item.destroyOnDeath = true;
            }
        }*/
    }

    public static void OnDeath() {
        //SNUtil.writeToChat("You died);
        //IngameMenu.main.QuitGame(true);
        _playerDied = true;
        C2CUtil.setupDeathScreen();
    }

    public static void OnSelfScan() {
        var msg = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE)
            ? PDAMessages.Messages.LiquidBreathingSelfScanHard
            : PDAMessages.Messages.LiquidBreathingSelfScanEasy;
        if (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(msg).key)) {
            PDAManager.getPage("liqbrefficiency").unlock();
        }
    }

    public static void OnScanComplete(PDAScanner.EntryData data) {
        C2CProgression.Instance.OnScanComplete(data);
        LifeformScanningSystem.instance.onScanComplete(data);
        DataCollectionTracker.instance.onScanComplete(data);
        MoraleSystem.instance.shiftMorale(1);
    }

    public static void OnTechUnlocked(TechType tech, bool vb) { /*
    if (tech == TechType.PrecursorKey_Orange) {
        Story.StoryGoal.Execute(SeaToSeaMod.crashMesaRadio.key, SeaToSeaMod.crashMesaRadio.goalType);
    }
    if (tech == TechType.NuclearReactor || tech == TechType.HighCapacityTank || tech == TechType.PrecursorKey_Purple || tech == TechType.SnakeMushroom || tech == CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType) {
        Story.StoryGoal.Execute("RadioKoosh26", Story.GoalType.Radio); //pod 12
    }*/
        C2CItems.onTechUnlocked(tech);
        MoraleSystem.instance.shiftMorale(2.5F);
    }

    public static void OnDataboxTooltipCalculate(BlueprintHandTarget tgt) {
        var lv = tgt.GetComponent<LiveMixin>();
        if (lv && lv.health < lv.maxHealth) {
            HandReticle.main.SetInteractText("NeedRepairDataBox");
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
        }
    }

    public static bool OnDataboxClick(BlueprintHandTarget tgt) { //return true to prevent use
        if (tgt.used)
            return true;
        if (tgt.unlockTechType == TechType.BaseReinforcement) {
            StoryGoal.Execute(SeaToSeaMod.ReinfDBGoal, Story.GoalType.Story);
        }

        var lv = tgt.GetComponent<LiveMixin>();
        return lv && lv.health < lv.maxHealth;
    }

    public static void ApplyGeyserFilterBuildability(DIHooks.BuildabilityCheck check) {
        if (VoidSpikesBiome.instance.IsInBiome(Player.main.transform.position) ||
            (Player.main.transform.position - VoidSpikesBiome.signalLocation).sqrMagnitude <= 40000) {
            check.Placeable = false;
            return;
        }

        if (Builder.constructableTechType == C2CItems.geyserFilter.TechType) {
            check.Placeable =
                !check.PlaceOn && GeyserFilterLogic.findGeyser(Builder.GetGhostModel().transform.position);
            check.IgnoreSpaceRequirements = check.Placeable;
            //check.ignoreSpaceRequirements = true;
        }

        if (C2CIntegration.seaVoyager != TechType.None && check.PlaceOn &&
            check.PlaceOn.gameObject.GetComponentInParent(C2CIntegration.seaVoyagerComponent))
            check.Placeable = false;
    }

    public static void OnHandSend(GameObject target, HandTargetEventType e, GUIHand hand) { /*
        SNUtil.writeToChat("Hand send fired for GO "+target+"$"+target.activeInHierarchy+"::"+target.GetFullHierarchyPath()+" @ "+target.transform.position+"#"+target.GetInstanceID()+" of type "+e+", on hand "+hand+", TT="+target.GetComponent<IHandTarget>());
        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            IHandTarget iht = target.GetComponent<IHandTarget>();
            if (iht != null)
                iht.OnHandClick(hand);
        }*/
        if (e == HandTargetEventType.Hover && target)
            LifeformScanningSystem.instance.onObjectSeen(target, true);
        var spt = target.GetComponent<SanctuaryPlantTag>();
        if (spt) {
            if (e == HandTargetEventType.Hover)
                spt.OnHandHover(hand);
            else if (e == HandTargetEventType.Click)
                spt.OnHandClick(hand);
        }
    }

    public static void OnKeypadFailed(KeypadDoorConsole con) {
        KeypadCodeSwappingSystem.instance.onCodeFailed(con);
    }

    public static void ChangeEquipmentCompatibility(DIHooks.EquipmentTypeCheck ch) {
        if (ch.Item == C2CItems.lightModule.TechType && Player.main.currentSub && Player.main.currentSub.isCyclops &&
            Vector3.Distance(
                Player.main.currentSub.GetComponentInChildren<CyclopsVehicleStorageTerminalManager>().transform
                    .position,
                Player.main.transform.position
            ) >= 4.5F) {
            ch.Type = EquipmentType.CyclopsModule;
        }
    }

    // TODO: FCS Compat
    // public static List<SMLHelper.V2.Crafting.Ingredient> filterFCSRecyclerOutput(
    //     List<SMLHelper.V2.Crafting.Ingredient> li
    // ) {
    //     li.RemoveAll(i => C2CProgression.Instance.IsTechGated(i.techType));
    //     return li;
    // }
    //
    // public static List<TechType> filterFCSDrillerOutput(List<TechType> li) {
    //     li.RemoveAll(C2CProgression.Instance.IsTechGated);
    //     return li;
    // }
    //
    // public static bool canFCSDrillOperate(bool orig, MonoBehaviour drill) { //orig is actually hasOil in some cases
    //     return orig && canFCSDrillOperate(drill);
    // }

    private static float _lastDrillDepletionTime = -1;

    // TODO: FCS Compat
    // public static bool canFCSDrillOperate(MonoBehaviour drill) {
    //     //SNUtil.writeToChat("Drill "+drill+" @ "+drill.transform.position+" is trying to mine: "+orig);
    //     var ret = DrillDepletionSystem.Instance.HasRemainingLife(drill);
    //     if (!ret) {
    //         var time = DayNightCycle.main.timePassedAsFloat;
    //         if (time - lastDrillDepletionTime >= 1) {
    //             lastDrillDepletionTime = time;
    //             SNUtil.writeToChat(
    //                 "Drill in " + WorldUtil.getRegionalDescription(drill.transform.position, true) +
    //                 " has depleted the local resources."
    //             );
    //             var com = drill.GetComponent(FCSIntegrationSystem.instance.getFCSDrillOreManager());
    //             if (com) {
    //                 var p = FCSIntegrationSystem.instance.getFCSDrillOreManager().GetProperty(
    //                     "AllowedOres",
    //                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
    //                 );
    //                 p.SetValue(com, new List<TechType> { });
    //             }
    //         }
    //     }
    //
    //     return ret;
    // }
    //
    // public static void tickFCSDrill(MonoBehaviour drill) {
    //     //SNUtil.writeToChat("Ticking drill "+drill+" @ "+drill.transform.position);
    //     if (DayNightCycle.main.deltaTime > 0)
    //         DrillDepletionSystem.Instance.Deplete(drill);
    // }
    //
    // public static TechType getFCSDrillFuel() {
    //     return FCSIntegrationSystem.instance.fcsDrillFuel.TechType;
    // }
    //
    // public static TechType pickFCSDrillOre(
    //     TechType orig,
    //     MonoBehaviour drill,
    //     bool filtering,
    //     bool blacklist,
    //     HashSet<TechType> filters,
    //     List<TechType> defaultSet
    // ) {
    //     var d = DrillDepletionSystem.Instance.GetMotherlode(drill);
    //     if (d != null) {
    //         var ret = getRandomValidMotherlodeDrillYield(d);
    //         if (filtering && filters.Contains(ret) == blacklist)
    //             ret = TechType.None;
    //         //SNUtil.writeToChat("picking new drop for drill "+drill+" on "+d.ClassID+": "+ret);
    //         return ret;
    //     }
    //
    //     return orig;
    // }

    // TODO: FCS Compat
    // private static TechType getRandomValidMotherlodeDrillYield(DrillableResourceArea d) {
    //     var ret = d.getRandomResourceType();
    //     while (!isFCSDrillMaterialAllowed(ret, false))
    //         ret = d.getRandomResourceType();
    //     return ret;
    // }
    //
    // internal static bool isFCSDrillMaterialAllowed(TechType tt, bool skipChance) {
    //     return tt == TechType.Nickel
    //         ? StoryGoalManager.main.completedGoals.Contains("Nickel") &&
    //           (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.4F)
    //         : tt == TechType.MercuryOre
    //             ? StoryGoalManager.main.completedGoals.Contains("Mercury") &&
    //               (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.2F)
    //             : tt == CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType
    //                 ? StoryGoalManager.main.completedGoals.Contains("Iridium") &&
    //                   (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.2F)
    //                 : tt == TechType.Kyanite
    //                     ? StoryGoalManager.main.completedGoals.Contains("Kyanite") &&
    //                       (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.25F)
    //                     : tt == TechType.Sulphur
    //                         ? StoryGoalManager.main.completedGoals.Contains("Sulfur") &&
    //                           (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.5F)
    //                         : tt != TechType.UraniniteCrystal ||
    //                           (StoryGoalManager.main.completedGoals.Contains("Uranium") &&
    //                            (skipChance || UnityEngine.Random.Range(0F, 1F) <= 0.5F));
    // }
    //
    // public static Action<int, int> cleanupFCSContainer(
    //     Action<int, int> notify,
    //     MonoBehaviour drill,
    //     Dictionary<TechType, int> dict
    // ) {
    //     if (dict.ContainsKey(TechType.None)) {
    //         SNUtil.writeToChat("Removed TechType.None from drill inventory");
    //         dict.Remove(TechType.None);
    //     }
    //
    //     var removed = 0;
    //     if (dict.ContainsKey(TechType.MercuryOre) && !StoryGoalManager.main.completedGoals.Contains("Mercury")) {
    //         removed += dict[TechType.MercuryOre];
    //         dict.Remove(TechType.MercuryOre);
    //     }
    //
    //     if (dict.ContainsKey(TechType.Nickel) && !StoryGoalManager.main.completedGoals.Contains("Nickel")) {
    //         removed += dict[TechType.Nickel];
    //         dict.Remove(TechType.Nickel);
    //     }
    //
    //     if (dict.ContainsKey(TechType.Kyanite) && !StoryGoalManager.main.completedGoals.Contains("Kyanite")) {
    //         removed += dict[TechType.Kyanite];
    //         dict.Remove(TechType.Kyanite);
    //     }
    //
    //     if (dict.ContainsKey(TechType.Sulphur) && !StoryGoalManager.main.completedGoals.Contains("Sulfur")) {
    //         removed += dict[TechType.Sulphur];
    //         dict.Remove(TechType.Sulphur);
    //     }
    //
    //     if (dict.ContainsKey(TechType.UraniniteCrystal) && !StoryGoalManager.main.completedGoals.Contains("Uranium")) {
    //         removed += dict[TechType.UraniniteCrystal];
    //         dict.Remove(TechType.UraniniteCrystal);
    //     }
    //
    //     if (dict.ContainsKey(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType) &&
    //         !StoryGoalManager.main.completedGoals.Contains("Iridium")) {
    //         removed += dict[CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType];
    //         dict.Remove(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType);
    //     }
    //
    //     if (removed > 0) {
    //         var d = DrillDepletionSystem.Instance.GetMotherlode(drill);
    //         SNUtil.writeToChat("Removing " + removed + " progression-gated resources from drill yield @ " + d);
    //         for (var i = 0; i < removed; i++) {
    //             var tt = d == null
    //                 ? UnityEngine.Random.Range(0F, 1F) <= 0.3 ? TechType.Copper : TechType.Titanium
    //                 : getRandomValidMotherlodeDrillYield(d);
    //             dict[tt] = dict.ContainsKey(tt) ? dict[tt] + 1 : 1;
    //         }
    //     }
    //
    //     return notify;
    // }
    //
    // public static float getFCSBioGenPowerFactor(float val, MonoBehaviour power, TechType item) {
    //     if (item == FCSIntegrationSystem.instance.getBiofuel())
    //         val *= 4;
    //     return val;
    // }

    public static void ControlPlayerInput(DIHooks.PlayerInput pi) {
        Drunk.manageDrunkenness(pi);
    }

    // TODO: FCS Compat
    // public static void onFCSPurchasedTech(TechType tt) {
    //     FCSIntegrationSystem.instance.onPlayerBuy(tt);
    // }
    //
    // public static bool isFCSItemBuyable(TechType tt) {
    //     //SNUtil.writeToChat("checking if "+tt.AsString()+" is buyable: unlocked="+CrafterLogic.IsCraftRecipeUnlocked(tt));
    //     return tt != TechType.None && !KnownTech.Contains(tt) &&
    //            tt != FCSIntegrationSystem.instance.getTeleportCard() &&
    //            tt != FCSIntegrationSystem.instance.getVehiclePad();
    // }
    //
    // public static int filterFCSCartAdd(int origLimit, System.Collections.IList cart, TechType adding) {
    //     //cart is a List<CartItem>, each of which has a TechType property which might == adding
    //     if (cart.Count <= 0 ||
    //         !FCSIntegrationSystem.instance
    //             .isUnlockingTypePurchase(
    //                 adding
    //             )) //always allow first item or as many non-unlocking purchases as you want
    //         return origLimit;
    //     var pi = cart[0].GetType().GetProperty(
    //         "TechType",
    //         BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
    //     );
    //     foreach (var obj in cart) {
    //         var tt = (TechType)pi.GetValue(obj);
    //         if (tt == adding)
    //             return -1;
    //     }
    //
    //     return origLimit;
    // }
    //
    // public static bool isTeleporterFunctional(bool orig, MonoBehaviour teleporter) {
    //     //SNUtil.writeToChat("Testing teleporter "+teleporter+" @ "+teleporter.transform.position);
    //     return orig && FCSIntegrationSystem.instance.checkTeleporterFunction(teleporter);
    // }
    //
    // public static float getCurrentGeneratorPower(float orig, MonoBehaviour generator) {
    //     var sp = DayNightCycle.main.dayNightSpeed * 2;
    //     return sp * 1.2F * FCSIntegrationSystem.instance.getCurrentGeneratorPowerFactor(generator);
    // }

    public static void OnMeteorImpact(GameObject meteor, Pickupable drop) {
        if (!PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.MeteorPrompt).key)) {
            StoryGoal.Execute(C2CProgression.MeteorGoal, Story.GoalType.Story);
        }
    }

    public static void BuildDisplayMonitorButton(MonoBehaviour screen, uGUI_ItemIcon icon) {
        icon.transform.localScale = new Vector3(0.5F, 0.45F, 1);
        var grid = screen.gameObject.getChildObject("Canvas/Screens/MainScreen/ActualScreen/MainGrid");
        var grp = grid.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        grp.cellSize = new Vector2(100, 90);
    }

    public static bool IsStorageVisibleToDisplayMonitor(bool skip, StorageContainer sc) {
        //SNUtil.writeToChat("checking SC="+sc+": "+skip);
        skip |= sc && sc.gameObject.FindAncestor<MapRoomFunctionality>();
        skip |= sc && sc.GetComponent<BioprocessorLogic>();
        skip |= sc && sc.GetComponent<Planter>();
        return skip;
    }

    public static void MergeDeathrunRecipeChange(TechType tt, RecipeData td) {
        var real = RecipeUtil.getRecipe(tt);
        if (real == null) {
            SNUtil.Log("Discarding deathrun " + tt + " recipe, as there is no vanilla recipe");
            return;
        }

        SNUtil.Log(
            "Integrating deathrun recipe change: " + tt + " = " + RecipeUtil.toString(td) + " into " +
            RecipeUtil.toString(real)
        );
        var cost = RecipeUtil.getIngredientsDict(real);
        foreach (var i in td.Ingredients) {
            if (cost.ContainsKey(i.techType)) {
                cost[i.techType] = Math.Max(cost[i.techType], i.amount);
            }
        }

        RecipeUtil.modifyIngredients(
            tt,
            i => {
                i._amount = cost[i.techType];
                return false;
            }
        );
    }

    public static void MergeDeathrunFragmentScanCount(TechType tt, int amt) {
        PDAHandler.EditFragmentsToScan(tt, Math.Max(amt, Reefbalance.ReefbalanceMod.GetScanCountOverride(tt)));
    }

    public static bool AllowSaving(bool orig) {
        if (!orig)
            return false;
        if (GameModeUtils.currentEffectiveMode == GameModeOption.Creative)
            return true;
        var ep = Player.main;
        var s = ep.GetComponent<Survival>();
        if (GameModeUtils.RequiresSurvival() && (s.water < 10 || s.food < 10))
            return false;
        if (VoidSpikesBiome.instance.getDistanceToBiome(ep.transform.position) < 400)
            return false;
        if (WorldUtil.isInRocket())
            return true;
        if (ep.currentEscapePod)
            return true;
        if (ep.radiationAmount > 0 && !(Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0 &&
                                        Inventory.main.equipment.GetCount(TechType.RadiationGloves) > 0 &&
                                        Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0))
            return false;
        if (DayNightCycle.main && DayNightCycle.main.timePassedAsFloat - _lastO2PipeTime <= 0.5)
            return true;
        if (ep.IsSwimming() && ep.transform.position.y < 0)
            return false;
        if (ep.currentWaterPark)
            return false;
        if (ep.currentSub && ep.currentSub.powerRelay && ep.currentSub.powerRelay.GetPower() > 0)
            return !ep.currentSub.isFlooded;
        if (ep.precursorOutOfWater)
            return true;
        if (BiomeBase.GetBiome(ep.transform.position) == VanillaBiomes.Alz ||
            WaterTemperatureSimulation.main.GetTemperature(ep.transform.position) > 150)
            return false;
        if (!SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
            var v = ep.GetVehicle();
            if (v && v.IsPowered())
                return true;
        }

        return false;
    }

    public static void OnWaterFilterSpawn(DIHooks.WaterFilterSpawn sp) {
        var id = TechType.None;
        var refpt = sp.Filter.transform.position; //basically right above the brine
        var bb = BiomeBase.GetBiome(refpt);
        if (bb == VanillaBiomes.Cove && sp.Item.GetTechType() == TechType.Salt) {
            var inBrine = false;
            var trig = Physics.queriesHitTriggers;
            Physics.queriesHitTriggers = true;
            foreach (var pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(refpt, 4)) {
                if (SNUtil.Match(pi, "32e0b9a0-236b-4e03-81cf-921a92ef735d")) {
                    inBrine = true;
                    break;
                }
            }

            Physics.queriesHitTriggers = trig;
            if (inBrine) {
                id = SeaToSeaMod.Geogel.TechType;
            }
        } else if (bb == VanillaBiomes.Lostriver && sp.Item.GetTechType() == TechType.Salt) {
            var inBrine = false;
            foreach (var hit in Physics.SphereCastAll(refpt, 4, Vector3.up, 0.1F, 1, QueryTriggerInteraction.Collide)) {
                if (hit.transform && hit.transform.GetComponent<AcidicBrineDamageTrigger>()) {
                    inBrine = true;
                    break;
                }
            }

            if (inBrine) {
                id = CraftingItems.getItem(CraftingItems.Items.Chlorine).TechType;
            }
        } else if (bb == UnderwaterIslandsFloorBiome.instance && sp.Item.GetTechType() == TechType.Salt) {
            if (refpt.y < -500)
                id = GeyserMaterialSpawner.GetRandomMineral(UnderwaterIslandsFloorBiome.instance);
            else
                id = CustomMaterials.getItem(CustomMaterials.Materials.OXYGENITE).TechType;
        } else if (bb == VanillaBiomes.Ilz && sp.Item.GetTechType() == TechType.Salt) {
            id = CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).TechType;
        }

        if (id != TechType.None) {
            var sz = TechData.GetItemSize(id);
            if (sp.Filter.storageContainer.container.HasRoomFor(sz.x, sz.y)) {
                var it = ObjectUtil.createWorldObject(id).GetComponent<Pickupable>();
                it.Pickup(false);
                var ii = new InventoryItem(it);
                sp.Filter.storageContainer.container.UnsafeAdd(ii);
            }
        }
    }

    public static void TickSwimCharge(UpdateSwimCharge ch) {
        var active = Inventory.main.equipment.GetCount(TechType.SwimChargeFins) > 0;
        var relay = active && Inventory.main.equipment.GetCount(C2CItems.chargeFinRelay.TechType) > 0;
        var charging = false;
        if (active && Player.main.IsUnderwater()) {
            var vel = Player.main.GetComponent<Rigidbody>().velocity.magnitude;
            if (vel > 2F) {
                var chargeAmount = (float)MathUtil.linterpolate(
                    vel,
                    10,
                    20,
                    0.005,
                    0.04,
                    true
                ); //0.005 in vanilla, give bonus if going > 10 (seaglide), azurite seaglide peaks about 17
                //SNUtil.writeToChat(vel+" > "+Mathf.Sqrt(vel)+" > "+chargeAmount);
                var held = Inventory.main.GetHeldTool();
                if (relay) {
                    foreach (var e in InventoryUtil.getAllHeldChargeables()) {
                        var tool = e.GetComponent<PlayerTool>();
                        if (tool) {
                            var add = tool == held
                                ? chargeAmount * 0.9F
                                : chargeAmount * 0.33F; //90% efficiency on held, 33% efficiency on non-helds
                            if (e && e.AddEnergy(add))
                                charging = true;
                        }
                    }
                } else if (held != null) {
                    var e = held.GetComponent<EnergyMixin>();
                    if (e && e.AddEnergy(chargeAmount))
                        charging = true;
                }
            }
        }

        if (charging)
            ch.swimChargeLoop.Play();
        else
            ch.swimChargeLoop.Stop();

        if (charging && relay)
            BatteryChargeIndicatorHandler.resyncChargeIndicators();
    }

    public static void OnStartInvUI(uGUI_InventoryTab gui) {
        RescueSystem.createRescuePDAButton();
    }

    /*
    class DelayedBatterySwapCallback : MonoBehaviour {

        internal TechType battery;
        internal float charge;
        internal EnergyMixin mixin;

        public DelayedBatterySwapCallback init(TechType tt, float f, EnergyMixin e) {
            battery = tt;
            charge = f;
            mixin = e;
            return this;
        }

        public void apply() {
            if (mixin)
                mixin.SetBattery(battery, charge);
            this.destroy(false);
        }

    }
    */
    public static void OnCollectFromVaseStrand(MushroomVaseStrand.MushroomVaseStrandTag plant, TechType item) {
        if (item == CraftingItems.getItem(CraftingItems.Items.Tungsten).TechType) {
            StoryGoal.Execute(C2CProgression.TungstenGoal, Story.GoalType.Story);
        }
    }

    private static void OnRocketStageComplete(Rocket r, int stage, bool anyComplete) {
        MoraleSystem.instance.shiftMorale(anyComplete ? 20 : 5);
    }

    private static void OnCuddlefishPlay(
        CuteFishHandTarget target,
        Player player,
        CuteFishHandTarget.CuteFishCinematic cinematic
    ) {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time - _lastCuddlefishPlay < 600) //10 min
            return;
        _lastCuddlefishPlay = time;
        MoraleSystem.instance.shiftMorale(25);
    }

    public static void OnSleep(Bed bed) {
        float f = 1;
        switch (bed.GetComponent<PrefabIdentifier>().ClassId) {
            case "c3994649-d0da-4f8c-bb77-1590f50838b9":
                f = 0.8F;
                break;
            case "cdb374fd-4f38-4bef-86a3-100cc87155b6":
                f = 1.25F;
                break;
        }

        MoraleSystem.instance.shiftMorale(
            f * (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 10 : 20)
        );
    }

    public static void OnEat(Survival s, GameObject go) {
        if (go) {
            var pp = go.GetComponent<Pickupable>();
            if (pp) {
                var tt = pp.GetTechType();
                if (tt is TechType.BigFilteredWater or TechType.DisinfectedWater or TechType.FilteredWater)
                    return;
                if (tt == C2CItems.treatment.TechType ||
                    tt == CraftingItems.getItem(CraftingItems.Items.WeakEnzyme42).TechType)
                    return;
                int morale;
                if (tt == TechType.Coffee) {
                    MoraleSystem.instance.onDrinkCoffee();
                    return;
                }

                if (tt is TechType.Snack1 or TechType.Snack2 or TechType.Snack3) {
                    morale = 20;
                    PlayerMovementSpeedModifier.add(0.9F, 60 * 10);
                } else if (tt == TechType.WaterFiltrationSuitWater) {
                    morale = -50;
                } else if (tt == TechType.Bladderfish) {
                    morale = -40;
                } else if (tt == TechType.Hoverfish || tt == TechType.CookedHoverfish ||
                           tt == TechType.CuredHoverfish ||
                           tt == Campfire.cookMap[TechType.Hoverfish].output.TechType) {
                    morale = -10;
                } else if (tt.isRawFish()) {
                    morale = -25;
                } else {
                    var li = ConsumableTracker.instance.getEvents();
                    var eatsSinceDifferent = 999999;
                    var back = 1;
                    for (var i = li.Count - 2; i >= 0; i--) {
                        //this event is already in the list so start an extra item back
                        var evt = li[i];
                        if (!evt.isEating)
                            continue;
                        if (tt is TechType.BigFilteredWater or TechType.DisinfectedWater or TechType.FilteredWater or TechType.WaterFiltrationSuitWater or TechType.Coffee)
                            continue;
                        //SNUtil.writeToChat("ate "+evt.itemType+" @ "+evt.eventTime);
                        if (MoraleSystem.instance.areFoodsDifferent(evt.itemType, tt)) {
                            eatsSinceDifferent = back;
                            break;
                        }

                        back++;
                    }

                    string msg;
                    switch (back) {
                        case 1: //different from last item -> boost
                            morale = 10;
                            msg = "Morale boost from dietary variety";
                            break;
                        case 2: //if same as last two items then no effect
                        case 3:
                            morale = 0;
                            msg = "Dietary variety recommended for optimum morale";
                            break;
                        case 4: //if have to go back five items then small penalty
                        case 5:
                            morale = -10;
                            msg = "Lack of dietary variety slightly harming morale";
                            break;
                        case 6: //if have to go back five items then moderate penalty
                        case 7:
                        case 8:
                            morale = -20;
                            msg = "Lack of dietary variety substantially harming morale";
                            break;
                        default: //eight or more and you are always eating the same thing, so big penalty
                            morale = -40;
                            msg = "Lack of dietary variety severely harming morale";
                            break;
                    }

                    SNUtil.WriteToChat(msg);
                }

                MoraleSystem.instance.shiftMorale(morale);
            }
        }
    }

    public static void AffectFoodRate(DIHooks.FoodRateCalculation calc) {
        if (GameModeUtils.currentGameMode != GameModeOption.Survival)
            return;
        var morale = MoraleSystem.instance.moralePercentage;
        if (morale < 40) {
            calc.Rate *= Mathf.Lerp(2.5F, 1, morale / 40F);
        } else if (morale > 80) {
            calc.Rate *= Mathf.Lerp(1, 0.5F, (morale - 80F) / 20F);
        }
    }

    public static float GetAmbientHealAmount(float orig) {
        if (GameModeUtils.currentGameMode != GameModeOption.Survival)
            return orig;
        var ret = orig;
        var morale = MoraleSystem.instance.moralePercentage;
        if (morale <= 20) {
            ret = 0;
        } else if (morale <= 50) {
            ret *= (morale - 20F) / 30F;
        } else if (morale >= 80) {
            ret = (float)MathUtil.linterpolate(morale, 80, 100, 1, 4, true);
        }

        return ret;
    }

    public static bool CanWarperAggroPlayer(WarperInspectPlayer warp, GameObject target) {
        if (target.isPlayer() && InBKelpBase && !WorldUtil.lineOfSight(target, warp.gameObject))
            return false;
        if (Vector3.Distance(target.transform.position, warp.transform.position) > warp.maxDistance) {
            return false;
        }

        if (!warp.warper.GetCanSeeObject(target)) {
            return false;
        }

        var component = target.GetComponent<InfectedMixin>();
        return !(component != null) || component.GetInfectedAmount() <= 0.33f;
    }

    public static void UnfoldKeyTerminal(PrecursorKeyTerminal pk) {
        if (pk.acceptKeyType == PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Blue &&
            !C2CProgression.Instance.IsPcfAccessible()) {
            PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.NeedPCFSecurityMessage).key);
            return;
        }

        if (!pk.slotted) {
            Utils.PlayFMODAsset(pk.openSound, pk.transform, 20f);
            pk.animator.SetBool("Open", true);
        }
    }

    public static bool CannotClickKeyTerminal(PrecursorKeyTerminal pk) {
        return pk.slotted || (pk.acceptKeyType == PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Blue &&
                              !C2CProgression.Instance.IsPcfAccessible());
    }
}