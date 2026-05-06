using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class PickedUpAsOtherItem : CustomPrefab {
    protected readonly TechType template;

    private static readonly Dictionary<TechType, List<PickedUpAsOtherItem>> items = new();

    private static readonly Dictionary<TechType, PickedUpAsOtherItem> techMap = new();

    [SetsRequiredMembers]
    public PickedUpAsOtherItem(string classID, string baseTemplate) : this(
        classID,
        CraftData.entClassTechTable[baseTemplate]
    ) {
    }

    [SetsRequiredMembers]
    public PickedUpAsOtherItem(string classID, TechType tt) : base(classID, "", "") {
        template = tt;

        var li = items.ContainsKey(tt) ? items[tt] : [];
        li.Add(this);
        items[tt] = li;

        AddOnRegister(() => { techMap[Info.TechType] = this; });

        SetGameObject(GetGameObject);
    }

    public virtual GameObject GetGameObject() {
        var world = ObjectUtil.createWorldObject(template);
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        var pp = world.EnsureComponent<Pickupable>();
        pp.SetTechTypeOverride(template);
        prepareGameObject(world);
        return world;
    }

    protected virtual void prepareGameObject(GameObject go) {
    }

    public override string ToString() {
        return $"[PickedUpAsOtherItem Template={template}x{getNumberCollectedAs()}]";
    }


    public TechType getTemplate() {
        return template;
    }

    public virtual int getNumberCollectedAs() {
        return 1;
    }

    public static PickedUpAsOtherItem getPickedUpAsOther(TechType tt) {
        return techMap.ContainsKey(tt) ? techMap[tt] : null;
    }

    public static void updateLocale() {
        foreach (var li in items.Values) {
            foreach (var d in li) {
                CustomLocaleKeyDatabase.registerKey(d.Info.TechType.AsString(), Language.main.Get(d.template));
                CustomLocaleKeyDatabase.registerKey(
                    "Tooltip_" + d.Info.TechType.AsString(),
                    Language.main.Get("Tooltip_" + d.template.AsString())
                );
                SNUtil.Log(
                    "Relocalized otherpickup " + d + " > " + d.Info.TechType.AsString() + " > " +
                    Language.main.Get(d.Info.TechType),
                    SNUtil.DiDLL
                );
            }
        }
    }
}