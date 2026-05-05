using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class CustomBattery : BasicCraftingItem {
    public readonly int capacity;

    [SetsRequiredMembers]
    public CustomBattery(XMLLocale.LocaleEntry e, int cap) : this(e.key, e.name, e.desc, cap) {
    }

    [SetsRequiredMembers]
    public CustomBattery(string id, string name, string desc, int cap) : base(
        id,
        name,
        desc,
        "d4bfebc0-a5e6-47d3-b4a7-d5e47f614ed6"
    ) {
        capacity = cap;
        sprite = TextureManager.getSprite(ownerMod, "Textures/Items/" + ObjectUtil.formatFileName(this));
        AddOnRegister(() => { CraftDataHandler.SetEquipmentType(Info.TechType, EquipmentType.BatteryCharger); });
    }

    public sealed override TechCategory CategoryForPDA => TechCategory.Electronics;

    public sealed override string[] StepsToFabricatorTab => ["Resources", "Electronics"]; //new string[]{"DIIntermediate"};

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);
        go.EnsureComponent<Battery>()._capacity = capacity;
        go.EnsureComponent<Battery>().charge = capacity;
        go.SetActive(false);
    }
}