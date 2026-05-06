using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class WorldCollectedItem : CustomPrefab, DIPrefab<StringPrefabContainer> {
    private readonly List<PlannedIngredient> recipe = [];
    public readonly string id;

    public Sprite sprite = null;
    public Vector2int inventorySize = new(1, 1);
    public readonly List<PlannedIngredient> byproducts = [];
    public Action<Renderer> renderModify = null;

    public float glowIntensity { get; set; }
    public StringPrefabContainer baseTemplate { get; set; }

    protected readonly Assembly ownerMod;

    [SetsRequiredMembers]
    public WorldCollectedItem(XMLLocale.LocaleEntry e, string template) : this(e.key, e.name, e.desc, template) {
    }

    [SetsRequiredMembers]
    public WorldCollectedItem(string id, string name, string desc, string template) : base(id, name, desc) {
        ownerMod = SNUtil.TryGetModDLL();
        // typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
        this.id = id;

        baseTemplate = new StringPrefabContainer(template.Contains("/") ? PrefabData.getPrefabID(template) : template);

        SetGameObject(GetGameObject);

        AddOnRegister(() => { ItemRegistry.instance.addItem(this); });

        Info.WithSizeInInventory(SizeInInventory).WithIcon(getIcon());
    }

    public GameObject GetGameObject() {
        var go = ObjectUtil.getModPrefabBaseObject(this);
        renderModify?.Invoke(go.GetComponentInChildren<Renderer>());
        return go;
    }

    public Assembly getOwnerMod() {
        return ownerMod;
    }

    public string ClassID => Info.ClassID;

    public virtual bool isResource() {
        return true;
    }

    public virtual string getTextureFolder() {
        return "Items/World";
    }

    public virtual void prepareGameObject(GameObject go, Renderer[] r) {
    }

    protected Sprite GetItemSprite() {
        return sprite;
    }

    public Sprite getIcon() {
        return GetItemSprite();
    }

    public Vector2int SizeInInventory => inventorySize;

    public sealed override string ToString() {
        return base.ToString() + " [" + Info.TechType + "] / " + Info.ClassID + " / " + Info.PrefabFileName;
    }
}