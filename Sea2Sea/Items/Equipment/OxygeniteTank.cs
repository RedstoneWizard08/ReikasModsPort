using System;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

[Obsolete]
public sealed class OxygeniteTank : CustomEquipable {
    public static readonly int TANK_CAPACITY = 855; //total 20 min

    [SetsRequiredMembers]
    public OxygeniteTank() : base(
        SeaToSeaMod.ItemLocale.getEntry("OxygeniteTank"),
        "WorldEntities/Tools/HighCapacityTank"
    ) {
        isArmor = true;
        preventNaturalUnlock();
    }

    public override Vector2int SizeInInventory => new(3, 4);

    public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;

    public override string[] StepsToFabricatorTab => ["TankMenu"];

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        var o2 = go.EnsureComponent<Oxygen>();
        o2.oxygenCapacity = TANK_CAPACITY;
        o2.oxygenAvailable = o2.oxygenCapacity;
    }

    public override EquipmentType EquipmentType => EquipmentType.Tank;
}