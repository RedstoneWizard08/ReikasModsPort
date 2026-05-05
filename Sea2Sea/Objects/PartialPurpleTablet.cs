using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class PartialPurpleTablet : CustomPrefab {
    private readonly bool includePartA;
    private readonly bool includePartB;

    [SetsRequiredMembers]
    internal PartialPurpleTablet(bool includeA, bool includeB) : base(
        "PartialPurpleTablet_" + (includeA ? "A" : "") + (includeB ? "B" : ""),
        "",
        ""
    ) {
        includePartA = includeA;
        includePartB = includeB;

        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        GameObject go = ObjectUtil.createWorldObject("83b61f89-1456-4ff5-815a-ecdc9b6cc9e4");
        GameObject mdl = go.getChildObject("precursor_key_cracked_01");
        if (!includePartA)
            mdl.removeChildObject("PrecursorKeyCracked_01");
        if (!includePartB)
            mdl.removeChildObject("PrecursorKeyCracked_02");
        go.EnsureComponent<TechTag>().type = TechType.PrecursorKey_PurpleFragment;
        return go;
    }
}