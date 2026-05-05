using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class GiantRockGrub : RetexturedFish {
    private readonly XMLLocale.LocaleEntry locale;

    [SetsRequiredMembers]
    internal GiantRockGrub(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.ROCKGRUB.prefab) {
        locale = e;

        ScanTime = 15;

        EggSpawnRate = 0F;
    }

    public override void prepareGameObject(GameObject world, Renderer[] r0) {
        var kc = world.EnsureComponent<GiantRockGrubTag>();

        world.transform.localScale = new Vector3(30, 20, 20);

        world.GetComponent<WorldForces>().underwaterGravity = 3;

        world.GetComponent<LiveMixin>().data.maxHealth *= 20;

        world.removeComponent<Scareable>();

        world.removeComponent<Eatable>();

        world.removeComponent<MoveTowardsTarget>();

        var c = world.GetComponent<Creature>();

        var agg = world.EnsureComponent<AggressiveWhenSeeTarget>();
        agg.targetType = EcoTargetType.Shark;
        agg.aggressionPerSecond = 0.5F;
        agg.creature = c;
        agg.ignoreSameKind = true;

        world.EnsureComponent<AttackLastTarget>().creature = c;
        var me = world.EnsureComponent<MeleeAttack>();
        me.ignoreSameKind = true;
        me.mouth = world;
        me.liveMixin = world.GetComponent<LiveMixin>();
        me.creature = c;

        world.GetComponent<Locomotion>().canWalkOnSurface = true;

        world.removeComponent<SphereCollider>();
        var cc = world.EnsureComponent<CapsuleCollider>();
        cc.direction = 2;
        cc.height = 0.3F;
        cc.radius = 0.04F;
    }

    public override BehaviourType GetBehavior() {
        return BehaviourType.Crab;
    }
}

internal class GiantRockGrubTag : MonoBehaviour {
    private Renderer[] renders;

    private float lastTickTime = -1;

    private void Awake() {
    }

    private void Update() {
        if (renders == null)
            renders = GetComponentsInChildren<Renderer>();
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time - lastTickTime >= 0.5F) {
            lastTickTime = time;
            transform.localScale = new Vector3(30, 20, 20);
        }
    }
}