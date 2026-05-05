using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ECCLibrary;
using ECCLibrary.Data;
using ECCLibrary.Mono;
using Nautilus.Utility;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class VoidThalassacean : CreatureAsset {
    internal VoidThalassacean(XMLLocale.LocaleEntry e)
        : base(Nautilus.Assets.PrefabInfo.WithTechType(e.key, e.name, e.desc)) {
        CreatureDataUtils.AddCreaturePDAEncyclopediaEntry(
            this,
            "Lifeforms/Fauna/Carnivores",
            e.name,
            e.desc,
            12,
            null,
            null
        );
    }

    protected override CreatureTemplate CreateTemplate() {
        var liveMixinData = ScriptableObject.CreateInstance<LiveMixinData>();

        liveMixinData.maxHealth = 1800f;
        liveMixinData.knifeable = true;

        var template = new CreatureTemplate(
            DeIntegrationSystem.Instance.GetThalassacean(),
            BehaviourType.Whale,
            EcoTargetType.Whale,
            160f
        ) {
            CellLevel = LargeWorldEntity.CellLevel.Global,
            SwimRandomData = new SwimRandomData(0.5f, 1.2f, new Vector3(30f, 0f, 30f), 5f),
            Mass = 250f,
            LocomotionData = new LocomotionData(forwardRotationSpeed: 0.15f),
            LiveMixinData = liveMixinData,
            SizeDistribution = new AnimationCurve(
                new Keyframe(0f, 0.8f),
                new Keyframe(1f, 1f)
            ),
        };

        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents cc) {
        if (cc.Locomotion) cc.Locomotion.maxVelocity = 6f;

        var trailManagerBuilder = new TrailManagerBuilder(cc, prefab.transform.SearchChild("root")) {
            Trails = [
                prefab.transform.SearchChild("spine1"),
                prefab.transform.SearchChild("spine2"),
                prefab.transform.SearchChild("spine3"),
                prefab.transform.SearchChild("spine4"),
            ],
            SegmentSnapSpeed = 0.2f,
            MaxSegmentOffset = -1f,
            AllowDisableOnScreen = false,
        };

        trailManagerBuilder.Apply();

        var voiceEmitter = prefab.AddComponent<FMOD_CustomEmitter>();

        voiceEmitter.followParent = true;

        var voice = prefab.AddComponent<CreatureVoice>();

        voice.emitter = voiceEmitter;
        voice.closeIdleSound = AudioUtils.GetFmodAsset("ThalassaceanRoar");
        voice.minInterval = 30;
        voice.maxInterval = 70;

        var r = prefab.GetComponentInChildren<Renderer>();
        RenderUtil.swapTextures(SeaToSeaMod.ModDLL, r, "Textures/Creature/VoidThalassacean");
        prefab.EnsureComponent<VoidThalassaceanTag>();

        yield break;
    }
}

internal class VoidThalassaceanTag : MonoBehaviour, IOnTakeDamage {
    private const float AggressionTime = 2.5F;

    private static readonly Color CalmColor = new(0.2F, 0.5F, 1F);
    private static readonly Color WarnColor = new(1F, 0.75F, 0.15F);
    private static readonly Color AttackingColor = new(1F, 0.1F, 0.05F);

