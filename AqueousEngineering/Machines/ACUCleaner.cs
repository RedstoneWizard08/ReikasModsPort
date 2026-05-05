using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class ACUCleaner : CustomMachine<ACUCleanerLogic> {
    internal static readonly float POWER_COST = 0.15F;

    [SetsRequiredMembers]
    public ACUCleaner(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "5fc7744b-5a2c-4572-8e53-eebf990de434") {
        addIngredient(TechType.Titanium, 5);
        addIngredient(TechType.FiberMesh, 2);
        addIngredient(TechType.ComputerChip, 1);
    }

    public override bool UnlockedAtStart => false;

    public override bool isOutdoors() {
        return false;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeChildObject("Label");

        var mdl = go.setModel("model", ObjectUtil.lookupPrefab("c0175cf7-0b6a-4a1d-938f-dad0dbb6fa06"));
        mdl.transform.localScale = Vector3.one * 0.67F;

        var con = go.GetComponentInChildren<StorageContainer>();
        initializeStorageContainer(con, 3, 5);

        var lgc = go.GetComponent<ACUCleanerLogic>();

        //GameObject air = ObjectUtil.lookupPrefab("7b4b90b8-6294-4354-9ebb-3e5aa49ae453");
        //GameObject mdl = go.setModel("discovery_trashcan_01_d", air.getChildObject("model"));
        //lgc.rotator = ObjectUtil.getChildObject(air, "model").getChildObject("_pipes_floating_air_intake_turbine_geo").clone();
        //lgc.rotator.transform.parent = go.transform;

        var c = go.GetComponent<Constructable>();
        c.model = mdl;
        c.allowedOnCeiling = false;
        c.allowedOnGround = false;
        c.allowedOnWall = true;
        c.allowedOnConstructables = true;
        c.allowedOutside = false;

        var r = go.GetComponentInChildren<Renderer>();
        //SNUtil.dumpTextures(r);
        RenderUtil.swapToModdedTextures(r, this);
        r.materials[0].SetFloat("_Shininess", 2.5F);
        r.materials[0].SetFloat("_Fresnel", 0.8F);
        r.materials[0].SetFloat("_SpecInt", 2.5F);

        //go.GetComponent<Constructable>().model = go;
        //go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
        //go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
    }
}

public class ACUCleanerLogic : CustomMachineLogic {
    private AcuCallbackSystem.AcuCallback connectedACU;

    //internal GameObject rotator;

    private void Start() {
        SNUtil.log("Reinitializing acu cleaner");
        AqueousEngineeringMod.acuCleanerBlock.initializeMachine(gameObject);
    }

    protected override float getTickRate() {
        return 2;
    }

    private AcuCallbackSystem.AcuCallback tryFindACU() {
        if (!sub)
            return null;
        foreach (var wp in sub.GetComponentsInChildren<WaterPark>()) {
            if (Vector3.Distance(wp.transform.position, transform.position) <= 6) {
                return wp.GetComponent<AcuCallbackSystem.AcuCallback>();
            }
        }

        return null;
    }

    /*
    private StorageContainer tryFindStorage() {
        SubRoot sub = getSub();
        if (!sub) {
            return null;
        }
        foreach (StorageContainer wp in sub.GetComponentsInChildren<StorageContainer>()) {
            if (Vector3.Distance(wp.transform.position, transform.position) <= 3 && !wp.GetComponent<WaterPark>() && wp.container.GetContainerType() == ItemsContainerType.Default) {
                return wp;
            }
        }
        return null;
    }
    */
    protected override void updateEntity(float seconds) {
        if (!connectedACU) {
            connectedACU = tryFindACU();
        }

        if (connectedACU && consumePower(ACUCleaner.POWER_COST * seconds)) {
            //rotator.transform.position = connectedACU.transform.position+Vector3.down*1.45F;
            //rotator.transform.localScale = new Vector3(13.8F, 1, 13.8F);
            foreach (var wp in connectedACU.Acu.items) {
                if (wp) {
                    var pp = wp.GetComponent<Pickupable>();
                    var tt = pp.GetTechType();
                    if (tt == TechType.SeaTreaderPoop || tt == AqueousEngineeringMod.poo.TechType ||
                        tt == TechType.StalkerTooth) {
                        var ii = storage.container.AddItem(pp);
                        if (ii != null) {
                            connectedACU.Acu.RemoveItem(pp);
                            pp.PlayPickupSound();
                            pp.gameObject.SetActive(false);
                            break;
                        }
                    }
                }
            }

            if (connectedACU.GasopodCount > 0 && connectedACU.ConsistentBiome && connectedACU.IsHealthy() &&
                connectedACU.CurrentTheme == BiomeRegions.Shallows) {
                var ch = connectedACU.GasopodCount * seconds * 0.0008F; //0.08%/s per gasopod
                if (Random.Range(0F, 1F) <= ch) {
                    var go = ObjectUtil.createWorldObject(TechType.GasPod);
                    storage.container.AddItem(go.GetComponent<Pickupable>());
                    go.SetActive(false);
                }
            }
        }
    }
}