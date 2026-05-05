using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public sealed class CompoundResource : CustomPrefab {
    public readonly PrefabReference containedItem;

    [SetsRequiredMembers]
    public CompoundResource(PrefabReference item, int amount, Vector3 scatter) : base(
        "Compound_" + item.getPrefabID(),
        "",
        ""
    ) {
        containedItem = item;
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var world = new GameObject();
        world.EnsureComponent<CompoundResourceTag>();
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        return world;
    }

    private class CompoundResourceTag : MonoBehaviour {
        private void Start() {
        }
    }
}