using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public abstract class RetexturedFish : CustomPrefab, DIPrefab<StringPrefabContainer> {
    public float glowIntensity { get; set; }
    public StringPrefabContainer baseTemplate { get; set; }

    private readonly Assembly ownerMod;

    private readonly List<LootDistributionData.BiomeData> nativeBiomesCave = new List<LootDistributionData.BiomeData>();

    private readonly List<LootDistributionData.BiomeData> nativeBiomesSurface =
        new List<LootDistributionData.BiomeData>();

    private static readonly Dictionary<TechType, RetexturedFish> creatures = new Dictionary<TechType, RetexturedFish>();
    private static readonly Dictionary<string, RetexturedFish> creatureIDs = new Dictionary<string, RetexturedFish>();

    public float scanTime = 2;
    public int cookableIntoBase = 0;
    private XMLLocale.LocaleEntry locale;
    public TechType eggBase = TechType.None;
    public float eggScale = 1;
    public float eggMaturationTime = 2400;
    public float acuSizeScale = 1;
    public bool bigEgg = true;
    public float eggSpawnRate = 0;
    public readonly List<BiomeType> eggSpawns = new List<BiomeType>();
    public WaterParkCreatureData data = null;

    [SetsRequiredMembers]
    protected RetexturedFish(XMLLocale.LocaleEntry e, string pfb) : this(e.key, e.name, e.desc, pfb) {
        locale = e;
    }

    [SetsRequiredMembers]
    protected RetexturedFish(string id, string name, string desc, string pfb) : base(id, name, desc) {
        baseTemplate = new StringPrefabContainer(pfb);
        ownerMod = SNUtil.tryGetModDLL();
        Info.WithIcon(getSprite());
        this.SetGameObject(GetGameObject);
        this.SetSpawns(nativeBiomesSurface.ToArray());
        Info.WithSizeInInventory(SizeInInventory);

        // typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
        AddOnRegister(() => {
                creatures[Info.TechType] = this;
                creatureIDs[Info.ClassID] = this;

                if (locale != null && !string.IsNullOrEmpty(locale.pda))
                    SNUtil.addPDAEntry(
                        this,
                        scanTime,
                        locale.getString("category"),
                        locale.pda,
                        locale.getString("header"),
                        null
                    );

                if (eggBase != TechType.None) {
                    SNUtil.log("Creating egg for " + this + " from " + eggBase.AsString());
                    if (eggBase.AsString().EndsWith("egg", StringComparison.InvariantCultureIgnoreCase))
                        throw new Exception("Egg base is invalid - choose the creature not the egg");
                    CustomEgg.createAndRegisterEgg(
                        this,
                        eggBase,
                        eggScale,
                        "", // Description
                        bigEgg,
                        e => {
                            e.eggProperties.daysToGrow = eggMaturationTime / 1200;
                            e.eggProperties.initialSize *= acuSizeScale;
                            e.eggProperties.maxSize *= acuSizeScale;
                        },
                        eggSpawnRate,
                        eggSpawns.ToArray()
                    );
                }

                //GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, EntitySlot.Type.Creature, LargeWorldEntity.CellLevel.Medium, BiomeType.SeaTreaderPath_OpenDeep_CreatureOnly, 1, 0.15F);
                //GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.GrandReef_TreaderPath, 1, 0.3F);

                CreatureData.behaviourTypeList[Info.TechType] = this.getBehavior();

                TechType basis = CraftData.entClassTechTable.ContainsKey(baseTemplate.prefab)
                    ? CraftData.entClassTechTable[baseTemplate.prefab]
                    : TechType.None;
                if (basis != TechType.None && BaseBioReactor.charge.ContainsKey(basis))
                    BioReactorHandler.SetBioReactorCharge(Info.TechType, BaseBioReactor.charge[basis]);

                if (basis != TechType.None && TechData.GetEquipmentType(basis) != null &&
                    TechData.GetEquipmentType(basis) == EquipmentType.Hand)
                    CraftDataHandler.SetEquipmentType(Info.TechType, EquipmentType.Hand);

                // TODO
                // if (basis != TechType.None && cookableIntoBase > 0 && TechData.cookedCreatureList.ContainsKey(basis)) {
                //     TechType cooked = TechData.cookedCreatureList[basis];
                //     TechType cured = SNUtil.getTechType(("Cured" + cooked).Replace("Cooked", ""));
                //     CraftDataHandler.SetCookedVariant(Info.TechType, cooked);
                //     SNUtil.log("Adding delegate cooking/curing of " + this + " into " + cooked + " & " + cured);
                //
                //     RecipeData rec = new RecipeData();
                //     rec.Ingredients.Add(new Ingredient(Info.TechType, 1));
                //     DuplicateRecipeDelegateWithRecipe alt = new DuplicateRecipeDelegateWithRecipe(cooked, rec);
                //     alt.category = TechCategory.CookedFood;
                //     alt.group = TechGroup.Survival;
                //     alt.craftingType = CraftTree.Type.Fabricator;
                //     alt.craftingMenuTree = new string[] { "Survival", "CookedFood" };
                //     alt.ownerMod = ownerMod;
                //     alt.craftTime = 2; //time not fetchable, not in dict(?!)
                //     alt.setRecipe(cookableIntoBase);
                //     alt.unlock = Info.TechType;
                //     alt.allowUnlockPopups = true;
                //     alt.Register();
                //     TechnologyUnlockSystem.instance.addDirectUnlock(Info.TechType, alt.Info.TechType);
                //
                //     rec = new RecipeData();
                //     rec.Ingredients.Add(new Ingredient(Info.TechType, 1));
                //     rec.Ingredients.Add(new Ingredient(TechType.Salt, 1));
                //     alt = new DuplicateRecipeDelegateWithRecipe(cured, rec);
                //     alt.category = TechCategory.CuredFood;
                //     alt.group = TechGroup.Survival;
                //     alt.craftingType = CraftTree.Type.Fabricator;
                //     alt.craftingMenuTree = new string[] { "Survival", "CuredFood" };
                //     alt.ownerMod = ownerMod;
                //     alt.craftTime = 2;
                //     alt.setRecipe(cookableIntoBase);
                //     alt.unlock = Info.TechType;
                //     alt.allowUnlockPopups = true;
                //     alt.Register();
                //     TechnologyUnlockSystem.instance.addDirectUnlock(Info.TechType, alt.Info.TechType);
                // }
            }
        );
    }

    public RetexturedFish addNativeBiome(BiomeType b, bool caveOnly = false) {
        nativeBiomesCave.Add(
            new LootDistributionData.BiomeData {
                biome = b
            }
        );
        if (!caveOnly)
            nativeBiomesSurface.Add(
                new LootDistributionData.BiomeData {
                    biome = b
                }
            );
        return this;
    }

    // public bool isNativeToBiome(Vector3 vec) {
    //     return this.isNativeToBiome(BiomeBase.getBiome(vec), WorldUtil.isInCave(vec));
    // }

    public virtual Vector2int SizeInInventory {
        get { return new Vector2int(1, 1); }
    }

    public bool isNativeToBiome(BiomeType b, bool cave) {
        return (cave ? nativeBiomesCave : nativeBiomesSurface).Any(it => it.biome == b);
    }

    public string getPrefabID() {
        return Info.ClassID;
    }

    public string ClassID => Info.ClassID;

    public bool isResource() {
        return false;
    }

    public virtual string getTextureFolder() {
        return "Creature";
    }

    public virtual GameObject GetGameObject() {
        GameObject world = ObjectUtil.getModPrefabBaseObject(this);
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        world.SetActive(true);

        if (data != null) {
            world.EnsureComponent<WaterParkCreature>().data = data;
        }

        return world;
    }

    public virtual Sprite getIcon() {
        return this.GetItemSprite();
    }

    protected virtual Sprite GetItemSprite() {
        return TextureManager.getSprite(ownerMod, "Textures/Items/" + ObjectUtil.formatFileName(this));
    }

    public virtual Sprite getSprite() {
        return this.GetItemSprite();
    }

    public virtual void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public sealed override string ToString() {
        return base.ToString() + " [" + Info.TechType + "] / " + Info.ClassID + " / " + Info.PrefabFileName;
    }

    public Assembly getOwnerMod() {
        return ownerMod;
    }

    public abstract BehaviourType getBehavior();

    public static RetexturedFish getFish(string id) {
        return creatureIDs.ContainsKey(id) ? creatureIDs[id] : null;
    }

    public static RetexturedFish getFish(TechType tt) {
        return creatures.ContainsKey(tt) ? creatures[tt] : null;
    }
}