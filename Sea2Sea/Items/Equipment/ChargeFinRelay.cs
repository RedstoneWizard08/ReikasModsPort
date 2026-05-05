using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public sealed class ChargeFinRelay : CustomEquipable {
    [SetsRequiredMembers]
    public ChargeFinRelay() : base(SeaToSeaMod.itemLocale.getEntry("ChargeFinRelay"), "WorldEntities/Tools/Compass") {
        this.preventNaturalUnlock();
    }

    public override Vector2int SizeInInventory {
        get { return new Vector2int(2, 2); }
    }

    public override CraftTree.Type FabricatorType {
        get { return CraftTree.Type.Workbench; }
    }

    public override string[] StepsToFabricatorTab {
        get { return new string[] { "C2CModElectronics" }; }
    }

    public override void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public override EquipmentType EquipmentType {
        get { return EquipmentType.Chip; }
    }
}