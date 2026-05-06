using System;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea;

public class EnvironmentalDamageSystem {
    public static readonly EnvironmentalDamageSystem instance = new();

    public static readonly float ENVIRO_RATE_SCALAR = 4;

    private readonly static Vector3 lavaPitEntranceCenter = new(260.6F, -838F, 688.2F);
    private readonly static Vector3 lavaPitTunnelMid = new(145.8F, -1225F, 528F);
    private readonly static double lavaPitEntranceRadius = 130;
    internal readonly static double lavaPitEntranceDepthStart = 900;
    internal readonly static double lavaPitEntranceDepthMaxTemp = 1100;

    //private readonly static Vector3 auroraPrawnBayCenter = new Vector3(996, 2.5F, -26.5F);//new Vector3(1003, 4, -18);
    //private readonly static Vector3 auroraTopLeftBack = new Vector3(1010, 13, ?);
    //private readonly static Vector3 auroraBottomRightFront = new Vector3(?, ?, ?);
    private readonly static Vector3 auroraFireCeilingTunnel = new(1047.3F, 1, 2);
    internal readonly static Vector3 auroraPrawnBayDoor = new(984, 8.5F, -36.2F);

    private readonly static Vector3 auroraPrawnBayCornerPatch = new(976.95F, 8.85F, -23.55F);
    private readonly static Vector3 auroraPrawnBayLineA1 = new(995, 2.6F, -38.6F);
    private readonly static Vector3 auroraPrawnBayLineA2 = new(1023.5F, 2.6F, -12.7F);
    private readonly static Vector3 auroraPrawnBayLineB1 = new(981.3F, 2.6F, -21.4F);
    private readonly static Vector3 auroraPrawnBayLineB2 = new(1010.9F, 2.6F, 9.9F);

    public readonly static float highO2UsageStart = 400;
    internal readonly static float depthFXRippleStart = 450;
    public readonly static float depthDamageStart = 500;
    public readonly static float depthDamageMax = 600;

    private readonly Dictionary<string, TemperatureEnvironment> temperatures = new();
    private readonly Dictionary<string, float> lrPoisonDamage = new();
    private readonly Dictionary<string, float> lrLeakage = new();

    private readonly SoundManager.SoundData pdaBeep;

    internal CustomHUDWarning lrPoisonHUDWarning;
    internal CustomHUDWarning lrLeakHUDWarning;
    internal CustomHUDWarning extremeHeatHUDWarning;
    internal CustomHUDWarning o2ConsumptionIncreasingHUDWarning;
    internal CustomHUDWarning o2ConsumptionMaxedOutHUDWarning;
    internal CustomHUDWarning infectedLungsHUDWarning;
    internal CustomHUDWarning teleportRecoveryHUDWarning;

    private readonly List<CustomHUDWarning> warnings = [];

    private float cyclopsHeatDamage;
    private float playerHeatDamage;

    private float cyclopsPowerLeak;
    private float vehiclePowerLeak;

    private float playerPressureDamageSince;
    private float playerPressureDamageDuration;

    private float recoveryWarningEndTime;

    internal float TEMPERATURE_OVERRIDE = -1;

    //private GameObject prawnBayHeatRippleCylinder;

    //private DepthRippleFX depthWarningFX1;
    //private DepthDarkeningFX depthWarningFX2;

    private EnvironmentalDamageSystem() {
        registerBiomeEnvironment("ILZCorridor", 90, 8, 0.5F, 40, 9);
        registerBiomeEnvironment("ILZCorridorDeep", 120, 9, 0.5F, 20, 12);
        registerBiomeEnvironment("ILZChamber", 150, 10, 0.5F, 10, 15);
        registerBiomeEnvironment("LavaPit", 200, 12, 0.5F, 8, 20);
        registerBiomeEnvironment("LavaFalls", 300, 15, 0.5F, 5, 25);
        registerBiomeEnvironment("LavaLakes", 350, 18, 0.5F, 2, 40);
        registerBiomeEnvironment("LavaLakes_LavaPool", 400, 18, 0.5F, 2, 40);
        registerBiomeEnvironment("ilzLava", 1200, 24, 0.5F, 0, 100); //in lava
        registerBiomeEnvironment("LavaCastle", 240, 18, 0.5F, 4, 20);
        registerBiomeEnvironment("LavaCastleInner", 360, 18, 0.5F, 4, 20);
        registerBiomeEnvironment("LavaPitEntrance", 320, 15, 0.5F, 5, 25);
        temperatures["ILZChamber_Dragon"] = temperatures["ILZChamber"];
        registerBiomeEnvironment("ILZCastleTunnel", 450, 18, 0.5F, 4, 20); //the lavafall entrance

        registerBiomeEnvironment("AuroraPrawnBay", 150, 10F, 2.5F, 9999, 0);
        registerBiomeEnvironment("AuroraPrawnBayDoor", 200, 40F, 2.5F, 9999, 0);
        registerBiomeEnvironment("AuroraFireCeilingTunnel", 175, 2.5F, 1.5F, 9999, 0);

        lrLeakage["LostRiver_BonesField_Corridor"] = 1;
        lrLeakage["LostRiver_BonesField"] = 1;
        lrLeakage["LostRiver_BonesField_LakePit"] = 1.5F;
        lrLeakage["LostRiver_BonesField_Cave"] = 1;
        lrLeakage["LostRiver_Junction"] = 1;
        lrLeakage["LostRiver_TreeCove"] = 0.9F;
        lrLeakage["LostRiver_Corridor"] = 1;
        lrLeakage["LostRiver_GhostTree_Lower"] = 1;
        lrLeakage["LostRiver_GhostTree"] = 1;
        lrLeakage["LostRiver_GhostTree_Skeleton"] = 1;
        lrLeakage["LostRiver_Canyon"] = 1.75F;
        lrLeakage["LostRiver_SkeletonCave"] = 1.75F; //around the six eye skull
        lrLeakage["Precursor_LostRiverBase"] = 1;

        lrPoisonDamage["LostRiver_BonesField_Corridor"] = 8;
        lrPoisonDamage["LostRiver_GhostTree"] = 8;
        lrPoisonDamage["LostRiver_GhostTree_Skeleton"] = 8;
        lrPoisonDamage["LostRiver_Corridor"] = 8;
        lrPoisonDamage["LostRiver_Canyon"] = 10;
        lrPoisonDamage["LostRiver_SkeletonCave"] = 10; //around the six eye skull
        lrPoisonDamage["LostRiver_BonesField"] = 15;
        lrPoisonDamage["LostRiver_BonesField_Skeleton"] = 15;
        lrPoisonDamage["LostRiver_Junction"] = 15;
        lrPoisonDamage["LostRiver_Junction_Skeleton"] = 15;
        lrPoisonDamage["LostRiver_GhostTree_Lower"] = 15;
        lrPoisonDamage["Precursor_LostRiverBase"] = 15;

        foreach (var kvp in new HashSet<string>(lrLeakage.Keys)) {
            lrLeakage[kvp + "_Water"] = lrLeakage[kvp] * 1.5F;
            lrLeakage[kvp + "_Lake"] = lrLeakage[kvp] * 1.5F;
            lrLeakage[kvp + "_LakePit"] = lrLeakage[kvp] * 1.5F;
        }

        foreach (var kvp in new HashSet<string>(lrPoisonDamage.Keys)) {
            lrPoisonDamage[kvp + "_Water"] = lrPoisonDamage[kvp] * 1.5F;
            lrPoisonDamage[kvp + "_Lake"] = lrPoisonDamage[kvp] * 1.5F;
            lrPoisonDamage[kvp + "_LakePit"] = lrPoisonDamage[kvp] * 1.5F;
        }

        pdaBeep = SoundManager.registerPDASound(SeaToSeaMod.ModDLL, "pda_beep", "Sounds/pdabeep.ogg");
    }