    private static readonly SoundManager.SoundData AggroStartSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "voidthalaroar2",
        "Sounds/voidthalaroar2.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 128); }
    );

    private static readonly SoundManager.SoundData AttackStartSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "voidthalachirp",
        "Sounds/voidthalachirp.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 128); }
    );

    private static readonly SoundManager.SoundData AttackHitSound = SoundManager.registerSound(
        SeaToSeaMod.ModDLL,
        "voidthalahit",
        "Sounds/voidthalahit.ogg",
        SoundManager.soundMode3D,
        s => { SoundManager.setup3D(s, 128); }
    );

    private Renderer _renderer;

    //private SwimRandom swimTarget;
    //private AggressiveWhenSeeTarget aggression;
    //private AggressiveToPilotingVehicle aggression2;
    private AttackLastTarget _attack;
    internal Rigidbody Body;
    private SwimBehaviour _behavior;

    private readonly List<VoidThalaHitDetection> _triggers = [];
    private static readonly List<int> AggroTokens = [1, 2, 3];

    private static readonly int Shininess = Shader.PropertyToID("_Shininess");

    private static readonly int SpecInt = Shader.PropertyToID("_SpecInt");

    private static readonly int Fresnel = Shader.PropertyToID("_Fresnel");

    private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");
    //prevent more than three tag teaming

    private float _aggressionLevel;
    private float _aggressionColorFade;

    private float _timeAggressive;
    private float _timeFleeing;

    private float _flashCycleVar;

    private Vector3 _runAwayTarget;

    private int _currentAggroToken;

    private float _returnAttackLifetime;

    private void Start() {
        float r = 100;
        _renderer = GetComponentInChildren<Renderer>();
        _renderer.materials[0].SetFloat(Shininess, 2.5F);
        _renderer.materials[0].SetFloat(SpecInt, 5.0F);
        _renderer.materials[0].SetFloat(Fresnel, 0.75F);
        Body = GetComponent<Rigidbody>();
        _behavior = GetComponent<SwimBehaviour>();
        _behavior.turnSpeed *= 1.5F;
        //swimTarget = GetComponent<SwimRandom>();
        /*
            aggression = gameObject.EnsureComponent<AggressiveWhenSeeTarget>();
            aggression.aggressionPerSecond = 1;
            aggression.creature = GetComponent<Creature>();
            aggression.ignoreSameKind = true;
            aggression.myTechType = instance.voidThelassacean.TechType;
            aggression.targetType = EcoTargetType.Shark;
            aggression.maxRangeScalar = r;
            aggression.isTargetValidFilter = (eco => eco.GetGameObject(.isPlayer()));
            aggression.lastTarget = gameObject.EnsureComponent<LastTarget>();
            aggression2 = gameObject.EnsureComponent<AggressiveToPilotingVehicle>();
            aggression2.aggressionPerSecond = aggression.aggressionPerSecond;
            aggression2.creature = aggression.creature;
            aggression2.lastTarget = aggression.lastTarget;
            aggression2.range = r;
            aggression2.updateAggressionInterval = 0.5F;
            */
        _attack = gameObject.EnsureComponent<AttackLastTarget>();
        _attack.aggressionThreshold = 0.5F;
        _attack.creature = GetComponent<Creature>(); //aggression.creature;
        _attack.lastTarget = gameObject.EnsureComponent<LastTarget>(); //aggression.lastTarget;
        _attack.maxAttackDuration = 60;
        _attack.minAttackDuration = 30;
        _attack.swimVelocity = 45;
        _attack.creature.ScanCreatureActions();

        GetComponent<LiveMixin>().damageReceivers = GetComponents<IOnTakeDamage>();

        Invoke(nameof(DelayedStart), 0.5F);
    }

    private void DelayedStart() {
        foreach (var c in GetComponentsInChildren<Collider>(true)) {
            if (!c.isTrigger) {
                c.gameObject.EnsureComponent<VoidThalaHitDetection>().Owner = this;
            }
        }
    }

    private GameObject GetTarget(out bool vehicle) {
        if (GameModeUtils.IsInvisible()) {
            vehicle = false;
            return null;
        }

        var v = Player.main.GetVehicle();
        vehicle = (bool)v;
        return v ? v.gameObject : Player.main.gameObject;
    }

    private void Update() {
        var far = Player.main ? (Player.main.transform.position - transform.position).sqrMagnitude : 999999;
        //SNUtil.writeToChat("D="+((int)(Mathf.Sqrt(distSq)))/10*10);
        if (far > 90000) { //more than 300m
            gameObject.destroy(false);
            return;
        }

        transform.localScale = Vector3.one * 1.5F;
        var go = GetTarget(out var vehicle);
        var flag = false;
        var c = CalmColor;

        var dT = Time.deltaTime;

        if (go) {
            var distSq = (go.transform.position - transform.position).sqrMagnitude;
            //SNUtil.writeToChat("D="+((int)(Mathf.Sqrt(distSq)))/10*10);
            if (distSq < (vehicle ? 2500 : 400) || _aggressionLevel > 0.9F ||
                (_returnAttackLifetime > 0 && distSq < 25600)) {
                //within 50m in vehicle or 20 on foot, or a queued attack
                flag = true;
            } else if (_aggressionLevel < 0 &&
                       (distSq > 2500 || (transform.position - _runAwayTarget).sqrMagnitude < 900)) {
                //more than 50m away while running, or at position
                _aggressionLevel = 0;
                //SNUtil.writeToChat("Zeroing flee");
            }
        } else {
            _aggressionLevel = 0;
        }

        if (Math.Abs(_aggressionLevel) < 0.01F && Body.velocity.magnitude >= 5) {
            Body.velocity *= 0.995F;
            //SNUtil.writeToChat("Braking");
        }

        if (_timeFleeing > 15)
            _aggressionLevel = 0;

        if (_returnAttackLifetime > 0)
            _returnAttackLifetime -= dT;

        if (flag) {
            if (_aggressionLevel >= 0) {
                var wasAny = _aggressionLevel > 0;
                var was = _aggressionLevel >= 1;
                _aggressionLevel = Mathf.Clamp01(_aggressionLevel + Time.deltaTime / AggressionTime);
                if (_aggressionLevel >= 1 && !was) {
                    SoundManager.playSoundAt(AttackStartSound, transform.position, false, 128, 2);
                } else if (_aggressionLevel > 0 && !wasAny) {
                    SoundManager.playSoundAt(AggroStartSound, transform.position, false, 128, 2);
                }
            }
        } else {
            _aggressionLevel = 0;
            //SNUtil.writeToChat("No target, calming");
        }

        var flag2 = false;
        if (_aggressionLevel < 0) {
            flag2 = true;
            _behavior.SwimTo(
                _runAwayTarget,
                (_runAwayTarget - transform.position).normalized,
                _attack.swimVelocity * 0.67F
            );
            _timeFleeing += dT;
        } else if (_aggressionLevel >= 1 && TryAllocateAggroToken()) {
            _attack.lastTarget.target = go;
            _attack.currentTarget = go;
            _behavior.Attack(
                go.transform.position,
                (go.transform.position - transform.position).normalized,
                _attack.swimVelocity
            );
            _aggressionColorFade = Mathf.Clamp01(_aggressionColorFade + dT * 2);
            _timeAggressive += dT;
            //SNUtil.writeToChat("Attacking!");
            _timeFleeing = 0;
        } else {
            flag2 = true;
            _timeFleeing = 0;
        }

        if (flag2) {
            _aggressionColorFade = Mathf.Clamp01(_aggressionColorFade - dT * 0.5F);
            ClearAggro();
        }

        if (_timeAggressive > 30)
            ResetAggro(true);

        c = _aggressionColorFade > 0
            ? Color.Lerp(WarnColor, AttackingColor, _aggressionColorFade)
            : Color.Lerp(CalmColor, WarnColor, _aggressionLevel);

        var f = Body.velocity.magnitude / _attack.swimVelocity;
        if (!flag2) //fast while fading from yellow to red
            f = 1.2F;
        _flashCycleVar += dT * Mathf.Deg2Rad * 6000 * f; //faster flashing the faster it goes
        var glow = 3.5F + 2.5F * Mathf.Sin(_flashCycleVar);
        if (_aggressionLevel < 0)
            glow *= 1 + _aggressionLevel;

        _renderer.materials[0].SetColor(GlowColor, c);
        RenderUtil.setEmissivity(_renderer, glow);
    }

    private bool TryAllocateAggroToken() {
        if (_currentAggroToken > 0)
            return true;
        if (AggroTokens.Count == 0)
            return false;
        _currentAggroToken = AggroTokens[0];
        AggroTokens.RemoveAt(0);
        return true;
    }

    private void ClearAggro() {
        _timeAggressive = 0;
        _attack.lastTarget.target = null;
        _attack.currentTarget = null;
        if (_currentAggroToken != 0) {
            if (AggroTokens.Contains(_currentAggroToken))
                SNUtil.writeToChat("Two voidthala with same aggro token: " + _currentAggroToken);
            AggroTokens.Add(_currentAggroToken);
            _currentAggroToken = 0;
        }
        //SNUtil.writeToChat("Clearing aggression values");
    }

    public void ResetAggro(bool deflect) {
        _aggressionLevel = -1;
        var offset = Body.velocity.setLength(120);
        offset = MathUtil.getRandomVectorAround(offset, deflect ? 90 : 50).setLength(120);
        _runAwayTarget =
            Player.main.transform.position + offset; //MathUtil.getRandomPointAtSetDistance(transform.position, 100);
        if (_runAwayTarget.y > -20)
            _runAwayTarget = _runAwayTarget.setY(-20);
        //SNUtil.writeToChat("Resetting aggro");
        Invoke(nameof(PlayFleeSound), 0.8F);
    }

    private void PlayFleeSound() {
        CancelInvoke(nameof(PlayFleeSound));
        SoundManager.playSoundAt(AttackHitSound, transform.position, false, 128);
    }

    public void OnTakeDamage(DamageInfo info) {
        if (info.type is DamageType.Electrical or DamageType.Normal)
            ResetAggro(true);
    }

    public void ReturnAttack() {
        _returnAttackLifetime = 30;
    }
}

