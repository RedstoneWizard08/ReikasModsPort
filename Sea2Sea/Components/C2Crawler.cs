using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class C2Crawler : MonoBehaviour {

	private BiomeBase biome;

	void Start() {
		base.InvokeRepeating("tick", 0f, 0.5F);
	}

	private void OnKill() {
		this.destroy(false);
	}

	void OnDisable() {
		base.CancelInvoke("tick");
	}

	internal void tick() {
		if (C2CHooks.skipCrawlerTick)
			return;
		BiomeBase at = BiomeBase.getBiome(transform.position);
		if (at != biome && (at == VanillaBiomes.BLOODKELP || at == CrashZoneSanctuaryBiome.instance)) {
			foreach (Renderer r in this.GetComponentsInChildren<Renderer>()) {
				RenderUtil.swapTextures(SeaToSeaMod.modDLL, r, "Textures/CaveCrawlerBlue");
			}
		}
		biome = at;
	}

}