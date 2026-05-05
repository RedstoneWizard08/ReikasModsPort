using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public sealed class CyclopsStorageModule : CyclopsModule {
    [SetsRequiredMembers]
    public CyclopsStorageModule() : base(SeaToSeaMod.itemLocale.getEntry("CyclopsStorage")) {
    }

    public override QuickSlotType QuickSlotType {
        get { return QuickSlotType.Passive; }
    }

    public override Vector2int SizeInInventory {
        get { return new Vector2int(2, 2); }
    }
    /*
    protected override Atlas.Sprite GetItemSprite()
    {
        return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
    }*/
}