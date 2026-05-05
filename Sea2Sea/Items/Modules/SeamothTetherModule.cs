using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public sealed class SeamothTetherModule : SeamothModule {
    [SetsRequiredMembers]
    public SeamothTetherModule() : base(SeaToSeaMod.itemLocale.getEntry("SeamothTether")) {
        this.preventNaturalUnlock();
    }

    public override QuickSlotType QuickSlotType {
        get { return QuickSlotType.Toggleable; }
    }

    protected override float getChargingPowerCost() {
        return 1;
    }
}