using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public sealed class LiquidTank : CustomEquipable {
    [SetsRequiredMembers]
    public LiquidTank() : base(SeaToSeaMod.ItemLocale.getEntry("LiquidTank"), "WorldEntities/Tools/HighCapacityTank") {
        isArmor = true;
        preventNaturalUnlock();
        AddOnRegister(() => {
                SaveSystem.addSaveHandler(
                    ClassID,
                    new SaveSystem.ComponentFieldSaveHandler<Battery>().addField("_charge")
                );
            }
        );
    }

    public override Vector2int SizeInInventory => new(3, 3);

    public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;

    public override string[] StepsToFabricatorTab => [];

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        var o2 = go.EnsureComponent<Oxygen>();
        o2.oxygenAvailable = 0;
        o2.oxygenCapacity = LiquidBreathingSystem.TankCapacity;
        var b = go.EnsureComponent<Battery>();
        b._capacity = LiquidBreathingSystem.ItemValue;
    }

    public override EquipmentType EquipmentType => EquipmentType.Tank;
}