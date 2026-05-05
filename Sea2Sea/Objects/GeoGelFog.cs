using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class GeoGelFog : CustomPrefab {
    public static Color fogColor0 = new Color(0.1F, 0.33F, 1.5F); //new Color(0.15F, 0.1F, 0.1F);
    public static Color fogColor1 = new Color(0.1F, 0.33F, 1.5F);
    public static Color fogColor2 = new Color(0.1F, 0.33F, 1.5F);

    public readonly bool isDrip;

    [SetsRequiredMembers]
    internal GeoGelFog(bool drip) : base("GeogelFog_" + drip, "", "") {
        isDrip = drip;
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        GameObject podRef = ObjectUtil.lookupPrefab("bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782");
        GasPod pod = podRef.GetComponent<GasPod>();
        GameObject fog = pod.gasEffectPrefab;
        GameObject world = fog.clone();
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        world.removeComponent<UWE.TriggerStayTracker>();
        world.removeComponent<FMOD_StudioEventEmitter>();
        world.removeComponent<FMOD_CustomEmitter>();
        world.removeChildObject("xflash");
        Renderer[] r0 = world.GetComponentsInChildren<Renderer>();
        foreach (ParticleSystem pp in world.GetComponentsInChildren<ParticleSystem>()) {
            ParticleSystem.MainModule main = pp.main;
            if (isDrip) {
                main.startSizeMultiplier *= 0.25F;
                main.startLifetimeMultiplier *= 0.67F;
            }

            main.startColor = Color.white.ToAlpha(main.startColor.color.a);
        }

        if (isDrip) {
            world.GetComponent<VFXDestroyAfterSeconds>().lifeTime *= 0.67F;
            world.GetComponent<VFXUnparentAfterSeconds>().timer *= 0.67F;
        }

        foreach (Renderer r in r0) {
            GameObject go = r.gameObject;
            if (go.name == "xSmkLong")
                r.materials[0].SetColor("_Color", fogColor2);
            else if (go.name == "xSmk")
                r.materials[0].SetColor("_Color", fogColor1);
            else
                r.materials[0].SetColor("_Color", fogColor0);
        }

        return world;
    }
}