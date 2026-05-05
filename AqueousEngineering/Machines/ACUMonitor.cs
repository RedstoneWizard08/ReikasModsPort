using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class ACUMonitor : CustomMachine<ACUMonitorLogic>, MultiTexturePrefab {
    internal XMLLocale.LocaleEntry locale;

    [SetsRequiredMembers]
    public ACUMonitor(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "5c06baec-0539-4f26-817d-78443548cc52") {
        locale = e;
        addIngredient(TechType.Titanium, 2);
        addIngredient(TechType.Quartz, 2);
        addIngredient(TechType.CopperWire, 1);
    }

    public override bool UnlockedAtStart => false;

    public override bool isOutdoors() {
        return false;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeComponent<Radio>();
        go.removeChildObject("xFlare");

        var mdl = go.setModel("Mesh", ObjectUtil.lookupPrefab("b460a6a6-2a05-472c-b4bf-c76ae49d9a29"));

        var lgc = go.GetComponent<ACUMonitorLogic>();

        var c = go.GetComponent<Constructable>();
        c.model = mdl;
        c.allowedOnCeiling = false;
        c.allowedOnGround = false;
        c.allowedOnWall = true;
        c.allowedOnConstructables = true;
        c.allowedOutside = false;

        foreach (var r in go.GetComponentsInChildren<Renderer>()) {
            RenderUtil.swapToModdedTextures(r, this);
            r.materials[0].SetColor("_Color", new Color(0.5F, 0.5F, 0.5F, 1));
            //RenderUtil.setGlossiness(r, );
        }

        go.GetComponent<Constructable>().model = mdl;
        //go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
        //go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
    }

    public Dictionary<int, string> getTextureLayers(Renderer r) {
        return new Dictionary<int, string> { { 0, "Base" }, { 1, "Screen" } };
    }
}

public class ACUMonitorLogic : CustomMachineLogic, IHandTarget {
    private WaterPark connectedACU;

    //internal GameObject rotator;

    private void Start() {
        SNUtil.log("Reinitializing acu monitor");
        AqueousEngineeringMod.acuMonitorBlock.initializeMachine(gameObject);
    }

    protected override float getTickRate() {
        return 0;
    }

    private WaterPark tryFindACU() {
        if (!sub)
            return null;
        foreach (var wp in sub.GetComponentsInChildren<WaterPark>()) {
            if (Vector3.Distance(wp.transform.position, transform.position) <= 6) {
                return wp;
            }
        }

        return null;
    }

    protected override void updateEntity(float seconds) {
        if (!connectedACU) {
            connectedACU = tryFindACU();
        }
    }

    public void OnHandHover(GUIHand hand) {
        if (connectedACU) {
            HandReticle.main.SetInteractText(AqueousEngineeringMod.acuMonitorBlock.locale.getString("tooltip"), false);
            HandReticle.main.SetIcon(HandReticle.IconType.Interact);
        } else {
            HandReticle.main.SetInteractText(AqueousEngineeringMod.acuMonitorBlock.locale.getString("noacu"), false);
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny);
        }
    }

    public void OnHandClick(GUIHand hand) {
        if (connectedACU) {
            var call = connectedACU.GetComponent<AcuCallbackSystem.AcuCallback>();
            if (call)
                call.PrintTerminalInfo();
            else
                SNUtil.writeToChat("ACU is in an invalid state.");
        }
    }
}