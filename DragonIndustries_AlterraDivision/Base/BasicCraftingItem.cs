using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class BasicCraftingItem : CustomPrefab, DIPrefab<BasicCraftingItem, StringPrefabContainer> {
    private static bool addedTab = false;

    private readonly List<PlannedIngredient> recipe = new List<PlannedIngredient>();
    public readonly string id;

    public int numberCrafted = 1;
    public TechType unlockRequirement = TechType.None;
    public Sprite sprite = null;
    public float craftingTime = 0;
    public Vector2int inventorySize = new Vector2int(1, 1);
    public readonly List<PlannedIngredient> byproducts = new List<PlannedIngredient>();
    public string craftingSubCategory = "" + TechCategory.BasicMaterials;
    public Action<Renderer> renderModify = null;

    public float glowIntensity { get; set; }
    public StringPrefabContainer baseTemplate { get; set; }

    protected readonly Assembly ownerMod;

    [SetsRequiredMembers]
    public BasicCraftingItem(XMLLocale.LocaleEntry e, string template) : this(e.key, e.name, e.desc, template) {
    }

    [SetsRequiredMembers]
    public BasicCraftingItem(string id, string name, string desc, string template) : base(id, name, desc) {
        ownerMod = SNUtil.tryGetModDLL();
        // typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
        this.id = id;

        if (!addedTab) {
            //CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, "DIIntermediate", "Intermediate Products", SpriteManager.Get(TechType.HatchingEnzymes));
            addedTab = true;
        }

        baseTemplate = new StringPrefabContainer(template.Contains("/") ? PrefabData.getPrefabID(template) : template);

        AddOnRegister(() => {
                ItemRegistry.instance.addItem(this);

                var recipe = this.SetRecipe(getRecipe());

                recipe.FabricatorType = FabricatorType;
                recipe.CraftingTime = CraftingTime;
            }
        );

        this.SetPdaGroupCategory(GroupForPDA, CategoryForPDA);
        SetGameObject(GetGameObject);
        Info.WithIcon(GetItemSprite()).WithSizeInInventory(SizeInInventory);
    }

    public virtual CraftTree.Type FabricatorType {
        get { return CraftTree.Type.Fabricator; }
    }

    /*
    public TechType getTechType() {
        TechType tech = TechType.None;
        TechTypeHandler.TryGetModdedTechType(id, out tech);
        return tech;
    }*/

    public BasicCraftingItem addIngredient(ItemDef item, int amt) {
        return this.addIngredient(item.getTechType(), amt);
    }

    public BasicCraftingItem addIngredient(CustomPrefab item, int amt) {
        return this.addIngredient(new ModPrefabTechReference(item), amt);
    }

    public BasicCraftingItem addIngredient(TechType item, int amt) {
        return this.addIngredient(new TechTypeContainer(item), amt);
    }

    public BasicCraftingItem addIngredient(TechTypeReference item, int amt) {
        recipe.Add(new PlannedIngredient(item, amt));
        return this;
    }

    public BasicCraftingItem scaleRecipe(float amt) {
        numberCrafted = (int)Mathf.Max(1, numberCrafted * amt);
        foreach (PlannedIngredient pi in recipe)
            pi.amount = (int)Mathf.Max(1, pi.amount * amt);
        return this;
    }

    public virtual TechType RequiredForUnlock {
        get { return unlockRequirement; }
    }

    public virtual TechGroup GroupForPDA {
        get { return TechGroup.Resources; }
    }

    public virtual TechCategory CategoryForPDA {
        get {
            TechCategory ret = TechCategory.Misc;
            return Enum.TryParse(craftingSubCategory, out ret) ? ret :
                EnumHandler.TryGetValue(craftingSubCategory, out ret) ? ret :
                TechCategory.BasicMaterials;
        }
    }

    public virtual string[] StepsToFabricatorTab {
        get {
            //SNUtil.log("Fetching craftingsubcat "+craftingSubCategory+" from "+FriendlyName);
            //RecipeUtil.dumpCraftTree(CraftTree.Type.Fabricator);
            return new string[] { "Resources", craftingSubCategory };
        }
    }

    public virtual GameObject GetGameObject() {
        GameObject go = ObjectUtil.getModPrefabBaseObject(this);
        if (renderModify != null)
            renderModify(go.GetComponentInChildren<Renderer>());
        return go;
    }

    public Assembly getOwnerMod() {
        return ownerMod;
    }

    public string ClassID => Info.ClassID;

    public virtual bool isResource() {
        return false;
    }

    public virtual string getTextureFolder() {
        return "Items/World";
    }

    public virtual void prepareGameObject(GameObject go, Renderer[] r) {
    }

    protected RecipeData GetBlueprintRecipe() {
        return new RecipeData {
            Ingredients = RecipeUtil.buildRecipeList(recipe),
            craftAmount = numberCrafted,
            LinkedItems = RecipeUtil.buildLinkedItems(byproducts)
        };
    }

    public virtual RecipeData getRecipe() {
        return this.GetBlueprintRecipe();
    }

    protected virtual Sprite GetItemSprite() {
        return sprite;
    }

    public Sprite getIcon() {
        return this.GetItemSprite();
    }

    public virtual float CraftingTime {
        get { return craftingTime; }
    }

    public virtual Vector2int SizeInInventory {
        get { return inventorySize; }
    }

    public sealed override string ToString() {
        return base.ToString() + " [" + Info.TechType + "] / " + Info.ClassID + " / " + Info.PrefabFileName;
    }
}