using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class FCSFuel : BasicCraftingItem {
    [SetsRequiredMembers]
    public FCSFuel() : base(SeaToSeaMod.ItemLocale.getEntry("FCSFuel"), "WorldEntities/Natural/Lubricant") {
        sprite = TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/Items/FCSFuel");
        unlockRequirement = TechType.Unobtanium;
        craftingTime = 6;
        numberCrafted = 4;
        inventorySize = new Vector2int(2, 2);
    }

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);
    }

    public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;

    public override TechGroup GroupForPDA => TechGroup.Resources;

    public override TechCategory CategoryForPDA => C2CItems.chemistryCategory;

    public override string[] StepsToFabricatorTab => ["Resources", "C2Chemistry"];
}