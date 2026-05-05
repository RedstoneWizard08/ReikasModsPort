using UnityEngine;

namespace ReikaKalseki.Ecocean;

internal class ECTreader : PassiveSonarEntity {

	private SeaTreaderSounds sounds;

	protected new void Update() {
		base.Update();
		if (!sounds) {
			sounds = this.GetComponentInChildren<SeaTreaderSounds>();
			this.createRadarSphere(sounds.frontLeg.gameObject, 0.33F);
			this.createRadarSphere(sounds.leftLeg.gameObject, 0.33F);
			this.createRadarSphere(sounds.rightLeg.gameObject, 0.33F);
		}
	}

	protected override GameObject getSphereRootGO() {
		return gameObject.getChildObject("Sea_Treader/Sea_Treader_Geo/Sea_Treader");
	}

	protected override void setSonarRanges() {
		minimumDistanceSq = 50 * 50;
		maximumDistanceSq = 90 * 90;
	}

	protected override bool isAudible() {
		return sounds && (sounds.attackSound.evt.hasHandle() || sounds.stepSound.evt.hasHandle() || sounds.stompSound.evt.hasHandle());
	}

	protected override Vector3 getRadarSphereSize() {
		return new Vector3(20, 30, 20);
	}

}