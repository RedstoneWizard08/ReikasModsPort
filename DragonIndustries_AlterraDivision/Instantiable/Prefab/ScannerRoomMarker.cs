using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using Nautilus.Handlers;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public sealed class ScannerRoomMarker : CustomPrefab {
    private readonly System.Reflection.Assembly ownerMod;
    public readonly TechType markerType;


    [SetsRequiredMembers]
    public ScannerRoomMarker(TechType markAs) : base("ScannerRoomMarker_" + markAs.AsString(), "", "") {
        markerType = markAs;
        ownerMod = SNUtil.tryGetModDLL();

        SpriteHandler.RegisterSprite(
            markerType,
            TextureManager.getSprite(ownerMod, "Textures/" + markerType.AsString())
        );
        SetGameObject(GetGameObject);
    }

    private GameObject GetGameObject() {
        GameObject world = new GameObject("ScannerRoomMarker(Clone)");
        world.EnsureComponent<TechTag>().type = markerType;
        PrefabIdentifier pi = world.EnsureComponent<PrefabIdentifier>();
        pi.ClassId = Info.ClassID;
        ResourceTracker tgt = world.EnsureComponent<ResourceTracker>();
        tgt.techType = markerType;
        tgt.overrideTechType = markerType;
        tgt.prefabIdentifier = pi;
        world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
        return world;
    }
}