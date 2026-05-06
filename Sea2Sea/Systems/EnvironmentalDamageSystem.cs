using System;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea;

public class EnvironmentalDamageSystem {
    public static readonly EnvironmentalDamageSystem Instance = new();

    public static readonly float EnviroRateScalar = 4;

    private readonly static Vector3 LavaPitEntranceCenter = new(260.6F, -838F, 688.2F);
    private readonly static Vector3 LavaPitTunnelMid = new(145.8F, -1225F, 528F);
    private readonly static double LavaPitEntranceRadius = 130;
    internal readonly static double LavaPitEntranceDepthStart = 900;
    internal readonly static double LavaPitEntranceDepthMaxTemp = 1100;

    //private readonly static Vector3 auroraPrawnBayCenter = new Vector3(996, 2.5F, -26.5F);//new Vector3(1003, 4, -18);
    //private readonly static Vector3 auroraTopLeftBack = new Vector3(1010, 13, ?);
    //private readonly static Vector3 auroraBottomRightFront = new Vector3(?, ?, ?);
    private readonly static Vector3 AuroraFireCeilingTunnel = new(1047.3F, 1, 2);
    internal readonly static Vector3 AuroraPrawnBayDoor = new(984, 8.5F, -36.2F);

    private readonly static Vector3 AuroraPrawnBayCornerPatch = new(976.95F, 8.85F, -23.55F);
    private readonly static Vector3 AuroraPrawnBayLineA1 = new(995, 2.6F, -38.6F);
    private readonly static Vector3 AuroraPrawnBayLineA2 = new(1023.5F, 2.6F, -12.7F);
    private readonly static Vector3 AuroraPrawnBayLineB1 = new(981.3F, 2.6F, -21.4F);
    private readonly static Vector3 AuroraPrawnBayLineB2 = new(1010.9F, 2.6F, 9.9F);

    public readonly static float HighO2UsageStart = 400;
    internal readonly static float DepthFXRippleStart = 450;
    public readonly static float DepthDamageStart = 500;
    public readonly static float DepthDamageMax = 600;

    private readonly Dictionary<string, TemperatureEnvironment> _temperatures = new();
    private readonly Dictionary<string, float> _lrPoisonDamage = new();
    private readonly Dictionary<string, float> _lrLeakage = new();

    private readonly SoundManager.SoundData _pdaBeep;

    internal CustomHUDWarning LrPoisonHUDWarning;
    internal CustomHUDWarning LrLeakHUDWarning;
    internal CustomHUDWarning ExtremeHeatHUDWarning;
    internal CustomHUDWarning O2ConsumptionIncreasingHUDWarning;
    internal CustomHUDWarning O2ConsumptionMaxedOutHUDWarning;
    internal CustomHUDWarning InfectedLungsHUDWarning;
    internal CustomHUDWarning TeleportRecoveryHUDWarning;

    private readonly List<CustomHUDWarning> _warnings = [];

    private float _cyclopsHeatDamage;
    private float _playerHeatDamage;

    private float _cyclopsPowerLeak;
    private float _vehiclePowerLeak;

    private float _playerPressureDamageSince;
    private float _playerPressureDamageDuration;

    private float _recoveryWarningEndTime;

    internal float TemperatureOverride = -1;

    //private GameObject prawnBayHeatRippleCylinder;

    //private DepthRippleFX depthWarningFX1;
    //private DepthDarkeningFX depthWarningFX2;

