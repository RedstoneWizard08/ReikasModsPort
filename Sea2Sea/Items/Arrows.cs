using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class Arrow : BasicCraftingItem {
    [SetsRequiredMembers]
    public Arrow(string id, string name, string desc, string template) : base(id, name, desc, template) {
        sprite = TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/Items/" + id);
    }

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);
    }

    public override CraftTree.Type FabricatorType => CraftTree.Type.None;

    public sealed override TechGroup GroupForPDA => TechGroup.Uncategorized;

    public override TechCategory CategoryForPDA => TechCategory.Misc;
}