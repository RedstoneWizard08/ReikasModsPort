using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using Story;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class C2CProgression : IStoryGoalListener {
    public static readonly C2CProgression Instance = new();

    internal readonly Vector3 Pod12Location = new(1117, -268, 568);
    internal readonly Vector3 Pod3Location = new(-33, -23, 409);
    internal readonly Vector3 Pod6Location = new(363, -110, 309);
    internal readonly Vector3 DronePdaCaveEntrance = new(-80, -79, 262);
    internal readonly Vector3 Pod2Location = new(-489, -500, 1328);

    public int PcfSecurityNodes = 9999;

    private readonly SoundManager.SoundData _securityNodePdaLine;

    public static bool ForceAllowPipeTravel = false;

    private readonly Dictionary<string, StoryGoal> _locationGoals = new() {
        {
            "OZZY_FORK_DEEP_ROOM",
            StoryHandler.instance.createLocationGoal(new Vector3(-645.6F, -102.7F, -16.2F), 12, "ozzyforkdeeproom")
        }, {
            "UNDERISLANDS_BLOCKED_ROOM",
            StoryHandler.instance.createLocationGoal(
                new Vector3(-124.38F, -200.69F, 855F),
                5,
                "underislandsblockedroom"
            )
        }, {
            "FLOATING_ARCH",
            StoryHandler.instance.createLocationGoal(
                new Vector3(-662.55F, 5.50F, -1064.35F),
                25,
                "floatarch",
                vec => vec.y > 0 && vec.y < 22.5F
            )
        }, {
            "PLANT_ALCOVE",
            StoryHandler.instance.createLocationGoal(
                new Vector3(375, 22, 870),
                15,
                "islandalcove",
                vec => vec.y > 15 && vec.y < 30F
            )
        },
        { "MUSHTREE", StoryHandler.instance.createLocationGoal(new Vector3(-883.7F, -144, 591.4F), 25, "mushtree") }, {
            "MUSHTREE_ARCH",
            StoryHandler.instance.createLocationGoal(new Vector3(-777.5F, -229.8F, 404.8F), 12, "musharch")
        }, {
            "CRAG_ARCH", StoryHandler.instance.createLocationGoal(new Vector3(-90.2F, -287.4F, -1261.5F), 6, "cragarch")
        }, {
            "KOOSH_ARCH",
            StoryHandler.instance.createLocationGoal(new Vector3(1344.8F, -309.2F, 730.7F), 8, "koosharch")
        },
        { "LR_ARCH", StoryHandler.instance.createLocationGoal(new Vector3(-914.7F, -621.2F, 1078.4F), 6, "lrarch") }, {
            "SOUTH_GRASS_WRECK",
            StoryHandler.instance.createLocationGoal(new Vector3(-29.19F, -103.46F, -608.40F), 20, "southgrasswreck")
        }, {
            "EAST_GRASS_WRECK",
            StoryHandler.instance.createLocationGoal(new Vector3(318.79F, -90.34F, 441.63F), 30, "eastgrasswreck")
        },
        { "LR_LAB", StoryHandler.instance.createLocationGoal(new Vector3(-1119.8F, -683.1F, -688.2F), 8, "lrlab") },
        { "SEE_GUN", StoryHandler.instance.createLocationGoal(new Vector3(402.3F, 19.7F, 1118.9F), 160, "see_gun") },
        { "SEE_ATP", StoryHandler.instance.createLocationGoal(WorldUtil.lavaCastleCenter, 80, "see_atp") }, {
            "SPARSE_CACHE",
            StoryHandler.instance.createLocationGoal(new Vector3(-889.8F, -305.6F, -815.3F), 10, "sparse_cache")
        }, {
            "DUNES_CACHE",
            StoryHandler.instance.createLocationGoal(new Vector3(-1224.6F, -393.3F, 1078.9F), 18, "dunes_cache")
        }, {
            "NBKELP_CACHE",
            StoryHandler.instance.createLocationGoal(new Vector3(-624.0F, -558.7F, 1485.9F), 18, "nbkelp_cache")
        }, {
            "FLOATISLAND_DEGASI",
            StoryHandler.instance.createLocationGoal(WorldUtil.DEGASI_FLOATING_BASE, 50, "floatisland_degasi")
        },
        { "JELLY_DEGASI", StoryHandler.instance.createLocationGoal(WorldUtil.DEGASI_JELLY_BASE, 50, "jelly_degasi") },
        { "DGR_DEGASI", StoryHandler.instance.createLocationGoal(WorldUtil.DEGASI_DGR_BASE, 75, "dgr_degasi") },
        //{"LRNEST", StoryHandler.instance.createLocationGoal(C2CHooks.lrnest, 30, "LRNest")},
    };

    internal static readonly string MeteorGoal = "meteorhit";
    internal static readonly string TungstenGoal = "filtertung";
    internal static readonly string PipeTravelEnabled = "EnablePipeTravel";

    private static readonly HashSet<string> MountainPodVisibilityTriggers = [
        "mountainpodearly",
        "mountainpodlate",
        "mountaincave",
        "islandpda",
        "islandcave",
    ];

    internal static readonly string MountainPodEntryVisibilityGoal = "mountainPodEntriesVisible";

    private readonly Vector3[] _seacrownCaveEntrances = [
        new(279, -140, 288), //new Vector3(300, -120, 288)/**0.67F+pod6Location*0.33F*/,
        new(-621, -130, -190), //new Vector3(-672, -100, -176),
        //new Vector3(-502, -80, -102), //empty in vanilla, and right by pod 17
    ];

    internal readonly Vector3[] BkelpNestBumps = [
        new(-847.46F, -530.82F, 1273.73F),
        new(-863.82F, -532.87F, 1302.29F),
        new(-841.12F, -535.97F, 1304.40F),
    ];

    private float _lastDunesEntry = -1;

    private readonly HashSet<TechType> _gatedTechnologies = [];
    private readonly HashSet<string> _requiredProgression = [];

    private readonly List<TechType> _pipeRoomTechs = [];

    private C2CProgression() {
        StoryHandler.instance.addListener(this);

        StoryHandler.instance.registerTrigger(
            new StoryTrigger(StoryGoals.AURORA_FIX),
            new DelayedProgressionEffect(
                VoidSpikesBiome.instance.fireRadio,
                VoidSpikesBiome.instance.isRadioFired,
                0.00003F
            )
        );
        StoryHandler.instance.registerTrigger(
            new TechTrigger(TechType.PrecursorKey_Orange),
            new DelayedStoryEffect(SeaToSeaMod.CrashMesaRadio, 0.00004F)
        );
        StoryHandler.instance.registerTrigger(
            new ProgressionTrigger(ep => ep.GetVehicle() is SeaMoth),
            new DelayedProgressionEffect(
                SeaToSeaMod.TreaderSignal.fireRadio,
                SeaToSeaMod.TreaderSignal.isRadioFired,
                0.000015F
            )
        );

        var pod12Radio = new StoryGoal(StoryGoals.POD12RADIO, Story.GoalType.Radio, 0);
        var ds = new DelayedStoryEffect(pod12Radio, 0.00008F);
        StoryHandler.instance.registerTrigger(new StoryTrigger(StoryGoals.SUNBEAM_DESTROY_START), ds);
        StoryHandler.instance.registerTrigger(new TechTrigger(TechType.BaseNuclearReactor), ds);
        StoryHandler.instance.registerTrigger(new TechTrigger(TechType.HighCapacityTank), ds);
        StoryHandler.instance.registerTrigger(new TechTrigger(TechType.PrecursorKey_Purple), ds);
        StoryHandler.instance.registerTrigger(new TechTrigger(TechType.BaseUpgradeConsole), ds);
        StoryHandler.instance.registerTrigger(
            new TechTrigger(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType),
            ds
        );
        StoryHandler.instance.registerTrigger(new EncylopediaTrigger("SnakeMushroom"), ds);

        AddPdaPrompt(
            PDAMessages.Messages.KooshCavePrompt,
            ep => Vector3.Distance(Pod12Location, ep.transform.position) <= 75
        );
        AddPdaPrompt(PDAMessages.Messages.RedGrassCavePrompt, IsNearSeacrownCave);
        AddPdaPrompt(PDAMessages.Messages.UnderwaterIslandsPrompt, IsInUnderwaterIslands);
        AddPdaPrompt(PDAMessages.Messages.KelpCavePrompt, ep => IsNearKelpCave(ep) && !IsJustStarting(ep));
        AddPdaPrompt(PDAMessages.Messages.KelpCavePromptLate, HasMissedKelpCavePromptLate);
        AddPdaPrompt(
            PDAMessages.Messages.BloodKelpNestPrompt,
            ep => Vector3.Distance(Pod2Location, ep.transform.position) <= 100
        );
        AddPdaPrompt(
            PDAMessages.Messages.TrailerBasePrompt,
            ep => VanillaBiomes.Crash.IsInBiome(ep.transform.position) &&
                  WorldUtil.isInsideAurora2D(ep.transform.position, 200)
        );
        /*
        PDAPrompt kelpLate = addPDAPrompt(PDAMessages.Messages.KelpCavePromptLate, new TechTrigger(TechType.HighCapacityTank), 0.0001F);
        addPDAPrompt(kelpLate, new TechTrigger(TechType.StasisRifle));
        addPDAPrompt(kelpLate, new TechTrigger(TechType.BaseMoonpool));
        */
        StoryHandler.instance.registerTrigger(
            new PdaPromptCondition(new ProgressionTrigger(DoDunesCheck)),
            new DunesPrompt()
        );
        StoryHandler.instance.registerTrigger(new PdaPromptCondition(new StoryTrigger(MeteorGoal)), new MeteorPrompt());

        AddPdaPrompt(PDAMessages.Messages.FollowRadioPrompt, HasMissedRadioSignals);

        StoryHandler.instance.registerTrigger(
            new ProgressionTrigger(CanUnlockEnzy42Recipe),
            new TechUnlockEffect(
                Bioprocessor.getByOutput(CraftingItems.getItem(CraftingItems.Items.WeakEnzyme42).TechType)
                    .outputDelegate.TechType,
                1,
                6
            )
        );
        StoryHandler.instance.registerTrigger(
            new ProgressionTrigger(CanUnlockEnzy42Recipe),
            new TechUnlockEffect(CraftingItems.getItem(CraftingItems.Items.WeakEnzyme42).TechType, 1, 6)
        );

        StoryHandler.instance.registerTrigger(
            new ProgressionTrigger(CanSunbeamCountdownBegin),
            new DelayedStoryEffect(SeaToSeaMod.SunbeamCountdownTrigger, 0.001F, 90)
        );

        foreach (var g in _locationGoals.Values) {
            StoryHandler.instance.registerTickedGoal(g);
        }

        _securityNodePdaLine = SoundManager.registerPDASound(
            SeaToSeaMod.ModDLL,
            "pda_pcf_node",
            "Sounds/pdaprompt/pcfnode.ogg"
        );

        //StoryHandler.instance.registerChainedRedirect("PrecusorPrisonAquariumIncubatorActive", null); //deregister

        _pipeRoomTechs.Add(TechType.PrecursorPipeRoomIncomingPipe);
        _pipeRoomTechs.Add(TechType.PrecursorPipeRoomOutgoingPipe);
        //pipeRoomTechs.Add(SeaToSeaMod.prisonPipeRoomTank);

        _gatedTechnologies.Add(TechType.Kyanite);
        _gatedTechnologies.Add(TechType.Sulphur);
        _gatedTechnologies.Add(TechType.Nickel);
        _gatedTechnologies.Add(TechType.MercuryOre);
        _gatedTechnologies.Add(TechType.JellyPlant);
        _gatedTechnologies.Add(TechType.BloodOil);
        _gatedTechnologies.Add(TechType.AramidFibers);
        _gatedTechnologies.Add(TechType.WhiteMushroom);
        _gatedTechnologies.Add(TechType.SeaCrown);
        _gatedTechnologies.Add(TechType.Aerogel);
        _gatedTechnologies.Add(TechType.Seamoth);
        _gatedTechnologies.Add(TechType.Cyclops);
        _gatedTechnologies.Add(TechType.Exosuit);
        _gatedTechnologies.Add(TechType.Benzene);
        _gatedTechnologies.Add(TechType.HydrochloricAcid);
        _gatedTechnologies.Add(TechType.Polyaniline);
        _gatedTechnologies.Add(TechType.ExosuitDrillArmModule);
        _gatedTechnologies.Add(TechType.ExoHullModule1);
        _gatedTechnologies.Add(TechType.ExoHullModule2);
        _gatedTechnologies.Add(TechType.VehicleHullModule2);
        _gatedTechnologies.Add(TechType.VehicleHullModule3);
        _gatedTechnologies.Add(TechType.SeamothElectricalDefense);
        _gatedTechnologies.Add(TechType.CyclopsHullModule2);
        _gatedTechnologies.Add(TechType.CyclopsHullModule3);
        _gatedTechnologies.Add(TechType.CyclopsThermalReactorModule);
        _gatedTechnologies.Add(TechType.CyclopsFireSuppressionModule);
        _gatedTechnologies.Add(TechType.CyclopsShieldModule);
        _gatedTechnologies.Add(TechType.StasisRifle);
        _gatedTechnologies.Add(TechType.LaserCutter);
        _gatedTechnologies.Add(TechType.ReinforcedDiveSuit);
        _gatedTechnologies.Add(TechType.ReinforcedGloves);
        _gatedTechnologies.Add(TechType.PrecursorIonCrystal);
        _gatedTechnologies.Add(TechType.PrecursorIonBattery);
        _gatedTechnologies.Add(TechType.PrecursorIonPowerCell);
        _gatedTechnologies.Add(TechType.PrecursorKey_Blue);
        _gatedTechnologies.Add(TechType.PrecursorKey_Red);
        _gatedTechnologies.Add(TechType.PrecursorKey_White);
        _gatedTechnologies.Add(TechType.PrecursorKey_Orange);
        _gatedTechnologies.Add(TechType.PrecursorKey_Purple);
        _gatedTechnologies.Add(TechType.HeatBlade);
        _gatedTechnologies.Add(TechType.ReactorRod);

        //requiredProgression.Add();
    }

    public IEnumerable<TechType> GetGatedTechnologies() {
        return new ReadOnlyCollection<TechType>(_gatedTechnologies.ToList());
    }

    public StoryGoal GetLocationGoal(string key) {
        return !_locationGoals.ContainsKey(key)
            ? throw new Exception("No such location goal '" + key + "'")
            : _locationGoals[key];
    }

    private bool CanSunbeamCountdownBegin(Player ep) {
        return StoryGoalManager.main.completedGoals.Contains(StoryGoals.getRadioPlayGoal(StoryGoals.SUNBEAM_FILLER)) &&
               ep.GetVehicle() is SeaMoth;
    }

    private bool CanUnlockEnzy42Recipe(Player ep) {
        return PDAEncyclopedia.entries.ContainsKey("HeroPeeper") &&
               KnownTech.knownTech.Contains(C2CItems.processor.TechType);
    }

    private bool HasMissedRadioSignals(Player ep) {
        var late = KnownTech.knownTech.Contains(TechType.StasisRifle) ||
                   KnownTech.knownTech.Contains(TechType.BaseMoonpool) ||
                   KnownTech.knownTech.Contains(TechType.HighCapacityTank);
        var all =
            PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.RedGrassCavePrompt).key) &&
            PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KelpCavePrompt).key) &&
            PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KooshCavePrompt).key);
        return late && !all;
    }

    private bool IsNearKelpCave(Player ep) {
        return
            (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KelpCavePromptLate).key) &&
             Vector3.Distance(ep.transform.position, Pod3Location) <= 80) ||
            MathUtil.isPointInCylinder(DronePdaCaveEntrance.SetY(-40), ep.transform.position, 60, 40) ||
            (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.FollowRadioPrompt).key) &&
             Vector3.Distance(Pod3Location, ep.transform.position) <= 60);
    }

    private bool IsJustStarting(Player ep) {
        if (Inventory.main.equipment.GetTechTypeInSlot("Head") != TechType.None ||
            KnownTech.knownTech.Contains(TechType.Seamoth) || KnownTech.knownTech.Contains(TechType.BaseMapRoom) ||
            KnownTech.knownTech.Contains(TechType.BaseRoom))
            return false;
        if (StoryGoalManager.main.completedGoals.Contains(StoryGoals.getRadioPlayGoal(StoryGoals.POD3RADIO)))
            return false;
        //if (StoryGoalManager.main.completedGoals.Contains("Goal_Builder") || StoryGoalManager.main.completedGoals.Contains("Goal_Seaglide")) //craft build tool or seaglide
        //	return false;
        return true;
    }

    private bool HasMissedKelpCavePromptLate(Player ep) {
        if (!StoryGoalManager.main.completedGoals.Contains(StoryGoals.getRadioPlayGoal(StoryGoals.POD3RADIO)))
            return false;
        var late1 = StoryGoalManager.main.completedGoals.Contains("Goal_LocationAuroraDriveEntry") ||
                    StoryGoalManager.main.completedGoals.Contains(StoryGoals.SUNBEAM_DESTROY_START);
        var late2 = KnownTech.knownTech.Contains(TechType.Workbench) ||
                    KnownTech.knownTech.Contains(TechType.StasisRifle) ||
                    KnownTech.knownTech.Contains(TechType.BaseMoonpool) ||
                    KnownTech.knownTech.Contains(TechType.HighCapacityTank);
        return late1 && late2 && ep.GetBiomeString().ToLowerInvariant().Contains("safe") &&
               !PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.KelpCavePrompt).key);
    }

    private bool IsInUnderwaterIslands(Player ep) {
        return ep.transform.position.y <= -150 &&
               (ep.transform.position - new Vector3(-112.3F, ep.transform.position.y, 990.3F)).magnitude <= 180 &&
               ep.GetBiomeString().ToLowerInvariant().Contains("underwaterislands");
    }

    private bool IsNearSeacrownCave(Player ep) {
        var pos = ep.transform.position;
        return _seacrownCaveEntrances.Any(vec => pos.y <= vec.y && MathUtil.isPointInCylinder(vec, pos, 30, 10));
    }

    private PdaPrompt AddPdaPrompt(PDAMessages.Messages m, Predicate<Player> condition, float ch = 0.01F) {
        return AddPdaPrompt(m, new ProgressionTrigger(condition), ch);
    }

    private PdaPrompt AddPdaPrompt(PDAMessages.Messages m, ProgressionTrigger pt, float ch = 0.01F) {
        var p = new PdaPrompt(m, ch);
        AddPdaPrompt(p, pt);
        return p;
    }

    private void AddPdaPrompt(PdaPrompt m, ProgressionTrigger pt) {
        StoryHandler.instance.registerTrigger(new PdaPromptCondition(pt), m);
    }

    private bool DoDunesCheck(Player ep) {
        var biome = ep.GetBiomeString();
        if (biome != null && biome.ToLowerInvariant().Contains("dunes")) {
            var time = DayNightCycle.main.timePassedAsFloat;
            if (_lastDunesEntry < 0)
                _lastDunesEntry = time;
            //SNUtil.writeToChat(lastDunesEntry+" > "+(time-lastDunesEntry));
            if (time - _lastDunesEntry >= 90) { //in dunes for at least 90s
                return true;
            }
        } else {
            _lastDunesEntry = -1;
        }

        return false;
    }

    public bool IsRequiredProgressionComplete() {
        foreach (var s in _requiredProgression) {
            if (!StoryGoalManager.main.IsGoalComplete(s))
                return false;
        }

        return true;
    }

    public void OnScanComplete(PDAScanner.EntryData data) {
        if (_pipeRoomTechs.Contains(data.key)) {
            foreach (var tt in _pipeRoomTechs) {
                if (!PDAScanner.complete.Contains(tt))
                    return;
            }
            //SeaToSeaMod.enviroSimulation.unlock();
        }
    }

    public void OnWorldLoaded() {
        foreach (var s in MountainPodVisibilityTriggers) {
            if (StoryGoalManager.main.completedGoals.Contains(s)) {
                StoryGoal.Execute(MountainPodEntryVisibilityGoal, Story.GoalType.Story);
            }
        }
    }

    public void NotifyGoalComplete(string key) {
        if (key.StartsWith("OnPlay", StringComparison.InvariantCultureIgnoreCase)) {
            if (key.Contains(SeaToSeaMod.TreaderSignal.storyGate)) {
                SeaToSeaMod.TreaderSignal.activate(20);
            } else if (key.Contains(VoidSpikesBiome.instance.getSignalKey())) {
                VoidSpikesBiome.instance.activateSignal();
            } else if (key.Contains(SeaToSeaMod.CrashMesaRadio.key)) {
                Player.main.gameObject.EnsureComponent<DelayedPromptsCallback>().Invoke("TriggerCrashMesa", 25);
            }
        } else if (key == PDAManager.getPage("voidpod").id) { //id is pda page story key
            SeaToSeaMod.VoidSpikeDirectionHint.activate(4);
        } else if (key == SeaToSeaMod.AuroraTerminal.key) {
            PDAManager.getPage("auroraringterminalinfo").unlock(false);
        } else if (MountainPodVisibilityTriggers.Contains(key)) {
            StoryGoal.Execute(MountainPodEntryVisibilityGoal, Story.GoalType.Story);
        } else {
            switch (key) {
                case StoryGoals.SUNBEAM_DESTROY_START:
                    Player.main.gameObject.EnsureComponent<AvoliteSpawner.TriggerCallback>().Invoke("Trigger", 39);
                    break;
                case StoryGoals.MAIDA_SEAMOTH_LOG:
                    Player.main.gameObject.EnsureComponent<DelayedPromptsCallback>()
                        .Invoke("TriggerJellySeamothDepth", 5);
                    break;
                case "drfwarperheat":
                    KnownTech.Add(C2CItems.cyclopsHeat.TechType);
                    break;
                case "stepcaveterminal":
                    KnownTech.Add(CraftingItems.getItem(CraftingItems.Items.MicroFilter).TechType);
                    //SNUtil.triggerTechPopup(CraftingItems.getItem(CraftingItems.Items.MicroFilter).TechType);
                    break;
                case "prisonpipeterminal": //removed, scanning the four tanks is now the trigger
                    //KnownTech.Add(CraftingItems.getItem(CraftingItems.Items.FluidPump).TechType);
                    break;
                case "prisoneggterminal":
                    //KnownTech.Add(C2CItems.incubatorInjector.TechType);
                    break;
                case "prisonhighterminal": //removed
                    //KnownTech.Add(TechType.HatchingEnzymes);						
                    break;
            }
        }
    }

    internal bool CanTriggerPdaPrompt(Player ep) {
        return SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.PROMPTS) &&
               (ep.IsSwimming() || Mathf.Abs(ep.transform.position.y) <= 1 || ep.GetVehicle() != null) &&
               ep.currentSub == null && !ep.currentEscapePod && !ep.precursorOutOfWater &&
               !WorldUtil.isPrecursorBiome(ep.transform.position);
    }

    public bool IsTechGated(TechType tt) {
        if (_gatedTechnologies.Contains(tt))
            return true;
        var s = ItemRegistry.instance.getItem(tt);
        return s is DIPrefab prefab && prefab.getOwnerMod() == SeaToSeaMod.ModDLL;
    }

    public static void OnSeamothDepthChit() {
        if (StoryGoalManager.main.IsGoalComplete("seamothdepthchit2")) {
            StoryGoal.Execute("seamothdepthchit3", Story.GoalType.Story);
            SNUtil.SetBlueprintUnlockProgress(SeaToSeaMod.SeamothDepthUnlockTracker, 3);
            KnownTech.Add(TechType.VehicleHullModule1);
            SNUtil.TriggerTechPopup(TechType.VehicleHullModule1);
        } else if (StoryGoalManager.main.IsGoalComplete("seamothdepthchit1")) {
            StoryGoal.Execute("seamothdepthchit2", Story.GoalType.Story);
            SNUtil.WriteToChat("2/3 Data Entries Recovered");
            SNUtil.SetBlueprintUnlockProgress(SeaToSeaMod.SeamothDepthUnlockTracker, 2);
        } else {
            StoryGoal.Execute("seamothdepthchit1", Story.GoalType.Story);
            SNUtil.WriteToChat("1/3 Data Entries Recovered");
            SNUtil.SetBlueprintUnlockProgress(SeaToSeaMod.SeamothDepthUnlockTracker, 1);
        }
    }

    private string GetPcfSecurityGate(int i) {
        return "PCFNodeClear_" + i;
    }

    public bool IsPcfAccessible() {
        return StoryGoalManager.main.IsGoalComplete(GetPcfSecurityGate(PcfSecurityNodes));
    }

    public void StepPcfSecurity() {
        for (var i = PcfSecurityNodes; i > 0; i--) {
            var key = GetPcfSecurityGate(i);
            var req = i == 1 ? null : GetPcfSecurityGate(i - 1);
            if (req != null && !StoryGoalManager.main.IsGoalComplete(req)) continue;
            StoryGoal.Execute(key, Story.GoalType.Story);
            Subtitles.Add(i + " of " + PcfSecurityNodes + " security nodes disabled.");
            SoundManager.playSoundAt(_securityNodePdaLine, Player.main.transform.position, false, -1);
            break;
        }
    }

    public static bool IsSeamothDepth1UnlockedLegitimately() {
        return
            StoryGoalManager.main.IsGoalComplete(
                "seamothdepthchit2"
            ); //2 for now because 3 did not exist at time of creation
    }

    public static void SpawnPoiMarker(string id, Vector3 at) {
        if (StoryGoalManager.main.IsGoalComplete(id))
            return;
        DIMod.areaOfInterestMarker.spawnGenericSignalHolder(at);
        StoryGoal.Execute(id, Story.GoalType.Story);
    }

    internal bool IsPipeTravelEnabled(out bool isInvisible) {
        isInvisible = false;
        if (ForceAllowPipeTravel)
            return true;
        isInvisible = !StoryGoalManager.main.IsGoalComplete(PipeTravelEnabled);
        return !isInvisible && StoryGoalManager.main.IsGoalComplete("Precursor_Prison_Aquarium_EmperorLog1") &&
               PDAScanner.complete.Contains(TechType.PrecursorPrisonAquariumPipe) &&
               PDAScanner.complete.Contains(TechType.PrecursorPipeRoomIncomingPipe) &&
               PDAScanner.complete.Contains(TechType.PrecursorPipeRoomOutgoingPipe) &&
               PDAScanner.complete.Contains(TechType.PrecursorSurfacePipe);
    }
}

