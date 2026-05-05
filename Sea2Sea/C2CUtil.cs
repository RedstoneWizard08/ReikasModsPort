using System;
using System.Collections.Generic;
using System.Xml;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public static class C2CUtil {
    //exclusion radius, target count, max range
    internal static readonly Dictionary<Vector3, Tuple<float, int, float>> mercurySpawners =
        new() {
            { new Vector3(908.7F, -235.1F, 615.7F), Tuple.Create(2F, 4, 32F) },
            { new Vector3(904.3F, -247F, 668.8F), Tuple.Create(1F, 3, 32F) },
            { new Vector3(915.1F, -246.8F, 651.2F), Tuple.Create(2F, 6, 32F) },
            { new Vector3(1273, -290, 604.3F), Tuple.Create(2F, 3, 32F) },
            { new Vector3(1254, -293.3F, 606.3F), Tuple.Create(2F, 3, 32F) },
            { new Vector3(1239, -286.4F, 617), Tuple.Create(2F, 3, 32F) },
            { new Vector3(1245, -308.2F, 555.8F), Tuple.Create(2F, 3, 32F) },
            { new Vector3(-1216, -299.1F, 510.3F), Tuple.Create(2F, 3, 32F) },
            { new Vector3(1278, -276.4F, 497.5F), Tuple.Create(2F, 3, 32F) },
            { new Vector3(1228, -275.6F, 483.9F), Tuple.Create(2F, 3, 32F) },
        };

    internal static readonly Dictionary<Vector3, Tuple<float, int, float>> calciteSpawners =
        new() {
            { new Vector3(-993.1F, -630.4F, -618.2F), Tuple.Create(4F, 3, 24F) },
            { new Vector3(-983.3F, -623.9F, -561.1F), Tuple.Create(4F, 5, 32F) },
            { new Vector3(-666.8F, -688.0F, -42.14F), Tuple.Create(4F, 6, 48F) },
            { new Vector3(-674.2F, -622.0F, -221.6F), Tuple.Create(4F, 6, 48F) },
            { new Vector3(-719.4F, -673.7F, -39.6F), Tuple.Create(4F, 5, 48F) },
            { new Vector3(-864.5F, -672.7F, -128.6F), Tuple.Create(4F, 8, 48F) },
        };

    static C2CUtil() {
    }

    public static bool checkConditionsAndShowPDAAndFirstVoicelogIfNot(
        params Tuple<bool, string, PDAMessages.Messages>[] checks
    ) {
        foreach (var check in checks) {
            if (!checkConditionAndShowPDAAndVoicelogIfNot(check.Item1, check.Item2, check.Item3))
                return false;
        }

        return true;
    }

    public static bool checkConditionAndShowPDAAndVoicelogIfNot(bool check, string page, PDAMessages.Messages msg) {
        if (check) {
            return true;
        } else {
            MoraleSystem.instance.shiftMorale(-10);
            if (PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(msg).key)) {
                if (!string.IsNullOrEmpty(page))
                    PDAManager.getPage(page).unlock(false);
            }

            return false;
        }
    }

    public static bool playerCanHeal() {
        var ep = Player.main;
        return !EnvironmentalDamageSystem.instance.isPlayerRecoveringFromPressure() && (!ep.IsSwimming() ||
            ep.GetDepth() < EnvironmentalDamageSystem.depthDamageStart ||
            LiquidBreathingSystem.instance.hasLiquidBreathing());
    }
    /*
    public static GameObject createMergedPropGun(bool toInv = false) {
        GameObject prop = ObjectUtil.createWorldObject(TechType.PropulsionCannon);
        GameObject repl = ObjectUtil.lookupPrefab(TechType.RepulsionCannon);
        prop.EnsureComponent<RepulsionCannon>().copyObject(repl.GetComponent<RepulsionCannon>());
        prop.EnsureComponent<PropGunTypeSwapper>().applyMode();
        if (toInv)
            Inventory.main.Pickup(prop.GetComponent<Pickupable>());
        else
            prop.SetActive(true);
        return prop;
    }

    public class PropGunTypeSwapper : MonoBehaviour {

        public bool isPropMode = true;

        public void applyMode() {
            if (isPropMode) {
                GetComponent<PropulsionCannon>().enabled = true;
                GetComponent<PropulsionCannonWeapon>().enabled = true;
                GetComponent<RepulsionCannon>().enabled = false;
                //RenderUtil.swapTextures(GetComponentInChildren<Renderer>().materials[0]);
            }
            else {
                GetComponent<PropulsionCannon>().enabled = false;
                GetComponent<PropulsionCannonWeapon>().enabled = false;
                GetComponent<RepulsionCannon>().enabled = true;
            }
        }

    }*/
    /*
public static bool hasNoGasMask() {
    return Inventory.main.equipment.GetCount(TechType.Rebreather) == 0 && Inventory.main.equipment.GetCount(rebreatherV2.TechType) == 0;
}*/

    public static void generateLavaCastleAzurite() {
        List<GameObject> azurite = [];
        var azur = CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).ClassID;
        foreach (var pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
            if (pi.ClassId == "407e40cf-69f2-4412-8ab6-45faac5c4ea2") {
                for (var ang = 0; ang < 360; ang += 10) {
                    var a = UnityEngine.Random.Range(ang - 5F, ang + 5F);
                    float r = 16;
                    var dt = new Vector3(
                        Mathf.Cos(a) * r,
                        -UnityEngine.Random.Range(0, UnityEngine.Random.Range(25, 40)),
                        Mathf.Sin(a) * r
                    );
                    var vec = pi.transform.position + dt;
                    var ray = new Ray(vec, -dt.setY(0));
                    if (UWE.Utils.RaycastIntoSharedBuffer(
                            ray,
                            24,
                            Voxeland.GetTerrainLayerMask(),
                            QueryTriggerInteraction.Ignore
                        ) > 0) {
                        var hit = UWE.Utils.sharedHitBuffer[0];
                        if (hit.transform != null) {
                            var flag = true;
                            foreach (var pi2 in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(
                                         hit.point,
                                         9F
                                     )) {
                                if (pi2.ClassId == azur) {
                                    flag = false;
                                    break;
                                }
                            }

                            if (!flag)
                                continue;
                            var go = ObjectUtil.createWorldObject(azur);
                            go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
                            go.transform.Rotate(Vector3.up * UnityEngine.Random.Range(0F, 360F), Space.Self);
                            go.transform.position = hit.point;
                            azurite.Add(go);
                        }
                    }
                }
            }
        }

        var path = BuildingHandler.instance.getDumpFile("lavacastle_vents");
        var doc = new XmlDocument();
        var rootnode = doc.CreateElement("Root");
        doc.AppendChild(rootnode);

        foreach (var go in azurite) {
            var pfb = new PositionedPrefab(go.GetComponent<PrefabIdentifier>());
            var e = doc.CreateElement("customprefab");
            pfb.saveToXML(e);
            doc.DocumentElement.AppendChild(e);
        }

        doc.Save(path);
    }

    public static void generateLRNestPlants() {
        var p1 = new Vector3(-786, -762.6F, -321);
        var p2 = new Vector3(-801, -764.9F, -280);
        var p3 = new Vector3(-788, -751.6F, -321);

        List<GameObject> plants = [];

        for (float f = 0; f <= 1; f += 0.05F) {
            var vec = Vector3.Lerp(p1, p2, f);
            for (var i = 0; i < 9; i++) {
                var rot = UnityEngine.Random.rotationUniform.eulerAngles.normalized;
                var ray = new Ray(vec, rot);
                if (UWE.Utils.RaycastIntoSharedBuffer(
                        ray,
                        6,
                        Voxeland.GetTerrainLayerMask(),
                        QueryTriggerInteraction.Ignore
                    ) > 0) {
                    var hit = UWE.Utils.sharedHitBuffer[0];
                    if (hit.transform != null) {
                        var flag = true;
                        foreach (var pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(
                                     hit.point,
                                     0.2F
                                 )) {
                            if (pi.ClassId == SeaToSeaMod.LrNestGrass.Info.ClassID) {
                                flag = false;
                                break;
                            }
                        }

                        if (!flag)
                            continue;
                        var go = ObjectUtil.createWorldObject(SeaToSeaMod.LrNestGrass.Info.ClassID);
                        go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
                        go.transform.position = hit.point;
                        plants.Add(go);
                    }
                }
            }
        }

        for (var i = 0; i < 9; i++) {
            var rot = UnityEngine.Random.rotationUniform.eulerAngles.normalized;
            var ray = new Ray(p3, rot);
            if (UWE.Utils.RaycastIntoSharedBuffer(
                    ray,
                    18,
                    Voxeland.GetTerrainLayerMask(),
                    QueryTriggerInteraction.Ignore
                ) > 0) {
                var hit = UWE.Utils.sharedHitBuffer[0];
                SNUtil.writeToChat(i + ": " + hit.transform);
                if (hit.transform != null && hit.normal.y > -0.7F) {
                    var flag = true;
                    foreach (var pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(
                                 hit.point,
                                 0.2F
                             )) {
                        if (pi.ClassId == SeaToSeaMod.LrNestGrass.Info.ClassID) {
                            flag = false;
                            break;
                        }
                    }

                    if (!flag)
                        continue;
                    var go = ObjectUtil.createWorldObject(SeaToSeaMod.LrNestGrass.Info.ClassID);
                    go.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
                    go.transform.position = hit.point;
                    plants.Add(go);
                }
            }
        }

        var path = BuildingHandler.instance.getDumpFile("lr_nest2");
        var doc = new XmlDocument();
        var rootnode = doc.CreateElement("Root");
        doc.AppendChild(rootnode);

        foreach (var go in plants) {
            var pfb = new PositionedPrefab(go.GetComponent<PrefabIdentifier>());
            var e = doc.CreateElement("customprefab");
            pfb.saveToXML(e);
            doc.DocumentElement.AppendChild(e);
        }

        doc.Save(path);
    }

    public static void resizeCyclopsStorage(SubRoot sub) { //vanilla is 3x6
        var amt = sub && sub.upgradeConsole && sub.upgradeConsole.modules != null
            ? sub.upgradeConsole.modules.GetCount(C2CItems.cyclopsStorage.Info.TechType)
            : 0;
        var slots = 18; //18 vanilla base
        // TODO
        // if (QModManager.API.QModServices.Main.ModPresent("MoreCyclopsUpgrades"))
        //     slots += 6 + (amt * 6) + (amt / 2 * 12); //https://i.imgur.com/JUr54tB.png
        // else
        slots += 18 * amt + 18 * (amt / 2); //https://i.imgur.com/K5UaRHZ.png
        //int w = Math.Min(3+amt, 6);
        //int h = 6+amt*2;
        var w = 3;
        var h = slots / w;
        while (w < 6 && h >= 9) {
            w++;
            while (slots % w != 0)
                w++;
            h = slots / w;
        }

        foreach (var cl in sub.GetComponentsInChildren<CyclopsLocker>()) {
            var sc = cl.GetComponent<StorageContainer>();
            sc.Resize(w, h);
        }
    }

    public static void setupDeathScreen() {
        uGUI.main.respawning.Hide();
        DamageFX.main.ClearHudDamage();
        Player.main.SuffocationReset();
        IngameMenu.main.gameObject.SetActive(true);
        IngameMenu.main.ChangeSubscreen("QuitConfirmation");
        var txt = IngameMenu.main.currentScreen.getChildObject("Header")
            .GetComponent<UnityEngine.UI.Text>();
        txt.text = "You died. Please reload your save.";
        txt.fontSize = 20;
        IngameMenu.main.currentScreen.getChildObject("ButtonNo").SetActive(false);
        var yes = IngameMenu.main.currentScreen.getChildObject("ButtonYes");
        yes.GetComponentInChildren<UnityEngine.UI.Text>().text = "Main Menu";
        yes.transform.localPosition = new Vector3(0, yes.transform.localPosition.y, yes.transform.localPosition.z);
    }

    public static void swapRepulsionCannons() {
        var ii = Inventory.main.quickSlots.heldItem;
        var tt = ii == null || !ii.item ? TechType.None : ii.item.GetTechType();
        if (ii != null && (tt == TechType.PropulsionCannon || tt == TechType.RepulsionCannon)) {
            var to = tt == TechType.PropulsionCannon ? TechType.RepulsionCannon : TechType.PropulsionCannon;
            var selSlot = InventoryUtil.getActiveQuickslot();
            //TechType batt = TechType.None;
            float battCh = -1;
            Pickupable batt = null;
            var pt = ii.item.GetComponent<PlayerTool>();
            var e = pt.energyMixin;
            if (e) {
                /*
                IBattery ib = e.battery;
                if (ib is Battery)
                    batt = ((Battery)ib).GetComponent<Pickupable>();
                    */
                //batt = (e.batterySlot.storedItem == null ? TechType.None : e.batterySlot.storedItem.item.GetTechType());
                batt = e.batterySlot.storedItem.item;
                battCh = e.charge / e.capacity;
                e.batterySlot.RemoveItem();
            }

            Inventory.main.container.forceRemoveItem(ii);
            InventoryUtil.addItem(to);
            var put = Inventory.main.container.getItem(to);
            if (put != null) {
                var pt2 = put.item.GetComponent<PlayerTool>();
                if (batt /* != TechType.None*/) {
                    //pt2.energyMixin.gameObject.EnsureComponent<DelayedBatterySwapCallback>().init(batt, battCh, e).Invoke("apply", 1.5F);
                    pt2.energyMixin.batterySlot.AddItem(batt);
                    pt2.energyMixin.RestoreBattery();
                }

                if (selSlot >= 0)
                    Inventory.main.quickSlots.Select(selSlot);
                SNUtil.writeToChat(
                    "Swapped to " + Language.main.Get(to) + (batt /* != TechType.None*/
                        ? ", with battery '" + Language.main.Get(batt.GetTechType()) + "' (" +
                          (battCh * 100F).ToString("0.0") + "% full)"
                        : "")
                );
            } else {
                SNUtil.writeToChat("Swapped (pro/re)pulsion gun but not found in inventory afterwards?!");
                if (batt /* != TechType.None*/)
                    //InventoryUtil.addItem(batt);
                    Inventory.main.Pickup(batt);
            }
        } else {
            SNUtil.writeToChat("Found no (pro/re)pulsion gun to swap");
        }
    }

    public static void cleanup() {
        var ptc = 0;
        foreach (var pt in UnityEngine.Object.FindObjectsOfType<PlatinumTag>()) {
            pt.gameObject.destroy();
            ptc++;
        }

        SNUtil.writeToChat("Removed " + ptc + " platinum.");
    }
}