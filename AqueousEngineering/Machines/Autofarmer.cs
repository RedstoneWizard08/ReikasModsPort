using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Utility;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class Autofarmer : CustomMachine<AutofarmerLogic> {
    internal static readonly float POWER_COST = 1F;

    [SetsRequiredMembers]
    public Autofarmer(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "f1cde32e-101a-4dd5-8084-8c950b9c2432") {
        addIngredient(TechType.TitaniumIngot, 1);
        addIngredient(TechType.AdvancedWiringKit, 1);
        addIngredient(TechType.VehicleStorageModule, 1);
        addIngredient(TechType.Knife, 1);

        glowIntensity = 2;
    }

    public override bool UnlockedAtStart => false;

    public override bool isOutdoors() {
        return true;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeComponent<Trashcan>();

        var con = go.GetComponentInChildren<StorageContainer>();
        initializeStorageContainer(con, 8, 8);
        con.errorSound = null;

        go.removeChildObject("descent_trashcan_01/descent_trash_01");
        go.removeChildObject("descent_trashcan_01/descent_trash_02");
        go.removeChildObject("descent_trashcan_01/descent_trashcan_interior_01");
        go.removeChildObject("descent_trashcan_01/descent_trashcan_interior_02");

        var lgc = go.GetComponent<AutofarmerLogic>();

        var r = go.GetComponentInChildren<Renderer>();
        RenderUtil.swapToModdedTextures(r, this);
        r.materials[0].SetColor("_Color", Color.white);
        r.materials[0].SetFloat("_Fresnel", 0.8F);
        r.materials[0].SetFloat("_SpecInt", 12F);
        /*
        //SNUtil.dumpTextures(r);
        r.materials[0].SetFloat("_Shininess", 7.5F);
        lgc.mainRenderer = r;*/

        //go.GetComponent<Constructable>().model = go;
        //go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
        //go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
    }
}

public class AutofarmerLogic : CustomMachineLogic {
    private List<Planter> growbeds = [];

    private VFXElectricLine effect;
    private float harvestTime;

    private void Start() {
        SNUtil.Log("Reinitializing base farmer");
        AqueousEngineeringMod.farmerBlock.initializeMachine(gameObject);
    }

    private void OnDisable() {
        effect.destroy();
    }

    protected override float getTickRate() {
        return 5;
    }

    protected override void updateEntity(float seconds) {
        if (!effect) {
            var go = ObjectUtil.lookupPrefab("d11dfcc3-bce7-4870-a112-65a5dab5141b");
            go = go.GetComponent<Gravsphere>().vfxPrefab;
            go = go.clone();
            effect = go.GetComponent<VFXElectricLine>();
            effect.transform.parent = transform;
        }

        if (growbeds.Count == 0) {
            if (sub) {
                var all = sub.GetComponentsInChildren<Planter>();
                foreach (var p in all) {
                    if (p && p.GetContainerType() == ItemsContainerType.WaterPlants &&
                        Vector3.Distance(p.transform.position, transform.position) <= 8) {
                        growbeds.Add(p);
                    }
                }
            }
        }

        if (growbeds.Count > 0 && !storage.container.IsFull() && consumePower(Autofarmer.POWER_COST * seconds)) {
            var p = growbeds[Random.Range(0, growbeds.Count)];
            if (p) {
                tryHarvestFrom(p);
            }
        }

        tickFX();
    }

