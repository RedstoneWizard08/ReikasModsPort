using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public sealed class SeamothTetherModule : SeamothModule {
    [SetsRequiredMembers]
    public SeamothTetherModule() : base(SeaToSeaMod.ItemLocale.getEntry("SeamothTether")) {
        preventNaturalUnlock();
    }

    public override QuickSlotType QuickSlotType => QuickSlotType.Toggleable;

    protected override float getChargingPowerCost() {
        return 1;
    }
}