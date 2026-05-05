using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class PowerSealModuleFragment : CustomPrefab {
    // C2CItems.powerSeal.FriendlyName,
    // C2CItems.powerSeal.Description
    [SetsRequiredMembers]
    internal PowerSealModuleFragment() : base(
        "powersealmodulefragment",
        "",
        ""
    ) {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var world = ObjectUtil.createWorldObject(C2CItems.powerSeal.ClassID);
        world.EnsureComponent<TechTag>().type = Info.TechType;
        var pi = world.EnsureComponent<PrefabIdentifier>();
        pi.ClassId = Info.ClassID;
        world.removeComponent<WorldForces>();
        world.removeComponent<Pickupable>();
        //world.removeComponent<ResourceTracker>();
        var rt = world.EnsureComponent<ResourceTracker>();
        rt.techType = TechType.Fragment;
        rt.overrideTechType = TechType.Fragment;
        rt.prefabIdentifier = pi;
        world.GetComponent<Rigidbody>().isKinematic = true;
        world.EnsureComponent<BrokenModule>();
        var r = world.GetComponentInChildren<Renderer>();
        return world;
    }

    public void register() {
        Register();
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, new List<TechType>() { C2CItems.powerSeal.TechType });
        var e = new PDAScanner.EntryData {
            key = Info.TechType,
            blueprint = C2CItems.powerSeal.TechType,
            destroyAfterScan = true,
            locked = true,
            totalFragments = 1,
            isFragment = true,
            scanTime = 8,
        };
        PDAHandler.AddCustomScannerEntry(e);
    }
}

internal class BrokenModule : MonoBehaviour {
    private VFXController _sparker;

    private bool _isSparking;

    private void Update() {
        if (!_sparker) {
            var welder = ObjectUtil.createWorldObject("9ef36033-b60c-4f8b-8c3a-b15035de3116", false, false);
            _sparker = welder.GetComponent<Welder>().fxControl.clone();
            _sparker.transform.parent = transform;
            _sparker.transform.localPosition = new Vector3(0, -0.05F, 0);
            _sparker.transform.eulerAngles = new Vector3(325, 180, 0);
            _sparker.gameObject.SetActive(true);
        }

        transform.localScale = new Vector3(1, 1.3F, 1);
        if (Random.Range(0, 30) != 0) return;
        if (_isSparking)
            _sparker.StopAndDestroy(0);
        else
            _sparker.Play(0);
        _isSparking = !_isSparking;
        /*
        if (UnityEngine.Random.Range(0, 5) == 0) { //prevent burying under a resource
            RaycastHit[] hit = Physics.SphereCastAll(gameObject.transform.position, 0.25F, new Vector3(1, 1, 1), 0.25F);
            foreach (RaycastHit rh in hit) {
                if (rh.transform != null && rh.transform.gameObject) {
                    Pickupable p = rh.transform.gameObject.GetComponent<Pickupable>();
                    if (p) {
                        p.gameObject.destroy();
                    }
                }
            }
        }*/
    }
}