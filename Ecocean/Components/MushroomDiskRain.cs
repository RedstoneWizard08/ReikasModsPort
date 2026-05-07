using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class MushroomDiskRain : MonoBehaviour {
    public static bool permanentRain = false;

    public static readonly Color color1 = new(0.4F, 1.0F, 1.5F, 1F);
    public static readonly Color color2 = new(1.8F, 1.1F, 0.5F, 1F);

    private BoxCollider collider;
    private Color renderColor;
    private ParticleSystem particles;

    public static bool locked = false;

    private bool rainOn;

    private void Start() {
        toggleOff();
    }

    private void prepare() {
        renderColor = transform.position.x > 0 ? color2 : color1;
        var refC = transform.parent.GetComponentInChildren<BoxCollider>();
        collider = gameObject.EnsureComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(
            refC.size.x * 0.9F,
            refC.size.y * 8,
            refC.size.z * 0.9F
        ); //new Vector3(0.6F, 1.6F, 0.6F);
        collider.center = refC.center + Vector3.down * 2F; //Vector3.down * 0.8F;
        var fx = gameObject.getChildObject("Particles");
        if (!fx) {
            fx = ObjectUtil.lookupPrefab("0e67804e-4a59-449d-929a-cd3fc2bef82c").GetComponent<ParticleSystem>()
                .gameObject.clone();
            fx.removeComponent<Creature>();
            fx.removeComponent<BloomCreature>();
            fx.removeComponent<SwimRandom>();
            fx.removeComponent<StayAtLeashPosition>();
            fx.removeComponent<CreatureUtils>();
            fx.removeComponent<LiveMixin>();
            fx.removeComponent<BehaviourLOD>();
            fx.removeComponent<FleeWhenScared>();
            fx.removeComponent<WorldForces>();
            fx.removeComponent<SwimBehaviour>();
            fx.removeComponent<SplineFollowing>();
            fx.removeComponent<Locomotion>();
            fx.removeComponent<Rigidbody>();
            fx.removeComponent<PrefabIdentifier>();
            fx.removeComponent<EntityTag>();
            fx.removeComponent<TechTag>();
            fx.removeComponent<LargeWorldEntity>();
            fx.setName("Particles");
            fx.transform.SetParent(transform);
        }

        foreach (Transform t in fx.transform)
            t.gameObject.destroy(false);
        fx.transform.localPosition = collider.center; //+Vector3.down*0.7F;
        particles = fx.GetComponent<ParticleSystem>();
        var main = particles.main;
        main.gravityModifier = 0.2F;
        var emit = particles.emission;
        var sh = particles.shape;
        var clr = particles.colorOverLifetime;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.rotation = new Vector3(0, 0, 0);
        sh.radius = Mathf.Max(collider.size.x, collider.size.z) * 1.2F;
        var c = renderColor.Exponent(1.25F).WithAlpha(1);
        clr.color = c;
        main.startColor = c;
        emit.rateOverTimeMultiplier = 2.5F;
    }

    public void toggleOn() {
        if (!collider)
            prepare();
        if (locked)
            return;
        rainOn = true;
        collider.enabled = true;
        particles.Play(true);
        Invoke(nameof(toggleOff), Random.Range(1F, 6F));
        CancelInvoke(nameof(toggleOn));
    }

    public void toggleOff() {
        if (permanentRain) {
            Invoke(nameof(toggleOff), 10F);
            return;
        }

        if (locked)
            return;
        rainOn = false;
        Invoke(nameof(toggleOn), Random.Range(5F, 90F));
        CancelInvoke(nameof(toggleOff));
        if (!collider || !particles)
            return;
        collider.enabled = false;
        particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void OnTriggerStay(Collider other) {
        if (!rainOn) return;
        if (other.name is "RainHolder" or "Mouth")
            return;
        if (other.isTrigger && other.GetComponent<MushroomDiskRain>())
            return;
        if (other.isPlayer()) {
            var e = Player.main.gameObject.EnsureComponent<FoodEffectSystem.VisualDistortionEffect>();
            e.intensity = 2;
            e.timeRemaining = 10;
            e.effectColor = renderColor.ToVectorA().Exponent(4F);
            e.tintIntensity = 0.32F; //0.28
            e.tintColor = (renderColor.Exponent(2) * 4).WithAlpha(1);
        } else {
            //if (other.isTrigger)
            //	SNUtil.writeToChat("Touching "+other.gameObject.GetFullHierarchyPath());
            var sm = other.isTrigger ? null : other.gameObject.FindAncestor<SeaMoth>();
            if (sm) {
                SeamothPlanktonScoop.checkAndTryScoop(
                    sm,
                    Time.deltaTime,
                    EcoceanMod.treeMushroomSpores.TechType,
                    out var drop
                );
            }

            var area = other.isTrigger ? other.gameObject.FindAncestor<PlanktonClearingArea>() : null;
            if (!area) return;
            area.setProperty("mushdisk", true);
            area.setProperty("dropBias", EcoceanMod.treeMushroomSpores.TechType);
            area.setProperty("dropBiasChance", 0.5F);
            area.tickExternal(4);
        }
    }
}