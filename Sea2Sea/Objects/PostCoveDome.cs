using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class PostCoveDome : InteractableSpawnable {

	public static readonly float HOT_THRESHOLD = -1070F;

	[SetsRequiredMembers]
	public PostCoveDome(XMLLocale.LocaleEntry e) : base(e) {
		scanTime = 10;
		AddOnRegister(() => {
			SaveSystem.addSaveHandler(Info.ClassID, new SaveSystem.ComponentFieldSaveHandler<PostCoveDomeTag>().addField("scanned"));
		});
	}

	public override GameObject GetGameObject() {
		var go = ObjectUtil.createWorldObject(VanillaCreatures.GIANT_FLOATER.prefab);
		foreach (var l in go.GetComponentsInChildren<Light>())
			l.gameObject.destroy(false);
		go.EnsureComponent<PostCoveDomeTag>();
		return go;
	}

	public void postRegister() {
		countGen<PostCoveDomeGenerator>(SeaToSeaMod.WorldGen);
		setFragment(CraftingItems.getItem(CraftingItems.Items.ObsidianGlass).TechType, fragmentCount);
		registerEncyPage();
	}

	public static void setupRenderGloss(Renderer r) {
		RenderUtil.setGlossiness(r.materials[0], 750, 15, 0.8F);
		RenderUtil.setEmissivity(r.materials[0], 0);
		if (r.materials.Length > 1) {
			RenderUtil.setGlossiness(r.materials[1], 0, 0, 0F);
			RenderUtil.setEmissivity(r.materials[1], 2);
		}
		r.materials[0].SetColor("_Color", Color.white);
		r.materials[0].SetColor("_SpecColor", Color.white);
		r.materials[0].SetColor("_GlowColor", Color.white);
		r.materials[0].SetFloat("_IBLreductionAtNight", 0);
		r.materials[0].SetFloat("_EmissionLM", 0);
		r.materials[0].SetFloat("_EmissionLMNight", 0);
	}

}

public class PostCoveDomeTag : MonoBehaviour {

	private bool scanned;
	private float scannedFade = 0;

	private Renderer[] renderers;

	private bool computedTexture;
	private bool isHot;

	private Light light;

	private void Start() {
		gameObject.removeComponent<Floater>();
		GetComponentInChildren<Animator>().speed = 0.04F;
		renderers = GetComponentsInChildren<Renderer>();
		foreach (var r in renderers)
			r.materials[1].color = Color.clear;
		transform.localScale = Vector3.one * 0.1F;

		light = GetComponentInChildren<Light>();
		if (!light)
			light = gameObject.addLight();
		light.range = 32;
		light.intensity = 1.5F;
		light.transform.localPosition = Vector3.up * 3;
		light.shadows = LightShadows.Soft;
		Invoke(nameof(spawnOffspring), 30);
	}

	private void Update() {
		var hot = transform.position.y < PostCoveDome.HOT_THRESHOLD;//VanillaBiomes.ILZ.isInBiome(transform.position) || WaterTemperatureSimulation.main.GetTemperature(transform.position) >= 90;
		var retexture = isHot != hot;
		isHot = hot;
		if (retexture || !computedTexture) {
			var tex = "Textures/Plants/PostCoveTree/"+(isHot ? "Hot" : "Cold");
			foreach (var r in renderers) {
				RenderUtil.swapTextures(SeaToSeaMod.ModDLL, r, tex, new Dictionary<int, string> { { 1, "Inner" }, { 0, "Shell" } });
				PostCoveDome.setupRenderGloss(r);
			}
			computedTexture = true;
			light.color = isHot ? new Color(1, 0.8F, 0.2F) : new Color(0.2F, 0.6F, 1F);
		}
	}

	private void OnScanned() {
		scanned = true;
		SNUtil.addBlueprintNotification(CraftingItems.getItem(CraftingItems.Items.ObsidianGlass).TechType);
	}

	public static int maximumDomeChildren = 16;
	public static bool fastReproduction = false;

	private void spawnOffspring() {
		var li = WorldUtil.getObjectsNearWithComponent<PostCoveDomeGenerator.ResourceDomeTag>(transform.position, 24).Select(tag => tag.transform.position);
		if (li.Count() < maximumDomeChildren) {
			var go = PostCoveDomeGenerator.placeRandomResourceDome(gameObject, li, id => ObjectUtil.createWorldObject(id));
			if (go) {
				go.GetComponent<PostCoveDomeGenerator.ResourceDomeTag>().growFade = 1;
			}
		}
		Invoke(nameof(spawnOffspring), fastReproduction ? 1 : Random.Range(30F, 120F));
	}

}