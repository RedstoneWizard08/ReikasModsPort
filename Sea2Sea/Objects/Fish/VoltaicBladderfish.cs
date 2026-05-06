using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class VoltaicBladderfish : RetexturedFish, MultiTexturePrefab {
    private readonly XMLLocale.LocaleEntry locale;

    [SetsRequiredMembers]
    internal VoltaicBladderfish(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.BLADDERFISH.prefab) {
        locale = e;
        glowIntensity = 1.0F;
    }

    public override void prepareGameObject(GameObject world, Renderer[] r0) {
        var kc = world.EnsureComponent<VoltaicBladderfishTag>();
        foreach (var r in r0) {
            foreach (var m in r.materials) {
                m.SetColor("_GlowColor", new Color(1, 1, 1, 1));
                RenderUtil.disableTransparency(m);
            }
        }

        var inner = ObjectUtil.lookupPrefab(VanillaCreatures.BOOMERANG.prefab).GetComponentInChildren<Animator>()
            .gameObject.clone();
        inner.transform.SetParent(world.GetComponentInChildren<Animator>().transform);
        inner.gameObject.name = "AuxMdl";
        foreach (var r in inner.GetComponentsInChildren<Renderer>()) {
            RenderUtil.swapTextures(SeaToSeaMod.ModDLL, r, "Textures/Creature/VoltaicBladderfishAux");
            RenderUtil.setEmissivity(r, 1);
        }

        Utils.ZeroTransform(inner.transform);
        inner.transform.localScale = new Vector3(1, 0.5F, 1.2F);
        inner.transform.localPosition = new Vector3(-0.0075F, 0, 0.155F);

        world.EnsureComponent<VoltaicFishSparker>();
    }

    public override BehaviourType GetBehavior() {
        return BehaviourType.SmallFish;
    }

    public Dictionary<int, string> getTextureLayers(Renderer r) {
        return new Dictionary<int, string> { { 0, "" }, { 1, "" } };
    }
}

internal class VoltaicFishSparker : AzuriteSparker {
    public VoltaicFishSparker() : base(2.5F, 0.5F, false, new Vector3(0, 0, 0)) {
    }

    public override bool disableSparking() {
        return GetComponent<WaterParkCreature>();
    }
}

internal class VoltaicBladderfishTag : MonoBehaviour, AmpeelAntennaCreature {
    public static readonly float POWER_EXPONENT = 0.125F;

    private Renderer[] renders;

    private float currentEmissivity = 1;

    private float targetEmissivity;

    /*
    static VoltaicBladderfishTag() {
        for (int i = 1; i <= 5; i++) {
            computeMaximumEfficiency(i);
        }
    }
    */
    public LiveMixin live => GetComponent<LiveMixin>();

    public float ampeelValue => 0F;

    public float powerExponentAddition => POWER_EXPONENT;

    public static void computeMaximumEfficiency(int acuSize) {
        SNUtil.Log("Voltaic Bladderfish yields for size-" + acuSize + " ACU:");
        var refAmt = AmpeelAntenna.ACU_COEFFICIENT * AmpeelAntenna.POWER_GEN;
        var ampSize = 3; //ampeel is 3 units each;
        var slots = 10 * acuSize;
        var maxEels = slots / ampSize;
        for (var ampeels = 1; ampeels <= maxEels; ampeels++) {
            var maxVolt = slots - ampSize * ampeels;
            for (var volt = 0; volt <= maxVolt; volt++) {
                var yield = Mathf.Min(AmpeelAntenna.AMPEEL_CAP, Mathf.Pow(ampeels, 1 + volt * POWER_EXPONENT));
                SNUtil.Log(
                    "    " + ampeels + " ampeels + " + volt + " voltaic: " + (yield * refAmt).ToString("0.00") + " (" +
                    yield.ToString("0.00") + "x)"
                );
            }
        }
    }

    private void Update() {
        if (renders == null)
            renders = GetComponentsInChildren<Renderer>();

        if (Mathf.Abs(currentEmissivity - targetEmissivity) < 0.1F) {
            targetEmissivity = Random.Range(0.9F, 2.5F);
        } else {
            currentEmissivity += Mathf.Sign(targetEmissivity - currentEmissivity) * Time.deltaTime;
        }

        foreach (var r in renders) {
            if (r)
                RenderUtil.setEmissivity(r, currentEmissivity);
        }

        transform.localScale = Vector3.one * 1.5F;
    }
}