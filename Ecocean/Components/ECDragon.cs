namespace ReikaKalseki.Ecocean;

internal class ECDragon : PassiveSonarEntity {

	private FMOD_CustomLoopingEmitterWithCallback roar;

	protected new void Update() {
		base.Update();
		if (!roar) {
			roar = this.GetComponent<FMOD_CustomLoopingEmitterWithCallback>();
		}
	}

	protected override void setSonarRanges() {
		minimumDistanceSq = 125 * 125;
		maximumDistanceSq = 250 * 250;
	}

	protected override bool isAudible() {
		return this.isRoaring(roar);
	}

}