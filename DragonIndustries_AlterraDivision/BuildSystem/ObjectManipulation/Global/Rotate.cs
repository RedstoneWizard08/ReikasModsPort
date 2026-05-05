using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal class Rotate : GlobalManipulation {

	private Vector3 min = Vector3.zero;
	private Vector3 max = Vector3.zero;
	private Vector3 origin = Vector3.zero;

	internal override void applyToGlobalObject(GameObject go) {
		var rot = MathUtil.getRandomVectorBetween(min, max);
		MathUtil.rotateObjectAround(go, origin, rot.y);

		go.transform.RotateAround(origin, Vector3.right, rot.x);
		go.transform.RotateAround(origin, Vector3.forward, rot.z);
	}

	internal override void applyToGlobalObject(PlacedObject go) {
		applyToObject(go.obj);
		go.setPosition(go.obj.transform.position);
		go.setRotation(go.obj.transform.rotation);
	}

	internal override void applyToSpecificObject(PlacedObject go) {
		applyToSpecificObject(go.obj);
		go.setRotation(go.obj.transform.rotation);
	}

	internal override void applyToSpecificObject(GameObject go) {
		var rot = MathUtil.getRandomVectorBetween(min, max);
		go.transform.RotateAround(go.transform.position, Vector3.up, rot.y);
		go.transform.RotateAround(go.transform.position, Vector3.right, rot.x);
		go.transform.RotateAround(go.transform.position, Vector3.forward, rot.z);
	}

	public override void loadFromXML(XmlElement e) {
		base.loadFromXML(e);
		min = e.getVector("min").Value;
		max = e.getVector("max").Value;
		var or = e.getVector("origin", true);
		if (or != null && or.HasValue)
			origin = or.Value;
	}

	public override void saveToXML(XmlElement e) {
		base.saveToXML(e);
		e.addProperty("min", min);
		e.addProperty("max", max);
		e.addProperty("origin", origin);
	}

}