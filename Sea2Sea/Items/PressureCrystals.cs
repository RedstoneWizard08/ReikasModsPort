using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class PressureCrystals : BasicCustomOre {
    [SetsRequiredMembers]
    public PressureCrystals(string id, string name, string desc, VanillaResources template) : base(
        id,
        name,
        desc,
        template
    ) {
        collectSound = "event:/loot/pickup_quartz";
    }

    public override void prepareGameObject(GameObject go, Renderer[] r0) {
        base.prepareGameObject(go, r0);
        foreach (var r in r0) {
            RenderUtil.makeTransparent(r);
            r.sharedMaterial.SetFloat("_Fresnel", 0.65F);
            r.sharedMaterial.SetFloat("_Shininess", 15F);
            r.sharedMaterial.SetFloat("_SpecInt", 18F);
            r.materials[0].SetFloat("_Fresnel", 0.6F);
            r.materials[0].SetFloat("_Shininess", 15F);
            r.materials[0].SetFloat("_SpecInt", 18F);
            r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
        }

        var l = go.addLight(0.3F, 4, new Color(1F, 0.45F, 1F));
        l.type = LightType.Point;
        l = go.addLight(1.2F, 1, new Color(1F, 0.45F, 1F));
        l.type = LightType.Point;
    }
}