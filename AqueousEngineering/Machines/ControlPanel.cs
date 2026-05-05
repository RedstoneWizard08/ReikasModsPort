using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class BaseControlPanel : CustomMachine<BaseControlPanelLogic> {
    [SetsRequiredMembers]
    public BaseControlPanel(XMLLocale.LocaleEntry e) : base(
        e.key,
        e.name,
        e.desc, /*"cf522a95-3038-4759-a53c-8dad1242c8ed"*/
        "c5ae1472-0bdc-4203-8418-fb1f74c8edf5"
    ) {
        addIngredient(TechType.WiringKit, 1);
        addIngredient(TechType.Titanium, 1);
    }

    public override bool UnlockedAtStart => true;

    public override bool isOutdoors() {
        return false;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);

        go.removeComponent<StorageContainer>();
        var lgc = go.GetComponent<BaseControlPanelLogic>();

        var mdl = go.setModel(
            "shelve_02",
            ObjectUtil.lookupPrefab("9460942c-2347-4b58-b9ff-0f7f693dc9ff").getChildObject("Starship_work_desk_01")
        );
        mdl.transform.localPosition = new Vector3(0, 0, 0);
        mdl.transform.localScale = Vector3.one;
        mdl.transform.SetParent(go.transform);
        mdl.transform.localRotation = Quaternion.Euler(0, 0, 0);

        go.removeChildObject("collisions");
        var b = go.EnsureComponent<BoxCollider>();
        b.size = new Vector3(2.75F, 1, 1);
        b.center = new Vector3(0.2F, 0, 0);

        var r = mdl.GetComponentInChildren<Renderer>();
        r.transform.localScale = new Vector3(1, 1, 0.1F);
        //SNUtil.dumpTextures(r);
        RenderUtil.swapToModdedTextures(r, this);
        /*
        r.materials[0].SetFloat("_Shininess", 7.5F);
        r.materials[0].SetFloat("_Fresnel", 1F);
        r.materials[0].SetFloat("_SpecInt", 15F);
        lgc.mainRenderer = r;*/

        var c = go.GetComponent<Constructable>();
        c.allowedInBase = true;
        c.allowedInSub = false;
        c.allowedOnCeiling = false;
        c.allowedOnGround = false;
        c.allowedOnConstructables = false;
        c.allowedOnWall = true;
        c.allowedOutside = false;
        c.rotationEnabled = true;
        c.model = mdl;
    }
}

public class BaseControlPanelLogic : CustomMachineLogic {
    private static float[] offsets = [0, -0.125F, 0.33F, 0.5F];

    private HolographicControl.HolographicControlTag[] buttons;

    private float lastButtonValidityCheck = -1;

    private readonly HashSet<string> activeButtons = [];

    private void Start() {
        SNUtil.log("Reinitializing base control panel");
        AqueousEngineeringMod.controlsBlock.initializeMachine(gameObject);
    }

    protected override void load(System.Xml.XmlElement data) {
        activeButtons.Clear();
        foreach (var e in data.getDirectElementsByTagName("activeButton")) {
            activeButtons.Add(e.InnerText);
        }

        SNUtil.log(
            "Loaded control panel with active buttons " + activeButtons.toDebugString(),
            AqueousEngineeringMod.modDLL
        );
        if (buttons != null) {
            foreach (var tag in buttons) {
                tag.setState(activeButtons.Contains(tag.GetComponentInParent<PrefabIdentifier>().ClassId));
            }
        }
    }

    protected override void save(System.Xml.XmlElement data) {
        foreach (var s in activeButtons) {
            var e = data.OwnerDocument.CreateElement("activeButton");
            e.InnerText = s;
            data.AppendChild(e);
        }
    }

    public void addButton(HolographicControl control) {
        var box = gameObject.getChildObject("ButtonHolder");
        if (!box) {
            box = new GameObject("ButtonHolder");
            box.transform.SetParent(transform);
        }

        box.transform.localPosition = Vector3.zero;
        box.transform.localRotation = Quaternion.identity;
        var com = HolographicControl.addButton(box, control);
        updateButtons();
        if (activeButtons.Contains(control.ClassID)) {
            com.setState(true);
            //activeButtons.Remove(control.ClassID);
        }

        SNUtil.log(
            "Added button " + control.ClassID + " to control panel; active: " + com.getState(),
            AqueousEngineeringMod.modDLL
        );
    }

    private void updateButtons() {
        buttons = GetComponentsInChildren<HolographicControl.HolographicControlTag>();
        var offset = -0.4F + buttons.Length * 0.3125F; //0.33F; //-0.125 for 1, 0.33 for 2, 0.5 for 3;
        if (buttons.Length < offsets.Length)
            offset = offsets[buttons.Length];
        for (var i = 0; i < buttons.Length; i++) {
            var tag = buttons[i];
            var f = 2F / buttons.Length * i - offset;
            tag.transform.parent.localPosition = new Vector3(f, 0, 0.1F);
            tag.transform.parent.localScale = new Vector3(2, 2, 1F);
            tag.transform.localRotation = Quaternion.identity;
            tag.transform.parent.localRotation = Quaternion.identity;
        }
    }

    private void SetHolographicControlState(HolographicControl.HolographicControlTag tag) {
        var id = tag.GetComponentInParent<PrefabIdentifier>().ClassId;
        if (tag.getState())
            activeButtons.Add(id);
        else
            activeButtons.Remove(id);
    }

    protected override void updateEntity(float seconds) {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time - lastButtonValidityCheck >= 1 && buttons != null) {
            lastButtonValidityCheck = time;
            var changed = false;
            foreach (var tag in buttons) {
                if (tag && !tag.isStillValid()) {
                    tag.destroy();
                    activeButtons.Remove(tag.GetComponentInParent<PrefabIdentifier>().ClassId);
                    changed = true;
                }
            }

            if (changed)
                updateButtons();
        }
    }
}