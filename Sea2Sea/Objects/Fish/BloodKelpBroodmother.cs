using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class BloodKelpBroodmother : RetexturedFish {
    private readonly XMLLocale.LocaleEntry locale;

    [SetsRequiredMembers]
    internal BloodKelpBroodmother(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.CAVECRAWLER.prefab) {
        locale = e;

        ScanTime = 20;

        EggSpawnRate = 0F;
    }

    public override void prepareGameObject(GameObject world, Renderer[] r0) {
        var kc = world.EnsureComponent<BloodKelpBroodmotherTag>();

        world.GetComponent<LiveMixin>().data.maxHealth *= 20;

        world.GetComponent<MeleeAttack>().biteDamage = 35F;

        world.GetComponent<AggressiveWhenSeeTarget>().ignoreSameKind = true;

        var cc = world.GetComponent<CaveCrawler>();
        cc.jumpMaxHeight *= 0.06F;

        world.GetComponent<WorldForces>().underwaterGravity *= 3;

        foreach (var r in world.GetComponentsInChildren<Renderer>()) {
            RenderUtil.setEmissivity(r.materials[0], 2);
            r.materials[0].SetFloat("_SpecInt", 0.5F);
            r.materials[0].SetFloat("_Fresnel", 0.7F);
            r.materials[0].SetFloat("_Shininess", 4F);
            RenderUtil.setEmissivity(r.materials[1], 0); //eye gloss
            r.materials[1].SetFloat("_SpecInt", 0.85F);
        }

        var leash = world.GetComponent<StayAtLeashPosition>();

        world.removeComponent<CrawlerJumpRandom>();

        var soundRef = ObjectUtil.lookupPrefab(VanillaCreatures.BLOODCRAWLER.prefab);
        var sounds = soundRef.GetComponentsInChildren<FMOD_CustomEmitter>()
            .Where(fc => !(fc is FMOD_CustomLoopingEmitter)).Select(fc => fc.asset).ToList();
        var soundLoops = soundRef.GetComponentsInChildren<FMOD_CustomLoopingEmitter>()
            .Select(fc => fc.asset).ToList();
        foreach (var snd in world.GetComponentsInChildren<FMOD_CustomEmitter>()) {
            var li = snd is FMOD_CustomLoopingEmitter ? soundLoops : sounds;
            if (li.Count > 0) {
                snd.asset = soundLoops[0];
                snd.gameObject.EnsureComponent<SoundPitchScale>().pitch = 0.33F;
                soundLoops.RemoveAt(0);
            }
        }
    }

    public override BehaviourType GetBehavior() {
        return BehaviourType.Crab;
    }
}

internal class SoundPitchScale : MonoBehaviour {
    public float pitch = 1;

    private FMOD_CustomEmitter sound;

    private void Start() {
        sound = GetComponent<FMOD_CustomEmitter>();
    }

    private void Update() {
        if (sound) {
            var evt = sound.GetEventInstance();
            if (evt.isValid() && evt.hasHandle()) {
                evt.setPitch(pitch);
            }
        }
    }
}

internal class BloodKelpBroodmotherTag : MonoBehaviour, DIHooks.IStasisReactant {
    private static readonly SoundManager.SoundData spitSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "broodmotherspit",
        "Sounds/broodmotherspit.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 64); }
    );

    private static readonly SoundManager.SoundData idleSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "broodmotheridle",
        "Sounds/broodmotheridle.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 64); }
    );

    private Renderer[] renders;
    private CaveCrawler creature;

    private CrawlerAttackLastTarget attack;
    private StayAtLeashPosition leash;
    private Locomotion walk;
    private SwimBehaviour behavior;
    private LastTarget target;

    private float lastTickTime = -1;
    private float nextSpitTime = -1;

    private float nextSoundTime = -1;

    private void Start() {
        attack = GetComponent<CrawlerAttackLastTarget>();
        attack.jumpToTarget = false;
        attack.timeNextJump = 999999999;
        leash = GetComponent<StayAtLeashPosition>();
        leash.leashDistance = 8;
        walk = GetComponent<Locomotion>();
        behavior = GetComponent<SwimBehaviour>();
        target = GetComponent<LastTarget>();
    }

    private void Update() {
        if (renders == null)
            renders = GetComponentsInChildren<Renderer>();
        if (creature == null)
            creature = GetComponentInChildren<CaveCrawler>();
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time - lastTickTime >= 0.5F) {
            lastTickTime = time;
            transform.localScale = new Vector3(9, 6, 9);
            creature.timeLastJump = time - 0.1F;
            creature.leashPosition = C2CProgression.Instance.BkelpNestBumps.getClosest(transform.position);
        }

        if ((creature.leashPosition - transform.position).sqrMagnitude < 2500)
            target.SetTarget(Player.main.gameObject);
        if (!target.target || (creature.leashPosition - transform.position).sqrMagnitude > 100)
            behavior.SwimTo(creature.leashPosition, 5);
        if (attack && target.target && time >= nextSpitTime) {
            shoot(target.target);
        }

        if (time >= nextSoundTime) {
            SoundManager.playSoundAt(idleSound, transform.position, false, 40, 1);
            nextSoundTime = time + Random.Range(3F, 10F);
        }
    }

    public void OnStasisHit(StasisSphere s) {
        nextSpitTime = DayNightCycle.main.timePassedAsFloat + 0.5F;
    }

    public void shoot(GameObject target) {
        var shot = ObjectUtil.createWorldObject(SeaToSeaMod.AcidSpit.Info.ClassID);
        shot.transform.position = transform.position + transform.forward * 2.4F + transform.up * 1.25F;
        shot.GetComponent<AcidSpitTag>().spawnPosition = shot.transform.position;
        shot.ignoreCollisions(gameObject);
        var diff = target.transform.position - Vector3.up * 0.5F - shot.transform.position;
        shot.GetComponent<Rigidbody>().velocity = 18 * diff.normalized;
        nextSpitTime = DayNightCycle.main.timePassedAsFloat + Random.Range(0.5F, 2F);
        SoundManager.playSoundAt(spitSound, transform.position, false, 40, 1);
    }
}