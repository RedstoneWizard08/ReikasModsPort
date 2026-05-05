using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class PlantHidingCollider : MonoBehaviour {

	private Collider collider;
	internal Color renderColor;

	internal void initialize(Collider c, Color clr) {
		collider = c;
		renderColor = clr;
	}

	void Update() {
		if (!collider)
			collider = this.GetComponent<Collider>();
	}

	private void OnTriggerStay(Collider other) {

	}

	private void OnTriggerEnter(Collider other) {
		if (!collider)
			return;
		PlantHidingTracker pc = other.gameObject.FindAncestor<PlantHidingTracker>();
		if (pc && pc.minRadius * pc.minRadius <= collider.bounds.size.sqrMagnitude)
			pc.addContact(this);
	}

	private void OnTriggerExit(Collider other) {
		if (!collider)
			return;
		PlantHidingTracker pc = other.gameObject.FindAncestor<PlantHidingTracker>();
		if (pc && pc.minRadius * pc.minRadius <= collider.bounds.size.sqrMagnitude)
			pc.removeContact(this);
	}

	public static void addToObject(Collider c, Color clr) {
		if (!c)
			return;
		if (c.gameObject.FindAncestor<WaterPark>())
			return;
		c.gameObject.EnsureComponent<PlantHidingCollider>().initialize(c, clr);
	}

	public static void addToObject(InteractionVolumeCollider c, Color clr) {
		if (!c)
			return;
		if (c.gameObject.FindAncestor<WaterPark>())
			return;
		c.gameObject.EnsureComponent<PlantHidingCollider>().initialize(c.GetComponent<Collider>(), clr);
	}

}