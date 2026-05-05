using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class UnmovingHeatBlade : PickedUpAsOtherItem {
    [SetsRequiredMembers]
    internal UnmovingHeatBlade() : base("UnmovingHeatBlade", TechType.HeatBlade) {
    }

    protected override void prepareGameObject(GameObject go) {
        go.GetComponent<Rigidbody>().isKinematic = true;
        go.EnsureComponent<UnmovingHeatBladeTag>();
        var l = go.addLight(0.8F, 2.4F);
        l.transform.localPosition = new Vector3(0, 0, 0.2F);
        l.lightShadowCasterMode = LightShadowCasterMode.Everything;
        l.shadows = LightShadows.Soft;
    }
}

internal class UnmovingHeatBladeTag : MonoBehaviour {
    private Rigidbody body;
    private Light light;

    private void Update() {
        if (!body) {
            body = GetComponent<Rigidbody>();
        }

        if (!light) {
            light = GetComponentInChildren<Light>();
            var num = 0.5F + 0.5F * Mathf.Sin(DayNightCycle.main.timePassedAsFloat * 3.417F);
            light.color = new Color(1, Mathf.Lerp(70F, 140F, num) / 255F, 0);
            light.intensity = Mathf.Lerp(0.45F, 0.67F, 1F - num);
        }

        body.isKinematic = true;
    }
}