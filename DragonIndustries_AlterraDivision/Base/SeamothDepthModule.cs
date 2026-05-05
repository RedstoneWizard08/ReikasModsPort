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

    public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;

    public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

    public override TechGroup GroupForPDA => TechGroup.Workbench;

    public override TechCategory CategoryForPDA => TechCategory.Workbench;

    public override string[] StepsToFabricatorTab => ["SeamothMenu"];
}