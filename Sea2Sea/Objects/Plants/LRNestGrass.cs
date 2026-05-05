using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class LRNestGrass : CustomPrefab {
    [SetsRequiredMembers]
    internal LRNestGrass() : base("LRNestGrass", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        GameObject go = ObjectUtil.createWorldObject("449f060e-1f82-4efa-a5e8-c4145a851a8f");
        go.removeComponent<LiveMixin>();
        go.removeComponent<Collider>();
        go.removeComponent<Rigidbody>();
        go.removeComponent<BloodGrass>();
        go.transform.localScale = new Vector3(2, 3, 2);
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        Renderer r = go.GetComponentInChildren<MeshRenderer>();
        r.material.SetColor("_Color", new Color(0.4F, 1, 0.85F, 1));
        r.material.SetColor("_SpecColor", new Color(0.15F, 1, 0.75F, 1));
        r.material.SetVector("_ObjectUp", new Color(0.0F, 0.4F, 1F, 0));
        go.EnsureComponent<LRNestGrassTag>();
        /*
        CapsuleCollider cc = Ecocean.PlantHidingCollider.addToObject<CapsuleCollider>(go); causes damage so hiding in is a bad idea
        cc.radius = 4;
        cc.height = 4;*/
        return go;
    }

    class LRNestGrassTag : MonoBehaviour {
        private ParticleSystem particles;

        //private float nextCheckTime = -1;

        private SphereCollider aoe;

        void Update() {
            /*
            float time = DayNightCycle.main.timePassedAsFloat;
            if (time >= nextCheckTime) {
                if ((Player.main.transform.position-transform.position).sqrMagnitude <= UnityEngine.Random.Range(200F, 300F)) {
                    particles.Play(true);
                }
                nextCheckTime = time+UnityEngine.Random.Range(0.5F, 1F);
            }*/

            if (!particles) {
                GameObject child = gameObject.getChildObject("xBloodGrassSmoke");
                //SNUtil.writeToChat(child ? child.ToString() : "no fx");
                if (child) {
                    particles = child.GetComponent<ParticleSystem>();
                    particles.transform.localPosition = Vector3.up * 0.15F;
                    foreach (ParticleSystem pp in particles.GetComponentsInChildren<ParticleSystem>(true)) {
                        ParticleSystem.MainModule main = pp.main;
                        main.duration *= 2;
                        main.startColor = new Color(0.4F, 1, 0.5F, 1);
                    }
                }
            }
        }

        void Start() {
            aoe = gameObject.EnsureComponent<SphereCollider>();
            aoe.center = Vector3.zero;
            aoe.radius = 1;
            aoe.isTrigger = true;
            gameObject.layer = LayerID.Player;
        }

        void OnTriggerEnter(Collider other) {
            if (!particles || other.isTrigger)
                return;
            LiveMixin lv = other.gameObject.FindAncestor<LiveMixin>();
            if (lv) {
                particles.Play();
                if (lv.isPlayer() || lv.GetComponent<Vehicle>()) {
                    //lv.TakeDamage(10, transform.position, DamageType.Acid, gameObject);
                    DamageOverTime dot = lv.gameObject.EnsureComponent<NestGrassAcid>();
                    dot.doer = gameObject;
                    dot.ActivateInterval(0.25F);
                }
            }
        }
    }

    class NestGrassAcid : DamageOverTime {
        NestGrassAcid() : base() {
            damageType = DamageType.Acid;
            totalDamage = 30;
            duration = 10;
        }
    }
}