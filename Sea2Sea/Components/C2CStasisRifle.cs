using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class C2CStasisRifle : CustomGrindable, ReactsOnDrilled {

	private float nextSphereTime = -1;

	public override GameObject chooseRandomResource() {
		return null; //unused as not recyclable
	}

	public void onDrilled(Vector3 pos) {
		var time = DayNightCycle.main.timePassedAsFloat;
		if (time >= nextSphereTime) {
			var ss = WorldUtil.createStasisSphere(pos, 2, 0.5F);
			SoundManager.playSoundAt(GetComponent<StasisRifle>().fireSound, pos);
			Utils.PlayOneShotPS(ObjectUtil.lookupPrefab(VanillaCreatures.CRASHFISH.prefab).GetComponent<Crash>().detonateParticlePrefab, transform.position, transform.rotation);
			nextSphereTime = time + ss.GetLifespan();
		}
	}

	public override bool isRecyclable() {
		return false;
	}

}