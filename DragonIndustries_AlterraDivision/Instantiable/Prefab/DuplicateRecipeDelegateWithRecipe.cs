using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public sealed class DuplicateRecipeDelegateWithRecipe : CustomPrefab, DuplicateItemDelegate {
    public readonly CustomPrefab prefab;
    public readonly TechType basis;
    private readonly RecipeData recipe;

    public Sprite sprite;
    public TechType unlock = TechType.None;
    public TechCategory category = TechCategory.Misc;
    public TechGroup group = TechGroup.Uncategorized;
    public CraftTree.Type craftingType = CraftTree.Type.None;
    public float craftTime = 0.1F;
    public string[] craftingMenuTree = [];
    public Assembly ownerMod;
    public bool allowUnlockPopups = false;

    public string suffixName = "";

    [SetsRequiredMembers]
    public DuplicateRecipeDelegateWithRecipe(CustomPrefab s, RecipeData r) :
        base(s.Info.ClassID + "_delegate", "", "") {
        basis = s.Info.TechType;
        prefab = s;
        recipe = r;
        unlock = s.GetGadget<ScanningGadget>().RequiredForUnlock;
        group = s.GetGadget<ScanningGadget>().GroupForPda;
        category = s.GetGadget<ScanningGadget>().CategoryForPda;
        craftingType = s.GetGadget<CraftingGadget>().FabricatorType;
        craftTime = s.GetGadget<CraftingGadget>().CraftingTime;
        craftingMenuTree = s.GetGadget<CraftingGadget>().StepsToFabricatorTab;
        suffixName = " (x" + r.craftAmount + ")";
        if (s is BasicCraftingItem item)
            sprite = item.sprite;
        if (s is DIPrefab<PrefabReference> diPrefab)
            ownerMod = diPrefab.getOwnerMod();
        // FriendlyName += suffixName;
        AddOnRegister(onPatched);

        Info.WithIcon(GetItemSprite()).WithSizeInInventory(SizeInInventory);
        this.SetPdaGroupCategory(GroupForPDA, CategoryForPDA);

        var craft = this.SetRecipe(GetBlueprintRecipe());

        craft.FabricatorType = FabricatorType;
        craft.CraftingTime = CraftingTime;
        craft.StepsToFabricatorTab = StepsToFabricatorTab;

        if (!UnlockedAtStart) this.SetUnlock(RequiredForUnlock);

        SetGameObject(GetGameObject);
    }

    [SetsRequiredMembers]
    public DuplicateRecipeDelegateWithRecipe(TechType from, RecipeData r) :
        base(from.AsString() + "_delegate", "", "") {
        basis = from;
        prefab = null;
        recipe = r;
        suffixName = r.craftAmount > 1 ? " (x" + r.craftAmount + ")" : "";
        sprite = SpriteManager.Get(from);
        AddOnRegister(onPatched);

        Info.WithIcon(GetItemSprite()).WithSizeInInventory(SizeInInventory);
        this.SetPdaGroupCategory(GroupForPDA, CategoryForPDA);

        var craft = this.SetRecipe(GetBlueprintRecipe());

        craft.FabricatorType = FabricatorType;
        craft.CraftingTime = CraftingTime;
        craft.StepsToFabricatorTab = StepsToFabricatorTab;

        if (!UnlockedAtStart) this.SetUnlock(RequiredForUnlock);

        SetGameObject(GetGameObject);
    }

    private void onPatched() {
        if (ownerMod == null)
            throw new Exception("Delegate item " + basis.AsString() + "/" + Info.ClassID + " has no source mod!");
        if (sprite == null)
            throw new Exception("Delegate item " + basis + "/" + Info.ClassID + " has no sprite!");
        SNUtil.log(
            "Constructed craftable delegate of " + basis.AsString() + ": " + Info.ClassID + " @ " +
            RecipeUtil.toString(recipe) + " @ " + string.Join("/", craftingMenuTree),
            ownerMod
        );
        DuplicateRecipeDelegate.addDelegate(this);
    }

    public void setRecipe(int amt = 1) {
        for (var i = 0; i < amt; i++)
            recipe.LinkedItems.Add(basis);
        recipe.craftAmount = 0;
        suffixName = amt > 1 ? " (x" + amt + ")" : "";
    }

    public TechGroup GroupForPDA => group;

    public TechCategory CategoryForPDA => category;

    public TechType RequiredForUnlock => unlock;

    public bool UnlockedAtStart => unlock == TechType.None;

    public Vector2int SizeInInventory => TechData.GetItemSize(basis);

    public CraftTree.Type FabricatorType => craftingType;

    public float CraftingTime => craftTime;

    public string[] StepsToFabricatorTab => craftingMenuTree;

    public GameObject GetGameObject() {
        return ObjectUtil.createWorldObject(CraftData.GetClassIdForTechType(basis), true, false);
    }

    protected Sprite GetItemSprite() {
        return sprite;
    }

    public override string ToString() {
        return base.ToString() + " [" + Info.TechType + "] / " + Info.ClassID + " / " + Info.PrefabFileName;
    }

    protected RecipeData GetBlueprintRecipe() {
        return recipe;
    }

    public string getNameSuffix() {
        return suffixName;
    }

    public CustomPrefab getPrefab() {
        return prefab;
    }

    public TechType getBasis() {
        return basis;
    }

    public Assembly getOwnerMod() {
        return ownerMod;
    }

    public string getTooltip() {
        return Language.main.Get("Tooltip_" + basis.AsString());
    }

    public bool allowTechUnlockPopups() {
        return allowUnlockPopups;
    }
}