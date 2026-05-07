using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class SeamothTetherController : MonoBehaviour {
    private static readonly float POWER_COST = 0.5F; //per second
    internal static readonly float RANGE = 18F;

    private SeaMoth vehicle;

    private int moduleSlot;

    internal GameObject tetherRoot;
    private FMOD_CustomLoopingEmitter grabSound;

    private List<Tether> tethers = [];

    private bool towing;

    private void FixedUpdate() {
        if (!vehicle) {
            vehicle = GetComponent<SeaMoth>();
        }

        if (!tetherRoot) {
            tetherRoot = gameObject.getChildObject("TetherRoot");
            if (!tetherRoot) {
                tetherRoot = new GameObject("TetherRoot");
                tetherRoot.transform.SetParent(transform);
                tetherRoot.transform.localRotation = Quaternion.identity;
                tetherRoot.transform.localPosition = Vector3.zero;

                var template = ObjectUtil.lookupPrefab(TechType.PropulsionCannon)
                    .GetComponent<PropulsionCannon>();
                grabSound = tetherRoot.EnsureComponent<FMOD_CustomLoopingEmitter>();
                grabSound.CopyObject(template.grabbingSound);
            }
        }

        while (tethers.Count < 5) {
            tethers.Add(new Tether());
        }

        if (vehicle && moduleSlot >= 0 && vehicle.IsToggled(moduleSlot)) {
            //SNUtil.writeToChat("Module is toggled");
            vehicle.GetEnergyValues(out var energy, out var capacity);
            var dT = Time.deltaTime;
            var amt = POWER_COST * dT;
            if (energy <= amt) {
                vehicle.ToggleSlot(moduleSlot, false);
                return;
            }

            vehicle.ConsumeEnergy(amt);
            HashSet<Rigidbody> grabbed = [];
            foreach (var t in tethers) {
                var rb = t.getTarget();
                if (rb) {
                    t.pull(this, dT);
                    grabbed.Add(rb);
                } else {
                    rb = tryFindGrabbableTarget();
                    if (rb && !grabbed.Contains(rb)) {
                        t.grab(rb);
                        grabbed.Add(rb);
                        //SNUtil.writeToChat(tethers.IndexOf(t)+" @ "+DayNightCycle.main.timePassedAsFloat.ToString("0.00000")+": Grabbed "+rb+" from list "+grabbed.toDebugString());
                    }
                }
            }

            towing = grabbed.Count > 0;
        } else {
            towing = false;
            //SNUtil.writeToChat("Module is NOT toggled");
            foreach (var t in tethers)
                t.drop();
        }

        if (grabSound) {
            if (towing)
                grabSound.Play();
            else
                grabSound.Stop();
        }
    }

    public bool isTowing() {
        return towing;
    }

    private Rigidbody tryFindGrabbableTarget() {
        List<Rigidbody> available = [];
        WorldUtil.getGameObjectsNear(
            transform.position,
            RANGE,
            go => {
                var rb = go.GetComponent<Rigidbody>();
                if (rb && !rb.GetComponent<Vehicle>() && !rb.GetComponent<WaterParkItem>()) {
                    var d = go.GetComponent<Drillable>();
                    if (d) {
                        available.Add(rb);
                        return;
                    } /*
                    EcoTarget e = go.GetComponent<EcoTarget>();
                    if (e && e.GetTargetType() == EcoTargetType.Shark) {
                        available.Add(rb);
                        return;
                    }*/

                    var c = go.GetComponent<Creature>();
                    if (c is GasoPod or Shocker or LavaLizard or Jellyray or Stalker or BoneShark or SpineEel or SandShark or Warper) {
                        available.Add(rb);
                        return;
                    }
                }
            }
        );
        return available.Count == 0 ? null : available.GetRandom();
    }

    public void recalculateModule() {
        if (!vehicle) {
            Invoke(nameof(recalculateModule), 0.5F);
            return;
        }

        foreach (var idx in vehicle.slotIndexes.Values) {
            var ii = vehicle.GetSlotItem(idx);
            if (ii != null && ii.item && ii.item.GetTechType() == C2CItems.tetherModule.Info.TechType) {
                moduleSlot = idx;
                return;
            }
        }

        moduleSlot = -1;
    }
}

internal class Tether {
    private Rigidbody target;

    private readonly List<GameObject> grabSphereFX = [];
    private readonly float massScale;

    private VFXElectricLine effect;

    public Tether() {
        var template = ObjectUtil.lookupPrefab(TechType.PropulsionCannon)
            .GetComponent<PropulsionCannon>();
        for (var i = 0; i < 4; i++) {
            var go = template.grabbedEffect.clone();
            go.SetActive(false);
            grabSphereFX.Add(go);
            go.transform.localRotation = Random.rotationUniform;
        }

        massScale = template.massScalingFactor;
    }

    internal Rigidbody getTarget() {
        return target;
    }

    internal void grab(Rigidbody rb) {
        target = rb;
        foreach (var go in grabSphereFX)
            go.SetActive(true);
        effect.gameObject.SetActive(true);
        rb.isKinematic = false;
    }

    internal void drop() {
        foreach (var go in grabSphereFX) {
            if (go)
                go.SetActive(false);
        }

        if (effect)
            effect.gameObject.SetActive(false);
        target = null;
    }

    internal void pull(SeamothTetherController mgr, float dT) {
        if (!effect) {
            var go = ObjectUtil.lookupPrefab("d11dfcc3-bce7-4870-a112-65a5dab5141b");
            go = go.GetComponent<Gravsphere>().vfxPrefab;
            go = go.clone();
            effect = go.GetComponent<VFXElectricLine>();
            effect.transform.parent = mgr.tetherRoot.transform;
        }

        if (!target)
            return;
        target.isKinematic = false;

        var animal = (bool)target.GetComponent<Creature>();

        var tgtFX = mgr.transform.position - mgr.transform.forward * 1 - mgr.transform.up * 0.25F;
        float f = animal ? 2 : 1;
        var tgt = mgr.transform.position +
                  (target.transform.position - mgr.transform.position).normalized * 5 *
                  f; //mgr.transform.position-mgr.transform.forward*8*f-mgr.transform.up*2.5F*f;
        effect.origin = tgtFX;
        effect.target = target.transform.position;
        effect.originVector = -mgr.transform.forward;

        var distance = tgt - target.transform.position;
        var magnitude = distance.magnitude;
        if (magnitude > SeamothTetherController.RANGE * 2.5F) {
            SNUtil.WriteToChat(
                "Lost grip of " + Language.main.Get(CraftData.GetTechType(target.gameObject).AsString())
            );
            drop();
            return;
        }

        var d = Mathf.Clamp(magnitude, 1f, 4f);
        var vector = target.velocity + Vector3.Normalize(distance) * (animal ? 300 : 1200) * d * dT /
            (1f + target.mass * massScale);
        var amount = vector * (10f + Mathf.Pow(Mathf.Clamp01(1f - magnitude), 1.75f) * 40f) * dT;
        vector = UWE.Utils.SlerpVector(vector, Vector3.zero, amount);
        target.velocity = vector;

        foreach (var go in grabSphereFX) {
            go.transform.SetParent(mgr.tetherRoot.transform);
            go.transform.localScale = Vector3.one * (animal ? 5 : 3);
            go.transform.position = target.transform.position;
        }
    }

    private class GrabbedTarget {
        private Rigidbody body;
        private Vector3 relativeCenter;
    }
}