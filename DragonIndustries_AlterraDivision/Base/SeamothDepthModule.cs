using System.Diagnostics.CodeAnalysis;

namespace ReikaKalseki.DIAlterra;

public sealed class SeamothDepthModule : SeamothModule {
    public readonly int maxDepth;
    public readonly int depthBonus;

    [SetsRequiredMembers]
    public SeamothDepthModule(string id, string name, string desc, int d) : base(id, name, desc) {
        maxDepth = d;
        depthBonus = maxDepth - 200;
        dependency = TechType.BaseUpgradeConsole;
    }

    public override CraftTree.Type FabricatorType {
        get { return CraftTree.Type.Workbench; }
    }

    public override QuickSlotType QuickSlotType {
        get { return QuickSlotType.Passive; }
    }

    public override TechGroup GroupForPDA {
        get { return TechGroup.Workbench; }
    }

    public override TechCategory CategoryForPDA {
        get { return TechCategory.Workbench; }
    }

    public override string[] StepsToFabricatorTab {
        get { return new string[] { "SeamothMenu" }; }
    }
}