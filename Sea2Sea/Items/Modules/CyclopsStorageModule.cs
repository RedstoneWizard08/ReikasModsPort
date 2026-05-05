using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public sealed class CyclopsStorageModule : CyclopsModule {
    [SetsRequiredMembers]
    public CyclopsStorageModule() : base(SeaToSeaMod.ItemLocale.getEntry("CyclopsStorage")) {
    }

    public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

    public override Vector2int SizeInInventory => new(2, 2);
    /*
    protected override Atlas.Sprite GetItemSprite()
    {
        return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
    }*/
}