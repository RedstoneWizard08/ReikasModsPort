using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public sealed class SeamothPowerSealModule : SeamothModule {
    [SetsRequiredMembers]
    public SeamothPowerSealModule() : base(SeaToSeaMod.ItemLocale.getEntry("SeamothPowerSeal")) {
        preventNaturalUnlock();
    }

    public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

    public override Vector2int SizeInInventory => new(2, 1);
    /*
    protected override Atlas.Sprite GetItemSprite()
    {
        return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
    }*/
}