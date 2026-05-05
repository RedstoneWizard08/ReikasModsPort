using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public sealed class SealedSuit : CustomEquipable {
    [SetsRequiredMembers]
    public SealedSuit() : base(
        SeaToSeaMod.itemLocale.getEntry("SealedSuit"),
        "WorldEntities/Tools/ReinforcedDiveSuit"
    ) {
        isArmor = true;
        this.preventNaturalUnlock();
    }

    public override Vector2int SizeInInventory {
        get { return new Vector2int(2, 2); }
    }

    public override void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public override sealed EquipmentType EquipmentType {
        get { return EquipmentType.Body; }
    }

    public override List<TechType> getAuxCrafted() {
        return new List<TechType> { C2CItems.sealGloves.Info.TechType };
    }
}