    private void tickFX() {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time - harvestTime > 5) {
            effect.gameObject.SetActive(false);
        }
    }

    private void tryAllocateFX(GameObject go) {
        effect.gameObject.SetActive(true);
        effect.enabled = true;
        effect.origin = transform.position + Vector3.up * 0.75F;
        effect.target = go.transform.position + Vector3.up * 0.125F;
        harvestTime = DayNightCycle.main.timePassedAsFloat;
    }

    private void tryHarvestFrom(Planter p) {
        var arr = Random.Range(0, 2) == 0 ? p.bigPlantSlots : p.smallPlantSlots;
        var slot = arr[Random.Range(0, arr.Length)];
        if (slot != null && slot.isOccupied) {
            var pt = slot.plantable;
            if (pt && pt.linkedGrownPlant) {
                tryHarvestPlant(p, pt);
            }
        }
    }

    private void tryHarvestPlant(Planter pl, Plantable pt) {
        var p = pt.linkedGrownPlant;
        var tt = CraftData.GetTechType(p.gameObject);
        //SNUtil.log("Try harvest "+p+" : "+tt);
        if (tt != TechType.None) {
            var plant = BasicCustomPlant.getPlant(tt);
            GameObject drop;
            var fp = p.GetComponent<FruitPlant>();
            var custom = plant is CustomHarvestBehavior;
            if (custom) {
                var c = (CustomHarvestBehavior)plant;
                if (!c.canBeAutoharvested())
                    return;
                drop = c.tryHarvest(p.gameObject);
            } else {
                drop = getHarvest(p, tt, fp);
                if (drop)
                    drop = drop.clone();
            }

            //SNUtil.log("drops "+drop);
            if (drop) {
                var td = CraftData.GetTechType(drop);
                if (fp && !custom) {
                    var pp = drop.GetComponent<PickPrefab>();
                    td = pp.pickTech;
                    drop = ObjectUtil.lookupPrefab(td);
                } else if (td is TechType.JellyPlantSeed or TechType.WhiteMushroomSpore or TechType.AcidMushroomSpore) {
                    td = tt;
                    drop = ObjectUtil.lookupPrefab(tt).clone();
                }

                //SNUtil.log("DT "+td+" > "+drop);
                drop.SetActive(false);
                var ppb = drop.GetComponent<Pickupable>();
                if (!ppb) {
                    ppb = ObjectUtil.createWorldObject(td).GetComponent<Pickupable>();
                }

                //SNUtil.log(""+ppb);
                if (ppb && storage.container.AddItem(ppb) != null) {
                    var ass = SoundManager.buildSound(
                        TechData.GetSoundPickup(td) ?? TechData.defaultSoundPickup
                    );
                    if (ass != null) {
                        SoundManager.playSoundAt(ass, gameObject.transform.position);
                    }

                    if (fp && !custom) {
                        var pp = drop.GetComponent<PickPrefab>();
                        //SNUtil.log("fp pp "+pp);
                        if (pp)
                            pp.SetPickedUp();
                    } else if (td is TechType.JellyPlant or TechType.WhiteMushroom or TechType.AcidMushroom) {
                        //pl.ReplaceItem(pt, drop.GetComponent<Plantable>());
                    }

                    //SNUtil.log("fx "+p);
                    if (p)
                        tryAllocateFX(p.gameObject);
                    else
                        tryAllocateFX(fp.fruits[0].gameObject);
                } else if (ppb) {
                    ppb.gameObject.destroy(false);
                }
            }
        }
    }

    private GameObject getHarvest(GrownPlant p, TechType tt, FruitPlant fp) {
        if (fp) {
            var pp = fp.fruits[Random.Range(0, fp.fruits.Length)];
            return pp && pp.isActiveAndEnabled ? pp.gameObject : null;
        }

        switch (tt) { /*
            case TechType.BloodVine:
                return TechType.BloodOil;
            case TechType.Creepvine:
                return  ? TechType.CreepvineSeedCluster : TechType.CreepvinePiece;*/
            default:
                return TechData.GetHarvestOutput(tt) != null
                    ? ObjectUtil.lookupPrefab(TechData.GetHarvestOutput(tt))
                    : null;
        }
    }
}
/*
class HarvestData {

    private readonly TechType plant;
    private readonly TechType drop;

    private GameObject createDroppedItem(GrownPlant p, TechType tt) {

    }

}*/