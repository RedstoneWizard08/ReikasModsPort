using UnityEngine;

namespace ReikaKalseki.Ecocean;

internal class ECEmperor : MonoBehaviour {

	void Start() {
		this.InvokeRepeating("applyPassivity", 0, 0.5F);
	}

	void OnDisable() {
		this.CancelInvoke("applyPassivity");
	}

	void OnDestroy() {
		this.OnDisable();
	}

	void applyPassivity() {
		foreach (AggressiveWhenSeeTarget a in WorldUtil.getObjectsNearWithComponent<AggressiveWhenSeeTarget>(transform.position, 100)) {
			a.creature.Aggression.Add(-1);
			a.lastTarget.target = null;
		}
	}

}