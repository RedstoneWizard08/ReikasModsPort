using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public sealed class RebreatherV2 : CustomEquipable {
    [SetsRequiredMembers]
    public RebreatherV2() : base(SeaToSeaMod.ItemLocale.getEntry("RebreatherV2"), "WorldEntities/Natural/rebreather") {
        isArmor = true;
        preventNaturalUnlock();
    }

    public override Vector2int SizeInInventory => new(2, 3);

    public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;

    public override string[] StepsToFabricatorTab => ["TankMenu"];

    public override void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public override EquipmentType EquipmentType => EquipmentType.Head;
}