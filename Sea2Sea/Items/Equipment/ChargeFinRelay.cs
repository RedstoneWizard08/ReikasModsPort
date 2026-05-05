using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public sealed class ChargeFinRelay : CustomEquipable {
    [SetsRequiredMembers]
    public ChargeFinRelay() : base(SeaToSeaMod.ItemLocale.getEntry("ChargeFinRelay"), "WorldEntities/Tools/Compass") {
        preventNaturalUnlock();
    }

    public override Vector2int SizeInInventory => new(2, 2);

    public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;

    public override string[] StepsToFabricatorTab => ["C2CModElectronics"];

    public override void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public override EquipmentType EquipmentType => EquipmentType.Chip;
}