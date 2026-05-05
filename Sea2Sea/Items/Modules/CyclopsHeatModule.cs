using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public sealed class CyclopsHeatModule : CyclopsModule {
    [SetsRequiredMembers]
    public CyclopsHeatModule() : base(SeaToSeaMod.itemLocale.getEntry("CyclopsHeat")) {
        this.preventNaturalUnlock();
    }

    public override QuickSlotType QuickSlotType {
        get { return QuickSlotType.Passive; }
    }

    public override Vector2int SizeInInventory {
        get { return new Vector2int(3, 3); }
    }
    /*
    protected override Atlas.Sprite GetItemSprite()
    {
        return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
    }*/
}