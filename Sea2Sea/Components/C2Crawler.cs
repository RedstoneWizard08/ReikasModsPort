using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class C2Crawler : MonoBehaviour {

	private BiomeBase biome;

	private void Start() {
		InvokeRepeating(nameof(tick), 0f, 0.5F);
	}

	private void OnKill() {
		this.destroy(false);
	}

	private void OnDisable() {
		CancelInvoke(nameof(tick));
	}

	internal void tick() {
		if (C2CHooks.SkipCrawlerTick)
			return;
		var at = BiomeBase.GetBiome(transform.position);
		if (at != biome && (at == VanillaBiomes.Bloodkelp || at == CrashZoneSanctuaryBiome.instance)) {
			foreach (var r in GetComponentsInChildren<Renderer>()) {
				RenderUtil.swapTextures(SeaToSeaMod.ModDLL, r, "Textures/CaveCrawlerBlue");
			}
		}
		biome = at;
	}

}