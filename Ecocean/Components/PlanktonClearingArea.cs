using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class PlanktonClearingArea : MonoBehaviour {

	public static bool skipPlanktonClear;

	//private CapsuleCollider AoE;

	public float clearingRate = 1;

	public event Action<PlanktonCloudTag, float> onClearTick;

	private Dictionary<string, object> properties = new();

	private void Start() {
		var rb = gameObject.EnsureComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.mass = 9999999;
		rb.useGravity = false;
		rb.freezeRotation = true;
		rb.constraints = RigidbodyConstraints.FreezeAll;
	}

	private void OnTriggerStay(Collider other) {
		if (Time.deltaTime <= 0)
			return;
		//SNUtil.writeToChat("Plankton clearer "+gameObject.name+" ticking with collider "+other.gameObject.GetFullHierarchyPath());
		if (skipPlanktonClear)
			return;
		var pc = other.GetComponent<PlanktonCloudClearableContactZone>(); //NOT ancestor - only interact with specific colliders
		if (pc && pc.parent && pc.parent.enabled) {
			var amt = Time.deltaTime*clearingRate;
			pc.parent.damage(this, amt);
			onClearTick?.Invoke(pc.parent, amt);
		}
	}

	public void tickExternal(float r = 1) {
		onClearTick?.Invoke(null, Time.deltaTime * clearingRate * r);
	}

	public void setProperty(string name, object value) {
		properties[name] = value;
	}

	public object getProperty(string name) {
		return properties.ContainsKey(name) ? properties[name] : null;
	}

	public E getProperty<E>(string name) {
		var o = getProperty(name);
		return o != null && o is E e ? e : default(E);
	}

	public bool getBooleanProperty(string name) {
		var o = getProperty(name);
		return o != null && o is bool b && b;
	}

}