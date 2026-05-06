using System;
using System.Reflection;
using System.Text;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Auroresource;

public static class ARHooks {
    static ARHooks() {
        SNUtil.Log("Initializing ARHooks");
        DIHooks.OnPlayerTickEvent += TickPlayer;
        DIHooks.ItemTooltipEvent += GenerateItemTooltips;
        DIHooks.OnItemPickedUpEvent += OnItemPickedUp;
        DIHooks.AuroraSpawnEvent += OnAuroraSpawn;

        DIHooks.ScannerRoomTechTypeListingEvent += FallingMaterialSystem.Instance.ModifyScannableList;
        DIHooks.ScannerRoomTickEvent += FallingMaterialSystem.Instance.TickMapRoom;
    }

    public static void GenerateItemTooltips(StringBuilder sb, TechType tt, GameObject go) {
        if (tt == TechType.LaserCutter &&
            Story.StoryGoalManager.main.completedGoals.Contains(AuroresourceMod.LaserCutterJailbroken)) {
            TooltipFactory.WriteDescription(
                sb,
                "\nDevice firmware has been modified to circumvent proscribed usage limitations."
            );
        }
    }

    public static void TickPlayer(Player ep) {
        var time = DayNightCycle.main.timePassedAsFloat;
        var dT = Time.deltaTime;
        FallingMaterialSystem.Instance.Tick(time, dT);
    }

    public static void OnItemPickedUp(DIHooks.ItemPickup ip) {
        var p = ip.Item;
        var tag = p.GetComponentInParent<FallingMaterialTag>();
        if (tag) {
            p.transform.SetParent(null);
            tag.gameObject.destroy(false);
        }
    }

    public static void OnGeyserSpawn(Geyser g) {
        g.gameObject.EnsureComponent<GeyserMaterialSpawner>().Geyser = g;
        //WorldUtil.registeredGeysers.Add(new PositionedPrefab(g.GetComponent<PrefabIdentifier>()));
    }

    public static void OnMapRoomSpawn(MapRoomFunctionality map) {
        if (Array.IndexOf(map.allowedUpgrades, AuroresourceMod.MeteorDetector.TechType) < 0)
            typeof(MapRoomFunctionality).GetField("allowedUpgrades", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(map, map.allowedUpgrades.AddToArray(AuroresourceMod.MeteorDetector.TechType));
    }

    public static void OnAuroraSpawn(CrashedShipExploder ex) {
        var s = ex.gameObject.EnsureComponent<Sealed>();
        s._sealed = true;
        s.maxOpenedAmount =
            250 / AuroresourceMod.ModConfig.getFloat(ARConfig.ConfigEntries.SPEED); //was 150, comparedto vanilla 100
        s.openedEvent.AddHandler(
            ex.gameObject,
            new UWE.Event<Sealed>.HandleFunction(se => {
                    var unlock =
                        Story.StoryGoalManager.main.completedGoals.Contains(AuroresourceMod.LaserCutterJailbroken);
                    se.openedAmount = 0;
                    se._sealed = true;
                    if (unlock) {
                        InventoryUtil.addItem(TechType.ScrapMetal);
                        PDAMessagePrompts.instance.trigger("auroracut");
                    }
                    //SNUtil.log("Cycled aurora laser cut: "+s.openedAmount);
                }
            )
        );
        var ht = ex.gameObject.EnsureComponent<GenericHandTarget>();
        ht.onHandHover = new HandTargetEvent();
        ht.onHandHover.AddListener(hte => {
                var unlock = Story.StoryGoalManager.main.completedGoals.Contains(AuroresourceMod.LaserCutterJailbroken);
                var held = Inventory.main.GetHeld();
                if (unlock) {
                    HandReticle.main.SetProgress(s.GetSealedPercentNormalized());
                    HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
                    HandReticle.main.SetInteractText("AuroraLaserCut"); //is a locale key
                } else if (held && held.GetTechType() == TechType.LaserCutter) {
                    HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                    HandReticle.main.SetInteractText("AuroraLaserCutNeedsUnlock"); //is a locale key
                } else {
                    HandReticle.main.SetIcon(HandReticle.IconType.Default, 1f);
                    HandReticle.main.SetInteractText("");
                }
            }
        );
    }

    public static GameObject GetDrillableDrop(Drillable d) {
        var pi = d.gameObject.GetComponent<PrefabIdentifier>();
        if (pi) {
            var di = DrillableResourceArea.getResourceNode(pi.ClassId);
            if (di != null) {
                return di.getRandomResource();
            }
        }

        return d.ChooseRandomResource();
    }

    public static void OnBreakableResourceSpawn(BreakableResource src) {
        if (src.gameObject.GetComponent<ReefbackPlant>()) {
            src.numChances = 4;
        }
    }
}