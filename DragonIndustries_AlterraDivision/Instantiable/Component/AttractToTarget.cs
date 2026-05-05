using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class AttractToTarget : MonoBehaviour {
    public static AttractToTarget attractCreatureToTarget(
        Creature c,
        MonoBehaviour obj,
        bool isHorn,
        float maxDuration = 20
    ) {
        if (obj is BaseRoot)
            obj = obj.GetComponentsInChildren<BaseCell>().GetRandom().GetComponent<LiveMixin>();
        var ac = c.gameObject.EnsureComponent<AttractToTarget>();
        //SNUtil.writeToChat("Attracted "+c+" @ "+c.transform.position+" to "+obj+" @ "+obj.transform.position);
        ac.fire(obj, isHorn, maxDuration);
        if (c is Reefback && isHorn)
            SoundManager.playSoundAt(
                c.GetComponent<FMOD_CustomLoopingEmitter>().asset,
                c.transform.position,
                false,
                -1,
                1
            );
        return ac;
    }

    private MonoBehaviour target;
    private bool isHorn;

    private Creature owner;
    private SwimBehaviour swimmer;
    private StayAtLeashPosition leash;
    private AttackCyclops cyclopsAttacker;
    private LastTarget targeter;
    private MeleeAttack[] attacks;
    private AggressiveWhenSeeTarget[] targeting;
    private AttackLastTarget attacker;

    private float lastTick;

    public bool deleteOnAttack;

    private float delete;

    private void fire(MonoBehaviour from, bool horn, float maxDuration = 20) {
        target = from;
        isHorn |= horn;
        delete = Mathf.Max(delete, DayNightCycle.main.timePassedAsFloat + maxDuration);
    }

    public void OnMeleeAttack(GameObject target) {
        if (target && target.isAncestorOf(this.target) && deleteOnAttack) {
            setTarget(null);
            this.destroy();
        }
    }

    private void Update() {
        if (!owner)
            owner = GetComponent<Creature>();
        if (!swimmer)
            swimmer = GetComponent<SwimBehaviour>();
        if (!leash)
            leash = GetComponent<StayAtLeashPosition>();
        if (!cyclopsAttacker)
            cyclopsAttacker = GetComponent<AttackCyclops>();
        if (!targeter)
            targeter = GetComponent<LastTarget>();
        if (attacks == null)
            attacks = GetComponents<MeleeAttack>();
        if (targeting == null)
            targeting = GetComponents<AggressiveWhenSeeTarget>();
        if (attacker == null)
            attacker = GetComponent<AttackLastTarget>();

        var time = DayNightCycle.main.timePassedAsFloat;
        if (time >= delete) {
            this.destroy();
            return;
        }

        if (target.isPlayer()) {
            if (Player.main.currentSub) {
                target = Player.main.currentSub;
            } else {
                var v = Player.main.GetVehicle();
                if (v)
                    target = v;
            }
        }

        if (target is AggroAttractor aa) {
            if (!aa.isAggroable) {
                setTarget(null);
                this.destroy();
                return;
            }
        }

        if (time - lastTick <= 0.5)
            return;
        lastTick = time;

        if (owner is Reefback reefback && isHorn) {
            swimmer.SwimTo(target.transform.position, reefback.maxMoveSpeed);
            reefback.SetFriend(target.gameObject);
            return;
        }

        if (target is SubRoot && !(cyclopsAttacker && cyclopsAttacker.isActiveAndEnabled)) {
            this.destroy();
            return;
        }

        if (Vector3.Distance(transform.position, target.transform.position) >= 40)
            swimmer.SwimTo(target.transform.position, 10);

        owner.Aggression.Add(deleteOnAttack && delete - time > 1000 ? 1 : isHorn ? 0.5F : 0.05F);
        if (owner is CrabSnake cs) {
            if (cs.IsInMushroom()) {
                cs.ExitMushroom(target.transform.position);
            }
        }

        //if (leash)
        //	leash.
        setTarget(target.gameObject);
    }

    private void setTarget(GameObject go) {
        if (cyclopsAttacker)
            cyclopsAttacker.SetCurrentTarget(go, false);
        if (targeter) {
            targeter.SetTarget(go);
            if (delete - DayNightCycle.main.timePassedAsFloat > 1000 && deleteOnAttack)
                targeter.SetLockedTarget(go);
        }

        if (attacker)
            attacker.currentTarget = go;
        foreach (var a in attacks)
            a.lastTarget.SetTarget(go);
        foreach (var a in targeting)
            a.lastTarget.SetTarget(go);
    }

    public bool isTargeting(GameObject go) {
        return target.gameObject == go;
    }
}

public interface AggroAttractor {
    bool isAggroable { get; }
}