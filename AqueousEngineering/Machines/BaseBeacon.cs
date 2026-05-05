using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class BaseBeacon : CustomMachine<BaseBeaconLogic> {
    private readonly SignalManager.ModSignal signal;

    [SetsRequiredMembers]
    public BaseBeacon(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
        addIngredient(TechType.MapRoomUpgradeScanRange, 1);
        addIngredient(TechType.Beacon, 1);
        addIngredient(TechType.LEDLight, 1);

        signal = SignalManager.createSignal("BaseBeacon", e.name, e.desc, "", "");
        signal.register(
            null,
            TextureManager.getSprite(AqueousEngineeringMod.modDLL, "Textures/base-beacon-marker"),
            Vector3.zero
        );
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

        var lgc = go.GetComponent<BaseBeaconLogic>();

        var r = go.GetComponentInChildren<Renderer>();
        //SNUtil.dumpTextures(r);
        RenderUtil.swapToModdedTextures(r, this); /*
        r.materials[0].SetFloat("_Shininess", 7.5F);
        r.materials[0].SetFloat("_Fresnel", 1F);
        r.materials[0].SetFloat("_SpecInt", 15F);
        lgc.mainRenderer = r;*/

        //go.GetComponent<Constructable>().model = go;
        //go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
        //go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);

        var b = go.EnsureComponent<Beacon>();
        b.pickupable =
            null; //this will cause the beacon to throw an exception, but this is actually good because it skips the rest of the Awake()
        b.beaconActiveState = true;

        var ping = go.EnsureComponent<PingInstance>();
        ping.pingType = signal.signalType; //PingType.Beacon;
        EnumHandler.AddEntry<PingType>("").WithIcon(null);
        //ping.displayPingInManager = false;
        ping.colorIndex = 0;
        ping.origin = go.transform;
        ping.minDist = 18f;
        // ping.maxDist = 1;
    }
}

public class BaseBeaconLogic : CustomMachineLogic {
    private Beacon beacon;
    private PingInstance ping;

    private string vehicleString = "";

    private Renderer effect;

    private int colorIndexToUse = -1;

    private void Start() {
        SNUtil.log("Reinitializing base beacon");
        AqueousEngineeringMod.beaconBlock.initializeMachine(gameObject);
    }

    protected override float getTickRate() {
        return 0.5F;
    }

    protected override void load(XmlElement data) {
        colorIndexToUse = data.getInt("color", -1);
    }

    protected override void save(XmlElement data) {
        if (ping)
            data.addProperty("color", ping.colorIndex);
    }

    protected override void updateEntity(float seconds) {
        if (!effect) {
            var go = ObjectUtil.createWorldObject("d11dfcc3-bce7-4870-a112-65a5dab5141b", true, false);
            go.SetActive(false);
            effect = go.GetComponent<Gravsphere>().pads[0].vfxHaloRenderer.clone();
            effect.transform.parent = transform;
            effect.GetComponent<FollowTransform>().parent = transform;
            effect.gameObject.SetActive(true);
        }

        if (!beacon) {
            beacon = gameObject.GetComponent<Beacon>();
            beacon.SetBeaconActiveState(false);
            ping = gameObject.GetComponent<PingInstance>();
        }

        if (beacon) {
            //if (beacon.beaconLabel && beacon.beaconLabel.pingInstance)
            //	colorIndex = beacon.beaconLabel.pingInstance.colorIndex;
            if (colorIndexToUse >= 0) {
                ping.colorIndex = colorIndexToUse;
                colorIndexToUse = -1;
                PingManager.NotifyColor(ping);
            }

            if (sub) {
                List<Vehicle> docked = [];
                var docks = sub.gameObject.GetComponentsInChildren<VehicleDockingBay>();
                if (docks.Length == 0) {
                    vehicleString = "";
                } else {
                    foreach (var dock in docks) {
                        var v = dock.dockedVehicle;
                        if (v)
                            docked.Add(v);
                    }

                    vehicleString = docked.Count == 0
                        ? "No docked vehicles"
                        : "Docked Vehicles: " + string.Join(", ", docked.Select(v => v.GetName()));
                }

                beacon.label = generateBeaconLabel(sub);
                ping.SetLabel(beacon.label);
            }
        }
    }

    private string generateBeaconLabel(SubRoot sub) {
        var loc = "Location: " + WorldUtil.getRegionalDescription(transform.position, true);
        var pw = "Power: " + sub.powerRelay.GetPower().ToString("0.0") + "/" + sub.powerRelay.GetMaxPower() + " (" +
                 sub.powerRelay.powerStatus + ")";
        var ret = loc + "\n" + pw;
        if (!string.IsNullOrEmpty(vehicleString))
            ret = ret + "\n" + vehicleString;
        return ret;
    }
}