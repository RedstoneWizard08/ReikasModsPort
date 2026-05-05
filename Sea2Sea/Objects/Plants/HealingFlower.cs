using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class HealingFlower : BasicCustomPlant {
    [SetsRequiredMembers]
    public HealingFlower() : base(
        SeaToSeaMod.ItemLocale.getEntry("HEALING_FLOWER"),
        new FloraPrefabFetch(VanillaFlora.VOXEL),
        "6f932b93-65e8-4c89-a63b-d105203ab84c",
        "Leaves"
    ) {
        glowIntensity = 1.5F;
        finalCutBonus = 1;
    }

    public override Vector2int SizeInInventory => new(1, 1);

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);
        go.EnsureComponent<HealingFlowerTag>();
        //SphereCollider sc = go.EnsureComponent<SphereCollider>();
        //sc.radius = 0.25F;
        //sc.isTrigger = true;
        go.GetComponentInChildren<Collider>().gameObject.EnsureComponent<HealingFlowerColliderTag>();
    }

    public override float getScaleInGrowbed(bool indoors) {
        return indoors ? 0.25F : 0.5F;
    }

    public override Plantable.PlantSize getSize() {
        return Plantable.PlantSize.Small;
    }
    /*
    public override float getGrowthTime() {
        return 6000; //5x
    }*/
}

internal class HealingFlowerTag : MonoBehaviour {
    private bool isGrown;

    private void Start() {
        isGrown = gameObject.GetComponent<GrownPlant>() != null;
        //if (gameObject.transform.position.y > -10)
        //	gameObject.destroy(false);
        if (isGrown) {
            gameObject.SetActive(true);
            //gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.8F, 1.2F);
        } else {
            gameObject.transform.localScale = Vector3.one * Random.Range(1.33F, 1.67F);
        }
    }

    private void Update() {
    }
}

internal class HealingFlowerColliderTag : MonoBehaviour {
    private bool isGrown;
    private LiveMixin live;

    private void Start() {
        isGrown = gameObject.FindAncestor<GrownPlant>() != null;
        live = gameObject.FindAncestor<LiveMixin>();
    }

    private void Update() {
        ObjectUtil.cleanUpOriginObjects(this);
    }

    private void OnTriggerStay(Collider other) {
        if (!other.isTrigger && other.isPlayer()) {
            var dt = Time.deltaTime;
            if (other.gameObject.FindAncestor<LiveMixin>().AddHealth((isGrown ? 0.2F : 0.5F) * dt) > 0.00001F) {
                if (isGrown && live != null)
                    live.TakeDamage(0.67F * dt, transform.position);
            }
        }
    }
}