    private void registerBiomeEnvironment(string b, float t, float dmg, float w, int cf, float dmgC) {
        temperatures[b] = new TemperatureEnvironment(b, t, dmg, w, cf, dmgC);
    }

    public void playPDABeep() {
        playPDABeep(Player.main.transform.position);
    }

    public void playPDABeep(Vector3 pos) {
        SoundManager.playSoundAt(pdaBeep, pos, false, -1);
    }

    public bool isPositionInAuroraPrawnBay(Vector3 pos) {
        var d1 = MathUtil.getDistanceToLineSegment(pos, auroraPrawnBayLineA1, auroraPrawnBayLineA2);
        var d2 = MathUtil.getDistanceToLineSegment(pos, auroraPrawnBayLineB1, auroraPrawnBayLineB2);
        var d3 = MathUtil.getDistanceToLineSegment(
            pos,
            (auroraPrawnBayLineA1 + auroraPrawnBayLineB1) / 2F,
            (auroraPrawnBayLineA2 + auroraPrawnBayLineB2) / 2F
        );
        return Math.Min(d1, Math.Min(d3, d2)) <= 6.25 || (pos - auroraPrawnBayCornerPatch).sqrMagnitude <= 36;
    }

    public bool isPlayerInOcean() {
        var ep = Player.main;
        var inWater = !ep.IsInsideWalkable() && Player.main.IsUnderwater() && ep.IsSwimming();
        return inWater && !ep.currentWaterPark && !(isInPrecursor(ep.gameObject) && ep.transform.position.y < -1300);
    }

    public bool isInPrecursor(GameObject go) {
        var biome = getBiome(go).ToLowerInvariant();
        return biome.Contains("prison") || biome.Contains("precursor") || WorldUtil.isInPCFTank(go);
    }

    /*
private GameObject getRippleCylinder() {
if (!prawnBayHeatRippleCylinder) {/*
    GameObject go = ObjectUtil.lookupPrefab("3877d31d-37a5-4c94-8eef-881a500c58bc");
    go = go.getChildObject("Extinguishable_Fire_medium");
    prawnBayHeatRippleCylinder = go.getChildObject("x_Fire_Cylindrical").clone();
    prawnBayHeatRippleCylinder.transform.localScale = new Vector3(0.1F, 0.5F, 0.1F);
    Material m = prawnBayHeatRippleCylinder.GetComponentInChildren<Renderer>().materials[0];
    m.color = new Color(0.07F, 0, 0, 0);
    m.SetColor("_ColorStrength", Color.clear);
    */
    /*
                }
                return prawnBayHeatRippleCylinder;
            }*/

