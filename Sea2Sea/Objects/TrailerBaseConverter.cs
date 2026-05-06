using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea;

public class TrailerBaseConverter : CustomPrefab {
    [SetsRequiredMembers]
    internal TrailerBaseConverter() : base("TrailerBaseConverter", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var go = new GameObject();
        go.EnsureComponent<TrailerBaseConverterTag>();
        go.EnsureComponent<TechTag>().type = Info.TechType;
        go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
        return go;
    }

    private class TrailerBaseConverterTag : MonoBehaviour {
        private static readonly HashSet<string> darken = [
            "BaseCorridorRoomGenericInteriorConnection",
            "BaseRoomGenericInteriorCoverTop01",
            "BaseInteriorRoomGenericWallmods02",
            //"Bio_Reactor_mesh_geo",
            "BaseRoomGenericInteriorCeilingmods02",
            "BaseCorridorXShapeInterior",
            "BaseRoomCoverTop",
            "BaseCorridorInteriorWallHatch",
        ];

        private Text text;
        private Renderer[] baseParts;

        private void Update() {
            if (text) {
                text.text = "<color=#ff0000>ERROR\n\nREACTOR VESSEL\nDAMAGED</color>";
                return;
            }

            var go = WorldUtil.getClosest<BaseBioReactorGeometry>(C2CHooks.trailerBaseBioreactor);
            if (go && Vector3.Distance(go.transform.position, C2CHooks.trailerBaseBioreactor) < 5F) {
                var child = go.gameObject.getChildObject("UI/Canvas/Text");
                text = child.GetComponent<Text>();
                go.gameObject.removeChildObject("Bio_reactor/Bio_Reactor_glass_geo");
                baseParts = go.gameObject.FindAncestor<SeabaseReconstruction.WorldgenSeabaseController>()
                    .GetComponentsInChildren<Renderer>(true);
                //SNUtil.log("Checking for decayed textures for "+baseParts.toDebugString(), SNUtil.diDLL);
                foreach (var r in baseParts) {
                    if (!r)
                        continue;
                    foreach (var m in r.materials) {
                        if (!m)
                            continue;
                        var rn = r.name;
                        if (rn == "LODs")
                            rn = r.transform.parent.name;
                        rn = rn.Replace("(Clone)", "");
                        if (darken.Contains(rn) ||
                            (r.name == "Bio_Reactor_mesh_geo" && m.name == "Bio_Reactor (Instance)")) {
                            m.SetColor("_SpecColor", Color.black);
                            m.SetColor("_Color", new Color(0.38F, 0.43F, 0.48F, 1));
                        }

                        m.DisableKeyword("MARMO_EMISSION");
                        var refName = m.mainTexture && m.mainTexture.name != null ? m.mainTexture.name : null;
                        if (refName != null) {
                            refName = refName.Replace(" (Instance)", "").Replace("_LOD1", "").Replace("_LOD2", "")
                                .Replace("_LOD3", "").ToLowerInvariant();
                            refName = refName.Replace("base", "base_abandoned")
                                .Replace("submarine", "submarine_abandoned");
                            //refName = refName.Replace("exterrior", "exterior").Replace("wallmods", "generic_wallmods");
                        }

                        if (m.IsKeywordEnabled("MARMO_SIMPLE_GLASS")) {
                            if (m.mainTexture == null) {
                                switch (r.transform.parent.name) {
                                    case "BaseRoomGenericInteriorWindowSide01":
                                        refName = "base_abandoned_interior_room_generic_window_side_01_glass";
                                        break;
                                    case "BaseCorridorhIShapeGlass01Exterior":
                                        //refName = "base_abandoned_interior_room_generic_window_side_01_glass";
                                        break;
                                }
                            } else if (m.name == "Base_interior_window_side_01_glass") {
                                refName = "base_abandoned_interior_room_generic_window_side_01_glass";
                            }
                        }

                        if (refName == "starship_work_desk_01") {
                            refName = "starship_work_desk_01_empty";
                        } else if (refName == "base_abandoned_interior_room_generic_window_side_01_glass") {
                            float a = 0;
                            switch (Random.Range(0, 3)) {
                                case 0:
                                    a = 0.33F;
                                    refName = "base_interior_window_side_01_glass";
                                    break;
                                case 1:
                                    a = 1.5F;
                                    refName = "base_abandoned_room_generic_wall_frame_02_glass";
                                    break;
                                case 2:
                                    a = 1.25F;
                                    refName = "base_abandoned_room_generic_wall_frame_02_glass_broken";
                                    break;
                            }

                            SNUtil.Log("Set glass ref tex to " + refName);
                            if (a > 0) {
                                var c = m.GetColor("_Color");
                                m.SetColor("_Color", new Color(c.r, c.g, c.b, a));
                            }
                        }

                        if (!string.IsNullOrEmpty(refName)) {
                            //SNUtil.log("Checking for decayed textures for "+r.gameObject.GetFullHierarchyPath()+" >>> "+refName, SNUtil.diDLL);
                            if (SeaToSeaMod.HasDegasiBaseTextures(refName)) {
                                HashSet<string> found = [];
                                foreach (var tex in m.GetTexturePropertyNames()) {
                                    var img = SeaToSeaMod.GetDegasiBaseTexture(refName, tex);
                                    if (img != null) {
                                        m.SetTexture(tex, img);
                                        found.Add(tex);
                                    }
                                }

                                if (found.Count == 0)
                                    SNUtil.Log("Found no decayed textures of " + refName + ", even with mappings");
                                //SNUtil.log("Decayed textures of "+refName+" in "+r.gameObject.GetFullHierarchyPath()+": "+found.toDebugString());
                            } else {
                                SNUtil.Log("Found no decayed textures of " + refName);
                            }
                        }
                    }

                    r.UpdateGIMaterials();
                }
            }
        }
    }
}