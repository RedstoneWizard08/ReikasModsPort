using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class PurpleBoomerang : RetexturedFish {
    private readonly XMLLocale.LocaleEntry locale;

    [SetsRequiredMembers]
    internal PurpleBoomerang(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.BOOMERANG.prefab) {
        locale = e;
        glowIntensity = 1.0F;
    }

    public override void prepareGameObject(GameObject world, Renderer[] r0) {
        var kc = world.EnsureComponent<PurpleBoomerangTag>();
        foreach (var r in r0) {
            r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
            RenderUtil.setGlossiness(r, 0.5F, 6, 0.5F);
        }
    }

    public override BehaviourType GetBehavior() {
        return BehaviourType.SmallFish;
    }
}

internal class PurpleBoomerangTag : MonoBehaviour {
    private Renderer[] renders;

    private void Update() {
        if (renders == null)
            renders = GetComponentsInChildren<Renderer>();

        var f = Mathf.Max(
            0,
            -0.5F + 2 * (0.5F +
                         0.5F * Mathf.Sin(8 * DayNightCycle.main.timePassedAsFloat + gameObject.GetInstanceID()))
        );
        foreach (var r in renders) {
            RenderUtil.setEmissivity(r, f);
        }
    }
}