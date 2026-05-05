using System;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

[Obsolete]
public class CrashZoneSanctuaryCoralSheet : CustomPrefab {
    [SetsRequiredMembers]
    internal CrashZoneSanctuaryCoralSheet() : base("CrashZoneSanctuaryCoralSheet", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        GameObject go = ObjectUtil.createWorldObject("");
        go.removeComponent<LiveMixin>();
        go.removeComponent<Collider>();
        go.removeComponent<PlantBehaviour>();
        go.removeComponent<FMOD_StudioEventEmitter>();
        go.removeComponent<CoralBlendWhite>();
        go.removeComponent<Light>();
        Renderer r = go.GetComponentInChildren<Renderer>();
        RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/SanctuaryCoral");
        r.material.DisableKeyword("MARMO_EMISSION");
        r.receiveShadows = false;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        go.transform.localScale = new Vector3(10, 1, 10);
        return go;
    }
}