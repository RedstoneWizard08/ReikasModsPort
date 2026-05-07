using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class PinkLeaves : BasicCustomPlant, MultiTexturePrefab {
    [SetsRequiredMembers]
    public PinkLeaves(XMLLocale.LocaleEntry e) : base(
        e,
        new FloraPrefabFetch(DecoPlants.BANANA_LEAF.prefab),
        "daff0e31-dd08-4219-8793-39547fdb745e",
        "Cuttings"
    ) {
        finalCutBonus = 1;
        glowIntensity = 1.2F;
        collectionMethod = HarvestType.DamageAlive;
        //OnFinishedPatching += () => {addPDAEntry(e.pda, 4F, e.getString("header"));};
    }

    public override Vector2int SizeInInventory => new(1, 1);

    public override void prepareGameObject(GameObject go, Renderer[] r0) {
        base.prepareGameObject(go, r0);

        var mdl = r0[0].transform.parent;

        foreach (var r in r0) {
            if (r)
                r.gameObject.destroy(false);
        }

        if (!go.GetComponentInChildren<Collider>()) {
            var cc = go.EnsureComponent<CapsuleCollider>();
            cc.radius = 0.67F;
            cc.center = Vector3.up * 0.9F;
            cc.height = 1.75F;
            cc.isTrigger = true;
        }

        go.layer = LayerID.Useable;

        go.EnsureComponent<LiveMixin>()
            .CopyObject<LiveMixin>(ObjectUtil.lookupPrefab(TechType.SeaCrown).GetComponent<LiveMixin>());

        var pfb = ObjectUtil.lookupPrefab(DecoPlants.BANANA_LEAF.prefab);
        foreach (var r in pfb.GetComponentsInChildren<Renderer>()) {
            if (!r || r.name.Contains("LOD"))
                continue;
            var rg = r.gameObject.clone();
            rg.transform.SetParent(mdl);
            rg.transform.localPosition = r.transform.localPosition;
            rg.transform.localRotation = r.transform.localRotation;
            rg.transform.localScale = r.transform.localScale * 0.33F;
        }

        r0 = go.GetComponentsInChildren<Renderer>();
        RenderUtil.swapToModdedTextures(r0, this);
        foreach (var r in r0) {
            if (!r)
                continue;
            RenderUtil.makeTransparent(r.materials[0]);
            if (r.materials.Length > 1)
                RenderUtil.setEmissivity(r.materials[1], 2);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            foreach (var m in r.materials) {
                m.SetColor("_GlowColor", Color.white);
                m.EnableKeyword("UWE_WAVING");
                m.SetVector("_Scale", new Vector4(0.05F, 0.05F, 0.05F, 0.05F));
                m.SetVector("_Frequency", new Vector4(0.3F, 0.3F, 0.3F, 0.3F));
                m.SetFloat("_Cutoff", 0.05F);
            }
        }

        var l = go.addLight(0.5F, 8, new Color(1F, 153 / 255F, 1F, 1F));
        l.lightShadowCasterMode = LightShadowCasterMode.Default;
        l.shadows = LightShadows.Soft;

        go.EnsureComponent<PinkLeavesTag>();
    }

    public Dictionary<int, string> getTextureLayers(Renderer r) {
        return new Dictionary<int, string> { { 0, "Leaf" }, { 1, "" } };
    }

    public override float getScaleInGrowbed(bool indoors) {
        return 0.33F;
    }

    public override bool isResource() {
        return false;
    }

    public override Plantable.PlantSize getSize() {
        return Plantable.PlantSize.Large;
    }

    public override bool canGrowAboveWater() {
        return true;
    }

    public override bool canGrowUnderWater() {
        return true;
    }
}

internal class PinkLeavesTag : MonoBehaviour {
    private GrownPlant grown;

    private void Start() {
        grown = gameObject.GetComponent<GrownPlant>();
        if (grown) {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
            var r0 = GetComponentsInChildren<Renderer>();
            if (r0.Length > 1) {
                for (var i = 1; i < r0.Length; i++) {
                    r0[i].gameObject.destroy(false);
                }
            }

            r0[0].transform.localScale = Vector3.one * 0.2F;
            EcoceanMod.pinkLeaves.prepareGameObject(gameObject, r0);
            var lv = gameObject.EnsureComponent<LiveMixin>();
            lv.CopyObject<LiveMixin>(ObjectUtil.lookupPrefab(TechType.SeaCrown).GetComponent<LiveMixin>());
            if (lv.damageInfo == null)
                lv.damageInfo = new DamageInfo();
            lv.ResetHealth();
        } else {
            transform.localScale = Vector3.one * 0.5F;
        }
    }
}