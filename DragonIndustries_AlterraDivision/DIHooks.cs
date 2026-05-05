using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Json.Converters;
using Nautilus.Utility;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Oculus.Newtonsoft.Json;
//using Oculus.Newtonsoft.Json.Linq;
using Story;
using TMPro;
using UnityEngine;
using UWE;
using Math = System.Math;

namespace ReikaKalseki.DIAlterra;

// ReSharper disable once InconsistentNaming
public static class DIHooks {
    internal static readonly float NearLavaRadius = 16;

    private static float _worldLoadTime = -1;

    public static event Action<DayNightCycle> OnDayNightTickEvent;
    public static event Action OnWorldLoadedEvent;
    public static event Action<Player> OnPlayerTickEvent;
    public static event Action<SeaMoth> OnSeamothTickEvent;
    public static event Action<Exosuit> OnPrawnTickEvent;
    public static event Action<SubRoot> OnCyclopsTickEvent;
    public static event Action<BaseRoot> OnBaseTickEvent;
    public static event Action<DamageToDeal> OnDamageEvent;
    public static event Action<ItemPickup> OnItemPickedUpEvent;
    public static event Action<CellManager, LargeWorldEntity> OnEntityRegisterEvent;
    public static event Action<SkyApplier> OnSkyApplierSpawnEvent;
    public static event Action<Constructable, bool> OnConstructedEvent;
    public static event Action<BaseRoot> OnBaseLoadedEvent;
    public static event Action<StorageContainer> InventoryOpenedEvent;
    public static event Action<StorageContainer> InventoryClosedEvent;
    public static event Action<BiomeCheck> GetBiomeEvent;
    public static event Action<WaterTemperatureCalculation> GetTemperatureEvent;
    public static event Action<GameObject> OnKnifedEvent;
    public static event Action<KnifeAttempt> KnifeAttemptEvent;
    public static event Action<GravTrapGrabAttempt> GravTrapAttemptEvent;
    public static event Action<SeaMoth, int, TechType, bool> OnSeamothModulesChangedEvent;
    public static event Action<SubRoot> OnCyclopsModulesChangedEvent;
    public static event Action<Exosuit, int, TechType, bool> OnPrawnModulesChangedEvent;
    public static event Action<SeaMoth, TechType, int> OnSeamothModuleUsedEvent;
    public static event Action<SNCameraRoot> OnSonarUsedEvent;
    public static event Action<SeaMoth> OnSeamothSonarUsedEvent;
    public static event Action<SubRoot> OnCyclopsSonarUsedEvent;
    public static event Action<GameObject> OnEggHatchedEvent;
    public static event Action<EMPBlast, GameObject> OnEmpHitEvent;
    public static event Action<EMPBlast, Collider> OnEmpTouchEvent;

    public static event Action<StringBuilder, TechType, GameObject> ItemTooltipEvent;

    //public static event Action<WaterFogValues> fogCalculateEvent;
    public static event Action<BuildabilityCheck> ConstructabilityEvent;

    public static event Action<BreathabilityCheck> BreathabilityEvent;

    //public static event Action<O2UseCheck> o2UseEvent;
    public static event Action<StoryHandCheck> StoryHandEvent;
    public static event Action<RadiationCheck> RadiationCheckEvent;
    public static event Action<BulkheadLaserCutterHoverCheck> BulkheadLaserHoverEvent;

    public static event Action<KnifeHarvest> KnifeHarvestEvent;

    //public static event Action<MusicSelectionCheck> musicBiomeChoiceEvent;
    public static event Action<FruitPlantTag> OnFruitPlantTickEvent;
    public static event Action<ReaperLeviathan, Vehicle> ReaperGrabVehicleEvent;
    public static event Action<SubRoot, DamageInfo> CyclopsDamageEvent;
    public static event Action<FMOD_CustomEmitter> OnSoundPlayedEvent;
    public static event Action<SolarEfficiencyCheck> SolarEfficiencyEvent;
    public static event Action<Vehicle, Player> VehicleEnterEvent;
    public static event Action<DepthCompassCheck> DepthCompassEvent;
    public static event Action<Survival, Player, bool> RespawnEvent;
    public static event Action<PropulsibilityCheck> PropulsibilityEvent;
    public static event Action<Drillable, Vector3, Exosuit> DrillableDrillTickEvent;
    public static event Action<DroppabilityCheck> DroppabilityEvent;
    public static event Action<MapRoomFunctionality> ScannerRoomTickEvent;
    public static event Action ItemsLostEvent;
    public static event Action<Vehicle> OnVehicleDestroyEvent;
    public static event Action<StorageContainer, GUIHand> StorageHoverEvent;
    public static event Action<ModuleFireCostCheck> ModuleFireCostEvent;
    public static event Action<PDAScanner.EntryData> ScanCompleteEvent;
    public static event Action SelfScanEvent;
    public static event Action<uGUI_MapRoomScanner> ScannerRoomTechTypeListingEvent;
    public static event Action<StasisEffectCheck> OnStasisRifleFreezeEvent;
    public static event Action<StasisEffectCheck> OnStasisRifleUnfreezeEvent;

    public static event Action<RedundantScanEvent> OnRedundantScanEvent;

    // public static event Action<EquipmentCompatibilityCheck> equipmentCompatibilityCheckEvent;
    public static event Action<EquipmentTypeCheck> EquipmentTypeCheckEvent;
    public static event Action<EatAttempt> TryEatEvent;
    public static event Action<Survival, GameObject> OnEatEvent;
    public static event Action<SwimSpeedCalculation> GetSwimSpeedEvent;
    public static event Action<Bed> OnSleepEvent;
    public static event Action<FoodRateCalculation> GetFoodRateEvent;
    public static event Action<PlayerInput> GetPlayerInputEvent;
    public static event Action<Bullet, Vehicle> OnTorpedoFireEvent;
    public static event Action<SeamothTorpedo, Transform> OnTorpedoExplodeEvent;
    public static event Action<CreatureSeeObjectCheck> CanCreatureSeeObjectEvent;
    public static event Action<AggressiveToPilotingVehicleCheck> AggressiveToPilotingEvent;
    public static event Action<Base> BaseRebuildEvent;
    public static event Action<BaseStrengthCalculation> BaseStrengthComputeEvent;
    public static event Action<WaterFilterSpawn> WaterFilterSpawnEvent;
    public static event Action<GrowingPlant, float> GrowingPlantTickEvent;
    public static event Action<CuteFishHandTarget, Player, CuteFishHandTarget.CuteFishCinematic> OnPlayWithCuddlefish;
    public static event Action<Rocket, int, bool> OnRocketStageCompletedEvent;
    public static event Action<CrashedShipExploder> AuroraSpawnEvent;
    public static event Action<CraftTimeCalculation> CraftTimeEvent;
    public static event Action<SeaMoth> SeamothDischargeEvent;
    public static event Action<SinkingGroundChunk> SpawnTreaderChunk;
    public static event Action<Crash> CrashfishExplodeEvent;
    public static event Action<TargetabilityCheck> TargetabilityEvent;

    private static BasicText _updateNotice = new(TextAlignmentOptions.Center);

    public static readonly HashSet<TechType> GravTrapTechSet = [];

    private static bool _hasLoadedAWorld;
    private static bool _outdatedMods;

    private static bool _isKnifeHarvesting;
    private static CustomBiome _currentCustomBiome;

    public static bool SkipWorldForces = false;
    public static bool SkipSkyApplier = false;

    private static bool _skipZeroedDeserialization =
        DIMod.config.getBoolean(DIConfig.ConfigEntries.SKIPZEROEDIDOVERWRITE);

    private static readonly List<Pickupable> CollectedItemsToDestroy = [];

    private static GameObject _teleportWithPlayer;
    private static PropulsionCannon _activePropulsionGun;
    private static Vector3 _relativeGrabPosition;
    private static int _selectedSlot;

    static DIHooks() {
        SNUtil.log("Initializing DIHooks");

        PrecursorTeleporter.TeleportEventStart += StartTeleport;
        PrecursorTeleporter.TeleportEventEnd += StopTeleport;

        GravTrapTechSet.AddRange(Gravsphere.allowedTechTypes);
    }

    public class PlayerInput {
        public readonly Vector3 OriginalInput;
        public Vector3 SelectedInput;

        internal PlayerInput(Vector3 vec) {
            OriginalInput = vec;
            SelectedInput = vec;
        }
    }

    public class DamageToDeal {
        public readonly float OriginalAmount;
        public readonly DamageType Type;
        public readonly GameObject Target;
        public readonly GameObject Dealer;

        private bool _disallowFurtherChanges;

        internal float Amount;

        internal DamageToDeal(float amt, DamageType tt, GameObject tgt, GameObject dl) {
            OriginalAmount = amt;
            Amount = OriginalAmount;
            Type = tt;
            Target = tgt;
            Dealer = dl;
            _disallowFurtherChanges = false;
        }

        public void LockValue() {
            _disallowFurtherChanges = true;
        }

        public void SetValue(float amt) {
            if (_disallowFurtherChanges)
                return;
            Amount = amt;
            if (Amount < 0)
                Amount = 0;
        }

        public float GetAmount() {
            return Amount;
        }
    }

    public class ItemPickup {
        public readonly Pickupable Item;
        public readonly Exosuit Prawn;
        public readonly bool IsKnife;

        public bool Destroy = false;

        internal ItemPickup(Pickupable pp, Exosuit exo, bool knife) {
            Item = pp;
            Prawn = exo;
            IsKnife = knife;
        }
    }

    public class KnifeHarvest {
        public readonly GameObject Hit;
        public readonly TechType ObjectType;
        public readonly bool IsAlive;
        public readonly bool WasAlive;

        public readonly HarvestType HarvestType;
        public readonly TechType DefaultDrop;

        public readonly Dictionary<TechType, int> Drops = new();

        internal KnifeHarvest(GameObject go, TechType tt, bool isa, bool was) {
            Hit = go;
            ObjectType = tt;
            IsAlive = isa;
            WasAlive = was;
            HarvestType = TechData.GetHarvestType(tt);
            DefaultDrop = TechData.GetHarvestOutput(tt);

            if ((HarvestType == HarvestType.DamageAlive && WasAlive) ||
                (HarvestType == HarvestType.DamageDead && !IsAlive)) {
                var num = 1;
                if (HarvestType == HarvestType.DamageAlive && !IsAlive)
                    num += TechData.GetHarvestFinalCutBonus(tt);

                if (DefaultDrop != TechType.None)
                    Drops[DefaultDrop] = num;
            }
        }
    }

    public class BiomeCheck {
        public readonly string OriginalValue;
        public readonly Vector3 Position;

        private bool _disallowFurtherChanges;

        internal string Biome;

        internal BiomeCheck(string amt, Vector3 pos) {
            OriginalValue = amt;
            Biome = OriginalValue;
            Position = pos;
            _disallowFurtherChanges = false;
        }

        public void LockValue() {
            _disallowFurtherChanges = true;
        }

        public void SetValue(string b) {
            if (_disallowFurtherChanges)
                return;
            Biome = b;
        }
    }
    /*
    public class MusicSelectionCheck {

        public readonly string originalBiome;
        public readonly MusicManager manager;

        private bool disallowFurtherChanges;

        internal string biomeToDelegateTo;

        internal MusicSelectionCheck(string biome, MusicManager mgr) {
            originalBiome = biome;
            biomeToDelegateTo = originalBiome;
            manager = mgr;
            disallowFurtherChanges = false;
        }

        public void lockValue() {
            disallowFurtherChanges = true;
        }

        public void setValue(string b) {
            if (disallowFurtherChanges)
                return;
            biomeToDelegateTo = b;
        }

    }*/

    public class WaterTemperatureCalculation {
        public readonly float OriginalValue;
        public readonly Vector3 Position;
        public readonly WaterTemperatureSimulation Manager;

        private bool _disallowFurtherChanges;

        internal float Temperature;

        internal WaterTemperatureCalculation(float amt, WaterTemperatureSimulation sim, Vector3 pos) {
            OriginalValue = amt;
            Temperature = OriginalValue;
            Position = pos;
            Manager = sim;
            _disallowFurtherChanges = false;
        }

        public void LockValue() {
            _disallowFurtherChanges = true;
        }

        public float GetTemperature() {
            return Temperature;
        }

        public void SetValue(float amt) {
            //SNUtil.writeToChat("Setting water temp to "+amt);
            if (_disallowFurtherChanges)
                return;
            Temperature = amt;
        }
    }

    public class SwimSpeedCalculation {
        public readonly float OriginalValue;

        private bool _disallowFurtherChanges;

        internal float Speed;

        internal SwimSpeedCalculation(float amt) {
            OriginalValue = amt;
            Speed = OriginalValue;
            _disallowFurtherChanges = false;
        }

        public void LockValue() {
            _disallowFurtherChanges = true;
        }

        public float GetValue() {
            return Speed;
        }

        public void SetValue(float amt) {
            //SNUtil.writeToChat("Setting water temp to "+amt);
            if (_disallowFurtherChanges)
                return;
            Speed = amt;
        }
    }

    public class FoodRateCalculation {
        public readonly float OriginalValue;
        public float Rate;

        internal FoodRateCalculation(float amt) {
            OriginalValue = amt;
            Rate = OriginalValue;
        }
    }

    public class WaterFogValues {
        public readonly Color OriginalColor;
        public readonly float OriginalDensity;
        public readonly float OriginalSunValue;

        public Color Color;
        public float Density;
        public float SunValue;

        internal WaterFogValues(Color c, float d, float s) {
            OriginalColor = c;
            OriginalDensity = d;
            OriginalSunValue = s;
            Density = d;
            Color = c;
            SunValue = s;
        }
    }

    public class EatAttempt {
        public readonly Survival Survival;
        public readonly GameObject Food;

        public bool AllowEat = true;

        internal EatAttempt(Survival s, GameObject go) {
            Survival = s;
            Food = go;
        }
    }

    public class KnifeAttempt {
        public readonly LiveMixin Target;
        public readonly bool DefaultValue;

        public bool AllowKnife = true;

        internal KnifeAttempt(LiveMixin tgt, bool def) {
            Target = tgt;
            DefaultValue = def;
        }
    }

    public class GravTrapGrabAttempt {
        public readonly Gravsphere Gravtrap;
        public readonly GameObject Target;
        public readonly bool DefaultValue;

