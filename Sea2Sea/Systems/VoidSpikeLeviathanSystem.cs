using System;
using System.Collections.Generic;
using FMOD;
using FMODUnity;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class VoidSpikeLeviathanSystem {
    public static readonly VoidSpikeLeviathanSystem instance = new();

    private static GameObject voidLeviathan;
    private static GameObject redirectedTarget;

    private static GameObject distantSparkFX;

    private static readonly float DAZZLE_FADE_LENGTH = 2;
    private static readonly float DAZZLE_TOTAL_LENGTH = 3.5F;
    private static readonly double MAXDEPTH = 1500; //800;

    private static readonly float SONAR_INTERFERENCE_LENGTH = 7.5F;
    private static readonly float SONAR_INTERFERENCE_DELAY = 1F;
    private static readonly float SONAR_INTERFERENCE_FADE_LENGTH_IN = 0.5F;
    private static readonly float SONAR_INTERFERENCE_FADE_LENGTH_OUT = 2.5F;

    public static readonly string
        PASSIVATION_GOAL = "SeaEmperorBabiesHatched"; //"PrecusorPrisonAquariumIncubatorActive";

    public static bool disableStealthInterruptions = false;

    private readonly List<SoundManager.SoundData> distantRoars = [];

    private readonly SoundManager.SoundData empSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "voidlevi-emp",
        "Sounds/voidlevi/emp6.ogg",
        SoundManager.soundMode3D
    );

    private readonly SoundManager.SoundData empHitSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "voidlevi-emp-hit",
        "Sounds/voidlevi/emp-hit.ogg",
        SoundManager.soundMode3D
    );

    private readonly SoundManager.SoundData sonarBlockSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "voidlevi-sonarblock",
        "Sounds/voidlevi/sonarblock3.ogg",
        SoundManager.soundMode3D
    );

    private float nextDistantRoarTime = -1;

    private float nextCyclopsEMPTime = -1;
    private float nextCyclopsEMPAbleTime = -1;

    private float lastFlashTime = -1;
    private float currentFlashDuration; //full blindness length; fade is (DAZZLE_FADE_LENGTH)x longer after that

    private float lastEMPHitSoundTime;

    private int randomDepth;
    private float timeSinceRandomDepthChange;

    private float lastVehicleCheck = -1;

    private Channel? sonarSoundEvent;

    private readonly LeviathanVisualFX fxHook;

    private static readonly List<DistantFX> distantFXList = [
        new DistantFX("ff8e782e-e6f3-40a6-9837-d5b6dcce92bc", 2),
        new DistantFX("6e4b4259-becc-4d2c-b56a-03ccedbc4672", 2),
        new DistantFX("3274b205-b153-41b6-9736-f3972e38f0ad", 2), /*
        new DistantFX("04781674-e27a-43ce-891f-a82781314c15", 2.5F, 5, 0.5F, go => {
            go.transform.rotation = UnityEngine.Random.rotationUniform;
            go.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.75F, 1.5F);
        }), //lavafall base*/
    ];

    private class DistantFX {
        internal readonly string prefab;
        internal readonly float distanceScalar;
        internal readonly float lifeScalar;
        internal readonly float speedScalar;
        internal readonly Action<GameObject> modification;

        internal DistantFX(string pfb, float d = 1, float l = 1, float s = 1, Action<GameObject> a = null) {
            prefab = pfb;
            distanceScalar = d;
            lifeScalar = l;
            speedScalar = s;
            modification = a;
        }
    }

    private VoidSpikeLeviathanSystem() {
        for (var i = 0; i <= 3; i++) {
            distantRoars.Add(
                SoundManager.registerSound(
                    SeaToSeaMod.ModDLL,
                    "voidlevi-roar-far-" + i,
                    "Sounds/voidlevi/roar-distant-" + i + ".ogg",
                    SoundManager.soundMode3D
                )
            );
        }

        fxHook = new LeviathanVisualFX();
        ScreenFXManager.instance.addOverride(fxHook);
    }

    private class LeviathanVisualFX : ScreenFXManager.ScreenFXOverride {
        internal readonly Color smokeDarkColors = new(-5, -5, -5, 1F);
        internal readonly Color smokeDazzleColors = new(50, 50, 50, 1F);
        internal readonly Vector4 mesmerDazzleColorsStart = new(600, 600, 1000, 0.5F);
        internal readonly Vector4 mesmerDazzleColorsEnd = new(600, 600, 1000, 0.2F);

        internal readonly Vector4 sonarInterferenceColor = new(-1, -1, -0.25F, 1F);

        internal bool smokeEnabled;
        internal float smokeAlpha;
        internal float smokeColorMix;

        internal float mesmerIntensity;

        internal float flashWashoutNetVisiblityFactor = 1;

        internal float lastSonarInterferenceTime = -1;
        internal float sonarInterferenceIntensity;

        internal LeviathanVisualFX() : base(9999) {
        }

        public override void onTick() {
            if (smokeEnabled) {
                ScreenFXManager.instance.registerOverrideThisTick(ScreenFXManager.instance.smokeShader);
                ScreenFXManager.instance.smokeShader.intensity = 1;
                ScreenFXManager.instance.smokeShader.mat.color =
                    Color.Lerp(smokeDazzleColors, smokeDarkColors, smokeColorMix).WithAlpha(smokeAlpha);
            }

            if (mesmerIntensity > 0) {
                ScreenFXManager.instance.registerOverrideThisTick(ScreenFXManager.instance.mesmerShader);
                ScreenFXManager.instance.mesmerShader.amount = mesmerIntensity * mesmerIntensity * 0.003F;
                ScreenFXManager.instance.mesmerShader.mat.SetVector(
                    "_ColorStrength",
                    Vector4.Lerp(mesmerDazzleColorsStart, mesmerDazzleColorsEnd, mesmerIntensity)
                );
            }

            if (sonarInterferenceIntensity > 0) {
                ScreenFXManager.instance.registerOverrideThisTick(ScreenFXManager.instance.sonarShader);
                ScreenFXManager.instance.radiationShader.noiseFactor =
                    sonarInterferenceIntensity * sonarInterferenceIntensity * 0.6F;
                ScreenFXManager.instance.radiationShader.color = sonarInterferenceColor.AsColor();
                ScreenFXManager.instance.sonarShader.enabled = false;
            }
        }
    }

    internal void deleteVoidLeviathan() {
        if (voidLeviathan)
            voidLeviathan.destroy(false);
        voidLeviathan = null;
    }

    internal void deleteGhostTarget() {
        if (redirectedTarget)
            redirectedTarget.destroy(false);
        redirectedTarget = null;
    }

    private GameObject getOrCreateTarget() {
        if (!redirectedTarget) {
            redirectedTarget = new GameObject("Void Ghost Target");
        }

        return redirectedTarget;
    }

    private GameObject getOrCreateSparkFX(DistantFX fx) {
        if (!distantSparkFX) {
            distantSparkFX = ObjectUtil.createWorldObject(fx.prefab); //or ff8e782e-e6f3-40a6-9837-d5b6dcce92bc
            distantSparkFX.EnsureComponent<VoidSparkFX>();
        }

        return distantSparkFX;
    }

    internal void tick(Player ep) {
        //SNUtil.log("========================");
        //SNUtil.log("All non-null: "+mesmerShader+" x "+mesmerController+" x "+smokeShader+" x "+smokeController);
        if (sonarSoundEvent != null && sonarSoundEvent.Value.hasHandle() && Camera.main) {
            var attr = Camera.main.transform.position.To3DAttributes();
            sonarSoundEvent.Value.set3DAttributes(ref attr.position, ref attr.velocity);
        }

        var day = DayNightCycle.main;
        if (!day)
            return;
        var time = day.timePassedAsFloat;
        if (VoidSpikesBiome.instance.IsInBiome(ep.transform.position) && !ep.GetVehicle() &&
            time - lastVehicleCheck > 1) {
            lastVehicleCheck = time;
            if (WorldUtil.getObjectsNearWithComponent<C2CMoth>(ep.transform.position, 400).Count == 0) {
                ECHooks.attemptTongueGrab();
            }
        }

        playDistantRoar(ep, time);
        var dtf = time - lastFlashTime;
        var maxL = currentFlashDuration * (1 + DAZZLE_FADE_LENGTH);
        var maxL2 = currentFlashDuration * (1 + DAZZLE_TOTAL_LENGTH);
        //SNUtil.log("L calc done");
        if (dtf <= maxL) {
            var blind = dtf < currentFlashDuration + 1.5F;
            Player.main.GetPDA().SetIgnorePDAInput(blind);
            if (blind)
                Player.main.GetPDA().Close();
            fxHook.smokeEnabled = true;
            fxHook.smokeAlpha = 0;
            fxHook.smokeColorMix = 0;
            if (dtf <= 0.33) {
                fxHook.smokeAlpha = dtf * 3;
                fxHook.smokeColorMix = 0;
            } else if (dtf <= 0.67) {
                fxHook.smokeAlpha = 1;
                fxHook.smokeColorMix = 0;
            } else if (dtf <= 1) { //4-78s
                fxHook.smokeAlpha = 1;
                fxHook.smokeColorMix = (float)MathUtil.linterpolate(dtf, 0.67F, 1, 0, 1);
            } else if (dtf <= currentFlashDuration) { //4-8s
                fxHook.smokeAlpha = 1;
                fxHook.smokeColorMix = 1;
            } else {
                fxHook.smokeAlpha = (float)MathUtil.linterpolate(dtf, currentFlashDuration, maxL, 1, 0);
                fxHook.smokeColorMix = 1;
            }

            fxHook.flashWashoutNetVisiblityFactor = 1 - fxHook.smokeAlpha;
            //SNUtil.log("pre smoke mat");
            //SNUtil.log(time+" > "+dtf+" / "+currentFlashDuration+" > A="+alpha.ToString("0.00000")+" & C="+colorMix.ToString("0.00000"));
        } else {
            fxHook.smokeEnabled = false;
        }

        //SNUtil.log("done smoke, now mesmer");
        float f = 0;
        if (dtf <= maxL2) {
            f = 0;
            f = dtf <= 0.5F ? dtf * 2 : dtf <= maxL ? 1 : (float)MathUtil.linterpolate(dtf, maxL, maxL2, 1, 0);
            //SNUtil.log("pre mesmer fx");
        }

        fxHook.mesmerIntensity = f;
        //SNUtil.log("end");
        //SNUtil.log("==============================");
        //SNUtil.log(radiationController+"");
        //SNUtil.log(radiationShader+"");
        //SNUtil.log(sonarShader+"");

        var dt = time - fxHook.lastSonarInterferenceTime;
        f = 0;
        if (dt <= SONAR_INTERFERENCE_LENGTH) {
            f = dt <= SONAR_INTERFERENCE_DELAY
                ? 0.02F
                : dt <= SONAR_INTERFERENCE_FADE_LENGTH_IN + SONAR_INTERFERENCE_DELAY
                    ? 0.02F + 0.98F * (dt - SONAR_INTERFERENCE_DELAY) / SONAR_INTERFERENCE_FADE_LENGTH_IN
                    : dt > SONAR_INTERFERENCE_LENGTH - SONAR_INTERFERENCE_FADE_LENGTH_OUT
                        ? 1 - (dt - (SONAR_INTERFERENCE_LENGTH - SONAR_INTERFERENCE_FADE_LENGTH_OUT)) /
                        SONAR_INTERFERENCE_FADE_LENGTH_OUT
                        : 1;
        }

        //SNUtil.log("f="+f);
        fxHook.sonarInterferenceIntensity = f;
    }

    public void triggerEMInterference() {
        //TODO excessive sonar pings could increase aggro once it's actually implemented
        fxHook.lastSonarInterferenceTime = DayNightCycle.main.timePassedAsFloat;
        var pos = Camera.main.transform.position;
        sonarSoundEvent = SoundManager.playSoundAt(sonarBlockSound, pos, false, -1, 1);
        spawnJustVisibleDistanceFX(Camera.main.transform);
        //if (UnityEngine.Random.Range(0F, 1F) < 0.4F)
        //	SoundManager.playSoundAt(distantRoars[UnityEngine.Random.Range(1, distantRoars.Count-1)], MathUtil.getRandomVectorAround(pos, 100), false, -1, 1);
    }

    public bool isSonarInterferenceActive() {
        return DayNightCycle.main.timePassedAsFloat - fxHook.lastSonarInterferenceTime <= SONAR_INTERFERENCE_LENGTH;
    }

    public bool isVoidFlashActive(bool any) { //false if only the blind part
        //SNUtil.writeToChat(DayNightCycle.main.timePassedAsFloat.ToString("00000.00")+"/"+lastFlashTime.ToString("00000.00")+"/"+currentFlashDuration.ToString("00.00"));
        if (currentFlashDuration <= 0)
            return false;
        var n = any ? DAZZLE_TOTAL_LENGTH : DAZZLE_FADE_LENGTH;
        return DayNightCycle.main.timePassedAsFloat <= lastFlashTime + currentFlashDuration * (1 + n);
    }

    public float getNetScreenVisibilityAfterFlash() {
        return currentFlashDuration <= 0 ? 1 : fxHook.flashWashoutNetVisiblityFactor;
    }

    public int getRandomDepthForDisplay() {
        timeSinceRandomDepthChange += Time.deltaTime;
        if (timeSinceRandomDepthChange > 0.05F) {
            randomDepth = UnityEngine.Random.Range(100, 1000);
            timeSinceRandomDepthChange = 0;
        }

        return randomDepth;
    }

    internal void playDistantRoar(Player ep, float time) {
        if (ep.currentSub)
            return;
        if (time < nextDistantRoarTime)
            return;
        if (voidLeviathan && voidLeviathan.activeInHierarchy && Vector3.Distance(
                voidLeviathan.transform.position,
                ep.transform.position
            ) <= 200)
            return;
        doDistantRoar(ep);
    }

    internal void doDistantRoar(Player ep, bool forceSpark = false, bool forceEMP = false) {
        SoundManager.SoundData? roar = null;
        var dist = VoidSpikesBiome.instance.getDistanceToBiome(ep.transform.position, true);
        var biome = ep.GetBiomeString();
        //SNUtil.writeToChat(dist+" @ "+biome);
        float vol = 1;
        var inBiome = false;
        if (dist <= VoidSpikesBiome.biomeVolumeRadius) {
            roar = distantRoars[UnityEngine.Random.Range(1, distantRoars.Count - 1)];
            var dd = Vector3.Distance(ep.transform.position, VoidSpikesBiome.end900m);
            vol = Mathf.Clamp01(2 - dd / 400F);
            inBiome = true;
        } else if (dist <= 1000 && (biome == null || biome == VoidSpikesBiome.instance.biomeName ||
                                    string.Equals(biome, "void", StringComparison.InvariantCultureIgnoreCase))) {
            roar = distantRoars[0];
            vol = 1 - (float)Math.Max(0, Math.Min(1, (dist - 250) / 1000D));
        }

        if (roar != null && roar.HasValue) {
            var delta = (float)Math.Max(30, UnityEngine.Random.Range(30F, 120F) * dist / 1000);
            nextDistantRoarTime = DayNightCycle.main.timePassedAsFloat + delta;
            //SNUtil.writeToChat(dist+" @ "+biome+" > "+roar+"/"+vol+" >> "+delta);
            SoundManager.playSoundAt(
                roar.Value,
                MathUtil.getRandomVectorAround(ep.transform.position, 100),
                false,
                -1,
                vol
            );
        }

        if (forceSpark || (inBiome && !Story.StoryGoalManager.main.completedGoals.Contains(PASSIVATION_GOAL))) {
            var pos = spawnJustVisibleDistanceFX(ep.transform);
            if (forceEMP || VoidSpikesBiome.instance.isPlayerInLeviathanZone(ep.transform.position)) {
                var chance = forceEMP ? 0.5F : Mathf.Min(0.1667F, (ep.GetDepth() - 600F) / 1800F);
                if (UnityEngine.Random.Range(0F, 1F) <= chance) { /*
                    Vector3 rel = getRandomVisibleDistantPosition(ep, 3, 3);
                    Vector3 pos = ep.transform.position+rel;*/
                    var rel = pos - ep.transform.position;
                    pos = ep.transform.position + rel.normalized * 150;
                    pos = MathUtil.getRandomVectorAround(pos, 15);

                    if (forceEMP)
                        SNUtil.WriteToChat("Spawning EMP blast @ " + pos);
                    spawnEMPBlast(pos);
                    //shutdownSeamoth(ep.GetVehicle(), false);
                }
            }
        }
    }

    public void onObjectEMPHit(EMPBlast e, GameObject go) { //this might be called many times!
        //SNUtil.writeToChat(">>"+e.gameObject.name+" > "+e.gameObject.name.StartsWith("VoidSpikeLevi_LightPulse", StringComparison.InvariantCultureIgnoreCase)+" @ "+go.FindAncestor<Player>());
        if (e.gameObject.name.StartsWith("VoidSpikeLevi_EMPulse", StringComparison.InvariantCultureIgnoreCase)) {
            //SNUtil.writeToChat("Match");
            var cam = go.FindAncestor<MapRoomCamera>();
            if (cam) {
                go.FindAncestor<LiveMixin>().TakeDamage(
                    999999,
                    go.transform.position,
                    DamageType.Electrical,
                    e.gameObject
                );
                //cam.energyMixin.ConsumeEnergy(99999); //completely kill it
            } else {
                var sub = go.FindAncestor<SubRoot>();
                if (sub && sub.isCyclops) {
                    if (DayNightCycle.main.timePassedAsFloat >= nextCyclopsEMPAbleTime) {
                        sub.EndSubShielded();
                        sub.powerRelay.ConsumeEnergy(Mathf.Max(300, sub.powerRelay.GetPower() * 0.25F), out var trash);
                        go.FindAncestor<LiveMixin>().TakeDamage(
                            250,
                            go.transform.position,
                            DamageType.Electrical,
                            e.gameObject
                        );
                        nextCyclopsEMPAbleTime = DayNightCycle.main.timePassedAsFloat + 2.5F;
                    }
                } else {
                    shutdownSeamoth(go.FindAncestor<Vehicle>(), true);
                }
            }
        }
    }

    //Returns the relative position, not absolute
    private Vector3 getRandomVisibleDistantPosition(Transform cam, float distSc, float fovSc = 1) {
        var range = UnityEngine.Random.Range(30F, 40F);
        range *= distSc;
        var pos = cam.position + /*ep.transform.forward*/MainCamera.camera.transform.forward.normalized * range;
        pos = MathUtil.getRandomVectorAround(pos, 20 * distSc * fovSc);
        var dist = pos - cam.position;
        dist = dist.SetLength(range);
        return dist;
    }

    private Vector3 spawnJustVisibleDistanceFX(Transform cam) {
        var type = distantFXList[UnityEngine.Random.Range(0, distantFXList.Count)];
        var dist = getRandomVisibleDistantPosition(cam, type.distanceScalar);
        var pos = cam.position + dist;
        var go = getOrCreateSparkFX(type);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * UnityEngine.Random.Range(1.5F, 2.5F);
        type.modification?.Invoke(distantSparkFX);
        var fx = go.GetComponent<VoidSparkFX>();
        fx.relativePosition = dist;
        var speed = UnityEngine.Random.Range(2.5F, 10F) + (dist.magnitude - 30) * 0.5F;
        if (UnityEngine.Random.Range(0F, 1F) <= 0.2F)
            speed *= 3;
        speed *= type.speedScalar;
        fx.velocity = MathUtil.getRandomVectorAround(Vector3.zero, 1).normalized * speed;
        go.SetActive(true);
        go.destroy(false, UnityEngine.Random.Range(0.33F, 0.75F) * type.lifeScalar);
        return pos;
    }

    internal void shutdownSeamoth(Vehicle v, bool disable, float factor = 1) {
        if (v) {
            if (v is SeaMoth moth) {
                ObjectUtil.createSeamothSparkSphere(moth);
                var time = DayNightCycle.main.timePassedAsFloat;
                if (time - lastEMPHitSoundTime > 1F) {
                    lastEMPHitSoundTime = time;
                    SoundManager.playSoundAt(empHitSound, moth.transform.position, false, -1, 1);
                }
            }

            v.ConsumeEnergy(UnityEngine.Random.Range(4F, 10F) * factor); //2-5% base
            if (disable)
                v.energyInterface.DisableElectronicsForTime(UnityEngine.Random.Range(1F, 5F) * factor);
        }
    }

    internal void spawnAoEBlast(Vector3 pos, string idName, Action<GameObject> modify = null) {
        var pfb = ObjectUtil.lookupPrefab(VanillaCreatures.CRABSQUID.prefab).GetComponent<EMPAttack>().ammoPrefab;
        for (var i = 0; i < 180; i += 30) {
            var emp = pfb.clone().setName(idName + "_" + i);
            emp.transform.position = pos;
            emp.transform.localRotation = Quaternion.Euler(i, 0, 0);
            //emp.EnsureComponent<VoidLeviElecSphereComponent>().spawn();
            var r = emp.GetComponentInChildren<Renderer>();
            r.materials[0].color = new Color(0.8F, 0.33F, 1F, 1F);
            r.materials[1].color = new Color(0.67F, 0.9F, 1F, 1F);
            r.materials[0].SetColor("_ColorStrength", new Color(1, 1, 1000, 1));
            r.materials[1].SetColor("_ColorStrength", new Color(1, 1, 100, 1));
            var e = emp.GetComponent<EMPBlast>();
            emp.removeComponent<VFXLerpColor>();
            e.blastRadius = AnimationCurve.Linear(0f, 0f, 1f, 400f);
            e.blastHeight = AnimationCurve.Linear(0f, 0f, 1f, 300f);
            e.lifeTime = 3.5F;
            e.disableElectronicsTime = UnityEngine.Random.Range(1F, 5F);
            modify?.Invoke(emp);
            emp.SetActive(true);
            //ObjectUtil.dumpObjectData(emp);
        }
    }

    internal void spawnEMPBlast(Vector3 pos) {
        SoundManager.playSoundAt(empSound, pos, false, -1, 1);
        spawnAoEBlast(pos, "VoidSpikeLevi_EMPulse");
    }

    internal void doDebugFlash(bool usePulse) {
        if (usePulse)
            doFlash(Player.main.transform.position + MainCamera.camera.transform.forward.normalized * 150);
        else
            onFlashHit();
    }

    internal void doFlash(Vector3 pos) {
        Action<GameObject> a = e => {
            var f = e.EnsureComponent<FlashBurst>();
            e.removeComponent<EMPBlast>();
            var r = e.GetComponentInChildren<Renderer>();
            r.materials[0].color = Color.white;
            r.materials[1].color = Color.white;
            r.materials[0].SetColor("_ColorStrength", new Color(1000, 1000, 1000, 1));
            r.materials[1].SetColor("_ColorStrength", new Color(100, 100, 100, 1));
            foreach (var tt in e.GetComponentsInChildren<OnTouch>()) {
                tt.onTouch = new OnTouch.OnTouchEvent();
                tt.onTouch.RemoveAllListeners();
                tt.onTouch.AddListener(f.OnTouch);
            }
        };
        Action<GameObject> a2 = e => {
            a(e);
            e.EnsureComponent<FlashBurst>().lifeTime += 0.075F;
        };
        spawnAoEBlast(pos, "VoidSpikeLevi_LightPulse", a);
        spawnAoEBlast(pos, "VoidSpikeLevi_LightPulse", a2);
    }

    internal void onFlashHit() {
        lastFlashTime = DayNightCycle.main.timePassedAsFloat;
        currentFlashDuration = UnityEngine.Random.Range(4F, 8F);
        Player.main.GetPDA().Close();
        Player.main.GetPDA().SetIgnorePDAInput(true);
    }

    internal void resetFlash() {
        lastFlashTime = -1;
        currentFlashDuration = 0;
    }

    public bool isSpawnableVoid(string biome) {
        var ep = Player.main;
        if (Ecocean.ECHooks.isVoidHeatColumn(ep.transform.position, out var trash))
            return false;
        if (VoidSpikesBiome.instance.isPlayerInLeviathanZone(ep.transform.position) && isLeviathanEnabled() &&
            voidLeviathan && voidLeviathan.activeInHierarchy) {
            return false;
        }

        var edge = string.Equals(biome, "void", StringComparison.InvariantCultureIgnoreCase);
        var far = string.IsNullOrEmpty(biome);
        if (VoidSpikesBiome.instance.getDistanceToBiome(ep.transform.position, true) <=
            VoidSpikesBiome.biomeVolumeRadius + 25)
            far = true;
        if (!far && !edge)
            return false;
        if (ep.inSeamoth) {
            var sm = ep.GetVehicle() as SeaMoth; //not cast in case of modded vehicle
            if (sm) {
                var ch = getAvoidanceChance(ep, sm, edge, far);
                //SNUtil.writeToChat(ch+" @ "+sm.transform.position);
                if (ch > 0 && (ch >= 1 || UnityEngine.Random.Range(0F, 1F) <= ch)) {
                    if (sm.vehicleHasUpgrade(C2CItems.voidStealth.TechType))
                        return false;
                    //SNUtil.writeToChat("Tried and failed");
                }
            }
        }

        return true;
    }

    public bool isLeviathanEnabled() {
        return false;
    }

    public GameObject getVoidLeviathan(VoidGhostLeviathansSpawner spawner, Vector3 pos) {
        if (isLeviathanEnabled() && VoidSpikesBiome.instance.isPlayerInLeviathanZone(Player.main.transform.position)) {
            var go = ObjectUtil.createWorldObject(C2CItems.voidSpikeLevi.ClassID);
            go.transform.position = pos;
            voidLeviathan = go;
            return go;
        } else {
            var go = UnityEngine.Object.Instantiate(spawner.ghostLeviathanPrefab, pos, Quaternion.identity);
            foreach (var c in go.GetComponentsInChildren<Collider>()) {
                if (c is BoxCollider collider)
                    collider.size *= 1.5F;
                if (c is SphereCollider sphereCollider)
                    sphereCollider.radius *= 1.5F;
                if (c is CapsuleCollider cc) {
                    cc.radius *= 1.5F;
                    cc.height *= 1.2F;
                }
            }

            return go;
        }
    }

    public void tickVoidLeviathan(GhostLeviatanVoid gv) {
        var main = Player.main;
        var main2 = VoidGhostLeviathansSpawner.main;
        if (!main || Vector3.Distance(main.transform.position, gv.transform.position) > gv.maxDistanceToPlayer) {
            gv.gameObject.destroy(false);
            if (gv.gameObject == voidLeviathan)
                voidLeviathan = null;
            return;
        }

        var spikeType = gv.gameObject.GetComponentInChildren<VoidSpikeLeviathan.VoidSpikeLeviathanAI>();
        bool spike = spikeType;
        var zone = VoidSpikesBiome.instance.isPlayerInLeviathanZone(main.transform.position);
        var validVoid = spike ? zone : !(zone && isLeviathanEnabled()) && main2.IsPlayerInVoid();
        var flag = main2 && validVoid;
        gv.updateBehaviour = flag;
        gv.AllowCreatureUpdates(gv.updateBehaviour);
        if (spike && UnityEngine.Random.Range(0, 100) == 0) {
            //SoundManager.playSoundAt(SeaToSeaMod.voidspikeLeviFX, gv.gameObject.transform.position, false, 128);
        }

        if (flag || (spike && Vector3.Distance(main.transform.position, gv.transform.position) <= 50)) {
            if (false) {
                gv.Aggression.Value = 0;
                gv.lastTarget.target = getOrCreateTarget();
            } else {
                gv.Aggression.Add(spike ? 2.5F : 1);
                gv.lastTarget.target = main.gameObject;
            }
        } else {
            var a = gv.transform.position - main.transform.position;
            var vector = gv.transform.position + a * gv.maxDistanceToPlayer;
            vector.y = Mathf.Min(vector.y, -50f);
            gv.swimBehaviour.SwimTo(vector, 30f);
        }
    }

    private double getAvoidanceChance(Player ep, SeaMoth sm, bool edge, bool far) {
        if (isLeviathanEnabled() && VoidSpikesBiome.instance.isPlayerInLeviathanZone(ep.transform.position))
            return 0;
        var time = DayNightCycle.main.timePassedAsFloat;
        var ping = sm.gameObject.EnsureComponent<SeamothStealthManager>();
        if (ping.nextStealthValidityTime >= 0 && time < ping.nextStealthValidityTime)
            return 0;
        var minDist = double.PositiveInfinity;
        foreach (var go in VoidGhostLeviathansSpawner.main.spawnedCreatures) {
            var dist = Vector3.Distance(go.transform.position, sm.transform.position);
            minDist = Math.Min(dist, minDist);
        }

        var frac2 = double.IsPositiveInfinity(minDist) || double.IsNaN(minDist) ? 0 : Math.Max(0, 1 - minDist / 120D);
        //SNUtil.writeToChat(minDist.ToString("000.0")+" > "+frac2.ToString("0.0000"));
        if (frac2 >= 1)
            return 0;
        double depth = -sm.transform.position.y;
        if (depth < MAXDEPTH)
            return Math.Pow(1 - frac2, 2);
        var over = depth - MAXDEPTH;
        double fade = sm.lightsActive ? 100 : 200;
        var frac = Math.Min(1, over / fade);
        return 1D - Math.Max(frac, frac2);
    }

    public void temporarilyDisableSeamothStealth(SeaMoth sm, float duration) {
        if (disableStealthInterruptions)
            return;
        if (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
            duration *= 1.5F;
        var ping = sm.gameObject.EnsureComponent<SeamothStealthManager>();
        ping.nextStealthValidityTime = Mathf.Max(
            ping.nextStealthValidityTime,
            DayNightCycle.main.timePassedAsFloat + duration
        );
    }

    public void triggerCyclopsEMP(SubRoot sub, float time) {
        if (time > nextCyclopsEMPTime) {
            nextCyclopsEMPTime = time + UnityEngine.Random.Range(10F, 30F);
            var d = UnityEngine.Random.Range(96F, 150F);
            var pos = sub.transform.position + Camera.main.transform.forward * d;
            pos = MathUtil.getRandomVectorAround(pos, 45);
            pos = sub.transform.position + (pos - sub.transform.position).SetLength(d);
            spawnEMPBlast(pos);
        }
    }

    private class SeamothStealthManager : MonoBehaviour {
        internal float nextStealthValidityTime = -1;

        private float lastBiomeCheckTime;

        private void Update() {
            var time = DayNightCycle.main.timePassedAsFloat;
            if (time - lastBiomeCheckTime >= 1) {
                lastBiomeCheckTime = time;
                var at = BiomeBase.GetBiome(transform.position);
                if (!at.IsVoidBiome())
                    nextStealthValidityTime = -1;
            }
        }
    }

    private class VoidSparkFX : MonoBehaviour {
        internal Vector3 velocity;
        internal Vector3 relativePosition;

        private void Update() {
            gameObject.transform.position += velocity * Time.deltaTime;
        }
    }

    private class FlashBurst : MonoBehaviour {
        private void Start() {
            startTime = Time.time;
            SetProgress(0f);
        }

        private void Update() {
            var num = Mathf.InverseLerp(startTime, startTime + lifeTime, Time.time);
            num = Mathf.Clamp01(num);
            SetProgress(num);
            if (Mathf.Approximately(num, 1)) {
                gameObject.destroy(false);
            }
        }

        private void SetProgress(float progress) {
            currentBlastRadius = blastRadius.Evaluate(progress);
            currentBlastHeight = blastHeight.Evaluate(progress);
            transform.localScale = new Vector3(currentBlastRadius, currentBlastHeight, currentBlastRadius);
        }

        public void OnTouch(Collider collider) {
            //SNUtil.writeToChat(collider+">"+ep);
            if (collider.isPlayer()) {
                instance.onFlashHit();
                return;
            }

            var v = collider.gameObject.FindAncestor<Vehicle>();
            if (v && v == Player.main.GetVehicle()) {
                instance.onFlashHit();
                return;
            }

            var s = collider.gameObject.FindAncestor<SubRoot>();
            if (s && s == Player.main.currentSub) {
                instance.onFlashHit();
                return;
            }
        }

        public float lifeTime = 0.5F;

        public AnimationCurve blastRadius = AnimationCurve.Linear(0f, 0f, 1f, 200F);

        public AnimationCurve blastHeight = AnimationCurve.Linear(0f, 0f, 1f, 150F);

        private float startTime;

        private float currentBlastRadius;

        private float currentBlastHeight;
    }
}