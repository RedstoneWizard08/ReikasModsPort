using System.Collections;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class RescueSystem {
    internal static readonly HashSet<BiomeBase> safeBiomes = [];
    //safe at surface

    private static readonly SoundManager.SoundData rescueSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "rescuewarp",
        "Sounds/rescue.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 64); }
    );

    private static UnityEngine.UI.Button rescuePDAButton;

    static RescueSystem() {
        safeBiomes.Add(VanillaBiomes.Shallows);
        safeBiomes.Add(VanillaBiomes.Kelp);
        safeBiomes.Add(VanillaBiomes.Redgrass);
        safeBiomes.Add(VanillaBiomes.Mushroom);
        safeBiomes.Add(VanillaBiomes.Grandreef);
        safeBiomes.Add(VanillaBiomes.Sparse);
        safeBiomes.Add(VanillaBiomes.Treader);
    }

    public static void createRescuePDAButton() {
        if (rescuePDAButton)
            return;
        rescuePDAButton = SNUtil.CreatePdauiButtonUnderTab<uGUI_InventoryTab>(
            TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/RescueUIBtn"),
            requestRescue
        ).setName("RescueButton");
        rescuePDAButton.transform.localPosition = new Vector3(
            -12,
            rescuePDAButton.transform.localPosition.y,
            rescuePDAButton.transform.localPosition.z
        );
        rescuePDAButton.GetComponent<UnityEngine.UI.Image>().color = new Color(1.5F, 0.5F, 0.5F, 1);
    }

    public static Vector3 getRandomSafePosition() {
        //surface in any of the following biomes: shallows, kelp, red grass, grand reef, sparse reef, mushroom forests
        var range = new Vector3(1500, 0, 1500);
        var ctr = Vector3.down;
        var rand = MathUtil.getRandomVectorAround(ctr, range);
        var bb = BiomeBase.GetBiome(rand);
        while (!safeBiomes.Contains(bb)) {
            rand = MathUtil.getRandomVectorAround(ctr, range);
            bb = BiomeBase.GetBiome(rand);
        }

        return rand;
    }

    public static void requestRescue() {
        Player.main.GetPDA().Close();
        Player.main.gameObject.EnsureComponent<TeleportCallback>().StartCoroutine("raiseConfirmationDialog");
    }

    public static bool rescue() {
        if (!Player.main.currentSub)
            return false;
        HashSet<TechType> allowedReturn = [
            TechType.Builder,
            TechType.Welder,
            TechType.Scanner,
            TechType.Knife,
            TechType.HeatBlade,
            //TechType.StasisRifle,
            TechType.AirBladder,
            TechType.Seaglide,
            TechType.Fins,
            TechType.UltraGlideFins,
            TechType.SwimChargeFins,
            TechType.ReinforcedDiveSuit,
            TechType.ReinforcedGloves,
            TechType.RadiationSuit,
            TechType.RadiationHelmet,
            TechType.RadiationGloves,
            TechType.WaterFiltrationSuit,
            TechType.Tank,
            TechType.DoubleTank,
            TechType.PlasteelTank,
            TechType.HighCapacityTank,
            TechType.Rebreather,
            C2CItems.sealSuit.TechType,
            C2CItems.sealGloves.TechType,
            C2CItems.liquidTank.TechType,
            C2CItems.rebreatherV2.TechType,
        ];
        foreach (var has in Inventory.main.container.GetItemTypes()) {
            if (!allowedReturn.Contains(has))
                return false;
        }

        float dur = 60 * 20; //20 min
        Drunk.add(dur).intensity = 0.5F; //only slow player and make them woozy at 50% power
        HealthModifier.add(2.5F, dur); //2.5x damage for the 20 min
        EnvironmentalDamageSystem.instance.setRecoveryWarning(dur);
        //O2ConsumptionRateModifier.add(1.5F, dur); //x1.5 O2 use for the 20 min

        uGUI_PlayerDeath.main.SendMessage(
            "TriggerDeathVignette",
            uGUI_PlayerDeath.DeathTypes.FadeToBlack,
            SendMessageOptions.RequireReceiver
        );
        Player.main.gameObject.SendMessage("EnableHeadCameraController", null, SendMessageOptions.RequireReceiver);
        Player.main.GetPDA().Close();
        //if (Player.main.deathMusic)
        //	Player.main.deathMusic.StartEvent();
        uGUI.main.overlays.Set(0, 1f);
        MainCameraControl.main.enabled = false;
        Player.main.playerController.inputEnabled = false;
        Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);
        Player.main.GetPDA().SetIgnorePDAInput(true);
        Player.main.playerController.SetEnabled(false);
        Player.main.FreezeStats();
        Player.main.gameObject.EnsureComponent<TeleportCallback>().StartCoroutine("triggerTeleportCutscene");
        return true;
    }

    private class TeleportCallback : MonoBehaviour {
        private IEnumerator raiseConfirmationDialog() {
            yield return new WaitForSeconds(0.67F);
            var root = IngameMenu.main.gameObject.clone().setName("RescueConfirmation");
            root.removeChildObject("PleaseWait");
            root.removeChildObject("Options");
            root.removeChildObject("Feedback");
            root.removeChildObject("Developer");
            root.removeChildObject("QuitConfirmationWithSaveWarning");
            root.removeChildObject("Legend");
            root.removeChildObject("Main");
            //GameObject main = root.getChildObject("Main");
            var go = root.getChildObject("QuitConfirmation");
            root.removeComponent<IngameMenu>();
            root.removeComponent<LanguageUpdater>();
            go.removeComponent<IngameMenuQuitConfirmation>();
            var grp = go.EnsureComponent<uGUI_InputGroup>();
            grp.Select(false);
            UWE.FreezeTime.Begin(UWE.FreezeTime.Id.ApplicationFocus);
            var txt = go.getChildObject("Header").GetComponent<UnityEngine.UI.Text>();
            txt.text = "Are you sure you want to do this?";
            txt.fontSize = 20;
            var yes = go.getChildObject("ButtonYes");
            var b = yes.GetComponentInChildren<UnityEngine.UI.Button>();
            var yesBtn = b.gameObject;
            var img = b.image;
            b.destroy();
            b = yesBtn.EnsureComponent<UnityEngine.UI.Button>();
            b.image = img;
            root.transform.SetParent(IngameMenu.main.gameObject.transform.parent);
            root.transform.localPosition = IngameMenu.main.gameObject.transform.localPosition;
            root.SetActive(true);
            go.SetActive(true);
            b.onClick.AddListener(() => {
                    unlockUI(grp);
                    if (!rescue()) {
                        if (Player.main.currentSub)
                            SNUtil.WriteToChat(
                                "You can only carry certified low-power Alterra equipment during the emergency rescue warp."
                            );
                        else
                            SNUtil.WriteToChat(
                                "Rescue warp can only be initiate from inside an Alterra seabase or mobile base platform."
                            );
                    }
                }
            );
            go.getChildObject("ButtonNo").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                    unlockUI(grp);
                }
            );
            yield break;
        }

        private void unlockUI(uGUI_InputGroup grp) {
            grp.Deselect();
            UWE.FreezeTime.End(UWE.FreezeTime.Id.ApplicationFocus);
            grp.transform.parent.gameObject.destroy(false);
        }

        private IEnumerator triggerTeleportCutscene() {
            yield return new WaitForSeconds(1F);
            SoundManager.playSoundAt(rescueSound, Player.main.transform.position, false, 40, 1);
            yield return new WaitForSeconds(2F);
            UWE.Utils.EnterPhysicsSyncSection();
            gameObject.SendMessage("DisableHeadCameraController", null, SendMessageOptions.RequireReceiver);
            uGUI.main.respawning.Show();
            Player.main.ToNormalMode(true);
            if (AtmosphereDirector.main) {
                AtmosphereDirector.main.ResetDirector();
            }

            yield return new WaitForSeconds(1f);
            while (!LargeWorldStreamer.main.IsWorldSettled()) {
                yield return UWE.CoroutineUtils.waitForNextFrame;
            }

            var pos = getRandomSafePosition();
            Player.main.SetPosition(pos);

            uGUI.main.respawning.Hide();
            if (Player.main.liveMixin) {
                Player.main.liveMixin.health = 5;
            }

            Player.main.oxygenMgr.AddOxygen(1000f);
            DamageFX.main.ClearHudDamage();
            Player.main.SuffocationReset();
            yield return null;
            Player.main.precursorOutOfWater = false;
            Player.main.SetDisplaySurfaceWater(true);
            Player.main.UnfreezeStats();
            Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
            Player.main.GetPDA().SetIgnorePDAInput(false);
            Player.main.playerController.inputEnabled = true;
            Player.main.playerController.SetEnabled(true);
            Player.main.SetCurrentSub(null);
            yield return new WaitForSeconds(1f);
            UWE.Utils.ExitPhysicsSyncSection();
            SNUtil.WriteToChat("You wake up an unknown amount of time later.");
            this.destroy(false);
            yield break;
        }
    }
}