    private EnvironmentalDamageSystem() {
        RegisterBiomeEnvironment("ILZCorridor", 90, 8, 0.5F, 40, 9);
        RegisterBiomeEnvironment("ILZCorridorDeep", 120, 9, 0.5F, 20, 12);
        RegisterBiomeEnvironment("ILZChamber", 150, 10, 0.5F, 10, 15);
        RegisterBiomeEnvironment("LavaPit", 200, 12, 0.5F, 8, 20);
        RegisterBiomeEnvironment("LavaFalls", 300, 15, 0.5F, 5, 25);
        RegisterBiomeEnvironment("LavaLakes", 350, 18, 0.5F, 2, 40);
        RegisterBiomeEnvironment("LavaLakes_LavaPool", 400, 18, 0.5F, 2, 40);
        RegisterBiomeEnvironment("ilzLava", 1200, 24, 0.5F, 0, 100); //in lava
        RegisterBiomeEnvironment("LavaCastle", 240, 18, 0.5F, 4, 20);
        RegisterBiomeEnvironment("LavaCastleInner", 360, 18, 0.5F, 4, 20);
        RegisterBiomeEnvironment("LavaPitEntrance", 320, 15, 0.5F, 5, 25);
        _temperatures["ILZChamber_Dragon"] = _temperatures["ILZChamber"];
        RegisterBiomeEnvironment("ILZCastleTunnel", 450, 18, 0.5F, 4, 20); //the lavafall entrance

        RegisterBiomeEnvironment("AuroraPrawnBay", 150, 10F, 2.5F, 9999, 0);
        RegisterBiomeEnvironment("AuroraPrawnBayDoor", 200, 40F, 2.5F, 9999, 0);
        RegisterBiomeEnvironment("AuroraFireCeilingTunnel", 175, 2.5F, 1.5F, 9999, 0);

        _lrLeakage["LostRiver_BonesField_Corridor"] = 1;
        _lrLeakage["LostRiver_BonesField"] = 1;
        _lrLeakage["LostRiver_BonesField_LakePit"] = 1.5F;
        _lrLeakage["LostRiver_BonesField_Cave"] = 1;
        _lrLeakage["LostRiver_Junction"] = 1;
        _lrLeakage["LostRiver_TreeCove"] = 0.9F;
        _lrLeakage["LostRiver_Corridor"] = 1;
        _lrLeakage["LostRiver_GhostTree_Lower"] = 1;
        _lrLeakage["LostRiver_GhostTree"] = 1;
        _lrLeakage["LostRiver_GhostTree_Skeleton"] = 1;
        _lrLeakage["LostRiver_Canyon"] = 1.75F;
        _lrLeakage["LostRiver_SkeletonCave"] = 1.75F; //around the six eye skull
        _lrLeakage["Precursor_LostRiverBase"] = 1;

        _lrPoisonDamage["LostRiver_BonesField_Corridor"] = 8;
        _lrPoisonDamage["LostRiver_GhostTree"] = 8;
        _lrPoisonDamage["LostRiver_GhostTree_Skeleton"] = 8;
        _lrPoisonDamage["LostRiver_Corridor"] = 8;
        _lrPoisonDamage["LostRiver_Canyon"] = 10;
        _lrPoisonDamage["LostRiver_SkeletonCave"] = 10; //around the six eye skull
        _lrPoisonDamage["LostRiver_BonesField"] = 15;
        _lrPoisonDamage["LostRiver_BonesField_Skeleton"] = 15;
        _lrPoisonDamage["LostRiver_Junction"] = 15;
        _lrPoisonDamage["LostRiver_Junction_Skeleton"] = 15;
        _lrPoisonDamage["LostRiver_GhostTree_Lower"] = 15;
        _lrPoisonDamage["Precursor_LostRiverBase"] = 15;

        foreach (var kvp in new HashSet<string>(_lrLeakage.Keys)) {
            _lrLeakage[kvp + "_Water"] = _lrLeakage[kvp] * 1.5F;
            _lrLeakage[kvp + "_Lake"] = _lrLeakage[kvp] * 1.5F;
            _lrLeakage[kvp + "_LakePit"] = _lrLeakage[kvp] * 1.5F;
        }

        foreach (var kvp in new HashSet<string>(_lrPoisonDamage.Keys)) {
            _lrPoisonDamage[kvp + "_Water"] = _lrPoisonDamage[kvp] * 1.5F;
            _lrPoisonDamage[kvp + "_Lake"] = _lrPoisonDamage[kvp] * 1.5F;
            _lrPoisonDamage[kvp + "_LakePit"] = _lrPoisonDamage[kvp] * 1.5F;
        }

        _pdaBeep = SoundManager.registerPDASound(SeaToSeaMod.ModDLL, "pda_beep", "Sounds/pdabeep.ogg");
    }

    private void RegisterBiomeEnvironment(string b, float t, float dmg, float w, int cf, float dmgC) {
        _temperatures[b] = new TemperatureEnvironment(b, t, dmg, w, cf, dmgC);
    }

    public void PlayPdaBeep() {
        PlayPdaBeep(Player.main.transform.position);
    }

    public void PlayPdaBeep(Vector3 pos) {
        SoundManager.playSoundAt(_pdaBeep, pos, false, -1);
    }

    public bool IsPositionInAuroraPrawnBay(Vector3 pos) {
        var d1 = MathUtil.getDistanceToLineSegment(pos, AuroraPrawnBayLineA1, AuroraPrawnBayLineA2);
        var d2 = MathUtil.getDistanceToLineSegment(pos, AuroraPrawnBayLineB1, AuroraPrawnBayLineB2);
        var d3 = MathUtil.getDistanceToLineSegment(
            pos,
            (AuroraPrawnBayLineA1 + AuroraPrawnBayLineB1) / 2F,
            (AuroraPrawnBayLineA2 + AuroraPrawnBayLineB2) / 2F
        );
        return Math.Min(d1, Math.Min(d3, d2)) <= 6.25 || (pos - AuroraPrawnBayCornerPatch).sqrMagnitude <= 36;
    }

    public bool IsPlayerInOcean() {
        var ep = Player.main;
        var inWater = !ep.IsInsideWalkable() && Player.main.IsUnderwater() && ep.IsSwimming();
        return inWater && !ep.currentWaterPark && !(IsInPrecursor(ep.gameObject) && ep.transform.position.y < -1300);
    }