    public void tickTemperatureDamages(TemperatureDamage dmg) {
        if (C2CHooks.skipEnviroDamage)
            return;
        if (DIHooks.GetWorldAge() < 0.25F)
            return;
        if (GameModeUtils.currentEffectiveMode == GameModeOption.Creative)
            return;
        //depthWarningFX1 = Camera.main.gameObject.EnsureComponent<DepthRippleFX>();
        //depthWarningFX2 = Camera.main.gameObject.EnsureComponent<DepthDarkeningFX>();
        //SBUtil.writeToChat("Doing enviro damage on "+dmg+" in "+dmg.gameObject+" = "+dmg.player);
        var biome = getBiome(dmg.gameObject); //Player.main.GetBiomeString();
        var prawn = biome == "AuroraPrawnBay" || biome == "AuroraPrawnBayDoor";
        var aurora = prawn || biome == "AuroraFireCeilingTunnel";
        var diveSuit = dmg.player && dmg.player.HasReinforcedGloves() && dmg.player.HasReinforcedSuit();
        if (aurora && !diveSuit &&
            !PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.AuroraFireWarn).key) &&
            !PDAMessagePrompts.instance.isTriggered(
                PDAMessages.getAttr(PDAMessages.Messages.AuroraFireWarn_NoRad).key
            )) {
            triggerAuroraPrawnBayWarning();
        }

        if (dmg.player && !isPlayerInOcean()) {
            playerPressureDamageDuration = Mathf.Max(0, playerPressureDamageDuration - Time.deltaTime);
            if (playerPressureDamageDuration > 0)
                dmg.liveMixin.TakeDamage(
                    (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 3 : 1) / ENVIRO_RATE_SCALAR,
                    dmg.transform.position,
                    DamageType.Pressure,
                    null
                );
            if (!aurora)
                return;
        }

        //SBUtil.writeToChat("not skipped");
        var temperature = dmg.GetTemperature();
        var te = temperatures.ContainsKey(biome) ? temperatures[biome] : null;
        if (te != null)
            temperature = Mathf.Max(temperature, te.temperature);
        float f = 1;
        float f0 = 1;
        float fw = 0;
        var time = DayNightCycle.main.timePassedAsFloat;
        //SNUtil.writeToChat(biome+" for "+dmg.gameObject);
        if (dmg.player) {
            /*GameObject ripple = getRippleCylinder();
            if (ripple) {
                ripple.SetActive(prawn || (te != null && te.isLavaZone && isPlayerInOcean()));
                ripple.transform.position = Player.main.transform.position+Vector3.down*0.6F;
            }*/
            if ((prawn && !dmg.player.HasReinforcedSuit()) || (te != null && te.isLavaZone && isPlayerInOcean())) {
                var wb = MainCamera.camera.GetComponent<WBOIT>();
                wb.nextTemperatureUpdate = Time.time + 1F;
                wb.temperatureScalar =
                    prawn
                        ? 5
                        : 2F + Mathf.Clamp01(
                            te.temperature - 250 / 150F
                        ); //lava >2x only when the player is free swimming
                wb.temperatureRefractEnabled = true;
                wb.compositeMaterial.EnableKeyword("FX_TEMPERATURE_REFRACT");
                //wb.compositeMaterial.SetTexture(wb.temperatureTexPropertyID, wb.temperatureRefractTex);
                //wb.compositeMaterial.SetFloat(wb.temperaturePropertyID, wb.temperatureScalar);
            }

            f0 = !diveSuit ? 2.5F : 0.4F;
            if (te != null) {
                f = te.damageScalar;
                temperature = te.temperature;
                fw = te.waterScalar;
            }

            float baseVal = 49;
            if (dmg.minDamageTemperature > baseVal && dmg.minDamageTemperature <= 75) { //stop repeating forever
                var add = dmg.minDamageTemperature - baseVal; //how much above default it is
                dmg.minDamageTemperature = baseVal + add * 2;
            }
        }

        if (temperature >= dmg.minDamageTemperature) {
            var num = temperature / dmg.minDamageTemperature;
            num *= dmg.baseDamagePerSecond;
            var amt = num * f * f0 / ENVIRO_RATE_SCALAR;
            if (aurora && dmg.player && biome == "AuroraPrawnBay" && Vector3.Distance(
                    auroraPrawnBayDoor,
                    dmg.player.transform.position
                ) <= 3)
                amt *= 2;
            if (aurora && dmg.player && biome == "AuroraPrawnBay" && dmg.player.IsUnderwaterForSwimming())
                amt *= 0.4F;
            if (aurora && diveSuit)
                amt = 0;
            //SNUtil.writeToChat(biome+" > "+temperature+" / "+dmg.minDamageTemperature+" > "+amt);
            if (amt > 0) {
                var v = dmg.gameObject.GetComponent<Vehicle>();
                if (!v || !(v.docked || v.precursorOutOfWater)) {
                    if (v && v is Exosuit) {
                        foreach (var tt in v.getVehicleUpgrades()) {
                            var f2 = getHeatDamageModuleFactor(tt);
                            amt *= f2;
                        }
                    }

                    dmg.liveMixin.TakeDamage(amt, dmg.transform.position, DamageType.Heat, null);
                    if (dmg.player && !diveSuit) {
                        var s = Player.main.GetComponent<Survival>();
                        s.water = Mathf.Clamp(s.water - amt * fw, 0f, 100f);
                    }

                    if (temperature > 105) { //do not do at vents/geysers
                        playerHeatDamage = time; //this also covers the seamoth
                        if (v && v is SeaMoth) { //only trigger for seamoth
                            PDAManager.getPage("heatdamage").unlock();
                            if (!KnownTech.Contains(C2CItems.heatSink.TechType)) {
                                KnownTech.Add(C2CItems.heatSinkModule.TechType);
                                KnownTech.Add(C2CItems.heatSink.TechType);
                            }
                        }
                    }
                }
            }
        }

        if (dmg.player) {
            var depth = dmg.player.GetDepth();
            var rb = LiquidBreathingSystem.instance.hasLiquidBreathing();
            //depthWarningFX1.setIntensities(rb ? 0 : depth);
            //depthWarningFX2.setIntensities(rb ? 0 : depth);
            if (depth > depthDamageStart && !rb) {
                var f2 = depth >= depthDamageMax
                    ? 1
                    : (float)MathUtil.linterpolate(depth, depthDamageStart, depthDamageMax, 0, 1);
                f2 *= 1 + playerPressureDamageDuration * 0.2F;
                dmg.liveMixin.TakeDamage(
                    (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 50 : 30) * 0.25F * f2 /
                    ENVIRO_RATE_SCALAR,
                    dmg.transform.position,
                    DamageType.Pressure,
                    null
                );
                playerPressureDamageSince = time;
                playerPressureDamageDuration += Time.deltaTime;
                dmg.player.gameObject.removeComponent<HealingOverTime>();
            }

            if (!C2CItems.hasSealedOrReinforcedSuit(out var seal, out var reinf)) {
                //SBUtil.writeToChat(biome+" # "+dmg.gameObject);
                var amt = getLRPoison(biome);
                if (amt > 0) {
                    dmg.liveMixin.TakeDamage(amt / ENVIRO_RATE_SCALAR, dmg.transform.position, DamageType.Poison, null);
                }
            }
        }

        var leak = getLRPowerLeakage(biome) * 0.9F;
        if (leak > 0) {
            var v = dmg.player ? dmg.player.GetVehicle() : dmg.gameObject.FindAncestor<Vehicle>();
            if (triggerPowerLeakage(v, dmg.player, leak)) {
                if (PDAManager.getPage("lostrivershortcircuit").unlock())
                    playPDABeep();
            }

            if (!dmg.player)
                vehiclePowerLeak = time;
        }
    }

    private bool triggerPowerLeakage(Vehicle v, Player ep, float leak) {
        float innerLeakFactor = 1;
        if (v) {
            leak *= getLRLeakFactor(v, out var upgrade);
            if (upgrade)
                innerLeakFactor = 0.5F;
        }

        if (leak > 0) {
            var used = false;
            if (ep) {
                used = triggerPowerLeakage(Inventory.main.container, leak, innerLeakFactor);
                //used = true;
            } else if (v) {
                v.ConsumeEnergy(Math.Min(v.energyInterface.TotalCanProvide(out var trash), leak / ENVIRO_RATE_SCALAR));
                used = true;
                foreach (var sc in v.GetComponentsInChildren<StorageContainer>(true))
                    if (!sc.gameObject.FindAncestor<Player>())
                        triggerPowerLeakage(sc.container, leak, innerLeakFactor);
                foreach (var sc in v.GetComponentsInChildren<SeamothStorageContainer>(true))
                    if (!sc.gameObject.FindAncestor<Player>())
                        triggerPowerLeakage(sc.container, leak, innerLeakFactor);
            }

            return used;
        } else {
            return false;
        }
    }

    internal float getHeatDamageModuleFactor(TechType tt) {
        return tt == TechType.ExosuitThermalReactorModule ? 0.8F : tt + "" == "ExosuitThermalModuleMk2" ? 0.67F : 1;
    }

    internal void triggerAuroraPrawnBayWarning() {
        if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
            PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.AuroraFireWarn).key);
        else
            PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.AuroraFireWarn_NoRad).key);
    }

    internal bool isPlayerRecoveringFromPressure() {
        return playerPressureDamageDuration > 0 || DayNightCycle.main.timePassedAsFloat - playerPressureDamageSince <
            (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 15 : 8);
    }

    internal void resetCooldowns() {
        playerPressureDamageDuration = 0;
        playerPressureDamageSince = -1;
    }

    internal void setRecoveryWarning(float dur) {
        recoveryWarningEndTime = DayNightCycle.main.timePassedAsFloat + dur;
    }

    private float getLRLeakFactor(Vehicle v, out bool hasUpgrade) {
        if (v.docked || v.precursorOutOfWater) {
            hasUpgrade = false;
            return 0;
        }

        float leak = 1;
        hasUpgrade = v.vehicleHasUpgrade(C2CItems.powerSeal.TechType);
        if (hasUpgrade)
            leak *= 0.2F;
        //SBUtil.writeToChat(biome+" # "+dmg.gameObject);
        if (v.playerSits)
            leak *= 2;
        var acid = v.GetComponent<AcidicBrineDamage>();
        if (acid && acid.numTriggers > 0)
            leak *= 8;
        float fb = 0;
        foreach (var mix in v.energyInterface.sources) {
            if (mix && /*mix.capacity > 0 && */!Mathf.Approximately(mix.capacity, 1000))
                fb++;
        }

        //fb /= v.energyInterface.sources.Length;
        leak *= fb;
        return leak;
    }

    private bool triggerPowerLeakage(ItemsContainer c, float leak, float relativeFactor) {
        var found = false;
        foreach (var item in c) {
            if (item != null && item.item.GetTechType() != TechType.PrecursorIonPowerCell &&
                item.item.GetTechType() != TechType.PrecursorIonBattery &&
                item.item.GetTechType() != C2CItems.t2Battery.TechType) {
                var b = item.item.gameObject.GetComponentInChildren<Battery>();
                //SBUtil.writeToChat(item.item.GetTechType()+": "+string.Join(",", (object[])item.item.gameObject.GetComponentsInChildren<MonoBehaviour>()));
                if (b != null && b.capacity > 100) {
                    b.charge = Math.Max(b.charge - leak * b.capacity / 200F * relativeFactor, 0);
                    //SBUtil.writeToChat("Discharging item "+item.item.GetTechType());
                    //used = true;
                    found = true;
                }
            }
        }

        return found;
    }

    public void tickCyclopsDamage(CrushDamage dmg) {
        if (C2CHooks.skipEnviroDamage)
            return;
        if (!dmg.gameObject.activeInHierarchy || !dmg.enabled) {
            return;
        }

        if (dmg.GetCanTakeCrushDamage() && dmg.GetDepth() > dmg.crushDepth) {
            dmg.liveMixin.TakeDamage(C2CHooks.getCrushDamage(dmg), dmg.transform.position, DamageType.Pressure, null);
            if (dmg.soundOnDamage) {
                dmg.soundOnDamage.Play();
            }
        }

        var sub = dmg.gameObject.GetComponentInParent<SubRoot>();
        if (sub != null && sub.isCyclops) {
            var time = DayNightCycle.main.timePassedAsFloat;
            if (VanillaBiomes.Void.IsInBiome(sub.transform.position) &&
                VoidSpikesBiome.instance.getDistanceToBiome(sub.transform.position, true) < 750) {
                if (sub.powerRelay.GetPower() > 0)
                    VoidSpikeLeviathanSystem.instance.triggerCyclopsEMP(sub, time);
            }

            var temp = getLavaHeatDamage(dmg.gameObject);
            //SBUtil.writeToChat("heat: "+temp);
            if (temp != null) {
                //SBUtil.writeToChat("immune: "+immune);
                if (!sub.cyclopsHasUpgrade(C2CItems.cyclopsHeat.TechType)) {
                    dmg.liveMixin.TakeDamage(
                        dmg.damagePerCrush * temp.damageScalar * 0.15F,
                        dmg.transform.position,
                        DamageType.Heat,
                        null
                    );
                    if (dmg.soundOnDamage) {
                        dmg.soundOnDamage.Play();
                    }

                    //DO NOT SPAWN HEAT PDA PAGE
                    if (temp.cyclopsFireChance <= 0 || UnityEngine.Random.Range(0, temp.cyclopsFireChance) == 0) {
                        var key = (CyclopsRooms)UnityEngine.Random.Range(0, Enum.GetNames(typeof(CyclopsRooms)).Length);
                        var fire = dmg.gameObject.GetComponentInParent<SubFire>();
                        fire.CreateFire(fire.roomFires[key]);
                    }

                    cyclopsHeatDamage = time;
                }
            }

            var leak = getLRPowerLeakage(dmg.gameObject);
            //SBUtil.writeToChat("leak "+leak);
            if (leak > 0) {
                leak *= 1.8F; //+80% more for cyclops (still less than 1/3 as fast per % as seamoth and more than 50% slower than prawn)

                var con = dmg.gameObject.GetComponentInParent<SubControl>();
                if (con.cyclopsMotorMode.engineOn)
                    leak *= 1.25F;
                if (con.appliedThrottle)
                    leak *= 1.5F;
                float f = 0;
                foreach (var b in sub.getCyclopsPowerCells()) {
                    if (b && !Mathf.Approximately(b.capacity, 1000))
                        f++;
                }

                f /= 6;
                leak *= f;
                if (leak > 0) {
                    cyclopsPowerLeak = time;
                    sub.powerRelay.ConsumeEnergy(leak * 2.5F, out var trash);
                    foreach (var sc in sub.GetComponentsInChildren<StorageContainer>(true)) {
                        if (!sc.gameObject.FindAncestor<Vehicle>() &&
                            !sc.gameObject
                                .FindAncestor<
                                    Player>()) //skip vehicle because will be included later in the docked call
                            triggerPowerLeakage(sc.container, leak, 1);
                    }

                    foreach (var d in sub.GetComponentsInChildren<VehicleDockingBay>(true)) {
                        if (d.dockedVehicle)
                            triggerPowerLeakage(d.dockedVehicle, null, leak * 0.5F);
                    }

                    if (PDAManager.getPage("lostrivershortcircuit").unlock())
                        playPDABeep();
                }
            }
        }
    }

    public TemperatureEnvironment getLavaHeatDamage(GameObject go) {
        return getLavaHeatDamage(getBiome(go)); //p.GetBiomeString());
    }

    public TemperatureEnvironment getLavaHeatDamage(string biome) {
        return biome != null && temperatures.ContainsKey(biome) ? temperatures[biome] : null;
    }

    public float getLRPoison(GameObject go) {
        return getLRPoison(getBiome(go)); //p.GetBiomeString());
    }

    public float getLRPoison(string biome) {
        return biome != null && lrPoisonDamage.ContainsKey(biome) ? lrPoisonDamage[biome] : 0;
    }

    public float getLRPowerLeakage(GameObject go) {
        return getLRPowerLeakage(getBiome(go)); //p.GetBiomeString());
    }

    public float getLRPowerLeakage(string biome) {
        var ret = biome != null && lrLeakage.ContainsKey(biome) ? lrLeakage[biome] : -1;
        if (ret > 0 && !SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
            ret *= 0.8F;
        return ret;
    }

    public string getBiome(GameObject go) {
        return !go ? null : getBiome(go.transform.position);
    }

    public string getBiome(Vector3 pos) {
        if (!WaterBiomeManager.main)
            return "";
        var ret = WaterBiomeManager.main.GetBiome(pos);
        if (string.IsNullOrEmpty(ret))
            ret = "void";
        //SNUtil.writeToChat(ret);
        if (ret.ToLowerInvariant().Contains("precursor"))
            return ret;
        if (ret == "ILZCorridor" && pos.y < -1175)
            ret = "ILZCorridorDeep";
        if (ret == "ILZCastleChamber")
            ret = "LavaCastleInner";
        if (ret.StartsWith("ILZ", StringComparison.InvariantCultureIgnoreCase)) {
            var rad = Vector3.Distance(WorldUtil.lavaCastleCenter, pos);
            if (rad < WorldUtil.lavaCastleInnerRadius)
                ret = "LavaCastleInner";
            else if (rad < WorldUtil.lavaCastleRadius)
                ret = "LavaCastle";
        }

        if ((pos.y <= -lavaPitEntranceDepthStart && MathUtil.isPointInCylinder(
                lavaPitEntranceCenter,
                pos,
                lavaPitEntranceRadius,
                999
            )) || Vector3.Distance(lavaPitTunnelMid, pos) <= 60)
            ret = "LavaPitEntrance";
        if (isPositionInAuroraPrawnBay(pos))
            ret = "AuroraPrawnBay";
        if (Vector3.Distance(auroraPrawnBayDoor, pos) <= 3)
            ret = "AuroraPrawnBayDoor";
        if (Vector3.Distance(auroraFireCeilingTunnel, pos) <= 9)
            ret = "AuroraFireCeilingTunnel";
        return ret;
    }

    public float getWaterTemperature(Vector3 pos) {
        var biome = getBiome(pos);
        var temp = getLavaHeatDamage(biome);
        var ret = temp != null ? temp.temperature : -1000;
        if (biome == "ILZCorridor" && pos.y <= -1100 && pos.y >= -1175) {
            ret = (float)MathUtil.linterpolate(
                -pos.y,
                1100,
                1175,
                temperatures["ILZCorridor"].temperature,
                temperatures["ILZCorridorDeep"].temperature
            );
        }

        if (biome == "LavaPitEntrance" && pos.y >= -lavaPitEntranceDepthMaxTemp) {
            ret = (float)MathUtil.linterpolate(
                -pos.y,
                lavaPitEntranceDepthStart,
                lavaPitEntranceDepthMaxTemp,
                WaterTemperatureSimulation.main.GetTemperature(
                    lavaPitEntranceCenter.SetY(-lavaPitEntranceDepthStart + 5)
                ),
                temperatures["LavaPitEntrance"].temperature
            );
        }

        return ret;
    }

    public float getPlayerO2Rate(Player ep) {
        if (C2CHooks.skipO2)
            return 3F;
        var mode = ep.mode;
        if (mode != Player.Mode.Normal && mode - Player.Mode.Piloting <= 1) {
            return 3f;
        }

        if (LiquidBreathingSystem.instance.hasLiquidBreathing()) {
            return 3f;
        }

        if (Inventory.main.equipment.GetTechTypeInSlot("Head") == TechType.Rebreather &&
            ep.GetDepth() < depthDamageStart) {
            return 3f;
        }

        switch (ep.GetDepthClass()) {
            case Ocean.DepthClass.Safe:
                return 3f;
            case Ocean.DepthClass.Unsafe:
                return 2.25f;
            case Ocean.DepthClass.Crush:
                return 1.5f;
        }

        return 99999f;
    }

    public float getPlayerO2Use(Player ep, float breathingInterval, int depthClass) {
        if (C2CHooks.skipO2)
            return 1;
        if (!GameModeUtils.RequiresOxygen())
            return 0;
        float num = 1;
        if (ep.mode != Player.Mode.Piloting && ep.mode != Player.Mode.LockedPiloting && isPlayerInOcean()) {
            var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
            var hasRebreatherV2 = Inventory.main.equipment.GetTechTypeInSlot("Head") == C2CItems.rebreatherV2.TechType;
            var hasRebreather = hasRebreatherV2 ||
                                Inventory.main.equipment.GetTechTypeInSlot("Head") == TechType.Rebreather;
            var rebreatherV2Functional = hasRebreatherV2 && !isRecoveryWarningActive();
            if (!hasRebreather) {
                if (depthClass == 2) {
                    num = 1.5F;
                } else if (depthClass == 3) {
                    num = 2;
                }
            }

            if (depthClass >= 3) {
                var liquid = LiquidBreathingSystem.instance.hasLiquidBreathing();
                var depth = Player.main.GetDepth();
                var increaseStart = highO2UsageStart;
                var rate = 10F;
                if (liquid) {
                    increaseStart = 99999;
                } else if (rebreatherV2Functional) {
                    increaseStart = depthDamageStart;
                    rate = 4F;
                } else {
                    rate = depth > depthDamageStart ? 8F : 10F;
                }

                if (hard) {
                    rate *= 0.8F; //do NOT adjust the depth
                }

                if (depth >= increaseStart) {
                    num = 2.5F + Math.Min(27.5F, (Player.main.GetDepth() - increaseStart) / rate);
                }

                if (!liquid) {
                    if (hard && depth > highO2UsageStart) {
                        num *= (float)MathUtil.linterpolate(depth, highO2UsageStart, depthDamageStart, 1, 1.5F, true);
                    }

                    if (depth >= depthDamageStart) {
                        num *= 1 + playerPressureDamageDuration * 0.2F;
                    }
                }
                //SNUtil.writeToChat(depth.ToString("000.0")+"/"+increaseStart+"&"+rate+">"+num.ToString("00.000"));
            }

            if (LiquidBreathingSystem.instance.hasReducedCapacity())
                num *= hard ? 5 : 3;
        }

        return breathingInterval * num;
    }

    public void tickPlayerEnviroAlerts(RebreatherDepthWarnings warn) {
        if (C2CHooks.skipEnviroDamage)
            return;
        if (!(warn.alerts[0] is EnviroAlert))
            upgradeAlertSystem(warn);

        if (DIHooks.GetWorldAge() < 0.25F)
            return;

        var flagged = false;
        var inOcean = isPlayerInOcean();
        for (var i = 0; i < warnings.Count; i++) {
            var w = warnings[i];
            if (!flagged && w.shouldShow(Player.main, inOcean)) {
                //SNUtil.writeToChat("Activated HUD warn "+w.getText());
                w.setActive(true);
                flagged = true;
            } else {
                w.setActive(false);
            }
        }

        if (!inOcean) {
            return;
        }

        foreach (EnviroAlert ee in warn.alerts) {
            //SNUtil.writeToChat(ee+" : "+ee.isActive());
            if (!ee.alertCooldown && !ee.wasActiveLastTick && ee.isActive()) {
                ee.fire(warn);
            } else {
                ee.wasActiveLastTick = false;
            }
        }
    }

    private void upgradeAlertSystem(RebreatherDepthWarnings warn) {
        List<EnviroAlert> li = [];
        foreach (var a in warn.alerts) {
            var e = new EnviroAlert(a);
            e.preventiveItem.Add(C2CItems.rebreatherV2.TechType);
            li.Add(e);
        }

        warn.alerts.Clear();
        warn.alerts.AddRange(li);

        var crush = new EnviroAlert(
            warn,
            ep => ep.GetDepth() >= depthDamageStart && !LiquidBreathingSystem.instance.hasLiquidBreathing(),
            SeaToSeaMod.MiscLocale.getEntry("deepair")
        );
        crush.preventiveItem.Clear();
        warn.alerts.Add(crush);

        var o2Up = new O2IncreasingAlert(warn);
        warn.alerts.Add(o2Up);

        var infLung = new InfectedLungAlert(warn);
        infLung.preventiveItem.Clear();
        warn.alerts.Add(infLung);

        var poison = new EnviroAlert(
            warn,
            p => getLRPoison(p.gameObject) > 0,
            SeaToSeaMod.MiscLocale.getEntry("lrpoison")
        );
        poison.preventiveItem.Clear();
        poison.preventiveItem.Add(C2CItems.sealSuit.TechType);
        poison.preventiveItem.Add(TechType.ReinforcedDiveSuit);
        warn.alerts.Add(poison);

        //GameObject hud = uGUI.main.screenCanvas.getChildObject("HUD/Content");
        var hudTemplate = uGUI.main.GetComponentInChildren<uGUI_RadiationWarning>(true).gameObject;
        lrPoisonHUDWarning = createHUDWarning(
            hudTemplate,
            "chemwarn",
            () => poison.isActive() && isPlayerInOcean(),
            50,
            new Color(0, 1F, 0.5F, 1)
        );
        o2ConsumptionIncreasingHUDWarning = createHUDWarning(
            hudTemplate,
            "o2warn",
            () => o2Up.isActive() && isPlayerInOcean() && !crush.isActive(),
            10
        );
        infectedLungsHUDWarning = createHUDWarning(
            hudTemplate,
            "influngwarn",
            () => infLung.isActive() && !crush.isActive(),
            10
        ); //, new Color(0.25F, 1F, 0.25F, 1));
        o2ConsumptionMaxedOutHUDWarning = createHUDWarning(
            hudTemplate,
            "pressurewarn",
            () => crush.isActive() && isPlayerInOcean(),
            20
        );
        lrLeakHUDWarning = createHUDWarning(hudTemplate, "leakwarn", isLeakingLRPower, 0, new Color(1F, 1F, 0.2F, 1));
        extremeHeatHUDWarning = createHUDWarning(
            hudTemplate,
            "heatwarn",
            isTakingHeatDamage,
            100,
            new Color(1, 0.875F, 0.75F, 1)
        );
        extremeHeatHUDWarning.showWhenNotSwimming = (ep) => true;
        lrLeakHUDWarning.showWhenNotSwimming = (ep) =>
            ep.GetVehicle() || SNUtil.GetControllingCamera(ep) ||
            (ep.currentSub && ep.currentSub.isCyclops && ep.isPiloting);
        teleportRecoveryHUDWarning = createHUDWarning(
            hudTemplate,
            "teleportrecovery",
            isRecoveryWarningActive,
            -50
        ); //, new Color(0.25F, 1F, 0.25F, 1));
        teleportRecoveryHUDWarning.showWhenNotSwimming = (ep) => true;
        warnings.Sort();
    }

    private bool isLeakingLRPower() {
        var ep = Player.main;
        if (!ep)
            return false;
        MapRoomCamera cam = SNUtil.GetControllingCamera(ep);
        //SNUtil.writeToChat(cam+"");
        if (cam && getLRPowerLeakage(cam.gameObject) > 0)
            return true;
        var v = ep.GetVehicle();
        if (v || (ep.currentSub && ep.currentSub.isCyclops)) {
            if (getLRPowerLeakage(ep.gameObject) <= 0)
                return false;
            if (v && getLRLeakFactor(v, out var upgrade) <= 0)
                return false;
            var time = DayNightCycle.main.timePassedAsFloat;
            return time - vehiclePowerLeak <= 1 || time - cyclopsPowerLeak <= 5;
        }

        return false;
    }

    private bool isTakingHeatDamage() {
        var ep = Player.main;
        if (!ep)
            return false;
        if (getLavaHeatDamage(ep.gameObject) == null)
            return false;
        var time = DayNightCycle.main.timePassedAsFloat;
        return time - playerHeatDamage <= 1 || time - cyclopsHeatDamage <= 5;
    }

    private bool isRecoveryWarningActive() {
        return recoveryWarningEndTime > 0 && DayNightCycle.main.timePassedAsFloat <= recoveryWarningEndTime;
    }

    private CustomHUDWarning createHUDWarning(
        GameObject template,
        string key,
        EnviroAlert e,
        int pri,
        Color? c = null
    ) {
        return createHUDWarning(template, key, e.isActive, pri, c);
    }

    private CustomHUDWarning createHUDWarning(GameObject template, string key, Func<bool> f, int pri, Color? c = null) {
        var go = template.clone().setName("CustomHudWarning_" + key);
        var rad = go.GetComponent<uGUI_RadiationWarning>();
        var warn = go.EnsureComponent<CustomHUDWarning>();
        warn.replace(rad, f);
        warn.setText(SeaToSeaMod.MiscLocale.getEntry("HUDAlerts").getString(key), c);
        warn.setTexture(TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/HUD/" + key));
        warn.transform.SetParent(template.transform.parent, false);
        warn.priority = pri;
        go.removeComponent<uGUI_RadiationWarning>();
        SNUtil.Log("Created custom hud warning " + go);
        go.SetActive(true);
        warnings.Add(warn);
        return warn;
    }
}

public class CustomHUDWarning : MonoBehaviour, IComparable {
    private GameObject overlay;
    private TextMeshProUGUI text;
    private Func<bool> condition;
    public bool forceShow;
    internal Func<Player, bool> showWhenNotSwimming = (ep) => false;

    public int priority;

    internal void replace(uGUI_RadiationWarning rad, Func<bool> f) {
        text = rad.text;
        overlay = rad.warning;
        overlay.transform.SetParent(gameObject.transform, false);
        text.transform.SetParent(overlay.transform, false);
        condition = f;
    }

    internal void setTexture(Texture2D tex) {
        var img = overlay.GetComponentInChildren<Image>();
        img.sprite = img.sprite.SetTexture(tex);
    }

    internal void setText(string s, Color? c = null) {
        text.text = s;
        if (c == null || !c.HasValue)
            c = Color.white;
        text.color = c.Value;
    }

    internal string getText() {
        return text.text;
    }

    public bool shouldShow(Player ep, bool inOcean) {
        return forceShow || ((showWhenNotSwimming(ep) || inOcean) && condition());
    }

    public void setActive(bool active) {
        if (overlay)
            overlay.SetActive(active);
    }

    public int CompareTo(object obj) {
        return obj is CustomHUDWarning warning ? warning.priority.CompareTo(priority) : 0; //inverse to put bigger first
    }
}

internal abstract class DepthFX : MonoBehaviour {
    protected float strength;
    protected float lastUpdate;

    private void OnPreRender() {
        if (DayNightCycle.main.timePassedAsFloat - lastUpdate >= 3) {
            strength *= 0.95F;
        }

        enabled = strength > 0.01;
    }

    internal void setIntensities(float depth) {
        lastUpdate = DayNightCycle.main.timePassedAsFloat;
        strength = calculateIntensity(depth);
        enabled = strength > 0.01;
    }

    protected abstract float calculateIntensity(float depth);
}

internal class DepthRippleFX : DepthFX {
    protected override float calculateIntensity(float depth) {
        return (float)MathUtil.linterpolate(
            depth,
            EnvironmentalDamageSystem.depthFXRippleStart,
            EnvironmentalDamageSystem.depthDamageStart,
            0,
            1,
            true
        );
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        var mat = gameObject.GetComponent<MesmerizedScreenFX>().mat;
        mat.SetFloat(ShaderPropertyID._Amount, strength);
        mat.SetColor(ShaderPropertyID._ColorStrength, new Color(0, 0, 0, strength));
        Graphics.Blit(source, destination, mat);
    }
}

internal class DepthDarkeningFX : DepthFX {
    protected override float calculateIntensity(float depth) {
        return (float)MathUtil.linterpolate(
            depth,
            EnvironmentalDamageSystem.depthDamageStart,
            EnvironmentalDamageSystem.depthDamageMax,
            0,
            1,
            true
        );
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) { /*
        //if (!intermediaryTexture || !intermediaryTexture.IsCreated() || intermediaryTexture.height != source.height || intermediaryTexture.width != source.width)
        //	intermediaryTexture = new RenderTexture(destination ? destination.descriptor : source.descriptor);
        RenderTexture intermediaryTexture = RenderTexture.GetTemporary(destination ? destination.descriptor : source.descriptor);
        Material mat1 = gameObject.GetComponent<MesmerizedScreenFX>().mat;
        Material mat2 = gameObject.GetComponent<CyclopsSmokeScreenFX>().mat;
        mat1.SetFloat(ShaderPropertyID._Amount, rippleStrength);
        //mat1.color = new Color(0, 0, 0, 1);
        mat1.SetColor(ShaderPropertyID._ColorStrength, new Color(0, 0, 0, 1));
        mat2.color = new Color(0, 0, 0, darkening*0.95F);
        Graphics.Blit(source, intermediaryTexture, mat1);
        Graphics.Blit(intermediaryTexture, destination, mat2);
        intermediaryTexture.Release();*/
        //Graphics.Blit(source, destination);

        var mat = gameObject.GetComponent<CyclopsSmokeScreenFX>().mat;
        mat.SetFloat(ShaderPropertyID._Amount, strength);
        mat.color = new Color(0, 0, 0, strength * 1F);
        Graphics.Blit(source, destination, mat);
    }
}

public class TemperatureEnvironment {
    public readonly float temperature;
    public readonly float damageScalar;
    public readonly float waterScalar;
    public readonly float damageScalarCyclops;
    public readonly int cyclopsFireChance;
    public readonly string biome;
    public readonly bool isLavaZone;

    internal TemperatureEnvironment(string b, float t, float dmg, float w, int cf, float dmgC) {
        biome = b;
        isLavaZone = b.StartsWith("lava", StringComparison.InvariantCultureIgnoreCase) ||
                     b.StartsWith("ilz", StringComparison.InvariantCultureIgnoreCase) || b.StartsWith(
                         "alz",
                         StringComparison.InvariantCultureIgnoreCase
                     );
        temperature = t;
        damageScalar = dmg;
        waterScalar = w;
        cyclopsFireChance = cf;
        damageScalarCyclops = dmgC;
    }

    public override string ToString() {
        return
            $"[TemperatureEnvironment Temperature={temperature}, DamageScalar={damageScalar}, CyclopsFireChance={cyclopsFireChance}]";
    }
}

internal class O2IncreasingAlert : EnviroAlert {
    internal O2IncreasingAlert(RebreatherDepthWarnings warn)
        : base(
            warn,
            ep => ep.GetDepth() >= EnvironmentalDamageSystem.highO2UsageStart &&
                  Inventory.main.equipment.GetTechTypeInSlot("Head") != C2CItems.rebreatherV2.TechType,
            null,
            null
        ) {
        preventiveItem.Clear();
    }

    internal override void fire(RebreatherDepthWarnings warn) {
        base.fire(warn);

        EnvironmentalDamageSystem.instance.playPDABeep();
        PDAManager.getPage("deepairuse").unlock();
    }

    public override string ToString() {
        return "o2 increase warning";
    }
}

internal class InfectedLungAlert : EnviroAlert {
    internal InfectedLungAlert(RebreatherDepthWarnings warn)
        : base(
            warn,
            ep => LiquidBreathingSystem.instance.hasReducedCapacity() &&
                  EnvironmentalDamageSystem.instance.isPlayerInOcean(),
            null,
            null
        ) {
        preventiveItem.Clear();
    }

    internal override void fire(RebreatherDepthWarnings warn) {
        base.fire(warn);

        EnvironmentalDamageSystem.instance.playPDABeep();
        var msg = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE)
            ? PDAMessages.Messages.LiquidBreathingSelfScanHard
            : PDAMessages.Messages.LiquidBreathingSelfScanEasy;
        PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(msg).key);
    }

    public override string ToString() {
        return "infected lungs warning";
    }
}

