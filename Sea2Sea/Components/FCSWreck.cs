using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class FCSWreck : MonoBehaviour {

	private float lastCheckTime = -1;

	private void Apply() {
		//SNUtil.writeToChat("Initializing FCS wreck");
		gameObject.removeChildObject("ExteriorEntities/Starship_doors_frame");
		gameObject.removeChildObject("ExteriorEntities/vent_constructor_section_01");
		//GameObject hull1 = gameObject.getChildObject("ExteriorEntities/ExplorableWreckHull01");
		var hull2 = gameObject.getChildObject("ExteriorEntities/ExplorableWreckHull02");

		hull2.transform.rotation = Quaternion.Euler(0, 116, 210.7F);
		hull2.transform.localPosition += new Vector3(-1, 0, -7.5F);
	}

	private void Update() {
		var time = DayNightCycle.main.timePassedAsFloat;
		if (time - lastCheckTime >= 1) {
			lastCheckTime = time;
			if (gameObject.getChildObject("ExteriorEntities/vent_constructor_section_01")) {
				Apply();

				foreach (var bpt in gameObject.GetComponentsInChildren<BlueprintHandTarget>()) {
					bpt.gameObject.destroy(false);
				}
				foreach (var bpt in gameObject.GetComponentsInChildren<DataboxSpawner>()) {
					bpt.gameObject.destroy(false);
				}
				foreach (var bpt in gameObject.GetComponentsInChildren<StoryHandTarget>()) {
					bpt.gameObject.destroy(false);
				}
			}
		}
	}

}