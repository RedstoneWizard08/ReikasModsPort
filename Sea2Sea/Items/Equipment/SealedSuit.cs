using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public sealed class SealedSuit : CustomEquipable {
    [SetsRequiredMembers]
    public SealedSuit() : base(
        SeaToSeaMod.ItemLocale.getEntry("SealedSuit"),
        "WorldEntities/Tools/ReinforcedDiveSuit"
    ) {
        isArmor = true;
        preventNaturalUnlock();
    }

    public override Vector2int SizeInInventory => new(2, 2);

    public override void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public override sealed EquipmentType EquipmentType => EquipmentType.Body;

    public override List<TechType> getAuxCrafted() {
        return C2CItems.sealGloves != null ? [C2CItems.sealGloves.Info.TechType] : [];
    }
}