using System;
using System.Collections.Generic;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public static class AEHooks {
    private static readonly Vector3 mountainWreckLaserable = new(684.46F, -359.33F, 1218.44F);
    private static readonly Vector3 mountainWreckBlock = new(686.81F, -364.29F, 1223.04F);

    private static BaseCell currentPlayerRoom;
    private static float lastPlayerRoomCheckTime;

    static AEHooks() {
        SNUtil.log("Initializing AEHooks");
        DIHooks.OnWorldLoadedEvent += onWorldLoaded;
        DIHooks.OnConstructedEvent += onConstructionComplete;
        DIHooks.OnItemPickedUpEvent += onPickup;
        DIHooks.OnDamageEvent += onTakeDamage;
        DIHooks.KnifeHarvestEvent += interceptItemHarvest;
        DIHooks.InventoryClosedEvent += onInvClosed;
        DIHooks.OnBaseLoadedEvent += onBaseLoaded;
        DIHooks.ConstructabilityEvent += enforceBuildability;
        DIHooks.GravTrapAttemptEvent += gravTryAttract;
        DIHooks.OnSkyApplierSpawnEvent += onSkyApplierSpawn;
        DIHooks.OnPlayerTickEvent += tickPlayer;
        DIHooks.CraftTimeEvent += affectCraftTime;
        DIHooks.OnSleepEvent += onSleep;
        DIHooks.BaseRebuildEvent += onBaseRebuild;
        DIHooks.BaseStrengthComputeEvent += onBaseHullCompute;

        //DIHooks.onRedundantScanEvent += ch => ch.preventNormalDrop = onRedundantScan();
        CustomMachineLogic.getMachinePowerCostFactorEvent += getCustomMachinePowerCostMultiplier;
    }

    public static void tickPlayer(Player ep) {
        if (ep.currentSub && ep.currentSub.isBase && ep.currentSub is BaseRoot sub) {
            var time = DayNightCycle.main.timePassedAsFloat;
            if (time - lastPlayerRoomCheckTime >= 0.5F) {
                currentPlayerRoom = ObjectUtil.getBaseRoom(sub, ep.transform.position);
                if (currentPlayerRoom && DIHooks.GetWorldAge() >= 15 &&
                    currentPlayerRoom.gameObject.getChildObject("BaseRoom"))
                    PDAManager.getPage("ROOMTYPESPDAPAGE").unlock();
                lastPlayerRoomCheckTime = time;
            }
        }
    }

    public static BaseCell getCurrentPlayerRoom() {
        return currentPlayerRoom;
    }

    public static void onWorldLoaded() {
        OutdoorPot.updateLocale();

        var s = AqueousEngineeringMod.machineLocale.getEntry("BaseRepairBeacon").getString("frag");

        foreach (var f in AqueousEngineeringMod.repairBeaconFragments)
            LanguageHandler.SetLanguageLine(f.GetGadget<ScanningGadget>().ScannerEntryData.blueprint.AsString(), s);
    }

    public static void onSkyApplierSpawn(SkyApplier sk) {
        var pi = sk.GetComponent<PrefabIdentifier>();
        MoonpoolRotationSystem.instance.processObject(sk.gameObject);
        if (pi && pi.name.StartsWith("Seamoth", StringComparison.InvariantCultureIgnoreCase) && pi.name.EndsWith(
                "Arm(Clone)",
                StringComparison.InvariantCultureIgnoreCase
            ))
            return;
        if (sk.GetComponent<StarshipDoor>() && Vector3.Distance(mountainWreckLaserable, sk.transform.position) <= 0.5)
            new WreckDoorSwaps.DoorSwap(sk.transform.position, "Laser").applyTo(sk.gameObject);
        else if (pi && pi.ClassId == "055b3160-f57b-46ba-80f5-b708d0c8180e" &&
                 Vector3.Distance(mountainWreckBlock, sk.transform.position) <= 0.5)
            new WreckDoorSwaps.DoorSwap(sk.transform.position, "Blocked").applyTo(sk.gameObject);
    }

    public static void onNuclearReactorSpawn(BaseNuclearReactor reactor) {
        reactor.gameObject.EnsureComponent<NuclearReactorFuelSystem.ReactorManager>();
    }

    public static void tickACU(WaterPark acu) {
        AcuCallbackSystem.Instance.Tick(acu);
    }

    public static void tryBreedACU(WaterPark acu, WaterParkCreature creature) {
        if (!acu.items.Contains(creature))
            return;
        var tt = creature.pickupable.GetTechType();
        var call = acu.gameObject.GetComponent<AcuCallbackSystem.AcuCallback>();
        var bio = call ? call.IsAboveBioreactor : null;
        var full = !acu.HasFreeSpace();
        if (full && !(bio && bio.IsAllowedToAdd(creature.pickupable, false) &&
                      bio.container.HasRoomCached(TechData.GetItemSize(tt))))
            return;
        var mate = acu.items.Find(item =>
            item && item != creature && item is WaterParkCreature parkCreature && parkCreature.pickupable &&
            parkCreature.GetCanBreed() && parkCreature.pickupable.GetTechType() == tt
        ) as WaterParkCreature;
        if (!mate)
            return;
        var flag = true;
        if (full) {
            var go = ObjectUtil.createWorldObject(tt);
            go.SetActive(false);
            flag = bio.container.AddItem(go.GetComponent<Pickupable>()) != null;
        } else {
            var go = ObjectUtil.lookupPrefab(tt).GetResult();
            var wp = go.GetComponent<WaterParkCreature>();
            if (wp != null) {
                WaterParkCreature.Born(
                    wp.data.eggOrChildPrefab,
                    acu,
                    creature.transform.position + Vector3.down
                );
            }
        }

        if (flag)
            mate.ResetBreedTime();
    }

    public static bool canAddItemToACU(Pickupable item) {
        if (!item)
            return false;
        var tt = item.GetTechType();
        if (AcuCallbackSystem.IsStalkerToy(tt))
            return true;
        var go = item.gameObject;
        if (go.GetComponent<Creature>() == null && go.GetComponent<CreatureEgg>() == null)
            return false;
        var lv = go.GetComponent<LiveMixin>();
        return !lv || lv.IsAlive();
    }

    public static void onChunkGenGrass(IVoxelandChunk2 chunk) {
        foreach (Renderer r in chunk.grassRenders) {
            AcuTheming.CacheGrassMaterial(r.materials[0]);
        }
    }

    public static float getCameraDistanceForRenderFX(MapRoomCamera cam, MapRoomScreen scr) {
        var sub = cam.dockingPoint ? cam.dockingPoint.gameObject.GetComponentInParent<SubRoot>() : null;
        if (!sub) {
            sub = WorldUtil.getClosest<SubRoot>(cam.gameObject);
        }

        if (sub) {
            var dist = Vector3.Distance(sub.transform.position, cam.transform.position);
            if (dist <= 400) {
                var lgc = sub.GetComponentInChildren<RemoteCameraAntennaLogic>();
                if (lgc && lgc.isReady()) {
                    return dist <= 350 ? 0 : (float)MathUtil.linterpolate(dist, 350, 400, 0, 400, true);
                }
            }
        }

        return cam.GetScreenDistance(scr);
    }

    private static bool isBuildingACUBuiltBlock() {
        return (AqueousEngineeringMod.acuBoosterBlock != null &&
                Builder.constructableTechType == AqueousEngineeringMod.acuBoosterBlock.TechType) ||
               (AqueousEngineeringMod.acuCleanerBlock != null &&
                Builder.constructableTechType == AqueousEngineeringMod.acuCleanerBlock.TechType) ||
               (AqueousEngineeringMod.acuMonitorBlock != null &&
                Builder.constructableTechType == AqueousEngineeringMod.acuMonitorBlock.TechType);
    }

    /*
    private static bool isOnACU(Collider c) {
        if (!c)
            return false;
        BaseExplicitFace face = c.gameObject.FindAncestor<BaseExplicitFace>();
        if (!face)
            return false;
        SNUtil.writeToChat(face+" > "+face.gameObject.GetFullHierarchyPath()+" > "+face.gameObject.name.Contains("WaterPark"));
        return face && face.gameObject.name.Contains("WaterPark");
    }
    */
    public static void enforceBuildability(DIHooks.BuildabilityCheck check) {
        if (isBuildingACUBuiltBlock()) {
            check.Placeable =
                check.PlaceOn &&
                check.PlaceOn.gameObject.FindAncestor<WaterParkGeometry>(); //isOnACU(check.placeOn && chec);
            check.IgnoreSpaceRequirements = true;
        } else if (Builder.constructableTechType == AqueousEngineeringMod.ampeelAntennaBlock.TechType &&
                   check.PlaceOn && Player.main.currentWaterPark &&
                   check.PlaceOn.gameObject.FindAncestor<WaterParkGeometry>().GetModule() ==
                   Player.main.currentWaterPark) {
            check.Placeable = true;
            check.IgnoreSpaceRequirements = true;
        } else if (Builder.constructableTechType == AqueousEngineeringMod.batteryBlock.TechType) {
            //SNUtil.writeToChat(check.placeOn ? check.placeOn.gameObject.GetFullHierarchyPath() : "null");
            check.Placeable &= check.PlaceOn && (check.PlaceOn.gameObject.isRoom(false) ||
                                                 check.PlaceOn.gameObject.isMoonpool(false, false));
            check.IgnoreSpaceRequirements = false;
        } else if (Builder.constructableTechType == AqueousEngineeringMod.pillarBlock.TechType) {
            //SNUtil.writeToChat(check.placeOn ? check.placeOn.gameObject.GetFullHierarchyPath() : "null");
            check.Placeable &= check.PlaceOn && check.PlaceOn.gameObject.isRoom(false);
            check.IgnoreSpaceRequirements = true;
        } else if (Builder.constructableTechType == AqueousEngineeringMod.powerRelayBlock.TechType) {
            check.Placeable = !check.PlaceOn;
            check.IgnoreSpaceRequirements = true;
        } else if (Builder.constructableTechType == AqueousEngineeringMod.atpTapBlock.TechType) {
            check.Placeable = check.PlaceOn && ATPTapLogic.isValidSourceObject(check.PlaceOn.gameObject) && WorldUtil
                .getObjectsNearMatching(
                    check.PlaceOn.transform.position,
                    100,
                    go => go.GetComponent<ATPTapLogic>() && go.GetComponent<Constructable>().constructed
                ).Count == 0;
            check.IgnoreSpaceRequirements = true;
        }
    }
    /*
   public static bool onRedundantScan() {
    PDAScanner.ScanTarget tgt = PDAScanner.scanTarget;
    if (tgt.gameObject) {
        PrefabIdentifier pi = tgt.gameObject.GetComponent<PrefabIdentifier>();
        if (pi && AqueousEngineeringMod.repa
    }
   }*/

    public static void interceptItemHarvest(DIHooks.KnifeHarvest h) {
        if (h.Hit && h.Drops.Count > 0) {
            var p = h.Hit.FindAncestor<Planter>();
            if (p && BaseRoomSpecializationSystem.instance.getSavedType(p) ==
                BaseRoomSpecializationSystem.RoomTypes.AGRICULTURAL) {
                if (UnityEngine.Random.Range(0F, 1F) < 0.33F)
                    h.Drops[h.DefaultDrop] += 1;
            }
        }
    }

    public static void onPickup(DIHooks.ItemPickup ip) {
        var pp = ip.Item;
        if (BaseRoomSpecializationSystem.instance.getPlayerRoomType(Player.main) ==
            BaseRoomSpecializationSystem.RoomTypes.AGRICULTURAL) {
            var ea = pp.GetComponent<Eatable>();
            if (ea) {
                //SNUtil.writeToChat(pp+" is edible, +25% to values since agri room");
                ea.waterValue *= 1.25F;
                ea.foodValue *= 1.25F;
            }
        }
    }

    public static float getReactorGeneration(float orig, MonoBehaviour reactor) { //either bio or nuclear
        //SNUtil.writeToChat("Reactor gen "+orig+" in "+BaseRoomSpecializationSystem.instance.getSavedType(reactor));
        return BaseRoomSpecializationSystem.instance.getSavedType(reactor) ==
               BaseRoomSpecializationSystem.RoomTypes.POWER
            ? orig * 1.25F
            : orig;
    }

    public static void onSleep(Bed bed) {
        //SNUtil.writeToChat("Slept in "+BaseRoomSpecializationSystem.instance.getSavedType(bed));
        if (BaseRoomSpecializationSystem.instance.getSavedType(bed, out var deco, out var thresh) ==
            BaseRoomSpecializationSystem.RoomTypes.LEISURE)
            Player.main.gameObject.AddComponent<HealingOverTime>().setValues(
                Mathf.Min(20, 15 + deco - thresh),
                bed.kSleepRealTimeDuration
            ).activate();
    }

    public static void affectFoodRate(DIHooks.FoodRateCalculation calc) {
        var type = BaseRoomSpecializationSystem.instance.getPlayerRoomType(Player.main, out var deco, out var thresh);
        //SNUtil.writeToChat("Current player room type: "+type);
        if (type == BaseRoomSpecializationSystem.RoomTypes.LEISURE)
            calc.Rate *= Mathf.Max(0.2F, 0.33F - 0.02F * (deco - thresh));
        else if (type == BaseRoomSpecializationSystem.RoomTypes.WORK)
            calc.Rate *= 0.8F - 0.01F * Mathf.Min(5, deco);
    }

    private static void affectCraftTime(DIHooks.CraftTimeCalculation calc) {
        if (BaseRoomSpecializationSystem.instance.getSavedType(calc.Crafter) ==
            BaseRoomSpecializationSystem.RoomTypes.WORK)
            calc.CraftingDuration /= 1.5F;
    }

    public static void onConstructionComplete(Constructable c, bool complete) {
        if (DIHooks.GetWorldAge() < 1F)
            return;
        if (Player.main.currentSub && Player.main.currentSub.isBase) {
            BaseRoomSpecializationSystem.instance.updateRoom(c.gameObject);
        }
    }

    public static void onInvClosed(StorageContainer sc) {
        if (Player.main.currentSub && Player.main.currentSub.isBase &&
            BaseRoomSpecializationSystem.instance.storageHasDecoValue(sc))
            BaseRoomSpecializationSystem.instance.updateRoom(sc.gameObject);
    }

    public static float getWaterFilterPowerCost(float cost, FiltrationMachine c) {
        //SNUtil.writeToChat("Waterfilter power cost "+cost+" in "+BaseRoomSpecializationSystem.instance.getSavedType(c));
        if (BaseRoomSpecializationSystem.instance.getSavedType(c) == BaseRoomSpecializationSystem.RoomTypes.MECHANICAL)
            cost *= 0.8F;
        return cost;
    }

    public static float getChargerSpeed(float speed, Charger c) {
        //SNUtil.writeToChat("Charger speed "+speed+" in "+BaseRoomSpecializationSystem.instance.getSavedType(c));
        if (BaseRoomSpecializationSystem.instance.getSavedType(c) == BaseRoomSpecializationSystem.RoomTypes.MECHANICAL)
            speed *= 1.5F;
        return speed;
    }

    public static void getCustomMachinePowerCostMultiplier(CustomMachinePowerCostFactorCheck ch) {
        if (BaseRoomSpecializationSystem.instance.getSavedType(ch.machine) ==
            BaseRoomSpecializationSystem.RoomTypes.MECHANICAL)
            ch.value *= 0.8F;
    }

    public static void onBaseLoaded(BaseRoot root) {
        BaseRoomSpecializationSystem.instance.recomputeBaseRooms(root, 1F);
    }
    /*
   public static void onPDAClosed() {
        XMLLocale.LocaleEntry e = AqueousEngineeringMod.acuMonitorBlock.locale;
        PDAManager.PDAPage pp = PDAManager.getPage(e.key+"PDA");
        pp.relock();
   }*/

    public static void gravTryAttract(DIHooks.GravTrapGrabAttempt h) {
        if (h.Gravtrap.GetComponent<ItemCollector.ItemCollectorLogic>()) {
            h.AllowGrab &= ItemCollector.ItemCollectorLogic.canGrab(h.Target);
        }
    }

    public static void onTakeDamage(DIHooks.DamageToDeal dmg) {
        if (dmg.Type == DamageType.Heat || dmg.Type == DamageType.Fire) {
            var pi = dmg.Target.FindAncestor<PrefabIdentifier>();
            if (pi && pi.ClassId == AqueousEngineeringMod.collector.ClassID)
                dmg.SetValue(0);
        }
    }

    public static void onEquipmentSlotActivated(uGUI_EquipmentSlot slot, bool active) {
        if (active && !slot.active && slot.slot.StartsWith(
                "NuclearReactor",
                StringComparison.InvariantCultureIgnoreCase
            )) {
            slot.gameObject.EnsureComponent<NuclearReactorFuelSystem.ReactorFuelDisplay>();
        }
    }

    public static void onPlacedItem(PlaceTool pt) {
        if (Player.main.currentSub && Player.main.currentSub.isBase)
            BaseRoomSpecializationSystem.instance.updateRoom(pt.gameObject);
    }

    public static void onBaseRebuild(Base b) {
        MoonpoolRotationSystem.instance.rebuildBase(b);
    }

    public static void onBaseHullCompute(DIHooks.BaseStrengthCalculation calc) {
        var arr = calc.Component.baseComp.GetComponentsInChildren<BasePillarLogic>();
        var pillarsByRoom = new Dictionary<BaseCell, RoomPillarTracker>();
        var bb = calc.Component.baseComp.GetComponent<BaseRoot>();
        for (var i = 0; i < arr.Length; i++) {
            if (!arr[i] || !arr[i].buildable || !arr[i].buildable.constructed)
                continue;
            var bc = ObjectUtil.getBaseRoom(bb, arr[i].gameObject);
            if (!bc)
                continue;
            var tr = pillarsByRoom.ContainsKey(bc) ? pillarsByRoom[bc] : null;
            if (tr == null) {
                tr = new RoomPillarTracker(bc);
                pillarsByRoom[bc] = tr;
            }

            tr.pillars.Add(arr[i]);
        }

        foreach (var tr in pillarsByRoom.Values) {
            float eff = 1;
            var n = 0;
            foreach (var lgc in tr.pillars) {
                n++;
                calc.AddBonusStrength(
                    lgc.gameObject,
                    eff * AqueousEngineeringMod.config.getFloat(AEConfig.ConfigEntries.PILLARHULL)
                );
                if (n >= AqueousEngineeringMod.config.getInt(AEConfig.ConfigEntries.PILLARLIM))
                    eff *= 0.5F;
            }
        }
    }

    private class RoomPillarTracker {
        internal readonly BaseCell room;
        internal readonly List<BasePillarLogic> pillars = [];

        internal RoomPillarTracker(BaseCell bc) {
            room = bc;
        }
    }
}