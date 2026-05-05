using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class DeadMelon : CustomPrefab {
    [SetsRequiredMembers]
    internal DeadMelon() : base("DeadMelon", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        GameObject go = ObjectUtil.createWorldObject("e9445fdf-fbae-49dc-a005-48c05bf9f401");
        go.removeComponent<Pickupable>();
        go.removeComponent<PickPrefab>();
        go.removeComponent<LiveMixin>();
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
        go.EnsureComponent<TechTag>().type = TechType.MelonPlant;
        return go;
    }
}