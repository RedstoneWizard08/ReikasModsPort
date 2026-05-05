using System.Collections.Generic;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class AoERadiationZone : MonoBehaviour {

	private static readonly string TRIGGER_NAME = "RadiationAoE";

	public float radius { get; private set; }
	public float innerRadius { get; private set; }
	public float maxIntensity;

	private GameObject trigger;

	private void Start() {
		trigger = gameObject.getChildObject(TRIGGER_NAME);
		if (!trigger) {
			trigger = new GameObject(TRIGGER_NAME);
			trigger.transform.SetParent(transform);
			Utils.ZeroTransform(trigger.transform);
			trigger.EnsureComponent<AoERadiationZoneTrigger>().owner = this;
			var sc = trigger.EnsureComponent<SphereCollider>();
			sc.radius = radius;
			sc.isTrigger = true;
		}
	}

	public void setRadii(float r, float inner) {
		radius = r;
		innerRadius = inner;
		if (trigger)
			trigger.EnsureComponent<SphereCollider>().radius = radius;
	}

	public float getScaledIntensity(float dist) {
		return dist <= innerRadius
			? maxIntensity
			: dist >= radius ? 0 : (float)MathUtil.linterpolate(dist, innerRadius, radius, maxIntensity, 0, true);
	}

}

internal class AoERadiationZoneTrigger : MonoBehaviour {

	internal AoERadiationZone owner;

	private void Start() {
		if (!owner)
			owner = gameObject.FindAncestor<AoERadiationZone>();
	}

	private void OnTriggerEnter(Collider other) {
		var tracker = other.gameObject.FindAncestor<AoERadiationTracker>();
		if (tracker)
			tracker.active.Add(owner);
	}

	private void OnTriggerExit(Collider other) {
		var tracker = other.gameObject.FindAncestor<AoERadiationTracker>();
		if (tracker)
			tracker.active.Remove(owner);
	}

}

public class AoERadiationTracker : MonoBehaviour {

	public readonly HashSet<AoERadiationZone> active = [];

	public float getRadiationIntensity() {
		float r = 0;
		foreach (var aoe in active) {
			r = Mathf.Max(r, aoe.getScaledIntensity(Vector3.Distance(transform.position, aoe.transform.position)));
		}
		//if (r > 0)
		//	SNUtil.writeToChat(r.ToString("0.00"));
		return r;
	}

}