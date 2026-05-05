using System;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class Magnetic : MonoBehaviour {

	private Rigidbody body;
	private Vehicle vehicle;

	private bool searched;

	private void FixedUpdate() {
		if (C2CHooks.skipMagnetic)
			return;
		if (!searched) {
			try {
				if (!body)
					body = GetComponentInChildren<Rigidbody>();
				if (!vehicle)
					vehicle = GetComponentInChildren<Vehicle>();
			}
			catch (Exception e) {
				SNUtil.log("Magnetic threw exception on search: " + e, SeaToSeaMod.ModDLL);
			}
			searched = true;
		}
		var dT = Time.deltaTime;
		if (dT > 0 && body && !body.isKinematic && !vehicle) {
			var set = WorldUtil.getObjectsNearWithComponent<Magnetic>(transform.position, 18);
			foreach (var m in set) {
				attract(this, m, dT);
			}
		}
	}

	private static void attract(Magnetic m1, Magnetic m2, float dT) {
		var diff = m2.transform.position-m1.transform.position;
		var dist = diff.sqrMagnitude;
		diff = diff.normalized;
		var mag = 240F*dT/Mathf.Max(0.1F, dist);
		if (m1.body && !m1.body.isKinematic)
			m1.body.AddForce(diff.setLength(mag), ForceMode.Force);
		if (m2.body && !m2.body.isKinematic)
			m2.body.AddForce(-diff.setLength(mag), ForceMode.Force);
	}

}