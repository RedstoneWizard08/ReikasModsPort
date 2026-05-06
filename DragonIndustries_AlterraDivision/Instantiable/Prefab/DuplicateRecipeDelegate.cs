using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class DuplicateRecipeDelegate : CustomPrefab, DuplicateItemDelegate {
    public readonly CustomPrefab prefab;
    public readonly TechType basis;
    public readonly string nameSuffix;

    public Sprite sprite;
    public TechType unlock = TechType.None;
    public TechCategory category = TechCategory.Misc;
    public TechGroup group = TechGroup.Uncategorized;
    public Assembly ownerMod;
    public bool allowUnlockPopups = false;

    private static readonly Dictionary<TechType, List<DuplicateItemDelegate>> delegates = new();

    private static readonly Dictionary<TechType, DuplicateItemDelegate> delegateItems = new();

    [SetsRequiredMembers]
    public DuplicateRecipeDelegate(CustomPrefab s, string suff = "") : base(
        s.Info.ClassID + "_delegate" + getIndexSuffix(s.Info.TechType),
        "" + suff,
        ""
    ) {
        basis = s.Info.TechType;
        prefab = s;
        unlock = s.GetGadget<ScanningGadget>().RequiredForUnlock;
        group = s.GetGadget<ScanningGadget>().GroupForPda;
        category = s.GetGadget<ScanningGadget>().CategoryForPda;
        nameSuffix = suff;
        if (s is DIPrefab<PrefabReference> diPrefab)
            ownerMod = diPrefab.getOwnerMod();
        AddOnRegister(onPatched);

        Info.WithIcon(GetItemSprite()).WithSizeInInventory(SizeInInventory);
        this.SetPdaGroupCategory(GroupForPDA, CategoryForPDA);

        if (GetBlueprintRecipe() != null) this.SetRecipe(GetBlueprintRecipe());
        SetGameObject(GetGameObject);
    }

    [SetsRequiredMembers]
    public DuplicateRecipeDelegate(TechType from, string suff = "") : base(
        from.AsString() + "_delegate" + getIndexSuffix(from),
        "",
        ""
    ) {
        basis = from;
        prefab = null;
        sprite = SpriteManager.Get(from);
        nameSuffix = suff;
        AddOnRegister(onPatched);

        Info.WithIcon(GetItemSprite()).WithSizeInInventory(SizeInInventory);
        this.SetPdaGroupCategory(GroupForPDA, CategoryForPDA);

        if (GetBlueprintRecipe() != null) this.SetRecipe(GetBlueprintRecipe());
        SetGameObject(GetGameObject);
    }

    private void onPatched() {
        addDelegate(this);
        if (ownerMod == null)
            throw new Exception("Delegate item " + basis + "/" + Info.ClassID + " has no source mod!");
        if (sprite == null)
            throw new Exception("Delegate item " + basis + "/" + Info.ClassID + " has no sprite!");
    }

    private static string getIndexSuffix(TechType tt) {
        var count = delegates.ContainsKey(tt) ? delegates[tt].Count : 0;
        return count <= 0 ? "" : "_" + (count + 1).ToString();
    }

    public static void addDelegate(DuplicateItemDelegate d) {
        var tt = d.getBasis();
        // FieldInfo fi = typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic);
        // ModPrefab pfb = tt.getModPrefabByTechType();
        // Assembly
        //     a = pfb == null
        //         ? /*SNUtil.gameDLL*/null
        //         : (Assembly)fi.GetValue(
        //             pfb
        //         ); //SML does not recognize game DLL and looks for a mod with that DLL, fails, and says error
        // if (a == null)
        //     a = d.getOwnerMod();
        // fi.SetValue(d, a);
        // fi = typeof(TechTypeHandler).GetField("TechTypesAddedBy", BindingFlags.Static | BindingFlags.NonPublic);
        // Dictionary<TechType, Assembly> dict = (Dictionary<TechType, Assembly>)fi.GetValue(null);
        // TechType ttsrc = ((Spawnable)d).TechType;
        // dict[ttsrc] = a;
        var li = delegates.ContainsKey(tt) ? delegates[tt] : [];
        li.Add(d);
        delegates[tt] = li;
        // delegateItems.Add(ttsrc, d);
        SNUtil.Log("Registering delegate item " + d + " ref pfb=" + tt, d.getOwnerMod());
    }

    public static IEnumerable<DuplicateItemDelegate> getDelegates(TechType of) {
        return delegates.ContainsKey(of)
            ? (IEnumerable<DuplicateItemDelegate>)delegates[of].AsReadOnly()
            : new List<DuplicateItemDelegate>();
    }

    public static bool isDelegateItem(TechType tt) {
        return delegateItems.ContainsKey(tt);
    }

    public static DuplicateItemDelegate getDelegateFromTech(TechType tt) {
        return delegateItems[tt];
    }

    public static void updateLocale() {
        foreach (var li in delegates.Values) {
            foreach (var d in li) {
                if (d.getPrefab() == null || !string.IsNullOrEmpty(d.getNameSuffix())) {
                    var tt = d.getBasis();
                    var dt = ((CustomPrefab)d).Info.TechType;
                    CustomLocaleKeyDatabase.registerKey(dt.AsString(), Language.main.Get(tt) + d.getNameSuffix());
                    CustomLocaleKeyDatabase.registerKey("Tooltip_" + dt.AsString(), d.getTooltip());
                    SNUtil.Log(
                        "Relocalized " + d + " > " + dt.AsString() + " > " + Language.main.Get(dt),
                        d.getOwnerMod()
                    );
                }
            }
        }
    }

    public string getTooltip() {
        return Language.main.Get("Tooltip_" + basis.AsString());
    }

    public virtual TechGroup GroupForPDA => group;

    public virtual TechCategory CategoryForPDA => category;

    public virtual TechType RequiredForUnlock => unlock;

    public virtual GameObject GetGameObject() {
        return ObjectUtil.createWorldObject(CraftData.GetClassIdForTechType(basis), true, false);
    }

    protected virtual Sprite GetItemSprite() {
        return sprite;
    }

    public override sealed string ToString() {
        return base.ToString() + " [" + Info.TechType + "] / " + Info.ClassID + " / " + Info.PrefabFileName + " in " +
               GroupForPDA +
               "/" + CategoryForPDA;
    }

    public virtual Vector2int SizeInInventory => TechData.GetItemSize(basis);

    public virtual string getNameSuffix() {
        return nameSuffix;
    }

    public virtual CustomPrefab getPrefab() {
        return prefab;
    }

    public virtual TechType getBasis() {
        return basis;
    }

    protected virtual RecipeData GetBlueprintRecipe() {
        return null;
    }

    public virtual Assembly getOwnerMod() {
        return ownerMod;
    }

    public virtual bool allowTechUnlockPopups() {
        return allowUnlockPopups;
    }
}

public interface DuplicateItemDelegate {
    string getNameSuffix();

    CustomPrefab getPrefab();

    TechType getBasis();

    string getTooltip();

    Assembly getOwnerMod();

    bool allowTechUnlockPopups();
}