using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class StalkerToy : BasicCraftingItem {
    [SetsRequiredMembers]
    public StalkerToy(XMLLocale.LocaleEntry e) : base(e, "WorldEntities/Food/CuredHoopfish") {
        sprite = TextureManager.getSprite(AqueousEngineeringMod.modDLL, "Textures/Items/StalkerToy");
        unlockRequirement = TechType.Unobtanium;
        craftingTime = 6;
        inventorySize = new Vector2int(2, 2);
    }

    public override void prepareGameObject(GameObject go, Renderer[] r0) {
        base.prepareGameObject(go, r0);
        foreach (var r in r0) {
            RenderUtil.setEmissivity(r, 0);
            RenderUtil.setGlossiness(r, 9, 15, 0);
        }

        RenderUtil.swapToModdedTextures(r0, this);
        go.removeComponent<Eatable>();
    }

    public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;

    public override TechGroup GroupForPDA => TechGroup.Personal;

    public override TechCategory CategoryForPDA => TechCategory.Equipment;

    public override string[] StepsToFabricatorTab => ["Machines"];
}