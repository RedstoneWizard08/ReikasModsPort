using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class PostCoveDomeGenerator : WorldGenerator {
    internal static readonly CustomPrefab coolResourceDome = new ResourceDome(false);
    internal static readonly CustomPrefab hotResourceDome = new ResourceDome(true);

    internal static readonly WeightedRandom<string> resourceTableCool = new();
    internal static readonly WeightedRandom<string> resourceTableHot = new();

    private Quaternion rotation;

    static PostCoveDomeGenerator() {
        resourceTableCool.addEntry(VanillaResources.RUBY.prefab, 50);
        resourceTableCool.addEntry(VanillaResources.DIAMOND.prefab, 30);
        resourceTableCool.addEntry(VanillaResources.QUARTZ.prefab, 70);
        resourceTableCool.addEntry(CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).ClassID, 20);

        resourceTableHot.addEntry(VanillaResources.RUBY.prefab, 20);
        resourceTableHot.addEntry(VanillaResources.DIAMOND.prefab, 50);
        resourceTableHot.addEntry(VanillaResources.QUARTZ.prefab, 60);
        resourceTableHot.addEntry(CustomMaterials.getItem(CustomMaterials.Materials.CALCITE).ClassID, 20);
        resourceTableHot.addEntry(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).ClassID, 5);

        coolResourceDome.Register();
        hotResourceDome.Register();
    }

    private class ResourceDome : CustomPrefab {
        private readonly bool isHot;

        [SetsRequiredMembers]
        internal ResourceDome(bool hot) : base("PostCoveResourceDome_" + hot, "", "") {
            isHot = hot;
            SetGameObject(GetGameObject);
        }

        public GameObject GetGameObject() {
            var go = ObjectUtil.createWorldObject(VanillaResources.SANDSTONE.prefab);
            var mdl = ObjectUtil.lookupPrefab(VanillaFlora.AMOEBOID.getPrefabID())
                .getChildObject("lost_river_plant_04/lost_river_plant_04_membrane");
            go.EnsureComponent<ResourceDomeTag>().isHot = isHot;
            go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
            go.EnsureComponent<TechTag>().type = Info.TechType;
            go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
            var rs = go.GetComponentsInChildren<Renderer>();
            foreach (var r in rs) {
                var r2 = r.setModel(mdl).GetComponentInChildren<Renderer>();
                r2.transform.localPosition = new Vector3(0, -0.05F * 0 + 0.02F, 0);
                r2.transform.localEulerAngles = new Vector3(-90, 0, 0);
                RenderUtil.swapTextures(
                    SeaToSeaMod.ModDLL,
                    r2,
                    "Textures/Plants/PostCoveTree/Res_" + (isHot ? "Hot" : "Cold")
                );
                PostCoveDome.setupRenderGloss(r2);
                RenderUtil.disableTransparency(r2.materials[0]);
                r2.materials[0].SetFloat("_SpecInt", isHot ? 7.5F : 2F);
                RenderUtil.setEmissivity(r2, 2F);
            }

            go.removeComponent<ResourceTracker>();
            go.GetComponent<VFXSurface>().surfaceType = VFXSurfaceTypes.glass;
            return go;
        }
    }

    internal class ResourceDomeTag : MonoBehaviour {
        internal bool isHot;
        private BreakableResource res;

        private SkyApplier[] skies;
        private Renderer[] renderers;

        internal float growFade;

        private void Start() {
            res = GetComponent<BreakableResource>();
            res.numChances = 0; //use own drop code
            res.hitsToBreak = UnityEngine.Random.Range(5, 9); //5-8 hits
            res.breakText = "BreakPostCoveDomeResource"; //locale key
            res.customGoalText = "BreakPostCoveDomeResource"; //StoryGoal
            res.defaultPrefabTechType = SeaToSeaMod.GeogelFogDrip.Info.TechType;
            // res.defaultPrefab = ObjectUtil.lookupPrefab(SeaToSeaMod.geogelFogDrip.Info.ClassID);//this is lead for sandstone, but we made it an FX
            GetComponentInChildren<Renderer>().transform.localScale = 0.3F * new Vector3(
                UnityEngine.Random.Range(0.9F, 1.1F),
                UnityEngine.Random.Range(0.9F, 1.1F),
                UnityEngine.Random.Range(0.6F, 1.4F)
            );
            skies = GetComponentsInChildren<SkyApplier>(true);
            renderers = GetComponentsInChildren<Renderer>();
            InvokeRepeating(nameof(setupSky), 0, 1F);
            Invoke(nameof(refinePosition), 2.5F);
            Invoke(nameof(refinePosition), 5F);
            Invoke(nameof(refinePosition), 10F);
            isHot = transform.position.y < PostCoveDome.HOT_THRESHOLD;
        }

        private void Update() {
            growFade = Mathf.Clamp01(growFade - Time.deltaTime);
            foreach (var r in renderers) {
                r.transform.localPosition = Vector3.down * 0.25F * growFade;
            }

            res.enabled = growFade <= 0;
        }

        private void refinePosition() {
            var pos = transform.position + transform.up * 5;
            var vec = -transform.up * 15;
            var ray = new Ray(pos, vec);
            if (UWE.Utils.RaycastIntoSharedBuffer(ray, vec.magnitude, Voxeland.GetTerrainLayerMask()) > 0) {
                var hit = UWE.Utils.sharedHitBuffer[0];
                if (hit.transform != null) {
                    transform.rotation = MathUtil.unitVecToRotation(hit.normal);
                    transform.position = hit.point;
                }
            }
        }

        private void setupSky() {
            var idx = WaterBiomeManager.main.GetBiomeIndex(
                isHot ? /*"ILZCorridor"*/"ILZChamber" : "LostRiver_TreeCove"
            );
            foreach (var sk in skies) {
                if (!sk)
                    continue;
                sk.renderers = renderers;
                gameObject.setSky(WaterBiomeManager.main.biomeSkies[idx]);
            }
        }

        private void OnBreakResource() {
            var wr = isHot ? resourceTableHot : resourceTableCool;

            // TODO
            // res.SpawnResourceFromPrefab(ObjectUtil.lookupPrefab(wr.getRandomEntry())); //use their spawn code
        }
    }

    public PostCoveDomeGenerator(Vector3 pos) : base(pos) {
    }

    public override void saveToXML(XmlElement e) {
        PositionedPrefab.saveRotation(e, rotation);
    }

    public override void loadFromXML(XmlElement e) {
        rotation = PositionedPrefab.readRotation(e);
    }

    public override bool generate(List<GameObject> li) {
        var go = spawner(SeaToSeaMod.PostCoveDome.Info.ClassID);
        go.transform.position = position;
        go.transform.rotation = rotation;
        li.Add(go);

        HashSet<Vector3> placed = [];
        var failed = 0;
        for (var i = 0; i < 24; i++) {
            var go2 = placeRandomResourceDome(go, placed, spawner);
            if (go2) {
                li.Add(go2);
                placed.Add(go2.transform.position);
            } else {
                i--;
                failed++;
                if (failed > 50)
                    break;
                else
                    continue;
            }
        }

        return go && placed.Count > 3;
    }

    public override LargeWorldEntity.CellLevel getCellLevel() {
        return LargeWorldEntity.CellLevel.Far;
    }

    public static GameObject placeRandomResourceDome(
        GameObject from,
        IEnumerable<Vector3> avoid,
        Func<string, GameObject> spawner
    ) {
        var pos = MathUtil.getRandomVectorAround(from.transform.position + from.transform.up * 5, 6);
        var vec = -from.transform.up * 15;
        var ray = new Ray(pos, vec);
        if (UWE.Utils.RaycastIntoSharedBuffer(ray, vec.magnitude, Voxeland.GetTerrainLayerMask()) > 0) {
            var hit = UWE.Utils.sharedHitBuffer[0];
            if (hit.transform != null) {
                if ((hit.point - from.transform.position).sqrMagnitude < 9)
                    return null;
                foreach (var at in avoid) {
                    if ((at - hit.point).sqrMagnitude < 0.5) {
                        return null;
                    }
                }

                var rt = from.transform.position.y < PostCoveDome.HOT_THRESHOLD
                    ? hotResourceDome
                    : coolResourceDome;
                var go2 = spawner(rt.Info.ClassID);
                go2.transform.rotation = MathUtil.unitVecToRotation(hit.normal);
                go2.transform.position = hit.point;
                go2.transform.RotateAroundLocal(go2.transform.up, UnityEngine.Random.Range(0F, 360F));
                return go2;
            }
        }

        return null;
    }
}