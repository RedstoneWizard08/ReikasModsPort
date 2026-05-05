using System;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

[Obsolete]
public class IonCubeBurner : CustomMachine<IonCubeBurnerLogic> {
    internal static readonly float POWER_RATE = 40F; //PPS
    internal static readonly float CUBE_VALUE = 300000; //total

    [SetsRequiredMembers]
    public IonCubeBurner(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "??") {
        addIngredient(TechType.AdvancedWiringKit, 2);
        addIngredient(TechType.Polyaniline, 1);
        addIngredient(TechType.EnameledGlass, 3);

        glowIntensity = 1;
    }

    public override bool UnlockedAtStart => false;

    public override bool isOutdoors() {
        return false;
    }

    protected override bool isPowerGenerator() {
        return true;
    }

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeComponent<Trashcan>();

        var lgc = go.GetComponent<IonCubeBurnerLogic>();

        var con = go.GetComponentInChildren<StorageContainer>();
        initializeStorageContainer(con, 2, 2);

        var mdl = go.setModel(
            "bedc40fb-bd97-4b4d-a943-d39360c9c7bd",
            ObjectUtil.lookupPrefab("??").getChildObject("??")
        );
        mdl.transform.localRotation = Quaternion.Euler(-90, 180, 0);
        mdl.transform.localPosition = new Vector3(0, -0.05F, 0);
        mdl.transform.localScale = new Vector3(1.5F, 0.5F, 0.5F);

        var r = mdl.GetComponentInChildren<Renderer>();
    }
}

public class IonCubeBurnerLogic : CustomMachineLogic {
    private void Start() {
        SNUtil.log("Reinitializing base ion cube burner");
        //AqueousEngineeringMod.ionCubeBlock.initializeMachine(gameObject);
    }

    protected override float getTickRate() {
        return 0.1F;
    }

    public override float getBaseEnergyStorageCapacityBonus() {
        return 400;
    }

    protected override void updateEntity(float seconds) {
        if (sub) {
            var space = sub.powerRelay.GetMaxPower() - sub.powerRelay.GetPower();
            if (space > 0) {
#pragma warning disable CS0612 // Type or member is obsolete
                var add = Mathf.Min(space, IonCubeBurner.POWER_RATE);
#pragma warning restore CS0612 // Type or member is obsolete
                if (storage) {
                    storage.container.GetItems(TechType.PrecursorIonCrystal);
                }
            }
        }
    }
}