using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class SeamothHeatSink : BasicCraftingItem {
    [SetsRequiredMembers]
    public SeamothHeatSink() : base(
        SeaToSeaMod.ItemLocale.getEntry("SeamothHeatSink"),
        "WorldEntities/Natural/CopperWire"
    ) {
        sprite = TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/Items/Seamothheatsink");
        craftingSubCategory = "Tools";
        craftingTime = 4;
        unlockRequirement = TechType.Unobtanium;
        inventorySize = new Vector2int(1, 2);
        renderModify = r => { EjectedHeatSink.setTexture(r); };
    }

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);
    }

    public override CraftTree.Type FabricatorType => CraftTree.Type.SeamothUpgrades;

    public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;

    public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;

    public override string[] StepsToFabricatorTab => ["Torpedoes"];
}