        public bool AllowGrab;

        internal GravTrapGrabAttempt(Gravsphere s, GameObject tgt, bool def) {
            Gravtrap = s;
            Target = tgt;
            DefaultValue = def;
            AllowGrab = def;
        }
    }

    public class BuildabilityCheck {
        public readonly bool OriginalValue;
        public readonly Collider PlaceOn;

        public bool Placeable;
        public bool IgnoreSpaceRequirements = false;

        internal BuildabilityCheck(bool orig, Collider pos) {
            OriginalValue = orig;
            Placeable = orig;
            PlaceOn = pos;
        }
    }

    public class BreathabilityCheck {
        public readonly bool OriginalValue;
        public readonly Player Player;

        public bool Breathable;

        internal BreathabilityCheck(bool orig, Player ep) {
            OriginalValue = orig;
            Breathable = orig;
            Player = ep;
        }
    }

    public class O2UseCheck {
        public readonly float OriginalValue;
        public readonly int DepthClass;
        public readonly Player Player;

        public float Value;

        internal O2UseCheck(float orig, Player ep, int depth) {
            OriginalValue = orig;
            Value = orig;
            Player = ep;
            DepthClass = depth;
        }
    }
    /*
    public class EquipmentCompatibilityCheck {

        public readonly bool originalValue;
        public readonly Equipment container;
        public readonly Pickupable item;
        public readonly EquipmentType itemType;
        public readonly EquipmentType slotType;

        public bool allow;

        internal EquipmentCompatibilityCheck(Equipment box, Pickupable pp, EquipmentType t1, EquipmentType t2, bool orig) {
            originalValue = orig;
            allow = orig;
            container = box;
            item = pp;
            itemType = t1;
            slotType = t2;
        }

    }*/

    public class EquipmentTypeCheck {
        public readonly EquipmentType OriginalValue;
        public readonly TechType Item;

        public EquipmentType Type;

        internal EquipmentTypeCheck(TechType pp, EquipmentType orig) {
            OriginalValue = orig;
            Type = orig;
            Item = pp;
        }
    }

    public class StoryHandCheck {
        public readonly StoryGoal OriginalValue;
        public readonly StoryHandTarget Component;

        public bool Usable = true;
        public StoryGoal Goal;

        internal StoryHandCheck(StoryGoal orig, StoryHandTarget tgt) {
            OriginalValue = orig;
            Goal = orig;
            Component = tgt;
        }
    }

    public class RadiationCheck {
        public readonly Vector3 Position;
        public readonly float OriginalValue;
        //0-1

        public float Value;

        internal RadiationCheck(Vector3 pos, float orig) {
            OriginalValue = orig;
            Value = orig;
            Position = pos;
        }
    }

    public class PropulsibilityCheck {
        public readonly GameObject Obj;
        public readonly float OriginalValue;
        public readonly MonoBehaviour GunComponent;
        public readonly bool IsMass;

        public float Value;

        internal PropulsibilityCheck(GameObject go, float orig, MonoBehaviour gun, bool mass) {
            OriginalValue = orig;
            Value = orig;
            Obj = go;
            IsMass = mass;
            GunComponent = gun;
        }
    }

    public class SolarEfficiencyCheck {
        public readonly SolarPanel Panel;
        public readonly float OriginalValue;

        public float Value;

        internal SolarEfficiencyCheck(SolarPanel pos, float orig) {
            OriginalValue = orig;
            Value = orig;
            Panel = pos;
        }
    }

    public class BulkheadLaserCutterHoverCheck {
        public readonly Sealed Obj;

        public string RefusalLocaleKey = null;

        internal BulkheadLaserCutterHoverCheck(Sealed s) {
            Obj = s;
        }
    }

    public class DepthCompassCheck {
        public readonly int OriginalValue;
        public readonly int OriginalCrushValue;

        public int Value;
        public int CrushValue;

        internal DepthCompassCheck(int orig, int crush) {
            OriginalValue = orig;
            Value = orig;

            OriginalCrushValue = crush;
            CrushValue = crush;
        }
    }

    public class DroppabilityCheck {
        public readonly Pickupable Item;
        public readonly bool Notify;
        public readonly bool DefaultAllow;

        public bool Allow;
        public string Error = null;

        internal DroppabilityCheck(Pickupable pp, bool n, bool a) {
            Item = pp;
            Notify = n;
            DefaultAllow = a;
            Allow = DefaultAllow;
        }
    }

    public class ModuleFireCostCheck {
        public readonly TechType Module;
        public readonly Vehicle Vehicle;
        public readonly float OriginalValue;

        public float Value;

        internal ModuleFireCostCheck(Vehicle v, TechType item, float orig) {
            OriginalValue = orig;
            Value = orig;
            Module = item;
            Vehicle = v;
        }
    }

    public class StasisEffectCheck {
        public readonly StasisSphere Sphere;
        public readonly Rigidbody Body;

        public bool ApplyKinematicChange = true;
        public bool AddToTargetList = true;
        public bool SendMessage = true;
        public bool DoFX = true;

        internal StasisEffectCheck(StasisSphere s, Rigidbody b) {
            Sphere = s;
            Body = b;
        }
    }

    public class RedundantScanEvent {
        public bool PreventNormalDrop = false;
    }

    public class CreatureSeeObjectCheck {
        public readonly Creature Creature;
        public readonly GameObject Target;
        public readonly bool DefaultValue;
        public readonly float AtDistance;

        public bool CanSee;

        internal CreatureSeeObjectCheck(Creature c, GameObject tgt, bool def, float dist) {
            Creature = c;
            Target = tgt;
            DefaultValue = def;
            AtDistance = dist;
            CanSee = def;
        }
    }

    public class AggressiveToPilotingVehicleCheck {
        public readonly AggressiveToPilotingVehicle AI;
        public readonly Vehicle Vehicle;
        public readonly bool DefaultVisiblity;

        public bool CanTarget;

        internal AggressiveToPilotingVehicleCheck(AggressiveToPilotingVehicle ai, Vehicle v, bool def) {
            AI = ai;
            Vehicle = v;
            DefaultVisiblity = def;
            CanTarget = def;
        }
    }

    public class WaterFilterSpawn {
        public readonly FiltrationMachine Filter;
        public readonly Pickupable DefaultItem;

        public Pickupable Item;

        internal WaterFilterSpawn(FiltrationMachine fm, Pickupable pp) {
            Filter = fm;
            DefaultItem = pp;
            Item = DefaultItem;
        }
    }

    public class BaseStrengthCalculation {
        public readonly BaseHullStrength Component;

        private readonly Dictionary<Int3, float>
            _cellContributions = new(); //can use Base.GetCellObject to get the BaseCell

        private readonly Dictionary<GameObject, float> _bonusStrength = new();
        public float DynamicStrength { get; private set; }

        public float InitialStrength = 10;

        public float FinalStrength => InitialStrength + DynamicStrength;

        internal BaseStrengthCalculation(BaseHullStrength b) {
            Component = b;
        }

        public void ComputeCellStrength(Int3 cell) {
            var amt = Component.baseComp.GetHullStrength(cell);
            _cellContributions[cell] = amt;
            DynamicStrength += amt;
        }

        public void AddBonusStrength(GameObject c, float amt) {
            _bonusStrength[c] = amt;
            DynamicStrength += amt;
        }

        public float GetStrength(Int3 cell) {
            return _cellContributions.TryGetValue(cell, out var contribution) ? contribution : 0;
        }
    }

    public class CraftTimeCalculation {
        public readonly float OriginalDuration;
        public readonly Crafter Crafter;
        public readonly TechType Recipe;

        public float CraftingDuration;

        internal CraftTimeCalculation(float amt, Crafter c, TechType tt) {
            OriginalDuration = amt;
            CraftingDuration = OriginalDuration;
            Recipe = tt;
            Crafter = c;
        }
    }

    public class TargetabilityCheck {
        public readonly bool OriginalValue;
        public readonly Transform Transform;
        public readonly PrefabIdentifier Prefab;

        public bool AllowTargeting;

        internal TargetabilityCheck(bool orig, Transform obj, PrefabIdentifier pi) {
            OriginalValue = orig;
            Transform = obj;
            AllowTargeting = orig;
            Prefab = pi;
        }
    }

    public static void OnTick(DayNightCycle cyc) {
        if (BuildingHandler.instance.isEnabled) {
            if (GameInput.GetButtonDown(GameInput.Button.LeftHand)) {
                BuildingHandler.instance.handleClick(Input.GetKeyDown(KeyCode.LeftControl));
            }

            if (GameInput.GetButtonDown(GameInput.Button.RightHand)) {
                BuildingHandler.instance.handleRClick(Input.GetKeyDown(KeyCode.LeftControl));
            }

            if (Input.GetKeyDown(KeyCode.Delete)) {
                BuildingHandler.instance.deleteSelected();
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt)) {
                BuildingHandler.instance.manipulateSelected();
            }
        }

        CustomBiome.tickMusic(cyc);

        if (GetWorldAge() > 0.25F) {
            SaveSystem.populateLoad();
        }

