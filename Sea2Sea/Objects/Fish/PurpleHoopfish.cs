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
        PurpleHoopfishTag kc = world.EnsureComponent<PurpleHoopfishTag>();
        foreach (Renderer r in r0) {
            r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
        }
    }

    public override BehaviourType getBehavior() {
        return BehaviourType.SmallFish;
    }
}

class PurpleHoopfishTag : MonoBehaviour {
    private Renderer[] renders;

    void Update() {
        if (renders == null)
            renders = this.GetComponentsInChildren<Renderer>();
    }
}