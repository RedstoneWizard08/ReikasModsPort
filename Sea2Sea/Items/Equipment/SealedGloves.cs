using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public sealed class SealedGloves : CustomEquipable {
    [SetsRequiredMembers]
    public SealedGloves() : base(
        SeaToSeaMod.ItemLocale.getEntry("SealedGloves"),
        "WorldEntities/Tools/ReinforcedGloves"
    ) {
        isArmor = true;
        preventNaturalUnlock();
    }

    public override Vector2int SizeInInventory => new(2, 2);

    public override CraftTree.Type FabricatorType => CraftTree.Type.None;

    public override TechGroup GroupForPDA => TechGroup.Uncategorized;

    public override TechCategory CategoryForPDA => TechCategory.Misc;

    public override void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public override sealed EquipmentType EquipmentType => EquipmentType.Gloves;
}