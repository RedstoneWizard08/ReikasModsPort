using System;
using Nautilus.Utility;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class CreepvineSonarScatterer : MonoBehaviour {
    private SonarFloraTag sonarFlora;
    public Action<SonarFloraTag> modify = null;

    private SonarOnlyRenderer cheapController;
    private float lastSonarFloraCleanupTime = -1;

    private void Update() {
        if (EcoceanMod.config.getBoolean(ECConfig.ConfigEntries.GOODCREEPSONAR) && false) {
            var time = DayNightCycle.main.timePassedAsFloat;
            if (time - lastSonarFloraCleanupTime >= 1) {
                //SNUtil.log("Removing cheap creepvine sonar halo");
                lastSonarFloraCleanupTime = time;
                var capsule = gameObject.getChildObject("SonarHalo");
                if (capsule)
                    capsule.destroy(false);
            }

            if (!sonarFlora) {
                if (!sonarFlora) {
                    //SNUtil.log("Adding expensive creepvine sonar halo");
                    var extents = getRenderBox();
                    var go = ObjectUtil.createWorldObject(EcoceanMod.sonarFlora.ClassID);
                    //Light l = GetComponentInChildren<Light>();
                    go.transform.SetParent( /*l ? l.transform : */transform);
                    go.transform.localPosition =
                        extents.center - transform.position; //l ? Vector3.zero : Vector3.up*12;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                    sonarFlora = go.GetComponent<SonarFloraTag>();
                    modify?.Invoke(sonarFlora);
                    sonarFlora.setShape( /*new Vector3(2.5F, 16, 2.5F)*/
                        extents.extents * 0.9F - new Vector3(0.5F, 1F, 0.5F)
                    );
                }
            }

            if (sonarFlora && sonarFlora.aoe.sqrMagnitude <= 0.01F) {
                sonarFlora.gameObject.destroy(false);
                sonarFlora = null;
            }
        } else {
            if (!cheapController)
                cheapController = gameObject.EnsureComponent<SonarOnlyRenderer>();
            if (cheapController) {
                if (cheapController.renderers.Count == 0) {
                    //SNUtil.log("Adding cheap creepvine sonar halo");
                    var capsule = gameObject.getChildObject("SonarHalo");
                    if (!capsule) {
                        //SNUtil.log("Constructing new object");
                        var extents = getRenderBox();
                        capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule).setName("SonarHalo");
                        capsule.transform.SetParent(transform);
                        capsule.transform.localPosition = extents.center - transform.position;
                        capsule.transform.localRotation = Quaternion.identity;
                        capsule.transform.localScale = extents.extents + Vector3.one;
                        MaterialUtils.ApplySNShaders(capsule, 0, 10, 5);
                        capsule.removeComponent<Collider>();
                    }

                    var r = capsule.GetComponentInChildren<Renderer>();
                    cheapController.renderers.Add(SonarFloraTag.prepareCheapSonarHalo(r));
                }

                //SNUtil.log(cheapController.renderers.toDebugString());
                if (cheapController.renderers.Count > 0 && cheapController.renderers[0].renderer)
                    cheapController.renderers[0].renderer.materials[0].SetFloat(
                        "_Built",
                        Mathf.Lerp(0.475F, 0.52F, cheapController.renderers[0].intensity)
                    );
            }

            var time = DayNightCycle.main.timePassedAsFloat;
            if (time - lastSonarFloraCleanupTime >= 1) {
                //SNUtil.log("Removing expensive creepvine sonar halo");
                lastSonarFloraCleanupTime = time;
                var sf = GetComponentInChildren<SonarFloraTag>();
                if (sf)
                    sf.gameObject.destroy(false);
                //foreach (Transform t in transform)
                //	SNUtil.log("Child: "+t.name);
            }
        }
    }

    private Bounds getRenderBox() {
        var extents = new Bounds(transform.position, Vector3.zero);
        foreach (var r in GetComponentsInChildren<Renderer>())
            extents.Encapsulate(r.bounds);
        return extents;
    }
}