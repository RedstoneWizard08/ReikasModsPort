using System;
using FMOD;
using FMODUnity;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea;

internal class C2CMoth : MonoBehaviour {
    private static readonly SoundManager.SoundData startPurgingSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "startheatsink",
        "Sounds/startheatsink2.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 40); }
    );

    private static readonly SoundManager.SoundData meltingSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "seamothmelt",
        "Sounds/seamothmelt2.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 120); }
    );

    private static readonly SoundManager.SoundData boostSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "seamothboost",
        "Sounds/seamothboost.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 120); }
    );

    private static readonly SoundManager.SoundData purgeEnergySound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "seamothsounddump",
        "Sounds/stealthsounddump2.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 120); }
    );
    //private static readonly SoundManager.SoundData ejectionPrepareSound = SoundManager.registerSound(SeaToSeaMod.modDLL, "heatsinkEjectPrepare", "Sounds/heatsinkejectprepare.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 120);}, SoundSystem.masterBus);

    private static readonly Vector3 sweepArchCave = new(1570, -338, 1075);

    internal static bool useSeamothVehicleTemperature = true;

    public static bool temperatureDebugActive = false;

    private static readonly float TICK_RATE = 0.1F;
    private static readonly float HOLD_LOW_TIME = 30.8F;

    public static readonly float MAX_VOIDSTEALTH_ENERGY = 2400;

    public static float getOverrideTemperature(float temp) {
        if (!useSeamothVehicleTemperature)
            return temp;
        var ep = Player.main;
        if (!ep)
            return temp;
        var v = ep.GetVehicle();
        return !v ? temp : getOverrideTemperature(v, temp);
    }

    public static float getOverrideTemperature(Vehicle v, float temp) {
        if (!useSeamothVehicleTemperature)
            return temp;
        if (v is SeaMoth) {
            var cm = v.GetComponent<C2CMoth>();
            if (cm)
                return cm.vehicleTemperature;
        }

        return temp;
    }

    public SeaMoth seamoth { get; private set; }
    public TemperatureDamage temperatureDamage { get; private set; }
    private VFXVehicleDamages damageFX;
    private FMOD_CustomLoopingEmitter engineSounds;
    private VehicleAccelerationModifier speedModifier;
    private SeamothTetherController tethers;
    private ECMoth ecocean;
    public Rigidbody body { get; private set; }

    private float baseDamageAmount;

    public float vehicleTemperature { get; private set; }

    private float holdTempLowTime;

    private float temperatureAtPurge = -1;

    public bool isPurgingHeat => temperatureAtPurge >= 0;

    private float lastMeltSound = -1;
    //private float lastPreEjectSound = -1;

    private Channel? heatsinkSoundEvent;
    private Channel? boostSoundEvent;

    private float lastTickTime = -1;

    public float speedBonus { get; private set; }

    private Vector3 jitterTorque;
    private Vector3 jitterTorqueTarget;

    public float voidStealthStoredEnergy { get; private set; }

    public bool hasVoidStealth;

    //private Renderer deepStalkerStorageDamage;

    private static uGUI_SeamothHUD seamothHUD;
    private SeamothWithStealthHUD stealthEnabledSeamothHUDElement;

    public float soundStorageScalar => Mathf.Clamp01(voidStealthStoredEnergy / MAX_VOIDSTEALTH_ENERGY); //0-1

    public C2CMoth() {
        vehicleTemperature = 25;
    }

    private void Start() {
        useSeamothVehicleTemperature = false;
        vehicleTemperature = WaterTemperatureSimulation.main.GetTemperature(transform.position);
        useSeamothVehicleTemperature = true;

        Invoke(nameof(validateDepthModules), 0.5F);
    }

    private void validateDepthModules() {
        if (!seamoth || seamoth.modules == null) {
            Invoke(nameof(validateDepthModules), 0.5F);
            return;
        }

        if (!C2CProgression.IsSeamothDepth1UnlockedLegitimately()) {
            foreach (var idx in seamoth.slotIndexes.Values) {
                var ii = seamoth.GetSlotItem(idx);
                if (ii != null && ii.item) {
                    var tt = ii.item.GetTechType();
                    if (tt == TechType.VehicleHullModule1 || tt == TechType.VehicleHullModule2 ||
                        tt == TechType.VehicleHullModule3 || tt == C2CItems.depth1300.Info.TechType) {
                        ItemUnlockLegitimacySystem.instance.destroyModule(seamoth.modules, ii, seamoth.slotIDs[idx]);
                        seamoth.liveMixin.TakeDamage(10); //stop cheating
                        KnownTech.Remove(TechType.VehicleHullModule1);
                        KnownTech.Remove(TechType.VehicleHullModule2);
                        KnownTech.Remove(TechType.VehicleHullModule3);
                        KnownTech.Remove(C2CItems.depth1300.Info.TechType);
                    }
                }
            }
        }
    }

    private void Update() {
        if (C2CHooks.skipSeamothTick)
            return;
        if (!stealthEnabledSeamothHUDElement) {
            seamothHUD = FindObjectOfType<uGUI_SeamothHUD>();
            if (seamothHUD) {
                stealthEnabledSeamothHUDElement = seamothHUD.gameObject.EnsureComponent<SeamothWithStealthHUD>();
                if (!stealthEnabledSeamothHUDElement.root) {
                    var hudRoot = seamothHUD.root.transform.parent.gameObject;
                    var exo = seamothHUD.GetComponent<uGUI_ExosuitHUD>();
                    var go = exo.root.gameObject.clone().setName("SeamothStealthHUD");
                    go.SetActive(true);
                    go.transform.SetParent(exo.root.transform.parent);
                    go.transform.localPosition = exo.root.transform.localPosition;
                    go.transform.localRotation = exo.root.transform.localRotation;
                    go.transform.localScale = exo.root.transform.localScale;
                    stealthEnabledSeamothHUDElement.init(exo);
                    stealthEnabledSeamothHUDElement.root = go;
                }

                var bcg = stealthEnabledSeamothHUDElement.root.getChildObject("Background").GetComponent<Image>();
                var tex = TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/SeamothStealthHUD");
                bcg.sprite = TextureManager.createSprite(tex);
                var bar = stealthEnabledSeamothHUDElement.root.getChildObject("ThrustBar").GetComponent<Image>();
                var mat = bar.material;
                tex = TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/SeamothStealthEnergyBar");
                bar.sprite = TextureManager.createSprite(tex);
                bar.material = mat;
                mat.mainTexture = tex;
                //bcg.GetComponent<RectTransform>().sizeDelta = new Vector2(tex.width / 2F, tex.height / 2F);
                //bcg.transform.localPosition = new Vector3(22, 0, 0);
            }
        }

        var time = DayNightCycle.main.timePassedAsFloat;
        var dT = time - lastTickTime;
        if (dT >= TICK_RATE) {
            tick(time, Mathf.Min(1, dT));
            lastTickTime = time;
        }
    }

    internal void purgeHeat() {
        temperatureAtPurge = vehicleTemperature;
        SNUtil.Log(
            "Starting heat purge (" + temperatureAtPurge + ") @ " + DayNightCycle.main.timePassedAsFloat,
            SeaToSeaMod.ModDLL
        );
        //Invoke("fireHeatsink", 1.5F);
        heatsinkSoundEvent = SoundManager.playSoundAt(startPurgingSound, transform.position, false, -1, 0.67F);
    }

    internal void fireHeatsink(float time) {
        SNUtil.Log(
            "Heat purge complete @ " + time + " (" + holdTempLowTime + "/" + HOLD_LOW_TIME + "), firing heatsink",
            SeaToSeaMod.ModDLL
        );
        var go = ObjectUtil.createWorldObject(SeaToSeaMod.EjectedHeatSink.Info.ClassID);
        go.transform.position = seamoth.transform.position + seamoth.transform.forward * 4;
        go.GetComponent<Rigidbody>().AddForce(seamoth.transform.forward * 20, ForceMode.VelocityChange);
        body.AddForce(-seamoth.transform.forward * 5, ForceMode.VelocityChange);
        go.GetComponent<HeatSinkTag>().onFired(Mathf.Clamp01(temperatureAtPurge / 250F * 0.25F + 0.75F));
    }

    internal void dumpSoundEnergy() {
        if (voidStealthStoredEnergy <= 0)
            return;
        Utils.PlayOneShotPS(
            ObjectUtil.lookupPrefab(VanillaCreatures.CRASHFISH.prefab).GetComponent<Crash>().detonateParticlePrefab,
            transform.position + transform.forward * 2,
            Quaternion.identity
        );
        SoundManager.playSoundAt(purgeEnergySound, transform.position, false, -1, 2 * soundStorageScalar);
        //ECHooks.attractToSoundPing(seamoth, false, 1); //range 400 to attract
        /*
        for (int i = 0; i < UWE.Utils.OverlapSphereIntoSharedBuffer(transform.position, 30); i++) {
            Collider collider = UWE.Utils.sharedColliderBuffer[i];
            GameObject go = UWE.Utils.GetEntityRoot(collider.gameObject);
            if (!go)
                go = collider.gameObject;
            Creature c = go.GetComponent<Creature>();
            LiveMixin lv = go.GetComponent<LiveMixin>();
            if (c != null && lv != null) {
                lv.TakeDamage(2, transform.position, DamageType.Explosive, gameObject);
            }
        }
        */
        var r = 150 * Mathf.Clamp(soundStorageScalar, 0.33F, 0.67F); //so minimum 50m <= 33% and max 100m >= 67%
        /*
        foreach (AggressiveToPilotingVehicle a in WorldUtil.getObjectsNearWithComponent<AggressiveToPilotingVehicle>(transform.position, r)) {
            if (a.lastTarget && a.lastTarget.target && a.lastTarget.target == gameObject)
                a.lastTarget.SetTarget(null);
        }*/
        foreach (var a in WorldUtil.getObjectsNearWithComponent<AttackLastTarget>(transform.position, r)) {
            a.ClearAttackTarget();
        }

        voidStealthStoredEnergy = 0;
    }

    internal void applySpeedBoost(float charge) {
        if (speedBonus > 0.5F || !seamoth.HasEnoughEnergy(5))
            return;
        speedBonus = 2F;
        seamoth.ConsumeEnergy(5);
        boostSoundEvent = SoundManager.playSoundAt(boostSound, transform.position, false, -1, 1);
        seamoth.screenEffectModel.SetActive(true);
        ECHooks.attractToSoundPing(seamoth, false, 0.33F);
        if (ecocean.holdingBloodKelp)
            ecocean.holdingBloodKelp.release();
        if (seamoth.liveMixin.GetHealthFraction() < 0.67F)
            seamoth.liveMixin.TakeDamage(5);
    }

    internal void onHitByLavaBomb(LavaBombTag bomb) {
        vehicleTemperature = Mathf.Max(vehicleTemperature, bomb.getTemperature());
    }

    internal void tick(float time, float tickTime) {
        if (!seamoth)
            seamoth = GetComponent<SeaMoth>();
        if (!body)
            body = GetComponent<Rigidbody>();
        if (!ecocean)
            ecocean = GetComponent<ECMoth>();
        if (!engineSounds) {
            engineSounds = GetComponentInChildren<EngineRpmSFXManager>().gameObject
                .GetComponent<FMOD_CustomLoopingEmitter>();
        }

        if (!speedModifier)
            speedModifier = seamoth.AddSpeedModifier();
        if (!temperatureDamage) {
            temperatureDamage = GetComponent<TemperatureDamage>();
            baseDamageAmount = temperatureDamage.baseDamagePerSecond;
        }

        if (!tethers)
            tethers = GetComponent<SeamothTetherController>();
        if (!damageFX)
            damageFX = gameObject.GetComponent<VFXVehicleDamages>();

        var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);

        var health = seamoth.liveMixin.GetHealthFraction();

        var minSpeedBonus = seamoth.isVehicleUpgradeSelected(C2CItems.speedModule.Info.TechType) ? 0.25F : 0;
        if (speedBonus > minSpeedBonus)
            speedBonus *= 0.933F;
        else
            speedBonus = Mathf.Min(minSpeedBonus, speedBonus + 0.1F);
        speedModifier.accelerationMultiplier = 1 + speedBonus;
        if (health < 0.9F)
            speedModifier.accelerationMultiplier *= Mathf.Max(0.1F, health / 0.9F);
        if (tethers.isTowing())
            speedModifier.accelerationMultiplier *= 0.6F;
        //SNUtil.writeToChat(speedBonus.ToString("0.000"));

        if (heatsinkSoundEvent != null && heatsinkSoundEvent.Value.hasHandle()) {
            var attr = transform.position.To3DAttributes();
            heatsinkSoundEvent.Value.set3DAttributes(ref attr.position, ref attr.velocity);
        }

        if (boostSoundEvent != null && boostSoundEvent.Value.hasHandle()) {
            var attr = transform.position.To3DAttributes();
            boostSoundEvent.Value.set3DAttributes(ref attr.position, ref attr.velocity);
        }

        if (engineSounds && engineSounds.evt.isValid()) {
            if (hasVoidStealth) {
                engineSounds.evt.setVolume(0.25F);
            } else {
                engineSounds.evt.setVolume(1 + speedBonus);
            }
        }

        if (hasVoidStealth && body) {
            voidStealthStoredEnergy += tickTime * 0.2F * body.velocity.magnitude;
            if (voidStealthStoredEnergy >= MAX_VOIDSTEALTH_ENERGY)
                seamoth.liveMixin.Kill(DamageType.Explosive);
        }

        if (speedBonus > 0.5F) { //during boost only
            var jitter = (speedBonus + 1) * (speedBonus + 1) - 1; //0.25 -> 0.56, 3 -> 8
            var add = jitterTorque * tickTime * jitter * 25000;
            body.AddTorque(add, ForceMode.Force);
            //SNUtil.writeToChat("Adding jitter: "+add);
        }

        if ((jitterTorque - jitterTorqueTarget).sqrMagnitude < 0.01) {
            jitterTorqueTarget =
                UnityEngine.Random.onUnitSphere; //MathUtil.getRandomVectorAround(Vector3.zero, 3).setLength(1);
        } else {
            jitterTorque += (jitterTorqueTarget - jitterTorque) * Mathf.Min(1, tickTime * 9);
        }

        var kooshCave = false;
        var geyser = time - ecocean.lastGeyserTime <= 0.5F;

        if (health < 0.5F) {
            //float force = 1+(Mathf.Pow((0.5F-health)*2, 1.5F)*9);
            body.AddForce(Vector3.down * tickTime * 50, ForceMode.Acceleration);
        }

        if (VanillaBiomes.Koosh.IsInBiome(transform.position)) {
            var biome = WaterBiomeManager.main.GetBiome(transform.position, false);
            if (biome != null && biome.ToLowerInvariant().Contains("cave") &&
                Vector3.Distance(transform.position, sweepArchCave) >= 40) {
                kooshCave = true;
                var vel = body.velocity;
                var vec = Vector3.zero;
                vec = vel.magnitude < 0.2
                    ? UnityEngine.Random.onUnitSphere * 0.6F
                    : MathUtil.rotateVectorAroundAxis(
                        Vector3.Cross(vel, Vector3.up),
                        seamoth.transform.forward,
                        UnityEngine.Random.Range(0F, 360F)
                    ).SetLength(0.8F);
                body.AddForce(vec, ForceMode.VelocityChange);
            }
        }

        if (seamoth.GetPilotingMode()) {
            VoidSpikesBiome.instance.tickTeleportCheck(seamoth);
        }

        if (isPurgingHeat) {
            vehicleTemperature -= tickTime * 150;
            if (vehicleTemperature <= 5) {
                vehicleTemperature = 5;
                holdTempLowTime += tickTime;
                if (holdTempLowTime >= HOLD_LOW_TIME) {
                    fireHeatsink(time);
                    temperatureAtPurge = -1;
                }
            } else {
                holdTempLowTime = 0;
            }

            if (temperatureDebugActive)
                SNUtil.WriteToChat(
                    "Purging: " + vehicleTemperature.ToString("0000.00") + " > " + holdTempLowTime.ToString("00.00")
                );
        } else {
            holdTempLowTime = 0;
            useSeamothVehicleTemperature = false;
            var Tamb = temperatureDamage.GetTemperature(); // this will call WaterTempSim, after the lava checks in DI
            if (seamoth.docked || seamoth.IsInsideAquarium() ||
                EnvironmentalDamageSystem.instance.isInPrecursor(gameObject))
                Tamb = 25;
            else if (kooshCave)
                Tamb = 95;
            else if (geyser)
                Tamb = 250;
            //else if (heatColumn) not necessary, handled in ECHooks getTemp
            //	Tamb = 72;
            useSeamothVehicleTemperature = true;
            var dT = Tamb - vehicleTemperature;
            var excess = Mathf.Clamp01((vehicleTemperature - 400) / 400F);
            var f0 = dT > 0 ? 4F : 25F - 15 * excess;
            var f1 = dT > 0 ? 5F : 1F + 1.5F * excess;
            var speed = seamoth.useRigidbody.velocity.magnitude;

            if (geyser) //whee, forced convection
                speed *= 18;
            else if (ecocean.heatColumn)
                speed *= 4;

            if (speed >= 2) {
                f0 /= 1 + (speed - 2) / 8F;
            }

            var qDot = tickTime * Math.Sign(dT) * Mathf.Min(Math.Abs(dT), Mathf.Max(f1, Math.Abs(dT) / f0));
            if (qDot > 0) {
                if (Tamb < 300)
                    qDot *= hard ? 0.33F : 0.25F;
                else
                    qDot *= hard ? 0.8F : 0.67F;
            }

            vehicleTemperature += qDot;
            if (temperatureDebugActive)
                SNUtil.WriteToChat(
                    Tamb + " > " + dT + " > " + speed.ToString("00.0") + " > " + f0.ToString("00.0000") + " > " +
                    qDot.ToString("00.0000") + " > " + vehicleTemperature.ToString("0000.00")
                );
        }

        var factor = 1 + Mathf.Max(0, vehicleTemperature - 250) / 25F;
        var f2 = Mathf.Min(hard ? 36 : 32, Mathf.Pow(factor, 2.5F));
        temperatureDamage.baseDamagePerSecond = baseDamageAmount * f2;
        //SNUtil.writeToChat(vehicleTemperature+" > "+factor.ToString("00.0000")+" > "+f2.ToString("00.0000")+" > "+temperatureDamage.baseDamagePerSecond.ToString("0000.00"));
        if (vehicleTemperature >= 90 && seamoth.GetPilotingMode()) {
            damageFX.OnTakeDamage(new DamageInfo { damage = 1, type = DamageType.Heat });
            if (time - lastMeltSound >= 0.5F && !seamoth.docked && UnityEngine.Random.Range(0F, 1F) <= 0.25F) {
                SoundManager.playSoundAt(
                    meltingSound,
                    Player.main.transform.position,
                    false,
                    -1,
                    0.125F + Mathf.Clamp01((vehicleTemperature - 90) / 100F) * 0.125F
                );
                lastMeltSound = time;
            }
        }
    }

    public void recalculateModules() {
        if (!seamoth) {
            Invoke(nameof(recalculateModules), 0.5F);
            return;
        }

        hasVoidStealth = seamoth.vehicleHasUpgrade(C2CItems.voidStealth.Info.TechType);
        if (!hasVoidStealth)
            voidStealthStoredEnergy = 0;
        validateDepthModules();
    }

    private class SeamothWithStealthHUD : uGUI_ExosuitHUD {
        internal void init(uGUI_ExosuitHUD from) {
            this.CopyObject(from);
        }

        private void Start() {
            textHealth = root.getChildObject("Health").GetComponent<TextMeshProUGUI>();
            textPower = root.getChildObject("Power").GetComponent<TextMeshProUGUI>();
            textTemperature = root.getChildObject("Temperature/TemperatureValue").GetComponent<TextMeshProUGUI>();
            imageThrust = root.getChildObject("ThrustBar").GetComponent<Image>();
            imageThrust.material = new Material(imageThrust.material);
        }

        private new void Update() {
            var flag1 = false;
            var flag2 = false;
            SeaMoth sm = null;
            if (Player.main) {
                sm = Player.main.GetVehicle() as SeaMoth;
                flag1 = (bool)sm && !(Player.main.GetPDA() && Player.main.GetPDA().isInUse);
                flag2 = flag1 && sm.vehicleHasUpgrade(C2CItems.voidStealth.Info.TechType);
            }

            root.SetActive(flag2);
            if (seamothHUD)
                seamothHUD.root.SetActive(flag1 && !flag2);
            if (!flag2)
                return;
            sm.GetHUDValues(out var health, out var power);
            var cm = sm.GetComponent<C2CMoth>();
            var thrust = cm.soundStorageScalar;
            var temperature = cm.vehicleTemperature;
            var num4 = Mathf.CeilToInt(health * 100f);
            if (lastHealth != num4) {
                lastHealth = num4;
                textHealth.text = IntStringCache.GetStringForInt(lastHealth);
            }

            var num5 = Mathf.CeilToInt(power * 100f);
            if (lastPower != num5) {
                lastPower = num5;
                textPower.text = IntStringCache.GetStringForInt(lastPower);
            }

            if (lastThrust != thrust) {
                lastThrust = thrust;
                imageThrust.material.SetFloat(ShaderPropertyID._Amount, lastThrust);
            }

            temperatureSmoothValue = temperatureSmoothValue < -10000f
                ? temperature
                : Mathf.SmoothDamp(temperatureSmoothValue, temperature, ref temperatureVelocity, 1f);
            var num6 = Mathf.CeilToInt(temperatureSmoothValue);
            if (lastTemperature != num6) {
                lastTemperature = num6;
                textTemperature.text = IntStringCache.GetStringForInt(lastTemperature);
                textTemperatureSuffix.text = Language.main.GetFormat("ThermometerFormat");
            }
        }
    }
}