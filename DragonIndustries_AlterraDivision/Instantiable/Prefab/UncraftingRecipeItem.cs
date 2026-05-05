using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public sealed class UncraftingRecipeItem : CustomPrefab, DuplicateItemDelegate {
    public readonly CustomPrefab prefab;
    public readonly TechType basis;

    public Sprite sprite;
    public TechCategory category = TechCategory.Misc;
    public TechGroup group = TechGroup.Uncategorized;
    public CraftTree.Type craftingType = CraftTree.Type.None;
    public float craftTime = 1F;
    public string[] craftingMenuTree = [];
    public Assembly ownerMod;

    [SetsRequiredMembers]
    public UncraftingRecipeItem(CustomPrefab s) : base(s.Info.ClassID + "_uncrafting", "", "") {
        basis = s.Info.TechType;
        prefab = s;
        group = s.GetGadget<ScanningGadget>().GroupForPda;
        category = s.GetGadget<ScanningGadget>().CategoryForPda;
        craftingType = s.GetGadget<CraftingGadget>().FabricatorType;
        craftTime = s.GetGadget<CraftingGadget>().CraftingTime;
        craftingMenuTree = s.GetGadget<CraftingGadget>().StepsToFabricatorTab;
        if (s is BasicCraftingItem item)
            sprite = item.sprite;
        if (s is DIPrefab<PrefabReference> diPrefab)
            ownerMod = diPrefab.getOwnerMod();
        AddOnRegister(onPatched);

        if (!UnlockedAtStart) this.SetUnlock(RequiredForUnlock);

        this.SetPdaGroupCategory(GroupForPDA, CategoryForPDA);

        var craft = this.SetRecipe(GetBlueprintRecipe());

        craft.FabricatorType = FabricatorType;
        craft.CraftingTime = CraftingTime;
        craft.StepsToFabricatorTab = StepsToFabricatorTab;

        Info.WithIcon(GetItemSprite());
        SetGameObject(GetGameObject);
    }

    [SetsRequiredMembers]
    public UncraftingRecipeItem(TechType from) : base(from.AsString() + "_uncrafting", "", "") {
        basis = from;
        prefab = null;
        sprite = SpriteManager.Get(from);
        AddOnRegister(onPatched);

        if (!UnlockedAtStart) this.SetUnlock(RequiredForUnlock);

        this.SetPdaGroupCategory(GroupForPDA, CategoryForPDA);

        var craft = this.SetRecipe(GetBlueprintRecipe());

        craft.FabricatorType = FabricatorType;
        craft.CraftingTime = CraftingTime;
        craft.StepsToFabricatorTab = StepsToFabricatorTab;

        Info.WithIcon(GetItemSprite());
        SetGameObject(GetGameObject);
    }

    private void onPatched() {
        if (ownerMod == null)
            throw new Exception("Uncrafting item " + basis + "/" + Info.TechType + " has no source mod!");
        SNUtil.log(
            "Constructed uncrafting of " + basis + ": " + Info.TechType + " @ " + string.Join("/", craftingMenuTree),
            ownerMod
        );
        DuplicateRecipeDelegate.addDelegate(this);
    }

    public TechGroup GroupForPDA => group;

    public TechCategory CategoryForPDA => category;

    public TechType RequiredForUnlock => basis;

    public bool UnlockedAtStart => false; //unlock == TechType.None;

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
        return RecipeUtil.createUncrafting(basis);
    }

    public string getNameSuffix() {
        return " (Uncrafting)";
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
        return "Reclaiming the crafting ingredients.";
    }

    public bool allowTechUnlockPopups() {
        return false;
    }
}