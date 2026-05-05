using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public sealed class SeamothPowerSealModule : SeamothModule {
    [SetsRequiredMembers]
    public SeamothPowerSealModule() : base(SeaToSeaMod.itemLocale.getEntry("SeamothPowerSeal")) {
        this.preventNaturalUnlock();
    }

    public override QuickSlotType QuickSlotType {
        get { return QuickSlotType.Passive; }
    }

    public override Vector2int SizeInInventory {
        get { return new Vector2int(2, 1); }
    }
    /*
    protected override Atlas.Sprite GetItemSprite()
    {
        return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
    }*/
}