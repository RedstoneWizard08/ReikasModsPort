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

    private readonly Assembly _ownerMod;
    private readonly List<BiomeBase> _nativeBiomesCave = [];
    private readonly List<BiomeBase> _nativeBiomesSurface = [];

    private static readonly Dictionary<TechType, RetexturedFish> Creatures = new();
    private static readonly Dictionary<string, RetexturedFish> CreatureIDs = new();

    public float ScanTime = 2;
    public int CookableIntoBase = 0;
    private XMLLocale.LocaleEntry _locale;
    public TechType EggBase = TechType.None;
    public float EggScale = 1;
    public float EggMaturationTime = 2400;
    public float AcuSizeScale = 1;
    public bool BigEgg = true;
    public float EggSpawnRate = 0;
    public readonly List<BiomeType> EggSpawns = [];
    public WaterParkCreatureData Data = null;

    [SetsRequiredMembers]
    protected RetexturedFish(XMLLocale.LocaleEntry e, string pfb) : this(e.key, e.name, e.desc, pfb) {
        _locale = e;
    }

    [SetsRequiredMembers]
    protected RetexturedFish(string id, string name, string desc, string pfb) : base(id, name, desc) {
        baseTemplate = new StringPrefabContainer(pfb);
        _ownerMod = SNUtil.TryGetModDLL();
        SetGameObject(GetGameObject);
        // TODO
        // this.SetSpawns(_nativeBiomesSurface.ToArray());

        // typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
        AddOnRegister(() => {
                Info.WithIcon(GetSprite()).WithSizeInInventory(SizeInInventory);
                Creatures[Info.TechType] = this;
                CreatureIDs[Info.ClassID] = this;

                if (_locale != null && !string.IsNullOrEmpty(_locale.pda))
                    SNUtil.AddPdaEntry(
                        this,
                        ScanTime,
                        _locale.getString("category"),
                        _locale.pda,
                        _locale.getString("header"),
                        null
                    );

                if (EggBase != TechType.None) {
                    SNUtil.Log("Creating egg for " + this + " from " + EggBase.AsString());
                    if (EggBase.AsString().EndsWith("egg", StringComparison.InvariantCultureIgnoreCase))
                        throw new Exception("Egg base is invalid - choose the creature not the egg");
                    CustomEgg.CreateAndRegisterEgg(
                        this,
                        EggBase,
                        EggScale,
                        "", // Description
                        BigEgg,
                        e => {
                            e.EggProperties.daysToGrow = EggMaturationTime / 1200;
                            e.EggProperties.initialSize *= AcuSizeScale;
                            e.EggProperties.maxSize *= AcuSizeScale;
                        },
                        EggSpawnRate,
                        EggSpawns.ToArray()
                    );
                }

                //GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, EntitySlot.Type.Creature, LargeWorldEntity.CellLevel.Medium, BiomeType.SeaTreaderPath_OpenDeep_CreatureOnly, 1, 0.15F);
                //GenUtil.registerSlotWorldgen(ClassID, PrefabFileName, TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.GrandReef_TreaderPath, 1, 0.3F);

                CreatureData.behaviourTypeList[Info.TechType] = GetBehavior();

                var basis = CraftData.entClassTechTable.TryGetValue(baseTemplate.prefab, out var tech)
                    ? tech
                    : TechType.None;

                if (basis != TechType.None && BaseBioReactor.charge.TryGetValue(basis, out var charge))
                    BioReactorHandler.SetBioReactorCharge(Info.TechType, charge);

                if (basis != TechType.None && TechData.GetEquipmentType(basis) == EquipmentType.Hand)
                    CraftDataHandler.SetEquipmentType(Info.TechType, EquipmentType.Hand);

                if (basis == TechType.None || CookableIntoBase <= 0 || TechData.GetProcessed(basis) == null) return;
                var cooked = TechData.GetProcessed(basis);
                var cured = SNUtil.GetTechType(("Cured" + cooked).Replace("Cooked", ""));
                CraftDataHandler.SetCookedVariant(Info.TechType, cooked);
                SNUtil.Log("Adding delegate cooking/curing of " + this + " into " + cooked + " & " + cured);

                var rec = new RecipeData();
                rec.Ingredients.Add(new Ingredient(Info.TechType, 1));
                var alt = new DuplicateRecipeDelegateWithRecipe(cooked, rec) {
                    category = TechCategory.CookedFood,
                    group = TechGroup.Survival,
                    craftingType = CraftTree.Type.Fabricator,
                    craftingMenuTree = ["Survival", "CookedFood"],
                    ownerMod = _ownerMod,
                    craftTime = 2, //time not fetchable, not in dict(?!)
                };
                alt.setRecipe(CookableIntoBase);
                alt.unlock = Info.TechType;
                alt.allowUnlockPopups = true;
                alt.Register();
                TechnologyUnlockSystem.instance.addDirectUnlock(Info.TechType, alt.Info.TechType);

                rec = new RecipeData();
                rec.Ingredients.Add(new Ingredient(Info.TechType, 1));
                rec.Ingredients.Add(new Ingredient(TechType.Salt, 1));
                alt = new DuplicateRecipeDelegateWithRecipe(cured, rec) {
                    category = TechCategory.CuredFood,
                    group = TechGroup.Survival,
                    craftingType = CraftTree.Type.Fabricator,
                    craftingMenuTree = ["Survival", "CuredFood"],
                    ownerMod = _ownerMod,
                    craftTime = 2,
                };
                alt.setRecipe(CookableIntoBase);
                alt.unlock = Info.TechType;
                alt.allowUnlockPopups = true;
                alt.Register();
                TechnologyUnlockSystem.instance.addDirectUnlock(Info.TechType, alt.Info.TechType);
            }
        );
    }

    public RetexturedFish AddNativeBiome(BiomeBase b, bool caveOnly = false) {
        _nativeBiomesCave.Add(b);

        if (!caveOnly)
            _nativeBiomesSurface.Add(b);

        return this;
    }

    public bool IsNativeToBiome(Vector3 vec) {
        return IsNativeToBiome(BiomeBase.GetBiome(vec), WorldUtil.isInCave(vec));
    }

    public virtual Vector2int SizeInInventory => new(1, 1);

    public bool IsNativeToBiome(BiomeBase b, bool cave) {
        return (cave ? _nativeBiomesCave : _nativeBiomesSurface).Contains(b);
    }

    public string GetPrefabID() {
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
        var world = ObjectUtil.getModPrefabBaseObject(this);
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        world.SetActive(true);

        if (Data != null) {
            world.EnsureComponent<WaterParkCreature>().data = Data;
        }

        return world;
    }

    public virtual Sprite getIcon() {
        return GetItemSprite();
    }

    protected virtual Sprite GetItemSprite() {
        return TextureManager.getSprite(_ownerMod, "Textures/Items/" + ObjectUtil.formatFileName(this));
    }

    public virtual Sprite GetSprite() {
        return GetItemSprite();
    }

    public virtual void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public sealed override string ToString() {
        return base.ToString() + " [" + Info.TechType + "] / " + Info.ClassID + " / " + Info.PrefabFileName;
    }

    public Assembly getOwnerMod() {
        return _ownerMod;
    }

    public abstract BehaviourType GetBehavior();

    public static RetexturedFish GetFish(string id) {
        return CreatureIDs.TryGetValue(id, out var fish) ? fish : null;
    }

    public static RetexturedFish GetFish(TechType tt) {
        return Creatures.TryGetValue(tt, out var fish) ? fish : null;
    }
}