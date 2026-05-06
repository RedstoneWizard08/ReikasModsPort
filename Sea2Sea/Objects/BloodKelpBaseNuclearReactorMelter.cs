using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea;

public class BloodKelpBaseNuclearReactorMelter : CustomPrefab {
    [SetsRequiredMembers]
    internal BloodKelpBaseNuclearReactorMelter() : base("BloodKelpBaseNuclearReactorMelter", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var go = new GameObject();
        go.EnsureComponent<BloodKelpBaseNuclearReactorMelterTag>();
        go.EnsureComponent<TechTag>().type = Info.TechType;
        go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
        return go;
    }

    private class BloodKelpBaseNuclearReactorMelterTag : MonoBehaviour {
        private bool triggered;

        private void Update() {
            if (triggered)
                return;
            var go =
                WorldUtil.getClosest<BaseNuclearReactorGeometry>(C2CHooks.BkelpBaseNuclearReactor);
            if (go && Vector3.Distance(go.transform.position, C2CHooks.BkelpBaseNuclearReactor) < 5F) {
                /*
                LeakingRadiation lr = go.EnsureComponent<LeakingRadiation>();
                lr.leaks = new List<RadiationLeak>();
                lr.radiationFixed = false;
                lr.kGrowRate = 0;
                lr.kNaturalDissipation = 0;
                lr.kStartRadius = lr.kMaxRadius = lr.currentRadius = 9;
                lr.damagePlayerInRadius = go.EnsureComponent<DamagePlayerInRadius>();
                lr.damagePlayerInRadius.damageType = DamageType.Radiation;
                lr.damagePlayerInRadius.damageAmount = 3;
                lr.radiatePlayerInRange = go.EnsureComponent<RadiatePlayerInRange>();
                */
                go.gameObject.EnsureComponent<BloodKelpBaseNuclearReactorGlower>();
                triggered = true;
            }
        }
    }

    private class BloodKelpBaseNuclearReactorGlower : MonoBehaviour {
        private bool textured;
        private Text text;

        private readonly List<ParticleSystem> bubbles = [];

        private void Update() {
            if (!textured) {
                textured = true;
                foreach (var r in GetComponentsInChildren<Renderer>())
                    RenderUtil.swapTextures(SeaToSeaMod.ModDLL, r, "Textures/bkelpreactor");
            }

            if (!text) {
                var child = gameObject.getChildObject("UI/Canvas/Text");
                text = child.GetComponent<Text>();
            }

            text.text =
                "<color=#ff0000>OPERATOR ERROR\n\nMOLTEN CORE WARNING\nTEMP AT SPIKEVALUE \n999999999999999</color>";

            while (bubbles.Count < 11) {
                var go = ObjectUtil.createWorldObject("0dbd3431-62cc-4dd2-82d5-7d60c71a9edf");
                go.transform.SetParent(transform);
                var y = Random.Range(-0.2F, 1.2F);
                var r = 0.8F;
                if (y < 0.2)
                    r += y * 0.33F;
                var ang = Random.Range(0F, 360F) * Mathf.PI / 180F;
                go.transform.localPosition = new Vector3(r * Mathf.Cos(ang), -y, r * Mathf.Sin(ang));
                go.transform.rotation = Quaternion.Euler(270, 0, 0); //not local - force to always be up
                var ps = go.GetComponent<ParticleSystem>();
                go.SetActive(true);
                bubbles.Add(ps);
                ps.Play();
            }
        }
    }
}