using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public sealed class CyclopsHeatModule : CyclopsModule {
    [SetsRequiredMembers]
    public CyclopsHeatModule() : base(SeaToSeaMod.ItemLocale.getEntry("CyclopsHeat")) {
        preventNaturalUnlock();
    }

    public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

    public override Vector2int SizeInInventory => new(3, 3);
    /*
    protected override Atlas.Sprite GetItemSprite()
    {
        return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
    }*/
}