    public bool IsInPrecursor(GameObject go) {
        var biome = GetBiome(go).ToLowerInvariant();
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

    public void TickTemperatureDamages(TemperatureDamage dmg) {
        if (C2CHooks.SkipEnviroDamage)
            return;
        if (DIHooks.GetWorldAge() < 0.25F)
            return;
        if (GameModeUtils.currentEffectiveMode == GameModeOption.Creative)
            return;
        //depthWarningFX1 = Camera.main.gameObject.EnsureComponent<DepthRippleFX>();
        //depthWarningFX2 = Camera.main.gameObject.EnsureComponent<DepthDarkeningFX>();
        //SBUtil.writeToChat("Doing enviro damage on "+dmg+" in "+dmg.gameObject+" = "+dmg.player);
        var biome = GetBiome(dmg.gameObject); //Player.main.GetBiomeString();
        var prawn = biome == "AuroraPrawnBay" || biome == "AuroraPrawnBayDoor";
        var aurora = prawn || biome == "AuroraFireCeilingTunnel";
        var diveSuit = dmg.player && dmg.player.HasReinforcedGloves() && dmg.player.HasReinforcedSuit();
        if (aurora && !diveSuit &&
            !PDAMessagePrompts.instance.isTriggered(PDAMessages.getAttr(PDAMessages.Messages.AuroraFireWarn).key) &&
            !PDAMessagePrompts.instance.isTriggered(
                PDAMessages.getAttr(PDAMessages.Messages.AuroraFireWarn_NoRad).key
            )) {
            TriggerAuroraPrawnBayWarning();
        }

        if (dmg.player && !IsPlayerInOcean()) {
            _playerPressureDamageDuration = Mathf.Max(0, _playerPressureDamageDuration - Time.deltaTime);
            if (_playerPressureDamageDuration > 0)
                dmg.liveMixin.TakeDamage(
                    (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 3 : 1) / EnviroRateScalar,
                    dmg.transform.position,
                    DamageType.Pressure,
                    null
                );
            if (!aurora)
                return;
        }

        //SBUtil.writeToChat("not skipped");
        var temperature = dmg.GetTemperature();
        var te = _temperatures.ContainsKey(biome) ? _temperatures[biome] : null;
        if (te != null)
            temperature = Mathf.Max(temperature, te.Temperature);
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
            if ((prawn && !dmg.player.HasReinforcedSuit()) || (te != null && te.IsLavaZone && IsPlayerInOcean())) {
                var wb = MainCamera.camera.GetComponent<WBOIT>();
                wb.nextTemperatureUpdate = Time.time + 1F;
                wb.temperatureScalar =
                    prawn
                        ? 5
                        : 2F + Mathf.Clamp01(
                            te.Temperature - 250 / 150F
                        ); //lava >2x only when the player is free swimming
                wb.temperatureRefractEnabled = true;
                wb.compositeMaterial.EnableKeyword("FX_TEMPERATURE_REFRACT");
                //wb.compositeMaterial.SetTexture(wb.temperatureTexPropertyID, wb.temperatureRefractTex);
                //wb.compositeMaterial.SetFloat(wb.temperaturePropertyID, wb.temperatureScalar);
            }

            f0 = !diveSuit ? 2.5F : 0.4F;
            if (te != null) {
                f = te.DamageScalar;
                temperature = te.Temperature;
                fw = te.WaterScalar;
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
            var amt = num * f * f0 / EnviroRateScalar;
            if (aurora && dmg.player && biome == "AuroraPrawnBay" && Vector3.Distance(
                    AuroraPrawnBayDoor,
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
                            var f2 = GetHeatDamageModuleFactor(tt);
                            amt *= f2;
                        }
                    }

                    dmg.liveMixin.TakeDamage(amt, dmg.transform.position, DamageType.Heat, null);
                    if (dmg.player && !diveSuit) {
                        var s = Player.main.GetComponent<Survival>();
                        s.water = Mathf.Clamp(s.water - amt * fw, 0f, 100f);
                    }

                    if (temperature > 105) { //do not do at vents/geysers
                        _playerHeatDamage = time; //this also covers the seamoth
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
            var rb = LiquidBreathingSystem.Instance.HasLiquidBreathing();
            //depthWarningFX1.setIntensities(rb ? 0 : depth);
            //depthWarningFX2.setIntensities(rb ? 0 : depth);
            if (depth > DepthDamageStart && !rb) {
                var f2 = depth >= DepthDamageMax
                    ? 1
                    : (float)MathUtil.linterpolate(depth, DepthDamageStart, DepthDamageMax, 0, 1);
                f2 *= 1 + _playerPressureDamageDuration * 0.2F;
                dmg.liveMixin.TakeDamage(
                    (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 50 : 30) * 0.25F * f2 /
                    EnviroRateScalar,
                    dmg.transform.position,
                    DamageType.Pressure,
                    null
                );
                _playerPressureDamageSince = time;
                _playerPressureDamageDuration += Time.deltaTime;
                dmg.player.gameObject.removeComponent<HealingOverTime>();
            }

            if (!C2CItems.hasSealedOrReinforcedSuit(out var seal, out var reinf)) {
                //SBUtil.writeToChat(biome+" # "+dmg.gameObject);
                var amt = GetLrPoison(biome);
                if (amt > 0) {
                    dmg.liveMixin.TakeDamage(amt / EnviroRateScalar, dmg.transform.position, DamageType.Poison, null);
                }
            }
        }

        var leak = GetLrPowerLeakage(biome) * 0.9F;
        if (leak > 0) {
            var v = dmg.player ? dmg.player.GetVehicle() : dmg.gameObject.FindAncestor<Vehicle>();
            if (TriggerPowerLeakage(v, dmg.player, leak)) {
                if (PDAManager.getPage("lostrivershortcircuit").unlock())
                    PlayPdaBeep();
            }

            if (!dmg.player)
                _vehiclePowerLeak = time;
        }
    }

    private bool TriggerPowerLeakage(Vehicle v, Player ep, float leak) {
        float innerLeakFactor = 1;
        if (v) {
            leak *= GetLrLeakFactor(v, out var upgrade);
            if (upgrade)
                innerLeakFactor = 0.5F;
        }

        if (leak > 0) {
            var used = false;
            if (ep) {
                used = TriggerPowerLeakage(Inventory.main.container, leak, innerLeakFactor);
                //used = true;
            } else if (v) {
                v.ConsumeEnergy(Math.Min(v.energyInterface.TotalCanProvide(out var trash), leak / EnviroRateScalar));
                used = true;
                foreach (var sc in v.GetComponentsInChildren<StorageContainer>(true))
                    if (!sc.gameObject.FindAncestor<Player>())
                        TriggerPowerLeakage(sc.container, leak, innerLeakFactor);
                foreach (var sc in v.GetComponentsInChildren<SeamothStorageContainer>(true))
                    if (!sc.gameObject.FindAncestor<Player>())
                        TriggerPowerLeakage(sc.container, leak, innerLeakFactor);
            }

            return used;
        } else {
            return false;
        }
    }

    internal float GetHeatDamageModuleFactor(TechType tt) {
        return tt == TechType.ExosuitThermalReactorModule ? 0.8F : tt + "" == "ExosuitThermalModuleMk2" ? 0.67F : 1;
    }

    internal void TriggerAuroraPrawnBayWarning() {
        if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
            PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.AuroraFireWarn).key);
        else
            PDAMessagePrompts.instance.trigger(PDAMessages.getAttr(PDAMessages.Messages.AuroraFireWarn_NoRad).key);
    }

