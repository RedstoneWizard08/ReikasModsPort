using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class FallingGlassForestWreck : CustomPrefab {
    public static readonly string STORY_TAG = "GlassWreckFall";

    [SetsRequiredMembers]
    internal FallingGlassForestWreck() : base("fallingwreck", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var world = ObjectUtil.createWorldObject("1618a787-67b7-4e35-9869-3ec558ed2835");
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;

        cleanup(world);

        world.EnsureComponent<FallingGFWreckTag>();

        var rb = world.EnsureComponent<Rigidbody>();
        rb.mass = 2000;
        rb.drag = 0;
        rb.isKinematic = true;
        var wf = world.EnsureComponent<WorldForces>();
        wf.underwaterGravity = 0.25F;
        wf.underwaterDrag *= 0.33F;

        world.fullyEnable();
        return world;
    }

    internal static void cleanup(GameObject world) {
        tryRemoveObjects(world, "Slots");
        tryRemoveObjects(world, "Starship_exploded_debris_*");
        tryRemoveObjects(world, "Starship_cargo_*");
        tryRemoveObjects(world, "*starship_work*");
        tryRemoveObjects(world, "*DataBox*");
        tryRemoveObjects(world, "*Spawner*");
        tryRemoveObjects(world, "*PDA*");
        tryRemoveObjects(world, "*Wrecks_LaserCutFX*");
        //tryRemoveObjects(world, "InteriorEntities");
        //tryRemoveObjects(world, "InteriorProps");
        var exterior = world.getChildObject("ExteriorEntities");
        tryRemoveObjects(exterior, "ExplorableWreckHull01");
        tryRemoveObjects(exterior, "ExplorableWreckHull02");

        world.removeComponent<StarshipDoor>();
        world.removeComponent<StarshipDoorLocked>();
        world.removeComponent<Sealed>();
        world.removeComponent<LaserCutObject>();
    }

    private static void tryRemoveObjects(GameObject go, string name) {
        if (go.removeChildObject(name) <= 0) {
            SNUtil.log("Failed to find any '" + name + "' objects to remove from " + go.GetFullHierarchyPath() + "!");
            ObjectUtil.dumpObjectData(go, false);
        }
    }
}

internal class FallingGFWreckTag : MonoBehaviour {
    private static readonly SoundManager.SoundData[] groanSounds = [
        SoundManager.registerSound(
            SeaToSeaMod.ModDLL,
            "wreckgroan1",
            "Sounds/wreckgroan1.ogg",
            SoundManager.soundMode3D,
            s => { SoundManager.setup3D(s, 180); }
        ),
        SoundManager.registerSound(
            SeaToSeaMod.ModDLL,
            "wreckgroan2",
            "Sounds/wreckgroan2.ogg",
            SoundManager.soundMode3D,
            s => { SoundManager.setup3D(s, 180); }
        ),
        SoundManager.registerSound(
            SeaToSeaMod.ModDLL,
            "wreckgroan3",
            "Sounds/wreckgroan3.ogg",
            SoundManager.soundMode3D,
            s => { SoundManager.setup3D(s, 180); }
        ),
    ];

    private static readonly SoundManager.SoundData[] hitSounds = [
        SoundManager.registerSound(
            SeaToSeaMod.ModDLL,
            "wreckhit1",
            "Sounds/wreckhit1.ogg",
            SoundManager.soundMode3D,
            s => { SoundManager.setup3D(s, 180); }
        ),
        SoundManager.registerSound(
            SeaToSeaMod.ModDLL,
            "wreckhit2",
            "Sounds/wreckhit2.ogg",
            SoundManager.soundMode3D,
            s => { SoundManager.setup3D(s, 180); }
        ),
    ];

