using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class BasePillar : CustomMachine<BasePillarLogic> {
    [SetsRequiredMembers]
    public BasePillar(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "4cb154ef-bdb6-4ff4-9107-f378ce21a9b7") {
        addIngredient(TechType.Titanium, 2);
    }

    public override bool UnlockedAtStart => true;

    public override bool isOutdoors() {
        return false;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeComponent<Bench>();

        var lgc = go.GetComponent<BasePillarLogic>();

        var mdl = go.getChildObject("bench");
        go.GetComponent<Constructable>().model = mdl;

        mdl.transform.localScale = new Vector3(0.8F, 1, 1.83F);
        mdl.transform.localEulerAngles = new Vector3(90, 0, 0);
        mdl.transform.localPosition = new Vector3(0, 1.45F, -0.15F);

        var mdl2 = go.getChildObject("bench2");
        if (!mdl2) {
            mdl2 = mdl.clone().setName("bench2");
            mdl2.transform.SetParent(mdl.transform.parent);
        }

        mdl2.transform.localScale = new Vector3(0.8F, 0.6F, 1.83F);
        mdl2.transform.localEulerAngles = new Vector3(90, 180, 0);
        mdl2.transform.localPosition = new Vector3(0, 1.45F, 0.15F * 0);

        RenderUtil.swapTextures(
            AqueousEngineeringMod.modDLL,
            mdl.GetComponentsInChildren<Renderer>(),
            "Textures/Machines/BasePillar"
        );
        RenderUtil.swapTextures(
            AqueousEngineeringMod.modDLL,
            mdl2.GetComponentsInChildren<Renderer>(),
            "Textures/Machines/BasePillar"
        );

        var box = go.GetComponentInChildren<BoxCollider>();
        box.size = new Vector3(0.5F, 3F, 0.5F);
        box.transform.localPosition = new Vector3(0, 1.25F, 0);
    }
}

public class BasePillarLogic : CustomMachineLogic {
    private bool recomputedStrength;

    private void Start() {
        SNUtil.log("Reinitializing base pillar");
        AqueousEngineeringMod.pillarBlock.initializeMachine(gameObject);
    }

    protected override float getTickRate() {
        return 0.5F;
    }

    public override void onConstructedChanged(bool finished) {
        triggerRecompute(true);
    }

    protected override void updateEntity(float seconds) {
        triggerRecompute();
    }

    protected override void onAttachToBase() {
        triggerRecompute();
    }

    protected void triggerRecompute(bool force = false) {
        if (recomputedStrength && !force)
            return;
        if (!sub)
            return;
        DIHooks.RecomputeBaseHullStrength(sub.GetComponent<BaseHullStrength>());
        recomputedStrength = true;
    }
}