using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class NotFabricable : BasicCraftingItem {
    [SetsRequiredMembers]
    public NotFabricable(XMLLocale.LocaleEntry e, string template) : base(e, template) {
    }

    [SetsRequiredMembers]
    public NotFabricable(string id, string name, string desc, string template) : base(id, name, desc, template) {
    }

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);
    }

    public override CraftTree.Type FabricatorType => CraftTree.Type.None;

    public override TechGroup GroupForPDA => TechGroup.Uncategorized;

    public override TechCategory CategoryForPDA => TechCategory.Misc;
}