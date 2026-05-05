using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public sealed class ObjectDeleter : CustomPrefab {
    private readonly System.Reflection.Assembly ownerMod;

    [SetsRequiredMembers]
    public ObjectDeleter() : base("ObjectDeleter", "", "") {
        ownerMod = SNUtil.tryGetModDLL(true);
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        GameObject world = new GameObject();
        world.EnsureComponent<ObjectDeleterTag>();
        world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        return world;
    }

    class ObjectDeleterTag : MonoBehaviour {
        void Start() {
            foreach (PrefabIdentifier go in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(
                         transform.position,
                         transform.localScale.x
                     )) {
                //if (go != this) //delete self too
                go.gameObject.destroy(false);
            }
        }
    }
}