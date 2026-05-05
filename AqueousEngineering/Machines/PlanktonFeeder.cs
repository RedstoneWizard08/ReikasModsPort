using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class PlanktonFeeder : CustomMachine<PlanktonFeederLogic>, MultiTexturePrefab {
    internal static readonly float POWER_COST = 0.125F; //per second
    internal static readonly float CONSUMPTION_RATE = 1F / 240F; //per second
    internal static readonly float RANGE = 250F; //m

    internal static readonly Dictionary<TechType, WildFeedingBehavior> behaviors = new();

    internal static WorldCollectedItem fuel;

    static PlanktonFeeder() {
        behaviors[TechType.Peeper] = new WildFeedingBehavior(150, 60, 6, 0.15F, 10);
        behaviors[TechType.Bladderfish] = new WildFeedingBehavior(100, 60, 4, 0.05F, 10);
        behaviors[TechType.Reginald] = new WildFeedingBehavior(150, 40, 4, 0.1F, 10);
        behaviors[TechType.Hoopfish] = new WildFeedingBehavior(150, 40, 6, 0.15F, 10);
        behaviors[TechType.Spinefish] = new WildFeedingBehavior(180, 40, 9, 0.15F, 10);
        behaviors[TechType.Shuttlebug] = new WildFeedingBehavior(30, 20, 2, 0.05F, 5);
        behaviors[TechType.Eyeye] = new WildFeedingBehavior(200, 50, 2, 0.2F, 10);
        behaviors[TechType.LavaEyeye] = new WildFeedingBehavior(150, 30, 2, 0.1F, 10);
        behaviors[TechType.GhostLeviathan] = new WildFeedingBehavior(300, 0, 20, 0F, 40);
    }

    [SetsRequiredMembers]
    public PlanktonFeeder(XMLLocale.LocaleEntry e) : base(
        e.key,
        e.name,
        e.desc,
        "bedc40fb-bd97-4b4d-a943-d39360c9c7bd"
    ) {
        addIngredient(TechType.FiberMesh, 1);
        addIngredient(TechType.Pipe, 5);
        addIngredient(TechType.Titanium, 3);

        glowIntensity = 1;
    }

    public override bool UnlockedAtStart => false;

    public override bool isOutdoors() {
        return true;
    }

    public Dictionary<int, string> getTextureLayers(Renderer r) {
        return new Dictionary<int, string> { { 0, "" }, { 1, "" } };
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeComponent<Trashcan>();

        var lgc = go.GetComponent<PlanktonFeederLogic>();

        var con = go.GetComponentInChildren<StorageContainer>();
        initializeStorageContainer(con, 6, 4);

        var mdl = go.setModel(
            "discovery_trashcan_01_d",
            ObjectUtil.lookupPrefab("8fb8a082-d40a-4473-99ec-1ded36cc6813").getChildObject("Starship_cargo")
        );
        mdl.transform.localRotation = Quaternion.Euler(-90, 180, 0);
        mdl.transform.localPosition = new Vector3(0, -0.05F, 0);
        mdl.transform.localScale = new Vector3(1.5F, 0.5F, 0.5F);
        var c = go.GetComponent<Constructable>();
        c.model = mdl;
        c.allowedOnCeiling = true;
        c.allowedOnGround = true;
        c.allowedOnWall = true;
        c.allowedOnConstructables = true;

        var r = mdl.GetComponentsInChildren<Renderer>();
        RenderUtil.swapToModdedTextures(r, this);
        RenderUtil.enableAlpha(r[0].materials[0]);
        r[0].materials[0].SetColor("_GlowColor", Color.white);
        //r.materials[1].color = Color.clear;

        var box = go.GetComponentInChildren<BoxCollider>();
        box.size = mdl.transform.localScale;
        box.center = Vector3.zero;

        var name = "BubbleRoot";
        var child = go.getChildObject(name);
        if (child == null) {
            child = new GameObject(name);
            child.transform.SetParent(go.transform);
            child.transform.localRotation = Quaternion.Euler(0, -15, 0);
            child.transform.localPosition = new Vector3(0, 0, 0.25F);
        }

        var pi = child.GetComponentsInChildren<PrefabIdentifier>();
        for (var i = pi.Length; i < 3; i++) {
            var bubbles = ObjectUtil.createWorldObject("0dbd3431-62cc-4dd2-82d5-7d60c71a9edf");
            bubbles.transform.SetParent(child.transform);
            bubbles.transform.localPosition = new Vector3((i - 1) * 0.5F, 0, 0.25F);
            bubbles.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }
    }
}

