using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class BreathingFluid : BasicCraftingItem {
    [SetsRequiredMembers]
    public BreathingFluid() : base(
        SeaToSeaMod.ItemLocale.getEntry("breathfluid"),
        "WorldEntities/Natural/polyaniline"
    ) {
        sprite = TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/Items/BreathFluid");
        unlockRequirement = TechType.Unobtanium; //SeaToSeaMod.rebreatherV2.TechType;
        craftingSubCategory = "C2Chemistry";
        craftingTime = 15;
        inventorySize = new Vector2int(3, 3);
        renderModify = r => {
            r.transform.localScale = new Vector3(2.4F, 2.4F, 1);
            r.setPolyanilineColor(new Color(1, 158 / 255F, 201 / 255F, 1.5F));
            r.materials[1].SetFloat("_Shininess", 5F);
            r.materials[1].SetFloat("_SpecInt", 12F);
            r.materials[1].SetFloat("_Fresnel", 0F);
        };
    }

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);
    }

    public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;

    public override TechGroup GroupForPDA => TechGroup.Resources;

    public override TechCategory CategoryForPDA => C2CItems.chemistryCategory;

    public override string[] StepsToFabricatorTab => ["Resources", "C2Chemistry"];
}