internal class VoidThalaHitDetection : MonoBehaviour {
    internal VoidThalassaceanTag Owner;

    private void Start() {
        if (!Owner)
            Owner = gameObject.FindAncestor<VoidThalassaceanTag>();
    }

    private void OnCollisionEnter(Collision c) {
        if (!Owner)
            return;
        if (c.gameObject.FindAncestor<VoidThalaImpactImmunity>())
            return;
        if (c.collider.isPlayer() || c.collider.gameObject.FindAncestor<Vehicle>()) {
            Owner.ResetAggro(false);
            if (UnityEngine.Random.Range(0F, 1F) < 0.67F) {
                Owner.Invoke(nameof(VoidThalassaceanTag.ReturnAttack), UnityEngine.Random.Range(10F, 20F));
            }

            var v = c.collider.gameObject.FindAncestor<Vehicle>();
            if (v && v.liveMixin) {
                v.liveMixin.TakeDamage(2, c.contacts[0].point, DamageType.Normal, Owner.gameObject);
            }

            c.rigidbody.AddForce(Owner.Body.velocity.setLength(15), ForceMode.VelocityChange);
            c.gameObject.EnsureComponent<VoidThalaImpactImmunity>().elapseWhen =
                DayNightCycle.main.timePassedAsFloat + 0.5F;
        }
    }
}

internal class VoidThalaImpactImmunity : SelfRemovingComponent {
}