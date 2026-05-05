using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class Platinum : BasicCustomOre {
    [SetsRequiredMembers]
    public Platinum(string id, string name, string desc, VanillaResources template) : base(id, name, desc, template) {
        collectSound = "event:/loot/pickup_diamond";
    }

    public override void prepareGameObject(GameObject go, Renderer[] r) {
        base.prepareGameObject(go, r);


        go.EnsureComponent<PlatinumTag>();
    }
}

internal class PlatinumTag : MonoBehaviour {
    private float lastTime;

    private float lastPickupTime;
    private float timeOnGround;
    private DeepStalkerTag currentStalker;

    private ResourceTracker resource;

    private float spawnTime;
    private float lastPlayerDistanceCheckTime;
    private float lastDensityCheckTime;

    private void Start() {
    }

    private void Update() {
        if (!resource)
            resource = GetComponent<ResourceTracker>();
        var time = DayNightCycle.main.timePassedAsFloat;
        var dT = time - lastTime;
        if (spawnTime <= 0)
            spawnTime = time;
        if (spawnTime > 0 && time - lastPlayerDistanceCheckTime >= 0.5) {
            lastPlayerDistanceCheckTime = time;
            if (Player.main && Vector3.Distance(transform.position, Player.main.transform.position) > 250 &&
                !gameObject.FindAncestor<StorageContainer>()) {
                gameObject.destroy(false);
            }
        }

        if (spawnTime > 0 && time - lastDensityCheckTime >= 0.5 && time - spawnTime >= 120 &&
            !gameObject.FindAncestor<StorageContainer>()) {
            lastDensityCheckTime = time;
            if (WorldUtil.getObjectsNearWithComponent<PlatinumTag>(transform.position, 60).Count > 5) {
                gameObject.destroy(false);
            }
        }

        if (spawnTime > 0 && time - spawnTime >= 600 && !currentStalker &&
            !gameObject.FindAncestor<StorageContainer>()) {
            gameObject.destroy(false);
        }

        if (dT >= 1) {
            gameObject.EnsureComponent<ResourceTrackerUpdater>().tracker = resource;
        }

        lastTime = time;
        if (currentStalker)
            timeOnGround = 0;
        else
            timeOnGround += dT;
    }

    public void pickup(DeepStalkerTag s) {
        currentStalker = s;
        lastPickupTime = DayNightCycle.main.timePassedAsFloat;
    }

    public void drop() {
        currentStalker = null;
    }

    public float getTimeOnGround() {
        return timeOnGround;
    }
}