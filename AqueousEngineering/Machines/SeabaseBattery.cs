using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class BaseBattery : CustomMachine<BaseBatteryLogic> {
    internal static readonly int
        POWERCELLS = AqueousEngineeringMod.config.getInt(AEConfig.ConfigEntries.BATTCELLS); //2;

    [SetsRequiredMembers]
    public BaseBattery(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "c5ae1472-0bdc-4203-8418-fb1f74c8edf5") {
        addIngredient(TechType.PowerCell, POWERCELLS);
        addIngredient(TechType.WiringKit, 1);
        addIngredient(TechType.Titanium, 4);
    }

    public override bool UnlockedAtStart => false;

    public override bool isOutdoors() {
        return false;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);

        var lgc = go.GetComponent<BaseBatteryLogic>();

        //GameObject mdl = ObjectUtil.lookupPrefab("0f779340-8064-4308-8baa-6be9324a1e05").getChildObject("Starship_tech_box_01_02/Starship_tech_box_01_01");
        var mdl = go.setModel(
            "shelve_02",
            ObjectUtil.lookupPrefab("0f779340-8064-4308-8baa-6be9324a1e05")
                .getChildObject("Starship_tech_box_01_02/Starship_tech_box_01_01")
        );
        //mdl = mdl.clone();
        mdl.transform.localPosition = new Vector3(0, -0.12F, 0.08F);
        mdl.transform.localScale = Vector3.one;
        mdl.transform.SetParent(go.transform);
        mdl.transform.localRotation = Quaternion.Euler(0, 90, 0);

        var r = mdl.GetComponentInChildren<Renderer>();
        //SNUtil.dumpTextures(r);
        RenderUtil.swapToModdedTextures(r, this);
        RenderUtil.setEmissivity(r, 2);
        r.materials[0].SetFloat("_Shininess", 2F);
        r.materials[0].SetFloat("_Fresnel", 0.6F);
        r.materials[0].SetFloat("_SpecInt", 8F);

        var c = go.GetComponent<Constructable>();
        c.model = mdl;
        c.allowedOnCeiling = false;
        c.allowedInSub = false;
        c.allowedInBase = true;
        c.allowedOnConstructables = false;
        c.allowedOnGround = false;
        c.allowedOnWall = true;
        c.allowedOutside = false;
    }
}

public class BaseBatteryLogic : CustomMachineLogic {
    private float storedLastTick;

    private Renderer render;

    private void Start() {
        SNUtil.log("Reinitializing base battery");
        AqueousEngineeringMod.batteryBlock.initializeMachine(gameObject);
    }

    protected override float getTickRate() {
        return 0.5F;
    }

    public override float getBaseEnergyStorageCapacityBonus() {
        return BaseBattery.POWERCELLS * 200;
    }

    protected override void updateEntity(float seconds) {
        if (!render) {
            render = gameObject.GetComponentInChildren<Renderer>();
        }

        if (!sub)
            return;
        var energy = sub.powerRelay.GetPower();
        var frac = energy / sub.powerRelay.GetMaxPower();
        if (energy > storedLastTick + 0.1F) {
            setEmissiveStates(false, frac);
        } else if (energy < storedLastTick - 0.1F) {
            setEmissiveStates(true, frac);
        }

        storedLastTick = energy;
    }

    private void setEmissiveStates(bool draining, float frac) {
        if (!render)
            return;
        //Color c = new Color(draining ? 1 : 0, halfOrMore ? 1 : 0, draining || halfOrMore ? 0 : 1);
        var hue = frac * 120F / 360F;
        var c = Color.HSVToRGB(hue, 1, 1);
        c.b = draining ? 0 : 1;
        render.materials[0].SetColor("_GlowColor", c);
    }
}