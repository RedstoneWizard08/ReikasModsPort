using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public static class AcuTheming {
    internal static readonly string AcuDecoSlotName = "ACUDecoHolder";

    private static readonly Dictionary<BiomeRegions.RegionType, WeightedRandom<AcuPropDefinition>> PropTypes = new();
    private static readonly Dictionary<BiomeRegions.RegionType, Texture2D> FloorTextures = new();

    private static readonly Dictionary<string, MaterialPropertyDefinition> TerrainGrassTextures = new();

    private static readonly string RootCachePath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
        "GrassTex"
    );

    private static readonly int Fresnel = Shader.PropertyToID("_Fresnel");
    private static readonly int Shininess = Shader.PropertyToID("_Shininess");
    private static readonly int SpecInt = Shader.PropertyToID("_SpecInt");
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");

    static AcuTheming() {
        foreach (var f in typeof(BiomeRegions).GetFields()) {
            if (f.IsStatic && f.FieldType == typeof(BiomeRegions.RegionType)) {
                var r = (BiomeRegions.RegionType)f.GetValue(null);
                var tex = TextureManager.getTexture(AqueousEngineeringMod.modDLL, "Textures/ACUFloor/" + r.ID);
                SetFloorTexture(r, tex);
            }
        }

        RegisterGrassProp(BiomeRegions.Kelp, null, 25, 0.5F);
        RegisterGrassProp(BiomeRegions.RedGrass, "Coral_reef_red_seaweed_03", 25, 0.5F);
        RegisterGrassProp(BiomeRegions.RedGrass, "Coral_reef_red_seaweed_02", 25, 0.5F);
        RegisterGrassProp(BiomeRegions.Koosh, "Coral_reef_small_deco_03_billboards", 15, 0.5F);
        RegisterGrassProp(BiomeRegions.Koosh, "coral_reef_grass_03_02", 15, 0.5F);
        //registerGrassProp(BiomeRegions.GrandReef, "coral_reef_grass_11_02_gr", 25, 0.5F);
        RegisterGrassProp(BiomeRegions.GrandReef, "coral_reef_grass_11_02", 12, 0.5F);
        RegisterGrassProp(BiomeRegions.GrandReef, "coral_reef_grass_07", 25, 0.5F);
        //registerGrassProp(BiomeRegions.GrandReef, "coral_reef_grass_07_gr", 25, 0.5F);
        //registerGrassProp(BiomeRegions.GrandReef, "coral_reef_grass_10_gr", 25, 0.5F);
        RegisterGrassProp(BiomeRegions.BloodKelp, "coral_reef_grass_07_bk", 25, 0.5F);
        RegisterGrassProp(BiomeRegions.LostRiver, "coral_reef_grass_11_03_lr", 25, 0.5F);
        RegisterGrassProp(BiomeRegions.LavaZone, "coral_reef_grass_10_lava", 25, 0.5F);

        //registerProp(BiomeRegions.Koosh, "eb5ea858-930d-4272-91b5-e9ebe2286ca8", 25, 0.5F);

        //foreach (string pfb in VanillaFlora.BLOOD_GRASS.getPrefabs(false, true))
        //	registerProp(BiomeRegions.RedGrass, pfb, 15);

        RegisterProp(BiomeRegions.Mushroom, "961194a9-e88b-40d7-900d-a48c5b739352", 5, false, 0.4F);
        RegisterProp(BiomeRegions.Mushroom, "fe145621-5b25-4000-a3dd-74c1aaa961e2", 5, false, 0.4F);
        RegisterProp(BiomeRegions.Mushroom, "f3de21af-550b-4901-a6e8-e45e31c1509d", 5, false, 0.4F);
        RegisterProp(BiomeRegions.Mushroom, "5086a02a-ea6d-41ba-90c3-ea74d97cf6b5", 5, false, 0.4F);
        RegisterProp(BiomeRegions.Mushroom, "7c7e0e95-8311-4ee0-80dd-30a61b151161", 5, false, 0.4F);

        RegisterProp(BiomeRegions.BloodKelp, "7bfe0629-a008-43b8-bd16-d69ad056769f", 15, true, PrepareBloodTendril);
        RegisterProp(BiomeRegions.BloodKelp, "e291d076-bf95-4cdd-9dd9-6acd37566cf6", 15, true, PrepareBloodTendril);
        RegisterProp(BiomeRegions.BloodKelp, "2bfcbaf4-1ae6-4628-9816-28a6a26ff340", 15, true, PrepareBloodTendril);
        RegisterProp(BiomeRegions.BloodKelp, "2ab96dc4-5201-4a41-aa5c-908f0a9a0da8", 15, true, PrepareBloodTendril);
        RegisterProp(
            BiomeRegions.BloodKelp,
            "18229b4b-3ed3-4b35-ae30-43b1c31a6d8d",
            25,
            true,
            0.4F,
            0.165F
        ); //blood oil
        /* too finicky
        foreach (string pfb in VanillaFlora.DEEP_MUSHROOM.getPrefabs(false, true)) {
            Action<GameObject> a = go => {
                go.transform.localScale = Vector3.one*0.33F;
                go.transform.localRotation = Quaternion.Euler(UnityEngine.Random.Range(260F, 280F), UnityEngine.Random.Range(0F, 360F)*0, 0);
            };
            registerProp(BiomeRegions.BloodKelp, pfb, 5, true, a);
            //registerProp(BiomeRegions.LostRiver, pfb, 5, a); is a native flora here
            //registerProp(BiomeRegions.LavaZone, pfb, 5, a); and here
        }*/

        foreach (var pfb in VanillaFlora.JELLYSHROOM_TINY.getPrefabs(true, true))
            RegisterProp(BiomeRegions.Jellyshroom, pfb, 5, false);

        foreach (var pfb in VanillaFlora.TREE_LEECH.getPrefabs(false, true))
            RegisterProp(BiomeRegions.Mushroom, pfb, 5, false, 0.25F);
        foreach (var pfb in VanillaFlora.GRUE_CLUSTER.getPrefabs(true, true))
            RegisterProp(
                BiomeRegions.Mushroom,
                pfb,
                5,
                false,
                0.00004F
            ); //why the hell is this thing so huge in native scale and vanilla scales it to 0.0001F

        RegisterProp(BiomeRegions.LostRiver, VanillaFlora.BRINE_LILY.getRandomPrefab(false), 10, false, 0.25F);
        foreach (var pfb in VanillaFlora.CLAW_KELP.getPrefabs(true, true))
            RegisterProp(
                BiomeRegions.LostRiver,
                pfb,
                5,
                true,
                0.1F,
                0,
                go => go.transform.rotation = Quaternion.Euler(270, 0, 0)
            );

        RegisterProp(BiomeRegions.GrandReef, VanillaFlora.ANCHOR_POD_SMALL1.getRandomPrefab(false), 10, true, 0.1F);
        RegisterProp(BiomeRegions.GrandReef, VanillaFlora.ANCHOR_POD_SMALL2.getRandomPrefab(false), 10, true, 0.1F);

        RegisterProp(BiomeRegions.LavaZone, "077ebe13-eb45-4ee4-8f6f-f566cfe11ab2", 10, false, 0.5F);

        if (Directory.Exists(RootCachePath)) {
            foreach (var folder in Directory.EnumerateDirectories(RootCachePath)) {
                var name = Path.GetFileName(folder);
                try {
                    SNUtil.log("Loading cached grass material '" + name + "' from " + folder);
                    var m = new MaterialPropertyDefinition(name);
                    m.readFromFile(AqueousEngineeringMod.modDLL, folder);
                    TerrainGrassTextures[m.name] = m;
                } catch (Exception ex) {
                    SNUtil.log("Could not load cached grass material '" + name + "': " + ex);
                }
            }
        } else {
            SNUtil.log("Grass material cache does not exist at " + RootCachePath + ".");
            Directory.CreateDirectory(RootCachePath);
        }
    }

    public static void SetFloorTexture(BiomeRegions.RegionType r, Texture2D tex) {
        if (tex && !FloorTextures.ContainsKey(r))
            FloorTextures[r] = tex;
    }

    public static void CacheGrassMaterial(Material m) {
        var n = m.mainTexture.name.Replace(" (Instance)", "");
        if (!TerrainGrassTextures.ContainsKey(n)) {
            var def = new MaterialPropertyDefinition(m);
            TerrainGrassTextures[n] = def;
            var path = Path.Combine(RootCachePath, n);
            def.writeToFile(path);
            SNUtil.log("Saved grass material '" + n + "' to " + path);
        }
    }

    private static void PrepareBloodTendril(GameObject go) {
        go.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.15F, 0.25F);
        go.transform.rotation = Quaternion.identity;
    }

    public static void RegisterGrassProp(
        BiomeRegions.RegionType r,
        string texture,
        double wt,
        float scale,
        float voff = 0
    ) {
        Action<GameObject> a = go => {
            go.transform.localScale = Vector3.one * UnityEngine.Random.Range(scale * 0.95F, scale * 1.05F);
            go.transform.position += Vector3.up * voff;
            if (!string.IsNullOrEmpty(texture)) {
                var rn = go.GetComponentInChildren<Renderer>();
                if (TerrainGrassTextures.ContainsKey(texture))
                    TerrainGrassTextures[texture]
                        .applyToMaterial(
                            rn.materials[0],
                            true,
                            false
                        ); //.mainTexture = RenderUtil.getVanillaTexture(texture);
                else
                    go.destroy();
            }
        };
        RegisterProp(r, "880b59b7-8fd6-412f-bbcb-a4260b263124", wt * 0.75F, false, a);
        RegisterProp(r, "bac42c90-8995-439f-be2f-29a6d164c82a", wt * 0.25F, false, a);
    }

    public static void RegisterProp(
        BiomeRegions.RegionType r,
        string pfb,
        double wt,
        bool up,
        float scale,
        float voff = 0,
        Action<GameObject> a = null
    ) {
        RegisterProp(
            r,
            pfb,
            wt,
            up,
            go => {
                go.transform.localScale = Vector3.one * UnityEngine.Random.Range(scale * 0.95F, scale * 1.05F);
                go.transform.position += Vector3.up * voff;
                a?.Invoke(go);
            }
        );
    }

    private static void RegisterProp(
        BiomeRegions.RegionType r,
        string pfb,
        double wt,
        bool up,
        Action<GameObject> a = null
    ) {
        var wr = PropTypes.ContainsKey(r) ? PropTypes[r] : new WeightedRandom<AcuPropDefinition>();
        wr.addEntry(new AcuPropDefinition(pfb, wt, up, a), wt);
        PropTypes[r] = wr;
    }

    private static AcuPropDefinition GetRandomAcuProp(WaterPark acu, BiomeRegions.RegionType r) {
        return PropTypes.ContainsKey(r) ? PropTypes[r].getRandomEntry() : null;
    }

    internal static void UpdateAcuTheming(
        AcuCallbackSystem.AcuCallback acu,
        BiomeRegions.RegionType theme,
        float time,
        bool changed
    ) {
        if (!acu.LowestSegment)
            return;
        //SNUtil.writeToChat(""+li.Count);
        //SNUtil.writeToChat("##"+theme+" > "+floor+" & "+glass+" & "+decoHolders.Count);
        foreach (Transform t in acu.LowestSegment.transform) {
            var n = t.gameObject.name;
            if (n.StartsWith("Coral_reef_shell_plates", StringComparison.InvariantCulture)) { //because is flat, skip it
                t.gameObject.SetActive(false);
            } else if (n.StartsWith("Coral_reef_small_deco", StringComparison.InvariantCulture)) {
                var flag = true;
                if (acu.DecoHolders.Count > 0) {
                    foreach (var slot in acu.DecoHolders) {
                        if (Vector3.Distance(slot.transform.position, t.position) <= 0.05F) {
                            t.gameObject.destroy();
                            flag = false;
                            break;
                        }
                    }
                }

                if (flag) {
                    var slot = new GameObject(AcuDecoSlotName);
                    slot.SetActive(true);
                    slot.transform.parent = acu.LowestSegment.transform;
                    slot.transform.position = t.position;
                    slot.transform.rotation = t.rotation;
                    //slot.transform.rotation = Quaternion.identity;
                    AddProp(t.gameObject, slot, BiomeRegions.Shallows);
                    acu.DecoHolders.Add(slot);
                }
            }
        }

        foreach (var slot in acu.DecoHolders) {
            if (!slot)
                continue;
            var found = false;
            foreach (Transform bt in slot.transform) {
                var biomeSlot = bt.gameObject;
                var match = biomeSlot.name == theme.ID;
                biomeSlot.SetActive(match);
                if (match) {
                    found = true;
                    if (bt.childCount == 0) {
                        var def = GetRandomAcuProp(acu.Acu, theme);
                        //SNUtil.writeToChat("$$"+def);
                        //SNUtil.log("$$"+def);
                        if (def != null)
                            AddProp(def.Spawn(), slot, theme, biomeSlot);
                    }
                }
            }

            if (!found) {
                AddProp(null, slot, theme);
            }
        }

        if (!changed)
            return;

        acu.LastThemeUpdate = time;
        acu.AppliedTheme = true;

        if (FloorTextures.ContainsKey(theme)) {
            var r = acu.Floor.GetComponentInChildren<Renderer>();
            r.material.mainTexture = FloorTextures[theme];
        }

        //SNUtil.writeToChat("::"+b);
        if (theme.baseBiome == null) return;
        {
            var biomeSky = WorldUtil.getSkybox(theme.baseBiome);
            if (!biomeSky) return;
            foreach (var glass in acu.Column.Select(wp =>
                         wp.gameObject.getChildObject("model/Large_Aquarium_generic_room_glass_01")
                     )) {
                glass.setSky(biomeSky);
                var r = glass.GetComponentInChildren<Renderer>();
                if (!r) {
                    SNUtil.writeToChat("No glass renderer");
                    return;
                }

                var m = r.materials[0];
                if (!m) {
                    SNUtil.writeToChat("No glass material");
                    return;
                }

                m.SetFloat(Fresnel, 0.5F);
                m.SetFloat(Shininess, 7.5F);
                m.SetFloat(SpecInt, 0.75F);
                m.SetColor(Color1, theme.waterColor);
                m.SetColor(SpecColor, theme.waterColor);
            }

            foreach (var wp in acu.Acu.items.Where(wp => wp)) {
                wp.gameObject.setSky(biomeSky);
            }

            foreach (var go in acu.DecoHolders) {
                go.setSky(biomeSky);
            }
        }
    }

    private static void AddProp(GameObject go, GameObject slot, BiomeRegions.RegionType r, GameObject rSlot = null) {
        var rname = r.ID;
        if (!rSlot)
            rSlot = slot.getChildObject(rname);
        if (!rSlot) {
            rSlot = new GameObject(rname) {
                transform = {
                    parent = slot.transform,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity,
                },
            };
        }

        if (go) {
            go.transform.parent = rSlot.transform;
            go.transform.localPosition = Vector3.zero;
            //go.transform.localRotation = Quaternion.identity;
            go.removeComponent<PrefabIdentifier>();
            go.removeComponent<TechTag>();
            go.removeComponent<Pickupable>();
            go.removeComponent<Collider>();
            go.removeComponent<PickPrefab>();
            go.removeComponent<Light>();
            go.removeComponent<SkyApplier>();
            var sk = go.EnsureComponent<SkyApplier>();
            sk.renderers = go.GetComponentsInChildren<Renderer>(true);
            go.setSky(MarmoSkies.main.skyBaseInterior);
        }
    }

    private class AcuPropDefinition {
        private readonly double _weight;
        private readonly string _prefab;
        private readonly bool _forceUpright;
        private readonly Action<GameObject> _modify;

        internal AcuPropDefinition(string pfb, double wt, bool up, Action<GameObject> a = null) {
            _weight = wt;
            _prefab = pfb;
            _modify = a;
            _forceUpright = up;
        }

        internal GameObject Spawn() {
            var go = ObjectUtil.createWorldObject(_prefab, true, false);
            if (go == null) {
                SNUtil.writeToChat("Could not spawn GO for " + this);
                return null;
            }

            var rs = go.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 1)
                go = rs[0].gameObject; //go.GetComponentInChildren<Renderer>(true).gameObject;
            go.SetActive(true);
            if (_forceUpright)
                go.transform.rotation = Quaternion.identity;
            _modify?.Invoke(go);
            return go;
        }

        public override string ToString() {
            return $"[ACUPropDefinition Weight={_weight}, Prefab={_prefab}]";
        }
    }
}