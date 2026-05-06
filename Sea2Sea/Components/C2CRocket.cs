using System.Collections.Generic;
using System.Linq;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class C2CRocket : MonoBehaviour {

	private Rocket rocket;
	private RocketLocker[] lockers;

	private float lastPDAUpdate = -1;

	private void Awake() {
		getLockers();
	}

	private void getLockers() {
		lockers = GetComponentsInChildren<RocketLocker>();
		foreach (var cl in lockers) {
			var sc = cl.GetComponent<StorageContainer>();
			sc.Resize(6, 8);
		}
	}

	private void Update() {
		if (C2CHooks.SkipRocketTick)
			return;
		if (!rocket)
			rocket = GetComponent<Rocket>();
		if (lockers == null)
			getLockers();

		var time = DayNightCycle.main.timePassedAsFloat;
		if (time - lastPDAUpdate >= 0.5F) {
			lastPDAUpdate = time;
			var li = lockers.Where(l => (bool)l).Select(sc => sc.GetComponent<StorageContainer>().container).ToList();
			if (WorldUtil.isInRocket())
				li.Add(Inventory.main.container);
			FinalLaunchAdditionalRequirementSystem.instance.updateContentsAndPDAPageChecklist(rocket, li);
		}
	}

}