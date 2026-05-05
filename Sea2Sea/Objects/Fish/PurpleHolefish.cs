using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class PurpleHolefish : RetexturedFish {
    private readonly XMLLocale.LocaleEntry locale;

    [SetsRequiredMembers]
    internal PurpleHolefish(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.HOLEFISH.prefab) {
        locale = e;
        glowIntensity = 0.5F;

        ScanTime = 4;

        EggBase = TechType.Gasopod;
        BigEgg = false;
        EggMaturationTime = 1200;
        EggSpawnRate = 1.5F;
        //eggSpawns.Add(BiomeType.UnderwaterIslands_IslandCaveFloor);
    }

    public override Vector2int SizeInInventory => new(2, 2);

    public override void prepareGameObject(GameObject world, Renderer[] r0) {
        var kc = world.EnsureComponent<PurpleHolefishTag>();
        foreach (var r in r0) {
            r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
            RenderUtil.setGlossiness(r, 3F, 0, 1F);
        }

        world.GetComponent<SwimBehaviour>().turnSpeed *= 0.125F;
        world.GetComponent<SwimRandom>().swimVelocity *= 0.25F;
        world.GetComponent<SwimRandom>().swimForward *= 0.5F;
        world.GetComponent<Locomotion>().maxVelocity *= 0.25F;
        world.GetComponent<Eatable>().foodValue *= 2.4F;
        world.removeComponent<Eatable>();
        world.GetComponentInChildren<AnimateByVelocity>().animSpeedValue *= 0.25F;
        world.GetComponentInChildren<AnimateByVelocity>().animationMoveMaxSpeed *= 0.25F;
        world.GetComponent<SplineFollowing>().inertia *= 2;
        world.removeComponent<FleeWhenScared>();
        world.removeComponent<Scareable>();
        //world.removeComponent<Pickupable>();
    }

    public override BehaviourType GetBehavior() {
        return BehaviourType.MediumFish;
    }
}

internal class PurpleHolefishTag : MonoBehaviour {
    private Renderer[] renders;
    private Animator[] animators;

    private Pickupable pickup;

    private bool isACU;

    private float lastTickTime = -1;

    private void Awake() {
        isACU = GetComponent<WaterParkCreature>();
    }

    private void Update() {
        if (renders == null)
            renders = GetComponentsInChildren<Renderer>();
        if (animators == null)
            animators = GetComponentsInChildren<Animator>();
        if (!pickup)
            pickup = GetComponent<Pickupable>();
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time - lastTickTime >= 0.5F) {
            lastTickTime = time;
            isACU = GetComponent<WaterParkCreature>();
            transform.localScale = Vector3.one * (isACU ? 1.5F : 5);
            foreach (var a in animators) {
                if (a)
                    a.speed = 0.25F;
            }

            if (isACU)
                gameObject.removeChildObject("model_FP");
        }

        pickup.isPickupable = isACU;
    }
}