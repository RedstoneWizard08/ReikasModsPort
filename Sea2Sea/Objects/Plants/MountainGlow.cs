using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class MountainGlow : BasicCustomPlant, MultiTexturePrefab {
    [SetsRequiredMembers]
    public MountainGlow() : base(
        SeaToSeaMod.ItemLocale.getEntry("MOUNTAIN_GLOW"),
        new FloraPrefabFetch("1d5877a7-bc56-46c8-a27c-f9d0ab99cc80"),
        "ba866b79-1db1-4689-a697-b7d2bc65959d",
        "Pods"
    ) {
        glowIntensity = 3F;
        collectionMethod = HarvestType.None;
    }

    protected override bool isExploitable() {
        return true;
    }

    protected override bool generateSeed() {
        return true;
    }

    public override Vector2int SizeInInventory => new(2, 2);

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);
        go.EnsureComponent<MountainGlowTag>();
        var c = go.EnsureComponent<SphereCollider>();
        c.isTrigger = true;
        c.radius = 4F;
        c.center = Vector3.zero;
        go.EnsureComponent<FruitPlant>();
        var l = go.addLight(1.6F, 1.5F, new Color(1, 0.1F, 0.2F));
        l.transform.localPosition = new Vector3(0, 1, 0);
    }

    public override float getScaleInGrowbed(bool indoors) {
        return indoors ? 0.25F : 0.5F;
    }

    public override Plantable.PlantSize getSize() {
        return Plantable.PlantSize.Large;
    }

    public Dictionary<int, string> getTextureLayers(Renderer r) {
        return new Dictionary<int, string> { { 0, "" }, { 1, "" }, { 2, "" }, { 3, "" } };
    }
}

internal class MountainGlowTag : MonoBehaviour {
    private static float lastDamageTime; //static so global, so does not stack lag OR damage

    private bool isGrown;

    private FruitPlant fruiter;
    private GameObject fruitHolder;
    private PickPrefab[] seeds;
    private Renderer[] renders;
    private Light light;
    private SphereCollider aoe;

    private bool needsAngling;

    private void Start() {
        isGrown = GetComponent<GrownPlant>() != null;
        //if (gameObject.transform.position.y > -10)
        //	gameObject.destroy(false);
        if (isGrown) {
            gameObject.SetActive(true);
            gameObject.transform.localScale = Vector3.one * Random.Range(0.8F, 1.2F);
        } else if (transform.position.y > -120 || transform.position.x < 275 ||
                   Vector3.Angle(transform.up, Vector3.up) >= 45) {
            gameObject.destroy(false);
        } else {
            needsAngling = true;
        }
    }

    private void Update() {
        ObjectUtil.cleanUpOriginObjects(this);
        if (!aoe)
            aoe = GetComponent<SphereCollider>();
        if (!fruiter)
            fruiter = GetComponent<FruitPlant>();
        if (!light)
            light = GetComponentInChildren<Light>();
        if (renders == null)
            renders = GetComponentsInChildren<Renderer>();
        foreach (var r in renders)
            r.transform.localPosition = Vector3.down * 0.5F;
        if (!fruitHolder) {
            var go = ObjectUtil.lookupPrefab("a17ef178-6952-4a91-8f66-44e1d8ca0575");
            fruitHolder = go.getChildObject("fruit_LODs").clone();
            fruitHolder.transform.SetParent(transform);
            fruitHolder.transform.localPosition = new Vector3(-0.08F, 6.38F, 0.06F);
            fruitHolder.transform.localScale = Vector3.one * 0.3F;
            fruitHolder.transform.localRotation = Quaternion.Euler(0, 0, 180);
            fruitHolder.removeComponent<ChildObjectIdentifier>();
            fruitHolder.removeComponent<TechTag>();
            fruitHolder.removeComponent<LargeWorldEntity>();
            fruitHolder.removeComponent<PrefabIdentifier>();
            if (seeds == null)
                seeds = fruitHolder.GetComponentsInChildren<PickPrefab>();
            foreach (var pp in seeds) {
                pp.pickTech = C2CItems.mountainGlow.seed.Info.TechType;
                pp.pickedEvent.AddHandler(
                    pp.gameObject,
                    new UWE.Event<PickPrefab>.HandleFunction(p => {
                            fruiter.inactiveFruits.Add(pp);
                            fruiter.timeNextFruit = DayNightCycle.main.timePassedAsFloat + fruiter.fruitSpawnInterval;
                        }
                    )
                );
                if (isGrown)
                    pp.SetPickedUp();
                var r = pp.GetComponentInChildren<Renderer>();
                RenderUtil.setEmissivity(r, 1.5F);
                RenderUtil.swapTextures(SeaToSeaMod.ModDLL, r, "Textures/Plants/MountainGlowSeed");
            }

            fruiter.fruits = seeds;
            fruiter.fruitSpawnEnabled = true;
            fruiter.fruitSpawnInterval = 300;
        }

        if (needsAngling && Vector3.Distance(transform.position, Player.main.transform.position) <= 200) {
            var hit = WorldUtil.getTerrainVectorAt(transform.position + Vector3.up * 2F, 4);
            if (hit.HasValue) {
                transform.up = (hit.Value.normal + Vector3.up) * 0.5F;
                needsAngling = false;
            }
        }

        light.intensity = 1.6F * Mathf.Lerp(1.4F, 2.2F, 1F - DayNightCycle.main.GetLightScalar()) *
                          (1F - fruiter.inactiveFruits.Count / (float)seeds.Length);
        aoe.isTrigger = true;
        aoe.radius = 4F;
        aoe.center = Vector3.zero;
    }

    private void OnTriggerStay(Collider other) {
        if (EnvironmentalDamageSystem.Instance.IsPlayerInOcean() &&
            DayNightCycle.main.timePassedAsFloat - lastDamageTime >= 0.05F && !other.isTrigger && other.isPlayer()) {
            C2CItems.hasSealedOrReinforcedSuit(out var trash, out var suit);
            if (!suit) {
                other.gameObject.FindAncestor<LiveMixin>().TakeDamage(
                    Time.deltaTime * 1.5F,
                    transform.position,
                    DamageType.Heat
                );
                lastDamageTime = DayNightCycle.main.timePassedAsFloat;
            }
        }
    }
}