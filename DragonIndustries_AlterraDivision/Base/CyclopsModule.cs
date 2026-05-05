using System.Diagnostics.CodeAnalysis;

namespace ReikaKalseki.DIAlterra;

public abstract class CyclopsModule : CustomEquipable {
    [SetsRequiredMembers]
    protected CyclopsModule(XMLLocale.LocaleEntry e) : this(e.key, e.name, e.desc) {
    }

    [SetsRequiredMembers]
    protected CyclopsModule(string id, string name, string desc) : base(
        id,
        name,
        desc,
        "WorldEntities/Tools/CyclopsHullModule3"
    ) {
        dependency = TechType.Cyclops;
    }

    public override CraftTree.Type FabricatorType => CraftTree.Type.CyclopsFabricator;

    public override string[] StepsToFabricatorTab => []; //return new string[]{"DICyclops"};//new string[]{"CyclopsModules"};

    public override sealed EquipmentType EquipmentType => EquipmentType.CyclopsModule;

    public override sealed TechGroup GroupForPDA => TechGroup.Cyclops;

    public override sealed TechCategory CategoryForPDA => TechCategory.CyclopsUpgrades;
}