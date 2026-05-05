using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class PurpleHoopfish : RetexturedFish {
    private readonly XMLLocale.LocaleEntry locale;

    [SetsRequiredMembers]
    internal PurpleHoopfish(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.HOOPFISH.prefab) {
        locale = e;
        glowIntensity = 1.0F;
    }

    public override void prepareGameObject(GameObject world, Renderer[] r0) {
        var kc = world.EnsureComponent<PurpleHoopfishTag>();
        foreach (var r in r0) {
            r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
        }
    }

    public override BehaviourType GetBehavior() {
        return BehaviourType.SmallFish;
    }
}

internal class PurpleHoopfishTag : MonoBehaviour {
    private Renderer[] renders;

    private void Update() {
        if (renders == null)
            renders = GetComponentsInChildren<Renderer>();
    }
}