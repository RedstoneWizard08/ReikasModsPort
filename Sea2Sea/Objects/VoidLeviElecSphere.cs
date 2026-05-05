using System;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

[Obsolete]
public class VoidLeviElecSphere : CustomPrefab {
    [SetsRequiredMembers]
    internal VoidLeviElecSphere() : base("levipulse", "", "") {
        AddOnRegister(() => {
                SaveSystem.addSaveHandler(
                    Info.ClassID,
                    new SaveSystem.ComponentFieldSaveHandler<HeatSinkTag>().addField("spawnTime")
                );
            }
        );

        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() { /*
        GameObject world = ObjectUtil.createWorldObject("1ff4c159-f8fe-443d-b3d3-f04a278459d9");
        */
        var sm = ObjectUtil.lookupPrefab("1c34945a-656d-4f70-bf86-8bc101a27eee");
        var def = sm.GetComponent<SeaMoth>().seamothElectricalDefensePrefab.GetComponent<ElectricalDefense>();
        var sphere = def.fxElecSpheres[def.fxElecSpheres.Length - 1];
        var world = sphere.clone();
        world.SetActive(false);
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.VeryFar;
        world.removeComponent<VFXDestroyAfterSeconds>();
        var kc = world.EnsureComponent<VoidLeviElecSphereComponent>();
        var r = world.GetComponentInChildren<Renderer>();
        return world;
    }
}

internal class VoidLeviElecSphereComponent : MonoBehaviour {
    private float _spawnTime;

    private void Awake() {
        LargeWorldStreamer.main.MakeEntityTransient(gameObject);
    }

    private void Update() {
        GetComponent<ParticleSystem>().Play(true);
        if (_spawnTime <= 0)
            return;
        var time = DayNightCycle.main.timePassedAsFloat;
        var dT = time - _spawnTime;
        transform.localScale = Vector3.one * (0.0001f + dT);
        if (dT >= 10)
            gameObject.destroy();
    }

    internal void Spawn() {
        _spawnTime = DayNightCycle.main.timePassedAsFloat;
    }
}