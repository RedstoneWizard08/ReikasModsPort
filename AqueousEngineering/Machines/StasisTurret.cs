using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class BaseStasisTurret : CustomMachine<BaseStasisTurretLogic> {
    internal static readonly float POWER_COST = 250F; //per shot
    internal static readonly float POWER_LEVEL = 0.75F;
    internal static readonly float RADIUS = 18F;
    internal static readonly float COOLDOWN = 20F;

    [SetsRequiredMembers]
    public BaseStasisTurret(XMLLocale.LocaleEntry e) : base(
        e.key,
        e.name,
        e.desc,
        "8949b0da-5173-431f-a989-e621af02f942"
    ) {
        //addIngredient(TechType.StasisRifle, 1);
        addIngredient(TechType.AdvancedWiringKit, 1);
        addIngredient(TechType.Polyaniline, 2);
        addIngredient(TechType.TitaniumIngot, 1);
    }

    public override bool UnlockedAtStart => false;

    public override bool isOutdoors() {
        return true;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeComponent<PowerRelay>();
        go.removeComponent<PowerFX>();
        go.removeComponent<PowerSystemPreview>();

        var lgc = go.GetComponent<BaseStasisTurretLogic>();

        var r = go.GetComponentInChildren<Renderer>();
        //SNUtil.dumpTextures(r);
        RenderUtil.swapToModdedTextures(r, this);
        /*
        r.materials[0].SetFloat("_Shininess", 7.5F);
        r.materials[0].SetFloat("_Fresnel", 1F);
        r.materials[0].SetFloat("_SpecInt", 15F);
        lgc.mainRenderer = r;*/

        //go.GetComponent<Constructable>().model = go;
        //go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
        //go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
    }
}

public class BaseStasisTurretLogic : CustomMachineLogic {
    private float lastFire;

    private float lastButtonCheck = -1;

    private GameObject sparker;

    private ParticleSystem[] particles;

    private Renderer mainRenderer;

    private void Start() {
        SNUtil.Log("Reinitializing base stasis turret");
        AqueousEngineeringMod.stasisBlock.initializeMachine(gameObject);
    }

    private void addButton() {
        if (!sub) {
            SNUtil.Log("Could not add button for stasis turret, no sub");
            return;
        }

        foreach (var panel in sub.GetComponentsInChildren<BaseControlPanelLogic>()) {
            panel.addButton(AqueousEngineeringMod.seabaseStasisControl);
        }
    }

    public void fire() {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time - lastFire > BaseStasisTurret.COOLDOWN && consumePower(BaseStasisTurret.POWER_COST)) {
            lastFire = time;
            WorldUtil.createStasisSphere(transform.position, BaseStasisTurret.RADIUS, BaseStasisTurret.POWER_LEVEL);
        }
    }

    protected override void updateEntity(float seconds) {
        if (mainRenderer == null)
            mainRenderer = GetComponentInChildren<Renderer>();
        if (!sparker) {
            sparker = ObjectUtil.createWorldObject("ff8e782e-e6f3-40a6-9837-d5b6dcce92bc");
            sparker.transform.localScale = new Vector3(0.4F, 0.4F, 0.4F);
            sparker.transform.parent = transform;
            //sparker.transform.eulerAngles = new Vector3(325, 180, 0);
            sparker.removeComponent<DamagePlayerInRadius>();
            sparker.removeComponent<PlayerDistanceTracker>();
            //sparker.removeChildObject("ElecLight");
            sparker.transform.localPosition = new Vector3(0, 0.95F, 0);
            foreach (var p in particles) {
                var pm = p.main;
                pm.startSize = 0.4F;
            }
        }

        if (particles == null) {
            particles = sparker.GetComponentsInChildren<ParticleSystem>();
        }

        var active = !GameModeUtils.RequiresPower() || (sub && sub.powerRelay.GetPower() > 0.1F);
        sparker.SetActive(active);
        if (mainRenderer)
            RenderUtil.setEmissivity(mainRenderer, active ? 200 : 0);
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time - lastButtonCheck >= 1) {
            lastButtonCheck = time;
            addButton();

            sparker.transform.localPosition = new Vector3(0, 0.95F, 0);
            foreach (var p in particles) {
                var pm = p.main;
                pm.startSize = 0.4F;
            }
        }
    }
}