        OnDayNightTickEvent?.Invoke(cyc);
    }

    public static void OnMainMenuLoaded() {
        _worldLoadTime = -1;
    }

    public static void OnWorldLoaded() {
        var warnRestart = _hasLoadedAWorld;
        _hasLoadedAWorld = true;
        _worldLoadTime = Time.time;
        SNUtil.log("Intercepted world load", SNUtil.diDLL);
        DuplicateRecipeDelegate.updateLocale();
        CustomEgg.updateLocale();
        PickedUpAsOtherItem.updateLocale();
        SeamothModule.updateLocale();

        StoryHandler.instance.onLoad();
        CustomLocaleKeyDatabase.onLoad();

        foreach (var cb in BiomeBase.GetCustomBiomes())
            cb.onLoad();

        foreach (var kvp in RecipeUtil.techsToRemoveIf) {
            if (!kvp.Value.isReady(Player.main))
                KnownTech.Remove(kvp.Key);
        }

        /*
        SNUtil.log("Item goals:", SNUtil.diDLL);
        foreach (Story.ItemGoal g in Story.StoryGoalManager.main.itemGoalTracker.goals)
            SNUtil.log(g.key+" from "+g.techType, SNUtil.diDLL);
        SNUtil.log("Location goals:", SNUtil.diDLL);
        foreach (Story.LocationGoal g in Story.StoryGoalManager.main.locationGoalTracker.goals)
            SNUtil.log(g.key+" at "+g.location+" ("+g.position+")", SNUtil.diDLL);
        SNUtil.log("Biome goals:", SNUtil.diDLL);
        foreach (Story.BiomeGoal g in Story.StoryGoalManager.main.biomeGoalTracker.goals)
            SNUtil.log(g.key+" in "+g.biome, SNUtil.diDLL);
        SNUtil.log("Compound goals:", SNUtil.diDLL);
        foreach (Story.CompoundGoal g in Story.StoryGoalManager.main.compoundGoalTracker.goals)
            SNUtil.log(g.key+" of ["+string.Join(", ",g.preconditions)+"]", SNUtil.diDLL);
        */

        //SaveSystem.populateLoad();

        var vers = ModVersionCheck.getOutdatedVersions();
        _updateNotice.SetLocation(0, 250);
        _updateNotice.SetSize(24);
        _updateNotice.SetColor(Color.yellow);
        List<string> li = [];
        _outdatedMods = vers.Count > 0;
        if (_outdatedMods) {
            li.Add("Your versions of the following mods are out of date:");
            foreach (var mv in vers) {
                li.Add(
                    mv.modName + ": Current version " + mv.currentVersion + ", newest version " +
                    mv.remoteVersion.Invoke()
                );
            }

            li.Add("Update your mods to remove this warning.");
            //li.Add("Run the /autoUpdate command to download and install these updates automatically.");
        }

        vers = ModVersionCheck.getErroredVersions();
        if (vers.Count > 0) {
            li.Add("Several mods failed to fetch version information:");
            foreach (var mv in vers) {
                li.Add(
                    mv.modName + ": Installed version " + mv.currentVersion + ", remote version " +
                    mv.remoteVersion.Invoke()
                );
            }

            if (SNUtil.checkPiracy()) {
                li.Add(
                    "<color=#ff5050ff>This appears to be a result of pirating the game, which cuts its internet connection. There is nothing that can be done without buying Subnautica.</color>"
                );
            } else {
                li.Add(
                    "You should redownload and reinstall mods with local errors and contact Reika if remote versions are invalid."
                );
                li.Add("This message can be temporarily hidden with /hideVersions");
            }
        }

        if (warnRestart)
            li.Add(
                "You have reloaded a save without exiting the game. This breaks mod loading and will damage your world. Restart your game when changing/reloading saves.\nExit the game now, and DO NOT SAVE before doing so."
            );
        if (li.Count > 0)
            _updateNotice.ShowMessage(string.Join("\n", li));
        else
            _updateNotice.Hide();

        OnWorldLoadedEvent?.Invoke();
    }

    public static void SetWarningText(params string[] s) {
        SetWarningText(s.AsEnumerable());
    }

    public static void SetWarningText(IEnumerable<string> li) {
        _updateNotice.ShowMessage(string.Join("\n", li));
    }
    /*
    internal static void autoUpdate() { //TODO move to own class, and make msg prep and call its own method
        if (outdatedMods | true) {
            SNUtil.writeToChat("Downloading new versions of mods...");
            string dirpath = Path.Combine(Environment.CurrentDirectory, "DIDownloads");
            Directory.CreateDirectory(dirpath);
            using(HttpClient client = new HttpClient()) {
                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/ReikaKalseki/Reika_SubnauticaModsShared/releases/latest");
                msg.Headers.Add("User-Agent", "Dragon Industries Autoupdate");
                msg.Headers.Add("Accept", "application/vnd.github+json");
                msg.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
                Task<HttpResponseMessage> resp = client.SendAsync(msg);
                resp.RunSynchronously();
                Task<string> task = resp.Result.Content.ReadAsStringAsync();
                task.RunSynchronously();
                JObject json = JObject.Parse(task.Result);
                int id = (int)json["id"];
                msg = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/ReikaKalseki/Reika_SubnauticaModsShared/releases/"+id+"/assets");
                msg.Headers.Add("User-Agent", "Dragon Industries Autoupdate");
                msg.Headers.Add("Accept", "application/vnd.github+json");
                msg.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
                resp = client.SendAsync(msg);
                resp.RunSynchronously();
                task = resp.Result.Content.ReadAsStringAsync();
                task.RunSynchronously();
                json = JObject.Parse(task.Result);
                foreach (JObject mod in json.Values()) {
                    string url = ((string)json["browser_download_url"]).Replace("\"", "");
                    SNUtil.writeToChat("Downloading from "+url);
                    new System.Net.WebClient().DownloadFile(url, Path.Combine(dirpath, url.Substring(url.LastIndexOf('/')+1)));
                }
            }
        }
        else {
            SNUtil.writeToChat("No outdated mods, no download will be performed.");
        }

        //https://github.com/ReikaKalseki/Reika_SubnauticaModsShared/releases/download/Downloads/AqueousEngineering.zip
        //https://github.com/ReikaKalseki/Reika_SubnauticaModsShared/releases/download/Downloads/Dragon_Industries_-_Alterra_Division.zip
    }*/

    internal static void HideVersions() {
        _updateNotice.Hide();
    }

    public static float GetWorldAge() {
        return _worldLoadTime < 0 ? -1 : Time.time - _worldLoadTime;
    }

    public static bool IsWorldLoaded() {
        return _worldLoadTime > 0;
    }

    public static bool HasWorldLoadStarted() {
        return _hasLoadedAWorld;
    }

    public static void TickPlayer(Player ep) {
        if (Camera.main != null) {
            var b = BiomeBase.GetBiome(Camera.main.transform.position) as CustomBiome;
            if (_currentCustomBiome != b)
                RecomputeFog();
            _currentCustomBiome = b;
        }

        if (Time.timeScale <= 0)
            return;
        _updateNotice.SetColor(Color.yellow);

        SpawnedItemTracker.instance.tick();

        if (CollectedItemsToDestroy.Count > 0) {
            foreach (var pp in CollectedItemsToDestroy.Where(pp => pp)) {
                Inventory.main.container.RemoveItem(pp, true);
                pp.gameObject.destroy(false);
            }

            CollectedItemsToDestroy.Clear();
        }

        StoryHandler.instance.tick(ep);
        ScreenFXManager.instance.tick();

        if (OnPlayerTickEvent != null) {
            try {
                OnPlayerTickEvent.Invoke(ep);
            } catch (Exception ex) {
                SNUtil.writeToChat("Threw exception running player tick hooks: " + ex);
            }
        }
    }

    public static void TickSeamoth(SeaMoth sm) {
        if (Time.timeScale <= 0)
            return;

        OnSeamothTickEvent?.Invoke(sm);
    }

    public static void TickPrawn(Exosuit sm) {
        if (Time.timeScale <= 0)
            return;

        OnPrawnTickEvent?.Invoke(sm);
    }

    public static void TickSub(SubRoot sub) {
        if (Time.timeScale <= 0)
            return;

        if (sub.isCyclops && OnCyclopsTickEvent != null)
            OnCyclopsTickEvent.Invoke(sub);
        else if (sub.isBase && OnBaseTickEvent != null)
            OnBaseTickEvent.Invoke(sub as BaseRoot);
    }

    public static float GetWaterTemperature(float ret, WaterTemperatureSimulation sim, Vector3 pos) {
        if (GetTemperatureEvent != null) {
            var calc = new WaterTemperatureCalculation(ret, sim, pos);
            GetTemperatureEvent.Invoke(calc);
            return calc.Temperature;
        } else {
            return ret;
        }
    }

    public static int DamageDebugLevel = 0;

    public static float RecalculateDamage(float damage, DamageType type, GameObject target, GameObject dealer) {
        if (DIMod.config.getBoolean(DIConfig.ConfigEntries.INFITUBE) && target.isCoralTube())
            return Mathf.Min(damage, target.FindAncestor<LiveMixin>().health - 1);
        //if (target.isPlayer()) {
        var hm = target.GetComponent<HealthModifier>();
        if (hm)
            damage *= hm.damageFactor;
        //}
        var pi = target.GetComponent<PrefabIdentifier>();
        if (pi && pi.ClassId == CustomEgg.getEgg(TechType.SpineEel).Info.ClassID &&
            (type == DamageType.Acid || type == DamageType.Poison))
            return 0;
        if (OnDamageEvent != null) {
            var deal = new DamageToDeal(damage, type, target, dealer);
            OnDamageEvent.Invoke(deal);
            if (DamageDebugLevel > 1 ||
                (DamageDebugLevel == 1 && !Mathf.Approximately(deal.OriginalAmount, deal.Amount)))
                SNUtil.writeToChat(
                    "Adjusting damage type " + type + " yield from " + deal.OriginalAmount + " to " + deal.Amount
                );
            return deal.Amount;
        } else {
            if (DamageDebugLevel > 2)
                SNUtil.writeToChat("Applying unchanged damage amount " + damage);
            return damage;
        }
    }

    public static string GetBiomeAt(string orig, Vector3 pos) {
        if (GetBiomeEvent != null) {
            var deal = new BiomeCheck(orig, pos);
            GetBiomeEvent.Invoke(deal);
            return deal.Biome;
        } else {
            return orig;
        }
    }

    public static void DoKnifeHarvest(Knife caller, GameObject target, bool isAlive, bool wasAlive) {
        var tt = CraftData.GetTechType(target);
        if (tt == TechType.Creepvine)
            GoalManager.main.OnCustomGoalEvent("Cut_Creepvine");
        if (tt == TechType.BigCoralTubes && DIMod.config.getBoolean(DIConfig.ConfigEntries.INFITUBE) &&
            target.FindAncestor<LiveMixin>().health <= 2)
            wasAlive = false;
        var harv = new KnifeHarvest(target, tt, isAlive, wasAlive);
        KnifeHarvestEvent?.Invoke(harv);

        _isKnifeHarvesting = true;
        foreach (var kvp in harv.Drops)
            CraftData.AddToInventory(kvp.Key, kvp.Value, false, false);
        _isKnifeHarvesting = false;
    }

    public static void FireKnifeHarvest(GameObject target, Dictionary<TechType, int> drops) {
        var harv = new KnifeHarvest(target, CraftData.GetTechType(target), false, false);
        harv.Drops.Clear();
        drops.ForEach(kvp => harv.Drops[kvp.Key] = kvp.Value);
        KnifeHarvestEvent?.Invoke(harv);

        foreach (var kvp in harv.Drops)
            CraftData.AddToInventory(kvp.Key, kvp.Value, false, false);
    }

    public static void OnPrawnItemPickedUp(Pickupable pp) {
        if (pp)
            OnItemPickedUp(pp, Player.main.GetVehicle() as Exosuit);
    }

    public static void OnItemPickedUp(Pickupable p) {
        OnItemPickedUp(p, null);
    }

    public static void OnItemPickedUp(Pickupable p, Exosuit prawn) {
        List<Pickupable> collected = [p];
        var tt = p.GetTechType();
        var mapTo = PickedUpAsOtherItem.getPickedUpAsOther(tt);
        //SNUtil.writeToChat("Pickup "+tt+" >> "+mapTo);
        if (mapTo != null) {
            if (prawn)
                prawn.storageContainer.container.DestroyItem(tt);
            else
                Inventory.main.container.DestroyItem(tt);

            p.gameObject.destroy(false); //not immediate because prawn is animation
            var tt2 = mapTo.getTemplate();
            var n = mapTo.getNumberCollectedAs();
            SNUtil.log("Converting pickup '" + p + "' to '" + tt2 + "' x" + n, SNUtil.diDLL);
            collected.Clear();
            for (var i = 0; i < n; i++) {
                var go = ObjectUtil.createWorldObject(tt2, true, false);
                p = go.GetComponent<Pickupable>();
                if (prawn)
                    prawn.storageContainer.container.UnsafeAdd(new InventoryItem(p));
                else
                    Inventory.main.Pickup(p, false);
                collected.Add(p);
            }

            SNUtil.log("Conversion complete", SNUtil.diDLL);
            tt = tt2;
        }

        if (tt == TechType.None) {
            var tag = p.gameObject.GetComponent<TechTag>();
            if (tag)
                tt = tag.type;
        }

        if (tt == TechType.None) {
            var pi = p.gameObject.GetComponent<PrefabIdentifier>();
            if (pi)
                tt = CraftData.entClassTechTable[pi.ClassId];
        }

        if (tt != TechType.None) {
            TechnologyUnlockSystem.instance.triggerDirectUnlock(tt);
            FirstObtainmentSystem.instance.onPickup(tt);
        }

        /*
        foreach (Renderer r in p.gameObject.GetComponentsInChildren<Renderer>()) {
            foreach (Material m in r.materials) {
                //m.DisableKeyword("FX_BUILDING"); //breaks items which use it for their appearance
            }
        }
        */
        var cc = WorldUtil.getClosest<GenUtil.CrateManagement>(p.gameObject);
        if (cc && Vector3.Distance(p.transform.position, cc.transform.position) < 1.5F)
            cc.onPickup(p);

        if (OnItemPickedUpEvent != null) {
            foreach (var pp in collected) {
                var ip = new ItemPickup(pp, prawn, _isKnifeHarvesting);
                OnItemPickedUpEvent.Invoke(ip);
                if (ip.Destroy)
                    CollectedItemsToDestroy
                        .Add(
                            pp
                        ); //need to delegate until later because this is called before it is actually added to the inv
            }
        }
    }

    public static bool CanPlayerBreathe(bool orig, Player p) {
        if (p.GetComponent<TemporaryBreathPrevention>())
            return false;
        if (BreathabilityEvent != null) {
            var deal = new BreathabilityCheck(orig, p);
            BreathabilityEvent.Invoke(deal);
            return deal.Breathable;
        }

        return orig;
    }

    /*
    public static float getPlayerO2Use(Player ep, float breathingInterval, int depthClass) {
        if (o2UseEvent != null) {
            O2UseCheck deal = new O2UseCheck(breathingInterval, ep, depthClass);
            o2UseEvent.Invoke(deal);
            return deal.value;
        }
        return
        return EnvironmentalDamageSystem.instance.getPlayerO2Use(ep, breathingInterval, depthClass);
    }
*/
    public static void OnEntityRegister(CellManager cm, LargeWorldEntity lw) {
        if (_worldLoadTime < 0) {
            OnWorldLoaded();
        } /*
        if (lw.cellLevel != LargeWorldEntity.CellLevel.Global) {
            BatchCells batchCells;
            Int3 block = cm.streamer.GetBlock(lw.transform.position);
            Int3 key = block / cm.streamer.blocksPerBatch;
            if (cm.batch2cells.TryGetValue(key, out batchCells)) {
                        Int3 u = block % cm.streamer.blocksPerBatch;
                        Int3 cellSize = BatchCells.GetCellSize((int)lw.cellLevel, cm.streamer.blocksPerBatch);
                        Int3 cellId = u / cellSize;
                        bool flag = cellId.x < 0 || cellId.y < 0 || cellId.z < 0;
                if (!flag) {
                    try {
                        //batchCells.Get(cellId, (int)lw.cellLevel);
                        batchCells.GetCells((int)lw.cellLevel).Get(cellId);
                    }
                    catch {
                        flag = true;
                    }
                }
                if (flag) {
                    SNUtil.log("Moving object "+lw.gameObject+" to global cell, as it is outside the world bounds and was otherwise going to bind to an OOB cell.");
                    lw.cellLevel = LargeWorldEntity.CellLevel.Global;
                }
            }
        }*/

        OnEntityRegisterEvent?.Invoke(cm, lw);
    }

    public static void OnPopup(uGUI_PopupNotification gui) { /*
        System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
        t.ToString();*/
        //SNUtil.log("TRIGGER POPUP UNLOCK "+System.Environment.StackTrace, SNUtil.diDLL);
    }

    public static void OnFarmedPlantGrowingSpawn(Plantable p, GameObject plant) {
        var tt = p.gameObject.GetComponent<TechTag>();
        if (tt) {
            var plantType = BasicCustomPlant.getPlant(tt.type);
            //SNUtil.writeToChat("Planted "+tt+" > "+plantType);
            if (plantType != null) {
                //SNUtil.writeToChat(plant.GetComponentsInChildren<Renderer>(true).Length+" Renderers");
                RenderUtil.swapToModdedTextures(plant.GetComponentInChildren<Renderer>(true), plantType);
                plant.gameObject.EnsureComponent<TechTag>().type = plantType.Info.TechType;
                plant.gameObject.EnsureComponent<PrefabIdentifier>().ClassId = plantType.ClassID;
            }
        }
    }

    public static void OnFarmedPlantGrowDone(GrowingPlant p, GameObject plant) {
        var tt = p.gameObject.GetComponent<TechTag>();
        if (tt) {
            var plantType = BasicCustomPlant.getPlant(tt.type);
            //SNUtil.writeToChat("Grew "+tt+" > "+plantType);
            if (plantType != null) {
                ObjectUtil.convertTemplateObject(plant, plantType);
            }
        }
    }

    public static void OnDockingBaySpawn(VehicleDockingBay b) {
        b.gameObject.EnsureComponent<DockLock>();
    }

    public static void OnSkyApplierSpawn(SkyApplier pk) {
        var pi = pk.GetComponent<PrefabIdentifier>(); /*
        if (pi) {
            foreach (Collider c in pi.GetComponentsInChildren<Collider>())
                c.gameObject.EnsureComponent<ColliderPrefabLink>().init(pi);
        }*/
        if (pk.isPlayer())
            pk.gameObject.EnsureComponent<AoERadiationTracker>();
        if (pk.GetComponent<Vehicle>()) {
            pk.gameObject.EnsureComponent<FixedBounds>()._bounds = new Bounds(Vector3.zero, Vector3.one * 5);
            var go = pk.gameObject.getChildObject("LavaWarningTrigger");
            if (!go) {
                go = new GameObject("LavaWarningTrigger") {
                    transform = {
                        localPosition = Vector3.zero,
                        localRotation = Quaternion.identity,
                    },
                };
                go.transform.SetParent(pk.transform);
            }

            var sp = go.EnsureComponent<SphereCollider>();
            sp.center = Vector3.zero;
            sp.radius = NearLavaRadius;
            sp.isTrigger = true;
            go.EnsureComponent<LavaWarningTriggerDetector>();
        }

        var fp = pk.GetComponent<FruitPlant>();
        if (fp) {
            fp.gameObject.EnsureComponent<FruitPlantTag>().SetPlant(fp);
        }

        if (pi && pi.ClassId == "bb16d2bf-bc85-4bfa-a90e-ddc7343b0ac2") {
            WreckDoorSwaps.setupRepairableDoor(pk.gameObject);
        }

        if (OnSkyApplierSpawnEvent != null) {
            try {
                OnSkyApplierSpawnEvent.Invoke(pk);
            } catch (Exception ex) {
                SNUtil.log(
                    "Threw error when processing SkyApplier spawn of " + pk.gameObject.GetFullHierarchyPath() + ": " +
                    ex.ToString()
                );
            }
        }
    }

    //private static bool needsLavaDump = true;

    public class FruitPlantTag : MonoBehaviour {
        private FruitPlant _plant;
        private float _baseGrowthTime;

        private float _lastTickTime = -1;

        internal void SetPlant(FruitPlant fp) {
            _plant = fp;
            _baseGrowthTime = fp.fruitSpawnInterval;
        }

        public FruitPlant GetPlant() {
            return _plant;
        }

        public float GetBaseGrowthTime() {
            return _baseGrowthTime;
        }

        private void Update() {
            if (OnFruitPlantTickEvent != null) {
                var time = DayNightCycle.main.timePassedAsFloat;
                if (time - _lastTickTime >= 0.5F) {
                    _lastTickTime = time;
                    OnFruitPlantTickEvent.Invoke(this);
                }
            }
        }
    }

    public class DockLock : MonoBehaviour {
        private VehicleDockingBay _bay;

        private float _lastTime;

        public void Update() {
            if (!_bay)
                _bay = GetComponent<VehicleDockingBay>();

            if (_bay.dockedVehicle && DayNightCycle.main.timePassedAsFloat - _lastTime >= 0.5F &&
                !_bay.dockedVehicle.GetComponentInParent<SubRoot>()) {
                _bay.DockVehicle(_bay.dockedVehicle, false);
                SNUtil.writeToChat(
                    "Re-binding vehicle " + _bay.dockedVehicle + " to docking bay " +
                    _bay.gameObject.GetFullHierarchyPath()
                );
                _lastTime = DayNightCycle.main.timePassedAsFloat;
            }
        }
    }

    public class LavaWarningTriggerDetector : IgnoreTrigger {
        private TemperatureDamage _damage;
        private Vehicle _vehicle;
        private Collider _sphere;

        private float _lastLavaTime = -1;
        private float _lastGeyserTime = -1;

        private float _lastCheckTime = -1;

        private static readonly List<Vector3> SpherePoints = [];
        private static readonly int RaysPerTick = 10;
        private static int _spherePointIndex;

        private float _ambientTemperatureMinusLava;

        static LavaWarningTriggerDetector() {
            ComputePoints();
        }

        private static void ComputePoints() {
            var phi = Mathf.PI * (3F - Mathf.Sqrt(5F)); // golden angle in radians
            for (var i = 0; i < 100; i++) {
                var y = 1 - i / (100 - 1F) * 2; // y goes from 1 to -1
                var radius = Mathf.Sqrt(1 - y * y); // radius at y

                var theta = phi * i; // golden angle increment

                var x = Mathf.Cos(theta) * radius;
                var z = Mathf.Sin(theta) * radius;

                SpherePoints.Add(new Vector3(x, y, z));
            }

            for (var i = 0; i < 150; i++) {
                var ang = UnityEngine.Random.Range(0F, 360F);
                var x = Mathf.Cos(Mathf.Deg2Rad * ang) * NearLavaRadius;
                var z = Mathf.Sin(Mathf.Deg2Rad * ang) * NearLavaRadius;
                SpherePoints.Add(new Vector3(x, -UnityEngine.Random.Range(0F, 1F), z));
            }

            SpherePoints.Shuffle();
        }

        private void Update() {
            if (!_damage)
                _damage = gameObject.FindAncestor<TemperatureDamage>();
            if (!_vehicle)
                _vehicle = gameObject.FindAncestor<Vehicle>();
            if (!_sphere)
                _sphere = gameObject.GetComponent<Collider>();
            gameObject.transform.localPosition = Vector3.zero;

            var time = DayNightCycle.main.timePassedAsFloat;
            var dT = time - _lastCheckTime;
            if (dT >= 0.5) {
                _lastCheckTime = time;
                _ambientTemperatureMinusLava = WaterTemperatureSimulation.main.GetTemperature(transform.position);
            }

            if (_damage && _ambientTemperatureMinusLava >= 90)
                CheckNearbyLava();
        }

        private void CheckNearbyLava() {
            for (var i = _spherePointIndex; i < Math.Min(_spherePointIndex + RaysPerTick, SpherePoints.Count); i++) {
                var vec = SpherePoints[i];
                var hits = Physics.RaycastAll(
                    transform.position,
                    vec.normalized,
                    NearLavaRadius,
                    Voxeland.GetTerrainLayerMask()
                );
                //SNUtil.writeToChat(vec+" > "+hits.Length);
                foreach (var hit in hits) {
                    if (hit.transform && CheckLava(hit.point, Vector3.zero)) {
                        _spherePointIndex = i;
                        return;
                    }
                }
            }

            _spherePointIndex += RaysPerTick;
            if (_spherePointIndex >= SpherePoints.Count)
                _spherePointIndex = 0;
        }

        private void OnTriggerStay(Collider other) {
            if (_damage && _ambientTemperatureMinusLava >= 90) {
                CheckLava(GetCollisionPoint(other, out var norm), norm);
            }
        }

        private Vector3 GetCollisionPoint(Collider other, out Vector3 norm) {
            var ctr = transform.position;
            return Physics.ComputePenetration(
                other,
                other.transform.position,
                other.transform.rotation,
                _sphere,
                ctr,
                Quaternion.identity,
                out norm,
                out var depth
            )
                ? ctr + norm * (NearLavaRadius - depth)
                : Vector3.zero;
        }

        private bool CheckLava(Vector3 pt, Vector3 norm) {
            //SNUtil.log("Checking lava: "+pt+" @ "+lastLavaTime, SNUtil.diDLL);
            if (norm.magnitude < 0.01F)
                norm = transform.position - pt;
            if (_damage.lavaDatabase.IsLava(pt, norm)) {
                MarkLavaDetected();
                //SNUtil.writeToChat("Wide lava: "+pt+" @ "+lastLavaTime);
                //SNUtil.log("Is lava", SNUtil.diDLL);
                return true;
            }

            return false;
        }

        public void MarkLavaDetected() {
            _lastLavaTime = DayNightCycle.main.timePassedAsFloat;
        }

        public void MarkGeyserDetected() {
            _lastGeyserTime = DayNightCycle.main.timePassedAsFloat;
        }

        public bool IsInGeyser() {
            return Mathf.Abs(DayNightCycle.main.timePassedAsFloat - _lastGeyserTime) <= 0.5F;
        }

        public bool IsInLava() { /*
            if (needsLavaDump) {
                dmg.lavaDatabase.LazyInitialize();
                needsLavaDump = false;
                List<string> li = new List<string>();
                Dictionary<byte, List<BlockTypeClassification>> db = dmg.lavaDatabase.lavaBlocks;
                foreach (KeyValuePair<byte, List<BlockTypeClassification>> kvp in db) {
                    List<BlockTypeClassification> li0 = kvp.Value;
                    li.Add("==========================");
                    li.Add("Byte "+kvp.Key+": "+li0.Count+" entries: ");
                    foreach (BlockTypeClassification bb in li0) {
                        li.Add("Type "+bb.blockType+", inclination ["+bb.minInclination+"-"+bb.maxInclination+"], mat='"+bb.material+"'");
                    }
                    li.Add("==========================");
                    li.Add("");
                }
                string path = "E:/INet/SNlavadump.txt";
                File.WriteAllLines(path, li);
            }*/
            return Mathf.Abs(DayNightCycle.main.timePassedAsFloat - _lastLavaTime) <= 2;
        }
    }

    public static void OnStoryGoalCompleted(string key) {
        StoryHandler.instance.NotifyGoalComplete(key);
    }

    public static bool IsItemUsable(TechType tt) {
        return tt == TechType.Bladderfish || UsableItemRegistry.instance.isUsable(tt);
    }

    public static bool UseItem(Survival s, GameObject useObj) {
        var flag = false;
        if (useObj != null) {
            var tt = CraftData.GetTechType(useObj);
            if (tt == TechType.None) {
                var component = useObj.GetComponent<Pickupable>();
                if (component)
                    tt = component.GetTechType();
            }

            SNUtil.log("Player used item " + tt, SNUtil.diDLL);
            flag = UsableItemRegistry.instance.use(tt, s, useObj);
            if (flag)
                FMODUWE.PlayOneShot(TechData.GetSoundUse(tt), Player.main.transform.position, 1f);
        }

        return flag;
    }

    public static bool IsItemDroppable(Pickupable pp, bool notify) {
        var flag = Inventory.CanDropItemHere(pp, notify);
        if (pp) {
            var data =
                IrreplaceableItemRegistry.instance.getEffects(pp.GetTechType());
            if (data != null)
                return data.onAttemptToDrop.Invoke(pp, notify);
        }

        if (pp && DroppabilityEvent != null) {
            var dropCheck = new DroppabilityCheck(pp, notify, flag);
            DroppabilityEvent.Invoke(dropCheck);
            flag = dropCheck.Allow;
            if (notify && !flag && !string.IsNullOrEmpty(dropCheck.Error)) {
                ErrorMessage.AddError(dropCheck.Error);
            }
        }

        return flag;
    }

    public static void OnScanComplete(PDAScanner.EntryData data) {
        if (data != null) {
            TechnologyUnlockSystem.instance.triggerDirectUnlock(data.key);
            TechUnlockTracker.instance.onScan(data);
            ScanCompleteEvent?.Invoke(data);
        }
    }

    public static void TickLaserCutting(Sealed s, float amt) {
        if (s._sealed && s.maxOpenedAmount >= 0) {
            string key = null;
            if (s.GetComponent<BulkheadDoor>() && BulkheadLaserHoverEvent != null) {
                var ch = new BulkheadLaserCutterHoverCheck(s);
                BulkheadLaserHoverEvent.Invoke(ch);
                key = ch.RefusalLocaleKey;
            }

            if (string.IsNullOrEmpty(key)) {
                s.openedAmount = Mathf.Min(s.openedAmount + amt, s.maxOpenedAmount);
                if (Mathf.Approximately(s.openedAmount, s.maxOpenedAmount)) {
                    s._sealed = false;
                    s.openedEvent.Trigger(s);
                    //Debug.Log("Trigger opened event");
                }
            }
        }
    }

    public static void GetBulkheadMouseoverText(BulkheadDoor bk) {
        if (bk.enabled) {
            var s = bk.GetComponent<Sealed>();
            if (s && s.IsSealed()) {
                if (s.maxOpenedAmount < 0) {
                    HandReticle.main.SetText(HandReticle.TextType.Use, "BulkheadInoperable", true);
                    HandReticle.main.SetIcon(HandReticle.IconType.None, 1f);
                } else {
                    string key = null;
                    if (BulkheadLaserHoverEvent != null) {
                        var ch = new BulkheadLaserCutterHoverCheck(s);
                        BulkheadLaserHoverEvent.Invoke(ch);
                        key = ch.RefusalLocaleKey;
                        HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                    }

                    if (string.IsNullOrEmpty(key)) {
                        HandReticle.main.SetText(
                            HandReticle.TextType.Use,
                            "SealedInstructions",
                            true
                        ); //is a locale key
                        HandReticle.main.SetProgress(s.GetSealedPercentNormalized());
                        HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
                    } else {
                        HandReticle.main.SetText(HandReticle.TextType.Use, key, true);
                        HandReticle.main.SetIcon(HandReticle.IconType.None, 1f);
                    }
                }
            } else {
                HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                HandReticle.main.SetText(HandReticle.TextType.Use, bk.opened ? "Close" : "Open", true);
            }
        }
    }

    public static void OnBulkheadClick(BulkheadDoor bk) {
        var componentInParent = bk.GetComponentInParent<Base>();
        //PreventDeconstruction prev = bk.GetComponentInParent<PreventDeconstruction>();
        var s = bk.GetComponent<Sealed>();
        if (s != null && s.IsSealed()) {
        } else if (componentInParent != null && componentInParent.isGhost) {
            bk.SetState(!bk.opened);
        } else if (bk.enabled) {
            if (GameOptions.GetVrAnimationMode()) {
                bk.SetState(!bk.opened);
                return;
            }

            bk.OnPlayerCinematicModeEnd();
        }
    }

    public static bool IsInsideForHatch(UseableDiveHatch hatch) {
        var wb =
            hatch.gameObject.GetComponent<SeabaseReconstruction.WorldgenBaseWaterparkHatch>();
        return wb ? wb.isPlayerInside() : Player.main.IsInsideWalkable() && Player.main.currentWaterPark == null;
    }

    public static void OnConstructionComplete(Constructable c, bool finished) {
        if (finished) {
            TechnologyUnlockSystem.instance.triggerDirectUnlock(c.techType);
            FirstObtainmentSystem.instance.onPickup(c.techType);
        }

        var lgc = c.GetComponent<CustomMachineLogic>();
        if (lgc)
            lgc.onConstructedChanged(finished);

        OnConstructedEvent?.Invoke(c, finished);
    }

    public static void OnVehicleBayFinish(Constructor c, GameObject go) {
        var tt = CraftData.GetTechType(go);
        if (tt != TechType.None)
            TechnologyUnlockSystem.instance.triggerDirectUnlock(tt);
    }

    public static void OnBaseLoaded(BaseRoot root) {
        OnBaseLoadedEvent?.Invoke(root);
    }

    public static void OnInvOpened(StorageContainer sc) {
        InventoryOpenedEvent?.Invoke(sc);
    }

    public static void OnInvClosed(StorageContainer sc) {
        InventoryClosedEvent?.Invoke(sc);
    }

    public static void OnKnifed(GameObject go) {
        if (go && OnKnifedEvent != null)
            OnKnifedEvent.Invoke(go);
        if (!go || Inventory.main.GetHeld().GetTechType() !=
            TechType.HeatBlade) return; //allow thermoblade to cook dead fish
        var tt = CraftData.GetTechType(go);
        if (tt == TechType.None || TechData.GetProcessed(tt) == null) return;
        var lv = go.GetComponent<LiveMixin>();
        if (!lv || lv.IsAlive()) return;
        var put = ObjectUtil.createWorldObject(TechData.GetProcessed(tt));
        if (!put) return;
        put.transform.position = go.transform.position;
        put.transform.rotation = go.transform.rotation;
        put.transform.localScale = go.transform.localScale;
        go.destroy(false);
    }

    public static bool IsObjectKnifeable(LiveMixin lv) {
        if (!lv)
            return true;
        var k = new KnifeAttempt(lv, !lv.weldable && lv.knifeable && !lv.GetComponent<EscapePod>());
        KnifeAttemptEvent?.Invoke(k);
        return k.AllowKnife;
    }

    public static bool CanGravTrapGrab(Gravsphere s, GameObject go) {
        if (!s || !go)
            return false;

        var pp = go.GetComponent<Pickupable>();
        var def = (!pp || !pp.attached) && !go.GetComponent<WaterParkItem>() &&
                  (GravTrapTechSet.Contains(CraftData.GetTechType(go)) || (bool)go.GetComponent<Creature>());

        var k = new GravTrapGrabAttempt(s, go, def);
        GravTrapAttemptEvent?.Invoke(k);
        //SNUtil.writeToChat("Gravsphre "+s+" tried to grab "+go+": "+def+" > "+k.allowGrab);
        return k.AllowGrab;
    }

    public static void HoverSeamothTorpedoStorage(SeaMoth sm, HandTargetEventData data) {
        for (var i = 0; i < sm.slotIDs.Length; i++) {
            var ii = sm.GetSlotItem(i);
            if (ii != null && ii.item) {
                var storage = SeamothModule.getStorageHandler(ii.item.GetTechType());
                if (storage != null && storage.storageType == SeamothModule.StorageAccessType.TORPEDO) {
                    var component = ii.item.GetComponent<SeamothStorageContainer>();
                    //SNUtil.writeToChat("Found "+component+" ["+storage.title+"] for "+ii.item.GetTechType());
                    if (component) {
                        HandReticle.main.SetText(HandReticle.TextType.Use, storage.localeKey, true);
                        HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                    }
                }
            }
        }
    }

    public static void OpenSeamothTorpedoStorage(SeaMoth sm, Transform transf) {
        var foundMatch = TechType.None;
        Inventory.main.ClearUsedStorage();
        for (var i = 0; i < sm.slotIDs.Length; i++) {
            var ii = sm.GetSlotItem(i);
            if (ii != null && ii.item) {
                var tt = ii.item.GetTechType();
                if (foundMatch == tt || foundMatch == TechType.None) {
                    var storage = SeamothModule.getStorageHandler(tt);
                    if (storage != null && storage.storageType == SeamothModule.StorageAccessType.TORPEDO) {
                        var component = ii.item.GetComponent<SeamothStorageContainer>();
                        if (component) {
                            foundMatch = tt;
                            storage.apply(component);
                            Inventory.main.SetUsedStorage(component.container, true);
                        }
                    }
                }
            }
        }

        if (foundMatch != TechType.None) {
            //SNUtil.writeToChat("Opening "+SeamothModule.getStorageHandler(foundMatch).title+" for "+foundMatch);
            Player.main.GetPDA().Open(PDATab.Inventory, transf, null);
        }
    }

    public static ItemsContainer GetVehicleStorageInSlot(Vehicle sm, int slotID, TechType seek) {
        var slotItem = sm.GetSlotItem(slotID);
        if (slotItem == null)
            return null;
        var item = slotItem.item;
        if (!item)
            return null;
        var tt = item.GetTechType();
        if (tt == seek) {
            var ssc = item.GetComponent<SeamothStorageContainer>();
            return ssc ? ssc.container : null;
        }

        if (sm is SeaMoth) {
            var storage = SeamothModule.getStorageHandler(tt);
            if (storage != null && storage.storageType == SeamothModule.StorageAccessType.BOX) {
                var ssc = item.GetComponent<SeamothStorageContainer>();
                if (ssc) {
                    storage.apply(ssc);
                    return ssc.container;
                }
            }
        }

        return null;
    }

    public static void UpdateSeamothModules(SeaMoth sm, int slotID, TechType techType, bool added) {
        if (added) {
            var storage = SeamothModule.getStorageHandler(techType);
            if (storage != null) {
                if (storage.storageType == SeamothModule.StorageAccessType.TORPEDO) {
                    if (sm.torpedoSilos != null && slotID < sm.torpedoSilos.Length)
                        sm.torpedoSilos[slotID].SetActive(true);
                } else if (storage.storageType == SeamothModule.StorageAccessType.BOX) {
                    if (sm.storageInputs != null && slotID < sm.storageInputs.Length)
                        sm.storageInputs[slotID].SetEnabled(true);
                }
            }
        }

        for (var i = 0; i < sm.slotIDs.Length; i++) {
            var slot = sm.slotIDs[i];
            var techTypeInSlot = sm.modules.GetTechTypeInSlot(slot);
            if (techTypeInSlot != TechType.None) {
                var sp = ItemRegistry.instance.getItem(techTypeInSlot, false);
                if (sp is SeamothDepthModule module) {
                    sm.crushDamage.SetExtraCrushDepth(
                        Mathf.Max(module.depthBonus, sm.crushDamage.extraCrushDepth)
                    );
                }
            }
        }

        OnSeamothModulesChangedEvent?.Invoke(sm, slotID, techType, added);
    }

    public static void UpdateCyclopsModules(SubRoot sm) {
        if (OnCyclopsModulesChangedEvent != null && !sm.isBase)
            OnCyclopsModulesChangedEvent.Invoke(sm);
    }

    public static void UpdatePrawnModules(Exosuit sm, int slotID, TechType techType, bool added) {
        OnPrawnModulesChangedEvent?.Invoke(sm, slotID, techType, added);
    }

    public static void UseSeamothModule(SeaMoth sm, TechType techType, int slotID) {
        var sp = ItemRegistry.instance.getItem(techType, false);
        if (sp is SeamothModule smm) {
            smm.onFired(sm, slotID, sm.GetSlotCharge(slotID));
            sm.quickSlotTimeUsed[slotID] = Time.time;
            sm.quickSlotCooldown[slotID] = smm.getUsageCooldown();
        }

        OnSeamothModuleUsedEvent?.Invoke(sm, techType, slotID);
    }

    public static float GetTemperatureForDamage(TemperatureDamage dmg) {
        if (Mathf.Abs(Time.time - dmg.timeDamageStarted) <= 2.5F) { //active lava dmg
            //SNUtil.writeToChat(dmg+" Touch lava: "+dmg.timeDamageStarted+" > "+Mathf.Abs(Time.time-dmg.timeDamageStarted));
            return 1200;
        }

        var warn = dmg.GetComponentInChildren<LavaWarningTriggerDetector>();
        if (warn && warn.IsInLava())
            return dmg.gameObject.FindAncestor<Exosuit>() ? 300 : 400;
        if (warn && warn.IsInGeyser())
            return 180;
        var v = dmg.GetComponent<Vehicle>();
        return v
            ? v.precursorOutOfWater ? 25 : v.GetTemperature()
            : WaterTemperatureSimulation.main.GetTemperature(dmg.transform.position);
    }

    public static void PingSonar(SNCameraRoot cam) {
        if (cam && OnSonarUsedEvent != null)
            OnSonarUsedEvent.Invoke(cam);
    }

    public static void PingSeamothSonar(SeaMoth cam) {
        if (cam && OnSeamothSonarUsedEvent != null)
            OnSeamothSonarUsedEvent.Invoke(cam);
    }

    public static void PingCyclopsSonar(CyclopsSonarButton cam) {
        if (cam && OnCyclopsSonarUsedEvent != null) {
            var sb = cam.gameObject.FindAncestor<SubRoot>();
            if (sb)
                OnCyclopsSonarUsedEvent.Invoke(sb);
        }
    }

    public static void OnEggHatched(GameObject hatched) {
        if (hatched) {
            hatched.fullyEnable();
            OnEggHatchedEvent?.Invoke(hatched);
        }
    }

    public static void OnEmpHit(EMPBlast e, MonoBehaviour com) {
        if (com && OnEmpHitEvent != null) {
            OnEmpHitEvent.Invoke(e, com.gameObject);
        }
    }

    public static void OnEmpTouch(EMPBlast e, Collider c) {
        if (c && OnEmpTouchEvent != null) {
            OnEmpTouchEvent.Invoke(e, c);
        }
    }

    public static void AppendItemTooltip(StringBuilder sb, TechType tt, GameObject obj) {
        var mix = obj.GetComponent<InfectedMixin>();
        if (mix) {
            var tip = GetInfectionTooltip(mix);
            if (!string.IsNullOrEmpty(tip))
                TooltipFactory.WriteDescription(
                    sb,
                    tip
                ); //TooltipFactory.WriteDescription(sb, "Infected: "+((int)(mix.infectedAmount*100))+"%");
        }

        var peep = obj.GetComponent<Peeper>();
        if (peep && peep.isHero) {
            TooltipFactory.WriteDescription(sb, "Contains unusual enzymes.");
        }

        ItemTooltipEvent?.Invoke(sb, tt, obj);

        var e = SpawnedItemTracker.instance.getSpawnEvent(obj);
        if (e != null)
            TooltipFactory.WriteDescription(sb, e.tooltip);
    }

    private static string GetInfectionTooltip(InfectedMixin mix) {
        if (mix.IsInfected()) {
            var amt = mix.infectedAmount;
            //return "Infected: "+((int)(amt*100))+"%";
            return amt >= 0.75
                ? "This creature is severely infected."
                : amt >= 0.5
                    ? "Exhibiting symptoms of significant systemic infection."
                    : amt >= 0.25
                        ? "Showing signs of infection."
                        : "Elevated bacterial levels detected.";
        } else {
            var lv = mix.GetComponent<LiveMixin>();
            return !lv || lv.IsAlive() ? "Status: Healthy." : null;
        }
    }
    /*
    public static WaterscapeVolume.Settings currentRenderVolume = new WaterscapeVolume.Settings();

    public static void overrideFog(WaterBiomeManager biomes, Vector3 pos, bool detail, WaterscapeVolume.Settings settings) {
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            currentRenderVolume.copyObject(settings);
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            settings = currentRenderVolume;
        }
        biomes.atmosphereVolumeMaterial.SetVector(ShaderPropertyID._Value, biomes.GetExtinctionTextureValue(settings));
        biomes.atmosphereVolumeMaterial.SetVector(ShaderPropertyID._Value1, biomes.GetScatteringTextureValue(settings));
        biomes.atmosphereVolumeMaterial.SetVector(ShaderPropertyID._Value2, biomes.GetEmissiveTextureValue(settings));
    }

    public static void onFogRasterized(WaterBiomeManager biomes, Vector3 pos, bool detail) {
        SNUtil.writeToChat("Rasterizing fog @ "+pos);
    }*/

    public static Vector4 InterceptExtinction(Vector4 orig, WaterscapeVolume.Settings settings) {
        var at = BiomeBase.GetBiome(Camera.main.transform.position);
        if (at is CustomBiome b) {
            var d = b.getMurkiness(settings.murkiness) / 100f;
            var scatter = b.getScatteringFactor(settings.scattering);
            var vector = b.getColorFalloff(settings.absorption) + scatter * Vector3.one;
            var ret = new Vector4(vector.x, vector.y, vector.z, scatter) * d;
            ret.w = b.getFogStart(settings.startDistance);
            return ret;
        }

        return orig;
    }

    public static Vector4 InterceptScattering(Vector4 orig, WaterscapeVolume.Settings settings) {
        var at = BiomeBase.GetBiome(Camera.main.transform.position);
        if (at is CustomBiome b) {
            var factor = b.getScatterFactor(settings.GetExtinctionAndScatteringCoefficients().w);
            var linear = b.getWaterColor(settings.scatteringColor.linear);
            Vector4 result;
            result.x = linear.r * factor;
            result.y = linear.g * factor;
            result.z = linear.b * factor;
            result.w = b.getSunScale(settings.sunlightScale) * WaterBiomeManager.main.waterTransmission;
            return result;
        }

        return orig;
    }

    public static Vector4 InterceptEmissive(Vector4 orig, WaterscapeVolume.Settings settings) {
        var at = BiomeBase.GetBiome(Camera.main.transform.position);
        return at is CustomBiome biome ? biome.getEmissiveVector(orig) : orig;
    }

    public static void RecomputeFog() {
        SNUtil.log("Recomputing fog @ " + Camera.main.transform.position, SNUtil.diDLL);
        WaterBiomeManager.main.Rebuild();
        WaterBiomeManager.main.BuildSettingsTextures();
    }

    public static void DumpWaterscapeTextures() {
        var wbm = WaterBiomeManager.main;
        //string biome = wbm.GetBiome(Camera.main.transform.position);
        //SNUtil.writeToChat("Dumping biome textures @ "+biome);
        foreach (var f in typeof(WaterBiomeManager).GetFields((BindingFlags)0x7fffffff)) {
            var get = f.GetValue(wbm);
            if (get is RenderTexture texture) {
                SNUtil.writeToChat("Dumping RenderTexture WaterBiomeManager::" + f.Name);
                RenderUtil.dumpTexture(SNUtil.diDLL, f.Name, texture);
            } else if (get is Texture2D texture2D) {
                SNUtil.writeToChat("Dumping Texture2D WaterBiomeManager::" + f.Name);
                RenderUtil.dumpTexture(SNUtil.diDLL, f.Name, texture2D);
            } else {
                //SNUtil.writeToChat("Skipping non-texture object "+get);
            }
        }
    }
    /*
    public static void interceptChosenFog(WaterscapeVolume vol, Camera cam) {
        vol.SetupWaterPlane(cam, vol.waterPlane);
        vol.biomeManager.SetupConstantsForCamera(cam);
        if (vol.fogEnabled)
            Shader.SetGlobalFloat(ShaderPropertyID._UweFogEnabled, 1f);
        float transmission = vol.GetTransmission();
        Shader.SetGlobalFloat(ShaderPropertyID._UweCausticsScale, vol.causticsScale * vol.surface.GetCausticsWorldToTextureScale());
        Shader.SetGlobalVector(ShaderPropertyID._UweCausticsAmount, new Vector3(vol.causticsAmount, vol.surface.GetCausticsTextureScale() * vol.causticsAmount, vol.surface.GetCausticsTextureScale()));
        Shader.SetGlobalFloat(ShaderPropertyID._UweWaterTransmission, transmission);
        Shader.SetGlobalFloat(ShaderPropertyID._UweWaterEmissionAmbientScale, vol.emissionAmbientScale);
        float depth = (cam.transform.position.y - vol.aboveWaterMinHeight) / (vol.aboveWaterMaxHeight - vol.aboveWaterMinHeight);
        float fogDensity = Mathf.Lerp(1f, vol.aboveWaterDensityScale, depth);
        Shader.SetGlobalFloat(ShaderPropertyID._UweExtinctionAndScatteringScale, fogDensity);
        if (vol.sky != null) {
            Vector3 lightDirection = vol.sky.GetLightDirection();
        if (Input.GetKeyDown(KeyCode.LeftAlt))
            lightDirection.x = 1;
        if (Input.GetKeyDown(KeyCode.LeftControl))
            lightDirection.y = 1;
        if (Input.GetKeyDown(KeyCode.LeftShift))
            lightDirection.z = 1;
        if (Input.GetKeyDown(KeyCode.Tab))
            SNUtil.writeToChat(lightDirection.ToString());
            lightDirection.y = Mathf.Min(lightDirection.y, -0.01f);
            Vector3 camLight = -cam.worldToCameraMatrix.MultiplyVector(lightDirection);
            Color lightColor = vol.sky.GetLightColor();
            Vector4 fogValues = lightColor;
            fogValues.w = vol.sunLightAmount * transmission;
            float brightness = lightColor.r * 0.3f + lightColor.g * 0.59f + lightColor.b * 0.11f;
            Shader.SetGlobalVector(ShaderPropertyID._UweFogVsLightDirection, camLight);
            Shader.SetGlobalVector(ShaderPropertyID._UweFogWsLightDirection, lightDirection);
            Shader.SetGlobalVector(ShaderPropertyID._UweFogLightColor, fogValues);
            Shader.SetGlobalFloat(ShaderPropertyID._UweFogLightGreyscaleColor, brightness);
        }
        else {
            Shader.SetGlobalFloat(ShaderPropertyID._UweFogLightAmount, 0f);
        }
        Shader.SetGlobalVector(ShaderPropertyID._UweColorCastFactor, new Vector2(vol.colorCastDistanceFactor, vol.colorCastDepthFactor));
        Shader.SetGlobalFloat(ShaderPropertyID._UweAboveWaterFogStartDistance, vol.aboveWaterStartDistance);
        Vector3 scatter = default(Vector3);
        scatter.x = (1f - vol.scatteringPhase * vol.scatteringPhase) / 12.566371f;
        scatter.y = 1f + vol.scatteringPhase * vol.scatteringPhase;
        scatter.z = 2f * vol.scatteringPhase;
        Shader.SetGlobalVector(ShaderPropertyID._UweFogMiePhaseConst, scatter);
        Shader.SetGlobalFloat(ShaderPropertyID._UweSunAttenuationFactor, vol.sunAttenuation);
    }*/
    /*
    public static void interceptChosenFog(WaterscapeVolume vol, Camera cam) {
        if (!vol || !cam)
            return;
        float t = (cam.transform.position.y - vol.aboveWaterMinHeight) / (vol.aboveWaterMaxHeight - vol.aboveWaterMinHeight);
        float fogDensity = Mathf.Lerp(1f, vol.aboveWaterDensityScale, t);

        Vector4 fogColor = default(Vector4);
        if (vol.sky != null)
        {
            Vector3 lightDirection = vol.sky.GetLightDirection();
            lightDirection.y = Mathf.Min(lightDirection.y, -0.01f);
            Vector3 v = -cam.worldToCameraMatrix.MultiplyVector(lightDirection);
            fogColor = vol.sky.GetLightColor();
            fogColor.w = vol.sunLightAmount * vol.GetTransmission();
            Shader.SetGlobalVector(ShaderPropertyID._UweFogVsLightDirection, v);
            Shader.SetGlobalVector(ShaderPropertyID._UweFogWsLightDirection, lightDirection);
        }/*
        CustomBiome b = BiomeBase.getBiome(cam.transform.position) as CustomBiome;
        if (b != null) {
            fogColor = fogColor.setXYZ(b.getFogColor(fogColor.getXYZ()));
            fogColor.w = b.getSunIntensity(fogColor.w);
            fogDensity = b.getFogDensity(fogDensity);
        }
        WaterFogValues wf = new WaterFogValues(fogColor.asColor(), fogDensity, fogColor.w);
        if (fogCalculateEvent != null)
            fogCalculateEvent.Invoke(wf);
        Vector4 vec4 = wf.color.toVectorA();
        vec4.w = wf.sunValue;*/
    /*Vector4 vec4 = fogColor;
                //SNUtil.writeToChat("Fog color "+vec4+", with density "+fogDensity.ToString("0.000"));
                Shader.SetGlobalVector(ShaderPropertyID._UweFogLightColor, vec4);
                Shader.SetGlobalFloat(ShaderPropertyID._UweExtinctionAndScatteringScale, /*wf.density*/
    /*fogDensity);
                float value3 = fogColor.x * 0.3f + fogColor.y * 0.59f + fogColor.z * 0.11f;
                Shader.SetGlobalFloat(ShaderPropertyID._UweFogLightGreyscaleColor, value3);
                Vector3 v2 = default(Vector3);
                v2.x = (1f - vol.scatteringPhase * vol.scatteringPhase) / 12.566371f;
                v2.y = 1f + vol.scatteringPhase * vol.scatteringPhase;
                v2.z = 2f * vol.scatteringPhase;
                Shader.SetGlobalVector(ShaderPropertyID._UweFogMiePhaseConst, v2);
                Shader.SetGlobalFloat(ShaderPropertyID._UweSunAttenuationFactor, vol.sunAttenuation);
                Shader.SetGlobalVector(ShaderPropertyID._UweColorCastFactor, new Vector2(vol.colorCastDistanceFactor, vol.colorCastDepthFactor));
                Shader.SetGlobalFloat(ShaderPropertyID._UweAboveWaterFogStartDistance, vol.aboveWaterStartDistance);
                //SNUtil.writeToChat("Applying fog of "+vol+" @ "+vol.transform.position);
            }*/

    public static bool InterceptConstructability( /*Collider c*/) {
        var orig = Builder.UpdateAllowed();
        //SNUtil.writeToChat("Testing constructability of "+Builder.constructableTechType+", default value = "+orig);
        if (ConstructabilityEvent != null) {
            //SNUtil.writeToChat("Event has listeners");
            var aimTransform = Builder.GetAimTransform();
            var target = Physics.Raycast(
                aimTransform.position,
                aimTransform.forward,
                out var hit,
                Builder.placeMaxDistance,
                Builder.placeLayerMask.value,
                QueryTriggerInteraction.Ignore
            )
                ? hit.collider
                : null;
            //SNUtil.writeToChat("Placement target: "+target+" "+(target == null ? "" : target.gameObject.GetFullHierarchyPath()));
            //SNUtil.writeToChat("Space check: "+Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds, Builder.placeLayerMask.value, target));
            var deal = new BuildabilityCheck(orig, target);
            ConstructabilityEvent.Invoke(deal);
            //SNUtil.writeToChat("Event state: "+deal.placeable+" / "+deal.ignoreSpaceRequirements);
            return
                deal.Placeable; // && (target == null || deal.ignoreSpaceRequirements || Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds, Builder.placeLayerMask.value, target));
        }

        return orig;
    }
    /*
    public static float getPowerRelayCapacity(float orig, PowerRelay relay) {
        SubRoot sub = relay.gameObject.FindAncestor<SubRoot>();
        if (sub) {
            foreach (CustomMachineLogic lgc in sub.GetComponentsInChildren<CustomMachineLogic>()) {
                orig += lgc.getBaseEnergyStorageCapacityBonus();
            }
        }
        return orig;
    }*/
    /*

            public static void addPowerToSeabaseDelegateViaPowerSourceSet(PowerSource src, float amt, MonoBehaviour component) {
                SubRoot sub = component.gameObject.FindAncestor<SubRoot>();
                if (sub) {
                    sub.powerRelay.AddEnergy(amount, out stored);
                }
                else {
                    src.power = amt;
                }
            }*/

    public static void UpdateSolarPanel(SolarPanel p) {
        if (!p)
            return;
        var c = p.gameObject.GetComponent<Constructable>();
        if (c && c.constructed) {
            var eff = p.GetRechargeScalar();
            if (SolarEfficiencyEvent != null) {
                var ch = new SolarEfficiencyCheck(p, eff);
                SolarEfficiencyEvent.Invoke(ch);
                eff = ch.Value;
            }

            var gen = eff * DayNightCycle.main.deltaTime * 0.25f * 5f;
            var sub = p.gameObject.FindAncestor<SubRoot>();
            //SNUtil.writeToChat("Solar panel adding "+gen.ToString("0.0000")+" energy to "+(sub ? sub.name+" ("+sub.powerRelay.internalPowerSource + "/"+sub.powerRelay.inboundPowerSources.toDebugString()+")" : "self"));
            if (sub) {
                sub.powerRelay.AddEnergy(gen, out var trash);
            } else {
                p.powerSource.power = Mathf.Clamp(p.powerSource.power + gen, 0f, p.powerSource.maxPower);
            }
        }
    }

    public static bool AddPowerToSeabaseDelegate(
        IPowerInterface pi,
        float amount,
        out float stored,
        MonoBehaviour component
    ) {
        var pref = component.GetComponent<PowerSourceBaseReference>();
        var sub = pref ? pref.Sub : null;
        //SNUtil.writeToChat(component+" adding " + amount.ToString("0.0000")+" energy to "+(sub ? sub.name+" ("+sub.powerRelay.internalPowerSource + "/"+sub.powerRelay.inboundPowerSources.toDebugString()+")" : "self"));
        return sub ? sub.powerRelay.AddEnergy(amount, out stored) : pi.AddEnergy(amount, out stored);
    }

    public static void LinkPowerRelayToBase(PowerRelay pr, IPowerInterface ipf) {
        if (ipf is MonoBehaviour mb) {
            var pbr = mb.gameObject.EnsureComponent<PowerSourceBaseReference>();
            pbr.Target = pr;
            if (pr is BasePowerRelay bpr)
                pbr.Sub = bpr.subRoot;
        }
    }


    internal class PowerSourceBaseReference : MonoBehaviour {
        internal SubRoot Sub;
        internal PowerRelay Target;

        private void Update() {
            if (!Sub && Target) {
                var pref = Target.GetComponent<PowerSourceBaseReference>();
                if (pref)
                    Sub = pref.Sub;
            }
        }
    }

    /*
    public static string getBiomeToUseForMusic(string biome, MusicManager mgr) {
        if (musicBiomeChoiceEvent != null) {
            MusicSelectionCheck mus = new MusicSelectionCheck(biome);
        }
        return biome;
    }*/

    public static void ClickStoryHandTarget(StoryHandTarget tgt) {
        if (!tgt.enabled || !tgt.isValidHandTarget)
            return;
        var goal = tgt.goal;
        if (StoryHandEvent != null) {
            var deal = new StoryHandCheck(goal, tgt);
            StoryHandEvent.Invoke(deal);
            if (!deal.Usable)
                return;
            goal = deal.Goal;
        }

        goal.Trigger();
        if (tgt.informGameObject)
            tgt.informGameObject.SendMessage("OnStoryHandTarget", SendMessageOptions.DontRequireReceiver);
        tgt.destroyGameObject.destroy(false);
    }

    public static float GetRadiationLevel(Player p, float orig) {
        var ret = orig;
        ret = Mathf.Max(ret, p.GetComponent<AoERadiationTracker>().getRadiationIntensity());
        //SNUtil.writeToChat("Rad "+ret.ToString());
        //SNUtil.writeToChat((radiationCheckEvent != null)+" # "+orig);
        if (RadiationCheckEvent != null) {
            var ch = new RadiationCheck(p.transform.position, ret);
            RadiationCheckEvent.Invoke(ch);
            ret = ch.Value;
        }

        //SNUtil.writeToChat("PRad "+ret.ToString());
        return ret;
    }

    public static void OnReaperGrabVehicle(ReaperLeviathan r, Vehicle v) {
        StoryGoal.Execute("ReaperGrab", Story.GoalType.Story);
        ReaperGrabVehicleEvent?.Invoke(r, v);
    }

    public static void OnCyclopsDamaged(SubRoot r, DamageInfo d) {
        CyclopsDamageEvent?.Invoke(r, d);
    }

    public static void OnDockingTriggerCollided(VehicleDockingBay v, Collider other) {
        if (other.isTrigger)
            return;
        if (v.GetDockedVehicle())
            return;
        if (GameModeUtils.RequiresPower() && !v.IsPowered())
            return;
        if (v.interpolatingVehicle != null)
            return;
        var componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Vehicle>(other.gameObject);
        if (componentInHierarchy == null || componentInHierarchy.docked || componentInHierarchy.GetRecentlyUndocked())
            return;
        v.timeDockingStarted = Time.time;
        v.interpolatingVehicle = componentInHierarchy;
        v.startPosition = v.interpolatingVehicle.transform.position;
        v.startRotation = v.interpolatingVehicle.transform.rotation;
    }

    public static void OnAcidTriggerCollided(AcidicBrineDamageTrigger v, Collider other) {
        if (other.isTrigger)
            return;
        var liveMixin = v.GetLiveMixin(other.gameObject);
        if (v.IsValidTarget(liveMixin)) {
            v.AddTarget(liveMixin);
        }
    }

    public static void OnAirlockTouched(PrecursorDoorMotorModeSetter door, Collider col) {
        if (col.isTrigger)
            return;
        if (door.setToMotorModeOnEnter == PrecursorDoorMotorMode.None)
            return;
        if (col.gameObject != null && col.gameObject.GetComponentInChildren<IgnoreTrigger>() != null)
            return;
        var gameObject = UWE.Utils.GetEntityRoot(col.gameObject);
        if (!gameObject)
            gameObject = col.gameObject;
        var componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Player>(gameObject);
        if (componentInHierarchy) {
            var precursorDoorMotorMode = door.setToMotorModeOnEnter;
            if (precursorDoorMotorMode != PrecursorDoorMotorMode.Auto) {
                if (precursorDoorMotorMode == PrecursorDoorMotorMode.ForceWalk) {
                    componentInHierarchy.precursorOutOfWater = true;
                }
            } else {
                componentInHierarchy.precursorOutOfWater = false;
            }
        }

        var componentInHierarchy2 = UWE.Utils.GetComponentInHierarchy<Exosuit>(gameObject);
        if (componentInHierarchy2) {
            var precursorDoorMotorMode = door.setToMotorModeOnEnter;
            if (precursorDoorMotorMode == PrecursorDoorMotorMode.Auto) {
                componentInHierarchy2.precursorOutOfWater = false;
                return;
            }

            if (precursorDoorMotorMode != PrecursorDoorMotorMode.ForceWalk) {
                return;
            }

            componentInHierarchy2.precursorOutOfWater = true;
        }

        var componentInHierarchy3 = UWE.Utils.GetComponentInHierarchy<SeaMoth>(gameObject);
        if (componentInHierarchy3) {
            var precursorDoorMotorMode = door.setToMotorModeOnEnter;
            if (precursorDoorMotorMode == PrecursorDoorMotorMode.Auto) {
                componentInHierarchy3.precursorOutOfWater = false;
                return;
            }

            if (precursorDoorMotorMode != PrecursorDoorMotorMode.ForceWalk) {
                return;
            }

            componentInHierarchy3.precursorOutOfWater = true;
            componentInHierarchy3.GetComponent<Rigidbody>().useGravity = true;
        }
    }
    /*
    public static Vector2int getItemDisplaySize(TechType tt, InventoryItem ii) {
        return getItemDisplaySize(tt, ii, ii.container);
    }

    public static Vector2int getItemDisplaySize(InventoryItem ii, TechType tt) {
        return getItemDisplaySize(tt, ii, ii.container);
    }

    public static Vector2int getItemDisplaySize(TechType tt, InventoryItem ii, IItemsContainer con) {
        if (ii != null && ii.item != null && ii.item && ii.item.gameObject != null && ii.item.gameObject) {
            //SNUtil.writeToChat((con != null ? con.label : "nocontainer")+" for "+tt+" in "+ii.item.gameObject.FindAncestor<Constructable>());
            BasicCustomPlant plant = BasicCustomPlant.getPlant(tt);
            if (plant != null && ii.item.gameObject.FindAncestor<Planter>()) {
                return plant.getSize() == Plantable.PlantSize.Large ? new Vector2int(2, 2) : new Vector2int(1, 1);
            }
        }
        return CraftData.GetItemSize(tt);
    }*/

    public static void OnFModEmitterPlay(FMOD_CustomEmitter snd) {
        OnSoundPlayedEvent?.Invoke(snd);
    }

    public static float GetMaxPropulsible(float orig, GameObject go, MonoBehaviour gun, bool isMass) {
        //SNUtil.writeToChat("Testing "+gun.gameObject.GetFullHierarchyPath()+" grab of "+go.GetFullHierarchyPath());
        if (go.FindAncestor<Constructable>() || go.FindAncestor<SubRoot>() ||
            gun.gameObject.FindAncestor<Vehicle>() == go)
            return -1;
        if (go.isAnchorPod()) {
            if (gun is RepulsionCannon rep)
                go.SendMessage("OnRepulsionHit", SendMessageOptions.DontRequireReceiver);
            return -1;
        }

        var val = orig;
        if (PropulsibilityEvent != null) {
            var e = new PropulsibilityCheck(go, val, gun, isMass);
            PropulsibilityEvent.Invoke(e);
            val = e.Value;
        }

        if (go.GetComponentInChildren<Vehicle>() || go.GetComponentInChildren<AlwaysPropulsible>())
            val = 999999999F;
        //Bounds aabb = go.GetComponent<FixedBounds>() ? go.GetComponent<FixedBounds>().bounds : UWE.Utils.GetEncapsulatedAABB(go, 20);
        //SNUtil.writeToChat("Modifying ["+isMass+"] propulsibility check of "+go+": "+orig+">"+val+"; mass="+go.GetComponent<Rigidbody>().mass+", AABB="+(aabb.size.x * aabb.size.y * aabb.size.z));
        return val;
    }

    public static Vector3 GetPropulsionTargetCenter(Vector3 orig, GameObject go) {
        var v = go.GetComponentInChildren<Vehicle>();
        if (v) {
            var ret = go.transform.position;
            if (v is SeaMoth)
                ret += go.transform.forward * -1.25F + go.transform.up * -0.125F;
            return ret;
        }

        return orig;
    }

    public static Vector3 GetPropulsionMoveToPoint(Vector3 orig, PropulsionCannon gun) {
        var v = Player.main.GetVehicle();
        return v is SeaMoth && gun.gameObject.FindAncestor<Vehicle>() == v ? v.transform.position : orig;
    }

    /*
    public static void logDockingVehicle(Vehicle v, bool dock) {
        string s = "Setting vehicle "+v+": dock state (path="+v.gameObject.GetFullHierarchyPath()+")"+" - "+dock;
        SNUtil.writeToChat(s);
        SNUtil.log(s, SNUtil.diDLL);
        SNUtil.log("from trace "+Environment.StackTrace, SNUtil.diDLL);
    }*/

    public static void OnVehicleEnter(Vehicle v, Player ep) {
        if (VehicleEnterEvent != null && v && ep) {
            VehicleEnterEvent.Invoke(v, ep);
        }
    }

    /*
    public static void getCompassDepth(uGUI_DepthCompass gui, ref int depth) {
        if (depthCompassEvent != null) {
            DepthCompassCheck ch = new DepthCompassCheck(depth);
            depthCompassEvent.Invoke(ch);
            depth = ch.value;
        }
    }
    */
    public static uGUI_DepthCompass.DepthMode GetCompassDepth(uGUI_DepthCompass gui, out int depth, out int crush) {
        var ret = gui.GetDepthInfo(out depth, out crush);
        if (DepthCompassEvent != null) {
            var ch = new DepthCompassCheck(depth, crush);
            DepthCompassEvent.Invoke(ch);
            depth = ch.Value;
            crush = ch.CrushValue;
        }

        return ret;
    }

    public static void OnRespawnPre(Survival s, Player ep) {
        if (RespawnEvent != null && s && ep)
            RespawnEvent.Invoke(s, ep, false);
    }

    public static void OnRespawnPost(Survival s, Player ep) {
        if (RespawnEvent != null && s && ep)
            RespawnEvent.Invoke(s, ep, true);
    }

    public static void OnDrillableDrilled(Drillable dr, Vector3 pos, Exosuit driller) {
        //SNUtil.writeToChat("Drilling "+dr+" @ "+pos+" by "+driller);
        if (DrillableDrillTickEvent != null && dr)
            DrillableDrillTickEvent.Invoke(dr, pos, driller);
    }

    public static void OnMapRoomTick(MapRoomFunctionality map) {
        if (ScannerRoomTickEvent != null && map)
            ScannerRoomTickEvent.Invoke(map);
    }

    public static void OnItemsLost() {
        Inventory.main.container.forEach((ii) => {
                var eff =
                    IrreplaceableItemRegistry.instance.getEffects(ii.item.GetTechType());
                if (eff != null) {
                    eff.onDiedWhileHolding.Invoke(ii);
                }
            }
        );
        ItemsLostEvent?.Invoke();
    }

    public static void OnStorageContainerHover(StorageContainer sc, GUIHand hand) {
        var lgc = sc.GetComponentInParent<DiscreteOperationalMachineLogic>();
        if (lgc) {
            if (lgc.isWorking()) {
                HandReticle.main.SetProgress(lgc.getProgressScalar());
                HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
            } else {
                var err = lgc.getErrorHover();
                if (string.IsNullOrEmpty(err)) {
                    HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
                } else {
                    HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                    HandReticle.main.SetText(HandReticle.TextType.Use, err, true); //locale key
                }
            }
        }

        StorageHoverEvent?.Invoke(sc, hand);
    }

    public static float GetModuleFireCost(float cost, Vehicle v, TechType module) {
        if (ModuleFireCostEvent != null) {
            var e = new ModuleFireCostCheck(v, module, cost);
            ModuleFireCostEvent.Invoke(e);
            cost = e.Value;
        }

        return cost;
    }

    public static void OnSelfScan() {
        SelfScanEvent?.Invoke();
    }

    public static void FilterScannerRoomResourceList(uGUI_MapRoomScanner gui) {
        ScannerRoomTechTypeListingEvent?.Invoke(gui);
    }

    public static void TickWorldForces(WorldForces wf) {
        if (SkipWorldForces)
            return;
        if (wf == null || wf.gameObject == null || !wf.gameObject.activeInHierarchy || !wf.enabled) {
            //WorldForcesManager.instance.RemoveWorldForces(wf);
            //SNUtil.log("Disabling invalid WF tick in "+wf);
            return;
        }

        wf.DoFixedUpdate();
    }

    public static void UpdateSkyApplier(SkyApplier wf) {
        if (SkipSkyApplier)
            return;
        if (!wf || !wf.gameObject || !wf.transform) {
            return;
        }

        wf.UpdateSkyIfNecessary();
    }
    /*
   public static bool isRightHandDownForLightToggle(Player p) {
    return p.GetRightHandDown();
   }*/

    public static bool OnStasisFreeze(StasisSphere s, Collider c, ref Rigidbody target) {
        var m = c.gameObject.GetComponentInParent<IStasisReactant>();
        //SNUtil.writeToChat("Stasis hit "+c+": "+m);
        if (m != null && m as MonoBehaviour)
            m.OnStasisHit(s);
        target = c.GetComponentInParent<Rigidbody>();
        if (!target)
            return false;
        if (target.GetComponent<BlueprintHandTarget>())
            return false;
        if (s.targets.Contains(target))
            return true;
        var ch = new StasisEffectCheck(s, target);
        OnStasisRifleFreezeEvent?.Invoke(ch);
        var name = target.name.ToLowerInvariant();
        if (name.StartsWith("explorablewreck", StringComparison.InvariantCultureIgnoreCase))
            return false;
        if (name.Contains("biodome"))
            return false;
        if (name.Contains("door") || name.Contains("starship_wires") || name.Contains("starship_exploded_debris"))
            return false;
        if (name.Contains("life_pod") || name.Contains("lifepod"))
            return false;
        if (name.Contains("precursor") && (name.Contains("room") || name.Contains("base")))
            return false;
        if (target.gameObject.isFossilPrefab())
            return false;
        if (c.GetComponentInParent<Player>() || c.GetComponentInParent<Vehicle>())
            return false;
        if (c.GetComponentInParent<WreckHandler>())
            return false;
        if (ch.AddToTargetList)
            s.targets.Add(target);
        if (ch.ApplyKinematicChange)
            target.isKinematic = true;
        if (ch.SendMessage)
            target.SendMessage("OnFreeze", SendMessageOptions.DontRequireReceiver);
        if (ch.DoFX) {
            Utils.PlayOneShotPS(s.vfxFreeze, target.GetComponent<Transform>().position, Quaternion.identity, null);
            FMODUWE.PlayOneShot(s.soundEnter, s.tr.position, 1f);
        }

        return !target.isKinematic;
    }

    public interface IStasisReactant {
        /// <summary>
        /// this is called every frame!
        /// </summary>
        void OnStasisHit(StasisSphere s);
    }

    public static void OnStasisUnfreeze(StasisSphere s, Rigidbody target) {
        if (!target)
            return;
        if (target.GetComponent<WreckHandler>())
            return;
        var ch = new StasisEffectCheck(s, target);
        OnStasisRifleUnfreezeEvent?.Invoke(ch);
        if (ch.DoFX)
            Utils.PlayOneShotPS(s.vfxUnfreeze, target.GetComponent<Transform>().position, Quaternion.identity, null);
        if (ch.ApplyKinematicChange)
            target.isKinematic = false;
        if (ch.SendMessage)
            target.SendMessage("OnUnfreeze", SendMessageOptions.DontRequireReceiver);
    }

    public static void OnRedundantFragmentScan() {
        var tgt = PDAScanner.scanTarget;
        SNUtil.writeToChat(
            Language.main.Get(PDAScanner.GetEntryData(tgt.techType).blueprint) + " already known"
        ); //Language.main.Get("ScannerRedundantScanned")
        var r = new RedundantScanEvent();
        OnRedundantScanEvent?.Invoke(r);
        if (!r.PreventNormalDrop)
            CraftData.AddToInventory(TechType.Titanium, 2, false, true);
    }
    /*
   [Obsolete]
   public static bool isEquipmentApplicable(EquipmentType itemType, EquipmentType slotType, Equipment box, Pickupable item) {
        bool ret = Equipment.IsCompatible(itemType, slotType);
        if (equipmentCompatibilityCheckEvent != null) {
            EquipmentCompatibilityCheck ch = new EquipmentCompatibilityCheck(box, item, itemType, slotType, ret);
            equipmentCompatibilityCheckEvent.Invoke(ch);
            ret = ch.allow;
        }
        return ret;
   }*/

    public static EquipmentType GetOverriddenEquipmentType(EquipmentType ret, TechType item) {
        if (EquipmentTypeCheckEvent != null) {
            var ch = new EquipmentTypeCheck(item, ret);
            EquipmentTypeCheckEvent.Invoke(ch);
            ret = ch.Type;
        }

        return ret;
    }

    public static bool TryEat(Survival s, GameObject go) {
        var ea = new EatAttempt(s, go);
        TryEatEvent?.Invoke(ea);

        if (ea.AllowEat && s.Eat(go)) {
            ConsumableTracker.instance.onConsume(go, true);
            OnEatEvent?.Invoke(s, go);
            return true;
        } else {
            SoundManager.playSoundAt(
                SoundManager.buildSound("event:/interface/select"),
                Player.main.transform.position,
                false,
                -1,
                1
            );
            return false;
        }
    }

    public static float GetSwimSpeed(float f) {
        foreach (var m in Player.main.gameObject.GetComponents<PlayerMovementSpeedModifier>())
            f *= m.speedModifier;
        if (GetSwimSpeedEvent != null) {
            var calc = new SwimSpeedCalculation(f);
            GetSwimSpeedEvent.Invoke(calc);
            return calc.Speed;
        } else {
            return f;
        }
    }

    public static float GetWalkSpeed(float f) {
        foreach (var m in Player.main.gameObject.GetComponents<PlayerMovementSpeedModifier>())
            f *= m.speedModifier;
        //SNUtil.writeToChat("Walk speed is "+f.ToString("0.000"));
        return f;
    }

    public static void OnVehicleDestroyed(Vehicle v) {
        OnVehicleDestroyEvent?.Invoke(v);
        List<Pickupable> storeInLocker = [];
        List<IItemsContainer> li = [];
        v.GetAllStorages(li);
        foreach (ItemsContainer sc in li) {
            sc.forEach(ii => FireVehicleLoss(v, false, ii, storeInLocker));
        }

        if (v.modules != null) {
            v.modules.equipment.Values.ForEach(ii => FireVehicleLoss(v, true, ii, storeInLocker));
        }

        if (storeInLocker.Count > 0) {
            TemporaryFloatingLocker.createFloatingLocker(v.transform.position, storeInLocker);
        }
    }

    private static void FireVehicleLoss(Vehicle v, bool module, InventoryItem ii, List<Pickupable> locker) {
        if (ii != null && ii.item) {
            var eff =
                IrreplaceableItemRegistry.instance.getEffects(ii.item.GetTechType());
            if (eff != null) {
                eff.onLostWithVehicle.Invoke(v, module, ii, locker);
            }
        }
    }

    public static void OnSleep(Bed bed) {
        OnSleepEvent?.Invoke(bed);
    }

    public static float GetFoodWaterConsumptionRate(float f) {
        if (GetFoodRateEvent != null) {
            var calc = new FoodRateCalculation(f);
            GetFoodRateEvent.Invoke(calc);
            return calc.Rate;
        }

        return f;
    }

    public static Vector3 GetPlayerMovementControl(Vector3 orig) { //used to override player controls
        if (GetPlayerInputEvent != null) {
            var calc = new PlayerInput(orig);
            GetPlayerInputEvent.Invoke(calc);
            return calc.SelectedInput;
        }

        return orig;
    }

    public static void DoShootTorpedo(
        Bullet b,
        Vector3 position,
        Quaternion rotation,
        float speed,
        float lifeTime,
        Vehicle v
    ) {
        b.Shoot(position, rotation, speed, lifeTime);
        OnTorpedoFireEvent?.Invoke(b, v);
    }

    public static Transform OnTorpedoExploded(Transform result, SeamothTorpedo sm) {
        result.position = sm.tr.position;
        result.rotation = sm.tr.rotation;
        OnTorpedoExplodeEvent?.Invoke(sm, result);
        return result;
    }

    public static bool CanSeeObject(Creature c, GameObject go) {
        float dist = 0;
        var ret = c.hasEyes && c.IsInFieldOfView(go);
        if (CanCreatureSeeObjectEvent != null) {
            var e = new CreatureSeeObjectCheck(c, go, ret, dist);
            CanCreatureSeeObjectEvent.Invoke(e);
            ret = e.CanSee;
        }

        return ret;
    }

    public static void TickPilotedVehicleAggression(AggressiveToPilotingVehicle ai) {
        var main = Player.main;
        if (main == null || main.GetMode() != Player.Mode.LockedPiloting) {
            return;
        }

        var vehicle = main.GetVehicle();
        if (vehicle == null) {
            return;
        }

        var can = Vector3.Distance(vehicle.transform.position, ai.transform.position) <= ai.range;
        if (AggressiveToPilotingEvent != null) {
            var e = new AggressiveToPilotingVehicleCheck(ai, vehicle, can);
            AggressiveToPilotingEvent.Invoke(e);
            can = e.CanTarget;
        }

        if (can) {
            ai.lastTarget.target = vehicle.gameObject;
            ai.creature.Aggression.Add(ai.aggressionPerSecond * ai.updateAggressionInterval);
        }
    }

    public static void OnBaseRebuild(Base b) {
        if (b.cellObjects != null && BaseRebuildEvent != null)
            BaseRebuildEvent.Invoke(b);
    }

    public static void RecomputeBaseHullStrength(BaseHullStrength b) {
        if (b.baseComp != null) {
            if (GameModeUtils.RequiresReinforcements()) {
                var calc = new BaseStrengthCalculation(b);
                b.victims.Clear();
                foreach (Int3 cell in b.baseComp.AllCells) {
                    if (b.baseComp.GridToWorld(cell).y < 0f) {
                        var t = b.baseComp.GetCellObject(cell);
                        if (t != null) {
                            b.victims.Add(t.GetComponent<LiveMixin>());
                            calc.ComputeCellStrength(cell);
                        }
                    }
                }

                BaseStrengthComputeEvent?.Invoke(calc);
                var total = calc.FinalStrength;
                if (!Mathf.Approximately(total, b.totalStrength))
                    SNUtil.writeToChat(
                        Language.main.GetFormat("BaseHullStrChanged", total - b.totalStrength, total)
                    );
                b.totalStrength = total;
            }
        }
    }

    public static void ApplyItemBackground(uGUI_ItemIcon ico, Sprite spr, InventoryItem ii) {
        ico.SetBackgroundSprite(spr);
        if (ii.item.GetTechType() == TechType.Peeper && ii.item.GetComponent<Peeper>().isHero) {
            //ico.background.color = new Color(2F, 1F, 0.2F, 1);
            try {
                ico.background.material.color = new Color(1.8F, 0.85F, 0.3F, 1);
            } catch (Exception e) {
                SNUtil.log(e.ToString());
            }
        } else {
            var im = ii.item.GetComponent<InfectedMixin>();
            if (im && im.IsInfected()) {
                try {
                    ico.background.material.color = new Color(0.2F, 1.5F, 0.2F, 1);
                } catch (Exception e) {
                    SNUtil.log(e.ToString());
                }
            }
        }
    }

    // public static WaterParkCreatureParameters GetWPCP(TechType tt) {
    //     WaterParkCreatureParameters pp = WaterParkCreature.waterParkCreatureParameters.GetOrDefault(
    //         tt,
    //         WaterParkCreatureParameters.GetDefaultValue()
    //     );
    //     if (pp == null) {
    //         SNUtil.writeToChat("Creature " + tt.AsString() + " had null ACU data! Using defaults.");
    //         pp = WaterParkCreatureParameters.GetDefaultValue();
    //     }
    //
    //     return pp;
    // }

    public static Pickupable OnWaterFilterSpawn(FiltrationMachine fm, Pickupable pp) {
        var ret = pp;
        if (WaterFilterSpawnEvent != null) {
            var e = new WaterFilterSpawn(fm, pp);
            WaterFilterSpawnEvent.Invoke(e);
            ret = e.Item;
        }

        return ret;
    }

    public static int SurfaceTypeDebugLevel = 1;

    public static SurfaceType DebugGetSurfaceType(SurfaceType s, Vector3 vec) {
        if (SurfaceTypeDebugLevel > 1 || (SurfaceTypeDebugLevel == 1 && s != SurfaceType.Ceiling &&
                                          s != SurfaceType.Wall && s != SurfaceType.Ground))
            SNUtil.writeToChat("Returning surface type " + s + " from " + vec);
        return s;
    }

    public static void RegisterUid(UniqueIdentifier uid) {
        var id = uid.id;
        if (string.IsNullOrEmpty(id)) {
            //SNUtil.log("Skipping register of UID with null ID: "+uid.name+" @ "+uid.transform.position, SNUtil.diDLL);
            return;
        }

        if (UniqueIdentifier.identifiers.TryGetValue(id, out var has)) {
            if (has != uid) {
                if (has) {
                    if (_skipZeroedDeserialization && has.transform.position.sqrMagnitude > 0.01 &&
                        uid.transform.position.sqrMagnitude < 0.01) {
                        SNUtil.log(
                            "Skipping setup of UID at origin: " + uid.name + " in favor of " + has.name + " @ " +
                            has.transform.position,
                            SNUtil.diDLL
                        );
                        uid.gameObject.destroy(false);
                    } else {
                        Debug.LogErrorFormat(
                            uid,
                            "Overwriting id '{0}' (old class '{1}', new class '{2}'), used to be '{3}' at {4} now '{5}' at {6}",
                            new object[] {
                                id,
                                has.classId,
                                uid.classId,
                                has.name,
                                has.transform.position,
                                uid.name,
                                uid.transform.position,
                            }
                        );
                        UniqueIdentifier.identifiers[id] = uid;
                    }

                    return;
                }

                UniqueIdentifier.identifiers[id] = uid;
                return;
            }
        } else {
            UniqueIdentifier.identifiers.Add(id, uid);
        }
    }

    public static GameObject CreateSpawnedItem(TechType tt, bool customOnly) {
        var ret = CraftData.InstantiateFromPrefab(
            CraftData.GetPrefabForTechTypeAsync(tt).GetResult(),
            tt,
            customOnly
        );
        if (GameModeUtils.currentEffectiveMode != GameModeOption.Creative) {
            var e = SpawnedItemTracker.instance.addSpawn(tt);
            if (ret) {
                var pi = ret.GetComponentInChildren<PrefabIdentifier>();
                if (pi)
                    e.setObject(pi);
                else
                    SNUtil.log("No PrefabIdentifier to attach to spawn event " + e);
            } else {
                SNUtil.log("No object at all for spawn event " + e);
            }
        }

        return ret;
    }

    public static void OnCommandUse(DevConsole c, string cmd) {
        CommandTracker.instance.onCommand(cmd);
    }

    public static float GetGrowingPlantProgressInTick(float progress, GrowingPlant g) {
        GrowingPlantTickEvent?.Invoke(g, progress);
        return progress;
    }

    public static void OnCuddlefishPlayed(
        CuteFishHandTarget tgt,
        Player ep,
        CuteFishHandTarget.CuteFishCinematic anim
    ) {
        OnPlayWithCuddlefish?.Invoke(tgt, ep, anim);
    }

    public static void OnRocketStageCompleted(Rocket r) {
        OnRocketStageCompletedEvent?.Invoke(r, r.currentRocketStage, Rocket.IsAnyRocketReady);
        StoryGoal.Execute("RocketStage" + r.currentRocketStage, Story.GoalType.Story);
        if (Rocket.IsAnyRocketReady)
            StoryGoalScheduler.main.Schedule(new StoryGoal("RocketComplete", Story.GoalType.Story, 10F));
    }

    public static float GetCrafterTime(float time, Crafter c, TechType recipe) {
        var calc = new CraftTimeCalculation(time, c, recipe);
        CraftTimeEvent?.Invoke(calc);
        //SNUtil.writeToChat("Crafting time adjusted to "+calc.craftingDuration.ToString("0.0")+"s from original "+calc.originalDuration.ToString("0.0")+"s");
        return calc.CraftingDuration;
    }

    public static void PulseSeamothDefence(SeaMoth sm) {
        SeamothDischargeEvent?.Invoke(sm);
    }

    public static void OnTreaderChunkSpawn(SinkingGroundChunk chunk) {
        SpawnTreaderChunk?.Invoke(chunk);
    }

    public static void OnCrashfishExplode(Crash c) {
        CrashfishExplodeEvent?.Invoke(c);
    }

    public static bool CheckTargetingSkip(bool orig, Transform obj) {
        if (!obj || !obj.gameObject)
            return orig;
        var id = obj.gameObject.FindAncestor<PrefabIdentifier>();
        if (!id)
            return orig;
        var calc = new TargetabilityCheck(!orig, obj, id);
        TargetabilityEvent?.Invoke(calc);
        //SNUtil.writeToChat("Crafting time adjusted to "+calc.craftingDuration.ToString("0.0")+"s from original "+calc.originalDuration.ToString("0.0")+"s");
        return !calc.AllowTargeting;
    }

    public static Pickupable OnRefundConstructableIngredient(Pickupable pp, Constructable c) {
        var b = pp.GetComponent<Battery>();
        if (b)
            b.charge = 0;
        return pp;
    }

    public static void OnAuroraSpawn(CrashedShipExploder ex) {
        AuroraSpawnEvent?.Invoke(ex);
        ex.gameObject.EnsureComponent<ShipExplosionListener>();
    }

    private class ShipExplosionListener : MonoBehaviour {
        private void OnShipExplode() {
            StoryGoalScheduler.main.Schedule(
                new StoryGoal("AuroraExplode", Story.GoalType.Story, 24F)
            ); //right as the ship explodes
        }
    }

    private static void StartTeleport() {
        if (!Player.main.GetVehicle() && !Player.main.currentSub) {
            var pp = Inventory.main.GetHeld();
            if (pp) {
                var pc = pp.GetComponent<PropulsionCannon>();
                if (pc && pc.grabbedObject) {
                    _selectedSlot = Inventory.main.quickSlots.activeSlot;
                    _activePropulsionGun = pc;
                    _teleportWithPlayer = pc.grabbedObject;
                    _relativeGrabPosition = _teleportWithPlayer.transform.position - Player.main.transform.position;
                    _teleportWithPlayer.transform.position =
                        WorldUtil.getClosest<PrecursorTeleporter>(Player.main.transform.position).warpToPos;
                    //SNUtil.writeToChat("Teleporting "+teleportWithPlayer+" with player, pre");
                }
            }
        }
    }

    private static void StopTeleport() {
        if (_activePropulsionGun) {
            if (_teleportWithPlayer) {
                //InventoryItem ii = Inventory.main.container.GetItems(activePropulsionGun.GetComponent<Pickupable>().GetTechType()).First();
                Inventory.main.quickSlots.SelectImmediate(_selectedSlot);
                _teleportWithPlayer.transform.position = Player.main.transform.position + _relativeGrabPosition;
                _activePropulsionGun.GrabObject(_teleportWithPlayer);
                //SNUtil.writeToChat("Teleporting "+teleportWithPlayer+" with player, post");
            } else {
                //SNUtil.writeToChat("Object to teleport with player does not yet exist");
            }
        }

        _teleportWithPlayer = null;
        _activePropulsionGun = null;
    }
}