using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class ATPTap : CustomMachine<ATPTapLogic> {
    [SetsRequiredMembers]
    public ATPTap(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "a620b5d5-b413-4627-84b0-1e3a7c6bf5b6") {
        addIngredient(TechType.PrecursorIonCrystal, 4);
        addIngredient(TechType.AdvancedWiringKit, 1);
    }

    public override bool UnlockedAtStart => false;

    public override bool isOutdoors() {
        return true;
    }

    protected override bool isPowerGenerator() {
        return true;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);

        var lgc = go.GetComponent<ATPTapLogic>();

        go.GetComponent<PowerRelay>().maxOutboundDistance = 20;
        go.GetComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;

        var r = go.GetComponentInChildren<Renderer>();
        //SNUtil.dumpTextures(r);
        RenderUtil.swapToModdedTextures(r, this);
        RenderUtil.setEmissivity(r, 2);
        //r.materials[0].SetFloat("_Shininess", 2F);
        //r.materials[0].SetFloat("_Fresnel", 0.6F);
        //r.materials[0].SetFloat("_SpecInt", 8F);

        var c = go.GetComponent<Constructable>();
        c.allowedOnWall = true;
        c.allowedOutside = true;
        c.allowedOnCeiling = true;
        c.allowedOnGround = true;
        c.allowedOnConstructables = true;
        c.forceUpright = false;

        go.removeChildObject("model/root/head");
        go.removeChildObject("UI/Canvas/temperatureBar");
    }
}

public class ATPTapLogic : CustomMachineLogic {
    internal static readonly SoundManager.SoundData workingSound = SoundManager.registerSound(
        AqueousEngineeringMod.modDLL,
        "atptap",
        "Sounds/atptap.ogg",
        SoundManager.soundMode3D
    );

    private GameObject powerSource;

    private ThermalPlant thermalComponent;

    private Renderer render;

    private float lastSound = -1;

    private static readonly HashSet<string> validObjects = [
        "31f84eba-d435-438c-a58e-f3f7bae8bfbd",
        "69cd7462-7cd2-456c-bfff-50903c391737",
        "94933bb3-0587-4e8d-a38d-b7ec4c859b1a",
        "37f07c77-ac44-4246-9f53-1d186fb99921",
        "2334eec8-0968-4e0f-8441-25e0f76fc6b6",

        //sanctuaries
        "640f57a6-6436-4132-a9bb-d914f3e19ef5", //pillars with light column, used as spotlights
    ];

    public static bool isValidSourceObject(GameObject go) {
        var pi = go.FindAncestor<PrefabIdentifier>();
        return pi && validObjects.Contains(pi.ClassId);
    }

    private static readonly Vector3 drfLocation = new(-248, -800, 281);

    private void Start() {
        SNUtil.Log("Reinitializing ATP tap");
        AqueousEngineeringMod.atpTapBlock.initializeMachine(gameObject);
        InvokeRepeating(nameof(tryFindCable), Random.value, 4f);
        InvokeRepeating(nameof(AddPower), Random.value, 1f);
    }

    protected override bool needsAttachedBase() {
        return false;
    }

    protected override float getTickRate() {
        return 0.5F;
    }

    public override float getBaseEnergyStorageCapacityBonus() {
        return 0; //100;
    }

    protected override void updateEntity(float seconds) {
        if (!render) {
            render = gameObject.GetComponentInChildren<Renderer>();
        }

        if (!thermalComponent)
            thermalComponent = GetComponent<ThermalPlant>();
        thermalComponent.enabled = false;
        thermalComponent.CancelInvoke();

        if (powerSource && buildable.constructed && DayNightCycle.main.timePassedAsFloat - lastSound >= 6.2F) {
            lastSound = DayNightCycle.main.timePassedAsFloat;
            SoundManager.playSoundAt(workingSound, transform.position);
        }
        /*
        if (!cableObject) {
            cableObject = tryFindCable();
        }*/
    }

    private void tryFindCable() {
        powerSource = null;
        if (Vector3.Distance(transform.position, drfLocation) <= 200) {
            return; //those cables are dead
        }

        powerSource = WorldUtil.areAnyObjectsNear(transform.position, 4, isValidCable);
        setEmissiveStates((bool)powerSource);
    }

    private bool isValidCable(GameObject go) {
        var pt = go.GetComponent<PrecursorTeleporter>();
        if (pt)
            return pt.isOpen;
        var pi = go.GetComponent<PrefabIdentifier>();
        return pi && validObjects.Contains(pi.classId);
    }

    private void AddPower() {
        if (powerSource && buildable.constructed) {
            DIHooks.AddPowerToSeabaseDelegate(
                thermalComponent.powerSource,
                AqueousEngineeringMod.config.getInt(AEConfig.ConfigEntries.ATPTAPRATE),
                out var trash,
                this
            );
            //thermalComponent.powerSource.AddEnergy(AqueousEngineeringMod.config.getInt(AEConfig.ConfigEntries.ATPTAPRATE), out float trash);
        }
    }

    private void setEmissiveStates(bool working) {
        if (!render)
            return;
        var c = working ? Color.green : Color.red;
        render.materials[0].SetColor("_GlowColor", c);
        thermalComponent.temperatureText.text = working ? "\u2713" : "\u26A0";
        thermalComponent.temperatureText.color = c;
        thermalComponent.temperatureText.transform.localScale = Vector3.one * 2.5F;
    }
}