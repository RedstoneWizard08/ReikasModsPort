using System.Collections;
using ECCLibrary;
using ECCLibrary.Data;
using Nautilus.Assets;
using Nautilus.Utility;
using Nautilus.Utility.MaterialModifiers;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class VoidSpikeLeviathan : CreatureAsset {
    private static readonly int Fresnel = Shader.PropertyToID("_Fresnel");
    private readonly XMLLocale.LocaleEntry locale;

    internal VoidSpikeLeviathan(XMLLocale.LocaleEntry e) : base(
        PrefabInfo.WithTechType("voidspikelevi", e.name, e.desc)
    ) {
        locale = e;

        // const string path = "Lifeforms/Fauna/Leviathans";
        //
        // var encyEntryData = new PDAEncyclopedia.EntryData {
        //     nodes = path.Split('/'),
        //     path = path,
        //     image = TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/PDA/" + locale.getString("header")),
        //     unlocked = true,
        //     key = e.key,
        //     kind = PDAEncyclopedia.EntryData.Kind.Encyclopedia,
        // };
        //
        // LanguageHandler.SetLanguageLine("EncyDesc_" + e.key, locale.pda);
        //
        // PDAHandler.AddEncyclopediaEntry(encyEntryData);
    }

    protected override CreatureTemplate CreateTemplate() {
        var liveMixinData = ScriptableObject.CreateInstance<LiveMixinData>();

        liveMixinData.maxHealth = 75000f; //15x reaper

        var template = new CreatureTemplate(loadAsset(), BehaviourType.Leviathan, EcoTargetType.Leviathan, 600f) {
            CellLevel = LargeWorldEntity.CellLevel.VeryFar,
            SwimRandomData = new SwimRandomData(0.1F, 10F, Vector3.one * 150, 5F, 1f, true),
            StayAtLeashData = new StayAtLeashData(0.5F, 10f, 150),
            AvoidObstaclesData = new AvoidObstaclesData(0.2f, 10f, false, 25f, 25f),
            Mass = 10000f, //4x reaper
            EyeFOV = -1f,
            AttackLastTargetData = new AttackLastTargetData(0.8f, 12f, 0.5f, 10f),
            RespawnData = new RespawnData(false),
            ScannerRoomScannable = true,
            CanBeInfected = false,
            AggressiveToPilotingVehicleData = new AggressiveToPilotingVehicleData(440f, 1f),
            BehaviourLODData = new BehaviourLODData(600, 999, 999),
            TraitsData = new CreatureTraitsData(0.1f, 0.02f, 0.25f),
            LiveMixinData = liveMixinData,
        };

        template.AddAggressiveWhenSeeTargetData(
            new AggressiveWhenSeeTargetData(EcoTargetType.Leviathan, 1f, 512, 8, hungerThreshold: 0)
        );

        template.AddAggressiveWhenSeeTargetData(
            new AggressiveWhenSeeTargetData(EcoTargetType.Whale, 0.5f, 512, 8, hungerThreshold: 0)
        );

        template.AddAggressiveWhenSeeTargetData(
            new AggressiveWhenSeeTargetData(EcoTargetType.Shark, 0.25f, 400, 6, hungerThreshold: 0.2f)
        );

        return template;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab, CreatureComponents cc) {
        // //if you have special components you want to add here (like your own custom CreatureActions) then add them here
        // if (cc.renderer)
        //     cc.renderer.materials[0].SetFloat("_Fresnel", 0.8F);

        if (cc.InfectedMixin) cc.InfectedMixin.RemoveInfection();
        if (cc.Locomotion) cc.Locomotion.maxVelocity = 30f;

        var decreasing = new AnimationCurve(
            new Keyframe(0f, 0.25f * 1.5f),
            new Keyframe(1f, 0.75f * 1.5f)
        );

        var trailManagerBuilder = new TrailManagerBuilder(cc, prefab.transform) {
            SegmentSnapSpeed = 1.5F,
            MaxSegmentOffset = 15f,
            PitchMultiplier = decreasing,
            RollMultiplier = decreasing,
            YawMultiplier = decreasing,
        };

        trailManagerBuilder.SetTrailArrayToPhysBoneChildren();
        trailManagerBuilder.AllowDisableOnScreen = false;
        trailManagerBuilder.Apply();

        prefab.EnsureComponent<VoidSpikeLeviathanAI>();

        yield break;
    }

    protected override void ApplyMaterials(GameObject prefab) {
        MaterialUtils.ApplySNShaders(prefab, 1f, 3f, 2f, new VoidSpikeMaterialMods());
    }

    // public override float TurnSpeedHorizontal => 0.5F;
    // public override float TurnSpeedVertical => 0.8F;

    private static GameObject loadAsset() {
        var ab = ReikaKalseki.DIAlterra.AssetBundleManager.getBundle(SeaToSeaMod.ModDLL, "voidlevi");
        return ab.LoadAsset<GameObject>("VoidSpikeLevi_FixedRig");
    }

    public void register() {
        Register();
        SNUtil.AddPdaEntry(CustomPrefab, 20, "Lifeforms/Fauna/Leviathans", locale.pda, locale.getString("header"));
    }

    public static void MakeReefbackTest() {
        var go = ObjectUtil.createWorldObject(VanillaCreatures.REEFBACK.prefab);
        var ai = go.EnsureComponent<VoidSpikeLeviathanAI>();
        ai.Spawn();
        var inner = go.getChildObject("Pivot/Reefback/Reefback");
        ai.CreatureRenderer = inner.GetComponent<Renderer>();
        go.transform.position = Player.main.transform.position + Camera.main.transform.forward.normalized * 80;
        ai.IsDebug = true;
    }

    internal class VoidSpikeLeviathanAI : MonoBehaviour {
        private const float EmpChargeTime = 4F;
        private const float FlashChargeTime = 1.0F;
        private const float MaxEmissivityDelta = 100F;

        internal bool IsDebug;

        private static readonly SoundManager.SoundData EmpChargeSound = SoundManager.registerSound(
            SeaToSeaMod.ModDLL,
            "voidlevi-emp-charge",
            "Sounds/voidlevi/emp-charge-2.ogg",
            SoundManager.soundMode3D
        );

        private static readonly SoundManager.SoundData FlashChargeSound = SoundManager.registerSound(
            SeaToSeaMod.ModDLL,
            "voidlevi-flash-charge",
            "Sounds/voidlevi/flash-charge-2.ogg",
            SoundManager.soundMode3D
        );

        private Creature _creatureClass;
        internal Renderer CreatureRenderer;

        private float _empRamp = -1;
        private float _flashRamp = -1;

        private float _nextPossibleBurstTime = 10;

        private float _targetEmissivity;
        private float _currentEmissivity = 1;

        private void Start() {
        }

        internal void Spawn() {
            var time = DayNightCycle.main.timePassedAsFloat;
            _nextPossibleBurstTime = time + 10;
        }

        private void Update() {
            if (!_creatureClass)
                _creatureClass = GetComponent<Creature>();
            if (!CreatureRenderer)
                CreatureRenderer = GetComponentInChildren<Renderer>();

            if (_creatureClass) {
                _creatureClass.leashPosition = Player.main.transform.position;
                if (IsDebug)
                    _creatureClass.Aggression.Add(1);

                if (Story.StoryGoalManager.main.completedGoals.Contains(VoidSpikeLeviathanSystem.PASSIVATION_GOAL)) {
                    //sea emperor befriend
                    _creatureClass.Aggression.Add(-0.1F);
                }
            }

            var dT = Time.deltaTime;
            var time = DayNightCycle.main.timePassedAsFloat;
            if (IsEmpInProgress()) {
                _empRamp += dT / EmpChargeTime;
                if (_empRamp >= 1) {
                    DoEmp();
                    _empRamp = -1;
                }
            } else if (IsFlashInProgress()) {
                _flashRamp += dT / FlashChargeTime;
                if (_flashRamp >= 1) {
                    DoFlash();
                    _flashRamp = -1;
                }
            } else if (time >= _nextPossibleBurstTime && CanStartAPulse() &&
                       Random.Range(0F, 1F) <= 0.01F * _creatureClass.Aggression.Value) {
                var f = 0.15F + 0.35F * _creatureClass.Aggression.Value;
                if (Random.Range(0F, 1F) < f)
                    StartFlash();
                else
                    StartEmp();
            }

            _targetEmissivity = 1F + 0.5F * Mathf.Sin(transform.position.magnitude * 0.075F);
            if (IsEmpInProgress()) {
                _targetEmissivity = Mathf.Max(
                    0,
                    _empRamp * 4F + Random.Range(0F, 2.5F) + 4F * Mathf.Sin(_empRamp * 8)
                );
            } else if (IsFlashInProgress()) {
                _targetEmissivity = Mathf.Max(
                    0,
                    _flashRamp * _flashRamp * 50 * Random.Range(0, 1F) /*+0.5F+15F*Mathf.Sin(flashRamp*50)*/
                );
            }

            if (_currentEmissivity < _targetEmissivity) {
                _currentEmissivity = Mathf.Min(_targetEmissivity, _currentEmissivity + dT * MaxEmissivityDelta);
            } else if (_currentEmissivity > _targetEmissivity) {
                _currentEmissivity = Mathf.Max(_targetEmissivity, _currentEmissivity - dT * MaxEmissivityDelta);
            }

            if (CreatureRenderer)
                RenderUtil.setEmissivity(CreatureRenderer.materials[0], Mathf.Max(0, _currentEmissivity));
        }

        internal void StartEmp() {
            if (!CanStartAPulse())
                return;
            _empRamp = 0;
            SoundManager.playSoundAt(EmpChargeSound, transform.position, false, -1, 1);
            _nextPossibleBurstTime =
                DayNightCycle.main.timePassedAsFloat + Random.Range(30, IsDebug ? 31 : 90);
        }

        internal void StartFlash() {
            if (!CanStartAPulse())
                return;
            _flashRamp = 0;
            SoundManager.playSoundAt(FlashChargeSound, transform.position, false, -1, 1);
            _nextPossibleBurstTime =
                DayNightCycle.main.timePassedAsFloat + Random.Range(60, IsDebug ? 90 : 120);
        }

        internal bool IsEmpInProgress() {
            return _empRamp >= 0;
        }

        internal bool IsFlashInProgress() {
            return _flashRamp >= 0;
        }

        internal bool CanStartAPulse() {
            return !IsFlashInProgress() && !IsEmpInProgress() &&
                   !VoidSpikeLeviathanSystem.instance.isVoidFlashActive(true);
        }

        private void DoEmp() {
            VoidSpikeLeviathanSystem.instance.spawnEMPBlast(transform.position);
        }

        private void DoFlash() {
            VoidSpikeLeviathanSystem.instance.doFlash(transform.position);
        }

        private void OnDisable() {
            gameObject.destroy(false);
        }

        private void OnDestroy() {
            VoidSpikeLeviathanSystem.instance.deleteVoidLeviathan();
        }

        public void OnMeleeAttack(GameObject target) {
            //SNUtil.writeToChat(this+" attacked "+target);
            var v = target.GetComponent<Vehicle>();
            if (v) {
                VoidSpikeLeviathanSystem.instance.shutdownSeamoth(
                    v,
                    true,
                    6
                ); //shut down for up to 30s, drain up to 25% power
            }
        }
    }

    internal class VoidSpikeMaterialMods : MaterialModifier {
        public override void EditMaterial(
            Material material,
            Renderer renderer,
            int materialIndex,
            MaterialUtils.MaterialType materialType
        ) {
            material.SetFloat(Fresnel, 0.8f);
        }
    }
}