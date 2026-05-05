using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class StepCaveTunnelAtmo : CustomPrefab {
    [SetsRequiredMembers]
    internal StepCaveTunnelAtmo() : base("StepCaveTunnelAtmo", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var world = ObjectUtil.createWorldObject("b179b366-4342-4545-aa4d-a86ad88b780e");
        world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
        return world;
    }
}