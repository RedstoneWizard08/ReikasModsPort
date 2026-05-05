using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public abstract class CustomEquipable : CustomPrefab, DIPrefab<CustomEquipable, StringPrefabContainer> {
    private readonly List<PlannedIngredient> recipe = [];
    public readonly string id;

    private readonly Assembly ownerMod;

    public float glowIntensity { get; set; }
    public bool isArmor { get; set; }
    public StringPrefabContainer baseTemplate { get; set; }

    public TechType dependency = TechType.None;
    private PDAManager.PDAPage page;

    [SetsRequiredMembers]
    protected CustomEquipable(XMLLocale.LocaleEntry e, string template) : this(e.key, e.name, e.desc, template) {
        if (!string.IsNullOrEmpty(e.pda)) {
            page = PDAManager.createPage("ency_" + Info.ClassID, Info.PrefabFileName, e.pda, "Tech/Equipment");
            var header = e.getString("header");
            if (header != null)
                page.setHeaderImage(TextureManager.getTexture(SNUtil.tryGetModDLL(), "Textures/PDA/" + header));
            page.register();
        }
    }

    [SetsRequiredMembers]
    protected CustomEquipable(string id, string name, string desc, string template) : base(id, name, desc) {
        ownerMod = SNUtil.tryGetModDLL();
        // typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
        this.id = id;

        baseTemplate = new StringPrefabContainer(template.Contains("/") ? PrefabData.getPrefabID(template) : template);
        SetGameObject(GetGameObject);
        Info.WithIcon(getIcon());

        AddOnRegister(() => {
                ItemRegistry.instance.addItem(this);
                if (page != null)
                    TechnologyUnlockSystem.instance.registerPage(Info.TechType, page);

                if (!UnlockedAtStart) {
                    this.SetUnlock(RequiredForUnlock);
                }

                var craft = this.SetRecipe(GetBlueprintRecipe());

                craft.FabricatorType = FabricatorType;
                craft.StepsToFabricatorTab = StepsToFabricatorTab;
                craft.CraftingTime = CraftingTime;

                this.SetPdaGroupCategory(GroupForPDA, CategoryForPDA);

                var equip = this.SetEquipment(EquipmentType);

                equip.QuickSlotType = QuickSlotType;
            }
        );
    }
    /*
    public TechType getTechType() {
        TechType tech = TechType.None;
        TechTypeHandler.TryGetModdedTechType(id, out tech);
        return tech;
    }*/

    public CustomEquipable addIngredient(ItemDef item, int amt) {
        return addIngredient(item.getTechType(), amt);
    }

    public CustomEquipable addIngredient(CustomPrefab item, int amt) {
        return addIngredient(new ModPrefabTechReference(item), amt);
    }

    public CustomEquipable addIngredient(TechType item, int amt) {
        return addIngredient(new TechTypeContainer(item), amt);
    }

    public CustomEquipable addIngredient(TechTypeReference item, int amt) {
        recipe.Add(new PlannedIngredient(item, amt));
        return this;
    }

    public virtual EquipmentType EquipmentType => EquipmentType.None;

    public virtual TechType RequiredForUnlock => dependency == TechType.Unobtanium ? TechType.None : dependency;

    public virtual bool UnlockedAtStart =>
        dependency != TechType.Unobtanium && RequiredForUnlock == TechType.None &&
        (GetGadget<ScanningGadget>() == null || GetGadget<ScanningGadget>().CompoundTechsForUnlock == null);

    public void preventNaturalUnlock() {
        dependency = TechType.Unobtanium;
    }

    public virtual QuickSlotType QuickSlotType => QuickSlotType.Passive;

    public virtual CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;

    public virtual float CraftingTime => 0f;

    public virtual string[] StepsToFabricatorTab =>
        ["Personal", "Equipment"]; //return new string[]{"DISeamoth"};//new string[]{"SeamothModules"};

    public virtual TechGroup GroupForPDA => TechGroup.Personal;

    public virtual TechCategory CategoryForPDA => isArmor ? TechCategory.Equipment : TechCategory.Tools;

    public virtual Vector2int SizeInInventory => new(1, 1);

    protected virtual Sprite GetItemSprite() {
        return TextureManager.getSprite(ownerMod, "Textures/Items/" + ObjectUtil.formatFileName(this));
    }

    public virtual GameObject GetGameObject() {
        return ObjectUtil.getModPrefabBaseObject(this);
    }

    public virtual void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public Assembly getOwnerMod() {
        return ownerMod;
    }

    public string ClassID => Info.ClassID;

    public virtual bool isResource() {
        return false;
    }

    public virtual string getTextureFolder() {
        return "Items/Tools";
    }

    public Sprite getIcon() {
        return GetItemSprite();
    }

    protected RecipeData GetBlueprintRecipe() {
        return new RecipeData {
            Ingredients = RecipeUtil.buildRecipeList(recipe),
            craftAmount = 1,
            LinkedItems = getAuxCrafted(),
        };
    }

    public virtual List<TechType> getAuxCrafted() {
        return [];
    }
}