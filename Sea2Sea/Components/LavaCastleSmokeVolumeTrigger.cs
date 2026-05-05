using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class LavaCastleSmokeVolumeTrigger : MonoBehaviour {
    private static float lastCollectTime;

    private GameObject sparkleObject;

    private ParticleSystem[] particles;

    private float animSpeed = Random.Range(0.2F, 0.3F) * 1.25F;

    private float age;

    private void OnTriggerStay(Collider other) {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time - lastCollectTime < 2.5F || Random.Range(0F, 1F) > 0.3F)
            return;
        var sm = other.gameObject.FindAncestor<SeaMoth>();
        if (sm) {
            SeamothPlanktonScoop.checkAndTryScoop(
                sm,
                Time.deltaTime,
                CraftingItems.getItem(CraftingItems.Items.LavaPlankton).Info.TechType,
                out var drop
            );
            if (drop) {
                lastCollectTime = time;
            }
        }
    }

    private void Update() {
        age += Time.deltaTime;

        if (!sparkleObject) {
            sparkleObject = gameObject.getChildObject("Sparkle");
            particles = sparkleObject.GetComponentsInChildren<ParticleSystem>();
        }

        sparkleObject.layer = LayerID.Useable;
        gameObject.layer = LayerID.Useable;

        foreach (var p in particles) {
            var main = p.main;
            main.simulationSpeed = animSpeed * (float)MathUtil.linterpolate(age, 0, 2, 100, 1, true);
        }
    }
}