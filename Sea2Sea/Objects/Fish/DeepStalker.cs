using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FMOD.Studio;
using Nautilus.Utility;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class DeepStalker : RetexturedFish {
    private readonly XMLLocale.LocaleEntry locale;

    [SetsRequiredMembers]
    internal DeepStalker(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.STALKER.prefab) {
        locale = e;
        glowIntensity = 1.25F;

        ScanTime = 8;
        EggBase = TechType.Stalker;
        EggMaturationTime = 2400;
        EggSpawnRate = 0.25F;
        EggSpawns.Add(BiomeType.GrandReef_TreaderPath);
    }

    public override void prepareGameObject(GameObject world, Renderer[] r0) {
        var kc = world.EnsureComponent<DeepStalkerTag>();
        foreach (var r in r0) {
            r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
        }

        var agv = world.EnsureComponent<AggressiveToPilotingVehicle>();
        agv.aggressionPerSecond = 0.2F;
        agv.creature = world.GetComponent<Creature>();
        agv.lastTarget = world.EnsureComponent<LastTarget>();
    }

    public override BehaviourType GetBehavior() {
        return BehaviourType.Shark;
    }
}

internal class DeepStalkerTag : MonoBehaviour {
    private static readonly SoundManager.SoundData biteSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "deepstalkerbite",
        "Sounds/deepstalker-bite.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 24); }
    );

    private static float lastPlayerBiteTime;

    private Renderer render;
    private Stalker creatureComponent;
    private AggressiveWhenSeeTarget playerHuntComponent;
    private MeleeAttack attackComponent;
    private CollectShiny collectorComponent;
    private SwimBehaviour swimmer;
    private WaterParkCreature acuComponent;

    private readonly Color peacefulColor = new(0.2F, 0.67F, 1F, 1);
    private readonly Color aggressiveColor = new(1, 0, 0, 1);
    private readonly float colorChangeSpeed = 1;

    private float aggressionForColor;
    private float aggressionForACUColor;

    private float platinumGrabTime = -1;
    private float lastAreaCheck = -1;
    private float lastDespawnCheck = -1;

    private float lastTextureSwapTime;

    private float targetCooldown;

    private GameObject currentForcedTarget;

    private SeaTreader treaderTarget;

    private void Start() {
        acuComponent = GetComponent<WaterParkCreature>();
    }

    private void Update() {
        if (!render) {
            render = GetComponentInChildren<Renderer>();
        }

        if (!creatureComponent) {
            creatureComponent = GetComponent<Stalker>();
            creatureComponent.liveMixin.data.maxHealth = 800; //stalker base is 300
        }

        if (!attackComponent) {
            attackComponent = GetComponent<MeleeAttack>();
            attackComponent.biteDamage *= 0.67F;
            attackComponent.biteAggressionDecrement *= 2;
            attackComponent.biteAggressionThreshold *= 0.8F;
            attackComponent.canBeFed = true;
            attackComponent.canBiteCyclops = false;
            attackComponent.canBiteCreature = true;
            attackComponent.canBitePlayer = true;
            attackComponent.canBiteVehicle = true;
            attackComponent.ignoreSameKind = false;
            //attackComponent.attackSound.asset = biteSound.asset;
            //attackComponent.attackSound.path = biteSound.asset.path;
        }

        if (!collectorComponent) {
            collectorComponent = GetComponent<CollectShiny>();
            //collectorComponent.priorityMultiplier.
        }

        if (!swimmer) {
            swimmer = GetComponent<SwimBehaviour>();
        }

        if (!playerHuntComponent) {
            foreach (var agg in GetComponents<AggressiveWhenSeeTarget>()) {
                if (agg.targetType == EcoTargetType.Shark) {
                    agg.aggressionPerSecond *= 0.15F;
                    agg.ignoreSameKind = false;
                    agg.maxRangeScalar *= 1.5F;
                    playerHuntComponent = agg;
                    break;
                }
            }
        }

        var dT = Time.deltaTime;

        if (render && creatureComponent) {
            var target = creatureComponent.Aggression.Value;
            if (acuComponent) {
                if (Random.Range(0F, 1F) <= 0.008F) {
                    aggressionForACUColor = Random.Range(0F, 1F);
                }

                target = aggressionForACUColor;
            }

            if (aggressionForColor < target) {
                aggressionForColor = Mathf.Min(target, aggressionForColor + dT * colorChangeSpeed);
            } else if (aggressionForColor > target) {
                aggressionForColor = Mathf.Max(target, aggressionForColor - dT * colorChangeSpeed);
            }

            render.materials[0].SetColor("_GlowColor", Color.Lerp(peacefulColor, aggressiveColor, aggressionForColor));
        }

        var time = DayNightCycle.main.timePassedAsFloat;

        if (render && time > lastTextureSwapTime + 1) {
            lastTextureSwapTime = time;
            RenderUtil.swapTextures(SeaToSeaMod.ModDLL, render, "Textures/Creature/DeepStalker", null);
        }

        if (time < targetCooldown) {
            playerHuntComponent.lastTarget.SetTarget(null);
            currentForcedTarget = null;
            collectorComponent.DropShinyTarget();
        } else {
            var has = currentlyHasPlatinum();
            if (has) {
                playerHuntComponent.lastTarget.SetTarget(null);
                currentForcedTarget = null;
                creatureComponent.Aggression.Add(-0.15F);
                collectorComponent.shinyTarget.EnsureComponent<ResourceTrackerUpdater>().tracker =
                    collectorComponent.shinyTarget.GetComponent<ResourceTracker>();
            }

            if (currentForcedTarget && currentForcedTarget == Player.main.gameObject &&
                Random.Range(0F, 1F) <= 0.12F) {
                if (has || time - lastPlayerBiteTime < 5 || Inventory.main.GetPickupCount(
                        CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).Info.TechType
                    ) == 0) {
                    //SNUtil.writeToChat("Dropped player target");
                    playerHuntComponent.lastTarget.SetTarget(null);
                    currentForcedTarget = null;
                }
            }

            if (currentForcedTarget && time - platinumGrabTime <= 12) {
                triggerPtAggro(currentForcedTarget, false);
            } else if (!has && !currentForcedTarget && time - lastAreaCheck >= 1 &&
                       !GetComponent<WaterParkCreature>()) {
                lastAreaCheck = time;
                if (!treaderTarget || !treaderTarget.gameObject.activeInHierarchy || Vector3.Distance(
                        treaderTarget.transform.position,
                        transform.position
                    ) >= 120)
                    bindToTreader(WorldUtil.getClosest<SeaTreader>(gameObject));
                List<GameObject> loosePlatinum = [];
                List<CollectShiny> stalkersWithPlatinum = [];
                WorldUtil.getGameObjectsNear(
                    transform.position,
                    40,
                    go => parseNearbyObject(go, loosePlatinum, stalkersWithPlatinum)
                );
                var flag = false;
                if (loosePlatinum.Count > 0) {
                    collectorComponent.shinyTarget = loosePlatinum[Random.Range(0, loosePlatinum.Count)];
                    flag = true;
                } else {
                    var ep = Player.main;
                    var dist = Vector3.Distance(ep.transform.position, transform.position);
                    if (dist <= 30) {
                        var amt = Inventory.main.GetPickupCount(
                            CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).Info.TechType
                        );
                        //SNUtil.writeToChat("Counting platinum = "+amt);
                        if (amt > 0 && Random.Range(0F, 1F) <= Mathf.Min(amt * 0.06F, 0.67F)) {
                            triggerPtAggro(ep.gameObject);
                            flag = true;
                        }
                    }
                }

                if (!flag && stalkersWithPlatinum.Count > 0) {
                    var c = stalkersWithPlatinum[Random.Range(0, stalkersWithPlatinum.Count)];
                    collectorComponent.shinyTarget = c.shinyTarget;
                    triggerPtAggro(c.gameObject);
                }
            }
        }

        if (!GetComponent<WaterParkCreature>()) {
            var distp = Vector3.Distance(transform.position, Player.main.transform.position);
            if (distp >= 200) {
                destroyCreature();
            } else if (treaderTarget) {
                var dist = Vector3.Distance(transform.position, treaderTarget.transform.position);
                if (dist >= 150 && distp >= 30) {
                    destroyCreature();
                } else if (dist >= 80) {
                    swimmer.SwimTo(treaderTarget.transform.position, 20);
                }
            }

            if (time >= lastDespawnCheck + 2.5F && distp > 40) {
                lastDespawnCheck = time;
                if (countDeepStalkersNear(transform) > 5)
                    destroyCreature();
            }
        }
    }

    internal static int countDeepStalkersNear(Transform t) {
        var amt = 0;
        WorldUtil.getGameObjectsNear(
            t.position,
            60,
            go => {
                var c = go.GetComponent<DeepStalkerTag>();
                if (c && c.isAlive() && !c.GetComponent<WaterParkCreature>())
                    amt++;
            }
        );
        return amt;
    }

    private void parseNearbyObject(
        GameObject go,
        List<GameObject> loosePlatinum,
        List<CollectShiny> stalkersWithPlatinum
    ) {
        var pt = go.GetComponent<PlatinumTag>();
        if (pt && pt.getTimeOnGround() >= 2.5F) {
            //collectorComponent.shinyTarget = go;
            loosePlatinum.Add(go);
        }

        var c = go.GetComponent<CollectShiny>();
        if (c && c.shinyTarget && c.targetPickedUp && c.shinyTarget.GetComponent<PlatinumTag>()) {
            //collectorComponent.shinyTarget = c.shinyTarget;
            //triggerPtAggro(go);
            //break;
            stalkersWithPlatinum.Add(c);
        }
    }

    private void destroyCreature() {
        collectorComponent.DropShinyTarget();
        gameObject.destroy();
    }

    public bool isAlive() {
        return creatureComponent && creatureComponent.liveMixin && creatureComponent.liveMixin.IsAlive();
    }

    public bool currentlyHasPlatinum() {
        return collectorComponent && collectorComponent.targetPickedUp && collectorComponent.shinyTarget &&
               collectorComponent.shinyTarget.GetComponent<PlatinumTag>();
    }

    internal void onHitWithElectricDefense() {
        currentForcedTarget = null;
        targetCooldown = DayNightCycle.main.timePassedAsFloat + 12;
        creatureComponent.Aggression.Add(-1F);
        collectorComponent.DropShinyTarget();
        collectorComponent.timeNextFindShiny = targetCooldown;
    }

    public void OnMeleeAttack(GameObject target) {
        //SNUtil.writeToChat(this+" attacked "+target);
        if (target == Player.main.gameObject) {
            lastPlayerBiteTime = DayNightCycle.main.timePassedAsFloat;
            var p = Inventory.main.container.RemoveItem(
                CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).Info.TechType
            );
            if (p) {
                var ch = Mathf.Clamp(SeaToSeaMod.ModConfig.getFloat(C2CConfig.ConfigEntries.PLATTHEFT), 0.25F, 1F);
                if (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE))
                    ch *= 3;
                if (Random.Range(0F, 1F) < ch) {
                    Inventory.main.InternalDropItem(p, false);
                    grab(p.gameObject);
                }
            }
        } else {
            var s = target.GetComponentInParent<Stalker>();
            if (s) {
                s.liveMixin.AddHealth(attackComponent.biteDamage);
                var c = s.GetComponent<CollectShiny>();
                var go = c.shinyTarget;
                if (go) {
                    c.DropShinyTarget();
                    grab(go);
                }
            }
        }
    }

    public void OnShinyPickedUp(GameObject target) {
        var pt = target.GetComponent<PlatinumTag>();
        if (pt)
            pt.pickup(this);
    }

    public void OnShinyDropped(GameObject target) {
        var pt = target.GetComponent<PlatinumTag>();
        if (pt)
            pt.drop();
    }

    private void grab(GameObject go) {
        collectorComponent.DropShinyTarget();
        collectorComponent.shinyTarget = go;
        collectorComponent.TryPickupShinyTarget();
    }

    internal void tryStealFrom(Stalker s) {
        triggerPtAggro(s.gameObject, true);
    }

    internal void bindToTreader(SeaTreader s) {
        treaderTarget = s;
        if (s)
            s.gameObject.GetComponent<C2CTreader>().attachStalker(this);
    }

    private void OnDestroy() {
        if (treaderTarget) {
            var c2c = treaderTarget.gameObject.GetComponent<C2CTreader>();
            if (c2c)
                c2c.removeStalker(this);
        }

        if (collectorComponent)
            collectorComponent.DropShinyTarget();
    }

    private void OnDisable() {
        OnDestroy();
    }

    private void OnKill() {
        OnDestroy();
    }

    internal void triggerPtAggro(GameObject target, bool isNew = true) {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time < targetCooldown)
            return;
        if (GetComponent<WaterParkCreature>())
            return;
        if (isNew) {
            platinumGrabTime = time;
            //SNUtil.writeToChat(this+" aimed at "+target);
        }

        //SNUtil.writeToChat(this+" is seeking "+target);
        currentForcedTarget = target;
        if (creatureComponent && creatureComponent.liveMixin && creatureComponent.liveMixin.IsAlive()) {
            creatureComponent.Aggression.Add(isNew ? 0.3F : 0.1F);
            if (playerHuntComponent) {
                playerHuntComponent.lastTarget.SetTarget(currentForcedTarget);
            }

            swimmer.SwimTo(currentForcedTarget.transform.position, 20);
        }
    }
}