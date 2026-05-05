using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class OxygeniteCharge : BasicCraftingItem {
    [SetsRequiredMembers]
    public OxygeniteCharge() : base(
        SeaToSeaMod.ItemLocale.getEntry("OxygeniteCharge"),
        "WorldEntities/Natural/FirstAidKit"
    ) {
        sprite = TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/Items/OxygeniteCharge");
        unlockRequirement = TechType.Unobtanium;
        craftingTime = 20;
        inventorySize = new Vector2int(3, 3);
        renderModify = r => { };
    }

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);
    }

    public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;

    public override TechGroup GroupForPDA => TechGroup.Workbench;

    public override TechCategory CategoryForPDA => TechCategory.Workbench;

    public override string[] StepsToFabricatorTab => ["Tank"];
}