    internal bool IsPlayerRecoveringFromPressure() {
        return _playerPressureDamageDuration > 0 || DayNightCycle.main.timePassedAsFloat - _playerPressureDamageSince <
            (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 15 : 8);
    }

    internal void ResetCooldowns() {
        _playerPressureDamageDuration = 0;
        _playerPressureDamageSince = -1;
    }

    internal void SetRecoveryWarning(float dur) {
        _recoveryWarningEndTime = DayNightCycle.main.timePassedAsFloat + dur;
    }

    private float GetLrLeakFactor(Vehicle v, out bool hasUpgrade) {
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

    private bool TriggerPowerLeakage(ItemsContainer c, float leak, float relativeFactor) {
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

    public void TickCyclopsDamage(CrushDamage dmg) {
        if (C2CHooks.SkipEnviroDamage)
            return;
        if (!dmg.gameObject.activeInHierarchy || !dmg.enabled) {
            return;
        }

        if (dmg.GetCanTakeCrushDamage() && dmg.GetDepth() > dmg.crushDepth) {
            dmg.liveMixin.TakeDamage(C2CHooks.GetCrushDamage(dmg), dmg.transform.position, DamageType.Pressure, null);
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

            var temp = GetLavaHeatDamage(dmg.gameObject);
            //SBUtil.writeToChat("heat: "+temp);
            if (temp != null) {
                //SBUtil.writeToChat("immune: "+immune);
                if (!sub.cyclopsHasUpgrade(C2CItems.cyclopsHeat.TechType)) {
                    dmg.liveMixin.TakeDamage(
                        dmg.damagePerCrush * temp.DamageScalar * 0.15F,
                        dmg.transform.position,
                        DamageType.Heat,
                        null
                    );
                    if (dmg.soundOnDamage) {
                        dmg.soundOnDamage.Play();
                    }

                    //DO NOT SPAWN HEAT PDA PAGE
                    if (temp.CyclopsFireChance <= 0 || UnityEngine.Random.Range(0, temp.CyclopsFireChance) == 0) {
                        var key = (CyclopsRooms)UnityEngine.Random.Range(0, Enum.GetNames(typeof(CyclopsRooms)).Length);
                        var fire = dmg.gameObject.GetComponentInParent<SubFire>();
                        fire.CreateFire(fire.roomFires[key]);
                    }

                    _cyclopsHeatDamage = time;
                }
            }

            var leak = GetLrPowerLeakage(dmg.gameObject);
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
                    _cyclopsPowerLeak = time;
                    sub.powerRelay.ConsumeEnergy(leak * 2.5F, out var trash);
                    foreach (var sc in sub.GetComponentsInChildren<StorageContainer>(true)) {
                        if (!sc.gameObject.FindAncestor<Vehicle>() &&
                            !sc.gameObject
                                .FindAncestor<
                                    Player>()) //skip vehicle because will be included later in the docked call
                            TriggerPowerLeakage(sc.container, leak, 1);
                    }

                    foreach (var d in sub.GetComponentsInChildren<VehicleDockingBay>(true)) {
                        if (d.dockedVehicle)
                            TriggerPowerLeakage(d.dockedVehicle, null, leak * 0.5F);
                    }

                    if (PDAManager.getPage("lostrivershortcircuit").unlock())
                        PlayPdaBeep();
                }
            }
        }
    }

