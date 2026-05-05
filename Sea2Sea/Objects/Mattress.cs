using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class Mattress : CustomPrefab {
    [SetsRequiredMembers]
    internal Mattress() : base("Mattress", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var go = ObjectUtil.lookupPrefab("c3994649-d0da-4f8c-bb77-1590f50838b9").getChildObject("bed_narrow")
            .clone();
        go.removeChildObject("bed_narrow");
        go.removeChildObject("blanket_narrow");
        go.removeChildObject("end_position");
        go.removeChildObject("obstacle_check");
        go.getChildObject("matress_narrow").transform.localPosition = Vector3.zero;
        go.getChildObject("pillow_01").transform.localPosition = new Vector3(0, 0.11F, -0.67F);
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        go.EnsureComponent<TechTag>().type = Info.TechType;
        return go;
    }

    // protected override void ProcessPrefab(GameObject go) {
    // 	base.ProcessPrefab(go);
    // }
}