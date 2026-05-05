using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public sealed class NuclearFuelItem : CustomEquipable {
    [SetsRequiredMembers]
    public NuclearFuelItem(string key) : base(
        AqueousEngineeringMod.itemLocale.getEntry(key),
        "WorldEntities/Natural/reactorrod"
    ) {
        preventNaturalUnlock();
    }

    public override void prepareGameObject(GameObject go, Renderer[] r0) {
        base.prepareGameObject(go, r0);
        RenderUtil.swapToModdedTextures(r0, this);
    }

    public override EquipmentType EquipmentType => EquipmentType.NuclearReactor;

    public override float CraftingTime => 4;

    public override Vector2int SizeInInventory => new(1, 1);

    public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;

    public override TechGroup GroupForPDA => TechGroup.Resources;

    public override TechCategory CategoryForPDA => AqueousEngineeringMod.nuclearCategory;

    public override string[] StepsToFabricatorTab => ["Resources", "Nuclear"];
}