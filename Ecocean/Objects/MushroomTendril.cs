using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class MushroomTendril : CustomPrefab {

	public static readonly Color color1 = new(0.4F, 1.0F, 1.5F, 1F);
	public static readonly Color color2 = new(1.8F, 1.1F, 0.5F, 1F);

	private static readonly Dictionary<string, float> radii = new(){
		{"e0d415d9-1bc6-4c8b-b3c0-69f5e5fa6b08", 1},
		{"79527fc2-7037-41c0-9e3d-e003f3cd0b06", 0.5F},
		{"775b6835-bd08-40d2-b80e-ab0ddc539c45", 0.5F},
	};

	internal static readonly Dictionary<string, Color> colors = new();

	public static float getPrefabRadius(string id) {
		return radii.ContainsKey(id) ? radii[id] : 0.75F;
	}

	public readonly Color glowColor;

	[SetsRequiredMembers]
	public MushroomTendril(XMLLocale.LocaleEntry e, Color c) : base(e.key + "_" + c.ToString(), e.name, e.desc) {
		glowColor = c;

		AddOnRegister(() => {
			colors[Info.ClassID] = c;
		});
	}

	public GameObject GetGameObject() {
		var world = new GameObject("MushroomTendril");
		world.EnsureComponent<MushroomTendrilTag>();
		var bc = world.EnsureComponent<BoxCollider>();
		bc.isTrigger = true;
		world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
		world.EnsureComponent<TechTag>().type = Info.TechType;
		world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Far;
		return world;
	}

	private class MushroomTendrilTag : MonoBehaviour {

		private Animator[] animators;
		private Renderer[] renders;
		private BoxCollider collider;

		private Color renderColor;

		private bool init;

		private void Update() {
			ObjectUtil.cleanUpOriginObjects(this);

			if (!collider)
				collider = GetComponentInChildren<BoxCollider>();

			if (animators == null)
				animators = GetComponentsInChildren<Animator>();

			if (animators.Length < 4) {
				for (var i = animators.Length; i < 4; i++) {
					GameObject go = ObjectUtil.lookupPrefab(VanillaFlora.STINGERS.getRandomPrefab(false)).GetComponentInChildren<Animator>().gameObject.clone();
					go.transform.SetParent(transform);
				}
				animators = GetComponentsInChildren<Animator>();
			}
			if (!init && animators.Length == 4) {
				var i = 0;
				foreach (var a in animators) {
					a.speed = 0.3F;
					a.transform.localRotation = Quaternion.Euler(0, 11.25F * i, 0);
					a.transform.localScale = Vector3.one;
					a.transform.localPosition = Vector3.zero;
					i++;
				}

				renders = GetComponentsInChildren<Renderer>();

				renderColor = colors[GetComponent<PrefabIdentifier>().ClassId];
				foreach (var r in renders) {
					RenderUtil.swapTextures(EcoceanMod.modDLL, r, "Textures/MushroomStrand", new Dictionary<int, string>() { { 1, "" } });
					r.materials[0].EnableKeyword("FX_BURST"); //make opaque part invisible
					r.materials[1].SetColor("_GlowColor", renderColor);
					RenderUtil.setEmissivity(r.materials[1], 7F);
				}

				if (collider) {
					collider.size = new Vector3(0.6F, 1.6F, 0.6F);
					collider.center = Vector3.down * 0.8F;
				}
				init = true;
			}

			if ((transform.position - Player.main.transform.position).sqrMagnitude >= 22500)
				return;

			if (renders != null) {
				foreach (var r in renders) {
					var f = 5F+(6F-2F*DayNightCycle.main.GetLightScalar())*Mathf.Sin(gameObject.GetInstanceID()*1.28F+DayNightCycle.main.timePassedAsFloat*0.267F);
					if (f < 0)
						f = 0;
					RenderUtil.setEmissivity(r.materials[1], f, f * 0.1F);
				}
			}
		}

		private void OnTriggerStay(Collider other) {
			if (!other.isTrigger && other.isPlayer()) {
				var e = Player.main.gameObject.EnsureComponent<FoodEffectSystem.VisualDistortionEffect>();
				e.intensity = 2;
				e.timeRemaining = 10;
				e.effectColor = renderColor.ToVectorA().Exponent(4F);
				e.tintIntensity = 0.32F; //0.28
				e.tintColor = (renderColor.Exponent(2) * 4).WithAlpha(1);
			}
		}

	}


}