    public TemperatureEnvironment GetLavaHeatDamage(GameObject go) {
        return GetLavaHeatDamage(GetBiome(go)); //p.GetBiomeString());
    }

    public TemperatureEnvironment GetLavaHeatDamage(string biome) {
        return biome != null && _temperatures.ContainsKey(biome) ? _temperatures[biome] : null;
    }

    public float GetLrPoison(GameObject go) {
        return GetLrPoison(GetBiome(go)); //p.GetBiomeString());
    }

    public float GetLrPoison(string biome) {
        return biome != null && _lrPoisonDamage.ContainsKey(biome) ? _lrPoisonDamage[biome] : 0;
    }

    public float GetLrPowerLeakage(GameObject go) {
        return GetLrPowerLeakage(GetBiome(go)); //p.GetBiomeString());
    }

    public float GetLrPowerLeakage(string biome) {
        var ret = biome != null && _lrLeakage.ContainsKey(biome) ? _lrLeakage[biome] : -1;
        if (ret > 0 && !SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
            ret *= 0.8F;
        return ret;
    }

    public string GetBiome(GameObject go) {
        return !go ? null : GetBiome(go.transform.position);
    }

    public string GetBiome(Vector3 pos) {
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

        if ((pos.y <= -LavaPitEntranceDepthStart && MathUtil.isPointInCylinder(
                LavaPitEntranceCenter,
                pos,
                LavaPitEntranceRadius,
                999
            )) || Vector3.Distance(LavaPitTunnelMid, pos) <= 60)
            ret = "LavaPitEntrance";
        if (IsPositionInAuroraPrawnBay(pos))
            ret = "AuroraPrawnBay";
        if (Vector3.Distance(AuroraPrawnBayDoor, pos) <= 3)
            ret = "AuroraPrawnBayDoor";
        if (Vector3.Distance(AuroraFireCeilingTunnel, pos) <= 9)
            ret = "AuroraFireCeilingTunnel";
        return ret;
    }

    public float GetWaterTemperature(Vector3 pos) {
        var biome = GetBiome(pos);
        var temp = GetLavaHeatDamage(biome);
        var ret = temp != null ? temp.Temperature : -1000;
        if (biome == "ILZCorridor" && pos.y <= -1100 && pos.y >= -1175) {
            ret = (float)MathUtil.linterpolate(
                -pos.y,
                1100,
                1175,
                _temperatures["ILZCorridor"].Temperature,
                _temperatures["ILZCorridorDeep"].Temperature
            );
        }

        if (biome == "LavaPitEntrance" && pos.y >= -LavaPitEntranceDepthMaxTemp) {
            ret = (float)MathUtil.linterpolate(
                -pos.y,
                LavaPitEntranceDepthStart,
                LavaPitEntranceDepthMaxTemp,
                WaterTemperatureSimulation.main.GetTemperature(
                    LavaPitEntranceCenter.SetY(-LavaPitEntranceDepthStart + 5)
                ),
                _temperatures["LavaPitEntrance"].Temperature
            );
        }

        return ret;
    }

    public float GetPlayerO2Rate(Player ep) {
        if (C2CHooks.SkipO2)
            return 3F;
        var mode = ep.mode;
        if (mode != Player.Mode.Normal && mode - Player.Mode.Piloting <= 1) {
            return 3f;
        }

        if (LiquidBreathingSystem.Instance.HasLiquidBreathing()) {
            return 3f;
        }

        if (Inventory.main.equipment.GetTechTypeInSlot("Head") == TechType.Rebreather &&
            ep.GetDepth() < DepthDamageStart) {
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

    public float GetPlayerO2Use(Player ep, float breathingInterval, int depthClass) {
        if (C2CHooks.SkipO2)
            return 1;
        if (!GameModeUtils.RequiresOxygen())
            return 0;
        float num = 1;
        if (ep.mode != Player.Mode.Piloting && ep.mode != Player.Mode.LockedPiloting && IsPlayerInOcean()) {
            var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
            var hasRebreatherV2 = Inventory.main.equipment.GetTechTypeInSlot("Head") == C2CItems.rebreatherV2.TechType;
            var hasRebreather = hasRebreatherV2 ||
                                Inventory.main.equipment.GetTechTypeInSlot("Head") == TechType.Rebreather;
            var rebreatherV2Functional = hasRebreatherV2 && !IsRecoveryWarningActive();
            if (!hasRebreather) {
                if (depthClass == 2) {
                    num = 1.5F;
                } else if (depthClass == 3) {
                    num = 2;
                }
            }

            if (depthClass >= 3) {
                var liquid = LiquidBreathingSystem.Instance.HasLiquidBreathing();
                var depth = Player.main.GetDepth();
                var increaseStart = HighO2UsageStart;
                var rate = 10F;
                if (liquid) {
                    increaseStart = 99999;
                } else if (rebreatherV2Functional) {
                    increaseStart = DepthDamageStart;
                    rate = 4F;
                } else {
                    rate = depth > DepthDamageStart ? 8F : 10F;
                }

                if (hard) {
                    rate *= 0.8F; //do NOT adjust the depth
                }

                if (depth >= increaseStart) {
                    num = 2.5F + Math.Min(27.5F, (Player.main.GetDepth() - increaseStart) / rate);
                }

                if (!liquid) {
                    if (hard && depth > HighO2UsageStart) {
                        num *= (float)MathUtil.linterpolate(depth, HighO2UsageStart, DepthDamageStart, 1, 1.5F, true);
                    }

                    if (depth >= DepthDamageStart) {
                        num *= 1 + _playerPressureDamageDuration * 0.2F;
                    }
                }
                //SNUtil.writeToChat(depth.ToString("000.0")+"/"+increaseStart+"&"+rate+">"+num.ToString("00.000"));
            }

            if (LiquidBreathingSystem.Instance.HasReducedCapacity())
                num *= hard ? 5 : 3;
        }

        return breathingInterval * num;
    }

    public void TickPlayerEnviroAlerts(RebreatherDepthWarnings warn) {
        if (C2CHooks.SkipEnviroDamage)
            return;
        if (!(warn.alerts[0] is EnviroAlert))
            UpgradeAlertSystem(warn);

        if (DIHooks.GetWorldAge() < 0.25F)
            return;

        var flagged = false;
        var inOcean = IsPlayerInOcean();
        for (var i = 0; i < _warnings.Count; i++) {
            var w = _warnings[i];
            if (!flagged && w.ShouldShow(Player.main, inOcean)) {
                //SNUtil.writeToChat("Activated HUD warn "+w.getText());
                w.SetActive(true);
                flagged = true;
            } else {
                w.SetActive(false);
            }
        }

        if (!inOcean) {
            return;
        }

        foreach (EnviroAlert ee in warn.alerts) {
            //SNUtil.writeToChat(ee+" : "+ee.isActive());
            if (!ee.alertCooldown && !ee.WasActiveLastTick && ee.IsActive()) {
                ee.Fire(warn);
            } else {
                ee.WasActiveLastTick = false;
            }
        }
    }

    private void UpgradeAlertSystem(RebreatherDepthWarnings warn) {
        List<EnviroAlert> li = [];
        foreach (var a in warn.alerts) {
            var e = new EnviroAlert(a);
            e.PreventiveItem.Add(C2CItems.rebreatherV2.TechType);
            li.Add(e);
        }

        warn.alerts.Clear();
        warn.alerts.AddRange(li);

        var crush = new EnviroAlert(
            warn,
            ep => ep.GetDepth() >= DepthDamageStart && !LiquidBreathingSystem.Instance.HasLiquidBreathing(),
            SeaToSeaMod.MiscLocale.getEntry("deepair")
        );
        crush.PreventiveItem.Clear();
        warn.alerts.Add(crush);

        var o2Up = new O2IncreasingAlert(warn);
        warn.alerts.Add(o2Up);

        var infLung = new InfectedLungAlert(warn);
        infLung.PreventiveItem.Clear();
        warn.alerts.Add(infLung);

        var poison = new EnviroAlert(
            warn,
            p => GetLrPoison(p.gameObject) > 0,
            SeaToSeaMod.MiscLocale.getEntry("lrpoison")
        );
        poison.PreventiveItem.Clear();
        poison.PreventiveItem.Add(C2CItems.sealSuit.TechType);
        poison.PreventiveItem.Add(TechType.ReinforcedDiveSuit);
        warn.alerts.Add(poison);

        //GameObject hud = uGUI.main.screenCanvas.getChildObject("HUD/Content");
        var hudTemplate = uGUI.main.GetComponentInChildren<uGUI_RadiationWarning>(true).gameObject;
        LrPoisonHUDWarning = CreateHUDWarning(
            hudTemplate,
            "chemwarn",
            () => poison.IsActive() && IsPlayerInOcean(),
            50,
            new Color(0, 1F, 0.5F, 1)
        );
        O2ConsumptionIncreasingHUDWarning = CreateHUDWarning(
            hudTemplate,
            "o2warn",
            () => o2Up.IsActive() && IsPlayerInOcean() && !crush.IsActive(),
            10
        );
        InfectedLungsHUDWarning = CreateHUDWarning(
            hudTemplate,
            "influngwarn",
            () => infLung.IsActive() && !crush.IsActive(),
            10
        ); //, new Color(0.25F, 1F, 0.25F, 1));
        O2ConsumptionMaxedOutHUDWarning = CreateHUDWarning(
            hudTemplate,
            "pressurewarn",
            () => crush.IsActive() && IsPlayerInOcean(),
            20
        );
        LrLeakHUDWarning = CreateHUDWarning(hudTemplate, "leakwarn", IsLeakingLrPower, 0, new Color(1F, 1F, 0.2F, 1));
        ExtremeHeatHUDWarning = CreateHUDWarning(
            hudTemplate,
            "heatwarn",
            IsTakingHeatDamage,
            100,
            new Color(1, 0.875F, 0.75F, 1)
        );
        ExtremeHeatHUDWarning.ShowWhenNotSwimming = (ep) => true;
        LrLeakHUDWarning.ShowWhenNotSwimming = (ep) =>
            ep.GetVehicle() || SNUtil.GetControllingCamera(ep) ||
            (ep.currentSub && ep.currentSub.isCyclops && ep.isPiloting);
        TeleportRecoveryHUDWarning = CreateHUDWarning(
            hudTemplate,
            "teleportrecovery",
            IsRecoveryWarningActive,
            -50
        ); //, new Color(0.25F, 1F, 0.25F, 1));
        TeleportRecoveryHUDWarning.ShowWhenNotSwimming = (ep) => true;
        _warnings.Sort();
    }

    private bool IsLeakingLrPower() {
        var ep = Player.main;
        if (!ep)
            return false;
        MapRoomCamera cam = SNUtil.GetControllingCamera(ep);
        //SNUtil.writeToChat(cam+"");
        if (cam && GetLrPowerLeakage(cam.gameObject) > 0)
            return true;
        var v = ep.GetVehicle();
        if (v || (ep.currentSub && ep.currentSub.isCyclops)) {
            if (GetLrPowerLeakage(ep.gameObject) <= 0)
                return false;
            if (v && GetLrLeakFactor(v, out var upgrade) <= 0)
                return false;
            var time = DayNightCycle.main.timePassedAsFloat;
            return time - _vehiclePowerLeak <= 1 || time - _cyclopsPowerLeak <= 5;
        }

        return false;
    }

    private bool IsTakingHeatDamage() {
        var ep = Player.main;
        if (!ep)
            return false;
        if (GetLavaHeatDamage(ep.gameObject) == null)
            return false;
        var time = DayNightCycle.main.timePassedAsFloat;
        return time - _playerHeatDamage <= 1 || time - _cyclopsHeatDamage <= 5;
    }

    private bool IsRecoveryWarningActive() {
        return _recoveryWarningEndTime > 0 && DayNightCycle.main.timePassedAsFloat <= _recoveryWarningEndTime;
    }

    private CustomHUDWarning CreateHUDWarning(
        GameObject template,
        string key,
        EnviroAlert e,
        int pri,
        Color? c = null
    ) {
        return CreateHUDWarning(template, key, e.IsActive, pri, c);
    }

    private CustomHUDWarning CreateHUDWarning(GameObject template, string key, Func<bool> f, int pri, Color? c = null) {
        var go = template.clone().setName("CustomHudWarning_" + key);
        var rad = go.GetComponent<uGUI_RadiationWarning>();
        var warn = go.EnsureComponent<CustomHUDWarning>();
        warn.Replace(rad, f);
        warn.SetText(SeaToSeaMod.MiscLocale.getEntry("HUDAlerts").getString(key), c);
        warn.SetTexture(TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/HUD/" + key));
        warn.transform.SetParent(template.transform.parent, false);
        warn.priority = pri;
        go.removeComponent<uGUI_RadiationWarning>();
        SNUtil.Log("Created custom hud warning " + go);
        go.SetActive(true);
        _warnings.Add(warn);
        return warn;
    }
}

public class CustomHUDWarning : MonoBehaviour, IComparable {
    private GameObject _overlay;
    private TextMeshProUGUI _text;
    private Func<bool> _condition;
    public bool forceShow;
    internal Func<Player, bool> ShowWhenNotSwimming = (ep) => false;

    public int priority;

    internal void Replace(uGUI_RadiationWarning rad, Func<bool> f) {
        _text = rad.text;
        _overlay = rad.warning;
        _overlay.transform.SetParent(gameObject.transform, false);
        _text.transform.SetParent(_overlay.transform, false);
        _condition = f;
    }

    internal void SetTexture(Texture2D tex) {
        var img = _overlay.GetComponentInChildren<Image>();
        img.sprite = img.sprite.SetTexture(tex);
    }

    internal void SetText(string s, Color? c = null) {
        _text.text = s;
        if (c == null || !c.HasValue)
            c = Color.white;
        _text.color = c.Value;
    }

    internal string GetText() {
        return _text.text;
    }

    public bool ShouldShow(Player ep, bool inOcean) {
        return forceShow || ((ShowWhenNotSwimming(ep) || inOcean) && _condition());
    }

    public void SetActive(bool active) {
        if (_overlay)
            _overlay.SetActive(active);
    }

    public int CompareTo(object obj) {
        return obj is CustomHUDWarning warning ? warning.priority.CompareTo(priority) : 0; //inverse to put bigger first
    }
}

internal abstract class DepthFX : MonoBehaviour {
    protected float Strength;
    protected float LastUpdate;

    private void OnPreRender() {
        if (DayNightCycle.main.timePassedAsFloat - LastUpdate >= 3) {
            Strength *= 0.95F;
        }

        enabled = Strength > 0.01;
    }

    internal void SetIntensities(float depth) {
        LastUpdate = DayNightCycle.main.timePassedAsFloat;
        Strength = CalculateIntensity(depth);
        enabled = Strength > 0.01;
    }

    protected abstract float CalculateIntensity(float depth);
}

internal class DepthRippleFX : DepthFX {
    protected override float CalculateIntensity(float depth) {
        return (float)MathUtil.linterpolate(
            depth,
            EnvironmentalDamageSystem.DepthFXRippleStart,
            EnvironmentalDamageSystem.DepthDamageStart,
            0,
            1,
            true
        );
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        var mat = gameObject.GetComponent<MesmerizedScreenFX>().mat;
        mat.SetFloat(ShaderPropertyID._Amount, Strength);
        mat.SetColor(ShaderPropertyID._ColorStrength, new Color(0, 0, 0, Strength));
        Graphics.Blit(source, destination, mat);
    }
}

internal class DepthDarkeningFX : DepthFX {
    protected override float CalculateIntensity(float depth) {
        return (float)MathUtil.linterpolate(
            depth,
            EnvironmentalDamageSystem.DepthDamageStart,
            EnvironmentalDamageSystem.DepthDamageMax,
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
        mat.SetFloat(ShaderPropertyID._Amount, Strength);
        mat.color = new Color(0, 0, 0, Strength * 1F);
        Graphics.Blit(source, destination, mat);
    }
}

public class TemperatureEnvironment {
    public readonly float Temperature;
    public readonly float DamageScalar;
    public readonly float WaterScalar;
    public readonly float DamageScalarCyclops;
    public readonly int CyclopsFireChance;
    public readonly string Biome;
    public readonly bool IsLavaZone;

    internal TemperatureEnvironment(string b, float t, float dmg, float w, int cf, float dmgC) {
        Biome = b;
        IsLavaZone = b.StartsWith("lava", StringComparison.InvariantCultureIgnoreCase) ||
                     b.StartsWith("ilz", StringComparison.InvariantCultureIgnoreCase) || b.StartsWith(
                         "alz",
                         StringComparison.InvariantCultureIgnoreCase
                     );
        Temperature = t;
        DamageScalar = dmg;
        WaterScalar = w;
        CyclopsFireChance = cf;
        DamageScalarCyclops = dmgC;
    }

    public override string ToString() {
        return
            $"[TemperatureEnvironment Temperature={Temperature}, DamageScalar={DamageScalar}, CyclopsFireChance={CyclopsFireChance}]";
    }
}

internal class O2IncreasingAlert : EnviroAlert {
    internal O2IncreasingAlert(RebreatherDepthWarnings warn)
        : base(
            warn,
            ep => ep.GetDepth() >= EnvironmentalDamageSystem.HighO2UsageStart &&
                  Inventory.main.equipment.GetTechTypeInSlot("Head") != C2CItems.rebreatherV2.TechType,
            null,
            null
        ) {
        PreventiveItem.Clear();
    }

    internal override void Fire(RebreatherDepthWarnings warn) {
        base.Fire(warn);

        EnvironmentalDamageSystem.Instance.PlayPdaBeep();
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
            ep => LiquidBreathingSystem.Instance.HasReducedCapacity() &&
                  EnvironmentalDamageSystem.Instance.IsPlayerInOcean(),
            null,
            null
        ) {
        PreventiveItem.Clear();
    }

    internal override void Fire(RebreatherDepthWarnings warn) {
        base.Fire(warn);

        EnvironmentalDamageSystem.Instance.PlayPdaBeep();
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
    internal List<TechType> PreventiveItem = [TechType.Rebreather];
    internal readonly Func<Player, bool> Applicability;
    internal bool WasActiveLastTick;

    internal EnviroAlert(RebreatherDepthWarnings warn, Func<Player, bool> f, string pda, SoundManager.SoundData? snd) {
        if (!string.IsNullOrEmpty(pda)) {
            alert = warn.gameObject.AddComponent<PDANotification>();
            alert.text = pda;
            if (snd != null && snd.HasValue)
                alert.sound = snd.Value.asset;
        }

        Applicability = f;
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

    internal virtual void Fire(RebreatherDepthWarnings warn) {
        alertCooldown = true;
        WasActiveLastTick = true;
        //SNUtil.writeToChat("Firing enviro alert "+this+" when "+Player.main.GetDepth());
        if (alert)
            alert.Play();
        warn.StartCoroutine(warn.ResetAlertCD(this));
    }

    internal bool IsActive() {
        var p = Player.main;
        var valid = Applicability != null
            ? Applicability(p)
            : p.GetDepth() >= alertDepth && GameModeUtils.RequiresOxygen();
        if (!valid)
            return false;
        foreach (var prevent in PreventiveItem) {
            if (Inventory.main.equipment.GetCount(prevent) != 0)
                return false;
        }

        return true;
    }

    public override string ToString() {
        return alert.text + " @ " + alertDepth + "/" + Applicability;
    }
}