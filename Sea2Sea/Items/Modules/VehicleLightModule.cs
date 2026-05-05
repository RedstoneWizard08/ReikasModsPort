using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public sealed class VehicleLightModule : CustomEquipable {
    [SetsRequiredMembers]
    public VehicleLightModule() : base(
        SeaToSeaMod.ItemLocale.getEntry("VehicleLightBonus"),
        "d290b5da-7370-4fb8-81bc-656c6bde78f8"
    ) {
        preventNaturalUnlock();
    }

    public override sealed EquipmentType EquipmentType => EquipmentType.VehicleModule;

    public override QuickSlotType QuickSlotType => QuickSlotType.Selectable;

    public override CraftTree.Type FabricatorType => CraftTree.Type.SeamothUpgrades;

    public override string[] StepsToFabricatorTab => ["CommonModules"];

    public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;

    public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;
}