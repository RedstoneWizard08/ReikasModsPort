using System.Collections.Generic;
using System.Linq;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class C2CRocket : MonoBehaviour {

	private Rocket rocket;
	private RocketLocker[] lockers;

	private float lastPDAUpdate = -1;

	void Awake() {
		this.getLockers();
	}

	void getLockers() {
		lockers = this.GetComponentsInChildren<RocketLocker>();
		foreach (RocketLocker cl in lockers) {
			StorageContainer sc = cl.GetComponent<StorageContainer>();
			sc.Resize(6, 8);
		}
	}

	void Update() {
		if (C2CHooks.skipRocketTick)
			return;
		if (!rocket)
			rocket = this.GetComponent<Rocket>();
		if (lockers == null)
			this.getLockers();

		float time = DayNightCycle.main.timePassedAsFloat;
		if (time - lastPDAUpdate >= 0.5F) {
			lastPDAUpdate = time;
			List<ItemsContainer> li = lockers.Where(l => (bool)l).Select(sc => sc.GetComponent<StorageContainer>().container).ToList();
			if (WorldUtil.isInRocket())
				li.Add(Inventory.main.container);
			FinalLaunchAdditionalRequirementSystem.instance.updateContentsAndPDAPageChecklist(rocket, li);
		}
	}

}