using System;
using System.Collections.Generic;
using System.Linq;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public static partial class ECHooks {
    private static readonly HashSet<string> bloodVine = [];

    private static float lastPiezoEMPDamage = -1;

    private static float lastSonarUsed = -1;
    private static float lastHornUsed = -1;

    internal static float nextVoidTongueGrab = -1;

    internal static readonly SimplexNoiseGenerator tongueDepthNoise =
        (SimplexNoiseGenerator)new SimplexNoiseGenerator(587129).setFrequency(0.08);
    //internal static readonly SimplexNoiseGenerator heatColumnNoise = (SimplexNoiseGenerator)new SimplexNoiseGenerator(2176328).setFrequency(0.1);

    internal static List<Vector3> heatColumns = [];

    static ECHooks() {
        SNUtil.Log("Initializing ECHooks");
        DIHooks.OnWorldLoadedEvent += onWorldLoaded;

        DIHooks.OnSkyApplierSpawnEvent += onSkyApplierSpawn;
        DIHooks.OnDamageEvent += onTakeDamage;
        DIHooks.OnKnifedEvent += onKnifed;
        DIHooks.KnifeAttemptEvent += tryKnife;
        DIHooks.KnifeHarvestEvent += getKnifeHarvest;
        DIHooks.OnItemPickedUpEvent += onPickup;

        DIHooks.ItemTooltipEvent += FoodEffectSystem.instance.applyTooltip;
        DIHooks.OnEatEvent += FoodEffectSystem.instance.onEaten;

        DIHooks.OnPlayerTickEvent += tickPlayer;
        DIHooks.OnPrawnTickEvent += tickPrawn;
        DIHooks.OnCyclopsTickEvent += tickCyclops;

        DIHooks.OnSeamothModulesChangedEvent += updateSeamothModules;

        DIHooks.OnEmpHitEvent += onEMPHit;
        DIHooks.OnEmpTouchEvent += onEMPTouch;

        DIHooks.GetTemperatureEvent += getWaterTemperature;

        DIHooks.OnSeamothSonarUsedEvent += pingSeamothSonar;
        DIHooks.OnCyclopsSonarUsedEvent += pingCyclopsSonar;

        DIHooks.DrillableDrillTickEvent += onDrillableTick;

        DIHooks.OnTorpedoExplodeEvent += onTorpedoExploded;

        DIHooks.CanCreatureSeeObjectEvent += checkCreatureCanSee;
        DIHooks.AggressiveToPilotingEvent += checkCreaturePilotedAggression;

        DIHooks.BaseRebuildEvent += onBaseRebuild;

        DIHooks.GrowingPlantTickEvent += tickGrowingPlant;

        DIHooks.TargetabilityEvent += checkTargetingSkip;

        bloodVine.AddRange(VanillaFlora.BLOOD_KELP.getPrefabs(true, true));
    }

    public static void onWorldLoaded() {
        heatColumns.Clear();
        UnityEngine.Random.InitState(SNUtil.GetWorldSeedInt());
        for (var i = 0; i < 100; i++) {
            var vec = new Vector3(UnityEngine.Random.Range(0F, 1000F), 0, UnityEngine.Random.Range(0F, 1000F));
            if (heatColumns.Any(v => (v - vec).sqrMagnitude <= 40000))
                continue;
            heatColumns.Add(vec);
        }

        SNUtil.Log("Computed heat columns: " + heatColumns.ToDebugString());
    }

    public static void tickPrawn(Exosuit e) {
        if (true) //lights always on
            GlowOil.handleLightTick(e.transform);
    }

    public static void tickCyclops(SubRoot sub) {
        if (sub.subLightsOn) {
            GlowOil.handleLightTick(sub.transform);
            if (UnityEngine.Random.Range(0F, 1F) <= 0.04F)
                attractToLight(sub);
        }
    }

    public static void tickPlayer(Player ep) {
        var time = DayNightCycle.main.timePassedAsFloat;
        GlowOil.checkPlayerLightTick(time, ep);

        var dT = Time.deltaTime;
        CompassDistortionSystem.instance.tick(time, dT);

        var f = 0.3F - DayNightCycle.main.GetLightScalar() * 0.15F;
        if (!ep.IsInBase()) {
            var data = PlanktonCloud.getSpawnData(BiomeBase.GetBiome(ep.transform.position));
            if (data != null) {
                if (UnityEngine.Random.Range(0F, 1F) <= f * dT * data.spawnSuccessRate)
                    EcoceanMod.plankton.tickSpawner(ep, data, dT);
            }
        }

        var vv = ep.GetVehicle();
        var pos = ep.transform.position;
        var inColumn = false;
        if (pos.SetY(0).magnitude >= 1700) { //more than 1200m from center
            EcoceanMod.voidBubble.tickSpawner(ep, time, dT);
            Vector3 mod = pos.Modulo(1000);
            var offset = pos - mod;
            foreach (var col in heatColumns) {
                var dist = (col.SetY(mod.y) - mod).magnitude;
                var at = col + offset;
                if (dist > 200 || !isVoidHeatColumn(at, out var trash, true))
                    continue;
                var inCol = dist <= 18;
                inColumn |= inCol;
                //SNUtil.writeToChat("Heat Column at " + col+" ("+inCol+")");
                for (var k = 0; k < 3; k++) {
                    var anyY = false;
                    var forceCenter = false;
                    var forcedY = float.NaN;
                    float yRange = -1;
                    Vector3 vec2 = MathUtil.getRandomVectorAround(at, 0, 15);
                    string id = EcoceanMod.heatBubble.ClassID;
                    //if (UnityEngine.Random.Range(0F, 1F) < 0.03F) { replace with scoop
                    //	id = EcoceanMod.voidOrganic.ClassID;
                    //}
                    if (UnityEngine.Random.Range(0F, 1F) < 0.075F) {
                        id = EcoceanMod.heatColumnBones.Values.GetRandomEntry().ClassID;
                        yRange = (float)MathUtil.linterpolate(dist, 80, 200, 100, 300);
                    } /*
                    else if (UnityEngine.Random.Range(0F, 1F) < 0.2F) {
                        id = EcoceanMod.heatColumnFog.ClassID;
                        anyY = true;
                    }*/ else if (UnityEngine.Random.Range(0F, 1F) < 0.03F) {
                        id = EcoceanMod.heatColumnShell.ClassID;
                        //anyY = true;
                        forceCenter = true;
                        forcedY = pos.y;
                    }

                    if (anyY) {
                        vec2 = vec2.SetY(pos.y - 200F + UnityEngine.Random.Range(0, 400F));
                    } else {
                        vec2 = vec2.SetY(pos.y - 100);
                    }

                    if (forceCenter)
                        vec2 = at.SetY(vec2.y);
                    if (!float.IsNaN(forcedY))
                        vec2 = vec2.SetY(forcedY);
                    if (yRange > 0)
                        vec2 += Vector3.up * UnityEngine.Random.Range(0F, yRange);
                    GameObject go = ObjectUtil.createWorldObject(id);
                    //SNUtil.log("Spawning object '"+id+"' in Heat Column at " + vec2);
                    go.transform.position = vec2;
                    go.transform.rotation = UnityEngine.Random.rotationUniform;
                    go.fullyEnable();
                }
            }

            if (inColumn) {
                if (!ep.currentSub && !vv) {
                    ep.rigidBody.AddForce(Vector3.up * Time.deltaTime * 150, ForceMode.Acceleration);
                    //ep.liveMixin.TakeDamage(5 * Time.deltaTime, type: DamageType.Acid);
                } else if (vv) {
                    //vv.liveMixin.TakeDamage(5 * Time.deltaTime, type: DamageType.Acid);
                }

                Story.StoryGoal.Execute("EnteredHeatColumn", Story.GoalType.Story);
            }
        }

        float minDepth = vv ? 800 : 1000;
        var maxDepth = minDepth + 300;
        minDepth += 50F * (float)tongueDepthNoise.getValue(ep.transform.position);
        HashSet<BiomeBase> biomes = [VanillaBiomes.Void];
        if (ep.currentSub) {
            minDepth = 600;
            maxDepth = 750;
            biomes.Add(BiomeBase.GetBiome("Void Spikes"));
        }

        if (time - getLastSonarUse() <= 5 || time - getLastHornUse() <= 5) {
            minDepth = 675;
            maxDepth = 900;
        }

        var ec = vv ? vv.GetComponent<ECMoth>() : null;
        if (ec) {
            minDepth -= 50 * ec.stuckCells;
            if (minDepth < 600)
                minDepth = 600;
        }

        if (inColumn && minDepth < 800)
            minDepth = 800;
        maxDepth = Mathf.Max(maxDepth, minDepth + 100);
        if (pos.y <= -UnityEngine.Random.Range(minDepth, maxDepth) && biomes.Contains(BiomeBase.GetBiome(pos))) {
            //if (UnityEngine.Object.FindObjectsOfType<VoidTongueTag>().Length == 0)
            //	SNUtil.writeToChat("Check void grab time = "+time.ToString("000.0")+"/"+nextVoidTongueGrab.ToString("000.0"));
            attemptTongueGrab();
        }
    }

    public static void attemptTongueGrab() {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time >= nextVoidTongueGrab) {
            var ep = Player.main;
            var pos = ep.transform.position;
            nextVoidTongueGrab = time + 10;
            GameObject go = ObjectUtil.createWorldObject(EcoceanMod.tongue.ClassID);
            go.fullyEnable();
            var depth = Mathf.Min(pos.y - UnityEngine.Random.Range(400F, 500F) * (ep.currentSub ? 2 : 1));
            Vector3 put = MathUtil.getRandomVectorAround(pos, 60).SetY(depth);
            go.transform.position = put;
            //go.transform.position = MathUtil.getRandomVectorAround(pos+Camera.main.transform.forward.normalized*400, 40).setY(-1600);
            var v = go.GetComponent<VoidTongueTag>();
            v.enabled = true;
            v.startGrab(
                Mathf.Max(
                    -depth - (ep.currentSub ? 250 : 150),
                    -pos.y + UnityEngine.Random.Range(200F, 400F) * (ep.currentSub ? 0.75F : 1)
                )
            );
        }
    }

    public static bool isVoidHeatColumn(Vector3 vec, out Vector3 colCenter, bool biomeOnly = false) {
        //2200 is significantly offshore
        colCenter = Vector3.zero;
        if (!(VanillaBiomes.Void.IsInBiome(vec) && vec.SetY(0).magnitude >= 2200))
            return false;
        if (biomeOnly)
            return true;
        Vector3 mod = vec.Modulo(1000);
        foreach (var v in heatColumns) {
            if ((v - mod).SetY(0).sqrMagnitude <= 500) {
                colCenter = v;
                return true;
            }
        }

        return false;
        //return heatColumnNoise.getValue(vec) > 0.8 && VanillaBiomes.VOID.isInBiome(vec) && vec.setY(0).magnitude >= 2200;
    }

    public static void onEMPTouch(EMPBlast e, Collider c) {
        if (c.isPlayer()) {
            CompassDistortionSystem.instance.onHitByEMP(
                e,
                isPiezo(e) ? 10 : 1
            ); //piezo is only the 15s base since electrionicsDisableTime is zero for piezo
        }
    }

    public static void onEMPHit(EMPBlast e, GameObject go) { //might be called many times
        if (isPiezo(e)) {
            //SNUtil.writeToChat("Match");
            var time = DayNightCycle.main.timePassedAsFloat;
            var sub = go.gameObject.FindAncestor<SubRoot>();
            var amt = UnityEngine.Random.Range(1F, 4F);
            var v = go.gameObject.FindAncestor<Vehicle>();
            if (v) {
                if (time >= lastPiezoEMPDamage + 1F) {
                    go.GetComponent<LiveMixin>().TakeDamage(
                        UnityEngine.Random.Range(10F, 20F),
                        v.transform.position,
                        DamageType.Electrical,
                        e.gameObject
                    );
                    lastPiezoEMPDamage = time;
                }

                v.ConsumeEnergy(amt * 3); //must be first as will no-op if electronics is disabled
                if (amt > 3)
                    v.energyInterface.DisableElectronicsForTime(amt - 3);
                if (v is SeaMoth sm)
                    ObjectUtil.createSeamothSparkSphere(sm);
            } else if (sub && sub.isCyclops) {
                sub.powerRelay.ConsumeEnergy(amt * 6, out var trash);
                if (amt > 2)
                    sub.powerRelay.DisableElectronicsForTime((amt - 2) * 3);
            }
        }
    }

    private static bool isPiezo(EMPBlast e) {
        return e.gameObject.name.StartsWith("PiezoCrystal_EMPulse", StringComparison.InvariantCultureIgnoreCase);
    }

    public static float getLastSonarUse() {
        return lastSonarUsed;
    }

    public static float getLastHornUse() {
        return lastHornUsed;
    }

    public static void updateSeamothModules(SeaMoth sm, int slotID, TechType tt, bool added) { /*
        if (added && sm.storageInputs != null && slotID < sm.storageInputs.Length) {
            if (tt == EcoceanMod.planktonScoop.TechType) {
                sm.storageInputs[slotID].gameObject.SetActive(true);
                sm.storageInputs[slotID].enabled = false;
                sm.storageInputs[slotID].gameObject.EnsureComponent<PlanktonScoopInteraction>().enabled = true;
            }
            else if (tt == TechType.VehicleStorageModule) {
                sm.storageInputs[slotID].gameObject.SetActive(true);
                sm.storageInputs[slotID].enabled = true;
                sm.storageInputs[slotID].gameObject.EnsureComponent<PlanktonScoopInteraction>().enabled = false;
            }
            else {
                sm.storageInputs[slotID].gameObject.SetActive(false);
            }
        }*/
    }

    /*
    class PlanktonScoopInteraction : MonoBehaviour, IHandTarget {

    }
    */
    public static void onTakeDamage(DIHooks.DamageToDeal dmg) {
        BaseRoot bb = dmg.Target.gameObject.FindAncestor<BaseRoot>();
        if (bb && !dmg.Target.gameObject.FindAncestor<Planter>()) { //bases but NOT farmed plants
            var str = bb.GetComponent<BaseHullStrength>();
            if (str.totalStrength > BaseHullStrength.InitialStrength) {
                var surplus = str.totalStrength - BaseHullStrength.InitialStrength;
                var f = 1 / (1 + surplus * 0.1F); //halve at 20 str, third at 30 str, quarter at 40 str, etc
                dmg.SetValue(dmg.GetAmount() * f);
            } else if (str.totalStrength > 0) {
                dmg.SetValue(
                    dmg.GetAmount() * (float)MathUtil.linterpolate(
                        str.totalStrength,
                        0,
                        BaseHullStrength.InitialStrength,
                        2,
                        1,
                        true
                    )
                ); //increase to 2x from 10 to 0
            } else if (str.totalStrength >= -1) {
                dmg.SetValue(dmg.GetAmount() * 2);
            } else if (str.totalStrength < -1) {
                dmg.SetValue(
                    dmg.GetAmount() * (1 - str.totalStrength)
                ); //increase by 100% for every point under -1, plus an additional 100%
            }

            //SNUtil.writeToChat("Base damage being modified due to strength "+str.totalStrength+": "+dmg.originalAmount.ToString("0.000")+" > "+dmg.getAmount().ToString("0.000"));
            return;
        }

        Player ep = dmg.Target.gameObject.FindAncestor<Player>();
        if (ep) {
            float f = 0;
            switch (dmg.Type) {
                case DamageType.Normal:
                case DamageType.Puncture:
                    f = 1;
                    break;
                case DamageType.Collide:
                case DamageType.Drill:
                    f = 0.67F;
                    break;
                case DamageType.Heat:
                case DamageType.Acid:
                case DamageType.Explosive:
                case DamageType.LaserCutter:
                case DamageType.Fire:
                    f = 0.33F;
                    break;
            }

            if (f > 0) {
                //dmg.target.GetComponent<ExplodingAnchorPod>());
                //SNUtil.writeToChat("Player '"+ep+"' took damage");
                foreach (Biter b in WorldUtil.getObjectsNearWithComponent<Biter>(ep.transform.position, 60 * f)) {
                    AttractToTarget.attractCreatureToTarget(b, ep, false);
                    //SNUtil.writeToChat("Attracted biter "+b+" @ "+b.transform.position);
                }
            }
        }

        if (dmg.Type == DamageType.Electrical && dmg.Dealer && dmg.Dealer.GetComponentInParent<ElectricalDefense>()) {
            dmg.SetValue(Mathf.Min(EcoceanMod.config.getFloat(ECConfig.ConfigEntries.DEFENSECLAMP), dmg.GetAmount()));
            //SNUtil.writeToChat(dmg.target+" > "+dmg.getAmount()+" from "+dmg.originalAmount);
        }

        Creature c = dmg.Target.gameObject.FindAncestor<Creature>();
        if (c is SeaDragon || c is GhostLeviatanVoid || c is GhostLeviathan || c is ReaperLeviathan || c is Reefback) {
            float f = EcoceanMod.config.getFloat(ECConfig.ConfigEntries.LEVIIMMUNE);
            if (f > 0) {
                dmg.SetValue(Mathf.Max(0.001F, dmg.GetAmount() * (1 - f)));
            }
        }

        PrefabIdentifier pi = dmg.Target.GetComponent<PrefabIdentifier>();
        if (pi && pi.ClassId == DecoPlants.PINK_BULB_STACK.prefab)
            dmg.SetValue(0.1F);
        if (pi && pi.ClassId == VanillaCreatures.PRECURSORCRAB.prefab && dmg.Type == DamageType.Drill) //player prawn
            precursorCrabRetaliate(dmg.Target, false);
    }

    public static void tryKnife(DIHooks.KnifeAttempt k) {
        if (CraftData.GetTechType(k.Target.gameObject) == EcoceanMod.pinkBulbStack.TechType) {
            k.AllowKnife = true;
            return;
        } else if (CraftData.GetTechType(k.Target.gameObject) == EcoceanMod.mushroomVaseStrand.TechType) {
            k.AllowKnife = true;
            return;
        } else if (CraftData.GetTechType(k.Target.gameObject) == TechType.LargeFloater) {
            k.AllowKnife = true;
            return;
        }
    }

    public static void onKnifed(GameObject go) {
        var tt = CraftData.GetTechType(go);
        if (tt == TechType.LargeFloater) {
            DIHooks.FireKnifeHarvest(go, new Dictionary<TechType, int> { { TechType.Floater, 1 } });
            return;
        }

        if (tt == TechType.RedTipRockThings) {
            DIHooks.FireKnifeHarvest(
                go,
                new Dictionary<TechType, int> { { TechType.CoralChunk, 1 } }
            ); //coral tube piece, for disinfected water
            return;
        }

        if (tt == TechType.PrecursorDroid) {
            precursorCrabRetaliate(go, true);
            return;
        }

        if (tt == EcoceanMod.mushroomVaseStrand.TechType) {
            var tag = go.GetComponent<MushroomVaseStrand.MushroomVaseStrandTag>();
            if (tag && tag.isHarvested()) {
                tag.health.TakeDamage(9999);
            } else {
                DIHooks.FireKnifeHarvest(
                    go,
                    new Dictionary<TechType, int> { { EcoceanMod.mushroomVaseStrand.seed.TechType, 1 } }
                );
            }

            return;
        }

        var e = go.FindAncestor<ExplodingAnchorPod>();
        if (e) {
            e.explode();
            return;
        }

        var vb = go.FindAncestor<VoidBubbleTag>();
        if (vb) {
            vb.Disconnect();
            return;
        }
    }

    public static void precursorCrabRetaliate(GameObject go, bool single) {
        var tgt = Player.main.gameObject;
        var v = Player.main.GetVehicle();
        if (v)
            tgt = v.gameObject;
        var lv = tgt.GetComponent<LiveMixin>();
        lv.TakeDamage(
            lv.maxHealth * (single ? 0.67F : Time.deltaTime * 6.0F),
            go.transform.position,
            DamageType.LaserCutter,
            go
        );
        WorldUtil.spawnParticlesAt(
            Vector3.Lerp(go.transform.position, tgt.transform.position, 0.5F),
            "361b23ed-58dd-4f45-9c5f-072fa66db88a",
            0.5F,
            true
        );
    }

    public static void getKnifeHarvest(DIHooks.KnifeHarvest h) {
        if (h.ObjectType == EcoceanMod.mushroomVaseStrand.TechType && h.Hit.isFarmedPlant()) {
            h.Hit.FindAncestor<MushroomVaseStrand.MushroomVaseStrandTag>().tryHarvest();
        }
    }

    public static void onPickup(DIHooks.ItemPickup ip) {
        Pickupable pp = ip.Item;
        FoodEffectSystem.instance.ensureEatable(pp);
        var g = pp.GetComponent<GlowOilTag>();
        if (g) {
            g.resetGlow();
        }
    }

    public static void getWaterTemperature(DIHooks.WaterTemperatureCalculation calc) {
        //SNUtil.writeToChat("EC: Checking water temp @ "+calc.position+" def="+calc.originalValue);
        LavaBomb.iterateLavaBombs(lb => {
                var dist = Vector3.Distance(lb.transform.position, calc.Position);
                if (dist <= LavaBomb.HEAT_RADIUS) {
                    var f2 = dist / LavaBomb.HEAT_RADIUS;
                    var f = 1F - f2 * f2 * 4;
                    //SNUtil.writeToChat("Found lava bomb "+lb.transform.position+" at dist "+dist+" > "+f+" > "+(f*lb.getTemperature()));
                    calc.SetValue(Mathf.Max(calc.GetTemperature(), f * lb.getTemperature()));
                }
            }
        );
        if (isVoidHeatColumn(calc.Position, out var trash)) {
            //SNUtil.writeToChat("Computing water temp @ " + calc.position + " in heat column " + trash);
            calc.SetValue(Mathf.Max(calc.GetTemperature(), 60));
        }
    }

    public static void onSkyApplierSpawn(SkyApplier pk) {
        var go = pk.gameObject;
        if (go.name.StartsWith("Seamoth", StringComparison.InvariantCultureIgnoreCase) && go.name.EndsWith(
                "Arm(Clone)",
                StringComparison.InvariantCultureIgnoreCase
            ))
            return;
        else if (pk.GetComponent<Player>()) {
            go.EnsureComponent<PlantHidingTracker>().minRadius = 0;
        }

        var pi = go.FindAncestor<PrefabIdentifier>();
        if (pi) {
            if (go.isAnchorPod() && !isSeaTreaderCave(go))
                go.EnsureComponent<ExplodingAnchorPod>();
            else if (bloodVine.Contains(pi.ClassId))
                go.EnsureComponent<PredatoryBloodvine>();
            else if (pi.ClassId == VanillaCreatures.REEFBACK.prefab) {
                go.EnsureComponent<ECReefback>();
                go.EnsureComponent<ReefbackJetSuctionManager>();
            } else if (pi.ClassId == VanillaCreatures.REAPER.prefab)
                go.EnsureComponent<ECReaper>();
            else if (pi.ClassId == VanillaCreatures.SEA_TREADER.prefab)
                go.EnsureComponent<ECTreader>();
            else if (pi.ClassId == VanillaCreatures.SEADRAGON.prefab)
                go.EnsureComponent<ECDragon>();
            else if (pi.ClassId == VanillaCreatures.EMPEROR_JUVENILE.prefab)
                go.EnsureComponent<ECEmperor>();
            else if (VanillaFlora.CREEPVINE_FERTILE.includes(pi.ClassId) ||
                     VanillaFlora.CREEPVINE.includes(pi.ClassId)) {
                go.EnsureComponent<CreepvineSonarScatterer>();
                if (VanillaFlora.CREEPVINE_FERTILE.includes(pi.ClassId)) {
                    var ctr = CreepvineCollisionDetector.addCreepvineSeedCollision(go);
                    if (go.gameObject.FindAncestor<Planter>()) {
                        var l = pi.GetComponentInChildren<Light>();
                        l.intensity = 0.95F;
                    }

                    foreach (var c in go.GetComponentsInChildren<InteractionVolumeCollider>()) {
                        if (c.gameObject.name == "core")
                            PlantHidingCollider.addToObject(c, new Color(0.1F, 0.5F, 0.1F));
                    }
                } else {
                    PlantHidingCollider.addToObject(go.GetComponent<Collider>(), new Color(0.1F, 0.5F, 0.1F));
                }
            } else if (VanillaFlora.CAVE_BUSH.includes(pi.ClassId)) {
                PlantHidingCollider.addToObject(
                    go.GetComponentInChildren<Collider>(),
                    new Color(130 / 255F, 73 / 255F, 183 / 255F)
                );
            } else if (VanillaFlora.MUSHROOM_DISK.includes(pi.ClassId)) {
                go.removeChildObject("StrandHolder");
                /*
                if (mushroomTendrilNoise == null)
                    mushroomTendrilNoise = ((Simplex3DGenerator)new Simplex3DGenerator(SNUtil.getWorldSeed()).setFrequency(0.2F));
                if (mushroomTendrilNoise.getValue(go.transform.position) > 0.4F) {
                    GameObject strands = go.getChildObject("StrandHolder");
                    if (!strands) {
                        strands = new GameObject("StrandHolder");
                        strands.transform.SetParent(go.transform);
                        Utils.ZeroTransform(strands.transform);
                    }
                    MeshRenderer[] r0 = strands.GetComponentsInChildren<MeshRenderer>();
                    //float r = Mathf.Max(go.transform.localScale.x, go.transform.localScale.z)*MushroomTendril.getPrefabRadius(pi.ClassId);
                    //int max = 2+pi.GetHashCode()%3;
                    //for (int i = r0.Length; i < max; i++) {
                    BoxCollider bc = go.GetComponentInChildren<BoxCollider>();
                    float r = Mathf.Max(bc.size.x, bc.size.z)*1.5F;
                        GameObject strand = ObjectUtil.createWorldObject(EcoceanMod.mushroomTendrils[go.transform.position.x > 0 ? 1 : 0].ClassID);
                        strand.transform.SetParent(strands.transform);
                        float ang = Mathf.Deg2Rad*UnityEngine.Random.Range(0F, 360F);
                        //strand.transform.localPosition = new Vector3(r*Mathf.Cos(ang), -1.5F, r*Mathf.Sin(ang));
                        strand.transform.localPosition = new Vector3(0, -bc.center.z, 0.2F*Mathf.Sqrt(r));
                        strand.transform.localEulerAngles = new Vector3(90, 0, 0);
                        strand.transform.localScale = new Vector3(r, Mathf.Sqrt(r)-0.05F, r);
                    //}
                }*/
                GameObject rain = go.getChildObject("RainHolder");
                if (!rain) {
                    rain = new GameObject("RainHolder");
                    rain.transform.SetParent(go.transform);
                    Utils.ZeroTransform(rain.transform);
                    rain.transform.localEulerAngles = new Vector3(90, 0, 0);
                }

                rain.EnsureComponent<MushroomDiskRain>();
            } else if (VanillaFlora.SPOTTED_DOCKLEAF.includes(pi.ClassId)) {
                PlantHidingCollider.addToObject(go.GetComponentInChildren<Collider>(), new Color(0.1F, 0.4F, 1F));
            } /* add component that makes pulse with light?
            else if (VanillaFlora.MUSHROOM_DISK.includes(pi.ClassId)) {
                foreach (Renderer r in pi.GetComponentsInChildren<Renderer>()) {
                    r.materials[0].SetFloat("_GlowStrengthNight", 0.25F);
                    if () {
                        r.materials[0].SetColor("_GlowColor", new Color(0.8F, 1.0F, 0.1F));
                    }
                }
            }*/ else if (pi.ClassId == VanillaResources.NICKEL.prefab ||
                         pi.ClassId == VanillaResources.LARGE_NICKEL.prefab) {
                foreach (var r in pi.GetComponentsInChildren<Renderer>()) {
                    RenderUtil.swapTextures(EcoceanMod.modDLL, r, "Textures/Nickel");
                    RenderUtil.setGlossiness(r, 4, 2, 0.4F);
                }
            } else if (pi.ClassId == VanillaResources.LARGE_URANIUM.prefab) {
                /*
                RadiatePlayerInRange rad = go.EnsureComponent<RadiatePlayerInRange>(); //will not do damage unless add a DamagePlayerInRadius
                rad.tracker = go.EnsureComponent<PlayerDistanceTracker>();
                rad.tracker.timeBetweenUpdates = 0.75F;//0.3F;
                rad.radiateRadius = 10;
                rad.tracker.maxDistance = rad.radiateRadius;
                */
                AoERadiationZone aoe = go.EnsureComponent<AoERadiationZone>();
                aoe.setRadii(10, 2);
                aoe.maxIntensity = 0.3F;
            } else if (pi.ClassId == "1c34945a-656d-4f70-bf86-8bc101a27eee") {
                go.EnsureComponent<ECMoth>();
                go.EnsureComponent<PlantHidingTracker>().minRadius = 4;
            } else if (pi.ClassId == DecoPlants.VINE_TREE.prefab || pi.ClassId == DecoPlants.VINE_TREE_2.prefab) {
                foreach (var r in go.GetComponentsInChildren<Renderer>())
                    r.materials[0].EnableKeyword("UWE_WAVING"); //make leaves move
            } else if (pi.ClassId == DecoPlants.PINK_BULB_STACK.prefab) {
                go.EnsureComponent<TechTag>().type = EcoceanMod.pinkBulbStack.TechType;
                var lv = go.GetComponent<LiveMixin>();
                lv.data.maxHealth = 100;
            } else if (pi.ClassId == DecoPlants.MUSHROOM_VASE_STRANDS.prefab) {
                go.EnsureComponent<TechTag>().type = EcoceanMod.mushroomVaseStrand.TechType;
                var lv = go.GetComponent<LiveMixin>();
                lv.data.maxHealth = 100;
                MushroomVaseStrand.setupCollider(go);
            }
        }
    }

    private static bool isSeaTreaderCave(GameObject go) { //skip the c2c prop ones
        return Vector3.Distance(go.transform.position, new Vector3(-1264, -281, -728)) <= 30;
    }

    public static void onGeyserSpawn(Geyser g) {
        g.gameObject.EnsureComponent<GeyserSonarSignal>();
        var cc = g.GetComponent<CapsuleCollider>();
        cc.center += Vector3.down * 1.5F;
        cc.height += 3.5F;
    }

    public static void tickObjectInGeyser(Geyser g, Collider c) {
        if (g.erupting) {
            //SNUtil.writeToChat(c.gameObject.name);
            var v = c.gameObject.FindAncestor<Vehicle>();
            if (v) {
                var ec = v.GetComponent<ECMoth>();
                if (ec) {
                    ec.lastGeyserTime = DayNightCycle.main.timePassedAsFloat;
                }

                if (v is SeaMoth) { //will set temp and do a ton of damage
                    v.GetComponentInChildren<DIHooks.LavaWarningTriggerDetector>().MarkGeyserDetected();
                } else if (v is Exosuit) {
                    v.liveMixin.TakeDamage(
                        g.damage * 0.04F * Time.deltaTime,
                        c.transform.position,
                        DamageType.Fire,
                        g.gameObject
                    );
                }
            }

            var sub = c.gameObject.FindAncestor<SubRoot>();
            if (sub && sub.isCyclops && sub.thermalReactorUpgrade) {
                sub.powerRelay.AddEnergy(5 * Time.deltaTime, out var num);
            }

            if (!sub) {
                var rb = c.gameObject.FindAncestor<Rigidbody>();
                if (rb && !rb.isKinematic) {
                    var dh = rb.transform.position.y - (g.transform.position.y - 1.5F);
                    var f = 1F - dh / 30;
                    if (v)
                        f *= 0.1F; /*
                    Vector3 vec = (rb.transform.position.setY(0)-g.transform.position.setY(0)).normalized;
                    float f2 = (dh/50F)+1;
                    vec *= f2*f2*f2*f2*0.025F;
                    vec.y = 1;*/
                    var vec = Vector3.up;
                    vec *= 80 * f;
                    rb.AddForce(vec, ForceMode.Force);
                    if (!v && !rb.isPlayer()) {
                        var obj = rb.gameObject.EnsureComponent<GeyserDisplacement>();
                        obj.geyser = g;
                        obj.destroyIn = DayNightCycle.main.timePassedAsFloat + 1.5F;
                    }
                }
            }
        }
    }

    private class GeyserDisplacement : MonoBehaviour {
        internal Geyser geyser;
        internal float destroyIn;

        private Rigidbody body;

        private void Update() {
            if (!body)
                body = GetComponent<Rigidbody>();
            if (DayNightCycle.main.timePassedAsFloat >= destroyIn) {
                this.destroy();
                return;
            }

            if (geyser && geyser.erupting) {
                var dh = transform.position.y - geyser.transform.position.y;
                Vector3 vec = transform.position.SetY(0) - geyser.transform.position.SetY(0);
                if (vec.sqrMagnitude >= 1600) {
                    this.destroy();
                } else {
                    //float f2 = (dh/40F)+1;
                    //vec *= f2*f2*f2*f2*Time.deltaTime;
                    vec *= Time.deltaTime;
                    body.AddForce(vec.normalized * 20, ForceMode.Force);
                    var vel = body.velocity;
                    vel.y *= 0.99F;
                    body.velocity = vel;
                }
            }
        }
    }
    /*
    class ECWaterFilter : MonoBehaviour {

        private FiltrationMachine machine;

        void Update() {
            if (!machine)
                machine = GetComponent<FiltrationMachine>();

            if (machine && machine.timeRemainingSalt > 0 && VanillaBiomes.LOSTRIVER.isInBiome(transform.position)) {
                machine.timeRemainingSalt = Mathf.Max(machine.timeRemainingSalt-DayNightCycle.main.dayNightSpeed*2*Time.deltaTime, 0.2F);
            }
        }

    }*/

    public static void onTorpedoExploded(SeamothTorpedo sm, Transform result) {
        var vortex = result.GetComponent<SeamothTorpedoWhirlpool>();
        //SNUtil.writeToChat(sm+" makes "+result);
        WorldUtil.getGameObjectsNear(
            result.position,
            50,
            go => {
                ExplodingAnchorPod ea = go.FindAncestor<ExplodingAnchorPod>();
                //SNUtil.writeToChat(result+" hits "+go);
                if (ea && UnityEngine.Random.Range(0F, 1F) <= 0.5F &&
                    Vector3.Distance(ea.getEffectivePodCenter(), result.position) <= 25) {
                    ea.scheduleExplode(UnityEngine.Random.Range(0F, 10F));
                }

                if (vortex) {
                    PlanktonCloudTag tag = go.FindAncestor<PlanktonCloudTag>();
                    if (tag)
                        tag.activateBy(result.gameObject);
                }
            }
        );
    }

    public static void honkCyclopsHorn(CyclopsHornButton b) {
        attractToSoundPing(b.gameObject.FindAncestor<SubRoot>(), true, 1);
        lastHornUsed = DayNightCycle.main.timePassedAsFloat;
    }

    public static void pingSeamothSonar(SeaMoth sm) {
        pingSonarFromObject(sm);
    }

    public static void pingCyclopsSonar(SubRoot sb) {
        pingSonarFromObject(sb);
    }

    public static void pingSonarFromObject(MonoBehaviour mb, float strength = 1) {
        attractToSoundPing(mb, false, strength);
        lastSonarUsed = DayNightCycle.main.timePassedAsFloat;
        float r = 120;
        WorldUtil.getGameObjectsNear(
            mb.transform.position,
            r,
            go => {
                var dist = (go.transform.position - mb.transform.position).sqrMagnitude;
                var f = (float)MathUtil.linterpolate(dist, 400, r * r, 0.05F, 0.02F, true);
                if (f > 0 && UnityEngine.Random.Range(0F, 1F) < f) {
                    ExplodingAnchorPod ea = go.GetComponent<ExplodingAnchorPod>();
                    if (ea) {
                        ea.scheduleExplode(0.25F + 0.004F * Mathf.Sqrt(dist));
                    }
                }
            }
        );
    }

    public static void attractToSoundPing(MonoBehaviour obj, bool isHorn, float strength) {
        if (obj is SubRoot sub && sub.isCyclops) {
            var noise = obj.gameObject.GetComponentInChildren<CyclopsNoiseManager>();
            if (noise) {
                noise.noiseScalar *= isHorn ? 6 : 2;
                noise.Invoke("RecalculateNoiseValues", isHorn ? 15 : 10);
            }
        }

        var range = 400 * strength;
        WorldUtil.getGameObjectsNear(
            obj.transform.position,
            range,
            go => tryAttractToSound(go, obj, isHorn, strength, range)
        );
    }

    private static void tryAttractToSound(GameObject go, MonoBehaviour obj, bool isHorn, float strength, float range) {
        var c = go.GetComponent<Creature>();
        if (c && attractedToSound(c, isHorn) && !c.GetComponent<WaterParkCreature>()) {
            var chance = 0.5F *
                         Mathf.Clamp01(1F - Vector3.Distance(c.transform.position, obj.transform.position) / range);
            if (!Mathf.Approximately(strength, 1))
                chance *= Mathf.Sqrt(strength);
            if (isHorn) {
                chance *= 4;
                chance = Mathf.Min(chance, 0.05F);
                if (c is Reefback || c is GhostLeviathan || c is GhostLeviatanVoid || c is ReaperLeviathan ||
                    c is SeaDragon) {
                    chance *= 5;
                    chance = Mathf.Min(chance, 0.25F);
                }

                if (c is Reefback && isHorn) {
                    chance *= 5;
                    chance = Mathf.Min(chance, 0.5F);
                }
            }

            if (UnityEngine.Random.Range(0F, 1F) <= chance)
                AttractToTarget.attractCreatureToTarget(c, obj, isHorn);
        }
    }

    internal static bool attractedToSound(Creature c, bool horn) {
        return c is GhostLeviathan || c is GhostLeviatanVoid || c is ReaperLeviathan || c is SeaDragon ||
               c.gameObject.FindAncestor<PrefabIdentifier>().ClassId == "GulperLeviathan" ||
               (c is Reefback || c is BoneShark ? horn : (c is CrabSnake || c is CrabSquid) && !horn);
    }

    internal static bool attractedToLight(Creature c, MonoBehaviour obj) {
        return c is SeaDragon || (c is BoneShark && !(obj is SubRoot));
    }

    public static void attractToLight(MonoBehaviour obj) {
        float range = obj is SubRoot ? 150 : 80;
        WorldUtil.getGameObjectsNear(obj.transform.position, range, go => tryAttractToLight(go, obj, range));
    }

    private static bool tryAttractToLight(GameObject go, MonoBehaviour obj, float range) {
        var c = go.GetComponent<Creature>();
        if (c && attractedToLight(c, obj) && !c.GetComponent<WaterParkCreature>() &&
            (obj is SubRoot || ObjectUtil.isLookingAt(obj.transform, c.transform.position, 45))) {
            var chance = Mathf.Clamp01(1F - Vector3.Distance(c.transform.position, obj.transform.position) / range);
            if (obj is SeaMoth)
                chance *= obj.GetComponent<ECMoth>().getLightIntensity();
            if (UnityEngine.Random.Range(0F, 1F) <= chance) {
                AttractToTarget.attractCreatureToTarget(c, obj, false);
                return true;
            }
        }

        return false;
    }

    internal static void attractCreaturesToBase(SubRoot sub, float range, Predicate<Creature> rule = null) {
        WorldUtil.getGameObjectsNear(
            sub.transform.position,
            range,
            go => {
                Creature c = go.GetComponent<Creature>();
                if (c && (rule == null || rule.Invoke(c)))
                    AttractToTarget.attractCreatureToTarget(c, sub, false);
            }
        );
    }

    public static void applyCurrentForce(Rigidbody rb, Vector3 force, ForceMode mode, Current c) {
        var wc = c.GetComponent<WaterCurrentTag>();
        var str = wc ? wc.getCurrentStrength(rb.transform.position) : 1;
        if (str > 0)
            rb.AddForce(force * str, mode);
    }

    public static void setHUDCompassDirection(uGUI_Compass compass, float dir) { /* 0-1 for 360 */
        compass.direction =
            (dir + CompassDistortionSystem.instance.getTotalDisplacement(Player.main.transform.position) / 360F) % 1F;
    }

    public static void setCyclopsCompassDirection(Transform obj, Quaternion dir) {
        obj.rotation = dir;
        obj.Rotate(0, CompassDistortionSystem.instance.getTotalDisplacement(obj.position), 0);
    }

    public static void onDrillableTick(Drillable d, Vector3 vec, Exosuit s) {
        var pt = d.GetComponent<PiezoCrystalTag>();
        if (pt)
            pt.onDrilled(vec);
    } /*

    public static void tickVortexTorpedo(SeamothTorpedoWhirlpool go) {

    }*/

    public static bool canMeleeBite(MeleeAttack me, GameObject go) {
        var bc = go.GetComponent<BaseCell>();
        return (bc && canCreatureAttackBase(bc, me)) || me.CanBite(go);
    }

    private static bool canCreatureAttackBase(BaseCell bc, MeleeAttack me) {
        AttractToTarget att = me.GetComponent<AttractToTarget>();
        if (att && att.isTargeting(bc.gameObject))
            return true;
        var c = me.GetComponent<Creature>();
        return c is GhostLeviatanVoid || c is GhostLeviathan || c is SeaDragon || c is CrabSquid || c is Shocker;
    }

    public static GameObject getMeleeTarget(MeleeAttack me, Collider c) {
        var ret = c.gameObject;
        var bc = ret.FindAncestor<BaseCell>();
        if (bc)
            ret = bc.gameObject;
        if (ret.GetComponent<LiveMixin>() == null && c.attachedRigidbody != null)
            ret = c.attachedRigidbody.gameObject;
        return ret;
    }

    public static float getWaterFilterSaltTickTime(float val, FiltrationMachine machine) {
        if (VanillaBiomes.Lostriver.IsInBiome(machine.transform.position))
            val *= 3;
        return val;
    }

    public static void checkCreatureCanSee(DIHooks.CreatureSeeObjectCheck ch) {
        PlantHidingTracker ph = ch.Target.GetComponent<PlantHidingTracker>();
        if (ph && ph.isActive())
            ch.CanSee = false;
    }

    public static void checkCreaturePilotedAggression(DIHooks.AggressiveToPilotingVehicleCheck ch) {
        PlantHidingTracker ph = ch.Vehicle.GetComponent<PlantHidingTracker>();
        if (ph && ph.isActive())
            ch.CanTarget = false;
    }

    public static void onBaseRebuild(Base b) {
        b.gameObject.removeComponent<BaseCellEnviroHandler>();
        foreach (var bc in b.GetComponentsInChildren<BaseCell>()) {
            var ce = bc.gameObject.EnsureComponent<BaseCellEnviroHandler>();
            ce.cell = bc;
            ce.seabase = b;
            ce.computeEnvironment();
        }
    }

    public static void tickGrowingPlant(GrowingPlant g, float prog) {
        //g.gameObject.EnsureComponent<GrowingPlantViabilityTracker>().plant = g;
    }

    public static void checkTargetingSkip(DIHooks.TargetabilityCheck ch) {
        if (ch.Prefab.ClassId == EcoceanMod.plankton.ClassID) {
            PlanktonCloudTag tag = ch.Prefab.GetComponentInChildren<PlanktonCloudTag>();
            ch.AllowTargeting = tag && !tag.isBaseBound && !Player.main.currentSub && !Player.main.GetVehicle();
        }

        if (ch.Prefab.ClassId == EcoceanMod.heatColumnShell.ClassID && PDAScanner.scanTarget.gameObject &&
            PDAScanner.scanTarget.gameObject.GetComponent<HeatColumnShellTag>() &&
            PDAScanner.scanTarget.gameObject != ch.Prefab.gameObject) {
            ch.AllowTargeting = false;
        }
    }
}