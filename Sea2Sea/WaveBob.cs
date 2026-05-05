using System;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

[Serializable]
//[ProtoContract]
//[ProtoInclude(30000, typeof(WaveBob))]
internal class WaveBob : MonoBehaviour {

	public Vector3 rootPosition = Vector3.zero;

	public double speed = 0.5;
	public double amplitude = 1;

	public double speed2Ratio = 2.5;
	public double amplitude2Ratio = 0.1;

	private void Start() {

	}

	private void Update() {
		var y = amplitude*Math.Sin(speed*DayNightCycle.main.timePassedAsDouble%(20*Math.PI));
		if (amplitude2Ratio > 0)
			y += amplitude * amplitude2Ratio * Math.Sin((speed * speed2Ratio * DayNightCycle.main.timePassedAsDouble + 238239) % (20 * Math.PI));
		gameObject.transform.position = new Vector3(rootPosition.x, rootPosition.y + (float)y, rootPosition.z);
	}

}