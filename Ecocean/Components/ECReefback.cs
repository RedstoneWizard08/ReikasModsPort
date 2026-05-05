using UnityEngine;

namespace ReikaKalseki.Ecocean;

internal class ECReefback : PassiveSonarEntity {

	private FMOD_CustomLoopingEmitter idleSound;

	protected new void Update() {
		base.Update();
		if (!idleSound) {
			idleSound = this.GetComponent<FMOD_CustomLoopingEmitter>();
		}
	}

	protected override GameObject getSphereRootGO() {
		return gameObject.getChildObject("Pivot/Reefback/Reefback").GetComponentInChildren<Renderer>().gameObject;
	}

	protected override void setSonarRanges() {
		minimumDistanceSq = 120 * 120;
		maximumDistanceSq = 200 * 200;
		if (VanillaBiomes.GRANDREEF.isInBiome(transform.position)) {
			minimumDistanceSq *= 0.5F;
			maximumDistanceSq *= 0.25F;
		}
		else if (VanillaBiomes.REDGRASS.isInBiome(transform.position)) {
			minimumDistanceSq *= 1.25F;
		}
	}

	protected override bool isAudible() {
		return this.isRoaring(idleSound);
	}

	protected override Vector3 getRadarSphereSize() {
		return new Vector3(45, 45, 60);
	}

}