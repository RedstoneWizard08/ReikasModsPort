using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public sealed class ObjectDeleter : CustomPrefab {
    private readonly System.Reflection.Assembly ownerMod;

    [SetsRequiredMembers]
    public ObjectDeleter() : base("ObjectDeleter", "", "") {
        ownerMod = SNUtil.TryGetModDLL(true);
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var world = new GameObject();
        world.EnsureComponent<ObjectDeleterTag>();
        world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        return world;
    }

    private class ObjectDeleterTag : MonoBehaviour {
        private void Start() {
            foreach (var go in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(
                         transform.position,
                         transform.localScale.x
                     )) {
                //if (go != this) //delete self too
                go.gameObject.destroy(false);
            }
        }
    }
}