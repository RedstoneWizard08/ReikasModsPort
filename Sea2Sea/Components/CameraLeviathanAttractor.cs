using ReikaKalseki.DIAlterra;
using ReikaKalseki.Ecocean;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class CameraLeviathanAttractor : MonoBehaviour, AggroAttractor {
    private float lastLeviCheckTime;

    private MapRoomCamera cam;

    public bool isAggroable => !cam.dockingPoint;

    public void Update() {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (!cam)
            cam = GetComponent<MapRoomCamera>();
        if (time - lastLeviCheckTime >= 2 && cam && !cam.dockingPoint) {
            doLeviCheck();
            lastLeviCheckTime = time;
        }
    }

    private void doLeviCheck() {
        ECHooks.AttractToSoundPing(this, false, 0.375F); //150m
    }
}