using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class CurativeBandage : BasicCraftingItem {
    [SetsRequiredMembers]
    public CurativeBandage() : base(
        SeaToSeaMod.ItemLocale.getEntry("CurativeBandage"),
        "WorldEntities/Natural/FirstAidKit"
    ) {
        sprite = TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/Items/CurativeBandage");
        unlockRequirement = TechType.Unobtanium; //TechType.Workbench;//SeaToSeaMod.healFlower.TechType;
        craftingTime = 6;
        inventorySize = new Vector2int(1, 2);
        renderModify = r => {
            r.transform.localScale = new Vector3(1, 3, 1);
            r.materials[0].SetFloat("_Shininess", 7.5F);
            r.materials[0].SetFloat("_SpecInt", 10F);
            r.materials[0].SetFloat("_Fresnel", 0.5F);
        };
    }

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);
    }

    public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;

    public override TechGroup GroupForPDA => TechGroup.Workbench;

    public override TechCategory CategoryForPDA => TechCategory.Workbench;

    public override string[] StepsToFabricatorTab => ["C2CMedical"];
}