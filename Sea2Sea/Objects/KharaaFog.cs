using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

[Obsolete]
public class KharaaFog : CustomPrefab {
    [SetsRequiredMembers]
    internal KharaaFog() : base("kharaafog", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var podRef = ObjectUtil.lookupPrefab("bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782");
        var pod = podRef.GetComponent<GasPod>();
        var fog = pod.gasEffectPrefab;
        var world = fog.clone();
        world.SetActive(false);
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        var sc = world.AddComponent<SphereCollider>();
        sc.radius = 7.5F;
        sc.isTrigger = true;
        var kc = world.EnsureComponent<KharaaFogComponent>();
        kc.Sphere = sc;
        kc.Tracker = world.GetComponent<UWE.TriggerStayTracker>();
        var r = world.GetComponentInChildren<Renderer>();
        return world;
    }
}

internal class KharaaFogComponent : MonoBehaviour {
    private const float DamagePerSecond = 2.5F;
    private const float DamageInterval = 0.25F; //TODO make healthkits not work here, also do for lost river

    internal SphereCollider Sphere;
    internal UWE.TriggerStayTracker Tracker;

    private float _timeLastDamageTick;

    private void Update() {
        if (!(_timeLastDamageTick + DamageInterval <= Time.time)) return;
        foreach (var go in Tracker.Get().Where(go => go)) {
            SNUtil.writeToChat("" + go);
            var live = gameObject.GetComponent<LiveMixin>();
            if (live == null || !live.IsAlive()) continue;
            if (gameObject.GetComponent<Player>() == null && gameObject.GetComponent<Living>() == null) continue;
            live.TakeDamage(
                DamagePerSecond * DamageInterval,
                gameObject.transform.position,
                DamageType.Starve
            );
            SNUtil.writeToChat("" + DamagePerSecond * DamageInterval);
        }

        _timeLastDamageTick = Time.time;
    }
}