internal class PdaPromptCondition : ProgressionTrigger {
    private readonly ProgressionTrigger _baseline;

    public PdaPromptCondition(ProgressionTrigger p) : base(ep =>
        C2CProgression.Instance.CanTriggerPdaPrompt(ep) && p.isReady(ep)
    ) {
        _baseline = p;
    }

    public override string ToString() {
        return "Free-swimming " + _baseline;
    }
}

internal class PdaPrompt : DelayedProgressionEffect {
    private readonly PDAMessages.Messages _prompt;

    public PdaPrompt(PDAMessages.Messages m, float f) : base(
        () => PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(m).key),
        () => PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(m).key),
        f
    ) {
        _prompt = m;
    }

    public override string ToString() {
        return "PDA Prompt " + _prompt;
    }
}

internal class DunesPrompt : DelayedProgressionEffect {
    private static readonly PDAManager.PDAPage Page = PDAManager.getPage("dunearchhint");

    public DunesPrompt() : base(
        () => {
            PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.DuneArchPrompt).key);
            Page.unlock(false);
        },
        () => PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.DuneArchPrompt).key),
        0.006F
    ) {
    }

    public override string ToString() {
        return "Dunes Prompt";
    }
}

internal class MeteorPrompt : DelayedProgressionEffect {
    public MeteorPrompt() : base(
        () => { PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.MeteorPrompt).key); },
        () => PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.MeteorPrompt).key),
        100F,
        2
    ) {
    }

    public override string ToString() {
        return "Meteor Prompt";
    }
}

internal class DelayedPromptsCallback : MonoBehaviour {
    private void TriggerCrashMesa() {
        SoundManager.playSound("event:/tools/scanner/new_encyclopediea"); //triple-click
        SoundManager.playSound("event:/player/story/RadioShallows22NoSignalAlt"); //"signal coordinates corrupted"
        PDAManager.getPage("crashmesahint").unlock(false);
    }

    private void TriggerSanctuary() {
        if (PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.SanctuaryPrompt).key))
            return;
        PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.SanctuaryPrompt).key);
        SeaToSeaMod.SanctuaryDirectionHint.activate(12);
    }

    private void TriggerJellySeamothDepth() {
        if (Player.main.GetPDA().isOpen) {
            Invoke(nameof(TriggerJellySeamothDepth), 2);
            return;
        }

        if (!PDAMessagePrompts.instance.isTriggered(
                PDAMessages.getAttr(PDAMessages.Messages.JellySeamothDepthPrompt).key
            )) {
            PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.JellySeamothDepthPrompt).key);
        }
    }
}