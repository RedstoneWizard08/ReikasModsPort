namespace ReikaKalseki.Ecocean;

internal class ECReaper : PassiveSonarEntity {

	private FMOD_CustomLoopingEmitter roar1;
	private FMOD_CustomLoopingEmitterWithCallback roar2;

	protected new void Update() {
		base.Update();
		if (!roar1) {
			foreach (var em in GetComponents<FMOD_CustomLoopingEmitter>()) {
				if (em.asset != null && em.asset.path.Contains("idle")) {
					roar1 = em;
					break;
				}
			}
			roar2 = GetComponent<FMOD_CustomLoopingEmitterWithCallback>();
		}
	}

	protected override bool isAudible() {
		return isRoaring(roar1) || isRoaring(roar2);
	}

}