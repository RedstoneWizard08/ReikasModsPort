using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class HeatColumnFog : CustomPrefab {
    public static Color fogColor0 = new(0.1F, 1.875F, 1.0F); //new Color(0.15F, 0.1F, 0.1F);
    public static Color fogColor1 = new(0.1F, 1.875F, 1.0F);
    public static Color fogColor2 = new(0.1F, 1.875F, 1.0F);

    [SetsRequiredMembers]
    internal HeatColumnFog() : base("HeatColumnFog", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        GameObject podRef = ObjectUtil.lookupPrefab("bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782");
        var pod = podRef.GetComponent<GasPod>();
        var fog = pod.gasEffectPrefab;
        GameObject world = fog.clone();
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        world.EnsureComponent<HeatColumnFogTag>();
        world.removeComponent<UWE.TriggerStayTracker>();
        world.removeComponent<FMOD_StudioEventEmitter>();
        world.removeComponent<FMOD_CustomEmitter>();
        world.removeChildObject("xflash");
        var r0 = world.GetComponentsInChildren<Renderer>();
        foreach (var pp in world.GetComponentsInChildren<ParticleSystem>()) {
            var main = pp.main;
            main.startColor = Color.white.ToAlpha(main.startColor.color.a * 0.4F);
            //main.startSizeMultiplier *= 2.5F;
            var size = pp.sizeOverLifetime;
            size.sizeMultiplier *= 2.5F;
            main.startLifetimeMultiplier *= 0.67F; //15.0F;
            //world.GetComponent<VFXDestroyAfterSeconds>().lifeTime *= 15F;
            //world.GetComponent<VFXUnparentAfterSeconds>().timer *= 15F;
            var speed = pp.velocityOverLifetime;
            speed.x = 0;
            speed.y = 4.5F;
            speed.z = 0;
            var em = pp.emission;
            var sh = pp.shape;
            sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale = Vector3.one;
        }

        foreach (var r in r0) {
            var go = r.gameObject;
            if (go.name == "xSmkLong")
                r.materials[0].SetColor("_Color", fogColor2);
            else if (go.name == "xSmk")
                r.materials[0].SetColor("_Color", fogColor1);
            else
                r.materials[0].SetColor("_Color", fogColor0);
            //r.materials[0].SetFloat("_SrcBlend", 5);
        }

        return world;
    }
}

internal class HeatColumnFogTag : HeatColumnObject {
    private void Start() {
    }

    private new void Update() {
        base.Update();
        if (body) {
            body.isKinematic = false;
            body.velocity = Vector3.up * 1.5F;
        }
    }
}