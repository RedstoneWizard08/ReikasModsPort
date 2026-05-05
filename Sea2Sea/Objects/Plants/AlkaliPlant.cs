using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class AlkaliPlant : BasicCustomPlant {
    [SetsRequiredMembers]
    public AlkaliPlant() : base(
        SeaToSeaMod.ItemLocale.getEntry("ALKALI_PLANT"),
        new FloraPrefabFetch(VanillaFlora.REDWORT),
        "daff0e31-dd08-4219-8793-39547fdb745e",
        "Samples"
    ) {
        glowIntensity = 2;
        finalCutBonus = 0;
        //seed.sprite = TextureManager.getSprite("Textures/Items/"+ObjectUtil.formatFileName(this));
    }

    public override Vector2int SizeInInventory => new(2, 2);

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);
        go.EnsureComponent<AlkaliPlantTag>();
        go.transform.localScale = Vector3.one * 2;
        /*
        GameObject seedRef = ObjectUtil.lookupPrefab("daff0e31-dd08-4219-8793-39547fdb745e").GetComponent<Plantable>().model;
        p.pickupable = go.GetComponentInChildren<Pickupable>();
        p.model = seedRef.clone();
        GrowingPlant grow = p.model.EnsureComponent<GrowingPlant>();
        grow.seed = p;
        RenderUtil.setModel(p.model, "coral_reef_plant_middle_05", go.getChildObject("coral_reef_plant_middle_05"));
        */ /*
        CapsuleCollider cu = go.GetComponentInChildren<CapsuleCollider>();
        if (cu != null) {
            CapsuleCollider cc = p.model.AddComponent<CapsuleCollider>();
            cc.radius = cu.radius*0.8F;
            cc.center = cu.center;
            cc.direction = cu.direction;
            cc.height = cu.height;
            cc.material = cu.material;
            cc.name = cu.name;
        }
        p.modelEulerAngles = new Vector3(270*0, UnityEngine.Random.Range(0, 360F), 0);*/
        go.EnsureComponent<LiveMixin>().data.maxHealth /= 2;
        foreach (var rr in r) {
            foreach (var m in rr.materials) {
                m.SetColor("_GlowColor", new Color(1, 1, 1, 1));
                m.SetVector("_Scale", new Vector4(0.35F, 0.2F, 0.1F, 0.0F));
                m.SetVector("_Frequency", new Vector4(1.2F, 0.5F, 1.5F, 0.5F));
                m.SetVector("_Speed", new Vector4(0.2F, 0.5F, 1.5F, 0.5F));
                m.SetVector("_ObjectUp", new Vector4(1F, 1F, 1F, 1F));
                m.SetFloat("_WaveUpMin", 0F);
            }
        }
    }

    public override float getScaleInGrowbed(bool indoors) {
        return indoors ? 0.25F : 0.5F;
    }
}

internal class AlkaliPlantTag : MonoBehaviour {
    private Renderer renderer;

    private bool isGrown;
    private float rootScale;

    private float timeVisible;
    private float currentScale = 1;
    private bool currentlyHiding;

    private float timePlayerStationary;

    private bool isFrozen;

    private static readonly Vector3 deleteArea = new(-1264, -282, -724);

    private void Start() {
        isGrown = gameObject.GetComponent<GrownPlant>() != null;
        currentScale = 1;
        rootScale = Random.Range(2, 2.5F);
        if (isGrown) {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one * Random.Range(0.8F, 1.2F);
        } else if (transform.position.y > -10 || Vector3.Distance(transform.position, deleteArea) <= 20) {
            gameObject.destroy(false);
        } else {
            transform.localScale = Vector3.one * rootScale;
        }
    }

    public void OnFreeze( /*float time*/) {
        //if (time <= 2)
        //	return;
        isFrozen = true;
        //Invoke("OnUnfreeze", time-2);
    }

    public void OnUnfreeze() {
        isFrozen = false;
        timeVisible = 1;
    }

    private void Update() {
        ObjectUtil.cleanUpOriginObjects(this);
        if (!renderer)
            renderer = GetComponentInChildren<Renderer>();
        var ep = Player.main;
        if (!isFrozen && ep &&
            (!isGrown || Vector3.Distance(transform.position, C2CHooks.mountainBaseGeoCenter) <= 30)) {
            var dT = Time.deltaTime;
            var v = ep.GetVehicle();
            if ((v && v.useRigidbody ? v.useRigidbody : ep.rigidBody).velocity.magnitude > 0.05F)
                timePlayerStationary = 0;
            else
                timePlayerStationary += dT;
            var dd = Vector3.Distance(ep.transform.position, transform.position);
            if (dd <= (v ? 25F : 15F) && canSeePlayer(ep) && timePlayerStationary < (v ? 20 : 30 * 999))
                timeVisible += dT;
            else
                timeVisible = 0;
            currentlyHiding = timeVisible >= 0.67F;
            if (currentlyHiding) {
                var sp = 1F * dT;
                if (dd <= 8)
                    sp *= 1.5F;
                currentScale = Mathf.Max(0.045F, currentScale - sp);
            } else {
                currentScale = Mathf.Min(1, currentScale + 0.15F * dT);
            }

            if (float.IsInfinity(currentScale) || float.IsNaN(currentScale)) //how this happens is beyond me
                currentScale = 1;
            currentScale = Mathf.Clamp(currentScale, 0.03F, 1);
            var f = rootScale * currentScale;
            var glow = C2CItems.alkali.glowIntensity * currentScale;
            if (glow <= 0.035)
                glow = 0;
            RenderUtil.setEmissivity(renderer, glow);
            transform.localScale = new Vector3(
                0.33F + f * 0.67F,
                f,
                0.33F + f * 0.67F
            ); //Vector3.one*f;//new Vector3(0.75F+f*0.25F, f, 0.75F+f*0.25F);
            GetComponent<LiveMixin>().data.knifeable = isHarvestable();
        }
    }

    private bool canSeePlayer(Player ep) {
        if (ep.IsInBase())
            return false;
        var v = ep.GetVehicle();
        if (v)
            return Vector3.Distance(v.transform.position, transform.position) <= 4 ||
                   (v.useRigidbody && v.useRigidbody.velocity.magnitude > 0.05F);
        var pos1 = ep.transform.position;
        var pos2 = transform.position + transform.up.normalized * 0.5F;
        if (WorldUtil.lineOfSight(ep.gameObject, gameObject, pos1, pos2))
            return true;
        pos2 = transform.position + transform.up.normalized * 1.5F;
        return WorldUtil.lineOfSight(ep.gameObject, gameObject, pos1, pos2);
    }

    public bool isHarvestable() {
        return currentScale >= 0.75F && canSeePlayer(Player.main);
    }
}