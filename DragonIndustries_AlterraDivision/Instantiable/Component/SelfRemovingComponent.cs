using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public abstract class SelfRemovingComponent : MonoBehaviour {

	public float elapseWhen;

	protected virtual void Update() {
		if (DayNightCycle.main.timePassedAsFloat >= elapseWhen) {
			this.destroy(false);
		}
	}

	private void OnKill() {
		this.destroy(false);
	}

}