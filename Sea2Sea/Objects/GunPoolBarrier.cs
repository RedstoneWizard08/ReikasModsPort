using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class GunPoolBarrier : LockedPrecursorDoor {
    [SetsRequiredMembers]
    internal GunPoolBarrier() : base(
        "GunPoolDoor",
        PrecursorKeyTerminal.PrecursorKeyType.PrecursorKey_Orange,
        new PositionedPrefab(
            "",
            new Vector3(481.808F, -125.032F, 1257.852F),
            Quaternion.Euler(0, 20, 0),
            Vector3.one * 4
        ),
        new PositionedPrefab("", new Vector3(460.4F, -93.85F, 1236.9F), Quaternion.Euler(0, 200, 0))
    ) {
    }

    public override GameObject GetGameObject() {
        GameObject go = base.GetGameObject();
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
        return go;
    }
}