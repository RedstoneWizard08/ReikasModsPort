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

    public override CraftTree.Type FabricatorType {
        get { return CraftTree.Type.CyclopsFabricator; }
    }

    public override string[] StepsToFabricatorTab {
        get {
            return new string[0]; //return new string[]{"DICyclops"};//new string[]{"CyclopsModules"};
        }
    }

    public override sealed EquipmentType EquipmentType {
        get { return EquipmentType.CyclopsModule; }
    }

    public override sealed TechGroup GroupForPDA {
        get { return TechGroup.Cyclops; }
    }

    public override sealed TechCategory CategoryForPDA {
        get { return TechCategory.CyclopsUpgrades; }
    }
}