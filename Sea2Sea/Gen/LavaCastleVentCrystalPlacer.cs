using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class LavaCastleVentCrystalPlacer : CustomPrefab {
    [SetsRequiredMembers]
    internal LavaCastleVentCrystalPlacer() : base("LavaCastleVentCrystalPlacer", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        GameObject go = new GameObject("LavaCastleVentCrystalPlacer");
        go.EnsureComponent<LavaCastleVentCrystalConverter>();
        go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        go.EnsureComponent<TechTag>().type = Info.TechType;
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
        return go;
    }
}

internal class LavaCastleVentCrystalConverter : MonoBehaviour {
    void Update() {
        if ((transform.position - Player.main.transform.position).sqrMagnitude <= 90000) {
            float ch = SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.HARDMODE) ? 0.15F : 0.25F;
            GameObject azur = null;
            if (UnityEngine.Random.Range(0F, 1F) < ch) {
                azur = ObjectUtil.createWorldObject(
                    CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).ClassID
                );
                azur.transform.rotation = transform.rotation;
                azur.transform.position = transform.position;
                azur.SetActive(true);
            }

            SNUtil.log(
                "Converted lava castle vent placeholder @ " + transform.position + ": " + (azur ? azur.name : "NULL")
            );
            gameObject.destroy(false);
        }
    }
}