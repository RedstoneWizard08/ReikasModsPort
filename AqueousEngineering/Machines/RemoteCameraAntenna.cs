using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class RemoteCameraAntenna : CustomMachine<RemoteCameraAntennaLogic> {
    internal static readonly float POWER_COST = 0.05F; //per second
    internal static readonly float POWER_COST_ACTIVE = 1.0F; //per second

    [SetsRequiredMembers]
    public RemoteCameraAntenna(XMLLocale.LocaleEntry e) : base(
        e.key,
        e.name,
        e.desc,
        "8949b0da-5173-431f-a989-e621af02f942"
    ) {
        addIngredient(TechType.Gold, 4);
        addIngredient(TechType.Beacon, 1);
        addIngredient(TechType.CopperWire, 3);
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

        var lgc = go.GetComponent<RemoteCameraAntennaLogic>();

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

public class RemoteCameraAntennaLogic : CustomMachineLogic {
    private MapRoomFunctionality scanner;

    private bool ready;

    private void Start() {
        SNUtil.log("Reinitializing base camera antenna");
        AqueousEngineeringMod.cameraAntennaBlock.initializeMachine(gameObject);
    }

    protected override void updateEntity(float seconds) {
        if (!scanner) {
            scanner = sub ? sub.gameObject.GetComponentInChildren<MapRoomFunctionality>() : null;
        }

        ready = scanner && consumePower(RemoteCameraAntenna.POWER_COST * seconds);
    }

    public bool isReady() {
        return ready;
    }
}