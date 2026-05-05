using System.Diagnostics.CodeAnalysis;
using Nautilus.Utility;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class SeamothPlanktonScoop : SeamothModule {
    [SetsRequiredMembers]
    internal SeamothPlanktonScoop() : base(
        EcoceanMod.locale.getEntry("PlanktonScoop"),
        "d290b5da-7370-4fb8-81bc-656c6bde78f8"
    ) {
        if (BepInExUtil.IsModLoaded("ReikaKalseki.SeaToSea")) //does not work
            this.preventNaturalUnlock();
    }

    public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

    public override SeamothModule.SeamothModuleStorage getStorage() {
        return new SeamothModule.SeamothModuleStorage("SCOOP STORAGE", StorageAccessType.BOX, 6, 6);
    }

    public void register() {
        this.addIngredient(TechType.VehicleStorageModule, 1);
        this.addIngredient(TechType.PropulsionCannon, 1);
        this.addIngredient(TechType.FiberMesh, 2);
        this.addIngredient(EcoceanMod.mushroomVaseStrand.seed.TechType, 3);
        this.Register();
    }

    public static bool checkAndTryScoop(SeaMoth sm, float dT, TechType harvest, out GameObject collected) {
        collected = null;
        if (sm.GetComponent<Rigidbody>().velocity.magnitude >= 4 &&
            sm.vehicleHasUpgrade(EcoceanMod.planktonScoop.TechType)) {
            if (Random.Range(0F, 1F) < 0.075F * dT * EcoceanMod.config.getFloat(ECConfig.ConfigEntries.PLANKTONRATE)) {
                foreach (var sc in sm.GetComponentsInChildren<SeamothStorageContainer>(true)) {
                    var tt = sc.GetComponent<TechTag>();
                    if (tt && tt.type == EcoceanMod.planktonScoop.TechType) {
                        collected = ObjectUtil.createWorldObject(harvest, true, false);
                        if (sc.container.AddItem(collected.GetComponentInChildren<Pickupable>()) != null)
                            uGUI_IconNotifier.main.Play(harvest, uGUI_IconNotifier.AnimationType.From, null);
                        if (sc.container.IsFull())
                            SNUtil.writeToChat("Plankton scoop is full");
                        break;
                    }
                }
            }

            return true;
        }

        return false;
    }
}