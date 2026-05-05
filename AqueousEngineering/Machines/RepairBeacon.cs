using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class RepairBeacon : CustomMachine<RepairBeaconLogic> {
    internal static readonly float POWER_COST = 0.2F; //per second
    internal static readonly float POWER_COST_ACTIVE = 10.0F; //per second

    [SetsRequiredMembers]
    public RepairBeacon(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
        addIngredient(TechType.AdvancedWiringKit, 1);
        addIngredient(TechType.TitaniumIngot, 1);
        addIngredient(TechType.CyclopsSeamothRepairModule, 1);
    }

    public override bool UnlockedAtStart => false;

    protected override bool shouldDeleteFragments() {
        return false;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeComponent<PowerRelay>();
        go.removeComponent<PowerFX>();
        go.removeComponent<PowerSystemPreview>();

        var lgc = go.GetComponent<RepairBeaconLogic>();

        var r = go.GetComponentInChildren<Renderer>(); /*
        //SNUtil.dumpTextures(r);
        RenderUtil.swapToModdedTextures(r, this);
        r.materials[0].SetFloat("_Shininess", 7.5F);
        r.materials[0].SetFloat("_Fresnel", 1F);
        r.materials[0].SetFloat("_SpecInt", 15F);
        lgc.mainRenderer = r;*/

        var c = go.GetComponent<Constructable>();
        c.allowedInBase = true;
        c.allowedInSub = true;
        c.allowedOnCeiling = true;
        c.allowedOnWall = true;
        c.allowedOnConstructables = false;
        c.allowedOutside = true;

        //go.GetComponent<Constructable>().model = go;
        //go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
        //go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
    }
}

public class RepairBeaconLogic : CustomMachineLogic {
    internal static readonly SoundManager.SoundData workingSound = SoundManager.registerSound(
        AqueousEngineeringMod.modDLL,
        "nanite",
        "Sounds/nanite.ogg",
        SoundManager.soundMode3D
    );

    private LiveMixin[] live;

    private float lastSound = -1;

    private void Start() {
        SNUtil.log("Reinitializing base repair beacon");
        AqueousEngineeringMod.repairBlock.initializeMachine(gameObject);
    }

    protected override void updateEntity(float seconds) {
        if (sub && live == null)
            live = sub.GetComponentsInChildren<LiveMixin>().Where(lv => !lv.GetComponent<Vehicle>()).ToArray();
        if (sub && GameModeUtils.RequiresReinforcements() && canHeal(sub) && live != null && live.Length > 0 &&
            consumePower(RepairBeacon.POWER_COST * seconds)) {
            var lv = live.GetRandom();
            if (lv && lv.health < lv.maxHealth &&
                consumePower((RepairBeacon.POWER_COST_ACTIVE - RepairBeacon.POWER_COST) * seconds)) {
                lv.AddHealth(seconds * 12);

                if (DayNightCycle.main.timePassedAsFloat - lastSound >= 1.25F) {
                    lastSound = DayNightCycle.main.timePassedAsFloat;
                    SoundManager.playSoundAt(workingSound, transform.position);
                }
            }
        }
    }

    private bool canHeal(SubRoot sub) {
        return !(sub is BaseRoot root) || root.GetComponentInChildren<BaseHullStrength>().totalStrength > 0;
    }
}