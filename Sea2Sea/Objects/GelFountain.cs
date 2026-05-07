using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class GelFountain : InteractableSpawnable {
    [SetsRequiredMembers]
    internal GelFountain(XMLLocale.LocaleEntry e) : base(e) {
        scanTime = 10;
        AddOnRegister(() => {
                SaveSystem.addSaveHandler(
                    Info.ClassID,
                    new SaveSystem.ComponentFieldSaveHandler<GelFountainTag>().addField("nextHarvestTime")
                );
            }
        );
    }

    public override GameObject GetGameObject() {
        //prefab ideas
        //b71823a1-4fbc-42dd-aa3a-caa5809f1f6c
        //b5a62048-0577-4a85-a7bd-a1896fbc1357
        //db86ef34-e1fa-4eb2-aa18-dda5af30cb45
        //9966bd1d-8db4-492a-b8c6-1f5e075c1d5b
        //eca96e8f-0097-4627-b906-f454c329d9e5
        //VanillaFlora.BRAIN_CORAL.getRandomPrefab(true)
        //VanillaFlora.MUSHROOM_BUMP.getRandomPrefab(false)
        var world = ObjectUtil.createWorldObject("1ce074ee-1a58-439b-bb5b-e5e3d9f0886f");
        world.EnsureComponent<TechTag>().type = Info.TechType;
        var pi = world.EnsureComponent<PrefabIdentifier>();
        pi.ClassId = Info.ClassID;
        world.EnsureComponent<GelFountainTag>();
        world.removeComponent<CoralBlendWhite>();
        world.removeComponent<Light>();
        //world.removeComponent<IntermittentInstantiate>();
        //world.removeComponent<BrainCoral>();
        world.removeComponent<LiveMixin>();
        world.removeComponent<Pickupable>();
        world.removeComponent<ResourceTracker>();
        world.makeMapRoomScannable(Info.TechType);
        world.removeComponent<Rigidbody>();
        world.removeComponent<WorldForces>();
        var bc = world.GetComponent<BoxCollider>();
        bc.size = Vector3.Scale(bc.size, new Vector3(1.5F, 1.5F, 4.0F));
        //world.removeComponent<GoalObject>();
        //world.removeChildObject("EmitPoint");
        var r = world.GetComponentInChildren<Renderer>();
        r.transform.localScale = new Vector3(2, 2, 5);
        r.transform.localPosition = Vector3.up * -0.1F;
        r.materials[0].SetFloat("_Shininess", 0F);
        r.materials[0].SetFloat("_SpecInt", 1F);
        r.materials[0].SetColor("_GlowColor", Color.white);
        //r.transform.localEulerAngles = new Vector3(-90, 0, 0);
        //world.GetComponentInChildren<Animator>().speed *= 0.25F;
        world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;

        RenderUtil.swapTextures(SeaToSeaMod.ModDLL, r, "Textures/GelFountain");
        RenderUtil.setEmissivity(r, 2);

        world.EnsureComponent<ImmuneToPropulsioncannon>().immuneToRepulsionCannon = true;

        if (false && !SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
            var rt = world.EnsureComponent<ResourceTracker>();
            rt.techType = Info.TechType;
            rt.overrideTechType = Info.TechType;
            rt.prefabIdentifier = pi;
        }

        return world;
    }

    public void register() {
        Register();
    }

    public void postRegister() {
        registerEncyPage();
        GenUtil.registerPrefabWorldgen(this, false, BiomeType.UnderwaterIslands_IslandCaveWall, 1, 3.0F);
    }
}

internal class GelFountainTag : MonoBehaviour, IHandTarget {
    private Renderer render;

    private float nextHarvestTime;
    private float nextDripTime;

    private bool hasDoneCaveCheck;
    private float nextCaveCheck = 10;

    private void Update() {
        if (!render)
            render = GetComponentInChildren<Renderer>();
        var time = DayNightCycle.main.timePassedAsFloat;
        var h = getHarvestReadiness();
        if (time >= nextDripTime && hasDoneCaveCheck) {
            spawn(true);
            var f = Random.Range(0.5F, 2) * (h > 0 ? Mathf.Min(5, 1F / h) : 5);
            nextDripTime = time + f;
        }

        if (time > nextCaveCheck) {
            var vec = transform.up;
            var ray = new Ray(transform.position, vec);
            if (UWE.Utils.RaycastIntoSharedBuffer(ray, 45, Voxeland.GetTerrainLayerMask()) > 0) {
                var hit = UWE.Utils.sharedHitBuffer[0];
                if (hit.transform == null || hit.distance < 5) {
                    gameObject.destroy(false);
                    return;
                }
            }

            hasDoneCaveCheck = true;
            nextCaveCheck = time + 10;
        }

        RenderUtil.setEmissivity(render, h * h);
        render.transform.localScale = new Vector3(2, 2, 3 + 2 * h);
    }

    private float getHarvestReadiness() {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (time >= nextHarvestTime)
            return 1;
        var diff = nextHarvestTime - time;
        return diff >= 1200 ? 0 : (float)MathUtil.linterpolate(diff, 0, 1200, 1, 0, true);
    }

    public void onKnifed() {
        if (tryHarvest()) {
        }
    }

    private bool tryHarvest() {
        if (getHarvestReadiness() >= 1) {
            spawn(false);
            nextHarvestTime = DayNightCycle.main.timePassedAsFloat + Random.Range(3600, 7200); //1-2h
            return true;
        }

        return false;
    }

    private void spawn(bool drip) {
        var go = ObjectUtil.createWorldObject(drip ? SeaToSeaMod.GeogelDrip.ClassID : SeaToSeaMod.Geogel.ClassID);
        go.fullyEnable();
        go.ignoreCollisions(gameObject);
        go.transform.position = transform.position + transform.up * 0.5F;
        if (!drip)
            go.EnsureComponent<GeogelTag>();
        var rb = go.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        var vec = MathUtil.getRandomVectorAround(transform.up.normalized, 0.5F) * 3;
        rb.AddForce(vec, ForceMode.VelocityChange);
        LargeWorldStreamer.main.MakeEntityTransient(go);
    }

    private void OnScanned() {
    }

    public void OnHandHover(GUIHand hand) {
        var h = getHarvestReadiness();
        HandReticle.main.SetProgress(h);
        HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
        HandReticle.main.SetTargetDistance(8);
        HandReticle.main.SetInteractText(h < 1 ? "GelFountainRecharging" : "GelFountainClick");
    }

    public void OnHandClick(GUIHand hand) {
    }
}