using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class ItemCollectorCyclopsTether : CustomMachine<ItemCollectorCyclopsTetherLogic> {
    [SetsRequiredMembers]
    public ItemCollectorCyclopsTether(XMLLocale.LocaleEntry e) : base(
        e.key,
        e.name,
        e.desc,
        "8949b0da-5173-431f-a989-e621af02f942"
    ) {
        addIngredient(TechType.Titanium, 2);
        addIngredient(TechType.CopperWire, 1);
        addIngredient(TechType.Magnetite, 4);
    }

    public override bool UnlockedAtStart => false;

    public override bool isOutdoors() {
        return false;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeComponent<PowerRelay>();
        go.removeComponent<PowerSystemPreview>();

        var lgc = go.GetComponent<ItemCollectorCyclopsTetherLogic>();

        var c = go.GetComponent<Constructable>();
        c.allowedOnWall = false;
        c.allowedOutside = false;
        c.allowedInBase = false;
        c.allowedOnGround = true;
        c.allowedOnConstructables = false;
        c.allowedOnCeiling = false;
        c.allowedInSub = true;
        c.forceUpright = true;

        var r = go.GetComponentInChildren<Renderer>(); /*
        //SNUtil.dumpTextures(r);
        RenderUtil.swapToModdedTextures(r, this);
        r.materials[0].SetFloat("_Shininess", 7.5F);
        r.materials[0].SetFloat("_Fresnel", 1F);
        r.materials[0].SetFloat("_SpecInt", 15F);
        lgc.mainRenderer = r;*/

        //go.GetComponent<Constructable>().model = go;
        //go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
        //go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
    }
}

public class ItemCollectorCyclopsTetherLogic : CustomMachineLogic {
    internal PowerFX lineRenderer;

    public GameObject itemCollector;

    private void Start() {
        SNUtil.log("Reinitializing cyclops item collector tether");
        AqueousEngineeringMod.collectorTetherBlock.initializeMachine(gameObject);
    }

    protected override void updateEntity(float seconds) {
        if (!lineRenderer) {
            lineRenderer = GetComponent<PowerFX>();
        }

        lineRenderer.SetTarget(itemCollector ? itemCollector.gameObject : null);
    }
}