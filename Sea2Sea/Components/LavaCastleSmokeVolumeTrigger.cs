using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class LavaCastleSmokeVolumeTrigger : MonoBehaviour {
    private static float lastCollectTime = 0;

    private GameObject sparkleObject;

    private ParticleSystem[] particles;

    private float animSpeed = UnityEngine.Random.Range(0.2F, 0.3F) * 1.25F;

    private float age;

    void OnTriggerStay(Collider other) {
        float time = DayNightCycle.main.timePassedAsFloat;
        if (time - lastCollectTime < 2.5F || UnityEngine.Random.Range(0F, 1F) > 0.3F)
            return;
        SeaMoth sm = other.gameObject.FindAncestor<SeaMoth>();
        if (sm) {
            SeamothPlanktonScoop.checkAndTryScoop(
                sm,
                Time.deltaTime,
                CraftingItems.getItem(CraftingItems.Items.LavaPlankton).Info.TechType,
                out GameObject drop
            );
            if (drop) {
                lastCollectTime = time;
            }
        }
    }

    void Update() {
        age += Time.deltaTime;

        if (!sparkleObject) {
            sparkleObject = gameObject.getChildObject("Sparkle");
            particles = sparkleObject.GetComponentsInChildren<ParticleSystem>();
        }

        sparkleObject.layer = LayerID.Useable;
        gameObject.layer = LayerID.Useable;

        foreach (ParticleSystem p in particles) {
            ParticleSystem.MainModule main = p.main;
            main.simulationSpeed = animSpeed * (float)MathUtil.linterpolate(age, 0, 2, 100, 1, true);
        }
    }
}