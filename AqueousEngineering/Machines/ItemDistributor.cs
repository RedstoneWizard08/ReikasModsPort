using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class ItemDistributor : CustomMachine<ItemDistributorLogic> {
    [SetsRequiredMembers]
    public ItemDistributor(XMLLocale.LocaleEntry e) : base(
        e.key,
        e.name,
        e.desc,
        "5fc7744b-5a2c-4572-8e53-eebf990de434"
    ) {
        addIngredient(TechType.ComputerChip, 2);
        addIngredient(TechType.Titanium, 2);
    }

    public override bool UnlockedAtStart => false;

    public override bool isOutdoors() {
        return false;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeChildObject("Label");

        var lgc = go.GetComponent<ItemDistributorLogic>();

        var con = go.GetComponentInChildren<StorageContainer>();
        initializeStorageContainer(con, 5, 5);

        var mdl = go.setModel("model", ObjectUtil.lookupPrefab("c5ae1472-0bdc-4203-8418-fb1f74c8edf5"));
        mdl.transform.localScale = new Vector3(1, 2, 1);

        var c = go.GetComponent<Constructable>();
        c.model = mdl;
        c.allowedOnCeiling = false;
        c.allowedOnGround = false;
        c.allowedOnWall = true;
        c.allowedOnConstructables = false;
        c.allowedOutside = true;

        var r = mdl.GetComponentInChildren<Renderer>();
    }
}

public class ItemDistributorLogic : CustomMachineLogic {
    private readonly Dictionary<TechType, List<StorageContainer>> targets = new();

    private void Start() {
        SNUtil.log("Reinitializing base item distributor");
        //AqueousEngineeringMod.ionCubeBlock.initializeMachine(gameObject);
    }

    protected override float getTickRate() {
        return 0.5F;
    }

    protected override void updateEntity(float seconds) {
        if (sub && storage) {
        }
    }

    public void rebuildStorages() {
        targets.Clear();
        if (!sub)
            return;
        foreach (var sc in sub.GetComponentsInChildren<StorageContainer>()) {
            addStorage(sc);
        }
    }

    public void removeStorage(StorageContainer sc) {
        foreach (var li in targets.Values) {
            li.Remove(sc);
        }
    }

    public void addStorage(StorageContainer sc) {
        var ie = getRelevantTypes(sc);
        if (ie == null)
            return;
        foreach (var tt in ie) {
            if (!targets.ContainsKey(tt)) {
                targets[tt] = [];
            }

            targets[tt].Add(sc);
        }
    }

    private IEnumerable<TechType> getRelevantTypes(StorageContainer sc) {
        if (SNUtil.match(sc.GetComponent<PrefabIdentifier>(), "5fc7744b-5a2c-4572-8e53-eebf990de434")) { //small locker
            var lbl = sc.gameObject.getChildObject("Label");
            var text = lbl.GetComponent<uGUI_SignInput>().inputField.text;
        }

        return sc.GetComponent<CyclopsLocker>() || sc.GetComponent<RocketLocker>()
            ? sc.container.GetItemTypes()
            : (IEnumerable<TechType>)null;
    }
}