    private static readonly SoundManager.SoundData impactSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "wreckland",
        "Sounds/wreckland2.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 200); }
    );

    private static readonly Vector3 root = new(-135.45F, -194.82F, 849.19F);

    private PrefabIdentifier prefab;

    private Rigidbody mainBody;

    //private Collider[] colliders;
    private List<MeshRenderer> doors = [];

    private bool isFalling;
    private float fallTime;

    private BoxCollider box;

    private Vector3 lastPos;

    private float nextShakeableTime;

    private void Start() {
        //FallingGlassForestWreck.cleanup(gameObject);
        gameObject.fullyEnable();
    }

    private void Update() {
        if (!mainBody)
            mainBody = GetComponentInChildren<Rigidbody>();
        if (!prefab)
            prefab = GetComponentInChildren<PrefabIdentifier>();

        //transform.localScale = Vector3.one*0.8F;

        var time = DayNightCycle.main.timePassedAsFloat;
        var dT = Time.deltaTime;
        if (dT <= 0)
            return;
        var ep = Player.main;
        if (isFalling) {
            if ((ep && Vector3.Distance(ep.transform.position, transform.position) >= 300) ||
                transform.position.y <= -475) {
                doImpactFX();
                gameObject.destroy();
            } else {
                fallTime += dT;
                foreach (var mr in doors) {
                    if (mr && mr.gameObject)
                        mr.gameObject.transform.localPosition = Vector3.zero;
                }

                var diff = (transform.position - root).normalized;
                if (fallTime < 1.5F) {
                    transform.RotateAround(root, new Vector3(0, 0, 1), 25 * dT * fallTime);
                    mainBody.MovePosition(transform.position + diff * dT * 10 * fallTime);
                    if (Random.Range(0F, 1F) < 0.067F)
                        doUnstableFX(true, true);
                } else {
                    mainBody.isKinematic = false;
                    box.enabled = (transform.position - lastPos).magnitude > 0.01F ||
                                  mainBody.angularVelocity.magnitude > 5;
                    mainBody.velocity = Vector3.down * 15 + diff * 10F;
                }
            }
        } else {
            if (ep && Vector3.Distance(ep.transform.position, transform.position) <= 30) {
                //LOS
                if (gameObject.isVisible()) {
                    startFall();
                }
            }

            if (Random.Range(0F, 1F) < 0.0075F)
                doUnstableFX(
                    Random.Range(0F, 1F) < 0.5F &&
                    DayNightCycle.main.timePassedAsFloat >= nextShakeableTime,
                    false
                );
        }

        lastPos = transform.position;
    }

    private void doUnstableFX(bool sound, bool groan) {
        var boxes = GetComponentsInChildren<Collider>(true);
        for (var i = 0; i < 14; i++) {
            var c = boxes[Random.Range(0, boxes.Length)];
            if (!c) {
                continue;
            }

            var pos = MathUtil.getRandomVectorBetween(c.bounds.min, c.bounds.max);
            if (Random.Range(0F, 1F) <= 0.75) {
                var fx = WorldUtil.spawnParticlesAt(pos, "ee56cc29-1da3-41d7-8cf3-d8f028cb9559", 5);
                if (!fx)
                    continue;
                var mod = fx.main;
                mod.duration *= 1.5F;
                mod.startSizeMultiplier *= 4.5F;
                mod.startSize = 6;
                mod.startColor = new Color(0.3F, 0.4F, 0.7F);
                var emit = fx.emission;
                emit.rateOverTimeMultiplier *= 16;
                //ParticleSystem.ShapeModule shape = fx.shape;
                //if (shape != null)
                //	shape.radius *= 9;
                //fx.main = mod;
            } else {
                var fx = getSmokeFX();
                if (!fx)
                    continue;
                fx.transform.position = pos;
                WorldUtil.setParticlesTemporary(fx, Random.Range(0.33F, 0.75F));
                var mod = fx.main;
                mod.duration *= 5F;
                mod.startLifetime = 2;
                mod.startSizeMultiplier = 15F;
                mod.startSize = 8;
                mod.gravityModifier = 0.3F;
                var clr = fx.colorOverLifetime;
                var color = clr.color;
                color.mode = ParticleSystemGradientMode.TwoColors;
                color.colorMin = new Color(0.2F, 0.2F, 0.15F, 0.7F);
                color.colorMax = new Color(0.2F, 0.2F, 0.15F, 0);
                var vel = fx.velocityOverLifetime;
                vel.zMultiplier = -3;
                //mod.startColor = new Color(0.2F, 0.2F, 0.15F);
                //ParticleSystem.ColorOverLifetimeModule clr = fx.colorOverLifetime;
                //clr.enabled = false;
            }
        }

        if (sound) {
            var snd = groan
                ? groanSounds[Random.Range(0, groanSounds.Length)]
                : hitSounds[Random.Range(0, hitSounds.Length)];
            SoundManager.playSoundAt(
                snd,
                transform.position,
                false,
                120,
                groan ? 2 : Random.Range(1F, 1.5F)
            );
            var intensity = 1 - Vector3.Distance(Player.main.transform.position, transform.position) / 100F;
            if (intensity > 0) {
                intensity = Mathf.Pow(intensity, 1.6F);
                if (groan)
                    intensity *= 0.67F;
                var dur = Random.Range(2F, 4F);
                SNUtil.shakeCamera(
                    dur,
                    intensity * Random.Range(1F, 2F),
                    Random.Range(2.5F, 4F)
                );
                nextShakeableTime = DayNightCycle.main.timePassedAsFloat + Random.Range(0.5F, 1.5F);
            }
        }
    }

    private void doImpactFX() {
        var fx = getSmokeFX();
        fx.transform.position = Player.main.transform.position + Vector3.down * 125; //transform.position.setY(-320);
        WorldUtil.setParticlesTemporary(fx, 2.5F, 60);
        var mod = fx.main;
        mod.duration *= 10F;
        mod.startLifetimeMultiplier *= 20; //10F;
        mod.startSizeMultiplier = 250; //120;//75F;
        mod.startSize = 30; //8;
        mod.startColor = new Color(1F, 0.6F, 0.4F);
        mod.gravityModifier = 0; //0.05F;
        //ParticleSystem.EmissionModule emit = fx.emission;
        //emit.rateOverTimeMultiplier *= 16;
        var shape = fx.shape;
        shape.radius = 8; //4;//12;
        //fx.main = mod;
        var clr = fx.colorOverLifetime;
        //clr.enabled = false;
        var color = clr.color;
        color.mode = ParticleSystemGradientMode.TwoColors;
        color.colorMin = new Color(1F, 0.6F, 0.4F);
        color.colorMax = new Color(1F, 0.6F, 0.4F, 0);
        var vel = fx.velocityOverLifetime;
        vel.zMultiplier = 30; //15;
        SoundManager.playSoundAt(impactSound, Player.main.transform.position, false, -1, 2);
        SoundManager.playSoundAt(impactSound, Player.main.transform.position, false, -1, 2);
        //foreach (SoundManager.SoundData snd in hitSounds)
        //	SoundManager.playSoundAt(snd, Player.main.transform.position, false, 120, 2);
    }

    private ParticleSystem getSmokeFX() {
        var go = ObjectUtil.createWorldObject("5ce9eb7b-064b-46e6-ae7b-43fc4bd016c3");
        go.GetComponent<ParticleSystem>().destroy();
        var child = go.getChildObject("xSmk");
        foreach (Transform t in go.transform) {
            if (t != child.transform)
                t.gameObject.destroy();
        }

        return child.GetComponent<ParticleSystem>();
    }

    private void startFall() {
        foreach (var c in GetComponentsInChildren<Collider>())
            c.gameObject.destroy(false);
        box = gameObject.EnsureComponent<BoxCollider>();
        box.center = new Vector3(5, 7F, -7.5F);
        box.size = new Vector3(20, 15, 35);
        box.isTrigger = false;
        box.enabled = false;

        Story.StoryGoal.Execute(FallingGlassForestWreck.STORY_TAG, Story.GoalType.Story);

        isFalling = true;
        //mainBody.isKinematic = true;
        foreach (var d in GetComponentsInChildren<StarshipDoor>()) {
            var pos = d.transform.position;
            var rot = d.transform.rotation;
            d.transform.SetParent(transform);
            d.transform.position = pos;
            d.transform.rotation = rot;
            doors.AddRange(d.GetComponentsInChildren<MeshRenderer>());
        }

        gameObject.removeComponent<Animator>();
        gameObject.removeComponent<StarshipDoor>();
        gameObject.removeComponent<StarshipDoorLocked>();
        gameObject.removeComponent<Sealed>();
        gameObject.removeComponent<LaserCutObject>();
        gameObject.removeComponent<TrailRenderer>();
        gameObject.removeChildObject("*Wrecks_LaserCutFX*");

        doUnstableFX(true, true);
    }

    private void OnDestroy() {
    }

    private void OnDisable() {
        //if (transform.position.y <= -400)
        //	gameObject.destroy(false);
    }
}