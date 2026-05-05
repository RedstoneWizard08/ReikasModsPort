using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class Obsidian : BasicCustomOre {
    internal static readonly Vector3 BASE_SCALE = new(0.1F, 0.4F, 0.25F);

    [SetsRequiredMembers]
    public Obsidian(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
        collectSound = "event:/loot/pickup_quartz";
    }

    public override void prepareGameObject(GameObject go, Renderer[] r0) {
        base.prepareGameObject(go, r0);
        var c = go.GetComponentInChildren<Collider>();
        var hold = c.gameObject;
        if (!(c is BoxCollider))
            c.destroy();
        var bc = hold.EnsureComponent<BoxCollider>();
        bc.size = BASE_SCALE;
        bc.center = new Vector3(0, 0.15F, 0);
        foreach (var r in r0) {
            //GameObject go = ;
            RenderUtil.setEmissivity(r, 0);
            r.transform.localScale = BASE_SCALE;
            r.materials[0].SetFloat("_Fresnel", 0.25F);
            r.materials[0].SetFloat("_SpecInt", 1F);
            r.materials[0].SetFloat("_Shininess", 45F);
        }

        go.EnsureComponent<ObsidianTag>();
    }
}

internal class ObsidianTag : MonoBehaviour {
    public static readonly float MELT_TIME = 15F;

    public float meltLevel;

    private Renderer[] renders;

    private void Start() {
        renders = GetComponentsInChildren<Renderer>();
    }

    private void Update() {
        var dT = Time.deltaTime;

        var temp = WaterTemperatureSimulation.main.GetTemperature(transform.position);
        if (temp >= 100) {
            meltLevel += dT / MELT_TIME;

            foreach (var r in renders) {
                r.materials[0].SetColor("_SpecColor", new Color(1 + 9 * meltLevel, 1 + 3 * meltLevel, 1, 1));
                if (meltLevel > 0.67) {
                    r.transform.localScale =
                        Obsidian.BASE_SCALE * (float)MathUtil.linterpolate(meltLevel, 0.67, 1, 1, 0, true);
                }
            }

            if (meltLevel >= 1)
                gameObject.destroy(false);
        }
    }
}