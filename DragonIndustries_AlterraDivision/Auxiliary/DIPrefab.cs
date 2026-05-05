using System.Collections.Generic;
using System.Reflection;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public interface DIPrefab<E, T> : DIPrefab<T>
    where T : PrefabReference { //ONLY EVER IMPLEMENT THIS ON MODPREFABS OR SUBCLASSES THEREOF

    E addIngredient(TechType item, int amt);

    E addIngredient(CustomPrefab item, int amt);

    E addIngredient(ItemDef item, int amt);
}

public interface DIPrefab<T> : DIPrefab where T : PrefabReference {
    T baseTemplate { get; set; }
}

public interface DIPrefab {
    float glowIntensity { get; set; }

    string ClassID { get; }

    bool isResource();

    string getTextureFolder();

    void prepareGameObject(GameObject go, Renderer[] r);

    Sprite getIcon();

    Assembly getOwnerMod();
}

public interface MultiTexturePrefab : DIPrefab {
    Dictionary<int, string> getTextureLayers(Renderer r);
}

public sealed class StringPrefabContainer : PrefabReference {
    public readonly string prefab;

    public StringPrefabContainer(string s) {
        prefab = s;
    }

    public string getPrefabID() {
        return prefab;
    }
}

public sealed class ModPrefabContainer : PrefabReference {
    public readonly CustomPrefab prefab;

    public ModPrefabContainer(CustomPrefab s) {
        prefab = s;
    }

    public string getPrefabID() {
        return prefab.Info.ClassID;
    }
}

public sealed class TechTypePrefabContainer : PrefabReference {
    public readonly TechType tech;

    public TechTypePrefabContainer(TechType t) {
        tech = t;
    }

    public string getPrefabID() {
        return CraftData.GetClassIdForTechType(tech);
    }
}

public sealed class ModPrefabTechReference : TechTypeReference {
    public readonly CustomPrefab prefab;

    public ModPrefabTechReference(CustomPrefab s) {
        prefab = s;
    }

    public TechType getTechType() {
        return prefab.Info.TechType;
    }

    public override string ToString() {
        return "ModPrefab " + prefab;
    }
}

public sealed class TechTypeContainer : TechTypeReference {
    public readonly TechType tech;

    public TechTypeContainer(TechType s) {
        tech = s;
    }

    public TechType getTechType() {
        return tech;
    }

    public override string ToString() {
        return "Tech " + tech.AsString();
    }
}

public interface TechTypeReference {
    TechType getTechType();
}

public sealed class PlannedIngredient {
    public readonly TechTypeReference item;
    public int amount;

    public PlannedIngredient(TechTypeReference item, int amt) {
        this.item = item;
        amount = amt;
    }
}