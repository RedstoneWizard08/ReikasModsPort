using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class PhysicsSettlingProp : MonoBehaviour {

	public static readonly Dictionary<string, List<PositionedPrefab>> locations = new();

	public PositionedPrefab prefabMarker { get; private set; }

	public int freeTime { get; private set; }
	public string key { get; private set; }

	public Rigidbody body { get; private set; }

	public Predicate<PhysicsSettlingProp> destroyCondition;

	private float time;

	public static void export(string key) {
		if (!locations.ContainsKey(key)) {
			SNUtil.WriteToChat("No physprops with key '" + key + "'");
			return;
		}
		var li = locations[key];
		var file = BuildingHandler.instance.dumpPrefabs(key, li);
		SNUtil.WriteToChat("Exported " + li.Count + " physprops of key '" + key + "' to " + file);
	}

	private static void addPrefab(string key, PositionedPrefab pfb) {
		if (!locations.ContainsKey(key))
			locations[key] = [];
		if (!locations[key].Contains(pfb))
			locations[key].Add(pfb);
	}

	private static void removePrefab(string key, PositionedPrefab pfb) {
		if (!locations.ContainsKey(key))
			return;
		locations[key].Remove(pfb);
	}

	private void Start() {
		body = GetComponentInChildren<Rigidbody>();
	}

	public void Update() {
		if (!body)
			body = GetComponentInChildren<Rigidbody>();
		time += Time.deltaTime;
		onUpdate();
		if (destroyCondition != null && destroyCondition.Invoke(this))
			gameObject.destroy(false);
		else if (time > 15F && body.velocity.magnitude < 0.05 && body.angularVelocity.magnitude < 0.05 && !GetComponent<PropulseCannonAmmoHandler>())
			fixInPlace();
	}

	protected virtual void onUpdate() {

	}

	public void init(string key, int duration) {
		freeTime = duration;
		this.key = key;
		Start();
		Invoke(nameof(fixInPlace), freeTime);
	}

	public void fixInPlace() {
		if (body.isKinematic)
			return;
		body.isKinematic = true;
		prefabMarker = new PositionedPrefab(GetComponent<PrefabIdentifier>());
		addPrefab(key, prefabMarker);
		//SNUtil.log("Locked "+prefabMarker+" at time "+time);
		//this.destroy(false);
	}

	public void unlock() {
		if (!body.isKinematic)
			return;
		removePrefab(key, prefabMarker);
		prefabMarker = null;
		time = 0;
		body.isKinematic = false;
		Invoke(nameof(fixInPlace), freeTime);
	}

	public void bump(float vel) {
		bump(UnityEngine.Random.onUnitSphere * vel);
	}

	public void bump(Vector3 vec) {
		unlock();
		body.velocity = vec;
	}

	private void OnDestroy() {
		removePrefab(key, prefabMarker);
	}

}