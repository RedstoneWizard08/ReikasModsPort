using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

[Obsolete]
public class IncubatorInjector : CustomMachine<IncubatorInjectorLogic>, MultiTexturePrefab {
    static IncubatorInjector() {
    }

    [SetsRequiredMembers]
    public IncubatorInjector(XMLLocale.LocaleEntry e) : base(
        e.key,
        e.name,
        e.desc,
        "bedc40fb-bd97-4b4d-a943-d39360c9c7bd"
    ) {
    }

    public override bool UnlockedAtStart => false;

    public override bool isOutdoors() {
        return true;
    }

    public Dictionary<int, string> getTextureLayers(Renderer r) {
        return new Dictionary<int, string> { { 0, "" }, { 1, "" } };
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeComponent<Trashcan>();
        go.removeChildObject("Bubbles");

        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;

        var con = go.GetComponentInChildren<StorageContainer>();
        con.hoverText = "Collect Filtrate";
        con.storageLabel = "FILTRATE";
        con.enabled = true;
        con.Resize(5, 2);
        //con.prefabRoot = go;
        var lgc = go.GetComponent<GeyserFilterLogic>();
        //lgc.storage = con;

        var mdl = go.setModel(
            "discovery_trashcan_01_d",
            ObjectUtil.lookupPrefab("8fb8a082-d40a-4473-99ec-1ded36cc6813").getChildObject("Starship_cargo")
        );
        mdl.transform.localRotation = Quaternion.Euler(0, 0, 0);
        mdl.transform.localPosition = new Vector3(0, -0.05F, -2.25F);
        float w = 4; //2.5F;
        var t = 0.4F; //0.125F;
        mdl.transform.localScale = new Vector3(w, t, w);
        var c = go.GetComponent<Constructable>();
        c.model = mdl;
        c.allowedOnCeiling = false;
        c.allowedOnGround = true;
        c.allowedOnWall = false;
        c.allowedOnConstructables = false;
        c.allowedInBase = false;
        c.allowedInSub = false;
        c.allowedOutside = true;
        c.forceUpright = true;

        var mdl2 = mdl.clone();
        mdl2.transform.SetParent(mdl.transform.parent);
        mdl2.transform.localRotation = Quaternion.Euler(180, 0, 0);
        mdl2.transform.localPosition = new Vector3(0, -(0.05F + 0.18F * t / 0.125F), 2.25F);
        mdl2.transform.localScale = new Vector3(w, t, w);

        var box = go.GetComponentInChildren<BoxCollider>();
        box.size = new Vector3(w, t * 2, w);
        box.center = Vector3.down * t * 1.5F;

        var r = mdl.GetComponentsInChildren<Renderer>();
        RenderUtil.swapToModdedTextures(r, this);
        foreach (var rr in r)
            RenderUtil.setEmissivity(rr, 1);
        r = mdl2.GetComponentsInChildren<Renderer>();
        RenderUtil.swapToModdedTextures(r, this);
        foreach (var rr in r)
            RenderUtil.setEmissivity(rr, 1);

        //go.EnsureComponent<PowerFX>().vfxPrefab = ObjectUtil.lookupPrefab(TechType.PowerTransmitter).GetComponent<PowerFX>().vfxPrefab;
    }
}

public class IncubatorInjectorLogic : CustomMachineLogic, IHandTarget {
    internal Renderer[] mainRenderers;

    private IncubatorComputerTerminal terminal;

    private void Start() {
        SNUtil.Log("Reinitializing incubator injector");
        //C2CItems.incubatorInjector.initializeMachine(gameObject);
    }

    protected override void load(System.Xml.XmlElement data) {
    }

    protected override void save(System.Xml.XmlElement data) {
    }

    protected override void updateEntity(float seconds) {
        if (mainRenderers == null)
            mainRenderers = GetComponentsInChildren<Renderer>();
        if (seconds <= 0)
            return;
        if ((Player.main.transform.position - transform.position).sqrMagnitude >= 2500)
            return;
        var time = DayNightCycle.main.timePassedAsFloat;
        if (DIHooks.GetWorldAge() > 0.5F && !terminal) {
            terminal = findTerminal();
        }

        if (!storage) {
            return;
        }

        storage.hoverText = "Add plant materials";
    }

    public IncubatorComputerTerminal findTerminal() {
        foreach (var g in WorldUtil.getObjectsNearWithComponent<IncubatorComputerTerminal>(transform.position, 50)) {
            if (g)
                return g;
        }

        return null;
    }

    public void OnHandHover(GUIHand hand) {
        //if (!StoryGoalManager.main.completedGoals.Contains()) {
        //  	
        //}
        /*
        if (!SeaToSeaMod.enviroSimulation.isUnlocked()) {
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
            HandReticle.main.SetInteractText(C2CHooks.incubatorNoEnviroLocaleKey);
            HandReticle.main.SetTargetDistance(8);
            return;
        }
        HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
        HandReticle.main.SetInteractText(C2CHooks.incubatorClickLocaleKey);
        HandReticle.main.SetTargetDistance(8);*/
    }

    public void OnHandClick(GUIHand hand) {
        //if (SeaToSeaMod.enviroSimulation.isUnlocked()) {
        //	storage.Open();
        //}
    }
}