public class PlanktonFeederLogic : CustomMachineLogic {
    private ParticleSystem[] bubbles;

    private bool active;

    private void Start() {
        SNUtil.log("Reinitializing base plankton feeder");
        AqueousEngineeringMod.planktonFeederBlock.initializeMachine(gameObject);
    }

    protected override float getTickRate() {
        return 1F;
    }

    protected override void updateEntity(float seconds) {
        if (bubbles == null || bubbles.Length == 0) {
            bubbles = GetComponentsInChildren<ParticleSystem>();
            foreach (var p in bubbles) {
                var main = p.main;
                main.startColor = new Color(0.2F, 1F, 0.4F);
                main.gravityModifierMultiplier = -0.05F;
                var force = p.forceOverLifetime;
                force.enabled = true;
                force.xMultiplier = -0.25F;
                force.yMultiplier = -0.5F;
                force.zMultiplier = 0;
            }

            setState(false);
        }
        //if (mainRenderer == null)
        //	mainRenderer = gameObject.getChildObject("model").GetComponent<Renderer>();

        //SNUtil.writeToChat("I am ticking @ "+go.transform.position);
        if (Vector3.Distance(Player.main.transform.position, transform.position) >= PlanktonFeeder.RANGE)
            return;
        if (consumePower(PlanktonFeeder.POWER_COST * seconds) &&
            storage.container.GetCount(PlanktonFeeder.fuel.TechType) > 0) {
            setState(true);
            var r = PlanktonFeeder.RANGE;
            var any = false;
            WorldUtil.getGameObjectsNear(gameObject.transform.position, r, go => any |= tryBreedCycle(go, seconds));
            if (any && Random.Range(0F, 1F) <= PlanktonFeeder.CONSUMPTION_RATE * seconds) {
                storage.container.DestroyItem(PlanktonFeeder.fuel.TechType);
            }
        } else {
            setState(false);
        }
    }

    private bool tryBreedCycle(GameObject go, float seconds) {
        var ttag = go.GetComponent<TechTag>();
        if (!ttag)
            return false;
        var tt = ttag.type;
        var feed = PlanktonFeeder.behaviors.ContainsKey(tt) ? PlanktonFeeder.behaviors[tt] : null;
        if (feed != null) {
            var c = go.GetComponent<Creature>();
            var dd = Vector3.Distance(go.transform.position, transform.position);
            if (dd >= feed.maxAttractRange)
                return false;
            go.GetComponent<SwimBehaviour>().SwimTo(transform.position, feed.attractionSpeed);
            c.leashPosition = transform.position;
            var leash = go.GetComponent<StayAtLeashPosition>();
            if (leash) {
                leash.leashDistance = feed.minRange;
                leash.swimVelocity = feed.attractionSpeed;
            }

            if (c is GhostLeviathan) {
                c.Aggression.Add(-0.005F * seconds);
            }

            c.Hunger.Add(-0.04F * seconds);
            if (dd <= feed.maxBreedRange && Random.Range(0F, 1F) <= feed.breedChance * seconds * 0.005F) {
                tryBreed(c);
            }

            return true;
        }

        return false;
    }

    private void setState(bool enable) {
        if (enable == active && getAge() >= 1)
            return;
        active = enable;
        foreach (var p in bubbles) {
            if (enable)
                p.Play();
            else
                p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void tryBreed(Creature c) {
        var clone = ObjectUtil.createWorldObject(c.GetComponent<PrefabIdentifier>().ClassId);
        clone.transform.position = MathUtil.getRandomVectorAround(c.transform.position, 2);
    }
}

internal class WildFeedingBehavior {
    public readonly float maxAttractRange;
    public readonly float maxBreedRange;
    public readonly float attractionSpeed;
    public readonly float breedChance;
    public readonly float minRange;

    internal WildFeedingBehavior(float r, float br, float s, float c, float m) {
        maxAttractRange = r;
        maxBreedRange = br;
        attractionSpeed = s;
        breedChance = c;
        minRange = m;
    }
}