internal class EnviroAlert : RebreatherDepthWarnings.DepthAlert {
    internal List<TechType> preventiveItem = [TechType.Rebreather];
    internal readonly Func<Player, bool> applicability;
    internal bool wasActiveLastTick;

    internal EnviroAlert(RebreatherDepthWarnings warn, Func<Player, bool> f, string pda, SoundManager.SoundData? snd) {
        if (!string.IsNullOrEmpty(pda)) {
            alert = warn.gameObject.AddComponent<PDANotification>();
            alert.text = pda;
            if (snd != null && snd.HasValue)
                alert.sound = snd.Value.asset;
        }

        applicability = f;
    }

    internal EnviroAlert(RebreatherDepthWarnings warn, Func<Player, bool> f, XMLLocale.LocaleEntry e)
        : this(warn, f, e.desc, SoundManager.registerPDASound(SeaToSeaMod.ModDLL, "enviroAlert_" + e.key, e.pda)) {
    }

    internal EnviroAlert(RebreatherDepthWarnings warn, int depth, XMLLocale.LocaleEntry e)
        : this(warn, depth, e.desc, SoundManager.registerPDASound(SeaToSeaMod.ModDLL, "enviroAlert_" + e.key, e.pda)) {
    }

    internal EnviroAlert(RebreatherDepthWarnings warn, int depth, string pda, SoundManager.SoundData snd)
        : this(warn, null, pda, snd) {
        alertDepth = depth;
    }

    internal EnviroAlert(RebreatherDepthWarnings.DepthAlert from) {
        alertDepth = from.alertDepth;
        alert = from.alert;
        alertCooldown = from.alertCooldown;
    }

    internal virtual void fire(RebreatherDepthWarnings warn) {
        alertCooldown = true;
        wasActiveLastTick = true;
        //SNUtil.writeToChat("Firing enviro alert "+this+" when "+Player.main.GetDepth());
        if (alert)
            alert.Play();
        warn.StartCoroutine(warn.ResetAlertCD(this));
    }

    internal bool isActive() {
        var p = Player.main;
        var valid = applicability != null
            ? applicability(p)
            : p.GetDepth() >= alertDepth && GameModeUtils.RequiresOxygen();
        if (!valid)
            return false;
        foreach (var prevent in preventiveItem) {
            if (Inventory.main.equipment.GetCount(prevent) != 0)
                return false;
        }

        return true;
    }

    public override string ToString() {
        return alert.text + " @ " + alertDepth + "/" + applicability;
    }
}