using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class DelayedKill : MonoBehaviour {

	private DamageType damage;

	public void initialize(float delay, DamageType dmg) {
		damage = dmg;
		Invoke("run", delay);
	}

	private void run() {
		GetComponent<LiveMixin>().Kill(damage);
	}


}